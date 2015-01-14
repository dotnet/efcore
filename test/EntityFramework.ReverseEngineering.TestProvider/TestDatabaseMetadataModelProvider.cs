// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.ReverseEngineering;

namespace EntityFramework.ReverseEngineering.TestProvider
{
    public class TestDatabaseMetadataModelProvider : IDatabaseMetadataModelProvider
    {
        public virtual IModel GenerateMetadataModel(string connectionString, string filters)
        {
            var modelBuilder = new ModelBuilder(new Model());

            modelBuilder.Entity<RevEngEntity1>(e =>
                {
                    e.Key(re1 => re1.Id);
                    e.Property(re1 => re1.Name);
                    e.Property(re1 => re1.Description);
                });

            modelBuilder.Entity<RevEngEntity2>(e =>
                {
                    e.Key(re2 => new { re2.Id1, re2.Id2 });
                    e.Property(re2 => re2.Moniker);
                    e.Property(re2 => re2.FulsomePraise);
                    e.ForeignKey<RevEngEntity1>(re2 => re2.RevEngEntity1Id);
                    e.ManyToOne("RevEngEntity1", "RevEngEntity1Id", null);
                });

            return modelBuilder.Model;
        }

        public string GetContextTemplate()
        {
            return ContextTemplate;
        }

        public string GetEntityTypeTemplate()
        {
            return EntityTypeTemplate;
        }

        public virtual ContextTemplatingHelper GetContextTemplateHelper(ContextTemplateModel contextTemplateModel)
        {
            return new TestProviderContextTemplateHelper(contextTemplateModel);
        }

        public virtual EntityTypeTemplatingHelper GetEntityTypeTemplateHelper(EntityTypeTemplateModel entityTypeTemplateModel)
        {
            return new TestProviderEntityTypeTemplateHelper(entityTypeTemplateModel);
        }

        public static string ContextTemplate =
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

        public static string EntityTypeTemplate =
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

    }

    public class RevEngEntity1
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public virtual ICollection<RevEngEntity2> Entity2s { get; set; }
    }

    public class RevEngEntity2
    {
        public int Id1 { get; set; }
        public int Id2 { get; set; }
        public string Moniker { get; set; }
        public string FulsomePraise { get; set; }
        public int RevEngEntity1Id { get; set; }
        public RevEngEntity1 RevEngEntity1 { get; set; }
    }
}