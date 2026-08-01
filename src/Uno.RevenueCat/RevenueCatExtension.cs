using Microsoft.Extensions.DependencyInjection;
using Uno.RevenueCat.Services;

namespace Uno.RevenueCat;

/// <summary>
/// Dependency injection registration for the RevenueCat billing services.
/// </summary>
public static class RevenueCatExtension
{
    /// <summary>
    /// Registers <see cref="IRevenueCatBilling"/> and its dependencies with the service collection.
    /// </summary>
    /// <param name="services">The service collection to register with.</param>
    /// <param name="forceEnableDebugLogs">
    /// When <c>null</c> (default), resolves to whether this library was built in a <c>DEBUG</c>
    /// configuration. A resolved or explicit value of <c>true</c> turns on native RevenueCat SDK debug
    /// logging; <c>false</c> is a no-op — it does not disable or reset logging, it simply skips enabling it.
    /// </param>
    /// <returns>The same <paramref name="services"/> instance, for chaining.</returns>
    public static IServiceCollection AddRevenueCat(this IServiceCollection services,
        bool? forceEnableDebugLogs = null)
    {
        if (forceEnableDebugLogs is null)
        {
            forceEnableDebugLogs = IsDebug();
        }

        RevenueCatBilling.EnableDebugLogs(forceEnableDebugLogs.Value);

        services.AddSingleton<IRevenueCatBilling, RevenueCatBilling>();

        services.AddLogging();

        return services;
    }

    private static bool IsDebug()
    {
#if DEBUG
        return true;
#else
        return false;
#endif
    }
}
