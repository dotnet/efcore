// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Query.ExpressionTranslators
{
    public class RelationalCompositeExpressionFragmentTranslator : IExpressionFragmentTranslator
    {
        private readonly List<IExpressionFragmentTranslator> _translators
            = new List<IExpressionFragmentTranslator>
                {
                    new StringCompareTranslator()
                };

        public virtual Expression Translate(Expression expression)
        {
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

        protected virtual void AddTranslators([NotNull] IEnumerable<IExpressionFragmentTranslator> translators)
            => _translators.AddRange(translators);
    }
}
