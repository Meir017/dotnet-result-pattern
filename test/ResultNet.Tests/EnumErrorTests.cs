using System;
using System.Threading.Tasks;
using Xunit;

namespace ResultNet.Tests;

public enum TestErrorCode
{
    None = 0,
    InvalidState = 1,
    NotFound = 2,
}

public class EnumErrorTests
{
    [Fact]
    public void ResultE_Success_HasNoError()
    {
        var r = ResultNet.Result<bool, TestErrorCode>.Success(true);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public void ResultE_Failure_WithEnumCode()
    {
        var err = new ResultNet.Error<TestErrorCode>(TestErrorCode.InvalidState, "Bad state");
        var r = ResultNet.Result<bool, TestErrorCode>.Failure(err);
        Assert.True(r.IsFailure);
        Assert.Equal(TestErrorCode.InvalidState, r.Error.Code);
        Assert.Equal("Bad state", r.Error.Message);
    }

    [Fact]
    public void ResultT_E_SuccessValue()
    {
        var r = ResultNet.Result<int, TestErrorCode>.Success(42);
        Assert.True(r.IsSuccess);
        Assert.Equal(42, r.Value);
    }

    [Fact]
    public void ResultT_E_Failure_WithStringImplicit()
    {
        var r = ResultNet.Result<int, TestErrorCode>.Failure("Oops");
        Assert.True(r.IsFailure);
        Assert.Equal("Oops", r.Error.Message);
        Assert.Equal(TestErrorCode.None, r.Error.Code);
    }

    [Fact]
    public void Results_Try_Generic_Code()
    {
        var r = ResultNet.Results.Try<int, TestErrorCode>(() => throw new InvalidOperationException("boom"));
        Assert.True(r.IsFailure);
        Assert.Equal("boom", r.Error.Message);
    }

    [Fact]
    public async Task Results_TryAsync_Generic_Code()
    {
        var r = await ResultNet.Results.TryAsync<int, TestErrorCode>(async () => { await Task.Delay(1); throw new InvalidOperationException("boom"); });
        Assert.True(r.IsFailure);
        Assert.Equal("boom", r.Error.Message);
    }
}
