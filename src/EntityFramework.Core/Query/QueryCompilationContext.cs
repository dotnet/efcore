// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Query
{
    public abstract class QueryCompilationContext
    {
        protected QueryCompilationContext(
            [NotNull] IModel model,
            [NotNull] ILogger logger,
            [NotNull] ILinqOperatorProvider linqOperatorProvider,
            [NotNull] IResultOperatorHandler resultOperatorHandler,
            [NotNull] EntityMaterializerSource entityMaterializerSource,
            [NotNull] EntityKeyFactorySource entityKeyFactorySource)
        {
            Check.NotNull(model, "model");
            Check.NotNull(logger, "logger");
            Check.NotNull(linqOperatorProvider, "linqOperatorProvider");
            Check.NotNull(resultOperatorHandler, "resultOperatorHandler");
            Check.NotNull(entityMaterializerSource, "entityMaterializerSource");
            Check.NotNull(entityKeyFactorySource, "entityKeyFactorySource");

            Model = model;
            Logger = logger;
            LinqOperatorProvider = linqOperatorProvider;
            ResultOperatorHandler = resultOperatorHandler;
            EntityMaterializerSource = entityMaterializerSource;
            EntityKeyFactorySource = entityKeyFactorySource;
        }

        public virtual IModel Model { get; }

        public virtual ILogger Logger { get; }

        public virtual ILinqOperatorProvider LinqOperatorProvider { get; }

        public virtual IResultOperatorHandler ResultOperatorHandler { get; }

        public virtual EntityMaterializerSource EntityMaterializerSource { get; }

        public virtual EntityKeyFactorySource EntityKeyFactorySource { get; }

        public virtual EntityQueryModelVisitor CreateQueryModelVisitor()
        {
            return CreateQueryModelVisitor(null);
        }

        public abstract EntityQueryModelVisitor CreateQueryModelVisitor(
            [CanBeNull] EntityQueryModelVisitor parentEntityQueryModelVisitor);
    }
}
