// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Utilities;

#nullable disable

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class CosmosShapedQueryCompilingExpressionVisitorFactory : IShapedQueryCompilingExpressionVisitorFactory
    {
        private readonly ShapedQueryCompilingExpressionVisitorDependencies _dependencies;
        private readonly ISqlExpressionFactory _sqlExpressionFactory;
        private readonly IQuerySqlGeneratorFactory _querySqlGeneratorFactory;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public CosmosShapedQueryCompilingExpressionVisitorFactory(
            ShapedQueryCompilingExpressionVisitorDependencies dependencies,
            ISqlExpressionFactory sqlExpressionFactory,
            IQuerySqlGeneratorFactory querySqlGeneratorFactory)
        {
            Check.NotNull(dependencies, nameof(dependencies));
            Check.NotNull(sqlExpressionFactory, nameof(sqlExpressionFactory));
            Check.NotNull(querySqlGeneratorFactory, nameof(querySqlGeneratorFactory));

            _dependencies = dependencies;
            _sqlExpressionFactory = sqlExpressionFactory;
            _querySqlGeneratorFactory = querySqlGeneratorFactory;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ShapedQueryCompilingExpressionVisitor Create(QueryCompilationContext queryCompilationContext)
        {
            Check.NotNull(queryCompilationContext, nameof(queryCompilationContext));

            return new CosmosShapedQueryCompilingExpressionVisitor(
                _dependencies,
                (CosmosQueryCompilationContext)queryCompilationContext,
                _sqlExpressionFactory,
                _querySqlGeneratorFactory);
        }
    }
}
