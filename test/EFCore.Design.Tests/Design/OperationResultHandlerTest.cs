// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Design;

public class OperationResultHandlerTest
{
    [ConditionalFact]
    public void Version_is_zero()
        => Assert.Equal(0, new OperationResultHandler().Version);

    [ConditionalFact]
    public void HasResult_defaults_to_false()
        => Assert.False(new OperationResultHandler().HasResult);

    [ConditionalFact]
    public void ErrorType_defaults_to_null()
        => Assert.Null(new OperationResultHandler().ErrorType);

    [ConditionalFact]
    public void OnResult_works()
    {
        var handler = new OperationResultHandler();
        var result = "Twilight Sparkle";

        handler.OnResult(result);

        Assert.True(handler.HasResult);
        Assert.Equal(result, handler.Result);
    }

    [ConditionalFact]
    public void OnError_works()
    {
        var handler = new OperationResultHandler();
        var type = "System.ArgumentOutOfRangeException";
        var message = "Needs to be about 20% more cool.";
        var stackTrace = "The Coolest Trace Yet";

        handler.OnError(type, message, stackTrace);

        Assert.Equal(type, handler.ErrorType);
        Assert.Equal(message, handler.ErrorMessage);
        Assert.Equal(stackTrace, handler.ErrorStackTrace);
    }
}
