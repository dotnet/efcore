// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.EntityFrameworkCore.Design
{
    public class DbContextActivatorTest
    {
        [Fact]
        public void CreateInstance_works()
        {
            var result = DbContextActivator.CreateInstance(typeof(TestContext));

            Assert.IsType<TestContext>(result);
        }

        private class TestContext : DbContext
        {
            protected override void OnConfiguring(DbContextOptionsBuilder options)
                => options.UseInMemoryDatabase(nameof(DbContextActivatorTest));
        }
    }
}
