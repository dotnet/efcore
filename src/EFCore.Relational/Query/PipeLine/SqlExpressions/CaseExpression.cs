// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions
{
    public class CaseExpression : SqlExpression
    {
        private readonly List<CaseWhenClause> _whenClauses = new List<CaseWhenClause>();

        public CaseExpression(
            SqlExpression operand,
            params CaseWhenClause[] whenClauses)
            : this(operand, whenClauses, null)
        {
        }

        public CaseExpression(
            IReadOnlyList<CaseWhenClause> whenClauses,
            SqlExpression elseResult)
            : this(null, whenClauses, elseResult)
        {
        }

        private CaseExpression(
            SqlExpression operand,
            IReadOnlyList<CaseWhenClause> whenClauses,
            SqlExpression elseResult)
            : base(whenClauses[0].Result.Type, whenClauses[0].Result.TypeMapping, false, true)
        {
            Operand = operand?.ConvertToValue(true);
            var testValue = operand != null;
            foreach (var whenClause in whenClauses)
            {
                _whenClauses.Add(
                    new CaseWhenClause(
                        whenClause.Test.ConvertToValue(testValue),
                        whenClause.Result.ConvertToValue(true)));
            }
            ElseResult = elseResult?.ConvertToValue(true);
        }

        private CaseExpression(
            SqlExpression operand,
            IReadOnlyList<CaseWhenClause> whenClauses,
            SqlExpression elseResult,
            bool treatAsValue)
            : base(whenClauses[0].Result.Type, whenClauses[0].Result.TypeMapping, false, treatAsValue)
        {
            Operand = operand;
            _whenClauses.AddRange(whenClauses);
            ElseResult = elseResult;
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var operand = (SqlExpression)visitor.Visit(Operand);
            var changed = operand != Operand;
            var whenClauses = new List<CaseWhenClause>();
            foreach (var whenClause in WhenClauses)
            {
                var test = (SqlExpression)visitor.Visit(whenClause.Test);
                var result = (SqlExpression)visitor.Visit(whenClause.Result);

                if (test != whenClause.Test || result != whenClause.Result)
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
                ? new CaseExpression(Operand, whenClauses, elseResult, ShouldBeValue)
                : this;
        }

        public override SqlExpression ConvertToValue(bool treatAsValue)
        {
            return new CaseExpression(Operand, WhenClauses, ElseResult, treatAsValue);
        }

        public SqlExpression Operand { get; }
        public IReadOnlyList<CaseWhenClause> WhenClauses => _whenClauses;
        public SqlExpression ElseResult { get; }

        public override bool Equals(object obj)
            => obj != null
            && (ReferenceEquals(this, obj)
                || obj is CaseExpression caseExpression
                    && Equals(caseExpression));

        private bool Equals(CaseExpression caseExpression)
            => base.Equals(caseExpression)
            && WhenClauses.SequenceEqual(caseExpression.WhenClauses)
            && ElseResult.Equals(caseExpression.ElseResult);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ WhenClauses.Aggregate(
                    0, (current, value) => current + ((current * 397) ^ value.GetHashCode()));
                hashCode = (hashCode * 397) ^ ElseResult.GetHashCode();

                return hashCode;
            }
        }
    }
}
