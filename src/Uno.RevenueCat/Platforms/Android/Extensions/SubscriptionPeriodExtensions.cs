using Com.Revenuecat.Purchases.Models;
using Uno.RevenueCat.Enums;
using Uno.RevenueCat.Models;

namespace Uno.RevenueCat.Platforms.Android.Extensions;

internal static class SubscriptionPeriodExtensions
{
    internal static SubscriptionPeriodDto? ToSubscriptionPeriodDto(this Period? period) =>
        period is null
            ? null
            : new SubscriptionPeriodDto
            {
                Value = period.Value,
                Unit = period.GetUnit().ToSubscriptionUnit(),
            };

    // GetUnit(), not the Iso8601 string: the bound property Period.Unit is unusable (it collides
    // with the nested Period.Unit type, CS0119), but the getter method is fine — and unlike parsing
    // the last character of Iso8601, it reports UNKNOWN rather than inventing a unit for a
    // compound period such as "P1Y6M".
    // Period.Unit is a bound Java enum (static fields, not constants), hence the `when ==` arms.
    private static SubscriptionUnit ToSubscriptionUnit(this Period.Unit unit) => unit switch
    {
        var u when u == Period.Unit.Day => SubscriptionUnit.Day,
        var u when u == Period.Unit.Week => SubscriptionUnit.Week,
        var u when u == Period.Unit.Month => SubscriptionUnit.Month,
        var u when u == Period.Unit.Year => SubscriptionUnit.Year,
        _ => SubscriptionUnit.Unknown,
    };
}
