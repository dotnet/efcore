// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using JetBrains.Annotations;

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
        public virtual string Schema { get; [param: CanBeNull] set; }

        /// <summary>
        ///     The name of the sequence.
        /// </summary>
        public virtual string Name { get; [param: NotNull] set; }

        /// <summary>
        ///     The CLR <see cref="Type" /> of values returned from the sequence.
        /// </summary>
        public virtual Type ClrType { get; [param: NotNull] set; }

        /// <summary>
        ///     The value at which the sequence will start counting, defaulting to 1.
        /// </summary>
        public virtual long StartValue { get; set; } = 1L;
    }
}
