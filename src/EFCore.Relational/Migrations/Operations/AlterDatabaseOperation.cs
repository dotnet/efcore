// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
