// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    /// <summary>
    ///     Extension methods for <see cref="IKey" />.
    /// </summary>
    public static class KeyExtensions
    {
        /// <summary>
        ///     Gets all foreign keys that target a given primary or alternate key.
        /// </summary>
        /// <param name="key"> The key to find the foreign keys for. </param>
        /// <returns> The foreign keys that reference the given key. </returns>
        public static IEnumerable<IForeignKey> FindReferencingForeignKeys([NotNull] this IKey key)
        {
            Check.NotNull(key, nameof(key));

            return key.DeclaringEntityType.Model.GetEntityTypes().SelectMany(e =>
                e.GetDeclaredForeignKeys()).Where(fk => fk.PrincipalKey == key);
        }
    }
}
