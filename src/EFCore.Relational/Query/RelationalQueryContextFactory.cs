// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class RelationalQueryContextFactory : QueryContextFactory
    {
        private readonly IRelationalConnection _connection;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public RelationalQueryContextFactory(
            [NotNull] QueryContextDependencies dependencies,
            [NotNull] IRelationalConnection connection,
            [NotNull] IExecutionStrategyFactory executionStrategyFactory)
            : base(dependencies)
        {
            _connection = connection;
            ExecutionStrategyFactory = executionStrategyFactory;
        }

        /// <summary>
        ///     The execution strategy factory.
        /// </summary>
        /// <value>
        ///     The execution strategy factory.
        /// </value>
        protected virtual IExecutionStrategyFactory ExecutionStrategyFactory { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override QueryContext Create()
            => new RelationalQueryContext(Dependencies, CreateQueryBuffer, _connection, ExecutionStrategyFactory);
    }
}
