// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Update;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public class DbUpdateException : Exception
    {
        private readonly LazyRef<IReadOnlyList<EntityEntry>> _entries;

        public DbUpdateException([NotNull] string message, [CanBeNull] Exception innerException)
            : base(message, innerException)
        {
            _entries = new LazyRef<IReadOnlyList<EntityEntry>>(() => new List<EntityEntry>());
        }

        public DbUpdateException(
            [NotNull] string message,
            [NotNull] IReadOnlyList<IUpdateEntry> entries)
            : this(message, null, entries)
        {
        }

        public DbUpdateException(
            [NotNull] string message,
            [CanBeNull] Exception innerException,
            [NotNull] IReadOnlyList<IUpdateEntry> entries)
            : base(message, innerException)
        {
            Check.NotEmpty(entries, nameof(entries));

            _entries = new LazyRef<IReadOnlyList<EntityEntry>>(() => entries.Select(e => e.ToEntityEntry()).ToList());
        }

        public virtual IReadOnlyList<EntityEntry> Entries => _entries.Value;
    }
}
