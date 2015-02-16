// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.ValueGeneration;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ValueGeneration
{
    public class SimpleTemporaryValueGeneratorTest
    {
        [Fact]
        public void Next_with_services_delegates_to_non_services_method()
        {
            var generator = new TestValueGenerator();

            var generatedValue = generator.Next(new DbContextService<DataStoreServices>(() => null));

            Assert.Equal(1, generatedValue);
            Assert.True(generator.GeneratesTemporaryValues);
        }

        private class TestValueGenerator : SimpleTemporaryValueGenerator<int>
        {
            public override int Next() => 1;
        }
    }
}
