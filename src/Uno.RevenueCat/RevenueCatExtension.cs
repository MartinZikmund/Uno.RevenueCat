using Microsoft.Extensions.DependencyInjection;
using Uno.RevenueCat.Services;

namespace Uno.RevenueCat;

public static class RevenueCatExtension
{
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
