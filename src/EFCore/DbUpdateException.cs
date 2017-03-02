// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     An exception that is thrown when an error is encountered while saving to the database.
    /// </summary>
    public class DbUpdateException : Exception
    {
        private readonly LazyRef<IReadOnlyList<EntityEntry>> _entries;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbUpdateException" /> class.
        /// </summary>
        /// <param name="message"> The error message that explains the reason for the exception. </param>
        /// <param name="innerException"> The exception that is the cause of the current exception. </param>
        public DbUpdateException([NotNull] string message, [CanBeNull] Exception innerException)
            : base(message, innerException)
        {
            _entries = new LazyRef<IReadOnlyList<EntityEntry>>(() => new List<EntityEntry>());
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbUpdateException" /> class.
        /// </summary>
        /// <param name="message"> The error message that explains the reason for the exception. </param>
        /// <param name="entries"> The entries that were involved in the error. </param>
        public DbUpdateException(
            [NotNull] string message,
            [NotNull] IReadOnlyList<IUpdateEntry> entries)
            : this(message, null, entries)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbUpdateException" /> class.
        /// </summary>
        /// <param name="message"> The error message that explains the reason for the exception. </param>
        /// <param name="entries"> The entries that were involved in the error. </param>
        /// <param name="innerException"> The exception that is the cause of the current exception. </param>
        public DbUpdateException(
            [NotNull] string message,
            [CanBeNull] Exception innerException,
            [NotNull] IReadOnlyList<IUpdateEntry> entries)
            : base(message, innerException)
        {
            Check.NotEmpty(entries, nameof(entries));

            _entries = new LazyRef<IReadOnlyList<EntityEntry>>(() => entries.Select(e => e.ToEntityEntry()).ToList());
        }

        /// <summary>
        ///     Gets the entries that were involved in the error. Typically this is a single entry, but in some cases it
        ///     may be zero or multiple entries.
        /// </summary>
        public virtual IReadOnlyList<EntityEntry> Entries => _entries.Value;
    }
}
