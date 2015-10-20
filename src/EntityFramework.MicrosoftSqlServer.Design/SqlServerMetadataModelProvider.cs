// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Data.Entity.Scaffolding.Internal;
using Microsoft.Data.Entity.Scaffolding.Metadata;
using Microsoft.Data.Entity.Scaffolding.Model;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Extensions.Logging;

namespace Microsoft.Data.Entity.Scaffolding
{
    public class SqlServerMetadataModelProvider : RelationalMetadataModelProvider
    {
        private readonly SqlServerLiteralUtilities _sqlServerLiteralUtilities;

        private const int DefaultDateTimePrecision = 7;
        private static readonly ISet<string> _dateTimePrecisionTypes = new HashSet<string> { "datetimeoffset", "datetime2", "time" };

        public SqlServerMetadataModelProvider(
            [NotNull] ILoggerFactory loggerFactory,
            [NotNull] IRelationalTypeMapper typeMapper,
            [NotNull] IMetadataReader metadataReader,
            [NotNull] SqlServerLiteralUtilities sqlServerLiteralUtilities)
            : base(loggerFactory, typeMapper, metadataReader)
        {
            Check.NotNull(sqlServerLiteralUtilities, nameof(sqlServerLiteralUtilities));

            _sqlServerLiteralUtilities = sqlServerLiteralUtilities;
        }

        protected override PropertyBuilder AddColumn([NotNull] EntityTypeBuilder builder, [NotNull] Column column)
        {
            var propertyBuilder = base.AddColumn(builder, column);

            AddSqlServerTypeMapping(column, propertyBuilder);

            AddSqlServerDefaultValue(column, propertyBuilder);

            return propertyBuilder;
        }

        protected override EntityTypeBuilder AddPrimaryKey([NotNull] EntityTypeBuilder builder, [NotNull] Table table)
        {
            base.AddPrimaryKey(builder, table);

            // If this property is the single integer primary key on the EntityType then
            // KeyConvention assumes ValueGeneratedOnAdd(). If the underlying column does
            // not have Identity set then we need to set to ValueGeneratedNever() to
            // override this behavior.

            // TODO use KeyConvention directly to detect when it will be applied
            var pkColumns = table.Columns.Where(c => c.PrimaryKeyOrdinal.HasValue).ToList();
            if (pkColumns.Count != 1 || pkColumns[0].IsIdentity == true)
            {
                return builder;
            }

            // TODO 
            var property = builder.Metadata.FindProperty(GetPropertyName(pkColumns[0]));
            var propertyType = property?.ClrType?.UnwrapNullableType();

            if (propertyType?.IsInteger() == true
                || propertyType == typeof(Guid))
            {
                property.ValueGenerated = ValueGenerated.Never;
                property.RelationalDesign().ExplicitValueGeneratedNever = true;
            }

            return builder;
        }

        private PropertyBuilder AddSqlServerTypeMapping(Column column, PropertyBuilder propertyBuilder)
        {
            if (column.IsIdentity == true)
            {
                if (typeof(byte) == propertyBuilder.Metadata.ClrType)
                {
                    Logger.LogWarning(
                        SqlServerDesignStrings.DataTypeDoesNotAllowSqlServerIdentityStrategy(
                            column.DisplayName(), column.DataType));
                }
                else
                {
                    propertyBuilder
                        .ValueGeneratedOnAdd()
                        .UseSqlServerIdentityColumn();
                }
            }

            if (_dateTimePrecisionTypes.Contains(column.DataType)
                && column.Scale.HasValue
                && column.Scale != DefaultDateTimePrecision)
            {
                propertyBuilder.Metadata.SetMaxLength(null);
                propertyBuilder.HasColumnType($"{column.DataType}({column.Scale})"); //not a typo: .Scale is the right property for datetime precision
            }

            // undo quirk in reverse type mapping to litters code with unnecessary nvarchar annotations
            if (typeof(string) == propertyBuilder.Metadata.ClrType
                && propertyBuilder.Metadata.Relational().ColumnType == "nvarchar")
            {
                propertyBuilder.Metadata.Relational().ColumnType = null;
            }

            return propertyBuilder;
        }

        private PropertyBuilder AddSqlServerDefaultValue(Column column, PropertyBuilder propertyBuilder)
        {
            if (column.DefaultValue != null)
            {
                // unset default
                propertyBuilder.Metadata.ValueGenerated = null;
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
                            column.DisplayName(), column.DefaultValue,
                            property.ClrType, property.Name, property.DeclaringEntityType.Name));
                }
            }
            return propertyBuilder;
        }
    }
}
