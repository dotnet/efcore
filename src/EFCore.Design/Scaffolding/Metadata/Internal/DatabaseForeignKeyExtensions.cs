// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Metadata.Internal
{
    public static class DatabaseForeignKeyExtensions
    {
        public static string DisplayName([NotNull] this DatabaseForeignKey foreignKey)
            => foreignKey.Table?.DisplayName() + "(" + string.Join(",", foreignKey.Columns.Select(f => f.Name)) + ")";
    }
}
