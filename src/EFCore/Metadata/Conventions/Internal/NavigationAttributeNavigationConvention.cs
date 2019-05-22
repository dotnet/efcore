// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalRelationshipBuilder Apply(
            InternalRelationshipBuilder relationshipBuilder,
            Navigation navigation)
        {
            Check.NotNull(relationshipBuilder, nameof(relationshipBuilder));
            Check.NotNull(navigation, nameof(navigation));

            var attributes = GetAttributes<TAttribute>(navigation.DeclaringEntityType, navigation.Name);
            foreach (var attribute in attributes)
            {
                relationshipBuilder = Apply(relationshipBuilder, navigation, attribute);
                if (relationshipBuilder == null)
                {
                    break;
                }
            }

            return relationshipBuilder;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public abstract InternalRelationshipBuilder Apply(
            [NotNull] InternalRelationshipBuilder relationshipBuilder,
            [NotNull] Navigation navigation, [NotNull] TAttribute attribute);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected static IEnumerable<TCustomAttribute> GetAttributes<TCustomAttribute>(
            [NotNull] EntityType entityType, [NotNull] string propertyName)
            where TCustomAttribute : Attribute
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(propertyName, nameof(propertyName));

            if (!entityType.HasClrType())
            {
                return Enumerable.Empty<TCustomAttribute>();
            }

            var property = entityType.GetRuntimeProperties().Find(propertyName);
            return property != null
                   && Attribute.IsDefined(property, typeof(TCustomAttribute), inherit: true)
                ? property.GetCustomAttributes<TCustomAttribute>(true)
                : Enumerable.Empty<TCustomAttribute>();
        }
    }
}
