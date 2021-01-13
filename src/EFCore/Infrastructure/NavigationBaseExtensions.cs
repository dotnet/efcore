// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         Extension methods for <see cref="INavigationBase" />.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public static class NavigationBaseExtensions
    {
        /// <summary>
        ///     Calls <see cref="ILazyLoader.SetLoaded" /> for a <see cref="INavigationBase" /> to mark it as loaded
        ///     when a no-tracking query has eagerly loaded this relationship.
        /// </summary>
        /// <param name="navigation"> The navigation loaded. </param>
        /// <param name="entity"> The entity for which the navigation has been loaded. </param>
        public static void SetIsLoadedWhenNoTracking([NotNull] this INavigationBase navigation, [NotNull] object entity)
        {
            Check.NotNull(navigation, nameof(navigation));
            Check.NotNull(entity, nameof(entity));

            var serviceProperties = navigation
                .DeclaringEntityType
                .GetDerivedTypesInclusive()
                .Where(t => t.ClrType.IsInstanceOfType(entity))
                .SelectMany(e => e.GetServiceProperties())
                .Where(p => p.ClrType == typeof(ILazyLoader));

            foreach (var serviceProperty in serviceProperties)
            {
                ((ILazyLoader)serviceProperty.GetGetter().GetClrValue(entity))?.SetLoaded(entity, navigation.Name);
            }
        }
    }
}
