// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Design.CodeGeneration;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering
{
    public abstract class EntityTypeCodeGenerator
    {
        protected readonly ReverseEngineeringGenerator _generator;
        private readonly IEntityType _entityType;
        private readonly string _namespaceName;

        private List<string> _usedNamespaces = new List<string>() // initialize with default namespaces
                {
                    "System",
                    "Microsoft.Data.Entity",
                    "Microsoft.Data.Entity.Metadata"
                };

        public EntityTypeCodeGenerator(
            [NotNull]ReverseEngineeringGenerator generator,
            [NotNull]IEntityType entityType,
            [CanBeNull]string namespaceName)
        {
            _generator = generator;
            _entityType = entityType;
            _namespaceName = namespaceName;
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
                return _generator.EntityTypeToClassNameMap[_entityType];
            }
        }

        public virtual string ClassNamespace
        {
            get
            {
                return _namespaceName;
            }
        }

        public virtual void Generate(IndentedStringBuilder sb)
        {
            GenerateCommentHeader(sb);
            GenerateUsings(sb);
            CSharpCodeGeneratorHelper.Instance.BeginNamespace(sb, ClassNamespace);
            CSharpCodeGeneratorHelper.Instance.BeginPublicPartialClass(sb, ClassName);
            GenerateProperties(sb);
            CSharpCodeGeneratorHelper.Instance.EndClass(sb);
            CSharpCodeGeneratorHelper.Instance.EndNamespace(sb);
        }

        public virtual void GenerateCommentHeader(IndentedStringBuilder sb)
        {
            CSharpCodeGeneratorHelper.Instance.Comment(sb, string.Empty);
            CSharpCodeGeneratorHelper.Instance.Comment(sb, "Generated code");
            CSharpCodeGeneratorHelper.Instance.Comment(sb, string.Empty);
            sb.AppendLine();
        }

        public virtual void GenerateUsings(IndentedStringBuilder sb)
        {
            var originalNamespaces = new List<string>(_usedNamespaces);
            foreach (var @namespace in _usedNamespaces.Concat(
                _entityType.Properties.Select(p => p.PropertyType.Namespace)
                    .Distinct().Where(ns => !originalNamespaces.Contains(ns)).OrderBy(ns => ns)))
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
            GenerateEntityProperties(sb);
            GenerateEntityNavigations(sb);
        }

        public void GenerateEntityProperties(IndentedStringBuilder sb)
        {
            foreach(var property in OrderedEntityProperties())
            {
                GenerateEntityProperty(sb, property);
            }
        }
        public abstract void GenerateEntityProperty(IndentedStringBuilder sb, IProperty property);

        public void GenerateEntityNavigations(IndentedStringBuilder sb)
        {
            foreach (var navigation in OrderedEntityNavigations())
            {
                GenerateEntityNavigation(sb, navigation);
            }
        }

        public abstract void GenerateEntityNavigation(IndentedStringBuilder sb, INavigation navigation);

        public virtual IEnumerable<IProperty> OrderedEntityProperties()
        {
            var primaryKeyPropertiesList = new List<IProperty>();
            IKey key = _entityType.TryGetPrimaryKey();
            if (key != null)
            {
                primaryKeyPropertiesList =
                    new List<IProperty>(
                        key.Properties.OrderBy(p => _generator.PropertyToPropertyNameMap[p]));
            }

            return primaryKeyPropertiesList.Concat(
                _entityType.Properties
                    .Where(p => !primaryKeyPropertiesList.Contains(p))
                    .OrderBy(p => _generator.PropertyToPropertyNameMap[p]));
        }

        public virtual IEnumerable<INavigation> OrderedEntityNavigations()
        {
            //TODO - do we need a map to CSharp names?
            return _entityType.Navigations.OrderBy(nav => nav.Name);
        }
    }
}