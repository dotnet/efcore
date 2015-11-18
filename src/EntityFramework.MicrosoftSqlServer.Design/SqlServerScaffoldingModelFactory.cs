// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Scaffolding.Internal;
using Microsoft.Data.Entity.Scaffolding.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Extensions.Logging;

namespace Microsoft.Data.Entity.Scaffolding
{
    public class SqlServerScaffoldingModelFactory : RelationalScaffoldingModelFactory
    {
        private const int DefaultTimeTimePrecision = 7;
        private readonly SqlServerLiteralUtilities _sqlServerLiteralUtilities;

        public SqlServerScaffoldingModelFactory(
            [NotNull] ILoggerFactory loggerFactory,
            [NotNull] IRelationalTypeMapper typeMapper,
            [NotNull] IDatabaseModelFactory databaseModelFactory,
            [NotNull] SqlServerLiteralUtilities sqlServerLiteralUtilities)
            : base(loggerFactory, typeMapper, databaseModelFactory)
        {
            Check.NotNull(sqlServerLiteralUtilities, nameof(sqlServerLiteralUtilities));

            _sqlServerLiteralUtilities = sqlServerLiteralUtilities;
        }

        public override IModel Create(string connectionString, TableSelectionSet tableSelectionSet)
        {
            var model = base.Create(connectionString, tableSelectionSet);
            model.Scaffolding().UseProviderMethodName = nameof(SqlServerDbContextOptionsExtensions.UseSqlServer);
            return model;
        }

        protected override PropertyBuilder VisitColumn([NotNull] EntityTypeBuilder builder, [NotNull] ColumnModel column)
        {
            var propertyBuilder = base.VisitColumn(builder, column);

            if(propertyBuilder == null)
            {
                return null;
            }

            VisitTypeMapping(propertyBuilder, column);

            VisitDefaultValue(column, propertyBuilder);

            return propertyBuilder;
        }

        protected override KeyBuilder VisitPrimaryKey([NotNull] EntityTypeBuilder builder, [NotNull] TableModel table)
        {
            var keyBuilder = base.VisitPrimaryKey(builder, table);

            if(keyBuilder == null)
            {
                return null;
            }

            // If this property is the single integer primary key on the EntityType then
            // KeyConvention assumes ValueGeneratedOnAdd(). If the underlying column does
            // not have Identity set then we need to set to ValueGeneratedNever() to
            // override this behavior.

            // TODO use KeyConvention directly to detect when it will be applied
            var pkColumns = table.Columns.OfType<SqlServerColumnModel>().Where(c => c.PrimaryKeyOrdinal.HasValue).ToList();
            if (pkColumns.Count != 1 || pkColumns[0].IsIdentity)
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

        protected override IndexBuilder VisitIndex(EntityTypeBuilder builder, IndexModel index)
        {
            var indexBuilder = base.VisitIndex(builder, index);

            if ((index as SqlServerIndexModel)?.IsClustered == true)
            {
                indexBuilder?.ForSqlServerIsClustered();
            }

            return indexBuilder;
        }

        private PropertyBuilder VisitTypeMapping(PropertyBuilder propertyBuilder, ColumnModel column)
        {
            var sqlColumn = column as SqlServerColumnModel;
            if (sqlColumn == null)
            {
                return propertyBuilder;
            }

            if (sqlColumn.IsIdentity)
            {
                if (typeof(byte) == propertyBuilder.Metadata.ClrType)
                {
                    Logger.LogWarning(
                        SqlServerDesignStrings.DataTypeDoesNotAllowSqlServerIdentityStrategy(
                            column.DisplayName, column.DataType));
                }
                else
                {
                    propertyBuilder
                        .ValueGeneratedOnAdd()
                        .UseSqlServerIdentityColumn();
                }
            }

            if (sqlColumn.DateTimePrecision.HasValue && sqlColumn.DateTimePrecision != DefaultTimeTimePrecision)
            {
                propertyBuilder.Metadata.SetMaxLength(null);
                propertyBuilder.HasColumnType($"{sqlColumn.DataType}({sqlColumn.DateTimePrecision.Value})");
            }

            // undo quirk in reverse type mapping to litters code with unnecessary nvarchar annotations
            if (typeof(string) == propertyBuilder.Metadata.ClrType
                && propertyBuilder.Metadata.Relational().ColumnType == "nvarchar")
            {
                propertyBuilder.Metadata.Relational().ColumnType = null;
            }

            return propertyBuilder;
        }

        private PropertyBuilder VisitDefaultValue(ColumnModel column, PropertyBuilder propertyBuilder)
        {
            if (column.DefaultValue != null)
            {
                ((Property)propertyBuilder.Metadata).SetValueGenerated(null, ConfigurationSource.Explicit);
                propertyBuilder.Metadata.Relational().GeneratedValueSql = null;

                var property = propertyBuilder.Metadata;
                var defaultExpressionOrValue =
                    _sqlServerLiteralUtilities
                        .ConvertSqlServerDefaultValue(
                            property.ClrType, column.DefaultValue);
                if (defaultExpressionOrValue?.DefaultExpression != null)
                {
                    propertyBuilder.HasDefaultValueSql(defaultExpressionOrValue.DefaultExpression);
                }
                else if (defaultExpressionOrValue != null)
                {
                    // Note: defaultExpressionOrValue.DefaultValue == null is valid
                    propertyBuilder.HasDefaultValue(defaultExpressionOrValue.DefaultValue);
                }
                else
                {
                    Logger.LogWarning(
                        SqlServerDesignStrings.UnableToConvertDefaultValue(
                            column.DisplayName, column.DefaultValue,
                            property.ClrType, property.Name, property.DeclaringEntityType.Name));
                }
            }
            return propertyBuilder;
        }
    }
}
