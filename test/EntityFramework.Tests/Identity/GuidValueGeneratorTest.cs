// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Identity
{
    public class GuidValueGeneratorTest
    {
        [Fact]
        public async Task Creates_GUIDs()
        {
            await Creates_GUIDs_test(async: false);
        }

        [Fact]
        public async Task Creates_GUIDs_async()
        {
            await Creates_GUIDs_test(async: true);
        }

        public async Task Creates_GUIDs_test(bool async)
        {
            var sequentialGuidIdentityGenerator = new GuidValueGenerator();

            var values = new HashSet<Guid>();
            for (var i = 0; i < 100; i++)
            {
                var guid = async
                    ? await sequentialGuidIdentityGenerator.NextAsync(Mock.Of<DbContextConfiguration>(), Mock.Of<IProperty>())
                    : sequentialGuidIdentityGenerator.Next(Mock.Of<DbContextConfiguration>(), Mock.Of<IProperty>());

                values.Add((Guid)guid);
            }

            Assert.Equal(100, values.Count);
        }
    }
}
