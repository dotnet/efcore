// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Data.Entity.Relational.Design.Model;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Internal;
using Microsoft.Data.Entity.SqlServer.Design.Utilities;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Extensions.Logging;

namespace Microsoft.Data.Entity.SqlServer.Design.ReverseEngineering
{
    public class SqlServerMetadataModelProvider : RelationalMetadataModelProvider
    {
        private readonly IMetadataReader _metadataReader;
        private readonly SqlServerLiteralUtilities _sqlServerLiteralUtilities;

        private const int DefaultDateTimePrecision = 7;
        private static readonly ISet<string> _dateTimePrecisionTypes = new HashSet<string> { "datetimeoffset", "datetime2", "time" };

        public SqlServerMetadataModelProvider(
            [NotNull] ILoggerFactory loggerFactory,
            [NotNull] IRelationalTypeMapper typeMapper,
            [NotNull] IMetadataReader metadataReader,
            [NotNull] SqlServerLiteralUtilities sqlServerLiteralUtilities)
            : base(loggerFactory, typeMapper)
        {
            Check.NotNull(metadataReader, nameof(metadataReader));
            Check.NotNull(sqlServerLiteralUtilities, nameof(sqlServerLiteralUtilities));

            _metadataReader = metadataReader;
            _sqlServerLiteralUtilities = sqlServerLiteralUtilities;
        }

        public override IModel GetModel([NotNull] string connectionString, [CanBeNull] TableSelectionSet tableSelectionSet)
        {
            Check.NotEmpty(connectionString, nameof(connectionString));

            var schemaInfo = _metadataReader.GetSchema(connectionString, tableSelectionSet ?? TableSelectionSet.InclusiveAll);

            return GetModel(schemaInfo);
        }

        protected override PropertyBuilder AddColumn([NotNull] EntityTypeBuilder builder, [NotNull] Column column)
        {
            var propertyBuilder = base.AddColumn(builder, column);

            AddSqlServerTypeMapping(column, propertyBuilder);

            AddSqlServerDefaultValue(column, propertyBuilder);

            if (column.DataType == "timestamp")
            {
                // synonym for rowversion
                propertyBuilder.ValueGeneratedOnAddOrUpdate();
            }

            return propertyBuilder;
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
