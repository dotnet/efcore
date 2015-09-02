// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Storage;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Query
{
    public class InMemoryQueryCompilationContextFactory : IQueryCompilationContextFactory
    {
        private readonly IModel _model;
        private readonly ILogger _logger;
        private readonly IEntityMaterializerSource _entityMaterializerSource;
        private readonly IEntityKeyFactorySource _entityKeyFactorySource;
        private readonly IClrAccessorSource<IClrPropertyGetter> _clrPropertyGetterSource;

        public InMemoryQueryCompilationContextFactory(
            [NotNull] IModel model,
            [NotNull] ILoggerFactory loggerFactory,
            [NotNull] IEntityMaterializerSource entityMaterializerSource,
            [NotNull] IEntityKeyFactorySource entityKeyFactorySource,
            [NotNull] IClrAccessorSource<IClrPropertyGetter> clrPropertyGetterSource)
        {
            _model = model;
            _logger = loggerFactory.CreateLogger<Database>();
            _entityMaterializerSource = entityMaterializerSource;
            _entityKeyFactorySource = entityKeyFactorySource;
            _clrPropertyGetterSource = clrPropertyGetterSource;
        }

        public virtual QueryCompilationContext Create(bool async)
            => new InMemoryQueryCompilationContext(
                _model,
                _logger,
                _entityMaterializerSource,
                _entityKeyFactorySource,
                _clrPropertyGetterSource);
    }
}
