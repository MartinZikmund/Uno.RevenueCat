using Microsoft.Extensions.Logging;
using Uno.RevenueCat.Enums;
using Uno.RevenueCat.Models;

namespace Uno.RevenueCat.Services;

/// <summary>
/// RevenueCat billing service implementation.
/// </summary>
public partial class RevenueCatBilling : IRevenueCatBilling
{
    private readonly ILogger<RevenueCatBilling> _logger;

    private static bool _isInstanceCreated = false;
    private volatile bool _isInitialized = false;
    private string _cachedManagementUrl = string.Empty;

    /// <summary>
    /// Creates a new instance of the RevenueCatBilling service.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <exception cref="InvalidOperationException">Thrown if an instance already exists.</exception>
    public RevenueCatBilling(ILogger<RevenueCatBilling> logger)
    {
        if (_isInstanceCreated)
        {
            throw new InvalidOperationException("You shouldn't create more instances of class RevenueCatBilling.");
        }

        _logger = logger;
        _isInstanceCreated = true;
    }

    /// <inheritdoc />
    public bool IsInitialized => _isInitialized;

    /// <inheritdoc />
    public partial bool IsAnonymous { get; }

    /// <inheritdoc />
    public partial string AppUserId { get; }

    /// <inheritdoc />
    public partial void Initialize(string apiKey);

    /// <inheritdoc />
    public partial void Initialize(string apiKey, string appUserId);

    /// <inheritdoc />
    public partial Task<IReadOnlyList<OfferingDto>> GetOfferingsAsync(
        bool forceRefresh,
        CancellationToken cancellationToken);

    /// <inheritdoc />
    public partial Task<IReadOnlyDictionary<string, IntroElegibilityStatus>> CheckTrialOrIntroDiscountEligibilityAsync(
        IEnumerable<string> productIdentifiers,
        CancellationToken cancellationToken);

    /// <inheritdoc />
    public partial Task<PurchaseResultDto> PurchaseProductAsync(
        PackageDto package,
        object? appWindow,
        CancellationToken cancellationToken);

    /// <inheritdoc />
    public partial Task<IReadOnlyList<string>> GetActiveSubscriptionsAsync(
        CancellationToken cancellationToken);

    /// <inheritdoc />
    public partial Task<IReadOnlyList<string>> GetAllPurchasedIdentifiersAsync(
        CancellationToken cancellationToken);

    /// <inheritdoc />
    public partial Task<DateTime?> GetPurchaseDateForProductIdentifierAsync(
        string productIdentifier,
        CancellationToken cancellationToken);

    /// <inheritdoc />
    public partial Task<string?> GetManagementSubscriptionUrlAsync(
        CancellationToken cancellationToken);

    /// <inheritdoc />
    public partial Task<CustomerInfoDto?> GetCustomerInfoAsync(
        CancellationToken cancellationToken);

    /// <inheritdoc />
    public partial Task<CustomerInfoDto?> LoginAsync(
        string appUserId,
        CancellationToken cancellationToken);

    /// <inheritdoc />
    public partial Task<CustomerInfoDto?> LogoutAsync(
        CancellationToken cancellationToken);

    /// <inheritdoc />
    public partial Task<CustomerInfoDto?> RestoreTransactionsAsync(
        CancellationToken cancellationToken);

    /// <inheritdoc />
    public partial void SetEmail(string email);

    /// <inheritdoc />
    public partial void SetDisplayName(string name);

    /// <inheritdoc />
    public partial void SetPhoneNumber(string phone);

    /// <inheritdoc />
    public partial void SetAttributes(IReadOnlyDictionary<string, string> attributes);

    /// <summary>
    /// Enables or disables debug logging.
    /// </summary>
    /// <param name="enable">Whether to enable debug logs.</param>
    internal static partial void EnableDebugLogs(bool enable);
}
