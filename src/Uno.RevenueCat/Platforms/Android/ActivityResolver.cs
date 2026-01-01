using Android.App;
using Android.Content;

namespace Uno.RevenueCat.Platforms.Android;

/// <summary>
/// Helper class to resolve Android Activity and Context.
/// </summary>
internal static class ActivityResolver
{
    /// <summary>
    /// Gets the ApplicationContext using Android static API.
    /// Used for Initialize and most SDK operations.
    /// </summary>
    internal static Context GetApplicationContext()
    {
        return global::Android.App.Application.Context
            ?? throw new InvalidOperationException("ApplicationContext not available.");
    }

    /// <summary>
    /// Gets Activity for purchase dialogs (PurchaseProductAsync only).
    /// </summary>
    /// <param name="appWindow">Optional Activity for multi-window scenarios.</param>
    /// <returns>The resolved Activity.</returns>
    internal static Activity GetActivity(object? appWindow = null)
    {
        // 1. Explicit Activity passed
        if (appWindow is Activity activity)
        {
            return activity;
        }

        // 2. Fallback: ContextHelper.Current (returns Activity in Uno)
        if (Uno.UI.ContextHelper.Current is Activity contextActivity)
        {
            return contextActivity;
        }

        throw new InvalidOperationException(
            "Unable to resolve Android Activity for purchase dialog. " +
            "Pass an Activity to PurchaseProductAsync, or ensure the app has a valid current Activity.");
    }
}
