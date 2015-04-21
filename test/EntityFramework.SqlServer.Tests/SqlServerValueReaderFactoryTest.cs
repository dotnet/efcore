// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.SqlServer.Query;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests
{
    public class SqlServerValueReaderFactoryTest
    {
        [Fact]
        public void Creates_RelationalObjectArrayValueReader()
        {
            Assert.IsType<RelationalObjectArrayValueReader>(
                new SqlServerValueReaderFactory().CreateValueReader(Mock.Of<DbDataReader>(), new[] { typeof(int) }, 0));
        }
    }
}
