// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;

namespace Microsoft.Data.Entity.InMemory.FunctionalTests
{
    public class InMemoryBuiltInDataTypesFixture : BuiltInDataTypesFixtureBase<InMemoryTestStore>
    {
        private readonly IServiceProvider _serviceProvider;

        public InMemoryBuiltInDataTypesFixture()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddInMemoryStore()
                .ServiceCollection
                .BuildServiceProvider();
        }

        public override InMemoryTestStore CreateTestStore()
        {
            return new InMemoryTestStore();
        }

        public override DbContext CreateContext(InMemoryTestStore testStore)
        {
            var options = new DbContextOptions()
                .UseModel(CreateModel())
                .UseInMemoryStore();

            return new DbContext(_serviceProvider, options);
        }

        public override void OnModelCreating(BasicModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BuiltInNonNullableDataTypes>(b =>
            {
                b.Property(dt => dt.TestInt16);
                b.Property(dt => dt.TestUnsignedInt16);
                b.Property(dt => dt.TestUnsignedInt32);
                b.Property(dt => dt.TestUnsignedInt64);
                b.Property(dt => dt.TestCharacter);
                b.Property(dt => dt.TestSignedByte);
            });

            modelBuilder.Entity<BuiltInNullableDataTypes>(b =>
            {
                b.Property(dt => dt.TestNullableInt16);
                b.Property(dt => dt.TestNullableUnsignedInt16);
                b.Property(dt => dt.TestNullableUnsignedInt32);
                b.Property(dt => dt.TestNullableUnsignedInt64);
                b.Property(dt => dt.TestNullableCharacter);
                b.Property(dt => dt.TestNullableSignedByte);
            });
        }
    }
}
