// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.ExpressionVisitors.Internal;

public class XGQueryTranslationPreprocessor : RelationalQueryTranslationPreprocessor
{
    private readonly RelationalQueryCompilationContext _relationalQueryCompilationContext;

    public XGQueryTranslationPreprocessor(
        QueryTranslationPreprocessorDependencies dependencies,
        RelationalQueryTranslationPreprocessorDependencies relationalDependencies,
        QueryCompilationContext queryCompilationContext)
        : base(dependencies, relationalDependencies, queryCompilationContext)
    {
        _relationalQueryCompilationContext = (RelationalQueryCompilationContext)queryCompilationContext;
    }

    /// <summary>
    /// Workaround https://github.com/dotnet/efcore/issues/30386.
    /// </summary>
    public override Expression NormalizeQueryableMethod(Expression expression)
    {
        // Implementation of base (RelationalQueryTranslationPreprocessor).
        expression = new RelationalQueryMetadataExtractingExpressionVisitor(_relationalQueryCompilationContext).Visit(expression);

        // Implementation of base.base (QueryTranslationPreprocessor), using `XGQueryableMethodNormalizingExpressionVisitor` instead of
        // `QueryableMethodNormalizingExpressionVisitor` directly.
        expression = new XGQueryableMethodNormalizingExpressionVisitor(QueryCompilationContext).Normalize(expression);
        expression = ProcessQueryRoots(expression);

        return expression;
    }
}
