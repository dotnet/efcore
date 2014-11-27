// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class EntityServiceCollectionExtensionsTest
    {
        [Fact]
        public void AddEntityFramework_does_not_replace_services_already_registered()
        {
            var services = new ServiceCollection()
                .AddSingleton<DbSetSource, FakeDbSetSource>();

            services.AddEntityFramework();

            var serviceProvider = services.BuildServiceProvider();

            Assert.IsType<FakeDbSetSource>(serviceProvider.GetRequiredService<DbSetSource>());
        }

        private class FakeDbSetSource : DbSetSource
        {
        }
    }
}
