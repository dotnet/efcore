// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Data.Entity.Metadata.Conventions;
using Microsoft.Data.Entity.Relational.Design.Model;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Extensions.Logging;
using ForeignKey = Microsoft.Data.Entity.Relational.Design.Model.ForeignKey;
using Index = Microsoft.Data.Entity.Relational.Design.Model.Index;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering
{
    public abstract class RelationalMetadataModelProvider : MetadataModelProvider
    {
        internal static IReadOnlyList<string> IgnoredAnnotations { get; } = new List<string>
        {
            CoreAnnotationNames.OriginalValueIndexAnnotation,
            CoreAnnotationNames.ShadowIndexAnnotation
        };

        private Dictionary<Table, CSharpUniqueNamer<Column>> _columnNamers;
        private readonly Table _nullTable = new Table { };
        private CSharpUniqueNamer<Table> _tableNamer;
        private readonly IRelationalTypeMapper _typeMapper;

        protected RelationalMetadataModelProvider(
            [NotNull] ILoggerFactory loggerFactory,
            [NotNull] IRelationalTypeMapper typeMapper)
            : base(loggerFactory)
        {
            Check.NotNull(typeMapper, nameof(typeMapper));

            _typeMapper = typeMapper;
        }

        protected virtual IModel GetModel([NotNull] SchemaInfo schemaInfo)
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            _tableNamer = new CSharpUniqueNamer<Table>(t => t.Name);
            _columnNamers = new Dictionary<Table, CSharpUniqueNamer<Column>>();

            AddEntityTypes(modelBuilder, schemaInfo);

            return modelBuilder.Model;
        }

        protected virtual string GetEntityTypeName([NotNull] Table table)
            => _tableNamer.GetName(Check.NotNull(table, nameof(table)));

        protected virtual string GetPropertyName([NotNull] Column column)
        {
            var table = column.Table ?? _nullTable;

            if (!_columnNamers.ContainsKey(table))
            {
                _columnNamers.Add(table, new CSharpUniqueNamer<Column>(c => c.Name));
            }

            return _columnNamers[table].GetName(column);
        }

        protected virtual ModelBuilder AddEntityTypes([NotNull] ModelBuilder modelBuilder, [NotNull] SchemaInfo schemaInfo)
        {
            foreach (var table in schemaInfo.Tables)
            {
                modelBuilder.Entity(GetEntityTypeName(table), builder =>
                    {
                        builder.ToTable(table.Name, table.SchemaName);

                        AddColumns(builder, table);
                        AddPrimaryKey(builder, table);
                        AddIndexes(builder, table);
                    });
            }

            // TODO can we add navigation properties inline with adding foreign keys?
            foreach (var table in schemaInfo.Tables)
            {
                AddForeignKeys(modelBuilder, table);
            }

            AddNavigationProperties(modelBuilder.Model);
            return modelBuilder;
        }

        protected virtual EntityTypeBuilder AddColumns([NotNull] EntityTypeBuilder builder, [NotNull] Table table)
        {
            foreach (var column in table.Columns)
            {
                try
                {
                    AddColumn(builder, column);
                }
                catch (NotSupportedException)
                {
                    LogUnmappableColumn(column);
                }
            }
            return builder;
        }

        protected virtual PropertyBuilder AddColumn([NotNull] EntityTypeBuilder builder, [NotNull] Column column)
        {
            RelationalTypeMapping mapping;

            if (!_typeMapper.TryGetMapping(column.DataType, out mapping)
                || mapping.ClrType == null)
            {
                throw new NotSupportedException();
            }

            var clrType = (column.IsNullable) ? mapping.ClrType.MakeNullable() : mapping.ClrType;

            var property = builder.Property(clrType, GetPropertyName(column));

            if (_typeMapper.GetMapping(property.Metadata).DefaultTypeName != column.DataType
                && !string.IsNullOrWhiteSpace(column.DataType))
            {
                property.HasColumnType(column.DataType);
            }

            property.HasColumnName(column.Name);

            if (column.MaxLength.HasValue)
            {
                property.HasMaxLength(column.MaxLength.Value);
            }

            if (column.IsStoreGenerated == true)
            {
                property.ValueGeneratedOnAdd();
            }

            if (column.IsComputed == true)
            {
                property.ValueGeneratedOnAddOrUpdate();
            }

            if (column.DefaultValue != null)
            {
                property.HasDefaultValueSql(column.DefaultValue);
            }

            if (!column.PrimaryKeyOrdinal.HasValue)
            {
                property.IsRequired(!column.IsNullable);
            }

            return property;
        }

        protected virtual EntityTypeBuilder AddPrimaryKey([NotNull] EntityTypeBuilder builder, [NotNull] Table table)
        {
            var keyProps = table.Columns
                .Where(c => c.PrimaryKeyOrdinal.HasValue)
                .OrderBy(c => c.PrimaryKeyOrdinal)
                .Select(GetPropertyName)
                .ToArray();

            if (keyProps.Length > 0)
            {
                try
                {
                    builder.HasKey(keyProps);
                    return builder;
                }
                catch (ModelItemNotFoundException)
                {
                    // swallow. Handled by logging
                }
            }

            var errorMessage = RelationalDesignStrings.MissingPrimaryKey(table.DisplayName());
            Logger.LogWarning(errorMessage);

            builder.Metadata.AddAnnotation(AnnotationNameEntityTypeError, RelationalDesignStrings.UnableToGenerateEntityType(builder.Metadata.DisplayName(), errorMessage));

            return builder;
        }

        protected virtual EntityTypeBuilder AddIndexes([NotNull] EntityTypeBuilder builder, [NotNull] Table table)
        {
            foreach (var index in table.Indexes)
            {
                try
                {
                    AddIndex(builder, index);
                }
                catch (ModelItemNotFoundException)
                {
                    Logger.LogWarning(RelationalDesignStrings.UnableToScaffoldIndex(index.Name));
                }
            }

            return builder;
        }

        private IndexBuilder AddIndex(EntityTypeBuilder builder, Index index)
        {
            var properties = index.Columns.Select(GetPropertyName).ToArray();

            var indexBuilder = builder.HasIndex(properties).IsUnique(index.IsUnique);

            if (!string.IsNullOrEmpty(index.Name))
            {
                indexBuilder.HasName(index.Name);
            }

            if (index.IsUnique)
            {
                var keyBuilder = builder.HasAlternateKey(properties);
                if (!string.IsNullOrEmpty(index.Name))
                {
                    keyBuilder.HasName(index.Name);
                }
            }

            return indexBuilder;
        }

        protected virtual ModelBuilder AddForeignKeys([NotNull] ModelBuilder modelBuilder, [NotNull] Table table)
        {
            foreach (var fkInfo in table.ForeignKeys)
            {
                if (fkInfo.PrincipalTable == null)
                {
                    LogFailedForeignKey(fkInfo);
                    continue;
                }

                try
                {
                    var dependentEntityType = modelBuilder.Model.GetEntityType(GetEntityTypeName(fkInfo.Table));
                    var principalEntityType = modelBuilder.Model.GetEntityType(GetEntityTypeName(fkInfo.PrincipalTable));

                    var principalProps = fkInfo.To
                        .Select(GetPropertyName)
                        .Select(to => principalEntityType.GetProperty(to))
                        .ToList()
                        .AsReadOnly();

                    var principalKey = principalEntityType.FindKey(principalProps);
                    if (principalKey == null)
                    {
                        var index = principalEntityType.FindIndex(principalProps);
                        if (index != null
                            && index.IsUnique == true)
                        {
                            principalKey = principalEntityType.AddKey(principalProps);
                        }
                        else
                        {
                            LogFailedForeignKey(fkInfo);
                            continue;
                        }
                    }

                    var depProps = fkInfo.From
                        .Select(GetPropertyName)
                        .Select(@from => dependentEntityType.GetProperty(@from))
                        .ToList()
                        .AsReadOnly();

                    var foreignKey = dependentEntityType.GetOrAddForeignKey(depProps, principalKey, principalEntityType);

                    foreignKey.IsUnique = dependentEntityType.FindKey(depProps) != null;
                }
                catch (Exception ex)
                {
                    if (ex is ModelItemNotFoundException
                        || ex is InvalidOperationException)
                    {
                        LogFailedForeignKey(fkInfo);
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return modelBuilder;
        }

        protected virtual void LogFailedForeignKey([NotNull] ForeignKey foreignKey)
            => Logger.LogWarning(
                RelationalDesignStrings.ForeignKeyScaffoldError(foreignKey.DisplayName()));

        protected virtual void LogUnmappableColumn([NotNull] Column column)
            => Logger.LogWarning(
                RelationalDesignStrings.CannotFindTypeMappingForColumn(
                    column.DisplayName(), column.DataType));
    }
}
