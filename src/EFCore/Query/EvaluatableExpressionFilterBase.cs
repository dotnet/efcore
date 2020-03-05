// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
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
    public class EvaluatableExpressionFilterBase : IEvaluatableExpressionFilter
    {
        /// <summary>
        ///     Parameter object containing dependencies for this service.
        /// </summary>
        protected virtual EvaluatableExpressionFilterDependencies Dependencies { get; }

        /// <summary>
        ///     <para>
        ///         Creates a new <see cref="EvaluatableExpressionFilter" /> instance.
        ///     </para>
        ///     <para>
        ///         This type is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="dependencies"> The dependencies to use. </param>
        public EvaluatableExpressionFilterBase(
            [NotNull] EvaluatableExpressionFilterDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
        }

        /// <summary>
        ///     Checks whether the given expression can be evaluated.
        /// </summary>
        /// <param name="expression"> The expression. </param>
        /// <param name="model"> The model. </param>
        /// <returns> True if the expression can be evaluated; false otherwise. </returns>
        public virtual bool IsEvaluatableExpression(Expression expression, IModel model)
        {
            Check.NotNull(expression, nameof(expression));
            Check.NotNull(model, nameof(model));

            foreach (var plugin in Dependencies.Plugins)
            {
                if (!plugin.IsEvaluatableExpression(expression))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
