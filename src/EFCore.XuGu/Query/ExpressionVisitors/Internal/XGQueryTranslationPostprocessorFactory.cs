// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Query.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.ExpressionVisitors.Internal
{
    public class XGQueryTranslationPostprocessorFactory : IQueryTranslationPostprocessorFactory
    {
        private readonly QueryTranslationPostprocessorDependencies _dependencies;
        private readonly RelationalQueryTranslationPostprocessorDependencies _relationalDependencies;
        private readonly IXGOptions _options;
        private readonly XGSqlExpressionFactory _sqlExpressionFactory;

        public XGQueryTranslationPostprocessorFactory(
            QueryTranslationPostprocessorDependencies dependencies,
            RelationalQueryTranslationPostprocessorDependencies relationalDependencies,
            IXGOptions options,
            ISqlExpressionFactory sqlExpressionFactory)
        {
            _dependencies = dependencies;
            _relationalDependencies = relationalDependencies;
            _options = options;
            _sqlExpressionFactory = (XGSqlExpressionFactory)sqlExpressionFactory;
        }

        public virtual QueryTranslationPostprocessor Create(QueryCompilationContext queryCompilationContext)
            => new XGQueryTranslationPostprocessor(
                _dependencies,
                _relationalDependencies,
                (XGQueryCompilationContext)queryCompilationContext,
                _options,
                _sqlExpressionFactory);
    }
}
