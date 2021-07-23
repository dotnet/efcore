// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     Represents an operation that should be performed when an annotation is changed on a foreign key.
    /// </summary>
    public interface IForeignKeyAnnotationChangedConvention : IConvention
    {
        /// <summary>
        ///     Called after an annotation is changed on a foreign key.
        /// </summary>
        /// <param name="relationshipBuilder"> The builder for the foreign key. </param>
        /// <param name="name"> The annotation name. </param>
        /// <param name="annotation"> The new annotation. </param>
        /// <param name="oldAnnotation"> The old annotation.  </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        void ProcessForeignKeyAnnotationChanged(
            IConventionForeignKeyBuilder relationshipBuilder,
            string name,
            IConventionAnnotation? annotation,
            IConventionAnnotation? oldAnnotation,
            IConventionContext<IConventionAnnotation> context);
    }
}
