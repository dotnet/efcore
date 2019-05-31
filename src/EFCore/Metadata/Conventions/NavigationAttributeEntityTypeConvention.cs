// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public abstract class NavigationAttributeEntityTypeConvention<TAttribute> :
        NavigationAttributeNavigationConvention<TAttribute>,
        IEntityTypeAddedConvention,
        IEntityTypeIgnoredConvention,
        IEntityTypeBaseTypeChangedConvention,
        IEntityTypeMemberIgnoredConvention
        where TAttribute : Attribute
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected NavigationAttributeEntityTypeConvention([NotNull] ProviderConventionSetBuilderDependencies dependencies)
            : base(dependencies)
        {
        }

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

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual Type FindCandidateNavigationPropertyType([NotNull] PropertyInfo propertyInfo)
            => Dependencies.MemberClassifier.FindCandidateNavigationPropertyType(propertyInfo);

        private Type FindCandidateNavigationWithAttributePropertyType([NotNull] PropertyInfo propertyInfo)
        {
            var targetClrType = Dependencies.MemberClassifier.FindCandidateNavigationPropertyType(propertyInfo);
            return targetClrType == null
                   || !Attribute.IsDefined(propertyInfo, typeof(TAttribute), inherit: true)
                ? null
                : targetClrType;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void ProcessEntityTypeAdded(
            [NotNull] IConventionEntityTypeBuilder entityTypeBuilder,
            [NotNull] MemberInfo navigationMemberInfo,
            [NotNull] Type targetClrType,
            [NotNull] TAttribute attribute,
            [NotNull] IConventionContext<IConventionEntityTypeBuilder> context)
            => throw new NotImplementedException();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void ProcessEntityTypeIgnored(
            [NotNull] IConventionModelBuilder modelBuilder,
            [NotNull] Type type,
            [NotNull] MemberInfo navigationMemberInfo,
            [NotNull] Type targetClrType,
            [NotNull] TAttribute attribute,
            [NotNull] IConventionContext<string> context)
            => throw new NotImplementedException();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void ProcessNavigationAdded(
            IConventionRelationshipBuilder relationshipBuilder,
            IConventionNavigation navigation,
            TAttribute attribute,
            IConventionContext<IConventionNavigation> context)
            => throw new NotImplementedException();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void ProcessEntityTypeMemberIgnored(
            [NotNull] IConventionEntityTypeBuilder entityTypeBuilder,
            [NotNull] MemberInfo navigationMemberInfo,
            [NotNull] Type targetClrType,
            [NotNull] TAttribute attribute,
            [NotNull] IConventionContext<string> context)
            => throw new NotImplementedException();
    }
}
