// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Relational.Utilities;

namespace Microsoft.Data.Relational.Update
{
    public class DbUpdateException : InvalidOperationException
    {
        public DbUpdateException()
        {
        }

        public DbUpdateException([NotNull] string message)
            : base(message)
        {
            Check.NotEmpty(message, "message");
        }

        public DbUpdateException([NotNull] string message, [CanBeNull] Exception innerException)
            : base(message, innerException)
        {
            Check.NotEmpty(message, "message");
        }
    }
}
