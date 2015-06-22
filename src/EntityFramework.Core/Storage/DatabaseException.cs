// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Storage
{
    public class DatabaseException : Exception
    {
        public DatabaseException()
        {
        }

        public DatabaseException([NotNull] string message, [NotNull] DbContext context)
            : this(message, context, null)
        {
        }

        public DatabaseException([NotNull] string message, [NotNull] DbContext context, [CanBeNull] Exception innerException)
            : base(message, innerException)
        {
            Check.NotNull(context, nameof(context));

            Context = context;
        }

        public virtual DbContext Context { get; }

        public static bool ContainsDatabaseException([CanBeNull] Exception ex)
        {
            while (ex != null)
            {
                if (ex is DatabaseException)
                {
                    return true;
                }
                ex = ex.InnerException;
            }

            return false;
        }
    }
}
