// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Query.Annotations;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Query
{
    public abstract class QueryCompilationContext
    {
        private IReadOnlyCollection<QueryAnnotationBase> _queryAnnotations;
        private IDictionary<IQuerySource, List<IReadOnlyList<INavigation>>> _trackableIncludes;

        protected QueryCompilationContext(
            [NotNull] IModel model,
            [NotNull] ILogger logger,
            [NotNull] ILinqOperatorProvider linqOperatorProvider,
            [NotNull] IResultOperatorHandler resultOperatorHandler,
            [NotNull] IEntityMaterializerSource entityMaterializerSource,
            [NotNull] IEntityKeyFactorySource entityKeyFactorySource,
            [NotNull] IClrAccessorSource<IClrPropertyGetter> clrPropertyGetterSource)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(logger, nameof(logger));
            Check.NotNull(linqOperatorProvider, nameof(linqOperatorProvider));
            Check.NotNull(resultOperatorHandler, nameof(resultOperatorHandler));
            Check.NotNull(entityMaterializerSource, nameof(entityMaterializerSource));
            Check.NotNull(entityKeyFactorySource, nameof(entityKeyFactorySource));
            Check.NotNull(clrPropertyGetterSource, nameof(clrPropertyGetterSource));

            Model = model;
            Logger = logger;
            LinqOperatorProvider = linqOperatorProvider;
            ResultOperatorHandler = resultOperatorHandler;
            EntityMaterializerSource = entityMaterializerSource;
            EntityKeyFactorySource = entityKeyFactorySource;
            ClrPropertyGetterSource = clrPropertyGetterSource;
        }

        public virtual IModel Model { get; }
        public virtual ILogger Logger { get; }
        public virtual ILinqOperatorProvider LinqOperatorProvider { get; }
        public virtual IResultOperatorHandler ResultOperatorHandler { get; }
        public virtual IEntityMaterializerSource EntityMaterializerSource { get; }
        public virtual IEntityKeyFactorySource EntityKeyFactorySource { get; }
        public virtual IClrAccessorSource<IClrPropertyGetter> ClrPropertyGetterSource { get; }

        public virtual QuerySourceMapping QuerySourceMapping { get; } = new QuerySourceMapping();

        public virtual IReadOnlyCollection<QueryAnnotationBase> QueryAnnotations
        {
            get { return _queryAnnotations; }
            [param: NotNull]
            set
            {
                Check.NotNull(value, nameof(value));

                _queryAnnotations = value;
            }
        }

        public virtual IEnumerable<QueryAnnotation> GetCustomQueryAnnotations([NotNull] MethodInfo methodInfo)
            => _queryAnnotations
                .OfType<QueryAnnotation>()
                .Where(qa => qa.IsCallTo(Check.NotNull(methodInfo, nameof(methodInfo))));

        public virtual EntityQueryModelVisitor CreateQueryModelVisitor() => CreateQueryModelVisitor(null);

        public abstract EntityQueryModelVisitor CreateQueryModelVisitor(
            [CanBeNull] EntityQueryModelVisitor parentEntityQueryModelVisitor);

        public virtual void AddTrackableInclude(
            [NotNull] IQuerySource querySource, [NotNull] IReadOnlyList<INavigation> navigationPath)
        {
            Check.NotNull(querySource, nameof(querySource));
            Check.NotNull(navigationPath, nameof(navigationPath));

            if (_trackableIncludes == null)
            {
                _trackableIncludes = new Dictionary<IQuerySource, List<IReadOnlyList<INavigation>>>();
            }

            List<IReadOnlyList<INavigation>> includes;
            if (!_trackableIncludes.TryGetValue(querySource, out includes))
            {
                _trackableIncludes.Add(querySource, includes = new List<IReadOnlyList<INavigation>>());
            }

            includes.Add(navigationPath);
        }

        public virtual IReadOnlyList<IReadOnlyList<INavigation>> GetTrackableIncludes([NotNull] IQuerySource querySource)
        {
            Check.NotNull(querySource, nameof(querySource));

            if (_trackableIncludes == null)
            {
                return null;
            }

            List<IReadOnlyList<INavigation>> includes;

            return _trackableIncludes.TryGetValue(querySource, out includes) ? includes : null;
        }
    }
}
