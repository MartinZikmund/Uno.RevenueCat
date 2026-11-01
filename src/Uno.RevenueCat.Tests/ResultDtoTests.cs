using Types.Result;
using Uno.RevenueCat.Enums;
using Uno.RevenueCat.Models;

namespace Uno.RevenueCat.Tests;

[TestClass]
public sealed class ResultDtoTests
{
    private static readonly Type[] _resultDtoTypes = typeof(PurchaseResultDto).Assembly
        .GetTypes()
        .Where(type => type.Namespace == typeof(PurchaseResultDto).Namespace && type.Name.EndsWith("ResultDto", StringComparison.Ordinal))
        .ToArray();

    [TestMethod]
    public void EveryResultDto_IsKeyedOnPurchaseErrorStatus()
    {
        Assert.IsTrue(_resultDtoTypes.Length >= 9, $"Found only {_resultDtoTypes.Length} result DTOs.");

        foreach (var type in _resultDtoTypes)
        {
            Assert.IsTrue(typeof(Result<PurchaseErrorStatus>).IsAssignableFrom(type), $"{type.Name} must derive from DataResult<T, PurchaseErrorStatus>.");
        }
    }

    [TestMethod]
    public void Success_HasNoError()
    {
        CustomerInfoResultDto result = new() { Value = CreateCustomerInfo() };

        Assert.IsTrue(result.IsSuccess);
        Assert.IsFalse(result.IsError);
        Assert.IsNull(result.Error);
    }

    [TestMethod]
    public void SuccessWithNullValue_IsStillSuccess()
    {
        // "Never purchased" / "no store-managed subscription" are successes, not failures.
        Assert.IsTrue(new PurchaseDateResultDto().IsSuccess);
        Assert.IsTrue(new ManagementUrlResultDto { Value = null }.IsSuccess);
    }

    [TestMethod]
    public void TypedError_IsError()
    {
        OfferingsResultDto result = new() { Error = PurchaseErrorStatus.NetworkError };

        Assert.IsTrue(result.IsError);
        Assert.AreEqual(PurchaseErrorStatus.NetworkError, result.Error);
    }

    [TestMethod]
    public void ExceptionWithoutTypedError_IsError()
    {
        StorefrontResultDto result = new() { ErrorException = new InvalidOperationException() };

        Assert.IsTrue(result.IsError);
        Assert.IsNull(result.Error);
    }

    [TestMethod]
    public void PendingPurchase_IsErrorButKeepsTransactionAndCustomerInfo()
    {
        StoreTransactionDto transaction = new()
        {
            ProductIdentifier = "pro_monthly",
            PurchaseDate = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc),
            TransactionIdentifier = "GPA.1234",
            Quantity = 1,
        };

        PurchaseResultDto result = new()
        {
            Error = PurchaseErrorStatus.PaymentPendingError,
            Transaction = transaction,
            Value = CreateCustomerInfo(),
        };

        Assert.IsTrue(result.IsError);
        Assert.AreSame(transaction, result.Transaction);
        Assert.IsNotNull(result.Value);
    }

    private static CustomerInfoDto CreateCustomerInfo() => new()
    {
        ActiveSubscriptions = [],
        AllPurchasedIdentifiers = [],
        NonConsumablePurchases = [],
        FirstSeen = null,
        LatestExpirationDate = null,
        ManagementUrl = null,
        Entitlements = [],
    };
}
