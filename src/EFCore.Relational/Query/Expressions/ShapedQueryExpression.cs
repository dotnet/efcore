// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Expressions
{
    /// <summary>
    ///     Reducible annotation expression representing a single <see cref="SelectExpression"/>
    ///     and an associated <see cref="Shaper"/>. This expression reduces down to the appropriate
    ///     call(s) to <see cref="ILinqOperatorProvider"/> and <see cref="IQueryMethodProvider"/>.
    /// </summary>
    public sealed class ShapedQueryExpression : Expression
    {
        private readonly MethodInfo _queryMethod;
        private readonly bool _defaultIfEmpty;

        /// <summary>
        ///     Creates an instance of <see cref="ShapedQueryExpression"/>.
        /// </summary>
        /// <param name="queryCompilationContext"> The query compilation context. </param>
        /// <param name="selectExpression"> The select expression. </param>
        /// <param name="shaper"> The shaper. </param>
        /// <param name="shaperCommandContext"> The shaper command context. </param>
        public ShapedQueryExpression(
            [NotNull] RelationalQueryCompilationContext queryCompilationContext,
            [NotNull] SelectExpression selectExpression,
            [NotNull] Shaper shaper,
            [NotNull] ShaperCommandContext shaperCommandContext)
            : this(Check.NotNull(queryCompilationContext, nameof(queryCompilationContext)),
                  Check.NotNull(selectExpression, nameof(selectExpression)),
                  Check.NotNull(shaper, nameof(shaper)),
                  Check.NotNull(shaperCommandContext, nameof(shaperCommandContext)),
                  defaultIfEmpty: false)
        {
        }

        private ShapedQueryExpression(
            RelationalQueryCompilationContext queryCompilationContext,
            SelectExpression selectExpression,
            Shaper shaper,
            ShaperCommandContext shaperCommandContext,
            bool defaultIfEmpty)
        {
            QueryCompilationContext = queryCompilationContext;
            SelectExpression = selectExpression;
            Shaper = shaper;
            ShaperCommandContext = shaperCommandContext;

            _defaultIfEmpty = defaultIfEmpty;

            _queryMethod 
                = (_defaultIfEmpty
                    ? QueryCompilationContext.QueryMethodProvider.DefaultIfEmptyShapedQueryMethod
                    : QueryCompilationContext.QueryMethodProvider.ShapedQueryMethod)
                    .MakeGenericMethod(Shaper.Type);
        }

        /// <summary>
        ///     The type.
        /// </summary>
        public override Type Type => _queryMethod.ReturnType;

        /// <summary>
        ///     Type of the node.
        /// </summary>
        public override ExpressionType NodeType => ExpressionType.Extension;

        /// <summary>
        ///     Indicates that the node can be reduced to a simpler node. If this returns true, Reduce() can be called to produce the reduced
        ///     form.
        /// </summary>
        /// <returns>True if the node can be reduced, otherwise false.</returns>
        public override bool CanReduce => true;

        /// <summary>
        ///     Reduces this node to a simpler expression. If CanReduce returns true, this should return a valid expression. This method can
        ///     return another node which itself must be reduced.
        /// </summary>
        /// <returns>The reduced expression.</returns>
        public override Expression Reduce()
        {
            var queryMethodCall 
                = Call(
                    _queryMethod,
                    EntityQueryModelVisitor.QueryContextParameter,
                    Constant(ShaperCommandContext),
                    Constant(Shaper));

            return _defaultIfEmpty
                ? Call(
                    QueryCompilationContext.LinqOperatorProvider.DefaultIfEmpty
                        .MakeGenericMethod(Shaper.Type), 
                    queryMethodCall)
                : queryMethodCall;
        }

        /// <summary>
        ///     Reduces the node and then calls the visitor delegate on the reduced expression. The method throws an exception if the node is not
        ///     reducible.
        /// </summary>
        /// <returns>The expression being visited, or an expression which should replace it in the tree.</returns>
        /// <param name="visitor">An instance of <see cref="T:System.Func`2" />.</param>
        protected override Expression VisitChildren(ExpressionVisitor visitor) => this;

        /// <summary>
        ///     The query compilation context.
        /// </summary>
        public RelationalQueryCompilationContext QueryCompilationContext { get; }

        /// <summary>
        ///     The select expression.
        /// </summary>
        public SelectExpression SelectExpression { get; }

        /// <summary>
        ///     The shaper.
        /// </summary>
        public Shaper Shaper { get; }

        /// <summary>
        ///     The shaper command context.
        /// </summary>
        public ShaperCommandContext ShaperCommandContext { get; }

        public ShapedQueryExpression AsDefaultIfEmpty()
            => new ShapedQueryExpression(
                QueryCompilationContext,
                SelectExpression,
                Shaper,
                ShaperCommandContext,
                true);

        public ShapedQueryExpression WithShaper([NotNull] Shaper shaper)
            => new ShapedQueryExpression(
                QueryCompilationContext,
                SelectExpression,
                Check.NotNull(shaper, nameof(shaper)),
                ShaperCommandContext,
                _defaultIfEmpty);
    }
}
