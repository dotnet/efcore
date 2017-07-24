// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    /// <summary>
    ///     A <see cref="MigrationOperation" /> to add a new foreign key.
    /// </summary>
    public class AddForeignKeyOperation : MigrationOperation
    {
        /// <summary>
        ///     The name of the foreign key constraint.
        /// </summary>
        public virtual string Name { get; [param: NotNull] set; }

        /// <summary>
        ///     The schema that contains the table, or <c>null</c> if the default schema should be used.
        /// </summary>
        public virtual string Schema { get; [param: CanBeNull] set; }

        /// <summary>
        ///     The table to which the foreign key should be added.
        /// </summary>
        public virtual string Table { get; [param: NotNull] set; }

        /// <summary>
        ///     The ordered-list of column names for the columns that make up the foreign key.
        /// </summary>
        public virtual string[] Columns { get; [param: NotNull] set; }

        /// <summary>
        ///     The schema that contains the table to which this foreign key is constrained,
        ///     or <c>null</c> if the default schema should be used.
        /// </summary>
        public virtual string PrincipalSchema { get; [param: CanBeNull] set; }

        /// <summary>
        ///     The table to which the foreign key is constrained.
        /// </summary>
        public virtual string PrincipalTable { get; [param: NotNull] set; }

        /// <summary>
        ///     The ordered-list of column names for the columns to which the columns that make up this foreign key are constrained.
        /// </summary>
        public virtual string[] PrincipalColumns { get; [param: NotNull] set; }

        /// <summary>
        ///     The <see cref="ReferentialAction" /> to use for updates.
        /// </summary>
        public virtual ReferentialAction OnUpdate { get; set; }

        /// <summary>
        ///     The <see cref="ReferentialAction" /> to use for deletes.
        /// </summary>
        public virtual ReferentialAction OnDelete { get; set; }
    }
}
