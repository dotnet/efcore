// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqlServerScaffoldingModelFactory : RelationalScaffoldingModelFactory
    {
        private const int DefaultTimeTimePrecision = 7;

        private static readonly ISet<string> _stringAndByteArrayTypesForbiddingMaxLength =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "image", "ntext", "text", "rowversion", "timestamp" };

        private static readonly ISet<string> _dataTypesAllowingMaxLengthMax =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "varchar", "nvarchar", "varbinary" };

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SqlServerScaffoldingModelFactory(
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Scaffolding> logger,
            [NotNull] IRelationalTypeMapper typeMapper,
            [NotNull] IDatabaseModelFactory databaseModelFactory,
            [NotNull] CandidateNamingService candidateNamingService,
            [NotNull] IPluralizer pluralizer,
            [NotNull] IScaffoldingHelper scaffoldingHelper)
            : base(logger, typeMapper, databaseModelFactory, candidateNamingService, pluralizer, scaffoldingHelper)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override PropertyBuilder VisitColumn(EntityTypeBuilder builder, ColumnModel column)
        {
            var propertyBuilder = base.VisitColumn(builder, column);

            if (propertyBuilder == null)
            {
                return null;
            }

            VisitDefaultValue(propertyBuilder, column);

            VisitComputedValue(propertyBuilder, column);

            return propertyBuilder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override RelationalTypeMapping GetTypeMapping(ColumnModel column)
        {
            RelationalTypeMapping mapping = null;
            if (column.StoreType != null)
            {
                string underlyingDataType = null;
                column.Table.Database.SqlServer().TypeAliases?.TryGetValue(
                    SqlServerDatabaseModelFactory.SchemaQualifiedKey(column.StoreType, column.SqlServer().DataTypeSchemaName), out underlyingDataType);

                mapping = TypeMapper.FindMapping(underlyingDataType ?? column.StoreType);
            }

            return mapping;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override KeyBuilder VisitPrimaryKey(EntityTypeBuilder builder, TableModel table)
        {
            var keyBuilder = base.VisitPrimaryKey(builder, table);

            if (keyBuilder == null)
            {
                return null;
            }

            // If this property is the single integer primary key on the EntityType then
            // KeyConvention assumes ValueGeneratedOnAdd(). If the underlying column does
            // not have Identity set then we need to set to ValueGeneratedNever() to
            // override this behavior.

            // TODO use KeyConvention directly to detect when it will be applied
            var pkColumns = table.Columns.Where(c => c.PrimaryKeyOrdinal.HasValue).ToList();
            if (pkColumns.Count != 1
                || pkColumns[0].ValueGenerated != null
                || pkColumns[0].DefaultValue != null)
            {
                return keyBuilder;
            }

            // TODO
            var property = builder.Metadata.FindProperty(GetPropertyName(pkColumns[0]));
            var propertyType = property?.ClrType?.UnwrapNullableType();

            if (propertyType?.IsInteger() == true
                || propertyType == typeof(Guid))
            {
                property.ValueGenerated = ValueGenerated.Never;
            }

            return keyBuilder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override IndexBuilder VisitIndex(EntityTypeBuilder builder, IndexModel index)
        {
            var indexBuilder = base.VisitIndex(builder, index);

            if (index.SqlServer().IsClustered)
            {
                indexBuilder?.ForSqlServerIsClustered();
            }

            return indexBuilder;
        }

        private void VisitDefaultValue(PropertyBuilder propertyBuilder, ColumnModel column)
        {
            if (column.DefaultValue != null)
            {
                propertyBuilder.Metadata.Relational().DefaultValueSql = null;

                if (!(column.DefaultValue == "(NULL)"
                        && propertyBuilder.Metadata.ClrType.IsNullableType()))
                {
                    propertyBuilder.HasDefaultValueSql(column.DefaultValue);
                }
                else
                {
                    ((Property)propertyBuilder.Metadata).SetValueGenerated(null, ConfigurationSource.Explicit);
                }
            }
        }

        private void VisitComputedValue(PropertyBuilder propertyBuilder, ColumnModel column)
        {
            if (column.ComputedValue != null)
            {
                propertyBuilder.Metadata.Relational().ComputedColumnSql = null;

                if (!(column.ComputedValue == "(NULL)"
                        && propertyBuilder.Metadata.ClrType.IsNullableType()))
                {
                    propertyBuilder.HasComputedColumnSql(column.ComputedValue);
                }
                else
                {
                    ((Property)propertyBuilder.Metadata).SetValueGenerated(null, ConfigurationSource.Explicit);
                }
            }
        }
    }
}
