// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Internal
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
    }
}
