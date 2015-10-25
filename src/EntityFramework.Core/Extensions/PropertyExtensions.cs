// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public static class PropertyExtensions
    {
        public static int? GetMaxLength([NotNull] this IProperty property)
        {
            Check.NotNull(property, nameof(property));

            return (int?)property[CoreAnnotationNames.MaxLengthAnnotation];
        }

        public static bool IsForeignKey([NotNull] this IProperty property)
            => FindContainingForeignKeys(property).Any();

        public static bool IsPrimaryKey([NotNull] this IProperty property)
            => FindContainingPrimaryKey(property) != null;

        public static bool IsKey([NotNull] this IProperty property)
            => FindContainingKeys(property).Any();

        public static IEnumerable<IForeignKey> FindContainingForeignKeys([NotNull] this IProperty property)
        {
            Check.NotNull(property, nameof(property));

            var entityType = property.DeclaringEntityType;
            return entityType.GetAllBaseTypesInclusive()
                .Concat(entityType.GetDerivedTypes())
                .SelectMany(et => et.GetDeclaredForeignKeys())
                .Where(k => k.Properties.Contains(property));
        }

        public static IKey FindContainingPrimaryKey([NotNull] this IProperty property)
        {
            Check.NotNull(property, nameof(property));

            var pk = property.DeclaringEntityType.FindPrimaryKey();
            if (pk != null
                && pk.Properties.Contains(property))
            {
                return pk;
            }

            return null;
        }

        public static IEnumerable<IKey> FindContainingKeys([NotNull] this IProperty property)
        {
            Check.NotNull(property, nameof(property));

            return property.DeclaringEntityType.GetKeys().Where(e => e.Properties.Contains(property));
        }
    }
}
