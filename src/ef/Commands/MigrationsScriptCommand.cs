// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Text;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tools.Commands
{
    // ReSharper disable once ArrangeTypeModifiers
    internal partial class MigrationsScriptCommand
    {
        protected override int Execute(string[] args)
        {
            var sql = CreateExecutor(args).ScriptMigration(
                _from.Value,
                _to.Value,
                _idempotent.HasValue(),
                _noTransactions.HasValue(),
                Context.Value());

            if (!_output.HasValue())
            {
                Reporter.WriteData(sql);
            }
            else
            {
                var output = _output.Value();
                if (WorkingDir.HasValue())
                {
                    output = Path.Combine(WorkingDir.Value(), output);
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
}
