// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ConcurrencyCheckAttributeConvention : PropertyAttributeConvention<ConcurrencyCheckAttribute>
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override InternalPropertyBuilder Apply(
            InternalPropertyBuilder propertyBuilder, ConcurrencyCheckAttribute attribute, MemberInfo clrMember)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NotNull(attribute, nameof(attribute));

            propertyBuilder.IsConcurrencyToken(true, ConfigurationSource.DataAnnotation);

            return propertyBuilder;
        }
    }
}
