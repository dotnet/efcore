// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Tools;

internal class WrappedException : Exception
{
    private readonly string _stackTrace;

    public WrappedException(string type, string message, string stackTrace)
        : base(message)
    {
        Type = type;
        _stackTrace = stackTrace;
    }

    public string Type { get; }

    public override string ToString()
        => _stackTrace;
}
