// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     <para>
    ///         Options set at the <see cref="IServiceProvider" /> singleton level to control how\
    ///         messages are logged and/or thrown in exceptions.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public interface ILoggingOptions : ISingletonOptions
    {
        /// <summary>
        ///     Reflects the option set by <see cref="DbContextOptionsBuilder.EnableSensitiveDataLogging" />.
        /// </summary>
        bool IsSensitiveDataLoggingEnabled { get; }

        /// <summary>
        ///     This flag is set once a warning about <see cref="IsSensitiveDataLoggingEnabled" /> has been
        ///     issued to avoid logging the warning again.
        /// </summary>
        bool IsSensitiveDataLoggingWarned { get; set; }

        /// <summary>
        ///     Reflects the option set by <see cref="DbContextOptionsBuilder.ConfigureWarnings" />.
        /// </summary>
        WarningsConfiguration WarningsConfiguration { get; }
    }
}
