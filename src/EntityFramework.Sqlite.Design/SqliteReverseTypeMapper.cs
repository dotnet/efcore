// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Sqlite.Design
{
    /// <summary>
    ///     This maps SQLite's loose type affinities to CLR types.
    /// </summary>
    public class SqliteReverseTypeMapper
    {
        /// <summary>
        ///     Returns a clr type for a SQLite column type. Defaults to typeof(string).
        ///     It uses the same heuristics from
        ///     <see href="https://www.sqlite.org/datatype3.html">"2.1 Determination of Column Affinity"</see>
        /// </summary>
        public virtual Type GetClrType([CanBeNull] string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                return _default;
            }

            Type clrType;
            foreach (var rules in _typeRules)
            {
                clrType = rules(typeName);
                if (clrType != null)
                {
                    return clrType;
                }
            }

            return _default;
        }

        private static readonly Type _default = typeof(string);

        private readonly Func<string, Type>[] _typeRules =
            {
                name => Contains(name, "INT") ? typeof(long) : null,
                name => Contains(name, "CHAR") || Contains(name, "CLOB") || Contains(name, "TEXT") ? typeof(string) : null,
                name => Contains(name, "BLOB") ? typeof(byte[]) : null,
                name => Contains(name, "REAL") || Contains(name, "FLOA") || Contains(name, "DOUB") ? typeof(double) : null
            };

        private static bool Contains(string haystack, string needle)
            => haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
