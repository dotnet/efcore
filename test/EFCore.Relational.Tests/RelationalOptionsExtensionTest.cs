// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Relational.Tests.TestUtilities.FakeProvider;
using Moq;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Relational.Tests
{
    public class RelationalOptionsExtensionTest
    {
        private const string ConnectionString = "Fraggle=Rock";

        [Fact]
        public void Can_set_Connection()
        {
            var optionsExtension = new FakeRelationalOptionsExtension();

            Assert.Null(optionsExtension.Connection);

            var connection = Mock.Of<DbConnection>();
            optionsExtension = (FakeRelationalOptionsExtension)optionsExtension.WithConnection(connection);

            Assert.Same(connection, optionsExtension.Connection);
        }

        [Fact]
        public void Throws_when_setting_Connection_to_null()
        {
            Assert.Throws<ArgumentNullException>(() => new FakeRelationalOptionsExtension().WithConnection(null));
        }

        [Fact]
        public void Can_set_ConnectionString()
        {
            var optionsExtension = new FakeRelationalOptionsExtension();

            Assert.Null(optionsExtension.ConnectionString);

            optionsExtension = (FakeRelationalOptionsExtension)optionsExtension.WithConnectionString(ConnectionString);

            Assert.Equal(ConnectionString, optionsExtension.ConnectionString);
        }

        [Fact]
        public void Throws_when_setting_ConnectionString_to_null()
        {
            Assert.Throws<ArgumentNullException>(() => new FakeRelationalOptionsExtension().WithConnectionString(null));
        }

        [Fact]
        public void Can_set_CommandTimeout()
        {
            var optionsExtension = new FakeRelationalOptionsExtension();

            Assert.Null(optionsExtension.CommandTimeout);

            optionsExtension = (FakeRelationalOptionsExtension)optionsExtension.WithCommandTimeout(1);

            Assert.Equal(1, optionsExtension.CommandTimeout);
        }

        [Fact]
        public void Throws_if_CommandTimeout_out_of_range()
        {
            Assert.Equal(
                RelationalStrings.InvalidCommandTimeout,
                Assert.Throws<InvalidOperationException>(
                    () => new FakeRelationalOptionsExtension().WithCommandTimeout(-1)).Message);
        }

        [Fact]
        public void Can_set_MaxBatchSize()
        {
            var optionsExtension = new FakeRelationalOptionsExtension();

            Assert.Null(optionsExtension.MaxBatchSize);

            optionsExtension = (FakeRelationalOptionsExtension)optionsExtension.WithMaxBatchSize(1);

            Assert.Equal(1, optionsExtension.MaxBatchSize);
        }

        [Fact]
        public void Throws_if_MaxBatchSize_out_of_range()
        {
            Assert.Equal(
                RelationalStrings.InvalidMaxBatchSize,
                Assert.Throws<InvalidOperationException>(
                    () => new FakeRelationalOptionsExtension().WithMaxBatchSize(-1)).Message);
        }
    }
}
