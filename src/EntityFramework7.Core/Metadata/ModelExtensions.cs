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
        // TODO: Perf: consider not needing to do a full scan here
        public static IEnumerable<INavigation> GetNavigations(
            [NotNull] this IModel model, [NotNull] IForeignKey foreignKey)
            => model.EntityTypes.SelectMany(e => e.GetNavigations()).Where(n => n.ForeignKey == foreignKey);

        public static string GetProductVersion([NotNull] this IModel model)
            => model[CoreAnnotationNames.ProductVersionAnnotation] as string;

        public static void SetProductVersion([NotNull] this Model model, [NotNull] string value)
        {
            Check.NotEmpty(value, nameof(value));

            model[CoreAnnotationNames.ProductVersionAnnotation] = value;
        }

        public static string GetContextKey([NotNull] this IModel model)
            => model[CoreAnnotationNames.ContextKeyAnnotation] as string;

        public static void SetContextKey([NotNull] this Model model, [NotNull] string value)
        {
            Check.NotEmpty(value, nameof(value));

            model[CoreAnnotationNames.ContextKeyAnnotation] = value;
        }
    }
}
