// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Metadata.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Storage.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Update.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Migrations
{
    // CHECK: Can we increase the usage of the new model over the old one, or are we done here?
    /// <summary>
    ///     XG-specific implementation of <see cref="MigrationsSqlGenerator" />.
    /// </summary>
    public class XGMigrationsSqlGenerator : MigrationsSqlGenerator
    {
        private const string InternalAnnotationPrefix = XGAnnotationNames.Prefix + "XGMigrationsSqlGenerator:";
        private const string OutputPrimaryKeyConstraintOnAutoIncrementAnnotationName = InternalAnnotationPrefix + "OutputPrimaryKeyConstraint";

        private static readonly Regex _typeRegex = new Regex(@"(?<Name>[a-z0-9]+)\s*?(?:\(\s*(?<Length>\d+)?\s*\))?",
            RegexOptions.IgnoreCase);

        private static readonly HashSet<string> _spatialStoreTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "geometry",
            "point",
            "curve",
            "linestring",
            "line",
            "linearring",
            "surface",
            "polygon",
            "geometrycollection",
            "multipoint",
            "multicurve",
            "multilinestring",
            "multisurface",
            "multipolygon",
        };

        private readonly ICommandBatchPreparer _commandBatchPreparer;
        private readonly IXGOptions _options;
        private readonly RelationalTypeMapping _stringTypeMapping;

        public XGMigrationsSqlGenerator(
            [NotNull] MigrationsSqlGeneratorDependencies dependencies,
            [NotNull] ICommandBatchPreparer commandBatchPreparer,
            [NotNull] IXGOptions options)
            : base(dependencies)
        {
            _commandBatchPreparer = commandBatchPreparer;
            _options = options;
            _stringTypeMapping = dependencies.TypeMappingSource.GetMapping(typeof(string));
        }

        public override IReadOnlyList<MigrationCommand> Generate(
            IReadOnlyList<MigrationOperation> operations,
            IModel model = null,
            MigrationsSqlGenerationOptions options = MigrationsSqlGenerationOptions.Default)
        {
            try
            {
                var filteredOperations = FilterOperations(operations, model);
                var migrationCommands = base.Generate(filteredOperations, model, options);

                return migrationCommands;
            }
            finally
            {
                CleanUpInternalAnnotations(operations);
            }
        }

        private static void CleanUpInternalAnnotations(IReadOnlyList<MigrationOperation> filteredOperations)
        {
            foreach (var filteredOperation in filteredOperations)
            {
                foreach (var annotation in filteredOperation.GetAnnotations().ToList())
                {
                    if (annotation.Name.StartsWith(InternalAnnotationPrefix,StringComparison.Ordinal))
                    {
                        filteredOperation.RemoveAnnotation(annotation.Name);
                    }
                }
            }
        }

        protected virtual IReadOnlyList<MigrationOperation> FilterOperations(IReadOnlyList<MigrationOperation> operations, IModel model)
        {
            if (operations.Count <= 0)
            {
                return operations;
            }

            var filteredOperations = new List<MigrationOperation>();

            var previousOperation = operations.First();
            filteredOperations.Add(previousOperation);

            foreach (var currentOperation in operations.Skip(1))
            {
                // Merge a ColumnOperation immediately followed by an AddPrimaryKeyOperation into a single operation (and SQL statement), if
                // the ColumnOperation is for an AUTO_INCREMENT column. The *immediately followed* restriction could be lifted, if it later
                // turns out to be necessary.
                // MySQL dictates that there can be only one AUTO_INCREMENT column and it has to be a key.
                // If we first add a new column with the AUTO_INCREMENT flag and *then* make it a primary key in the *next* statement, the
                // first statement will fail, because the column is not a key yet, and AUTO_INCREMENT columns have to be keys.
                if (previousOperation is ColumnOperation columnOperation &&
                    currentOperation is AddPrimaryKeyOperation addPrimaryKeyOperation &&
                    addPrimaryKeyOperation.Schema == columnOperation.Schema &&
                    addPrimaryKeyOperation.Table == columnOperation.Table &&
                    addPrimaryKeyOperation.Columns.Length == 1 &&
                    addPrimaryKeyOperation.Columns[0] == columnOperation.Name &&
                    // The following 3 conditions match the ones from `ColumnDefinition()`.
                    XGValueGenerationStrategyCompatibility.GetValueGenerationStrategy(columnOperation.GetAnnotations().OfType<IAnnotation>().ToArray()) is var valueGenerationStrategy &&
                    GetColumBaseTypeAndLength(columnOperation, model) is var (columnBaseType, _) &&
                    IsAutoIncrement(columnOperation, columnBaseType, valueGenerationStrategy))
                {
                    // This internal annotation lets our `ColumnDefinition()` implementation generate a second clause for the primary key
                    // constraint in the same statement.
                    columnOperation[OutputPrimaryKeyConstraintOnAutoIncrementAnnotationName] = true;

                    // We now skip adding the AddPrimaryKeyOperation to the list of operations.
                }
                else
                {
                    filteredOperations.Add(currentOperation);
                }

                previousOperation = currentOperation;
            }

            return filteredOperations.AsReadOnly();
        }

        /// <summary>
        ///     <para>
        ///         Builds commands for the given <see cref="MigrationOperation" /> by making calls on the given
        ///         <see cref="MigrationCommandListBuilder" />.
        ///     </para>
        ///     <para>
        ///         This method uses a double-dispatch mechanism to call one of the 'Generate' methods that are
        ///         specific to a certain subtype of <see cref="MigrationOperation" />. Typically database providers
        ///         will override these specific methods rather than this method. However, providers can override
        ///         this methods to handle provider-specific operations.
        ///     </para>
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null"/> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(MigrationOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));
            CheckSchema(operation);

            switch (operation)
            {
                case XGCreateDatabaseOperation createDatabaseOperation:
                    Generate(createDatabaseOperation, model, builder);
                    break;
                case XGDropDatabaseOperation dropDatabaseOperation:
                    Generate(dropDatabaseOperation, model, builder);
                    break;
                case XGDropPrimaryKeyAndRecreateForeignKeysOperation dropPrimaryKeyAndRecreateForeignKeysOperation:
                    Generate(dropPrimaryKeyAndRecreateForeignKeysOperation, model, builder);
                    break;
                case XGDropUniqueConstraintAndRecreateForeignKeysOperation dropUniqueConstraintAndRecreateForeignKeysOperation:
                    Generate(dropUniqueConstraintAndRecreateForeignKeysOperation, model, builder);
                    break;
                default:
                    base.Generate(operation, model, builder);
                    break;
            }
        }

        protected virtual void CheckSchema(MigrationOperation operation)
        {
            if (_options.SchemaNameTranslator != null)
            {
                return;
            }

            var schema = operation.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty)
                .Where(p => p.Name.IndexOf(nameof(AddForeignKeyOperation.Schema), StringComparison.Ordinal) >= 0)
                .Select(p => p.GetValue(operation) as string)
                .FirstOrDefault(schemaValue => schemaValue != null);

            if (schema != null)
            {
                var name = operation.GetType()
                    .GetProperty(nameof(AddForeignKeyOperation.Name), BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty)
                    ?.GetValue(operation) as string;

                throw new InvalidOperationException($"A schema \"{schema}\" has been set for an object of type \"{operation.GetType().Name}\"{(string.IsNullOrEmpty(name) ? string.Empty : $" with the name of \"{name}\"")}. MySQL does not support the EF Core concept of schemas. Any schema property of any \"MigrationOperation\" must be null. This behavior can be changed by setting the `SchemaBehavior` option in the `UseXG` call.");
            }
        }

        protected override void Generate(
            [NotNull] CreateTableOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder,
            bool terminate = true)
        {
            // Create a unique constraint for an AUTO_INCREMENT column that is part of a compound primary key, but is not the first column
            // in that key. In this case, for InnoDB to be satisfied, an index (preferably UNIQUE) with the AUTO_INCREMENT column as the first
            // column, has to exists.
            //
            // TODO: Only add the column, if there is not an already existing index that has the AUTO_INCREMENT column as the first index.
            //       We should also monitor all related operations, to remove the index, if it is not needed anymore.
            //       Check/test, whether this does not only apply to primary keys, but also to other (alternate) ones as well, or is
            //       completely independent of keys, and really just applies to any AUTO_INCREMENT column.
            //       Also, move handling to conventions.
            if (operation.PrimaryKey is { Columns.Length: > 1 } primaryKey &&
                primaryKey.Columns[0] is var firstPrimaryKeyColumnName &&
                operation.Columns.Single(c => c.Name == firstPrimaryKeyColumnName) is var firstPrimaryKeyColumn &&
                operation.Columns.FirstOrDefault(c => c[XGAnnotationNames.ValueGenerationStrategy] is XGValueGenerationStrategy.IdentityColumn) is { } autoIncrementColumn &&
                operation.Columns.Contains(autoIncrementColumn) &&
                autoIncrementColumn != firstPrimaryKeyColumn)
            {
                operation.UniqueConstraints.Add(
                    new AddUniqueConstraintOperation
                    {
                        Schema = operation.PrimaryKey.Schema,
                        Table = operation.PrimaryKey.Table,
                        Columns = [autoIncrementColumn.Name],
                    });
            }

            base.Generate(operation, model, builder, false);

            var tableOptions = new List<(string, string)>();

            if (operation[XGAnnotationNames.CharSet] is string charSet)
            {
                tableOptions.Add(("CHARACTER SET", charSet));
            }

            if (operation[RelationalAnnotationNames.Collation] is string collation)
            {
                tableOptions.Add(("COLLATE", collation));
            }

            if (operation.Comment != null)
            {
                tableOptions.Add(("COMMENT", XGStringTypeMapping.EscapeSqlLiteralWithLineBreaks(operation.Comment, !_options.NoBackslashEscapes, false)));
            }

            tableOptions.AddRange(
                XGEntityTypeExtensions.DeserializeTableOptions(operation[XGAnnotationNames.StoreOptions] as string)
                    .Select(kvp => (kvp.Key, kvp.Value)));

            foreach (var (key, value) in tableOptions)
            {
                builder
                    .Append(" ")
                    .Append(key)
                    .Append("=")
                    .Append(value);
            }

            if (terminate)
            {
                builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
                EndStatement(builder);
            }
        }

        protected override void Generate(AlterTableOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            var oldCharSet = operation.OldTable[XGAnnotationNames.CharSet] as string;
            var newCharSet = operation[XGAnnotationNames.CharSet] as string;

            var oldCollation = operation.OldTable[RelationalAnnotationNames.Collation] as string;
            var newCollation = operation[RelationalAnnotationNames.Collation] as string;

            // Collations are more specific than charsets. So if a collation has been set, we use the collation instead of the charset.
            if (newCollation != oldCollation &&
                newCollation != null)
            {
                // A new collation has been set. It takes precedence over any defined charset.
                builder
                    .Append("ALTER TABLE ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema))
                    .Append(" COLLATE=")
                    .Append(newCollation)
                    .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

                EndStatement(builder);
            }
            else if (newCharSet != oldCharSet ||
                     newCollation != oldCollation && newCollation == null)
            {
                // The charset has been changed or the collation has been reset to the default.
                if (newCharSet != null)
                {
                    // A new charset has been set without an explicit collation.
                    builder
                        .Append("ALTER TABLE ")
                        .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema))
                        .Append(" CHARACTER SET=")
                        .Append(newCharSet)
                        .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

                    EndStatement(builder);
                }
                else
                {
                    var collationColumnName = _options.ServerVersion.Supports.CollationCharacterSetApplicabilityWithFullCollationNameColumn
                        ? "FULL_COLLATION_NAME"
                        : "COLLATION_NAME";

                    // The charset (and any collation) has been reset to the default.
                    var resetCharSetSql = $"""
set @__pomelo_TableCharset = (
SELECT `ccsa`.`CHARACTER_SET_NAME` as `TABLE_CHARACTER_SET`
FROM `INFORMATION_SCHEMA`.`TABLES` as `t`
LEFT JOIN `INFORMATION_SCHEMA`.`COLLATION_CHARACTER_SET_APPLICABILITY` as `ccsa` ON `ccsa`.`{collationColumnName}` = `t`.`TABLE_COLLATION`
WHERE `TABLE_SCHEMA` = SCHEMA() AND `TABLE_NAME` = {_stringTypeMapping.GenerateSqlLiteral(operation.Name)} AND `TABLE_TYPE` IN ('BASE TABLE', 'VIEW'));

SET @__pomelo_SqlExpr = CONCAT('ALTER TABLE {Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name)} CHARACTER SET = ', @__pomelo_TableCharset, ';');
PREPARE __pomelo_SqlExprExecute FROM @__pomelo_SqlExpr;
EXECUTE __pomelo_SqlExprExecute;
DEALLOCATE PREPARE __pomelo_SqlExprExecute;
""";

                    builder.AppendLine(resetCharSetSql);
                    EndStatement(builder);
                }
            }

            if (operation.Comment != operation.OldTable.Comment)
            {
                builder.Append("ALTER TABLE ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema));

                // An existing comment will be removed, when set to an empty string.
                GenerateComment(operation.Comment ?? string.Empty, builder);

                builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
                EndStatement(builder);
            }

            var oldTableOptions = XGEntityTypeExtensions.DeserializeTableOptions(operation.OldTable[XGAnnotationNames.StoreOptions] as string);
            var newTableOptions = XGEntityTypeExtensions.DeserializeTableOptions(operation[XGAnnotationNames.StoreOptions] as string);
            var addedOrChangedTableOptions = newTableOptions.Except(oldTableOptions).ToArray();

            if (addedOrChangedTableOptions.Length > 0)
            {
                builder
                    .Append("ALTER TABLE ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema));

                foreach (var (key, value) in addedOrChangedTableOptions)
                {
                    builder
                        .Append(" ")
                        .Append(key)
                        .Append("=")
                        .Append(value);
                }

                builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
                EndStatement(builder);
            }
        }

        /// <summary>
        ///     Builds commands for the given <see cref="AlterColumnOperation" />
        ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null"/> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(
            AlterColumnOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("ALTER TABLE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                .Append(" MODIFY COLUMN ");

            ColumnDefinition(
                operation.Schema,
                operation.Table,
                operation.Name,
                operation,
                model,
                builder);

            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            builder.EndCommand();
        }

        /// <summary>
        ///     Builds commands for the given <see cref="RenameIndexOperation" />
        ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null"/> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(
            RenameIndexOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            if (string.IsNullOrEmpty(operation.Table))
            {
                throw new InvalidOperationException(XGStrings.IndexTableRequired);
            }

            if (operation.NewName != null)
            {
                if (_options.ServerVersion.Supports.RenameIndex)
                {
                    builder.Append("ALTER TABLE ")
                        .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                        .Append(" RENAME INDEX ")
                        .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                        .Append(" TO ")
                        .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.NewName))
                        .AppendLine(";");

                    EndStatement(builder);
                }
                else
                {
                    var index = model?
                        .GetRelationalModel()
                        .FindTable(operation.Table, operation.Schema)
                        ?.Indexes
                        .FirstOrDefault(i => i.Name == operation.NewName);

                    if (index == null)
                    {
                        throw new InvalidOperationException(
                            $"Could not find the model index: {Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema)}.{Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.NewName)}. Upgrade to Mysql 5.7+ or split the 'RenameIndex' call into 'DropIndex' and 'CreateIndex'");
                    }

                    Generate(new DropIndexOperation
                    {
                        Schema = operation.Schema,
                        Table = operation.Table,
                        Name = operation.Name
                    }, model, builder);

                    var createIndexOperation = CreateIndexOperation.CreateFrom(index);
                    createIndexOperation.Name = operation.NewName;

                    Generate(createIndexOperation, model, builder);
                }
            }
        }

        /// <summary>
        ///     Builds commands for the given <see cref="RestartSequenceOperation" /> by making calls on the given
        ///     <see cref="MigrationCommandListBuilder" />, and then terminates the final command.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null"/> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(
            RestartSequenceOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            if (!_options.ServerVersion.Supports.Sequences)
            {
                throw new InvalidOperationException(
                    $"Cannot restart sequence '{operation.Name}' because sequences are not supported in server version {_options.ServerVersion}.");
            }

            builder
                .Append("ALTER SEQUENCE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema));

            if (operation.StartValue.HasValue)
            {
                builder
                    .Append(" START WITH ")
                    .Append(IntegerConstant(operation.StartValue));
            }

            builder
                .Append(" RESTART")
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

            EndStatement(builder);
        }

        /// <summary>
        ///     Builds commands for the given <see cref="RenameTableOperation" />
        ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null"/> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(
            RenameTableOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("ALTER TABLE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema))
                .Append(" RENAME ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.NewName, operation.NewSchema))
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

            EndStatement(builder);
        }

        /// <summary>
        ///     Builds commands for the given <see cref="CreateIndexOperation" /> by making calls on the given
        ///     <see cref="MigrationCommandListBuilder" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null"/> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        /// <param name="terminate"> Indicates whether or not to terminate the command after generating SQL for the operation. </param>
        protected override void Generate(
            CreateIndexOperation operation,
            IModel model,
            MigrationCommandListBuilder builder,
            bool terminate = true)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            if (!_options.ServerVersion.Supports.SpatialIndexes &&
                operation[XGAnnotationNames.SpatialIndex] is true)
            {
                Dependencies.MigrationsLogger.Logger.LogWarning(
                    $"Spatial indexes are not supported on {_options.ServerVersion}. The CREATE INDEX operation will be ignored.");
                return;
            }

            builder.Append("CREATE ");

            if (operation.IsUnique)
            {
                builder.Append("UNIQUE ");
            }

            IndexTraits(operation, model, builder);

            builder
                .Append("INDEX ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(Truncate(operation.Name, 64)))
                .Append(" ON ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                .Append(" (")
                .Append(ColumnListWithIndexPrefixLengthAndSortOrder(operation, operation.Columns, operation[XGAnnotationNames.IndexPrefixLength] as int[], operation.IsDescending))
                .Append(")");

            IndexOptions(operation, model, builder);

            if (terminate)
            {
                builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
                EndStatement(builder);
            }
        }

        /// /// <summary>
        ///     Ignored, since schemas are not supported by MySQL and are silently ignored to improve testing compatibility.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null"/> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(EnsureSchemaOperation operation, IModel model,
            MigrationCommandListBuilder builder)
        {
        }

        /// <summary>
        ///     Ignored, since schemas are not supported by MySQL and are silently ignored to improve testing compatibility.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null"/> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(DropSchemaOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
        }

        /// <summary>
        ///     Builds commands for the given <see cref="CreateSequenceOperation" /> by making calls on the given
        ///     <see cref="MigrationCommandListBuilder" />, and then terminates the final command.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null"/> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(
            [NotNull] CreateSequenceOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            if (!_options.ServerVersion.Supports.Sequences)
            {
                throw new InvalidOperationException(
                    $"Cannot create sequence '{operation.Name}' because sequences are not supported in server version {_options.ServerVersion}.");
            }

            // "CREATE SEQUENCE"  supported only in MariaDb from 10.3.
            // However, "CREATE SEQUENCE name AS type" expression is currently not supported.
            // The base MigrationsSqlGenerator.Generate method generates that expression.
            // Also, when creating a sequence current version of MariaDb doesn't tolerate "NO MINVALUE"
            // when specifying "STARTS WITH" so, StartValue mus be set accordingly.
            // https://github.com/aspnet/EntityFrameworkCore/blob/master/src/EFCore.Relational/Migrations/MigrationsSqlGenerator.cs#L535-L543
            var oldValue = operation.ClrType;
            operation.ClrType = typeof(long);
            if (operation.StartValue <= 0)
            {
                operation.MinValue = operation.StartValue;
            }
            base.Generate(operation, model, builder);
            operation.ClrType = oldValue;
        }

        protected override void Generate(AlterSequenceOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            if (!_options.ServerVersion.Supports.Sequences)
            {
                throw new InvalidOperationException(
                    $"Cannot alter sequence '{operation.Name}' because sequences are not supported in server version {_options.ServerVersion}.");
            }

            base.Generate(operation, model, builder);
        }

        protected override void Generate(DropSequenceOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            if (!_options.ServerVersion.Supports.Sequences)
            {
                throw new InvalidOperationException(
                    $"Cannot alter sequence '{operation.Name}' because sequences are not supported in server version {_options.ServerVersion}.");
            }

            base.Generate(operation, model, builder);
        }

        protected override void Generate(RenameSequenceOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            if (!_options.ServerVersion.Supports.Sequences)
            {
                throw new InvalidOperationException(
                    $"Cannot alter sequence '{operation.Name}' because sequences are not supported in server version {_options.ServerVersion}.");
            }

            builder
                .Append("ALTER TABLE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema))
                .Append(" RENAME ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.NewName, operation.NewSchema))
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

            EndStatement(builder);
        }

        /// <summary>
        ///     Builds commands for the given <see cref="XGCreateDatabaseOperation" />
        ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null"/> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected virtual void Generate(
            [NotNull] XGCreateDatabaseOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("CREATE DATABASE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name));

            if (operation.CharSet != null)
            {
                builder
                    .Append(" CHARACTER SET ")
                    .Append(operation.CharSet);
            }

            if (operation.Collation != null)
            {
                builder
                    .Append(" COLLATE ")
                    .Append(operation.Collation);
            }

            builder
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
                .EndCommand();
        }

        /// <summary>
        ///     Builds commands for the given <see cref="XGDropDatabaseOperation" />
        ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null"/> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected virtual void Generate(
            [NotNull] XGDropDatabaseOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("DROP DATABASE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                .Append(Dependencies.SqlGenerationHelper.StatementTerminator)
                .AppendLine(Dependencies.SqlGenerationHelper.BatchTerminator);
            EndStatement(builder);
        }

        protected override void Generate(AlterDatabaseOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            // Also at this point, all explicitly added `Relational:Collation` annotations (through delegation) should have been set to the
            // `Collation` property and removed.
            Debug.Assert(operation.FindAnnotation(RelationalAnnotationNames.Collation) == null);

            var oldCharSet = operation.OldDatabase[XGAnnotationNames.CharSet] as string;
            var newCharSet = operation[XGAnnotationNames.CharSet] as string;

            var oldCollation = operation.OldDatabase.Collation;
            var newCollation = operation.Collation;

            // Collations are more specific than charsets. So if a collation has been set, we use the collation instead of the charset.
            if (newCollation != oldCollation &&
                newCollation != null)
            {
                // A new collation has been set. It takes precedence over any defined charset.
                builder
                    .Append("ALTER DATABASE COLLATE ")
                    .Append(newCollation)
                    .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

                EndStatement(builder);
            }
            else if (newCharSet != oldCharSet ||
                     newCollation != oldCollation && newCollation == null)
            {
                // The charset has been changed or the collation has been reset to the default.
                if (newCharSet != null)
                {
                    // A new charset has been set without an explicit collation.
                    builder
                        .Append("ALTER DATABASE CHARACTER SET ")
                        .Append(newCharSet)
                        .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

                    EndStatement(builder);
                }
                else
                {
                    Dependencies.MigrationsLogger.Logger.LogWarning(@"ALTER DATABASE operations can currently not implicitly reset the character set AND the collation to the server default values. Please explicitly specify a character set, a collation or both.");
                }
            }
        }

        protected override void Generate(
            [NotNull] DropIndexOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder,
            bool terminate = true)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            if (string.IsNullOrEmpty(operation.Table))
            {
                throw new InvalidOperationException(XGStrings.IndexTableRequired);
            }

            builder
                .Append("ALTER TABLE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                .Append(" DROP INDEX ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name));

            if (terminate)
            {
                builder
                    .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
                    .EndCommand();
            }
        }

        protected override void Generate(
            DropUniqueConstraintOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
            => Generate(
                new XGDropUniqueConstraintAndRecreateForeignKeysOperation
                {
                    IsDestructiveChange = operation.IsDestructiveChange,
                    Name = operation.Name,
                    Schema = operation.Schema,
                    Table = operation.Table,
                    RecreateForeignKeys = false,
                },
                model,
                builder);

        protected virtual void Generate(
            XGDropUniqueConstraintAndRecreateForeignKeysOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            void DropUniqueKey()
            {
                builder.Append("ALTER TABLE ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                    .Append(" DROP KEY ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                    .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

                EndStatement(builder);
            }

            // A foreign key might reuse the alternate key for its own purposes and prohibit its deletion,
            // if the foreign key columns are listed as the first columns and in the same order as in the foreign key (#678).
            // We therefore drop and later recreate all foreign keys to ensure, that no other dependencies on the
            // alternate key exist, if explicitly requested by the user via `XGMigrationBuilderExtensions.DropUniqueConstraint()`.
            // This particularily targets FK contraints, that are on the same table as the PK and might reuse indexes already used by the PK.
            // A common case is a many-to-many relationship table, with a PK containing the 2 FK columns.
            if (operation.RecreateForeignKeys)
            {
                TemporarilyDropForeignKeys(
                    model,
                    builder,
                    operation.Schema,
                    operation.Table,
                    DropUniqueKey);
            }
            else
            {
                DropUniqueKey();
            }
        }

        protected virtual void TemporarilyDropForeignKeys(
            IModel model,
            MigrationCommandListBuilder builder,
            string schemaName,
            string tableName,
            Action action)
        {
            var foreignKeys = model.GetRelationalModel()
                .FindTable(tableName, schemaName)
                ?.ForeignKeyConstraints
                .ToArray() ?? Array.Empty<IForeignKeyConstraint>();

            foreach (var foreignKey in foreignKeys)
            {
                Generate(new DropForeignKeyOperation
                {
                    Schema = foreignKey.Table.Schema,
                    Table = foreignKey.Table.Name,
                    Name = foreignKey.Name,
                }, model, builder);
            }

            action();

            foreach (var foreignKey in foreignKeys)
            {
                Generate(new AddForeignKeyOperation
                {
                    Schema = foreignKey.Table.Schema,
                    Table = foreignKey.Table.Name,
                    Name = foreignKey.Name,
                    Columns = foreignKey.Columns.Select(c => c.Name).ToArray(),
                    PrincipalSchema = foreignKey.PrincipalTable.Schema,
                    PrincipalTable = foreignKey.PrincipalTable.Name,
                    PrincipalColumns = foreignKey.PrincipalColumns.Select(c => c.Name).ToArray(),
                    OnDelete = foreignKey.OnDeleteAction,
                }, model, builder);
            }
        }

        protected static ReferentialAction ToReferentialAction(DeleteBehavior deleteBehavior)
        {
            switch (deleteBehavior)
            {
                case DeleteBehavior.SetNull:
                    return ReferentialAction.SetNull;
                case DeleteBehavior.Cascade:
                    return ReferentialAction.Cascade;
                case DeleteBehavior.NoAction:
                case DeleteBehavior.ClientNoAction:
                    return ReferentialAction.NoAction;
                default:
                    return ReferentialAction.Restrict;
            }
        }

        /// <summary>
        ///     Builds commands for the given <see cref="DropForeignKeyOperation" /> by making calls on the given
        ///     <see cref="MigrationCommandListBuilder" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null"/> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        /// <param name="terminate"> Indicates whether or not to terminate the command after generating SQL for the operation. </param>
        protected override void Generate(
            [NotNull] DropForeignKeyOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder,
            bool terminate)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("ALTER TABLE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                .Append(" DROP FOREIGN KEY ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name));

            if (terminate)
            {
                builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
                EndStatement(builder);
            }
        }

        // CHECK: Can we improve this implementation?
        /// <summary>
        ///     Builds commands for the given <see cref="RenameColumnOperation" />
        ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null"/> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(
            RenameColumnOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.Append("ALTER TABLE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema));

            if (_options.ServerVersion.Supports.RenameColumn)
            {
                builder.Append(" RENAME COLUMN ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                    .Append(" TO ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.NewName))
                    .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

                EndStatement(builder);
                return;
            }

            builder.Append(" CHANGE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                .Append(" ");

            var column = model?.GetRelationalModel().FindTable(operation.Table, operation.Schema)?.FindColumn(operation.NewName);
            if (column is null)
            {
                throw new InvalidOperationException(
                    $"The column '{Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema)}.{Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.NewName)}' could not be found in the target model. Make sure the table name exists in the target model and check the order of all migration operations. Generally, rename tables first, then columns.");
            }

            var columnType = (string)(operation[RelationalAnnotationNames.ColumnType] ??
                                      column[RelationalAnnotationNames.ColumnType] ??
                                      column.StoreType);

            var typeMapping = column.PropertyMappings.FirstOrDefault()?.TypeMapping;
            var converter = typeMapping?.Converter;
            var defaultValue = converter != null
                ? converter.ConvertToProvider(column.DefaultValue)
                : column.DefaultValue;

            var addColumnOperation = new AddColumnOperation
            {
                Schema = operation.Schema,
                Table = operation.Table,
                Name = operation.NewName,
                ClrType = (converter?.ProviderClrType ?? typeMapping?.ClrType).UnwrapNullableType(),
                ColumnType = columnType,
                IsUnicode = column.IsUnicode,
                MaxLength = column.MaxLength,
                IsFixedLength = column.IsFixedLength,
                IsRowVersion = column.IsRowVersion,
                IsNullable = column.IsNullable,
                DefaultValue = defaultValue,
                DefaultValueSql = column.DefaultValueSql,
                ComputedColumnSql = column.ComputedColumnSql,
                IsStored = column.IsStored,
            };

            ColumnDefinition(
                addColumnOperation,
                model,
                builder);
            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            EndStatement(builder);
        }

        protected override void SequenceOptions(
            string schema,
            string name,
            SequenceOperation operation,
            IModel model,
            MigrationCommandListBuilder builder,
            bool forAlter)
        {
            var intTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(int));
            var longTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(long));

            builder
                .Append(" INCREMENT BY ")
                .Append(intTypeMapping.GenerateSqlLiteral(operation.IncrementBy));

            if (operation.MinValue != null)
            {
                builder
                    .Append(" MINVALUE ")
                    .Append(longTypeMapping.GenerateSqlLiteral(operation.MinValue));
            }
            else if (forAlter)
            {
                builder
                    .Append(" NO MINVALUE");
            }

            if (operation.MaxValue != null)
            {
                builder
                    .Append(" MAXVALUE ")
                    .Append(longTypeMapping.GenerateSqlLiteral(operation.MaxValue));
            }
            else if (forAlter)
            {
                builder
                    .Append(" NO MAXVALUE");
            }

            builder.Append(operation.IsCyclic ? " CYCLE" : " NOCYCLE");
        }

        /// <summary>
        ///     Generates a SQL fragment for a column definition in an <see cref="AddColumnOperation" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null"/> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to add the SQL fragment. </param>
        protected override void ColumnDefinition(AddColumnOperation operation, IModel model,
            MigrationCommandListBuilder builder)
            => ColumnDefinition(
                operation.Schema,
                operation.Table,
                operation.Name,
                operation,
                model,
                builder);

        /// <summary>
        ///     Generates a SQL fragment for a column definition for the given column metadata.
        /// </summary>
        /// <param name="schema"> The schema that contains the table, or <see langword="null"/> to use the default schema. </param>
        /// <param name="table"> The table that contains the column. </param>
        /// <param name="name"> The column name. </param>
        /// <param name="operation"> The column metadata. </param>
        /// <param name="model"> The target model which may be <see langword="null"/> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to add the SQL fragment. </param>
        protected override void ColumnDefinition(
            [CanBeNull] string schema,
            [NotNull] string table,
            [NotNull] string name,
            [NotNull] ColumnOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            var (matchType, matchLen) = GetColumBaseTypeAndLength(schema, table, name, operation, model);
            var valueGenerationStrategy = XGValueGenerationStrategyCompatibility.GetValueGenerationStrategy(operation.GetAnnotations().OfType<IAnnotation>().ToArray());
            var autoIncrement = IsAutoIncrement(operation, matchType, valueGenerationStrategy);

            if (!autoIncrement &&
                valueGenerationStrategy == XGValueGenerationStrategy.IdentityColumn &&
                string.IsNullOrWhiteSpace(operation.DefaultValueSql))
            {
                switch (matchType)
                {
                    case "datetime":
                        if (!_options.ServerVersion.Supports.DateTimeCurrentTimestamp)
                        {
                            throw new InvalidOperationException(
                                $"Error in {table}.{name}: DATETIME does not support values generated " +
                                $"on Add or Update in server version {_options.ServerVersion}. Try explicitly setting the column type to TIMESTAMP.");
                        }
                        goto case "timestamp";

                    case "timestamp":
                        operation.DefaultValueSql = $"CURRENT_TIMESTAMP({matchLen})";
                        break;
                }
            }

            string onUpdateSql = null;
            if (operation.IsRowVersion || valueGenerationStrategy == XGValueGenerationStrategy.ComputedColumn)
            {
                switch (matchType)
                {
                    case "datetime":
                        if (!_options.ServerVersion.Supports.DateTimeCurrentTimestamp)
                        {
                            throw new InvalidOperationException(
                                $"Error in {table}.{name}: DATETIME does not support values generated " +
                                $"on Add or Update in server version {_options.ServerVersion}. Try explicitly setting the column type to TIMESTAMP.");
                        }

                        goto case "timestamp";
                    case "timestamp":
                        if (string.IsNullOrWhiteSpace(operation.DefaultValueSql) && operation.DefaultValue == null)
                        {
                            operation.DefaultValueSql = $"CURRENT_TIMESTAMP({matchLen})";
                        }

                        onUpdateSql = $"CURRENT_TIMESTAMP({matchLen})";
                        break;
                }
            }

            if (operation.ComputedColumnSql == null)
            {
                // AUTO_INCREMENT columns don't support DEFAULT values.
                ColumnDefinitionWithCharSet(schema, table, name, operation, model, builder, withDefaultValue: !autoIncrement);

                GenerateComment(operation.Comment, builder);

                // AUTO_INCREMENT has priority over reference definitions.
                if (autoIncrement)
                {
                    builder.Append(" AUTO_INCREMENT");

                    // TODO: Add support for a non-primary key that is used as with auto_increment.
                    if (model?.GetRelationalModel().FindTable(table, schema) is { PrimaryKey: { Columns.Count: 1 } primaryKey } &&
                        primaryKey.Columns[0].Name == operation.Name &&
                        (bool?)operation[OutputPrimaryKeyConstraintOnAutoIncrementAnnotationName] == true)
                    {
                        builder
                            .AppendLine(",")
                            .Append("ADD ");

                        PrimaryKeyConstraint(
                            AddPrimaryKeyOperation.CreateFrom(primaryKey),
                            model,
                            builder);
                    }
                }
                else if (onUpdateSql != null)
                {
                    builder
                        .Append(" ON UPDATE ")
                        .Append(onUpdateSql);
                }
            }
            else
            {
                builder
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(name))
                    .Append(" ")
                    .Append(GetColumnType(schema, table, name, operation, model));
                builder
                    .Append(" AS ")
                    .Append($"({operation.ComputedColumnSql})");

                if (operation.IsStored.GetValueOrDefault())
                {
                    builder.Append(" STORED");
                }

                if (operation.IsNullable && _options.ServerVersion.Supports.NullableGeneratedColumns)
                {
                    builder.Append(" NULL");
                }

                GenerateComment(operation.Comment, builder);
            }
        }

        protected virtual (string matchType, string matchLen) GetColumBaseTypeAndLength(
            ColumnOperation operation,
            IModel model)
            => GetColumBaseTypeAndLength(operation.Schema, operation.Table, operation.Name, operation, model);

        protected virtual (string matchType, string matchLen) GetColumBaseTypeAndLength(
            string schema,
            string table,
            string name,
            ColumnOperation operation,
            IModel model)
        {
            var matchType = GetColumnType(schema, table, name, operation, model);
            var matchLen = "";
            var match = _typeRegex.Match(matchType ?? "-");
            if (match.Success)
            {
                matchType = match.Groups["Name"].Value.ToLower();
                if (match.Groups["Length"].Success)
                {
                    matchLen = match.Groups["Length"].Value;
                }
            }

            return (matchType, matchLen);
        }

        protected virtual bool IsAutoIncrement(ColumnOperation operation,
            string columnType,
            XGValueGenerationStrategy? valueGenerationStrategy)
        {
            if (valueGenerationStrategy == XGValueGenerationStrategy.IdentityColumn &&
                string.IsNullOrWhiteSpace(operation.DefaultValueSql))
            {
                switch (columnType)
                {
                    case "tinyint":
                    case "smallint":
                    case "mediumint":
                    case "int":
                    case "bigint":
                        return true;
                }
            }

            return false;
        }

        private void GenerateComment(string comment, MigrationCommandListBuilder builder)
        {
            if (comment == null)
            {
                return;
            }

            builder.Append(" COMMENT ")
                .Append(XGStringTypeMapping.EscapeSqlLiteralWithLineBreaks(comment, !_options.NoBackslashEscapes, false));
        }

        private void ColumnDefinitionWithCharSet(
            string schema,
            string table,
            string name,
            ColumnOperation operation,
            IModel model,
            MigrationCommandListBuilder builder,
            bool withDefaultValue)
        {
            if (operation.ComputedColumnSql != null)
            {
                ComputedColumnDefinition(schema, table, name, operation, model, builder);
                return;
            }

            var columnType = GetColumnType(schema, table, name, operation, model);

            builder
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(name))
                .Append(" ")
                .Append(columnType);

            builder.Append(operation.IsNullable ? " NULL" : " NOT NULL");

            if (withDefaultValue)
            {
                DefaultValue(operation.DefaultValue, operation.DefaultValueSql, columnType, builder);
            }

            var srid = operation[XGAnnotationNames.SpatialReferenceSystemId];
            if (srid is int &&
                IsSpatialStoreType(columnType))
            {
                builder.Append($" /*!80003 SRID {srid} */");
            }
        }

        protected override string GetColumnType(string schema, string table, string name, ColumnOperation operation, IModel model)
            => GetColumnTypeWithCharSetAndCollation(
                operation,
                operation.ColumnType ?? base.GetColumnType(schema, table, name, operation, model));

        private static string GetColumnTypeWithCharSetAndCollation(ColumnOperation operation, string columnType)
        {
            if (columnType.IndexOf("json", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return columnType;
            }

            var charSet = operation[XGAnnotationNames.CharSet];
            if (charSet != null)
            {
                const string characterSetClausePattern = @"(CHARACTER SET|CHARSET)\s+\w+";
                var characterSetClause = $@"CHARACTER SET {charSet}";

                columnType = Regex.IsMatch(columnType, characterSetClausePattern, RegexOptions.IgnoreCase)
                    ? Regex.Replace(columnType, characterSetClausePattern, characterSetClause)
                    : columnType.TrimEnd() + " " + characterSetClause;
            }

            // At this point, all legacy `XG:Collation` annotations should have been replaced by `Relational:Collation` ones.
#pragma warning disable 618
            Debug.Assert(operation.FindAnnotation(XGAnnotationNames.Collation) == null);
#pragma warning restore 618

            // Also at this point, all explicitly added `Relational:Collation` annotations (through delegation) should have been set to the
            // `Collation` property and removed.
            Debug.Assert(operation.FindAnnotation(RelationalAnnotationNames.Collation) == null);

            // If we set the collation through delegation, we use the `Relational:Collation` annotation, so the collation will not be in the
            // `Collation` property.
            var collation = operation.Collation;
            if (collation != null)
            {
                const string collationClausePattern = @"COLLATE \w+";
                var collationClause = $@"COLLATE {collation}";

                columnType = Regex.IsMatch(columnType, collationClausePattern, RegexOptions.IgnoreCase)
                    ? Regex.Replace(columnType, collationClausePattern, collationClause)
                    : columnType.TrimEnd() + " " + collationClause;
            }

            return columnType;
        }

        protected override void DefaultValue(
            object defaultValue,
            string defaultValueSql,
            string columnType,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            if (defaultValueSql is not null)
            {
                if (IsDefaultValueSqlSupported(defaultValueSql, columnType))
                {
                    builder
                        .Append(" DEFAULT ")
                        .Append(defaultValueSql);
                }
                else
                {
                    Dependencies.MigrationsLogger.DefaultValueNotSupportedWarning(defaultValueSql, _options.ServerVersion, columnType);
                }
            }
            else if (defaultValue is not null)
            {
                var isDefaultValueSupported = IsDefaultValueSupported(columnType);
                var supportsDefaultExpressionSyntax = _options.ServerVersion.Supports.DefaultExpression ||
                                                      _options.ServerVersion.Supports.AlternativeDefaultExpression;

                var typeMapping = Dependencies.TypeMappingSource.GetMappingForValue(defaultValue);

                if (typeMapping is IDefaultValueCompatibilityAware defaultValueCompatibilityAware)
                {
                    typeMapping = defaultValueCompatibilityAware.Clone(isDefaultValueCompatible: true);
                }

                var sqlLiteralDefaultValue = typeMapping.GenerateSqlLiteral(defaultValue);

                if (isDefaultValueSupported ||
                    supportsDefaultExpressionSyntax)
                {
                    var useDefaultExpressionSyntax = !isDefaultValueSupported;

                    builder.Append(" DEFAULT ");

                    if (useDefaultExpressionSyntax)
                    {
                        builder.Append("(");
                    }

                    builder.Append(sqlLiteralDefaultValue);

                    if (useDefaultExpressionSyntax)
                    {
                        builder.Append(")");
                    }
                }
                else
                {
                    Dependencies.MigrationsLogger.DefaultValueNotSupportedWarning(
                        sqlLiteralDefaultValue,
                        _options.ServerVersion,
                        columnType);
                }
            }
        }

        private bool IsDefaultValueSqlSupported(string defaultValueSql, string columnType)
        {
            if (IsDefaultValueSupported(columnType))
            {
                return true;
            }

            var trimmedDefaultValueSql = defaultValueSql.Trim();

            if (_options.ServerVersion.Supports.DefaultExpression)
            {
                if (trimmedDefaultValueSql.StartsWith("(", StringComparison.Ordinal) && trimmedDefaultValueSql.EndsWith(")", StringComparison.Ordinal))
                {
                    return true;
                }
            }
            else if (_options.ServerVersion.Supports.AlternativeDefaultExpression)
            {
                if ((trimmedDefaultValueSql.EndsWith("()", StringComparison.Ordinal) && !trimmedDefaultValueSql.StartsWith("(", StringComparison.Ordinal)) ||
                    (trimmedDefaultValueSql.StartsWith("(", StringComparison.Ordinal) && trimmedDefaultValueSql.EndsWith(")", StringComparison.Ordinal)))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Generates a SQL fragment for the primary key constraint of a <see cref="CreateTableOperation" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null"/> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to add the SQL fragment. </param>
        protected override void CreateTablePrimaryKeyConstraint(
            [NotNull] CreateTableOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            // We used to move an AUTO_INCREMENT column to the first position in a primary key, if the PK was a compound key and the column
            // was not in the first position. We did this to satisfy InnoDB.
            // However, this is technically an inaccuracy, and leads to incompatible FK -> PK mappings in MySQL 8.4.
            // We will therefore reverse that behavior to leaving the key order unchanged again.
            // This will lead to two issues:
            //     - Migrations that upgrade vom Pomelo < 9.0 to Pomelo 9.0 will not include this change automatically, because the model
            //       never changed (we only made the change (before and now) here in XGMigrationsSqlGenerator).
            //     - There now needs to be an index for those cases, that contains the AUTO_INCREMENT column as its first column.

            base.CreateTablePrimaryKeyConstraint(operation, model, builder);
        }

        protected override void PrimaryKeyConstraint(
            [NotNull] AddPrimaryKeyOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            if (operation.Name != null)
            {
                builder
                    .Append("CONSTRAINT ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                    .Append(" ");
            }

            builder
                .Append("PRIMARY KEY ");

            IndexTraits(operation, model, builder);

            builder.Append("(")
                .Append(ColumnListWithIndexPrefixLengthAndSortOrder(operation, operation.Columns, operation[XGAnnotationNames.IndexPrefixLength] as int[]))
                .Append(")");
        }

        protected override void UniqueConstraint(
            [NotNull] AddUniqueConstraintOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            if (operation.Name != null)
            {
                builder
                    .Append("CONSTRAINT ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                    .Append(" ");
            }

            builder
                .Append("UNIQUE ");

            IndexTraits(operation, model, builder);

            builder.Append("(")
                .Append(ColumnListWithIndexPrefixLengthAndSortOrder(operation, operation.Columns, operation[XGAnnotationNames.IndexPrefixLength] as int[]))
                .Append(")");
        }

        protected override void Generate(AddPrimaryKeyOperation operation, IModel model, MigrationCommandListBuilder builder, bool terminate = true)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("ALTER TABLE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                .Append(" ADD ");
            PrimaryKeyConstraint(operation, model, builder);
            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

            if (operation.Columns.Length == 1)
            {
                builder.Append(
                    $"CALL POMELO_AFTER_ADD_PRIMARY_KEY({_stringTypeMapping.GenerateSqlLiteral(operation.Schema)}, {_stringTypeMapping.GenerateSqlLiteral(operation.Table)}, {_stringTypeMapping.GenerateSqlLiteral(operation.Columns.First())});");

                builder.AppendLine();
            }

            EndStatement(builder);
        }

        protected override void Generate(
            DropPrimaryKeyOperation operation,
            IModel model,
            MigrationCommandListBuilder builder,
            bool terminate = true)
            => Generate(
                new XGDropPrimaryKeyAndRecreateForeignKeysOperation
                {
                    IsDestructiveChange = operation.IsDestructiveChange,
                    Name = operation.Name,
                    Schema = operation.Schema,
                    Table = operation.Table,
                    RecreateForeignKeys = false,
                },
                model,
                builder,
                terminate);

        protected virtual void Generate(
            XGDropPrimaryKeyAndRecreateForeignKeysOperation operation,
            IModel model,
            MigrationCommandListBuilder builder,
            bool terminate = true)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            void DropPrimaryKey()
            {
                builder.Append($"CALL POMELO_BEFORE_DROP_PRIMARY_KEY({_stringTypeMapping.GenerateSqlLiteral(operation.Schema)}, {_stringTypeMapping.GenerateSqlLiteral(operation.Table)});")
                    .AppendLine()
                    .Append("ALTER TABLE ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                    .Append(" DROP PRIMARY KEY");

                if (terminate)
                {
                    builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
                    EndStatement(builder);
                }
            }

            // A foreign key might reuse the primary key for its own purposes and prohibit its deletion,
            // if the foreign key columns are listed as the first columns and in the same order as in the foreign key (#678).
            // We therefore drop and later recreate all foreign keys to ensure, that no other dependencies on the
            // primary key exist, if explicitly requested by the user via `XGMigrationBuilderExtensions.DropPrimaryKey()`.
            if (operation.RecreateForeignKeys)
            {
                TemporarilyDropForeignKeys(
                    model,
                    builder,
                    operation.Schema,
                    operation.Table,
                    DropPrimaryKey);
            }
            else
            {
                DropPrimaryKey();
            }
        }

        /// <summary>
        ///     Generates a SQL fragment for a foreign key constraint of an <see cref="AddForeignKeyOperation" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null"/> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to add the SQL fragment. </param>
        protected override void ForeignKeyConstraint(
            AddForeignKeyOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            operation.Name = Truncate(operation.Name, 64);
            base.ForeignKeyConstraint(operation, model, builder);
        }

        /// <summary>
        ///     Generates a SQL fragment for traits of an index from a <see cref="CreateIndexOperation" />,
        ///     <see cref="AddPrimaryKeyOperation" />, or <see cref="AddUniqueConstraintOperation" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null"/> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to add the SQL fragment. </param>
        protected override void IndexTraits(MigrationOperation operation, IModel model,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            var fullText = operation[XGAnnotationNames.FullTextIndex] as bool?;
            if (fullText == true)
            {
                builder.Append("FULLTEXT ");
            }

            var spatial = operation[XGAnnotationNames.SpatialIndex] as bool?;
            if (spatial == true)
            {
                builder.Append("SPATIAL ");
            }
        }

        protected override void IndexOptions(MigrationOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            // The base implementation supports index filters in form of a WHERE clause.
            // This is not supported by MySQL, so we don't call it here.

            var fullText = operation[XGAnnotationNames.FullTextIndex] as bool?;
            if (fullText == true)
            {
                var fullTextParser = operation[XGAnnotationNames.FullTextParser] as string;
                if (!string.IsNullOrEmpty(fullTextParser))
                {
                    // Official MySQL support exists since 5.1, but since MariaDB does not support full-text parsers and does not recognize
                    // the "/*!xxxxx" syntax for versions below 50700, we use 50700 here, even though the statement would work in lower
                    // versions as well. Since we don't support MySQL 5.6 officially anymore, this is fine.
                    builder.Append(" /*!50700 WITH PARSER ")
                        .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(fullTextParser))
                        .Append(" */");
                }
            }
        }

        /// <summary>
        ///     Generates a SQL fragment for the given referential action.
        /// </summary>
        /// <param name="referentialAction"> The referential action. </param>
        /// <param name="builder"> The command builder to use to add the SQL fragment. </param>
        protected override void ForeignKeyAction(ReferentialAction referentialAction,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            if (referentialAction == ReferentialAction.Restrict)
            {
                builder.Append("RESTRICT");
            }
            else
            {
                base.ForeignKeyAction(referentialAction, builder);
            }
        }

        /// <summary>
        /// Use VALUES batches for INSERT commands where possible.
        /// </summary>
        protected override void Generate(InsertDataOperation operation, IModel model, MigrationCommandListBuilder builder, bool terminate = true)
        {
            var sqlBuilder = new StringBuilder();

            var modificationCommands = GenerateModificationCommands(operation, model).ToList();
            var updateSqlGenerator = (IXGUpdateSqlGenerator)Dependencies.UpdateSqlGenerator;

            foreach (var batch in _commandBatchPreparer.CreateCommandBatches(modificationCommands, moreCommandSets: true))
            {
                updateSqlGenerator.AppendBulkInsertOperation(sqlBuilder, batch.ModificationCommands, commandPosition: 0, out _);
            }

            builder.Append(sqlBuilder.ToString());

            if (terminate)
            {
                builder.EndCommand();
            }
        }

        /// <remarks>
        /// There is no need to check for explicit index collation/descending support, because ASC and DESC modifiers are being silently
        /// ignored in versions of MySQL and MariaDB, that do not support them.
        /// </remarks>
        private string ColumnListWithIndexPrefixLengthAndSortOrder(MigrationOperation operation, string[] columns, int[] prefixValues, bool[] isDescending = null)
            => ColumnList(
                columns,
                (c, i)
                    => $"{(prefixValues is not null && prefixValues.Length > i && prefixValues[i] > 0 ? $"({prefixValues[i]})" : null)}{(isDescending is not null && (isDescending.Length == 0 || isDescending[i]) ? " DESC" : null)}");

        protected virtual string ColumnList([NotNull] string[] columns, Func<string, int, string> columnPostfix)
            => string.Join(", ", columns.Select((c, i) => Dependencies.SqlGenerationHelper.DelimitIdentifier(c) + columnPostfix?.Invoke(c, i)));

        private string IntegerConstant(long? value)
            => string.Format(CultureInfo.InvariantCulture, "{0}", value);

        private static string Truncate(string source, int maxLength)
        {
            if (source == null
                || source.Length <= maxLength)
            {
                return source;
            }

            return source.Substring(0, maxLength);
        }

        private static bool IsSpatialStoreType(string storeType)
            => _spatialStoreTypes.Contains(storeType);

        private static bool IsDefaultValueSupported(string columnType)
            => !columnType.Contains("blob", StringComparison.OrdinalIgnoreCase) &&
               !columnType.Contains("text", StringComparison.OrdinalIgnoreCase) &&
               !columnType.Contains("json", StringComparison.OrdinalIgnoreCase) &&
               !IsSpatialStoreType(columnType);
    }
}
