// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Base class for helpers that can handle setting annotations when used with
    ///     conventions that may or may not be able to override an annotation that has
    ///     already been set.
    /// </summary>
    public class RelationalAnnotations
    {
        /// <summary>
        ///     Constructs a new helper for the given <see cref="IAnnotatable" /> metadata item.
        /// </summary>
        /// <param name="metadata"> The metadata item to be annotated. </param>
        public RelationalAnnotations([NotNull] IAnnotatable metadata)
        {
            Check.NotNull(metadata, nameof(metadata));

            Metadata = metadata;
        }

        /// <summary>
        ///     The metadata item that is being annotated.
        /// </summary>
        public virtual IAnnotatable Metadata { get; }

        /// <summary>
        ///     Attempts to set an annotation with the given name to the given value and
        ///     returns whether or not this was successful.
        /// </summary>
        /// <param name="annotationName"> The name of the annotation to set. </param>
        /// <param name="value"> The value to set. </param>
        /// <returns><c>True</c> if the annotation was set; <c>false</c> otherwise. </returns>
        public virtual bool SetAnnotation(
            [NotNull] string annotationName,
            [CanBeNull] object value)
        {
            ((IMutableAnnotatable)Metadata).SetAnnotation(annotationName, value);

            return true;
        }

        /// <summary>
        ///     Checks whether or not the annotation with the given name can be set to the given value.
        /// </summary>
        /// <param name="relationalAnnotationName"> The name of the annotation to set. </param>
        /// <param name="value"> The value to set. </param>
        /// <returns><c>True</c> if the annotation can be set; <c>false</c> otherwise. </returns>
        public virtual bool CanSetAnnotation(
            [NotNull] string relationalAnnotationName,
            [CanBeNull] object value)
            => true;

        /// <summary>
        ///     Attempts to remove an annotation with the given name and
        ///     returns whether or not this was successful.
        /// </summary>
        /// <param name="annotationName"> The name of the annotation to remove. </param>
        /// <returns><c>True</c> if the annotation was removed; <c>false</c> otherwise. </returns>
        public virtual bool RemoveAnnotation([NotNull] string annotationName)
        {
            ((IMutableAnnotatable)Metadata).RemoveAnnotation(annotationName);

            return true;
        }
    }
}
