// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests
{
    public class RelationalObjectArrayValueReaderFactoryTest
    {
        [Fact]
        public void Creates_RelationalObjectArrayValueReader()
        {
            Assert.IsType<RelationalObjectArrayValueReader>(
                new RelationalObjectArrayValueReaderFactory().Create(Mock.Of<DbDataReader>()));
        }
    }
}
