# Uno.RevenueCat

A .NET wrapper around [RevenueCat](https://www.revenuecat.com/) SDK for [Uno Platform](https://platform.uno/) applications, providing in-app purchase and subscription management for Android and iOS.

This library is based on [Kebechet/Maui.RevenueCat.InAppBilling](https://github.com/Kebechet/Maui.RevenueCat.InAppBilling) and adapted specifically for Uno Platform.

## Platform Support

| Platform | Status |
|----------|--------|
| Android | Supported (API 21+) |
| iOS | Supported (14.2+) |

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

var currentOffering = offerings.FirstOrDefault(o => o.IsCurrent);
if (currentOffering != null)
{
    foreach (var package in currentOffering.AvailablePackages)
    {
        var product = package.Product;
        Console.WriteLine($"{product.Sku}: {product.Pricing.PriceLocalized}");
    }
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
| `GetOfferingsAsync()` | Fetch available offerings |
| `PurchaseProductAsync(package)` | Purchase a package |
| `GetActiveSubscriptionsAsync()` | Get active subscription identifiers |
| `GetAllPurchasedIdentifiersAsync()` | Get all purchased product identifiers |
| `GetCustomerInfoAsync()` | Get customer info with entitlements |
| `LoginAsync(appUserId)` | Login with a user ID |
| `LogoutAsync()` | Logout current user |
| `RestoreTransactionsAsync()` | Restore previous purchases |
| `GetManagementSubscriptionUrlAsync()` | Get subscription management URL |
| `CheckTrialOrIntroDiscountEligibilityAsync()` | Check intro offer eligibility (iOS) |
| `SetEmail(email)` | Set subscriber email |
| `SetDisplayName(name)` | Set subscriber display name |
| `SetPhoneNumber(phone)` | Set subscriber phone number |
| `SetAttributes(attributes)` | Set custom subscriber attributes |

## Error Handling

The library uses a non-throwing approach for runtime errors. Methods return result objects with error status codes rather than throwing exceptions:

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

## Credits

- [RevenueCat](https://www.revenuecat.com/) - In-app purchase infrastructure
- [Kebechet/Maui.RevenueCat.InAppBilling](https://github.com/Kebechet/Maui.RevenueCat.InAppBilling) - Original MAUI wrapper this library is based on
- [Uno Platform](https://platform.uno/) - Cross-platform UI framework

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
