// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class DerivedTypeDiscoveryConvention : InheritanceDiscoveryConventionBase, IEntityTypeAddedConvention
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityTypeBuilder Apply(InternalEntityTypeBuilder entityTypeBuilder)
        {
            var entityType = entityTypeBuilder.Metadata;
            var clrType = entityType.ClrType;
            if (clrType == null
                || entityType.HasDefiningNavigation())
            {
                return entityTypeBuilder;
            }

            var model = entityType.Model;
            var directlyDerivedTypes = model.GetEntityTypes().Where(
                t => t != entityType
                     && t.HasClrType()
                     && !t.HasDefiningNavigation()
                     && t.FindDeclaredOwnership() == null
                     && !model.ShouldBeOwnedType(model.GetDisplayName(t.ClrType))
                     && ((t.BaseType == null && clrType.GetTypeInfo().IsAssignableFrom(t.ClrType.GetTypeInfo()))
                         || (t.BaseType == entityType.BaseType && FindClosestBaseType(t) == entityType)))
                .ToList();

            foreach (var directlyDerivedType in directlyDerivedTypes)
            {
                directlyDerivedType.Builder.HasBaseType(entityType, ConfigurationSource.Convention);
            }

            return entityTypeBuilder;
        }
    }
}
