// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

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

            conventionSet.ModelFinalizingConventions.Add(new ETagPropertyConvention());

            var storeKeyConvention = new StoreKeyConvention(Dependencies);
            var discriminatorConvention = new CosmosDiscriminatorConvention(Dependencies);
            var keyDiscoveryConvention = new CosmosKeyDiscoveryConvention(Dependencies);
            conventionSet.EntityTypeAddedConventions.Add(storeKeyConvention);
            conventionSet.EntityTypeAddedConventions.Add(discriminatorConvention);
            ReplaceConvention(conventionSet.EntityTypeAddedConventions, (KeyDiscoveryConvention)keyDiscoveryConvention);

            ReplaceConvention(conventionSet.EntityTypeRemovedConventions, (DiscriminatorConvention)discriminatorConvention);

            conventionSet.EntityTypeBaseTypeChangedConventions.Add(storeKeyConvention);
            ReplaceConvention(conventionSet.EntityTypeBaseTypeChangedConventions, (DiscriminatorConvention)discriminatorConvention);
            ReplaceConvention(conventionSet.EntityTypeBaseTypeChangedConventions, (KeyDiscoveryConvention)keyDiscoveryConvention);

            conventionSet.EntityTypePrimaryKeyChangedConventions.Add(storeKeyConvention);

            conventionSet.KeyAddedConventions.Add(storeKeyConvention);

            conventionSet.KeyRemovedConventions.Add(storeKeyConvention);
            ReplaceConvention(conventionSet.KeyRemovedConventions, (KeyDiscoveryConvention)keyDiscoveryConvention);

            ReplaceConvention(conventionSet.ForeignKeyAddedConventions, (KeyDiscoveryConvention)keyDiscoveryConvention);

            conventionSet.ForeignKeyRemovedConventions.Add(discriminatorConvention);
            conventionSet.ForeignKeyRemovedConventions.Add(storeKeyConvention);
            ReplaceConvention(conventionSet.ForeignKeyRemovedConventions, (KeyDiscoveryConvention)keyDiscoveryConvention);

            ReplaceConvention(conventionSet.ForeignKeyPropertiesChangedConventions, (KeyDiscoveryConvention)keyDiscoveryConvention);

            ReplaceConvention(conventionSet.ForeignKeyUniquenessChangedConventions, (KeyDiscoveryConvention)keyDiscoveryConvention);

            conventionSet.ForeignKeyOwnershipChangedConventions.Add(discriminatorConvention);
            conventionSet.ForeignKeyOwnershipChangedConventions.Add(storeKeyConvention);
            ReplaceConvention(conventionSet.ForeignKeyOwnershipChangedConventions, (KeyDiscoveryConvention)keyDiscoveryConvention);

            conventionSet.EntityTypeAnnotationChangedConventions.Add(storeKeyConvention);
            conventionSet.EntityTypeAnnotationChangedConventions.Add(keyDiscoveryConvention);

            ReplaceConvention(conventionSet.PropertyAddedConventions, (KeyDiscoveryConvention)keyDiscoveryConvention);

            conventionSet.PropertyAnnotationChangedConventions.Add(storeKeyConvention);

            return conventionSet;
        }

        /// <summary>
        ///     <para>
        ///         Call this method to build a <see cref="ModelBuilder" /> for Cosmos outside of <see cref="DbContext.OnModelCreating" />.
        ///     </para>
        ///     <para>
        ///         Note that it is unusual to use this method.
        ///         Consider using <see cref="DbContext" /> in the normal way instead.
        ///     </para>
        /// </summary>
        /// <returns> The convention set. </returns>
        public static ModelBuilder CreateModelBuilder()
        {
            using var serviceScope = CreateServiceScope();
            using var context = serviceScope.ServiceProvider.GetService<DbContext>();
            return new ModelBuilder(ConventionSet.CreateConventionSet(context), context.GetService<ModelDependencies>());
        }

        private static IServiceScope CreateServiceScope()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkCosmos()
                .AddDbContext<DbContext>(
                    (p, o) =>
                        o.UseCosmos("localhost", "_", "_")
                            .UseInternalServiceProvider(p))
                .BuildServiceProvider();

            return serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        }
    }
}
