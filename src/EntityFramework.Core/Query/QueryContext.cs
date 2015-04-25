// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Query
{
    public class QueryContext
    {
        private readonly IStateManager _stateManager;

        private IDictionary<string, object> _parameterValues;

        public QueryContext(
            [NotNull] ILogger logger,
            [NotNull] IQueryBuffer queryBuffer,
            [NotNull] IStateManager stateManager)
        {
            Check.NotNull(logger, nameof(logger));
            Check.NotNull(queryBuffer, nameof(queryBuffer));

            Logger = logger;
            QueryBuffer = queryBuffer;

            _stateManager = stateManager;
        }

        // TODO: Move this to compilation context
        public virtual ILogger Logger { get; }

        public virtual IQueryBuffer QueryBuffer { get; }

        public virtual CancellationToken CancellationToken { get; set; }

        public virtual IDictionary<string, object> ParameterValues
            => _parameterValues
               ?? (_parameterValues = new Dictionary<string, object>());

        public virtual Type ContextType { get; [param: NotNull] set; }

        public virtual void StartTracking(
            [NotNull] IEntityType entityType,
            [NotNull] object instance,
            ValueBuffer valueBuffer)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(instance, nameof(instance));
            Check.NotNull(valueBuffer, nameof(valueBuffer));

            _stateManager.StartTracking(entityType, instance, valueBuffer);

            // TODO: Remove #2015, #2016
            QueryBuffer.StartTracking(instance);
        }

        public virtual object GetPropertyValue(
            [NotNull] object entity,
            [NotNull] QuerySourceScope querySourceScope,
            [NotNull] IProperty property)
        {
            Check.NotNull(entity, nameof(entity));
            Check.NotNull(querySourceScope, nameof(querySourceScope));
            Check.NotNull(property, nameof(property));

            var entry = _stateManager.TryGetEntry(entity);

            return entry != null 
                ? entry[property] 
                : querySourceScope.GetValueBuffer(entity)[property.Index];
        }
    }
}
