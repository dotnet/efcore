// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Identity
{
    public class SimpleValueGeneratorTest
    {
        [Fact]
        public async Task NextAsync_delegates_to_sync_method()
        {
            var stateEntry = TestHelpers.CreateStateEntry<AnEntity>(TestHelpers.BuildModelFor<AnEntity>());
            var property = stateEntry.EntityType.GetProperty("Id");

            var generator = new TestValueGenerator();

            await generator.NextAsync(stateEntry, property);

            Assert.Same(generator.StateEntry, stateEntry);
            Assert.Same(generator.Property, property);
        }

        private class TestValueGenerator : SimpleValueGenerator
        {
            public StateEntry StateEntry { get; set; }
            public IProperty Property { get; set; }

            public override void Next(StateEntry stateEntry, IProperty property)
            {
                StateEntry = stateEntry;
                Property = property;
            }
        }

        private class AnEntity
        {
            public int Id { get; set; }
        }
    }
}
