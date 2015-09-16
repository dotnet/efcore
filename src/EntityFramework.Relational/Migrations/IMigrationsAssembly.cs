// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;

namespace Microsoft.Data.Entity.Migrations
{
    public interface IMigrationsAssembly
    {
        IReadOnlyDictionary<string, TypeInfo> Migrations { get; }
        ModelSnapshot ModelSnapshot { get; }
        string FindMigrationId([NotNull] string nameOrId);
        Migration CreateMigration([NotNull] TypeInfo migrationClass, [NotNull] string activeProvider);
    }
}
