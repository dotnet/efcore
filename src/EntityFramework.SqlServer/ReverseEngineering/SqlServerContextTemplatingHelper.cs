// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Text;
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

@foreach(var et in @Model.MetadataModel.EntityTypes)
{
@:        public virtual DbSet<@et.SimpleName> @et.SimpleName { get; set; }
}

        protected override void OnConfiguring(DbContextOptions options)
        {
@Model.Helper. (indent: ""            "")
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
@Model.Helper.OnModelCreatingCode(indent: ""            "")
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
            var sb = new StringBuilder();
            sb.Append(indent);
            sb.AppendLine("modelBuilder.AddSqlServer();");
            foreach (var entity in ContextTemplateModel.MetadataModel.EntityTypes)
            {
                sb.Append(indent);
                sb.Append("modelBuilder.Entity<");
                sb.Append(entity.SimpleName);
                sb.Append(">()");
                var key = entity.TryGetPrimaryKey();
                if (key != null && key.Properties.Count > 0)
                {
                    sb.AppendLine();
                    sb.Append(indent + "    ");
                    sb.Append(".Key(e => ");
                    if (key.Properties.Count > 1)
                    {
                        sb.Append("new { ");
                        sb.Append(string.Join(", ", key.Properties.OrderBy(p => int.Parse(p["PrimaryKeyOrdinalPosition"])).Select(p => "e." + p.Name)));
                        sb.Append(" }");
                    }
                    else
                    {
                        sb.Append("e." + key.Properties[0].Name);
                    }
                    sb.Append(")");
                }
                sb.AppendLine(";");
            }

            return sb.ToString();
        }
    }
}