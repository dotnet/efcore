// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that configures the inverse navigation property based on the <see cref="InversePropertyAttribute" />
    ///     specified on the other navigation property.
    /// </summary>
    public class InversePropertyAttributeConvention :
        NavigationAttributeConventionBase<InversePropertyAttribute>,
        IModelFinalizingConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="InversePropertyAttributeConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public InversePropertyAttributeConvention([NotNull] ProviderConventionSetBuilderDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <summary>
        ///     Called for every navigation property that has an attribute after an entity type is added to the model.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type. </param>
        /// <param name="navigationMemberInfo"> The navigation member info. </param>
        /// <param name="targetClrType"> The CLR type of the target entity type</param>
        /// <param name="attribute"> The attribute. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public override void ProcessEntityTypeAdded(
            IConventionEntityTypeBuilder entityTypeBuilder,
            MemberInfo navigationMemberInfo,
            Type targetClrType,
            InversePropertyAttribute attribute,
            IConventionContext<IConventionEntityTypeBuilder> context)
            => Process(entityTypeBuilder, navigationMemberInfo, targetClrType, attribute);

        private void Process(
            IConventionEntityTypeBuilder entityTypeBuilder,
            MemberInfo navigationMemberInfo,
            Type targetClrType,
            InversePropertyAttribute attribute)
        {
            var entityType = (EntityType)entityTypeBuilder.Metadata;
            var navigationName = navigationMemberInfo.GetSimpleMemberName();
            if (entityTypeBuilder.IsIgnored(navigationName, fromDataAnnotation: true)
                || entityType.FindPropertiesInHierarchy(navigationName).Cast<IConventionPropertyBase>()
                    .Concat(entityType.FindServicePropertiesInHierarchy(navigationName))
                    .Any(m => !ConfigurationSource.DataAnnotation.Overrides(m.GetConfigurationSource())))
            {
                return;
            }

            var targetEntityTypeBuilder = ((InternalEntityTypeBuilder)entityTypeBuilder).GetTargetEntityTypeBuilder(
                targetClrType, navigationMemberInfo, ConfigurationSource.DataAnnotation);

            if (targetEntityTypeBuilder == null)
            {
                return;
            }

            ConfigureInverseNavigation(entityTypeBuilder, navigationMemberInfo, targetEntityTypeBuilder, attribute);
        }

        private IConventionForeignKeyBuilder ConfigureInverseNavigation(
            IConventionEntityTypeBuilder entityTypeBuilder,
            MemberInfo navigationMemberInfo,
            IConventionEntityTypeBuilder targetEntityTypeBuilder,
            InversePropertyAttribute attribute)
        {
            var entityType = entityTypeBuilder.Metadata;
            var targetClrType = targetEntityTypeBuilder.Metadata.ClrType;
            var inverseNavigationPropertyInfo = targetEntityTypeBuilder.Metadata.GetRuntimeProperties().Values
                .FirstOrDefault(p => string.Equals(p.GetSimpleMemberName(), attribute.Property, StringComparison.OrdinalIgnoreCase));

            if (inverseNavigationPropertyInfo == null
                || !Dependencies.MemberClassifier.FindCandidateNavigationPropertyType(inverseNavigationPropertyInfo)
                    .IsAssignableFrom(entityType.ClrType))
            {
                throw new InvalidOperationException(
                    CoreStrings.InvalidNavigationWithInverseProperty(
                        navigationMemberInfo.Name, entityType.DisplayName(), attribute.Property, targetClrType.ShortDisplayName()));
            }

            if (Equals(inverseNavigationPropertyInfo, navigationMemberInfo))
            {
                throw new InvalidOperationException(
                    CoreStrings.SelfReferencingNavigationWithInverseProperty(
                        navigationMemberInfo.Name,
                        entityType.DisplayName(),
                        navigationMemberInfo.Name,
                        entityType.DisplayName()));
            }

            // Check for InversePropertyAttribute on the inverse navigation to verify that it matches.
            if (Attribute.IsDefined(inverseNavigationPropertyInfo, typeof(InversePropertyAttribute)))
            {
                var inverseAttribute = inverseNavigationPropertyInfo.GetCustomAttribute<InversePropertyAttribute>(true);
                if (inverseAttribute.Property != navigationMemberInfo.GetSimpleMemberName())
                {
                    throw new InvalidOperationException(
                        CoreStrings.InversePropertyMismatch(
                            navigationMemberInfo.Name,
                            entityType.DisplayName(),
                            inverseNavigationPropertyInfo.Name,
                            targetEntityTypeBuilder.Metadata.DisplayName()));
                }
            }

            var referencingNavigationsWithAttribute =
                AddInverseNavigation(entityType, navigationMemberInfo, targetEntityTypeBuilder.Metadata, inverseNavigationPropertyInfo);

            var ambiguousInverse = FindAmbiguousInverse(
                navigationMemberInfo, entityType, referencingNavigationsWithAttribute);
            var baseType = targetEntityTypeBuilder.Metadata.BaseType;
            while (ambiguousInverse == null
                && baseType != null)
            {
                var navigationMap = GetInverseNavigations(baseType);
                if (navigationMap != null
                    && navigationMap.TryGetValue(inverseNavigationPropertyInfo.Name, out var inverseTuple))
                {
                    referencingNavigationsWithAttribute = inverseTuple.References;
                    ambiguousInverse = FindAmbiguousInverse(navigationMemberInfo, entityType, referencingNavigationsWithAttribute);
                }

                baseType = baseType.BaseType;
            }

            if (ambiguousInverse != null)
            {
                if (entityType.FindSkipNavigation(navigationMemberInfo) is IConventionSkipNavigation existingSkipNavigation)
                {
                    var existingSkipNavigationInverse = existingSkipNavigation.Inverse;
                    var inverseSkipNavigation = targetEntityTypeBuilder.Metadata.FindSkipNavigation(inverseNavigationPropertyInfo);
                    var existingInverse = inverseSkipNavigation?.Inverse;
                    var existingInverseType = existingInverse?.DeclaringEntityType;
                    if (existingInverse != null
                        && IsAmbiguousInverse(
                            existingInverse.GetIdentifyingMemberInfo(), existingInverseType, referencingNavigationsWithAttribute))
                    {
                        existingInverse.DeclaringEntityType.Builder.HasNoSkipNavigation(existingInverse, fromDataAnnotation: true);
                        inverseSkipNavigation.DeclaringEntityType.Builder.HasNoSkipNavigation(
                            inverseSkipNavigation, fromDataAnnotation: true);
                    }

                    if (existingSkipNavigation.Builder != null)
                    {
                        entityType.Builder.HasNoSkipNavigation(existingSkipNavigation, fromDataAnnotation: true);
                    }

                    if (existingSkipNavigationInverse?.Builder != null)
                    {
                        existingSkipNavigationInverse.DeclaringEntityType.Builder
                            .HasNoSkipNavigation(existingSkipNavigationInverse, fromDataAnnotation: true);
                    }

                    var existingAmbiguousNavigation = FindActualEntityType(ambiguousInverse.Value.Item2)
                        .FindSkipNavigation(ambiguousInverse.Value.Item1);
                    if (existingAmbiguousNavigation != null)
                    {
                        existingAmbiguousNavigation.DeclaringEntityType.Builder.HasNoSkipNavigation(
                            existingAmbiguousNavigation, fromDataAnnotation: true);
                    }

                    return entityType.FindSkipNavigation(navigationMemberInfo)?.ForeignKey.Builder;
                }
                else
                {
                    var existingInverse = targetEntityTypeBuilder.Metadata.FindNavigation(inverseNavigationPropertyInfo)?.Inverse;
                    var existingInverseType = existingInverse?.DeclaringEntityType;
                    if (existingInverse != null
                        && IsAmbiguousInverse(
                            existingInverse.GetIdentifyingMemberInfo(), existingInverseType, referencingNavigationsWithAttribute))
                    {
                        var fk = existingInverse.ForeignKey;
                        if (fk.IsOwnership
                            || fk.DeclaringEntityType.Builder.HasNoRelationship(fk, fromDataAnnotation: true) == null)
                        {
                            fk.Builder.HasNavigation(
                                (string)null,
                                existingInverse.IsOnDependent,
                                fromDataAnnotation: true);
                        }
                    }

                    var existingNavigation = entityType.FindNavigation(navigationMemberInfo);
                    if (existingNavigation != null)
                    {
                        var fk = existingNavigation.ForeignKey;
                        if (fk.IsOwnership
                            || fk.DeclaringEntityType.Builder.HasNoRelationship(fk, fromDataAnnotation: true) == null)
                        {
                            fk.Builder.HasNavigation(
                                (string)null,
                                existingNavigation.IsOnDependent,
                                fromDataAnnotation: true);
                        }
                    }

                    var existingAmbiguousNavigation = FindActualEntityType(ambiguousInverse.Value.Item2)
                        .FindNavigation(ambiguousInverse.Value.Item1);
                    if (existingAmbiguousNavigation != null)
                    {
                        var fk = existingAmbiguousNavigation.ForeignKey;
                        if (fk.IsOwnership
                            || fk.DeclaringEntityType.Builder.HasNoRelationship(fk, fromDataAnnotation: true) == null)
                        {
                            fk.Builder.HasNavigation(
                                (string)null,
                                existingAmbiguousNavigation.IsOnDependent,
                                fromDataAnnotation: true);
                        }
                    }

                    return entityType.FindNavigation(navigationMemberInfo)?.ForeignKey.Builder;
                }
            }

            var ownership = entityType.FindOwnership();
            if (ownership != null
                && ownership.PrincipalEntityType == targetEntityTypeBuilder.Metadata
                && ownership.PrincipalToDependent?.GetIdentifyingMemberInfo() != inverseNavigationPropertyInfo)
            {
                Dependencies.Logger.NonOwnershipInverseNavigationWarning(
                    entityType, navigationMemberInfo,
                    targetEntityTypeBuilder.Metadata, inverseNavigationPropertyInfo,
                    ownership.PrincipalToDependent?.GetIdentifyingMemberInfo());

                return null;
            }

            if (entityType.DefiningEntityType != null
                && entityType.DefiningEntityType == targetEntityTypeBuilder.Metadata
                && entityType.DefiningNavigationName != inverseNavigationPropertyInfo.GetSimpleMemberName())
            {
                Dependencies.Logger.NonDefiningInverseNavigationWarning(
                    entityType, navigationMemberInfo,
                    targetEntityTypeBuilder.Metadata, inverseNavigationPropertyInfo,
                    entityType.DefiningEntityType.GetRuntimeProperties()[entityType.DefiningNavigationName]);

                return null;
            }

            if (entityType.Model.FindIsOwnedConfigurationSource(entityType.ClrType) != null
                && !entityType.IsInOwnershipPath(targetEntityTypeBuilder.Metadata))
            {
                return targetEntityTypeBuilder.HasOwnership(
                    entityTypeBuilder.Metadata.ClrType,
                    inverseNavigationPropertyInfo,
                    navigationMemberInfo,
                    fromDataAnnotation: true);
            }

            var newForeignKeyBuilder = targetEntityTypeBuilder.HasRelationship(
                entityType,
                inverseNavigationPropertyInfo,
                navigationMemberInfo,
                fromDataAnnotation: true);

            if (newForeignKeyBuilder == null
                && navigationMemberInfo is PropertyInfo navigationPropertyInfo)
            {
                var navigationTargetType = navigationPropertyInfo.PropertyType.TryGetSequenceType();
                var inverseNavigationTargetType = inverseNavigationPropertyInfo.PropertyType.TryGetSequenceType();
                if (navigationTargetType != null
                    && inverseNavigationTargetType != null
                    && navigationTargetType.IsAssignableFrom(targetClrType)
                    && inverseNavigationTargetType.IsAssignableFrom(entityType.ClrType))
                {
                    entityTypeBuilder.HasSkipNavigation(
                        navigationPropertyInfo, targetEntityTypeBuilder.Metadata,
                        inverseNavigationPropertyInfo, collections: true, onDependent: false, fromDataAnnotation: true);
                }
            }

            return newForeignKeyBuilder;
        }

        /// <inheritdoc />
        public override void ProcessEntityTypeRemoved(
            IConventionModelBuilder modelBuilder,
            Type type,
            MemberInfo navigationMemberInfo,
            Type targetClrType,
            InversePropertyAttribute attribute,
            IConventionContext<IConventionEntityType> context)
        {
            var targetEntityType = modelBuilder.Metadata.FindEntityType(targetClrType);
            if (targetEntityType != null)
            {
                RemoveInverseNavigation(type, navigationMemberInfo, targetEntityType, attribute.Property);
            }

            var declaringType = navigationMemberInfo.DeclaringType;
            Check.DebugAssert(declaringType != null, "declaringType is null");
            if (modelBuilder.Metadata.FindEntityType(declaringType) != null)
            {
                return;
            }

            var leastDerivedEntityTypes = modelBuilder.Metadata.FindLeastDerivedEntityTypes(
                declaringType,
                t => !t.Builder.IsIgnored(navigationMemberInfo.GetSimpleMemberName(), fromDataAnnotation: true));
            foreach (var leastDerivedEntityType in leastDerivedEntityTypes)
            {
                Process(leastDerivedEntityType.Builder, navigationMemberInfo, targetClrType, attribute);
            }
        }

        /// <inheritdoc />
        public override void ProcessNavigationAdded(
            IConventionNavigationBuilder navigationBuilder,
            InversePropertyAttribute attribute,
            IConventionContext<IConventionNavigationBuilder> context)
        {
            var navigation = navigationBuilder.Metadata;
            var foreignKey = navigation.ForeignKey;
            if (foreignKey.DeclaringEntityType.HasDefiningNavigation()
                || foreignKey.DeclaringEntityType.IsOwned()
                || foreignKey.PrincipalEntityType.HasDefiningNavigation()
                || foreignKey.PrincipalEntityType.IsOwned())
            {
                return;
            }

            var newRelationshipBuilder = ConfigureInverseNavigation(
                navigation.DeclaringEntityType.Builder,
                navigation.GetIdentifyingMemberInfo(),
                navigation.TargetEntityType.Builder,
                attribute);

            if (newRelationshipBuilder == null)
            {
                context.StopProcessing();
                return;
            }

            var newNavigation = navigation.IsOnDependent
                ? newRelationshipBuilder.Metadata.DependentToPrincipal
                : newRelationshipBuilder.Metadata.PrincipalToDependent;

            context.StopProcessingIfChanged(newNavigation?.Builder);
        }

        /// <inheritdoc />
        public override void ProcessEntityTypeBaseTypeChanged(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionEntityType newBaseType,
            IConventionEntityType oldBaseType,
            MemberInfo navigationMemberInfo,
            Type targetClrType,
            InversePropertyAttribute attribute,
            IConventionContext<IConventionEntityType> context)
        {
            var entityClrType = entityTypeBuilder.Metadata.ClrType;
            if (navigationMemberInfo.DeclaringType != entityClrType)
            {
                if (newBaseType == null)
                {
                    Process(entityTypeBuilder, navigationMemberInfo, targetClrType, attribute);
                }
                else
                {
                    var targetEntityType = entityTypeBuilder.Metadata.Model.FindEntityType(targetClrType);
                    if (targetEntityType == null)
                    {
                        return;
                    }

                    RemoveInverseNavigation(entityClrType, navigationMemberInfo, targetEntityType, attribute.Property);
                }
            }
        }

        /// <inheritdoc />
        public override void ProcessEntityTypeMemberIgnored(
            IConventionEntityTypeBuilder entityTypeBuilder,
            MemberInfo navigationMemberInfo,
            Type targetClrType,
            InversePropertyAttribute attribute,
            IConventionContext<string> context)
        {
            var targetEntityType = ((InternalEntityTypeBuilder)entityTypeBuilder).GetTargetEntityTypeBuilder(
                targetClrType, navigationMemberInfo, null)?.Metadata;
            if (targetEntityType == null)
            {
                return;
            }

            RemoveInverseNavigation(entityTypeBuilder.Metadata.ClrType, navigationMemberInfo, targetEntityType, attribute.Property);
        }

        /// <inheritdoc />
        public virtual void ProcessModelFinalizing(
            IConventionModelBuilder modelBuilder,
            IConventionContext<IConventionModelBuilder> context)
        {
            var model = modelBuilder.Metadata;
            foreach (var entityType in model.GetEntityTypes())
            {
                var inverseNavigations = GetInverseNavigations(entityType);
                if (inverseNavigations == null)
                {
                    continue;
                }

                foreach (var inverseNavigation in inverseNavigations.Values)
                {
                    foreach (var referencingNavigationWithAttribute in inverseNavigation.References)
                    {
                        var ambiguousInverse = FindAmbiguousInverse(
                            referencingNavigationWithAttribute.Item1,
                            referencingNavigationWithAttribute.Item2,
                            inverseNavigation.References);

                        var baseType = entityType.BaseType;
                        while (ambiguousInverse == null
                            && baseType != null)
                        {
                            var navigationMap = GetInverseNavigations(baseType);
                            if (navigationMap != null
                                && navigationMap.TryGetValue(inverseNavigation.Navigation.Name, out var inverseTuple))
                            {
                                var referencingNavigationsWithAttribute = inverseTuple.References;
                                ambiguousInverse = FindAmbiguousInverse(
                                    referencingNavigationWithAttribute.Item1,
                                    referencingNavigationWithAttribute.Item2,
                                    referencingNavigationsWithAttribute);
                            }

                            baseType = baseType.BaseType;
                        }

                        if (ambiguousInverse != null)
                        {
                            Dependencies.Logger.MultipleInversePropertiesSameTargetWarning(
                                new[]
                                {
                                    Tuple.Create(
                                        referencingNavigationWithAttribute.Item1, referencingNavigationWithAttribute.Item2.ClrType),
                                    Tuple.Create(ambiguousInverse.Value.Item1, ambiguousInverse.Value.Item2.ClrType)
                                },
                                inverseNavigation.Navigation,
                                entityType.ClrType);
                            break;
                        }
                    }
                }
            }

            foreach (var entityType in model.GetEntityTypes())
            {
                entityType.RemoveAnnotation(CoreAnnotationNames.InverseNavigations);
            }
        }

        /// <summary>
        ///     Returns a value indication whether the given navigation has ambiguous inverse navigations with
        ///     <see cref="InversePropertyAttribute" />.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="navigation"> The navigation. </param>
        /// <param name="targetEntityType"> Target entity type. </param>
        /// <returns>
        ///     <see langword="true" /> if the given navigation has ambiguous inverse navigations with <see cref="InversePropertyAttribute" />.
        /// </returns>
        public static bool IsAmbiguous(
            [NotNull] IConventionEntityType entityType,
            [NotNull] MemberInfo navigation,
            [NotNull] IConventionEntityType targetEntityType)
        {
            if (!Attribute.IsDefined(navigation, typeof(InversePropertyAttribute)))
            {
                return false;
            }

            while (targetEntityType != null)
            {
                var navigationMap = GetInverseNavigations(targetEntityType);
                if (navigationMap != null)
                {
                    foreach (var inverseNavigationTuple in navigationMap.Values)
                    {
                        if (inverseNavigationTuple.Navigation.GetMemberType().IsAssignableFrom(entityType.ClrType)
                            && IsAmbiguousInverse(navigation, entityType, inverseNavigationTuple.References))
                        {
                            return true;
                        }
                    }
                }

                targetEntityType = targetEntityType.BaseType;
            }

            return false;
        }

        private static bool IsAmbiguousInverse(
            MemberInfo navigation,
            IConventionEntityType entityType,
            List<(MemberInfo, IConventionEntityType)> referencingNavigationsWithAttribute)
            => FindAmbiguousInverse(navigation, entityType, referencingNavigationsWithAttribute) != null;

        private static (MemberInfo, IConventionEntityType)? FindAmbiguousInverse(
            MemberInfo navigation,
            IConventionEntityType entityType,
            List<(MemberInfo Inverse, IConventionEntityType InverseEntityType)> referencingNavigationsWithAttribute)
        {
            (MemberInfo, IConventionEntityType)? ambiguousTuple = null;
            foreach (var referencingTuple in referencingNavigationsWithAttribute)
            {
                var inverseEntityType = FindActualEntityType(referencingTuple.InverseEntityType);
                if (inverseEntityType?.Builder.IsIgnored(
                    referencingTuple.Inverse.GetSimpleMemberName(), fromDataAnnotation: true) != false)
                {
                    continue;
                }

                if (!referencingTuple.Inverse.IsSameAs(navigation)
                    || (!entityType.IsAssignableFrom(inverseEntityType)
                        && !inverseEntityType.IsAssignableFrom(entityType)))
                {
                    ambiguousTuple = referencingTuple;
                    break;
                }
            }

            return ambiguousTuple;
        }

        private static List<(MemberInfo, IConventionEntityType)> AddInverseNavigation(
            IConventionEntityType entityType,
            MemberInfo navigation,
            IConventionEntityType targetEntityType,
            MemberInfo inverseNavigation)
        {
            var inverseNavigations = GetInverseNavigations(targetEntityType);
            if (inverseNavigations == null)
            {
                inverseNavigations = new Dictionary<string, (MemberInfo, List<(MemberInfo, IConventionEntityType)>)>();
                SetInverseNavigations(targetEntityType.Builder, inverseNavigations);
            }

            List<(MemberInfo, IConventionEntityType)> referencingNavigationsWithAttribute;
            if (!inverseNavigations.TryGetValue(inverseNavigation.Name, out var inverseTuple))
            {
                referencingNavigationsWithAttribute = new List<(MemberInfo, IConventionEntityType)>();
                inverseNavigations[inverseNavigation.Name] = (inverseNavigation, referencingNavigationsWithAttribute);
            }
            else
            {
                referencingNavigationsWithAttribute = inverseTuple.References;
            }

            foreach (var referencingTuple in referencingNavigationsWithAttribute)
            {
                if (referencingTuple.Item1.IsSameAs(navigation)
                    && referencingTuple.Item2.ClrType == entityType.ClrType
                    && FindActualEntityType(referencingTuple.Item2) == entityType)
                {
                    return referencingNavigationsWithAttribute;
                }
            }

            referencingNavigationsWithAttribute.Add((navigation, entityType));

            return referencingNavigationsWithAttribute;
        }

        private static void RemoveInverseNavigation(
            Type declaringType,
            MemberInfo navigation,
            IConventionEntityType targetEntityType,
            string inverseNavigationName)
        {
            var inverseNavigations = GetInverseNavigations(targetEntityType);
            if (inverseNavigations == null
                || !inverseNavigations.TryGetValue(inverseNavigationName, out var inverseNavigationPair))
            {
                return;
            }

            var anyRemoved = false;
            var (inverseNavigation, referencingNavigationsWithAttribute) = inverseNavigationPair;
            for (var index = 0; index < referencingNavigationsWithAttribute.Count; index++)
            {
                var referencingTuple = referencingNavigationsWithAttribute[index];
                if (referencingTuple.Item1.IsSameAs(navigation)
                    && declaringType.IsAssignableFrom(referencingTuple.Item2.ClrType))
                {
                    anyRemoved = true;
                    referencingNavigationsWithAttribute.RemoveAt(index--);
                    if (referencingNavigationsWithAttribute.Count == 0)
                    {
                        inverseNavigations.Remove(inverseNavigation.Name);
                    }
                }
            }

            if (anyRemoved
                && referencingNavigationsWithAttribute.Count == 1)
            {
                var otherEntityType = FindActualEntityType(referencingNavigationsWithAttribute[0].Item2);
                if (otherEntityType != null)
                {
                    targetEntityType.Builder.HasRelationship(
                        otherEntityType,
                        inverseNavigation,
                        referencingNavigationsWithAttribute[0].Item1,
                        fromDataAnnotation: true);
                }
            }
        }

        private static IConventionEntityType FindActualEntityType(IConventionEntityType entityType)
            => ((Model)entityType.Model).FindActualEntityType((EntityType)entityType);

        private static Dictionary<string, (MemberInfo Navigation, List<(MemberInfo, IConventionEntityType)> References)>
            GetInverseNavigations(
                IConventionAnnotatable entityType)
            => entityType.FindAnnotation(CoreAnnotationNames.InverseNavigations)?.Value
                as Dictionary<string, (MemberInfo, List<(MemberInfo, IConventionEntityType)>)>;

        private static void SetInverseNavigations(
            IConventionAnnotatableBuilder entityTypeBuilder,
            Dictionary<string, (MemberInfo, List<(MemberInfo, IConventionEntityType)>)> inverseNavigations)
            => entityTypeBuilder.HasAnnotation(CoreAnnotationNames.InverseNavigations, inverseNavigations);
    }
}
