// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.FunctionalTests;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Tests
{
    public class RelationalTestHelpers : TestHelpers
    {
        protected RelationalTestHelpers()
        {
        }

        public new static RelationalTestHelpers Instance { get; } = new RelationalTestHelpers();

        public override EntityFrameworkServicesBuilder AddProviderServices(EntityFrameworkServicesBuilder builder)
            => builder.AddInMemoryDatabase().AddRelational();

        protected override void UseProviderOptions(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseInMemoryDatabase().IgnoreTransactions();
    }
}
