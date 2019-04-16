// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    // Issue#11266 This type is being used by provider code. Do not break.
    public class CoreConventionSetBuilder : ICoreConventionSetBuilder
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CoreConventionSetBuilder" /> class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        public CoreConventionSetBuilder([NotNull] CoreConventionSetBuilderDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual CoreConventionSetBuilderDependencies Dependencies { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConventionSet CreateConventionSet()
        {
            var conventionSet = new ConventionSet();

            var propertyDiscoveryConvention
                = new PropertyDiscoveryConvention(
                    Dependencies.TypeMappingSource);

            var keyDiscoveryConvention
                = new KeyDiscoveryConvention(Dependencies.Logger);

            var inversePropertyAttributeConvention
                = new InversePropertyAttributeConvention(Dependencies.MemberClassifier, Dependencies.Logger);

            var relationshipDiscoveryConvention
                = new RelationshipDiscoveryConvention(Dependencies.MemberClassifier, Dependencies.Logger);

            var servicePropertyDiscoveryConvention
                = new ServicePropertyDiscoveryConvention(Dependencies.TypeMappingSource, Dependencies.ParameterBindingFactories);

            conventionSet.EntityTypeAddedConventions.Add(new NotMappedEntityTypeAttributeConvention());
            conventionSet.EntityTypeAddedConventions.Add(new OwnedEntityTypeAttributeConvention());
            conventionSet.EntityTypeAddedConventions.Add(new NotMappedMemberAttributeConvention());
            conventionSet.EntityTypeAddedConventions.Add(new BaseTypeDiscoveryConvention());
            conventionSet.EntityTypeAddedConventions.Add(propertyDiscoveryConvention);
            conventionSet.EntityTypeAddedConventions.Add(servicePropertyDiscoveryConvention);
            conventionSet.EntityTypeAddedConventions.Add(keyDiscoveryConvention);
            conventionSet.EntityTypeAddedConventions.Add(inversePropertyAttributeConvention);
            conventionSet.EntityTypeAddedConventions.Add(relationshipDiscoveryConvention);
            conventionSet.EntityTypeAddedConventions.Add(new DerivedTypeDiscoveryConvention());

            conventionSet.EntityTypeIgnoredConventions.Add(inversePropertyAttributeConvention);

            conventionSet.EntityTypeRemovedConventions.Add(new OwnedTypesConvention());

            var foreignKeyIndexConvention = new ForeignKeyIndexConvention(Dependencies.Logger);
            var valueGeneratorConvention = new ValueGeneratorConvention();

            conventionSet.BaseEntityTypeChangedConventions.Add(propertyDiscoveryConvention);
            conventionSet.BaseEntityTypeChangedConventions.Add(servicePropertyDiscoveryConvention);
            conventionSet.BaseEntityTypeChangedConventions.Add(keyDiscoveryConvention);
            conventionSet.BaseEntityTypeChangedConventions.Add(inversePropertyAttributeConvention);
            conventionSet.BaseEntityTypeChangedConventions.Add(relationshipDiscoveryConvention);
            conventionSet.BaseEntityTypeChangedConventions.Add(foreignKeyIndexConvention);
            conventionSet.BaseEntityTypeChangedConventions.Add(valueGeneratorConvention);

            var foreignKeyPropertyDiscoveryConvention = new ForeignKeyPropertyDiscoveryConvention(Dependencies.Logger);

            conventionSet.EntityTypeMemberIgnoredConventions.Add(inversePropertyAttributeConvention);
            conventionSet.EntityTypeMemberIgnoredConventions.Add(relationshipDiscoveryConvention);
            conventionSet.EntityTypeMemberIgnoredConventions.Add(foreignKeyPropertyDiscoveryConvention);
            conventionSet.EntityTypeMemberIgnoredConventions.Add(servicePropertyDiscoveryConvention);

            var keyAttributeConvention = new KeyAttributeConvention();
            var backingFieldConvention = new BackingFieldConvention();
            var concurrencyCheckAttributeConvention = new ConcurrencyCheckAttributeConvention();
            var databaseGeneratedAttributeConvention = new DatabaseGeneratedAttributeConvention();
            var requiredPropertyAttributeConvention = new RequiredPropertyAttributeConvention();
            var maxLengthAttributeConvention = new MaxLengthAttributeConvention();
            var stringLengthAttributeConvention = new StringLengthAttributeConvention();
            var timestampAttributeConvention = new TimestampAttributeConvention();

            conventionSet.PropertyAddedConventions.Add(backingFieldConvention);
            conventionSet.PropertyAddedConventions.Add(concurrencyCheckAttributeConvention);
            conventionSet.PropertyAddedConventions.Add(databaseGeneratedAttributeConvention);
            conventionSet.PropertyAddedConventions.Add(requiredPropertyAttributeConvention);
            conventionSet.PropertyAddedConventions.Add(maxLengthAttributeConvention);
            conventionSet.PropertyAddedConventions.Add(stringLengthAttributeConvention);
            conventionSet.PropertyAddedConventions.Add(timestampAttributeConvention);
            conventionSet.PropertyAddedConventions.Add(keyAttributeConvention);
            conventionSet.PropertyAddedConventions.Add(keyDiscoveryConvention);
            conventionSet.PropertyAddedConventions.Add(foreignKeyPropertyDiscoveryConvention);

            conventionSet.PrimaryKeyChangedConventions.Add(foreignKeyPropertyDiscoveryConvention);
            conventionSet.PrimaryKeyChangedConventions.Add(valueGeneratorConvention);

            conventionSet.KeyAddedConventions.Add(foreignKeyPropertyDiscoveryConvention);
            conventionSet.KeyAddedConventions.Add(foreignKeyIndexConvention);

            conventionSet.KeyRemovedConventions.Add(foreignKeyPropertyDiscoveryConvention);
            conventionSet.KeyRemovedConventions.Add(foreignKeyIndexConvention);
            conventionSet.KeyRemovedConventions.Add(keyDiscoveryConvention);

            var cascadeDeleteConvention = new CascadeDeleteConvention();
            var foreignKeyAttributeConvention = new ForeignKeyAttributeConvention(Dependencies.MemberClassifier, Dependencies.Logger);

            conventionSet.ForeignKeyAddedConventions.Add(foreignKeyAttributeConvention);
            conventionSet.ForeignKeyAddedConventions.Add(foreignKeyPropertyDiscoveryConvention);
            conventionSet.ForeignKeyAddedConventions.Add(keyDiscoveryConvention);
            conventionSet.ForeignKeyAddedConventions.Add(valueGeneratorConvention);
            conventionSet.ForeignKeyAddedConventions.Add(cascadeDeleteConvention);
            conventionSet.ForeignKeyAddedConventions.Add(foreignKeyIndexConvention);

            conventionSet.ForeignKeyRemovedConventions.Add(keyDiscoveryConvention);
            conventionSet.ForeignKeyRemovedConventions.Add(valueGeneratorConvention);
            conventionSet.ForeignKeyRemovedConventions.Add(foreignKeyIndexConvention);

            conventionSet.ForeignKeyUniquenessChangedConventions.Add(foreignKeyPropertyDiscoveryConvention);
            conventionSet.ForeignKeyUniquenessChangedConventions.Add(keyDiscoveryConvention);
            conventionSet.ForeignKeyUniquenessChangedConventions.Add(foreignKeyIndexConvention);

            conventionSet.ForeignKeyRequirednessChangedConventions.Add(cascadeDeleteConvention);
            conventionSet.ForeignKeyRequirednessChangedConventions.Add(foreignKeyPropertyDiscoveryConvention);

            conventionSet.ForeignKeyOwnershipChangedConventions.Add(new NavigationEagerLoadingConvention());
            conventionSet.ForeignKeyOwnershipChangedConventions.Add(keyDiscoveryConvention);
            conventionSet.ForeignKeyOwnershipChangedConventions.Add(relationshipDiscoveryConvention);

            conventionSet.ModelBuiltConventions.Add(new ModelCleanupConvention());
            conventionSet.ModelBuiltConventions.Add(keyAttributeConvention);
            conventionSet.ModelBuiltConventions.Add(foreignKeyAttributeConvention);
            conventionSet.ModelBuiltConventions.Add(new ChangeTrackingStrategyConvention());
            conventionSet.ModelBuiltConventions.Add(new ConstructorBindingConvention(Dependencies.ConstructorBindingFactory));
            conventionSet.ModelBuiltConventions.Add(new TypeMappingConvention(Dependencies.TypeMappingSource));
            conventionSet.ModelBuiltConventions.Add(new IgnoredMembersValidationConvention());
            conventionSet.ModelBuiltConventions.Add(foreignKeyIndexConvention);

            conventionSet.ModelBuiltConventions.Add(
                new PropertyMappingValidationConvention(
                    Dependencies.TypeMappingSource,
                    Dependencies.MemberClassifier));

            conventionSet.ModelBuiltConventions.Add(new RelationshipValidationConvention());
            conventionSet.ModelBuiltConventions.Add(foreignKeyPropertyDiscoveryConvention);
            conventionSet.ModelBuiltConventions.Add(servicePropertyDiscoveryConvention);
            conventionSet.ModelBuiltConventions.Add(new CacheCleanupConvention());

            conventionSet.NavigationAddedConventions.Add(backingFieldConvention);
            conventionSet.NavigationAddedConventions.Add(new RequiredNavigationAttributeConvention(Dependencies.Logger));
            conventionSet.NavigationAddedConventions.Add(inversePropertyAttributeConvention);
            conventionSet.NavigationAddedConventions.Add(foreignKeyPropertyDiscoveryConvention);
            conventionSet.NavigationAddedConventions.Add(relationshipDiscoveryConvention);

            conventionSet.NavigationRemovedConventions.Add(relationshipDiscoveryConvention);

            conventionSet.IndexAddedConventions.Add(foreignKeyIndexConvention);

            conventionSet.IndexRemovedConventions.Add(foreignKeyIndexConvention);

            conventionSet.IndexUniquenessChangedConventions.Add(foreignKeyIndexConvention);

            conventionSet.PrincipalEndChangedConventions.Add(foreignKeyPropertyDiscoveryConvention);

            conventionSet.PropertyFieldChangedConventions.Add(keyDiscoveryConvention);
            conventionSet.PropertyFieldChangedConventions.Add(foreignKeyPropertyDiscoveryConvention);
            conventionSet.PropertyFieldChangedConventions.Add(keyAttributeConvention);
            conventionSet.PropertyFieldChangedConventions.Add(concurrencyCheckAttributeConvention);
            conventionSet.PropertyFieldChangedConventions.Add(databaseGeneratedAttributeConvention);
            conventionSet.PropertyFieldChangedConventions.Add(requiredPropertyAttributeConvention);
            conventionSet.PropertyFieldChangedConventions.Add(maxLengthAttributeConvention);
            conventionSet.PropertyFieldChangedConventions.Add(stringLengthAttributeConvention);
            conventionSet.PropertyFieldChangedConventions.Add(timestampAttributeConvention);

            return conventionSet;
        }
    }
}
