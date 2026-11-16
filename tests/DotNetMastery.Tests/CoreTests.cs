using DotNetMastery.Core.CleanCode;
using DotNetMastery.Core.Delegates;
using DotNetMastery.Core.Fundamentals;

namespace DotNetMastery.Tests;

public class CoreTests
{
    [Fact]
    public void ValueType_Copy_ShouldNotMutateOriginal()
    {
        var (original, modifiedCopy) = StackVsHeapDemo.DemonstrateValueTypeCopy();

        Assert.Equal(10, original.X);
        Assert.Equal(99, modifiedCopy.X);
    }

    [Fact]
    public void ReferenceType_Copy_ShouldMutateSharedObject()
    {
        var (original, shared) = StackVsHeapDemo.DemonstrateReferenceTypeCopy();

        Assert.Equal(99, original.X);
        Assert.Equal(99, shared.X);
        Assert.Same(original, shared);
    }

    [Fact]
    public void BoxingAndUnboxing_ShouldRoundtripCorrectly()
    {
        int initial = 77;
        object boxed = BoxingUnboxingDemo.BoxInteger(initial);
        int unboxed = BoxingUnboxingDemo.UnboxInteger(boxed);

        Assert.Equal(initial, unboxed);
    }

    [Fact]
    public void Unboxing_WithWrongType_ShouldThrowInvalidCast()
    {
        object boxedInt = 42;
        bool threwException = BoxingUnboxingDemo.TryInvalidUnboxing(boxedInt);

        Assert.True(threwException);
    }

    [Fact]
    public void Keywords_RefAndOut_ShouldBehaveAsExpected()
    {
        int num = 5;
        KeywordsDemo.Increment(ref num);
        Assert.Equal(6, num);

        bool success = KeywordsDemo.TryParseInteger("999", out int parsed);
        Assert.True(success);
        Assert.Equal(999, parsed);

        KeywordsDemo.GetUserInfo(2, out string name, out int age);
        Assert.Equal("User_2", name);
        Assert.Equal(27, age);
    }

    [Fact]
    public void Yield_LazyEvaluation_ShouldOnlyProcessRequestedElements()
    {
        var numbers = KeywordsDemo.GenerateNumbersLazy(1_000_000).Take(5).ToList();

        Assert.Equal(5, numbers.Count);
        Assert.Equal([1, 2, 3, 4, 5], numbers);
    }

    [Fact]
    public void EqualsVsEqualityOperator_ShouldReflectTypeSemantics()
    {
        var (operatorEq, equalsEq, recordEq) = KeywordsDemo.CompareEqualities();

        Assert.False(operatorEq, "Classes without overloaded == check reference equality.");
        Assert.True(equalsEq, "Custom .Equals() checks semantic value equality.");
        Assert.True(recordEq, "Records have built-in value equality for ==.");
    }

    [Fact]
    public void EnumNameof_ShouldReturnCorrectIdentifier()
    {
        string status = KeywordsDemo.GetStatusNameFast(KeywordsDemo.UserStatus.Active);
        Assert.Equal("Active", status);
    }

    [Fact]
    public void Delegates_Multicast_ShouldExecuteAllSubscribedHandlers()
    {
        var logs = DelegatesAndLambdasDemo.RunMulticastDelegate("TestEvent");

        Assert.Equal(2, logs.Count);
        Assert.Contains("Handler 1: TestEvent", logs);
        Assert.Contains("Handler 2: TestEvent", logs);
    }

    [Fact]
    public void CleanCode_Kiss_ShouldReturnTrueForValidActiveUser()
    {
        var activeUser = new CleanCodePrinciples.UserProfile
        {
            Username = "johndoe",
            IsActive = true,
            IsDeleted = false
        };

        Assert.True(CleanCodePrinciples.CanAccessSystemKiss(activeUser));
        Assert.True(CleanCodePrinciples.CanAccessSystemOverEngineered(activeUser));
    }

    [Fact]
    public void CleanCode_MaintainableParsing_ShouldFilterIgnoredAndExtractErrors()
    {
        string logData = "ERR:DiskFull;INFO:SystemUp;ERR:NetworkTimeout;IGNORED:TestAlert";
        var errors = CleanCodePrinciples.ParseErrorsMaintainable(logData);

        Assert.Equal(2, errors.Count);
        Assert.Equal("DiskFull", errors[0]);
        Assert.Equal("NetworkTimeout", errors[1]);
    }
}
