// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.EntityFrameworkCore.Design.Internal;

namespace Microsoft.EntityFrameworkCore.Commands
{
    public class ConsoleCommandLogger : CommandLogger
    {
        public ConsoleCommandLogger([NotNull] string name)
            : base(name)
        {
        }

        protected override void WriteDebug(string message)
            => Reporter.Verbose.WriteLine(message.Bold().Black());

        protected override void WriteError(string message)
            => Reporter.Error.WriteLine(message.Bold().Red());

        protected override void WriteInformation(string message)
            => Reporter.Error.WriteLine(message);

        protected override void WriteTrace(string message)
            => Reporter.Verbose.WriteLine(message.Bold().Black());

        protected override void WriteWarning(string message)
            => Reporter.Error.WriteLine(message.Bold().Yellow());
    }
}
