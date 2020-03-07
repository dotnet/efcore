// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
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
        /// <param name="relationshipBuilder"> The builder for the foreign key. </param>
        /// <param name="navigation"> The navigation. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        void ProcessNavigationAdded(
            [NotNull] IConventionRelationshipBuilder relationshipBuilder,
            [NotNull] IConventionNavigation navigation,
            [NotNull] IConventionContext<IConventionNavigation> context);
    }
}
