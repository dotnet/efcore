// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Parsing.ExpressionVisitors;

namespace Microsoft.EntityFrameworkCore.Query.Expressions.Internal
{
    /// <summary>
    ///     Expression representing null-conditional access.
    ///     Logic in this file is based on https://github.com/bartdesmet/ExpressionFutures
    /// </summary>
    public class NullConditionalExpression : Expression, IPrintable
    {
        private readonly Type _type;

        /// <summary>
        ///     Creates a new instance of NullConditionalExpression.
        /// </summary>
        /// <param name="caller"> Expression representing potentially nullable caller that needs to be tested for it's nullability. </param>
        /// <param name="accessOperation"> Expression representing access operation. </param>
        public NullConditionalExpression(
            [NotNull] Expression caller,
            [NotNull] Expression accessOperation)
        {
            Check.NotNull(caller, nameof(caller));
            Check.NotNull(accessOperation, nameof(accessOperation));

            Caller = caller;
            AccessOperation = accessOperation;

            _type = accessOperation.Type.IsNullableType()
                ? accessOperation.Type
                : accessOperation.Type.MakeNullable();
        }

        /// <summary>
        ///     Expression representing potentially nullable caller that needs to be tested for it's nullability.
        /// </summary>
        public virtual Expression Caller { get; }

        /// <summary>
        ///     Expression representing access operation.
        /// </summary>
        public virtual Expression AccessOperation { get; }

        /// <summary>
        ///     Indicates that the node can be reduced to a simpler node. If this returns true,
        ///     Reduce() can be called to produce the reduced form.
        /// </summary>
        public override bool CanReduce => true;

        /// <summary>
        ///     Gets the static type of the expression that this expression represents.
        /// </summary>
        public override Type Type => _type;

        /// <summary>
        ///     Gets the node type of this expression.
        /// </summary>
        public override ExpressionType NodeType => ExpressionType.Extension;

        /// <summary>
        ///     Reduces this node to a simpler expression. If CanReduce returns true, this should
        ///     return a valid expression. This method can return another node which itself must
        ///     be reduced.
        /// </summary>
        public override Expression Reduce()
        {
            var nullableCallerType = Caller.Type;
            var nullableCaller = Parameter(nullableCallerType, "__caller");
            var result = Parameter(_type, "__result");

            var caller = Caller.Type != nullableCaller.Type
                ? (Expression)Convert(nullableCaller, Caller.Type)
                : nullableCaller;

            var operation
                = ReplacingExpressionVisitor
                    .Replace(Caller, caller, AccessOperation);

            if (operation.Type != _type)
            {
                operation = Convert(operation, _type);
            }

            var resultExpression
                = Block(
                    new[] { nullableCaller, result },
                    Assign(nullableCaller, Caller),
                    Assign(result, Default(_type)),
                    IfThen(
                        NotEqual(nullableCaller, Default(nullableCallerType)),
                        Assign(result, operation)),
                    result);

            return resultExpression;
        }

        /// <summary>
        ///     Reduces the node and then calls the visitor delegate on the reduced expression.
        ///     The method throws an exception if the node is not
        ///     reducible.
        /// </summary>
        /// <returns>
        ///     The expression being visited, or an expression which should replace it in the tree.
        /// </returns>
        /// <param name="visitor">An instance of <see cref="T:System.Func`2" />.</param>
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var newCaller = visitor.Visit(Caller);
            var newAccessOperation = visitor.Visit(AccessOperation);

            if (newCaller != Caller
                || newAccessOperation != AccessOperation
                && (newAccessOperation as NullConditionalExpression)?.AccessOperation != AccessOperation)
            {
                return new NullConditionalExpression(newCaller, newAccessOperation);
            }

            return this;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Print(ExpressionPrinter expressionPrinter)
        {
            if (AccessOperation is MemberExpression memberExpression)
            {
                expressionPrinter.Visit(Caller);
                expressionPrinter.StringBuilder.Append("?." + memberExpression.Member.Name);

                return;
            }

            if (AccessOperation is MethodCallExpression methodCallExpression)
            {
                if (methodCallExpression.Object != null)
                {
                    expressionPrinter.Visit(Caller);
                    expressionPrinter.StringBuilder.Append("?." + methodCallExpression.Method.Name + "(");
                    VisitArguments(expressionPrinter, methodCallExpression.Arguments);
                    expressionPrinter.StringBuilder.Append(")");

                    return;
                }
                else if (methodCallExpression.Method.IsEFPropertyMethod())
                {
                    var method = methodCallExpression.Method;

                    expressionPrinter.StringBuilder.Append(method.DeclaringType?.Name + "." + method.Name + "(?");
                    expressionPrinter.Visit(Caller);
                    expressionPrinter.StringBuilder.Append("?, ");
                    expressionPrinter.Visit(methodCallExpression.Arguments[1]);
                    expressionPrinter.StringBuilder.Append(")");

                    return;
                }
            }

            expressionPrinter.StringBuilder.Append("?");
            expressionPrinter.Visit(Caller);
            expressionPrinter.StringBuilder.Append(" | ");
            expressionPrinter.Visit(AccessOperation);
            expressionPrinter.StringBuilder.Append("?");
        }

        private static void VisitArguments(ExpressionPrinter expressionPrinter, IList<Expression> arguments)
        {
            for (var i = 0; i < arguments.Count; i++)
            {
                expressionPrinter.Visit(arguments[i]);
                expressionPrinter.StringBuilder.Append(i == arguments.Count - 1 ? "" : ", ");
            }
        }

        /// <summary>
        ///     Returns a textual representation of the <see cref="T:System.Linq.Expressions.Expression" />.
        /// </summary>
        /// <returns>
        ///     A textual representation of the <see cref="T:System.Linq.Expressions.Expression" />.
        /// </returns>
        public override string ToString()
        {
            if (AccessOperation is MemberExpression memberExpression)
            {
                return Caller + "?." + memberExpression.Member.Name;
            }

            if (AccessOperation is MethodCallExpression methodCallExpression)
            {
                if (methodCallExpression.Object != null)
                {
                    return Caller
                           + "?." + methodCallExpression.Method.Name
                           + "(" + string.Join(",", methodCallExpression.Arguments) + ")";
                }

                var method = methodCallExpression.Method;
                if (method.IsEFPropertyMethod())
                {
                    return method.DeclaringType?.Name + "." + method.Name
                           + "(?" + Caller + "?, " + methodCallExpression.Arguments[1] + ")";
                }
            }

            return $"?{Caller} | {AccessOperation}?";
        }
    }
}
