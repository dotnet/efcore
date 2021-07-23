// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    /// <summary>
    ///     A <see cref="MigrationOperation" /> for dropping a sequence.
    /// </summary>
    [DebuggerDisplay("DROP SEQUENCE {Name}")]
    public class DropSequenceOperation : MigrationOperation
    {
        /// <summary>
        ///     The name of the sequence.
        /// </summary>
        public virtual string Name { get; set; } = null!;

        /// <summary>
        ///     The schema that contains the sequence, or <see langword="null" /> if the default schema should be used.
        /// </summary>
        public virtual string? Schema { get; set; }
    }
}
