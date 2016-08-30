// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators
{
    /// <summary>
    ///     A base composite method call translator that dispatches to multiple specialized
    ///     method call translators.
    /// </summary>
    public abstract class RelationalCompositeMethodCallTranslator : IMethodCallTranslator
    {
        private readonly List<IMethodCallTranslator> _methodCallTranslators;

        /// <summary>
        ///     Specialised constructor for use only by derived class.
        /// </summary>
        /// <param name="logger"> A logger. </param>
        protected RelationalCompositeMethodCallTranslator([NotNull] ILogger logger)
        {
            _methodCallTranslators
                = new List<IMethodCallTranslator>
                {
                    new ContainsTranslator(logger),
                    new EndsWithTranslator(logger),
                    new EnumHasFlagTranslator(),
                    new EqualsTranslator(logger),
                    new IsNullOrEmptyTranslator(),
                    new StartsWithTranslator(logger)
                };
        }

        /// <summary>
        ///     Translates the given method call expression.
        /// </summary>
        /// <param name="methodCallExpression"> The method call expression. </param>
        /// <returns>
        ///     A SQL expression representing the translated MethodCallExpression.
        /// </returns>
        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
            return
                _methodCallTranslators
                    .Select(translator => translator.Translate(methodCallExpression))
                    .FirstOrDefault(translatedMethodCall => translatedMethodCall != null);
        }

        /// <summary>
        ///     Adds additional translators to the dispatch list.
        /// </summary>
        /// <param name="translators"> The translators. </param>
        protected virtual void AddTranslators([NotNull] IEnumerable<IMethodCallTranslator> translators)
            => _methodCallTranslators.InsertRange(0, translators);
    }
}
