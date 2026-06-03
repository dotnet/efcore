// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     Represents an operation that should be performed when an annotation is changed on a model.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public interface IModelAnnotationChangedConvention : IConvention
{
    /// <summary>
    ///     Called after an annotation is changed on an model.
    /// </summary>
    /// <param name="modelBuilder">The builder for the model.</param>
    /// <param name="name">The annotation name.</param>
    /// <param name="annotation">The new annotation.</param>
    /// <param name="oldAnnotation">The old annotation.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    void ProcessModelAnnotationChanged(
        IConventionModelBuilder modelBuilder,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation,
        IConventionContext<IConventionAnnotation> context);
}
