// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.ReverseEngineering;

namespace EntityFramework.SqlServer.ReverseEngineering
{
    public class SqlServerContextTemplatingHelper : ContextTemplatingHelper
    {
        public static readonly string ContextTemplate =
@"@inherits Microsoft.Framework.CodeGeneration.Templating.RazorTemplateBase
// Generated using Provider Assembly: @Model.ProviderAssembly
// And Database Connection String: @Model.ConnectionString
// With Database Filters: @Model.Filters

using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Metadata;

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
@:        public DbSet<@et.SimpleName> @et.SimpleName { get; set; }
}
    }
}
";

        public SqlServerContextTemplatingHelper(ContextTemplateModel model) : base(model) { }

        public override string OnConfiguringCode(string indent)
        {
            return indent + "options.UseSqlServer(\"" + ContextTemplateModel.ConnectionString + "\");";
        }

        public override string OnModelCreatingCode(string indent)
        {
            return indent + "builder.AddSqlServer();";
        }
    }
}