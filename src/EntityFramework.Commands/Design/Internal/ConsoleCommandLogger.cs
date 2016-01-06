// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if DNX451 || DNXCORE50

using JetBrains.Annotations;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace Microsoft.Data.Entity.Design.Internal
{
    internal class ConsoleCommandLogger : CommandLogger
    {
        private readonly bool _verbose;
        private readonly AnsiConsole _ansiConsole;

        public ConsoleCommandLogger([NotNull] string name, bool verbose, [NotNull] AnsiConsole ansiConsole)
            : base(name)
        {
            _verbose = verbose;
            _ansiConsole = ansiConsole;
        }

        public override bool IsEnabled(LogLevel logLevel) =>
            base.IsEnabled(logLevel) && (logLevel > LogLevel.Debug || _verbose);

        protected override void WriteError(string message)
            => _ansiConsole.WriteLine("\x1b[1m\x1b[31m" + message + "\x1b[22m\x1b[39m");

        protected override void WriteWarning(string message)
            => _ansiConsole.WriteLine("\x1b[1m\x1b[33m" + message + "\x1b[22m\x1b[39m");

        protected override void WriteInformation(string message)
            => _ansiConsole.WriteLine("\x1b[37m" + message + "\x1b[39m");

        protected override void WriteDebug(string message)
            => _ansiConsole.WriteLine("\x1b[1m\x1b[30m" + message + "\x1b[22m\x1b[39m");

        protected override void WriteTrace(string message) => WriteDebug(message);
    }
}

#endif
