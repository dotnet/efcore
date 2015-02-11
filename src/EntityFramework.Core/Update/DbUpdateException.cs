// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Update
{
    public class DbUpdateException : DataStoreException
    {
        public DbUpdateException()
        {
            Entries = new List<InternalEntityEntry>();
        }

        public DbUpdateException([NotNull] string message, [NotNull] DbContext context)
            : base(message, context)
        {
            Check.NotEmpty(message, "message");
            Check.NotNull(context, "context");

            Entries = new List<InternalEntityEntry>();
        }

        public DbUpdateException([NotNull] string message, [NotNull] DbContext context, [CanBeNull] Exception innerException)
            : base(message, context, innerException)
        {
            Check.NotEmpty(message, "message");
            Check.NotNull(context, "context");

            Entries = new List<InternalEntityEntry>();
        }

        public DbUpdateException([NotNull] string message, [NotNull] DbContext context, [NotNull] IReadOnlyList<InternalEntityEntry> entries)
            : base(message, context)
        {
            Check.NotEmpty(message, "message");
            Check.NotNull(context, "context");
            Check.NotEmpty(entries, "entries");

            Entries = entries;
        }

        public DbUpdateException([NotNull] string message, [NotNull] DbContext context, [CanBeNull] Exception innerException, [NotNull] IReadOnlyList<InternalEntityEntry> entries)
            : base(message, context, innerException)
        {
            Check.NotEmpty(message, "message");
            Check.NotNull(context, "context");
            Check.NotEmpty(entries, "entries");

            Entries = entries;
        }

        public virtual IReadOnlyList<InternalEntityEntry> Entries { get; }
    }
}
