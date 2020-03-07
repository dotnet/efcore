// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    /// <summary>
    ///     A <see cref="MigrationOperation" /> for operations on tables.
    ///     See also <see cref="CreateTableOperation" /> and <see cref="AlterTableOperation" />.
    /// </summary>
    public class TableOperation : MigrationOperation
    {
        /// <summary>
        ///     Comment for this table
        /// </summary>
        public virtual string Comment { get; [param: CanBeNull] set; }
    }
}
