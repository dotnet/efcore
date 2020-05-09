// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="IMutableAnnotatable" />.
    /// </summary>
    public static class MutableAnnotatableExtensions
    {
        /// <summary>
        ///     Gets the annotation with the given name, throwing if it does not exist.
        /// </summary>
        /// <param name="annotatable"> The object to find the annotation on. </param>
        /// <param name="annotationName"> The key of the annotation to find. </param>
        /// <returns> The annotation with the specified name. </returns>
        public static IAnnotation GetAnnotation([NotNull] this IMutableAnnotatable annotatable, [NotNull] string annotationName)
            => ((IAnnotatable)annotatable).GetAnnotation(annotationName);

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

        /// <summary>
        ///     Sets the annotation stored under the given name. Overwrites the existing annotation if an
        ///     annotation with the specified name already exists. Removes the existing annotation if <see langword="null" /> is supplied.
        /// </summary>
        /// <param name="annotatable"> The object to set the annotation for. </param>
        /// <param name="name"> The name of the annotation to be added. </param>
        /// <param name="value"> The value to be stored in the annotation. </param>
        public static void SetOrRemoveAnnotation(
            [NotNull] this IMutableAnnotatable annotatable,
            [NotNull] string name,
            [CanBeNull] object value)
            => ((ConventionAnnotatable)annotatable).SetOrRemoveAnnotation(name, value, ConfigurationSource.Explicit);
    }
}
