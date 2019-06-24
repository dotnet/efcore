// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Query.NavigationExpansion
{
    public class MaterializeCollectionNavigationExpression : Expression, IPrintable
    {
        private Type _returnType;
        public MaterializeCollectionNavigationExpression(Expression operand, INavigation navigation)
        {
            Operand = operand;
            Navigation = navigation;
            _returnType = navigation.ClrType;
        }

        public virtual Expression Operand { get; }
        public virtual INavigation Navigation { get; }

        public override ExpressionType NodeType => ExpressionType.Extension;
        public override Type Type => _returnType;
        public override bool CanReduce => false;

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var newOperand = visitor.Visit(Operand);

            return Update(newOperand);
        }

        public virtual MaterializeCollectionNavigationExpression Update(Expression operand)
            => operand != Operand
            ? new MaterializeCollectionNavigationExpression(operand, Navigation)
            : this;

        public virtual void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.StringBuilder.Append($"MATERIALIZE_COLLECTION({ Navigation }, ");
            expressionPrinter.Visit(Operand);
            expressionPrinter.StringBuilder.Append(")");
        }
    }
}
