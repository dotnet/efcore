// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Conventions.Internal
{
    public class NotMappedNavigationAttributeConvention : NavigationAttributeEntityTypeConvention<NotMappedAttribute>
    {
        public override InternalEntityTypeBuilder Apply(InternalEntityTypeBuilder entityTypeBuilder, PropertyInfo navigationPropertyInfo, NotMappedAttribute attribute)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NotNull(navigationPropertyInfo, nameof(navigationPropertyInfo));
            Check.NotNull(attribute, nameof(attribute));

            return entityTypeBuilder.Ignore(navigationPropertyInfo.Name, ConfigurationSource.DataAnnotation) ? null : entityTypeBuilder;
        }
    }
}
