// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.SqlClient;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Scaffolding
{
    // TODO merge with DbDataReaderExtension.GetValueOrDefault when Mono supports GetFieldValue. See #2079
    public static class SqlDataReaderExtension
    {
        public static T GetValueOrDefault<T>([NotNull] this SqlDataReader reader, [NotNull] string name)
        {
            var idx = reader.GetOrdinal(name);
            return reader.IsDBNull(idx)
                ? default(T)
                // : reader.GetFieldValue<T>(idx);
                : (T)reader.GetValue(idx);
        }
    }
}
