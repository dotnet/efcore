// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Redis.Extensions;

namespace Microsoft.Data.Entity.Redis
{
    public abstract class BaseClassFixture
    {
        protected DbContext _context;

        public DbContext GetOrCreateContext()
        {
            if (!RedisTestConfig.GetOrStartServer())
            {
                throw new Exception(@"Could not find existing Redis Server nor start one at path "
                    + RedisTestConfig.GetServerPath());
            }

            if (_context == null)
            {
                var options = new DbContextOptions()
                    .UseModel(CreateModel())
                    .UseRedis();

                _context = new DbContext(options);
            }

            return _context;
        }

        public abstract IModel CreateModel();
    }
}
