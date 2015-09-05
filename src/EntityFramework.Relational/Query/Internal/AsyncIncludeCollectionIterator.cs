// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.Query.Internal
{
    public class AsyncIncludeCollectionIterator : IDisposable
    {
        private readonly IAsyncEnumerator<ValueBuffer> _relatedValuesEnumerator;

        private bool _hasRemainingRows;
        private bool _moveNextPending = true;

        public AsyncIncludeCollectionIterator(
            [NotNull] IAsyncEnumerator<ValueBuffer> relatedValuesEnumerator)
        {
            _relatedValuesEnumerator = relatedValuesEnumerator;
        }

        public virtual IAsyncEnumerable<ValueBuffer> GetRelatedValues(
            [NotNull] EntityKey primaryKey,
            [NotNull] Func<ValueBuffer, EntityKey> relatedKeyFactory)
            => new RelatedValuesEnumerable(this, primaryKey, relatedKeyFactory);

        private sealed class RelatedValuesEnumerable : IAsyncEnumerable<ValueBuffer>
        {
            private readonly AsyncIncludeCollectionIterator _iterator;
            private readonly EntityKey _primaryKey;
            private readonly Func<ValueBuffer, EntityKey> _relatedKeyFactory;

            public RelatedValuesEnumerable(
                AsyncIncludeCollectionIterator iterator,
                EntityKey primaryKey,
                Func<ValueBuffer, EntityKey> relatedKeyFactory)
            {
                _iterator = iterator;
                _primaryKey = primaryKey;
                _relatedKeyFactory = relatedKeyFactory;
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
                    if (_relatedValuesEnumerable._iterator._moveNextPending)
                    {
                        _relatedValuesEnumerable._iterator._hasRemainingRows
                            = await _relatedValuesEnumerable._iterator._relatedValuesEnumerator
                                .MoveNext(cancellationToken);

                        _relatedValuesEnumerable._iterator._moveNextPending = false;
                    }

                    if (_relatedValuesEnumerable._iterator._hasRemainingRows
                        && _relatedValuesEnumerable._relatedKeyFactory(
                            _relatedValuesEnumerable._iterator._relatedValuesEnumerator.Current)
                            .Equals(_relatedValuesEnumerable._primaryKey))
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

        public virtual void Dispose()
        {
            _relatedValuesEnumerator.Dispose();
        }
    }
}
