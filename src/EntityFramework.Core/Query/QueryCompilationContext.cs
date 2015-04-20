// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Query
{
    public abstract class QueryCompilationContext
    {
        private ICollection<QueryAnnotation> _queryAnnotations;

        protected QueryCompilationContext(
            [NotNull] IModel model,
            [NotNull] ILogger logger,
            [NotNull] ILinqOperatorProvider linqOperatorProvider,
            [NotNull] IResultOperatorHandler resultOperatorHandler,
            [NotNull] IEntityMaterializerSource entityMaterializerSource,
            [NotNull] IEntityKeyFactorySource entityKeyFactorySource)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(logger, nameof(logger));
            Check.NotNull(linqOperatorProvider, nameof(linqOperatorProvider));
            Check.NotNull(resultOperatorHandler, nameof(resultOperatorHandler));
            Check.NotNull(entityMaterializerSource, nameof(entityMaterializerSource));
            Check.NotNull(entityKeyFactorySource, nameof(entityKeyFactorySource));

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

        public virtual IEntityMaterializerSource EntityMaterializerSource { get; }

        public virtual IEntityKeyFactorySource EntityKeyFactorySource { get; }

        public virtual QuerySourceMapping QuerySourceMapping { get; } = new QuerySourceMapping();

        public virtual ICollection<QueryAnnotation> QueryAnnotations
        {
            get { return _queryAnnotations; }
            [param: NotNull]
            set
            {
                Check.NotNull(value, nameof(value));

                _queryAnnotations = value;
            }
        }

        public virtual EntityQueryModelVisitor CreateQueryModelVisitor() => CreateQueryModelVisitor(null);

        public abstract EntityQueryModelVisitor CreateQueryModelVisitor(
            [CanBeNull] EntityQueryModelVisitor parentEntityQueryModelVisitor);
    }
}
