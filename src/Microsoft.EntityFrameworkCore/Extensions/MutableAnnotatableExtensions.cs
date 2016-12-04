// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="IMutableAnnotatable" />.
    /// </summary>
    public static class MutableAnnotatableExtensions
    {
        /// <summary>
        ///     Gets the existing annotation with a given key, or adds a new annotation if one does not exist.
        /// </summary>
        /// <param name="annotatable"> The object to find or add the annotation to. </param>
        /// <param name="annotationName"> The key of the annotation to be found or added. </param>
        /// <param name="value"> The value to be stored in the annotation if a new one is created. </param>
        /// <returns> The found or added annotation. </returns>
        public static Annotation GetOrAddAnnotation(
            [NotNull] this IMutableAnnotatable annotatable, [NotNull] string annotationName, [NotNull] string value)
            => annotatable.FindAnnotation(annotationName) ?? annotatable.AddAnnotation(annotationName, value);

        /// <summary>
        ///     Adds annotations to an object.
        /// </summary>
        /// <param name="annotatable"> The object to add the annotations to. </param>
        /// <param name="annotations"> The annotations to be added. </param>
        public static void AddAnnotations(
            [NotNull] this IMutableAnnotatable annotatable,
            [NotNull] IEnumerable<IAnnotation> annotations)
        {
            foreach (var annotation in annotations)
            {
                annotatable.AddAnnotation(annotation.Name, annotation.Value);
            }
        }
    }
}
