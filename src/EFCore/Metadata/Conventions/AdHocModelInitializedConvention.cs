// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     Puts an attribute on the model indicating it is being used to build ad-hoc types.
/// </summary>
public class AdHocModelInitializedConvention : IModelInitializedConvention
{
    /// <summary>
    ///     Puts an attribute on the model indicating it is being used to build ad-hoc types.
    /// </summary>
    /// <param name="modelBuilder">The builder for the model.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessModelInitialized(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
        => modelBuilder.HasAnnotation(CoreAnnotationNames.AdHocModel, true);
}
