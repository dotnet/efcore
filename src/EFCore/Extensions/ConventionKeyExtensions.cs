// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="IConventionKey" />.
    /// </summary>
    public static class ConventionKeyExtensions
    {
        /// <summary>
        ///     Gets all foreign keys that target a given primary or alternate key.
        /// </summary>
        /// <param name="key"> The key to find the foreign keys for. </param>
        /// <returns> The foreign keys that reference the given key. </returns>
        public static IEnumerable<IConventionForeignKey> GetReferencingForeignKeys([NotNull] this IConventionKey key)
            => ((IKey)key).GetReferencingForeignKeys().Cast<IConventionForeignKey>();
    }
}
