// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a scalar property of an entity type.
    /// </summary>
    public interface IProperty : IReadOnlyProperty, IPropertyBase
    {
        /// <summary>
        ///     Gets the type that this property belongs to.
        /// </summary>
        new IEntityType DeclaringEntityType { get; }

        /// <summary>
        ///     Finds the first principal property that the given property is constrained by
        ///     if the given property is part of a foreign key.
        /// </summary>
        /// <returns> The first associated principal property, or <see langword="null" /> if none exists. </returns>
        IProperty? FindFirstPrincipal()
            => (IProperty?)((IReadOnlyProperty)this).FindFirstPrincipal();

        /// <summary>
        ///     Finds the list of principal properties including the given property that the given property is constrained by
        ///     if the given property is part of a foreign key.
        /// </summary>
        /// <returns> The list of all associated principal properties including the given property. </returns>
        IReadOnlyList<IProperty> FindPrincipals()
            => ((IReadOnlyProperty)this).FindPrincipals().Cast<IProperty>().ToList();

        /// <summary>
        ///     Gets all foreign keys that use this property (including composite foreign keys in which this property
        ///     is included).
        /// </summary>
        /// <returns>
        ///     The foreign keys that use this property.
        /// </returns>
        IEnumerable<IForeignKey> GetContainingForeignKeys();

        /// <summary>
        ///     Gets all indexes that use this property (including composite indexes in which this property
        ///     is included).
        /// </summary>
        /// <returns>
        ///     The indexes that use this property.
        /// </returns>
        IEnumerable<IIndex> GetContainingIndexes();

        /// <summary>
        ///     Gets the primary key that uses this property (including a composite primary key in which this property
        ///     is included).
        /// </summary>
        /// <returns>
        ///     The primary that use this property, or <see langword="null" /> if it is not part of the primary key.
        /// </returns>
        IKey? FindContainingPrimaryKey()
            => (IKey?)((IReadOnlyProperty)this).FindContainingPrimaryKey();

        /// <summary>
        ///     Gets all primary or alternate keys that use this property (including composite keys in which this property
        ///     is included).
        /// </summary>
        /// <returns>
        ///     The primary and alternate keys that use this property.
        /// </returns>
        IEnumerable<IKey> GetContainingKeys();
    }
}
