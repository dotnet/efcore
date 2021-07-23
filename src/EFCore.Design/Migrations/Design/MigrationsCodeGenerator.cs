// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Migrations.Design
{
    /// <summary>
    ///     Used to generate code for migrations.
    /// </summary>
    public abstract class MigrationsCodeGenerator : IMigrationsCodeGenerator
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MigrationsCodeGenerator" /> class.
        /// </summary>
        /// <param name="dependencies"> The dependencies. </param>
        protected MigrationsCodeGenerator(MigrationsCodeGeneratorDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
        }

        /// <summary>
        ///     Gets the file extension code files should use.
        /// </summary>
        /// <value> The file extension. </value>
        public abstract string FileExtension { get; }

        /// <summary>
        ///     Gets the programming language supported by this service.
        /// </summary>
        /// <value> The language. </value>
        public virtual string? Language
            => null;

        /// <summary>
        ///     Parameter object containing dependencies for this service.
        /// </summary>
        protected virtual MigrationsCodeGeneratorDependencies Dependencies { get; }

        /// <summary>
        ///     Generates the migration code.
        /// </summary>
        /// <param name="migrationNamespace"> The migration's namespace. </param>
        /// <param name="migrationName"> The migration's name. </param>
        /// <param name="upOperations"> The migration's up operations. </param>
        /// <param name="downOperations"> The migration's down operations. </param>
        /// <returns> The migration code. </returns>
        public abstract string GenerateMigration(
            string? migrationNamespace,
            string migrationName,
            IReadOnlyList<MigrationOperation> upOperations,
            IReadOnlyList<MigrationOperation> downOperations);

        /// <summary>
        ///     Generates the migration metadata code.
        /// </summary>
        /// <param name="migrationNamespace"> The migration's namespace. </param>
        /// <param name="contextType"> The migration's <see cref="DbContext" /> type. </param>
        /// <param name="migrationName"> The migration's name. </param>
        /// <param name="migrationId"> The migration's ID. </param>
        /// <param name="targetModel"> The migration's target model. </param>
        /// <returns> The migration metadata code. </returns>
        public abstract string GenerateMetadata(
            string? migrationNamespace,
            Type contextType,
            string migrationName,
            string migrationId,
            IModel targetModel);

        /// <summary>
        ///     Generates the model snapshot code.
        /// </summary>
        /// <param name="modelSnapshotNamespace"> The model snapshot's namespace. </param>
        /// <param name="contextType"> The model snapshot's <see cref="DbContext" /> type. </param>
        /// <param name="modelSnapshotName"> The model snapshot's name. </param>
        /// <param name="model"> The model. </param>
        /// <returns> The model snapshot code. </returns>
        public abstract string GenerateSnapshot(
            string? modelSnapshotNamespace,
            Type contextType,
            string modelSnapshotName,
            IModel model);

        /// <summary>
        ///     Gets the namespaces required for a list of <see cref="MigrationOperation" /> objects.
        /// </summary>
        /// <param name="operations"> The operations. </param>
        /// <returns> The namespaces. </returns>
        protected virtual IEnumerable<string> GetNamespaces(IEnumerable<MigrationOperation> operations)
            => operations.OfType<ColumnOperation>().SelectMany(GetColumnNamespaces)
                .Concat(operations.OfType<CreateTableOperation>().SelectMany(o => o.Columns).SelectMany(GetColumnNamespaces))
                .Concat(
                    operations.OfType<InsertDataOperation>().Select(o => o.Values)
                        .Concat(operations.OfType<UpdateDataOperation>().SelectMany(o => new[] { o.KeyValues, o.Values }))
                        .Concat(operations.OfType<DeleteDataOperation>().Select(o => o.KeyValues))
                        .SelectMany(GetDataNamespaces))
                .Concat(GetAnnotationNamespaces(GetAnnotatables(operations)));

        private static IEnumerable<string> GetColumnNamespaces(ColumnOperation columnOperation)
        {
            foreach (var ns in columnOperation.ClrType.GetNamespaces())
            {
                yield return ns;
            }

            var alterColumnOperation = columnOperation as AlterColumnOperation;
            if (alterColumnOperation?.OldColumn != null)
            {
                foreach (var ns in alterColumnOperation.OldColumn.ClrType.GetNamespaces())
                {
                    yield return ns;
                }
            }
        }

        private static IEnumerable<string> GetDataNamespaces(object?[,] values)
        {
            for (var row = 0; row < values.GetLength(0); row++)
            {
                for (var column = 0; column < values.GetLength(1); column++)
                {
                    var value = values[row, column];
                    if (value != null)
                    {
                        foreach (var ns in value.GetType().GetNamespaces())
                        {
                            yield return ns;
                        }
                    }
                }
            }
        }

        private static IEnumerable<IAnnotatable> GetAnnotatables(IEnumerable<MigrationOperation> operations)
        {
            foreach (var operation in operations)
            {
                yield return operation;

                if (operation is CreateTableOperation createTableOperation)
                {
                    foreach (var column in createTableOperation.Columns)
                    {
                        yield return column;
                    }

                    if (createTableOperation.PrimaryKey != null)
                    {
                        yield return createTableOperation.PrimaryKey;
                    }

                    foreach (var uniqueConstraint in createTableOperation.UniqueConstraints)
                    {
                        yield return uniqueConstraint;
                    }

                    foreach (var foreignKey in createTableOperation.ForeignKeys)
                    {
                        yield return foreignKey;
                    }
                }
            }
        }

        /// <summary>
        ///     Gets the namespaces required for an <see cref="IModel" />.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <returns> The namespaces. </returns>
        protected virtual IEnumerable<string> GetNamespaces(IModel model)
            => model.GetEntityTypes().SelectMany(
                    e => e.GetDeclaredProperties()
                        .SelectMany(p => (FindValueConverter(p)?.ProviderClrType ?? p.ClrType).GetNamespaces()))
                .Concat(GetAnnotationNamespaces(GetAnnotatables(model)));

        private static IEnumerable<IAnnotatable> GetAnnotatables(IModel model)
        {
            yield return model;

            foreach (var entityType in model.GetEntityTypes())
            {
                yield return entityType;

                foreach (var property in entityType.GetDeclaredProperties())
                {
                    yield return property;
                }

                foreach (var key in entityType.GetDeclaredKeys())
                {
                    yield return key;
                }

                foreach (var foreignKey in entityType.GetDeclaredForeignKeys())
                {
                    yield return foreignKey;
                }

                foreach (var index in entityType.GetDeclaredIndexes())
                {
                    yield return index;
                }
            }
        }

        private IEnumerable<string> GetAnnotationNamespaces(IEnumerable<IAnnotatable> items)
            => items.SelectMany(
                i => Dependencies.AnnotationCodeGenerator.FilterIgnoredAnnotations(i.GetAnnotations())
                    .Where(a => a.Value != null)
                    .Select(a => new { Annotatable = i, Annotation = a })
                    .SelectMany(a => GetProviderType(a.Annotatable, a.Annotation.Value!.GetType()).GetNamespaces()));

        private ValueConverter? FindValueConverter(IProperty property)
            => (property.FindTypeMapping()
                ?? Dependencies.RelationalTypeMappingSource.FindMapping(property))?.Converter;

        private Type GetProviderType(IAnnotatable annotatable, Type valueType)
            => annotatable is IProperty property
                && valueType.UnwrapNullableType() == property.ClrType.UnwrapNullableType()
                    ? FindValueConverter(property)?.ProviderClrType ?? valueType
                    : valueType;
    }
}
