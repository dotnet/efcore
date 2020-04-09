// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class RelationalQueryTranslationPreprocessor : QueryTranslationPreprocessor
    {
        public RelationalQueryTranslationPreprocessor(
            [NotNull] QueryTranslationPreprocessorDependencies dependencies,
            [NotNull] RelationalQueryTranslationPreprocessorDependencies relationalDependencies,
            [NotNull] QueryCompilationContext queryCompilationContext)
            : base(dependencies, queryCompilationContext)
        {
            Check.NotNull(relationalDependencies, nameof(relationalDependencies));

            RelationalDependencies = relationalDependencies;
        }

        protected virtual RelationalQueryTranslationPreprocessorDependencies RelationalDependencies { get; }

        public override Expression NormalizeQueryableMethodCall(Expression expression)
        {
            expression = base.NormalizeQueryableMethodCall(expression);
            expression = new QueryableFunctionToQueryRootConvertingExpressionVisitor(QueryCompilationContext.Model).Visit(expression);

            return expression;
        }
    }
}
