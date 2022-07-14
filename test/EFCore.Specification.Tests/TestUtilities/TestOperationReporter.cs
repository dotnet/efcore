// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Design.Internal;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class TestOperationReporter : IOperationReporter
{
    private readonly ITestOutputHelper _output;
    private readonly List<(LogLevel, string)> _messages = new();

    public TestOperationReporter(ITestOutputHelper output = null)
        => _output = output;

    public IReadOnlyList<(LogLevel Level, string Message)> Messages
        => _messages;

    public void Clear()
        => _messages.Clear();

    public void WriteInformation(string message)
    {
        _output?.WriteLine("info:    " + message);
        _messages.Add((LogLevel.Information, message));
    }

    public void WriteVerbose(string message)
    {
        _output?.WriteLine("verbose: " + message);
        _messages.Add((LogLevel.Debug, message));
    }

    public void WriteWarning(string message)
    {
        _output?.WriteLine("warn:    " + message);
        _messages.Add((LogLevel.Warning, message));
    }

    public void WriteError(string message)
    {
        _output?.WriteLine("error:   " + message);
        _messages.Add((LogLevel.Error, message));
    }
}
