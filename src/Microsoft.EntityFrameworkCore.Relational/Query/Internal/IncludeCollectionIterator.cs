// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class IncludeCollectionIterator : IDisposable
    {
        private readonly IEnumerator<ValueBuffer> _relatedValuesEnumerator;

        private bool _hasRemainingRows;
        private bool _initialized;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IncludeCollectionIterator([NotNull] IEnumerator<ValueBuffer> relatedValuesEnumerator)
        {
            _relatedValuesEnumerator = relatedValuesEnumerator;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<ValueBuffer> GetRelatedValues(
            [NotNull] IIncludeKeyComparer keyComparer)
        {
            if (!_initialized)
            {
                _hasRemainingRows = _relatedValuesEnumerator.MoveNext();
                _initialized = true;
            }

            while (_hasRemainingRows
                   && keyComparer.ShouldInclude(_relatedValuesEnumerator.Current))
            {
                yield return _relatedValuesEnumerator.Current;

                _hasRemainingRows = _relatedValuesEnumerator.MoveNext();
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Dispose() => _relatedValuesEnumerator.Dispose();
    }
}
