// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.Internal;

public abstract class XGQueryCompilationContextMethodTranslator : IMethodCallTranslator
{
    private readonly Func<QueryCompilationContext> _queryCompilationContextResolver;

    protected XGQueryCompilationContextMethodTranslator(Func<QueryCompilationContext> queryCompilationContextResolver)
    {
        _queryCompilationContextResolver = queryCompilationContextResolver;
    }

    public virtual SqlExpression Translate(
        SqlExpression instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        => Translate(instance, method, arguments, _queryCompilationContextResolver() ?? throw new InvalidOperationException());

    public abstract SqlExpression Translate(
        SqlExpression instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        QueryCompilationContext queryCompilationContext);
}
