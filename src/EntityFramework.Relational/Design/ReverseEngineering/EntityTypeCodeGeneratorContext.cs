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
    public abstract class EntityTypeCodeGeneratorContext : ModelCodeGeneratorContext
    {
        private readonly IEntityType _entityType;
        private readonly string _namespaceName;
        private readonly DbContextCodeGeneratorContext _contextCodeGeneratorContext;

        protected Dictionary<IProperty, string> _propertyToPropertyNameMap = new Dictionary<IProperty, string>();

        public EntityTypeCodeGeneratorContext(
            [NotNull]IEntityType entityType,
            [CanBeNull]string namespaceName,
            [NotNull]DbContextCodeGeneratorContext contextCodeGeneratorContext)
        {
            _entityType = entityType;
            _namespaceName = namespaceName;
            _contextCodeGeneratorContext = contextCodeGeneratorContext;
            InitializePropertyNames();
        }

        private void InitializePropertyNames()
        {
            foreach (var property in _entityType.Properties)
            {
                _propertyToPropertyNameMap[property] =
                    CSharpUtilities.Instance.GenerateCSharpIdentifier(
                        property.Name, _propertyToPropertyNameMap.Values);
            }
        }

        public override string GetClassName()
        {
            return _contextCodeGeneratorContext.EntityTypeToClassNameMap[_entityType];
        }

        public override string GetClassNamespace()
        {
            return _namespaceName;
        }

        public Dictionary<IProperty, string> PropertyToPropertyNameMap
        {
            get
            {
                return _propertyToPropertyNameMap;
            }
        }

        public override void GenerateCommentHeader(IndentedStringBuilder sb)
        {
            sb.AppendLine("//");
            sb.AppendLine("// Generated code");
            sb.AppendLine("//");
            sb.AppendLine();
        }

        public override void GenerateProperties(IndentedStringBuilder sb)
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

        //
        // helper methods
        //

        public override IEnumerable<string> GetUsedNamespaces()
        {
            return base.GetUsedNamespaces().Concat(
                _entityType.Properties.Select(p => p.PropertyType.Namespace).Distinct().OrderBy(p => p));
        }


        public virtual IEnumerable<IProperty> OrderedEntityProperties()
        {
            var primaryKeyPropertiesList = new List<IProperty>();
            IKey key = _entityType.TryGetPrimaryKey();
            if (key != null)
            {
                primaryKeyPropertiesList =
                    new List<IProperty>(
                        key.Properties.OrderBy(p => _propertyToPropertyNameMap[p]));
            }

            return primaryKeyPropertiesList.Concat(
                _entityType.Properties
                    .Where(p => !primaryKeyPropertiesList.Contains(p))
                    .OrderBy(p => _propertyToPropertyNameMap[p]));
        }

        public virtual IEnumerable<INavigation> OrderedEntityNavigations()
        {
            //TODO - do we need a map to CSharp names?
            return _entityType.Navigations.OrderBy(nav => nav.Name);
        }
    }
}