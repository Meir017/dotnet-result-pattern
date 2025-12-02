namespace ResultNet.Tests;

public class ResultTests
{
    [Fact]
    public void Success_CreatesSuccessfulResult()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
    }

    [Fact]
    public void Failure_WithError_CreatesFailedResult()
    {
        var error = new Error("TestError", "Test error message");
        var result = Result.Failure(error);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void Failure_WithMessage_CreatesFailedResult()
    {
        var result = Result.Failure("Test error message");

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Error", result.Error.Code);
        Assert.Equal("Test error message", result.Error.Message);
    }

    [Fact]
    public void ImplicitConversion_FromError_CreatesFailedResult()
    {
        Error error = new("TestError", "Test message");
        Result result = error;

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void AccessingError_OnSuccessfulResult_ThrowsException()
    {
        var result = Result.Success();

        var exception = Assert.Throws<InvalidOperationException>(() => result.Error);
        Assert.Contains("Cannot access Error on a successful result", exception.Message);
    }

    [Fact]
    public void Match_OnSuccess_ExecutesSuccessFunc()
    {
        var result = Result.Success();

        var value = result.Match(
            onSuccess: () => "success",
            onFailure: _ => "failure");

        Assert.Equal("success", value);
    }

    [Fact]
    public void Match_OnFailure_ExecutesFailureFunc()
    {
        var result = Result.Failure("error");

        var value = result.Match(
            onSuccess: () => "success",
            onFailure: error => $"failed: {error.Message}");

        Assert.Equal("failed: error", value);
    }

    [Fact]
    public void Match_WithActions_OnSuccess_ExecutesSuccessAction()
    {
        var result = Result.Success();
        var executed = "";

        result.Match(
            onSuccess: () => executed = "success",
            onFailure: _ => executed = "failure");

        Assert.Equal("success", executed);
    }

    [Fact]
    public void Match_WithActions_OnFailure_ExecutesFailureAction()
    {
        var result = Result.Failure("error");
        var executed = "";

        result.Match(
            onSuccess: () => executed = "success",
            onFailure: error => executed = $"failed: {error.Message}");

        Assert.Equal("failed: error", executed);
    }

    [Fact]
    public void Tap_OnSuccess_ExecutesAction()
    {
        var result = Result.Success();
        var executed = false;

        var newResult = result.Tap(() => executed = true);

        Assert.True(executed);
        Assert.True(newResult.IsSuccess);
    }

    [Fact]
    public void Tap_OnFailure_DoesNotExecuteAction()
    {
        var result = Result.Failure("error");
        var executed = false;

        var newResult = result.Tap(() => executed = true);

        Assert.False(executed);
        Assert.True(newResult.IsFailure);
    }

    [Fact]
    public void TapError_OnFailure_ExecutesAction()
    {
        var result = Result.Failure("error");
        Error? capturedError = null;

        var newResult = result.TapError(error => capturedError = error);

        Assert.NotNull(capturedError);
        Assert.Equal("error", capturedError.Value.Message);
        Assert.True(newResult.IsFailure);
    }

    [Fact]
    public void TapError_OnSuccess_DoesNotExecuteAction()
    {
        var result = Result.Success();
        var executed = false;

        var newResult = result.TapError(_ => executed = true);

        Assert.False(executed);
        Assert.True(newResult.IsSuccess);
    }

    [Fact]
    public void OnSuccess_OnSuccessfulResult_ExecutesAction()
    {
        var result = Result.Success();
        var executed = false;

        result.OnSuccess(() => executed = true);

        Assert.True(executed);
    }

    [Fact]
    public void OnSuccess_OnFailedResult_DoesNotExecuteAction()
    {
        var result = Result.Failure("error");
        var executed = false;

        result.OnSuccess(() => executed = true);

        Assert.False(executed);
    }

    [Fact]
    public void OnFailure_OnFailedResult_ExecutesAction()
    {
        var result = Result.Failure("error");
        Error? capturedError = null;

        result.OnFailure(error => capturedError = error);

        Assert.NotNull(capturedError);
        Assert.Equal("error", capturedError.Value.Message);
    }

    [Fact]
    public void OnFailure_OnSuccessfulResult_DoesNotExecuteAction()
    {
        var result = Result.Success();
        var executed = false;

        result.OnFailure(_ => executed = true);

        Assert.False(executed);
    }
}
