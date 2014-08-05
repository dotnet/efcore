// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Infrastructure;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Update
{
    public class ModificationCommandBatchFactoryTest
    {
        [Fact]
        public void Constructor_checks_arguments()
        {
            Assert.Equal(
                "sqlGenerator",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() =>
                    new ModificationCommandBatchFactory(null)).ParamName);
        }

        [Fact]
        public void Create_returns_new_instances()
        {
            var factory = new ModificationCommandBatchFactory(
                new Mock<SqlGenerator>().Object);

            var firstBatch = factory.Create();
            var secondBatch = factory.Create();

            Assert.NotNull(firstBatch);
            Assert.NotNull(secondBatch);
            Assert.NotSame(firstBatch, secondBatch);
        }

        [Fact]
        public void AddCommand_checks_arguments()
        {
            var factory = new ModificationCommandBatchFactory(
                new Mock<SqlGenerator>().Object);

            Assert.Equal(
                "modificationCommandBatch",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() =>
                    factory.AddCommand(null, new Mock<ModificationCommand>().Object)).ParamName);

            Assert.Equal(
                "modificationCommand",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() =>
                    factory.AddCommand(new Mock<ModificationCommandBatch>().Object, null)).ParamName);
        }

        [Fact]
        public void AddCommand_delegates()
        {
            var sqlGenerator = new Mock<SqlGenerator>().Object;
            var factory = new ModificationCommandBatchFactory(
                sqlGenerator);

            var modificationCommandBatchMock = new Mock<ModificationCommandBatch>();
            var mockModificationCommand = new Mock<ModificationCommand>().Object;

            factory.AddCommand(modificationCommandBatchMock.Object, mockModificationCommand);

            modificationCommandBatchMock.Verify(mcb => mcb.AddCommand(mockModificationCommand, sqlGenerator));
        }
    }
}
