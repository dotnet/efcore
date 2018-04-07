// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Migrations.Design
{
    /// <summary>
    ///     Used to generate code for migrations.
    /// </summary>
#pragma warning disable CA1012 // Abstract types should not have constructors
    // Already shipped
    public abstract class MigrationsCodeGenerator : IMigrationsCodeGenerator
#pragma warning restore CA1012 // Abstract types should not have constructors
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MigrationsCodeGenerator" /> class.
        /// </summary>
        /// <param name="dependencies"> The dependencies. </param>
        public MigrationsCodeGenerator([NotNull] MigrationsCodeGeneratorDependencies dependencies)
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
        public virtual string Language => null;

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
            string migrationNamespace,
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
        /// <param name="targetModel"> The migraiton's target model. </param>
        /// <returns> The migration metadata code. </returns>
        public abstract string GenerateMetadata(
            string migrationNamespace,
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
            string modelSnapshotNamespace,
            Type contextType,
            string modelSnapshotName,
            IModel model);

        /// <summary>
        ///     Gets the namespaces required for a list of <see cref="MigrationOperation" /> objects.
        /// </summary>
        /// <param name="operations"> The operations. </param>
        /// <returns> The namespaces. </returns>
        protected virtual IEnumerable<string> GetNamespaces([NotNull] IEnumerable<MigrationOperation> operations)
            => operations.OfType<ColumnOperation>().SelectMany(GetColumnNamespaces)
                .Concat(operations.OfType<CreateTableOperation>().SelectMany(o => o.Columns).SelectMany(GetColumnNamespaces))
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
        protected virtual IEnumerable<string> GetNamespaces([NotNull] IModel model)
            => model.GetEntityTypes().SelectMany(
                    e => e.GetDeclaredProperties()
                        .SelectMany(p => (p.FindMapping()?.Converter?.ProviderClrType ?? p.ClrType).GetNamespaces()))
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

        private static IEnumerable<string> GetAnnotationNamespaces(IEnumerable<IAnnotatable> items)
        {
            var ignoredAnnotations = new List<string>
            {
                RelationshipDiscoveryConvention.NavigationCandidatesAnnotationName,
                RelationshipDiscoveryConvention.AmbiguousNavigationsAnnotationName,
                InversePropertyAttributeConvention.InverseNavigationsAnnotationName,
                CoreAnnotationNames.TypeMapping,
                CoreAnnotationNames.ValueComparer,
                CoreAnnotationNames.KeyValueComparer
            };

            return items.SelectMany(i => i.GetAnnotations())
                .Where(a => a.Value != null
                            && !ignoredAnnotations.Contains(a.Name)).SelectMany(a => a.Value.GetType().GetNamespaces());
        }
    }
}
