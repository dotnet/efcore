// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.InMemory.Query.Pipeline
{
    public class InMemoryQueryExpression : Expression
    {
        private sealed class ResultEnumerable : IEnumerable<ValueBuffer>
        {
            private readonly Func<ValueBuffer> _getElement;

            public ResultEnumerable(Func<ValueBuffer> getElement)
            {
                _getElement = getElement;
            }

            public IEnumerator<ValueBuffer> GetEnumerator() => new ResultEnumerator(_getElement());

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            private sealed class ResultEnumerator : IEnumerator<ValueBuffer>
            {
                private readonly ValueBuffer _value;
                private bool _moved;

                public ResultEnumerator(ValueBuffer value) => _value = value;

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

                object IEnumerator.Current => Current;

                public ValueBuffer Current => !_moved ? ValueBuffer.Empty : _value;

                void IDisposable.Dispose()
                {
                }
            }
        }

        public static ParameterExpression ValueBufferParameter = Parameter(typeof(ValueBuffer), "valueBuffer");
        private static ConstructorInfo _valueBufferConstructor = typeof(ValueBuffer).GetConstructors().Single(ci => ci.GetParameters().Length == 1);

        private readonly List<Expression> _valueBufferSlots = new List<Expression>();
        private readonly IDictionary<ProjectionMember, Expression> _projectionMapping = new Dictionary<ProjectionMember, Expression>();

        public InMemoryQueryExpression(IEntityType entityType)
        {
            ServerQueryExpression = new InMemoryTableExpression(entityType);

            var entityValues = new EntityValuesExpression(0);
            _projectionMapping[new ProjectionMember()] = entityValues;
            foreach (var property in entityType.GetProperties())
            {
                _valueBufferSlots.Add(CreateReadValueExpression(property.ClrType, property.GetIndex(), property));
            }
        }

        public Expression GetSingleScalarProjection()
        {
            _valueBufferSlots.Clear();
            _valueBufferSlots.Add(CreateReadValueExpression(ServerQueryExpression.Type, 0, null));

            _projectionMapping.Clear();
            _projectionMapping[new ProjectionMember()] = _valueBufferSlots[0];

            return new ProjectionBindingExpression(this, new ProjectionMember(), ServerQueryExpression.Type);
        }

        public Expression BindProperty(Expression projectionExpression, IProperty property)
        {
            var member = (projectionExpression as ProjectionBindingExpression).ProjectionMember;

            var entityValuesExpression = (EntityValuesExpression)_projectionMapping[member];
            var offset = entityValuesExpression.StartIndex;

            return _valueBufferSlots[offset + property.GetIndex()];
        }

        public void ApplyProjection(IDictionary<ProjectionMember, Expression> projectionMappings)
        {
            _valueBufferSlots.Clear();
            _projectionMapping.Clear();

            foreach (var kvp in projectionMappings)
            {
                var member = kvp.Key;
                var expression = kvp.Value;
                _valueBufferSlots.Add(expression);
                // TODO: Infer property from inner
                _projectionMapping[member] = CreateReadValueExpression(expression.Type, _valueBufferSlots.Count - 1, null);
            }
        }

        public Expression GetProjectionExpression(ProjectionMember member)
        {
            return _projectionMapping[member];
        }

        public LambdaExpression GetScalarProjectionLambda()
        {
            Debug.Assert(_valueBufferSlots.Count == 1, "Not a scalar query");

            return Lambda(_valueBufferSlots[0], ValueBufferParameter);
        }

        public void ApplyServerProjection()
        {
            if (ServerQueryExpression.Type.TryGetSequenceType() == null)
            {
                if (ServerQueryExpression.Type != typeof(ValueBuffer))
                {
                    ServerQueryExpression = New(
                        typeof(ResultEnumerable).GetConstructors().Single(),
                        Lambda<Func<ValueBuffer>>(
                            New(
                                _valueBufferConstructor,
                                NewArrayInit(
                                    typeof(object),
                                    new[]
                                    {
                                        Convert(ServerQueryExpression, typeof(object))
                                    }))));
                }
                else
                {
                    ServerQueryExpression = New(
                        typeof(ResultEnumerable).GetConstructors().Single(),
                        Lambda<Func<ValueBuffer>>(ServerQueryExpression));
                }

                return;
            }


            var newValueBufferSlots = _valueBufferSlots
                .Select((e, i) => CreateReadValueExpression(
                    e.Type,
                    i,
                    null))
                .ToList();

            var lambda = Lambda(
                New(
                    _valueBufferConstructor,
                    NewArrayInit(
                        typeof(object),
                        _valueBufferSlots
                            .Select(e => Convert(e, typeof(object)))
                            .ToArray())),
                ValueBufferParameter);

            _valueBufferSlots.Clear();
            _valueBufferSlots.AddRange(newValueBufferSlots);

            ServerQueryExpression = Call(
                InMemoryLinqOperatorProvider.Select.MakeGenericMethod(typeof(ValueBuffer), typeof(ValueBuffer)),
                ServerQueryExpression,
                lambda);
        }

        public Expression ServerQueryExpression { get; set; }
        public override Type Type => typeof(IEnumerable<ValueBuffer>);
        public override ExpressionType NodeType => ExpressionType.Extension;

        private Expression CreateReadValueExpression(
            Type type,
            int index,
            IPropertyBase property)
            => Call(
                _tryReadValueMethod.MakeGenericMethod(type),
                ValueBufferParameter,
                Constant(index),
                Constant(property, typeof(IPropertyBase)));

        private static readonly MethodInfo _tryReadValueMethod
            = typeof(InMemoryQueryExpression).GetTypeInfo()
                .GetDeclaredMethod(nameof(TryReadValue));


#pragma warning disable IDE0052 // Remove unread private members
        private static TValue TryReadValue<TValue>(
#pragma warning restore IDE0052 // Remove unread private members
            in ValueBuffer valueBuffer, int index, IPropertyBase property)
            => (TValue)valueBuffer[index];
    }

}
