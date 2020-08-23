// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Metadata
{
    /// <summary>
    ///     A simple model for a database foreign key constraint used when reverse engineering an existing database.
    /// </summary>
    public class DatabaseForeignKey : Annotatable
    {
        /// <summary>
        ///     The table that contains the foreign key constraint.
        /// </summary>
        public virtual DatabaseTable Table { get; [param: NotNull] set; }

        /// <summary>
        ///     The table to which the columns are constrained.
        /// </summary>
        public virtual DatabaseTable PrincipalTable { get; [param: NotNull] set; }

        /// <summary>
        ///     The ordered list of columns that are constrained.
        /// </summary>
        public virtual IList<DatabaseColumn> Columns { get; } = new List<DatabaseColumn>();

        /// <summary>
        ///     The ordered list of columns in the <see cref="PrincipalTable" /> to which the <see cref="Columns" />
        ///     of the foreign key are constrained.
        /// </summary>
        public virtual IList<DatabaseColumn> PrincipalColumns { get; } = new List<DatabaseColumn>();

        /// <summary>
        ///     The foreign key constraint name.
        /// </summary>
        public virtual string Name { get; [param: CanBeNull] set; }

        /// <summary>
        ///     The action performed by the database when a row constrained by this foreign key
        ///     is deleted, or <see langword="null" /> if there is no action defined.
        /// </summary>
        public virtual ReferentialAction? OnDelete { get; set; }

        /// <inheritdoc />
        public override string ToString()
            => Name ?? "<UNKNOWN>";
    }
}
