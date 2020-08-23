// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
    public class ManyToManyJoinEntityTypeConvention :
        ISkipNavigationAddedConvention,
        ISkipNavigationInverseChangedConvention,
        ISkipNavigationForeignKeyChangedConvention,
        ISkipNavigationRemovedConvention
    {
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
            CreateJoinEntityType(skipNavigationBuilder);
        }

        /// <inheritdoc />
        public virtual void ProcessSkipNavigationInverseChanged(
            IConventionSkipNavigationBuilder skipNavigationBuilder,
            IConventionSkipNavigation inverse,
            IConventionSkipNavigation oldInverse,
            IConventionContext<IConventionSkipNavigation> context)
        {
            CreateJoinEntityType(skipNavigationBuilder);
        }

        /// <inheritdoc />
        public virtual void ProcessSkipNavigationForeignKeyChanged(
            IConventionSkipNavigationBuilder skipNavigationBuilder,
            IConventionForeignKey foreignKey,
            IConventionForeignKey oldForeignKey,
            IConventionContext<IConventionForeignKey> context)
        {
            var joinEntityType = oldForeignKey?.DeclaringEntityType;
            var navigation = skipNavigationBuilder.Metadata;
            if (joinEntityType?.Builder != null
                && navigation.IsCollection
                && navigation.ForeignKey?.DeclaringEntityType != joinEntityType)
            {
                ((InternalModelBuilder)joinEntityType.Model.Builder).RemoveImplicitJoinEntity((EntityType)joinEntityType);
            }
        }

        /// <inheritdoc />
        public virtual void ProcessSkipNavigationRemoved(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionSkipNavigation navigation,
            IConventionContext<IConventionSkipNavigation> context)
        {
            var joinEntityType = navigation.ForeignKey?.DeclaringEntityType;
            if (joinEntityType?.Builder != null
                && navigation.IsCollection)
            {
                ((InternalModelBuilder)joinEntityType.Model.Builder).RemoveImplicitJoinEntity((EntityType)joinEntityType);
            }
        }

        private void CreateJoinEntityType(IConventionSkipNavigationBuilder skipNavigationBuilder)
        {
            var skipNavigation = (SkipNavigation)skipNavigationBuilder.Metadata;
            if (skipNavigation.ForeignKey != null
                || !skipNavigation.IsCollection)
            {
                return;
            }

            var inverseSkipNavigation = skipNavigation.Inverse;
            if (inverseSkipNavigation == null
                || inverseSkipNavigation.ForeignKey != null
                || !inverseSkipNavigation.IsCollection)
            {
                return;
            }

            Check.DebugAssert(
                inverseSkipNavigation.Inverse == skipNavigation,
                "Inverse's inverse should be the original skip navigation");

            var declaringEntityType = skipNavigation.DeclaringEntityType;
            var inverseEntityType = inverseSkipNavigation.DeclaringEntityType;
            var model = declaringEntityType.Model;

            var joinEntityTypeName = declaringEntityType.ShortName();
            var inverseName = inverseEntityType.ShortName();
            joinEntityTypeName = StringComparer.Ordinal.Compare(joinEntityTypeName, inverseName) < 0
                ? joinEntityTypeName + inverseName
                : inverseName + joinEntityTypeName;

            if (model.FindEntityType(joinEntityTypeName) != null)
            {
                var otherIdentifiers = model.GetEntityTypes().ToDictionary(et => et.Name, et => 0);
                joinEntityTypeName = Uniquifier.Uniquify(
                    joinEntityTypeName,
                    otherIdentifiers,
                    int.MaxValue);
            }

            var joinEntityTypeBuilder = model.Builder.SharedTypeEntity(
                joinEntityTypeName, Model.DefaultPropertyBagType, ConfigurationSource.Convention);

            var leftForeignKey = CreateSkipNavigationForeignKey(skipNavigation, joinEntityTypeBuilder);
            if (leftForeignKey == null)
            {
                model.Builder.HasNoEntityType(joinEntityTypeBuilder.Metadata, ConfigurationSource.Convention);
                return;
            }

            var rightForeignKey = CreateSkipNavigationForeignKey(inverseSkipNavigation, joinEntityTypeBuilder);
            if (rightForeignKey == null)
            {
                model.Builder.HasNoEntityType(joinEntityTypeBuilder.Metadata, ConfigurationSource.Convention);
                return;
            }

            skipNavigation.Builder.HasForeignKey(leftForeignKey, ConfigurationSource.Convention);
            inverseSkipNavigation.Builder.HasForeignKey(rightForeignKey, ConfigurationSource.Convention);
        }

        private static ForeignKey CreateSkipNavigationForeignKey(
            SkipNavigation skipNavigation,
            InternalEntityTypeBuilder joinEntityTypeBuilder)
            => joinEntityTypeBuilder
                .HasRelationship(
                    skipNavigation.DeclaringEntityType,
                    ConfigurationSource.Convention,
                    required: true,
                    skipNavigation.Inverse.Name)
                .IsUnique(false, ConfigurationSource.Convention)
                .Metadata;
    }
}
