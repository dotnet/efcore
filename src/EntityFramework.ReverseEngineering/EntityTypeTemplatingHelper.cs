// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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

        public string Usings()
        {
            var propertyTypeNamespaces =
                EntityTypeTemplateModel.EntityType.Properties.Select(p => p.PropertyType.Namespace);
            var navigationTypeNamespaces =
                EntityTypeTemplateModel.EntityType.Navigations.Select(n => n.GetTargetType().Type.Namespace);

            return ConstructUsings(propertyTypeNamespaces.Concat(navigationTypeNamespaces));
        }

        public IEnumerable<IProperty> SortedProperties()
        {
            var primaryKeyPropertiesList = new List<IProperty>(
                EntityTypeTemplateModel.EntityType.GetPrimaryKey().Properties.OrderBy(p => p.Name));
            return primaryKeyPropertiesList.Concat(
                EntityTypeTemplateModel.EntityType.Properties
                .Where(p => !primaryKeyPropertiesList.Contains(p)).OrderBy(p => p.Name));
        }

        public IEnumerable<INavigation> SortedNavigations()
        {
            return EntityTypeTemplateModel.EntityType.Navigations.OrderBy(n => n.Name);
        }

        public string NavigationsCode(string indent)
        {
            var sb = new StringBuilder();
            foreach (var nav in SortedNavigations())
            {
                sb.AppendLine();
                sb.Append(indent);
                if (nav.IsCollection())
                {
                    sb.Append("public virtual ICollection<");
                    sb.Append(nav.GetTargetType().Type.Name);
                    sb.Append("> ");
                    sb.Append(nav.Name);
                    sb.Append(" { get; set; }");
                }
                else
                {
                    sb.Append("public virtual ");
                    sb.Append(nav.GetTargetType().Type.Name);
                    sb.Append(" ");
                    sb.Append(nav.Name);
                    sb.Append(" { get; set; }");
                }
            }

            return sb.ToString();
        }
    }
}