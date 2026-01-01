using Maui.RevenueCat.iOS;
using Microsoft.Extensions.Logging;
using Uno.RevenueCat.Enums;
using Uno.RevenueCat.Models;
using Uno.RevenueCat.Platforms.iOS.Exceptions;
using Uno.RevenueCat.Platforms.iOS.Extensions;
using Uno.RevenueCat.Platforms.iOS.Models;
using Purchases = Maui.RevenueCat.iOS.RCPurchases;

namespace Uno.RevenueCat.Services;

public partial class RevenueCatBilling : IRevenueCatBilling
{
    private Purchases _purchases = default!;
    private RCOfferings? _cachedOfferingPackages = null;

    /// <inheritdoc />
    public partial bool IsAnonymous => Purchases.SharedPurchases.IsAnonymous;

    /// <inheritdoc />
    public partial string AppUserId => Purchases.SharedPurchases.AppUserID;

    /// <inheritdoc />
    public partial void Initialize(string apiKey)
    {
        try
        {
            _purchases = Purchases.ConfigureWithAPIKey(apiKey);
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
            _purchases = Purchases.ConfigureWithAPIKey(apiKey, appUserId);
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
        try
        {
            using var eligibilities = await _purchases.CheckTrialOrIntroDiscountEligibilityAsync(productIdentifiers, cancellationToken);
            if (eligibilities is null || eligibilities.Count == 0)
            {
                return new Dictionary<string, IntroElegibilityStatus>();
            }

            var eligibilitiesResult = new Dictionary<string, IntroElegibilityStatus>();

            for (ulong i = 0; i < eligibilities.Count; i++)
            {
                eligibilitiesResult.Add(eligibilities.Keys[i], eligibilities.Values[i].Status.Convert());
            }

            return eligibilitiesResult;
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogDebug(ex, "{MethodName} was cancelled.", nameof(CheckTrialOrIntroDiscountEligibilityAsync));
            return new Dictionary<string, IntroElegibilityStatus>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{MethodName} didn't succeed.", nameof(CheckTrialOrIntroDiscountEligibilityAsync));
            return new Dictionary<string, IntroElegibilityStatus>();
        }
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
            _cachedOfferingPackages = await _purchases.GetOfferingsAsync(cancellationToken);
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
        // appWindow is ignored on iOS - no Activity needed
        if (!_isInitialized)
        {
            throw new InvalidOperationException("RevenueCatBilling wasn't initialized");
        }

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

        PurchaseSuccessInfo? purchaseSuccessInfo = null;

        try
        {
            purchaseSuccessInfo = await _purchases.PurchasePackageAsync(packageToBuy, cancellationToken);
        }
        catch (PurchasesErrorException ex)
        {
            var purchaseError = (PurchaseErrorStatus)(int)(ex?.PurchasesError?.Code ?? 0);

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

        var isSuccess = purchaseSuccessInfo.StoreTransaction.Sk1Transaction is not null
            ? purchaseSuccessInfo.StoreTransaction.Sk1Transaction.TransactionState == StoreKit.SKPaymentTransactionState.Purchased
            : !string.IsNullOrEmpty(purchaseSuccessInfo.StoreTransaction.TransactionIdentifier);

        return new PurchaseResultDto
        {
            IsSuccess = isSuccess,
            Transaction = purchaseSuccessInfo.StoreTransaction.ToStoreTransactionDto(),
            CustomerInfo = purchaseSuccessInfo.CustomerInfo.ToCustomerInfoDto()
        };
    }

    /// <inheritdoc />
    public async partial Task<IReadOnlyList<string>> GetActiveSubscriptionsAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var customerInfo = await _purchases.GetCustomerInfoAsync(cancellationToken);
            if (customerInfo is null)
            {
                return [];
            }

            var subscriptions = customerInfo.ActiveSubscriptions.ToStringList();
            if (subscriptions.Count == 0)
            {
                return [];
            }

            return subscriptions;
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
            using var customerInfo = await _purchases.GetCustomerInfoAsync(cancellationToken);
            if (customerInfo is null)
            {
                return [];
            }

            var identifiers = customerInfo.AllPurchasedProductIdentifiers.ToStringList();
            if (identifiers.Count == 0)
            {
                return [];
            }

            return identifiers;
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
            using var customerInfo = await _purchases.GetCustomerInfoAsync(cancellationToken);
            if (customerInfo is null)
            {
                return null;
            }

            return customerInfo.PurchaseDateForProductIdentifier(productIdentifier).ToDateTime();
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
            using var customerInfo = await _purchases.GetCustomerInfoAsync(cancellationToken);
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
            var loginResult = await Purchases.SharedPurchases.LoginAsync(appUserId, cancellationToken);
            var customerInfo = loginResult.CustomerInfo;

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
            var customerInfo = await Purchases.SharedPurchases.LogOutAsync(cancellationToken);

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
            var customerInfo = await Purchases.SharedPurchases.RestorePurchasesAsync(cancellationToken);

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
            var customerInfo = await Purchases.SharedPurchases.GetCustomerInfoAsync(cancellationToken);

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
        Purchases.SharedPurchases.Attribution.SetEmail(email);
    }

    /// <inheritdoc />
    public partial void SetDisplayName(string name)
    {
        Purchases.SharedPurchases.Attribution.SetDisplayName(name);
    }

    /// <inheritdoc />
    public partial void SetPhoneNumber(string phone)
    {
        Purchases.SharedPurchases.Attribution.SetPhoneNumber(phone);
    }

    /// <inheritdoc />
    public partial void SetAttributes(IReadOnlyDictionary<string, string> attributes)
    {
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
}
