// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Design.Internal
{
    public class ConsoleCommandLogger : CommandLogger
    {
        private static readonly object _sync = new object();
        private readonly bool _verbose;

        public ConsoleCommandLogger([NotNull] string name, bool verbose)
            : base(name)
        {
            _verbose = verbose;
        }

        public override bool IsEnabled(LogLevel logLevel) =>
            base.IsEnabled(logLevel) && (logLevel > LogLevel.Verbose || _verbose);

        protected override void WriteError(string message)
        {
            lock (_sync)
            {
                using (new ColorScope(ConsoleColor.Red))
                {
                    Console.WriteLine(message);
                }
            }
        }

        protected override void WriteWarning(string message)
        {
            lock (_sync)
            {
                using (new ColorScope(ConsoleColor.Yellow))
                {
                    Console.WriteLine(message);
                }
            }
        }

        protected override void WriteInformation(string message)
        {
            lock (_sync)
            {
                using (new ColorScope(ConsoleColor.Gray))
                {
                    Console.WriteLine(message);
                }
            }
        }

        protected override void WriteVerbose(string message)
        {
            lock (_sync)
            {
                using (new ColorScope(ConsoleColor.DarkGray))
                {
                    Console.WriteLine(message);
                }
            }
        }

        protected override void WriteDebug(string message) => WriteVerbose(message);
    }
}
