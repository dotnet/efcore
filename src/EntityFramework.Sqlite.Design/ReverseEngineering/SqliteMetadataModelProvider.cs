// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Data.Entity.Metadata.Conventions;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Relational.Design;
using Microsoft.Data.Entity.Relational.Design.Model;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;
using Microsoft.Data.Entity.Relational.Design.Utilities;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Extensions.Logging;
using ForeignKey = Microsoft.Data.Entity.Relational.Design.Model.ForeignKey;

namespace Microsoft.Data.Entity.Sqlite.Design.ReverseEngineering
{
    public class SqliteMetadataModelProvider : RelationalMetadataModelProvider
    {
        private readonly IMetadataReader _metadataReader;
        private readonly SqliteReverseTypeMapper _typeMapper;

        public SqliteMetadataModelProvider(
            [NotNull] SqliteReverseTypeMapper typeMapper,
            [NotNull] ILoggerFactory loggerFactory,
            [NotNull] ModelUtilities modelUtilities,
            [NotNull] CSharpUtilities cSharpUtilities,
            [NotNull] IMetadataReader metadataReader
            )
            : base(loggerFactory, modelUtilities, cSharpUtilities)
        {
            Check.NotNull(typeMapper, nameof(typeMapper));
            Check.NotNull(metadataReader, nameof(metadataReader));

            _typeMapper = typeMapper;
            _metadataReader = metadataReader;
        }

        protected override IRelationalAnnotationProvider ExtensionsProvider => new SqliteAnnotationProvider();

        public override IModel ConstructRelationalModel([NotNull] string connectionString)
        {
            Check.NotEmpty(connectionString, nameof(connectionString));

            var databaseInfo = _metadataReader.GetSchema(connectionString, _tableSelectionSet);

            return GetModel(databaseInfo);
        }

        private IModel GetModel(SchemaInfo schemaInfo)
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            AddEntityTypes(modelBuilder, schemaInfo);

            return modelBuilder.Model;
        }

        private void AddEntityTypes(ModelBuilder modelBuilder, SchemaInfo schemaInfo)
        {
            foreach (var table in schemaInfo.Tables)
            {
                modelBuilder.Entity(table.Name, builder =>
                    {
                        builder.ToTable(table.Name);

                        AddColumns(builder, table);
                        AddIndexes(builder, table);
                    });
            }

            foreach (var table in schemaInfo.Tables)
            {
                AddForeignKeys(modelBuilder, table);
            }
        }

        private void AddColumns(EntityTypeBuilder builder, Table table)
        {
            foreach (var column in table.Columns)
            {
                AddColumn(builder, column);
            }

            var keyProps = table.Columns
                .Where(c => c.IsPrimaryKey)
                .Select(c => c.Name).ToArray();

            if (keyProps.Length > 0)
            {
                builder.HasKey(keyProps);
            }
            else
            {
                var errorMessage = SqliteDesignStrings.MissingPrimaryKey(table.Name);
                builder.Metadata.AddAnnotation(AnnotationNameEntityTypeError, errorMessage);
                Logger.LogWarning(errorMessage);
            }
        }

        private void AddColumn(EntityTypeBuilder builder, Column column)
        {
            // TODO log bad datatypes
            var clrType = _typeMapper.GetClrType(column.DataType, nullable: column.IsNullable);
            var property = builder.Property(clrType, column.Name)
                .HasColumnName(column.Name);

            // TODO don't need to add unless the ClrType and DataType are not the same
            // but this always needs to be added for SQLite
            if (!string.IsNullOrEmpty(column.DataType))
            {
                property.HasColumnType(column.DataType);
            }

            if (column.DefaultValue != null)
            {
                property.HasDefaultValueSql(column.DefaultValue);
            }

            if (!column.IsPrimaryKey)
            {
                property.IsRequired(!column.IsNullable);
            }
        }

        private void AddIndexes(EntityTypeBuilder entity, Table table)
        {
            foreach (var index in table.Indexes)
            {
                var columns = index.Columns.Select(c => c.Name).ToArray();

                entity.HasIndex(columns)
                    .IsUnique(index.IsUnique)
                    .HasName(index.Name);

                if (index.IsUnique)
                {
                    entity.HasAlternateKey(columns);
                }
            }
        }

        private void AddForeignKeys(ModelBuilder modelBuilder, Table table)
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
                    var dependentEntityType = modelBuilder.Model.GetEntityType(fkInfo.Table.Name);
                    var principalEntityType = modelBuilder.Model.GetEntityType(fkInfo.PrincipalTable.Name);

                    var principalProps = fkInfo.To
                        .Select(to => principalEntityType.GetProperty(to.Name))
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
                        .Select(
                            @from => dependentEntityType.GetProperty(from.Name)
                        )
                        .ToList()
                        .AsReadOnly();

                    var foreignKey = dependentEntityType.GetOrAddForeignKey(depProps, principalKey, principalEntityType);

                    if (dependentEntityType.FindIndex(depProps)?.IsUnique == true
                        || dependentEntityType.GetKeys().Any(k => k.Properties.All(p => depProps.Contains(p))))
                    {
                        foreignKey.IsUnique = true;
                    }
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
        }

        private void LogFailedForeignKey(ForeignKey foreignKey)
            => Logger.LogWarning(SqliteDesignStrings.ForeignKeyScaffoldError(foreignKey.Table.Name, string.Join(",", foreignKey.From.Select(f => f.Name))));
    }
}
