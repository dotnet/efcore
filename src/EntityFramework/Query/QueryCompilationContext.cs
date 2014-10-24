// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Query
{
    public abstract class QueryCompilationContext
    {
        private readonly IModel _model;
        private readonly ILogger _logger;
        private readonly ILinqOperatorProvider _linqOperatorProvider;
        private readonly IResultOperatorHandler _resultOperatorHandler;
        private readonly EntityMaterializerSource _entityMaterializerSource;

        protected QueryCompilationContext(
            [NotNull] IModel model,
            [NotNull] ILogger logger,
            [NotNull] ILinqOperatorProvider linqOperatorProvider,
            [NotNull] IResultOperatorHandler resultOperatorHandler,
            [NotNull] EntityMaterializerSource entityMaterializerSource)
        {
            Check.NotNull(model, "model");
            Check.NotNull(logger, "logger");
            Check.NotNull(linqOperatorProvider, "linqOperatorProvider");
            Check.NotNull(resultOperatorHandler, "resultOperatorHandler");
            Check.NotNull(entityMaterializerSource, "entityMaterializerSource");

            _model = model;
            _logger = logger;
            _linqOperatorProvider = linqOperatorProvider;
            _resultOperatorHandler = resultOperatorHandler;
            _entityMaterializerSource = entityMaterializerSource;
        }

        public virtual IModel Model
        {
            get { return _model; }
        }

        public virtual ILogger Logger
        {
            get { return _logger; }
        }

        public virtual ILinqOperatorProvider LinqOperatorProvider
        {
            get { return _linqOperatorProvider; }
        }

        public virtual IResultOperatorHandler ResultOperatorHandler
        {
            get { return _resultOperatorHandler; }
        }

        public virtual EntityMaterializerSource EntityMaterializerSource
        {
            get { return _entityMaterializerSource; }
        }

        public virtual EntityQueryModelVisitor CreateQueryModelVisitor()
        {
            return CreateQueryModelVisitor(null);
        }

        public abstract EntityQueryModelVisitor CreateQueryModelVisitor(
            [CanBeNull] EntityQueryModelVisitor parentEntityQueryModelVisitor);
    }
}
