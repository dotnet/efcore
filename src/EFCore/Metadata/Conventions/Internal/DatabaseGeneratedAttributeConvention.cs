// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    // Issue#11266 This type is being used by provider code. Do not break.
    public class DatabaseGeneratedAttributeConvention : PropertyAttributeConvention<DatabaseGeneratedAttribute>
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override InternalPropertyBuilder Apply(
            InternalPropertyBuilder propertyBuilder, DatabaseGeneratedAttribute attribute, MemberInfo clrMember)
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
