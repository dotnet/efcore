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
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public interface IConventionAnnotatable : IReadOnlyAnnotatable
{
    /// <summary>
    ///     Gets the builder that can be used to configure this object.
    /// </summary>
    /// <exception cref="InvalidOperationException">If the object has been removed from the model.</exception>
    IConventionAnnotatableBuilder Builder { get; }

    /// <summary>
    ///     Indicates whether this object is in a model, i.e. hasn't been removed from one.
    /// </summary>
    bool IsInModel { get; }

    /// <summary>
    ///     Gets all annotations on the current object.
    /// </summary>
    new IEnumerable<IConventionAnnotation> GetAnnotations();

    /// <summary>
    ///     Adds an annotation to this object. Throws if an annotation with the specified name already exists.
    /// </summary>
    /// <param name="name">The name of the annotation to be added.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The newly added annotation.</returns>
    IConventionAnnotation AddAnnotation(string name, object? value, bool fromDataAnnotation = false);

    /// <summary>
    ///     Sets the annotation stored under the given name. Overwrites the existing annotation if an
    ///     annotation with the specified name already exists.
    /// </summary>
    /// <param name="name">The name of the annotation to be set.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The new annotation.</returns>
    IConventionAnnotation? SetAnnotation(string name, object? value, bool fromDataAnnotation = false);

    /// <summary>
    ///     Gets the annotation with the given name, returning <see langword="null" /> if it does not exist.
    /// </summary>
    /// <param name="name">The name of the annotation to find.</param>
    /// <returns>
    ///     The existing annotation if an annotation with the specified name already exists. Otherwise, <see langword="null" />.
    /// </returns>
    new IConventionAnnotation? FindAnnotation(string name);

    /// <summary>
    ///     Removes the annotation with the given name from this object.
    /// </summary>
    /// <param name="name">The name of the annotation to remove.</param>
    /// <returns>The annotation that was removed.</returns>
    IConventionAnnotation? RemoveAnnotation(string name);

    /// <summary>
    ///     Gets the annotation with the given name, throwing if it does not exist.
    /// </summary>
    /// <param name="annotationName">The key of the annotation to find.</param>
    /// <returns>The annotation with the specified name.</returns>
    new IConventionAnnotation GetAnnotation(string annotationName)
        => (IConventionAnnotation)((IReadOnlyAnnotatable)this).GetAnnotation(annotationName);

    /// <summary>
    ///     Adds annotations to an object.
    /// </summary>
    /// <param name="annotations">The annotations to be added.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    void AddAnnotations(
        IEnumerable<IConventionAnnotation> annotations,
        bool fromDataAnnotation = false)
    {
        foreach (var annotation in annotations)
        {
            AddAnnotation(annotation.Name, annotation.Value, fromDataAnnotation);
        }
    }

    /// <summary>
    ///     Sets the annotation stored under the given name. Overwrites the existing annotation if an
    ///     annotation with the specified name already exists. Removes the existing annotation if <see langword="null" /> is supplied.
    /// </summary>
    /// <param name="name">The name of the annotation to be added.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The new annotation or <see langword="null" /> if it was removed.</returns>
    IConventionAnnotation? SetOrRemoveAnnotation(
        string name,
        object? value,
        bool fromDataAnnotation = false);
}
