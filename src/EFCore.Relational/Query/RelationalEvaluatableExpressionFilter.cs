// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         Represents a filter for evaluatable expressions.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
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
        /// <param name="dependencies"> The dependencies to use. </param>
        /// <param name="relationalDependencies"> The relational-specific dependencies to use. </param>
        public RelationalEvaluatableExpressionFilter(
            EvaluatableExpressionFilterDependencies dependencies,
            RelationalEvaluatableExpressionFilterDependencies relationalDependencies)
            : base(dependencies)
        {
            Check.NotNull(relationalDependencies, nameof(relationalDependencies));

            RelationalDependencies = relationalDependencies;
        }

        /// <summary>
        ///     Dependencies used to create a <see cref="RelationalEvaluatableExpressionFilter" />
        /// </summary>
        protected virtual RelationalEvaluatableExpressionFilterDependencies RelationalDependencies { get; }

        /// <summary>
        ///     Checks whether the given expression can be evaluated.
        /// </summary>
        /// <param name="expression"> The expression. </param>
        /// <param name="model"> The model. </param>
        /// <returns> <see langword="true" /> if the expression can be evaluated; <see langword="false" /> otherwise. </returns>
        public override bool IsEvaluatableExpression(Expression expression, IModel model)
        {
            Check.NotNull(expression, nameof(expression));
            Check.NotNull(model, nameof(model));

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
}
