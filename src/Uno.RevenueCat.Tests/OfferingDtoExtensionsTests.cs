using Uno.RevenueCat.Extensions;
using Uno.RevenueCat.Models;

namespace Uno.RevenueCat.Tests;

[TestClass]
public sealed class OfferingDtoExtensionsTests
{
    private static OfferingDto Offering(string id, bool isCurrent) =>
        new() { Identifier = id, IsCurrent = isCurrent };

    [TestMethod]
    public void GetCurrent_IsCallableOnTheTypeGetOfferingsAsyncReturns()
    {
        // IRevenueCatBilling.GetOfferingsAsync returns IReadOnlyList<OfferingDto>.
        // If GetCurrent only extends List<OfferingDto>, this does not compile.
        IReadOnlyList<OfferingDto> offerings = [Offering("a", false), Offering("b", true)];

        Assert.AreEqual("b", offerings.GetCurrent()?.Identifier);
    }

    [TestMethod]
    public void GetCurrent_ReturnsNull_WhenNoneIsCurrent()
    {
        IReadOnlyList<OfferingDto> offerings = [Offering("a", false)];

        Assert.IsNull(offerings.GetCurrent());
    }

    [TestMethod]
    public void GetCurrent_ReturnsNull_WhenEmpty() =>
        Assert.IsNull(Array.Empty<OfferingDto>().GetCurrent());
}
