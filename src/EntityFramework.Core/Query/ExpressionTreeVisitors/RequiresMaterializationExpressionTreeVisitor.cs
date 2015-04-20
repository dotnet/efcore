// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.Data.Entity.Query.ExpressionTreeVisitors
{
    public class RequiresMaterializationExpressionTreeVisitor : ExpressionTreeVisitorBase
    {
        private readonly EntityQueryModelVisitor _queryModelVisitor;
        private readonly Dictionary<IQuerySource, int> _querySources = new Dictionary<IQuerySource, int>();

        public RequiresMaterializationExpressionTreeVisitor([NotNull] EntityQueryModelVisitor queryModelVisitor)
        {
            Check.NotNull(queryModelVisitor, nameof(queryModelVisitor));

            _queryModelVisitor = queryModelVisitor;
        }

        public virtual ISet<IQuerySource> QuerySourcesRequiringMaterialization
        {
            get { return new HashSet<IQuerySource>(_querySources.Where(kv => kv.Value > 0).Select(kv => kv.Key)); }
        }

        protected override Expression VisitQuerySourceReferenceExpression(
            QuerySourceReferenceExpression querySourceReferenceExpression)
        {
            if (!_querySources.ContainsKey(querySourceReferenceExpression.ReferencedQuerySource))
            {
                _querySources.Add(querySourceReferenceExpression.ReferencedQuerySource, 0);
            }

            if (_queryModelVisitor.QueryCompilationContext.Model
                .FindEntityType(querySourceReferenceExpression.Type) != null)
            {
                _querySources[querySourceReferenceExpression.ReferencedQuerySource]++;
            }

            return base.VisitQuerySourceReferenceExpression(querySourceReferenceExpression);
        }

        protected override Expression VisitMemberExpression(MemberExpression memberExpression)
        {
            var newExpression = base.VisitMemberExpression(memberExpression);

            _queryModelVisitor
                .BindMemberExpression(
                    memberExpression,
                    (property, querySource) =>
                        {
                            if (querySource != null)
                            {
                                _querySources[querySource]--;
                            }
                        });

            return newExpression;
        }

        protected override Expression VisitMethodCallExpression(MethodCallExpression methodCallExpression)
        {
            var newExpression = base.VisitMethodCallExpression(methodCallExpression);

            _queryModelVisitor
                .BindMethodCallExpression(
                    methodCallExpression,
                    (property, querySource) =>
                        {
                            if (querySource != null)
                            {
                                _querySources[querySource]--;
                            }
                        });

            return newExpression;
        }
    }
}
