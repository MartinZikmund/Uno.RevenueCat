using Maui.RevenueCat.iOS;

namespace Uno.RevenueCat.Platforms.iOS.Models;

/// <summary>
/// Contains the result of a login operation.
/// </summary>
public sealed class LoginResult
{
    /// <summary>
    /// Gets the customer info.
    /// </summary>
    public RCCustomerInfo CustomerInfo { get; }

    /// <summary>
    /// Gets whether the user was newly created.
    /// </summary>
    public bool Created { get; }

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public LoginResult(RCCustomerInfo customerInfo, bool created)
    {
        CustomerInfo = customerInfo;
        Created = created;
    }
}
