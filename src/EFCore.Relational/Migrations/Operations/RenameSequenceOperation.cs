// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    public class RenameSequenceOperation : MigrationOperation
    {
        public virtual string Name { get; [param: NotNull] set; }
        public virtual string Schema { get; [param: CanBeNull] set; }

        /// <summary>
        ///     The new schema name or null if unchanged.
        /// </summary>
        public virtual string NewName { get; [param: CanBeNull] set; }

        /// <summary>
        ///     The new sequence name or null if unchanged.
        /// </summary>
        public virtual string NewSchema { get; [param: CanBeNull] set; }
    }
}
