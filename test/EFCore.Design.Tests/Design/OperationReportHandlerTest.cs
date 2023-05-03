// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Design;

public class OperationReportHandlerTest
{
    [ConditionalFact]
    public void Version_is_zero()
        => Assert.Equal(0, new OperationReportHandler().Version);

    [ConditionalFact]
    public void On_methods_are_noops_when_null()
    {
        var handler = new OperationReportHandler();

        handler.OnWarning("Princess Celestia is in danger.");
        handler.OnInformation("Princess Celestia is on her way.");
        handler.OnVerbose("Princess Celestia is an alicorn.");
    }

    [ConditionalFact]
    public void OnWarning_works()
    {
        string result = null;
        var handler = new OperationReportHandler(warningHandler: m => result = m);
        var message = "Princess Celestia is in danger.";

        handler.OnWarning(message);

        Assert.Equal(message, result);
    }

    [ConditionalFact]
    public void OnInformation_works()
    {
        string result = null;
        var handler = new OperationReportHandler(informationHandler: m => result = m);
        var message = "Princess Celestia is on her way.";

        handler.OnInformation(message);

        Assert.Equal(message, result);
    }

    [ConditionalFact]
    public void OnVerbose_works()
    {
        string result = null;
        var handler = new OperationReportHandler(verboseHandler: m => result = m);
        var message = "Princess Celestia is an alicorn.";

        handler.OnVerbose(message);

        Assert.Equal(message, result);
    }
}
