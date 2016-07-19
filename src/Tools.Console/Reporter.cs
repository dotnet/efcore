// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Tools
{
    public class Reporter
    {
        public const string JsonPrefix = "//BEGIN";
        public const string JsonSuffix = "//END";

        private static readonly object _lock = new object();
        public static bool IsVerbose { get; set; }
        private static IReporter _reporter = new ColorConsoleReporter();

        public static bool SupportsColor => _reporter.SupportsColor;

        public static void Use(IReporter value)
        {
            _reporter = value;
        }

        public static void Verbose(string message)
        {
            if (!IsVerbose)
            {
                return;
            }
            lock (_lock)
            {
                _reporter.Verbose(message);
            }
        }

        public static void Output(string message)
        {
            lock (_lock)
            {
                _reporter.Output(message);
            }
        }

        public static void Warning(string message)
        {
            lock (_lock)
            {
                _reporter.Warning(message);
            }
        }

        public static void Error(string message)
        {
            lock (_lock)
            {
                _reporter.Error(message);
            }
        }
    }
}