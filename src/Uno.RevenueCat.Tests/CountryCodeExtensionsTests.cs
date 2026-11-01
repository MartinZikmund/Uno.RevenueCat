using Uno.RevenueCat.Extensions;

namespace Uno.RevenueCat.Tests;

[TestClass]
public sealed class CountryCodeExtensionsTests
{
    [TestMethod]
    [DataRow("USA", "US")]
    [DataRow("GBR", "GB")]
    [DataRow("CZE", "CZ")]
    [DataRow("DEU", "DE")]
    [DataRow("usa", "US")]
    public void ToIsoAlpha2CountryCode_ConvertsAlpha3(string alpha3, string expectedAlpha2) =>
        Assert.AreEqual(expectedAlpha2, alpha3.ToIsoAlpha2CountryCode());

    [TestMethod]
    [DataRow("US")]
    [DataRow("XXX")]
    [DataRow("")]
    public void ToIsoAlpha2CountryCode_LeavesOtherInputUnchanged(string countryCode) =>
        Assert.AreEqual(countryCode, countryCode.ToIsoAlpha2CountryCode());
}
