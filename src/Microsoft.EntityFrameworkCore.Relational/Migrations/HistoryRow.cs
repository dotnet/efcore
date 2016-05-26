// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    public class HistoryRow
    {
        public HistoryRow([NotNull] string migrationId, [NotNull] string productVersion, [NotNull] string downScript)
        {
            Check.NotEmpty(migrationId, nameof(migrationId));
            Check.NotEmpty(productVersion, nameof(productVersion));
            Check.NotEmpty(downScript, nameof(downScript));

            MigrationId = migrationId;
            ProductVersion = productVersion;
            DownScript = downScript;
        }

        public virtual string MigrationId { get; }
        public virtual string ProductVersion { get; }
        public virtual string DownScript { get; }
    }
}