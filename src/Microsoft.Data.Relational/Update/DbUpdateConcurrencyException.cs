// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Relational.Utilities;

namespace Microsoft.Data.Relational.Update
{
    public class DbUpdateConcurrencyException : DbUpdateException
    {
        public DbUpdateConcurrencyException()
        {
        }

        public DbUpdateConcurrencyException([NotNull] string message)
            : base(message)
        {
            Check.NotEmpty(message, "message");
        }

        public DbUpdateConcurrencyException([NotNull] string message, [CanBeNull] Exception innerException)
            : base(message, innerException)
        {
            Check.NotEmpty(message, "message");
        }
    }
}
