// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Commands
{
    public class OperationException : Exception
    {
        private readonly string _stackTrace;

        public OperationException([NotNull] string type, [NotNull] string stackTrace, [NotNull] string message)
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
