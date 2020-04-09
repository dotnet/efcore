// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    public class CaseExpression : SqlExpression
    {
        private readonly List<CaseWhenClause> _whenClauses = new List<CaseWhenClause>();

        public CaseExpression(
            [NotNull] SqlExpression operand,
            [NotNull] IReadOnlyList<CaseWhenClause> whenClauses)
            : this(operand, whenClauses, null)
        {
            Check.NotNull(operand, nameof(operand));
        }

        public CaseExpression(
            [NotNull] IReadOnlyList<CaseWhenClause> whenClauses,
            [CanBeNull] SqlExpression elseResult)
            : this(null, whenClauses, elseResult)
        {
        }

        public CaseExpression(
            [CanBeNull] SqlExpression operand,
            [NotNull] IReadOnlyList<CaseWhenClause> whenClauses,
            [CanBeNull] SqlExpression elseResult)
            : base(whenClauses[0].Result.Type, whenClauses[0].Result.TypeMapping)
        {
            Check.NotNull(whenClauses, nameof(whenClauses));

            Operand = operand;
            _whenClauses.AddRange(whenClauses);
            ElseResult = elseResult;
        }

        public virtual SqlExpression Operand { get; }
        public virtual IReadOnlyList<CaseWhenClause> WhenClauses => _whenClauses;
        public virtual SqlExpression ElseResult { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var operand = (SqlExpression)visitor.Visit(Operand);
            var changed = operand != Operand;
            var whenClauses = new List<CaseWhenClause>();
            foreach (var whenClause in WhenClauses)
            {
                var test = (SqlExpression)visitor.Visit(whenClause.Test);
                var result = (SqlExpression)visitor.Visit(whenClause.Result);

                if (test != whenClause.Test
                    || result != whenClause.Result)
                {
                    changed |= true;
                    whenClauses.Add(new CaseWhenClause(test, result));
                }
                else
                {
                    whenClauses.Add(whenClause);
                }
            }

            var elseResult = (SqlExpression)visitor.Visit(ElseResult);
            changed |= elseResult != ElseResult;

            return changed
                ? new CaseExpression(operand, whenClauses, elseResult)
                : this;
        }

        public virtual CaseExpression Update(
            [CanBeNull] SqlExpression operand,
            [CanBeNull] IReadOnlyList<CaseWhenClause> whenClauses,
            [CanBeNull] SqlExpression elseResult)
            => operand != Operand || !whenClauses.SequenceEqual(WhenClauses) || elseResult != ElseResult
                ? new CaseExpression(operand, whenClauses, elseResult)
                : this;

        public override void Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

            expressionPrinter.Append("CASE");
            if (Operand != null)
            {
                expressionPrinter.Append(" ");
                expressionPrinter.Visit(Operand);
            }

            using (expressionPrinter.Indent())
            {
                foreach (var whenClause in WhenClauses)
                {
                    expressionPrinter.AppendLine().Append("WHEN ");
                    expressionPrinter.Visit(whenClause.Test);
                    expressionPrinter.Append(" THEN ");
                    expressionPrinter.Visit(whenClause.Result);
                }

                if (ElseResult != null)
                {
                    expressionPrinter.AppendLine().Append("ELSE ");
                    expressionPrinter.Visit(ElseResult);
                }
            }

            expressionPrinter.AppendLine().Append("END");
        }

        public override bool Equals(object obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is CaseExpression caseExpression
                    && Equals(caseExpression));

        private bool Equals(CaseExpression caseExpression)
            => base.Equals(caseExpression)
                && (Operand == null ? caseExpression.Operand == null : Operand.Equals(caseExpression.Operand))
                && WhenClauses.SequenceEqual(caseExpression.WhenClauses)
                && (ElseResult == null ? caseExpression.ElseResult == null : ElseResult.Equals(caseExpression.ElseResult));

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(base.GetHashCode());
            hash.Add(Operand);
            for (var i = 0; i < WhenClauses.Count; i++)
            {
                hash.Add(WhenClauses[i]);
            }

            hash.Add(ElseResult);
            return hash.ToHashCode();
        }
    }
}
