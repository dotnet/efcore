// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Remotion.Linq.Parsing;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class TaskLiftingExpressionVisitor : RelinqExpressionVisitor
    {
        private static readonly ParameterExpression _resultsParameter
            = Expression.Parameter(typeof(object[]), name: "results");

        private readonly List<Expression> _taskExpressions = new List<Expression>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ParameterExpression CancellationTokenParameter { get; private set; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Expression LiftTasks([NotNull] Expression expression)
        {
            var newExpression = Visit(expression);

            if (newExpression != expression)
            {
                newExpression
                    = Expression.Call(
                        _executeAsync.MakeGenericMethod(expression.Type),
                        Expression.NewArrayInit(typeof(Func<Task<object>>), _taskExpressions),
                        Expression.Lambda(newExpression, _resultsParameter));

                if (CancellationTokenParameter == null)
                {
                    CancellationTokenParameter = IncludeCompiler.CancellationTokenParameter;
                }
            }

            return newExpression;
        }

        private static readonly MethodInfo _executeAsync
            = typeof(TaskLiftingExpressionVisitor)
                .GetTypeInfo()
                .GetDeclaredMethod(nameof(_ExecuteAsync));

        // ReSharper disable once InconsistentNaming
        private static async Task<T> _ExecuteAsync<T>(
            IReadOnlyList<Func<Task<object>>> taskFactories,
            Func<object[], T> selector)
        {
            var results = new object[taskFactories.Count];

            for (var i = 0; i < taskFactories.Count; i++)
            {
                results[i] = await taskFactories[i]();
            }

            return selector(results);
        }

        private static readonly MethodInfo _toObjectTask
            = typeof(TaskLiftingExpressionVisitor)
                .GetTypeInfo()
                .GetDeclaredMethod(nameof(_ToObjectTask));

        // ReSharper disable once InconsistentNaming
        private static Task<object> _ToObjectTask<T>(Task<T> task)
        {
            var tcs = new TaskCompletionSource<object>();

            task.ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        // ReSharper disable once PossibleNullReferenceException
                        tcs.TrySetException(t.Exception.InnerExceptions);
                    }
                    else if (t.IsCanceled)
                    {
                        tcs.TrySetCanceled();
                    }
                    else
                    {
                        tcs.TrySetResult(t.Result);
                    }
                },
                TaskContinuationOptions.ExecuteSynchronously);

            return tcs.Task;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitParameter(ParameterExpression parameterExpression)
        {
            if (parameterExpression.Type == typeof(CancellationToken))
            {
                CancellationTokenParameter = parameterExpression;
            }

            return parameterExpression;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            var memberExpressionType = memberExpression.Expression.Type;

            if (memberExpressionType.IsConstructedGenericType
                && memberExpressionType.GetGenericTypeDefinition() == typeof(Task<>)
                && memberExpression.Member.Name == nameof(Task<object>.Result))
            {
                _taskExpressions.Add(
                    Expression.Lambda<Func<Task<object>>>(
                        Expression.Call(
                            _toObjectTask.MakeGenericMethod(
                                memberExpressionType.GenericTypeArguments[0]),
                            memberExpression.Expression)));

                return
                    Expression.Convert(
                        Expression.ArrayAccess(
                            _resultsParameter,
                            Expression.Constant(_taskExpressions.Count - 1)),
                        memberExpression.Type);
            }

            return base.VisitMember(memberExpression);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method
                .MethodIsClosedFormOf(TaskBlockingExpressionVisitor.ResultMethodInfo))
            {
                _taskExpressions.Add(
                    Expression.Lambda<Func<Task<object>>>(
                        Expression.Call(
                            _toObjectTask.MakeGenericMethod(
                                methodCallExpression.Method.ReturnType),
                            methodCallExpression.Arguments[0])));

                return Expression.Convert(
                    Expression.ArrayAccess(
                        _resultsParameter,
                        Expression.Constant(_taskExpressions.Count - 1)),
                    methodCallExpression.Method.ReturnType);
            }

            return base.VisitMethodCall(methodCallExpression);
        }

        // Prune nodes

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitLambda<T>(Expression<T> node) => node;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitBlock(BlockExpression node) => node;
    }
}
