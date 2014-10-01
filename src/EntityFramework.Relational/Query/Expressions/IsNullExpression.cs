// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Query.Sql;
using Microsoft.Data.Entity.Relational.Utilities;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;
using System;
using System.Linq.Expressions;

namespace Microsoft.Data.Entity.Relational.Query.Expressions
{
    public class IsNullExpression : ExtensionExpression
    {
        private readonly Expression _operand;

        public IsNullExpression([NotNull] Expression operand)
            : base(Check.NotNull(operand, "operand").Type)
        {
            _operand = operand;
        }

        public virtual Expression Operand
        {
            get { return _operand; }
        }

        public override Expression Accept([NotNull] ExpressionTreeVisitor visitor)
        {
            Check.NotNull(visitor, "visitor");

            var specificVisitor = visitor as ISqlExpressionVisitor;

            if (specificVisitor != null)
            {
                return specificVisitor.VisitIsNullExpression(this);
            }

            return base.Accept(visitor);
        }

        protected override Expression VisitChildren(ExpressionTreeVisitor visitor)
        {
            var newExpression = visitor.VisitExpression(_operand);

            return newExpression != _operand
                ? new IsNullExpression(newExpression)
                : this;
        }

        public override Type Type
        {
            get
            {
                return typeof(bool);
            }
        }
    }
}
