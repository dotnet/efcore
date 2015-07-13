// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Migrations.Infrastructure
{
    public interface IMigrationIdGenerator
    {
        string CreateId([NotNull] string name);
        string GetName([NotNull] string id);
        bool IsValidId([NotNull] string value);
        string ResolveId([NotNull] string nameOrId, [NotNull] IReadOnlyList<Migration> migrations);
    }
}
