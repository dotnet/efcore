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
        public virtual Type GetClrType([CanBeNull] string typeName, [NotNull] bool nullable)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                return _default;
            }

            Type clrType;
            foreach (var rules in _typeRules)
            {
                clrType = rules(typeName, nullable);
                if (clrType != null)
                {
                    return clrType;
                }
            }

            return _default;
        }

        private static readonly Type _default = typeof(string);

        private readonly Func<string, bool, Type>[] _typeRules =
        {
            (name, nullable) =>
                {
                    if (Contains(name, "INT"))
                    {
                        return nullable ? typeof(long?) : typeof(long);
                    }
                    return null;
                },
            (name, nullable) => Contains(name, "CHAR") || Contains(name, "CLOB") || Contains(name, "TEXT") ? typeof(string) : null,
            (name, nullable) => Contains(name, "BLOB") ? typeof(byte[]) : null,
            (name, nullable) =>
                {
                    if (Contains(name, "REAL")
                        || Contains(name, "FLOA")
                        || Contains(name, "DOUB"))
                    {
                        return nullable ? typeof(double?) : typeof(double);
                    }
                    return null;
                }
        };

        private static bool Contains(string haystack, string needle)
            => haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
