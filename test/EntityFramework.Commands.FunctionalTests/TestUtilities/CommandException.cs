// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Data.Entity.Commands.TestUtilities
{
    public class CommandException : Exception
    {
        private readonly string _errorStackTrace;
        private readonly string _errorType;

        public CommandException(string message, string errorStackTrace, string errorType)
            : base(message)
        {
            _errorStackTrace = errorStackTrace;
            _errorType = errorType;
        }

        public string ErrorStackTrace
        {
            get { return _errorStackTrace; }
        }

        public string ErrorType
        {
            get { return _errorType; }
        }
    }
}
