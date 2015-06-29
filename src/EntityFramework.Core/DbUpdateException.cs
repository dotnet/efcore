// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public class DbUpdateException : Exception
    {
        public DbUpdateException()
        {
            Entries = new List<InternalEntityEntry>();
        }

        public DbUpdateException([NotNull] string message)
            : this(message, (Exception)null)
        {
        }

        public DbUpdateException([NotNull] string message, [CanBeNull] Exception innerException)
            : base(message, innerException)
        {
            Entries = new List<InternalEntityEntry>();
        }

        public DbUpdateException(
            [NotNull] string message,
            [NotNull] IReadOnlyList<InternalEntityEntry> entries)
            : this(message, null, entries)
        {
        }

        public DbUpdateException(
            [NotNull] string message,
            [CanBeNull] Exception innerException,
            [NotNull] IReadOnlyList<InternalEntityEntry> entries)
            : base(message, innerException)
        {
            Check.NotEmpty(entries, nameof(entries));

            Entries = entries;
        }

        public virtual IReadOnlyList<InternalEntityEntry> Entries { get; }
    }
}
