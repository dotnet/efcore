// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class ListDiagnosticSource(List<Tuple<string, object>> diagnosticList) : DiagnosticSource
{
    public List<Tuple<string, object>> DiagnosticList { get; } = diagnosticList;

    public override void Write(string diagnosticName, object parameters)
        => DiagnosticList?.Add(new Tuple<string, object>(diagnosticName, parameters));

    public override bool IsEnabled(string diagnosticName)
        => true;
}
