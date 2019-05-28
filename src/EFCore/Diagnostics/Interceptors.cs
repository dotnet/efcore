// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     <para>
    ///         Base implementation for all interceptors.
    ///     </para>
    ///     <para>
    ///         Non-relational providers that need to add interceptors should inherit from this class.
    ///         Relational providers should inherit from 'RelationalInterceptors'.
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
    public class Interceptors : IInterceptors
    {
        /// <summary>
        ///     Creates a new <see cref="Interceptors"/> instance using the given dependencies.
        /// </summary>
        /// <param name="dependencies"> The dependencies for this service. </param>
        public Interceptors([NotNull] InterceptorsDependencies dependencies)
        {
            Dependencies = dependencies;
        }

        /// <summary>
        ///     The dependencies for this service.
        /// </summary>
        protected virtual InterceptorsDependencies Dependencies { get; }
    }
}
