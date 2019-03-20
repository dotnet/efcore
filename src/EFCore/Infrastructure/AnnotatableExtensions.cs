// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     Extension methods for <see cref="IAnnotatable" />.
    /// </summary>
    public static class AnnotatableExtensions
    {
        /// <summary>
        ///     Gets the annotation with the given name, throwing if it does not exist.
        /// </summary>
        /// <param name="annotatable"> The object to find the annotation on. </param>
        /// <param name="annotationName"> The key of the annotation to find. </param>
        /// <returns> The annotation with the specified name. </returns>
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
