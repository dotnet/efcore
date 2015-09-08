// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Xunit;

namespace Microsoft.Data.Entity.InMemory.Tests
{
    public class InMemoryDbContextOptionsExtensionsTest
    {
        [Fact]
        public void Can_add_extension_with_connection_string()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseInMemoryDatabase(persist: false);

            var extension = (InMemoryOptionsExtension)optionsBuilder.Options.Extensions.Single();

            Assert.False(extension.Persist);
        }

        [Fact]
        public void Can_add_extension_with_connection_string_using_generic_builder()
        {
            var optionsBuilder = new DbContextOptionsBuilder<DbContext>();
            optionsBuilder.UseInMemoryDatabase(persist: false);

            var extension = (InMemoryOptionsExtension)optionsBuilder.Options.Extensions.Single();

            Assert.False(extension.Persist);
        }

        [Fact]
        public void Can_add_extension_using_persist_true()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseInMemoryDatabase(persist: true);

            var extension = (InMemoryOptionsExtension)optionsBuilder.Options.Extensions.Single();

            Assert.True(extension.Persist);
        }
    }
}
