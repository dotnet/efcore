// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

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
        /// <param name="context">The context on which the operations will be invoked.</param>
        /// <param name="logger">The logger to be used.</param>
        public ExecutionStrategyContext(
            [NotNull] DbContext context,
            [NotNull] ILogger<IExecutionStrategy> logger)
        {
            Context = context;
            Logger = logger;
        }

        /// <summary>
        ///     The context on which the operations will be invoked.
        /// </summary>
        public virtual DbContext Context { get; }

        /// <summary>
        ///     The logger for the <see cref="ExecutionStrategy" />.
        /// </summary>
        public virtual ILogger<IExecutionStrategy> Logger { get; }
    }
}
