// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Relational.Utilities;

namespace Microsoft.Data.Entity.Relational.Query
{
    public class IncludeCollectionAsyncEnumerable<TResult> : IAsyncEnumerable<TResult>
    {
        private readonly QueryContext _queryContext;
        private readonly IAsyncEnumerable<TResult> _source;
        private readonly INavigation _navigation;
        private readonly IAsyncEnumerable<IValueReader> _relatedValueReaders;
        private readonly Func<TResult, object> _accessorLambda;

        public IncludeCollectionAsyncEnumerable(
            [NotNull] QueryContext queryContext,
            [NotNull] IAsyncEnumerable<TResult> source,
            [NotNull] INavigation navigation,
            [NotNull] IAsyncEnumerable<IValueReader> relatedValueReaders,
            [NotNull] Func<TResult, object> accessorLambda)
        {
            Check.NotNull(queryContext, "queryContext");
            Check.NotNull(queryContext, "source");
            Check.NotNull(queryContext, "navigation");
            Check.NotNull(queryContext, "relatedValueReaders");
            Check.NotNull(queryContext, "accessorLambda");

            _queryContext = queryContext;
            _source = source;
            _navigation = navigation;
            _relatedValueReaders = relatedValueReaders;
            _accessorLambda = accessorLambda;
        }

        public virtual IAsyncEnumerator<TResult> GetEnumerator()
        {
            return new IncludeCollectionAsyncEnumerator(this);
        }

        private sealed class IncludeCollectionAsyncEnumerator : IAsyncEnumerator<TResult>
        {
            private readonly IncludeCollectionAsyncEnumerable<TResult> _enumerable;
            private readonly AsyncIncludeCollectionIterator _relatedValuesIterator;
            private readonly IAsyncEnumerator<TResult> _enumerator;

            public IncludeCollectionAsyncEnumerator(IncludeCollectionAsyncEnumerable<TResult> enumerable)
            {
                _enumerable = enumerable;
                _enumerator = _enumerable._source.GetEnumerator();

                _relatedValuesIterator
                    = new AsyncIncludeCollectionIterator(_enumerable._relatedValueReaders.GetEnumerator());
            }

            public async Task<bool> MoveNext(CancellationToken cancellationToken)
            {
                if (!await _enumerator.MoveNext(cancellationToken).WithCurrentCulture())
                {
                    return false;
                }

                await _enumerable._queryContext.QueryBuffer
                    .IncludeAsync(
                        _enumerable._accessorLambda.Invoke(_enumerator.Current),
                        _enumerable._navigation,
                        _relatedValuesIterator.GetRelatedValues,
                        cancellationToken)
                    .WithCurrentCulture();

                return true;
            }

            public TResult Current
            {
                get { return _enumerator.Current; }
            }

            public void Dispose()
            {
                _relatedValuesIterator.Dispose();
                _enumerator.Dispose();
            }
        }
    }
}
