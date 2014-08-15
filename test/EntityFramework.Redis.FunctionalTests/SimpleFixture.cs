// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Redis
{
    public class SimpleFixture : BaseClassFixture, IDisposable
    {
        public override IModel CreateModel()
        {
            var model = new Model();
            var builder = new BasicModelBuilder(model);
            builder.Entity<Customer>(b =>
                {
                    b.Key(cust => cust.CustomerID);
                    b.Property(cust => cust.CustomerID);
                    b.Property(cust => cust.Name);
                });

            return model;
        }

        void IDisposable.Dispose()
        {
            RedisTestConfig.StopRedisServer();
        }
    }

    public class Customer
    {
        public int CustomerID { get; set; }
        public string Name { get; set; }
    }
}
