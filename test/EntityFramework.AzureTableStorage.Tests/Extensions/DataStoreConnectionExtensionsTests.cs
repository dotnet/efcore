// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Storage;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests.Extensions
{
    public class DataStoreConnectionExtensionsTests
    {
        [Fact]
        public void It_requires_ats()
        {
            var connection = Mock.Of<DataStoreConnection>();
            Assert.Equal(
                Strings.AtsConnectionNotInUse,
                Assert.Throws<InvalidOperationException>(() => connection.AsAtsConnection()).Message
                );
        }

        [Fact]
        public void It_sets_batching()
        {
            var connection = new Mock<AtsConnection>();
            connection.Object.UseBatching(true);
            connection.VerifySet(s => s.Batching = true, Times.Once);
        }
    }
}
