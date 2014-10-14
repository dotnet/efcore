// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Commands.Utilities
{
    public class ConsoleCommandLogger : CommandLogger
    {
        private readonly bool _verbose;

        public ConsoleCommandLogger([NotNull] string name, bool verbose)
            : base(name)
        {
            _verbose = verbose;
        }

        public override bool IsEnabled(TraceType eventType)
        {
            return base.IsEnabled(eventType) && (eventType != TraceType.Verbose || _verbose);
        }

        protected override void WriteWarning(string message)
        {
            using (new ColorScope(ConsoleColor.Yellow))
            {
                Console.WriteLine(message);
            }
        }

        protected override void WriteInformation(string message)
        {
            using (new ColorScope(ConsoleColor.Gray))
            {
                Console.WriteLine(message);
            }
        }

        protected override void WriteVerbose(string message)
        {
            using (new ColorScope(ConsoleColor.DarkGray))
            {
                Console.WriteLine(message);
            }
        }
    }
}
