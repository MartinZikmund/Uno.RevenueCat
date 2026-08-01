using System.Runtime.Serialization;

namespace Uno.RevenueCat.Enums;

//https://github.com/RevenueCat/purchases-android/blob/main/public/src/main/java/com/revenuecat/purchases/errors.kt
//https://github.com/RevenueCat/purchases-ios/blob/main/Sources/Error%20Handling/ErrorCode.swift#L39
//https://www.revenuecat.com/docs/test-and-launch/errors
/// <summary>
/// Mirrors RevenueCat's native error codes (shared by the Android and iOS SDKs). The numeric
/// value 27 is intentionally absent: the native enum skips it too.
/// </summary>
public enum PurchaseErrorStatus
{
    /// <summary>Catch-all for a failure that doesn't map to a more specific code below.</summary>
    [EnumMember(Value = "UNKNOWN")]
    UnknownError = 0,

    /// <summary>The user backed out of the purchase flow before completing it.</summary>
    [EnumMember(Value = "PURCHASE_CANCELLED")]
    PurchaseCancelledError = 1,

    /// <summary>The App Store/Play Store was unreachable or returned an error unrelated to the purchase.</summary>
    [EnumMember(Value = "STORE_PROBLEM")]
    StoreProblemError = 2,

    /// <summary>This device or account isn't permitted to make purchases (e.g. parental controls).</summary>
    [EnumMember(Value = "PURCHASE_NOT_ALLOWED")]
    PurchaseNotAllowedError = 3,

    /// <summary>The store rejected the purchase request as invalid.</summary>
    [EnumMember(Value = "PURCHASE_INVALID")]
    PurchaseInvalidError = 4,

    /// <summary>The product isn't available for purchase in the current store or region.</summary>
    [EnumMember(Value = "PRODUCT_NOT_AVAILABLE_FOR_PURCHASE")]
    ProductNotAvailableForPurchaseError = 5,

    /// <summary>The user already owns this non-consumable product or active subscription.</summary>
    [EnumMember(Value = "PRODUCT_ALREADY_PURCHASED")]
    ProductAlreadyPurchasedError = 6,

    /// <summary>The store receipt backing this purchase is already tied to a different RevenueCat subscriber.</summary>
    [EnumMember(Value = "RECEIPT_ALREADY_IN_USE")]
    ReceiptAlreadyInUseError = 7,

    /// <summary>The purchase receipt failed validation with the store.</summary>
    [EnumMember(Value = "INVALID_RECEIPT")]
    InvalidReceiptError = 8,

    /// <summary>No local receipt file was found to validate the purchase.</summary>
    [EnumMember(Value = "MISSING_RECEIPT_FILE")]
    MissingReceiptFileError = 9,

    /// <summary>The request to the store or to RevenueCat's servers failed due to a network problem.</summary>
    [EnumMember(Value = "NETWORK_ERROR")]
    NetworkError = 10,

    /// <summary>The RevenueCat API key or store credentials are invalid or misconfigured.</summary>
    [EnumMember(Value = "INVALID_CREDENTIALS")]
    InvalidCredentialsError = 11,

    /// <summary>RevenueCat's backend returned a response the SDK did not expect.</summary>
    [EnumMember(Value = "UNEXPECTED_BACKEND_RESPONSE_ERROR")]
    UnexpectedBackendResponseError = 12,

    /// <summary>The receipt is already tied to a different app user and can't be transferred automatically.</summary>
    [EnumMember(Value = "RECEIPT_IN_USE_BY_OTHER_SUBSCRIBER")]
    ReceiptInUseByOtherSubscriberError = 13,

    /// <summary>The app user ID passed to the SDK is invalid (e.g. empty or malformed).</summary>
    [EnumMember(Value = "INVALID_APP_USER_ID")]
    InvalidAppUserIdError = 14,

    /// <summary>A purchase for this product is already in progress.</summary>
    [EnumMember(Value = "OPERATION_ALREADY_IN_PROGRESS_FOR_PRODUCT_ERROR")]
    OperationAlreadyInProgressForProductError = 15,

    /// <summary>RevenueCat's backend reported an error it doesn't have a specific code for.</summary>
    [EnumMember(Value = "UNKNOWN_BACKEND_ERROR")]
    UnknownBackendError = 16,

    /// <summary>The App Store Connect subscription key configured for the app is invalid.</summary>
    [EnumMember(Value = "INVALID_APPLE_SUBSCRIPTION_KEY")]
    InvalidAppleSubscriptionKeyError = 17,

    /// <summary>The user isn't eligible for the requested offer or discount.</summary>
    [EnumMember(Value = "INELIGIBLE_ERROR")]
    IneligibleError = 18,

    /// <summary>The API key doesn't have permission to perform the requested action.</summary>
    [EnumMember(Value = "INSUFFICIENT_PERMISSIONS_ERROR")]
    InsufficientPermissionsError = 19,

    /// <summary>The purchase requires additional user action (e.g. parental approval) before it completes.</summary>
    [EnumMember(Value = "PAYMENT_PENDING_ERROR")]
    PaymentPendingError = 20,

    /// <summary>One or more subscriber attributes failed validation and were not saved.</summary>
    [EnumMember(Value = "INVALID_SUBSCRIBER_ATTRIBUTES")]
    InvalidSubscriberAttributesError = 21,

    /// <summary>Logout was called while the current user is anonymous, which has no identity to log out of.</summary>
    [EnumMember(Value = "LOGOUT_CALLED_WITH_ANONYMOUS_USER")]
    LogOutAnonymousUserError = 22,

    /// <summary>The SDK was configured incorrectly, e.g. an invalid API key or duplicate configuration.</summary>
    [EnumMember(Value = "CONFIGURATION_ERROR")]
    ConfigurationError = 23,

    /// <summary>The requested operation isn't supported on this platform or store.</summary>
    [EnumMember(Value = "UNSUPPORTED_ERROR")]
    UnsupportedError = 24,

    /// <summary>Setting subscriber attributes was called with an empty attributes dictionary.</summary>
    [EnumMember(Value = "EMPTY_SUBSCRIBER_ATTRIBUTES")]
    EmptySubscriberAttributesError = 25,

    /// <summary>A product discount is missing the identifier required to apply it.</summary>
    [EnumMember(Value = "PRODUCT_DISCOUNT_MISSING_IDENTIFIER_ERROR")]
    ProductDiscountMissingIdentifierError = 26,

    //27 is not specified anywhere
    /// <summary>A product discount is missing its subscription group identifier.</summary>
    [EnumMember(Value = "PRODUCT_DISCOUNT_MISSING_SUBSCRIPTION_GROUP_IDENTIFIER_ERROR")]
    ProductDiscountMissingSubscriptionGroupIdentifierError = 28,

    /// <summary>The customer info returned by RevenueCat could not be parsed or is invalid.</summary>
    [EnumMember(Value = "CUSTOMER_INFO_ERROR")]
    CustomerInfoError = 29,

    /// <summary>The SDK failed to gather required device or system information.</summary>
    [EnumMember(Value = "SYSTEM_INFO_ERROR")]
    SystemInfoError = 30,

    /// <summary>The refund request flow could not be started.</summary>
    [EnumMember(Value = "BEGIN_REFUND_REQUEST_ERROR")]
    BeginRefundRequestError = 31,

    /// <summary>The request for product or pricing information from the store timed out.</summary>
    [EnumMember(Value = "PRODUCT_REQUEST_TIMED_OUT_ERROR")]
    ProductRequestTimedOut = 32,

    /// <summary>A RevenueCat API endpoint was blocked, e.g. by a firewall, VPN, or ad blocker.</summary>
    [EnumMember(Value = "API_ENDPOINT_BLOCKED_ERROR")]
    APIEndpointBlocked = 33,

    /// <summary>The promotional offer signature or parameters were rejected by the store.</summary>
    [EnumMember(Value = "INVALID_PROMOTIONAL_OFFER_ERROR")]
    InvalidPromotionalOfferError = 34,

    /// <summary>The device appears to be offline.</summary>
    [EnumMember(Value = "OFFLINE_CONNECTION_ERROR")]
    OfflineConnectionError = 35,

    /// <summary>The requested feature isn't available while RevenueCat runs in custom entitlements computation mode.</summary>
    [EnumMember(Value = "FEATURE_NOT_AVAILABLE_IN_CUSTOM_ENTITLEMENTS_COMPUTATION_MODE_ERROR")]
    FeatureNotAvailableInCustomEntitlementsComputationMode = 36,

    /// <summary>A response's cryptographic signature could not be verified.</summary>
    [EnumMember(Value = "SIGNATURE_VERIFICATION_FAILED")]
    SignatureVerificationFailed = 37,

    /// <summary>The requested feature requires StoreKit 2 and isn't available under StoreKit 1.</summary>
    [EnumMember(Value = "FEATURE_NOT_SUPPORTED_WITH_STOREKIT1")]
    FeatureNotSupportedWithStoreKit1 = 38,

    /// <summary>The web purchase redemption token is invalid.</summary>
    [EnumMember(Value = "INVALID_WEB_PURCHASE_TOKEN")]
    InvalidWebPurchaseToken = 39,

    /// <summary>The web purchase redemption token was already redeemed by a different app user.</summary>
    [EnumMember(Value = "ALREADY_REDEEMED_WEB_PURCHASE_TOKEN")]
    PurchaseBelongsToOtherUser = 40,

    /// <summary>The web purchase redemption token has expired.</summary>
    [EnumMember(Value = "EXPIRED_WEB_PURCHASE_TOKEN")]
    ExpiredWebPurchaseToken = 41,

    /// <summary>A simulated failure raised by RevenueCat's test store, used to exercise error handling.</summary>
    [EnumMember(Value = "TEST_STORE_SIMULATED_PURCHASE_ERROR")]
    TestStoreSimulatedPurchaseError = 42,
}
