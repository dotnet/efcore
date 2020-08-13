// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public class RelationalOptionsExtensionTest
    {
        private const string ConnectionString = "Fraggle=Rock";

        [ConditionalFact]
        public void Can_set_Connection()
        {
            var optionsExtension = new FakeRelationalOptionsExtension();

            Assert.Null(optionsExtension.Connection);

            var connection = new FakeDbConnection("A=B");
            optionsExtension = (FakeRelationalOptionsExtension)optionsExtension.WithConnection(connection);

            Assert.Same(connection, optionsExtension.Connection);
        }

        [ConditionalFact]
        public void Can_set_ConnectionString()
        {
            var optionsExtension = new FakeRelationalOptionsExtension();

            Assert.Null(optionsExtension.ConnectionString);

            optionsExtension = (FakeRelationalOptionsExtension)optionsExtension.WithConnectionString(ConnectionString);

            Assert.Equal(ConnectionString, optionsExtension.ConnectionString);
        }

        [ConditionalFact]
        public void Can_set_CommandTimeout()
        {
            var optionsExtension = new FakeRelationalOptionsExtension();

            Assert.Null(optionsExtension.CommandTimeout);

            optionsExtension = (FakeRelationalOptionsExtension)optionsExtension.WithCommandTimeout(1);

            Assert.Equal(1, optionsExtension.CommandTimeout);
        }

        [ConditionalFact]
        public void Throws_if_CommandTimeout_out_of_range()
        {
            Assert.Equal(
                RelationalStrings.InvalidCommandTimeout,
                Assert.Throws<InvalidOperationException>(
                    () => new FakeRelationalOptionsExtension().WithCommandTimeout(-1)).Message);
        }

        [ConditionalFact]
        public void Can_set_MaxBatchSize()
        {
            var optionsExtension = new FakeRelationalOptionsExtension();

            Assert.Null(optionsExtension.MaxBatchSize);

            optionsExtension = (FakeRelationalOptionsExtension)optionsExtension.WithMaxBatchSize(1);

            Assert.Equal(1, optionsExtension.MaxBatchSize);
        }

        [ConditionalFact]
        public void Throws_if_MaxBatchSize_out_of_range()
        {
            Assert.Equal(
                RelationalStrings.InvalidMaxBatchSize,
                Assert.Throws<InvalidOperationException>(
                    () => new FakeRelationalOptionsExtension().WithMaxBatchSize(-1)).Message);
        }
    }
}
