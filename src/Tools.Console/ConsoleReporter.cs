// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.Tools
{
    public class ConsoleReporter : IReporter
    {
        public virtual bool SupportsColor => false;
        public virtual void Verbose(string message) => Console.WriteLine(message);
        public virtual void Warning(string message) => Console.WriteLine(message);
        public virtual void Output(string message) => Console.WriteLine(message);
        public virtual void Error(string message) => Console.Error.WriteLine(message);
    }
}
