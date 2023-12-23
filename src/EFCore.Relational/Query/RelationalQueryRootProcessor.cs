// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query;

/// <inheritdoc />
public class RelationalQueryRootProcessor : QueryRootProcessor
{
    private readonly IModel _model;

    /// <summary>
    ///     Creates a new instance of the <see cref="RelationalQueryRootProcessor" /> class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this class.</param>
    /// <param name="relationalDependencies">Parameter object containing relational dependencies for this class.</param>
    /// <param name="queryCompilationContext">The query compilation context object to use.</param>
    public RelationalQueryRootProcessor(
        QueryTranslationPreprocessorDependencies dependencies,
        RelationalQueryTranslationPreprocessorDependencies relationalDependencies,
        QueryCompilationContext queryCompilationContext)
        : base(dependencies, queryCompilationContext)
    {
        _model = queryCompilationContext.Model;
    }

    /// <summary>
    ///     Indicates that a <see cref="ConstantExpression" /> can be converted to a <see cref="InlineQueryRootExpression" />;
    ///     the latter will end up in <see cref="RelationalQueryableMethodTranslatingExpressionVisitor.TranslateInlineQueryRoot" /> for
    ///     translation to a SQL <see cref="ValuesExpression" />.
    /// </summary>
    protected override bool ShouldConvertToInlineQueryRoot(NewArrayExpression newArrayExpression)
        => true;

    /// <summary>
    ///     Indicates that a <see cref="ParameterExpression" /> can be converted to a <see cref="ParameterQueryRootExpression" />;
    ///     the latter will end up in <see cref="RelationalQueryableMethodTranslatingExpressionVisitor.TranslatePrimitiveCollection" /> for
    ///     translation to a provider-specific SQL expansion mechanism, e.g. <c>OPENJSON</c> on SQL Server.
    /// </summary>
    protected override bool ShouldConvertToParameterQueryRoot(ParameterExpression constantExpression)
        => true;

    /// <inheritdoc />
    protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
    {
        // Create query root node for table-valued functions
        if (_model.FindDbFunction(methodCallExpression.Method) is { IsScalar: false, StoreFunction: var storeFunction })
        {
            // See issue #19970
            return new TableValuedFunctionQueryRootExpression(
                (IEntityType)storeFunction.EntityTypeMappings.Single().TypeBase,
                storeFunction,
                methodCallExpression.Arguments);
        }

        return base.VisitMethodCall(methodCallExpression);
    }

    /// <inheritdoc />
    protected override Expression VisitExtension(Expression node)
        => node switch
        {
            // We skip FromSqlQueryRootExpression, since that contains the arguments as an object array parameter, and don't want to convert
            // that to a query root
            FromSqlQueryRootExpression e => e,

            _ => base.VisitExtension(node)
        };
}
