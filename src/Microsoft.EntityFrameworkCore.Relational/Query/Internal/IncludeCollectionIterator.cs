// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Query.Internal
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

        public virtual IEnumerable<ValueBuffer> GetRelatedValues([NotNull] IIncludeKeyComparer keyComparer)
        {
            if (!_initialized)
            {
                _hasRemainingRows = _relatedValuesEnumerator.MoveNext();
                _initialized = true;
            }

            while (_hasRemainingRows
                   && !keyComparer.ShouldInclude(_relatedValuesEnumerator.Current))
            {
                _hasRemainingRows = _relatedValuesEnumerator.MoveNext();
            }

            while (_hasRemainingRows
                   && keyComparer.ShouldInclude(_relatedValuesEnumerator.Current))
            {
                yield return _relatedValuesEnumerator.Current;

                _hasRemainingRows = _relatedValuesEnumerator.MoveNext();
            }
        }

        public virtual void Dispose() => _relatedValuesEnumerator.Dispose();
    }
}
