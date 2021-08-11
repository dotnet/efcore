// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         A service on the EF internal service provider that creates the <see cref="ConventionSet" />
    ///         for the current database provider. This is combined with <see cref="IConventionSetPlugin" />
    ///         instances to produce the full convention set exposed by the <see cref="IConventionSetBuilder" />
    ///         service.
    ///     </para>
    ///     <para>
    ///         Database providers should implement this service by inheriting from either
    ///         this class (for non-relational providers) or `RelationalConventionSetBuilder` (for relational providers).
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public class ProviderConventionSetBuilder : IProviderConventionSetBuilder
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ProviderConventionSetBuilder" /> class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        public ProviderConventionSetBuilder(ProviderConventionSetBuilderDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
        }

        /// <summary>
        ///     Dependencies for this service.
        /// </summary>
        protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

        /// <summary>
        ///     Builds and returns the convention set for the current database provider.
        /// </summary>
        /// <returns> The convention set for the current database provider. </returns>
        public virtual ConventionSet CreateConventionSet()
        {
            var conventionSet = new ConventionSet();

            var propertyDiscoveryConvention = new PropertyDiscoveryConvention(Dependencies);
            var keyDiscoveryConvention = new KeyDiscoveryConvention(Dependencies);
            var inversePropertyAttributeConvention = new InversePropertyAttributeConvention(Dependencies);
            var relationshipDiscoveryConvention = new RelationshipDiscoveryConvention(Dependencies);
            var servicePropertyDiscoveryConvention = new ServicePropertyDiscoveryConvention(Dependencies);
            var indexAttributeConvention = new IndexAttributeConvention(Dependencies);
            var baseTypeDiscoveryConvention = new BaseTypeDiscoveryConvention(Dependencies);
            conventionSet.EntityTypeAddedConventions.Add(new NotMappedEntityTypeAttributeConvention(Dependencies));
            conventionSet.EntityTypeAddedConventions.Add(new OwnedEntityTypeAttributeConvention(Dependencies));
            conventionSet.EntityTypeAddedConventions.Add(new KeylessEntityTypeAttributeConvention(Dependencies));
            conventionSet.EntityTypeAddedConventions.Add(new EntityTypeConfigurationEntityTypeAttributeConvention(Dependencies));
            conventionSet.EntityTypeAddedConventions.Add(new NotMappedMemberAttributeConvention(Dependencies));
            conventionSet.EntityTypeAddedConventions.Add(baseTypeDiscoveryConvention);
            conventionSet.EntityTypeAddedConventions.Add(propertyDiscoveryConvention);
            conventionSet.EntityTypeAddedConventions.Add(servicePropertyDiscoveryConvention);
            conventionSet.EntityTypeAddedConventions.Add(keyDiscoveryConvention);
            conventionSet.EntityTypeAddedConventions.Add(indexAttributeConvention);
            conventionSet.EntityTypeAddedConventions.Add(inversePropertyAttributeConvention);
            conventionSet.EntityTypeAddedConventions.Add(relationshipDiscoveryConvention);

            conventionSet.EntityTypeIgnoredConventions.Add(relationshipDiscoveryConvention);

            var discriminatorConvention = new DiscriminatorConvention(Dependencies);
            conventionSet.EntityTypeRemovedConventions.Add(inversePropertyAttributeConvention);
            conventionSet.EntityTypeRemovedConventions.Add(discriminatorConvention);

            var foreignKeyIndexConvention = new ForeignKeyIndexConvention(Dependencies);
            var valueGeneratorConvention = new ValueGenerationConvention(Dependencies);

            conventionSet.EntityTypeBaseTypeChangedConventions.Add(propertyDiscoveryConvention);
            conventionSet.EntityTypeBaseTypeChangedConventions.Add(servicePropertyDiscoveryConvention);
            conventionSet.EntityTypeBaseTypeChangedConventions.Add(keyDiscoveryConvention);
            conventionSet.EntityTypeBaseTypeChangedConventions.Add(indexAttributeConvention);
            conventionSet.EntityTypeBaseTypeChangedConventions.Add(inversePropertyAttributeConvention);
            conventionSet.EntityTypeBaseTypeChangedConventions.Add(relationshipDiscoveryConvention);
            conventionSet.EntityTypeBaseTypeChangedConventions.Add(foreignKeyIndexConvention);
            conventionSet.EntityTypeBaseTypeChangedConventions.Add(valueGeneratorConvention);
            conventionSet.EntityTypeBaseTypeChangedConventions.Add(discriminatorConvention);

            var foreignKeyPropertyDiscoveryConvention = new ForeignKeyPropertyDiscoveryConvention(Dependencies);

            conventionSet.EntityTypeMemberIgnoredConventions.Add(inversePropertyAttributeConvention);
            conventionSet.EntityTypeMemberIgnoredConventions.Add(relationshipDiscoveryConvention);
            conventionSet.EntityTypeMemberIgnoredConventions.Add(keyDiscoveryConvention);
            conventionSet.EntityTypeMemberIgnoredConventions.Add(foreignKeyPropertyDiscoveryConvention);

            var keyAttributeConvention = new KeyAttributeConvention(Dependencies);
            var backingFieldConvention = new BackingFieldConvention(Dependencies);
            var concurrencyCheckAttributeConvention = new ConcurrencyCheckAttributeConvention(Dependencies);
            var databaseGeneratedAttributeConvention = new DatabaseGeneratedAttributeConvention(Dependencies);
            var requiredPropertyAttributeConvention = new RequiredPropertyAttributeConvention(Dependencies);
            var nonNullableReferencePropertyConvention = new NonNullableReferencePropertyConvention(Dependencies);
            var maxLengthAttributeConvention = new MaxLengthAttributeConvention(Dependencies);
            var stringLengthAttributeConvention = new StringLengthAttributeConvention(Dependencies);
            var timestampAttributeConvention = new TimestampAttributeConvention(Dependencies);
            var backingFieldAttributeConvention = new BackingFieldAttributeConvention(Dependencies);
            var unicodeAttributeConvention = new UnicodeAttributeConvention(Dependencies);
            var precisionAttributeConvention = new PrecisionAttributeConvention(Dependencies);

            conventionSet.PropertyAddedConventions.Add(backingFieldAttributeConvention);
            conventionSet.PropertyAddedConventions.Add(backingFieldConvention);
            conventionSet.PropertyAddedConventions.Add(concurrencyCheckAttributeConvention);
            conventionSet.PropertyAddedConventions.Add(databaseGeneratedAttributeConvention);
            conventionSet.PropertyAddedConventions.Add(requiredPropertyAttributeConvention);
            conventionSet.PropertyAddedConventions.Add(nonNullableReferencePropertyConvention);
            conventionSet.PropertyAddedConventions.Add(maxLengthAttributeConvention);
            conventionSet.PropertyAddedConventions.Add(stringLengthAttributeConvention);
            conventionSet.PropertyAddedConventions.Add(timestampAttributeConvention);
            conventionSet.PropertyAddedConventions.Add(keyAttributeConvention);
            conventionSet.PropertyAddedConventions.Add(keyDiscoveryConvention);
            conventionSet.PropertyAddedConventions.Add(foreignKeyPropertyDiscoveryConvention);
            conventionSet.PropertyAddedConventions.Add(unicodeAttributeConvention);
            conventionSet.PropertyAddedConventions.Add(precisionAttributeConvention);

            conventionSet.EntityTypePrimaryKeyChangedConventions.Add(foreignKeyPropertyDiscoveryConvention);
            conventionSet.EntityTypePrimaryKeyChangedConventions.Add(valueGeneratorConvention);

            conventionSet.KeyAddedConventions.Add(foreignKeyPropertyDiscoveryConvention);
            conventionSet.KeyAddedConventions.Add(foreignKeyIndexConvention);

            conventionSet.KeyRemovedConventions.Add(foreignKeyPropertyDiscoveryConvention);
            conventionSet.KeyRemovedConventions.Add(foreignKeyIndexConvention);
            conventionSet.KeyRemovedConventions.Add(keyDiscoveryConvention);

            var cascadeDeleteConvention = new CascadeDeleteConvention(Dependencies);
            var foreignKeyAttributeConvention = new ForeignKeyAttributeConvention(Dependencies);

            conventionSet.ForeignKeyAddedConventions.Add(foreignKeyAttributeConvention);
            conventionSet.ForeignKeyAddedConventions.Add(foreignKeyPropertyDiscoveryConvention);
            conventionSet.ForeignKeyAddedConventions.Add(keyDiscoveryConvention);
            conventionSet.ForeignKeyAddedConventions.Add(valueGeneratorConvention);
            conventionSet.ForeignKeyAddedConventions.Add(cascadeDeleteConvention);
            conventionSet.ForeignKeyAddedConventions.Add(foreignKeyIndexConvention);

            conventionSet.ForeignKeyRemovedConventions.Add(baseTypeDiscoveryConvention);
            conventionSet.ForeignKeyRemovedConventions.Add(relationshipDiscoveryConvention);
            conventionSet.ForeignKeyRemovedConventions.Add(keyDiscoveryConvention);
            conventionSet.ForeignKeyRemovedConventions.Add(valueGeneratorConvention);
            conventionSet.ForeignKeyRemovedConventions.Add(foreignKeyIndexConvention);

            conventionSet.ForeignKeyPropertiesChangedConventions.Add(foreignKeyPropertyDiscoveryConvention);
            conventionSet.ForeignKeyPropertiesChangedConventions.Add(keyDiscoveryConvention);
            conventionSet.ForeignKeyPropertiesChangedConventions.Add(valueGeneratorConvention);
            conventionSet.ForeignKeyPropertiesChangedConventions.Add(foreignKeyIndexConvention);

            conventionSet.ForeignKeyUniquenessChangedConventions.Add(foreignKeyPropertyDiscoveryConvention);
            conventionSet.ForeignKeyUniquenessChangedConventions.Add(keyDiscoveryConvention);
            conventionSet.ForeignKeyUniquenessChangedConventions.Add(foreignKeyIndexConvention);

            conventionSet.ForeignKeyRequirednessChangedConventions.Add(cascadeDeleteConvention);
            conventionSet.ForeignKeyRequirednessChangedConventions.Add(foreignKeyPropertyDiscoveryConvention);

            conventionSet.ForeignKeyOwnershipChangedConventions.Add(new NavigationEagerLoadingConvention(Dependencies));
            conventionSet.ForeignKeyOwnershipChangedConventions.Add(relationshipDiscoveryConvention);
            conventionSet.ForeignKeyOwnershipChangedConventions.Add(keyDiscoveryConvention);
            conventionSet.ForeignKeyOwnershipChangedConventions.Add(valueGeneratorConvention);

            conventionSet.ForeignKeyNullNavigationSetConventions.Add(relationshipDiscoveryConvention);

            var requiredNavigationAttributeConvention = new RequiredNavigationAttributeConvention(Dependencies);
            var nonNullableNavigationConvention = new NonNullableNavigationConvention(Dependencies);
            conventionSet.NavigationAddedConventions.Add(new NavigationBackingFieldAttributeConvention(Dependencies));
            conventionSet.NavigationAddedConventions.Add(backingFieldConvention);
            conventionSet.NavigationAddedConventions.Add(requiredNavigationAttributeConvention);
            conventionSet.NavigationAddedConventions.Add(nonNullableNavigationConvention);
            conventionSet.NavigationAddedConventions.Add(inversePropertyAttributeConvention);
            conventionSet.NavigationAddedConventions.Add(foreignKeyPropertyDiscoveryConvention);
            conventionSet.NavigationAddedConventions.Add(relationshipDiscoveryConvention);
            conventionSet.NavigationAddedConventions.Add(foreignKeyAttributeConvention);

            var manyToManyJoinEntityTypeConvention = new ManyToManyJoinEntityTypeConvention(Dependencies);
            conventionSet.SkipNavigationAddedConventions.Add(new NavigationBackingFieldAttributeConvention(Dependencies));
            conventionSet.SkipNavigationAddedConventions.Add(backingFieldConvention);
            conventionSet.SkipNavigationAddedConventions.Add(manyToManyJoinEntityTypeConvention);

            conventionSet.SkipNavigationRemovedConventions.Add(manyToManyJoinEntityTypeConvention);

            conventionSet.SkipNavigationInverseChangedConventions.Add(manyToManyJoinEntityTypeConvention);
            conventionSet.SkipNavigationInverseChangedConventions.Add(foreignKeyPropertyDiscoveryConvention);

            conventionSet.SkipNavigationForeignKeyChangedConventions.Add(manyToManyJoinEntityTypeConvention);
            conventionSet.SkipNavigationForeignKeyChangedConventions.Add(foreignKeyAttributeConvention);
            conventionSet.SkipNavigationForeignKeyChangedConventions.Add(keyDiscoveryConvention);
            conventionSet.SkipNavigationForeignKeyChangedConventions.Add(foreignKeyPropertyDiscoveryConvention);

            conventionSet.NavigationRemovedConventions.Add(relationshipDiscoveryConvention);

            conventionSet.IndexAddedConventions.Add(foreignKeyIndexConvention);

            conventionSet.IndexRemovedConventions.Add(foreignKeyIndexConvention);

            conventionSet.IndexUniquenessChangedConventions.Add(foreignKeyIndexConvention);

            conventionSet.ForeignKeyPrincipalEndChangedConventions.Add(foreignKeyPropertyDiscoveryConvention);
            conventionSet.ForeignKeyPrincipalEndChangedConventions.Add(requiredNavigationAttributeConvention);
            conventionSet.ForeignKeyPrincipalEndChangedConventions.Add(nonNullableNavigationConvention);

            conventionSet.PropertyNullabilityChangedConventions.Add(foreignKeyPropertyDiscoveryConvention);

            conventionSet.PropertyFieldChangedConventions.Add(foreignKeyPropertyDiscoveryConvention);
            conventionSet.PropertyFieldChangedConventions.Add(keyAttributeConvention);
            conventionSet.PropertyFieldChangedConventions.Add(concurrencyCheckAttributeConvention);
            conventionSet.PropertyFieldChangedConventions.Add(databaseGeneratedAttributeConvention);
            conventionSet.PropertyFieldChangedConventions.Add(requiredPropertyAttributeConvention);
            conventionSet.PropertyFieldChangedConventions.Add(nonNullableReferencePropertyConvention);
            conventionSet.PropertyFieldChangedConventions.Add(maxLengthAttributeConvention);
            conventionSet.PropertyFieldChangedConventions.Add(stringLengthAttributeConvention);
            conventionSet.PropertyFieldChangedConventions.Add(timestampAttributeConvention);

            conventionSet.ModelInitializedConventions.Add(new DbSetFindingConvention(Dependencies));

            conventionSet.ModelFinalizingConventions.Add(new ModelCleanupConvention(Dependencies));
            conventionSet.ModelFinalizingConventions.Add(keyAttributeConvention);
            conventionSet.ModelFinalizingConventions.Add(indexAttributeConvention);
            conventionSet.ModelFinalizingConventions.Add(foreignKeyAttributeConvention);
            conventionSet.ModelFinalizingConventions.Add(new ChangeTrackingStrategyConvention(Dependencies));
            conventionSet.ModelFinalizingConventions.Add(new ConstructorBindingConvention(Dependencies));
            conventionSet.ModelFinalizingConventions.Add(foreignKeyIndexConvention);
            conventionSet.ModelFinalizingConventions.Add(foreignKeyPropertyDiscoveryConvention);
            conventionSet.ModelFinalizingConventions.Add(nonNullableReferencePropertyConvention);
            conventionSet.ModelFinalizingConventions.Add(nonNullableNavigationConvention);
            conventionSet.ModelFinalizingConventions.Add(new QueryFilterRewritingConvention(Dependencies));
            conventionSet.ModelFinalizingConventions.Add(inversePropertyAttributeConvention);
            conventionSet.ModelFinalizingConventions.Add(backingFieldConvention);

            conventionSet.ModelFinalizedConventions.Add(new RuntimeModelConvention(Dependencies));

            return conventionSet;
        }

        /// <summary>
        ///     Replaces an existing convention with a derived convention.
        /// </summary>
        /// <typeparam name="TConvention"> The type of convention being replaced. </typeparam>
        /// <typeparam name="TImplementation"> The type of the old convention. </typeparam>
        /// <param name="conventionsList"> The list of existing convention instances to scan. </param>
        /// <param name="newConvention"> The new convention. </param>
        protected virtual bool ReplaceConvention<TConvention, TImplementation>(
            IList<TConvention> conventionsList,
            TImplementation newConvention)
            where TImplementation : TConvention
            => ConventionSet.Replace(conventionsList, newConvention);
    }
}
