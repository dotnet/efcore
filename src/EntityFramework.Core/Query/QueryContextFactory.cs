// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Query.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query
{
    public abstract class QueryContextFactory : IQueryContextFactory
    {
        private readonly IStateManager _stateManager;
        private readonly IClrCollectionAccessorSource _collectionAccessorSource;
        private readonly IClrAccessorSource<IClrPropertySetter> _propertySetterSource;
        private readonly IEntityKeyFactorySource _entityKeyFactorySource;

        protected QueryContextFactory(
            [NotNull] IStateManager stateManager,
            [NotNull] IEntityKeyFactorySource entityKeyFactorySource,
            [NotNull] IClrCollectionAccessorSource collectionAccessorSource,
            [NotNull] IClrAccessorSource<IClrPropertySetter> propertySetterSource)
        {
            Check.NotNull(stateManager, nameof(stateManager));
            Check.NotNull(entityKeyFactorySource, nameof(entityKeyFactorySource));
            Check.NotNull(collectionAccessorSource, nameof(collectionAccessorSource));
            Check.NotNull(propertySetterSource, nameof(propertySetterSource));

            _stateManager = stateManager;
            _entityKeyFactorySource = entityKeyFactorySource;
            _collectionAccessorSource = collectionAccessorSource;
            _propertySetterSource = propertySetterSource;
        }

        protected virtual IQueryBuffer CreateQueryBuffer()
            => new QueryBuffer(_stateManager, _entityKeyFactorySource, _collectionAccessorSource, _propertySetterSource);

        public abstract QueryContext Create();
    }
}
