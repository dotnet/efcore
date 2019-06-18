// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Pipeline
{
    public class SqlFunctionExpression : SqlExpression
    {
        public SqlFunctionExpression(
            string functionName,
            IEnumerable<SqlExpression> arguments,
            Type type,
            CoreTypeMapping typeMapping)
            : base(type, typeMapping)
        {
            FunctionName = functionName;
            Arguments = (arguments ?? Array.Empty<SqlExpression>()).ToList();
        }

        public string FunctionName { get; }
        public IReadOnlyList<SqlExpression> Arguments { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var changed = false;
            var arguments = new SqlExpression[Arguments.Count];
            for (var i = 0; i < arguments.Length; i++)
            {
                arguments[i] = (SqlExpression)visitor.Visit(Arguments[i]);
                changed |= arguments[i] != Arguments[i];
            }

            return changed
                ? new SqlFunctionExpression(
                    FunctionName,
                    arguments,
                    Type,
                    TypeMapping)
                : this;
        }

        public SqlFunctionExpression ApplyTypeMapping(CoreTypeMapping typeMapping)
            => new SqlFunctionExpression(
                FunctionName,
                Arguments,
                Type,
                typeMapping ?? TypeMapping);

        public SqlFunctionExpression Update(IReadOnlyList<SqlExpression> arguments)
            => !arguments.SequenceEqual(Arguments)
                ? new SqlFunctionExpression(FunctionName, arguments, Type, TypeMapping)
                : this;

        public override void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.StringBuilder.Append(FunctionName);
            expressionPrinter.StringBuilder.Append("(");
            expressionPrinter.VisitList(Arguments);
            expressionPrinter.StringBuilder.Append(")");
        }

        public override bool Equals(object obj)
            => obj != null
            && (ReferenceEquals(this, obj)
                || obj is SqlFunctionExpression sqlFunctionExpression
                    && Equals(sqlFunctionExpression));

        private bool Equals(SqlFunctionExpression sqlFunctionExpression)
            => base.Equals(sqlFunctionExpression)
            && string.Equals(FunctionName, sqlFunctionExpression.FunctionName)
            && Arguments.SequenceEqual(sqlFunctionExpression.Arguments);

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(base.GetHashCode());
            hash.Add(FunctionName);
            for (var i = 0; i < Arguments.Count; i++)
            {
                hash.Add(Arguments[i]);
            }
            return hash.ToHashCode();
        }
    }
}
