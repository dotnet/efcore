// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     <para>
    ///         Base implementation for all relational interceptors.
    ///     </para>
    ///     <para>
    ///         Relational providers that need to add interceptors should inherit from this class.
    ///         Non-Relational providers should inherit from <see cref="Interceptors"/>.
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
    public class RelationalInterceptors : Interceptors, IRelationalInterceptors
    {
        private bool _initialized;
        private IDbCommandInterceptor _interceptor;

        /// <summary>
        ///     Creates a new <see cref="RelationalInterceptors"/> instance using the given dependencies.
        /// </summary>
        /// <param name="dependencies"> The dependencies for this service. </param>
        /// <param name="relationalDependencies"> The relational-specific dependencies for this service. </param>
        public RelationalInterceptors(
            [NotNull] InterceptorsDependencies dependencies,
            [NotNull] RelationalInterceptorsDependencies relationalDependencies)
            : base(dependencies)
        {
            Check.NotNull(relationalDependencies, nameof(relationalDependencies));

            RelationalDependencies = relationalDependencies;
        }

        /// <summary>
        ///     The relational-specific dependencies for this service.
        /// </summary>
        protected virtual RelationalInterceptorsDependencies RelationalDependencies { get; }

        /// <summary>
        ///     The <see cref="IDbCommandInterceptor"/> registered, or null if none is registered.
        /// </summary>
        public virtual IDbCommandInterceptor CommandInterceptor
        {
            get
            {
                if (!_initialized)
                {
                    var injectedInterceptors = RelationalDependencies.CommandInterceptors.ToList();

                    if (TryFindAppInterceptor(out var appInterceptor))
                    {
                        injectedInterceptors.Add(appInterceptor);
                    }

                    _interceptor = DbCommandInterceptor.CreateChain(injectedInterceptors);

                    _initialized = true;
                }

                return _interceptor;
            }
        }

        /// <summary>
        ///     We resolve this lazily because loggers are created very early in the initialization
        ///     process where <see cref="IDbContextOptions"/> is not yet available from D.I.
        ///     This means those loggers can't do interception, but that's okay because nothing
        ///     else is ready for them to do interception anyway.
        /// </summary>
        private  bool TryFindAppInterceptor(out IDbCommandInterceptor interceptor)
        {
            interceptor = Dependencies
                .ServiceProvider
                .GetService<IDbContextOptions>()
                .Extensions
                .OfType<RelationalOptionsExtension>()
                .FirstOrDefault()
                ?.CommandInterceptor;

            return interceptor != null;
        }
    }
}
