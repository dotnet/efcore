// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.ChangeTracking;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking
{
    public class PropertyEntryTest
    {
        [Fact]
        public void Can_get_name()
        {
            var entry = TestHelpers.Instance.CreateInternalEntry(
                TestHelpers.Instance.BuildModelFor<Wotty>(),
                EntityState.Unchanged,
                new Wotty { Id = 1, Primate = "Monkey" });

            Assert.Equal("Primate", new PropertyEntry(entry, "Primate").Metadata.Name);
        }

        [Fact]
        public void Can_get_current_value()
        {
            var entry = TestHelpers.Instance.CreateInternalEntry(
                TestHelpers.Instance.BuildModelFor<Wotty>(),
                EntityState.Unchanged,
                new Wotty { Id = 1, Primate = "Monkey" });

            Assert.Equal("Monkey", new PropertyEntry(entry, "Primate").CurrentValue);
        }

        [Fact]
        public void Can_set_current_value()
        {
            var entity = new Wotty { Id = 1, Primate = "Monkey" };

            var entry = TestHelpers.Instance.CreateInternalEntry(
                TestHelpers.Instance.BuildModelFor<Wotty>(),
                EntityState.Unchanged,
                entity);

            new PropertyEntry(entry, "Primate").CurrentValue = "Chimp";

            Assert.Equal("Chimp", entity.Primate);
        }

        [Fact]
        public void Can_set_current_value_to_null()
        {
            var entity = new Wotty { Id = 1, Primate = "Monkey" };

            var entry = TestHelpers.Instance.CreateInternalEntry(
                TestHelpers.Instance.BuildModelFor<Wotty>(),
                EntityState.Unchanged,
                entity);

            new PropertyEntry(entry, "Primate").CurrentValue = null;

            Assert.Null(entity.Primate);
        }

        [Fact]
        public void Can_set_and_get_original_value()
        {
            var entity = new Wotty { Id = 1, Primate = "Monkey" };

            var entry = TestHelpers.Instance.CreateInternalEntry(
                TestHelpers.Instance.BuildModelFor<Wotty>(),
                EntityState.Unchanged,
                entity);

            Assert.Equal("Monkey", new PropertyEntry(entry, "Primate").OriginalValue);

            new PropertyEntry(entry, "Primate").OriginalValue = "Chimp";

            Assert.Equal("Chimp", new PropertyEntry(entry, "Primate").OriginalValue);
            Assert.Equal("Monkey", entity.Primate);
        }

        [Fact]
        public void Can_set_original_value_to_null()
        {
            var entry = TestHelpers.Instance.CreateInternalEntry(
                TestHelpers.Instance.BuildModelFor<Wotty>(),
                EntityState.Unchanged,
                new Wotty { Id = 1, Primate = "Monkey" });

            new PropertyEntry(entry, "Primate").OriginalValue = null;

            Assert.Null(new PropertyEntry(entry, "Primate").OriginalValue);
        }

        [Fact]
        public void Can_set_and_clear_modified()
        {
            var entity = new Wotty { Id = 1, Primate = "Monkey" };

            var entry = TestHelpers.Instance.CreateInternalEntry(
                TestHelpers.Instance.BuildModelFor<Wotty>(),
                EntityState.Unchanged,
                entity);

            Assert.False(new PropertyEntry(entry, "Primate").IsModified);

            new PropertyEntry(entry, "Primate").IsModified = true;

            Assert.True(new PropertyEntry(entry, "Primate").IsModified);

            new PropertyEntry(entry, "Primate").IsModified = false;

            Assert.False(new PropertyEntry(entry, "Primate").IsModified);
        }

        [Fact]
        public void Can_get_name_generic()
        {
            var entry = TestHelpers.Instance.CreateInternalEntry(
                TestHelpers.Instance.BuildModelFor<Wotty>(),
                EntityState.Unchanged,
                new Wotty { Id = 1, Primate = "Monkey" });

            Assert.Equal("Primate", new PropertyEntry<Wotty, string>(entry, "Primate").Metadata.Name);
        }

        [Fact]
        public void Can_get_current_value_generic()
        {
            var entry = TestHelpers.Instance.CreateInternalEntry(
                TestHelpers.Instance.BuildModelFor<Wotty>(),
                EntityState.Unchanged,
                new Wotty { Id = 1, Primate = "Monkey" });

            Assert.Equal("Monkey", new PropertyEntry<Wotty, string>(entry, "Primate").CurrentValue);
        }

        [Fact]
        public void Can_set_current_value_generic()
        {
            var entity = new Wotty { Id = 1, Primate = "Monkey" };

            var entry = TestHelpers.Instance.CreateInternalEntry(
                TestHelpers.Instance.BuildModelFor<Wotty>(),
                EntityState.Unchanged,
                entity);

            new PropertyEntry<Wotty, string>(entry, "Primate").CurrentValue = "Chimp";

            Assert.Equal("Chimp", entity.Primate);
        }

        [Fact]
        public void Can_set_current_value_to_null_generic()
        {
            var entity = new Wotty { Id = 1, Primate = "Monkey" };

            var entry = TestHelpers.Instance.CreateInternalEntry(
                TestHelpers.Instance.BuildModelFor<Wotty>(),
                EntityState.Unchanged,
                entity);

            new PropertyEntry<Wotty, string>(entry, "Primate").CurrentValue = null;

            Assert.Null(entity.Primate);
        }

        [Fact]
        public void Can_set_and_get_original_value_generic()
        {
            var entity = new Wotty { Id = 1, Primate = "Monkey" };

            var entry = TestHelpers.Instance.CreateInternalEntry(
                TestHelpers.Instance.BuildModelFor<Wotty>(),
                EntityState.Unchanged,
                entity);

            Assert.Equal("Monkey", new PropertyEntry<Wotty, string>(entry, "Primate").OriginalValue);

            new PropertyEntry<Wotty, string>(entry, "Primate").OriginalValue = "Chimp";

            Assert.Equal("Chimp", new PropertyEntry<Wotty, string>(entry, "Primate").OriginalValue);
            Assert.Equal("Monkey", entity.Primate);
        }

        [Fact]
        public void Can_set_original_value_to_null_generic()
        {
            var entry = TestHelpers.Instance.CreateInternalEntry(
                TestHelpers.Instance.BuildModelFor<Wotty>(),
                EntityState.Unchanged,
                new Wotty { Id = 1, Primate = "Monkey" });

            new PropertyEntry<Wotty, string>(entry, "Primate").OriginalValue = null;

            Assert.Null(new PropertyEntry<Wotty, string>(entry, "Primate").OriginalValue);
        }

        [Fact]
        public void Can_set_and_clear_modified_generic()
        {
            var entity = new Wotty { Id = 1, Primate = "Monkey" };

            var entry = TestHelpers.Instance.CreateInternalEntry(
                TestHelpers.Instance.BuildModelFor<Wotty>(),
                EntityState.Unchanged,
                entity);

            Assert.False(new PropertyEntry<Wotty, string>(entry, "Primate").IsModified);

            new PropertyEntry(entry, "Primate").IsModified = true;

            Assert.True(new PropertyEntry<Wotty, string>(entry, "Primate").IsModified);

            new PropertyEntry(entry, "Primate").IsModified = false;

            Assert.False(new PropertyEntry<Wotty, string>(entry, "Primate").IsModified);
        }

        private class Wotty
        {
            public int Id { get; set; }
            public string Primate { get; set; }
        }
    }
}
