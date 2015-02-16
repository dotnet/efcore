// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Relational.Design.CodeGeneration;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering
{
    public abstract class DbContextCodeGenerator
    {
        private static readonly List<Tuple<string, string>> _onConfiguringMethodParameters =
            new List<Tuple<string, string>>()
                {
                    new Tuple<string, string>("DbContextOptions", "options")
                };

        private static readonly List<Tuple<string, string>> _onModelCreatingMethodParameters =
            new List<Tuple<string, string>>()
                {
                    new Tuple<string, string>("ModelBuilder", "modelBuilder")
                };


        private List<string> _usedNamespaces = new List<string>()
                {
                    "System",
                    "Microsoft.Data.Entity",
                    "Microsoft.Data.Entity.Metadata"
                };

        public DbContextCodeGenerator(
            [NotNull]ReverseEngineeringGenerator generator,
            [NotNull]IModel model, [NotNull]string namespaceName,
            [NotNull]string className, [NotNull]string connectionString)
        {
            Generator = generator;
            Model = model;
            ClassNamespace = namespaceName;
            ClassName = className;
            ConnectionString = connectionString;
        }

        public virtual void Generate(IndentedStringBuilder sb)
        {
            GenerateCommentHeader(sb);
            GenerateUsings(sb);
            CSharpCodeGeneratorHelper.Instance.BeginNamespace(ClassNamespace, sb);
            CSharpCodeGeneratorHelper.Instance.BeginClass(AccessModifier.Public, ClassName, isPartial: true, sb: sb, inheritsFrom: new string[] { "DbContext" });
            GenerateProperties(sb);
            GenerateMethods(sb);
            CSharpCodeGeneratorHelper.Instance.EndClass(sb);
            CSharpCodeGeneratorHelper.Instance.EndNamespace(sb);
        }

        public virtual ReverseEngineeringGenerator Generator { get; }

        public virtual IModel Model { get; }

        public virtual string ClassName { get; }

        public virtual string ClassNamespace { get; }

        public virtual string ConnectionString { get; }

        public virtual void GenerateCommentHeader(IndentedStringBuilder sb)
        {
            CSharpCodeGeneratorHelper.Instance.SingleLineComment(string.Empty, sb);
            CSharpCodeGeneratorHelper.Instance.SingleLineComment("Generated using Connection String: " + ConnectionString, sb);
            CSharpCodeGeneratorHelper.Instance.SingleLineComment(string.Empty, sb);
            sb.AppendLine();
        }

        public virtual void GenerateUsings(IndentedStringBuilder sb)
        {
            // TODO - add in other namespaces
            foreach (var @namespace in _usedNamespaces)
            {
                CSharpCodeGeneratorHelper.Instance.AddUsingStatement(@namespace, sb);
            }

            if (_usedNamespaces.Any())
            {
                sb.AppendLine();
            }
        }

        public virtual void GenerateProperties(IndentedStringBuilder sb)
        {
            foreach (var entityType in OrderedEntityTypes())
            {
                CSharpCodeGeneratorHelper.Instance.AddProperty(
                    AccessModifier.Public,
                    VirtualModifier.Virtual,
                    "DbSet<" + entityType.Name + ">",
                    entityType.Name,
                    sb);
            }

            if (Model.EntityTypes.Any())
            {
                sb.AppendLine();
            }
        }

        public virtual void GenerateMethods(IndentedStringBuilder sb)
        {
            GenerateOnConfiguringCode(sb);
            sb.AppendLine();
            GenerateOnModelCreatingCode(sb);
        }

        public virtual void GenerateOnConfiguringCode(IndentedStringBuilder sb)
        {
            CSharpCodeGeneratorHelper.Instance.BeginMethod(AccessModifier.Protected,
                VirtualModifier.Override, "void", "OnConfiguring", sb, _onConfiguringMethodParameters);
            sb.Append("options.UseSqlServer(");
            sb.Append(CSharpUtilities.Instance.GenerateVerbatimStringLiteral(ConnectionString));
            sb.AppendLine(");");
            CSharpCodeGeneratorHelper.Instance.EndMethod(sb);
        }

        public virtual void GenerateOnModelCreatingCode(IndentedStringBuilder sb)
        {
            CSharpCodeGeneratorHelper.Instance.BeginMethod(AccessModifier.Protected,
                VirtualModifier.Override, "void", "OnModelCreating", sb, _onModelCreatingMethodParameters);

            var first = true;
            foreach (var entityType in OrderedEntityTypes())
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sb.AppendLine();
                }
                sb.Append("modelBuilder.Entity<");
                sb.Append(entityType.Name);
                sb.AppendLine(">(entity =>");
                GenerateEntityKeyAndPropertyConfiguration(entityType, sb);
                sb.AppendLine(");");
            }

            foreach (var entityType in OrderedEntityTypes())
            {
                var navigationsStringBuilder = new IndentedStringBuilder(sb);
                navigationsStringBuilder.IncrementIndent().IncrementIndent();
                GenerateNavigationsConfiguration(entityType, navigationsStringBuilder);
                var navigationsCode = navigationsStringBuilder.ToString();
                if (!string.IsNullOrEmpty(navigationsCode))
                {
                    sb.AppendLine();
                    sb.Append("modelBuilder.Entity<");
                    sb.Append(entityType.Name);
                    sb.AppendLine(">(entity =>");
                    using (sb.Indent())
                    {
                        sb.AppendLine("{");
                        sb.DecrementIndent().DecrementIndent().DecrementIndent().DecrementIndent();
                        sb.AppendLine(navigationsCode);
                        sb.IncrementIndent().IncrementIndent().IncrementIndent().IncrementIndent();
                        sb.AppendLine("}");
                    }
                    sb.AppendLine(");");
                }
            }

            CSharpCodeGeneratorHelper.Instance.EndMethod(sb);
        }

        public virtual void GenerateEntityKeyAndPropertyConfiguration(IEntityType entityType, IndentedStringBuilder sb)
        {
            using (sb.Indent())
            {
                sb.AppendLine("{");
                using (sb.Indent())
                {
                    var key = entityType.TryGetPrimaryKey();
                    if (key != null && key.Properties.Count > 0)
                    {
                        GenerateEntityKeyConfiguration(key, sb);
                    }
                    GenerateEntityFacetsConfiguration(entityType, sb);
                    foreach (var property in OrderedProperties(entityType))
                    {
                        GeneratePropertyFacetsConfiguration(property, sb);
                    }
                }
                sb.AppendLine();
                sb.AppendLine("}");
            }
        }

        public virtual void GenerateEntityKeyConfiguration(IKey key, IndentedStringBuilder sb)
        {
            sb.Append("entity.Key(e => ");
            sb.Append(ModelUtilities.Instance
                .GenerateLambdaToKey(key.Properties, "e"));
            sb.Append(");");
        }

        public virtual void GenerateEntityFacetsConfiguration(IEntityType entityType, IndentedStringBuilder sb)
        {
            var nonForRelationalEntityFacetsConfiguration = GenerateNonForRelationalEntityFacetsConfiguration(entityType);
            var forRelationalEntityFacetsConfiguration = GenerateForRelationalEntityFacetsConfiguration(entityType);

            if (nonForRelationalEntityFacetsConfiguration.Count > 0
                || forRelationalEntityFacetsConfiguration.Count > 0)
            {
                foreach (var facetConfig in nonForRelationalEntityFacetsConfiguration)
                {
                    sb.AppendLine();
                    sb.Append(facetConfig);
                }

                if (forRelationalEntityFacetsConfiguration.Count > 0)
                {
                    sb.AppendLine();
                    sb.Append("entity.ForRelational()");
                    using (sb.Indent())
                    {
                        foreach (var facetConfig in forRelationalEntityFacetsConfiguration)
                        {
                            sb.AppendLine();
                            sb.Append(facetConfig);
                        }
                    }
                }
                sb.Append(";");
            }
        }

        public virtual List<string> GenerateNonForRelationalEntityFacetsConfiguration(IEntityType entityType)
        {
            return new List<string>();
        }

        public virtual List<string> GenerateForRelationalEntityFacetsConfiguration(IEntityType entityType)
        {
            var facetsConfig = new List<string>();
            var tableNameFacetConfig = GenerateTableNameFacetConfiguration(entityType);
            if (tableNameFacetConfig != null)
            {
                facetsConfig.Add(tableNameFacetConfig);
            }

            return facetsConfig;
        }

        public virtual string GenerateTableNameFacetConfiguration(IEntityType entityType)
        {
            if ("dbo" != entityType.Relational().Schema)
            {
                return string.Format(CultureInfo.InvariantCulture, ".Table({0}, {1})",
                    CSharpUtilities.Instance.DelimitString(entityType.Relational().Table),
                    CSharpUtilities.Instance.DelimitString(entityType.Relational().Schema));
            }

            if (entityType.Relational().Table != null
                && entityType.Relational().Table != entityType.Name)
            {
                return string.Format(CultureInfo.InvariantCulture, ".Table({0})",
                    CSharpUtilities.Instance.DelimitString(entityType.Relational().Table));
            }

            return null;
        }

        public abstract void GenerateNavigationsConfiguration(IEntityType entityType, IndentedStringBuilder sb);

        public virtual void GeneratePropertyFacetsConfiguration(IProperty property, IndentedStringBuilder sb)
        {
            var nonProviderSpecificPropertyFacetsConfiguration = GenerateNonProviderSpecificPropertyFacetsConfiguration(property);
            var relationalPropertyFacetsConfiguration = GenerateRelationalPropertyFacetsConfiguration(property);

            var anyFacets = (nonProviderSpecificPropertyFacetsConfiguration.Count > 0
                             || relationalPropertyFacetsConfiguration.Count > 0);
            if (anyFacets)
            {
                sb.AppendLine();
                sb.Append("entity.Property(e => e.");
                sb.Append(property.Name);
                sb.Append(")");
                using (sb.Indent())
                {
                    foreach (var facetConfig in nonProviderSpecificPropertyFacetsConfiguration)
                    {
                        sb.AppendLine();
                        sb.Append(facetConfig);
                    }

                    if (relationalPropertyFacetsConfiguration.Count > 0)
                    {
                        sb.AppendLine();
                        sb.Append(".ForRelational()");
                        using (sb.Indent())
                        {
                            foreach (var facetConfig in relationalPropertyFacetsConfiguration)
                            {
                                sb.AppendLine();
                                sb.Append(facetConfig);
                            }
                        }
                    }
                }
                sb.Append(";");
            }

            GenerateProviderSpecificPropertyFacetsConfiguration(property, "entity", sb);
        }


        public virtual List<string> GenerateNonProviderSpecificPropertyFacetsConfiguration(IProperty property)
        {
            var facetsConfig = new List<string>();
            var maxLengthFacetConfig = GenerateMaxLengthFacetConfiguration(property);
            if (maxLengthFacetConfig != null)
            {
                facetsConfig.Add(maxLengthFacetConfig);
            }

            var storeComputedFacetConfig = GenerateStoreComputedFacetConfiguration(property);
            if (storeComputedFacetConfig != null)
            {
                facetsConfig.Add(storeComputedFacetConfig);
            }

            return facetsConfig;
        }

        public virtual List<string> GenerateRelationalPropertyFacetsConfiguration(IProperty property)
        {
            var facetsConfig = new List<string>();
            var columnNameFacetConfig = GenerateColumnNameFacetConfiguration(property);
            if (columnNameFacetConfig != null)
            {
                facetsConfig.Add(columnNameFacetConfig);
            }

            var columnTypeFacetConfig = GenerateColumnTypeFacetConfiguration(property);
            if (columnTypeFacetConfig != null)
            {
                facetsConfig.Add(columnTypeFacetConfig);
            }

            return facetsConfig;
        }

        public abstract void GenerateProviderSpecificPropertyFacetsConfiguration(
            IProperty property, string entityVariableName, IndentedStringBuilder sb);

        public virtual string GenerateMaxLengthFacetConfiguration(IProperty property)
        {
            if (((Property)property).MaxLength.HasValue)
            {
                return string.Format(CultureInfo.InvariantCulture,
                    ".MaxLength({0})",
                    CSharpUtilities.Instance.GenerateLiteral(
                        ((Property)property).MaxLength.Value));
            }

            return null;
        }

        public virtual string GenerateStoreComputedFacetConfiguration(IProperty property)
        {
            if (((Property)property).IsStoreComputed.HasValue)
            {
                return string.Format(CultureInfo.InvariantCulture,
                    ".StoreComputed({0})",
                    CSharpUtilities.Instance.GenerateLiteral(
                        ((Property)property).IsStoreComputed.Value));
            }

            return null;
        }

        public virtual string GenerateColumnNameFacetConfiguration(IProperty property)
        {
            if (property.Relational().Column != null && property.Relational().Column != property.Name)
            {
                return string.Format(CultureInfo.InvariantCulture,
                    ".Column({0})",
                    CSharpUtilities.Instance.DelimitString(property.Relational().Column));
            }

            return null;
        }

        public virtual string GenerateColumnTypeFacetConfiguration(IProperty property)
        {
            // output columnType if decimal or datetime2 to define precision and scale
            var columnType = property.Relational().ColumnType;
            if (columnType != null
                && (columnType.StartsWith("decimal") || columnType.StartsWith("datetime2")))
            {
                return string.Format(CultureInfo.InvariantCulture,
                    ".ColumnType({0})",
                    CSharpUtilities.Instance.DelimitString(property.Relational().ColumnType));
            }

            return null;
        }

        public virtual IEnumerable<IEntityType> OrderedEntityTypes()
        {
            // default ordering is by Name, which is what we want here
            return Model.EntityTypes;
        }

        public virtual IEnumerable<IProperty> OrderedProperties(IEntityType entityType)
        {
            return ModelUtilities.Instance.OrderedProperties(entityType);
        }
    }
}