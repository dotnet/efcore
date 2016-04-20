// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public static class ModelExtensions
    {
        public static string GetProductVersion([NotNull] this IModel model)
            => model[CoreAnnotationNames.ProductVersionAnnotation] as string;

        public static void SetProductVersion([NotNull] this Model model, [NotNull] string value)
            => model[CoreAnnotationNames.ProductVersionAnnotation] = value;

        public static IEnumerable<IEntityType> GetRootEntityTypes([NotNull] this IModel model)
            => model.GetEntityTypes().Where(e => e.BaseType == null);

        public static Model AsModel([NotNull] this IModel model, [CallerMemberName] [NotNull] string methodName = "")
            => model.AsConcreteMetadataType<IModel, Model>(methodName);
    }
}
