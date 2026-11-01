# Uno.RevenueCat

A .NET wrapper around [RevenueCat](https://www.revenuecat.com/) SDK for [Uno Platform](https://platform.uno/) applications, providing in-app purchase and subscription management for Android and iOS.

This library is based on [Kebechet/Maui.RevenueCat.InAppBilling](https://github.com/Kebechet/Maui.RevenueCat.InAppBilling) and adapted specifically for Uno Platform.

## Platform Support

| Platform | Status |
|----------|--------|
| Android | Supported (API 23+) |
| iOS | Supported (14.2+) |

Android requires API 23 (Android 6.0) or later, which is the floor of the RevenueCat Android SDK 10.x.

The package targets `net10.0-android` and `net10.0-ios` and is built on Uno Platform 7.0 (`Uno.Sdk`).
It wraps RevenueCat Android SDK 10.16.1 and iOS SDK 5.83.1.

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

Every async member returns a result object (`OfferingsResultDto`, `CustomerInfoResultDto`, ...)
with `IsSuccess`, `Value`, a typed `Error` and the raw `ErrorException`. Check `IsSuccess` before
reading `Value` - see [Error Handling](#error-handling).

### Fetching Offerings

Retrieve available product offerings configured in RevenueCat:

```csharp
var offeringsResult = await _billing.GetOfferingsAsync();
if (!offeringsResult.IsSuccess)
{
    // e.g. offeringsResult.Error == PurchaseErrorStatus.NetworkError while offline
    return;
}

var currentOffering = offeringsResult.Value!.GetCurrent();
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
var offering = (await _billing.GetOfferingsAsync()).Value?.GetCurrent();

if (offering?.Metadata.TryGetValue("badge", out var badge) == true)
{
    Console.WriteLine(badge.GetString());
}
```

### Making a Purchase

```csharp
// Get a package to purchase
var offerings = (await _billing.GetOfferingsAsync()).Value ?? [];
var package = offerings
    .GetCurrent()?
    .AvailablePackages
    .FirstOrDefault(p => p.Identifier == "monthly");

if (package == null) return;

// Make the purchase
var result = await _billing.PurchaseProductAsync(package);

if (result.IsSuccess)
{
    // Purchase successful; result.Value is the refreshed customer info
    Console.WriteLine($"Purchased: {result.Transaction?.ProductIdentifier}");
}
else if (result.Error == PurchaseErrorStatus.PurchaseCancelledError)
{
    // User dismissed the store sheet - not a fault
}
else if (result.Error == PurchaseErrorStatus.PaymentPendingError)
{
    // Accepted but not settled yet (Android PENDING, iOS Ask to Buy) - don't unlock yet
}
else
{
    Console.WriteLine($"Purchase failed: {result.Error}");
}
```

### Checking Subscriptions

```csharp
// Get active subscriptions
var activeSubscriptions = (await _billing.GetActiveSubscriptionsAsync()).Value ?? [];

// Get detailed customer info with entitlements
var customerInfoResult = await _billing.GetCustomerInfoAsync();
if (customerInfoResult.IsSuccess)
{
    foreach (var entitlement in customerInfoResult.Value!.Entitlements)
    {
        if (entitlement.IsActive)
        {
            Console.WriteLine($"Active entitlement: {entitlement.Identifier}");
            Console.WriteLine($"Expires: {entitlement.ExpirationDate}");
        }
    }
}
```

Customer info is cached by the SDK and can be stale. Pass a `CustomerInfoFetchPolicy` when the
entitlements may have changed outside the app, e.g. after a web purchase:

```csharp
var fresh = await _billing.GetCustomerInfoAsync(CustomerInfoFetchPolicy.FetchCurrent);
```

| Policy | Behavior |
|--------|----------|
| `CachedOrFetched` (default) | Cached data if any (even stale), otherwise fetch |
| `FetchCurrent` | Always fetch; fails when offline |
| `NotStaleCachedOrFetched` | Fresh cached data, otherwise fetch; fails rather than return stale data |
| `FromCacheOnly` | Never fetch; fails with `CustomerInfoError` when nothing is cached |

### Restoring Purchases

```csharp
var restoreResult = await _billing.RestoreTransactionsAsync();
if (restoreResult.IsSuccess)
{
    Console.WriteLine($"Restored {restoreResult.Value!.AllPurchasedIdentifiers.Count} purchases");
}
```

### User Management

```csharp
// Login with a user ID
var loginResult = await _billing.LoginAsync("user_123");

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
// A success with a null Value means the user has no store-managed subscription.
var managementUrl = (await _billing.GetManagementSubscriptionUrlAsync()).Value;
if (!string.IsNullOrEmpty(managementUrl))
{
    // Open the URL in browser
    await Launcher.LaunchUriAsync(new Uri(managementUrl));
}
```

### Checking Trial Eligibility (iOS only)

```csharp
var eligibilityResult = await _billing.CheckTrialOrIntroDiscountEligibilityAsync(
    new[] { "product_monthly", "product_yearly" });

foreach (var (productId, status) in eligibilityResult.Value ?? new Dictionary<string, IntroElegibilityStatus>())
{
    if (status == IntroElegibilityStatus.Eligible)
    {
        Console.WriteLine($"{productId} is eligible for intro offer");
    }
}
```

> Note: This method succeeds with an empty dictionary on Android.

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
| `GetCustomerInfoAsync(fetchPolicy)` | Get customer info with entitlements |
| `GetStorefrontCountryCodeAsync()` | Get the user's storefront country code (ISO alpha-2) |
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
`InvalidOperationException`. Runtime failures never throw. Every async member returns a
`DataResult<T, PurchaseErrorStatus>` (from [Kebechet.Types.Result](https://www.nuget.org/packages/Kebechet.Types.Result)):

| Member | Meaning |
|--------|---------|
| `IsSuccess` / `IsError` | Whether the call succeeded |
| `Value` | The result. Only meaningful when `IsSuccess` is `true` |
| `Error` | The documented failure mode to branch on |
| `ErrorException` | The raw store SDK exception, for logs - don't show it to users |

`PurchaseErrorStatus.PurchaseCancelledError` means the `CancellationToken` you passed fired, or, for
`PurchaseProductAsync`, that the user dismissed the store sheet.

> The native `GetOfferings` call fails when the device is offline, so `GetOfferingsAsync` returns
> an error result (typically `NetworkError`) rather than an empty list.

```csharp
var result = await _billing.PurchaseProductAsync(package);

if (result.IsError)
{
    switch (result.Error)
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

## Migrating from 0.2.x to 0.3.0

0.3.0 catches up with upstream `Maui.RevenueCat.InAppBilling` 10.0.0 and moves to .NET 10 and
Uno Platform 7.0. It contains breaking changes.

| Before | After |
|--------|-------|
| `net9.0-android` / `net9.0-ios` | `net10.0-android` / `net10.0-ios`, Uno.Sdk 7.0 |
| `Task<IReadOnlyList<OfferingDto>> GetOfferingsAsync()` | `Task<OfferingsResultDto>` - read `.Value` |
| `Task<CustomerInfoDto?>` from `GetCustomerInfoAsync`, `LoginAsync`, `LogoutAsync`, `RestoreTransactionsAsync` | `Task<CustomerInfoResultDto>` |
| `Task<IReadOnlyList<string>>` from `GetActiveSubscriptionsAsync`, `GetAllPurchasedIdentifiersAsync` | `Task<ProductIdentifiersResultDto>` |
| `Task<DateTime?> GetPurchaseDateForProductIdentifierAsync(...)` | `Task<PurchaseDateResultDto>` |
| `Task<string?> GetManagementSubscriptionUrlAsync()` | `Task<ManagementUrlResultDto>` |
| `Task<bool> CanMakePaymentsAsync()` | `Task<CanMakePaymentsResultDto>` |
| `Task<string> GetStorefrontCountryCodeAsync()` | `Task<StorefrontResultDto>` |
| `Task<IReadOnlyDictionary<...>> CheckTrialOrIntroDiscountEligibilityAsync(...)` | `Task<IntroEligibilityResultDto>` |
| `PurchaseResultDto.ErrorStatus` | `PurchaseResultDto.Error` |
| `PurchaseResultDto.CustomerInfo` | `PurchaseResultDto.Value` |
| `PurchaseResultDto` was a `sealed record` | It is a `class`: `==` is reference equality and `with` is gone |
| `CustomerInfoDto.ManagementURL` | `CustomerInfoDto.ManagementUrl` |
| `GetCustomerInfoAsync(cancellationToken)` | `GetCustomerInfoAsync(fetchPolicy, cancellationToken)` - pass the token by name |

Behavior changes worth knowing about:

- **Failures are distinguishable from empty results.** An offline `GetOfferingsAsync` used to return
  an empty list; it now returns an error result.
- **A pending purchase is an error, not a limbo.** A purchase that did not reach "purchased" used to
  return a result that was neither `IsSuccess` nor `IsError`. Android `PENDING` and iOS `Deferred`
  (Ask to Buy) now report `PaymentPendingError`; any other state reports `UnknownError`. Both still
  carry `Transaction` and `Value`.
- **The iOS storefront country code is ISO alpha-2** (`"US"`), matching Android. StoreKit's alpha-3
  (`"USA"`) used to leak through.
- **Android purchases launch on the UI thread.** Calling `PurchaseProductAsync` off the UI thread used to
  crash the RevenueCat Test Store with a `Looper.prepare()` error.

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
