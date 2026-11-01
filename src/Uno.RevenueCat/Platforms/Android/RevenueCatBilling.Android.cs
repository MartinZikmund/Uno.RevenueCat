using System.Runtime.CompilerServices;
using Com.Revenuecat.Purchases;
using Com.Revenuecat.Purchases.Models;
using Microsoft.Extensions.Logging;
using Uno.RevenueCat.Enums;
using Uno.RevenueCat.Models;
using Uno.RevenueCat.Platforms.Android;
using Uno.RevenueCat.Platforms.Android.Exceptions;
using Uno.RevenueCat.Platforms.Android.Extensions;
using Uno.RevenueCat.Platforms.Android.Models;

namespace Uno.RevenueCat.Services;

public partial class RevenueCatBilling : IRevenueCatBilling
{
    private Purchases _purchases = default!;
    private Offerings? _cachedOfferingPackages = null;

    private partial void InvalidateIdentityScopedCaches()
    {
        Interlocked.Increment(ref _identityGeneration);
        _cachedOfferingPackages = null;
    }

    /// <inheritdoc />
    public partial bool IsAnonymous => Purchases.SharedInstance.IsAnonymous;

    /// <inheritdoc />
    public partial string AppUserId => Purchases.SharedInstance.AppUserID;

    /// <inheritdoc />
    public async partial Task<CanMakePaymentsResultDto> CanMakePaymentsAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Purchases.CanMakePayments is static and only needs a Context, so the application
            // context is enough - no Activity required.
            var context = ActivityResolver.GetApplicationContext();
            return new() { Value = await PurchasesExtensions.CanMakePaymentsAsync(context, cancellationToken) };
        }
        catch (Exception ex)
        {
            return new() { Error = LogAndMapError(ex), ErrorException = ex };
        }
    }

    /// <inheritdoc />
    public async partial Task<StorefrontResultDto> GetStorefrontCountryCodeAsync(CancellationToken cancellationToken)
    {
        EnsureInitialized();

        try
        {
            return new() { Value = await Purchases.SharedInstance.GetStorefrontCountryCodeAsync(cancellationToken) };
        }
        catch (Exception ex)
        {
            return new() { Error = LogAndMapError(ex), ErrorException = ex };
        }
    }

    /// <inheritdoc />
    public partial void Initialize(string apiKey)
    {
        var context = ActivityResolver.GetApplicationContext();

        try
        {
            _purchases = Purchases.Configure(
                new PurchasesConfiguration(
                    new PurchasesConfiguration.Builder(context, apiKey)
                )
            );

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
        var context = ActivityResolver.GetApplicationContext();

        try
        {
            _purchases = Purchases.Configure(
                new PurchasesConfiguration(
                    new PurchasesConfiguration.Builder(context, apiKey)
                        .AppUserID(appUserId)
                )
            );

            _isInitialized = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Initialization exception");
            throw;
        }
    }

    /// <inheritdoc />
    public partial Task<IntroEligibilityResultDto> CheckTrialOrIntroDiscountEligibilityAsync(
        IEnumerable<string> productIdentifiers,
        CancellationToken cancellationToken) =>
        // iOS only - the RevenueCat Android SDK has no equivalent.
        Task.FromResult(new IntroEligibilityResultDto { Value = new Dictionary<string, IntroElegibilityStatus>() });

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
            var offerings = await Purchases.SharedInstance.GetOfferingsAsync(cancellationToken);

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
        EnsureInitialized();

        var activity = ActivityResolver.GetActivity(appWindow);

        if (_cachedOfferingPackages is null)
        {
            throw new InvalidOperationException("GetOfferingsAsync must be called prior to purchasing a product.");
        }

        var offeringToBuy = _cachedOfferingPackages.GetOffering(package.OfferingIdentifier);
        if (offeringToBuy is null)
        {
            _logger.LogError("No offering with identifier: {OfferingIdentifier} found. Make sure you called GetOfferingsAsync before.", package.OfferingIdentifier);
            throw new InvalidOperationException($"No offering with identifier: {package.OfferingIdentifier} found. Make sure you called GetOfferingsAsync before.");
        }

        var packageToBuy = offeringToBuy.AvailablePackages.FirstOrDefault(p => p.Identifier == package.Identifier);
        if (packageToBuy is null)
        {
            _logger.LogError("No package with identifier: {PackageIdentifier} found. Make sure you called GetOfferingsAsync before.", package.Identifier);
            throw new InvalidOperationException($"No package with identifier: {package.Identifier} found. Make sure you called GetOfferingsAsync before.");
        }

        PurchaseSuccessInfo? purchaseSuccessInfo;

        try
        {
            purchaseSuccessInfo = await _purchases.PurchaseAsync(activity, packageToBuy, cancellationToken);
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

        var transaction = purchaseSuccessInfo.StoreTransaction.ToStoreTransactionDto();
        var customerInfo = purchaseSuccessInfo.CustomerInfo.ToCustomerInfoDto();
        var purchaseState = purchaseSuccessInfo.StoreTransaction.PurchaseState;

        if (purchaseState != PurchaseState.Purchased)
        {
            // Only PENDING means "awaiting a slow payment method". UNSPECIFIED_STATE is Play Billing
            // not knowing the state, so "check back later" would be a lie.
            var errorStatus = purchaseState == PurchaseState.Pending
                ? PurchaseErrorStatus.PaymentPendingError
                : PurchaseErrorStatus.UnknownError;

            _logger.LogWarning(
                "{OperationName} returned a transaction in state {TransactionState}, reported as {ErrorStatus}.",
                nameof(PurchaseProductAsync),
                purchaseState?.ToString() ?? "unknown",
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
            using var customerInfo = await Purchases.SharedInstance.GetCustomerInfoAsync(cancellationToken);
            if (customerInfo?.ActiveSubscriptions is null)
            {
                return new() { Value = [] };
            }

            return new() { Value = customerInfo.ActiveSubscriptions.ToList() };
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
            using var customerInfo = await Purchases.SharedInstance.GetCustomerInfoAsync(cancellationToken);
            if (customerInfo?.AllPurchasedProductIds is null)
            {
                return new() { Value = [] };
            }

            return new() { Value = customerInfo.AllPurchasedProductIds.ToList() };
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
            using var customerInfo = await Purchases.SharedInstance.GetCustomerInfoAsync(cancellationToken);

            return new() { Value = customerInfo?.GetPurchaseDateForProductId(productIdentifier).ToDateTime() };
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
            using var customerInfo = await Purchases.SharedInstance.GetCustomerInfoAsync(cancellationToken);

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
            var customerInfo = await Purchases.SharedInstance.LogInAsync(appUserId, cancellationToken);

            InvalidateIdentityScopedCaches();

            return new() { Value = customerInfo.ToCustomerInfoDto() };
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
            var customerInfo = await Purchases.SharedInstance.LogOutAsync(cancellationToken);

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
            var customerInfo = await Purchases.SharedInstance.RestorePurchasesAsync(cancellationToken);

            return new() { Value = customerInfo.ToCustomerInfoDto() };
        }
        catch (Exception ex)
        {
            return new() { Error = LogAndMapError(ex), ErrorException = ex };
        }
    }

    /// <inheritdoc />
    public async partial Task<CustomerInfoResultDto> GetCustomerInfoAsync(
        CustomerInfoFetchPolicy fetchPolicy,
        CancellationToken cancellationToken)
    {
        EnsureInitialized();

        try
        {
            var customerInfo = await Purchases.SharedInstance.GetCustomerInfoAsync(cancellationToken, fetchPolicy);

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

        Purchases.SharedInstance.SetEmail(email);
    }

    /// <inheritdoc />
    public partial void SetDisplayName(string name)
    {
        EnsureInitialized();

        Purchases.SharedInstance.SetDisplayName(name);
    }

    /// <inheritdoc />
    public partial void SetPhoneNumber(string phone)
    {
        EnsureInitialized();

        Purchases.SharedInstance.SetPhoneNumber(phone);
    }

    /// <inheritdoc />
    public partial void SetAttributes(IReadOnlyDictionary<string, string> attributes)
    {
        EnsureInitialized();

        Purchases.SharedInstance.SetAttributes(new Dictionary<string, string>(attributes));
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
            // UserCancelled is authoritative: a missing PurchasesError or Code would otherwise
            // degrade a cancellation into UnknownError.
            PurchasesErrorException { UserCancelled: true } => PurchaseErrorStatus.PurchaseCancelledError,
            PurchasesErrorException purchasesErrorException =>
                purchasesErrorException.PurchasesError?.Code?.ToPurchaseErrorStatus() ?? PurchaseErrorStatus.UnknownError,
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
