// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.Query.Internal
{
    public class RelationalQueryContextFactory : QueryContextFactory
    {
        private readonly IRelationalConnection _connection;

        public RelationalQueryContextFactory(
            [NotNull] IStateManager stateManager,
            [NotNull] IKeyValueFactorySource keyValueFactorySource,
            [NotNull] IRelationalConnection connection)
            : base(stateManager, keyValueFactorySource)
        {
            _connection = connection;
        }

        public override QueryContext Create()
            => new RelationalQueryContext(CreateQueryBuffer, _connection);
    }
}
