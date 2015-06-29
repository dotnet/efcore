// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Infrastructure
{
    public static class RelationalLoggingEventIds
    {
        public const int ExecutingSql = 42;
        public const int CreatingDatabase = 43;
        public const int OpeningConnection = 44;
        public const int ClosingConnection = 45;
        public const int BeginningTransaction = 46;
        public const int CommittingTransaction = 47;
        public const int RollingbackTransaction = 48;
    }
}
