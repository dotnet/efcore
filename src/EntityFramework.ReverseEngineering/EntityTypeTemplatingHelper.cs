// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
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
            return UniqueSortedList(
                EntityTypeTemplateModel.EntityType.Properties.Select(p => p.PropertyType.Namespace),
                "using ",
                ";" + Environment.NewLine);
        }

        public IEnumerable<IProperty> SortedProperties()
        {
            var primaryKeyPropertiesList = new List<IProperty>(
                EntityTypeTemplateModel.EntityType.GetPrimaryKey().Properties.OrderBy(p => p.Name));
            return primaryKeyPropertiesList.Concat(
                EntityTypeTemplateModel.EntityType.Properties
                .Where(p => !primaryKeyPropertiesList.Contains(p)).OrderBy(p => p.Name));
        }
    }
}