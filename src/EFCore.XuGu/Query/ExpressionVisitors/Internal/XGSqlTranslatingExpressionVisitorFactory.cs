// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.XuGu.Query.ExpressionTranslators.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.ExpressionVisitors.Internal
{
    public class XGSqlTranslatingExpressionVisitorFactory : IRelationalSqlTranslatingExpressionVisitorFactory
    {
        private readonly RelationalSqlTranslatingExpressionVisitorDependencies _dependencies;
        private readonly IXGJsonPocoTranslator _jsonPocoTranslator;

        public XGSqlTranslatingExpressionVisitorFactory(
            [NotNull] RelationalSqlTranslatingExpressionVisitorDependencies dependencies,
            [NotNull] IServiceProvider serviceProvider)
        {
            _dependencies = dependencies;
            _jsonPocoTranslator = serviceProvider.GetService<IXGJsonPocoTranslator>();
        }

        public virtual RelationalSqlTranslatingExpressionVisitor Create(
            QueryCompilationContext queryCompilationContext,
            QueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor)
            => new XGSqlTranslatingExpressionVisitor(
                _dependencies,
                queryCompilationContext,
                queryableMethodTranslatingExpressionVisitor,
                _jsonPocoTranslator);
    }
}
