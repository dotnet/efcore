// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         A service on the EF internal service provider that creates the <see cref="ConventionSet" />
    ///         for the current database provider. This is combined with <see cref="IConventionSetCustomizer" />
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
        public ProviderConventionSetBuilder([NotNull] ProviderConventionSetBuilderDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

        /// <summary>
        ///     Builds and returns the convention set for the current database provider.
        /// </summary>
        /// <returns> The convention set for the current database provider. </returns>
        public virtual ConventionSet CreateConventionSet()
        {
            var conventionSet = new ConventionSet();
            var logger = Dependencies.Logger;

            var propertyDiscoveryConvention
                = new PropertyDiscoveryConvention(
                    Dependencies.TypeMappingSource, logger);

            var keyDiscoveryConvention
                = new KeyDiscoveryConvention(logger);

            var inversePropertyAttributeConvention
                = new InversePropertyAttributeConvention(Dependencies.MemberClassifier, logger);

            var relationshipDiscoveryConvention
                = new RelationshipDiscoveryConvention(Dependencies.MemberClassifier, logger);

            var servicePropertyDiscoveryConvention
                = new ServicePropertyDiscoveryConvention(Dependencies.TypeMappingSource, Dependencies.ParameterBindingFactories, logger);

            conventionSet.EntityTypeAddedConventions.Add(new NotMappedEntityTypeAttributeConvention(logger));
            conventionSet.EntityTypeAddedConventions.Add(new OwnedEntityTypeAttributeConvention(logger));
            conventionSet.EntityTypeAddedConventions.Add(new NotMappedMemberAttributeConvention(logger));
            conventionSet.EntityTypeAddedConventions.Add(new BaseTypeDiscoveryConvention(logger));
            conventionSet.EntityTypeAddedConventions.Add(propertyDiscoveryConvention);
            conventionSet.EntityTypeAddedConventions.Add(servicePropertyDiscoveryConvention);
            conventionSet.EntityTypeAddedConventions.Add(keyDiscoveryConvention);
            conventionSet.EntityTypeAddedConventions.Add(inversePropertyAttributeConvention);
            conventionSet.EntityTypeAddedConventions.Add(relationshipDiscoveryConvention);
            conventionSet.EntityTypeAddedConventions.Add(new DerivedTypeDiscoveryConvention(logger));

            conventionSet.EntityTypeIgnoredConventions.Add(inversePropertyAttributeConvention);

            var discriminatorConvention = new DiscriminatorConvention(logger);
            conventionSet.EntityTypeRemovedConventions.Add(new OwnedTypesConvention(logger));
            conventionSet.EntityTypeRemovedConventions.Add(discriminatorConvention);

            var foreignKeyIndexConvention = new ForeignKeyIndexConvention(logger);
            var valueGeneratorConvention = new ValueGeneratorConvention(logger);

            conventionSet.EntityTypeBaseTypeChangedConventions.Add(propertyDiscoveryConvention);
            conventionSet.EntityTypeBaseTypeChangedConventions.Add(servicePropertyDiscoveryConvention);
            conventionSet.EntityTypeBaseTypeChangedConventions.Add(keyDiscoveryConvention);
            conventionSet.EntityTypeBaseTypeChangedConventions.Add(inversePropertyAttributeConvention);
            conventionSet.EntityTypeBaseTypeChangedConventions.Add(relationshipDiscoveryConvention);
            conventionSet.EntityTypeBaseTypeChangedConventions.Add(foreignKeyIndexConvention);
            conventionSet.EntityTypeBaseTypeChangedConventions.Add(valueGeneratorConvention);
            conventionSet.EntityTypeBaseTypeChangedConventions.Add(discriminatorConvention);

            var foreignKeyPropertyDiscoveryConvention = new ForeignKeyPropertyDiscoveryConvention(logger);

            conventionSet.EntityTypeMemberIgnoredConventions.Add(inversePropertyAttributeConvention);
            conventionSet.EntityTypeMemberIgnoredConventions.Add(relationshipDiscoveryConvention);
            conventionSet.EntityTypeMemberIgnoredConventions.Add(foreignKeyPropertyDiscoveryConvention);
            conventionSet.EntityTypeMemberIgnoredConventions.Add(servicePropertyDiscoveryConvention);

            var keyAttributeConvention = new KeyAttributeConvention(logger);
            var backingFieldConvention = new BackingFieldConvention(logger);
            var concurrencyCheckAttributeConvention = new ConcurrencyCheckAttributeConvention(logger);
            var databaseGeneratedAttributeConvention = new DatabaseGeneratedAttributeConvention(logger);
            var requiredPropertyAttributeConvention = new RequiredPropertyAttributeConvention(logger);
            var nonNullableReferencePropertyConvention = new NonNullableReferencePropertyConvention(logger);
            var maxLengthAttributeConvention = new MaxLengthAttributeConvention(logger);
            var stringLengthAttributeConvention = new StringLengthAttributeConvention(logger);
            var timestampAttributeConvention = new TimestampAttributeConvention(logger);

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

            conventionSet.EntityTypePrimaryKeyChangedConventions.Add(foreignKeyPropertyDiscoveryConvention);
            conventionSet.EntityTypePrimaryKeyChangedConventions.Add(valueGeneratorConvention);

            conventionSet.KeyAddedConventions.Add(foreignKeyPropertyDiscoveryConvention);
            conventionSet.KeyAddedConventions.Add(foreignKeyIndexConvention);

            conventionSet.KeyRemovedConventions.Add(foreignKeyPropertyDiscoveryConvention);
            conventionSet.KeyRemovedConventions.Add(foreignKeyIndexConvention);
            conventionSet.KeyRemovedConventions.Add(keyDiscoveryConvention);

            var cascadeDeleteConvention = new CascadeDeleteConvention(logger);
            var foreignKeyAttributeConvention = new ForeignKeyAttributeConvention(Dependencies.MemberClassifier, logger);

            conventionSet.ForeignKeyAddedConventions.Add(foreignKeyAttributeConvention);
            conventionSet.ForeignKeyAddedConventions.Add(foreignKeyPropertyDiscoveryConvention);
            conventionSet.ForeignKeyAddedConventions.Add(keyDiscoveryConvention);
            conventionSet.ForeignKeyAddedConventions.Add(valueGeneratorConvention);
            conventionSet.ForeignKeyAddedConventions.Add(cascadeDeleteConvention);
            conventionSet.ForeignKeyAddedConventions.Add(foreignKeyIndexConvention);

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

            conventionSet.ForeignKeyOwnershipChangedConventions.Add(new NavigationEagerLoadingConvention(logger));
            conventionSet.ForeignKeyOwnershipChangedConventions.Add(keyDiscoveryConvention);
            conventionSet.ForeignKeyOwnershipChangedConventions.Add(relationshipDiscoveryConvention);

            conventionSet.ModelFinalizedConventions.Add(new ModelCleanupConvention(logger));
            conventionSet.ModelFinalizedConventions.Add(keyAttributeConvention);
            conventionSet.ModelFinalizedConventions.Add(foreignKeyAttributeConvention);
            conventionSet.ModelFinalizedConventions.Add(new ChangeTrackingStrategyConvention(logger));
            conventionSet.ModelFinalizedConventions.Add(new ConstructorBindingConvention(Dependencies.ConstructorBindingFactory, logger));
            conventionSet.ModelFinalizedConventions.Add(new TypeMappingConvention(Dependencies.TypeMappingSource, logger));
            conventionSet.ModelFinalizedConventions.Add(foreignKeyIndexConvention);

            conventionSet.ModelFinalizedConventions.Add(foreignKeyPropertyDiscoveryConvention);
            conventionSet.ModelFinalizedConventions.Add(servicePropertyDiscoveryConvention);

            conventionSet.NavigationAddedConventions.Add(backingFieldConvention);
            conventionSet.NavigationAddedConventions.Add(new RequiredNavigationAttributeConvention(logger));
            conventionSet.NavigationAddedConventions.Add(new NonNullableNavigationConvention(logger));
            conventionSet.NavigationAddedConventions.Add(inversePropertyAttributeConvention);
            conventionSet.NavigationAddedConventions.Add(foreignKeyPropertyDiscoveryConvention);
            conventionSet.NavigationAddedConventions.Add(relationshipDiscoveryConvention);

            conventionSet.NavigationRemovedConventions.Add(relationshipDiscoveryConvention);

            conventionSet.IndexAddedConventions.Add(foreignKeyIndexConvention);

            conventionSet.IndexRemovedConventions.Add(foreignKeyIndexConvention);

            conventionSet.IndexUniquenessChangedConventions.Add(foreignKeyIndexConvention);

            conventionSet.ForeignKeyPrincipalEndChangedConventions.Add(foreignKeyPropertyDiscoveryConvention);

            conventionSet.PropertyNullabilityChangedConventions.Add(foreignKeyPropertyDiscoveryConvention);

            conventionSet.PropertyFieldChangedConventions.Add(keyDiscoveryConvention);
            conventionSet.PropertyFieldChangedConventions.Add(foreignKeyPropertyDiscoveryConvention);
            conventionSet.PropertyFieldChangedConventions.Add(keyAttributeConvention);
            conventionSet.PropertyFieldChangedConventions.Add(concurrencyCheckAttributeConvention);
            conventionSet.PropertyFieldChangedConventions.Add(databaseGeneratedAttributeConvention);
            conventionSet.PropertyFieldChangedConventions.Add(requiredPropertyAttributeConvention);
            conventionSet.PropertyFieldChangedConventions.Add(nonNullableReferencePropertyConvention);
            conventionSet.PropertyFieldChangedConventions.Add(maxLengthAttributeConvention);
            conventionSet.PropertyFieldChangedConventions.Add(stringLengthAttributeConvention);
            conventionSet.PropertyFieldChangedConventions.Add(timestampAttributeConvention);

            return conventionSet;
        }

        /// <summary>
        ///     Helper method used to replace an existing convention implementation
        ///     with a new implementation.
        /// </summary>
        /// <typeparam name="TConvention"> The type defining convention being replaced. </typeparam>
        /// <typeparam name="TImplementation"> The type of the new implementation. </typeparam>
        /// <param name="conventionsList"> The list of existing convention instances to scan. </param>
        /// <param name="newConvention"> The new convention. </param>
        protected virtual void ReplaceConvention<TConvention, TImplementation>(
            [NotNull] IList<TConvention> conventionsList,
            [NotNull] TImplementation newConvention)
            where TImplementation : TConvention
        {
            Check.NotNull(conventionsList, nameof(conventionsList));
            Check.NotNull(newConvention, nameof(newConvention));

            var oldConvention = conventionsList.OfType<TImplementation>().FirstOrDefault();
            if (oldConvention == null)
            {
                return; // Nothing to replace.
            }

            var index = conventionsList.IndexOf(oldConvention);

            conventionsList.RemoveAt(index);
            conventionsList.Insert(index, newConvention);
        }
    }
}
