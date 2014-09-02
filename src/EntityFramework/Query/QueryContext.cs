// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Query
{
    public class QueryContext
    {
        private readonly IModel _model;
        private readonly ILogger _logger;
        private readonly IMaterializationStrategy _materializationStrategy;

        public QueryContext(
            [NotNull] IModel model,
            [NotNull] ILogger logger,
            [NotNull] IMaterializationStrategy materializationStrategy)
        {
            Check.NotNull(model, "model");
            Check.NotNull(logger, "logger");
            Check.NotNull(materializationStrategy, "materializationStrategy");

            _model = model;
            _logger = logger;
            _materializationStrategy = materializationStrategy;
        }

        public virtual IModel Model
        {
            get { return _model; }
        }

        public virtual ILogger Logger
        {
            get { return _logger; }
        }

        public virtual IMaterializationStrategy MaterializationStrategy
        {
            get { return _materializationStrategy; }
        }

        public virtual CancellationToken CancellationToken { get; set; }
    }
}
