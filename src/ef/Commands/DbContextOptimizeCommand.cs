// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tools.Commands;

// ReSharper disable once ArrangeTypeModifiers
internal partial class DbContextOptimizeCommand
{
    protected override void Validate()
    {
        base.Validate();

        if (_noScaffold!.HasValue()
            && !_precompileQueries!.HasValue())
        {
            throw new CommandException(Resources.MissingConditionalOption(_precompileQueries.LongName, _noScaffold.LongName));
        }

        if (_precompileQueries!.HasValue())
        {
            Reporter.WriteWarning(Resources.PrecompileQueriesWarning);
        }
    }

    protected override int Execute(string[] args)
    {
        if (new SemanticVersionComparer().Compare(EFCoreVersion, "6.0.0") < 0)
        {
            throw new CommandException(Resources.VersionRequired("6.0.0"));
        }

        using var executor = CreateExecutor(args);
        var result = executor.OptimizeContext(
            _outputDir!.Value(),
            _namespace!.Value(),
            Context!.Value(),
            _suffix!.Value() ?? "",
            !_noScaffold!.HasValue(),
            _precompileQueries!.HasValue());

        ReportResults(result);

        return base.Execute(args);
    }

    private static void ReportResults(IEnumerable<string> generatedFiles)
    {
        foreach (var file in generatedFiles)
        {
            Reporter.WriteData(file);
        }
    }
}
