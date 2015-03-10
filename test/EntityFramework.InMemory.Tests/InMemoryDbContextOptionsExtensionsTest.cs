//// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
//// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

//using System.Linq;
//using Microsoft.Data.Entity.Infrastructure;
//using Xunit;

//namespace Microsoft.Data.Entity.InMemory.Tests
//{
//    public class InMemoryDbContextOptionsExtensionsTest
//    {
//        [Fact]
//        public void Can_add_extension_with_connection_string()
//        {
//            var optionsBuilder = new DbContextOptionsBuilder();
//            optionsBuilder.UseInMemoryStore(persist: false);

//            var extension = ((IDbContextOptions)options).Extensions.OfType<InMemoryOptionsExtension>().Single();

//            Assert.False(extension.Persist);
//        }

//        [Fact]
//        public void Can_add_extension_with_connection_string_using_generic_options()
//        {
//            var options = new DbContextOptions<DbContext>();
//            optionsBuilder.UseInMemoryStore(persist: false);

//            var extension = ((IDbContextOptions)options).Extensions.OfType<InMemoryOptionsExtension>().Single();

//            Assert.False(extension.Persist);
//        }

//        [Fact]
//        public void Can_add_extension_using_persist_true()
//        {
//            var optionsBuilder = new DbContextOptionsBuilder();
//            optionsBuilder.UseInMemoryStore(persist: true);

//            var extension = ((IDbContextOptions)options).Extensions.OfType<InMemoryOptionsExtension>().Single();

//            Assert.True(extension.Persist);
//        }
//    }
//}
