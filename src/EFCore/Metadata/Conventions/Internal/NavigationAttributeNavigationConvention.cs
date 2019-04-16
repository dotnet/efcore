// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public abstract class NavigationAttributeNavigationConvention<TAttribute> : INavigationAddedConvention
        where TAttribute : Attribute
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public abstract InternalRelationshipBuilder Apply(
            [NotNull] InternalRelationshipBuilder relationshipBuilder,
            [NotNull] Navigation navigation, [NotNull] TAttribute attribute);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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
