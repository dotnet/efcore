// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         A class that exposes annotations. Annotations allow for arbitrary metadata to be stored on an object.
    ///     </para>
    ///     <para>
    ///         This interface is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public interface IAnnotatable
    {
        /// <summary>
        ///     Gets the value annotation with the given name, returning <see langword="null" /> if it does not exist.
        /// </summary>
        /// <param name="name"> The name of the annotation to find. </param>
        /// <returns>
        ///     The value of the existing annotation if an annotation with the specified name already exists. Otherwise, <see langword="null" />.
        /// </returns>
        object this[[NotNull] string name] { get; }

        /// <summary>
        ///     Gets the annotation with the given name, returning <see langword="null" /> if it does not exist.
        /// </summary>
        /// <param name="name"> The name of the annotation to find. </param>
        /// <returns>
        ///     The existing annotation if an annotation with the specified name already exists. Otherwise, <see langword="null" />.
        /// </returns>
        IAnnotation FindAnnotation([NotNull] string name);

        /// <summary>
        ///     Gets all annotations on the current object.
        /// </summary>
        IEnumerable<IAnnotation> GetAnnotations();
    }
}
