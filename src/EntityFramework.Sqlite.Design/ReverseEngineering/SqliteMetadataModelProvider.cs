// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Conventions;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;
using Microsoft.Data.Entity.Relational.Design.Utilities;
using Microsoft.Data.Entity.Sqlite.Design.ReverseEngineering.Model;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace Microsoft.Data.Entity.Sqlite.Design.ReverseEngineering
{
    public class SqliteMetadataModelProvider : RelationalMetadataModelProvider
    {
        private readonly SqliteMetadataReader _metadataReader;

        public SqliteMetadataModelProvider(
            [NotNull] ILoggerFactory loggerFactory,
            [NotNull] ModelUtilities modelUtilities,
            [NotNull] CSharpUtilities cSharpUtilities,
            [NotNull] SqliteMetadataReader metadataReader
            )
            : base(loggerFactory, modelUtilities, cSharpUtilities)
        {
            Check.NotNull(metadataReader, nameof(metadataReader));

            _metadataReader = metadataReader;
        }

        protected override IRelationalAnnotationProvider ExtensionsProvider => new SqliteAnnotationProvider();

        public override IModel ConstructRelationalModel([NotNull] string connectionString)
        {
            Check.NotEmpty(connectionString, nameof(connectionString));

            var modelBuilder = new ModelBuilder(new ConventionSet());

            DatabaseInfo databaseInfo;

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                databaseInfo = _metadataReader.ReadDatabaseInfo(connection, _tableSelectionSet);
            }

            LoadEntityTypes(modelBuilder, databaseInfo);
            LoadIndexes(modelBuilder, databaseInfo);
            LoadForeignKeys(modelBuilder, databaseInfo);

            return modelBuilder.Model;
        }

        private void LoadForeignKeys(ModelBuilder modelBuilder, DatabaseInfo databaseInfo)
        {
            foreach (var fkInfo in databaseInfo.ForeignKeys)
            {
                var dependentEntityType = modelBuilder.Entity(fkInfo.TableName).Metadata;

                try
                {
                    var principalEntityType = modelBuilder.Model.EntityTypes.First(e => e.Name.Equals(fkInfo.PrincipalTableName, StringComparison.OrdinalIgnoreCase));

                    var principalProps = fkInfo.To
                        .Select(to => principalEntityType
                            .Properties
                            .First(p => p.Sqlite().ColumnName.Equals(to, StringComparison.OrdinalIgnoreCase))
                        )
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
                            @from => dependentEntityType
                                .Properties.
                                First(p => p.Sqlite().ColumnName.Equals(@from, StringComparison.OrdinalIgnoreCase))
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
                catch (InvalidOperationException)
                {
                    LogFailedForeignKey(fkInfo);
                }
            }
        }

        private void LogFailedForeignKey(ForeignKeyInfo foreignKey)
            => Logger.LogWarning(SqliteDesignStrings.ForeignKeyScaffoldError(foreignKey.TableName, string.Join(",", foreignKey.From)));

        private void LoadIndexes(ModelBuilder modelBuilder, DatabaseInfo databaseInfo)
        {
            foreach (var index in databaseInfo.Indexes)
            {
                var columns = index.Columns.ToArray();
                modelBuilder.Entity(index.TableName, entity =>
                    {
                        entity.HasIndex(columns)
                            .IsUnique(index.IsUnique)
                            .ForSqliteHasName(index.Name);

                        if (index.IsUnique)
                        {
                            entity.HasAlternateKey(columns);
                        }
                    });
            }
        }

        private void LoadEntityTypes(ModelBuilder modelBuilder, DatabaseInfo databaseInfo)
        {
            foreach (var table in databaseInfo.Tables)
            {
                var tableName = table.Name;

                modelBuilder.Entity(tableName, builder =>
                    {
                        builder.ToTable(tableName);

                        var keyProps = new List<string>();

                        foreach (var column in databaseInfo.Columns.Where(c => c.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase)))
                        {
                            var property = builder.Property(column.ClrType, column.Name)
                                .ForSqliteHasColumnName(column.Name);

                            if (!string.IsNullOrEmpty(column.DataType))
                            {
                                property.ForSqliteHasColumnType(column.DataType);
                            }

                            if (!string.IsNullOrEmpty(column.DefaultValue))
                            {
                                property.ForSqliteHasDefaultValueSql(column.DefaultValue);
                            }

                            if (column.IsPrimaryKey)
                            {
                                keyProps.Add(column.Name);
                            }
                            else
                            {
                                property.IsRequired(!column.IsNullable);
                            }
                        }

                        if (keyProps.Count > 0)
                        {
                            builder.HasKey(keyProps.ToArray());
                        }
                        else
                        {
                            var errorMessage = SqliteDesignStrings.MissingPrimaryKey(tableName);
                            builder.Metadata.AddAnnotation(AnnotationNameEntityTypeError, errorMessage);
                            Logger.LogWarning(errorMessage);
                        }
                    });
            }
        }
    }
}
