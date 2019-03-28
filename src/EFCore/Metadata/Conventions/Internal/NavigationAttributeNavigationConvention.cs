// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public abstract class NavigationAttributeNavigationConvention<TAttribute> : INavigationAddedConvention
        where TAttribute : Attribute
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected NavigationAttributeNavigationConvention([NotNull] IDiagnosticsLogger<DbLoggerCategory.Model> logger)
        {
            Logger = logger;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IDiagnosticsLogger<DbLoggerCategory.Model> Logger { get; }

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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public abstract void ProcessNavigationAdded(
            [NotNull] IConventionRelationshipBuilder relationshipBuilder,
            [NotNull] IConventionNavigation navigation,
            [NotNull] TAttribute attribute,
            [NotNull] IConventionContext<IConventionNavigation> context);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
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
    }
}
