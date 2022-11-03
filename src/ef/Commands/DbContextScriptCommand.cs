// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;
using System.Text;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tools.Commands;

// ReSharper disable once ArrangeTypeModifiers
internal partial class DbContextScriptCommand
{
    protected override int Execute(string[] args)
    {
        using var executor = CreateExecutor(args);
        var sql = executor.ScriptDbContext(Context!.Value());

        if (!_output!.HasValue())
        {
            Reporter.WriteData(sql);
        }
        else
        {
            var output = _output.Value()!;
            if (WorkingDir!.HasValue())
            {
                output = Path.Combine(WorkingDir.Value()!, output);
            }

            var directory = Path.GetDirectoryName(output);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            Reporter.WriteVerbose(Resources.WritingFile(_output.Value()));
            File.WriteAllText(output, sql, Encoding.UTF8);
        }

        return base.Execute(args);
    }
}
