// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
using Microsoft.Data.Entity.Relational.Migrations.Operations;
using Microsoft.Data.Entity.SqlServer.Metadata;

namespace Microsoft.Data.Entity.SqlServer.Migrations
{
    public class SqlServerModelDiffer : ModelDiffer
    {
        public SqlServerModelDiffer(
            [NotNull] IRelationalTypeMapper typeMapper,
            [NotNull] IRelationalMetadataExtensionProvider metadataExtensions)
            : base(typeMapper, metadataExtensions)
        {
        }

        #region IProperty

        protected override IEnumerable<MigrationOperation> Diff(IProperty source, IProperty target)
        {
            var operations = base.Diff(source, target).ToList();

            var sourceValueGenerationStrategy = GetValueGenerationStrategy(source);
            var targetValueGenerationStrategy = GetValueGenerationStrategy(target);

            var alterColumnOperation = operations.OfType<AlterColumnOperation>().SingleOrDefault();
            if (alterColumnOperation == null
                && (source.SqlServer().ComputedExpression != target.SqlServer().ComputedExpression
                    || sourceValueGenerationStrategy != targetValueGenerationStrategy))
            {
                var sourceExtensions = MetadataExtensions.Extensions(source);
                var sourceEntityTypeExtensions = MetadataExtensions.Extensions(source.EntityType);
                var targetExtensions = MetadataExtensions.Extensions(target);

                alterColumnOperation = new AlterColumnOperation
                {
                    Schema = sourceEntityTypeExtensions.Schema,
                    Table = sourceEntityTypeExtensions.Table,
                    Name = sourceExtensions.Column,
                    Type = targetExtensions.ColumnType ?? TypeMapper.MapPropertyType(target).DefaultTypeName,
                    IsNullable = target.IsNullable,
                    DefaultValue = targetExtensions.DefaultValue,
                    DefaultExpression = targetExtensions.DefaultExpression
                };
                operations.Add(alterColumnOperation);
            }

            if (alterColumnOperation != null)
            {
                if (targetValueGenerationStrategy == SqlServerValueGenerationStrategy.Identity)
                {
                    alterColumnOperation[SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.ValueGeneration] =
                        targetValueGenerationStrategy.ToString();
                }

                if (target.SqlServer().ComputedExpression != null)
                {
                    alterColumnOperation[SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.ColumnComputedExpression] =
                        target.SqlServer().ComputedExpression;
                    alterColumnOperation.IsDestructiveChange |= source.SqlServer().ComputedExpression == null;
                }
            }

            return operations;
        }

        protected override IEnumerable<MigrationOperation> Add(IProperty target)
        {
            var operation = base.Add(target).Cast<AddColumnOperation>().Single();

            var targetValueGenerationStrategy = GetValueGenerationStrategy(target);

            if (targetValueGenerationStrategy == SqlServerValueGenerationStrategy.Identity)
            {
                operation[SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.ValueGeneration] =
                    targetValueGenerationStrategy.ToString();
            }

            if (target.SqlServer().ComputedExpression != null)
            {
                operation[SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.ColumnComputedExpression] =
                    target.SqlServer().ComputedExpression;
            }

            yield return operation;
        }

        // TODO: Move to metadata API?
        private static SqlServerValueGenerationStrategy? GetValueGenerationStrategy(IProperty property)
            => property.StoreGeneratedPattern == StoreGeneratedPattern.Identity
               && property.SqlServer().DefaultExpression == null
               && property.SqlServer().DefaultValue == null
                ? property.SqlServer().ValueGenerationStrategy
                : null;

        #endregion

        #region IKey

        protected override IEnumerable<MigrationOperation> Diff(IKey source, IKey target)
        {
            var operations = base.Diff(source, target).ToList();

            var addOperation = operations.SingleOrDefault(o => o is AddPrimaryKeyOperation || o is AddUniqueConstraintOperation);
            if (addOperation == null)
            {
                if (source.SqlServer().IsClustered != target.SqlServer().IsClustered)
                {
                    operations.AddRange(Remove(source).Concat(Add(target)));
                }
            }
            else if (target.SqlServer().IsClustered != null)
            {
                addOperation[SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.Clustered] =
                    target.SqlServer().IsClustered.ToString();
            }

            return operations;
        }

        protected override IEnumerable<MigrationOperation> Add(IKey target)
        {
            var operation = base.Add(target)
                .Single(o => o is AddPrimaryKeyOperation || o is AddUniqueConstraintOperation);

            if (target.SqlServer().IsClustered != null)
            {
                operation[SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.Clustered] =
                    target.SqlServer().IsClustered.ToString();
            }

            yield return operation;
        }

        #endregion

        #region IIndex

        protected override IEnumerable<MigrationOperation> Diff(IIndex source, IIndex target)
        {
            var operations = base.Diff(source, target).ToList();

            var createIndexOperation = operations.OfType<CreateIndexOperation>().SingleOrDefault();
            if (createIndexOperation == null
                && source.SqlServer().IsClustered != target.SqlServer().IsClustered)
            {
                operations.AddRange(Remove(source));

                var targetExtensions = MetadataExtensions.Extensions(target);
                var targetEntityTypeExtensions = MetadataExtensions.Extensions(target.EntityType);

                createIndexOperation = new CreateIndexOperation
                {
                    Name = targetExtensions.Name,
                    Schema = targetEntityTypeExtensions.Schema,
                    Table = targetEntityTypeExtensions.Table,
                    Columns = GetColumnNames(target.Properties),
                    IsUnique = target.IsUnique
                };
                operations.Add(createIndexOperation);
            }

            if (createIndexOperation != null
                && target.SqlServer().IsClustered != null)
            {
                createIndexOperation[SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.Clustered] =
                    target.SqlServer().IsClustered.ToString();
            }

            return operations;
        }

        protected override IEnumerable<MigrationOperation> Add(IIndex target)
        {
            var operation = base.Add(target).Cast<CreateIndexOperation>().Single();

            if (target.SqlServer().IsClustered != null)
            {
                operation[SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.Clustered] =
                    target.SqlServer().IsClustered.ToString();
            }

            return base.Add(target);
        }

        #endregion
    }
}
