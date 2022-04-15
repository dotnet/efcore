// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class TableValuedFunctionToQueryRootConvertingExpressionVisitor : ExpressionVisitor
{
    private readonly IModel _model;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public TableValuedFunctionToQueryRootConvertingExpressionVisitor(IModel model)
    {
        _model = model;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
    {
        var function = _model.FindDbFunction(methodCallExpression.Method);

        return function?.IsScalar == false
            ? CreateTableValuedFunctionQueryRootExpression(function.StoreFunction, methodCallExpression.Arguments)
            : base.VisitMethodCall(methodCallExpression);
    }

    private static Expression CreateTableValuedFunctionQueryRootExpression(
            IStoreFunction function,
            IReadOnlyCollection<Expression> arguments)
        // See issue #19970
        => new TableValuedFunctionQueryRootExpression(function.EntityTypeMappings.Single().EntityType, function, arguments);
}
