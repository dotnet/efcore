// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Migrations.Internal
{
    public static class MigrationExtensions
    {
        public static string GetId([NotNull] this Migration migration)
            => migration.GetType().GetTypeInfo().GetCustomAttribute<MigrationAttribute>()?.Id;
    }
}
