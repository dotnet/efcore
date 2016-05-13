// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    public enum RelationalLoggingEventId
    {
        ExecutedCommand = 1,
        CreatingDatabase,
        OpeningConnection,
        ClosingConnection,
        BeginningTransaction,
        CommittingTransaction,
        RollingbackTransaction,
        ClientEvalWarning,
        PossibleUnintendedUseOfEquals
    }
}
