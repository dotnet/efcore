// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class PoolableDbContext : DbContext
    {
        private IDbContextPool _contextPool;

        protected PoolableDbContext()
            : this(new DbContextOptions<PoolableDbContext>())
        {
        }

        public PoolableDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public virtual void SetPool(IDbContextPool contextPool) => _contextPool = contextPool;

        public override void Dispose()
        {
            if (_contextPool != null)
            {
                if (!_contextPool.Return(this))
                {
                    ((IDbContextPoolable)this).SetPool(null);
                    base.Dispose();
                }

                _contextPool = null;
            }
            else
            {
                base.Dispose();
            }
        }
    }
}
