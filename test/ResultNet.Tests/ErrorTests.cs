namespace ResultNet.Tests;

public class ErrorTests
{
    [Fact]
    public void Error_CreatedWithCodeAndMessage_StoresValues()
    {
        var error = new Error("TestCode", "Test message");

        Assert.Equal("TestCode", error.Code);
        Assert.Equal("Test message", error.Message);
    }

    [Fact]
    public void ImplicitConversion_FromString_CreatesErrorWithDefaultCode()
    {
        Error error = "Test message";

        Assert.Equal("Error", error.Code);
        Assert.Equal("Test message", error.Message);
    }

    [Fact]
    public void Equality_SameCodeAndMessage_AreEqual()
    {
        var error1 = new Error("TestCode", "Test message");
        var error2 = new Error("TestCode", "Test message");

        Assert.Equal(error1, error2);
    }

    [Fact]
    public void Equality_DifferentCode_AreNotEqual()
    {
        var error1 = new Error("TestCode1", "Test message");
        var error2 = new Error("TestCode2", "Test message");

        Assert.NotEqual(error1, error2);
    }

    [Fact]
    public void Equality_DifferentMessage_AreNotEqual()
    {
        var error1 = new Error("TestCode", "Test message 1");
        var error2 = new Error("TestCode", "Test message 2");

        Assert.NotEqual(error1, error2);
    }

    [Fact]
    public void CommonErrorCodes_CanBeUsedThroughoutApplication()
    {
        var validationError = new Error("Validation", "Invalid input");
        var notFoundError = new Error("NotFound", "Resource not found");
        var unauthorizedError = new Error("Unauthorized", "Access denied");

        Assert.Equal("Validation", validationError.Code);
        Assert.Equal("NotFound", notFoundError.Code);
        Assert.Equal("Unauthorized", unauthorizedError.Code);
    }
}
