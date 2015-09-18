// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Relational.Internal;
using Microsoft.Data.Entity.TestUtilities.FakeProvider;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class RelationalOptionsExtensionTest
    {
        private const string ConnectionString = "Fraggle=Rock";

        [Fact]
        public void Can_set_Connection()
        {
            var optionsExtension = new FakeRelationalOptionsExtension();

            Assert.Null(optionsExtension.Connection);

            var connection = new FakeDbConnection(ConnectionString);
            optionsExtension.Connection = connection;

            Assert.Same(connection, optionsExtension.Connection);
        }

        [Fact]
        public void Throws_when_setting_Connection_to_null()
        {
            Assert.Throws<ArgumentNullException>(() => { new FakeRelationalOptionsExtension().Connection = null; });
        }

        [Fact]
        public void Can_set_ConnectionString()
        {
            var optionsExtension = new FakeRelationalOptionsExtension();

            Assert.Null(optionsExtension.ConnectionString);

            optionsExtension.ConnectionString = ConnectionString;

            Assert.Equal(ConnectionString, optionsExtension.ConnectionString);
        }

        [Fact]
        public void Throws_when_setting_ConnectionString_to_null()
        {
            Assert.Throws<ArgumentNullException>(() => { new FakeRelationalOptionsExtension().ConnectionString = null; });
        }

        [Fact]
        public void Can_set_CommandTimeout()
        {
            var optionsExtension = new FakeRelationalOptionsExtension();

            Assert.Null(optionsExtension.CommandTimeout);

            optionsExtension.CommandTimeout = 1;

            Assert.Equal(1, optionsExtension.CommandTimeout);
        }

        [Fact]
        public void Throws_if_CommandTimeout_out_of_range()
        {
            Assert.Equal(
                Strings.InvalidCommandTimeout,
                Assert.Throws<InvalidOperationException>(
                    () => { new FakeRelationalOptionsExtension().CommandTimeout = -1; }).Message);
        }

        [Fact]
        public void Can_set_MaxBatchSize()
        {
            var optionsExtension = new FakeRelationalOptionsExtension();

            Assert.Null(optionsExtension.MaxBatchSize);

            optionsExtension.MaxBatchSize = 1;

            Assert.Equal(1, optionsExtension.MaxBatchSize);
        }

        [Fact]
        public void Throws_if_MaxBatchSize_out_of_range()
        {
            Assert.Equal(
                Strings.InvalidMaxBatchSize,
                Assert.Throws<InvalidOperationException>(
                    () => { new FakeRelationalOptionsExtension().MaxBatchSize = -1; }).Message);
        }
    }
}
