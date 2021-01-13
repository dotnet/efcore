// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     Represents an operation that should be performed when a skip navigation inverse is changed.
    /// </summary>
    public interface ISkipNavigationInverseChangedConvention : IConvention
    {
        /// <summary>
        ///     Called after a skip navigation inverse is changed.
        /// </summary>
        /// <param name="skipNavigationBuilder"> The builder for the skip navigation. </param>
        /// <param name="inverse"> The current inverse skip navigation. </param>
        /// <param name="oldInverse"> The old inverse skip navigation. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        void ProcessSkipNavigationInverseChanged(
            [NotNull] IConventionSkipNavigationBuilder skipNavigationBuilder,
            [CanBeNull] IConventionSkipNavigation inverse,
            [CanBeNull] IConventionSkipNavigation oldInverse,
            [NotNull] IConventionContext<IConventionSkipNavigation> context);
    }
}
