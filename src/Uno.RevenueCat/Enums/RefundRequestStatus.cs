namespace Uno.RevenueCat.Enums;

/// <summary>
/// Outcome of asking the store to begin a refund request for a purchase (StoreKit's native
/// refund-request sheet on iOS).
/// </summary>
public enum RefundRequestStatus
{
    /// <summary>The refund request could not be submitted.</summary>
    Error,

    /// <summary>The store accepted the refund request.</summary>
    Success,

    /// <summary>The user dismissed the refund request flow without submitting it.</summary>
    UserCancelled
}
