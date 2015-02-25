// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
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
            [NotNull] ReverseEngineeringGenerator generator,
            [NotNull] IModel model, [NotNull] string namespaceName,
            [CanBeNull] string className, [NotNull] string connectionString)
        {
            Check.NotNull(generator, nameof(generator));
            Check.NotNull(model, nameof(model));
            Check.NotEmpty(namespaceName, nameof(namespaceName));
            Check.NotEmpty(connectionString, nameof(connectionString));

            Generator = generator;
            Model = model;
            ClassNamespace = namespaceName;
            ClassName = className;
            ConnectionString = connectionString;
        }

        public virtual void Generate([NotNull] IndentedStringBuilder sb)
        {
            Check.NotNull(sb, nameof(sb));

            GenerateCommentHeader(sb);
            GenerateUsings(sb);
            Generator.CSharpCodeGeneratorHelper.BeginNamespace(ClassNamespace, sb);
            Generator.CSharpCodeGeneratorHelper.BeginClass(AccessModifier.Public, ClassName, isPartial: true, sb: sb, inheritsFrom: new string[] { "DbContext" });
            GenerateProperties(sb);
            GenerateMethods(sb);
            Generator.CSharpCodeGeneratorHelper.EndClass(sb);
            Generator.CSharpCodeGeneratorHelper.EndNamespace(sb);
        }

        public virtual ReverseEngineeringGenerator Generator { get; }

        public virtual IModel Model { get; }

        public virtual string ClassName { get; }

        public virtual string ClassNamespace { get; }

        public virtual string ConnectionString { get; }

        public virtual void GenerateCommentHeader([NotNull] IndentedStringBuilder sb)
        {
            Check.NotNull(sb, nameof(sb));

            Generator.CSharpCodeGeneratorHelper.SingleLineComment(string.Empty, sb);
            Generator.CSharpCodeGeneratorHelper.SingleLineComment("Generated using Connection String: " + ConnectionString, sb);
            Generator.CSharpCodeGeneratorHelper.SingleLineComment(string.Empty, sb);
            sb.AppendLine();
        }

        public virtual void GenerateUsings([NotNull] IndentedStringBuilder sb)
        {
            Check.NotNull(sb, nameof(sb));

            // TODO - add in other namespaces
            foreach (var @namespace in _usedNamespaces)
            {
                Generator.CSharpCodeGeneratorHelper.AddUsingStatement(@namespace, sb);
            }

            if (_usedNamespaces.Any())
            {
                sb.AppendLine();
            }
        }

        public virtual void GenerateProperties([NotNull] IndentedStringBuilder sb)
        {
            Check.NotNull(sb, nameof(sb));

            foreach (var entityType in OrderedEntityTypes())
            {
                Generator.CSharpCodeGeneratorHelper.AddProperty(
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

        public virtual void GenerateMethods([NotNull] IndentedStringBuilder sb)
        {
            Check.NotNull(sb, nameof(sb));

            GenerateOnConfiguringCode(sb);
            sb.AppendLine();
            GenerateOnModelCreatingCode(sb);
        }

        public virtual void GenerateOnConfiguringCode([NotNull] IndentedStringBuilder sb)
        {
            Check.NotNull(sb, nameof(sb));

            Generator.CSharpCodeGeneratorHelper.BeginMethod(AccessModifier.Protected,
                VirtualModifier.Override, "void", "OnConfiguring", sb, _onConfiguringMethodParameters);
            sb.Append("options.UseSqlServer(");
            sb.Append(CSharpUtilities.Instance.GenerateVerbatimStringLiteral(ConnectionString));
            sb.AppendLine(");");
            Generator.CSharpCodeGeneratorHelper.EndMethod(sb);
        }

        public virtual void GenerateOnModelCreatingCode([NotNull] IndentedStringBuilder sb)
        {
            Check.NotNull(sb, nameof(sb));

            Generator.CSharpCodeGeneratorHelper.BeginMethod(AccessModifier.Protected,
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

            Generator.CSharpCodeGeneratorHelper.EndMethod(sb);
        }

        public virtual void GenerateEntityKeyAndPropertyConfiguration(
            [NotNull] IEntityType entityType, [NotNull] IndentedStringBuilder sb)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(sb, nameof(sb));

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

        public virtual void GenerateEntityKeyConfiguration([NotNull] IKey key, [NotNull] IndentedStringBuilder sb)
        {
            Check.NotNull(key, nameof(key));
            Check.NotNull(sb, nameof(sb));

            sb.Append("entity.Key(e => ");
            sb.Append(ModelUtilities.Instance
                .GenerateLambdaToKey(key.Properties, "e"));
            sb.Append(");");
        }

        public virtual void GenerateEntityFacetsConfiguration(
            [NotNull] IEntityType entityType, [NotNull] IndentedStringBuilder sb)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(sb, nameof(sb));

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

        public virtual List<string> GenerateNonForRelationalEntityFacetsConfiguration([NotNull] IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return new List<string>();
        }

        public virtual List<string> GenerateForRelationalEntityFacetsConfiguration([NotNull] IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            var facetsConfig = new List<string>();
            var tableNameFacetConfig = GenerateTableNameFacetConfiguration(entityType);
            if (tableNameFacetConfig != null)
            {
                facetsConfig.Add(tableNameFacetConfig);
            }

            return facetsConfig;
        }

        public virtual string GenerateTableNameFacetConfiguration([NotNull] IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

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

        public abstract void GenerateNavigationsConfiguration(
            [NotNull] IEntityType entityType, [NotNull] IndentedStringBuilder sb);

        public virtual void GeneratePropertyFacetsConfiguration(
            [NotNull] IProperty property, [NotNull] IndentedStringBuilder sb)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(sb, nameof(sb));

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


        public virtual List<string> GenerateNonProviderSpecificPropertyFacetsConfiguration(
            [NotNull] IProperty property)
        {
            Check.NotNull(property, nameof(property));

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

        public virtual List<string> GenerateRelationalPropertyFacetsConfiguration(
            [NotNull] IProperty property)
        {
            Check.NotNull(property, nameof(property));

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

            var defaultValueFacetConfig = GenerateDefaultValueFacetConfiguration(property);
            if (defaultValueFacetConfig != null)
            {
                facetsConfig.Add(defaultValueFacetConfig);
            }

            var defaultExpressionFacetConfig = GenerateDefaultExpressionFacetConfiguration(property);
            if (defaultExpressionFacetConfig != null)
            {
                facetsConfig.Add(defaultExpressionFacetConfig);
            }

            return facetsConfig;
        }

        public abstract void GenerateProviderSpecificPropertyFacetsConfiguration(
            [NotNull] IProperty property, [NotNull] string entityVariableName,
            [NotNull] IndentedStringBuilder sb);

        public virtual string GenerateMaxLengthFacetConfiguration([NotNull] IProperty property)
        {
            Check.NotNull(property, nameof(property));

            if (((Property)property).MaxLength.HasValue)
            {
                return string.Format(CultureInfo.InvariantCulture,
                    ".MaxLength({0})",
                    CSharpUtilities.Instance.GenerateLiteral(
                        ((Property)property).MaxLength.Value));
            }

            return null;
        }

        public virtual string GenerateStoreComputedFacetConfiguration([NotNull] IProperty property)
        {
            Check.NotNull(property, nameof(property));

            if (((Property)property).IsStoreComputed.HasValue)
            {
                return string.Format(CultureInfo.InvariantCulture,
                    ".StoreComputed({0})",
                    CSharpUtilities.Instance.GenerateLiteral(
                        ((Property)property).IsStoreComputed.Value));
            }

            return null;
        }

        public virtual string GenerateColumnNameFacetConfiguration([NotNull] IProperty property)
        {
            Check.NotNull(property, nameof(property));

            if (property.Relational().Column != null && property.Relational().Column != property.Name)
            {
                return string.Format(CultureInfo.InvariantCulture,
                    ".Column({0})",
                    CSharpUtilities.Instance.DelimitString(property.Relational().Column));
            }

            return null;
        }

        public virtual string GenerateColumnTypeFacetConfiguration([NotNull] IProperty property)
        {
            Check.NotNull(property, nameof(property));

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

        public virtual string GenerateDefaultValueFacetConfiguration([NotNull] IProperty property)
        {
            Check.NotNull(property, nameof(property));

            if (property.Relational().DefaultValue != null)
            {
                return string.Format(CultureInfo.InvariantCulture,
                    ".DefaultValue({0})",
                    CSharpUtilities.Instance.GenerateLiteral(
                        (dynamic)property.Relational().DefaultValue));
            }

            return null;
        }

        public virtual string GenerateDefaultExpressionFacetConfiguration([NotNull] IProperty property)
        {
            Check.NotNull(property, nameof(property));

            if (property.Relational().DefaultExpression != null)
            {
                return string.Format(CultureInfo.InvariantCulture,
                    ".DefaultExpression({0})",
                    CSharpUtilities.Instance.DelimitString(
                        property.Relational().DefaultExpression));
            }

            return null;
        }

        public virtual IEnumerable<IEntityType> OrderedEntityTypes()
        {
            // default ordering is by Name, which is what we want here
            return Model.EntityTypes;
        }

        public virtual IEnumerable<IProperty> OrderedProperties([NotNull] IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return ModelUtilities.Instance.OrderedProperties(entityType);
        }
    }
}