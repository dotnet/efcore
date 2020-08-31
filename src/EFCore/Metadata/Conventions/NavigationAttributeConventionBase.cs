// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A base type for conventions that perform configuration based on an attribute applied to a navigation.
    /// </summary>
    /// <typeparam name="TAttribute"> The attribute type to look for. </typeparam>
    public abstract class NavigationAttributeConventionBase<TAttribute> :
        IEntityTypeAddedConvention,
        IEntityTypeIgnoredConvention,
        IEntityTypeRemovedConvention,
        IEntityTypeBaseTypeChangedConvention,
        IEntityTypeMemberIgnoredConvention,
        INavigationAddedConvention,
        ISkipNavigationAddedConvention,
        IForeignKeyPrincipalEndChangedConvention
        where TAttribute : Attribute
    {
        /// <summary>
        ///     Creates a new instance of <see cref="NavigationAttributeConventionBase{TAttribute}" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        protected NavigationAttributeConventionBase([NotNull] ProviderConventionSetBuilderDependencies dependencies)
        {
            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

        /// <inheritdoc />
        public virtual void ProcessEntityTypeAdded(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionContext<IConventionEntityTypeBuilder> context)
        {
            var entityType = entityTypeBuilder.Metadata;
            if (!entityType.HasClrType())
            {
                return;
            }

            var navigations = GetNavigationsWithAttribute(entityType);
            if (navigations == null)
            {
                return;
            }

            foreach (var navigationTuple in navigations)
            {
                var (navigationPropertyInfo, targetClrType) = navigationTuple;
                var attributes = navigationPropertyInfo.GetCustomAttributes<TAttribute>(inherit: true);
                foreach (var attribute in attributes)
                {
                    ProcessEntityTypeAdded(entityTypeBuilder, navigationPropertyInfo, targetClrType, attribute, context);
                    if (((ConventionContext<IConventionEntityTypeBuilder>)context).ShouldStopProcessing())
                    {
                        return;
                    }
                }
            }
        }

        /// <inheritdoc />
        public virtual void ProcessEntityTypeIgnored(
            IConventionModelBuilder modelBuilder,
            string name,
            Type type,
            IConventionContext<string> context)
        {
            if (type == null)
            {
                return;
            }

            var navigations = new List<(PropertyInfo, Type)>();
            foreach (var navigationPropertyInfo in type.GetRuntimeProperties())
            {
                var targetClrType = FindCandidateNavigationWithAttributePropertyType(navigationPropertyInfo);
                if (targetClrType == null)
                {
                    continue;
                }

                navigations.Add((navigationPropertyInfo, targetClrType));
            }

            if (navigations.Count == 0)
            {
                return;
            }

            Sort(navigations);

            foreach (var navigationTuple in navigations)
            {
                var (navigationPropertyInfo, targetClrType) = navigationTuple;
                var attributes = navigationPropertyInfo.GetCustomAttributes<TAttribute>(inherit: true);
                foreach (var attribute in attributes)
                {
                    ProcessEntityTypeIgnored(modelBuilder, type, navigationPropertyInfo, targetClrType, attribute, context);
                    if (((ConventionContext<string>)context).ShouldStopProcessing())
                    {
                        return;
                    }
                }
            }
        }

        /// <inheritdoc />
        public virtual void ProcessEntityTypeRemoved(
            IConventionModelBuilder modelBuilder,
            IConventionEntityType entityType,
            IConventionContext<IConventionEntityType> context)
        {
            var type = entityType.ClrType;
            if (type == null)
            {
                return;
            }

            var navigations = GetNavigationsWithAttribute(entityType);
            if (navigations == null)
            {
                return;
            }

            foreach (var navigationTuple in navigations)
            {
                var (navigationPropertyInfo, targetClrType) = navigationTuple;
                var attributes = navigationPropertyInfo.GetCustomAttributes<TAttribute>(inherit: true);
                foreach (var attribute in attributes)
                {
                    ProcessEntityTypeRemoved(modelBuilder, type, navigationPropertyInfo, targetClrType, attribute, context);
                    if (((ConventionContext<IConventionEntityType>)context).ShouldStopProcessing())
                    {
                        return;
                    }
                }
            }
        }

        /// <inheritdoc />
        public virtual void ProcessEntityTypeBaseTypeChanged(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionEntityType newBaseType,
            IConventionEntityType oldBaseType,
            IConventionContext<IConventionEntityType> context)
        {
            var entityType = entityTypeBuilder.Metadata;
            if (!entityType.HasClrType()
                || entityTypeBuilder.Metadata.BaseType != newBaseType)
            {
                return;
            }

            var navigations = GetNavigationsWithAttribute(entityType);
            if (navigations == null)
            {
                return;
            }

            foreach (var navigationTuple in navigations)
            {
                var (navigationPropertyInfo, targetClrType) = navigationTuple;
                var attributes = navigationPropertyInfo.GetCustomAttributes<TAttribute>(inherit: true);
                foreach (var attribute in attributes)
                {
                    ProcessEntityTypeBaseTypeChanged(
                        entityTypeBuilder, newBaseType, oldBaseType, navigationPropertyInfo, targetClrType, attribute, context);
                    if (((ConventionContext<IConventionEntityType>)context).ShouldStopProcessing())
                    {
                        return;
                    }
                }
            }
        }

        private List<(PropertyInfo, Type)> GetNavigationsWithAttribute(IConventionEntityType entityType)
        {
            var navigations = new List<(PropertyInfo, Type)>();
            foreach (var navigationPropertyInfo in entityType.GetRuntimeProperties().Values)
            {
                var targetClrType = FindCandidateNavigationWithAttributePropertyType(navigationPropertyInfo);
                if (targetClrType == null)
                {
                    continue;
                }

                navigations.Add((navigationPropertyInfo, targetClrType));
            }

            if (navigations.Count == 0)
            {
                return null;
            }

            Sort(navigations);

            return navigations;
        }

        private static void Sort(List<(PropertyInfo, Type)> navigations)
            => navigations.Sort((x, y) => StringComparer.Ordinal.Compare(x.Item1.Name, y.Item1.Name));

        /// <inheritdoc />
        public virtual void ProcessNavigationAdded(
            IConventionNavigationBuilder navigationBuilder,
            IConventionContext<IConventionNavigationBuilder> context)
        {
            var navigation = navigationBuilder.Metadata;
            var attributes = GetAttributes<TAttribute>(navigation.DeclaringEntityType, navigation);
            foreach (var attribute in attributes)
            {
                ProcessNavigationAdded(navigationBuilder, attribute, context);
                if (((IReadableConventionContext)context).ShouldStopProcessing())
                {
                    break;
                }
            }
        }

        /// <inheritdoc />
        public virtual void ProcessSkipNavigationAdded(
            IConventionSkipNavigationBuilder skipNavigationBuilder,
            IConventionContext<IConventionSkipNavigationBuilder> context)
        {
            var skipNavigation = skipNavigationBuilder.Metadata;
            var attributes = GetAttributes<TAttribute>(skipNavigation.DeclaringEntityType, skipNavigation);
            foreach (var attribute in attributes)
            {
                ProcessSkipNavigationAdded(skipNavigationBuilder, attribute, context);
                if (((IReadableConventionContext)context).ShouldStopProcessing())
                {
                    break;
                }
            }
        }

        /// <inheritdoc />
        public virtual void ProcessForeignKeyPrincipalEndChanged(
            IConventionForeignKeyBuilder relationshipBuilder,
            IConventionContext<IConventionForeignKeyBuilder> context)
        {
            var fk = relationshipBuilder.Metadata;
            var dependentToPrincipalAttributes = fk.DependentToPrincipal == null
                ? null
                : GetAttributes<TAttribute>(fk.DeclaringEntityType, fk.DependentToPrincipal);
            var principalToDependentAttributes = fk.PrincipalToDependent == null
                ? null
                : GetAttributes<TAttribute>(fk.PrincipalEntityType, fk.PrincipalToDependent);
            ProcessForeignKeyPrincipalEndChanged(
                relationshipBuilder, dependentToPrincipalAttributes, principalToDependentAttributes, context);
        }

        /// <inheritdoc />
        public virtual void ProcessEntityTypeMemberIgnored(
            IConventionEntityTypeBuilder entityTypeBuilder,
            string name,
            IConventionContext<string> context)
        {
            var navigationPropertyInfo = entityTypeBuilder.Metadata.GetRuntimeProperties()?.Find(name);
            if (navigationPropertyInfo == null)
            {
                return;
            }

            var targetClrType = FindCandidateNavigationWithAttributePropertyType(navigationPropertyInfo);
            if (targetClrType == null)
            {
                return;
            }

            var attributes = navigationPropertyInfo.GetCustomAttributes<TAttribute>(true);
            foreach (var attribute in attributes)
            {
                ProcessEntityTypeMemberIgnored(entityTypeBuilder, navigationPropertyInfo, targetClrType, attribute, context);
                if (((ConventionContext<string>)context).ShouldStopProcessing())
                {
                    return;
                }
            }
        }

        private Type FindCandidateNavigationWithAttributePropertyType([NotNull] PropertyInfo propertyInfo)
        {
            var targetClrType = Dependencies.MemberClassifier.FindCandidateNavigationPropertyType(propertyInfo);
            return targetClrType == null
                || !Attribute.IsDefined(propertyInfo, typeof(TAttribute), inherit: true)
                    ? null
                    : targetClrType;
        }

        /// <summary>
        ///     Returns the attributes applied to the given navigation.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="navigation"> The navigation. </param>
        /// <typeparam name="TCustomAttribute"> The attribute type to look for. </typeparam>
        /// <returns> The attributes applied to the given navigation. </returns>
        protected static IEnumerable<TCustomAttribute> GetAttributes<TCustomAttribute>(
            [NotNull] IConventionEntityType entityType,
            [NotNull] IConventionNavigation navigation)
            where TCustomAttribute : Attribute
            => GetAttributes<TCustomAttribute>(entityType, navigation.GetIdentifyingMemberInfo());

        /// <summary>
        ///     Returns the attributes applied to the given skip navigation.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="skipNavigation"> The skip navigation. </param>
        /// <typeparam name="TCustomAttribute"> The attribute type to look for. </typeparam>
        /// <returns> The attributes applied to the given skip navigation. </returns>
        protected static IEnumerable<TCustomAttribute> GetAttributes<TCustomAttribute>(
            [NotNull] IConventionEntityType entityType,
            [NotNull] IConventionSkipNavigation skipNavigation)
            where TCustomAttribute : Attribute
            => GetAttributes<TCustomAttribute>(entityType, skipNavigation.GetIdentifyingMemberInfo());

        private static IEnumerable<TCustomAttribute> GetAttributes<TCustomAttribute>(
            [NotNull] IConventionEntityType entityType,
            [NotNull] MemberInfo memberInfo)
            where TCustomAttribute : Attribute
        {
            if (!entityType.HasClrType()
                || memberInfo == null)
            {
                return Enumerable.Empty<TCustomAttribute>();
            }

            return Attribute.IsDefined(memberInfo, typeof(TCustomAttribute), inherit: true)
                ? memberInfo.GetCustomAttributes<TCustomAttribute>(true)
                : Enumerable.Empty<TCustomAttribute>();
        }

        /// <summary>
        ///     Called for every navigation property that has an attribute after an entity type is added to the model.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type. </param>
        /// <param name="navigationMemberInfo"> The navigation member info. </param>
        /// <param name="targetClrType"> The CLR type of the target entity type</param>
        /// <param name="attribute"> The attribute. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessEntityTypeAdded(
            [NotNull] IConventionEntityTypeBuilder entityTypeBuilder,
            [NotNull] MemberInfo navigationMemberInfo,
            [NotNull] Type targetClrType,
            [NotNull] TAttribute attribute,
            [NotNull] IConventionContext<IConventionEntityTypeBuilder> context)
            => throw new NotImplementedException();

        /// <summary>
        ///     Called for every navigation property that has an attribute after an entity type is ignored.
        /// </summary>
        /// <param name="modelBuilder"> The builder for the model. </param>
        /// <param name="type"> The ignored entity type. </param>
        /// <param name="navigationMemberInfo"> The navigation member info. </param>
        /// <param name="targetClrType"> The CLR type of the target entity type. </param>
        /// <param name="attribute"> The attribute. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessEntityTypeIgnored(
            [NotNull] IConventionModelBuilder modelBuilder,
            [NotNull] Type type,
            [NotNull] MemberInfo navigationMemberInfo,
            [NotNull] Type targetClrType,
            [NotNull] TAttribute attribute,
            [NotNull] IConventionContext<string> context)
            => throw new NotImplementedException();

        /// <summary>
        ///     Called for every navigation property that has an attribute after an entity type is removed.
        /// </summary>
        /// <param name="modelBuilder"> The builder for the model. </param>
        /// <param name="type"> The ignored entity type. </param>
        /// <param name="navigationMemberInfo"> The navigation member info. </param>
        /// <param name="targetClrType"> The CLR type of the target entity type. </param>
        /// <param name="attribute"> The attribute. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessEntityTypeRemoved(
            [NotNull] IConventionModelBuilder modelBuilder,
            [NotNull] Type type,
            [NotNull] MemberInfo navigationMemberInfo,
            [NotNull] Type targetClrType,
            [NotNull] TAttribute attribute,
            [NotNull] IConventionContext<IConventionEntityType> context)
            => throw new NotImplementedException();

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
        public virtual void ProcessEntityTypeBaseTypeChanged(
            [NotNull] IConventionEntityTypeBuilder entityTypeBuilder,
            [CanBeNull] IConventionEntityType newBaseType,
            [CanBeNull] IConventionEntityType oldBaseType,
            [NotNull] MemberInfo navigationMemberInfo,
            [NotNull] Type targetClrType,
            [NotNull] TAttribute attribute,
            [NotNull] IConventionContext<IConventionEntityType> context)
            => throw new NotImplementedException();

        /// <summary>
        ///     Called after a navigation property that has an attribute is added to an entity type.
        /// </summary>
        /// <param name="navigationBuilder"> The builder for the navigation. </param>
        /// <param name="attribute"> The attribute. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessNavigationAdded(
            [NotNull] IConventionNavigationBuilder navigationBuilder,
            [NotNull] TAttribute attribute,
            [NotNull] IConventionContext<IConventionNavigationBuilder> context)
            => throw new NotImplementedException();

        /// <summary>
        ///     Called after a skip navigation property that has an attribute is added to an entity type.
        /// </summary>
        /// <param name="skipNavigationBuilder"> The builder for the navigation. </param>
        /// <param name="attribute"> The attribute. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessSkipNavigationAdded(
            [NotNull] IConventionSkipNavigationBuilder skipNavigationBuilder,
            [NotNull] TAttribute attribute,
            [NotNull] IConventionContext<IConventionSkipNavigationBuilder> context)
            => throw new NotImplementedException();

        /// <summary>
        ///     Called after a navigation property that has an attribute is ignored.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type. </param>
        /// <param name="navigationMemberInfo"> The navigation member info. </param>
        /// <param name="targetClrType"> The CLR type of the target entity type. </param>
        /// <param name="attribute"> The attribute. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessEntityTypeMemberIgnored(
            [NotNull] IConventionEntityTypeBuilder entityTypeBuilder,
            [NotNull] MemberInfo navigationMemberInfo,
            [NotNull] Type targetClrType,
            [NotNull] TAttribute attribute,
            [NotNull] IConventionContext<string> context)
            => throw new NotImplementedException();

        /// <summary>
        ///     Called after the principal end of a foreign key is changed.
        /// </summary>
        /// <param name="relationshipBuilder"> The builder for the foreign key. </param>
        /// <param name="dependentToPrincipalAttributes"> The attributes on the dependent to principal navigation. </param>
        /// <param name="principalToDependentAttributes"> The attributes on the principal to dependent navigation. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessForeignKeyPrincipalEndChanged(
            [NotNull] IConventionForeignKeyBuilder relationshipBuilder,
            [CanBeNull] IEnumerable<TAttribute> dependentToPrincipalAttributes,
            [CanBeNull] IEnumerable<TAttribute> principalToDependentAttributes,
            [NotNull] IConventionContext<IConventionForeignKeyBuilder> context)
            => throw new NotImplementedException();
    }
}
