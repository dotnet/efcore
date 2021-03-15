// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that configures a navigation property as having a backing field
    ///     based on the <see cref="BackingFieldAttribute" /> attribute.
    /// </summary>
    public class NavigationBackingFieldAttributeConvention : NavigationAttributeConventionBase<BackingFieldAttribute>
    {
        /// <summary>
        ///     Creates a new instance of <see cref="NavigationBackingFieldAttributeConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public NavigationBackingFieldAttributeConvention([NotNull] ProviderConventionSetBuilderDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <summary>
        ///     Called after a navigation property that has an attribute is added to an entity type.
        /// </summary>
        /// <param name="navigationBuilder"> The builder for the navigation. </param>
        /// <param name="attribute"> The attribute. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public override void ProcessNavigationAdded(
            IConventionNavigationBuilder navigationBuilder,
            BackingFieldAttribute attribute,
            IConventionContext<IConventionNavigationBuilder> context)
        {
            navigationBuilder.HasField(attribute.Name, fromDataAnnotation: true);
        }

        /// <inheritdoc />
        public override void ProcessSkipNavigationAdded(
            IConventionSkipNavigationBuilder skipNavigationBuilder,
            BackingFieldAttribute attribute,
            IConventionContext<IConventionSkipNavigationBuilder> context)
        {
            skipNavigationBuilder.HasField(attribute.Name, fromDataAnnotation: true);
        }
    }
}
