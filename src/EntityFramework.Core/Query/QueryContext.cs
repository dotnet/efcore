// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query
{
    public class QueryContext
    {
        private readonly Func<IQueryBuffer> _queryBufferFactory;

        private IDictionary<string, object> _parameterValues;
        private IQueryBuffer _queryBuffer;

        public QueryContext([NotNull] Func<IQueryBuffer> queryBufferFactory)
        {
            Check.NotNull(queryBufferFactory, nameof(queryBufferFactory));

            _queryBufferFactory = queryBufferFactory;
        }

        public virtual IQueryBuffer QueryBuffer => _queryBuffer ?? (_queryBuffer = _queryBufferFactory());

        public virtual CancellationToken CancellationToken { get; set; }

        public virtual IDictionary<string, object> ParameterValues
            => _parameterValues ?? (_parameterValues = new Dictionary<string, object>());
    }
}
