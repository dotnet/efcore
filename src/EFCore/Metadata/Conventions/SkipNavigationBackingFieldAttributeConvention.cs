// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that configures a skip navigation property as having a backing field
    ///     based on the <see cref="BackingFieldAttribute"/> attribute.
    /// </summary>
    public class SkipNavigationBackingFieldAttributeConvention : NavigationAttributeConventionBase<BackingFieldAttribute>
    {
        /// <summary>
        ///     Creates a new instance of <see cref="SkipNavigationBackingFieldAttributeConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public SkipNavigationBackingFieldAttributeConvention([NotNull] ProviderConventionSetBuilderDependencies dependencies)
            : base(dependencies)
        {
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
