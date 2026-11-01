using Types.Result;
using Uno.RevenueCat.Enums;
using Uno.RevenueCat.Models;

namespace Uno.RevenueCat.Services;

/// <summary>
/// Interface for RevenueCat billing operations.
/// </summary>
/// <remarks>
/// Every asynchronous member returns a <see cref="DataResult{TValue, TError}"/> keyed on
/// <see cref="PurchaseErrorStatus"/>. Read <c>Value</c> only when <c>IsSuccess</c> is <c>true</c>.
/// On failure, <c>Error</c> is the documented failure mode to branch on, and <c>ErrorException</c>
/// carries the raw store SDK exception for logs - do not show it to users.
/// <para>
/// Developer mistakes, such as calling a member before <see cref="Initialize(string)"/>, still throw.
/// </para>
/// <para>
/// <see cref="PurchaseErrorStatus.PurchaseCancelledError"/> means the supplied
/// <see cref="CancellationToken"/> fired - except on <see cref="PurchaseProductAsync"/>, where it
/// also means the user dismissed the store sheet.
/// </para>
/// </remarks>
public interface IRevenueCatBilling
{
    /// <summary>
    /// Gets whether the SDK has been initialized.
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// Gets whether the current user is anonymous.
    /// </summary>
    bool IsAnonymous { get; }

    /// <summary>
    /// Gets the current app user ID.
    /// </summary>
    string AppUserId { get; }

    /// <summary>
    /// Determines whether the device is able to make payments (Play Billing / StoreKit available).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> in <c>Value</c> if payments can be made.</returns>
    Task<CanMakePaymentsResultDto> CanMakePaymentsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the ISO country code of the user's storefront (e.g. "US", "CZ").
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// The ISO 3166-1 alpha-2 storefront country code, or <see cref="string.Empty"/> when the
    /// platform has not observed the storefront yet.
    /// </returns>
    Task<StorefrontResultDto> GetStorefrontCountryCodeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Initializes the RevenueCat SDK with the specified API key.
    /// </summary>
    /// <param name="apiKey">The RevenueCat API key.</param>
    void Initialize(string apiKey);

    /// <summary>
    /// Initializes the RevenueCat SDK with the specified API key and user ID.
    /// </summary>
    /// <param name="apiKey">The RevenueCat API key.</param>
    /// <param name="appUserId">The app user ID to identify the user.</param>
    void Initialize(string apiKey, string appUserId);

    /// <summary>
    /// Gets the available offerings.
    /// </summary>
    /// <param name="forceRefresh">Whether to force a refresh from the server.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The available offerings.</returns>
    Task<OfferingsResultDto> GetOfferingsAsync(
        bool forceRefresh = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks trial or intro discount eligibility for the specified product identifiers.
    /// iOS only - succeeds with an empty dictionary on Android.
    /// </summary>
    /// <param name="productIdentifiers">The product identifiers to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Product identifiers mapped to their eligibility status.</returns>
    Task<IntroEligibilityResultDto> CheckTrialOrIntroDiscountEligibilityAsync(
        IEnumerable<string> productIdentifiers,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Purchases a product package.
    /// </summary>
    /// <param name="package">The package to purchase.</param>
    /// <param name="appWindow">Optional AppWindow, WindowId, or Activity for multi-window Android apps.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The purchase result.</returns>
    /// <remarks>
    /// <see cref="PurchaseErrorStatus.PurchaseCancelledError"/> here usually means the user dismissed
    /// the store sheet - an expected outcome, not a fault. <see cref="PurchaseErrorStatus.PaymentPendingError"/>
    /// means the store accepted the purchase but it has not settled yet (Android PENDING, iOS Deferred);
    /// the result still carries the transaction and customer info.
    /// </remarks>
    Task<PurchaseResultDto> PurchaseProductAsync(
        PackageDto package,
        object? appWindow = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the list of active subscription identifiers.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Active subscription identifiers.</returns>
    Task<ProductIdentifiersResultDto> GetActiveSubscriptionsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the list of all purchased product identifiers.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>All purchased product identifiers.</returns>
    Task<ProductIdentifiersResultDto> GetAllPurchasedIdentifiersAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the purchase date for a specific product identifier.
    /// </summary>
    /// <param name="productIdentifier">The product identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The purchase date. A success with a <c>null</c> value means the product was never purchased.</returns>
    Task<PurchaseDateResultDto> GetPurchaseDateForProductIdentifierAsync(
        string productIdentifier,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the subscription management URL.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The management URL. A success with a <c>null</c> value means the user has no store-managed subscription.</returns>
    Task<ManagementUrlResultDto> GetManagementSubscriptionUrlAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current customer info.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The customer info.</returns>
    Task<CustomerInfoResultDto> GetCustomerInfoAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs in with the specified app user ID.
    /// </summary>
    /// <param name="appUserId">The app user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The customer info after login.</returns>
    Task<CustomerInfoResultDto> LoginAsync(
        string appUserId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs out the current user.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The customer info after logout.</returns>
    Task<CustomerInfoResultDto> LogoutAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Restores transactions for the current user.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The customer info after restoration.</returns>
    Task<CustomerInfoResultDto> RestoreTransactionsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the email for the current user.
    /// </summary>
    /// <param name="email">The email address.</param>
    void SetEmail(string email);

    /// <summary>
    /// Sets the display name for the current user.
    /// </summary>
    /// <param name="name">The display name.</param>
    void SetDisplayName(string name);

    /// <summary>
    /// Sets the phone number for the current user.
    /// </summary>
    /// <param name="phone">The phone number.</param>
    void SetPhoneNumber(string phone);

    /// <summary>
    /// Sets custom attributes for the current user.
    /// </summary>
    /// <param name="attributes">The attributes to set.</param>
    void SetAttributes(IReadOnlyDictionary<string, string> attributes);
}
