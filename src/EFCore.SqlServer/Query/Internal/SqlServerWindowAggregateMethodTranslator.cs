// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

/// <summary>
/// todo
/// </summary>
public class SqlServerWindowAggregateMethodTranslator : RelationalWindowAggregateMethodTranslator
{
    /// <summary>
    /// todo
    /// </summary>
    /// <param name="dependencies">todo</param>
    public SqlServerWindowAggregateMethodTranslator(RelationalWindowAggregateMethodTranslatorDependencies dependencies)
        : base(dependencies)
    {
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

                return SqlExpressionFactory.Function("COUNT_BIG", new[] { SqlExpressionFactory.Fragment("*") }, false, [false], typeof(long));

            case nameof(SqlServerWindowAggregateFunctionExtensions.CountBig)
                when methodInfo == SqlServerWindowAggregateMethods.CountBigAllFilter:

                return SqlExpressionFactory.Function("COUNT_BIG", BuildCaseExpression(arguments, SqlExpressionFactory.Constant("1")), true, [false], typeof(long));

            case nameof(SqlServerWindowAggregateFunctionExtensions.CountBig)
                when methodInfo == SqlServerWindowAggregateMethods.CountBigCol:

                return SqlExpressionFactory.Function("COUNT_BIG", arguments, false, [false], typeof(long));

            case nameof(SqlServerWindowAggregateFunctionExtensions.CountBig)
                when methodInfo == SqlServerWindowAggregateMethods.CountBigColFilter:

                return SqlExpressionFactory.Function("COUNT_BIG", BuildCaseExpression(arguments), true, [false], typeof(long));

            case nameof(SqlServerWindowAggregateFunctionExtensions.Stdev)
                when methodInfo == SqlServerWindowAggregateMethods.Stdev:

                return SqlExpressionFactory.Function("STDEV", arguments, true, [false], typeof(double));

            case nameof(SqlServerWindowAggregateFunctionExtensions.Stdev)
                when methodInfo == SqlServerWindowAggregateMethods.StdevFilter:

                return SqlExpressionFactory.Function("STDEV", BuildCaseExpression(arguments), true, [false], typeof(double));

            case nameof(SqlServerWindowAggregateFunctionExtensions.StdevP)
                when methodInfo == SqlServerWindowAggregateMethods.StdevP:

                return SqlExpressionFactory.Function("STDEVP", arguments, true, [false], typeof(double));

            case nameof(SqlServerWindowAggregateFunctionExtensions.StdevP)
                when methodInfo == SqlServerWindowAggregateMethods.StdevPFilter:

                return SqlExpressionFactory.Function("STDEVP", BuildCaseExpression(arguments), true, [false], typeof(double));

            case nameof(SqlServerWindowAggregateFunctionExtensions.Var)
                when methodInfo == SqlServerWindowAggregateMethods.Var:

                return SqlExpressionFactory.Function("VAR", arguments, true, [false], typeof(double));

            case nameof(SqlServerWindowAggregateFunctionExtensions.Var)
                when methodInfo == SqlServerWindowAggregateMethods.VarFilter:

                return SqlExpressionFactory.Function("VAR", BuildCaseExpression(arguments), true, [false], typeof(double));

            case nameof(SqlServerWindowAggregateFunctionExtensions.VarP)
                when methodInfo == SqlServerWindowAggregateMethods.VarP:

                return SqlExpressionFactory.Function("VARP", arguments, true, [false], typeof(double));

            case nameof(SqlServerWindowAggregateFunctionExtensions.VarP)
                when methodInfo == SqlServerWindowAggregateMethods.VarPFilter:

                return SqlExpressionFactory.Function("VARP", BuildCaseExpression(arguments), true, [false], typeof(double));
        }

        return null;
    }
}
