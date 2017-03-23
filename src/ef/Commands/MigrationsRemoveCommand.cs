// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.EntityFrameworkCore.Tools.Commands
{
    partial class MigrationsRemoveCommand
    {
        protected override int Execute()
        {
            var deletedFiles = CreateExecutor().RemoveMigration(Context.Value(), _force.HasValue()).ToList();
            if (_json.HasValue())
            {
                ReportJsonResults(deletedFiles);
            }

            return base.Execute();
        }

        private void ReportJsonResults(IReadOnlyList<string> files)
        {
            Reporter.WriteData("[");

            for (var i = 0; i < files.Count; i++)
            {
                var line = "  \"" + Json.Escape(files[i]) + "\"";
                if (i != files.Count - 1)
                {
                    line += ",";
                }

                Reporter.WriteData(line);
            }

            Reporter.WriteData("]");
        }
    }
}
