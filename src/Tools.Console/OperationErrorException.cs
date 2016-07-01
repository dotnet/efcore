// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Tools
{
    /// <summary>
    ///     Represents an exception whose stack trace should, by default, not be reported by the commands.
    /// </summary>
    public class OperationErrorException : Exception
    {
        public OperationErrorException([NotNull] string message)
            : base(message)
        {
        }

        public OperationErrorException([NotNull] string message, [CanBeNull] Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
