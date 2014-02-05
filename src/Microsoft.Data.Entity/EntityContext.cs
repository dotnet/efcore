// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public class EntityContext : IDisposable
    {
        private readonly EntityConfiguration _entityConfiguration;

        public EntityContext(EntityConfiguration entityConfiguration)
        {
            Check.NotNull(entityConfiguration, "entityConfiguration");

            _entityConfiguration = entityConfiguration;
        }

        public virtual int SaveChanges()
        {
            // TODO
            return 0;
        }

        public virtual Task<int> SaveChangesAsync()
        {
            return SaveChangesAsync(CancellationToken.None);
        }

        public virtual Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            // TODO
            return Task.FromResult(0);
        }

        public void Dispose()
        {
            // TODO
        }

        public virtual Database Database
        {
            get { return new Database(); }
        }
    }
}
