// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Internal;
using Moq;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Tests
{
    public class DbSetSourceTest
    {
        [Fact]
        public void Can_create_new_generic_DbSet()
        {
            var context = new Mock<DbContext>().Object;

            var factorySource = new DbSetSource();

            var set = factorySource.Create(context, typeof(Random));

            Assert.IsType<InternalDbSet<Random>>(set);
        }

        [Fact]
        public void Always_creates_a_new_DbSet_instance()
        {
            var context = new Mock<DbContext>().Object;

            var factorySource = new DbSetSource();

            Assert.NotSame(factorySource.Create(context, typeof(Random)), factorySource.Create(context, typeof(Random)));
        }
    }
}
