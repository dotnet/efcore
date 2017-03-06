// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     Options set at the <see cref="IServiceProvider" /> singleton level to control how\
    ///     messages are logged and/or thrown in exceptions.
    /// </summary>
    public interface ILoggingOptions : ISingletonOptions
    {
        /// <summary>
        ///     Reflects the option set by <see cref="DbContextOptionsBuilder.EnableSensitiveDataLogging" />.
        /// </summary>
        bool SensitiveDataLoggingEnabled { get; }

        /// <summary>
        ///     This flag is set once a warning about <see cref="SensitiveDataLoggingEnabled" /> has been
        ///     issued to avoid logging the warning again.
        /// </summary>
        bool SensitiveDataLoggingWarned { get; set; }

        /// <summary>
        ///     Reflects the option set by <see cref="DbContextOptionsBuilder.ConfigureWarnings" />.
        /// </summary>
        WarningsConfiguration WarningsConfiguration { get; }
    }
}
