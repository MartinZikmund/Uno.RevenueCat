using Uno.RevenueCat.Enums;
using Uno.RevenueCat.Models;

namespace Uno.RevenueCat.Services;

/// <summary>
/// Interface for RevenueCat billing operations.
/// </summary>
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
    /// <returns>List of available offerings.</returns>
    Task<IReadOnlyList<OfferingDto>> GetOfferingsAsync(
        bool forceRefresh = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks trial or intro discount eligibility for the specified product identifiers.
    /// iOS only - returns empty dictionary on Android.
    /// </summary>
    /// <param name="productIdentifiers">The product identifiers to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Dictionary mapping product identifiers to their eligibility status.</returns>
    Task<IReadOnlyDictionary<string, IntroElegibilityStatus>> CheckTrialOrIntroDiscountEligibilityAsync(
        IEnumerable<string> productIdentifiers,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Purchases a product package.
    /// </summary>
    /// <param name="package">The package to purchase.</param>
    /// <param name="appWindow">Optional AppWindow, WindowId, or Activity for multi-window Android apps.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The purchase result.</returns>
    Task<PurchaseResultDto> PurchaseProductAsync(
        PackageDto package,
        object? appWindow = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the list of active subscription identifiers.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of active subscription identifiers.</returns>
    Task<IReadOnlyList<string>> GetActiveSubscriptionsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the list of all purchased product identifiers.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of all purchased product identifiers.</returns>
    Task<IReadOnlyList<string>> GetAllPurchasedIdentifiersAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the purchase date for a specific product identifier.
    /// </summary>
    /// <param name="productIdentifier">The product identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The purchase date, or null if not purchased.</returns>
    Task<DateTime?> GetPurchaseDateForProductIdentifierAsync(
        string productIdentifier,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the subscription management URL.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The management URL, or null if not available.</returns>
    Task<string?> GetManagementSubscriptionUrlAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current customer info.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The customer info, or null on failure.</returns>
    Task<CustomerInfoDto?> GetCustomerInfoAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs in with the specified app user ID.
    /// </summary>
    /// <param name="appUserId">The app user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The customer info after login, or null on failure.</returns>
    Task<CustomerInfoDto?> LoginAsync(
        string appUserId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs out the current user.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The customer info after logout, or null on failure.</returns>
    Task<CustomerInfoDto?> LogoutAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Restores transactions for the current user.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The customer info after restoration, or null on failure.</returns>
    Task<CustomerInfoDto?> RestoreTransactionsAsync(
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
