// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tools.Commands;

// ReSharper disable once ArrangeTypeModifiers
internal partial class DbContextInfoCommand
{
    protected override int Execute(string[] args)
    {
        using var executor = CreateExecutor(args);
        var result = executor.GetContextInfo(Context!.Value());

        if (_json!.HasValue())
        {
            ReportJsonResult(result);
        }
        else
        {
            ReportResult(result);
        }

        return base.Execute(args);
    }

    private static void ReportJsonResult(IDictionary result)
    {
        Reporter.WriteData("{");
        Reporter.WriteData("  \"type\": " + Json.Literal(result["Type"] as string) + ",");
        Reporter.WriteData("  \"providerName\": " + Json.Literal(result["ProviderName"] as string) + ",");
        Reporter.WriteData("  \"databaseName\": " + Json.Literal(result["DatabaseName"] as string) + ",");
        Reporter.WriteData("  \"dataSource\": " + Json.Literal(result["DataSource"] as string) + ",");
        Reporter.WriteData("  \"options\": " + Json.Literal(result["Options"] as string));
        Reporter.WriteData("}");
    }

    private static void ReportResult(IDictionary result)
    {
        Reporter.WriteData(Resources.DbContextType(result["Type"]));
        Reporter.WriteData(Resources.ProviderName(result["ProviderName"]));
        Reporter.WriteData(Resources.DatabaseName(result["DatabaseName"]));
        Reporter.WriteData(Resources.DataSource(result["DataSource"]));
        Reporter.WriteData(Resources.Options(result["Options"]));
    }
}
