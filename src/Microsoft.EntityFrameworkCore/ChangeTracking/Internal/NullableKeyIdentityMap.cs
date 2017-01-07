// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class NullableKeyIdentityMap<TKey> : IdentityMap<TKey>
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public NullableKeyIdentityMap(
            [NotNull] IKey key,
            [NotNull] IPrincipalKeyValueFactory<TKey> principalKeyValueFactory,
            bool sensitiveLoggingEnabled)
            : base(key, principalKeyValueFactory, sensitiveLoggingEnabled)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void Add(InternalEntityEntry entry)
        {
            var key = PrincipalKeyValueFactory.CreateFromCurrentValues(entry);

            if (key == null)
            {
                if (Key.IsPrimaryKey())
                {
                    throw new InvalidOperationException(
                        CoreStrings.InvalidKeyValue(
                            entry.EntityType.DisplayName(),
                            PrincipalKeyValueFactory.FindNullPropertyInCurrentValues(entry).Name));
                }

                throw new InvalidOperationException(
                    CoreStrings.InvalidAlternateKeyValue(
                        entry.EntityType.DisplayName(),
                        PrincipalKeyValueFactory.FindNullPropertyInCurrentValues(entry).Name));
            }

            Add(key, entry);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void RemoveUsingRelationshipSnapshot(InternalEntityEntry entry)
        {
            var key = PrincipalKeyValueFactory.CreateFromRelationshipSnapshot(entry);

            if (key != null)
            {
                Remove(key, entry);
            }
        }
    }
}
