// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Pipeline
{
    public class SqliteSqlTranslatingExpressionVisitorFactory : RelationalSqlTranslatingExpressionVisitorFactory
    {
        private readonly ISqlExpressionFactory _sqlExpressionFactory;
        private readonly IMemberTranslatorProvider _memberTranslatorProvider;
        private readonly IMethodCallTranslatorProvider _methodCallTranslatorProvider;

        public SqliteSqlTranslatingExpressionVisitorFactory(
            ISqlExpressionFactory sqlExpressionFactory,
            IMemberTranslatorProvider memberTranslatorProvider,
            IMethodCallTranslatorProvider methodCallTranslatorProvider)
            : base(sqlExpressionFactory, memberTranslatorProvider, methodCallTranslatorProvider)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
            _memberTranslatorProvider = memberTranslatorProvider;
            _methodCallTranslatorProvider = methodCallTranslatorProvider;
        }

        public override RelationalSqlTranslatingExpressionVisitor Create(
            IModel model,
            QueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor)
        {
            return new SqliteSqlTranslatingExpressionVisitor(
                model,
                queryableMethodTranslatingExpressionVisitor,
                _sqlExpressionFactory,
                _memberTranslatorProvider,
                _methodCallTranslatorProvider);
        }
    }
}
