// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    /// <summary>
    ///     A XG Server-specific <see cref="MigrationOperation" /> to create a database.
    /// </summary>
    public class XGCreateDatabaseOperation : DatabaseOperation
    {
        /// <summary>
        ///     The name of the database.
        /// </summary>
        public virtual string Name { get; [param: NotNull] set; }

        /// <summary>
        ///     The default character set of the database.
        /// </summary>
        public virtual string CharSet { get; [param: CanBeNull] set; }
        public virtual string TimeZone { get; [param: CanBeNull] set; }
    }
}
