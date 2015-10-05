// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Internal
{
    internal static class TableSelectionSetSqliteExtensions
    {
        public static bool Allows(this TableSelectionSet tableSet, string tableName)
        {
            if (tableSet == null
                || tableSet.Tables.Count == 0)
            {
                return true;
            }
            return tableSet.Tables.Any(t => t.Equals(tableName, StringComparison.OrdinalIgnoreCase));
        }
    }
}