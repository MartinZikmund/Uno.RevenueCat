using Maui.RevenueCat.iOS;
using Uno.RevenueCat.Enums;

namespace Uno.RevenueCat.Platforms.iOS.Extensions;

internal static class PurchasesErrorCodeExtensions
{
    /// <summary>
    /// Maps an iOS <see cref="RCPurchasesErrorCode"/> to <see cref="PurchaseErrorStatus"/> by name.
    /// iOS and Android emit different integer codes for the same conceptual error, so a raw
    /// <c>(PurchaseErrorStatus)(int)</c> cast cannot be correct on both platforms.
    /// </summary>
    internal static PurchaseErrorStatus ToPurchaseErrorStatus(this RCPurchasesErrorCode code) => code switch
    {
        RCPurchasesErrorCode.UnknownError => PurchaseErrorStatus.UnknownError,
        RCPurchasesErrorCode.PurchaseCancelledError => PurchaseErrorStatus.PurchaseCancelledError,
        RCPurchasesErrorCode.StoreProblemError => PurchaseErrorStatus.StoreProblemError,
        RCPurchasesErrorCode.PurchaseNotAllowedError => PurchaseErrorStatus.PurchaseNotAllowedError,
        RCPurchasesErrorCode.PurchaseInvalidError => PurchaseErrorStatus.PurchaseInvalidError,
        RCPurchasesErrorCode.ProductNotAvailableForPurchaseError => PurchaseErrorStatus.ProductNotAvailableForPurchaseError,
        RCPurchasesErrorCode.ProductAlreadyPurchasedError => PurchaseErrorStatus.ProductAlreadyPurchasedError,
        RCPurchasesErrorCode.ReceiptAlreadyInUseError => PurchaseErrorStatus.ReceiptAlreadyInUseError,
        RCPurchasesErrorCode.InvalidReceiptError => PurchaseErrorStatus.InvalidReceiptError,
        RCPurchasesErrorCode.MissingReceiptFileError => PurchaseErrorStatus.MissingReceiptFileError,
        RCPurchasesErrorCode.NetworkError => PurchaseErrorStatus.NetworkError,
        RCPurchasesErrorCode.InvalidCredentialsError => PurchaseErrorStatus.InvalidCredentialsError,
        RCPurchasesErrorCode.UnexpectedBackendResponseError => PurchaseErrorStatus.UnexpectedBackendResponseError,
        RCPurchasesErrorCode.ReceiptInUseByOtherSubscriberError => PurchaseErrorStatus.ReceiptInUseByOtherSubscriberError,
        RCPurchasesErrorCode.InvalidAppUserIdError => PurchaseErrorStatus.InvalidAppUserIdError,
        RCPurchasesErrorCode.OperationAlreadyInProgressForProductError => PurchaseErrorStatus.OperationAlreadyInProgressForProductError,
        RCPurchasesErrorCode.UnknownBackendError => PurchaseErrorStatus.UnknownBackendError,
        RCPurchasesErrorCode.InvalidAppleSubscriptionKeyError => PurchaseErrorStatus.InvalidAppleSubscriptionKeyError,
        RCPurchasesErrorCode.IneligibleError => PurchaseErrorStatus.IneligibleError,
        RCPurchasesErrorCode.InsufficientPermissionsError => PurchaseErrorStatus.InsufficientPermissionsError,
        RCPurchasesErrorCode.PaymentPendingError => PurchaseErrorStatus.PaymentPendingError,
        RCPurchasesErrorCode.InvalidSubscriberAttributesError => PurchaseErrorStatus.InvalidSubscriberAttributesError,
        RCPurchasesErrorCode.LogOutAnonymousUserError => PurchaseErrorStatus.LogOutAnonymousUserError,
        RCPurchasesErrorCode.ConfigurationError => PurchaseErrorStatus.ConfigurationError,
        RCPurchasesErrorCode.UnsupportedError => PurchaseErrorStatus.UnsupportedError,
        RCPurchasesErrorCode.EmptySubscriberAttributesError => PurchaseErrorStatus.EmptySubscriberAttributesError,
        RCPurchasesErrorCode.ProductDiscountMissingIdentifierError => PurchaseErrorStatus.ProductDiscountMissingIdentifierError,
        RCPurchasesErrorCode.ProductDiscountMissingSubscriptionGroupIdentifierError => PurchaseErrorStatus.ProductDiscountMissingSubscriptionGroupIdentifierError,
        RCPurchasesErrorCode.CustomerInfoError => PurchaseErrorStatus.CustomerInfoError,
        RCPurchasesErrorCode.SystemInfoError => PurchaseErrorStatus.SystemInfoError,
        RCPurchasesErrorCode.BeginRefundRequestError => PurchaseErrorStatus.BeginRefundRequestError,
        RCPurchasesErrorCode.ProductRequestTimedOut => PurchaseErrorStatus.ProductRequestTimedOut,
        RCPurchasesErrorCode.APIEndpointBlocked => PurchaseErrorStatus.APIEndpointBlocked,
        RCPurchasesErrorCode.InvalidPromotionalOfferError => PurchaseErrorStatus.InvalidPromotionalOfferError,
        RCPurchasesErrorCode.OfflineConnectionError => PurchaseErrorStatus.OfflineConnectionError,
        RCPurchasesErrorCode.FeatureNotAvailableInCustomEntitlementsComputationMode => PurchaseErrorStatus.FeatureNotAvailableInCustomEntitlementsComputationMode,
        RCPurchasesErrorCode.SignatureVerificationFailed => PurchaseErrorStatus.SignatureVerificationFailed,
        RCPurchasesErrorCode.FeatureNotSupportedWithStoreKit1 => PurchaseErrorStatus.FeatureNotSupportedWithStoreKit1,
        RCPurchasesErrorCode.InvalidWebPurchaseToken => PurchaseErrorStatus.InvalidWebPurchaseToken,
        RCPurchasesErrorCode.PurchaseBelongsToOtherUser => PurchaseErrorStatus.PurchaseBelongsToOtherUser,
        RCPurchasesErrorCode.ExpiredWebPurchaseToken => PurchaseErrorStatus.ExpiredWebPurchaseToken,
        RCPurchasesErrorCode.TestStoreSimulatedPurchaseError => PurchaseErrorStatus.TestStoreSimulatedPurchaseError,
        _ => PurchaseErrorStatus.UnknownError,
    };
}
