// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tools.Commands;

// ReSharper disable once ArrangeTypeModifiers
internal partial class MigrationsScriptCommand
{
    protected override int Execute(string[] args)
    {
        using var executor = CreateExecutor(args);

        var sql = executor.ScriptMigration(
            _from!.Value,
            _to!.Value,
            _idempotent!.HasValue(),
            _noTransactions!.HasValue(),
            Context!.Value());

        if (!_output!.HasValue())
        {
            Reporter.WriteData(sql);
        }
        else
        {
            var output = ResolveOutputPath(_output.Value()!, WorkingDir!.Value());

            var directory = Path.GetDirectoryName(output);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            Reporter.WriteInformation(Resources.WritingFile(output));
            File.WriteAllText(output, sql, Encoding.UTF8);
        }

        return base.Execute(args);
    }
}
