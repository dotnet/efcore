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
        private List<string> _usedNamespaces = new List<string>()
                {
                    "System",
                    "System.Collections.Generic",
                    "Microsoft.Data.Entity",
                    "Microsoft.Data.Entity.Metadata"
                };

        public EntityTypeCodeGenerator(
            [NotNull]ReverseEngineeringGenerator generator,
            [NotNull]IEntityType entityType,
            [CanBeNull]string namespaceName)
        {
            Generator = generator;
            EntityType = entityType;
            ClassNamespace = namespaceName;
        }

        public virtual ReverseEngineeringGenerator Generator { get; }

        public virtual IEntityType EntityType { get; }

        public virtual string ClassName
        {
            get
            {
                return EntityType.Name;
            }
        }

        public virtual string ClassNamespace { get; }

        public virtual void Generate(IndentedStringBuilder sb)
        {
            GenerateCommentHeader(sb);
            GenerateUsings(sb);
            CSharpCodeGeneratorHelper.Instance.BeginNamespace(ClassNamespace, sb);
            CSharpCodeGeneratorHelper.Instance.BeginClass(AccessModifier.Public, ClassName, isPartial: true, sb: sb);
            GenerateConstructors(sb);
            GenerateProperties(sb);
            CSharpCodeGeneratorHelper.Instance.EndClass(sb);
            CSharpCodeGeneratorHelper.Instance.EndNamespace(sb);
        }

        public virtual void GenerateCommentHeader(IndentedStringBuilder sb)
        {
            CSharpCodeGeneratorHelper.Instance.SingleLineComment(string.Empty, sb);
            CSharpCodeGeneratorHelper.Instance.SingleLineComment("Generated code", sb);
            CSharpCodeGeneratorHelper.Instance.SingleLineComment(string.Empty, sb);
            sb.AppendLine();
        }

        public virtual void GenerateUsings(IndentedStringBuilder sb)
        {
            foreach (var @namespace in _usedNamespaces.Concat(
                EntityType.Properties.Select(p => p.PropertyType.Namespace)
                    .Distinct().Except(_usedNamespaces).OrderBy(ns => ns)))
            {
                CSharpCodeGeneratorHelper.Instance.AddUsingStatement(@namespace, sb);
            }

            if (_usedNamespaces.Any())
            {
                sb.AppendLine();
            }
        }

        public virtual void GenerateConstructors(IndentedStringBuilder sb)
        {
            GenerateZeroArgConstructor(sb);
        }

        public virtual void GenerateZeroArgConstructor(IndentedStringBuilder sb)
        {
            CSharpCodeGeneratorHelper.Instance.BeginConstructor(AccessModifier.Public, ClassName, sb);
            GenerateZeroArgConstructorContents(sb);
            CSharpCodeGeneratorHelper.Instance.EndConstructor(sb);
        }

        public virtual void GenerateZeroArgConstructorContents(IndentedStringBuilder sb)
        {
        }

        public virtual void GenerateProperties(IndentedStringBuilder sb)
        {
            GenerateEntityProperties(sb);
            GenerateEntityNavigations(sb);
        }

        public virtual void GenerateEntityProperties(IndentedStringBuilder sb)
        {
            foreach(var property in OrderedEntityProperties())
            {
                GenerateEntityProperty(property, sb);
            }
        }

        public abstract void GenerateEntityProperty(IProperty property, IndentedStringBuilder sb);

        public abstract void GenerateEntityNavigations(IndentedStringBuilder sb);

        public virtual IEnumerable<IProperty> OrderedEntityProperties()
        {
            return ModelUtilities.Instance.OrderedProperties(EntityType);
        }
    }
}