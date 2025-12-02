namespace ResultNet.Tests;

public class ResultTTests
{
    [Fact]
    public void Success_WithValue_CreatesSuccessfulResult()
    {
        var result = Result<int>.Success(42);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Failure_WithError_CreatesFailedResult()
    {
        var error = new Error("TestError", "Test error message");
        var result = Result<int>.Failure(error);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void Failure_WithMessage_CreatesFailedResult()
    {
        var result = Result<int>.Failure("Test error message");

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Error", result.Error.Code);
        Assert.Equal("Test error message", result.Error.Message);
    }

    [Fact]
    public void ImplicitConversion_FromValue_CreatesSuccessfulResult()
    {
        Result<int> result = 42;

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void ImplicitConversion_FromError_CreatesFailedResult()
    {
        Error error = new("TestError", "Test message");
        Result<int> result = error;

        Assert.False(result.IsSuccess);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void AccessingValue_OnFailedResult_ThrowsException()
    {
        var result = Result<int>.Failure("error");

        var exception = Assert.Throws<InvalidOperationException>(() => result.Value);
        Assert.Contains("Cannot access Value on a failed result", exception.Message);
    }

    [Fact]
    public void AccessingError_OnSuccessfulResult_ThrowsException()
    {
        var result = Result<int>.Success(42);

        var exception = Assert.Throws<InvalidOperationException>(() => result.Error);
        Assert.Contains("Cannot access Error on a successful result", exception.Message);
    }

    [Fact]
    public void Match_OnSuccess_ExecutesSuccessFunc()
    {
        var result = Result<int>.Success(42);

        var value = result.Match(
            onSuccess: x => $"success: {x}",
            onFailure: _ => "failure");

        Assert.Equal("success: 42", value);
    }

    [Fact]
    public void Match_OnFailure_ExecutesFailureFunc()
    {
        var result = Result<int>.Failure("error");

        var value = result.Match(
            onSuccess: x => $"success: {x}",
            onFailure: error => $"failed: {error.Message}");

        Assert.Equal("failed: error", value);
    }

    [Fact]
    public void Match_WithActions_OnSuccess_ExecutesSuccessAction()
    {
        var result = Result<int>.Success(42);
        var executed = "";

        result.Match(
            onSuccess: x => executed = $"success: {x}",
            onFailure: _ => executed = "failure");

        Assert.Equal("success: 42", executed);
    }

    [Fact]
    public void Match_WithActions_OnFailure_ExecutesFailureAction()
    {
        var result = Result<int>.Failure("error");
        var executed = "";

        result.Match(
            onSuccess: x => executed = $"success: {x}",
            onFailure: error => executed = $"failed: {error.Message}");

        Assert.Equal("failed: error", executed);
    }

    [Fact]
    public void Map_OnSuccess_TransformsValue()
    {
        var result = Result<int>.Success(42);

        var mapped = result.Map(x => x.ToString());

        Assert.True(mapped.IsSuccess);
        Assert.Equal("42", mapped.Value);
    }

    [Fact]
    public void Map_OnFailure_PropagatesError()
    {
        var error = new Error("TestError", "Test message");
        var result = Result<int>.Failure(error);

        var mapped = result.Map(x => x.ToString());

        Assert.True(mapped.IsFailure);
        Assert.Equal(error, mapped.Error);
    }

    [Fact]
    public void Bind_OnSuccess_ExecutesBinder()
    {
        var result = Result<int>.Success(42);

        var bound = result.Bind(x => Result<string>.Success(x.ToString()));

        Assert.True(bound.IsSuccess);
        Assert.Equal("42", bound.Value);
    }

    [Fact]
    public void Bind_OnSuccess_WithFailedBinder_ReturnsFailure()
    {
        var result = Result<int>.Success(42);

        var bound = result.Bind(x => Result<string>.Failure("binder error"));

        Assert.True(bound.IsFailure);
        Assert.Equal("binder error", bound.Error.Message);
    }

    [Fact]
    public void Bind_OnFailure_PropagatesError()
    {
        var error = new Error("TestError", "Test message");
        var result = Result<int>.Failure(error);

        var bound = result.Bind(x => Result<string>.Success(x.ToString()));

        Assert.True(bound.IsFailure);
        Assert.Equal(error, bound.Error);
    }

    [Fact]
    public void Ensure_OnSuccess_WithValidPredicate_ReturnsSuccess()
    {
        var result = Result<int>.Success(42);

        var ensured = result.Ensure(x => x > 0, "Must be positive");

        Assert.True(ensured.IsSuccess);
        Assert.Equal(42, ensured.Value);
    }

    [Fact]
    public void Ensure_OnSuccess_WithInvalidPredicate_ReturnsFailure()
    {
        var result = Result<int>.Success(-5);

        var ensured = result.Ensure(x => x > 0, "Must be positive");

        Assert.True(ensured.IsFailure);
        Assert.Equal("Must be positive", ensured.Error.Message);
    }

    [Fact]
    public void Ensure_OnFailure_PropagatesError()
    {
        var error = new Error("TestError", "Original error");
        var result = Result<int>.Failure(error);

        var ensured = result.Ensure(x => x > 0, "Must be positive");

        Assert.True(ensured.IsFailure);
        Assert.Equal(error, ensured.Error);
    }

    [Fact]
    public void Tap_OnSuccess_ExecutesAction()
    {
        var result = Result<int>.Success(42);
        var capturedValue = 0;

        var newResult = result.Tap(x => capturedValue = x);

        Assert.Equal(42, capturedValue);
        Assert.True(newResult.IsSuccess);
        Assert.Equal(42, newResult.Value);
    }

    [Fact]
    public void Tap_OnFailure_DoesNotExecuteAction()
    {
        var result = Result<int>.Failure("error");
        var executed = false;

        var newResult = result.Tap(_ => executed = true);

        Assert.False(executed);
        Assert.True(newResult.IsFailure);
    }

    [Fact]
    public void TapError_OnFailure_ExecutesAction()
    {
        var result = Result<int>.Failure("error");
        Error? capturedError = null;

        var newResult = result.TapError(error => capturedError = error);

        Assert.NotNull(capturedError);
        Assert.Equal("error", capturedError.Value.Message);
        Assert.True(newResult.IsFailure);
    }

    [Fact]
    public void TapError_OnSuccess_DoesNotExecuteAction()
    {
        var result = Result<int>.Success(42);
        var executed = false;

        var newResult = result.TapError(_ => executed = true);

        Assert.False(executed);
        Assert.True(newResult.IsSuccess);
    }

    [Fact]
    public void ValueOr_OnSuccess_ReturnsValue()
    {
        var result = Result<int>.Success(42);

        var value = result.ValueOr(0);

        Assert.Equal(42, value);
    }

    [Fact]
    public void ValueOr_OnFailure_ReturnsDefaultValue()
    {
        var result = Result<int>.Failure("error");

        var value = result.ValueOr(0);

        Assert.Equal(0, value);
    }

    [Fact]
    public void ValueOr_WithFactory_OnSuccess_ReturnsValue()
    {
        var result = Result<int>.Success(42);

        var value = result.ValueOr(_ => 0);

        Assert.Equal(42, value);
    }

    [Fact]
    public void ValueOr_WithFactory_OnFailure_ExecutesFactory()
    {
        var result = Result<int>.Failure("error");

        var value = result.ValueOr(error => error.Message.Length);

        Assert.Equal(5, value);
    }

    [Fact]
    public void OnSuccess_OnSuccessfulResult_ExecutesAction()
    {
        var result = Result<int>.Success(42);
        var capturedValue = 0;

        result.OnSuccess(x => capturedValue = x);

        Assert.Equal(42, capturedValue);
    }

    [Fact]
    public void OnSuccess_OnFailedResult_DoesNotExecuteAction()
    {
        var result = Result<int>.Failure("error");
        var executed = false;

        result.OnSuccess(_ => executed = true);

        Assert.False(executed);
    }

    [Fact]
    public void OnFailure_OnFailedResult_ExecutesAction()
    {
        var result = Result<int>.Failure("error");
        Error? capturedError = null;

        result.OnFailure(error => capturedError = error);

        Assert.NotNull(capturedError);
        Assert.Equal("error", capturedError.Value.Message);
    }

    [Fact]
    public void OnFailure_OnSuccessfulResult_DoesNotExecuteAction()
    {
        var result = Result<int>.Success(42);
        var executed = false;

        result.OnFailure(_ => executed = true);

        Assert.False(executed);
    }

    [Fact]
    public void ChainingOperations_Success_ExecutesAllSteps()
    {
        var result = Result<int>.Success(5)
            .Map(x => x * 2)
            .Ensure(x => x > 5, "Must be greater than 5")
            .Map(x => x.ToString())
            .Tap(x => { })
            .Map(x => x + "!")
            .OnSuccess(x => { });

        Assert.True(result.IsSuccess);
        Assert.Equal("10!", result.Value);
    }

    [Fact]
    public void ChainingOperations_WithFailure_StopsAtFirstFailure()
    {
        var mapExecuted = false;

        var result = Result<int>.Success(5)
            .Map(x => x * 2)
            .Ensure(x => x > 20, "Must be greater than 20")
            .Map(x => 
            {
                mapExecuted = true;
                return x.ToString();
            });

        Assert.True(result.IsFailure);
        Assert.False(mapExecuted);
        Assert.Equal("Must be greater than 20", result.Error.Message);
    }
}
