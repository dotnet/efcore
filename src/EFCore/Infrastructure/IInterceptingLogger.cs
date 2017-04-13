// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     A specialed <see cref="ILogger" /> that intercepts messages such that warnings
    ///     can be either logged or thrown, and such that a decision as to whether to log
    ///     sensitive data or not can be made.
    /// </summary>
    /// <typeparam name="TLoggerCategory"> The category of this logger. </typeparam>
    // ReSharper disable once UnusedTypeParameter
    public interface IInterceptingLogger<TLoggerCategory> : ILogger
        where TLoggerCategory : LoggerCategory<TLoggerCategory>, new()
    {
        /// <summary>
        ///     Entity Framework logging options.
        /// </summary>
        ILoggingOptions Options { get; }

        /// <summary>
        ///     Gets a value indicating whether sensitive information should be written
        ///     to the underlying logger. This also has the side effect of writing a warning
        ///     to the log the first time sensitive data is logged.
        /// </summary>
        bool LogSensitiveData { get; }
    }
}
