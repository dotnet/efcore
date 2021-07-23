// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Metadata
{
    /// <summary>
    ///     A simple model for a database table used when reverse engineering an existing database.
    /// </summary>
    public class DatabaseTable : Annotatable
    {
        /// <summary>
        ///     The database that contains the table.
        /// </summary>
        public virtual DatabaseModel? Database { get; set; }

        /// <summary>
        ///     The table name.
        /// </summary>
        public virtual string Name { get; set; } = null!;

        /// <summary>
        ///     The table schema, or <see langword="null" /> to use the default schema.
        /// </summary>
        public virtual string? Schema { get; set; }

        /// <summary>
        ///     The table comment, or <see langword="null" /> if none is set.
        /// </summary>
        public virtual string? Comment { get; set; }

        /// <summary>
        ///     The primary key of the table.
        /// </summary>
        public virtual DatabasePrimaryKey? PrimaryKey { get; set; }

        /// <summary>
        ///     The ordered list of columns in the table.
        /// </summary>
        public virtual IList<DatabaseColumn> Columns { get; } = new List<DatabaseColumn>();

        /// <summary>
        ///     The list of unique constraints defined on the table.
        /// </summary>
        public virtual IList<DatabaseUniqueConstraint> UniqueConstraints { get; } = new List<DatabaseUniqueConstraint>();

        /// <summary>
        ///     The list of indexes defined on the table.
        /// </summary>
        public virtual IList<DatabaseIndex> Indexes { get; } = new List<DatabaseIndex>();

        /// <summary>
        ///     The list of foreign key constraints defined on the table.
        /// </summary>
        public virtual IList<DatabaseForeignKey> ForeignKeys { get; } = new List<DatabaseForeignKey>();

        /// <inheritdoc />
        public override string ToString()
        {
            var name = Name ?? "<UNKNOWN>";
            return Schema == null ? name : $"{Schema}.{name}";
        }
    }
}
