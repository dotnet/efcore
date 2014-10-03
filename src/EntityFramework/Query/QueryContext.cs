// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Query
{
    public class QueryContext
    {
        private readonly ILogger _logger;
        private readonly IQueryBuffer _queryBuffer;

        public QueryContext(
            [NotNull] ILogger logger,
            [NotNull] IQueryBuffer queryBuffer)
        {
            Check.NotNull(logger, "logger");
            Check.NotNull(queryBuffer, "queryBuffer");

            _logger = logger;
            _queryBuffer = queryBuffer;
        }

        // TODO: Move this to compilation context
        public virtual ILogger Logger
        {
            get { return _logger; }
        }

        public virtual IQueryBuffer QueryBuffer
        {
            get { return _queryBuffer; }
        }

        public virtual CancellationToken CancellationToken { get; set; }
    }
}
