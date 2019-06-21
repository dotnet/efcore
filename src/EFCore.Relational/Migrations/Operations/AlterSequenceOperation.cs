// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    /// <summary>
    ///     A <see cref="MigrationOperation" /> to alter an existing sequence.
    /// </summary>
    public class AlterSequenceOperation : SequenceOperation, IAlterMigrationOperation
    {
        /// <summary>
        ///     The schema that contains the sequence, or <c>null</c> if the default schema should be used.
        /// </summary>
        public virtual string Schema { get; [param: CanBeNull] set; }

        /// <summary>
        ///     The name of the sequence.
        /// </summary>
        public virtual string Name { get; [param: NotNull] set; }

        /// <summary>
        ///     An operation representing the sequence as it was before being altered.
        /// </summary>
        public virtual SequenceOperation OldSequence { get; [param: NotNull] set; } = new SequenceOperation();

        /// <inheritdoc />
        IMutableAnnotatable IAlterMigrationOperation.OldAnnotations => OldSequence;
    }
}
