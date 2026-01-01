using Foundation;

namespace Uno.RevenueCat.Platforms.iOS.Extensions;

internal static class NsDateExtensions
{
    internal static DateTime? ToDateTime(this NSDate? date)
    {
        if (date is null)
        {
            return null;
        }

        return (DateTime)date;
    }
}
