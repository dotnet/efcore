// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     Represents an operation that should be performed when an annotation is changed on a navigation.
    /// </summary>
    public interface INavigationAnnotationChangedConvention : IConvention
    {
        /// <summary>
        ///     Called after an annotation is changed on a navigation.
        /// </summary>
        /// <param name="relationshipBuilder"> The builder for the foreign key. </param>
        /// <param name="navigation"> The navigation. </param>
        /// <param name="name"> The annotation name. </param>
        /// <param name="annotation"> The new annotation. </param>
        /// <param name="oldAnnotation"> The old annotation.  </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        void ProcessNavigationAnnotationChanged(
            [NotNull] IConventionForeignKeyBuilder relationshipBuilder,
            [NotNull] IConventionNavigation navigation,
            [NotNull] string name,
            [CanBeNull] IConventionAnnotation annotation,
            [CanBeNull] IConventionAnnotation oldAnnotation,
            [NotNull] IConventionContext<IConventionAnnotation> context);
    }
}
