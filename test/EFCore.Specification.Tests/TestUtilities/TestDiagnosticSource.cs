// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class TestDiagnosticSource : DiagnosticSource
{
    public string EnableFor { get; set; } = null!;
    public string LoggedEventName { get; set; } = null!;
    public string LoggedMessage { get; set; } = null!;

    public override void Write(string name, object? value)
    {
        LoggedEventName = name;

        Assert.IsAssignableFrom<EventData>(value);

        LoggedMessage = value!.ToString()!;

        var exceptionProperty = value.GetType().GetTypeInfo().GetDeclaredProperty("Exception");
        if (exceptionProperty != null)
        {
            Assert.IsAssignableFrom<IErrorEventData>(value);
        }
    }

    public override bool IsEnabled(string name)
        => name == EnableFor;
}
