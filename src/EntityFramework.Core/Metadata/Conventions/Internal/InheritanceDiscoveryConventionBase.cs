// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Conventions.Internal
{
    public class InheritanceDiscoveryConventionBase
    {
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
