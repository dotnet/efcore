// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;

namespace Microsoft.EntityFrameworkCore.Tools
{
    public class PrefixConsoleReporter : ConsoleReporter
    {
        // private const string DebugPrefix = "DEBUG   : ";
        private const string VerbosePrefix = "VERBOSE : ";
        private const string WarningPrefix = "WARNING : ";
        private const string ErrorPrefix = "ERROR   : ";
        private const string OutputPrefix = "OUTPUT  : ";

        public override void Verbose(string message)
            => base.Verbose(PrefixLines(VerbosePrefix, message));

        public override void Warning(string message)
            => base.Verbose(PrefixLines(WarningPrefix, message));

        public override void Error(string message)
            // Intentionally does not write to stderr
            => base.Output(PrefixLines(ErrorPrefix, message));

        public override void Output(string message)
            => base.Output(PrefixLines(OutputPrefix, message));

        private static string PrefixLines(string prefix, string message)
        {
            var lines = message.Split(new[] { Environment.NewLine, "\n" }, StringSplitOptions.None).Select(m => prefix + m);

            if (message.EndsWith(Environment.NewLine, StringComparison.OrdinalIgnoreCase))
            {
                // remove hanging empty line
                lines = lines.Take(lines.Count() - 1);
            }

            return string.Join(Environment.NewLine, lines);
        }
    }
}
