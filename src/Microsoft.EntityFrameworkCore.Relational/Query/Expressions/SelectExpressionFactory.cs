// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Expressions
{
    public class SelectExpressionFactory : ISelectExpressionFactory
    {
        private readonly IQuerySqlGeneratorFactory _querySqlGeneratorFactory;

        public SelectExpressionFactory([NotNull] IQuerySqlGeneratorFactory querySqlGeneratorFactory)
        {
            Check.NotNull(querySqlGeneratorFactory, nameof(querySqlGeneratorFactory));

            _querySqlGeneratorFactory = querySqlGeneratorFactory;
        }

        public virtual SelectExpression Create(RelationalQueryCompilationContext queryCompilationContext)
            => new SelectExpression(_querySqlGeneratorFactory, queryCompilationContext);

        public virtual SelectExpression Create(RelationalQueryCompilationContext queryCompilationContext, string alias)
            => new SelectExpression(
                _querySqlGeneratorFactory,
                queryCompilationContext,
                Check.NotEmpty(alias, nameof(alias)));
    }
}
