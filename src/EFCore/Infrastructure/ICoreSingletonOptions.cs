// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     Options set at the <see cref="IServiceProvider" /> singleton level to control core options.
    /// </summary>
    public interface ICoreSingletonOptions : ISingletonOptions
    {
        /// <summary>
        ///     Reflects the option set by <see cref="DbContextOptionsBuilder.EnableDetailedErrors" />.
        /// </summary>
        bool AreDetailedErrorsEnabled { get; }
    }
}
