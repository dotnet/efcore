// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
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
