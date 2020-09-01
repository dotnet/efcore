// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    /// <summary>
    ///     A builder providing a fluentish API for building <see cref="MigrationOperation" />s.
    /// </summary>
    public class MigrationBuilder
    {
        /// <summary>
        ///     Creates a new instance of the builder.
        /// </summary>
        /// <param name="activeProvider"> The name of the database provider being used. </param>
        public MigrationBuilder([CanBeNull] string activeProvider)
        {
            ActiveProvider = activeProvider;
        }

        /// <summary>
        ///     The name of the database provider being used.
        /// </summary>
        public virtual string ActiveProvider { get; }

        /// <summary>
        ///     The list of <see cref="MigrationOperation" />s being built.
        /// </summary>
        public virtual List<MigrationOperation> Operations { get; } = new List<MigrationOperation>();

        /// <summary>
        ///     Builds an <see cref="AddColumnOperation" /> to add a new column to a table.
        /// </summary>
        /// <typeparam name="T"> The CLR type that the column is mapped to. </typeparam>
        /// <param name="name"> The column name. </param>
        /// <param name="table"> The name of the table that contains the column. </param>
        /// <param name="type"> The store/database type of the column. </param>
        /// <param name="unicode">
        ///     Indicates whether or not the column can contain Unicode data, or <see langword="null" /> if not specified or not applicable.
        /// </param>
        /// <param name="maxLength">
        ///     The maximum length of data that can be stored in the column, or <see langword="null" /> if not specified or not applicable.
        /// </param>
        /// <param name="rowVersion">
        ///     Indicates whether or not the column acts as an automatic concurrency token, such as a rowversion/timestamp column
        ///     in SQL Server.
        /// </param>
        /// <param name="schema"> The schema that contains the table, or <see langword="null" /> if the default schema should be used. </param>
        /// <param name="nullable"> Indicates whether or not the column can store <see langword="null" /> values. </param>
        /// <param name="defaultValue"> The default value for the column. </param>
        /// <param name="defaultValueSql"> The SQL expression to use for the column's default constraint. </param>
        /// <param name="computedColumnSql"> The SQL expression to use to compute the column value. </param>
        /// <param name="fixedLength"> Indicates whether or not the column is constrained to fixed-length data. </param>
        /// <param name="comment"> A comment to associate with the column. </param>
        /// <param name="collation"> A collation to apply to the column. </param>
        /// <param name="precision">
        ///     The maximum number of digits that is allowed in this column, or <see langword="null" /> if not specified or not applicable.
        /// </param>
        /// <param name="scale">
        ///     The maximum number of decimal places that is allowed in this column, or <see langword="null" /> if not specified or not applicable.
        /// </param>
        /// <param name="stored"> Whether the value of the computed column is stored in the database or not. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual OperationBuilder<AddColumnOperation> AddColumn<T>(
            [NotNull] string name,
            [NotNull] string table,
            [CanBeNull] string type = null,
            bool? unicode = null,
            int? maxLength = null,
            bool rowVersion = false,
            [CanBeNull] string schema = null,
            bool nullable = false,
            [CanBeNull] object defaultValue = null,
            [CanBeNull] string defaultValueSql = null,
            [CanBeNull] string computedColumnSql = null,
            bool? fixedLength = null,
            [CanBeNull] string comment = null,
            [CanBeNull] string collation = null,
            int? precision = null,
            int? scale = null,
            bool? stored = null)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotEmpty(table, nameof(table));

            var operation = new AddColumnOperation
            {
                Schema = schema,
                Table = table,
                Name = name,
                ClrType = typeof(T),
                ColumnType = type,
                IsUnicode = unicode,
                MaxLength = maxLength,
                IsRowVersion = rowVersion,
                IsNullable = nullable,
                DefaultValue = defaultValue,
                DefaultValueSql = defaultValueSql,
                ComputedColumnSql = computedColumnSql,
                IsFixedLength = fixedLength,
                Comment = comment,
                Collation = collation,
                Precision = precision,
                Scale = scale,
                IsStored = stored
            };
            Operations.Add(operation);

            return new OperationBuilder<AddColumnOperation>(operation);
        }

        /// <summary>
        ///     Builds an <see cref="AddForeignKeyOperation" /> to add a new foreign key to a table.
        /// </summary>
        /// <param name="name"> The foreign key constraint name. </param>
        /// <param name="table"> The table that contains the foreign key. </param>
        /// <param name="column"> The column that is constrained. </param>
        /// <param name="principalTable"> The table to which the foreign key is constrained. </param>
        /// <param name="schema"> The schema that contains the table, or <see langword="null" /> if the default schema should be used. </param>
        /// <param name="principalSchema">
        ///     The schema that contains principal table, or <see langword="null" /> if the default schema should be used.
        /// </param>
        /// <param name="principalColumn">
        ///     The column to which the foreign key column is constrained, or <see langword="null" /> to constrain to the primary key
        ///     column.
        /// </param>
        /// <param name="onUpdate"> The action to take on updates. </param>
        /// <param name="onDelete"> The action to take on deletes. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual OperationBuilder<AddForeignKeyOperation> AddForeignKey(
            [NotNull] string name,
            [NotNull] string table,
            [NotNull] string column,
            [NotNull] string principalTable,
            [CanBeNull] string schema = null,
            [CanBeNull] string principalSchema = null,
            [CanBeNull] string principalColumn = null,
            ReferentialAction onUpdate = ReferentialAction.NoAction,
            ReferentialAction onDelete = ReferentialAction.NoAction)
            => AddForeignKey(
                name,
                table,
                new[] { Check.NotEmpty(column, nameof(column)) },
                principalTable,
                schema,
                principalSchema,
                new[] { principalColumn },
                onUpdate,
                onDelete);

        /// <summary>
        ///     Builds an <see cref="AddForeignKeyOperation" /> to add a new composite (multi-column) foreign key to a table.
        /// </summary>
        /// <param name="name"> The foreign key constraint name. </param>
        /// <param name="table"> The table that contains the foreign key. </param>
        /// <param name="columns"> The ordered list of columns that are constrained. </param>
        /// <param name="principalTable"> The table to which the foreign key is constrained. </param>
        /// <param name="schema"> The schema that contains the table, or <see langword="null" /> if the default schema should be used. </param>
        /// <param name="principalSchema">
        ///     The schema that contains principal table, or <see langword="null" /> if the default schema should be used.
        /// </param>
        /// <param name="principalColumns">
        ///     The columns to which the foreign key columns are constrained, or <see langword="null" /> to constrain to the primary key
        ///     column.
        /// </param>
        /// <param name="onUpdate"> The action to take on updates. </param>
        /// <param name="onDelete"> The action to take on deletes. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual OperationBuilder<AddForeignKeyOperation> AddForeignKey(
            [NotNull] string name,
            [NotNull] string table,
            [NotNull] string[] columns,
            [NotNull] string principalTable,
            [CanBeNull] string schema = null,
            [CanBeNull] string principalSchema = null,
            [CanBeNull] string[] principalColumns = null,
            ReferentialAction onUpdate = ReferentialAction.NoAction,
            ReferentialAction onDelete = ReferentialAction.NoAction)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotEmpty(table, nameof(table));
            Check.NotEmpty(columns, nameof(columns));
            Check.NotEmpty(principalTable, nameof(principalTable));

            var operation = new AddForeignKeyOperation
            {
                Schema = schema,
                Table = table,
                Name = name,
                Columns = columns,
                PrincipalSchema = principalSchema,
                PrincipalTable = principalTable,
                PrincipalColumns = principalColumns,
                OnUpdate = onUpdate,
                OnDelete = onDelete
            };
            Operations.Add(operation);

            return new OperationBuilder<AddForeignKeyOperation>(operation);
        }

        /// <summary>
        ///     Builds an <see cref="AddPrimaryKeyOperation" /> to add a new primary key to a table.
        /// </summary>
        /// <param name="name"> The primary key constraint name. </param>
        /// <param name="table"> The table that will contain the primary key. </param>
        /// <param name="column"> The column that constitutes the primary key. </param>
        /// <param name="schema"> The schema that contains the table, or <see langword="null" /> to use the default schema. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual OperationBuilder<AddPrimaryKeyOperation> AddPrimaryKey(
            [NotNull] string name,
            [NotNull] string table,
            [NotNull] string column,
            [CanBeNull] string schema = null)
            => AddPrimaryKey(
                name,
                table,
                new[] { Check.NotEmpty(column, nameof(column)) },
                schema);

        /// <summary>
        ///     Builds an <see cref="AddPrimaryKeyOperation" /> to add a new composite (multi-column) primary key to a table.
        /// </summary>
        /// <param name="name"> The primary key constraint name. </param>
        /// <param name="table"> The table that will contain the primary key. </param>
        /// <param name="columns"> The ordered list of columns that constitute the primary key. </param>
        /// <param name="schema"> The schema that contains the table, or <see langword="null" /> to use the default schema. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual OperationBuilder<AddPrimaryKeyOperation> AddPrimaryKey(
            [NotNull] string name,
            [NotNull] string table,
            [NotNull] string[] columns,
            [CanBeNull] string schema = null)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotEmpty(table, nameof(table));
            Check.NotEmpty(columns, nameof(columns));

            var operation = new AddPrimaryKeyOperation
            {
                Schema = schema,
                Table = table,
                Name = name,
                Columns = columns
            };
            Operations.Add(operation);

            return new OperationBuilder<AddPrimaryKeyOperation>(operation);
        }

        /// <summary>
        ///     Builds an <see cref="AddUniqueConstraintOperation" /> to add a new unique constraint to a table.
        /// </summary>
        /// <param name="name"> The constraint name. </param>
        /// <param name="table"> The table that will contain the constraint. </param>
        /// <param name="column"> The column that is constrained. </param>
        /// <param name="schema"> The schema that contains the table, or <see langword="null" /> to use the default schema. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual OperationBuilder<AddUniqueConstraintOperation> AddUniqueConstraint(
            [NotNull] string name,
            [NotNull] string table,
            [NotNull] string column,
            [CanBeNull] string schema = null)
            => AddUniqueConstraint(
                name,
                table,
                new[] { Check.NotEmpty(column, nameof(column)) },
                schema);

        /// <summary>
        ///     Builds an <see cref="AddUniqueConstraintOperation" /> to add a new composite (multi-column) unique constraint to a table.
        /// </summary>
        /// <param name="name"> The constraint name. </param>
        /// <param name="table"> The table that will contain the constraint. </param>
        /// <param name="columns"> The ordered list of columns that are constrained. </param>
        /// <param name="schema"> The schema that contains the table, or <see langword="null" /> to use the default schema. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual OperationBuilder<AddUniqueConstraintOperation> AddUniqueConstraint(
            [NotNull] string name,
            [NotNull] string table,
            [NotNull] string[] columns,
            [CanBeNull] string schema = null)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotEmpty(table, nameof(table));
            Check.NotEmpty(columns, nameof(columns));

            var operation = new AddUniqueConstraintOperation
            {
                Schema = schema,
                Table = table,
                Name = name,
                Columns = columns
            };
            Operations.Add(operation);

            return new OperationBuilder<AddUniqueConstraintOperation>(operation);
        }

        /// <summary>
        ///     Builds an <see cref="AlterColumnOperation" /> to alter an existing column.
        /// </summary>
        /// <typeparam name="T"> The CLR type that the column is mapped to. </typeparam>
        /// <param name="name"> The column name. </param>
        /// <param name="table"> The name of the table that contains the column. </param>
        /// <param name="type"> The store/database type of the column. </param>
        /// <param name="unicode">
        ///     Indicates whether or not the column can contain Unicode data, or <see langword="null" /> if not specified or not applicable.
        /// </param>
        /// <param name="maxLength">
        ///     The maximum length of data that can be stored in the column, or <see langword="null" /> if not specified or not applicable.
        /// </param>
        /// <param name="rowVersion">
        ///     Indicates whether or not the column acts as an automatic concurrency token, such as a rowversion/timestamp column
        ///     in SQL Server.
        /// </param>
        /// <param name="schema"> The schema that contains the table, or <see langword="null" /> if the default schema should be used. </param>
        /// <param name="nullable"> Indicates whether or not the column can store <see langword="null" /> values. </param>
        /// <param name="defaultValue"> The default value for the column. </param>
        /// <param name="defaultValueSql"> The SQL expression to use for the column's default constraint. </param>
        /// <param name="computedColumnSql"> The SQL expression to use to compute the column value. </param>
        /// <param name="oldClrType">
        ///     The CLR type that the column was previously mapped to. Can be <see langword="null" />, in which case previous value is considered
        ///     unknown.
        /// </param>
        /// <param name="oldType">
        ///     The previous store/database type of the column. Can be <see langword="null" />, in which case previous value is considered unknown.
        /// </param>
        /// <param name="oldUnicode">
        ///     Indicates whether or not the column could previously contain Unicode data, or <see langword="null" /> if not specified or not
        ///     applicable.
        /// </param>
        /// <param name="oldMaxLength">
        ///     The previous maximum length of data that can be stored in the column, or <see langword="null" /> if not specified or not applicable.
        /// </param>
        /// <param name="oldRowVersion">
        ///     Indicates whether or not the column previously acted as an automatic concurrency token, such as a rowversion/timestamp column
        ///     in SQL Server. Can be <see langword="null" />, in which case previous value is considered unknown.
        /// </param>
        /// <param name="oldNullable">
        ///     Indicates whether or not the column could previously store <see langword="null" /> values. Can be <see langword="null" />, in which
        ///     case previous value is
        ///     considered unknown.
        /// </param>
        /// <param name="oldDefaultValue">
        ///     The previous default value for the column. Can be <see langword="null" />, in which case previous value is considered unknown.
        /// </param>
        /// <param name="oldDefaultValueSql">
        ///     The previous SQL expression used for the column's default constraint. Can be <see langword="null" />, in which case previous value is
        ///     considered
        ///     unknown.
        /// </param>
        /// <param name="oldComputedColumnSql">
        ///     The previous SQL expression used to compute the column value. Can be <see langword="null" />, in which case previous value is
        ///     considered unknown.
        /// </param>
        /// <param name="fixedLength"> Indicates whether or not the column is constrained to fixed-length data. </param>
        /// <param name="oldFixedLength"> Indicates whether or not the column was previously constrained to fixed-length data. </param>
        /// <param name="comment"> A comment to associate with the column. </param>
        /// <param name="oldComment"> The previous comment to associate with the column. </param>
        /// <param name="collation"> A collation to apply to the column. </param>
        /// <param name="oldCollation"> The previous collation to apply to the column. </param>
        /// <param name="precision">
        ///     The maximum number of digits that is allowed in this column, or <see langword="null" /> if not specified or not applicable.
        /// </param>
        /// <param name="oldPrecision">
        ///     The previous maximum number of digits that is allowed in this column, or <see langword="null" /> if not specified or not applicable.
        /// </param>
        /// <param name="scale">
        ///     The maximum number of decimal places that is allowed in this column, or <see langword="null" /> if not specified or not applicable.
        /// </param>
        /// <param name="oldScale">
        ///     The previous maximum number of decimal places that is allowed in this column, or <see langword="null" /> if not specified or not
        ///     applicable.
        /// </param>
        /// <param name="stored"> Whether the value of the computed column is stored in the database or not. </param>
        /// <param name="oldStored"> Whether the value of the previous computed column was stored in the database or not. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual AlterOperationBuilder<AlterColumnOperation> AlterColumn<T>(
            [NotNull] string name,
            [NotNull] string table,
            [CanBeNull] string type = null,
            bool? unicode = null,
            int? maxLength = null,
            bool rowVersion = false,
            [CanBeNull] string schema = null,
            bool nullable = false,
            [CanBeNull] object defaultValue = null,
            [CanBeNull] string defaultValueSql = null,
            [CanBeNull] string computedColumnSql = null,
            [CanBeNull] Type oldClrType = null,
            [CanBeNull] string oldType = null,
            bool? oldUnicode = null,
            int? oldMaxLength = null,
            bool oldRowVersion = false,
            bool oldNullable = false,
            [CanBeNull] object oldDefaultValue = null,
            [CanBeNull] string oldDefaultValueSql = null,
            [CanBeNull] string oldComputedColumnSql = null,
            bool? fixedLength = null,
            bool? oldFixedLength = null,
            [CanBeNull] string comment = null,
            [CanBeNull] string oldComment = null,
            [CanBeNull] string collation = null,
            [CanBeNull] string oldCollation = null,
            int? precision = null,
            int? oldPrecision = null,
            int? scale = null,
            int? oldScale = null,
            bool? stored = null,
            bool? oldStored = null)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotEmpty(table, nameof(table));

            var operation = new AlterColumnOperation
            {
                Schema = schema,
                Table = table,
                Name = name,
                ClrType = typeof(T),
                ColumnType = type,
                IsUnicode = unicode,
                MaxLength = maxLength,
                IsRowVersion = rowVersion,
                IsNullable = nullable,
                DefaultValue = defaultValue,
                DefaultValueSql = defaultValueSql,
                ComputedColumnSql = computedColumnSql,
                IsFixedLength = fixedLength,
                Comment = comment,
                Collation = collation,
                Precision = precision,
                Scale = scale,
                IsStored = stored,
                OldColumn = new AddColumnOperation
                {
                    ClrType = oldClrType ?? typeof(T),
                    ColumnType = oldType,
                    IsUnicode = oldUnicode,
                    MaxLength = oldMaxLength,
                    IsRowVersion = oldRowVersion,
                    IsNullable = oldNullable,
                    DefaultValue = oldDefaultValue,
                    DefaultValueSql = oldDefaultValueSql,
                    ComputedColumnSql = oldComputedColumnSql,
                    IsFixedLength = oldFixedLength,
                    Comment = oldComment,
                    Collation = oldCollation,
                    Precision = oldPrecision,
                    Scale = oldScale,
                    IsStored = oldStored
                }
            };

            Operations.Add(operation);

            return new AlterOperationBuilder<AlterColumnOperation>(operation);
        }

        /// <summary>
        ///     Builds an <see cref="AlterDatabaseOperation" /> to alter an existing database.
        /// </summary>
        /// <param name="collation"> A collation to apply to the column. </param>
        /// <param name="oldCollation"> The previous collation to apply to the column. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual AlterOperationBuilder<AlterDatabaseOperation> AlterDatabase(
            [CanBeNull] string collation = null,
            [CanBeNull] string oldCollation = null)
        {
            var operation = new AlterDatabaseOperation { Collation = collation };
            Operations.Add(operation);

            return new AlterOperationBuilder<AlterDatabaseOperation>(operation);
        }

        /// <summary>
        ///     Builds an <see cref="AlterSequenceOperation" /> to alter an existing sequence.
        /// </summary>
        /// <param name="name"> The sequence name. </param>
        /// <param name="schema"> The schema that contains the sequence, or <see langword="null" /> to use the default schema. </param>
        /// <param name="incrementBy"> The amount to increment by when generating the next value in the sequence, defaulting to 1. </param>
        /// <param name="minValue"> The minimum value of the sequence, or <see langword="null" /> if not specified. </param>
        /// <param name="maxValue"> The maximum value of the sequence, or <see langword="null" /> if not specified. </param>
        /// <param name="cyclic"> Indicates whether or not the sequence will re-start when the maximum value is reached. </param>
        /// <param name="oldIncrementBy"> The previous amount to increment by when generating the next value in the sequence, defaulting to 1. </param>
        /// <param name="oldMinValue"> The previous minimum value of the sequence, or <see langword="null" /> if not specified. </param>
        /// <param name="oldMaxValue"> The previous maximum value of the sequence, or <see langword="null" /> if not specified. </param>
        /// <param name="oldCyclic"> Indicates whether or not the sequence would previously re-start when the maximum value is reached. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual AlterOperationBuilder<AlterSequenceOperation> AlterSequence(
            [NotNull] string name,
            [CanBeNull] string schema = null,
            int incrementBy = 1,
            long? minValue = null,
            long? maxValue = null,
            bool cyclic = false,
            int oldIncrementBy = 1,
            long? oldMinValue = null,
            long? oldMaxValue = null,
            bool oldCyclic = false)
        {
            Check.NotEmpty(name, nameof(name));

            var operation = new AlterSequenceOperation
            {
                Schema = schema,
                Name = name,
                IncrementBy = incrementBy,
                MinValue = minValue,
                MaxValue = maxValue,
                IsCyclic = cyclic,
                OldSequence = new CreateSequenceOperation
                {
                    IncrementBy = oldIncrementBy,
                    MinValue = oldMinValue,
                    MaxValue = oldMaxValue,
                    IsCyclic = oldCyclic
                }
            };
            Operations.Add(operation);

            return new AlterOperationBuilder<AlterSequenceOperation>(operation);
        }

        /// <summary>
        ///     Builds an <see cref="AlterTableOperation" /> to alter an existing table.
        /// </summary>
        /// <param name="name"> The table name. </param>
        /// <param name="schema"> The schema that contains the table, or <see langword="null" /> to use the default schema. </param>
        /// <param name="comment"> A comment to associate with the table. </param>
        /// <param name="oldComment"> The previous comment to associate with the table. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual AlterOperationBuilder<AlterTableOperation> AlterTable(
            [NotNull] string name,
            [CanBeNull] string schema = null,
            [CanBeNull] string comment = null,
            [CanBeNull] string oldComment = null)
        {
            Check.NotEmpty(name, nameof(name));

            var operation = new AlterTableOperation
            {
                Schema = schema,
                Name = name,
                Comment = comment,
                OldTable = new CreateTableOperation { Comment = oldComment }
            };
            Operations.Add(operation);

            return new AlterOperationBuilder<AlterTableOperation>(operation);
        }

        /// <summary>
        ///     Builds an <see cref="CreateIndexOperation" /> to create a new index.
        /// </summary>
        /// <param name="name"> The index name. </param>
        /// <param name="table"> The table that contains the index. </param>
        /// <param name="column"> The column that is indexed. </param>
        /// <param name="schema"> The schema that contains the table, or <see langword="null" /> to use the default schema. </param>
        /// <param name="unique"> Indicates whether or not the index enforces uniqueness. </param>
        /// <param name="filter"> The filter to apply to the index, or <see langword="null" /> for no filter. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual OperationBuilder<CreateIndexOperation> CreateIndex(
            [NotNull] string name,
            [NotNull] string table,
            [NotNull] string column,
            [CanBeNull] string schema = null,
            bool unique = false,
            [CanBeNull] string filter = null)
            => CreateIndex(
                name,
                table,
                new[] { Check.NotEmpty(column, nameof(column)) },
                schema,
                unique,
                filter);

        /// <summary>
        ///     Builds an <see cref="CreateIndexOperation" /> to create a new composite (multi-column) index.
        /// </summary>
        /// <param name="name"> The index name. </param>
        /// <param name="table"> The table that contains the index. </param>
        /// <param name="columns"> The ordered list of columns that are indexed. </param>
        /// <param name="schema"> The schema that contains the table, or <see langword="null" /> to use the default schema. </param>
        /// <param name="unique"> Indicates whether or not the index enforces uniqueness. </param>
        /// <param name="filter"> The filter to apply to the index, or <see langword="null" /> for no filter. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual OperationBuilder<CreateIndexOperation> CreateIndex(
            [NotNull] string name,
            [NotNull] string table,
            [NotNull] string[] columns,
            [CanBeNull] string schema = null,
            bool unique = false,
            [CanBeNull] string filter = null)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotEmpty(table, nameof(table));
            Check.NotEmpty(columns, nameof(columns));

            var operation = new CreateIndexOperation
            {
                Schema = schema,
                Table = table,
                Name = name,
                Columns = columns,
                IsUnique = unique,
                Filter = filter
            };
            Operations.Add(operation);

            return new OperationBuilder<CreateIndexOperation>(operation);
        }

        /// <summary>
        ///     Builds an <see cref="EnsureSchemaOperation" /> to ensure that a schema exists.
        /// </summary>
        /// <param name="name"> The name of the schema. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual OperationBuilder<EnsureSchemaOperation> EnsureSchema(
            [NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            var operation = new EnsureSchemaOperation { Name = name };
            Operations.Add(operation);

            return new OperationBuilder<EnsureSchemaOperation>(operation);
        }

        /// <summary>
        ///     Builds an <see cref="CreateSequenceOperation" /> to create a new sequence.
        /// </summary>
        /// <param name="name"> The sequence name. </param>
        /// <param name="schema"> The schema that contains the sequence, or <see langword="null" /> to use the default schema. </param>
        /// <param name="startValue"> The value at which the sequence will start, defaulting to 1. </param>
        /// <param name="incrementBy"> The amount to increment by when generating the next value in the sequence, defaulting to 1. </param>
        /// <param name="minValue"> The minimum value of the sequence, or <see langword="null" /> if not specified. </param>
        /// <param name="maxValue"> The maximum value of the sequence, or <see langword="null" /> if not specified. </param>
        /// <param name="cyclic"> Indicates whether or not the sequence will re-start when the maximum value is reached. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual OperationBuilder<CreateSequenceOperation> CreateSequence(
            [NotNull] string name,
            [CanBeNull] string schema = null,
            long startValue = 1L,
            int incrementBy = 1,
            long? minValue = null,
            long? maxValue = null,
            bool cyclic = false)
            => CreateSequence<long>(name, schema, startValue, incrementBy, minValue, maxValue, cyclic);

        /// <summary>
        ///     Builds an <see cref="CreateSequenceOperation" /> to create a new sequence.
        /// </summary>
        /// <typeparam name="T"> The CLR type of the values generated by the sequence. </typeparam>
        /// <param name="name"> The sequence name. </param>
        /// <param name="schema"> The schema that contains the sequence, or <see langword="null" /> to use the default schema. </param>
        /// <param name="startValue"> The value at which the sequence will start, defaulting to 1. </param>
        /// <param name="incrementBy"> The amount to increment by when generating the next value in the sequence, defaulting to 1. </param>
        /// <param name="minValue"> The minimum value of the sequence, or <see langword="null" /> if not specified. </param>
        /// <param name="maxValue"> The maximum value of the sequence, or <see langword="null" /> if not specified. </param>
        /// <param name="cyclic"> Indicates whether or not the sequence will re-start when the maximum value is reached. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual OperationBuilder<CreateSequenceOperation> CreateSequence<T>(
            [NotNull] string name,
            [CanBeNull] string schema = null,
            long startValue = 1L,
            int incrementBy = 1,
            long? minValue = null,
            long? maxValue = null,
            bool cyclic = false)
        {
            Check.NotEmpty(name, nameof(name));

            var operation = new CreateSequenceOperation
            {
                Schema = schema,
                Name = name,
                ClrType = typeof(T),
                StartValue = startValue,
                IncrementBy = incrementBy,
                MinValue = minValue,
                MaxValue = maxValue,
                IsCyclic = cyclic
            };
            Operations.Add(operation);

            return new OperationBuilder<CreateSequenceOperation>(operation);
        }

        /// <summary>
        ///     <para>
        ///         Warning, this API is obsolete. Use <see cref="AddCheckConstraint" /> instead.
        ///     </para>
        ///     <para>
        ///         Builds an <see cref="AddCheckConstraintOperation" /> to create a new check constraint.
        ///     </para>
        /// </summary>
        /// <param name="name"> The check constraint name. </param>
        /// <param name="table"> The name of the table for the check constraint. </param>
        /// <param name="sql"> The constraint sql for the check constraint. </param>
        /// <param name="schema"> The schema that contains the check constraint, or <see langword="null" /> to use the default schema. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual OperationBuilder<AddCheckConstraintOperation> CreateCheckConstraint(
            [NotNull] string name,
            [NotNull] string table,
            [NotNull] string sql,
            [CanBeNull] string schema = null)
            => AddCheckConstraint(name, table, sql, schema);

        /// <summary>
        ///     Builds an <see cref="AddCheckConstraintOperation" /> to add a new check constraint to a table.
        /// </summary>
        /// <param name="name"> The check constraint name. </param>
        /// <param name="table"> The name of the table for the check constraint. </param>
        /// <param name="sql"> The constraint sql for the check constraint. </param>
        /// <param name="schema"> The schema that contains the check constraint, or <see langword="null" /> to use the default schema. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual OperationBuilder<AddCheckConstraintOperation> AddCheckConstraint(
            [NotNull] string name,
            [NotNull] string table,
            [NotNull] string sql,
            [CanBeNull] string schema = null)
        {
            Check.NotEmpty(name, nameof(name));

            var operation = new AddCheckConstraintOperation
            {
                Schema = schema,
                Name = name,
                Table = table,
                Sql = sql
            };
            Operations.Add(operation);

            return new OperationBuilder<AddCheckConstraintOperation>(operation);
        }

        /// <summary>
        ///     Builds an <see cref="CreateTableOperation" /> to create a new table.
        /// </summary>
        /// <typeparam name="TColumns"> Type of a typically anonymous type for building columns. </typeparam>
        /// <param name="name"> The name of the table. </param>
        /// <param name="columns">
        ///     A delegate using a <see cref="ColumnsBuilder" /> to create an anonymous type configuring the columns of the table.
        /// </param>
        /// <param name="schema"> The schema that contains the table, or <see langword="null" /> to use the default schema. </param>
        /// <param name="constraints">
        ///     A delegate allowing constraints to be applied over the columns configured by the 'columns' delegate above.
        /// </param>
        /// <param name="comment"> A comment to be applied to the table. </param>
        /// <returns> A <see cref="CreateTableBuilder{TColumns}" /> to allow further configuration to be chained. </returns>
        public virtual CreateTableBuilder<TColumns> CreateTable<TColumns>(
            [NotNull] string name,
            [NotNull] Func<ColumnsBuilder, TColumns> columns,
            [CanBeNull] string schema = null,
            [CanBeNull] Action<CreateTableBuilder<TColumns>> constraints = null,
            [CanBeNull] string comment = null)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(columns, nameof(columns));

            var createTableOperation = new CreateTableOperation
            {
                Schema = schema,
                Name = name,
                Comment = comment
            };

            var columnsBuilder = new ColumnsBuilder(createTableOperation);
            var columnsObject = columns(columnsBuilder);
            var columnMap = new Dictionary<PropertyInfo, AddColumnOperation>();
            foreach (var property in typeof(TColumns).GetTypeInfo().DeclaredProperties)
            {
                var addColumnOperation = ((IInfrastructure<AddColumnOperation>)property.GetMethod.Invoke(columnsObject, null)).Instance;
                if (addColumnOperation.Name == null)
                {
                    addColumnOperation.Name = property.Name;
                }

                columnMap.Add(property, addColumnOperation);
            }

            var builder = new CreateTableBuilder<TColumns>(createTableOperation, columnMap);
            constraints?.Invoke(builder);

            Operations.Add(createTableOperation);

            return builder;
        }

        /// <summary>
        ///     Builds an <see cref="DropColumnOperation" /> to drop an existing column.
        /// </summary>
        /// <param name="name"> The name of the column to drop. </param>
        /// <param name="table"> The table that contains the column. </param>
        /// <param name="schema"> The schema that contains the table, or <see langword="null" /> to use the default schema. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual OperationBuilder<DropColumnOperation> DropColumn(
            [NotNull] string name,
            [NotNull] string table,
            [CanBeNull] string schema = null)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotEmpty(table, nameof(table));

            var operation = new DropColumnOperation
            {
                Schema = schema,
                Table = table,
                Name = name
            };
            Operations.Add(operation);

            return new OperationBuilder<DropColumnOperation>(operation);
        }

        /// <summary>
        ///     Builds an <see cref="DropForeignKeyOperation" /> to drop an existing foreign key constraint.
        /// </summary>
        /// <param name="name"> The name of the foreign key constraint to drop. </param>
        /// <param name="table"> The table that contains the foreign key. </param>
        /// <param name="schema"> The schema that contains the table, or <see langword="null" /> to use the default schema. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual OperationBuilder<DropForeignKeyOperation> DropForeignKey(
            [NotNull] string name,
            [NotNull] string table,
            [CanBeNull] string schema = null)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotEmpty(table, nameof(table));

            var operation = new DropForeignKeyOperation
            {
                Schema = schema,
                Table = table,
                Name = name
            };
            Operations.Add(operation);

            return new OperationBuilder<DropForeignKeyOperation>(operation);
        }

        /// <summary>
        ///     Builds an <see cref="DropIndexOperation" /> to drop an existing index.
        /// </summary>
        /// <param name="name"> The name of the index to drop. </param>
        /// <param name="table"> The table that contains the index. </param>
        /// <param name="schema"> The schema that contains the table, or <see langword="null" /> to use the default schema. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual OperationBuilder<DropIndexOperation> DropIndex(
            [NotNull] string name,
            [CanBeNull] string table = null,
            [CanBeNull] string schema = null)
        {
            Check.NotEmpty(name, nameof(name));

            var operation = new DropIndexOperation
            {
                Schema = schema,
                Table = table,
                Name = name
            };
            Operations.Add(operation);

            return new OperationBuilder<DropIndexOperation>(operation);
        }

        /// <summary>
        ///     Builds an <see cref="DropPrimaryKeyOperation" /> to drop an existing primary key.
        /// </summary>
        /// <param name="name"> The name of the primary key constraint to drop. </param>
        /// <param name="table"> The table that contains the key. </param>
        /// <param name="schema"> The schema that contains the table, or <see langword="null" /> to use the default schema. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual OperationBuilder<DropPrimaryKeyOperation> DropPrimaryKey(
            [NotNull] string name,
            [NotNull] string table,
            [CanBeNull] string schema = null)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotEmpty(table, nameof(table));

            var operation = new DropPrimaryKeyOperation
            {
                Schema = schema,
                Table = table,
                Name = name
            };
            Operations.Add(operation);

            return new OperationBuilder<DropPrimaryKeyOperation>(operation);
        }

        /// <summary>
        ///     Builds an <see cref="DropSchemaOperation" /> to drop an existing schema.
        /// </summary>
        /// <param name="name"> The name of the schema to drop. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual OperationBuilder<DropSchemaOperation> DropSchema(
            [NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            var operation = new DropSchemaOperation { Name = name };
            Operations.Add(operation);

            return new OperationBuilder<DropSchemaOperation>(operation);
        }

        /// <summary>
        ///     Builds an <see cref="DropSequenceOperation" /> to drop an existing sequence.
        /// </summary>
        /// <param name="name"> The name of the sequence to drop. </param>
        /// <param name="schema"> The schema that contains the sequence, or <see langword="null" /> to use the default schema. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual OperationBuilder<DropSequenceOperation> DropSequence(
            [NotNull] string name,
            [CanBeNull] string schema = null)
        {
            Check.NotEmpty(name, nameof(name));

            var operation = new DropSequenceOperation { Schema = schema, Name = name };
            Operations.Add(operation);

            return new OperationBuilder<DropSequenceOperation>(operation);
        }

        /// <summary>
        ///     Builds an <see cref="DropCheckConstraintOperation" /> to drop an existing check constraint.
        /// </summary>
        /// <param name="name"> The name of the check constraint to drop. </param>
        /// <param name="table"> The name of the table for the check constraint to drop. </param>
        /// <param name="schema"> The schema that contains the check constraint, or <see langword="null" /> to use the default schema. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual OperationBuilder<DropCheckConstraintOperation> DropCheckConstraint(
            [NotNull] string name,
            [NotNull] string table,
            [CanBeNull] string schema = null)
        {
            Check.NotEmpty(name, nameof(name));

            var operation = new DropCheckConstraintOperation
            {
                Name = name,
                Table = table,
                Schema = schema
            };
            Operations.Add(operation);

            return new OperationBuilder<DropCheckConstraintOperation>(operation);
        }

        /// <summary>
        ///     Builds an <see cref="DropTableOperation" /> to drop an existing table.
        /// </summary>
        /// <param name="name"> The name of the table to drop. </param>
        /// <param name="schema"> The schema that contains the table, or <see langword="null" /> to use the default schema. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual OperationBuilder<DropTableOperation> DropTable(
            [NotNull] string name,
            [CanBeNull] string schema = null)
        {
            Check.NotEmpty(name, nameof(name));

            var operation = new DropTableOperation { Schema = schema, Name = name };
            Operations.Add(operation);

            return new OperationBuilder<DropTableOperation>(operation);
        }

        /// <summary>
        ///     Builds an <see cref="DropUniqueConstraintOperation" /> to drop an existing unique constraint.
        /// </summary>
        /// <param name="name"> The name of the constraint to drop. </param>
        /// <param name="table"> The table that contains the constraint. </param>
        /// <param name="schema"> The schema that contains the table, or <see langword="null" /> to use the default schema. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual OperationBuilder<DropUniqueConstraintOperation> DropUniqueConstraint(
            [NotNull] string name,
            [NotNull] string table,
            [CanBeNull] string schema = null)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotEmpty(table, nameof(table));

            var operation = new DropUniqueConstraintOperation
            {
                Schema = schema,
                Table = table,
                Name = name
            };
            Operations.Add(operation);

            return new OperationBuilder<DropUniqueConstraintOperation>(operation);
        }

        /// <summary>
        ///     Builds an <see cref="RenameColumnOperation" /> to rename an existing column.
        /// </summary>
        /// <param name="name"> The name of the column to be renamed.</param>
        /// <param name="table"> The table that contains the column. </param>
        /// <param name="newName"> The new name for the column. </param>
        /// <param name="schema"> The schema that contains the table, or <see langword="null" /> to use the default schema. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual OperationBuilder<RenameColumnOperation> RenameColumn(
            [NotNull] string name,
            [NotNull] string table,
            [NotNull] string newName,
            [CanBeNull] string schema = null)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotEmpty(table, nameof(table));
            Check.NotEmpty(newName, nameof(newName));

            var operation = new RenameColumnOperation
            {
                Name = name,
                Schema = schema,
                Table = table,
                NewName = newName
            };
            Operations.Add(operation);

            return new OperationBuilder<RenameColumnOperation>(operation);
        }

        /// <summary>
        ///     Builds an <see cref="RenameIndexOperation" /> to rename an existing index.
        /// </summary>
        /// <param name="name"> The name of the index to be renamed.</param>
        /// <param name="newName"> The new name for the column. </param>
        /// <param name="table"> The table that contains the index. </param>
        /// <param name="schema"> The schema that contains the table, or <see langword="null" /> to use the default schema. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual OperationBuilder<RenameIndexOperation> RenameIndex(
            [NotNull] string name,
            [NotNull] string newName,
            [CanBeNull] string table = null,
            [CanBeNull] string schema = null)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotEmpty(newName, nameof(newName));

            var operation = new RenameIndexOperation
            {
                Schema = schema,
                Table = table,
                Name = name,
                NewName = newName
            };
            Operations.Add(operation);

            return new OperationBuilder<RenameIndexOperation>(operation);
        }

        /// <summary>
        ///     Builds an <see cref="RenameSequenceOperation" /> to rename an existing sequence.
        /// </summary>
        /// <param name="name"> The name of the sequence to be renamed.</param>
        /// <param name="schema"> The schema that contains the sequence, or <see langword="null" /> to use the default schema. </param>
        /// <param name="newName"> The new sequence name or <see langword="null" /> if only the schema has changed. </param>
        /// <param name="newSchema"> The new schema name or <see langword="null" /> if only the name has changed. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual OperationBuilder<RenameSequenceOperation> RenameSequence(
            [NotNull] string name,
            [CanBeNull] string schema = null,
            [CanBeNull] string newName = null,
            [CanBeNull] string newSchema = null)
        {
            Check.NotEmpty(name, nameof(name));

            var operation = new RenameSequenceOperation
            {
                Name = name,
                Schema = schema,
                NewName = newName,
                NewSchema = newSchema
            };
            Operations.Add(operation);

            return new OperationBuilder<RenameSequenceOperation>(operation);
        }

        /// <summary>
        ///     Builds an <see cref="RenameTableOperation" /> to rename an existing table.
        /// </summary>
        /// <param name="name"> The name of the table to be renamed.</param>
        /// <param name="schema"> The schema that contains the table, or <see langword="null" /> to use the default schema. </param>
        /// <param name="newName"> The new table name or <see langword="null" /> if only the schema has changed. </param>
        /// <param name="newSchema"> The new schema name, or <see langword="null" /> to use the default schema. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual OperationBuilder<RenameTableOperation> RenameTable(
            [NotNull] string name,
            [CanBeNull] string schema = null,
            [CanBeNull] string newName = null,
            [CanBeNull] string newSchema = null)
        {
            Check.NotEmpty(name, nameof(name));

            var operation = new RenameTableOperation
            {
                Schema = schema,
                Name = name,
                NewName = newName,
                NewSchema = newSchema
            };
            Operations.Add(operation);

            return new OperationBuilder<RenameTableOperation>(operation);
        }

        /// <summary>
        ///     Builds an <see cref="RestartSequenceOperation" /> to re-start an existing sequence.
        /// </summary>
        /// <param name="name"> The name of the sequence.</param>
        /// <param name="startValue"> The value at which the sequence will start, defaulting to 1. </param>
        /// <param name="schema"> The schema that contains the sequence, or <see langword="null" /> to use the default schema. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual OperationBuilder<RestartSequenceOperation> RestartSequence(
            [NotNull] string name,
            long startValue = 1L,
            [CanBeNull] string schema = null)
        {
            Check.NotEmpty(name, nameof(name));

            var operation = new RestartSequenceOperation
            {
                Name = name,
                Schema = schema,
                StartValue = startValue
            };
            Operations.Add(operation);

            return new OperationBuilder<RestartSequenceOperation>(operation);
        }

        /// <summary>
        ///     Builds an <see cref="SqlOperation" /> to execute raw SQL.
        /// </summary>
        /// <param name="sql"> The SQL string to be executed to perform the operation. </param>
        /// <param name="suppressTransaction">
        ///     Indicates whether or not transactions will be suppressed while executing the SQL.
        /// </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual OperationBuilder<SqlOperation> Sql(
            [NotNull] string sql,
            bool suppressTransaction = false)
        {
            Check.NotEmpty(sql, nameof(sql));

            var operation = new SqlOperation { Sql = sql, SuppressTransaction = suppressTransaction };
            Operations.Add(operation);

            return new OperationBuilder<SqlOperation>(operation);
        }

        /// <summary>
        ///     Builds an <see cref="InsertDataOperation" /> to insert a single seed data value for a single column.
        /// </summary>
        /// <param name="table"> The table into which the data will be inserted. </param>
        /// <param name="column"> The name of the column into which the data will be inserted. </param>
        /// <param name="value"> The value to insert. </param>
        /// <param name="schema"> The schema that contains the table, or <see langword="null" /> to use the default schema. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual OperationBuilder<InsertDataOperation> InsertData(
            [NotNull] string table,
            [NotNull] string column,
            [CanBeNull] object value,
            [CanBeNull] string schema = null)
            => InsertData(table, new[] { Check.NotEmpty(column, nameof(column)) }, new[] { value }, schema);

        /// <summary>
        ///     Builds an <see cref="InsertDataOperation" /> to insert a single seed data value for a single column.
        /// </summary>
        /// <param name="table"> The table into which the data will be inserted. </param>
        /// <param name="column"> The name of the column into which the data will be inserted. </param>
        /// <param name="columnType"> The store type for the column into which data will be inserted. </param>
        /// <param name="value"> The value to insert. </param>
        /// <param name="schema"> The schema that contains the table, or <see langword="null" /> to use the default schema. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual OperationBuilder<InsertDataOperation> InsertData(
            [NotNull] string table,
            [NotNull] string column,
            [NotNull] string columnType,
            [CanBeNull] object value,
            [CanBeNull] string schema = null)
            => InsertData(
                table,
                new[] { Check.NotEmpty(column, nameof(column)) },
                new[] { Check.NotEmpty(columnType, nameof(columnType)) },
                new[] { value }, schema);

        /// <summary>
        ///     Builds an <see cref="InsertDataOperation" /> to insert a single row of seed data values.
        /// </summary>
        /// <param name="table"> The table into which the data will be inserted. </param>
        /// <param name="columns"> The names of the columns into which the data will be inserted. </param>
        /// <param name="values"> The values to insert, one value for each column in 'columns'. </param>
        /// <param name="schema"> The schema that contains the table, or <see langword="null" /> to use the default schema. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual OperationBuilder<InsertDataOperation> InsertData(
            [NotNull] string table,
            [NotNull] string[] columns,
            [NotNull] object[] values,
            [CanBeNull] string schema = null)
            => InsertData(table, columns, ToMultidimensionalArray(Check.NotNull(values, nameof(values))), schema);

        /// <summary>
        ///     Builds an <see cref="InsertDataOperation" /> to insert a single row of seed data values.
        /// </summary>
        /// <param name="table"> The table into which the data will be inserted. </param>
        /// <param name="columns"> The names of the columns into which the data will be inserted. </param>
        /// <param name="columnTypes"> A list of store types for the columns into which data will be inserted. </param>
        /// <param name="values"> The values to insert, one value for each column in 'columns'. </param>
        /// <param name="schema"> The schema that contains the table, or <see langword="null" /> to use the default schema. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual OperationBuilder<InsertDataOperation> InsertData(
            [NotNull] string table,
            [NotNull] string[] columns,
            [NotNull] string[] columnTypes,
            [NotNull] object[] values,
            [CanBeNull] string schema = null)
            => InsertData(table, columns, columnTypes, ToMultidimensionalArray(Check.NotNull(values, nameof(values))), schema);

        /// <summary>
        ///     Builds an <see cref="InsertDataOperation" /> to insert multiple rows of seed data values for a single column.
        /// </summary>
        /// <param name="table"> The table into which the data will be inserted. </param>
        /// <param name="column"> The name of the column into which the data will be inserted. </param>
        /// <param name="values"> The values to insert, one value for each row. </param>
        /// <param name="schema"> The schema that contains the table, or <see langword="null" /> to use the default schema. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual OperationBuilder<InsertDataOperation> InsertData(
            [NotNull] string table,
            [NotNull] string column,
            [NotNull] object[] values,
            [CanBeNull] string schema = null)
            => InsertDataInternal(
                table,
                new[] { Check.NotEmpty(column, nameof(column)) },
                null,
                ToMultidimensionalArray(Check.NotNull(values, nameof(values)), firstDimension: true),
                schema);

        /// <summary>
        ///     Builds an <see cref="InsertDataOperation" /> to insert multiple rows of seed data values for a single column.
        /// </summary>
        /// <param name="table"> The table into which the data will be inserted. </param>
        /// <param name="column"> The name of the column into which the data will be inserted. </param>
        /// <param name="columnType"> The store type for the column into which data will be inserted. </param>
        /// <param name="values"> The values to insert, one value for each row. </param>
        /// <param name="schema"> The schema that contains the table, or <see langword="null" /> to use the default schema. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual OperationBuilder<InsertDataOperation> InsertData(
            [NotNull] string table,
            [NotNull] string column,
            [NotNull] string columnType,
            [NotNull] object[] values,
            [CanBeNull] string schema = null)
            => InsertDataInternal(
                table,
                new[] { Check.NotEmpty(column, nameof(column)) },
                new[] { Check.NotEmpty(columnType, nameof(columnType)) },
                ToMultidimensionalArray(Check.NotNull(values, nameof(values)), firstDimension: true),
                schema);

        /// <summary>
        ///     Builds an <see cref="InsertDataOperation" /> to insert multiple rows of seed data values for multiple columns.
        /// </summary>
        /// <param name="table"> The table into which the data will be inserted. </param>
        /// <param name="columns"> The names of the columns into which the data will be inserted. </param>
        /// <param name="values">
        ///     The values to insert where each element of the outer array represents a row, and each inner array contains values for each of the
        ///     columns in 'columns'.
        /// </param>
        /// <param name="schema"> The schema that contains the table, or <see langword="null" /> to use the default schema. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual OperationBuilder<InsertDataOperation> InsertData(
            [NotNull] string table,
            [NotNull] string[] columns,
            [NotNull] object[,] values,
            [CanBeNull] string schema = null)
            => InsertDataInternal(table, columns, null, values, schema);

        /// <summary>
        ///     Builds an <see cref="InsertDataOperation" /> to insert multiple rows of seed data values for multiple columns.
        /// </summary>
        /// <param name="table"> The table into which the data will be inserted. </param>
        /// <param name="columns"> The names of the columns into which the data will be inserted. </param>
        /// <param name="columnTypes"> A list of store types for the columns into which data will be inserted. </param>
        /// <param name="values">
        ///     The values to insert where each element of the outer array represents a row, and each inner array contains values for each of the
        ///     columns in 'columns'.
        /// </param>
        /// <param name="schema"> The schema that contains the table, or <see langword="null" /> to use the default schema. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual OperationBuilder<InsertDataOperation> InsertData(
            [NotNull] string table,
            [NotNull] string[] columns,
            [NotNull] string[] columnTypes,
            [NotNull] object[,] values,
            [CanBeNull] string schema = null)
        {
            Check.NotEmpty(columnTypes, nameof(columnTypes));

            return InsertDataInternal(table, columns, columnTypes, values, schema);
        }

        private OperationBuilder<InsertDataOperation> InsertDataInternal(
            string table,
            string[] columns,
            string[] columnTypes,
            object[,] values,
            string schema)
        {
            Check.NotEmpty(table, nameof(table));
            Check.NotNull(columns, nameof(columns));
            Check.NotNull(values, nameof(values));

            var operation = new InsertDataOperation
            {
                Table = table,
                Schema = schema,
                Columns = columns,
                Values = values
            };
            Operations.Add(operation);

            return new OperationBuilder<InsertDataOperation>(operation);
        }

        /// <summary>
        ///     Builds an <see cref="DeleteDataOperation" /> to delete a single row of seed data.
        /// </summary>
        /// <param name="table"> The table from which the data will be deleted. </param>
        /// <param name="keyColumn"> The name of the key column used to select the row to delete. </param>
        /// <param name="keyValue"> The key value of the row to delete. </param>
        /// <param name="schema"> The schema that contains the table, or <see langword="null" /> to use the default schema. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual OperationBuilder<DeleteDataOperation> DeleteData(
            [NotNull] string table,
            [NotNull] string keyColumn,
            [CanBeNull] object keyValue,
            [CanBeNull] string schema = null)
            => DeleteData(table, new[] { Check.NotNull(keyColumn, nameof(keyValue)) }, new[] { keyValue }, schema);

        /// <summary>
        ///     Builds an <see cref="DeleteDataOperation" /> to delete a single row of seed data.
        /// </summary>
        /// <param name="table"> The table from which the data will be deleted. </param>
        /// <param name="keyColumn"> The name of the key column used to select the row to delete. </param>
        /// <param name="keyColumnType">
        ///     The store type for the column that will be used to identify the rows that should be deleted.
        ///  </param>
        /// <param name="keyValue"> The key value of the row to delete. </param>
        /// <param name="schema"> The schema that contains the table, or <see langword="null" /> to use the default schema. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual OperationBuilder<DeleteDataOperation> DeleteData(
            [NotNull] string table,
            [NotNull] string keyColumn,
            [NotNull] string keyColumnType,
            [CanBeNull] object keyValue,
            [CanBeNull] string schema = null)
            => DeleteData(
                table,
                new[] { Check.NotNull(keyColumn, nameof(keyValue)) },
                new[] { Check.NotNull(keyColumnType, nameof(keyColumnType)) },
                new[] { keyValue },
                schema);

        /// <summary>
        ///     Builds an <see cref="DeleteDataOperation" /> to delete a single row of seed data from
        ///     a table with a composite (multi-column) key.
        /// </summary>
        /// <param name="table"> The table from which the data will be deleted. </param>
        /// <param name="keyColumns"> The names of the key columns used to select the row to delete. </param>
        /// <param name="keyValues"> The key values of the row to delete, one value for each column in 'keyColumns'. </param>
        /// <param name="schema"> The schema that contains the table, or <see langword="null" /> to use the default schema. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual OperationBuilder<DeleteDataOperation> DeleteData(
            [NotNull] string table,
            [NotNull] string[] keyColumns,
            [NotNull] object[] keyValues,
            [CanBeNull] string schema = null)
            => DeleteData(
                table,
                keyColumns,
                ToMultidimensionalArray(Check.NotNull(keyValues, nameof(keyValues))),
                schema);

        /// <summary>
        ///     Builds an <see cref="DeleteDataOperation" /> to delete a single row of seed data from
        ///     a table with a composite (multi-column) key.
        /// </summary>
        /// <param name="table"> The table from which the data will be deleted. </param>
        /// <param name="keyColumns"> The names of the key columns used to select the row to delete. </param>
        /// <param name="keyColumnTypes">
        ///     The store types for the columns that will be used to identify the rows that should be deleted.
        ///  </param>
        /// <param name="keyValues"> The key values of the row to delete, one value for each column in 'keyColumns'. </param>
        /// <param name="schema"> The schema that contains the table, or <see langword="null" /> to use the default schema. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual OperationBuilder<DeleteDataOperation> DeleteData(
            [NotNull] string table,
            [NotNull] string[] keyColumns,
            [NotNull] string[] keyColumnTypes,
            [NotNull] object[] keyValues,
            [CanBeNull] string schema = null)
            => DeleteDataInternal(
                table,
                keyColumns,
                keyColumnTypes,
                ToMultidimensionalArray(Check.NotNull(keyValues, nameof(keyValues))),
                schema);

        /// <summary>
        ///     Builds an <see cref="DeleteDataOperation" /> to delete multiple rows of seed data.
        /// </summary>
        /// <param name="table"> The table from which the data will be deleted. </param>
        /// <param name="keyColumn"> The name of the key column used to select the row to delete. </param>
        /// <param name="keyValues"> The key values of the rows to delete, one value per row. </param>
        /// <param name="schema"> The schema that contains the table, or <see langword="null" /> to use the default schema. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual OperationBuilder<DeleteDataOperation> DeleteData(
            [NotNull] string table,
            [NotNull] string keyColumn,
            [NotNull] object[] keyValues,
            [CanBeNull] string schema = null)
            => DeleteData(
                table,
                new[] { Check.NotEmpty(keyColumn, nameof(keyColumn)) },
                ToMultidimensionalArray(Check.NotNull(keyValues, nameof(keyValues)), firstDimension: true),
                schema);

        /// <summary>
        ///     Builds an <see cref="DeleteDataOperation" /> to delete multiple rows of seed data.
        /// </summary>
        /// <param name="table"> The table from which the data will be deleted. </param>
        /// <param name="keyColumn"> The name of the key column used to select the row to delete. </param>
        /// <param name="keyColumnType">
        ///     The store type for the column that will be used to identify the rows that should be deleted.
        ///  </param>
        /// <param name="keyValues"> The key values of the rows to delete, one value per row. </param>
        /// <param name="schema"> The schema that contains the table, or <see langword="null" /> to use the default schema. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual OperationBuilder<DeleteDataOperation> DeleteData(
            [NotNull] string table,
            [NotNull] string keyColumn,
            [NotNull] string keyColumnType,
            [NotNull] object[] keyValues,
            [CanBeNull] string schema = null)
            => DeleteData(
                table,
                new[] { Check.NotEmpty(keyColumn, nameof(keyColumn)) },
                new[] { Check.NotEmpty(keyColumnType, nameof(keyColumnType)) },
                ToMultidimensionalArray(Check.NotNull(keyValues, nameof(keyValues)), firstDimension: true),
                schema);

        /// <summary>
        ///     Builds an <see cref="DeleteDataOperation" /> to delete multiple rows of seed data from
        ///     a table with a composite (multi-column) key.
        /// </summary>
        /// <param name="table"> The table from which the data will be deleted. </param>
        /// <param name="keyColumns"> The names of the key columns used to select the rows to delete. </param>
        /// <param name="keyValues">
        ///     The key values of the rows to delete, where each element of the outer array represents a row, and each inner array contains values for
        ///     each of the key columns in 'keyColumns'.
        /// </param>
        /// <param name="schema"> The schema that contains the table, or <see langword="null" /> to use the default schema. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual OperationBuilder<DeleteDataOperation> DeleteData(
            [NotNull] string table,
            [NotNull] string[] keyColumns,
            [NotNull] object[,] keyValues,
            [CanBeNull] string schema = null)
            => DeleteDataInternal(table, keyColumns, null, keyValues, schema);

        /// <summary>
        ///     Builds an <see cref="DeleteDataOperation" /> to delete multiple rows of seed data from
        ///     a table with a composite (multi-column) key.
        /// </summary>
        /// <param name="table"> The table from which the data will be deleted. </param>
        /// <param name="keyColumns"> The names of the key columns used to select the rows to delete. </param>
        /// <param name="keyColumnTypes">
        ///     The store types for the columns that will be used to identify the rows that should be deleted.
        ///  </param>
        /// <param name="keyValues">
        ///     The key values of the rows to delete, where each element of the outer array represents a row, and each inner array contains values for
        ///     each of the key columns in 'keyColumns'.
        /// </param>
        /// <param name="schema"> The schema that contains the table, or <see langword="null" /> to use the default schema. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual OperationBuilder<DeleteDataOperation> DeleteData(
            [NotNull] string table,
            [NotNull] string[] keyColumns,
            [NotNull] string[] keyColumnTypes,
            [NotNull] object[,] keyValues,
            [CanBeNull] string schema = null)
        {
            Check.NotEmpty(keyColumnTypes, nameof(keyColumnTypes));

            return DeleteDataInternal(table, keyColumns, keyColumnTypes, keyValues, schema);
        }

        private OperationBuilder<DeleteDataOperation> DeleteDataInternal(
            string table,
            string[] keyColumns,
            string[] keyColumnTypes,
            object[,] keyValues,
            string schema)
        {
            Check.NotEmpty(table, nameof(table));
            Check.NotNull(keyColumns, nameof(keyColumns));
            Check.NotNull(keyValues, nameof(keyValues));

            var operation = new DeleteDataOperation
            {
                Table = table,
                Schema = schema,
                KeyColumns = keyColumns,
                KeyColumnTypes = keyColumnTypes,
                KeyValues = keyValues
            };
            Operations.Add(operation);

            return new OperationBuilder<DeleteDataOperation>(operation);
        }

        /// <summary>
        ///     Builds an <see cref="UpdateDataOperation" /> to update a single row of seed data.
        /// </summary>
        /// <param name="table"> The table containing the data to be updated. </param>
        /// <param name="keyColumn"> The name of the key column used to select the row to update. </param>
        /// <param name="keyValue"> The key value of the row to update. </param>
        /// <param name="column"> The column to update. </param>
        /// <param name="value"> The new value for the column in the selected row. </param>
        /// <param name="schema"> The schema that contains the table, or <see langword="null" /> to use the default schema. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual OperationBuilder<UpdateDataOperation> UpdateData(
            [NotNull] string table,
            [NotNull] string keyColumn,
            [CanBeNull] object keyValue,
            [NotNull] string column,
            [CanBeNull] object value,
            [CanBeNull] string schema = null)
            => UpdateData(
                table,
                keyColumn,
                keyValue,
                new[] { Check.NotEmpty(column, nameof(column)) },
                new[] { value },
                schema);

        /// <summary>
        ///     Builds an <see cref="UpdateDataOperation" /> to update a single row of seed data.
        /// </summary>
        /// <param name="table"> The table containing the data to be updated. </param>
        /// <param name="keyColumn"> The name of the key column used to select the row to update. </param>
        /// <param name="keyValue"> The key value of the row to update. </param>
        /// <param name="columns"> The columns to update. </param>
        /// <param name="values"> The new values, one for each column in 'columns', for the selected row. </param>
        /// <param name="schema"> The schema that contains the table, or <see langword="null" /> to use the default schema. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual OperationBuilder<UpdateDataOperation> UpdateData(
            [NotNull] string table,
            [NotNull] string keyColumn,
            [CanBeNull] object keyValue,
            [NotNull] string[] columns,
            [NotNull] object[] values,
            [CanBeNull] string schema = null)
            => UpdateData(
                table,
                new[] { Check.NotEmpty(keyColumn, nameof(keyColumn)) },
                new[] { keyValue },
                columns,
                values,
                schema);

        /// <summary>
        ///     Builds an <see cref="UpdateDataOperation" /> to update a single row of seed data for a table with
        ///     a composite (multi-column) key.
        /// </summary>
        /// <param name="table"> The table containing the data to be updated. </param>
        /// <param name="keyColumns"> The names of the key columns used to select the row to update. </param>
        /// <param name="keyValues"> The key values of the row to update, one value for each column in 'keyColumns'. </param>
        /// <param name="column"> The column to update. </param>
        /// <param name="value"> The new value for the column in the selected row. </param>
        /// <param name="schema"> The schema that contains the table, or <see langword="null" /> to use the default schema. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual OperationBuilder<UpdateDataOperation> UpdateData(
            [NotNull] string table,
            [NotNull] string[] keyColumns,
            [NotNull] object[] keyValues,
            [NotNull] string column,
            [CanBeNull] object value,
            [CanBeNull] string schema = null)
            => UpdateData(
                table,
                keyColumns,
                keyValues,
                new[] { Check.NotEmpty(column, nameof(column)) },
                new[] { value },
                schema);

        /// <summary>
        ///     Builds an <see cref="UpdateDataOperation" /> to update a single row of seed data for a table with
        ///     a composite (multi-column) key.
        /// </summary>
        /// <param name="table"> The table containing the data to be updated. </param>
        /// <param name="keyColumns"> The names of the key columns used to select the row to update. </param>
        /// <param name="keyValues"> The key values of the row to update, one value for each column in 'keyColumns'. </param>
        /// <param name="columns"> The columns to update. </param>
        /// <param name="values"> The new values, one for each column in 'columns', for the selected row. </param>
        /// <param name="schema"> The schema that contains the table, or <see langword="null" /> to use the default schema. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual OperationBuilder<UpdateDataOperation> UpdateData(
            [NotNull] string table,
            [NotNull] string[] keyColumns,
            [NotNull] object[] keyValues,
            [NotNull] string[] columns,
            [NotNull] object[] values,
            [CanBeNull] string schema = null)
            => UpdateData(
                table,
                keyColumns,
                ToMultidimensionalArray(Check.NotNull(keyValues, nameof(keyValues))),
                columns,
                ToMultidimensionalArray(Check.NotNull(values, nameof(values))),
                schema);

        /// <summary>
        ///     Builds an <see cref="UpdateDataOperation" /> to update a single row of seed data for a table with
        ///     a composite (multi-column) key.
        /// </summary>
        /// <param name="table"> The table containing the data to be updated. </param>
        /// <param name="keyColumns"> The names of the key columns used to select the row to update. </param>
        /// <param name="keyColumnTypes">
        ///     A list of store types for the columns that will be used to identify the rows that should be updated.
        /// </param>
        /// <param name="keyValues"> The key values of the row to update, one value for each column in 'keyColumns'. </param>
        /// <param name="columns"> The columns to update. </param>
        /// <param name="columnTypes"> A list of store types for the columns in which data will be updated. </param>
        /// <param name="values"> The new values, one for each column in 'columns', for the selected row. </param>
        /// <param name="schema"> The schema that contains the table, or <see langword="null" /> to use the default schema. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual OperationBuilder<UpdateDataOperation> UpdateData(
            [NotNull] string table,
            [NotNull] string[] keyColumns,
            [NotNull] string[] keyColumnTypes,
            [NotNull] object[] keyValues,
            [NotNull] string[] columns,
            [NotNull] string[] columnTypes,
            [NotNull] object[] values,
            [CanBeNull] string schema = null)
            => UpdateData(
                table,
                keyColumns,
                keyColumnTypes,
                ToMultidimensionalArray(Check.NotNull(keyValues, nameof(keyValues))),
                columns,
                columnTypes,
                ToMultidimensionalArray(Check.NotNull(values, nameof(values))),
                schema);

        /// <summary>
        ///     Builds an <see cref="UpdateDataOperation" /> to update multiple rows of seed data.
        /// </summary>
        /// <param name="table"> The table containing the data to be updated. </param>
        /// <param name="keyColumn"> The name of the key column used to select the row to update. </param>
        /// <param name="keyValues"> The key values of the rows to update, one value per row. </param>
        /// <param name="column"> The column to update. </param>
        /// <param name="values"> The new values for the column, one for each row specified in 'keyValues'. </param>
        /// <param name="schema"> The schema that contains the table, or <see langword="null" /> to use the default schema. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual OperationBuilder<UpdateDataOperation> UpdateData(
            [NotNull] string table,
            [NotNull] string keyColumn,
            [NotNull] object[] keyValues,
            [NotNull] string column,
            [NotNull] object[] values,
            [CanBeNull] string schema = null)
            => UpdateData(
                table,
                keyColumn,
                keyValues,
                new[] { Check.NotEmpty(column, nameof(column)) },
                ToMultidimensionalArray(Check.NotNull(values, nameof(values)), firstDimension: true),
                schema);

        /// <summary>
        ///     Builds an <see cref="UpdateDataOperation" /> to update multiple rows of seed data.
        /// </summary>
        /// <param name="table"> The table containing the data to be updated. </param>
        /// <param name="keyColumn"> The name of the key column used to select the row to update. </param>
        /// <param name="keyValues"> The key values of the rows to update, one value per row. </param>
        /// <param name="columns"> The columns to update. </param>
        /// <param name="values">
        ///     The values for each update, where each element of the outer array represents a row specified in
        ///     'keyValues', and each inner array contains values for each of the columns in 'columns'.
        /// </param>
        /// <param name="schema"> The schema that contains the table, or <see langword="null" /> to use the default schema. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual OperationBuilder<UpdateDataOperation> UpdateData(
            [NotNull] string table,
            [NotNull] string keyColumn,
            [NotNull] object[] keyValues,
            [NotNull] string[] columns,
            [NotNull] object[,] values,
            [CanBeNull] string schema = null)
            => UpdateData(
                table,
                new[] { Check.NotEmpty(keyColumn, nameof(keyColumn)) },
                ToMultidimensionalArray(Check.NotNull(keyValues, nameof(keyValues)), firstDimension: true),
                columns,
                values,
                schema);

        /// <summary>
        ///     Builds an <see cref="UpdateDataOperation" /> to update multiple rows of seed data for a table with
        ///     a composite (multi-column) key.
        /// </summary>
        /// <param name="table"> The table containing the data to be updated. </param>
        /// <param name="keyColumns"> The names of the key columns used to select the rows to update. </param>
        /// <param name="keyValues">
        ///     The key values of the rows to update, where each element of the outer array represents a row, and each inner array contains values for
        ///     each of the key columns in 'keyColumns'.
        /// </param>
        /// <param name="column"> The column to update. </param>
        /// <param name="values"> The new values for the column, one for each row specified in 'keyValues'. </param>
        /// <param name="schema"> The schema that contains the table, or <see langword="null" /> to use the default schema. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual OperationBuilder<UpdateDataOperation> UpdateData(
            [NotNull] string table,
            [NotNull] string[] keyColumns,
            [NotNull] object[,] keyValues,
            [NotNull] string column,
            [NotNull] object[] values,
            [CanBeNull] string schema = null)
            => UpdateData(
                table,
                keyColumns,
                keyValues,
                new[] { Check.NotEmpty(column, nameof(column)) },
                ToMultidimensionalArray(Check.NotNull(values, nameof(values)), firstDimension: true),
                schema);

        /// <summary>
        ///     Builds an <see cref="UpdateDataOperation" /> to update multiple rows of seed data for a table with
        ///     a composite (multi-column) key.
        /// </summary>
        /// <param name="table"> The table containing the data to be updated. </param>
        /// <param name="keyColumns"> The names of the key columns used to select the rows to update. </param>
        /// <param name="keyValues">
        ///     The key values of the rows to update, where each element of the outer array represents a row, and each inner array contains values for
        ///     each of the key columns in 'keyColumns'.
        /// </param>
        /// <param name="columns"> The columns to update. </param>
        /// <param name="values">
        ///     The values for each update, where each element of the outer array represents a row specified in
        ///     'keyValues', and each inner array contains values for each of the columns in 'columns'.
        /// </param>
        /// <param name="schema"> The schema that contains the table, or <see langword="null" /> to use the default schema. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual OperationBuilder<UpdateDataOperation> UpdateData(
            [NotNull] string table,
            [NotNull] string[] keyColumns,
            [NotNull] object[,] keyValues,
            [NotNull] string[] columns,
            [NotNull] object[,] values,
            [CanBeNull] string schema = null)
            => UpdateDataInternal(table, keyColumns, null, keyValues, columns, null, values, schema);

        /// <summary>
        ///     Builds an <see cref="UpdateDataOperation" /> to update multiple rows of seed data for a table with
        ///     a composite (multi-column) key.
        /// </summary>
        /// <param name="table"> The table containing the data to be updated. </param>
        /// <param name="keyColumns"> The names of the key columns used to select the rows to update. </param>
        /// <param name="keyColumnTypes">
        ///     A list of store types for the columns that will be used to identify the rows that should be updated.
        /// </param>
        /// <param name="keyValues">
        ///     The key values of the rows to update, where each element of the outer array represents a row, and each inner array contains values for
        ///     each of the key columns in 'keyColumns'.
        /// </param>
        /// <param name="columns"> The columns to update. </param>
        /// <param name="columnTypes"> A list of store types for the columns in which data will be updated. </param>
        /// <param name="values">
        ///     The values for each update, where each element of the outer array represents a row specified in
        ///     'keyValues', and each inner array contains values for each of the columns in 'columns'.
        /// </param>
        /// <param name="schema"> The schema that contains the table, or <see langword="null" /> to use the default schema. </param>
        /// <returns> A builder to allow annotations to be added to the operation. </returns>
        public virtual OperationBuilder<UpdateDataOperation> UpdateData(
            [NotNull] string table,
            [NotNull] string[] keyColumns,
            [NotNull] string[] keyColumnTypes,
            [NotNull] object[,] keyValues,
            [NotNull] string[] columns,
            [NotNull] string[] columnTypes,
            [NotNull] object[,] values,
            [CanBeNull] string schema = null)
        {
            Check.NotEmpty(keyColumnTypes, nameof(keyColumnTypes));
            Check.NotEmpty(columnTypes, nameof(columnTypes));

            return UpdateDataInternal(table, keyColumns, keyColumnTypes, keyValues, columns, columnTypes, values, schema);
        }

        private OperationBuilder<UpdateDataOperation> UpdateDataInternal(
            string table,
            string[] keyColumns,
            string[] keyColumnTypes,
            object[,] keyValues,
            string[] columns,
            string[] columnTypes,
            object[,] values,
            string schema)
        {
            Check.NotEmpty(table, nameof(table));
            Check.NotNull(keyColumns, nameof(keyColumns));
            Check.NotNull(keyValues, nameof(keyValues));
            Check.NotNull(columns, nameof(columns));
            Check.NotNull(values, nameof(values));

            var operation = new UpdateDataOperation
            {
                Table = table,
                Schema = schema,
                KeyColumns = keyColumns,
                KeyColumnTypes = keyColumnTypes,
                KeyValues = keyValues,
                Columns = columns,
                ColumnTypes = columnTypes,
                Values = values
            };
            Operations.Add(operation);

            return new OperationBuilder<UpdateDataOperation>(operation);
        }

        private static object[,] ToMultidimensionalArray(object[] values, bool firstDimension = false)
        {
            var result = firstDimension
                ? new object[values.Length, 1]
                : new object[1, values.Length];
            for (var i = 0; i < values.Length; i++)
            {
                if (firstDimension)
                {
                    result[i, 0] = values[i];
                }
                else
                {
                    result[0, i] = values[i];
                }
            }

            return result;
        }

        #region Hidden System.Object members

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns> A string that represents the current object. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString()
            => base.ToString();

        /// <summary>
        ///     Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj"> The object to compare with the current object. </param>
        /// <returns> <see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
            => base.Equals(obj);

        /// <summary>
        ///     Serves as the default hash function.
        /// </summary>
        /// <returns> A hash code for the current object. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
            => base.GetHashCode();

        #endregion
    }
}
