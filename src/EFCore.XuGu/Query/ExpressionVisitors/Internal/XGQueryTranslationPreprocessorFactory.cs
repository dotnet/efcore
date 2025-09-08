// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.ExpressionVisitors.Internal;

public class XGQueryTranslationPreprocessorFactory : IQueryTranslationPreprocessorFactory
{
    private readonly QueryTranslationPreprocessorDependencies _dependencies;
    private readonly RelationalQueryTranslationPreprocessorDependencies _relationalDependencies;

    public XGQueryTranslationPreprocessorFactory(
        QueryTranslationPreprocessorDependencies dependencies,
        RelationalQueryTranslationPreprocessorDependencies relationalDependencies)
    {
        _dependencies = dependencies;
        _relationalDependencies = relationalDependencies;
    }

    public virtual QueryTranslationPreprocessor Create(QueryCompilationContext queryCompilationContext)
        => new XGQueryTranslationPreprocessor(
            _dependencies,
            _relationalDependencies,
            queryCompilationContext);
}
