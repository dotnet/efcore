// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    /// <summary>
    ///     A <see cref="MigrationOperation" /> to alter an existing database.
    /// </summary>
    [DebuggerDisplay("ALTER DATABASE {Name}")]
    public class AlterDatabaseOperation : DatabaseOperation, IAlterMigrationOperation
    {
        /// <summary>
        ///     An operation representing the database as it was before being altered.
        /// </summary>
        public virtual DatabaseOperation OldDatabase { get; } = new CreateDatabaseOperation();

        /// <inheritdoc />
        IMutableAnnotatable IAlterMigrationOperation.OldAnnotations
            => OldDatabase;

        private sealed class CreateDatabaseOperation : DatabaseOperation
        {
        }
    }
}
