# Uno.RevenueCat

A .NET wrapper around [RevenueCat](https://www.revenuecat.com/) SDK for [Uno Platform](https://platform.uno/) applications, providing in-app purchase and subscription management for Android and iOS.

This library is based on [Kebechet/Maui.RevenueCat.InAppBilling](https://github.com/Kebechet/Maui.RevenueCat.InAppBilling) and adapted specifically for Uno Platform.

## Platform Support

| Platform | Status |
|----------|--------|
| Android | Supported (API 23+) |
| iOS | Supported (14.2+) |

Android requires API 23 (Android 6.0) or later, which is the floor of the RevenueCat Android SDK 10.x.

## Installation

```
dotnet add package MZikmund.Uno.RevenueCat
```

## Setup

### 1. Register the Service

Add the RevenueCat billing service to your dependency injection container:

```csharp
using Uno.RevenueCat;

// In your service configuration
services.AddRevenueCat();

// Or with debug logging enabled
services.AddRevenueCat(forceEnableDebugLogs: true);
```

### 2. Initialize the SDK

Initialize RevenueCat early in your app lifecycle. Inject `IRevenueCatBilling` and call `Initialize()`:

```csharp
public sealed partial class MainPage : Page
{
    private readonly IRevenueCatBilling _billing;

    public MainPage(IRevenueCatBilling billing)
    {
        _billing = billing;
        InitializeComponent();

        // Initialize with your RevenueCat API key
        _billing.Initialize("your_revenuecat_api_key");

        // Or initialize with a specific user ID
        _billing.Initialize("your_revenuecat_api_key", "user_id");
    }
}
```

> You can find your API keys in the [RevenueCat dashboard](https://app.revenuecat.com/) under Project Settings > API Keys.

## Usage

### Fetching Offerings

Retrieve available product offerings configured in RevenueCat:

```csharp
var offerings = await _billing.GetOfferingsAsync();

var currentOffering = offerings.GetCurrent();
if (currentOffering != null)
{
    foreach (var package in currentOffering.AvailablePackages)
    {
        var product = package.Product;
        Console.WriteLine($"{product.Sku}: {product.Pricing.PriceLocalized}");

        // Show the per-month cost of any plan, e.g. "$4.99" for a $59.88 annual package.
        Console.WriteLine(package.GetPriceWithCurrencyFor(PriceDuration.Monthly));
    }
}
```

### Reading Offering Metadata

Metadata configured in the RevenueCat dashboard is exposed as typed JSON:

```csharp
var offering = (await _billing.GetOfferingsAsync()).GetCurrent();

if (offering?.Metadata.TryGetValue("badge", out var badge) == true)
{
    Console.WriteLine(badge.GetString());
}
```

### Making a Purchase

```csharp
// Get a package to purchase
var offerings = await _billing.GetOfferingsAsync();
var package = offerings
    .FirstOrDefault(o => o.IsCurrent)?
    .AvailablePackages
    .FirstOrDefault(p => p.Identifier == "monthly");

if (package == null) return;

// Make the purchase
var result = await _billing.PurchaseProductAsync(package);

if (result.IsSuccess)
{
    // Purchase successful
    var transaction = result.Transaction;
    Console.WriteLine($"Purchased: {transaction?.ProductIdentifier}");
}
else if (result.ErrorStatus == PurchaseErrorStatus.PurchaseCancelledError)
{
    // User cancelled - not an error
}
else
{
    // Handle error
    Console.WriteLine($"Purchase failed: {result.ErrorStatus}");
}
```

### Checking Subscriptions

```csharp
// Get active subscriptions
var activeSubscriptions = await _billing.GetActiveSubscriptionsAsync();

// Get detailed customer info with entitlements
var customerInfo = await _billing.GetCustomerInfoAsync();
if (customerInfo != null)
{
    foreach (var entitlement in customerInfo.Entitlements)
    {
        if (entitlement.IsActive)
        {
            Console.WriteLine($"Active entitlement: {entitlement.Identifier}");
            Console.WriteLine($"Expires: {entitlement.ExpirationDate}");
        }
    }
}
```

### Restoring Purchases

```csharp
var customerInfo = await _billing.RestoreTransactionsAsync();
if (customerInfo != null)
{
    Console.WriteLine($"Restored {customerInfo.AllPurchasedIdentifiers.Count} purchases");
}
```

### User Management

```csharp
// Login with a user ID
var customerInfo = await _billing.LoginAsync("user_123");

// Check if user is anonymous
if (_billing.IsAnonymous)
{
    Console.WriteLine("User is anonymous");
}

// Set subscriber attributes
_billing.SetEmail("user@example.com");
_billing.SetDisplayName("John Doe");
_billing.SetPhoneNumber("+1234567890");

// Set custom attributes
_billing.SetAttributes(new Dictionary<string, string>
{
    { "favorite_color", "blue" }
});

// Logout
await _billing.LogoutAsync();
```

### Managing Subscriptions

```csharp
// Get URL to manage subscriptions (App Store / Play Store)
var managementUrl = await _billing.GetManagementSubscriptionUrlAsync();
if (!string.IsNullOrEmpty(managementUrl))
{
    // Open the URL in browser
    await Launcher.LaunchUriAsync(new Uri(managementUrl));
}
```

### Checking Trial Eligibility (iOS only)

```csharp
var eligibilities = await _billing.CheckTrialOrIntroDiscountEligibilityAsync(
    new[] { "product_monthly", "product_yearly" });

foreach (var (productId, status) in eligibilities)
{
    if (status == IntroElegibilityStatus.Eligible)
    {
        Console.WriteLine($"{productId} is eligible for intro offer");
    }
}
```

> Note: This method returns an empty dictionary on Android.

## API Reference

### IRevenueCatBilling

| Property | Description |
|----------|-------------|
| `IsInitialized` | Whether the SDK has been initialized |
| `IsAnonymous` | Whether the current user is anonymous |
| `AppUserId` | The current user's ID |

| Method | Description |
|--------|-------------|
| `Initialize(apiKey)` | Initialize with API key (anonymous user) |
| `Initialize(apiKey, appUserId)` | Initialize with API key and user ID |
| `CanMakePaymentsAsync()` | Whether the device can make payments |
| `GetOfferingsAsync()` | Fetch available offerings |
| `PurchaseProductAsync(package, appWindow)` | Purchase a package |
| `GetActiveSubscriptionsAsync()` | Get active subscription identifiers |
| `GetAllPurchasedIdentifiersAsync()` | Get all purchased product identifiers |
| `GetPurchaseDateForProductIdentifierAsync(id)` | Get the purchase date for a product |
| `GetCustomerInfoAsync()` | Get customer info with entitlements |
| `GetStorefrontCountryCodeAsync()` | Get the user's storefront country code |
| `LoginAsync(appUserId)` | Login with a user ID |
| `LogoutAsync()` | Logout current user |
| `RestoreTransactionsAsync()` | Restore previous purchases |
| `GetManagementSubscriptionUrlAsync()` | Get subscription management URL |
| `CheckTrialOrIntroDiscountEligibilityAsync()` | Check intro offer eligibility (iOS) |
| `SetEmail(email)` | Set subscriber email |
| `SetDisplayName(name)` | Set subscriber display name |
| `SetPhoneNumber(phone)` | Set subscriber phone number |
| `SetAttributes(attributes)` | Set custom subscriber attributes |

### Price display helpers

`PackageDtoExtensions` converts a package's price into any per-period figure, for paywalls that
show "billed annually, just $4.99/month":

| Method | Description |
|--------|-------------|
| `package.GetPriceFor(PriceDuration)` | The price normalized to a period, as a `decimal` |
| `package.GetPriceWithCurrencyFor(PriceDuration)` | The same, as a localized currency string |
| `PackageDtoExtensions.GetLocalizedPrice(isoCurrencyCode, price)` | Format any price as localized currency |

`PricingDto.PriceLocalized` is produced by `GetLocalizedPrice`, not by the store's own localized
price string. Number conventions follow `CultureInfo.CurrentCulture`; the currency symbol comes from
the currency itself. Whole prices drop the fractional part (`199 Kč`, not `199,00 Kč`). If your app
head sets `<InvariantGlobalization>true</InvariantGlobalization>`, no culture data is available and
the symbol falls back to the ISO code (`USD 9.99`).

## Error Handling

The library throws only on developer error — calling any method before `Initialize` raises
`InvalidOperationException`. Runtime failures never throw: they surface as
`PurchaseResultDto.ErrorStatus`, an empty collection, or `null`.

> The native `GetOfferings` call fails when the device is offline. `GetOfferingsAsync` returns an
> empty list in that case, so check connectivity before showing a paywall.

```csharp
var result = await _billing.PurchaseProductAsync(package);

if (result.IsError)
{
    switch (result.ErrorStatus)
    {
        case PurchaseErrorStatus.PurchaseCancelledError:
            // User cancelled - handle gracefully
            break;
        case PurchaseErrorStatus.NetworkError:
            // Network issue - prompt to retry
            break;
        case PurchaseErrorStatus.ProductAlreadyPurchasedError:
            // Already purchased - restore instead
            break;
        default:
            // Log and show generic error
            break;
    }
}
```

## Migrating from 0.1.x to 0.2.0

0.2.0 realigns the library with upstream `Maui.RevenueCat.InAppBilling` and fixes several
correctness bugs. It contains breaking changes.

| Before | After |
|--------|-------|
| `PricingDto.OriginalPrice`, `.OriginalPriceMicros`, `.OriginalPriceLocalized` | **Removed.** They were never populated on either platform. |
| `ProductDto.SubscriptionPeriod` (`string`) | `SubscriptionPeriodDto?` — `{ int Value; SubscriptionUnit Unit; }`, or `null` for non-subscription products. The old string was an unparseable debug rendering that differed per platform. |
| `OfferingDto.Metadata` (`string?`) | `IReadOnlyDictionary<string, JsonElement>` |
| `package.GetMonthlyPrice(...)` | `package.GetPriceFor(PriceDuration.Monthly, ...)` |
| `package.GetWeeklyPrice(...)` | `package.GetPriceFor(PriceDuration.Weekly, ...)` |
| `package.GetMonthlyPriceWithCurrency(...)` | `package.GetPriceWithCurrencyFor(PriceDuration.Monthly, ...)` |
| `package.GetWeeklyPriceWithCurrency(...)` | `package.GetPriceWithCurrencyFor(PriceDuration.Weekly, ...)` |
| `GetCurrent(this List<OfferingDto>)` | `GetCurrent(this IReadOnlyList<OfferingDto>)` — it now actually works on what `GetOfferingsAsync` returns. |
| Android `minSdkVersion` 21 | **23** |

Behavior changes worth knowing about, even though they are not API breaks:

- **Android purchase errors were previously wrong.** `PurchaseResultDto.ErrorStatus` was cast
  straight from the native integer code, but Android and iOS number the same conceptual error
  differently. Errors are now mapped by name. If you branched on `ErrorStatus` on Android, that code
  was matching the wrong cases and should be re-checked.
- **`PeriodType.Prepaid` was added.** A Google Play prepaid plan previously threw during customer-info
  mapping, which surfaced as a `null` customer (locking out a paying user).
- **`PriceLocalized` formatting changed.** Whole prices lose the `,00`, and separators now follow
  `CultureInfo.CurrentCulture`.
- **The subscription-management URL is no longer cached.** It used to be cached for the process
  lifetime with no invalidation, so after a user switch it returned the previous user's URL.

New in 0.2.0: `CanMakePaymentsAsync()`, `GetStorefrontCountryCodeAsync()`, `PeriodType.Prepaid`, and
the `StoreType` values `RcBilling`, `External`, `Paddle`, `TestStore`, `Galaxy`.

## Credits

- [RevenueCat](https://www.revenuecat.com/) - In-app purchase infrastructure
- [Kebechet/Maui.RevenueCat.InAppBilling](https://github.com/Kebechet/Maui.RevenueCat.InAppBilling) - Original MAUI wrapper this library is based on
- [Uno Platform](https://platform.uno/) - Cross-platform UI framework

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
