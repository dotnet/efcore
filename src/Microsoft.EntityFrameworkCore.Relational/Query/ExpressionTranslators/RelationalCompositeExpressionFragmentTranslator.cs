// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators
{
    /// <summary>
    ///     A composite expression fragment translator that dispatches to multiple specialized
    ///     fragment translators.
    /// </summary>
    public class RelationalCompositeExpressionFragmentTranslator : IExpressionFragmentTranslator
    {
        private readonly List<IExpressionFragmentTranslator> _translators
            = new List<IExpressionFragmentTranslator>
            {
                new StringCompareTranslator(),
                new StringConcatTranslator()
            };

        /// <summary>
        ///     Translates the given expression.
        /// </summary>
        /// <param name="expression"> The expression to translate. </param>
        /// <returns>
        ///     A SQL expression representing the translated expression.
        /// </returns>
        public virtual Expression Translate(Expression expression)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var translator in _translators)
            {
                var result = translator.Translate(expression);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        /// <summary>
        ///     Adds additional translators to the dispatch list.
        /// </summary>
        /// <param name="translators"> The translators. </param>
        protected virtual void AddTranslators([NotNull] IEnumerable<IExpressionFragmentTranslator> translators)
            => _translators.InsertRange(0, translators);
    }
}
