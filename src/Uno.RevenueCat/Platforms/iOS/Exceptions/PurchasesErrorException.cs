using Foundation;
using Maui.RevenueCat.iOS;
using Maui.RevenueCat.iOS.Additions;

namespace Uno.RevenueCat.Platforms.iOS.Exceptions;

/// <summary>
/// Exception thrown when a RevenueCat purchase operation fails.
/// </summary>
public class PurchasesErrorException : Exception
{
    /// <summary>
    /// Gets the underlying NSError.
    /// </summary>
    public NSError? PurchasesError { get; }

    /// <summary>
    /// Gets whether the user cancelled the operation.
    /// </summary>
    public bool UserCancelled { get; }

    /// <summary>
    /// Gets the readable error code.
    /// </summary>
    public NSObject? ReadableErrorCode { get; }

    /// <summary>
    /// Gets the underlying error.
    /// </summary>
    public NSObject? UnderlyingError { get; }

    /// <summary>
    /// Gets the localized description.
    /// </summary>
    public string? LocalizedDescription { get; }

    /// <summary>
    /// Gets the RevenueCat error code.
    /// </summary>
    public RCPurchasesErrorCode PurchasesErrorCode { get; }

    /// <summary>
    /// Creates a new instance of the exception.
    /// </summary>
    public PurchasesErrorException(NSError? purchasesError, bool userCancelled)
        : base($"{purchasesError?.Description} userCancelled: {userCancelled}", WrapError(purchasesError))
    {
        PurchasesError = purchasesError;
        UserCancelled = userCancelled;
        if (purchasesError is not null)
        {
            purchasesError.UserInfo.TryGetValue(ErrorDetails.ReadableErrorCodeKey, out var readableErrorCode);
            ReadableErrorCode = readableErrorCode;
            purchasesError.UserInfo.TryGetValue(NSError.UnderlyingErrorKey, out var underlyingError);
            UnderlyingError = underlyingError;
            var localizedDescription = purchasesError.LocalizedDescription;
            LocalizedDescription = localizedDescription;

            var purchaseErrorCodeInt = (int)purchasesError.Code;
            PurchasesErrorCode = (RCPurchasesErrorCode)purchaseErrorCodeInt;
        }
    }

    private static NSErrorException? WrapError(NSError? purchasesError)
    {
        if (purchasesError is not null)
        {
            return new NSErrorException(purchasesError);
        }

        return null;
    }
}
