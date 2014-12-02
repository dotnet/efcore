// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Query
{
    public class IncludeCollectionIterator : IDisposable
    {
        private readonly IEnumerator<IValueReader> _relatedValuesEnumerator;

        private bool _hasRemainingRows;
        private bool _initialized;

        public IncludeCollectionIterator([NotNull] IEnumerator<IValueReader> relatedValuesEnumerator)
        {
            Check.NotNull(relatedValuesEnumerator, "relatedValuesEnumerator");

            _relatedValuesEnumerator = relatedValuesEnumerator;
        }

        public virtual IEnumerable<IValueReader> GetRelatedValues(
            [NotNull] EntityKey primaryKey,
            [NotNull] Func<IValueReader, EntityKey> relatedKeyFactory)
        {
            Check.NotNull(primaryKey, "primaryKey");
            Check.NotNull(relatedKeyFactory, "relatedKeyFactory");

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

        public virtual void Dispose()
        {
            _relatedValuesEnumerator.Dispose();
        }
    }
}
