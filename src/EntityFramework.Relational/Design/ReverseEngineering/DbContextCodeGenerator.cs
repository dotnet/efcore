// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
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


        private readonly ReverseEngineeringGenerator _generator;
        private readonly IModel _model;
        private readonly string _namespaceName;
        private readonly string _className;
        private readonly string _connectionString;

        // initialize default namespaces
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
            _generator = generator;
            _model = model;
            _namespaceName = namespaceName;
            _className = className;
            _connectionString = connectionString;
        }

        public virtual void Generate(IndentedStringBuilder sb)
        {
            GenerateCommentHeader(sb);
            GenerateUsings(sb);
            CSharpCodeGeneratorHelper.Instance.BeginNamespace(sb, ClassNamespace);
            CSharpCodeGeneratorHelper.Instance.BeginClass(sb, AccessModifier.Public, ClassName, isPartial: true, inheritsFrom: new string[] { "DbContext" });
            GenerateProperties(sb);
            GenerateMethods(sb);
            CSharpCodeGeneratorHelper.Instance.EndClass(sb);
            CSharpCodeGeneratorHelper.Instance.EndNamespace(sb);
        }

        public virtual ReverseEngineeringGenerator Generator
        {
            get
            {
                return _generator;
            }
        }

        public virtual string ClassName
        {
            get
            {
                return _className;
            }
        }

        public virtual string ClassNamespace
        {
            get
            {
                return _namespaceName;
            }
        }

        public virtual string ConnectionString
        {
            get
            {
                return _connectionString;
            }
        }

        public virtual void GenerateCommentHeader(IndentedStringBuilder sb)
        {
            CSharpCodeGeneratorHelper.Instance.Comment(sb, string.Empty);
            CSharpCodeGeneratorHelper.Instance.Comment(sb, "Generated using Connection String: " + ConnectionString);
            CSharpCodeGeneratorHelper.Instance.Comment(sb, string.Empty);
            sb.AppendLine();
        }

        public virtual void GenerateUsings(IndentedStringBuilder sb)
        {
            // TODO - add in other namespaces
            foreach (var @namespace in _usedNamespaces)
            {
                CSharpCodeGeneratorHelper.Instance.AddUsingStatement(sb, @namespace);
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
                    sb
                    , AccessModifier.Public
                    , VirtualModifier.Virtual
                    , "DbSet<" + _generator.EntityTypeToClassNameMap[entityType] + ">"
                    , _generator.EntityTypeToClassNameMap[entityType]);
            }

            if (_model.EntityTypes.Any())
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
            CSharpCodeGeneratorHelper.Instance.BeginMethod(sb,
                AccessModifier.Protected, VirtualModifier.Override, "void", "OnConfiguring", _onConfiguringMethodParameters);
            sb.Append("options.UseSqlServer(");
            sb.Append(CSharpUtilities.Instance.GenerateVerbatimStringLiteral(ConnectionString));
            sb.AppendLine(");");
            CSharpCodeGeneratorHelper.Instance.EndMethod(sb);
        }

        public virtual void GenerateOnModelCreatingCode(IndentedStringBuilder sb)
        {
            CSharpCodeGeneratorHelper.Instance.BeginMethod(sb,
                AccessModifier.Protected, VirtualModifier.Override, "void", "OnModelCreating", _onModelCreatingMethodParameters);
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
                sb.Append(_generator.EntityTypeToClassNameMap[entityType]);
                sb.Append(">(");
                GenerateEntityConfiguration(sb, entityType);
                sb.AppendLine(");");
            }
            CSharpCodeGeneratorHelper.Instance.EndMethod(sb);
        }

        public virtual void GenerateEntityConfiguration(IndentedStringBuilder sb, IEntityType entityType)
        {
            sb.AppendLine("entity =>");
            using (sb.Indent())
            {
                sb.AppendLine("{");
                using (sb.Indent())
                {
                    var key = entityType.TryGetPrimaryKey();
                    if (key != null && key.Properties.Count > 0)
                    {
                        GenerateEntityKeyConfiguration(sb, key);
                    }
                    GenerateEntityFacetsConfiguration(sb, entityType);
                    GenerateForeignKeysConfiguration(sb, entityType);
                    foreach (var property in OrderedProperties(entityType))
                    {
                        GeneratePropertyFacetsConfiguration(sb, property);
                    }
                }
                sb.AppendLine();
                sb.AppendLine("}");
            }
        }

        public virtual void GenerateEntityKeyConfiguration(IndentedStringBuilder sb, IKey key)
        {
            sb.Append("entity.Key( e => ");
            sb.Append(ModelUtilities.Instance
                .GenerateLambdaToKey(key.Properties, "e"));
            sb.Append(" );");
        }

        public virtual void GenerateEntityFacetsConfiguration(IndentedStringBuilder sb, IEntityType entityType)
        {
        }

        public abstract void GenerateForeignKeysConfiguration(IndentedStringBuilder sb, IEntityType entityType);

        public virtual void GeneratePropertyFacetsConfiguration(IndentedStringBuilder sb, IProperty property)
        {
        }

        public virtual IEnumerable<IEntityType> OrderedEntityTypes()
        {
            return _model.EntityTypes.OrderBy(e => e.Name);
        }

        public virtual IEnumerable<IProperty> OrderedProperties(IEntityType entityType)
        {
            var primaryKeyProperties = entityType.GetPrimaryKey().Properties.ToList();
            foreach (var property in primaryKeyProperties)
            {
                yield return property;
            }

            var foreignKeyProperties = entityType.ForeignKeys.SelectMany(fk => fk.Properties).Distinct().ToList();
            foreach (var property in
                entityType
                .Properties
                .Where(p => !primaryKeyProperties.Contains(p) && !foreignKeyProperties.Contains(p))
                .OrderBy(p => p.Name))
            {
                yield return property;
            }
        }
    }
}