// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqlServerExecutionStrategyFactory : RelationalExecutionStrategyFactory
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SqlServerExecutionStrategyFactory(
            [NotNull] IDbContextOptions options,
            [NotNull] ICurrentDbContext currentDbContext,
            [NotNull] ILogger<IExecutionStrategy> logger)
            : base(options, currentDbContext, logger)
        {
        }

        protected override IExecutionStrategy CreateDefaultStrategy(ExecutionStrategyContext context) => SqlServerExecutionStrategy.Instance;
    }
}
