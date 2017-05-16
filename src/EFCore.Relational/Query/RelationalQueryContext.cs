// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     The principal data structure used by a compiled relational query during execution.
    /// </summary>
    public class RelationalQueryContext : QueryContext
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public RelationalQueryContext(
            [NotNull] QueryContextDependencies dependencies,
            [NotNull] Func<IQueryBuffer> queryBufferFactory,
            [NotNull] IRelationalConnection connection,
            [NotNull] IExecutionStrategyFactory executionStrategyFactory)
            : base(dependencies, queryBufferFactory)
        {
            Check.NotNull(connection, nameof(connection));
            Check.NotNull(executionStrategyFactory, nameof(executionStrategyFactory));

            Connection = connection;
            ExecutionStrategyFactory = executionStrategyFactory;
        }

        /// <summary>
        ///     Gets the active relational connection.
        /// </summary>
        /// <value>
        ///     The connection.
        /// </value>
        public virtual IRelationalConnection Connection { get; }

        /// <summary>
        ///     The execution strategy factory.
        /// </summary>
        /// <value>
        ///     The execution strategy factory.
        /// </value>
        public virtual IExecutionStrategyFactory ExecutionStrategyFactory { get; }
    }
}
