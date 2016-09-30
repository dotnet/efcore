// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     Represents log data that should be reported in tools.
    /// </summary>
    public interface IReportedLogData
    {
        /// <summary>
        ///     Gets the message to report.
        /// </summary>
        string Message { get; }
    }
}
