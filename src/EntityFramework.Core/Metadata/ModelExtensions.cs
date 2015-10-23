// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public static class ModelExtensions
    {
        public static IEntityType FindEntityType([NotNull] this IModel model, [NotNull] Type type)
        {
            Check.NotNull(type, nameof(type));

            return model.FindEntityType(type.DisplayName());
        }

        public static IMutableEntityType FindEntityType([NotNull] this IMutableModel model, [NotNull] Type type)
            => (IMutableEntityType)((IModel)model).FindEntityType(type);

        public static string GetProductVersion([NotNull] this IModel model)
        {
            Check.NotNull(model, nameof(model));

            return model[CoreAnnotationNames.ProductVersionAnnotation] as string;
        }

        public static void SetProductVersion([NotNull] this Model model, [NotNull] string value)
        {
            Check.NotNull(model, nameof(model));
            Check.NotEmpty(value, nameof(value));

            model[CoreAnnotationNames.ProductVersionAnnotation] = value;
        }
        
        public static IEnumerable<IForeignKey> FindDeclaredReferencingForeignKeys([NotNull] this IModel model, [NotNull] IEntityType entityType)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(entityType, nameof(entityType));

            // TODO: Perf: Add additional indexes so that this isn't a linear lookup
            // Issue #1179
            return model.GetEntityTypes().SelectMany(et => et.GetDeclaredForeignKeys())
                .Where(fk => fk.PrincipalEntityType == entityType);
        }

        public static IEnumerable<IForeignKey> FindReferencingForeignKeys([NotNull] this IModel model, [NotNull] IEntityType entityType)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(entityType, nameof(entityType));

            // TODO: Perf: Add additional indexes so that this isn't a linear lookup
            // Issue #1179
            return model.GetEntityTypes().SelectMany(et => et.GetDeclaredForeignKeys())
                .Where(fk => fk.PrincipalEntityType.IsAssignableFrom(entityType));
        }

        public static IEnumerable<IForeignKey> FindReferencingForeignKeys([NotNull] this IModel model, [NotNull] IKey key)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(key, nameof(key));

            // TODO: Perf: Add additional indexes so that this isn't a linear lookup
            // Issue #1179
            return model.GetEntityTypes().SelectMany(e => e.GetDeclaredForeignKeys()).Where(fk => fk.PrincipalKey == key);
        }

        public static IEnumerable<IForeignKey> FindReferencingForeignKeys([NotNull] this IModel model, [NotNull] IProperty property)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(property, nameof(property));

            // TODO: Perf: Add additional indexes so that this isn't a linear lookup
            // Issue #1179
            return model.GetEntityTypes()
                .SelectMany(e => e.GetDeclaredForeignKeys()
                    .Where(f => f.PrincipalKey.Properties.Contains(property)));
        }
    }
}
