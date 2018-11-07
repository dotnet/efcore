// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Expressions
{
    /// <summary>
    ///     Represents a SQL CASE expression.
    /// </summary>
    public class CaseExpression : Expression, IPrintable
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CaseExpression"/> class.
        /// </summary>
        /// <param name="whenClauses"> The list of when clauses. </param>
        public CaseExpression([NotNull] params CaseWhenClause[] whenClauses)
            : this(operand: null, whenClauses, elseResult: null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CaseExpression"/> class.
        /// </summary>
        /// <param name="operand"> The case operand expression. </param>
        /// <param name="whenClauses"> The list of when clauses. </param>
        public CaseExpression([CanBeNull] Expression operand, [NotNull] params CaseWhenClause[] whenClauses)
            : this(operand, whenClauses, elseResult: null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CaseExpression"/> class.
        /// </summary>
        /// <param name="whenClauses"> The list of when clauses. </param>
        /// <param name="elseResult"> The else result expression. </param>
        public CaseExpression([NotNull] IReadOnlyList<CaseWhenClause> whenClauses, [CanBeNull] Expression elseResult)
            : this(operand: null, whenClauses, elseResult)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CaseExpression"/> class.
        /// </summary>
        /// <param name="operand"> The case operand expression. </param>
        /// <param name="whenClauses"> The list of when clauses. </param>
        /// <param name="elseResult"> The else result expression. </param>
        public CaseExpression(
            [CanBeNull] Expression operand,
            [NotNull] IReadOnlyList<CaseWhenClause> whenClauses,
            [CanBeNull] Expression elseResult)
        {
            Check.NotEmpty(whenClauses, nameof(whenClauses));

            var resultType = whenClauses[0].Result.Type;
            var expectedWhenOperandType = operand?.Type ?? typeof(bool);

            foreach (var whenClause in whenClauses)
            {
                if (operand != null && whenClause.Test.Type != expectedWhenOperandType)
                {
                    throw new ArgumentException(
                        RelationalStrings.CaseWhenClauseTestTypeUnexpected(
                            whenClause.Test.Type,
                            expectedWhenOperandType),
                        nameof(whenClauses));
                }
                if (whenClause.Result.Type != resultType)
                {
                    throw new ArgumentException(
                        RelationalStrings.CaseWhenClauseResultTypeUnexpected(whenClause.Result.Type, resultType),
                        nameof(whenClauses));
                }
            }

            if (elseResult != null && elseResult.Type != resultType)
            {
                throw new ArgumentException(
                    RelationalStrings.CaseElseResultTypeUnexpected(elseResult.Type, resultType),
                    nameof(elseResult));
            }

            Type = resultType;
            Operand = operand;
            ElseResult = elseResult;
            WhenClauses = whenClauses;
        }

        /// <summary>
        ///     Gets the case operand expression.
        /// </summary>
        public virtual Expression Operand { get; }

        /// <summary>
        ///     Gets the list of when clauses.
        /// </summary>
        public virtual IReadOnlyList<CaseWhenClause> WhenClauses { get; }

        /// <summary>
        ///     Gets the else result expression.
        /// </summary>
        public virtual Expression ElseResult { get; }

        /// <summary>
        ///      Gets the node type of this <see cref="Expression"/>.
        /// </summary>
        /// <value> One of the <see cref="ExpressionType"/> values. </value>
        public override ExpressionType NodeType
            => ExpressionType.Extension;

        /// <summary>
        ///     Gets the static type of the expression that this <see cref="Expression"/> represents.
        /// </summary>
        /// <value> The <see cref="Type"/> that represents the static type of the expression. </value>
        public override Type Type { get; }

        /// <summary>
        ///     Dispatches to the specific visit method for this node type.
        /// </summary>
        /// <param name="visitor"> The visitor to visit this node with. </param>
        /// <returns> The result of visiting this node. </returns>
        protected override Expression Accept(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            return visitor is ISqlExpressionVisitor sqlExpressionVisitor
                ? sqlExpressionVisitor.VisitCase(this)
                : base.Accept(visitor);
        }

        /// <summary>
        ///     Reduces the node and then calls the visitor delegate on the reduced expression. The method throws an
        ///     exception if the node is not reducible.
        /// </summary>
        /// <param name="visitor"> The visitor. </param>
        /// <returns> The expression being visited, or an expression which should replace it in the tree. </returns>
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var newOperand = visitor.Visit(Operand);

            var whenThenListChanged = false;
            var newWhenThenList = new List<CaseWhenClause>();
            foreach (var whenClause in WhenClauses)
            {
                var newTest = visitor.Visit(whenClause.Test);
                var newResult = visitor.Visit(whenClause.Result);
                var newWhenThen = newTest != whenClause.Test || newResult != whenClause.Result
                    ? new CaseWhenClause(newTest, newResult)
                    : whenClause;

                newWhenThenList.Add(newWhenThen);
                whenThenListChanged |= newWhenThen != whenClause;
            }

            var newElseResult = visitor.Visit(ElseResult);

            return newOperand != Operand || whenThenListChanged || newElseResult != ElseResult
                ? new CaseExpression(newOperand, newWhenThenList, newElseResult)
                : this;
        }

        /// <summary>
        ///     Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj"> The object to compare with the current object. </param>
        /// <returns> true if the specified object is equal to the current object; otherwise, false. </returns>
        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            return ReferenceEquals(this, obj)
                ? true
                : obj is CaseExpression caseExpression && Equals(caseExpression);
        }

        private bool Equals(CaseExpression other)
            => (Operand == null
                    ? other.Operand == null
                    : Operand.Equals(other.Operand))
                && Enumerable.SequenceEqual(WhenClauses, other.WhenClauses, EqualityComparer<CaseWhenClause>.Default)
                && (ElseResult == null
                    ? other.ElseResult == null
                    : ElseResult.Equals(other.ElseResult));

        /// <summary>
        ///     Gets a hash code for the current object.
        /// </summary>
        /// <returns> The hash code. </returns>
        public override int GetHashCode()
        {
            var hashCode = Operand?.GetHashCode() ?? 0;
            hashCode = WhenClauses.Aggregate(hashCode, (current, value) => (current * 397) ^ value.GetHashCode());
            hashCode = (hashCode * 397) ^ (ElseResult?.GetHashCode() ?? 0);

            return hashCode;
        }

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns> The string. </returns>
        public override string ToString()
            => "CASE " + (Operand.ToString() ?? WhenClauses[0].ToString()) + " ... END";

        void IPrintable.Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.StringBuilder.Append("CASE");

            if (Operand != null)
            {
                expressionPrinter.StringBuilder.Append(" ");
                expressionPrinter.Visit(Operand);
            }

            expressionPrinter.StringBuilder.IncrementIndent();

            foreach (var whenClause in WhenClauses)
            {
                expressionPrinter.StringBuilder.AppendLine();
                expressionPrinter.StringBuilder.Append("WHEN ");
                expressionPrinter.Visit(whenClause.Test);
                expressionPrinter.StringBuilder.Append(" THEN ");
                expressionPrinter.Visit(whenClause.Result);
            }

            if (ElseResult != null)
            {
                expressionPrinter.StringBuilder.AppendLine();
                expressionPrinter.StringBuilder.Append("ELSE ");
                expressionPrinter.Visit(ElseResult);
            }

            expressionPrinter.StringBuilder.AppendLine();
            expressionPrinter.StringBuilder.DecrementIndent();
            expressionPrinter.StringBuilder.Append("END");
        }
    }
}
