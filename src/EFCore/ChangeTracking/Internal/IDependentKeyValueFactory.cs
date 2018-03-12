// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public interface IDependentKeyValueFactory<TKey>
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [ContractAnnotation("=>true, key:notnull; =>false, key:null")]
        bool TryCreateFromBuffer(in ValueBuffer valueBuffer, out TKey key);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [ContractAnnotation("=>true, key:notnull; =>false, key:null")]
        bool TryCreateFromCurrentValues([NotNull] InternalEntityEntry entry, out TKey key);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [ContractAnnotation("=>true, key:notnull; =>false, key:null")]
        bool TryCreateFromPreStoreGeneratedCurrentValues([NotNull] InternalEntityEntry entry, out TKey key);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [ContractAnnotation("=>true, key:notnull; =>false, key:null")]
        bool TryCreateFromOriginalValues([NotNull] InternalEntityEntry entry, out TKey key);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [ContractAnnotation("=>true, key:notnull; =>false, key:null")]
        bool TryCreateFromRelationshipSnapshot([NotNull] InternalEntityEntry entry, out TKey key);
    }
}
