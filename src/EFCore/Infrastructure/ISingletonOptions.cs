// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         Implemented by any class that represents options that can only be set at the
    ///         <see cref="IServiceProvider" /> singleton level.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" /> and multiple registrations
    ///         are allowed. This means a single instance of each service is used by many <see cref="DbContext" />
    ///         instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public interface ISingletonOptions
    {
        /// <summary>
        ///     Initializes the singleton options from the given <see cref="IDbContextOptions" />.
        /// </summary>
        void Initialize([NotNull] IDbContextOptions options);

        /// <summary>
        ///     Validates that the options in given <see cref="IDbContextOptions" /> have not
        ///     changed when compared to the options already set here, and throws if they have.
        /// </summary>
        void Validate([NotNull] IDbContextOptions options);
    }
}
