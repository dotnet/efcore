// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Metadata.Internal
{
    public static class DatabaseTableExtensions
    {
        public static string DisplayName([NotNull] this DatabaseTable table)
            => !string.IsNullOrEmpty(table.Schema) ? table.Schema + "." + table.Name : table.Name;
    }
}
