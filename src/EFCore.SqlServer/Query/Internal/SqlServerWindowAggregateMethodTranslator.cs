// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

/// <summary>
/// todo
/// </summary>
public class SqlServerWindowAggregateMethodTranslator : RelationalWindowAggregateMethodTranslator
{
    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    /// todo
    /// </summary>
    /// <param name="sqlExpressionFactory">todo</param>
    public SqlServerWindowAggregateMethodTranslator(ISqlExpressionFactory sqlExpressionFactory)
        : base(sqlExpressionFactory)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
    }

    /// <summary>
    /// todo
    /// </summary>
    /// <param name="method">todo</param>
    /// <param name="arguments">todo</param>
    /// <param name="logger">todo</param>
    /// <returns>todo</returns>
    public override SqlExpression? Translate(MethodInfo method, IReadOnlyList<SqlExpression> arguments, IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        var translation = base.Translate(method, arguments, logger);

        if (translation != null)
            return translation;

        var methodInfo = method.IsGenericMethod
        ? method.GetGenericMethodDefinition()
        : method;

        switch (methodInfo.Name)
        {
            case nameof(SqlServerWindowAggregateFunctionExtensions.CountBig)
                when methodInfo == SqlServerWindowAggregateMethods.CountBigAll:

                return _sqlExpressionFactory.Function("COUNT_BIG", new[] { _sqlExpressionFactory.Fragment("*") }, false, [false], typeof(long));

            case nameof(SqlServerWindowAggregateFunctionExtensions.CountBig)
                when methodInfo == SqlServerWindowAggregateMethods.CountBigAllFilter:

                return _sqlExpressionFactory.Function("COUNT_BIG", BuildCaseExpression(arguments, _sqlExpressionFactory.Constant("1")), true, [false], typeof(long));

            case nameof(SqlServerWindowAggregateFunctionExtensions.CountBig)
                when methodInfo == SqlServerWindowAggregateMethods.CountBigCol:

                return _sqlExpressionFactory.Function("COUNT_BIG", arguments, false, [false], typeof(long));

            case nameof(SqlServerWindowAggregateFunctionExtensions.CountBig)
                when methodInfo == SqlServerWindowAggregateMethods.CountBigColFilter:

                return _sqlExpressionFactory.Function("COUNT_BIG", BuildCaseExpression(arguments), true, [false], typeof(long));

            case nameof(SqlServerWindowAggregateFunctionExtensions.Stdev)
                when methodInfo == SqlServerWindowAggregateMethods.Stdev:

                return _sqlExpressionFactory.Function("STDEV", arguments, true, [false], typeof(double));

            case nameof(SqlServerWindowAggregateFunctionExtensions.Stdev)
                when methodInfo == SqlServerWindowAggregateMethods.StdevFilter:

                return _sqlExpressionFactory.Function("STDEV", BuildCaseExpression(arguments), true, [false], typeof(double));

            case nameof(SqlServerWindowAggregateFunctionExtensions.StdevP)
                when methodInfo == SqlServerWindowAggregateMethods.StdevP:

                return _sqlExpressionFactory.Function("STDEVP", arguments, true, [false], typeof(double));

            case nameof(SqlServerWindowAggregateFunctionExtensions.StdevP)
                when methodInfo == SqlServerWindowAggregateMethods.StdevPFilter:

                return _sqlExpressionFactory.Function("STDEVP", BuildCaseExpression(arguments), true, [false], typeof(double));

            case nameof(SqlServerWindowAggregateFunctionExtensions.Var)
                when methodInfo == SqlServerWindowAggregateMethods.Var:

                return _sqlExpressionFactory.Function("VAR", arguments, true, [false], typeof(double));

            case nameof(SqlServerWindowAggregateFunctionExtensions.Var)
                when methodInfo == SqlServerWindowAggregateMethods.VarFilter:

                return _sqlExpressionFactory.Function("VAR", BuildCaseExpression(arguments), true, [false], typeof(double));

            case nameof(SqlServerWindowAggregateFunctionExtensions.VarP)
                when methodInfo == SqlServerWindowAggregateMethods.VarP:

                return _sqlExpressionFactory.Function("VARP", arguments, true, [false], typeof(double));

            case nameof(SqlServerWindowAggregateFunctionExtensions.VarP)
                when methodInfo == SqlServerWindowAggregateMethods.VarPFilter:

                return _sqlExpressionFactory.Function("VARP", BuildCaseExpression(arguments), true, [false], typeof(double));
        }

        return null;
    }
}
