// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    /// <summary>
    ///     A XG Server-specific <see cref="MigrationOperation" /> to drop a database.
    /// </summary>
    public class XGDropDatabaseOperation : MigrationOperation
    {
        /// <summary>
        ///     The name of the database.
        /// </summary>
        public virtual string Name { get; [param: NotNull] set; }
    }
}
