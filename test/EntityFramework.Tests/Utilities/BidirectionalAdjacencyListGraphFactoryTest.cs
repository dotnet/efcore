// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Utilities;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Utilities
{
    public class BidirectionalAdjacencyListGraphFactoryTest
    {
        [Fact]
        public void Creates_an_instance_of_the_generic_type()
        {
            var factory = new BidirectionalAdjacencyListGraphFactory().Create<string>();

            Assert.IsType<BidirectionalAdjacencyListGraph<string>>(factory);
        }
    }
}
