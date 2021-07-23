// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    /// <summary>
    ///     A <see cref="MigrationOperation" /> for creating a new sequence.
    /// </summary>
    [DebuggerDisplay("CREATE SEQUENCE {Name}")]
    public class CreateSequenceOperation : SequenceOperation
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
        ///     The CLR <see cref="Type" /> of values returned from the sequence.
        /// </summary>
        public virtual Type ClrType { get; set; } = null!;

        /// <summary>
        ///     The value at which the sequence will start counting, defaulting to 1.
        /// </summary>
        public virtual long StartValue { get; set; } = 1L;
    }
}
