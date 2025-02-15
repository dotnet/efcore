// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     A visitor which adds additional query root nodes during preprocessing.
/// </summary>
public class QueryRootProcessor : ExpressionVisitor
{
    private readonly QueryCompilationContext _queryCompilationContext;

    /// <summary>
    ///     Creates a new instance of the <see cref="QueryRootProcessor" /> class with associated query provider.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this class.</param>
    /// <param name="queryCompilationContext">The query compilation context object to use.</param>
    public QueryRootProcessor(
        QueryTranslationPreprocessorDependencies dependencies,
        QueryCompilationContext queryCompilationContext)
        => _queryCompilationContext = queryCompilationContext;

    /// <inheritdoc />
    protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
    {
        // We'll look for IEnumerable/IQueryable arguments to methods on Enumerable/Queryable, and convert these to constant/parameter query
        // root nodes. These will later get translated to e.g. VALUES (constant) and OPENJSON (parameter) on SQL Server.
        var method = methodCallExpression.Method;
        if (method.DeclaringType != typeof(Queryable)
            && method.DeclaringType != typeof(Enumerable)
            && method.DeclaringType != typeof(EntityFrameworkQueryableExtensions))
        {
            return base.VisitMethodCall(methodCallExpression);
        }

        var parameters = method.GetParameters();

        // Note that we don't need to look at methodCallExpression.Object, since IQueryable<> doesn't declare any methods.
        // All methods over queryable are extensions.
        Expression[]? newArguments = null;

        for (var i = 0; i < methodCallExpression.Arguments.Count; i++)
        {
            var argument = methodCallExpression.Arguments[i];
            var parameterType = parameters[i].ParameterType;

            // This converts collections over constants and parameters to query roots, for later translation of LINQ operators over them.
            // The element type doesn't have to be directly mappable; we allow unknown CLR types in order to support value convertors
            // (the precise type mapping - with the value converter - will be inferred later based on LINQ operators composed on the root).
            // However, we do exclude element CLR types which are associated to entity types in our model, since Contains over entity
            // collections isn't yet supported (#30712).
            var visitedArgument = parameterType.IsGenericType
                && (parameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                    || parameterType.GetGenericTypeDefinition() == typeof(IQueryable<>))
                && parameterType.GetGenericArguments()[0] is var elementClrType
                && !_queryCompilationContext.Model.FindEntityTypes(elementClrType).Any()
                    ? VisitQueryRootCandidate(argument, elementClrType)
                    : Visit(argument);

            if (newArguments is not null)
            {
                newArguments[i] = visitedArgument;
            }
            else if (!ReferenceEquals(visitedArgument, argument))
            {
                newArguments = new Expression[methodCallExpression.Arguments.Count];

                for (var j = 0; j < i; j++)
                {
                    newArguments[j] = methodCallExpression.Arguments[j];
                }

                newArguments[i] = visitedArgument;
            }
        }

        return newArguments is null
            ? methodCallExpression
            : methodCallExpression.Update(methodCallExpression.Object, newArguments);
    }

    private Expression VisitQueryRootCandidate(Expression expression, Type elementClrType)
    {
        var candidateExpression = expression;

        // In case the collection was value type, in order to call methods like AsQueryable,
        // we need to convert it to IEnumerable<T> which requires boxing.
        // We do that with Convert expression which we need to unwrap here.
        if (expression is UnaryExpression { NodeType: ExpressionType.Convert } convertExpression
            && convertExpression.Type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
        {
            candidateExpression = convertExpression.Operand;
        }

        switch (candidateExpression)
        {
            // An array containing only constants is represented as a ConstantExpression with the array as the value.
            // Convert that into a NewArrayExpression for use with InlineQueryRootExpression
            case ConstantExpression { Value: IEnumerable values }:
                var valueExpressions = new List<ConstantExpression>();
                foreach (var value in values)
                {
                    valueExpressions.Add(Expression.Constant(value, elementClrType));
                }

                if (ShouldConvertToInlineQueryRoot(Expression.NewArrayInit(elementClrType, valueExpressions)))
                {
                    return new InlineQueryRootExpression(valueExpressions, elementClrType);
                }

                goto default;

            case NewArrayExpression newArray when ShouldConvertToInlineQueryRoot(newArray):
                return new InlineQueryRootExpression(newArray.Expressions, elementClrType);

            case QueryParameterExpression queryParameter when ShouldConvertToParameterQueryRoot(queryParameter):
                return new ParameterQueryRootExpression(queryParameter.Type.GetSequenceType(), queryParameter);

            case ListInitExpression listInit
                when listInit.Type.TryGetElementType(typeof(IList<>)) is not null
                && listInit.Initializers.All(x => x.Arguments.Count == 1)
                && ShouldConvertToInlineQueryRoot(listInit):
                return new InlineQueryRootExpression(listInit.Initializers.Select(x => x.Arguments[0]).ToList(), elementClrType);

            default:
                return Visit(expression);
        }
    }

    /// <summary>
    ///     Determines whether a <see cref="Expression" /> should be converted to a <see cref="InlineQueryRootExpression" />.
    /// </summary>
    /// <param name="expression">The expression that's a candidate for conversion to a query root.</param>
    protected virtual bool ShouldConvertToInlineQueryRoot(Expression expression)
        => false;

    /// <summary>
    ///     Determines whether a <see cref="QueryParameterExpression" /> should be converted to a
    ///     <see cref="ParameterQueryRootExpression" />.
    /// </summary>
    /// <param name="queryParameterExpression">The query parameter expression that's a candidate for conversion to a query root.</param>
    protected virtual bool ShouldConvertToParameterQueryRoot(QueryParameterExpression queryParameterExpression)
        => false;
}
