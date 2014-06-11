// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query
{
    public abstract class QueryCompilationContext
    {
        private readonly IModel _model;
        private readonly ILinqOperatorProvider _linqOperatorProvider;
        private readonly IResultOperatorHandler _resultOperatorHandler;

        protected QueryCompilationContext(
            [NotNull] IModel model,
            [NotNull] ILinqOperatorProvider linqOperatorProvider,
            [NotNull] IResultOperatorHandler resultOperatorHandler)
        {
            Check.NotNull(model, "model");
            Check.NotNull(linqOperatorProvider, "linqOperatorProvider");
            Check.NotNull(resultOperatorHandler, "resultOperatorHandler");

            _model = model;
            _linqOperatorProvider = linqOperatorProvider;
            _resultOperatorHandler = resultOperatorHandler;
        }

        public virtual IModel Model
        {
            get { return _model; }
        }

        public virtual ILinqOperatorProvider LinqOperatorProvider
        {
            get { return _linqOperatorProvider; }
        }

        public virtual IResultOperatorHandler ResultOperatorHandler
        {
            get { return _resultOperatorHandler; }
        }

        public abstract EntityQueryModelVisitor CreateQueryModelVisitor();
    }
}
