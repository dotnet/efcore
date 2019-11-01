// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    /// <summary>
    ///     A <see cref="MigrationOperation" /> for renaming an existing sequence.
    /// </summary>
    [DebuggerDisplay("ALTER SEQUENCE {Name} RENAME TO {NewName}")]
    public class RenameSequenceOperation : MigrationOperation
    {
        /// <summary>
        ///     The old name of the sequence.
        /// </summary>
        public virtual string Name { get; [param: NotNull] set; }

        /// <summary>
        ///     The schema that contains the sequence, or <c>null</c> if the default schema should be used.
        /// </summary>
        public virtual string Schema { get; [param: CanBeNull] set; }

        /// <summary>
        ///     The new sequence name or <c>null</c> if only the schema has changed.
        /// </summary>
        public virtual string NewName { get; [param: CanBeNull] set; }

        /// <summary>
        ///     The new schema name or <c>null</c> if only the name has changed.
        /// </summary>
        public virtual string NewSchema { get; [param: CanBeNull] set; }
    }
}
