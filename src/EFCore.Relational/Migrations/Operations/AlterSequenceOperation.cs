// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    /// <summary>
    ///     A <see cref="MigrationOperation" /> to alter an existing sequence.
    /// </summary>
    [DebuggerDisplay("ALTER SEQUENCE {Name}")]
    public class AlterSequenceOperation : SequenceOperation, IAlterMigrationOperation
    {
        /// <summary>
        ///     The schema that contains the sequence, or <see langword="null" /> if the default schema should be used.
        /// </summary>
        public virtual string? Schema { get; set; }

        /// <summary>
        ///     The name of the sequence.
        /// </summary>
        public virtual string Name { get; set; } = null!;

        /// <summary>
        ///     An operation representing the sequence as it was before being altered.
        /// </summary>
        public virtual SequenceOperation OldSequence { get; set; } = new CreateSequenceOperation();

        /// <inheritdoc />
        IMutableAnnotatable IAlterMigrationOperation.OldAnnotations
            => OldSequence;
    }
}
