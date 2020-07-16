// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention which looks for matching skip navigations and automatically creates
    ///     a many-to-many association entity with suitable foreign keys, sets the two
    ///     matching skip navigations to use those foreign keys.
    /// </summary>
    public class ManyToManyAssociationEntityTypeConvention : ISkipNavigationAddedConvention, ISkipNavigationInverseChangedConvention
    {
        private const string AssociationEntityTypeNameTemplate = "{0}{1}";
        private const string AssociationPropertyNameTemplate = "{0}_{1}";

        /// <summary>
        ///     Creates a new instance of <see cref="ManyToManyAssociationEntityTypeConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public ManyToManyAssociationEntityTypeConvention([NotNull] ProviderConventionSetBuilderDependencies dependencies)
        {
            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

        /// <inheritdoc />
        public virtual void ProcessSkipNavigationAdded(
            IConventionSkipNavigationBuilder skipNavigationBuilder,
            IConventionContext<IConventionSkipNavigationBuilder> context)
        {
            Check.NotNull(skipNavigationBuilder, nameof(skipNavigationBuilder));
            Check.NotNull(context, nameof(context));

            CreateAssociationEntityType(skipNavigationBuilder);
        }

        /// <inheritdoc />
        public virtual void ProcessSkipNavigationInverseChanged(
            IConventionSkipNavigationBuilder skipNavigationBuilder,
            IConventionSkipNavigation inverse,
            IConventionSkipNavigation oldInverse,
            IConventionContext<IConventionSkipNavigation> context)
        {
            Check.NotNull(skipNavigationBuilder, nameof(skipNavigationBuilder));
            Check.NotNull(context, nameof(context));

            CreateAssociationEntityType(skipNavigationBuilder);
        }

        private void CreateAssociationEntityType(
            IConventionSkipNavigationBuilder skipNavigationBuilder)
        {
            var skipNavigation = (SkipNavigation)skipNavigationBuilder.Metadata;
            if (skipNavigation.AssociationEntityType != null)
            {
                return;
            }

            if (skipNavigation.ForeignKey != null
                || skipNavigation.TargetEntityType == skipNavigation.DeclaringEntityType
                || !skipNavigation.IsCollection)
            {
                // do not create the association entity type for a self-referencing
                // skip navigation, or for one that is already "in use"
                // (i.e. has its Foreign Key assigned).
                return;
            }

            var inverseSkipNavigation = skipNavigation.Inverse;
            if (inverseSkipNavigation == null
                || inverseSkipNavigation.ForeignKey != null
                || !inverseSkipNavigation.IsCollection)
            {
                // do not create the association entity type if
                // the inverse skip navigation is already "in use"
                // (i.e. has its Foreign Key assigned).
                return;
            }

            Check.DebugAssert(inverseSkipNavigation.Inverse == skipNavigation,
                "Inverse's inverse should be the original skip navigation");

            var declaringEntityType = skipNavigation.DeclaringEntityType;
            var inverseEntityType = inverseSkipNavigation.DeclaringEntityType;
            var model = declaringEntityType.Model;

            // create the association entity type
            var otherIdentifiers = model.GetEntityTypes().ToDictionary(et => et.Name, et => 0);
            var associationEntityTypeName = Uniquifier.Uniquify(
                string.Format(
                    AssociationEntityTypeNameTemplate,
                    declaringEntityType.ShortName(),
                    inverseEntityType.ShortName()),
                otherIdentifiers,
                int.MaxValue);

            var associationEntityTypeBuilder = model.Builder.SharedEntity(
                associationEntityTypeName, Model.DefaultPropertyBagType, ConfigurationSource.Convention);

            // Create left and right foreign keys from the outer entity types to
            // the association entity type and configure the skip navigations.
            // Roll back if any of this fails.
            var leftForeignKey =
                CreateSkipNavigationForeignKey(skipNavigation, associationEntityTypeBuilder);
            if (leftForeignKey == null)
            {
                model.Builder.HasNoEntityType(
                    associationEntityTypeBuilder.Metadata, ConfigurationSource.Convention);
                return;
            }

            var rightForeignKey =
                CreateSkipNavigationForeignKey(inverseSkipNavigation, associationEntityTypeBuilder);
            if (rightForeignKey == null)
            {
                // Removing the association entity type will also remove
                // the leftForeignKey created above.
                model.Builder.HasNoEntityType(
                    associationEntityTypeBuilder.Metadata, ConfigurationSource.Convention);
                return;
            }

            skipNavigation.Builder.HasForeignKey(leftForeignKey, ConfigurationSource.Convention);
            inverseSkipNavigation.Builder.HasForeignKey(rightForeignKey, ConfigurationSource.Convention);

            // Creating the primary key below also negates the need for an index on
            // the properties of leftForeignKey - that index is automatically removed.
            associationEntityTypeBuilder.PrimaryKey(
                leftForeignKey.Properties.Concat(rightForeignKey.Properties).ToList(),
                ConfigurationSource.Convention);
        }

        private static ForeignKey CreateSkipNavigationForeignKey(
            SkipNavigation skipNavigation,
            InternalEntityTypeBuilder associationEntityTypeBuilder)
        {
            var principalEntityType = skipNavigation.DeclaringEntityType;
            var principalKey = principalEntityType.FindPrimaryKey();
            if (principalKey == null)
            {
                return null;
            }

            var dependentEndForeignKeyPropertyNames = new List<string>();
            var otherIdentifiers = associationEntityTypeBuilder.Metadata
                .GetDeclaredProperties().ToDictionary(p => p.Name, p => 0);
            foreach (var property in principalKey.Properties)
            {
                var propertyName = Uniquifier.Uniquify(
                    string.Format(
                        AssociationPropertyNameTemplate,
                        principalEntityType.ShortName(),
                        property.Name),
                    otherIdentifiers,
                    int.MaxValue);
                dependentEndForeignKeyPropertyNames.Add(propertyName);
                otherIdentifiers.Add(propertyName, 0);
            }

            return associationEntityTypeBuilder
                .HasRelationship(
                    principalEntityType.Name,
                    dependentEndForeignKeyPropertyNames,
                    principalKey,
                    ConfigurationSource.Convention)
                .IsUnique(false, ConfigurationSource.Convention)
                .Metadata;
        }
    }
}
