// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public static class ModelExtensions
    {
        public static IEnumerable<INavigation> GetNavigations(
            [NotNull] this IModel model, [NotNull] IForeignKey foreignKey)
        {
            Check.NotNull(model, nameof(model));

            // TODO: Perf: consider not needing to do a full scan here
            return model.EntityTypes.SelectMany(e => e.GetNavigations()).Where(n => n.ForeignKey == foreignKey);
        }

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
            return model.EntityTypes.SelectMany(et => et.GetDeclaredForeignKeys())
                .Where(fk => fk.PrincipalEntityType == entityType);
        }

        public static IEnumerable<IForeignKey> FindReferencingForeignKeys([NotNull] this IModel model, [NotNull] IEntityType entityType)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(entityType, nameof(entityType));

            // TODO: Perf: Add additional indexes so that this isn't a linear lookup
            // Issue #1179
            return model.EntityTypes.SelectMany(et => et.GetDeclaredForeignKeys())
                .Where(fk => fk.PrincipalEntityType.IsAssignableFrom(entityType));
        }

        public static IEnumerable<IForeignKey> FindReferencingForeignKeys([NotNull] this IModel model, [NotNull] IKey key)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(key, nameof(key));

            // TODO: Perf: Add additional indexes so that this isn't a linear lookup
            // Issue #1179
            return model.EntityTypes.SelectMany(e => e.GetDeclaredForeignKeys()).Where(fk => fk.PrincipalKey == key);
        }

        public static IEnumerable<IForeignKey> FindReferencingForeignKeys([NotNull] this IModel model, [NotNull] IProperty property)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(property, nameof(property));

            // TODO: Perf: Add additional indexes so that this isn't a linear lookup
            // Issue #1179
            return model.EntityTypes
                .SelectMany(e => e.GetDeclaredForeignKeys()
                    .Where(f => f.PrincipalKey.Properties.Contains(property)));
        }
    }
}
