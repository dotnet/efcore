// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqlServerModelValidator : RelationalModelValidator
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SqlServerModelValidator(
            [NotNull] ModelValidatorDependencies dependencies,
            [NotNull] RelationalModelValidatorDependencies relationalDependencies)
            : base(dependencies, relationalDependencies)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void Validate(IModel model)
        {
            base.Validate(model);

            ValidateDefaultDecimalMapping(model);
            ValidateByteIdentityMapping(model);
            ValidateNonKeyValueGeneration(model);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual void ValidateDefaultDecimalMapping([NotNull] IModel model)
        {
            foreach (var property in model.GetEntityTypes()
                .SelectMany(t => t.GetDeclaredProperties())
                .Where(p => p.ClrType.UnwrapNullableType() == typeof(decimal)
                            && !p.IsForeignKey()))
            {
                var type = property.FindAnnotation(RelationalAnnotationNames.ColumnType) as ConventionalAnnotation;
                var typeMapping = property.FindAnnotation(CoreAnnotationNames.TypeMapping) as ConventionalAnnotation;
                if ((type == null
                     && (typeMapping == null
                         || ConfigurationSource.Convention.Overrides(typeMapping.GetConfigurationSource())))
                    || (type != null
                        && ConfigurationSource.Convention.Overrides(type.GetConfigurationSource())))
                {
                    Dependencies.Logger.DecimalTypeDefaultWarning(property);
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual void ValidateByteIdentityMapping([NotNull] IModel model)
        {
            foreach (var property in model.GetEntityTypes()
                .SelectMany(t => t.GetDeclaredProperties())
                .Where(
                    p => p.ClrType.UnwrapNullableType() == typeof(byte)
                         && p.SqlServer().ValueGenerationStrategy == SqlServerValueGenerationStrategy.IdentityColumn))
            {
                Dependencies.Logger.ByteIdentityColumnWarning(property);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual void ValidateNonKeyValueGeneration([NotNull] IModel model)
        {
            foreach (var property in model.GetEntityTypes()
                .SelectMany(t => t.GetDeclaredProperties())
                .Where(
                    p => ((SqlServerPropertyAnnotations)p.SqlServer()).GetSqlServerValueGenerationStrategy(fallbackToModel: false)
                         == SqlServerValueGenerationStrategy.SequenceHiLo
                         && !p.IsKey()
                         && p.ValueGenerated != ValueGenerated.Never
                         && (!(p.FindAnnotation(SqlServerAnnotationNames.ValueGenerationStrategy) is ConventionalAnnotation strategy)
                             || !ConfigurationSource.Convention.Overrides(strategy.GetConfigurationSource()))))
            {
                throw new InvalidOperationException(
                    SqlServerStrings.NonKeyValueGeneration(property.Name, property.DeclaringEntityType.DisplayName()));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override void ValidateSharedTableCompatibility(
            IReadOnlyList<IEntityType> mappedTypes, string tableName)
        {
            var firstMappedType = mappedTypes[0];
            var isMemoryOptimized = firstMappedType.SqlServer().IsMemoryOptimized;

            foreach (var otherMappedType in mappedTypes.Skip(1))
            {
                if (isMemoryOptimized != otherMappedType.SqlServer().IsMemoryOptimized)
                {
                    throw new InvalidOperationException(
                        SqlServerStrings.IncompatibleTableMemoryOptimizedMismatch(
                            tableName, firstMappedType.DisplayName(), otherMappedType.DisplayName(),
                            isMemoryOptimized ? firstMappedType.DisplayName() : otherMappedType.DisplayName(),
                            !isMemoryOptimized ? firstMappedType.DisplayName() : otherMappedType.DisplayName()));
                }
            }

            base.ValidateSharedTableCompatibility(mappedTypes, tableName);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override void ValidateSharedColumnsCompatibility(IReadOnlyList<IEntityType> mappedTypes, string tableName)
        {
            base.ValidateSharedColumnsCompatibility(mappedTypes, tableName);

            var identityColumns = new List<IProperty>();
            var propertyMappings = new Dictionary<string, IProperty>();

            foreach (var property in mappedTypes.SelectMany(et => et.GetDeclaredProperties()))
            {
                var propertyAnnotations = property.Relational();
                var columnName = propertyAnnotations.ColumnName;
                if (propertyMappings.TryGetValue(columnName, out var duplicateProperty))
                {
                    var propertyStrategy = property.SqlServer().ValueGenerationStrategy;
                    var duplicatePropertyStrategy = duplicateProperty.SqlServer().ValueGenerationStrategy;
                    if (propertyStrategy != duplicatePropertyStrategy
                        && (propertyStrategy == SqlServerValueGenerationStrategy.IdentityColumn
                            || duplicatePropertyStrategy == SqlServerValueGenerationStrategy.IdentityColumn))
                    {
                        throw new InvalidOperationException(
                            SqlServerStrings.DuplicateColumnNameValueGenerationStrategyMismatch(
                                duplicateProperty.DeclaringEntityType.DisplayName(),
                                duplicateProperty.Name,
                                property.DeclaringEntityType.DisplayName(),
                                property.Name,
                                columnName,
                                tableName));
                    }
                }
                else
                {
                    propertyMappings[columnName] = property;
                    if (property.SqlServer().ValueGenerationStrategy == SqlServerValueGenerationStrategy.IdentityColumn)
                    {
                        identityColumns.Add(property);
                    }
                }
            }

            if (identityColumns.Count > 1)
            {
                var sb = new StringBuilder()
                    .AppendJoin(identityColumns.Select(p => "'" + p.DeclaringEntityType.DisplayName() + "." + p.Name + "'"));
                throw new InvalidOperationException(SqlServerStrings.MultipleIdentityColumns(sb, tableName));
            }
        }
    }
}
