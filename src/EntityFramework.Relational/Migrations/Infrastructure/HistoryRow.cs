// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Relational.Migrations.Infrastructure
{
    public class HistoryRow
    {
        public virtual string MigrationId { get; internal set; }
        public virtual string ContextKey { get; internal set; }
        public virtual string ProductVersion { get; internal set; }
    }
}
