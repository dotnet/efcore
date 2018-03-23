// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    // Issue#11266 This type is being used by provider code. Do not break.
    public class RelationalValueGeneratorConvention : ValueGeneratorConvention, IPropertyAnnotationChangedConvention
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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
