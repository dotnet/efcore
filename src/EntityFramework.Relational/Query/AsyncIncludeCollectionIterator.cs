// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Query
{
    public class AsyncIncludeCollectionIterator : IDisposable
    {
        private readonly IAsyncEnumerator<IValueReader> _relatedValuesEnumerator;

        private bool _hasRemainingRows;
        private bool _moveNextPending = true;

        public AsyncIncludeCollectionIterator(
            [NotNull] IAsyncEnumerator<IValueReader> relatedValuesEnumerator)
        {
            Check.NotNull(relatedValuesEnumerator, "relatedValuesEnumerator");

            _relatedValuesEnumerator = relatedValuesEnumerator;
        }

        public virtual IAsyncEnumerable<IValueReader> GetRelatedValues(
            [NotNull] EntityKey primaryKey,
            [NotNull] Func<IValueReader, EntityKey> relatedKeyFactory)
        {
            Check.NotNull(primaryKey, "primaryKey");
            Check.NotNull(relatedKeyFactory, "relatedKeyFactory");

            return new RelatedValuesEnumerable(this, primaryKey, relatedKeyFactory);
        }

        private sealed class RelatedValuesEnumerable : IAsyncEnumerable<IValueReader>
        {
            private readonly AsyncIncludeCollectionIterator _iterator;
            private readonly EntityKey _primaryKey;
            private readonly Func<IValueReader, EntityKey> _relatedKeyFactory;

            public RelatedValuesEnumerable(
                AsyncIncludeCollectionIterator iterator,
                EntityKey primaryKey,
                Func<IValueReader, EntityKey> relatedKeyFactory)
            {
                _iterator = iterator;
                _primaryKey = primaryKey;
                _relatedKeyFactory = relatedKeyFactory;
            }

            public IAsyncEnumerator<IValueReader> GetEnumerator()
            {
                return new RelatedValuesEnumerator(this);
            }

            private sealed class RelatedValuesEnumerator : IAsyncEnumerator<IValueReader>
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
                                .MoveNext(cancellationToken)
                                .WithCurrentCulture();

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

                public IValueReader Current { get; private set; }

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
