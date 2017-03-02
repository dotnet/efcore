// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     Factory for creating <see cref="IExecutionStrategy" /> instances for use with relational
    ///     database providers.
    /// </summary>
    public class RelationalExecutionStrategyFactory : IExecutionStrategyFactory
    {
        private readonly Func<ExecutionStrategyContext, IExecutionStrategy> _createExecutionStrategy;

        /// <summary>
        ///     Creates a new instance of this class with the given service dependencies.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        public RelationalExecutionStrategyFactory([NotNull] ExecutionStrategyContextDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;

            Context = new ExecutionStrategyContext(dependencies);

            var configuredFactory = dependencies.Options == null
                ? null
                : RelationalOptionsExtension.Extract(dependencies.Options)?.ExecutionStrategyFactory;

            _createExecutionStrategy = configuredFactory ?? CreateDefaultStrategy;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual ExecutionStrategyContextDependencies Dependencies { get; }

        /// <summary>
        ///     The <see cref="ExecutionStrategyContext" />.
        /// </summary>
        protected virtual ExecutionStrategyContext Context { get; }

        /// <summary>
        ///     Creates or returns a cached instance of the default <see cref="IExecutionStrategy" /> for the
        ///     current database provider.
        /// </summary>
        protected virtual IExecutionStrategy CreateDefaultStrategy([NotNull] ExecutionStrategyContext context)
            => NoopExecutionStrategy.Instance;

        /// <summary>
        ///     Creates an <see cref="IExecutionStrategy" /> for the current database provider.
        /// </summary>
        public virtual IExecutionStrategy Create() => _createExecutionStrategy(Context);
    }
}
