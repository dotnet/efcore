// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.FunctionalTests.TestUtilities
{
    public class OperationErrorException : Exception
    {
        private readonly string _stackTrace;

        public OperationErrorException(string type, string stackTrace, string message)
            : base(message)
        {
            _stackTrace = stackTrace;
            Type = type;
        }

        public virtual string Type { get; }

        public override string ToString()
            => _stackTrace;
    }
}
