// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <inheritdoc />
    public class RelationalQueryTranslationPreprocessor : QueryTranslationPreprocessor
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="QueryTranslationPreprocessor" /> class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this class. </param>
        /// <param name="relationalDependencies"> Parameter object containing relational dependencies for this class. </param>
        /// <param name="queryCompilationContext"> The query compilation context object to use. </param>
        public RelationalQueryTranslationPreprocessor(
            [NotNull] QueryTranslationPreprocessorDependencies dependencies,
            [NotNull] RelationalQueryTranslationPreprocessorDependencies relationalDependencies,
            [NotNull] QueryCompilationContext queryCompilationContext)
            : base(dependencies, queryCompilationContext)
        {
            Check.NotNull(relationalDependencies, nameof(relationalDependencies));

            RelationalDependencies = relationalDependencies;
        }

        /// <summary>
        ///     Parameter object containing relational service dependencies.
        /// </summary>
        protected virtual RelationalQueryTranslationPreprocessorDependencies RelationalDependencies { get; }

        /// <inheritdoc />
        public override Expression NormalizeQueryableMethod(Expression expression)
        {
            expression = base.NormalizeQueryableMethod(expression);
            expression = new TableValuedFunctionToQueryRootConvertingExpressionVisitor(QueryCompilationContext.Model).Visit(expression);

            return expression;
        }
    }
}
