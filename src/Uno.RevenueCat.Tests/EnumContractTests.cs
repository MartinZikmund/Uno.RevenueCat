using System.Runtime.Serialization;
using Uno.RevenueCat.Enums;

namespace Uno.RevenueCat.Tests;

/// <summary>
/// The managed enums mirror native RevenueCat SDK enums. Their values are part of the
/// public contract, and a missing member means a native value maps to nothing (or throws).
/// </summary>
[TestClass]
public sealed class EnumContractTests
{
    [TestMethod]
    public void PeriodType_HasPrepaid()
    {
        // Google Play prepaid base plans surface PeriodType.PREPAID; without this member
        // the mapper threw and locked paying users out of their entitlements.
        Assert.IsTrue(Enum.IsDefined(PeriodType.Prepaid));
    }

    [TestMethod]
    [DataRow(PeriodType.Intro, 0)]
    [DataRow(PeriodType.Normal, 1)]
    [DataRow(PeriodType.Trial, 2)]
    [DataRow(PeriodType.Prepaid, 3)]
    public void PeriodType_ValuesAreStable(PeriodType member, int expected) =>
        Assert.AreEqual(expected, (int)member);

    [TestMethod]
    [DataRow(StoreType.Amazon, 0)]
    [DataRow(StoreType.AppStore, 1)]
    [DataRow(StoreType.MacAppStore, 2)]
    [DataRow(StoreType.PlayStore, 3)]
    [DataRow(StoreType.Promotional, 4)]
    [DataRow(StoreType.Stripe, 5)]
    [DataRow(StoreType.UnknownStore, 6)]
    [DataRow(StoreType.RcBilling, 7)]
    [DataRow(StoreType.External, 8)]
    [DataRow(StoreType.Paddle, 9)]
    [DataRow(StoreType.TestStore, 10)]
    [DataRow(StoreType.Galaxy, 11)]
    public void StoreType_ValuesAreStable(StoreType member, int expected) =>
        Assert.AreEqual(expected, (int)member);

    [TestMethod]
    [DataRow(PurchaseErrorStatus.FeatureNotSupportedWithStoreKit1, 38)]
    [DataRow(PurchaseErrorStatus.InvalidWebPurchaseToken, 39)]
    [DataRow(PurchaseErrorStatus.PurchaseBelongsToOtherUser, 40)]
    [DataRow(PurchaseErrorStatus.ExpiredWebPurchaseToken, 41)]
    [DataRow(PurchaseErrorStatus.TestStoreSimulatedPurchaseError, 42)]
    public void PurchaseErrorStatus_HasNewNativeCodes(PurchaseErrorStatus member, int expected) =>
        Assert.AreEqual(expected, (int)member);

    [TestMethod]
    public void PurchaseErrorStatus_CoversZeroToFortyTwoExceptTwentySeven()
    {
        var values = Enum.GetValues<PurchaseErrorStatus>().Select(v => (int)v).ToHashSet();

        // 27 genuinely does not exist in RevenueCat's native enum (it jumps 26 -> 28).
        var expected = Enumerable.Range(0, 43).Where(i => i != 27).ToHashSet();

        CollectionAssert.AreEquivalent(expected.ToList(), values.ToList());
    }

    [TestMethod]
    public void PurchaseErrorStatus_HasNoDuplicateValues()
    {
        var values = Enum.GetValues<PurchaseErrorStatus>().Select(v => (int)v).ToList();

        CollectionAssert.AreEquivalent(values.Distinct().ToList(), values);
    }

    [TestMethod]
    public void PurchaseErrorStatus_EveryMemberCarriesEnumMemberAttribute()
    {
        var undecorated = typeof(PurchaseErrorStatus)
            .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(f => f.GetCustomAttributes(typeof(EnumMemberAttribute), false).Length == 0)
            .Select(f => f.Name)
            .ToList();

        CollectionAssert.AreEqual(Array.Empty<string>(), undecorated);
    }
}
