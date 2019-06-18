// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Pipeline;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Pipeline
{
    public class CosmosQueryableMethodTranslatingExpressionVisitorFactory : IQueryableMethodTranslatingExpressionVisitorFactory
    {
        private readonly ISqlExpressionFactory _sqlExpressionFactory;
        private readonly IMemberTranslatorProvider _memberTranslatorProvider;
        private readonly IMethodCallTranslatorProvider _methodCallTranslatorProvider;

        public CosmosQueryableMethodTranslatingExpressionVisitorFactory(
            ISqlExpressionFactory sqlExpressionFactory,
            IMemberTranslatorProvider memberTranslatorProvider,
            IMethodCallTranslatorProvider methodCallTranslatorProvider)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
            _memberTranslatorProvider = memberTranslatorProvider;
            _methodCallTranslatorProvider = methodCallTranslatorProvider;
        }

        public QueryableMethodTranslatingExpressionVisitor Create(IModel model)
        {
            return new CosmosQueryableMethodTranslatingExpressionVisitor(
                model,
                _sqlExpressionFactory,
                _memberTranslatorProvider,
                _methodCallTranslatorProvider);
        }
    }
}
