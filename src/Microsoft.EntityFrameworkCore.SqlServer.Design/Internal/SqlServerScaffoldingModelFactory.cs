// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

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
            [NotNull] ILoggerFactory loggerFactory,
            [NotNull] IRelationalTypeMapper typeMapper,
            [NotNull] IDatabaseModelFactory databaseModelFactory,
            [NotNull] CandidateNamingService candidateNamingService)
            : base(loggerFactory, typeMapper, databaseModelFactory, candidateNamingService)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override IModel Create(string connectionString, TableSelectionSet tableSelectionSet)
        {
            var model = base.Create(connectionString, tableSelectionSet);
            model.Scaffolding().UseProviderMethodName = nameof(SqlServerDbContextOptionsExtensions.UseSqlServer);
            return model;
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

            VisitTypeMapping(propertyBuilder, column);

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
            if (column.DataType != null)
            {
                string underlyingDataType = null;
                column.Table.Database.SqlServer().TypeAliases
                    ?.TryGetValue(column.DataType, out underlyingDataType);

                mapping = TypeMapper.FindMapping(underlyingDataType ?? column.DataType);
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

        private void VisitTypeMapping(PropertyBuilder propertyBuilder, ColumnModel column)
        {
            if (column.SqlServer().IsIdentity)
            {
                if (typeof(byte) == propertyBuilder.Metadata.ClrType)
                {
                    Logger.LogWarning(
                        SqlServerDesignEventId.DataTypeDoesNotAllowSqlServerIdentityStrategyWarning,
                        () => SqlServerDesignStrings.DataTypeDoesNotAllowSqlServerIdentityStrategy(
                            column.DisplayName, column.DataType));
                }
                else
                {
                    propertyBuilder
                        .ValueGeneratedOnAdd()
                        .UseSqlServerIdentityColumn();
                }
            }

            var dateTimePrecision = column.SqlServer().DateTimePrecision;
            if (dateTimePrecision.HasValue
                && dateTimePrecision.Value != DefaultTimeTimePrecision)
            {
                propertyBuilder.Metadata.SetMaxLength(null);
                propertyBuilder.HasColumnType(
                    string.Format(CultureInfo.InvariantCulture, "{0}({1})", column.DataType, dateTimePrecision.Value));
            }
            else if (!HasTypeAlias(column))
            {
                var qualifiedColumnTypeAndMaxLength =
                    MaxLengthQualifiedDataType(column.DataType, column.MaxLength);
                if (qualifiedColumnTypeAndMaxLength != null)
                {
                    propertyBuilder.HasColumnType(qualifiedColumnTypeAndMaxLength.Item1);
                    propertyBuilder.Metadata.SetMaxLength(qualifiedColumnTypeAndMaxLength.Item2);
                }
            }

            var columnType = propertyBuilder.Metadata.SqlServer().ColumnType;
            if (columnType != null)
            {
                TypeMapper.ValidateTypeName(columnType);
            }
        }

        private static bool HasTypeAlias(ColumnModel column)
            => column.DataType != null
               && column.Table.Database.SqlServer().TypeAliases?.ContainsKey(column.DataType) == true;

        // Turns an unqualified SQL Server type name (e.g. varchar) and its
        // max length into a qualified SQL Server type name (e.g. varchar(max))
        // and its max length for use on string and byte[] properties.
        // Null for either value in the tuple means don't use that value.
        // A null tuple means don't change anything.
        private Tuple<string, int?> MaxLengthQualifiedDataType(
            string unqualifiedTypeName, int? maxLength)
        {
            var typeMapping = TypeMapper.FindMapping(unqualifiedTypeName);
            if (typeMapping != null
                && (typeof(string) == typeMapping.ClrType
                    || typeof(byte[]) == typeMapping.ClrType)
                && !_stringAndByteArrayTypesForbiddingMaxLength.Contains(unqualifiedTypeName))
            {
                if (typeMapping.StoreType == "nvarchar"
                    || typeMapping.StoreType == "varbinary")
                {
                    // nvarchar is the default column type for string properties,
                    // so we don't need to define it using HasColumnType() and removing
                    // the column type allows the HasMaxLength() API to have effect.
                    // Similarly for varbinary and byte[] properties.
                    return new Tuple<string, int?>(null, maxLength);
                }
                if (_dataTypesAllowingMaxLengthMax.Contains(typeMapping.StoreType))
                {
                    return new Tuple<string, int?>(
                        unqualifiedTypeName
                        + "("
                        + (maxLength?.ToString(CultureInfo.InvariantCulture) ?? "max")
                        + ")",
                        null);
                }
                return new Tuple<string, int?>(
                    unqualifiedTypeName
                    + (maxLength == null ? string.Empty : "(" + maxLength + ")"),
                    null);
            }

            return null;
        }

        private void VisitDefaultValue(PropertyBuilder propertyBuilder, ColumnModel column)
        {
            if (column.DefaultValue != null)
            {
                ((Property)propertyBuilder.Metadata).SetValueGenerated(null, ConfigurationSource.Explicit);
                propertyBuilder.Metadata.Relational().DefaultValueSql = null;

                var defaultExpression = ConvertSqlServerDefaultValue(column.DefaultValue);
                if (defaultExpression != null)
                {
                    if (!(defaultExpression == "NULL"
                          && propertyBuilder.Metadata.ClrType.IsNullableType()))
                    {
                        propertyBuilder.HasDefaultValueSql(defaultExpression);
                    }
                }
                else
                {
                    Logger.LogWarning(
                        SqlServerDesignEventId.CannotInterpretDefaultValueWarning,
                        () => SqlServerDesignStrings.CannotInterpretDefaultValue(
                            column.DisplayName,
                            column.DefaultValue,
                            propertyBuilder.Metadata.Name,
                            propertyBuilder.Metadata.DeclaringEntityType.DisplayName()));
                }
            }
        }

        private void VisitComputedValue(PropertyBuilder propertyBuilder, ColumnModel column)
        {
            if (column.ComputedValue != null)
            {
                ((Property)propertyBuilder.Metadata).SetValueGenerated(null, ConfigurationSource.Explicit);
                propertyBuilder.Metadata.Relational().ComputedColumnSql = null;

                var computedExpression = ConvertSqlServerDefaultValue(column.ComputedValue);
                if (computedExpression != null)
                {
                    if (!(computedExpression == "NULL"
                          && propertyBuilder.Metadata.ClrType.IsNullableType()))
                    {
                        propertyBuilder.HasComputedColumnSql(computedExpression);
                    }
                }
                else
                {
                    Logger.LogWarning(
                        SqlServerDesignEventId.CannotInterpretComputedValueWarning,
                        () => SqlServerDesignStrings.CannotInterpretComputedValue(
                            column.DisplayName,
                            column.ComputedValue,
                            propertyBuilder.Metadata.Name,
                            propertyBuilder.Metadata.DeclaringEntityType.DisplayName()));
                }
            }
        }

        private static string ConvertSqlServerDefaultValue(string sqlServerDefaultValue)
        {
            if (sqlServerDefaultValue.Length < 2)
            {
                return null;
            }

            while (sqlServerDefaultValue[0] == '('
                   && sqlServerDefaultValue[sqlServerDefaultValue.Length - 1] == ')')
            {
                sqlServerDefaultValue = sqlServerDefaultValue.Substring(1, sqlServerDefaultValue.Length - 2);
            }

            return sqlServerDefaultValue;
        }
    }
}
