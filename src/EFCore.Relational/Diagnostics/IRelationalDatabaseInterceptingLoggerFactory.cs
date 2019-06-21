// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     <para>
    ///         A factory for creating <see cref="RelationalDatabaseInterceptingLogger" /> instances that are
    ///         used when logging with <see cref="DatabaseFacade.Log" />.
    ///     </para>
    ///     <para>
    ///         This service is resolved from Entity Framework's internal service provider.
    ///         Use <see cref="DbContextOptionsBuilder.ReplaceService{TService,TImplementation}" /> to change the
    ///         implementation used.
    ///     </para>
    /// </summary>
    public interface IRelationalDatabaseInterceptingLoggerFactory
    {
        /// <summary>
        ///     Creates a new <see cref="RelationalDatabaseInterceptingLogger" />.
        /// </summary>
        /// <param name="action"> The delegate to which log messages will be written. </param>
        /// <param name="level"> The log level to filter by. Only events at or more severe than this level will be logged. </param>
        /// <returns> The new logger. </returns>
        RelationalDatabaseInterceptingLogger Create([NotNull] Action<string> action, LogLevel level);
    }
}
