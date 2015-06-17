// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Tests;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.Relational.Tests
{
    public class RelationalTestHelpers : TestHelpers
    {
        protected RelationalTestHelpers()
        {
        }

        public new static RelationalTestHelpers Instance { get; } = new RelationalTestHelpers();

        public override EntityFrameworkServicesBuilder AddProviderServices(EntityFrameworkServicesBuilder builder) 
            => builder.AddInMemoryStore().AddRelational();

        protected override void UseProviderOptions(DbContextOptionsBuilder optionsBuilder)
        {
        }
    }
}
