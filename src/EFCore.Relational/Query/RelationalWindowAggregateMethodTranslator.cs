// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class RelationalWindowAggregateMethodTranslator : IWindowAggregateMethodCallTranslator
{
    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public RelationalWindowAggregateMethodTranslator(ISqlExpressionFactory sqlExpressionFactory)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression? Translate(MethodInfo method, IReadOnlyList<SqlExpression> arguments, IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        var methodInfo = method.IsGenericMethod
         ? method.GetGenericMethodDefinition()
         : method;

        //todo find better way to make sure we are dealing with the correct method
        //todo - dictionary instead of switch?
        switch (methodInfo.Name)
        {
            case nameof(RelationalWindowAggregateFunctionExtensions.Average)
                when methodInfo == RelationalWindowAggregateMethods.Average:

                return _sqlExpressionFactory.Function("AVG", arguments, false, [false], arguments[0].Type, arguments[0].TypeMapping);

            case nameof(RelationalWindowAggregateFunctionExtensions.Average)
                when methodInfo == RelationalWindowAggregateMethods.AverageFilter:

                return _sqlExpressionFactory.Function("AVG", BuildCaseExpression(arguments), true, [false], arguments[0].Type, arguments[0].TypeMapping);

            case nameof(RelationalWindowAggregateFunctionExtensions.Count)
                when methodInfo == RelationalWindowAggregateMethods.CountAll:

                return _sqlExpressionFactory.Function("COUNT", [_sqlExpressionFactory.Fragment("*")], false, [false], typeof(int));

            case nameof(RelationalWindowAggregateFunctionExtensions.Count)
                when methodInfo == RelationalWindowAggregateMethods.CountAllFilter:

                return _sqlExpressionFactory.Function("COUNT", BuildCaseExpression(arguments, _sqlExpressionFactory.Constant("1")), true, [false], typeof(int));

            case nameof(RelationalWindowAggregateFunctionExtensions.Count)
                when methodInfo == RelationalWindowAggregateMethods.CountCol:

                return _sqlExpressionFactory.Function("COUNT", arguments, false, [false], typeof(int));

            case nameof(RelationalWindowAggregateFunctionExtensions.Count)
                when methodInfo == RelationalWindowAggregateMethods.CountColFilter:

                return _sqlExpressionFactory.Function("COUNT", BuildCaseExpression(arguments), true, [false], typeof(int));

            case nameof(RelationalWindowAggregateFunctionExtensions.CumeDist)
                when methodInfo == RelationalWindowAggregateMethods.CumeDist:

                return _sqlExpressionFactory.Function("CUME_DIST", Enumerable.Empty<SqlExpression>(), false, [], typeof(double));

            case nameof(RelationalWindowAggregateFunctionExtensions.DenseRank)
                when methodInfo == RelationalWindowAggregateMethods.DenseRank:

                return _sqlExpressionFactory.Function("DENSE_RANK", Enumerable.Empty<SqlExpression>(), false, [], typeof(long));

            case nameof(RelationalWindowAggregateFunctionExtensions.FirstValue)
                when methodInfo == RelationalWindowAggregateMethods.FirstValueFrameResults:

            case nameof(RelationalWindowAggregateFunctionExtensions.FirstValue)
                when methodInfo == RelationalWindowAggregateMethods.FirstValueOrderThen:

                return _sqlExpressionFactory.Function("FIRST_VALUE", arguments, true, [false], arguments[0].Type, arguments[0].TypeMapping);

            case nameof(RelationalWindowAggregateFunctionExtensions.Lag)
                when methodInfo == RelationalWindowAggregateMethods.Lag:

                return _sqlExpressionFactory.Function("LAG", arguments, true, [false, false, false], arguments[0].Type, arguments[0].TypeMapping);

            case nameof(RelationalWindowAggregateFunctionExtensions.LastValue)
                when methodInfo == RelationalWindowAggregateMethods.LastValueOrderThen:

            case nameof(RelationalWindowAggregateFunctionExtensions.LastValue)
                when methodInfo == RelationalWindowAggregateMethods.LastValueFrameResults:

                return _sqlExpressionFactory.Function("LAST_VALUE", arguments, true, [false], arguments[0].Type, arguments[0].TypeMapping);

            case nameof(RelationalWindowAggregateFunctionExtensions.Lead)
                when methodInfo == RelationalWindowAggregateMethods.Lead:

                return _sqlExpressionFactory.Function("LEAD", arguments, true, [false, false, false], arguments[0].Type, arguments[0].TypeMapping);

            case nameof(RelationalWindowAggregateFunctionExtensions.Max)
                when methodInfo == RelationalWindowAggregateMethods.Max:

                return _sqlExpressionFactory.Function("MAX", arguments, false, [false], arguments[0].Type, arguments[0].TypeMapping);

            case nameof(RelationalWindowAggregateFunctionExtensions.Max)
                when methodInfo == RelationalWindowAggregateMethods.MaxFilter:

                return _sqlExpressionFactory.Function("MAX", BuildCaseExpression(arguments), true, [false], arguments[0].Type, arguments[0].TypeMapping);

            case nameof(RelationalWindowAggregateFunctionExtensions.Min)
                when methodInfo == RelationalWindowAggregateMethods.Min:

                return _sqlExpressionFactory.Function("MIN", arguments, false, [false], arguments[0].Type, arguments[0].TypeMapping);

            case nameof(RelationalWindowAggregateFunctionExtensions.Min)
                when methodInfo == RelationalWindowAggregateMethods.MinFilter:

                return _sqlExpressionFactory.Function("MIN", BuildCaseExpression(arguments), true, [false], arguments[0].Type, arguments[0].TypeMapping);

            case nameof(RelationalWindowAggregateFunctionExtensions.NTile)
                when methodInfo == RelationalWindowAggregateMethods.NTile:

                return _sqlExpressionFactory.Function("NTILE", arguments, false, [false], typeof(long));

            case nameof(RelationalWindowAggregateFunctionExtensions.PercentRank)
                when methodInfo == RelationalWindowAggregateMethods.PercentRank:

                return _sqlExpressionFactory.Function("PERCENT_RANK", Enumerable.Empty<SqlExpression>(), false, [], typeof(double));

            case nameof(RelationalWindowAggregateFunctionExtensions.Rank)
                when methodInfo == RelationalWindowAggregateMethods.Rank:

                return _sqlExpressionFactory.Function("RANK", Enumerable.Empty<SqlExpression>(), false, [], typeof(long));

            case nameof(RelationalWindowAggregateFunctionExtensions.RowNumber)
                when methodInfo == RelationalWindowAggregateMethods.RowNumber:

                return _sqlExpressionFactory.Function("ROW_NUMBER", Enumerable.Empty<SqlExpression>(), false, [], typeof(long));

            case nameof(RelationalWindowAggregateFunctionExtensions.Sum)
                when methodInfo == RelationalWindowAggregateMethods.Sum:

                return _sqlExpressionFactory.Function("SUM", arguments, false, [false], arguments[0].Type, arguments[0].TypeMapping);

            case nameof(RelationalWindowAggregateFunctionExtensions.Sum)
                when methodInfo == RelationalWindowAggregateMethods.SumFilter:

                return _sqlExpressionFactory.Function("SUM", BuildCaseExpression(arguments), true, [false], arguments[0].Type, arguments[0].TypeMapping);
        }

        return null;
    }

    /// <summary>
    /// todo
    /// </summary>
    /// <param name="result">todo</param>
    /// <param name="arguments">todo</param>
    /// <returns>todo</returns>
    protected virtual SqlExpression[] BuildCaseExpression(IReadOnlyList<SqlExpression> arguments, SqlExpression? result = null)
        => [_sqlExpressionFactory.Case([new CaseWhenClause(ProcessCaseWhen(arguments[result == null ? 1 : 0]), result ?? arguments[0])], _sqlExpressionFactory.Constant(null, typeof(object)))] ;


    /// <summary>
    /// todo
    /// </summary>
    /// <param name="whenExpression">todo</param>
    /// <returns>todo</returns>
    protected virtual SqlExpression ProcessCaseWhen(SqlExpression whenExpression)
    {
        if(whenExpression is SqlBinaryExpression { Left : InExpression inExpression, Right : SqlConstantExpression constantExpression })
        {
            return constantExpression.Value as bool? == true
                ? inExpression
                : _sqlExpressionFactory.Not(inExpression);
        }

        return whenExpression;
    }
}
