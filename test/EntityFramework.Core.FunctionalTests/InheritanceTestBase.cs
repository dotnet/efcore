// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.FunctionalTests.TestModels.Inheritance;
using Xunit;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class InheritanceTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : InheritanceFixtureBase, new()
    {
        //[Fact]
        public virtual void Can_query_all_animals()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(2, context.Animals.ToList().Count);
            }
        }

        protected AnimalContext CreateContext()
        {
            return Fixture.CreateContext();
        }

        protected TFixture Fixture { get; }

        protected InheritanceTestBase(TFixture fixture)
        {
            Fixture = fixture;
        }
    }
}
