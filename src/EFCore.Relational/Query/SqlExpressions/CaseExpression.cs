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
    /// <summary>
    ///     <para>
    ///         An expression that represents a CASE statement in a SQL tree.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class CaseExpression : SqlExpression
    {
        private readonly List<CaseWhenClause> _whenClauses = new List<CaseWhenClause>();

        /// <summary>
        ///     Creates a new instance of the <see cref="CaseExpression" /> class which represents a simple CASE expression.
        /// </summary>
        /// <param name="operand"> An expression to compare with <see cref="CaseWhenClause.Test" /> in <see cref="WhenClauses" />. </param>
        /// <param name="whenClauses"> A list of <see cref="CaseWhenClause" /> to compare and get result from. </param>
        /// <param name="elseResult"> A value to return if no <see cref="WhenClauses" /> matches, if any. </param>
        public CaseExpression(
            [NotNull] SqlExpression operand,
            [NotNull] IReadOnlyList<CaseWhenClause> whenClauses,
            [CanBeNull] SqlExpression elseResult = null)
            : base(Check.NotEmpty(whenClauses, nameof(whenClauses))[0].Result.Type, whenClauses[0].Result.TypeMapping)
        {
            Check.NotNull(operand, nameof(operand));

            Operand = operand;
            _whenClauses.AddRange(whenClauses);
            ElseResult = elseResult;
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="CaseExpression" /> class which represents a searched CASE expression.
        /// </summary>
        /// <param name="whenClauses"> A list of <see cref="CaseWhenClause" /> to evaluate condition and get result from. </param>
        /// <param name="elseResult"> A value to return if no <see cref="WhenClauses" /> matches, if any. </param>
        public CaseExpression(
            [NotNull] IReadOnlyList<CaseWhenClause> whenClauses,
            [CanBeNull] SqlExpression elseResult = null)
            : base(Check.NotEmpty(whenClauses, nameof(whenClauses))[0].Result.Type, whenClauses[0].Result.TypeMapping)
        {
            _whenClauses.AddRange(whenClauses);
            ElseResult = elseResult;
        }

        /// <summary>
        ///     The value to compare in <see cref="WhenClauses" />.
        /// </summary>
        public virtual SqlExpression Operand { get; }

        /// <summary>
        ///     The list of <see cref="CaseWhenClause" /> to match <see cref="Operand" /> or evaluate condition to get result.
        /// </summary>
        public virtual IReadOnlyList<CaseWhenClause> WhenClauses
            => _whenClauses;

        /// <summary>
        ///     The value to return if none of the <see cref="WhenClauses" /> matches.
        /// </summary>
        public virtual SqlExpression ElseResult { get; }

        /// <inheritdoc />
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
                ? operand == null
                    ? new CaseExpression(whenClauses, elseResult)
                    : new CaseExpression(operand, whenClauses, elseResult)
                : this;
        }

        /// <summary>
        ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
        ///     return this expression.
        /// </summary>
        /// <param name="operand"> The <see cref="Operand" /> property of the result. </param>
        /// <param name="whenClauses"> The <see cref="WhenClauses" /> property of the result. </param>
        /// <param name="elseResult"> The <see cref="ElseResult" /> property of the result. </param>
        /// <returns> This expression if no children changed, or an expression with the updated children. </returns>
        public virtual CaseExpression Update(
            [CanBeNull] SqlExpression operand,
            [NotNull] IReadOnlyList<CaseWhenClause> whenClauses,
            [CanBeNull] SqlExpression elseResult)
            => operand != Operand || !whenClauses.SequenceEqual(WhenClauses) || elseResult != ElseResult
                ? (Operand == null
                    ? new CaseExpression(whenClauses, elseResult)
                    : new CaseExpression(operand, whenClauses, elseResult))
                : this;

        /// <inheritdoc />
        protected override void Print(ExpressionPrinter expressionPrinter)
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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
