// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Tools.Core.Utilities.Internal;

namespace Microsoft.EntityFrameworkCore.Tools.Cli
{
    public class ConsoleCommandLogger : CommandLogger
    {
        private static object _lock = new object();
        public static bool IsVerbose { get; set; }

        public static void Output(string message)
        {
            lock (_lock)
            {
                Console.WriteLine(message);
            }
        }

        public static void Error(string message)
        {
            lock (_lock)
            {
                Console.Error.WriteLine(message);
            }
        }

        public static void Verbose(string message)
        {
            if (IsVerbose)
            {
                Output(message);
            }
        }

        public static void Json(object result)
        {
            lock (_lock)
            {
                Console.WriteLine("//BEGIN");
                Console.WriteLine(JsonUtility.Serialize(result));
                Console.WriteLine("//END");
            }
        }

        public ConsoleCommandLogger([NotNull] string name)
            : base(name)
        {
        }

        protected override void WriteTrace(string message)
            => Verbose(message.Bold().Black());

        protected override void WriteDebug(string message)
            => Verbose(message.Bold().Black());

        protected override void WriteInformation(string message)
            => Output(message);

        protected override void WriteWarning(string message)
            => Output(message.Bold().Yellow());

        protected override void WriteError(string message)
            => Error(message.Bold().Red());
    }
}