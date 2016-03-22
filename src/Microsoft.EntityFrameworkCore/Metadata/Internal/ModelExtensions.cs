// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public static class ModelExtensions
    {
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

        public static IEnumerable<IEntityType> GetRootEntityTypes([NotNull] this IModel model)
            => model.GetEntityTypes().Where(e => e.BaseType == null);
    }
}
