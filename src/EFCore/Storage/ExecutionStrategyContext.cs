// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     A class that provides dependencies for <see cref="ExecutionStrategy" />
    /// </summary>
    public class ExecutionStrategyContext
    {
        /// <summary>
        ///     Creates a new instance of <see cref="ExecutionStrategyContext" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        public ExecutionStrategyContext([NotNull] ExecutionStrategyContextDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual ExecutionStrategyContextDependencies Dependencies { get; }

        /// <summary>
        ///     The context on which the operations will be invoked.
        /// </summary>
        public virtual DbContext Context => Dependencies.CurrentDbContext.Context;

        /// <summary>
        ///     The logger for the <see cref="ExecutionStrategy" />.
        /// </summary>
        public virtual IDiagnosticsLogger<DbLoggerCategory.Infrastructure> Logger => Dependencies.Logger;
    }
}
