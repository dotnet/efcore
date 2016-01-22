// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Design
{
    public class OperationException : Exception
    {
        public OperationException([NotNull] string message)
            : base(message)
        {
        }

        public OperationException([NotNull] string message, [CanBeNull] Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
