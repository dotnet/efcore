// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

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
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public interface IMutableAnnotatable : IReadOnlyAnnotatable
{
    /// <summary>
    ///     Gets or sets the value of the annotation with the given name.
    /// </summary>
    /// <param name="name">The name of the annotation.</param>
    /// <returns>
    ///     The value of the existing annotation if an annotation with the specified name already exists. Otherwise, <see langword="null" />.
    /// </returns>
    new object? this[string name] { get; set; }

    /// <summary>
    ///     Adds an annotation to this object. Throws if an annotation with the specified name already exists.
    /// </summary>
    /// <param name="name">The name of the annotation to be added.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    /// <returns>The newly added annotation.</returns>
    IAnnotation AddAnnotation(string name, object? value);

    /// <summary>
    ///     Sets the annotation stored under the given key. Overwrites the existing annotation if an
    ///     annotation with the specified name already exists.
    /// </summary>
    /// <param name="name">The name of the annotation to be added.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    void SetAnnotation(string name, object? value);

    /// <summary>
    ///     Removes the given annotation from this object.
    /// </summary>
    /// <param name="name">The name of the annotation to remove.</param>
    /// <returns>The annotation that was removed.</returns>
    IAnnotation? RemoveAnnotation(string name);

    /// <summary>
    ///     Adds annotations to an object.
    /// </summary>
    /// <param name="annotations">The annotations to be added.</param>
    void AddAnnotations(IEnumerable<IAnnotation> annotations)
        => AnnotatableBase.AddAnnotations((AnnotatableBase)this, annotations);

    /// <summary>
    ///     Sets the annotation stored under the given name. Overwrites the existing annotation if an
    ///     annotation with the specified name already exists. Removes the existing annotation if <see langword="null" /> is supplied.
    /// </summary>
    /// <param name="name">The name of the annotation to be added.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    void SetOrRemoveAnnotation(string name, object? value);
}
