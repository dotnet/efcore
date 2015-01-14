// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.ReverseEngineering;
using EntityFramework.SqlServer.ReverseEngineering.Model;

namespace EntityFramework.SqlServer.ReverseEngineering
{
    public class SqlServerMetadataModelProvider : IDatabaseMetadataModelProvider
    {
        public static readonly Dictionary<string, Type> _sqlTypeToClrTypeMap
            = new Dictionary<string, Type>()
                {
                    // exact numerics
                    { "bigint", typeof(long) },
                    { "bit", typeof(byte) },
                    { "decimal", typeof(decimal) },
                    { "int", typeof(int) },
                    //TODO { "money", typeof(decimal) },
                    { "numeric", typeof(decimal) },
                    { "smallint", typeof(short) },
                    //TODO{ "smallmoney", typeof(decimal) },
                    { "tinyint", typeof(byte) },

                    // approximate numerics
                    { "float", typeof(float) },
                    { "real", typeof(double) },

                    // date and time
                    { "date", typeof(DateTime) },
                    { "datetime", typeof(DateTime) },
                    { "datetime2", typeof(DateTime) },
                    { "datetimeoffset", typeof(DateTimeOffset) },
                    { "smalldatetime", typeof(DateTime) },
                    { "time", typeof(DateTime) },

                    // character strings
                    { "char", typeof(string) },
                    { "text", typeof(string) },
                    { "varchar", typeof(string) },

                    // unicode character strings
                    { "nchar", typeof(string) },
                    { "ntext", typeof(string) },
                    { "nvarchar", typeof(string) },

                    // binary
                    { "binary", typeof(byte[]) },
                    { "image", typeof(byte[]) },
                    { "varbinary", typeof(byte[]) },

                    //TODO other
                    //{ "cursor", typeof(yyy) },
                    //{ "hierarchyid", typeof(yyy) },
                    //{ "sql_variant", typeof(yyy) },
                    //{ "table", typeof(yyy) },
                    //{ "timestamp", typeof(yyy) },
                    //{ "uniqueidentifier", typeof(yyy) },
                    //{ "xml", typeof(yyy) },

                    //TODO spatial
                };

        public IModel GenerateMetadataModel(string connectionString, string filters)
        {
            Dictionary<string, Table> tables;
            Dictionary<string, TableColumn> tableColumns;
            Dictionary<string, TableConstraintColumn> tableConstraintColumns;
            using (var conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    tables = LoadData<Table>(conn, Table.Query, Table.CreateFromReader, t => t.Id);
                    tableColumns = LoadData<TableColumn>(conn, TableColumn.Query, TableColumn.CreateFromReader, tc => tc.Id);
                    tableConstraintColumns = LoadData<TableConstraintColumn>(
                        conn, TableConstraintColumn.Query, TableConstraintColumn.CreateFromReader, tc => tc.Id);
                }
                finally
                {
                    if (conn != null)
                    {
                        if (conn.State == ConnectionState.Open)
                        {
                            try
                            {
                                conn.Close();
                            }
                            catch (SqlException)
                            {
                                // do nothing if attempt to close connection fails
                            }
                        }
                    }
                }
            }

            //Console.WriteLine("Tables");
            //foreach (var t in tables)
            //{
            //    var table = t.Value;
            //    Console.WriteLine(table.ToString());
            //}

            //Console.WriteLine("Columns");
            //foreach (var tc in tableColumns)
            //{
            //    Console.WriteLine(tc.Value.ToString());
            //}

            //Console.WriteLine("Constraint Columns");
            //foreach (var tc in tableConstraintColumns)
            //{
            //    Console.WriteLine(tc.Value.ToString());
            //}

            return CreateModel(tables, tableColumns, tableConstraintColumns);
        }

        public static Dictionary<string, T> LoadData<T>(
            SqlConnection conn, string query, Func<SqlDataReader, T> createFromReader, Func<T, string> identifier)
        {
            var data = new Dictionary<string, T>();
            var sqlCommand = new SqlCommand(query);
            sqlCommand.Connection = conn;

            using (var reader = sqlCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    var item = createFromReader(reader);
                    data.Add(identifier(item), item);
                }
            }

            return data;
        }

        public static IModel CreateModel(
            Dictionary<string, Table> tables,
            Dictionary<string, TableColumn> tableColumns,
            Dictionary<string, TableConstraintColumn> tableConstraintColumns)
        {
            var model = new Microsoft.Data.Entity.Metadata.Model();
            foreach (var t in tables)
            {
                var table = t.Value;
                var entityType = model.AddEntityType(EscapeForCSharp(table.SchemaName) + "." + EscapeForCSharp(table.TableName));
                var primaryKeys = new List<Property>();
                foreach (var tc in tableColumns.Values.Where(col => col.ParentId == table.Id))
                {
                    Type clrPropertyType;
                    if (_sqlTypeToClrTypeMap.TryGetValue(tc.DataType, out clrPropertyType))
                    {
                        // have to add property in shadow state as we have no CLR type representing the EntityType at this stage
                        var property = entityType.AddProperty(EscapeForCSharp(tc.ColumnName), clrPropertyType, true);

                        // make column a primary key if it appears in the PK constraint
                        var primaryKeyConstrainColumn =
                            tableConstraintColumns.Values
                            .FirstOrDefault(c => c.ParentId == table.Id && c.ColumnName == tc.ColumnName && c.ConstraintType == "PRIMARY KEY");
                        if (primaryKeyConstrainColumn != null)
                        {
                            primaryKeys.Add(property);
                            property.AddAnnotation("PrimaryKeyOrdinalPosition", primaryKeyConstrainColumn.Ordinal.ToString());
                        }

                        ApplyPropertyProperties(property, tc);
                    }
                    // else skip this property
                }

                entityType.SetPrimaryKey(primaryKeys);
            }

            //return modelBuilder.Model;

            return model;
        }


        public static void ApplyPropertyProperties(Property property, TableColumn tc)
        {
            property.IsNullable = tc.IsNullable;
            property.MaxLength = tc.MaxLength == -1 ? null : tc.MaxLength;
            if (tc.NumericPrecision.HasValue)
            {
                property.AddAnnotation("Precision", tc.NumericPrecision.Value.ToString());
            }
            if (tc.DateTimePrecision.HasValue)
            {
                property.AddAnnotation("Precision", tc.DateTimePrecision.Value.ToString());
            }
            if (tc.Scale.HasValue)
            {
                property.AddAnnotation("Scale", tc.Scale.Value.ToString());
            }
            if (tc.IsIdentity)
            {
                property.AddAnnotation("IsIdentity", tc.Scale.Value.ToString());
            }
            property.IsStoreComputed = tc.IsStoreGenerated;
            if (tc.DefaultValue != null)
            {
                property.UseStoreDefault = true;
            }
        }
        public static string EscapeForCSharp(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return "_";
            }

            var cSharpName = name.Replace(".", "_");
            char firstChar = cSharpName.ElementAt(0);
            if (firstChar >= '0' && firstChar <= '9')
            {
                cSharpName = "_" + cSharpName;
            }

            return cSharpName;
        }

        public string GetContextTemplate() { return SqlServerContextTemplatingHelper.ContextTemplate; }

        public string GetEntityTypeTemplate() { return SqlServerEntityTypeTemplatingHelper.EntityTypeTemplate; }

        public ContextTemplatingHelper GetContextTemplateHelper(ContextTemplateModel contextTemplateModel)
        {
            return new SqlServerContextTemplatingHelper(contextTemplateModel);
        }

        public EntityTypeTemplatingHelper GetEntityTypeTemplateHelper(EntityTypeTemplateModel entityTypeTemplateModel)
        {
            return new SqlServerEntityTypeTemplatingHelper(entityTypeTemplateModel);
        }
    }
}