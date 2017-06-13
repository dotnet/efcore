// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.EntityFrameworkCore.Migrations.Internal
{
    public class MigrationsIdGeneratorTest
    {
        [Fact]
        public void CreateId_works()
        {
            var id = new MigrationsIdGenerator().GenerateId("Twilight");

            Assert.Matches("[0-9]{14}_Twilight", id);
        }

        [Fact]
        public void CreateId_always_increments_timestamp()
        {
            var generator = new MigrationsIdGenerator();

            var id1 = generator.GenerateId("Rainbow");
            var id2 = generator.GenerateId("Rainbow");

            Assert.NotEqual(id1, id2);
        }

        [Fact]
        public void GetName_works()
        {
            var name = new MigrationsIdGenerator().GetName("20150302100620_Apple");

            Assert.Equal("Apple", name);
        }

        [Fact]
        public void IsValidId_returns_true_when_valid()
        {
            var valid = new MigrationsIdGenerator().IsValidId("20150302100930_Rarity");

            Assert.True(valid);
        }

        [Fact]
        public void IsValidId_returns_false_when_invalid()
        {
            var valid = new MigrationsIdGenerator().IsValidId("Rarity");

            Assert.False(valid);
        }
    }
}
