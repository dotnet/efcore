// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query
{
    public class QueryContext
    {
        private readonly Func<IQueryBuffer> _queryBufferFactory;
        private readonly IDictionary<string, object> _parameterValues = new Dictionary<string, object>();

        private IQueryBuffer _queryBuffer;

        public QueryContext([NotNull] Func<IQueryBuffer> queryBufferFactory)
        {
            Check.NotNull(queryBufferFactory, nameof(queryBufferFactory));

            _queryBufferFactory = queryBufferFactory;
        }

        public virtual IQueryBuffer QueryBuffer
            => _queryBuffer ?? (_queryBuffer = _queryBufferFactory());

        public virtual CancellationToken CancellationToken { get; set; }

        public virtual IReadOnlyDictionary<string, object> ParameterValues
            => (IReadOnlyDictionary<string, object>)_parameterValues;

        public virtual void AddParameter([NotNull] string name, [CanBeNull] object value)
        {
            Check.NotEmpty(name, nameof(name));

            _parameterValues.Add(name, value);
        }
    }
}
