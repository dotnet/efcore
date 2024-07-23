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
/// todo
/// </summary>
public interface IWindowAggregateMethodCallTranslator
{
    /// <summary>
    /// todo
    /// </summary>
    /// <param name="method">todo</param>
    /// <param name="arguments">todo</param>
    /// <param name="logger">todo</param>
    /// <returns>todo</returns>
    SqlExpression? Translate(
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger);
}
