// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a relationship where a foreign key composed of properties on the dependent entity type
    ///     references a corresponding primary or alternate key on the principal entity type.
    /// </summary>
    public interface IReadOnlyForeignKey : IReadOnlyAnnotatable
    {
        /// <summary>
        ///     Gets the dependent entity type. This may be different from the type that <see cref="Properties" />
        ///     are defined on when the relationship is defined a derived type in an inheritance hierarchy (since the properties
        ///     may be defined on a base type).
        /// </summary>
        IReadOnlyEntityType DeclaringEntityType { get; }

        /// <summary>
        ///     Gets the foreign key properties in the dependent entity.
        /// </summary>
        IReadOnlyList<IReadOnlyProperty> Properties { get; }

        /// <summary>
        ///     Gets the principal entity type that this relationship targets. This may be different from the type that
        ///     <see cref="PrincipalKey" /> is defined on when the relationship targets a derived type in an inheritance
        ///     hierarchy (since the key is defined on the base type of the hierarchy).
        /// </summary>
        IReadOnlyEntityType PrincipalEntityType { get; }

        /// <summary>
        ///     Gets the primary or alternate key that the relationship targets.
        /// </summary>
        IReadOnlyKey PrincipalKey { get; }

        /// <summary>
        ///     Gets the navigation property on the dependent entity type that points to the principal entity.
        /// </summary>
        IReadOnlyNavigation? DependentToPrincipal { get; }

        /// <summary>
        ///     Gets the navigation property on the principal entity type that points to the dependent entity.
        /// </summary>
        IReadOnlyNavigation? PrincipalToDependent { get; }

        /// <summary>
        ///     Gets a value indicating whether the values assigned to the foreign key properties are unique.
        /// </summary>
        bool IsUnique { get; }

        /// <summary>
        ///     Gets a value indicating whether the principal entity is required.
        ///     If <see langword="true" />, the dependent entity must always be assigned to a valid principal entity.
        /// </summary>
        bool IsRequired { get; }

        /// <summary>
        ///     Gets a value indicating whether the dependent entity is required.
        ///     If <see langword="true" />, the principal entity must always have a valid dependent entity assigned.
        /// </summary>
        bool IsRequiredDependent { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether this relationship defines an ownership.
        ///     If <see langword="true" />, the dependent entity must always be accessed via the navigation from the principal entity.
        /// </summary>
        bool IsOwnership { get; }

        /// <summary>
        ///     Gets a value indicating how a delete operation is applied to dependent entities in the relationship when the
        ///     principal is deleted or the relationship is severed.
        /// </summary>
        DeleteBehavior DeleteBehavior { get; }

        /// <summary>
        ///     Gets the skip navigations using this foreign key.
        /// </summary>
        /// <returns> The skip navigations using this foreign key. </returns>
        IEnumerable<IReadOnlySkipNavigation> GetReferencingSkipNavigations()
            => PrincipalEntityType.GetSkipNavigations().Where(n => !n.IsOnDependent && n.ForeignKey == this)
                .Concat(DeclaringEntityType.GetSkipNavigations().Where(n => n.IsOnDependent && n.ForeignKey == this));
    }
}
