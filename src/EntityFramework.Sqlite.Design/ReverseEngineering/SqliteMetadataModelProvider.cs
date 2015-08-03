// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Conventions;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;
using Microsoft.Data.Entity.Relational.Design.Utilities;
using Microsoft.Data.Entity.Sqlite.Metadata;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Sqlite;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Sqlite.Design.ReverseEngineering
{
    public class SqliteMetadataModelProvider : ReverseEngineeringMetadataModelProvider
    {
        private readonly SqliteReverseTypeMapper _typeMapper;

        public SqliteMetadataModelProvider(
            [NotNull] ILogger logger,
            [NotNull] ModelUtilities modelUtilities,
            [NotNull] SqliteReverseTypeMapper typeMapper)
            : base(logger, modelUtilities)
        {
            Check.NotNull(typeMapper, nameof(typeMapper));

            _typeMapper = typeMapper;
        }

        protected override IRelationalMetadataExtensionProvider ExtensionsProvider => new SqliteMetadataExtensionProvider();

        public override DbContextCodeGeneratorHelper DbContextCodeGeneratorHelper(DbContextGeneratorModel model) => new SqliteContextCodeGeneratorHelper(model, ExtensionsProvider);

        public override IModel ConstructRelationalModel([NotNull] string connectionString)
        {
            Check.NotEmpty(connectionString, nameof(connectionString));

            var modelBuilder = new ModelBuilder(new ConventionSet());

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var tables = new Dictionary<string, string>();
                var indexes = new List<SqliteIndex>();
                var master = connection.CreateCommand();
                master.CommandText = "SELECT type, name, sql, tbl_name FROM sqlite_master";
                using (var reader = master.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var type = reader.GetString(0);
                        var name = reader.GetString(1);
                        var sql = reader.GetValue(2) as string; // can be null
                        var tableName = reader.GetString(3);

                        if (type == "table"
                            && name != "sqlite_sequence")
                        {
                            tables.Add(name, sql);
                        }
                        else if (type == "index")
                        {
                            indexes.Add(new SqliteIndex
                                {
                                    Name = name,
                                    TableName = tableName,
                                    Sql = sql
                                });
                        }
                    }
                }

                LoadTablesAndColumns(connection, modelBuilder, tables.Keys);
                LoadIndexes(connection, modelBuilder, indexes);
                LoadForeignKeys(connection, modelBuilder, tables.Keys);

                foreach (var item in tables)
                {
                    SqliteDmlParser.ParseTableDefinition(modelBuilder, item.Key, item.Value);
                }
            }

            return modelBuilder.Model;
        }

        private enum ForeignKeyList
        {
            Id,
            Seq,
            Table,
            From,
            To,
            OnUpdate,
            OnDelete,
            Match
        }

        private void LoadForeignKeys(SqliteConnection connection, ModelBuilder modelBuilder, ICollection<string> tables)
        {
            foreach (var tableName in tables)
            {
                var fkList = connection.CreateCommand();
                fkList.CommandText = $"PRAGMA foreign_key_list(\"{tableName.Replace("\"", "\"\"")}\");";

                var foreignKeys = new Dictionary<int, ForeignKey>();
                using (var reader = fkList.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var id = reader.GetInt32((int)ForeignKeyList.Id);
                        var refTable = reader.GetString((int)ForeignKeyList.Table);
                        ForeignKey foreignKey;
                        if (!foreignKeys.TryGetValue(id, out foreignKey))
                        {
                            foreignKeys.Add(id, (foreignKey = new ForeignKey { Table = tableName, ReferencedTable = refTable }));
                        }
                        foreignKey.From.Add(reader.GetString((int)ForeignKeyList.From));
                        foreignKey.To.Add(reader.GetString((int)ForeignKeyList.To));
                    }
                }

                var entityType = modelBuilder.Entity(tableName).Metadata;

                foreach (var foreignKey in foreignKeys.Values)
                {
                    try
                    {
                        var referenceType = modelBuilder.Model.EntityTypes.First(e => e.Name.Equals(foreignKey.ReferencedTable, StringComparison.OrdinalIgnoreCase));

                        var principalProps = new List<Property>();
                        foreach (var to in foreignKey.To)
                        {
                            var prop = referenceType.Properties.First(p => p.Sqlite().ColumnName.Equals(to, StringComparison.OrdinalIgnoreCase));
                            principalProps.Add(prop);
                        }

                        var principalKey = referenceType.GetOrAddKey(principalProps.AsReadOnly());

                        var depProps = new List<Property>();

                        foreach (var from in foreignKey.From)
                        {
                            var prop = entityType.Properties.First(p => p.Sqlite().ColumnName.Equals(from, StringComparison.OrdinalIgnoreCase));
                            depProps.Add(prop);
                        }

                        entityType.GetOrAddForeignKey(depProps.AsReadOnly(), principalKey, referenceType);
                    }
                    catch (InvalidOperationException)
                    {
                        LogFailedForeignKey(foreignKey);
                    }
                }
            }
        }

        private enum IndexInfo
        {
            Seqno,
            Cid,
            Name
        }

        private void LogFailedForeignKey(ForeignKey foreignKey)
            => Logger.LogWarning(Strings.ForeignKeyScaffoldError(foreignKey.Table, string.Join(",", foreignKey.From)));

        private void LoadIndexes(SqliteConnection connection, ModelBuilder modelBuilder, ICollection<SqliteIndex> indexes)
        {
            foreach (var index in indexes)
            {
                var indexInfo = connection.CreateCommand();
                indexInfo.CommandText = $"PRAGMA index_info(\"{index.Name.Replace("\"", "\"\"")}\");";

                var indexProps = new List<string>();
                using (var reader = indexInfo.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var name = reader.GetValue((int)IndexInfo.Name) as string;
                        if (!string.IsNullOrEmpty(name))
                        {
                            indexProps.Add(name);
                        }
                    }
                }

                if (indexProps.Count > 0)
                {
                    var indexBuilder = modelBuilder.Entity(index.TableName)
                        .Index(indexProps.ToArray())
                        .SqliteIndexName(index.Name);

                    if (!string.IsNullOrEmpty(index.Sql))
                    {
                        var uniqueKeyword = index.Sql.IndexOf("UNIQUE", StringComparison.OrdinalIgnoreCase);
                        var indexKeyword = index.Sql.IndexOf("INDEX", StringComparison.OrdinalIgnoreCase);

                        indexBuilder.Unique(uniqueKeyword > 0 && uniqueKeyword < indexKeyword);
                    }
                }
            }
        }

        private enum TableInfo
        {
            Cid,
            Name,
            Type,
            NotNull,
            DefaultValue,
            Pk
        }

        private void LoadTablesAndColumns(SqliteConnection connection, ModelBuilder modelBuilder, ICollection<string> tables)
        {
            foreach (var tableName in tables)
            {
                modelBuilder.Entity(tableName, builder =>
                    {
                        builder.ToTable(tableName);

                        var tableInfo = connection.CreateCommand();
                        tableInfo.CommandText = $"PRAGMA table_info(\"{tableName.Replace("\"", "\"\"")}\");";

                        var keyProps = new List<string>();

                        using (var reader = tableInfo.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var colName = reader.GetString((int)TableInfo.Name);
                                var typeName = reader.GetString((int)TableInfo.Type);

                                var clrType = _typeMapper.GetClrType(typeName);

                                var property = builder.Property(clrType, colName)
                                    .HasColumnName(colName);
                                if (!string.IsNullOrEmpty(typeName))
                                {
                                    property.HasColumnType(typeName);
                                }

                                var defaultVal = reader.GetValue((int)TableInfo.DefaultValue) as string;

                                if (!string.IsNullOrEmpty(defaultVal))
                                {
                                    property.HasDefaultValueSql(defaultVal);
                                }

                                if (reader.GetBoolean((int)TableInfo.NotNull))
                                {
                                    property.Required();
                                }

                                if (reader.GetBoolean((int)TableInfo.Pk))
                                {
                                    keyProps.Add(colName);
                                }
                            }
                        }

                        if (keyProps.Count > 0)
                        {
                            builder.Key(keyProps.ToArray());
                        }
                    });
            }
        }

        private class SqliteIndex
        {
            public string Name { get; set; }
            public string TableName { get; set; }
            public string Sql { get; set; }
        }

        private class ForeignKey
        {
            public string Table { get; set; }
            public string ReferencedTable { get; set; }
            public List<string> From { get; } = new List<string>();
            public List<string> To { get; } = new List<string>();

            // TODO foreign key triggers
            //public string OnUpdate { get; set; }

            // TODO https://github.com/aspnet/EntityFramework/issues/333
            //public string OnDelete { get; set; }
        }
    }
}
