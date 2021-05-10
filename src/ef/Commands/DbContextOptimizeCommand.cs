// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Tools.Commands
{
    // ReSharper disable once ArrangeTypeModifiers
    internal partial class DbContextOptimizeCommand
    {
        protected override int Execute(string[] args)
        {
            using var executor = CreateExecutor(args);
            var result = executor.Optimize(
                _outputDir!.Value(),
                _namespace!.Value(),
                Context!.Value());

            if (_json!.HasValue())
            {
                ReportJsonResults(result);
            }

            return base.Execute(args);
        }
        private static void ReportJsonResults(IReadOnlyList<string> result)
        {
            Reporter.WriteData("{");
            Reporter.WriteData("  \"files\": [");

            for (var i = 0; i < result.Count; i++)
            {
                var line = "    " + Json.Literal(result[i]);
                if (i != result.Count - 1)
                {
                    line += ",";
                }

                Reporter.WriteData(line);
            }

            Reporter.WriteData("  ]");
            Reporter.WriteData("}");
        }
    }
}
