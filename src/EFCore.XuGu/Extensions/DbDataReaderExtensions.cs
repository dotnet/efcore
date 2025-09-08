// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace System.Data.Common
{
    internal static class DbDataReaderExtension
    {
        public static T GetValueOrDefault<T>([NotNull] this DbDataReader reader, [NotNull] string name)
        {
            var idx = reader.GetOrdinal(name);
            return reader.IsDBNull(idx)
                ? default
                : reader.GetFieldValue<T>(idx);
        }

        public static bool HasName(this DbDataReader reader, string columnName)
            => Enumerable.Range(0, reader.FieldCount)
                .Any(i => string.Equals(reader.GetName(i), columnName, StringComparison.OrdinalIgnoreCase));
    }
}
