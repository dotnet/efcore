// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ExecutionStrategyFactory : IExecutionStrategyFactory
    {
        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual ExecutionStrategyDependencies Dependencies { get; }

        /// <summary>
        ///     Creates a new instance of this class with the given service dependencies.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        public ExecutionStrategyFactory([NotNull] ExecutionStrategyDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
        }

        /// <summary>
        ///     Creates a new  <see cref="IExecutionStrategy" />.
        /// </summary>
        /// <returns>An instance of <see cref="IExecutionStrategy" />.</returns>
        public virtual IExecutionStrategy Create() => new NoopExecutionStrategy(Dependencies);
    }
}
