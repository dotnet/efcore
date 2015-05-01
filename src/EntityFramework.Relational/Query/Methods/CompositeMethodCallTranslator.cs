// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Relational.Query.Methods
{
    public class CompositeMethodCallTranslator : IMethodCallTranslator
    {
        private readonly ILogger _logger;

        public CompositeMethodCallTranslator([NotNull] ILogger logger)
        {
            Check.NotNull(logger, nameof(logger));

            _logger = logger;
        }

        public virtual Expression Translate(MethodCallExpression methodCallExpression)
            => new EqualsTranslator(_logger).Translate(methodCallExpression)
               ?? new StartsWithTranslator().Translate(methodCallExpression)
               ?? new EndsWithTranslator().Translate(methodCallExpression)
               ?? new ContainsTranslator().Translate(methodCallExpression);
    }
}
