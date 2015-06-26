// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Query
{
    public abstract class QueryContextFactory : IQueryContextFactory
    {
        private readonly LazyRef<ILogger> _logger;
        private readonly IStateManager _stateManager;
        private readonly IClrCollectionAccessorSource _collectionAccessorSource;
        private readonly IClrAccessorSource<IClrPropertySetter> _propertySetterSource;
        private readonly IEntityKeyFactorySource _entityKeyFactorySource;

        protected QueryContextFactory(
            [NotNull] IStateManager stateManager,
            [NotNull] IEntityKeyFactorySource entityKeyFactorySource,
            [NotNull] IClrCollectionAccessorSource collectionAccessorSource,
            [NotNull] IClrAccessorSource<IClrPropertySetter> propertySetterSource,
            [NotNull] ILoggerFactory loggerFactory)
        {
            Check.NotNull(stateManager, nameof(stateManager));
            Check.NotNull(entityKeyFactorySource, nameof(entityKeyFactorySource));
            Check.NotNull(collectionAccessorSource, nameof(collectionAccessorSource));
            Check.NotNull(propertySetterSource, nameof(propertySetterSource));
            Check.NotNull(loggerFactory, nameof(loggerFactory));

            _stateManager = stateManager;
            _entityKeyFactorySource = entityKeyFactorySource;
            _collectionAccessorSource = collectionAccessorSource;
            _propertySetterSource = propertySetterSource;

            _logger = new LazyRef<ILogger>(loggerFactory.CreateLogger<QueryContextFactory>);
        }

        public virtual ILogger Logger => _logger.Value;

        protected virtual IQueryBuffer CreateQueryBuffer()
            => new QueryBuffer(_stateManager, _entityKeyFactorySource, _collectionAccessorSource, _propertySetterSource);

        public abstract QueryContext Create();
    }
}
