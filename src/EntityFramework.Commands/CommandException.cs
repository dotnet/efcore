// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Commands
{
    public class CommandException : Exception
    {
        public CommandException()
        {
        }

        public CommandException([NotNull] string message)
            : base(message)
        {
        }

        public CommandException([NotNull] string message, [CanBeNull] Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
