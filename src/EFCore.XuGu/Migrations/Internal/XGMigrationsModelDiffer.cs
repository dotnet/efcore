// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Migrations.Internal
{
    public class XGMigrationsModelDiffer : MigrationsModelDiffer
    {
        // CHECK: Might be a good use case for runtime annotations.
        protected static class InternalLocalAnnotationNames
        {
            public const string InternalLocalPrefix = XGAnnotationNames.Prefix + "Internal:MigrationsModelDiffer:";

            public const string ExecuteBefore = InternalLocalPrefix + "ExecuteBefore";
        }

        public XGMigrationsModelDiffer(
            IRelationalTypeMappingSource typeMappingSource,
            IMigrationsAnnotationProvider migrationsAnnotationProvider,
            IRelationalAnnotationProvider relationalAnnotationProvider,
            IRowIdentityMapFactory rowIdentityMapFactory,
            CommandBatchPreparerDependencies commandBatchPreparerDependencies)
            : base(
                typeMappingSource,
                migrationsAnnotationProvider,
                relationalAnnotationProvider,
                rowIdentityMapFactory,
                commandBatchPreparerDependencies)
        {
            AssertAllMigrationOperationProperties();
        }

        public override IReadOnlyList<MigrationOperation> GetDifferences(IRelationalModel source, IRelationalModel target)
        {
            var operations = base.GetDifferences(source, target);

            // Ensure that we don't leak internal local annotations.
            AssertInternalLocalAnnotations(operations);

            return operations;
        }

        protected override IReadOnlyList<MigrationOperation> Sort(IEnumerable<MigrationOperation> operations, DiffContext diffContext)
        {
            // EF Core sorts all migration operations in a predefined order.
            // So because we want to execute certain operations in specific relation to one another, we anchor those operations via an
            // internal annotation to each other and relocate them after EF Core is done sorting.

            var sortedOperations = base.Sort(operations, diffContext);

            var anchoredOperations = new List<MigrationOperation>();
            var finalOperations = new List<MigrationOperation>();

            foreach (var operation in sortedOperations)
            {
                if (operation[InternalLocalAnnotationNames.ExecuteBefore] is MigrationOperation)
                {
                    anchoredOperations.Add(operation);
                }
                else
                {
                    finalOperations.Add(operation);
                }
            }

            foreach (var anchoredOperation in anchoredOperations)
            {
                var targetOperation = (MigrationOperation)anchoredOperation[InternalLocalAnnotationNames.ExecuteBefore];
                var targetOperationIndex = finalOperations.IndexOf(targetOperation);

                finalOperations.Insert(targetOperationIndex, anchoredOperation);

                anchoredOperation[InternalLocalAnnotationNames.ExecuteBefore] = null;
            }

            return finalOperations;
        }

        protected override IEnumerable<MigrationOperation> Add(IRelationalModel target, DiffContext diffContext)
            => PostFilterOperations(base.Add(target, diffContext));

        protected override IEnumerable<MigrationOperation> Diff(IRelationalModel source, IRelationalModel target, DiffContext diffContext)
            => PostFilterOperations(base.Diff(source, target, diffContext));

        protected override IEnumerable<MigrationOperation> Add(ITable target, DiffContext diffContext)
            => PostFilterOperations(base.Add(target, diffContext));

        protected override IEnumerable<MigrationOperation> Diff(ITable source, ITable target, DiffContext diffContext)
            => PostFilterOperations(base.Diff(source, target, diffContext));

        protected override IEnumerable<MigrationOperation> Add(IColumn target, DiffContext diffContext, bool inline = false)
        {
            if (!inline)
            {
                foreach (var propertyMapping in target.PropertyMappings)
                {
                    if (propertyMapping.Property.FindTypeMapping() is RelationalTypeMapping storeType)
                    {
                        var valueGenerationStrategy = XGValueGenerationStrategyCompatibility.GetValueGenerationStrategy(
                            target.GetAnnotations()
                                .ToArray());

                        // Ensure that null will be set for the columns default value, if CURRENT_TIMESTAMP has been required,
                        // or when the store type of the column does not support default values at all.
                        inline = inline ||
                                 (storeType.StoreTypeNameBase.Equals("datetime", StringComparison.OrdinalIgnoreCase) ||
                                  storeType.StoreTypeNameBase.Equals("timestamp", StringComparison.OrdinalIgnoreCase)) &&
                                 (valueGenerationStrategy == XGValueGenerationStrategy.IdentityColumn ||
                                  valueGenerationStrategy == XGValueGenerationStrategy.ComputedColumn) ||
                                 storeType.StoreTypeNameBase.Contains("text", StringComparison.OrdinalIgnoreCase) ||
                                 storeType.StoreTypeNameBase.Contains("blob", StringComparison.OrdinalIgnoreCase) ||
                                 storeType.StoreTypeNameBase.Equals("geometry", StringComparison.OrdinalIgnoreCase) ||
                                 storeType.StoreTypeNameBase.Equals("json", StringComparison.OrdinalIgnoreCase);

                        if (inline)
                        {
                            break;
                        }
                    }
                }
            }

            return PostFilterOperations(base.Add(target, diffContext, inline));
        }

        protected override IEnumerable<MigrationOperation> Diff(IColumn source, IColumn target, DiffContext diffContext)
            => PostFilterOperations(
                SkipRedundantCharSetSpecifyingAlterColumnOperations(
                    MakeStringColumnsRequiredWithoutUnexpectedDefaultValue(
                        source,
                        target,
                        base.Diff(source, target, diffContext))));

        /// <summary>
        /// Use a one-time `UPDATE` statement instead of an `ALTER COLUMN` operation in cases, where a non-required (`NULL`) string
        /// property is changed to a required (`NOT NULL`) one.
        /// EF Core generates an `ALTER COLUMN` statement with an unexpected default value of an empty string for those properties.
        /// While it is usually nice to have existing `NULL` values converted to empty strings, this should be a one-time operation and have
        /// no side effects on the table structure itself (so it should not introduce an unexpected default value).
        /// </summary>
        /// <remarks>
        /// See https://github.com/dotnet/efcore/issues/25899
        /// </remarks>
        private static IEnumerable<MigrationOperation> MakeStringColumnsRequiredWithoutUnexpectedDefaultValue(
            IColumn source,
            IColumn target,
            IEnumerable<MigrationOperation> migrationOperations)
        {
            foreach (var migrationOperation in migrationOperations)
            {
                if (migrationOperation is AlterColumnOperation alterColumnOperation &&
                    alterColumnOperation.IsDestructiveChange &&
                    alterColumnOperation.Schema == target.Table.Schema &&
                    alterColumnOperation.Table == target.Table.Name &&
                    alterColumnOperation.Name == target.Name &&
                    alterColumnOperation.ClrType == typeof(string) &&
                    alterColumnOperation.DefaultValue is "" &&
                    target.DefaultValue is null &&
                    !target.IsNullable &&
                    source.IsNullable)
                {
                    alterColumnOperation.DefaultValue = null;

                    yield return new UpdateDataOperation
                    {
                        IsDestructiveChange = true,
                        Table = alterColumnOperation.Table,
                        Schema = alterColumnOperation.Schema,
                        KeyColumns = new[] { alterColumnOperation.Name },
                        KeyColumnTypes = new[] { alterColumnOperation.ColumnType },
                        KeyValues = new object[,] { { null } },
                        Columns = new[] { alterColumnOperation.Name },
                        ColumnTypes = new[] { alterColumnOperation.ColumnType },
                        Values = new object[,] { { string.Empty } },
                        [InternalLocalAnnotationNames.ExecuteBefore] = alterColumnOperation,
                    };

                    yield return alterColumnOperation;
                }
                else
                {
                    yield return migrationOperation;
                }
            }
        }

        /// <remarks>
        /// When generating the first migration in Pomelo 5.0+, after previously using Pomelo 3.x, a significant amount of
        /// AlterColumnOperation might be generated, that don't really need to change anything in the database, because the legacy way of
        /// specifying a character set was used before and now the store type got cleaned-up and a CharSet annotation got added.
        /// This method ensures, that no useless operations get generated for this case.
        /// Everything between the old and the new column needs to be the same, except the store type definition, which contains the
        /// charset clause in the old, but not in the new store type.
        /// </remarks>
        private IEnumerable<MigrationOperation> SkipRedundantCharSetSpecifyingAlterColumnOperations(
            IEnumerable<MigrationOperation> migrationOperations)
        {
            foreach (var operation in migrationOperations)
            {
                const string charSetMatchPattern = @"^\s*(?<StoreType>[\w\s]*\w+)\s+(CHARACTER SET|CHARSET)\s+(?<CharSet>\w+)\s*$";

                // Depends on AssertMigrationOperationProperties check.
                if (operation is not AlterColumnOperation alterColumnOperation ||
                    alterColumnOperation.OldColumn[XGAnnotationNames.CharSet] is not string oldColumnCharSet ||
                    alterColumnOperation[XGAnnotationNames.CharSet] is not string newColumnCharSet ||
                    oldColumnCharSet != newColumnCharSet||
                    alterColumnOperation.ColumnType is not string newColumnType ||
                    alterColumnOperation.OldColumn.ColumnType is not string oldColumnType ||
                    newColumnType == oldColumnType ||
                    Regex.Match(oldColumnType, charSetMatchPattern, RegexOptions.IgnoreCase) is not Match sourceStoreTypeMatch ||
                    !sourceStoreTypeMatch.Success ||
                    !newColumnType.Trim().Equals(sourceStoreTypeMatch.Groups["StoreType"].Value, StringComparison.Ordinal) ||
                    !Equals(alterColumnOperation.ClrType, alterColumnOperation.OldColumn.ClrType) ||
                    !Equals(alterColumnOperation.IsUnicode, alterColumnOperation.OldColumn.IsUnicode) ||
                    !Equals(alterColumnOperation.IsFixedLength, alterColumnOperation.OldColumn.IsFixedLength) ||
                    !Equals(alterColumnOperation.MaxLength, alterColumnOperation.OldColumn.MaxLength) ||
                    !Equals(alterColumnOperation.Precision, alterColumnOperation.OldColumn.Precision) ||
                    !Equals(alterColumnOperation.Scale, alterColumnOperation.OldColumn.Scale) ||
                    !Equals(alterColumnOperation.IsRowVersion, alterColumnOperation.OldColumn.IsRowVersion) ||
                    !Equals(alterColumnOperation.IsNullable, alterColumnOperation.OldColumn.IsNullable) ||
                    !Equals(alterColumnOperation.DefaultValue, alterColumnOperation.OldColumn.DefaultValue) ||
                    !Equals(alterColumnOperation.DefaultValueSql, alterColumnOperation.OldColumn.DefaultValueSql) ||
                    !Equals(alterColumnOperation.ComputedColumnSql, alterColumnOperation.OldColumn.ComputedColumnSql) ||
                    !Equals(alterColumnOperation.IsStored, alterColumnOperation.OldColumn.IsStored) ||
                    !Equals(alterColumnOperation.Comment, alterColumnOperation.OldColumn.Comment) ||
                    !Equals(alterColumnOperation.Collation, alterColumnOperation.OldColumn.Collation) ||
                    HasDifferences(alterColumnOperation.GetAnnotations(), alterColumnOperation.OldColumn.GetAnnotations()))
                {
                    yield return operation;
                }
            }
        }

        protected virtual IEnumerable<MigrationOperation> PostFilterOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            foreach (var migrationOperation in migrationOperations)
            {
                var resultOperation = migrationOperation switch
                {
                    AlterDatabaseOperation operation => PostFilterOperation(operation),
                    CreateTableOperation operation => PostFilterOperation(operation),
                    AlterTableOperation operation => PostFilterOperation(operation),
                    AddColumnOperation operation => PostFilterOperation(operation),
                    AlterColumnOperation operation => PostFilterOperation(operation),
                    _ => migrationOperation
                };

                if (resultOperation != null)
                {
                    yield return resultOperation;
                }
            }
        }

        protected virtual AlterDatabaseOperation PostFilterOperation(AlterDatabaseOperation operation)
        {
            HandleCharSetDelegation(operation, DelegationModes.ApplyToDatabases);
            HandleCharSetDelegation(operation.OldDatabase, DelegationModes.ApplyToDatabases);

            ApplyCollationAnnotation(operation, (operation, collation) => operation.Collation ??= collation);
            ApplyCollationAnnotation(operation.OldDatabase, (operation, collation) => operation.Collation ??= collation);

            HandleCollationDelegation(operation, DelegationModes.ApplyToDatabases, o => o.Collation = null);
            HandleCollationDelegation(operation.OldDatabase, DelegationModes.ApplyToDatabases, o => o.Collation = null);

            // Ensure, that this hasn't become an empty operation.
            // Depends on AssertMigrationOperationProperties check.
            return operation.Collation != operation.OldDatabase.Collation ||
                   operation.IsReadOnly != operation.OldDatabase.IsReadOnly ||
                   HasDifferences(operation.GetAnnotations(), operation.OldDatabase.GetAnnotations())
                ? operation
                : null;
        }

        protected virtual CreateTableOperation PostFilterOperation(CreateTableOperation operation)
        {
            HandleCharSetDelegation(operation, DelegationModes.ApplyToTables);
            HandleCollationDelegation(operation, DelegationModes.ApplyToTables);

            for (var i = 0; i < operation.Columns.Count; i++)
            {
                var oldColumn = operation.Columns[i];
                var newColumn = PostFilterOperation(oldColumn);

                if (newColumn != oldColumn)
                {
                    operation.Columns[i] = newColumn;
                }
            }

            return operation;
        }

        protected virtual AlterTableOperation PostFilterOperation(AlterTableOperation operation)
        {
            HandleCharSetDelegation(operation, DelegationModes.ApplyToTables);
            HandleCharSetDelegation(operation.OldTable, DelegationModes.ApplyToTables);

            HandleCollationDelegation(operation, DelegationModes.ApplyToTables);
            HandleCollationDelegation(operation.OldTable, DelegationModes.ApplyToTables);

            // Ensure, that this hasn't become an empty operation.
            // We do not check Name and Schema, because changes would have resulted in a RenameTableOperation already.
            // Depends on AssertMigrationOperationProperties check.
            return operation.Comment != operation.OldTable.Comment ||
                   HasDifferences(operation.GetAnnotations(), operation.OldTable.GetAnnotations())
                ? operation
                : null;
        }

        protected virtual AddColumnOperation PostFilterOperation(AddColumnOperation operation)
        {
            ApplyCollationAnnotation(operation, (operation, collation) => operation.Collation ??= collation);

            return operation;
        }

        protected virtual AlterColumnOperation PostFilterOperation(AlterColumnOperation operation)
        {
            ApplyCollationAnnotation(operation, (operation, collation) => operation.Collation ??= collation);

            // Ensure, that this hasn't become an empty operation.
            // Depends on AssertMigrationOperationProperties check.
            return !Equals(operation.ClrType, operation.OldColumn.ClrType) ||
                   !Equals(operation.ColumnType, operation.OldColumn.ColumnType) ||
                   !Equals(operation.IsUnicode, operation.OldColumn.IsUnicode) ||
                   !Equals(operation.IsFixedLength, operation.OldColumn.IsFixedLength) ||
                   !Equals(operation.MaxLength, operation.OldColumn.MaxLength) ||
                   !Equals(operation.Precision, operation.OldColumn.Precision) ||
                   !Equals(operation.Scale, operation.OldColumn.Scale) ||
                   !Equals(operation.IsRowVersion, operation.OldColumn.IsRowVersion) ||
                   !Equals(operation.IsNullable, operation.OldColumn.IsNullable) ||
                   !Equals(operation.DefaultValue, operation.OldColumn.DefaultValue) ||
                   !Equals(operation.DefaultValueSql, operation.OldColumn.DefaultValueSql) ||
                   !Equals(operation.ComputedColumnSql, operation.OldColumn.ComputedColumnSql) ||
                   !Equals(operation.IsStored, operation.OldColumn.IsStored) ||
                   !Equals(operation.Comment, operation.OldColumn.Comment) ||
                   !Equals(operation.Collation, operation.OldColumn.Collation) ||
                   HasDifferences(operation.GetAnnotations(), operation.OldColumn.GetAnnotations())
                ? operation
                : null;
        }

        private static void ApplyCollationAnnotation<TOperation>(TOperation operation, Action<TOperation, string> applyCollation)
            where TOperation : MigrationOperation
        {
            if (operation[RelationalAnnotationNames.Collation] is string collation)
            {
                operation.RemoveAnnotation(RelationalAnnotationNames.Collation);
                applyCollation(operation, collation);
            }
        }

        private static void HandleCollationDelegation<TOperation>(
            TOperation operation,
            DelegationModes delegationModes,
            Action<TOperation> resetCollationProperty = null)
            where TOperation : MigrationOperation
        {
            // If the database collation should not be applied to the database itself, we need to reset the Collation property.
            // If the CollationDelegation annotation does not exist, it is ApplyToAll implicitly.
            if (operation[XGAnnotationNames.CollationDelegation] is DelegationModes databaseCollationDelegation)
            {
                // Don't leak the CollationDelegation annotation to the MigrationOperation.
                operation[XGAnnotationNames.CollationDelegation] = null;

                if (!databaseCollationDelegation.HasFlag(delegationModes))
                {
                    if (resetCollationProperty == null)
                    {
                        operation[RelationalAnnotationNames.Collation] = null;
                    }
                    else
                    {
                        resetCollationProperty(operation);
                    }
                }
            }
        }

        private static void HandleCharSetDelegation(MigrationOperation operation, DelegationModes delegationModes)
        {
            // If the character set should not be applied to the database itself, we need to remove the CharSet annotation.
            // If the CharSetDelegation annotation does not exist, it is ApplyToAll implicitly.
            if (operation[XGAnnotationNames.CharSetDelegation] is DelegationModes charSetDelegation)
            {
                // Don't leak the CharSetDelegation annotation to the MigrationOperation.
                operation[XGAnnotationNames.CharSetDelegation] = null;

                if (!charSetDelegation.HasFlag(delegationModes))
                {
                    operation[XGAnnotationNames.CharSet] = null;
                }
            }
        }

        /// <summary>
        /// Ensure, that no properties have been added by the EF Core team in the meantime.
        /// If they have, they may need to be added to checks in methods of this class
        /// (search for "Depends on AssertMigrationOperationProperties check").
        /// </summary>
        [Conditional("DEBUG")]
        private static void AssertAllMigrationOperationProperties()
        {
            AssertMigrationOperationProperties(
                typeof(AlterDatabaseOperation),
                new[]
                {
                    nameof(AlterDatabaseOperation.OldDatabase),
                    nameof(AlterDatabaseOperation.Collation),
                });

            AssertMigrationOperationProperties(
                typeof(AlterTableOperation),
                new[]
                {
                    nameof(AlterTableOperation.OldTable),
                    nameof(AlterTableOperation.Name),
                    nameof(AlterTableOperation.Schema),
                    nameof(AlterTableOperation.Comment),
                });

            AssertMigrationOperationProperties(
                typeof(AlterColumnOperation),
                new[]
                {
                    nameof(AlterColumnOperation.OldColumn),
                    nameof(AlterColumnOperation.Name),
                    nameof(AlterColumnOperation.Schema),
                    nameof(AlterColumnOperation.Table),
                    nameof(AlterColumnOperation.ClrType),
                    nameof(AlterColumnOperation.ColumnType),
                    nameof(AlterColumnOperation.IsUnicode),
                    nameof(AlterColumnOperation.IsFixedLength),
                    nameof(AlterColumnOperation.MaxLength),
                    nameof(AlterColumnOperation.Precision),
                    nameof(AlterColumnOperation.Scale),
                    nameof(AlterColumnOperation.IsRowVersion),
                    nameof(AlterColumnOperation.IsNullable),
                    nameof(AlterColumnOperation.DefaultValue),
                    nameof(AlterColumnOperation.DefaultValueSql),
                    nameof(AlterColumnOperation.ComputedColumnSql),
                    nameof(AlterColumnOperation.IsStored),
                    nameof(AlterColumnOperation.Comment),
                    nameof(AlterColumnOperation.Collation),
                });
        }

        [Conditional("DEBUG")]
        private static void AssertMigrationOperationProperties(Type migrationOperationType, IEnumerable<string> propertyNames)
        {
            if (migrationOperationType
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(p => p.Name)
                .Except(
                    propertyNames.Concat(
                        new[]
                        {
                            "Item",
                            nameof(AlterDatabaseOperation.IsReadOnly),
                            nameof(MigrationOperation.IsDestructiveChange)
                        }))
                .FirstOrDefault() is string unexpectedProperty)
            {
                throw new InvalidOperationException(
                    $"The migration operation of type '{migrationOperationType.Name}' contains an unexpected property '{unexpectedProperty}'.");
            }
        }

        [Conditional("DEBUG")]
        private static void AssertInternalLocalAnnotations(IReadOnlyList<MigrationOperation> operations)
        {
            foreach (var operation in operations)
            {
                foreach (var annotation in operation.GetAnnotations())
                {
                    if (annotation.Name.StartsWith(InternalLocalAnnotationNames.InternalLocalPrefix, StringComparison.Ordinal))
                    {
                        throw new InvalidOperationException(
                            $"The migration operation of type '{operation.GetType().Name}' leaked the internal local annotation '{annotation.Name}'.");
                    }
                }
            }
        }
    }
}
