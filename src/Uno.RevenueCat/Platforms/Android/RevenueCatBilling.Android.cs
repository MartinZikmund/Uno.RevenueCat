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
    public async partial Task<bool> CanMakePaymentsAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Purchases.CanMakePayments is static and only needs a Context, so the application
            // context is enough - no Activity required.
            var context = ActivityResolver.GetApplicationContext();
            return await PurchasesExtensions.CanMakePaymentsAsync(context, cancellationToken);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogDebug(ex, "{MethodName} was cancelled.", nameof(CanMakePaymentsAsync));
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{MethodName} didn't succeed.", nameof(CanMakePaymentsAsync));
            return false;
        }
    }

    /// <inheritdoc />
    public async partial Task<string> GetStorefrontCountryCodeAsync(CancellationToken cancellationToken)
    {
        EnsureInitialized();

        try
        {
            return await Purchases.SharedInstance.GetStorefrontCountryCodeAsync(cancellationToken);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogDebug(ex, "{MethodName} was cancelled.", nameof(GetStorefrontCountryCodeAsync));
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{MethodName} didn't succeed.", nameof(GetStorefrontCountryCodeAsync));
            return string.Empty;
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
    public async partial Task<IReadOnlyDictionary<string, IntroElegibilityStatus>> CheckTrialOrIntroDiscountEligibilityAsync(
        IEnumerable<string> productIdentifiers,
        CancellationToken cancellationToken)
    {
        // This method is iOS only
        await Task.CompletedTask;
        return new Dictionary<string, IntroElegibilityStatus>();
    }

    /// <inheritdoc />
    public async partial Task<IReadOnlyList<OfferingDto>> GetOfferingsAsync(
        bool forceRefresh,
        CancellationToken cancellationToken)
    {
        EnsureInitialized();

        if (!forceRefresh && _cachedOfferingPackages != null)
        {
            return _cachedOfferingPackages.ToOfferingDtoList();
        }

        try
        {
            var generation = Volatile.Read(ref _identityGeneration);
            var offerings = await Purchases.SharedInstance.GetOfferingsAsync(cancellationToken);

            if (offerings is null)
            {
                return [];
            }

            // Drop the result if the user changed while the fetch was in flight.
            if (Volatile.Read(ref _identityGeneration) == generation)
            {
                _cachedOfferingPackages = offerings;
            }

            return offerings.ToOfferingDtoList();
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogDebug(ex, "{MethodName} was cancelled.", nameof(GetOfferingsAsync));
            return [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{MethodName} didn't succeed.", nameof(GetOfferingsAsync));
            return [];
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

        PurchaseSuccessInfo? purchaseSuccessInfo = null;

        try
        {
            purchaseSuccessInfo = await _purchases.PurchaseAsync(activity, packageToBuy, cancellationToken);
        }
        catch (PurchasesErrorException ex)
        {
            var errorCode = ex?.PurchasesError?.Code;
            var purchaseError = errorCode is null
                ? PurchaseErrorStatus.UnknownError
                : errorCode.ToPurchaseErrorStatus();

            if (purchaseError != PurchaseErrorStatus.PurchaseCancelledError)
            {
                _logger.LogError(ex, "PurchasesErrorException");
            }

            return new PurchaseResultDto
            {
                ErrorStatus = purchaseError
            };
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogDebug(ex, "{MethodName} was cancelled.", nameof(PurchaseProductAsync));
            return new PurchaseResultDto
            {
                ErrorStatus = PurchaseErrorStatus.PurchaseCancelledError
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {MethodName}", nameof(PurchaseProductAsync));

            return new PurchaseResultDto
            {
                ErrorStatus = PurchaseErrorStatus.UnknownError
            };
        }

        if (purchaseSuccessInfo is null)
        {
            _logger.LogError("{VariableName} is null.", nameof(purchaseSuccessInfo));

            return new PurchaseResultDto
            {
                ErrorStatus = PurchaseErrorStatus.UnknownError
            };
        }

        return new PurchaseResultDto
        {
            IsSuccess = purchaseSuccessInfo.StoreTransaction.PurchaseState == PurchaseState.Purchased,
            Transaction = purchaseSuccessInfo.StoreTransaction.ToStoreTransactionDto(),
            CustomerInfo = purchaseSuccessInfo.CustomerInfo.ToCustomerInfoDto()
        };
    }

    /// <inheritdoc />
    public async partial Task<IReadOnlyList<string>> GetActiveSubscriptionsAsync(CancellationToken cancellationToken)
    {
        EnsureInitialized();

        try
        {
            using var customerInfo = await Purchases.SharedInstance.GetCustomerInfoAsync(cancellationToken);
            if (customerInfo is null)
            {
                return [];
            }

            if (customerInfo.ActiveSubscriptions is null || customerInfo.ActiveSubscriptions.Count == 0)
            {
                return [];
            }

            return customerInfo.ActiveSubscriptions.ToList();
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogDebug(ex, "{MethodName} was cancelled.", nameof(GetActiveSubscriptionsAsync));
            return [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Couldn't retrieve active subscriptions.");
            return [];
        }
    }

    /// <inheritdoc />
    public async partial Task<IReadOnlyList<string>> GetAllPurchasedIdentifiersAsync(CancellationToken cancellationToken)
    {
        EnsureInitialized();

        try
        {
            using var customerInfo = await Purchases.SharedInstance.GetCustomerInfoAsync(cancellationToken);
            if (customerInfo is null)
            {
                return [];
            }

            if (customerInfo.AllPurchasedProductIds is null || customerInfo.AllPurchasedProductIds.Count == 0)
            {
                return [];
            }

            return customerInfo.AllPurchasedProductIds.ToList();
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogDebug(ex, "{MethodName} was cancelled.", nameof(GetAllPurchasedIdentifiersAsync));
            return [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Couldn't retrieve all purchased identifiers.");
            return [];
        }
    }

    /// <inheritdoc />
    public async partial Task<DateTime?> GetPurchaseDateForProductIdentifierAsync(
        string productIdentifier,
        CancellationToken cancellationToken)
    {
        EnsureInitialized();

        try
        {
            using var customerInfo = await Purchases.SharedInstance.GetCustomerInfoAsync(cancellationToken);
            if (customerInfo is null)
            {
                return null;
            }

            return customerInfo.GetPurchaseDateForProductId(productIdentifier).ToDateTime();
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogDebug(ex, "{MethodName} was cancelled.", nameof(GetPurchaseDateForProductIdentifierAsync));
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Couldn't retrieve purchase date.");
            return null;
        }
    }

    /// <inheritdoc />
    public async partial Task<string?> GetManagementSubscriptionUrlAsync(CancellationToken cancellationToken)
    {
        EnsureInitialized();

        try
        {
            using var customerInfo = await Purchases.SharedInstance.GetCustomerInfoAsync(cancellationToken);
            if (customerInfo is null || customerInfo.ManagementURL is null)
            {
                return null;
            }

            return customerInfo.ManagementURL.ToString();
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogDebug(ex, "{MethodName} was cancelled.", nameof(GetManagementSubscriptionUrlAsync));
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Couldn't retrieve management url.");
            return null;
        }
    }

    /// <inheritdoc />
    public async partial Task<CustomerInfoDto?> LoginAsync(string appUserId, CancellationToken cancellationToken)
    {
        EnsureInitialized();

        try
        {
            var customerInfo = await Purchases.SharedInstance.LogInAsync(appUserId, cancellationToken);

            InvalidateIdentityScopedCaches();

            return customerInfo.ToCustomerInfoDto();
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogDebug(ex, "{MethodName} was cancelled.", nameof(LoginAsync));
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{MethodName} failed.", nameof(LoginAsync));
            return null;
        }
    }

    /// <inheritdoc />
    public async partial Task<CustomerInfoDto?> LogoutAsync(CancellationToken cancellationToken)
    {
        EnsureInitialized();

        try
        {
            var customerInfo = await Purchases.SharedInstance.LogOutAsync(cancellationToken);

            InvalidateIdentityScopedCaches();

            return customerInfo.ToCustomerInfoDto();
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogDebug(ex, "{MethodName} was cancelled.", nameof(LogoutAsync));
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{MethodName} failed.", nameof(LogoutAsync));
            return null;
        }
    }

    /// <inheritdoc />
    public async partial Task<CustomerInfoDto?> RestoreTransactionsAsync(CancellationToken cancellationToken)
    {
        EnsureInitialized();

        try
        {
            var customerInfo = await Purchases.SharedInstance.RestorePurchasesAsync(cancellationToken);
            return customerInfo.ToCustomerInfoDto();
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogDebug(ex, "{MethodName} was cancelled.", nameof(RestoreTransactionsAsync));
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{MethodName} failed.", nameof(RestoreTransactionsAsync));
            return null;
        }
    }

    /// <inheritdoc />
    public async partial Task<CustomerInfoDto?> GetCustomerInfoAsync(CancellationToken cancellationToken)
    {
        EnsureInitialized();

        try
        {
            var customerInfo = await Purchases.SharedInstance.GetCustomerInfoAsync(cancellationToken);
            return customerInfo.ToCustomerInfoDto();
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogDebug(ex, "{MethodName} was cancelled.", nameof(GetCustomerInfoAsync));
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{MethodName} failed.", nameof(GetCustomerInfoAsync));
            return null;
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
}
