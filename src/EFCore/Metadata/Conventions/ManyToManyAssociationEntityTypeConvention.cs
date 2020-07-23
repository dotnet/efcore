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
    ///     a many-to-many join entity with suitable foreign keys, sets the two
    ///     matching skip navigations to use those foreign keys.
    /// </summary>
    public class ManyToManyJoinEntityTypeConvention : ISkipNavigationAddedConvention, ISkipNavigationInverseChangedConvention
    {
        private const string JoinEntityTypeNameTemplate = "{0}{1}";
        private const string JoinPropertyNameTemplate = "{0}_{1}";

        /// <summary>
        ///     Creates a new instance of <see cref="ManyToManyJoinEntityTypeConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public ManyToManyJoinEntityTypeConvention([NotNull] ProviderConventionSetBuilderDependencies dependencies)
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

            CreateJoinEntityType(skipNavigationBuilder);
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

            CreateJoinEntityType(skipNavigationBuilder);
        }

        private void CreateJoinEntityType(
            IConventionSkipNavigationBuilder skipNavigationBuilder)
        {
            var skipNavigation = (SkipNavigation)skipNavigationBuilder.Metadata;
            if (skipNavigation.JoinEntityType != null)
            {
                return;
            }

            if (skipNavigation.ForeignKey != null
                || skipNavigation.TargetEntityType == skipNavigation.DeclaringEntityType
                || !skipNavigation.IsCollection)
            {
                // do not create the join entity type for a self-referencing
                // skip navigation, or for one that is already "in use"
                // (i.e. has its Foreign Key assigned).
                return;
            }

            var inverseSkipNavigation = skipNavigation.Inverse;
            if (inverseSkipNavigation == null
                || inverseSkipNavigation.ForeignKey != null
                || !inverseSkipNavigation.IsCollection)
            {
                // do not create the join entity type if
                // the inverse skip navigation is already "in use"
                // (i.e. has its Foreign Key assigned).
                return;
            }

            Check.DebugAssert(inverseSkipNavigation.Inverse == skipNavigation,
                "Inverse's inverse should be the original skip navigation");

            var declaringEntityType = skipNavigation.DeclaringEntityType;
            var inverseEntityType = inverseSkipNavigation.DeclaringEntityType;
            var model = declaringEntityType.Model;

            // create the join entity type
            var joinEntityTypeName = string.Format(
                    JoinEntityTypeNameTemplate,
                    declaringEntityType.ShortName(),
                    inverseEntityType.ShortName());
            if (model.FindEntityType(joinEntityTypeName) != null)
            {
                var otherIdentifiers = model.GetEntityTypes().ToDictionary(et => et.Name, et => 0);
                joinEntityTypeName = Uniquifier.Uniquify(
                    joinEntityTypeName,
                    otherIdentifiers,
                    int.MaxValue);
            }

            var joinEntityTypeBuilder = model.Builder.SharedEntity(
                joinEntityTypeName, Model.DefaultPropertyBagType, ConfigurationSource.Convention);

            // Create left and right foreign keys from the outer entity types to
            // the join entity type and configure the skip navigations.
            // Roll back if any of this fails.
            var leftForeignKey =
                CreateSkipNavigationForeignKey(skipNavigation, joinEntityTypeBuilder);
            if (leftForeignKey == null)
            {
                model.Builder.HasNoEntityType(
                    joinEntityTypeBuilder.Metadata, ConfigurationSource.Convention);
                return;
            }

            var rightForeignKey =
                CreateSkipNavigationForeignKey(inverseSkipNavigation, joinEntityTypeBuilder);
            if (rightForeignKey == null)
            {
                // Removing the join entity type will also remove
                // the leftForeignKey created above.
                model.Builder.HasNoEntityType(
                    joinEntityTypeBuilder.Metadata, ConfigurationSource.Convention);
                return;
            }

            skipNavigation.Builder.HasForeignKey(leftForeignKey, ConfigurationSource.Convention);
            inverseSkipNavigation.Builder.HasForeignKey(rightForeignKey, ConfigurationSource.Convention);

            // Creating the primary key below also negates the need for an index on
            // the properties of leftForeignKey - that index is automatically removed.
            joinEntityTypeBuilder.PrimaryKey(
                leftForeignKey.Properties.Concat(rightForeignKey.Properties).ToList(),
                ConfigurationSource.Convention);
        }

        private static ForeignKey CreateSkipNavigationForeignKey(
            SkipNavigation skipNavigation,
            InternalEntityTypeBuilder joinEntityTypeBuilder)
        {
            var principalEntityType = skipNavigation.DeclaringEntityType;
            var principalKey = principalEntityType.FindPrimaryKey();
            if (principalKey == null)
            {
                return null;
            }

            var dependentEndForeignKeyPropertyNames = new List<string>();
            var otherIdentifiers = joinEntityTypeBuilder.Metadata
                .GetDeclaredProperties().ToDictionary(p => p.Name, p => 0);
            foreach (var property in principalKey.Properties)
            {
                var propertyName = Uniquifier.Uniquify(
                    string.Format(
                        JoinPropertyNameTemplate,
                        principalEntityType.ShortName(),
                        property.Name),
                    otherIdentifiers,
                    int.MaxValue);
                dependentEndForeignKeyPropertyNames.Add(propertyName);
                otherIdentifiers.Add(propertyName, 0);
            }

            return joinEntityTypeBuilder
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
