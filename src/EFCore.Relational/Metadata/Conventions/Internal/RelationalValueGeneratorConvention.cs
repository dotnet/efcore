// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    public class RelationalValueGeneratorConvention : ValueGeneratorConvention, IPropertyAnnotationSetConvention
    {
        public virtual Annotation Apply(InternalPropertyBuilder propertyBuilder, string name, Annotation annotation, Annotation oldAnnotation)
        {
            var property = propertyBuilder.Metadata;
            if (name == RelationalAnnotationNames.DefaultValue
                || name == RelationalAnnotationNames.DefaultValueSql
                || name == RelationalAnnotationNames.ComputedColumnSql)
            {
                propertyBuilder.ValueGenerated(GetValueGenerated(property), ConfigurationSource.Convention);
            }

            return annotation;
        }

        public override ValueGenerated? GetValueGenerated(Property property)
        {
            var valueGenerated = base.GetValueGenerated(property);
            if (valueGenerated != null)
            {
                return valueGenerated;
            }

            var relationalProperty = property.Relational();
            return relationalProperty.ComputedColumnSql != null
                ? ValueGenerated.OnAddOrUpdate
                : relationalProperty.DefaultValue != null
                  || relationalProperty.DefaultValueSql != null
                    ? ValueGenerated.OnAdd
                    : (ValueGenerated?)null;
        }
    }
}
