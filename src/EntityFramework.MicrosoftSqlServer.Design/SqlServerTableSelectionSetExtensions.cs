// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Scaffolding
{
    internal static class SqlServerTableSelectionSetExtensions
    {
        public static bool Allows(this TableSelectionSet tableSelectionSet, [NotNull] string schemaName, [NotNull] string tableName)
        {
            if ((tableSelectionSet == null)
                || ((tableSelectionSet.Schemas.Count == 0)
                && (tableSelectionSet.Tables.Count == 0)))
            {
                return true;
            }

            if (tableSelectionSet.Schemas.Contains(schemaName))
            {
                return true;
            }

            return tableSelectionSet.Tables.Contains($"{schemaName}.{tableName}", StringComparer.OrdinalIgnoreCase)
                || tableSelectionSet.Tables.Contains($"[{schemaName}].[{tableName}]", StringComparer.OrdinalIgnoreCase)
                || tableSelectionSet.Tables.Contains($"{schemaName}.[{tableName}]", StringComparer.OrdinalIgnoreCase)
                || tableSelectionSet.Tables.Contains($"[{schemaName}].{tableName}", StringComparer.OrdinalIgnoreCase)
                || tableSelectionSet.Tables.Contains($"{tableName}", StringComparer.OrdinalIgnoreCase)
                || tableSelectionSet.Tables.Contains($"[{tableName}]", StringComparer.OrdinalIgnoreCase);
        }
    }
}
