// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors
{
    public class RequiresMaterializationExpressionVisitor : ExpressionVisitorBase
    {
        private readonly EntityQueryModelVisitor _queryModelVisitor;
        private readonly Dictionary<IQuerySource, int> _querySources = new Dictionary<IQuerySource, int>();

        public RequiresMaterializationExpressionVisitor([NotNull] EntityQueryModelVisitor queryModelVisitor)
        {
            Check.NotNull(queryModelVisitor, nameof(queryModelVisitor));

            _queryModelVisitor = queryModelVisitor;
        }

        public virtual ISet<IQuerySource> QuerySourcesRequiringMaterialization
        {
            get { return new HashSet<IQuerySource>(_querySources.Where(kv => kv.Value > 0).Select(kv => kv.Key)); }
        }

        protected override Expression VisitQuerySourceReference(
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

            return base.VisitQuerySourceReference(querySourceReferenceExpression);
        }

        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            var newExpression = base.VisitMember(memberExpression);

            if (memberExpression.Expression != null)
            {
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
            }

            return newExpression;
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            var newExpression = base.VisitMethodCall(methodCallExpression);

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
