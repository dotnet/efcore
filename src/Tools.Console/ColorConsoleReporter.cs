// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.Tools
{
    public class ColorConsoleReporter : ConsoleReporter
    {
        public override bool SupportsColor => true;

        public override void Verbose(string message)
            => base.Verbose(message.Bold().White());

        public override void Warning(string message)
            => base.Verbose(message.Bold().Yellow());

        public override void Error(string message)
            => base.Error(message.Bold().Red());
    }
}
