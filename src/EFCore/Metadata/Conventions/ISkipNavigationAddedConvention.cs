// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     Represents an operation that should be performed when a skip navigation is added to the entity type.
    /// </summary>
    public interface ISkipNavigationAddedConvention : IConvention
    {
        /// <summary>
        ///     Called after a skip navigation is added to the entity type.
        /// </summary>
        /// <param name="skipNavigationBuilder"> The builder for the skip navigation. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        void ProcessSkipNavigationAdded(
            [NotNull] IConventionSkipNavigationBuilder skipNavigationBuilder,
            [NotNull] IConventionContext<IConventionSkipNavigationBuilder> context);
    }
}
