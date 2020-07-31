// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     <para>
    ///         Represents a relationship where a foreign key property(s) in a dependent entity type
    ///         reference a corresponding primary or alternate key in a principal entity type.
    ///     </para>
    ///     <para>
    ///         This interface is used during model creation and allows the metadata to be modified.
    ///         Once the model is built, <see cref="IForeignKey" /> represents a read-only view of the same metadata.
    ///     </para>
    /// </summary>
    public interface IMutableForeignKey : IForeignKey, IMutableAnnotatable
    {
        /// <summary>
        ///     Gets the foreign key properties in the dependent entity.
        /// </summary>
        new IReadOnlyList<IMutableProperty> Properties { get; }

        /// <summary>
        ///     Gets the primary or alternate key that the relationship targets.
        /// </summary>
        new IMutableKey PrincipalKey { get; }

        /// <summary>
        ///     Gets the dependent entity type. This may be different from the type that <see cref="Properties" />
        ///     are defined on when the relationship is defined a derived type in an inheritance hierarchy (since the properties
        ///     may be defined on a base type).
        /// </summary>
        new IMutableEntityType DeclaringEntityType { get; }

        /// <summary>
        ///     Gets the principal entity type that this relationship targets. This may be different from the type that
        ///     <see cref="PrincipalKey" /> is defined on when the relationship targets a derived type in an inheritance
        ///     hierarchy (since the key is defined on the base type of the hierarchy).
        /// </summary>
        new IMutableEntityType PrincipalEntityType { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether the values assigned to the foreign key properties are unique.
        /// </summary>
        new bool IsUnique { get; set; }

        /// <summary>
        ///     Sets a value indicating whether the principal entity is required.
        ///     If <see langword="true" />, the dependent entity must always be assigned to a valid principal entity.
        /// </summary>
        new bool IsRequired { get; set; }

        /// <summary>
        ///     Sets a value indicating whether the dependent entity is required.
        ///     If <see langword="true" />, the principal entity must always have a valid dependent entity assigned.
        /// </summary>
        new bool IsRequiredDependent { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this relationship defines ownership. If true, the dependent entity must always be
        ///     accessed via the navigation from the principal entity.
        /// </summary>
        new bool IsOwnership { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating how a delete operation is applied to dependent entities in the relationship when the
        ///     principal is deleted or the relationship is severed.
        /// </summary>
        new DeleteBehavior DeleteBehavior { get; set; }

        /// <summary>
        ///     Gets the navigation property on the dependent entity type that points to the principal entity.
        /// </summary>
        new IMutableNavigation DependentToPrincipal { get; }

        /// <summary>
        ///     Gets the navigation property on the principal entity type that points to the dependent entity.
        /// </summary>
        new IMutableNavigation PrincipalToDependent { get; }

        /// <summary>
        ///     Sets the foreign key properties and that target principal key.
        /// </summary>
        /// <param name="properties"> Foreign key properties in the dependent entity. </param>
        /// <param name="principalKey"> The primary or alternate key to target. </param>
        void SetProperties([NotNull] IReadOnlyList<IMutableProperty> properties, [NotNull] IMutableKey principalKey);

        /// <summary>
        ///     Sets the navigation property on the dependent entity type that points to the principal entity.
        /// </summary>
        /// <param name="name">
        ///     The name of the navigation property on the dependent type. Passing <see langword="null" /> will result in there being
        ///     no navigation property defined.
        /// </param>
        /// <returns> The newly set navigation property. </returns>
        IMutableNavigation SetDependentToPrincipal([CanBeNull] string name);

        /// <summary>
        ///     Sets the navigation property on the dependent entity type that points to the principal entity.
        /// </summary>
        /// <param name="property">
        ///     The navigation property on the dependent type. Passing <see langword="null" /> will result in there being
        ///     no navigation property defined.
        /// </param>
        /// <returns> The newly set navigation property. </returns>
        IMutableNavigation SetDependentToPrincipal([CanBeNull] MemberInfo property);

        /// <summary>
        ///     Sets the navigation property on the dependent entity type that points to the principal entity.
        /// </summary>
        /// <param name="name">
        ///     The name of the navigation property on the dependent type. Passing <see langword="null" /> will result in there being
        ///     no navigation property defined.
        /// </param>
        /// <returns> The newly created navigation property. </returns>
        [Obsolete("Use SetDependentToPrincipal")]
        IMutableNavigation HasDependentToPrincipal([CanBeNull] string name)
            => SetDependentToPrincipal(name);

        /// <summary>
        ///     Sets the navigation property on the dependent entity type that points to the principal entity.
        /// </summary>
        /// <param name="property">
        ///     The navigation property on the dependent type. Passing <see langword="null" /> will result in there being
        ///     no navigation property defined.
        /// </param>
        /// <returns> The newly created navigation property. </returns>
        [Obsolete("Use SetDependentToPrincipal")]
        IMutableNavigation HasDependentToPrincipal([CanBeNull] MemberInfo property)
            => SetDependentToPrincipal(property);

        /// <summary>
        ///     Sets the navigation property on the principal entity type that points to the dependent entity.
        /// </summary>
        /// <param name="name">
        ///     The name of the navigation property on the principal type. Passing <see langword="null" /> will result in there being
        ///     no navigation property defined.
        /// </param>
        /// <returns> The newly set navigation property. </returns>
        IMutableNavigation SetPrincipalToDependent([CanBeNull] string name);

        /// <summary>
        ///     Sets the navigation property on the principal entity type that points to the dependent entity.
        /// </summary>
        /// <param name="property">
        ///     The name of the navigation property on the principal type. Passing <see langword="null" /> will result in there being
        ///     no navigation property defined.
        /// </param>
        /// <returns> The newly set navigation property. </returns>
        IMutableNavigation SetPrincipalToDependent([CanBeNull] MemberInfo property);

        /// <summary>
        ///     Sets the navigation property on the principal entity type that points to the dependent entity.
        /// </summary>
        /// <param name="name">
        ///     The name of the navigation property on the principal type. Passing <see langword="null" /> will result in there being
        ///     no navigation property defined.
        /// </param>
        /// <returns> The newly created navigation property. </returns>
        [Obsolete("Use SetPrincipalToDependent")]
        IMutableNavigation HasPrincipalToDependent([CanBeNull] string name)
            => SetPrincipalToDependent(name);

        /// <summary>
        ///     Sets the navigation property on the principal entity type that points to the dependent entity.
        /// </summary>
        /// <param name="property">
        ///     The name of the navigation property on the principal type. Passing <see langword="null" /> will result in there being
        ///     no navigation property defined.
        /// </param>
        /// <returns> The newly created navigation property. </returns>
        [Obsolete("Use SetPrincipalToDependent")]
        IMutableNavigation HasPrincipalToDependent([CanBeNull] MemberInfo property)
            => SetPrincipalToDependent(property);

        /// <summary>
        ///     Gets all skip navigations using this foreign key.
        /// </summary>
        /// <returns> The skip navigations using this foreign key. </returns>
        new IEnumerable<IMutableSkipNavigation> GetReferencingSkipNavigations()
            => ((IForeignKey)this).GetReferencingSkipNavigations().Cast<IMutableSkipNavigation>();
    }
}
