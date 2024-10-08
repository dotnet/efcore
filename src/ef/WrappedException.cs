// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Tools;

internal class WrappedException(string type, string message, string stackTrace) : Exception(message)
{
    public string Type { get; } = type;

    public override string ToString()
        => stackTrace;
}
