// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

/// <inheritdoc />
public class RelationalEvaluatableExpressionFilter : EvaluatableExpressionFilter
{
    /// <summary>
    ///     <para>
    ///         Creates a new <see cref="RelationalEvaluatableExpressionFilter" /> instance.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <param name="dependencies">The dependencies to use.</param>
    /// <param name="relationalDependencies">The relational-specific dependencies to use.</param>
    public RelationalEvaluatableExpressionFilter(
        EvaluatableExpressionFilterDependencies dependencies,
        RelationalEvaluatableExpressionFilterDependencies relationalDependencies)
        : base(dependencies)
    {
        RelationalDependencies = relationalDependencies;
    }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalEvaluatableExpressionFilterDependencies RelationalDependencies { get; }

    /// <inheritdoc />
    public override bool IsEvaluatableExpression(Expression expression, IModel model)
    {
        if (expression is MethodCallExpression methodCallExpression)
        {
            var method = methodCallExpression.Method;

            if (model.FindDbFunction(method) != null)
            {
                // Never evaluate DbFunction
                // If it is inside lambda then we will have whole method call
                // If it is outside of lambda then it will be evaluated for table valued function already.
                return false;
            }

            if (method.DeclaringType == typeof(RelationalDbFunctionsExtensions))
            {
                return false;
            }
        }

        return base.IsEvaluatableExpression(expression, model);
    }
}
