using Xunit;

namespace ResultNet.Tests;

public enum ErrorCode
{
    None = 0,
    NotFound,
    ValidationFailed,
    Unauthorized,
    InternalError
}

public class GenericErrorCodeTests
{
    [Fact]
    public void Error_WithEnumCode_ShouldStoreCodeAndMessage()
    {
        // Arrange
        var code = ErrorCode.NotFound;
        var message = "Resource not found";

        // Act
        var error = new Error<ErrorCode>(code, message);

        // Assert
        Assert.Equal(code, error.Code);
        Assert.Equal(message, error.Message);
    }

    [Fact]
    public void Error_ImplicitFromString_ShouldUseDefaultCode()
    {
        // Arrange
        var message = "Some error message";

        // Act
        Error<ErrorCode> error = message;

        // Assert
        Assert.Equal(ErrorCode.None, error.Code);
        Assert.Equal(message, error.Message);
    }









    [Fact]
    public void ResultTTCode_Success_ShouldStoreValue()
    {
        // Arrange
        var value = 42;

        // Act
        var result = Result<int, ErrorCode>.Success(value);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(value, result.Value);
    }

    [Fact]
    public void ResultTTCode_Failure_ShouldStoreError()
    {
        // Arrange
        var error = new Error<ErrorCode>(ErrorCode.NotFound, "Item not found");

        // Act
        var result = Result<int, ErrorCode>.Failure(error);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCode.NotFound, result.Error.Code);
        Assert.Equal("Item not found", result.Error.Message);
    }

    [Fact]
    public void ResultTTCode_ImplicitFromValue_ShouldBeSuccess()
    {
        // Arrange
        var value = "test";

        // Act
        Result<string, ErrorCode> result = value;

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(value, result.Value);
    }

    [Fact]
    public void ResultTTCode_ImplicitFromError_ShouldBeFailure()
    {
        // Arrange
        var error = new Error<ErrorCode>(ErrorCode.ValidationFailed, "Invalid input");

        // Act
        Result<string, ErrorCode> result = error;

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.ValidationFailed, result.Error.Code);
    }

    [Fact]
    public void ResultTTCode_Match_ShouldCallCorrectCallback()
    {
        // Arrange
        var successResult = Result<int, ErrorCode>.Success(100);
        var failureResult = Result<int, ErrorCode>.Failure(ErrorCode.NotFound, "Not found");

        // Act
        var successValue = successResult.Match(
            onSuccess: val => $"value: {val}",
            onFailure: err => $"error: {err.Code}");

        var failureValue = failureResult.Match(
            onSuccess: val => $"value: {val}",
            onFailure: err => $"error: {err.Code}");

        // Assert
        Assert.Equal("value: 100", successValue);
        Assert.Equal("error: NotFound", failureValue);
    }

    [Fact]
    public void ResultTTCode_AccessValue_OnFailure_ShouldThrow()
    {
        // Arrange
        var result = Result<int, ErrorCode>.Failure(ErrorCode.InternalError, "Error");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void ResultTTCode_AccessError_OnSuccess_ShouldThrow()
    {
        // Arrange
        var result = Result<int, ErrorCode>.Success(42);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => result.Error);
    }


}
