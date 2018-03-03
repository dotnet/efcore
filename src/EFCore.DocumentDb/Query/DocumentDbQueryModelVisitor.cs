// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq;
using Remotion.Linq.Clauses;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class DocumentDbQueryModelVisitor : EntityQueryModelVisitor
    {
        private readonly ISqlTranslatingExpressionVisitorFactory _sqlTranslatingExpressionVisitorFactory;
        public virtual DocumentDbQueryModelVisitor ParentQueryModelVisitor { get; }
        public virtual bool CanBindToParentQueryModel { get; protected set; }
        protected virtual Dictionary<IQuerySource, SelectExpression> QueriesBySource { get; }
            = new Dictionary<IQuerySource, SelectExpression>();
        public virtual ICollection<SelectExpression> Queries => QueriesBySource.Values;

        public DocumentDbQueryModelVisitor([NotNull] EntityQueryModelVisitorDependencies dependencies,
            [NotNull] QueryCompilationContext queryCompilationContext,
            ISqlTranslatingExpressionVisitorFactory sqlTranslatingExpressionVisitorFactory,
            DocumentDbQueryModelVisitor parentQueryModelVisitor)
            : base(dependencies, queryCompilationContext)
        {
            _sqlTranslatingExpressionVisitorFactory = sqlTranslatingExpressionVisitorFactory;
            ParentQueryModelVisitor = parentQueryModelVisitor;
        }

        public virtual void AddQuery([NotNull] IQuerySource querySource, [NotNull] SelectExpression selectExpression)
        {
            Check.NotNull(querySource, nameof(querySource));
            Check.NotNull(selectExpression, nameof(selectExpression));

            QueriesBySource.Add(querySource, selectExpression);
        }

        public virtual SelectExpression TryGetQuery([NotNull] IQuerySource querySource)
        {
            Check.NotNull(querySource, nameof(querySource));

            return QueriesBySource.TryGetValue(querySource, out var selectExpression)
                ? selectExpression
                : QueriesBySource.Values.LastOrDefault(se => se.HandlesQuerySource(querySource));
        }

        public override void VisitWhereClause(WhereClause whereClause, QueryModel queryModel, int index)
        {
            if (Expression is ShapedQueryExpression)
            {
                var selectExpression = TryGetQuery(queryModel.MainFromClause);

                var sqlTranslatingExpressionVisitor
                    = _sqlTranslatingExpressionVisitorFactory.Create(this, selectExpression);

                var sql = sqlTranslatingExpressionVisitor.Visit(whereClause.Predicate);

                if (sql != null)
                {
                    selectExpression.AddToPredicate(sql);

                    return;
                }
            }
            base.VisitWhereClause(whereClause, queryModel, index);
        }

        public virtual TResult BindMemberExpression<TResult>(
            [NotNull] MemberExpression memberExpression,
            [NotNull] Func<IProperty, IQuerySource, SelectExpression, TResult> memberBinder)
        {
            Check.NotNull(memberExpression, nameof(memberExpression));
            Check.NotNull(memberBinder, nameof(memberBinder));

            return BindMemberExpression(memberExpression, null, memberBinder);
        }

        private TResult BindMemberExpression<TResult>(
            [NotNull] MemberExpression memberExpression,
            [CanBeNull] IQuerySource querySource,
            Func<IProperty, IQuerySource, SelectExpression, TResult> memberBinder)
        {
            Check.NotNull(memberExpression, nameof(memberExpression));
            Check.NotNull(memberBinder, nameof(memberBinder));

            return base.BindMemberExpression(
                memberExpression, querySource,
                (property, qs) => BindMemberOrMethod(memberBinder, qs, property));
        }

        public virtual TResult BindMethodCallExpression<TResult>(
            [NotNull] MethodCallExpression memberExpression,
            [NotNull] Func<IProperty, IQuerySource, SelectExpression, TResult> memberBinder)
        {
            Check.NotNull(memberExpression, nameof(memberExpression));
            Check.NotNull(memberBinder, nameof(memberBinder));

            return BindMethodCallExpression(memberExpression, null, memberBinder);
        }

        private TResult BindMethodCallExpression<TResult>(
            [NotNull] MethodCallExpression memberExpression,
            [CanBeNull] IQuerySource querySource,
            Func<IProperty, IQuerySource, SelectExpression, TResult> memberBinder)
        {
            Check.NotNull(memberExpression, nameof(memberExpression));
            Check.NotNull(memberBinder, nameof(memberBinder));

            return base.BindMethodCallExpression(
                memberExpression, querySource,
                (property, qs) => BindMemberOrMethod(memberBinder, qs, property));
        }

        private TResult BindMemberOrMethod<TResult>(
            Func<IProperty, IQuerySource, SelectExpression, TResult> memberBinder,
            IQuerySource querySource,
            IProperty property)
        {
            if (querySource != null)
            {
                var selectExpression = TryGetQuery(querySource);

                if (selectExpression != null)
                {
                    return memberBinder(property, querySource, selectExpression);
                }
            }

            return default;
        }
    }
}
