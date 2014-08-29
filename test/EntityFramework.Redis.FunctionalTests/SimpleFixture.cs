// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Redis.Extensions;

namespace Microsoft.Data.Entity.Redis.FunctionalTests
{
    public class SimpleFixture
    {
        public DbContext CreateContext()
        {
            var options = new DbContextOptions()
                .UseModel(CreateModel())
                .UseRedis("127.0.0.1", RedisTestConfig.RedisPort);

            return new DbContext(options);
        }

        public IModel CreateModel()
        {
            var model = new Model();
            var builder = new BasicModelBuilder(model);
            builder.Entity<SimplePoco>(b =>
                {
                    b.Key(cust => cust.PocoKey);
                    b.Property(cust => cust.PocoKey);
                    b.Property(cust => cust.Name);
                });

            return model;
        }
    }

    public class SimplePoco
    {
        public int PocoKey { get; set; }
        public string Name { get; set; }
    }
}
