// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Infrastructure;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests
{
    public class AtsDataStoreSourceTests
    {
        [Fact]
        public void Available_when_configured()
        {
            var config = new DbContextConfiguration();
            config.Initialize(
                Mock.Of<IServiceProvider>(),
                Mock.Of<IServiceProvider>(),
                new DbContextOptions(),
                Mock.Of<DbContext>(),
                DbContextConfiguration.ServiceProviderSource.Implicit);

            var source = new AtsDataStoreSource(config);

            Assert.False(source.IsAvailable);

            config.ContextOptions.AddExtension(new AtsOptionsExtension());

            Assert.True(source.IsAvailable);
        }

        [Fact]
        public void Named_correctly()
        {
            Assert.Equal("AzureTableStorage", new AtsDataStoreSource(Mock.Of<DbContextConfiguration>()).Name);
        }
    }
}
