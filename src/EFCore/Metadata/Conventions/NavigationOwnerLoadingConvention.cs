// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that configures how to load navigations to owned entity types.
    /// </summary>
    public class NavigationOwnerLoadingConvention : INavigationAddedConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="NavigationOwnerLoadingConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public NavigationOwnerLoadingConvention([NotNull] ProviderConventionSetBuilderDependencies dependencies)
        {
            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

        public virtual void ProcessNavigationAdded(IConventionRelationshipBuilder relationshipBuilder,
            IConventionNavigation navigation, IConventionContext<IConventionNavigation> context)
        {
            if (navigation.ForeignKey.IsOwnership)
            {
                navigation.SetOrRemoveAnnotation(CoreAnnotationNames.LoadedByOwner, true);
            }
        }
    }
}
