// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
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
            var optionsExtension = new TestRelationalOptionsExtension();

            Assert.Null(optionsExtension.Connection);

            var connection = Mock.Of<DbConnection>();
            optionsExtension.Connection = connection;

            Assert.Same(connection, optionsExtension.Connection);
        }

        [Fact]
        public void Throws_when_setting_Connection_to_null()
        {
            Assert.Throws<ArgumentNullException>(() => { new TestRelationalOptionsExtension().Connection = null; });
        }

        [Fact]
        public void Can_set_ConnectionString()
        {
            var optionsExtension = new TestRelationalOptionsExtension();

            Assert.Null(optionsExtension.ConnectionString);

            optionsExtension.ConnectionString = ConnectionString;

            Assert.Equal(ConnectionString, optionsExtension.ConnectionString);
        }

        [Fact]
        public void Throws_when_setting_ConnectionString_to_null()
        {
            Assert.Throws<ArgumentNullException>(() => { new TestRelationalOptionsExtension().ConnectionString = null; });
        }

        [Fact]
        public void Can_set_CommandTimeout()
        {
            var optionsExtension = new TestRelationalOptionsExtension();

            Assert.Null(optionsExtension.CommandTimeout);

            optionsExtension.CommandTimeout = 1;

            Assert.Equal(1, optionsExtension.CommandTimeout);
        }

        [Fact]
        public void Throws_if_CommandTimeout_out_of_range()
        {
            Assert.Equal(
                RelationalStrings.InvalidCommandTimeout,
                Assert.Throws<InvalidOperationException>(
                    () => { new TestRelationalOptionsExtension().CommandTimeout = -1; }).Message);
        }

        [Fact]
        public void Can_set_MaxBatchSize()
        {
            var optionsExtension = new TestRelationalOptionsExtension();

            Assert.Null(optionsExtension.MaxBatchSize);

            optionsExtension.MaxBatchSize = 1;

            Assert.Equal(1, optionsExtension.MaxBatchSize);
        }

        [Fact]
        public void Throws_if_MaxBatchSize_out_of_range()
        {
            Assert.Equal(
                RelationalStrings.InvalidMaxBatchSize,
                Assert.Throws<InvalidOperationException>(
                    () => { new TestRelationalOptionsExtension().MaxBatchSize = -1; }).Message);
        }

        private class TestRelationalOptionsExtension : RelationalOptionsExtension
        {
            public override void ApplyServices(IServiceCollection services)
            {
                throw new NotImplementedException();
            }
        }
    }
}
