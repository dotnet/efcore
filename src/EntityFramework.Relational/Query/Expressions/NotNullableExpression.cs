// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.Data.Entity.Utilities;
using JetBrains.Annotations;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;
using Microsoft.Data.Entity.Relational.Query.Sql;
using System;

namespace Microsoft.Data.Entity.Relational.Query.Expressions
{
    public class NotNullableExpression : ExtensionExpression
    {
        private readonly Expression _operand;

        public NotNullableExpression([NotNull] Expression operand)
            : base(Check.NotNull(operand, nameof(operand)).Type)
        {
            _operand = operand;
        }

        public virtual Expression Operand
        {
            get { return _operand; }
        }

        protected override Expression VisitChildren(ExpressionTreeVisitor visitor)
        {
            var newExpression = visitor.VisitExpression(_operand);

            return newExpression != _operand
                ? new NotNullableExpression(newExpression)
                : this;
        }

        public override bool CanReduce => true;

        public override Expression Reduce()
        {
            return _operand;
        }

        public override Type Type
        {
            get { return typeof(bool); }
        }
    }
}