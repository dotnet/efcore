// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that configures the navigations to owned entity types as eager loaded.
    /// </summary>
    public class NavigationEagerLoadingConvention : IForeignKeyOwnershipChangedConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="NavigationEagerLoadingConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public NavigationEagerLoadingConvention(ProviderConventionSetBuilderDependencies dependencies)
        {
            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

        /// <summary>
        ///     Called after the ownership value for a foreign key is changed.
        /// </summary>
        /// <param name="relationshipBuilder"> The builder for the foreign key. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessForeignKeyOwnershipChanged(
            IConventionForeignKeyBuilder relationshipBuilder,
            IConventionContext<bool?> context)
        {
            relationshipBuilder.Metadata.PrincipalToDependent?.Builder.AutoInclude(relationshipBuilder.Metadata.IsOwnership);
        }
    }
}
