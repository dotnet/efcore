// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;
using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Expressions.Internal
{
    public class QueryShaperExpression : Expression
    {
        private readonly bool _async;

        public QueryShaperExpression(bool async, Expression queryExpression, IShaper shaper)
        {
            if (queryExpression.Type.TryGetSequenceType() != typeof(JObject))
            {
                throw new InvalidOperationException("Invalid type");
            }

            _async = async;
            QueryExpression = queryExpression;
            Shaper = shaper;
        }

        public Expression QueryExpression { get; }
        public IShaper Shaper { get; }

        public override Type Type => _async
            ? typeof(IAsyncEnumerable<>).MakeGenericType(Shaper.Type)
            : typeof(IEnumerable<>).MakeGenericType(Shaper.Type);

        public override ExpressionType NodeType => ExpressionType.Extension;
        public override bool CanReduce => true;

        public override Expression Reduce()
            => Call(
                (_async ? _shapeAsyncMethodInfo : _shapeMethodInfo).MakeGenericMethod(Shaper.Type),
                QueryExpression,
                Shaper.CreateShaperLambda());

        private static readonly MethodInfo _shapeMethodInfo
            = typeof(QueryShaperExpression).GetTypeInfo().GetDeclaredMethod(nameof(_Shape));

        [UsedImplicitly]
        private static IEnumerable<T> _Shape<T>(
            IEnumerable<JObject> innerEnumerable,
            Func<JObject, T> shaper)
        {
            foreach (var jObject in innerEnumerable)
            {
                yield return shaper(jObject);
            }
        }

        private static readonly MethodInfo _shapeAsyncMethodInfo
            = typeof(QueryShaperExpression).GetTypeInfo().GetDeclaredMethod(nameof(_ShapeAsync));

        [UsedImplicitly]
        private static IAsyncEnumerable<T> _ShapeAsync<T>(
            IAsyncEnumerable<JObject> innerEnumerable,
            Func<JObject, T> shaper)
            => new AsyncShaperEnumerable<T>(innerEnumerable, shaper);

        private class AsyncShaperEnumerable<T> : IAsyncEnumerable<T>
        {
            private readonly IAsyncEnumerable<JObject> _innerEnumerable;
            private readonly Func<JObject, T> _shaper;

            public AsyncShaperEnumerable(
                IAsyncEnumerable<JObject> innerEnumerable,
                Func<JObject, T> shaper)
            {
                _innerEnumerable = innerEnumerable;
                _shaper = shaper;
            }

            public IAsyncEnumerator<T> GetEnumerator() => new AsyncShaperEnumerator(this);

            private class AsyncShaperEnumerator : IAsyncEnumerator<T>
            {
                private readonly IAsyncEnumerator<JObject> _enumerator;
                private readonly Func<JObject, T> _shaper;

                public AsyncShaperEnumerator(AsyncShaperEnumerable<T> enumerable)
                {
                    _enumerator = enumerable._innerEnumerable.GetEnumerator();
                    _shaper = enumerable._shaper;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public async Task<bool> MoveNext(CancellationToken cancellationToken)
                {
                    if (!await _enumerator.MoveNext(cancellationToken))
                    {
                        Current = default;
                        return false;
                    }

                    Current = _shaper(_enumerator.Current);
                    return true;
                }

                public T Current { get; private set; }

                public void Dispose() => _enumerator.Dispose();
            }
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor) => this;
    }
}
