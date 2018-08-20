// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators
{
    /// <summary>
    ///     A base composite method call translator that dispatches to multiple specialized
    ///     method call translators.
    /// </summary>
    public abstract class RelationalCompositeMethodCallTranslator : ICompositeMethodCallTranslator
    {
        private readonly List<IMethodCallTranslator> _methodCallTranslators;

        /// <summary>
        ///     Initializes a new instance of the this class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        protected RelationalCompositeMethodCallTranslator(
            [NotNull] RelationalCompositeMethodCallTranslatorDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;

            _methodCallTranslators
                = new List<IMethodCallTranslator>
                {
                    new EnumHasFlagTranslator(),
                    new EqualsTranslator(dependencies.Logger),
                    new IsNullOrEmptyTranslator(),
                    new LikeTranslator()
                };

            if (!AppContext.TryGetSwitch("Microsoft.EntityFrameworkCore.Issue10153", out var isEnabled) || !isEnabled)
            {
                _methodCallTranslators.Add(new GetValueOrDefaultTranslator());
            }
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual RelationalCompositeMethodCallTranslatorDependencies Dependencies { get; }

        /// <summary>
        ///     Translates the given method call expression.
        /// </summary>
        /// <param name="methodCallExpression"> The method call expression. </param>
        /// <param name="model"> The current model. </param>
        /// <returns>
        ///     A SQL expression representing the translated MethodCallExpression.
        /// </returns>
        public virtual Expression Translate(MethodCallExpression methodCallExpression, IModel model)
            => ((IMethodCallTranslator)model.Relational().FindDbFunction(methodCallExpression.Method))?.Translate(methodCallExpression)
               ?? _methodCallTranslators
                   .Select(translator => translator.Translate(methodCallExpression))
                   .FirstOrDefault(translatedMethodCall => translatedMethodCall != null);

        /// <summary>
        ///     Adds additional translators to the dispatch list.
        /// </summary>
        /// <param name="translators"> The translators. </param>
        protected virtual void AddTranslators([NotNull] IEnumerable<IMethodCallTranslator> translators)
            => _methodCallTranslators.InsertRange(0, translators);
    }
}
