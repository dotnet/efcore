// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Query.Sql;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;
using System;

namespace Microsoft.Data.Entity.Relational.Query.Expressions
{
    public class CaseExpression : ExtensionExpression
    {
        public CaseExpression([NotNull] Expression when, [NotNull] Type type)
            : base(type)
        {
            Check.NotNull(when, nameof(when));

            When = when;
        }

        public virtual Expression When { get; }

        public override Expression Accept([NotNull] ExpressionTreeVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var specificVisitor = visitor as ISqlExpressionVisitor;

            if (specificVisitor != null)
            {
                return specificVisitor.VisitCaseExpression(this);
            }

            return base.Accept(visitor);
        }

        protected override Expression VisitChildren(ExpressionTreeVisitor visitor)
        {
            var when = visitor.VisitExpression(When);

            var conditional = when as ConditionalExpression;
            var type = conditional != null ? conditional.IfTrue.Type : typeof(bool);

            return new CaseExpression(when, type);
        }
    }
}
