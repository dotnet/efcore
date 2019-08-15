// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     Expression representing null-conditional access.
    ///     Logic in this file is based on https://github.com/bartdesmet/ExpressionFutures
    /// </summary>
    public class NullConditionalExpression : Expression, IPrintableExpression
    {
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

            Type = accessOperation.Type.IsNullableType()
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
        public override Type Type { get; }

        /// <summary>
        ///     Gets the node type of this expression.
        /// </summary>
        public sealed override ExpressionType NodeType => ExpressionType.Extension;

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
            => Update(visitor.Visit(Caller), visitor.Visit(AccessOperation));

        /// <summary>
        ///     Reduces this node to a simpler expression. If CanReduce returns true, this should
        ///     return a valid expression. This method can return another node which itself must
        ///     be reduced.
        /// </summary>
        public override Expression Reduce()
        {
            var nullableCallerType = Caller.Type;
            var nullableCaller = Parameter(nullableCallerType, "__caller");
            var result = Parameter(Type, "__result");

            var caller = Caller.Type != nullableCaller.Type
                ? (Expression)Convert(nullableCaller, Caller.Type)
                : nullableCaller;

            var operation
                = ReplacingExpressionVisitor
                    .Replace(Caller, caller, AccessOperation);

            if (operation.Type != Type)
            {
                operation = Convert(operation, Type);
            }

            return Block(
                new[] { nullableCaller, result },
                Assign(nullableCaller, Caller),
                Assign(result, Default(Type)),
                IfThen(
                    NotEqual(nullableCaller, Default(nullableCallerType)),
                    Assign(result, operation)),
                result);
        }

        public virtual Expression Update(Expression newCaller, Expression newAccessOperation)
            => newCaller != Caller || newAccessOperation != AccessOperation
               && !ExpressionEqualityComparer.Instance.Equals(
                   (newAccessOperation as NullConditionalExpression)?.AccessOperation, AccessOperation)
                ? new NullConditionalExpression(newCaller, newAccessOperation)
                : this;

        /// <summary>
        ///     Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other">
        ///     The object to compare with the current object.
        /// </param>
        /// <returns>
        ///     True if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        protected virtual bool Equals([CanBeNull] NullConditionalExpression other)
            => Equals(AccessOperation, other?.AccessOperation);

        /// <summary>
        ///     Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">
        ///     The object to compare with the current object.
        /// </param>
        /// <returns>
        ///     True if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
            => obj != null
               && (obj == this
                   || obj.GetType() == GetType()
                   && Equals((NullConditionalExpression)obj));

        /// <summary>
        ///     Serves as the default hash function.
        /// </summary>
        /// <returns>
        ///     A hash code for the current object.
        /// </returns>
        public override int GetHashCode()
            => AccessOperation.GetHashCode();

        public virtual void Print(ExpressionPrinter expressionPrinter)
        {
            if (AccessOperation is MemberExpression memberExpression)
            {
                expressionPrinter.Visit(Caller);
                expressionPrinter.Append("?." + memberExpression.Member.Name);

                return;
            }

            if (AccessOperation is MethodCallExpression methodCallExpression)
            {
                if (methodCallExpression.Object != null)
                {
                    expressionPrinter.Visit(Caller);
                    expressionPrinter.Append("?." + methodCallExpression.Method.Name + "(");
                    VisitArguments(expressionPrinter, methodCallExpression.Arguments);
                    expressionPrinter.Append(")");

                    return;
                }

                if (methodCallExpression.TryGetEFPropertyArguments(out _, out var propertyName))
                {
                    var method = methodCallExpression.Method;

                    expressionPrinter.Append(method.DeclaringType?.Name + "." + method.Name + "(?");
                    expressionPrinter.Visit(Caller);
                    expressionPrinter.Append("?, ");
                    expressionPrinter.Visit(Constant(propertyName));
                    expressionPrinter.Append(")");

                    return;
                }
            }

            expressionPrinter.Append("?");
            expressionPrinter.Visit(Caller);
            expressionPrinter.Append(" | ");
            expressionPrinter.Visit(AccessOperation);
            expressionPrinter.Append("?");
        }

        private static void VisitArguments(ExpressionPrinter expressionPrinter, IReadOnlyList<Expression> arguments)
        {
            for (var i = 0; i < arguments.Count; i++)
            {
                expressionPrinter.Visit(arguments[i]);
                expressionPrinter.Append(i == arguments.Count - 1 ? "" : ", ");
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

                if (methodCallExpression.TryGetEFPropertyArguments(out _, out var propertyName))
                {
                    var method = methodCallExpression.Method;
                    return method.DeclaringType?.Name + "." + method.Name
                           + "(?" + Caller + "?, " + propertyName + ")";
                }
            }

            return $"?{Caller} | {AccessOperation}?";
        }
    }
}
