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

    /// <inheritdoc />
    public partial bool IsAnonymous => Purchases.SharedInstance.IsAnonymous;

    /// <inheritdoc />
    public partial string AppUserId => Purchases.SharedInstance.AppUserID;

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
        if (!forceRefresh && _cachedOfferingPackages != null)
        {
            return _cachedOfferingPackages.ToOfferingDtoList();
        }

        try
        {
            _cachedOfferingPackages = await Purchases.SharedInstance.GetOfferingsAsync(cancellationToken);
            if (_cachedOfferingPackages is null)
            {
                return [];
            }

            return _cachedOfferingPackages.ToOfferingDtoList();
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
        if (!_isInitialized)
        {
            _logger.LogError("To call {MethodName} you firstly have to call Initialize method.", nameof(PurchaseProductAsync));
            throw new InvalidOperationException("RevenueCatBilling wasn't initialized");
        }

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
            var purchaseError = (PurchaseErrorStatus)(ex?.PurchasesError?.Code.Code ?? 0);

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
        if (!string.IsNullOrEmpty(_cachedManagementUrl))
        {
            return _cachedManagementUrl;
        }

        try
        {
            using var customerInfo = await Purchases.SharedInstance.GetCustomerInfoAsync(cancellationToken);
            if (customerInfo is null || customerInfo.ManagementURL is null)
            {
                return null;
            }

            _cachedManagementUrl = customerInfo.ManagementURL.ToString()!;
            return _cachedManagementUrl;
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
        try
        {
            var customerInfo = await Purchases.SharedInstance.LogInAsync(appUserId, cancellationToken);
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
        try
        {
            var customerInfo = await Purchases.SharedInstance.LogOutAsync(cancellationToken);
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
        Purchases.SharedInstance.SetEmail(email);
    }

    /// <inheritdoc />
    public partial void SetDisplayName(string name)
    {
        Purchases.SharedInstance.SetDisplayName(name);
    }

    /// <inheritdoc />
    public partial void SetPhoneNumber(string phone)
    {
        Purchases.SharedInstance.SetPhoneNumber(phone);
    }

    /// <inheritdoc />
    public partial void SetAttributes(IReadOnlyDictionary<string, string> attributes)
    {
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
