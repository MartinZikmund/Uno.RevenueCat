using Maui.RevenueCat.iOS;
using Uno.RevenueCat.Models;

namespace Uno.RevenueCat.Platforms.iOS.Extensions;

internal static class SubscriptionPeriodExtensions
{
    internal static SubscriptionPeriodDto? ToSubscriptionPeriodDto(this RCSubscriptionPeriod? period) =>
        period is null
            ? null
            : new SubscriptionPeriodDto
            {
                Value = (int)period.Value,
                Unit = period.Unit.ToSubscriptionUnit(),
            };
}
