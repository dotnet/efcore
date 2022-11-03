// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tools.Commands;

// ReSharper disable once ArrangeTypeModifiers
internal partial class DbContextScaffoldCommand
{
    protected override void Validate()
    {
        base.Validate();

        if (string.IsNullOrEmpty(_connection!.Value))
        {
            throw new CommandException(Resources.MissingArgument(_connection.Name));
        }

        if (string.IsNullOrEmpty(_provider!.Value))
        {
            throw new CommandException(Resources.MissingArgument(_provider.Name));
        }
    }

    protected override int Execute(string[] args)
    {
        using var executor = CreateExecutor(args);
        var result = executor.ScaffoldContext(
            _provider!.Value!,
            _connection!.Value!,
            _outputDir!.Value(),
            _contextDir!.Value(),
            _context!.Value(),
            _schemas!.Values!,
            _tables!.Values!,
            _dataAnnotations!.HasValue(),
            _force!.HasValue(),
            _useDatabaseNames!.HasValue(),
            _namespace!.Value(),
            _contextNamespace!.Value(),
            _suppressOnConfiguring!.HasValue(),
            _noPluralize!.HasValue());
        if (_json!.HasValue())
        {
            ReportJsonResults(result);
        }

        return base.Execute(args);
    }

    private static void ReportJsonResults(IDictionary result)
    {
        Reporter.WriteData("{");
        Reporter.WriteData("  \"contextFile\": " + Json.Literal(result["ContextFile"] as string) + ",");
        Reporter.WriteData("  \"entityTypeFiles\": [");

        var files = (IReadOnlyList<string?>)result["EntityTypeFiles"]!;
        for (var i = 0; i < files.Count; i++)
        {
            var line = "    " + Json.Literal(files[i]);
            if (i != files.Count - 1)
            {
                line += ",";
            }

            Reporter.WriteData(line);
        }

        Reporter.WriteData("  ]");
        Reporter.WriteData("}");
    }
}
