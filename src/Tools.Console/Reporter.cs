// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.Tools
{
    public class Reporter
    {
        public const string JsonPrefix = "//BEGIN";
        public const string JsonSuffix = "//END";

        private static object _lock = new object();
        public static bool IsVerbose { get; set; }

        public static void Verbose(string message)
        {
            if (IsVerbose)
            {
                Output(message.Bold().Black());
            }
        }

        public static void Output(string message)
        {
            lock (_lock)
            {
                Console.WriteLine(message);
            }
        }

        public static void Warning(string message)
        {
            lock (_lock)
            {
                Console.WriteLine(message.Bold().Yellow());
            }
        }

        public static void Error(string message)
        {
            lock (_lock)
            {
                Console.Error.WriteLine(message.Bold().Red());
            }
        }
    }
}