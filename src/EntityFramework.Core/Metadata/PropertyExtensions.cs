// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public static class PropertyExtensions
    {
        public static bool IsForeignKey([NotNull] this IProperty property)
        {
            Check.NotNull(property, nameof(property));

            // TODO: Perf: Avoid doing Contains check everywhere we need to know if a property is part of a foreign key
            return property.EntityType.ForeignKeys.SelectMany(k => k.Properties).Contains(property);
        }

        public static bool IsPrimaryKey([NotNull] this IProperty property)
        {
            Check.NotNull(property, nameof(property));

            // TODO: Perf: make it fast to check if a property is part of the primary key
            return property.EntityType.GetPrimaryKey().Properties.Contains(property);
        }

        public static bool IsKey([NotNull] this IProperty property)
        {
            Check.NotNull(property, nameof(property));

            // TODO: Perf: make it fast to check if a property is part of a key
            return property.EntityType.Keys.SelectMany(e => e.Properties).Contains(property);
        }

        public static bool IsSentinelValue([NotNull] this IProperty property, [CanBeNull] object value)
        {
            Check.NotNull(property, nameof(property));

            return value == null || value.Equals(property.SentinelValue);
        }
    }
}
