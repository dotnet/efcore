// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.EntityFrameworkCore.Tools
{
    internal class CommandException : Exception
    {
        public CommandException(string message)
            : base(message)
        {
        }

        public CommandException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
