// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Microsoft.Data.Entity.Query.ExpressionTranslators
{
    public abstract class RelationalCompositeMethodCallTranslator : IMethodCallTranslator
    {
        private readonly List<IMethodCallTranslator> _methodCallTranslators;

        protected RelationalCompositeMethodCallTranslator([NotNull] ILogger logger)
        {
            _methodCallTranslators 
                = new List<IMethodCallTranslator>
            {
                new ContainsTranslator(),
                new EndsWithTranslator(),
                new EqualsTranslator(logger),
                new StartsWithTranslator()
            };
        }

        public virtual Expression Translate(MethodCallExpression expression)
        {
            return
                _methodCallTranslators
                    .Select(translator => translator.Translate(expression))
                    .FirstOrDefault(translatedMethodCall => translatedMethodCall != null);
        }

        protected virtual void AddTranslators([NotNull] IEnumerable<IMethodCallTranslator> translators)
            => _methodCallTranslators.AddRange(translators);
    }
}
