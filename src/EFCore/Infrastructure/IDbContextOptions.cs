// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         The options to be used by a <see cref="DbContext" />. You normally override
    ///         <see cref="DbContext.OnConfiguring(DbContextOptionsBuilder)" /> or use a <see cref="DbContextOptionsBuilder" />
    ///         to create instances of classes that implement this interface, they are not designed to be directly created
    ///         in your application code.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public interface IDbContextOptions
    {
        /// <summary>
        ///     Gets the extensions that store the configured options.
        /// </summary>
        IEnumerable<IDbContextOptionsExtension> Extensions { get; }

        /// <summary>
        ///     Gets the extension of the specified type. Returns null if no extension of the specified type is configured.
        /// </summary>
        /// <typeparam name="TExtension"> The type of the extension to get. </typeparam>
        /// <returns> The extension, or null if none was found. </returns>
        TExtension FindExtension<TExtension>()
            where TExtension : class, IDbContextOptionsExtension;
    }
}
