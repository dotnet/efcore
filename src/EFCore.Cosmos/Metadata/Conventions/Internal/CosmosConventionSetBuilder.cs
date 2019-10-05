// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Cosmos.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class CosmosConventionSetBuilder : ProviderConventionSetBuilder
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public CosmosConventionSetBuilder(
            [NotNull] ProviderConventionSetBuilderDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override ConventionSet CreateConventionSet()
        {
            var conventionSet = base.CreateConventionSet();

            conventionSet.ModelInitializedConventions.Add(new ContextContainerConvention(Dependencies));

            var discriminatorConvention = new CosmosDiscriminatorConvention(Dependencies);
            var storeKeyConvention = new StoreKeyConvention(Dependencies);
            conventionSet.EntityTypeAddedConventions.Add(storeKeyConvention);
            conventionSet.EntityTypeAddedConventions.Add(discriminatorConvention);

            ReplaceConvention(conventionSet.EntityTypeRemovedConventions, (DiscriminatorConvention)discriminatorConvention);

            conventionSet.EntityTypeBaseTypeChangedConventions.Add(storeKeyConvention);
            ReplaceConvention(conventionSet.EntityTypeBaseTypeChangedConventions, (DiscriminatorConvention)discriminatorConvention);

            conventionSet.ForeignKeyRemovedConventions.Add(discriminatorConvention);
            conventionSet.ForeignKeyRemovedConventions.Add(storeKeyConvention);

            conventionSet.ForeignKeyOwnershipChangedConventions.Add(discriminatorConvention);
            conventionSet.ForeignKeyOwnershipChangedConventions.Add(storeKeyConvention);

            conventionSet.EntityTypeAnnotationChangedConventions.Add(storeKeyConvention);

            return conventionSet;
        }
    }
}
