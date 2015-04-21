// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Metadata
{
    public static class EntityTypeExtensions
    {
        public static IEnumerable<Type> GetValueTypes([NotNull] this IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            var concreteEntityTypes = entityType.GetConcreteTypesInHierarchy().ToList();

            var valueTypes = concreteEntityTypes[0].GetProperties().Select(p => p.ClrType).ToList();

            if (concreteEntityTypes.Count > 1
                || entityType != concreteEntityTypes[0])
            {
                valueTypes.Add(typeof(byte));
            }

            return valueTypes;
        }
    }
}
