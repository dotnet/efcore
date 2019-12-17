// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Scaffolding.Metadata
{
    /// <summary>
    ///     A simple model for a database table used when reverse engineering an existing database.
    /// </summary>
    public class DatabaseTable : Annotatable
    {
        public DatabaseTable([NotNull] DatabaseModel database, [NotNull] string name)
        {
            Database = database;
            Name = name;
            Columns = new List<DatabaseColumn>();
            UniqueConstraints = new List<DatabaseUniqueConstraint>();
            Indexes = new List<DatabaseIndex>();
            ForeignKeys = new List<DatabaseForeignKey>();
        }

        /// <summary>
        ///     The database that contains the table.
        /// </summary>
        public virtual DatabaseModel Database { get; [param: NotNull] set; }

        /// <summary>
        ///     The table name.
        /// </summary>
        public virtual string Name { get; [param: NotNull] set; }

        /// <summary>
        ///     The table schema, or <c>null</c> to use the default schema.
        /// </summary>
        public virtual string? Schema { get; [param: CanBeNull] set; }

        /// <summary>
        ///     The table comment, or <c>null</c> if none is set.
        /// </summary>
        public virtual string? Comment { get; [param: CanBeNull] set; }

        /// <summary>
        ///     The primary key of the table.
        /// </summary>
        public virtual DatabasePrimaryKey? PrimaryKey { get; [param: CanBeNull] set; }

        /// <summary>
        ///     The ordered list of columns in the table.
        /// </summary>
        public virtual IList<DatabaseColumn> Columns { get; }

        /// <summary>
        ///     The list of unique constraints defined on the table.
        /// </summary>
        public virtual IList<DatabaseUniqueConstraint> UniqueConstraints { get; }

        /// <summary>
        ///     The list of indexes defined on the table.
        /// </summary>
        public virtual IList<DatabaseIndex> Indexes { get; }

        /// <summary>
        ///     The list of foreign key constraints defined on the table.
        /// </summary>
        public virtual IList<DatabaseForeignKey> ForeignKeys { get; }

        public override string ToString() => Schema == null ? Name : $"{Schema}.{Name}";
    }
}
