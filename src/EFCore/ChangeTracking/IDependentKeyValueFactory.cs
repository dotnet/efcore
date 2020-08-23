// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;

namespace Microsoft.EntityFrameworkCore.ChangeTracking
{
    /// <summary>
    ///     <para>
    ///         A factory for key values based on the foreign key values taken from various forms of entity data.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <typeparam name="TKey"> The generic type of the key. </typeparam>
    public interface IDependentKeyValueFactory<TKey>
    {
        /// <summary>
        ///     Attempts to create a key instance using foreign key values from the given <see cref="ValueBuffer" />.
        /// </summary>
        /// <param name="valueBuffer"> The value buffer representing the entity instance. </param>
        /// <param name="key"> The key instance. </param>
        /// <returns> <see langword="true" /> if the key instance was created; <see langword="false" /> otherwise. </returns>
        [ContractAnnotation("=>true, key:notnull; =>false, key:null")]
        bool TryCreateFromBuffer(in ValueBuffer valueBuffer, out TKey key);

        /// <summary>
        ///     Attempts to create a key instance using foreign key values from the given <see cref="IUpdateEntry" />.
        /// </summary>
        /// <param name="entry"> The entry tracking an entity instance. </param>
        /// <param name="key"> The key instance. </param>
        /// <returns> <see langword="true" /> if the key instance was created; <see langword="false" /> otherwise. </returns>
        [ContractAnnotation("=>true, key:notnull; =>false, key:null")]
        bool TryCreateFromCurrentValues([NotNull] IUpdateEntry entry, out TKey key);

        /// <summary>
        ///     Attempts to create a key instance from the given <see cref="IUpdateEntry" />
        ///     using foreign key values that were set before any store-generated values were propagated.
        /// </summary>
        /// <param name="entry"> The entry tracking an entity instance. </param>
        /// <param name="key"> The key instance. </param>
        /// <returns> <see langword="true" /> if the key instance was created; <see langword="false" /> otherwise. </returns>
        [ContractAnnotation("=>true, key:notnull; =>false, key:null")]
        bool TryCreateFromPreStoreGeneratedCurrentValues([NotNull] IUpdateEntry entry, out TKey key);

        /// <summary>
        ///     Attempts to create a key instance using original foreign key values from the given <see cref="IUpdateEntry" />.
        /// </summary>
        /// <param name="entry"> The entry tracking an entity instance. </param>
        /// <param name="key"> The key instance. </param>
        /// <returns> <see langword="true" /> if the key instance was created; <see langword="false" /> otherwise. </returns>
        [ContractAnnotation("=>true, key:notnull; =>false, key:null")]
        bool TryCreateFromOriginalValues([NotNull] IUpdateEntry entry, out TKey key);

        /// <summary>
        ///     Attempts to create a key instance from the given <see cref="IUpdateEntry" />
        ///     using foreign key values from the previously known relationship.
        /// </summary>
        /// <param name="entry"> The entry tracking an entity instance. </param>
        /// <param name="key"> The key instance. </param>
        /// <returns> <see langword="true" /> if the key instance was created; <see langword="false" /> otherwise. </returns>
        [ContractAnnotation("=>true, key:notnull; =>false, key:null")]
        bool TryCreateFromRelationshipSnapshot([NotNull] IUpdateEntry entry, out TKey key);

        /// <summary>
        ///     The <see cref="IEqualityComparer{T}" /> to use for comparing key instances.
        /// </summary>
        IEqualityComparer<TKey> EqualityComparer { get; }
    }
}
