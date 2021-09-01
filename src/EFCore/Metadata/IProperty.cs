// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a scalar property of an entity type.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information.
    /// </remarks>
    public interface IProperty : IReadOnlyProperty, IPropertyBase
    {
        /// <summary>
        ///     Gets the type that this property belongs to.
        /// </summary>
        new IEntityType DeclaringEntityType { get; }

        /// <summary>
        ///     Creates an <see cref="IEqualityComparer{T}" /> for values of the given property type.
        /// </summary>
        /// <typeparam name="TProperty"> The property type. </typeparam>
        /// <returns> A new equality comparer. </returns>
        IEqualityComparer<TProperty> CreateKeyEqualityComparer<TProperty>()
        {
            var comparer = GetKeyValueComparer()!;

            return comparer is IEqualityComparer<TProperty> nullableComparer
                ? nullableComparer
                : new NullableComparer<TProperty>(comparer);
        }

        private sealed class NullableComparer<TNullableKey> : IEqualityComparer<TNullableKey>
        {
            private readonly IEqualityComparer _comparer;

            public NullableComparer(IEqualityComparer comparer)
            {
                _comparer = comparer;
            }

            public bool Equals(TNullableKey? x, TNullableKey? y)
                => (x == null && y == null)
                    || (x != null && y != null && _comparer.Equals(x, y));

            public int GetHashCode(TNullableKey obj)
                => obj is null ? 0 : _comparer.GetHashCode(obj);
        }

        /// <summary>
        ///     Finds the first principal property that the given property is constrained by
        ///     if the given property is part of a foreign key.
        /// </summary>
        /// <returns> The first associated principal property, or <see langword="null" /> if none exists. </returns>
        new IProperty? FindFirstPrincipal()
            => (IProperty?)((IReadOnlyProperty)this).FindFirstPrincipal();

        /// <summary>
        ///     Finds the list of principal properties including the given property that the given property is constrained by
        ///     if the given property is part of a foreign key.
        /// </summary>
        /// <returns> The list of all associated principal properties including the given property. </returns>
        new IReadOnlyList<IProperty> GetPrincipals()
            => ((IReadOnlyProperty)this).GetPrincipals().Cast<IProperty>().ToList();

        /// <summary>
        ///     Gets all foreign keys that use this property (including composite foreign keys in which this property
        ///     is included).
        /// </summary>
        /// <returns>
        ///     The foreign keys that use this property.
        /// </returns>
        new IEnumerable<IForeignKey> GetContainingForeignKeys();

        /// <summary>
        ///     Gets all indexes that use this property (including composite indexes in which this property
        ///     is included).
        /// </summary>
        /// <returns>
        ///     The indexes that use this property.
        /// </returns>
        new IEnumerable<IIndex> GetContainingIndexes();

        /// <summary>
        ///     Gets the primary key that uses this property (including a composite primary key in which this property
        ///     is included).
        /// </summary>
        /// <returns>
        ///     The primary that use this property, or <see langword="null" /> if it is not part of the primary key.
        /// </returns>
        new IKey? FindContainingPrimaryKey()
            => (IKey?)((IReadOnlyProperty)this).FindContainingPrimaryKey();

        /// <summary>
        ///     Gets all primary or alternate keys that use this property (including composite keys in which this property
        ///     is included).
        /// </summary>
        /// <returns>
        ///     The primary and alternate keys that use this property.
        /// </returns>
        new IEnumerable<IKey> GetContainingKeys();

        /// <summary>
        ///     Gets the <see cref="ValueComparer" /> for this property.
        /// </summary>
        /// <returns> The comparer. </returns>
        [DebuggerStepThrough]
        new ValueComparer GetValueComparer();

        /// <summary>
        ///     Gets the <see cref="ValueComparer" /> to use with keys for this property.
        /// </summary>
        /// <returns> The comparer. </returns>
        [DebuggerStepThrough]
        new ValueComparer GetKeyValueComparer();
    }
}
