// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a navigation property which can be used to navigate a relationship.
    /// </summary>
    public interface INavigationBase : IReadOnlyNavigationBase, IPropertyBase
    {
        /// <summary>
        ///     Gets the entity type that this navigation property belongs to.
        /// </summary>
        new IEntityType DeclaringEntityType
        {
            [DebuggerStepThrough]
            get => (IEntityType)((IReadOnlyNavigationBase)this).DeclaringEntityType;
        }

        /// <summary>
        ///     Gets the entity type that this navigation property will hold an instance(s) of.
        /// </summary>
        new IEntityType TargetEntityType
        {
            [DebuggerStepThrough]
            get => (IEntityType)((IReadOnlyNavigationBase)this).TargetEntityType;
        }

        /// <summary>
        ///     Gets the inverse navigation.
        /// </summary>
        new INavigationBase? Inverse
        {
            [DebuggerStepThrough]
            get => (INavigationBase?)((IReadOnlyNavigationBase)this).Inverse;
        }

        /// <summary>
        ///     Calls <see cref="ILazyLoader.SetLoaded" /> for a <see cref="INavigationBase" /> to mark it as loaded
        ///     when a no-tracking query has eagerly loaded this relationship.
        /// </summary>
        /// <param name="entity"> The entity for which the navigation has been loaded. </param>
        void SetIsLoadedWhenNoTracking([NotNull] object entity)
        {
            Check.NotNull(entity, nameof(entity));

            var serviceProperties = DeclaringEntityType
                .GetDerivedTypesInclusive()
                .Where(t => t.ClrType.IsInstanceOfType(entity))
                .SelectMany(e => e.GetServiceProperties())
                .Where(p => p.ClrType == typeof(ILazyLoader));

            foreach (var serviceProperty in serviceProperties)
            {
                ((ILazyLoader?)serviceProperty.GetGetter().GetClrValue(entity))?.SetLoaded(entity, Name);
            }
        }
    }
}
