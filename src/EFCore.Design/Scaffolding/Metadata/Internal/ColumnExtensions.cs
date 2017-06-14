// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Metadata.Internal
{
    public static class ColumnExtensions
    {
        public static string DisplayName([NotNull] this Column column)
        {
            var tablePrefix = column.Table?.DisplayName();
            return (!string.IsNullOrEmpty(tablePrefix) ? tablePrefix + "." : "") + column.Name;
        }
    }
}
