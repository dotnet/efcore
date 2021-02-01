// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         A class that exposes build-time and run-time annotations. Annotations allow for arbitrary metadata to be stored on an object.
    ///     </para>
    ///     <para>
    ///         This interface is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public interface IAnnotatable : IReadOnlyAnnotatable
    {
        /// <summary>
        ///     Gets the runtime annotation with the given name, returning <see langword="null" /> if it does not exist.
        /// </summary>
        /// <param name="name"> The name of the annotation to find. </param>
        /// <returns>
        ///     The existing runtime annotation if an annotation with the specified name already exists. Otherwise, <see langword="null" />.
        /// </returns>
        IAnnotation? FindRuntimeAnnotation([NotNull] string name);

        /// <summary>
        ///     Gets the value of the runtime annotation with the given name, returning <see langword="null" /> if it does not exist.
        /// </summary>
        /// <param name="name"> The name of the annotation to find. </param>
        /// <returns>
        ///     The value of the existing runtime annotation if an annotation with the specified name already exists.
        ///     Otherwise, <see langword="null" />.
        /// </returns>
        object? FindRuntimeAnnotationValue([NotNull] string name)
            => FindRuntimeAnnotation(name)?.Value;

        /// <summary>
        ///     Gets all the runtime annotations on the current object.
        /// </summary>
        IEnumerable<IAnnotation> GetRuntimeAnnotations();

        /// <summary>
        ///     Adds a runtime annotation to this object. Throws if an annotation with the specified name already exists.
        /// </summary>
        /// <param name="name"> The name of the annotation to be added. </param>
        /// <param name="value"> The value to be stored in the annotation. </param>
        /// <returns> The newly added annotation. </returns>
        IAnnotation AddRuntimeAnnotation([NotNull] string name, [CanBeNull] object? value);

        /// <summary>
        ///     Sets the runtime annotation stored under the given key. Overwrites the existing annotation if an
        ///     annotation with the specified name already exists.
        /// </summary>
        /// <param name="name"> The name of the annotation to be added. </param>
        /// <param name="value"> The value to be stored in the annotation. </param>
        /// <returns> The newly added annotation. </returns>
        IAnnotation SetRuntimeAnnotation([NotNull] string name, [CanBeNull] object? value);

        /// <summary>
        ///     Removes the given runtime annotation from this object.
        /// </summary>
        /// <param name="name"> The name of the annotation to remove. </param>
        /// <returns> The annotation that was removed. </returns>
        IAnnotation? RemoveRuntimeAnnotation([NotNull] string name);

        /// <summary>
        ///     Gets the value of the runtime annotation with the given name, adding it if one does not exist.
        /// </summary>
        /// <param name="name"> The name of the annotation. </param>
        /// <param name="valueFactory"> The factory used to create the value if the annotation doesn't exist. </param>
        /// <param name="factoryArgument"> An argument for the factory method. </param>
        /// <returns>
        ///     The value of the existing runtime annotation if an annotation with the specified name already exists.
        ///     Otherwise a newly created value.
        /// </returns>
        TValue GetOrAddRuntimeAnnotationValue<TValue, TArg>(
            [NotNull] string name,
            [NotNull] Func<TArg?, TValue> valueFactory,
            [CanBeNull] TArg? factoryArgument);
    }
}
