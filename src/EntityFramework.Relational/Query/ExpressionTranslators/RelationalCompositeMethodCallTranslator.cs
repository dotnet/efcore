// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Query.ExpressionTranslators
{
    public abstract class RelationalCompositeMethodCallTranslator : IMethodCallTranslator
    {
        private readonly List<IMethodCallTranslator> _translators;

        public RelationalCompositeMethodCallTranslator([NotNull] ILoggerFactory loggerFactory)
        {
            _translators = new List<IMethodCallTranslator>
            {
                new ContainsTranslator(),
                new EndsWithTranslator(),
                new EqualsTranslator(loggerFactory),
                new StartsWithTranslator()
            };
        }

        public virtual Expression Translate(MethodCallExpression expression)
        {
            foreach (var translator in _translators)
            {
                var translatedMethodCall = translator.Translate(expression);
                if (translatedMethodCall != null)
                {
                    return translatedMethodCall;
                }
            }

            return null;
        }

        protected virtual void AddTranslators([NotNull] IEnumerable<IMethodCallTranslator> translators)
        {
            _translators.AddRange(translators);
        }
    }
}
