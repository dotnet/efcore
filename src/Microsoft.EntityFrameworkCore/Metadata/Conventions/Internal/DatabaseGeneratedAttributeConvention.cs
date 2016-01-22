// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    public class DatabaseGeneratedAttributeConvention : PropertyAttributeConvention<DatabaseGeneratedAttribute>
    {
        public override InternalPropertyBuilder Apply(InternalPropertyBuilder propertyBuilder, DatabaseGeneratedAttribute attribute, PropertyInfo clrProperty)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NotNull(attribute, nameof(attribute));

            var valueGenerated =
                attribute.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity
                    ? ValueGenerated.OnAdd
                    : attribute.DatabaseGeneratedOption == DatabaseGeneratedOption.Computed
                        ? ValueGenerated.OnAddOrUpdate
                        : ValueGenerated.Never;

            propertyBuilder.ValueGenerated(valueGenerated, ConfigurationSource.DataAnnotation);

            return propertyBuilder;
        }
    }
}
