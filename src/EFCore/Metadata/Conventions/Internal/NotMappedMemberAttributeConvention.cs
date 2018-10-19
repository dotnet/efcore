// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class NotMappedMemberAttributeConvention : IEntityTypeAddedConvention
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityTypeBuilder Apply(InternalEntityTypeBuilder entityTypeBuilder)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            var entityType = entityTypeBuilder.Metadata;
            if (!entityType.HasClrType())
            {
                return entityTypeBuilder;
            }

            var members = entityType.GetRuntimeProperties().Values.Cast<MemberInfo>()
                .Concat(entityType.GetRuntimeFields().Values);

            foreach (var member in members)
            {
                if (Attribute.IsDefined(member, typeof(NotMappedAttribute), inherit: true))
                {
                    entityTypeBuilder.Ignore(member.GetSimpleMemberName(), ConfigurationSource.DataAnnotation);
                }
            }

            return entityTypeBuilder;
        }
    }
}
