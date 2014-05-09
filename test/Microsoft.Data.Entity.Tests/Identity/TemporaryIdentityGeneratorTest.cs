// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Data.Entity.Identity;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Identity
{
    public class TemporaryIdentityGeneratorTest
    {
        [Fact]
        public async Task Creates_negative_keys()
        {
            var generator = new TemporaryIdentityGenerator();

            Assert.Equal(-1, await generator.NextAsync());
            Assert.Equal(-2, await generator.NextAsync());
            Assert.Equal(-3, await generator.NextAsync());

            Assert.Equal(-4, await ((IIdentityGenerator)generator).NextAsync());
            Assert.Equal(-5, await ((IIdentityGenerator)generator).NextAsync());
            Assert.Equal(-6, await ((IIdentityGenerator)generator).NextAsync());
        }
    }
}
