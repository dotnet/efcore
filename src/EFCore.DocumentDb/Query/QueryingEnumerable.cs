// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using Microsoft.Azure.Documents;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class QueryingEnumerable<T> : IEnumerable<T>
    {
        private readonly DocumentDbQueryContext _documentDbQueryContext;
        private readonly DocumentCommandContext _documentCommandContext;
        private readonly IShaper<T> _shaper;

        public QueryingEnumerable(
            DocumentDbQueryContext queryContext,
            DocumentCommandContext documentCommandContext,
            IShaper<T> shaper)
        {
            _documentDbQueryContext = queryContext;
            _documentCommandContext = documentCommandContext;
            _shaper = shaper;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private sealed class Enumerator : IEnumerator<T>
        {
            private readonly DocumentDbQueryContext _documentDbQueryContext;
            private readonly DocumentCommandContext _documentCommandContext;
            private readonly IShaper<T> _shaper;

            private ValueBufferFactory _valueBufferFactory;
            private IEnumerator<Document> _enumerator;

            public Enumerator(QueryingEnumerable<T> queryingEnumerable)
            {
                _documentDbQueryContext = queryingEnumerable._documentDbQueryContext;
                _documentCommandContext = queryingEnumerable._documentCommandContext;
                _shaper = queryingEnumerable._shaper;
            }

            public bool MoveNext()
            {
                if (_enumerator == null)
                {
                    _enumerator = _documentDbQueryContext.ExecuteQuery(
                        _documentCommandContext.CollectionUri,
                        _documentCommandContext.GetSqlQuerySpec(_documentDbQueryContext.ParameterValues));
                    _valueBufferFactory = _documentCommandContext.ValueBufferFactory;
                }

                var hasNext = _enumerator.MoveNext();

                Current = hasNext
                    ? _shaper.Shape(_documentDbQueryContext, _valueBufferFactory.CreateValueBuffer(_enumerator.Current))
                    : default;

                return hasNext;
            }

            public void Reset()
            {
                _enumerator.Reset();
            }

            public T Current { get; private set; }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                _enumerator?.Dispose();
            }
        }
    }
}
