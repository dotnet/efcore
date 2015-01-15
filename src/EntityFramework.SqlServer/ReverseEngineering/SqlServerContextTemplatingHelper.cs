// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Data.Entity.Metadata;
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
@Model.Helper.OnConfiguringCode(indent: ""            "")
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
            return indent + "options.UseSqlServer(@\"" + ContextTemplateModel.ConnectionString + "\");";
        }

        public override string OnModelCreatingCode(string indent)
        {
            var sb = new StringBuilder();
            foreach (var entity in ContextTemplateModel.MetadataModel.EntityTypes)
            {
                sb.AppendLine();
                sb.Append(indent);
                sb.Append("modelBuilder.Entity<");
                sb.Append(entity.SimpleName);
                sb.Append(">(");
                AddKeyToOnModelCreating(sb, indent, entity);
                sb.Append(");");
            }

            return sb.ToString();
        }

        public static void AddKeyToOnModelCreating(StringBuilder sb, string indent, IEntityType entityType)
        {
            var key = entityType.TryGetPrimaryKey();
            if (key != null && key.Properties.Count > 0)
            {
                sb.AppendLine("entity =>");
                sb.AppendLine(indent + "{");
                sb.Append(indent + "    ");
                sb.Append("entity.Key( e => ");
                sb.Append(KeyConstructor(key.Properties, p => int.Parse(p[SqlServerMetadataModelProvider.AnnotationNamePrimaryKeyOrdinal])));
                sb.AppendLine(" );");
                AddForeignKeysToOnModelCreating(sb, indent, entityType);
                sb.Append(indent + "}");
            }
        }

        public static void AddForeignKeysToOnModelCreating(StringBuilder sb, string indent, IEntityType entityType)
        {
            // construct dictionary mapping foreignKeyConstraintId to the list of Properties which constitute that foreign key
            var allForeignKeyConstraints = new Dictionary<string, List<Property>>(); // maps foreignKeyConstraintId to Properties 
            foreach (var prop in entityType.Properties.Cast<Property>())
            {
                var foreignKeyConstraintsAnnotation = prop.TryGetAnnotation(SqlServerMetadataModelProvider.AnnotationNameForeignKeyConstraints);
                if (foreignKeyConstraintsAnnotation != null)
                {
                    var foreignKeyConstraintIds = SqlServerMetadataModelProvider.SplitString(
                        SqlServerMetadataModelProvider.AnnotationFormatForeignKeyConstraintSeparator.ToCharArray()
                        , foreignKeyConstraintsAnnotation.Value);
                    foreach (var fkcId in foreignKeyConstraintIds)
                    {
                        List<Property> properties;
                        if (!allForeignKeyConstraints.TryGetValue(fkcId, out properties))
                        {
                            properties = new List<Property>();
                            allForeignKeyConstraints.Add(fkcId, properties);
                        }
                        if (!properties.Contains(prop))
                        {
                            properties.Add(prop);
                        }
                    }
                }
            }

            // loop over all constraints constructing foreign key entry in OnModelCreating()
            foreach (var fkcEntry in allForeignKeyConstraints)
            {
                var constraintId = fkcEntry.Key;
                var propertyList = fkcEntry.Value;
                if (propertyList.Count > 0)
                {
                    var targetEntity = propertyList.ElementAt(0)[SqlServerMetadataModelProvider.GetForeignKeyTargetEntityTypeAnnotationName(constraintId)];
                    var targetEntityLastIndex = targetEntity.LastIndexOf(SqlServerMetadataModelProvider.AnnotationNameTableIdSchemaTableSeparator);
                    if (targetEntityLastIndex > 0)
                    {
                        targetEntity = targetEntity.Substring(targetEntityLastIndex + 1);
                    }

                    var ordinalAnnotationName = SqlServerMetadataModelProvider.GetForeignKeyOrdinalPositionAnnotationName(constraintId);

                    sb.Append(indent + "    ");
                    sb.Append("entity.ForeignKey<");
                    sb.Append(targetEntity);
                    sb.Append(">( e => ");
                    sb.Append(KeyConstructor(propertyList, p => int.Parse(p[ordinalAnnotationName])));
                    sb.AppendLine(" );");
                }
            }
        }

        public static string KeyConstructor(IEnumerable<IProperty> properties, Func<IProperty, int> orderingExpression)
        {
            var sb = new StringBuilder();

            if (properties.Count() > 1)
            {
                sb.Append("new { ");
                sb.Append(string.Join(", ", properties.OrderBy(orderingExpression).Select(p => "e." + p.Name)));
                sb.Append(" }");
            }
            else
            {
                sb.Append("e." + properties.ElementAt(0).Name);
            }

            return sb.ToString();
        }
    }
}