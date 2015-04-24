// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using Microsoft.Data.Entity.SqlServer.Query;
using Microsoft.Data.Entity.Storage;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests
{
    public class SqlServerValueReaderFactoryTest
    {
        [Fact]
        public void Creates_ObjectArrayValueReader()
        {
            Assert.IsType<ObjectArrayValueReader>(
                new SqlServerValueReaderFactoryFactory().CreateValueReaderFactory(new Type[0], 0)
                    .CreateValueReader(Mock.Of<DbDataReader>()));
        }
    }
}
