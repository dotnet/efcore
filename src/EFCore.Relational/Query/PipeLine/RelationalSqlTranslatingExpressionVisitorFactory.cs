// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
{
    public class RelationalSqlTranslatingExpressionVisitorFactory : IRelationalSqlTranslatingExpressionVisitorFactory
    {
        private readonly ISqlExpressionFactory _sqlExpressionFactory;
        private readonly IMemberTranslatorProvider _memberTranslatorProvider;
        private readonly IMethodCallTranslatorProvider _methodCallTranslatorProvider;

        public RelationalSqlTranslatingExpressionVisitorFactory(
            ISqlExpressionFactory sqlExpressionFactory,
            IMemberTranslatorProvider memberTranslatorProvider,
            IMethodCallTranslatorProvider methodCallTranslatorProvider)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
            _memberTranslatorProvider = memberTranslatorProvider;
            _methodCallTranslatorProvider = methodCallTranslatorProvider;
        }

        public virtual RelationalSqlTranslatingExpressionVisitor Create(IModel model)
        {
            return new RelationalSqlTranslatingExpressionVisitor(
                model,
                _sqlExpressionFactory,
                _memberTranslatorProvider,
                _methodCallTranslatorProvider);
        }
    }
}
