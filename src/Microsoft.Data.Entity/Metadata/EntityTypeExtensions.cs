// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public static class EntityTypeExtensions
    {
        public static IEnumerable<IPropertyBase> GetPropertiesAndNavigations([NotNull] this IEntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            return entityType.Properties.Concat<IPropertyBase>(entityType.Navigations);
        }

        [NotNull]
        public static IEnumerable<IForeignKey> GetReferencingForeignKeys([NotNull] this IEntityType entityType)
        {
            return entityType.Model.GetReferencingForeignKeys(entityType);
        }
    }
}
