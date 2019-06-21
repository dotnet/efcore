// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     <para>
    ///         Service dependencies parameter class for <see cref="RelationalInterceptors" />
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    ///     <para>
    ///         Do not construct instances of this class directly from either provider or application code as the
    ///         constructor signature may change as new dependencies are added. Instead, use this type in
    ///         your constructor so that an instance will be created and injected automatically by the
    ///         dependency injection container. To create an instance with some dependent services replaced,
    ///         first resolve the object from the dependency injection container, then replace selected
    ///         services using the 'With...' methods. Do not call the constructor at any point in this process.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public sealed class RelationalInterceptorsDependencies
    {
        /// <summary>
        ///     <para>
        ///         Creates the service dependencies parameter object for a <see cref="Interceptors" />.
        ///     </para>
        ///     <para>
        ///         Do not call this constructor directly from either provider or application code as it may change
        ///         as new dependencies are added. Instead, use this type in your constructor so that an instance
        ///         will be created and injected automatically by the dependency injection container. To create
        ///         an instance with some dependent services replaced, first resolve the object from the dependency
        ///         injection container, then replace selected services using the 'With...' methods. Do not call
        ///         the constructor at any point in this process.
        ///     </para>
        /// </summary>
        /// <param name="commandInterceptors"> Command interceptors registered in D.I.  </param>
        /// <param name="databaseInterceptingLoggerFactory"> A factory for creating an intercepting logger. </param>
        public RelationalInterceptorsDependencies(
            [NotNull] IEnumerable<IDbCommandInterceptor> commandInterceptors,
            [NotNull] IRelationalDatabaseInterceptingLoggerFactory databaseInterceptingLoggerFactory)
        {
            Check.NotNull(commandInterceptors, nameof(commandInterceptors));
            Check.NotNull(databaseInterceptingLoggerFactory, nameof(databaseInterceptingLoggerFactory));

            CommandInterceptors = commandInterceptors;
            DatabaseInterceptingLoggerFactory = databaseInterceptingLoggerFactory;
        }

        /// <summary>
        ///     Command interceptors registered in D.I.
        /// </summary>
        public IEnumerable<IDbCommandInterceptor> CommandInterceptors { get; }

        /// <summary>
        ///     A factory for creating an intercepting logger.
        /// </summary>
        public IRelationalDatabaseInterceptingLoggerFactory DatabaseInterceptingLoggerFactory { get; }

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="commandInterceptors"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public RelationalInterceptorsDependencies With([NotNull] IEnumerable<IDbCommandInterceptor> commandInterceptors)
            => new RelationalInterceptorsDependencies(commandInterceptors, DatabaseInterceptingLoggerFactory);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="databaseInterceptingLoggerFactory"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public RelationalInterceptorsDependencies With([NotNull] IRelationalDatabaseInterceptingLoggerFactory databaseInterceptingLoggerFactory)
            => new RelationalInterceptorsDependencies(CommandInterceptors, databaseInterceptingLoggerFactory);
    }
}
