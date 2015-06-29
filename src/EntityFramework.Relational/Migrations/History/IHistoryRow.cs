// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Migrations.History
{
    public interface IHistoryRow
    {
        string MigrationId { get; }
        string ProductVersion { get; }
    }
}
