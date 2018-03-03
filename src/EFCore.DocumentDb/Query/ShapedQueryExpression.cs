// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class ShapedQueryExpression : Expression
    {
        public override bool CanReduce => true;

        public override ExpressionType NodeType => ExpressionType.Extension;
        private readonly Type _returnType;

        public override Type Type => typeof(IEnumerable<>).MakeGenericType(_returnType);

        public DocumentCommandContext DocumentCommandContext { get; }
        public Expression ShaperExpression { get; }

        public ShapedQueryExpression(
            Type returnType,
            DocumentCommandContext documentCommandContext,
            Expression shaperExpression)
        {
            _returnType = returnType;
            DocumentCommandContext = documentCommandContext;
            ShaperExpression = shaperExpression;
        }

        public override Expression Reduce()
        {
            return Call(
                null,
                typeof(ShapedQueryExpression).GetTypeInfo()
                    .GetDeclaredMethod(nameof(ShapedQuery)).MakeGenericMethod(_returnType),
                EntityQueryModelVisitor.QueryContextParameter,
                Constant(DocumentCommandContext),
                ShaperExpression);
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            return this;
        }

        public static IEnumerable<T> ShapedQuery<T>(
            QueryContext queryContext,
            DocumentCommandContext documentCommandContext,
            IShaper<T> shaper)
        {
            return new QueryingEnumerable<T>(
                (DocumentDbQueryContext)queryContext,
                documentCommandContext,
                shaper);
        }

        public override string ToString()
        {
            var stringBuilder = new IndentedStringBuilder();
            stringBuilder.AppendLine("SelectExpression: ");
            stringBuilder.IncrementIndent();

            var querySqlGenerator = DocumentCommandContext.GetSqlGenerator();
            var sql = querySqlGenerator.GenerateSql();

            var lines = sql.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            foreach (var line in lines)
            {
                stringBuilder.AppendLine(line);
            }

            stringBuilder.DecrementIndent();

            return stringBuilder.ToString();
        }
    }
}
