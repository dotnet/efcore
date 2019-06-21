// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    /// <summary>
    ///     A <see cref="MigrationOperation" /> to alter an existing database.
    /// </summary>
    public class AlterDatabaseOperation : MigrationOperation, IAlterMigrationOperation
    {
        /// <summary>
        ///     An operation representing the database as it was before being altered.
        /// </summary>
        public virtual Annotatable OldDatabase { get; } = new Annotatable();

        /// <inheritdoc />
        IMutableAnnotatable IAlterMigrationOperation.OldAnnotations => OldDatabase;
    }
}
