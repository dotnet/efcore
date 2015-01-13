// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.ReverseEngineering;
using System;

namespace EntityFramework.SqlServer.ReverseEngineering
{
    public class SqlServerMetadataModelProvider : IDatabaseMetadataModelProvider
    {
        public static readonly string ContextTemplate =
@"@inherits Microsoft.Framework.CodeGeneration.Templating.RazorTemplateBase
// Generated using Provider Assembly: @Model.ProviderAssembly
// And Database Connection String: @Model.ConnectionString
// With Database Filters: @Model.Filters

using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Metadata;
@Model.Helper.Usings()
namespace @Model.Namespace
{
    public partial class @Model.ClassName : DbContext
    {
        protected override void OnConfiguring(DbContextOptions options)
        {
@Model.Helper.OnConfiguringCode(indent: ""            "")
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
@Model.Helper.OnModelCreatingCode(indent: ""            "")
        }

@foreach(var et in @Model.MetadataModel.EntityTypes)
{
@:        public DbSet<@et.Type.Name> @et.SimpleName { get; set; }
}
    }
}
";

        public static readonly string EntityTypeTemplate =
@"@inherits Microsoft.Framework.CodeGeneration.Templating.RazorTemplateBase
@using Microsoft.Data.Entity.Metadata
// Generated using Provider Assembly: @Model.ProviderAssembly
// And Database Connection String: @Model.ConnectionString
// With Database Filters: @Model.Filters

@Model.Helper.Usings()
namespace @Model.Namespace
{
    public class @Model.EntityType.SimpleName
    {
@Model.Helper.PropertiesCode(indent: ""        "")
@Model.Helper.NavigationsCode(indent:  ""        "")
    }
}";

        public IModel GenerateMetadataModel(string connectionString, string filters)
        {
            Dictionary<string, Table> tables;
            Dictionary<string, TableColumn> tableColumns;
            using (var conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    tables = LoadData<Table>(conn, Table.Query, Table.CreateFromReader, t => t.Id);
                    tableColumns = LoadData<TableColumn>(conn, TableColumn.Query, TableColumn.CreateFromReader, tc => tc.Id);
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

            Console.WriteLine("Tables");
            foreach (var t in tables)
            {
                Console.WriteLine(t.Value.ToString());
            }

            Console.WriteLine("Columns");
            foreach (var tc in tableColumns)
            {
                Console.WriteLine(tc.Value.ToString());
            }

            return null;
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

        public string GetContextTemplate() { return ContextTemplate; }

        public string GetEntityTypeTemplate() { return EntityTypeTemplate; }

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