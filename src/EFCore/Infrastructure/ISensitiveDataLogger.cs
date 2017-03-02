// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     An <see cref="ILogger" /> for which logging of sensitive data can be enabled or disabled.
    /// </summary>
    public interface ISensitiveDataLogger : ILogger
    {
        /// <summary>
        ///     Gets a value indicating whether sensitive data should be logged.
        /// </summary>
        bool LogSensitiveData { get; }
    }
}
