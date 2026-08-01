using System.Text.Json;
using Uno.RevenueCat.Extensions;

namespace Uno.RevenueCat.Tests;

[TestClass]
public sealed class MetadataExtensionsTests
{
    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    public void ToMetadataDictionary_ReturnsEmpty_ForNullOrEmpty(string? json) =>
        Assert.AreEqual(0, json.ToMetadataDictionary().Count);

    [TestMethod]
    public void ToMetadataDictionary_ReturnsEmpty_ForEmptyObject() =>
        Assert.AreEqual(0, "{}".ToMetadataDictionary().Count);

    [TestMethod]
    public void ToMetadataDictionary_PreservesValueTypes()
    {
        // The whole point of the typed dictionary: a number must stay a number.
        // Before the iOS ToJson fix, every value arrived as a JSON string.
        var metadata = """{"count":5,"enabled":true,"name":"pro","ratio":1.5,"missing":null}"""
            .ToMetadataDictionary();

        Assert.AreEqual(JsonValueKind.Number, metadata["count"].ValueKind);
        Assert.AreEqual(JsonValueKind.True, metadata["enabled"].ValueKind);
        Assert.AreEqual(JsonValueKind.String, metadata["name"].ValueKind);
        Assert.AreEqual(JsonValueKind.Number, metadata["ratio"].ValueKind);
        Assert.AreEqual(JsonValueKind.Null, metadata["missing"].ValueKind);

        Assert.AreEqual(5, metadata["count"].GetInt32());
        Assert.IsTrue(metadata["enabled"].GetBoolean());
        Assert.AreEqual("pro", metadata["name"].GetString());
    }

    [TestMethod]
    public void ToMetadataDictionary_PreservesNestedContainers()
    {
        var metadata = """{"offer":{"badge":"Best value","months":12},"tiers":[1,2,3]}"""
            .ToMetadataDictionary();

        Assert.AreEqual(JsonValueKind.Object, metadata["offer"].ValueKind);
        Assert.AreEqual(JsonValueKind.Array, metadata["tiers"].ValueKind);
        Assert.AreEqual("Best value", metadata["offer"].GetProperty("badge").GetString());
        Assert.AreEqual(12, metadata["offer"].GetProperty("months").GetInt32());
        Assert.AreEqual(3, metadata["tiers"].GetArrayLength());
    }

    [TestMethod]
    public void ToMetadataDictionary_ValuesSurviveTheParentDocumentBeingDisposed()
    {
        // Elements must be cloned out of the JsonDocument, else they are invalid after parsing.
        var metadata = """{"badge":"Best value"}""".ToMetadataDictionary();

        GC.Collect();

        Assert.AreEqual("Best value", metadata["badge"].GetString());
    }

    [TestMethod]
    [DataRow("not json at all")]
    [DataRow("{\"unterminated\":")]
    public void ToMetadataDictionary_ReturnsEmpty_ForMalformedJson(string json) =>
        // Dashboard metadata is user-authored; malformed JSON must not throw out of GetOfferingsAsync.
        Assert.AreEqual(0, json.ToMetadataDictionary().Count);

    [TestMethod]
    [DataRow("[1,2,3]")]
    [DataRow("\"a string\"")]
    [DataRow("42")]
    public void ToMetadataDictionary_ReturnsEmpty_ForNonObjectRoot(string json) =>
        Assert.AreEqual(0, json.ToMetadataDictionary().Count);
}
