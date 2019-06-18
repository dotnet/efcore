// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Pipeline
{
    public class SqlConditionalExpression : SqlExpression
    {
        public SqlConditionalExpression(
            SqlExpression test,
            SqlExpression ifTrue,
            SqlExpression ifFalse)
            : base(ifTrue.Type, ifTrue.TypeMapping ?? ifFalse.TypeMapping)
        {
            Test = test;
            IfTrue = ifTrue;
            IfFalse = ifFalse;
        }
        public SqlExpression Test { get; }
        public SqlExpression IfTrue { get; }
        public SqlExpression IfFalse { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var test = (SqlExpression)visitor.Visit(Test);
            var ifTrue = (SqlExpression)visitor.Visit(IfTrue);
            var ifFalse = (SqlExpression)visitor.Visit(IfFalse);

            return Update(test, ifTrue, ifFalse);
        }

        public virtual SqlConditionalExpression Update(
            SqlExpression test,
            SqlExpression ifTrue,
            SqlExpression ifFalse)
            => test != Test || ifTrue != IfTrue || ifFalse != IfFalse
                ? new SqlConditionalExpression(test, ifTrue, ifFalse)
                : this;

        public override void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.StringBuilder.Append("(");
            expressionPrinter.Visit(Test);
            expressionPrinter.StringBuilder.Append(" ? ");
            expressionPrinter.Visit(IfTrue);
            expressionPrinter.StringBuilder.Append(" : ");
            expressionPrinter.Visit(IfFalse);
            expressionPrinter.StringBuilder.Append(")");
        }

        public override bool Equals(object obj)
            => obj != null
            && (ReferenceEquals(this, obj)
                || obj is SqlConditionalExpression sqlConditionalExpression
                    && Equals(sqlConditionalExpression));

        private bool Equals(SqlConditionalExpression sqlConditionalExpression)
            => base.Equals(sqlConditionalExpression)
            && Test.Equals(sqlConditionalExpression.Test)
            && IfTrue.Equals(sqlConditionalExpression.IfTrue)
            && IfFalse.Equals(sqlConditionalExpression.IfFalse);

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Test, IfTrue, IfFalse);
        }
    }
}
