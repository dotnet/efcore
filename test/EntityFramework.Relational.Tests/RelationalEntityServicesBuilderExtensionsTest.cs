// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Framework.DependencyInjection;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests
{
    public class RelationalEntityServicesBuilderExtensionsTest
    {
        [Fact]
        public void AddRelational_does_not_replace_services_already_registered()
        {
            var services = new ServiceCollection()
                .AddSingleton<IComparer<ModificationCommand>, FakeModificationCommandComparer>();

            services.AddEntityFramework().AddRelational();

            var serviceProvider = services.BuildServiceProvider();

            Assert.IsType<FakeModificationCommandComparer>(serviceProvider.GetRequiredService<IComparer<ModificationCommand>>());
        }

        private class FakeModificationCommandComparer : ModificationCommandComparer
        {
        }
    }
}
