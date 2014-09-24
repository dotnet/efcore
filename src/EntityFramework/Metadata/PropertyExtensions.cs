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
            Check.NotNull(property, "property");

            // TODO: Perf: Avoid doing Contains check everywhere we need to know if a property is part of a foreign key
            return property.EntityType.ForeignKeys.SelectMany(k => k.Properties).Contains(property);
        }

        public static bool IsPrimaryKey([NotNull] this IProperty property)
        {
            Check.NotNull(property, "property");

            // TODO: Perf: make it fast to check if a property is part of the primary key
            return property.EntityType.GetPrimaryKey().Properties.Contains(property);
        }

        public static bool IsKey([NotNull] this IProperty property)
        {
            Check.NotNull(property, "property");

            // TODO: Perf: make it fast to check if a property is part of a key
            return property.EntityType.Keys.SelectMany(e => e.Properties).Contains(property);
        }

        public static string FindAnnotationInHierarchy([NotNull] this IProperty property, [NotNull] string name)
        {
            Check.NotNull(property, "property");
            Check.NotEmpty(name, "name");

            var value = property[name];
            if (value != null)
            {
                return value;
            }

            value = property.EntityType[name];
            if (value != null
                || property.EntityType.Model == null)
            {
                return value;
            }

            return property.EntityType.Model[name];
        }
    }
}
