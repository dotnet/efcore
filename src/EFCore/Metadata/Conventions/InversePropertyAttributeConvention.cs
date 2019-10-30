// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that configures the inverse navigation property based on the <see cref="InversePropertyAttribute" />
    ///     specified on the other navigation property.
    /// </summary>
    public class InversePropertyAttributeConvention :
        NavigationAttributeConventionBase<InversePropertyAttribute>,
        IModelFinalizedConvention
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
            IConventionEntityTypeBuilder entityTypeBuilder, MemberInfo navigationMemberInfo, Type targetClrType,
            InversePropertyAttribute attribute)
        {
            if (!entityTypeBuilder.CanAddNavigation(
                navigationMemberInfo.GetSimpleMemberName(), fromDataAnnotation: true))
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

        private IConventionRelationshipBuilder ConfigureInverseNavigation(
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
                || !Dependencies.MemberClassifier.FindCandidateNavigationPropertyType(inverseNavigationPropertyInfo).GetTypeInfo()
                    .IsAssignableFrom(entityType.ClrType.GetTypeInfo()))
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
                var existingInverse = targetEntityTypeBuilder.Metadata.FindNavigation(inverseNavigationPropertyInfo)?.FindInverse();
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
                            existingInverse.IsDependentToPrincipal(),
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
                            existingNavigation.IsDependentToPrincipal(),
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
                            existingAmbiguousNavigation.IsDependentToPrincipal(),
                            fromDataAnnotation: true);
                    }
                }

                return entityType.FindNavigation(navigationMemberInfo)?.ForeignKey.Builder;
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

            return entityType.Model.FindIsOwnedConfigurationSource(entityType.ClrType) != null
                && !entityType.IsInOwnershipPath(targetEntityTypeBuilder.Metadata)
                    ? targetEntityTypeBuilder.HasOwnership(
                        entityTypeBuilder.Metadata.ClrType,
                        inverseNavigationPropertyInfo,
                        navigationMemberInfo,
                        fromDataAnnotation: true)
                    : targetEntityTypeBuilder.HasRelationship(
                        entityType,
                        inverseNavigationPropertyInfo,
                        navigationMemberInfo,
                        fromDataAnnotation: true);
        }

        /// <summary>
        ///     Called for every navigation property that has an attribute after an entity type is ignored.
        /// </summary>
        /// <param name="modelBuilder"> The builder for the model. </param>
        /// <param name="type"> The ignored entity type. </param>
        /// <param name="navigationMemberInfo"> The navigation member info. </param>
        /// <param name="targetClrType"> The CLR type of the target entity type. </param>
        /// <param name="attribute"> The attribute. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public override void ProcessEntityTypeIgnored(
            IConventionModelBuilder modelBuilder,
            Type type,
            MemberInfo navigationMemberInfo,
            Type targetClrType,
            InversePropertyAttribute attribute,
            IConventionContext<string> context)
        {
            var declaringType = navigationMemberInfo.DeclaringType;
            Debug.Assert(declaringType != null);
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

        /// <summary>
        ///     Called after a navigation property that has an attribute is added to an entity type.
        /// </summary>
        /// <param name="relationshipBuilder"> The builder for the relationship. </param>
        /// <param name="navigation"> The navigation. </param>
        /// <param name="attribute"> The attribute. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public override void ProcessNavigationAdded(
            IConventionRelationshipBuilder relationshipBuilder,
            IConventionNavigation navigation,
            InversePropertyAttribute attribute,
            IConventionContext<IConventionNavigation> context)
        {
            if (relationshipBuilder.Metadata.DeclaringEntityType.HasDefiningNavigation()
                || relationshipBuilder.Metadata.DeclaringEntityType.IsOwned()
                || relationshipBuilder.Metadata.PrincipalEntityType.HasDefiningNavigation()
                || relationshipBuilder.Metadata.PrincipalEntityType.IsOwned())
            {
                return;
            }

            var newRelationship = ConfigureInverseNavigation(
                navigation.DeclaringEntityType.Builder,
                navigation.GetIdentifyingMemberInfo(),
                navigation.GetTargetType().Builder,
                attribute);
            if (newRelationship != relationshipBuilder)
            {
                if (newRelationship == null)
                {
                    context.StopProcessingIfChanged(null);
                    return;
                }

                var newNavigation = navigation.IsDependentToPrincipal()
                    ? newRelationship.Metadata.DependentToPrincipal
                    : newRelationship.Metadata.PrincipalToDependent;

                context.StopProcessingIfChanged(newNavigation);
            }
        }

        /// <summary>
        ///     Called for every navigation property that has an attribute after the base type for an entity type is changed.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type. </param>
        /// <param name="newBaseType"> The new base type. </param>
        /// <param name="oldBaseType"> The old base type. </param>
        /// <param name="navigationMemberInfo"> The navigation member info. </param>
        /// <param name="targetClrType"> The CLR type of the target entity type. </param>
        /// <param name="attribute"> The attribute. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
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

                    RemoveInverseNavigation(entityTypeBuilder.Metadata, navigationMemberInfo, targetEntityType);
                }
            }
        }

        /// <summary>
        ///     Called after a navigation property that has an attribute is ignored.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type. </param>
        /// <param name="navigationMemberInfo"> The navigation member info. </param>
        /// <param name="targetClrType"> The CLR type of the target entity type. </param>
        /// <param name="attribute"> The attribute. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
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

            RemoveInverseNavigation(entityTypeBuilder.Metadata, navigationMemberInfo, targetEntityType);
        }

        /// <summary>
        ///     Called after a model is finalized.
        /// </summary>
        /// <param name="modelBuilder"> The builder for the model. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessModelFinalized(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
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
        ///     <c>true</c> if the given navigation has ambiguous inverse navigations with <see cref="InversePropertyAttribute" />.
        /// </returns>
        public static bool IsAmbiguous(
            [NotNull] IConventionEntityType entityType, [NotNull] MemberInfo navigation, [NotNull] IConventionEntityType targetEntityType)
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
            List<(MemberInfo, IConventionEntityType)> referencingNavigationsWithAttribute)
        {
            List<(MemberInfo, IConventionEntityType)> tuplesToRemove = null;
            (MemberInfo, IConventionEntityType)? ambiguousTuple = null;
            foreach (var referencingTuple in referencingNavigationsWithAttribute)
            {
                var inverseTargetEntityType = FindActualEntityType(referencingTuple.Item2);
                if ((inverseTargetEntityType?.Builder.IsIgnored(
                        referencingTuple.Item1.GetSimpleMemberName(), fromDataAnnotation: true)
                    != false))
                {
                    if (tuplesToRemove == null)
                    {
                        tuplesToRemove = new List<(MemberInfo, IConventionEntityType)>();
                    }

                    tuplesToRemove.Add(referencingTuple);
                    continue;
                }

                if (!referencingTuple.Item1.IsSameAs(navigation)
                    || !entityType.IsSameHierarchy(inverseTargetEntityType))
                {
                    ambiguousTuple = referencingTuple;
                    break;
                }
            }

            if (tuplesToRemove != null)
            {
                foreach (var tuple in tuplesToRemove)
                {
                    referencingNavigationsWithAttribute.Remove(tuple);
                }
            }

            return ambiguousTuple;
        }

        private static List<(MemberInfo, IConventionEntityType)> AddInverseNavigation(
            IConventionEntityType entityType, MemberInfo navigation, IConventionEntityType targetEntityType, MemberInfo inverseNavigation)
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
            IConventionEntityType entityType,
            MemberInfo navigation,
            IConventionEntityType targetEntityType)
        {
            var inverseNavigations = GetInverseNavigations(targetEntityType);

            if (inverseNavigations == null)
            {
                return;
            }

            foreach (var inverseNavigationPair in inverseNavigations.Values)
            {
                var (inverseNavigation, referencingNavigationsWithAttribute) = inverseNavigationPair;
                for (var index = 0; index < referencingNavigationsWithAttribute.Count; index++)
                {
                    var referencingTuple = referencingNavigationsWithAttribute[index];
                    if (referencingTuple.Item1.IsSameAs(navigation)
                        && referencingTuple.Item2.ClrType == entityType.ClrType
                        && FindActualEntityType(referencingTuple.Item2) == entityType)
                    {
                        referencingNavigationsWithAttribute.RemoveAt(index);
                        if (referencingNavigationsWithAttribute.Count == 0)
                        {
                            inverseNavigations.Remove(inverseNavigation.Name);
                        }

                        if (referencingNavigationsWithAttribute.Count == 1)
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

                        return;
                    }
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
