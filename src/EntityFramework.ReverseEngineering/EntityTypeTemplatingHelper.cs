// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.ReverseEngineering
{
    public class EntityTypeTemplatingHelper : BaseTemplatingHelper
    {
        public EntityTypeTemplatingHelper(EntityTypeTemplateModel model) : base(model) { }

        public EntityTypeTemplateModel EntityTypeTemplateModel
        {
            get { return Model as EntityTypeTemplateModel; }
        }

        public virtual string Usings()
        {
            var propertyTypeNamespaces =
                EntityTypeTemplateModel.EntityType.Properties.Select(p => p.PropertyType.Namespace);
            var navigationTypeNamespaces =
                EntityTypeTemplateModel.EntityType.Navigations.Select(n => n.GetTargetType().Type.Namespace);

            return ConstructUsings(propertyTypeNamespaces.Concat(navigationTypeNamespaces));
        }

        public virtual IEnumerable<IProperty> SortedProperties()
        {
            var primaryKeyPropertiesList = new List<IProperty>(
                EntityTypeTemplateModel.EntityType.GetPrimaryKey().Properties.OrderBy(p => p.Name));
            return primaryKeyPropertiesList.Concat(
                EntityTypeTemplateModel.EntityType.Properties
                .Where(p => !primaryKeyPropertiesList.Contains(p)).OrderBy(p => p.Name));
        }

        public virtual IEnumerable<INavigation> SortedNavigations()
        {
            return EntityTypeTemplateModel.EntityType.Navigations.OrderBy(n => n.Name);
        }

        public virtual string PropertiesCode(string indent)
        {
            var sb = new StringBuilder();
            var first = true;
            foreach (var property in SortedProperties())
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sb.AppendLine();
                }

                sb.Append(PropertyAttributesCode(indent, property));
                sb.Append(indent);
                sb.Append("public ");
                sb.Append(property.PropertyType.Name);
                sb.Append(" ");
                sb.Append(property.Name);
                sb.Append(" { get; set; }");
            }

            return sb.ToString();
        }

        public virtual string PropertyAttributesCode(string indent, IProperty property)
        {
            return string.Empty;
        }

        public virtual string NavigationsCode(string indent)
        {
            var sb = new StringBuilder();
            foreach (var nav in SortedNavigations())
            {
                sb.Append(NavigationAttributesCode(indent, nav));
                sb.AppendLine();
                sb.Append(indent);
                sb.Append("public virtual ");
                if (nav.IsCollection())
                {
                    sb.Append("ICollection<");
                    sb.Append(nav.GetTargetType().Type.Name);
                    sb.Append(">");
                }
                else
                {
                    sb.Append(nav.GetTargetType().Type.Name);
                }
                sb.Append(" ");
                sb.Append(nav.Name);
                sb.Append(" { get; set; }");
            }

            return sb.ToString();
        }

        public virtual string NavigationAttributesCode(string indent, INavigation navigation)
        {
            return string.Empty;
        }
    }
}