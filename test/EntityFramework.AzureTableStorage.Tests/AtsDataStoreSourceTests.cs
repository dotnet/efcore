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
        private readonly AtsDataStoreSource _source = new AtsDataStoreSource();

        [Fact]
        public void Available_when_configured()
        {
            var config = new DbContextConfiguration();
            config.Initialize(
                Mock.Of<IServiceProvider>(),
                Mock.Of<IServiceProvider>(),
                new DbContextOptions(),
                Mock.Of<DbContext>(),
                DbContextConfiguration.ServiceProviderSource.Implicit)
                ;

            Assert.False(_source.IsAvailable(config));

            config.ContextOptions.AddExtension(new AtsOptionsExtension());

            Assert.True(_source.IsAvailable(config));
        }

        [Fact]
        public void Named_correctly()
        {
            Assert.Equal("AzureTableStorage", _source.Name);
        }
    }
}
