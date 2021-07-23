// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
            IConventionSkipNavigationBuilder skipNavigationBuilder,
            IConventionSkipNavigation? inverse,
            IConventionSkipNavigation? oldInverse,
            IConventionContext<IConventionSkipNavigation> context);
    }
}
