// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    /// <summary>
    ///     A <see cref="MigrationOperation" /> for re-starting an existing sequence.
    /// </summary>
    [DebuggerDisplay("ALTER SEQUENCE {Name} RESTART")]
    public class RestartSequenceOperation : MigrationOperation
    {
        /// <summary>
        ///     The name of the sequence.
        /// </summary>
        public virtual string Name { get; [param: NotNull] set; }

        /// <summary>
        ///     The schema that contains the sequence, or <c>null</c> if the default schema should be used.
        /// </summary>
        public virtual string Schema { get; [param: CanBeNull] set; }

        /// <summary>
        ///     The value at which the sequence should re-start, defaulting to 1.
        /// </summary>
        public virtual long StartValue { get; set; } = 1L;
    }
}
