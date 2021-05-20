// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.InMemory.Query.Internal
{
    public partial class InMemoryQueryExpression
    {
        private sealed class ResultEnumerable : IEnumerable<ValueBuffer>
        {
            private readonly Func<ValueBuffer> _getElement;

            public ResultEnumerable(Func<ValueBuffer> getElement)
            {
                _getElement = getElement;
            }

            public IEnumerator<ValueBuffer> GetEnumerator()
                => new ResultEnumerator(_getElement());

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
                {
                    _moved = false;
                }

                object IEnumerator.Current
                    => Current;

                public ValueBuffer Current
                    => !_moved ? ValueBuffer.Empty : _value;

                void IDisposable.Dispose()
                {
                }
            }
        }

        private sealed class ProjectionMemberRemappingExpressionVisitor : ExpressionVisitor
        {
            private readonly Expression _queryExpression;
            private readonly Dictionary<ProjectionMember, ProjectionMember> _projectionMemberMappings;

            public ProjectionMemberRemappingExpressionVisitor(
                Expression queryExpression, Dictionary<ProjectionMember, ProjectionMember> projectionMemberMappings)
            {
                _queryExpression = queryExpression;
                _projectionMemberMappings = projectionMemberMappings;
            }

            [return: NotNullIfNotNull("expression")]
            public override Expression? Visit(Expression? expression)
            {
                if (expression is ProjectionBindingExpression projectionBindingExpression)
                {
                    Check.DebugAssert(projectionBindingExpression.ProjectionMember != null,
                        "ProjectionBindingExpression must have projection member.");

                    return new ProjectionBindingExpression(
                        _queryExpression,
                        _projectionMemberMappings[projectionBindingExpression.ProjectionMember],
                        projectionBindingExpression.Type);
                }

                return base.Visit(expression);
            }
        }

        private sealed class ProjectionMemberToIndexConvertingExpressionVisitor : ExpressionVisitor
        {
            private readonly Expression _queryExpression;
            private readonly Dictionary<ProjectionMember, int> _projectionMemberMappings;

            public ProjectionMemberToIndexConvertingExpressionVisitor(
                Expression queryExpression, Dictionary<ProjectionMember, int> projectionMemberMappings)
            {
                _queryExpression = queryExpression;
                _projectionMemberMappings = projectionMemberMappings;
            }

            [return: NotNullIfNotNull("expression")]
            public override Expression? Visit(Expression? expression)
            {
                if (expression is ProjectionBindingExpression projectionBindingExpression)
                {
                    Check.DebugAssert(projectionBindingExpression.ProjectionMember != null,
                        "ProjectionBindingExpression must have projection member.");

                    return new ProjectionBindingExpression(
                        _queryExpression,
                        _projectionMemberMappings[projectionBindingExpression.ProjectionMember],
                        projectionBindingExpression.Type);
                }

                return base.Visit(expression);
            }
        }

        private sealed class ProjectionIndexRemappingExpressionVisitor : ExpressionVisitor
        {
            private readonly Expression _oldExpression;
            private readonly Expression _newExpression;
            private readonly int[] _indexMap;

            public ProjectionIndexRemappingExpressionVisitor(
                Expression oldExpression, Expression newExpression, int[] indexMap)
            {
                _oldExpression = oldExpression;
                _newExpression = newExpression;
                _indexMap = indexMap;
            }

            [return: NotNullIfNotNull("expression")]
            public override Expression? Visit(Expression? expression)
            {
                if (expression is ProjectionBindingExpression projectionBindingExpression
                    && ReferenceEquals(projectionBindingExpression.QueryExpression, _oldExpression))
                {
                    Check.DebugAssert(projectionBindingExpression.Index != null,
                        "ProjectionBindingExpression must have index.");

                    return new ProjectionBindingExpression(
                        _newExpression,
                        _indexMap[projectionBindingExpression.Index.Value],
                        projectionBindingExpression.Type);
                }

                return base.Visit(expression);
            }
        }

        private sealed class EntityShaperNullableMarkingExpressionVisitor : ExpressionVisitor
        {
            protected override Expression VisitExtension(Expression extensionExpression)
            {
                Check.NotNull(extensionExpression, nameof(extensionExpression));

                return extensionExpression is EntityShaperExpression entityShaper
                    ? entityShaper.MakeNullable()
                    : base.VisitExtension(extensionExpression);
            }
        }
    }
}
