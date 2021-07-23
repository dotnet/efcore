// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     Represents an operation that should be performed when a navigation is added to the entity type.
    /// </summary>
    public interface INavigationAddedConvention : IConvention
    {
        /// <summary>
        ///     Called after a navigation is added to the entity type.
        /// </summary>
        /// <param name="navigationBuilder"> The builder for the navigation. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        void ProcessNavigationAdded(
            IConventionNavigationBuilder navigationBuilder,
            IConventionContext<IConventionNavigationBuilder> context);
    }
}
