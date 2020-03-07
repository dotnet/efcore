// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     <para>
    ///         A class that exposes annotations that can be modified. Annotations allow for arbitrary metadata to be
    ///         stored on an object.
    ///     </para>
    ///     <para>
    ///         This interface is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public interface IConventionAnnotatable : IAnnotatable
    {
        /// <summary>
        ///     Gets all annotations on the current object.
        /// </summary>
        new IEnumerable<IConventionAnnotation> GetAnnotations();

        /// <summary>
        ///     Adds an annotation to this object. Throws if an annotation with the specified name already exists.
        /// </summary>
        /// <param name="name"> The name of the annotation to be added. </param>
        /// <param name="value"> The value to be stored in the annotation. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The newly added annotation. </returns>
        IConventionAnnotation AddAnnotation([NotNull] string name, [CanBeNull] object value, bool fromDataAnnotation = false);

        /// <summary>
        ///     Sets the annotation stored under the given name. Overwrites the existing annotation if an
        ///     annotation with the specified name already exists.
        /// </summary>
        /// <param name="name"> The name of the annotation to be set. </param>
        /// <param name="value"> The value to be stored in the annotation. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        void SetAnnotation([NotNull] string name, [CanBeNull] object value, bool fromDataAnnotation = false);

        /// <summary>
        ///     Gets the annotation with the given name, returning <c>null</c> if it does not exist.
        /// </summary>
        /// <param name="name"> The name of the annotation to find. </param>
        /// <returns>
        ///     The existing annotation if an annotation with the specified name already exists. Otherwise, <c>null</c>.
        /// </returns>
        new IConventionAnnotation FindAnnotation([NotNull] string name);

        /// <summary>
        ///     Removes the annotation with the given name from this object.
        /// </summary>
        /// <param name="name"> The name of the annotation to remove. </param>
        /// <returns> The annotation that was removed. </returns>
        IConventionAnnotation RemoveAnnotation([NotNull] string name);
    }
}
