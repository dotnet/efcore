// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.ReverseEngineering;

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
            return null;
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