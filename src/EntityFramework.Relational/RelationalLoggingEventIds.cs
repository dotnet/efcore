// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Relational
{
    public static class RelationalLoggingEventIds
    {
        public static readonly int Sql = 42;
        public static readonly int CreatingDatabase = 43;
        public static readonly int OpeningConnection = 44;
        public static readonly int ClosingConnection = 45;
        public static readonly int BeginningTransaction = 46;
        public static readonly int CommittingTransaction = 47;
        public static readonly int RollingbackTransaction = 48;
    }
}
