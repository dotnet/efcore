// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Scaffolding.Model
{
    public static class ModelExtensions
    {
        public static string DisplayName([NotNull] this Table table)
            => (!string.IsNullOrEmpty(table.SchemaName) ? table.SchemaName + "." : "") + table.Name;

        public static string DisplayName([NotNull] this Column column)
        {
            var tablePrefix = column.Table?.DisplayName();
            return (!string.IsNullOrEmpty(tablePrefix) ? tablePrefix + "." : "") + column.Name;
        }

        public static string DisplayName([NotNull] this ForeignKey foreignKey)
            => foreignKey.Table?.DisplayName() + "(" + string.Join(",", foreignKey.From.Select(f => f.Name)) + ")";
    }
}