// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.Collections.Generic;

namespace Microsoft.DotNet.Cli.CommandLine;
using System.Linq;

internal class CommandArgument
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public List<string> Values { get; } = [];
    public bool MultipleValues { get; set; }

    public string? Value
        => Values.FirstOrDefault();
}
