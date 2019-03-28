// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
    public interface IMutableAnnotatable : IAnnotatable
    {
        /// <summary>
        ///     Gets or sets the value of the annotation with the given name.
        /// </summary>
        /// <param name="name"> The name of the annotation. </param>
        /// <returns>
        ///     The value of the existing annotation if an annotation with the specified name already exists. Otherwise, <c>null</c>.
        /// </returns>
        new object this[[NotNull] string name] { get; [param: CanBeNull] set; }

        /// <summary>
        ///     Adds an annotation to this object. Throws if an annotation with the specified name already exists.
        /// </summary>
        /// <param name="name"> The name of the annotation to be added. </param>
        /// <param name="value"> The value to be stored in the annotation. </param>
        /// <returns> The newly added annotation. </returns>
        IAnnotation AddAnnotation([NotNull] string name, [CanBeNull] object value);

        /// <summary>
        ///     Sets the annotation stored under the given key. Overwrites the existing annotation if an
        ///     annotation with the specified name already exists.
        /// </summary>
        /// <param name="name"> The name of the annotation to be added. </param>
        /// <param name="value"> The value to be stored in the annotation. </param>
        void SetAnnotation([NotNull] string name, [CanBeNull] object value);

        /// <summary>
        ///     Removes the given annotation from this object.
        /// </summary>
        /// <param name="name"> The name of the annotation to remove. </param>
        /// <returns> The annotation that was removed. </returns>
        IAnnotation RemoveAnnotation([NotNull] string name);
    }
}
