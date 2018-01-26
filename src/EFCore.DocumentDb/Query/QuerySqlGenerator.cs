// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Azure.Documents;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Parsing;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class QuerySqlGenerator : ThrowingExpressionVisitor
    {
        private static QuerySqlGenerationHelper _sqlGenerationHelper = new QuerySqlGenerationHelper();

        private static readonly Dictionary<ExpressionType, string> _operatorMap = new Dictionary<ExpressionType, string>
        {
            { ExpressionType.Equal, " = " },
            { ExpressionType.NotEqual, " <> " },
            { ExpressionType.GreaterThan, " > " },
            { ExpressionType.GreaterThanOrEqual, " >= " },
            { ExpressionType.LessThan, " < " },
            { ExpressionType.LessThanOrEqual, " <= " },
            { ExpressionType.AndAlso, " AND " },
            { ExpressionType.OrElse, " OR " },
            { ExpressionType.Add, " + " },
            { ExpressionType.Subtract, " - " },
            { ExpressionType.Multiply, " * " },
            { ExpressionType.Divide, " / " },
            { ExpressionType.Modulo, " % " },
            { ExpressionType.And, " & " },
            { ExpressionType.Or, " | " },
            { ExpressionType.Coalesce, " ?? " }
        };

        public IDictionary<string, string> ParametersToInclude = new Dictionary<string, string>();

        private readonly SelectExpression _selectExpression;
        public IndentedStringBuilder Sql;

        public QuerySqlGenerator(SelectExpression selectExpression)
        {
            _selectExpression = selectExpression;
        }

        public string GenerateSql()
        {
            Sql = new IndentedStringBuilder();
            Visit(_selectExpression);
            return Sql.ToString();
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (_operatorMap.TryGetValue(node.NodeType, out var op))
            {
                if (node.Type == typeof(bool))
                {
                    if (node.NodeType == ExpressionType.Or)
                    {
                        op = " OR ";
                    }
                    else if (node.NodeType == ExpressionType.And)
                    {
                        op = " AND ";
                    }
                }

                var needParens = node.Left is BinaryExpression;
                if (needParens)
                {
                    Sql.Append("(");
                }
                Visit(node.Left);
                if (needParens)
                {
                    Sql.Append(")");
                }
                Sql.Append(op);

                needParens = node.Right is BinaryExpression;
                if (needParens)
                {
                    Sql.Append("(");
                }
                Visit(node.Right);
                if (needParens)
                {
                    Sql.Append(")");
                }

                return node;
            }

            return node;
        }

        protected override Expression VisitConditional(ConditionalExpression node)
        {
            Sql.Append("(");
            Visit(node.Test);
            Sql.Append(" ? ");
            Visit(node.IfTrue);
            Sql.Append(" : ");
            Visit(node.IfFalse);
            Sql.Append(")");

            return node;
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Not:
                    Sql.Append("NOT(");
                    Visit(node.Operand);
                    Sql.Append(")");
                    return node;

                default:
                    break;
            }

            Visit(node.Operand);

            return node;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            Sql.Append($"@{node.Name}");
            if (!ParametersToInclude.ContainsKey(node.Name))
            {
                ParametersToInclude.Add(node.Name, $"@{node.Name}");
            }

            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Value == null)
            {
                Sql.Append("null");
            }
            else
            {
                Sql.Append(_sqlGenerationHelper.GenerateLiteral(node.Value));
            }

            return node;
        }

        protected override Expression VisitExtension(Expression node)
        {
            switch (node)
            {
                case SelectExpression se:
                    Sql.Append("SELECT ");
                    if (se.Projection.Count == 0)
                    {
                        Sql.Append("1");
                    }
                    else
                    {
                        GenerateList(se.Projection, e => Visit(e), t => t.Append(", "));
                    }
                    Sql.AppendLine();
                    GenerateList(se.Source, e => Visit(e), t => t.Append(Environment.NewLine));
                    Sql.AppendLine();
                    if (se.Predicate != null)
                    {
                        if (se.Predicate is ConstantExpression constantExpression
                            && constantExpression.Value is bool boolValue)
                        {
                            if (!boolValue)
                            {
                                Sql.Append("WHERE 0 = 1");
                            }
                        }
                        else
                        {

                            Sql.Append("WHERE ");
                            Visit(se.Predicate);
                            //Sql.AppendLine();
                        }
                    }
                    break;
                case ColumnExpression ce:
                    // TODO: Escape if the name is not simple using c["Column"]
                    Sql.Append($"{ce.Collection.Alias}.{ce.Name}");
                    break;
                case CollectionExpression coe:
                    Sql.Append($"FROM {coe.CollectionName.Replace(" ", "")} {coe.Alias}");
                    break;
            }

            return node;
        }

        protected virtual void GenerateList<T>(
            [NotNull] IReadOnlyList<T> items,
            [NotNull] Action<T> generationAction,
            [CanBeNull] Action<IndentedStringBuilder> joinAction = null)
        {
            Check.NotNull(items, nameof(items));
            Check.NotNull(generationAction, nameof(generationAction));

            joinAction = joinAction ?? (isb => isb.Append(", "));

            for (var i = 0; i < items.Count; i++)
            {
                if (i > 0)
                {
                    joinAction(Sql);
                }

                generationAction(items[i]);
            }
        }

        public ValueBufferFactory CreateValueBufferFactory()
        {
            return new ValueBufferFactory(CreateValueBuffer(_selectExpression.GetProjectedProperties()));
        }

        private static Func<Document, object[]> CreateValueBuffer(
            IEnumerable<IProperty> properties)
        {
            var documentParameter = Expression.Parameter(typeof(Document), "document");

            return Expression.Lambda<Func<Document, object[]>>(
                    Expression.NewArrayInit(
                        typeof(object),
                        properties
                            .Select(
                                mi =>
                                    CreateGetValueExpression(
                                        documentParameter,
                                        Expression.Constant(mi.Name),
                                        mi.ClrType))),
                    documentParameter)
                .Compile();
        }

        private static Expression CreateGetValueExpression(
            Expression documentExpression,
            Expression propertyNameExpression,
            Type propertyType)
        {
            return Expression.Convert(
                Expression.Call(
                    documentExpression,
                    typeof(Document).GetTypeInfo()
                        .GetMethod(nameof(Document.GetPropertyValue)).MakeGenericMethod(propertyType),
                    propertyNameExpression), typeof(object));
        }

        protected override Exception CreateUnhandledItemException<T>(T unhandledItem, string visitMethod)
        {
            throw new NotImplementedException();
        }
    }
}
