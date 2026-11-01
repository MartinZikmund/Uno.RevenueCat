using System.Runtime.CompilerServices;
using Maui.RevenueCat.iOS;
using Microsoft.Extensions.Logging;
using Uno.RevenueCat.Enums;
using Uno.RevenueCat.Extensions;
using Uno.RevenueCat.Models;
using Uno.RevenueCat.Platforms.iOS.Exceptions;
using Uno.RevenueCat.Platforms.iOS.Extensions;
using Uno.RevenueCat.Platforms.iOS.Models;
using Purchases = Maui.RevenueCat.iOS.RCPurchases;

namespace Uno.RevenueCat.Services;

public partial class RevenueCatBilling : IRevenueCatBilling
{
    private RCOfferings? _cachedOfferingPackages = null;

    private partial void InvalidateIdentityScopedCaches()
    {
        Interlocked.Increment(ref _identityGeneration);
        _cachedOfferingPackages = null;
    }

    /// <inheritdoc />
    public partial bool IsAnonymous => Purchases.SharedPurchases.IsAnonymous;

    /// <inheritdoc />
    public partial string AppUserId => Purchases.SharedPurchases.AppUserID;

    /// <inheritdoc />
    public partial Task<CanMakePaymentsResultDto> CanMakePaymentsAsync(CancellationToken cancellationToken) =>
        Task.FromResult(new CanMakePaymentsResultDto { Value = Purchases.CanMakePayments });

    /// <inheritdoc />
    public partial Task<StorefrontResultDto> GetStorefrontCountryCodeAsync(CancellationToken cancellationToken)
    {
        EnsureInitialized();

        // Populated once StoreKit has reported the user's storefront; null until then.
        // StoreKit reports alpha-3 ("USA") while the contract and Android use alpha-2.
        var countryCode = Purchases.SharedPurchases?.StoreFrontCountryCode ?? string.Empty;
        return Task.FromResult(new StorefrontResultDto { Value = countryCode.ToIsoAlpha2CountryCode() });
    }

    /// <inheritdoc />
    public partial void Initialize(string apiKey)
    {
        try
        {
            Purchases.ConfigureWithAPIKey(apiKey);
            _isInitialized = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Initialization exception");
            throw;
        }
    }

    /// <inheritdoc />
    public partial void Initialize(string apiKey, string appUserId)
    {
        try
        {
            Purchases.ConfigureWithAPIKey(apiKey, appUserId);
            _isInitialized = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Initialization exception");
            throw;
        }
    }

    /// <inheritdoc />
    public async partial Task<IntroEligibilityResultDto> CheckTrialOrIntroDiscountEligibilityAsync(
        IEnumerable<string> productIdentifiers,
        CancellationToken cancellationToken)
    {
        EnsureInitialized();

        try
        {
            using var eligibilities = await Purchases.SharedPurchases.CheckTrialOrIntroDiscountEligibilityAsync(productIdentifiers, cancellationToken);

            Dictionary<string, IntroElegibilityStatus> eligibilitiesResult = new();

            if (eligibilities is not null)
            {
                for (ulong i = 0; i < eligibilities.Count; i++)
                {
                    eligibilitiesResult.Add(eligibilities.Keys[i], eligibilities.Values[i].Status.Convert());
                }
            }

            return new() { Value = eligibilitiesResult };
        }
        catch (Exception ex)
        {
            return new() { Error = LogAndMapError(ex), ErrorException = ex };
        }
    }

    /// <inheritdoc />
    public async partial Task<OfferingsResultDto> GetOfferingsAsync(
        bool forceRefresh,
        CancellationToken cancellationToken)
    {
        EnsureInitialized();

        if (!forceRefresh && _cachedOfferingPackages != null)
        {
            return new() { Value = _cachedOfferingPackages.ToOfferingDtoList() };
        }

        try
        {
            var generation = Volatile.Read(ref _identityGeneration);
            var offerings = await Purchases.SharedPurchases.GetOfferingsAsync(cancellationToken);

            if (offerings is null)
            {
                return new() { Value = [] };
            }

            // Drop the result if the user changed while the fetch was in flight.
            if (Volatile.Read(ref _identityGeneration) == generation)
            {
                _cachedOfferingPackages = offerings;
            }

            return new() { Value = offerings.ToOfferingDtoList() };
        }
        catch (Exception ex)
        {
            return new() { Error = LogAndMapError(ex), ErrorException = ex };
        }
    }

    /// <inheritdoc />
    public async partial Task<PurchaseResultDto> PurchaseProductAsync(
        PackageDto package,
        object? appWindow,
        CancellationToken cancellationToken)
    {
        // appWindow is ignored on iOS - no Activity needed
        EnsureInitialized();

        if (_cachedOfferingPackages is null)
        {
            throw new InvalidOperationException("GetOfferingsAsync must be called prior to purchasing a product.");
        }

        var offeringToBuy = _cachedOfferingPackages.OfferingWithIdentifier(package.OfferingIdentifier);
        if (offeringToBuy is null)
        {
            throw new InvalidOperationException($"No offering with identifier: {package.OfferingIdentifier} found. Make sure you called GetOfferingsAsync before.");
        }

        var packageToBuy = offeringToBuy.AvailablePackages.FirstOrDefault(p => p.Identifier == package.Identifier);
        if (packageToBuy is null)
        {
            throw new InvalidOperationException($"No package with identifier: {package.Identifier} found. Make sure you called GetOfferingsAsync before.");
        }

        PurchaseSuccessInfo? purchaseSuccessInfo;

        try
        {
            purchaseSuccessInfo = await Purchases.SharedPurchases.PurchasePackageAsync(packageToBuy, cancellationToken);
        }
        catch (Exception ex)
        {
            return new() { Error = LogAndMapError(ex), ErrorException = ex };
        }

        if (purchaseSuccessInfo is null)
        {
            InvalidOperationException missingInfo = new($"{nameof(purchaseSuccessInfo)} is null");
            return new() { Error = LogAndMapError(missingInfo), ErrorException = missingInfo };
        }

        var sk1Transaction = purchaseSuccessInfo.StoreTransaction.Sk1Transaction;
        var isPurchased = sk1Transaction is not null
            ? sk1Transaction.TransactionState == StoreKit.SKPaymentTransactionState.Purchased
            : !string.IsNullOrEmpty(purchaseSuccessInfo.StoreTransaction.TransactionIdentifier);

        var transaction = purchaseSuccessInfo.StoreTransaction.ToStoreTransactionDto();
        var customerInfo = purchaseSuccessInfo.CustomerInfo.ToCustomerInfoDto();

        if (!isPurchased)
        {
            // Only Deferred means "awaiting approval". Purchasing/Failed/Restored, or a StoreKit 2
            // transaction without an identifier, are anomalies - "check back later" would be a lie.
            var errorStatus = sk1Transaction?.TransactionState == StoreKit.SKPaymentTransactionState.Deferred
                ? PurchaseErrorStatus.PaymentPendingError
                : PurchaseErrorStatus.UnknownError;

            _logger.LogWarning(
                "{OperationName} returned a transaction in state {TransactionState}, reported as {ErrorStatus}.",
                nameof(PurchaseProductAsync),
                sk1Transaction?.TransactionState.ToString() ?? "StoreKit2 (no identifier)",
                errorStatus);

            return new()
            {
                Error = errorStatus,
                Transaction = transaction,
                Value = customerInfo,
            };
        }

        return new()
        {
            Transaction = transaction,
            Value = customerInfo,
        };
    }

    /// <inheritdoc />
    public async partial Task<ProductIdentifiersResultDto> GetActiveSubscriptionsAsync(CancellationToken cancellationToken)
    {
        EnsureInitialized();

        try
        {
            using var customerInfo = await Purchases.SharedPurchases.GetCustomerInfoAsync(cancellationToken);

            return new() { Value = customerInfo?.ActiveSubscriptions.ToStringList() ?? [] };
        }
        catch (Exception ex)
        {
            return new() { Error = LogAndMapError(ex), ErrorException = ex };
        }
    }

    /// <inheritdoc />
    public async partial Task<ProductIdentifiersResultDto> GetAllPurchasedIdentifiersAsync(CancellationToken cancellationToken)
    {
        EnsureInitialized();

        try
        {
            using var customerInfo = await Purchases.SharedPurchases.GetCustomerInfoAsync(cancellationToken);

            return new() { Value = customerInfo?.AllPurchasedProductIdentifiers.ToStringList() ?? [] };
        }
        catch (Exception ex)
        {
            return new() { Error = LogAndMapError(ex), ErrorException = ex };
        }
    }

    /// <inheritdoc />
    public async partial Task<PurchaseDateResultDto> GetPurchaseDateForProductIdentifierAsync(
        string productIdentifier,
        CancellationToken cancellationToken)
    {
        EnsureInitialized();

        try
        {
            using var customerInfo = await Purchases.SharedPurchases.GetCustomerInfoAsync(cancellationToken);

            return new() { Value = customerInfo?.PurchaseDateForProductIdentifier(productIdentifier).ToDateTime() };
        }
        catch (Exception ex)
        {
            return new() { Error = LogAndMapError(ex), ErrorException = ex };
        }
    }

    /// <inheritdoc />
    public async partial Task<ManagementUrlResultDto> GetManagementSubscriptionUrlAsync(CancellationToken cancellationToken)
    {
        EnsureInitialized();

        try
        {
            using var customerInfo = await Purchases.SharedPurchases.GetCustomerInfoAsync(cancellationToken);

            return new() { Value = customerInfo?.ManagementURL?.ToString() };
        }
        catch (Exception ex)
        {
            return new() { Error = LogAndMapError(ex), ErrorException = ex };
        }
    }

    /// <inheritdoc />
    public async partial Task<CustomerInfoResultDto> LoginAsync(string appUserId, CancellationToken cancellationToken)
    {
        EnsureInitialized();

        try
        {
            var loginResult = await Purchases.SharedPurchases.LoginAsync(appUserId, cancellationToken);

            InvalidateIdentityScopedCaches();

            return new() { Value = loginResult.CustomerInfo.ToCustomerInfoDto() };
        }
        catch (Exception ex)
        {
            return new() { Error = LogAndMapError(ex), ErrorException = ex };
        }
    }

    /// <inheritdoc />
    public async partial Task<CustomerInfoResultDto> LogoutAsync(CancellationToken cancellationToken)
    {
        EnsureInitialized();

        try
        {
            var customerInfo = await Purchases.SharedPurchases.LogOutAsync(cancellationToken);

            InvalidateIdentityScopedCaches();

            return new() { Value = customerInfo.ToCustomerInfoDto() };
        }
        catch (Exception ex)
        {
            return new() { Error = LogAndMapError(ex), ErrorException = ex };
        }
    }

    /// <inheritdoc />
    public async partial Task<CustomerInfoResultDto> RestoreTransactionsAsync(CancellationToken cancellationToken)
    {
        EnsureInitialized();

        try
        {
            var customerInfo = await Purchases.SharedPurchases.RestorePurchasesAsync(cancellationToken);

            return new() { Value = customerInfo.ToCustomerInfoDto() };
        }
        catch (Exception ex)
        {
            return new() { Error = LogAndMapError(ex), ErrorException = ex };
        }
    }

    /// <inheritdoc />
    public async partial Task<CustomerInfoResultDto> GetCustomerInfoAsync(CancellationToken cancellationToken)
    {
        EnsureInitialized();

        try
        {
            var customerInfo = await Purchases.SharedPurchases.GetCustomerInfoAsync(cancellationToken);

            return new() { Value = customerInfo.ToCustomerInfoDto() };
        }
        catch (Exception ex)
        {
            return new() { Error = LogAndMapError(ex), ErrorException = ex };
        }
    }

    /// <inheritdoc />
    public partial void SetEmail(string email)
    {
        EnsureInitialized();

        Purchases.SharedPurchases.Attribution.SetEmail(email);
    }

    /// <inheritdoc />
    public partial void SetDisplayName(string name)
    {
        EnsureInitialized();

        Purchases.SharedPurchases.Attribution.SetDisplayName(name);
    }

    /// <inheritdoc />
    public partial void SetPhoneNumber(string phone)
    {
        EnsureInitialized();

        Purchases.SharedPurchases.Attribution.SetPhoneNumber(phone);
    }

    /// <inheritdoc />
    public partial void SetAttributes(IReadOnlyDictionary<string, string> attributes)
    {
        EnsureInitialized();

        var nsAttributes = attributes.ToNSDictionary();
        Purchases.SharedPurchases.Attribution.SetAttributes(nsAttributes);
    }

    internal static partial void EnableDebugLogs(bool enable)
    {
        if (!enable)
        {
            return;
        }

        Purchases.LogLevel = Enums.LogLevel.Debug.ToRCLogLevel();
    }

    private PurchaseErrorStatus LogAndMapError(Exception exception, [CallerMemberName] string operationName = "")
    {
        var errorStatus = exception switch
        {
            // StoreKit can signal cancellation with no NSError, leaving PurchasesErrorCode at its
            // default (UnknownError), so UserCancelled has to win over the code.
            PurchasesErrorException { UserCancelled: true } => PurchaseErrorStatus.PurchaseCancelledError,
            PurchasesErrorException purchasesErrorException => purchasesErrorException.PurchasesErrorCode.ToPurchaseErrorStatus(),
            OperationCanceledException => PurchaseErrorStatus.PurchaseCancelledError,
            _ => PurchaseErrorStatus.UnknownError,
        };

        if (errorStatus == PurchaseErrorStatus.PurchaseCancelledError)
        {
            _logger.LogDebug(exception, "{OperationName} was cancelled.", operationName);
        }
        else
        {
            _logger.LogError(exception, "{OperationName} failed.", operationName);
        }

        return errorStatus;
    }
}
