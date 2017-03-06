// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Microsoft.EntityFrameworkCore.Migrations.Design
{
    public abstract class MigrationsCodeGenerator
    {
        public abstract string FileExtension { get; }

        public abstract string GenerateMigration(
            [NotNull] string migrationNamespace,
            [NotNull] string migrationName,
            [NotNull] IReadOnlyList<MigrationOperation> upOperations,
            [NotNull] IReadOnlyList<MigrationOperation> downOperations);

        public abstract string GenerateMetadata(
            [NotNull] string migrationNamespace,
            [NotNull] Type contextType,
            [NotNull] string migrationName,
            [NotNull] string migrationId,
            [NotNull] IModel targetModel);

        public abstract string GenerateSnapshot(
            [NotNull] string modelSnapshotNamespace,
            [NotNull] Type contextType,
            [NotNull] string modelSnapshotName,
            [NotNull] IModel model);

        protected virtual IEnumerable<string> GetNamespaces([NotNull] IEnumerable<MigrationOperation> operations)
            => operations.OfType<ColumnOperation>().SelectMany(GetColumnNamespaces)
                .Concat(operations.OfType<CreateTableOperation>().SelectMany(o => o.Columns).SelectMany(GetColumnNamespaces))
                .Concat(GetAnnotationNamespaces(GetAnnotatables(operations)));

        private static IEnumerable<string> GetColumnNamespaces(ColumnOperation columnOperation)
        {
            yield return columnOperation.ClrType.Namespace;

            var alterColumnOperation = columnOperation as AlterColumnOperation;
            if (alterColumnOperation?.OldColumn != null)
            {
                yield return alterColumnOperation.OldColumn.ClrType.Namespace;
            }
        }

        private IEnumerable<IAnnotatable> GetAnnotatables(IEnumerable<MigrationOperation> operations)
        {
            foreach (var operation in operations)
            {
                yield return operation;

                var createTableOperation = operation as CreateTableOperation;
                if (createTableOperation != null)
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
            => model.GetEntityTypes().SelectMany(e => e.GetDeclaredProperties().Select(p => p.ClrType.Namespace))
                .Concat(GetAnnotationNamespaces(GetAnnotatables(model)));

        private IEnumerable<IAnnotatable> GetAnnotatables(IModel model)
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
            => from i in items
               from a in i.GetAnnotations()
               where a.Value != null
                     && a.Name != RelationshipDiscoveryConvention.NavigationCandidatesAnnotationName
                     && a.Name != RelationshipDiscoveryConvention.AmbiguousNavigationsAnnotationName
                     && a.Name != InversePropertyAttributeConvention.InverseNavigationsAnnotationName
               select a.Value.GetType().Namespace;
    }
}
