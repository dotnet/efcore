// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Expressions
{
    /// <summary>
    ///     A SelectExpression factory.
    /// </summary>
    public class SelectExpressionFactory : ISelectExpressionFactory
    {
        private readonly IQuerySqlGeneratorFactory _querySqlGeneratorFactory;

        /// <summary>
        ///     Initializes a new instance of the Microsoft.EntityFrameworkCore.Query.Expressions.SelectExpressionFactory class.
        /// </summary>
        /// <param name="querySqlGeneratorFactory"> The query SQL generator factory. </param>
        public SelectExpressionFactory([NotNull] IQuerySqlGeneratorFactory querySqlGeneratorFactory)
        {
            Check.NotNull(querySqlGeneratorFactory, nameof(querySqlGeneratorFactory));

            _querySqlGeneratorFactory = querySqlGeneratorFactory;
        }

        /// <summary>
        ///     Creates a new SelectExpression.
        /// </summary>
        /// <param name="queryCompilationContext"> Context for the query compilation. </param>
        /// <returns>
        ///     A SelectExpression.
        /// </returns>
        public virtual SelectExpression Create(RelationalQueryCompilationContext queryCompilationContext)
            => new SelectExpression(_querySqlGeneratorFactory, queryCompilationContext);

        /// <summary>
        ///     Creates a new SelectExpression.
        /// </summary>
        /// <param name="queryCompilationContext"> Context for the query compilation. </param>
        /// <param name="alias"> The alias of this SelectExpression. </param>
        /// <returns>
        ///     A SelectExpression.
        /// </returns>
        public virtual SelectExpression Create(RelationalQueryCompilationContext queryCompilationContext, string alias)
            => new SelectExpression(
                _querySqlGeneratorFactory,
                queryCompilationContext,
                Check.NotEmpty(alias, nameof(alias)));
    }
}
