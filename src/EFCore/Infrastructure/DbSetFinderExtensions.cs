// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     Extension methods for <see cref="IDbSetFinder" />.
    /// </summary>
    public static class DbSetFinderExtensions
    {
        /// <summary>
        ///     Create a mapping between clrType and <see cref="DbSetProperty"/> specified on given context type.
        /// </summary>
        /// <param name="setFinder"> The <see cref="IDbSetFinder"/> to use to find DbSet on context. </param>
        /// <param name="contextType"> The type of context to create mapping for. </param>
        /// <returns>
        ///     Mapping of type to <see cref="DbSetProperty"/> for given context type.
        /// </returns>
        public static IDictionary<Type, DbSetProperty> CreateClrTypeDbSetMapping(
            [NotNull] this IDbSetFinder setFinder, [NotNull] Type contextType)
        {
            var sets = new Dictionary<Type, DbSetProperty>();
            var alreadySeen = new HashSet<Type>();
            foreach (var set in setFinder.FindSets(contextType))
            {
                if (!alreadySeen.Contains(set.ClrType))
                {
                    alreadySeen.Add(set.ClrType);
                    sets.Add(set.ClrType, set);
                }
                else
                {
                    sets.Remove(set.ClrType);
                }
            }

            return sets;
        }
    }
}
