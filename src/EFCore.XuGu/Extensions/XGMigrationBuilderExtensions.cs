// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Migrations
{
    /// <summary>
    ///     MySQL specific extension methods for <see cref="MigrationBuilder" />.
    /// </summary>
    public static class XGMigrationBuilderExtensions
    {
        /// <summary>
        ///     <para>
        ///         Returns true if the database provider currently in use is the MySQL provider.
        ///     </para>
        /// </summary>
        /// <param name="migrationBuilder"> The migrationBuilder from the parameters on <see cref="Migration.Up(MigrationBuilder)" /> or <see cref="Migration.Down(MigrationBuilder)" />. </param>
        /// <returns> True if MySQL is being used; false otherwise. </returns>
        public static bool IsXG([NotNull] this MigrationBuilder migrationBuilder)
            => string.Equals(migrationBuilder.ActiveProvider,
                typeof(XGOptionsExtension).GetTypeInfo().Assembly.GetName().Name,
                StringComparison.Ordinal);

        /// <summary>
        ///     Builds an <see cref="XGDropPrimaryKeyAndRecreateForeignKeysOperation" /> to drop an existing primary key and optionally
        ///     recreate all foreign keys of the table.
        /// </summary>
        /// <param name="migrationBuilder"> The migrationBuilder from the parameters on <see cref="Migration.Up(MigrationBuilder)" /> or <see cref="Migration.Down(MigrationBuilder)" />. </param>
        /// <param name="name"> The name of the primary key constraint to drop. </param>
        /// <param name="table"> The table that contains the key. </param>
        /// <param name="schema"> The schema that contains the table, or <see langword="null" /> to use the default schema. </param>
        /// <param name="recreateForeignKeys"> The sole reasion to use this extension method. Set this parameter to `true`, to force all
        /// foreign keys of the table be be dropped before the primary key is dropped, and created again afterwards.</param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public static OperationBuilder<XGDropPrimaryKeyAndRecreateForeignKeysOperation> DropPrimaryKey(
            [NotNull] this MigrationBuilder migrationBuilder,
            [NotNull] string name,
            [NotNull] string table,
            [CanBeNull] string schema = null,
            bool recreateForeignKeys = false)
        {
            Check.NotNull(migrationBuilder, nameof(migrationBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NotEmpty(table, nameof(table));

            var operation = new XGDropPrimaryKeyAndRecreateForeignKeysOperation
            {
                Schema = schema,
                Table = table,
                Name = name,
                RecreateForeignKeys = recreateForeignKeys,
            };
            migrationBuilder.Operations.Add(operation);

            return new OperationBuilder<XGDropPrimaryKeyAndRecreateForeignKeysOperation>(operation);
        }

        /// <summary>
        ///     Builds an <see cref="XGDropPrimaryKeyAndRecreateForeignKeysOperation" /> to drop an existing unique constraint and optionally
        ///     recreate all foreign keys of the table.
        /// </summary>
        /// <param name="migrationBuilder"> The migrationBuilder from the parameters on <see cref="Migration.Up(MigrationBuilder)" /> or <see cref="Migration.Down(MigrationBuilder)" />. </param>
        /// <param name="name"> The name of the constraint to drop. </param>
        /// <param name="table"> The table that contains the constraint. </param>
        /// <param name="schema"> The schema that contains the table, or <see langword="null" /> to use the default schema. </param>
        /// <param name="recreateForeignKeys"> The sole reasion to use this extension method. Set this parameter to `true`, to force all
        /// foreign keys of the table be be dropped before the primary key is dropped, and created again afterwards.</param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public static OperationBuilder<XGDropUniqueConstraintAndRecreateForeignKeysOperation> DropUniqueConstraint(
            [NotNull] this MigrationBuilder migrationBuilder,
            [NotNull] string name,
            [NotNull] string table,
            [CanBeNull] string schema = null,
            bool recreateForeignKeys = false)
        {
            Check.NotNull(migrationBuilder, nameof(migrationBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NotEmpty(table, nameof(table));

            var operation = new XGDropUniqueConstraintAndRecreateForeignKeysOperation
            {
                Schema = schema,
                Table = table,
                Name = name,
                RecreateForeignKeys = recreateForeignKeys,
            };
            migrationBuilder.Operations.Add(operation);

            return new OperationBuilder<XGDropUniqueConstraintAndRecreateForeignKeysOperation>(operation);
        }
    }
}
