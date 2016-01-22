// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Infrastructure
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
