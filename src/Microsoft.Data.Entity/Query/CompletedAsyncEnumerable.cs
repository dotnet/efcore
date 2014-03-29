// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query
{
    public sealed class CompletedAsyncEnumerable<T> : IAsyncEnumerable<T>
    {
        private readonly IEnumerable<T> _enumerable;

        public CompletedAsyncEnumerable([NotNull] IEnumerable<T> enumerable)
        {
            Check.NotNull(enumerable, "enumerable");

            _enumerable = enumerable;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator()
        {
            return new Enumerator(_enumerable.GetEnumerator());
        }

        IAsyncEnumerator IAsyncEnumerable.GetAsyncEnumerator()
        {
            return GetAsyncEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return GetAsyncEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private sealed class Enumerator : IAsyncEnumerator<T>
        {
            private readonly IEnumerator<T> _enumerator;

            public Enumerator(IEnumerator<T> enumerator)
            {
                _enumerator = enumerator;
            }

            public Task<bool> MoveNextAsync(CancellationToken cancellationToken = default(CancellationToken))
            {
                return Task.FromResult(MoveNext());
            }

            public bool MoveNext()
            {
                return _enumerator.MoveNext();
            }

            public void Reset()
            {
                _enumerator.Reset();
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            public T Current
            {
                get { return _enumerator.Current; }
            }

            public void Dispose()
            {
                _enumerator.Dispose();
            }
        }
    }
}
