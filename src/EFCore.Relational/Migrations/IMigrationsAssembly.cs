// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    public interface IMigrationsAssembly
    {
        IReadOnlyDictionary<string, TypeInfo> Migrations { get; }
        ModelSnapshot ModelSnapshot { get; }
        Assembly Assembly { get; }
        string FindMigrationId([NotNull] string nameOrId);
        Migration CreateMigration([NotNull] TypeInfo migrationClass, [NotNull] string activeProvider);
    }
}
