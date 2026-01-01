using Com.Revenuecat.Purchases;

namespace Uno.RevenueCat.Platforms.Android.Exceptions;

/// <summary>
/// Exception thrown when a RevenueCat purchase operation fails.
/// </summary>
public class PurchasesErrorException : Exception
{
    /// <summary>
    /// Gets the underlying RevenueCat error.
    /// </summary>
    public PurchasesError? PurchasesError { get; }

    /// <summary>
    /// Gets whether the user cancelled the operation.
    /// </summary>
    public bool UserCancelled { get; }

    /// <summary>
    /// Creates a new instance of the exception.
    /// </summary>
    public PurchasesErrorException(PurchasesError? purchasesError, bool userCancelled)
        : base($"{purchasesError?.Message} ({purchasesError?.UnderlyingErrorMessage}) code: {purchasesError?.Code} userCancelled: {userCancelled}")
    {
        PurchasesError = purchasesError;
        UserCancelled = userCancelled;
    }
}
