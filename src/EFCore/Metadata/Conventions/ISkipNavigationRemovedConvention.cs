// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     Represents an operation that should be performed when a skip navigation is removed from the entity type.
    /// </summary>
    public interface ISkipNavigationRemovedConvention : IConvention
    {
        /// <summary>
        ///     Called after a skip navigation is removed from the entity type.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type that contained the navigation. </param>
        /// <param name="navigation"> The removed navigation. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        void ProcessSkipNavigationRemoved(
            [NotNull] IConventionEntityTypeBuilder entityTypeBuilder,
            [NotNull] IConventionSkipNavigation navigation,
            [NotNull] IConventionContext<IConventionSkipNavigation> context);
    }
}
