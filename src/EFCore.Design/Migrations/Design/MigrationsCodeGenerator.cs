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
    public abstract class MigrationsCodeGenerator : IMigrationsCodeGenerator
    {
        public MigrationsCodeGenerator([NotNull] MigrationsCodeGeneratorDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
        }

        public abstract string FileExtension { get; }

        protected virtual MigrationsCodeGeneratorDependencies Dependencies { get; }

        public abstract string GenerateMigration(
            string migrationNamespace,
            string migrationName,
            IReadOnlyList<MigrationOperation> upOperations,
            IReadOnlyList<MigrationOperation> downOperations);

        public abstract string GenerateMetadata(
            string migrationNamespace,
            Type contextType,
            string migrationName,
            string migrationId,
            IModel targetModel);

        public abstract string GenerateSnapshot(
            string modelSnapshotNamespace,
            Type contextType,
            string modelSnapshotName,
            IModel model);

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

                    yield return createTableOperation.PrimaryKey;

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

        protected virtual IEnumerable<string> GetNamespaces([NotNull] IModel model)
            => model.GetEntityTypes().SelectMany(e => e.GetDeclaredProperties().SelectMany(p => p.ClrType.GetNamespaces()))
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
                InversePropertyAttributeConvention.InverseNavigationsAnnotationName
            };

            return items.SelectMany(i => i.GetAnnotations())
                .Where(
                    a => a.Value != null
                         && !ignoredAnnotations.Contains(a.Name)).SelectMany(a => a.Value.GetType().GetNamespaces());
        }
    }
}
