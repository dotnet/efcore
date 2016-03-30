// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Internal
{
    public static class DbSetFinderExtensions
    {
        public static IDictionary<Type, DbSetProperty> CreateClrTypeDbSetMapping(
            [NotNull] this IDbSetFinder setFinder, [NotNull] DbContext context)
        {
            var sets = new Dictionary<Type, DbSetProperty>();
            var alreadySeen = new HashSet<Type>();
            foreach (var set in setFinder.FindSets(context))
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
