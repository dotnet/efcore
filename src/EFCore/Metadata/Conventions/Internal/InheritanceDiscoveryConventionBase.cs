// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public abstract class InheritanceDiscoveryConventionBase
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual EntityType FindClosestBaseType([NotNull] EntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            var clrType = entityType.ClrType;
            Check.NotNull(clrType, nameof(entityType.ClrType));

            var baseType = clrType.GetTypeInfo().BaseType;
            EntityType baseEntityType = null;

            while (baseType != null
                   && baseEntityType == null)
            {
                baseEntityType = entityType.Model.FindEntityType(baseType);
                baseType = baseType.GetTypeInfo().BaseType;
            }
            return baseEntityType;
        }
    }
}
