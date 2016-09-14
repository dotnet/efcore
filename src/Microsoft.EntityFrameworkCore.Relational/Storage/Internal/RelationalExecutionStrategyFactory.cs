// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class RelationalExecutionStrategyFactory : IExecutionStrategyFactory
    {
        private readonly Func<ExecutionStrategyContext, IExecutionStrategy> _createExecutionStrategy;
        private readonly ExecutionStrategyContext _context;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public RelationalExecutionStrategyFactory(
            [NotNull] IDbContextOptions options,
            [NotNull] ICurrentDbContext currentDbContext,
            [NotNull] ILogger<IExecutionStrategy> logger)
            : this(options, new ExecutionStrategyContext(currentDbContext.Context, logger))
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected RelationalExecutionStrategyFactory(
            [NotNull] IDbContextOptions options,
            [NotNull] ExecutionStrategyContext context)
        {
            var optionsExtension = RelationalOptionsExtension.Extract(options);
            var configuredFactory = optionsExtension?.ExecutionStrategyFactory;

            _createExecutionStrategy = configuredFactory ?? CreateDefaultStrategy;
            _context = context;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IExecutionStrategy CreateDefaultStrategy([NotNull] ExecutionStrategyContext context) => NoopExecutionStrategy.Instance;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IExecutionStrategy Create() => _createExecutionStrategy(_context);
    }
}
