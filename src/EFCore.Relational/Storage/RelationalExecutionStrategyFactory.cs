// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Factory for creating <see cref="IExecutionStrategy" /> instances for use with relational
    ///         database providers.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public class RelationalExecutionStrategyFactory : IExecutionStrategyFactory
    {
        private readonly Func<ExecutionStrategyDependencies, IExecutionStrategy> _createExecutionStrategy;

        /// <summary>
        ///     Creates a new instance of this class with the given service dependencies.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        public RelationalExecutionStrategyFactory([NotNull] ExecutionStrategyDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;

            var configuredFactory = dependencies.Options == null
                ? null
                : RelationalOptionsExtension.Extract(dependencies.Options)?.ExecutionStrategyFactory;

            _createExecutionStrategy = configuredFactory ?? CreateDefaultStrategy;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual ExecutionStrategyDependencies Dependencies { get; }

        /// <summary>
        ///     Creates or returns a cached instance of the default <see cref="IExecutionStrategy" /> for the
        ///     current database provider.
        /// </summary>
        protected virtual IExecutionStrategy CreateDefaultStrategy([NotNull] ExecutionStrategyDependencies dependencies)
            => new NonRetryingExecutionStrategy(Dependencies);

        /// <summary>
        ///     Creates an <see cref="IExecutionStrategy" /> for the current database provider.
        /// </summary>
        public virtual IExecutionStrategy Create()
            => _createExecutionStrategy(Dependencies);
    }
}
