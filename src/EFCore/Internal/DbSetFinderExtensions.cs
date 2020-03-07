// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static class DbSetFinderExtensions
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
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
