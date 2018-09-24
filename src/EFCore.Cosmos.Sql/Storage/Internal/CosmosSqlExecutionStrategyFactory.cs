// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Cosmos.Sql.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Cosmos.Sql.Storage.Internal
{
    public class CosmosSqlExecutionStrategyFactory : IExecutionStrategyFactory
    {
        private readonly Func<ExecutionStrategyDependencies, IExecutionStrategy> _createExecutionStrategy;

        /// <summary>
        ///     Creates a new instance of this class with the given service dependencies.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        public CosmosSqlExecutionStrategyFactory([NotNull] ExecutionStrategyDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;

            _createExecutionStrategy = dependencies.Options?.FindExtension<CosmosSqlDbOptionsExtension>()?.ExecutionStrategyFactory
                                       ?? CreateDefaultStrategy;
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
            => new CosmosSqlExecutionStrategy(Dependencies);

        /// <summary>
        ///     Creates an <see cref="IExecutionStrategy" /> for the current database provider.
        /// </summary>
        public virtual IExecutionStrategy Create() => _createExecutionStrategy(Dependencies);
    }
}
