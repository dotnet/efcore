// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
/// todo
/// </summary>
public class WindowBuilderExpressionFactory : IWindowBuilderExpressionFactory
{
    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    /// todo
    /// </summary>
    /// <param name="sqlExpressionFactory">todo</param>
    public WindowBuilderExpressionFactory(ISqlExpressionFactory sqlExpressionFactory)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
    }

    /// <summary>
    /// todo
    /// </summary>
    /// <returns>todo</returns>
    public RelationalWindowBuilderExpression CreateWindowBuilder()
    {
        return new RelationalWindowBuilderExpression(_sqlExpressionFactory);
    }
}
