// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    public interface IMigrator
    {
        void Migrate([CanBeNull] string targetMigration = null);

        Task MigrateAsync(
            [CanBeNull] string targetMigration = null,
            CancellationToken cancellationToken = default(CancellationToken));

        string GenerateScript([CanBeNull] string fromMigration = null, [CanBeNull] string toMigration = null, bool idempotent = false);
    }
}
