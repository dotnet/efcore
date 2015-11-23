// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Query.Internal;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query
{
    public class QueryContext
    {
        private readonly Func<IQueryBuffer> _queryBufferFactory;

        private readonly IDictionary<string, object> _parameterValues = new Dictionary<string, object>();

        private IQueryBuffer _queryBuffer;

        public QueryContext(
            [NotNull] Func<IQueryBuffer> queryBufferFactory,
            [NotNull] IStateManager stateManager)
        {
            Check.NotNull(queryBufferFactory, nameof(queryBufferFactory));
            Check.NotNull(stateManager, nameof(stateManager));

            _queryBufferFactory = queryBufferFactory;

            StateManager = stateManager;
        }

        public virtual IQueryBuffer QueryBuffer
            => _queryBuffer ?? (_queryBuffer = _queryBufferFactory());

        public virtual IStateManager StateManager { get; }

        public virtual CancellationToken CancellationToken { get; set; }

        public virtual IReadOnlyDictionary<string, object> ParameterValues
            => (IReadOnlyDictionary<string, object>)_parameterValues;

        public virtual void AddParameter([NotNull] string name, [CanBeNull] object value)
        {
            Check.NotEmpty(name, nameof(name));

            _parameterValues.Add(name, value);
        }

        public virtual void BeginTrackingQuery() => StateManager.BeginTrackingQuery();

        public virtual void StartTracking(
            [NotNull] object entity, [NotNull] EntityTrackingInfo entityTrackingInfo)
        {
            if (_queryBuffer != null)
            {
                _queryBuffer.StartTracking(entity, entityTrackingInfo);
            }
            else
            {
                entityTrackingInfo.StartTracking(StateManager, entity, ValueBuffer.Empty);
            }
        }
    }
}
