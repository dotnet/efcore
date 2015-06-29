// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.Methods;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Query
{
    public abstract class RelationalCompositeMethodCallTranslator : IMethodCallTranslator
    {
        private readonly List<IMethodCallTranslator> _relationalTranslators;

        public RelationalCompositeMethodCallTranslator([NotNull] ILoggerFactory loggerFactory)
        {
            _relationalTranslators = new List<IMethodCallTranslator>
            {
                new ContainsTranslator(),
                new EndsWithTranslator(),
                new EqualsTranslator(loggerFactory),
                new StartsWithTranslator()
            };
        }

        public virtual Expression Translate(MethodCallExpression expression)
        {
            foreach (var translator in Translators)
            {
                var translatedMethodCall = translator.Translate(expression);
                if (translatedMethodCall != null)
                {
                    return translatedMethodCall;
                }
            }

            return null;
        }

        protected virtual IReadOnlyList<IMethodCallTranslator> Translators => _relationalTranslators;
    }
}
