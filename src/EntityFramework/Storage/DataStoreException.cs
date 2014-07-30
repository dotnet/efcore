// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using System;

namespace Microsoft.Data.Entity.Storage
{
    public class DataStoreException : Exception
    {
        private readonly DbContext _context;

        public DataStoreException()
        { }

        public DataStoreException([NotNull] string message, [NotNull] DbContext context)
            : this(message, context, null)
        { }

        public DataStoreException([NotNull] string message, [NotNull] DbContext context, [CanBeNull] Exception innerException)
            : base(message, innerException)
        {
            Check.NotNull(context, "contextType");

            _context = context;
        }

        public virtual DbContext Context
        {
            get { return _context; }
        }

        public static bool ContainsDataStoreException([CanBeNull] Exception ex)
        {
            while (ex != null)
            {
                if (ex is DataStoreException)
                {
                    return true;
                }
                ex = ex.InnerException;
            }

            return false;
        }
    }
}
