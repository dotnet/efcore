// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Operations;

namespace Microsoft.Data.Entity.Migrations.Design
{
    public abstract class MigrationsCodeGenerator
    {
        public static IReadOnlyList<string> IgnoredAnnotations { get; } = new List<string>
        {
            CoreAnnotationNames.OriginalValueIndexAnnotation,
            CoreAnnotationNames.ShadowIndexAnnotation
        };

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
            => GetAnnotationNamespaces(GetAnnotatables(operations));

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
            => GetAnnotationNamespaces(GetAnnotatables(model));

        private IEnumerable<IAnnotatable> GetAnnotatables(IModel model)
        {
            yield return model;

            foreach (var entityType in model.EntityTypes)
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
               from a in i.Annotations
               where a.Value != null && !IgnoredAnnotations.Contains(a.Name)
               select a.Value.GetType().Namespace;
    }
}
