// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Query.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query
{
    public abstract class QueryContextFactory : IQueryContextFactory
    {
        private readonly IStateManager _stateManager;
        private readonly IKeyValueFactorySource _keyValueFactorySource;

        protected QueryContextFactory(
            [NotNull] IStateManager stateManager,
            [NotNull] IKeyValueFactorySource keyValueFactorySource)
        {
            Check.NotNull(stateManager, nameof(stateManager));
            Check.NotNull(keyValueFactorySource, nameof(keyValueFactorySource));

            _stateManager = stateManager;
            _keyValueFactorySource = keyValueFactorySource;
        }

        protected virtual IQueryBuffer CreateQueryBuffer()
            => new QueryBuffer(_stateManager, _keyValueFactorySource);

        public abstract QueryContext Create();
    }
}
