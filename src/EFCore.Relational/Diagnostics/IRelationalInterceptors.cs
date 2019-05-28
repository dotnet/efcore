// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     <para>
    ///         Base interface for all relational interceptor definitions.
    ///     </para>
    ///     <para>
    ///         Rather than implementing this interface directly, relational providers that need to add interceptors should inherit
    ///         from <see cref="RelationalInterceptors"/>. Relational providers should inherit from <see cref="Interceptors"/>.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public interface IRelationalInterceptors : IInterceptors
    {
        /// <summary>
        ///     The <see cref="IDbCommandInterceptor"/> registered, or null if none is registered.
        /// </summary>
        IDbCommandInterceptor CommandInterceptor { get; }
    }
}
