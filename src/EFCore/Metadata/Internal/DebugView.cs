// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class DebugView<TMetadata>
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly TMetadata _metadata;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly Func<TMetadata, string> _toDebugString;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _view;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public DebugView([NotNull] TMetadata metadata, [NotNull] Func<TMetadata, string> toDebugString)
        {
            _metadata = metadata;
            _toDebugString = toDebugString;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string View => _view ?? (_view = _toDebugString(_metadata));
    }
}
