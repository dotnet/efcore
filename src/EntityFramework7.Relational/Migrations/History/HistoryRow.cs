// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Migrations.History
{
    public class HistoryRow
    {
        public HistoryRow([NotNull] string migrationId, [NotNull] string productVersion, [NotNull] string contextKey)
        {
            Check.NotEmpty(migrationId, nameof(migrationId));
            Check.NotEmpty(productVersion, nameof(productVersion));
            Check.NotEmpty(contextKey, nameof(contextKey));

            MigrationId = migrationId;
            ProductVersion = productVersion;
            ContextKey = contextKey;
        }

        public virtual string MigrationId { get; }
        public virtual string ProductVersion { get; }
        public virtual string ContextKey { get; }
    }
}
