// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.InMemory.Query.Internal;

public partial class InMemoryQueryExpression
{
    private sealed class ResultEnumerable(Func<ValueBuffer> getElement) : IEnumerable<ValueBuffer>
    {
        public IEnumerator<ValueBuffer> GetEnumerator()
            => new ResultEnumerator(getElement());

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        private sealed class ResultEnumerator : IEnumerator<ValueBuffer>
        {
            private readonly ValueBuffer _value;
            private bool _moved;

            public ResultEnumerator(ValueBuffer value)
            {
                _value = value;
                _moved = _value.IsEmpty;
            }

            public bool MoveNext()
            {
                if (!_moved)
                {
                    _moved = true;

                    return _moved;
                }

                return false;
            }

            public void Reset()
                => _moved = false;

            object IEnumerator.Current
                => Current;

            public ValueBuffer Current
                => !_moved ? ValueBuffer.Empty : _value;

            void IDisposable.Dispose()
            {
            }
        }
    }

    private sealed class ProjectionMemberRemappingExpressionVisitor(
        Expression queryExpression,
        Dictionary<ProjectionMember, ProjectionMember> projectionMemberMappings)
        : ExpressionVisitor
    {
        protected override Expression VisitExtension(Expression expression)
        {
            if (expression is ProjectionBindingExpression projectionBindingExpression)
            {
                Check.DebugAssert(
                    projectionBindingExpression.ProjectionMember is not null,
                    "ProjectionBindingExpression must have projection member.");

                return new ProjectionBindingExpression(
                    queryExpression,
                    projectionMemberMappings[projectionBindingExpression.ProjectionMember],
                    projectionBindingExpression.Type);
            }

            return base.VisitExtension(expression);
        }
    }

    private sealed class ProjectionMemberToIndexConvertingExpressionVisitor(
        Expression queryExpression,
        Dictionary<ProjectionMember, int> projectionMemberMappings)
        : ExpressionVisitor
    {
        [return: NotNullIfNotNull(nameof(expression))]
        public override Expression? Visit(Expression? expression)
        {
            if (expression is ProjectionBindingExpression projectionBindingExpression)
            {
                Check.DebugAssert(
                    projectionBindingExpression.ProjectionMember != null,
                    "ProjectionBindingExpression must have projection member.");

                return new ProjectionBindingExpression(
                    queryExpression,
                    projectionMemberMappings[projectionBindingExpression.ProjectionMember],
                    projectionBindingExpression.Type);
            }

            return base.Visit(expression);
        }
    }

    private sealed class ProjectionIndexRemappingExpressionVisitor(
        Expression oldExpression,
        Expression newExpression,
        int[] indexMap)
        : ExpressionVisitor
    {
        [return: NotNullIfNotNull(nameof(expression))]
        public override Expression? Visit(Expression? expression)
        {
            if (expression is ProjectionBindingExpression projectionBindingExpression
                && ReferenceEquals(projectionBindingExpression.QueryExpression, oldExpression))
            {
                Check.DebugAssert(
                    projectionBindingExpression.Index != null,
                    "ProjectionBindingExpression must have index.");

                return new ProjectionBindingExpression(
                    newExpression,
                    indexMap[projectionBindingExpression.Index.Value],
                    projectionBindingExpression.Type);
            }

            return base.Visit(expression);
        }
    }

    private sealed class EntityShaperNullableMarkingExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitExtension(Expression extensionExpression)
            => extensionExpression is StructuralTypeShaperExpression shaper
                ? shaper.MakeNullable()
                : base.VisitExtension(extensionExpression);
    }

    private sealed class QueryExpressionReplacingExpressionVisitor(Expression oldQuery, Expression newQuery) : ExpressionVisitor
    {
        [return: NotNullIfNotNull(nameof(expression))]
        public override Expression? Visit(Expression? expression)
            => expression is ProjectionBindingExpression projectionBindingExpression
                && ReferenceEquals(projectionBindingExpression.QueryExpression, oldQuery)
                    ? projectionBindingExpression.ProjectionMember != null
                        ? new ProjectionBindingExpression(
                            newQuery, projectionBindingExpression.ProjectionMember!, projectionBindingExpression.Type)
                        : new ProjectionBindingExpression(
                            newQuery, projectionBindingExpression.Index!.Value, projectionBindingExpression.Type)
                    : base.Visit(expression);
    }

    private sealed class CloningExpressionVisitor : ExpressionVisitor
    {
        [return: NotNullIfNotNull(nameof(expression))]
        public override Expression? Visit(Expression? expression)
        {
            if (expression is InMemoryQueryExpression inMemoryQueryExpression)
            {
                var clonedInMemoryQueryExpression = new InMemoryQueryExpression(
                    inMemoryQueryExpression.ServerQueryExpression, inMemoryQueryExpression._valueBufferParameter)
                {
                    _groupingParameter = inMemoryQueryExpression._groupingParameter,
                    _singleResultMethodInfo = inMemoryQueryExpression._singleResultMethodInfo,
                    _scalarServerQuery = inMemoryQueryExpression._scalarServerQuery
                };

                clonedInMemoryQueryExpression._clientProjections.AddRange(
                    inMemoryQueryExpression._clientProjections.Select(e => Visit(e)));
                clonedInMemoryQueryExpression._projectionMappingExpressions.AddRange(
                    inMemoryQueryExpression._projectionMappingExpressions);
                foreach (var (projectionMember, value) in inMemoryQueryExpression._projectionMapping)
                {
                    clonedInMemoryQueryExpression._projectionMapping[projectionMember] = Visit(value);
                }

                return clonedInMemoryQueryExpression;
            }

            if (expression is EntityProjectionExpression entityProjectionExpression)
            {
                return entityProjectionExpression.Clone();
            }

            return base.Visit(expression);
        }
    }
}
