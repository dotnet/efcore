// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class AsyncIncludeCollectionIterator : IDisposable
    {
        private readonly IAsyncEnumerator<ValueBuffer> _relatedValuesEnumerator;

        private bool _hasRemainingRows;
        private bool _moveNextPending = true;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public AsyncIncludeCollectionIterator(
            [NotNull] IAsyncEnumerator<ValueBuffer> relatedValuesEnumerator)
        {
            _relatedValuesEnumerator = relatedValuesEnumerator;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IAsyncEnumerable<ValueBuffer> GetRelatedValues(
            [NotNull] IIncludeKeyComparer keyComparer)
            => new RelatedValuesEnumerable(this, keyComparer);

        private sealed class RelatedValuesEnumerable : IAsyncEnumerable<ValueBuffer>
        {
            private readonly AsyncIncludeCollectionIterator _iterator;
            private readonly IIncludeKeyComparer _keyComparer;

            public RelatedValuesEnumerable(
                AsyncIncludeCollectionIterator iterator,
                IIncludeKeyComparer keyComparer)
            {
                _iterator = iterator;
                _keyComparer = keyComparer;
            }

            public IAsyncEnumerator<ValueBuffer> GetEnumerator()
                => new RelatedValuesEnumerator(this);

            private sealed class RelatedValuesEnumerator : IAsyncEnumerator<ValueBuffer>
            {
                private readonly RelatedValuesEnumerable _relatedValuesEnumerable;

                public RelatedValuesEnumerator(RelatedValuesEnumerable relatedValuesEnumerable)
                {
                    _relatedValuesEnumerable = relatedValuesEnumerable;
                }

                public async Task<bool> MoveNext(CancellationToken cancellationToken)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (_relatedValuesEnumerable._iterator._moveNextPending)
                    {
                        _relatedValuesEnumerable._iterator._hasRemainingRows
                            = await _relatedValuesEnumerable._iterator._relatedValuesEnumerator
                                .MoveNext(cancellationToken);

                        _relatedValuesEnumerable._iterator._moveNextPending = false;
                    }

                    if (_relatedValuesEnumerable._iterator._hasRemainingRows
                        && _relatedValuesEnumerable._keyComparer.ShouldInclude(
                            _relatedValuesEnumerable._iterator._relatedValuesEnumerator.Current))
                    {
                        Current = _relatedValuesEnumerable._iterator._relatedValuesEnumerator.Current;

                        _relatedValuesEnumerable._iterator._moveNextPending = true;

                        return true;
                    }

                    return false;
                }

                public ValueBuffer Current { get; private set; }

                public void Dispose()
                {
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Dispose() => _relatedValuesEnumerator.Dispose();
    }
}
