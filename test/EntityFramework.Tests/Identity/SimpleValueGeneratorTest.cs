// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Identity
{
    public class SimpleValueGeneratorTest
    {
        [Fact]
        public async Task NextAsync_delegates_to_sync_method()
        {
            var stateEntry = Mock.Of<StateEntry>();
            var property = Mock.Of<IProperty>();

            var generatorMock = new Mock<SimpleValueGenerator> { CallBase = true };
            generatorMock.Setup(m => m.Next(stateEntry, property)).Returns("Boo!");

            Assert.Equal("Boo!", await generatorMock.Object.NextAsync(stateEntry, property));
        }
    }
}
