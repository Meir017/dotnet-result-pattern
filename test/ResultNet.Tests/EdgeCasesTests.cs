namespace ResultNet.Tests;

public class EdgeCasesTests
{
    [Fact]
    public void Result_WithNullValue_StoresNull()
    {
        Result<string?> result = Result<string?>.Success(null);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

    [Fact]
    public void Result_WithDefaultStruct_StoresDefault()
    {
        Result<int> result = Result<int>.Success(default);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value);
    }

    [Fact]
    public void Result_WithEmptyString_StoresEmptyString()
    {
        Result<string> result = Result<string>.Success(string.Empty);

        Assert.True(result.IsSuccess);
        Assert.Equal(string.Empty, result.Value);
    }

    [Fact]
    public void Error_WithEmptyMessage_StoresEmptyMessage()
    {
        var error = new Error("TestCode", string.Empty);
        var result = Result<int>.Failure(error);

        Assert.True(result.IsFailure);
        Assert.Equal(string.Empty, result.Error.Message);
    }

    [Fact]
    public void Map_WithExceptionInMapper_ThrowsException()
    {
        var result = Result<int>.Success(42);

        Assert.Throws<DivideByZeroException>(() =>
            result.Map(x => x / 0));
    }

    [Fact]
    public void Bind_WithExceptionInBinder_ThrowsException()
    {
        var result = Result<int>.Success(42);

        Assert.Throws<InvalidOperationException>(() =>
            result.Bind<int, int>(x => throw new InvalidOperationException("Test")));
    }

    [Fact]
    public void Ensure_WithExceptionInPredicate_ThrowsException()
    {
        var result = Result<int>.Success(42);

        Assert.Throws<InvalidOperationException>(() =>
            result.Ensure(x => throw new InvalidOperationException("Test"), "error"));
    }

    [Fact]
    public void Tap_WithExceptionInAction_ThrowsException()
    {
        var result = Result<int>.Success(42);

        Assert.Throws<InvalidOperationException>(() =>
            result.Tap(x => throw new InvalidOperationException("Test")));
    }

    [Fact]
    public void Match_WithExceptionInSuccessFunc_ThrowsException()
    {
        var result = Result<int>.Success(42);

        Assert.Throws<InvalidOperationException>(() =>
            result.Match(
                onSuccess: x => throw new InvalidOperationException("Test"),
                onFailure: _ => 0));
    }

    [Fact]
    public void Match_WithExceptionInFailureFunc_ThrowsException()
    {
        var result = Result<int>.Failure("error");

        Assert.Throws<InvalidOperationException>(() =>
            result.Match(
                onSuccess: x => 0,
                onFailure: _ => throw new InvalidOperationException("Test")));
    }

    [Fact]
    public void ValueOr_WithExceptionInFactory_ThrowsException()
    {
        var result = Result<int>.Failure("error");

        Assert.Throws<InvalidOperationException>(() =>
            result.ValueOr(_ => throw new InvalidOperationException("Test")));
    }

    [Fact]
    public void Results_Try_WithNullFunc_CatchesException()
    {
        var result = Results.Try<int>(null!);
        
        Assert.True(result.IsFailure);
        Assert.Equal("Exception", result.Error.Code);
    }

    [Fact]
    public void Results_Try_WithNullAction_CatchesException()
    {
        var result = Results.Try(null!);
        
        Assert.True(result.IsFailure);
        Assert.Equal("Exception", result.Error.Code);
    }

    [Fact]
    public void CombineAll_WithEmptyArray_ReturnsEmptyCollection()
    {
        var result = Results.CombineAll<int>();

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public void Combine_WithEmptyArray_ReturnsSuccess()
    {
        var result = Results.Combine();

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ImplicitConversion_FromError_MultipleTimesCreatesSeparateResults()
    {
        Error error = new("TestError", "Test message");
        
        Result<int> result1 = error;
        Result<int> result2 = error;

        Assert.True(result1.IsFailure);
        Assert.True(result2.IsFailure);
        Assert.Equal(error, result1.Error);
        Assert.Equal(error, result2.Error);
    }

    [Fact]
    public void Result_ChainedOperations_WithNullableReferenceTypes()
    {
        Result<string?> GetName() => Result<string?>.Success(null);

        var result = GetName()
            .Map(name => name?.ToUpper())
            .Ensure(name => name != null, "Name is required");

        Assert.True(result.IsFailure);
        Assert.Equal("Name is required", result.Error.Message);
    }

    [Fact]
    public void Result_WithLargeObject_HandlesCorrectly()
    {
        var largeArray = new int[10000];
        Array.Fill(largeArray, 42);

        var result = Result<int[]>.Success(largeArray);

        Assert.True(result.IsSuccess);
        Assert.Equal(10000, result.Value.Length);
        Assert.All(result.Value, x => Assert.Equal(42, x));
    }

    [Fact]
    public async Task TryAsync_WithSynchronousException_CatchesException()
    {
        var result = await Results.TryAsync<int>(async () =>
        {
            throw new InvalidOperationException("Immediate exception");
        });

        Assert.True(result.IsFailure);
        Assert.Equal("Exception", result.Error.Code);
        Assert.Equal("Immediate exception", result.Error.Message);
    }

    [Fact]
    public async Task TryAsync_WithTaskCancellation_CatchesException()
    {
        var result = await Results.TryAsync<int>(async () =>
        {
            var cts = new CancellationTokenSource();
            cts.Cancel();
            await Task.Delay(1000, cts.Token);
            return 42;
        });

        Assert.True(result.IsFailure);
        Assert.Equal("Exception", result.Error.Code);
    }

    [Fact]
    public void MultipleEnsure_WithDifferentErrors_StopsAtFirstFailure()
    {
        var result = Result<int>.Success(5)
            .Ensure(x => x > 0, "Must be positive")
            .Ensure(x => x < 10, "Must be less than 10")
            .Ensure(x => x % 2 == 0, "Must be even");

        Assert.True(result.IsFailure);
        Assert.Equal("Must be even", result.Error.Message);
    }

    [Fact]
    public void ComplexChaining_WithMultipleTypes()
    {
        var result = Result<string>.Success("42")
            .Bind(s => Results.Try(() => int.Parse(s)))
            .Map(i => i * 2)
            .Map(i => i.ToString())
            .Ensure(s => s.Length > 0, "Empty string");

        Assert.True(result.IsSuccess);
        Assert.Equal("84", result.Value);
    }

    [Fact]
    public void Result_Equality_UsesStructEquality()
    {
        var result1 = Result<int>.Success(42);
        var result2 = Result<int>.Success(42);

        // Results are structs with value equality
        Assert.Equal(result1, result2);
    }
}
