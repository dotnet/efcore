// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Relational.Migrations.Infrastructure
{
    public static class MigratorLoggerEventIds
    {
        public static readonly int CreatingHistoryTable = 100;
        public static readonly int DroppingHistoryTable = 101;
        public static readonly int ApplyingMigration = 102;
        public static readonly int RevertingMigration = 103;
        public static readonly int UpToDate = 104;
    }
}
