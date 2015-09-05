// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.Query.Internal
{
    public class IncludeCollectionIterator : IDisposable
    {
        private readonly IEnumerator<ValueBuffer> _relatedValuesEnumerator;

        private bool _hasRemainingRows;
        private bool _initialized;

        public IncludeCollectionIterator([NotNull] IEnumerator<ValueBuffer> relatedValuesEnumerator)
        {
            _relatedValuesEnumerator = relatedValuesEnumerator;
        }

        public virtual IEnumerable<ValueBuffer> GetRelatedValues(
            [NotNull] EntityKey primaryKey,
            [NotNull] Func<ValueBuffer, EntityKey> relatedKeyFactory)
        {
            if (!_initialized)
            {
                _hasRemainingRows = _relatedValuesEnumerator.MoveNext();
                _initialized = true;
            }

            while (_hasRemainingRows
                   && relatedKeyFactory(_relatedValuesEnumerator.Current).Equals(primaryKey))
            {
                yield return _relatedValuesEnumerator.Current;

                _hasRemainingRows = _relatedValuesEnumerator.MoveNext();
            }
        }

        public virtual void Dispose() => _relatedValuesEnumerator.Dispose();
    }
}
