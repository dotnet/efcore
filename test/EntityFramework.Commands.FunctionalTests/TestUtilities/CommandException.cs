// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Data.Entity.Commands.TestUtilities
{
    public class CommandException : Exception
    {
        public CommandException(string message, string errorStackTrace, string errorType)
            : base(message)
        {
            ErrorStackTrace = errorStackTrace;
            ErrorType = errorType;
        }

        public string ErrorStackTrace { get; }

        public string ErrorType { get; }
    }
}
