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
        IEntityTypeBaseTypeChangedConvention,
        INavigationAddedConvention,
        IEntityTypeMemberIgnoredConvention
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

        /// <summary>
        ///     Called after an entity type is added to the model.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessEntityTypeAdded(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionContext<IConventionEntityTypeBuilder> context)
        {
            var entityType = entityTypeBuilder.Metadata;
            if (!entityType.HasClrType())
            {
                return;
            }

            foreach (var navigationPropertyInfo in entityType.GetRuntimeProperties().Values.OrderBy(p => p.Name))
            {
                var targetClrType = FindCandidateNavigationWithAttributePropertyType(navigationPropertyInfo);
                if (targetClrType == null)
                {
                    continue;
                }

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

        /// <summary>
        ///     Called after an entity type is ignored.
        /// </summary>
        /// <param name="modelBuilder"> The builder for the model. </param>
        /// <param name="name"> The name of the ignored entity type. </param>
        /// <param name="type"> The ignored entity type. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessEntityTypeIgnored(
            IConventionModelBuilder modelBuilder, string name, Type type, IConventionContext<string> context)
        {
            if (type == null)
            {
                return;
            }

            foreach (var navigationPropertyInfo in type.GetRuntimeProperties().OrderBy(p => p.Name))
            {
                var targetClrType = FindCandidateNavigationWithAttributePropertyType(navigationPropertyInfo);
                if (targetClrType == null)
                {
                    continue;
                }

                type.IsDefined(typeof(TAttribute));
                navigationPropertyInfo.IsDefined(typeof(TAttribute));
                var attributes = navigationPropertyInfo.GetCustomAttributes<TAttribute>(true);
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

        /// <summary>
        ///     Called after the base type of an entity type changes.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type. </param>
        /// <param name="newBaseType"> The new base entity type. </param>
        /// <param name="oldBaseType"> The old base entity type. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
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

            foreach (var navigationPropertyInfo in entityType.GetRuntimeProperties().Values.OrderBy(p => p.Name))
            {
                var targetClrType = FindCandidateNavigationWithAttributePropertyType(navigationPropertyInfo);
                if (targetClrType == null)
                {
                    continue;
                }

                var attributes = navigationPropertyInfo.GetCustomAttributes<TAttribute>(true);
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

        /// <summary>
        ///     Called after a navigation is added to the entity type.
        /// </summary>
        /// <param name="relationshipBuilder"> The builder for the foreign key. </param>
        /// <param name="navigation"> The navigation. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessNavigationAdded(
            IConventionRelationshipBuilder relationshipBuilder,
            IConventionNavigation navigation,
            IConventionContext<IConventionNavigation> context)
        {
            Check.NotNull(relationshipBuilder, nameof(relationshipBuilder));
            Check.NotNull(navigation, nameof(navigation));

            var attributes = GetAttributes<TAttribute>(navigation.DeclaringEntityType, navigation);
            foreach (var attribute in attributes)
            {
                ProcessNavigationAdded(relationshipBuilder, navigation, attribute, context);
                if (((IReadableConventionContext)context).ShouldStopProcessing())
                {
                    break;
                }
            }
        }

        /// <summary>
        ///     Called after an entity type member is ignored.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type. </param>
        /// <param name="name"> The name of the ignored member. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessEntityTypeMemberIgnored(
            IConventionEntityTypeBuilder entityTypeBuilder, string name, IConventionContext<string> context)
        {
            var navigationPropertyInfo =
                entityTypeBuilder.Metadata.GetRuntimeProperties()?.Find(name);
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
            [NotNull] IConventionEntityType entityType, [NotNull] IConventionNavigation navigation)
            where TCustomAttribute : Attribute
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(navigation, nameof(navigation));

            var memberInfo = navigation.GetIdentifyingMemberInfo();
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
        /// <param name="relationshipBuilder"> The builder for the relationship. </param>
        /// <param name="navigation"> The navigation. </param>
        /// <param name="attribute"> The attribute. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessNavigationAdded(
            [NotNull] IConventionRelationshipBuilder relationshipBuilder,
            [NotNull] IConventionNavigation navigation,
            [NotNull] TAttribute attribute,
            [NotNull] IConventionContext<IConventionNavigation> context)
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
    }
}
