// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query
{
    public class QueryContext
    {
        private IDictionary<string, object> _parameterValues;

        public QueryContext([NotNull] IQueryBuffer queryBuffer)
        {
            Check.NotNull(queryBuffer, nameof(queryBuffer));

            QueryBuffer = queryBuffer;
        }

        public virtual IQueryBuffer QueryBuffer { get; }

        public virtual CancellationToken CancellationToken { get; set; }

        public virtual IDictionary<string, object> ParameterValues
            => _parameterValues ?? (_parameterValues = new Dictionary<string, object>());
    }
}
