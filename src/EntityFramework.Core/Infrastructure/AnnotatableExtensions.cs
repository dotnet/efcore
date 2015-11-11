// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Infrastructure
{
    public static class AnnotatableExtensions
    {
        public static IAnnotation GetAnnotation([NotNull] this IAnnotatable annotatable, [NotNull] string annotationName)
        {
            Check.NotNull(annotatable, nameof(annotatable));
            Check.NotEmpty(annotationName, nameof(annotationName));

            var annotation = annotatable.FindAnnotation(annotationName);
            if (annotation == null)
            {
                throw new InvalidOperationException(CoreStrings.AnnotationNotFound(annotationName));
            }

            return annotation;
        }
    }
}
