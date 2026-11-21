// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace BenchmarkDotNet.Attributes;

// Used to generate a single BDN output column combining all the parameters.
// Required by the perflab's PowerBI setup.
public class ParamsSummaryColumn : IColumn
{
    public string Id
        => nameof(ParamsSummaryColumn);

    public string ColumnName { get; } = "Params";

    public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase)
        => false;

    public string GetValue(Summary summary, BenchmarkCase benchmarkCase)
        => benchmarkCase.Parameters.DisplayInfo;

    public bool IsAvailable(Summary summary)
        => true;

    public bool AlwaysShow
        => true;

    public ColumnCategory Category
        => ColumnCategory.Params;

    public int PriorityInCategory
        => 0;

    public override string ToString()
        => ColumnName;

    public bool IsNumeric
        => false;

    public UnitType UnitType
        => UnitType.Dimensionless;

    public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style)
        => GetValue(summary, benchmarkCase);

    public string Legend
        => "Summary of all parameter values";
}
