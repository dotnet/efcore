// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Commands
{
    public class OperationErrorException : Exception
    {
        public const string OperationException = "Microsoft.EntityFrameworkCore.Design.OperationException";

        private readonly string _stackTrace;

        public OperationErrorException([NotNull] string type, [NotNull] string stackTrace, [NotNull] string message)
            : base(message)
        {
            _stackTrace = stackTrace;
            Type = type;
        }

        public virtual string Type { get; }

        public override string ToString()
            => _stackTrace;

        public static OperationErrorException CreateOperationException([NotNull] string message)
            => new OperationErrorException(
                OperationException,
                OperationException + ": " + message + Environment.NewLine + Environment.StackTrace,
                message);
    }
}
