// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Relational.Update;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests.Update
{
    public class ModificationCommandBatchFactoryTest
    {
        [Fact]
        public void Create_returns_new_instances()
        {
            var factory = new TestModificationCommandBatchFactory(
                Mock.Of<ISqlGenerator>());

            var options = new Mock<IEntityOptions>().Object;
            var metadataExtensionProvider = Mock.Of<IRelationalMetadataExtensionProvider>();

            var firstBatch = factory.Create(options, metadataExtensionProvider);
            var secondBatch = factory.Create(options, metadataExtensionProvider);

            Assert.NotNull(firstBatch);
            Assert.NotNull(secondBatch);
            Assert.NotSame(firstBatch, secondBatch);
        }

        [Fact]
        public void AddCommand_delegates()
        {
            var sqlGenerator = new Mock<ISqlGenerator>().Object;
            var factory = new TestModificationCommandBatchFactory(sqlGenerator);

            var modificationCommandBatchMock = new Mock<ModificationCommandBatch>(sqlGenerator);
            var mockModificationCommand = new Mock<ModificationCommand>(
                "T",
                "S",
                new ParameterNameGenerator(),
                (Func<IProperty, IRelationalPropertyExtensions>)(p => p.Relational()),
                Mock.Of<IRelationalValueBufferFactoryFactory>()).Object;

            factory.AddCommand(modificationCommandBatchMock.Object, mockModificationCommand);

            modificationCommandBatchMock.Verify(mcb => mcb.AddCommand(mockModificationCommand));
        }

        private class TestMetadataExtensionProvider : IRelationalMetadataExtensionProvider
        {
            public IRelationalEntityTypeExtensions Extensions(IEntityType entityType) => entityType.Relational();
            public IRelationalForeignKeyExtensions Extensions(IForeignKey foreignKey) => foreignKey.Relational();
            public IRelationalIndexExtensions Extensions(IIndex index) => index.Relational();
            public IRelationalKeyExtensions Extensions(IKey key) => key.Relational();
            public IRelationalPropertyExtensions Extensions(IProperty property) => property.Relational();
            public IRelationalModelExtensions Extensions(IModel model) => model.Relational();
        }

        private class TestModificationCommandBatchFactory : ModificationCommandBatchFactory
        {
            public TestModificationCommandBatchFactory(
                ISqlGenerator sqlGenerator)
                : base(sqlGenerator)
            {
            }

            public override ModificationCommandBatch Create(
                IEntityOptions options,
                IRelationalMetadataExtensionProvider metadataExtensionProvider)
            {
                return new SingularModificationCommandBatch(SqlGenerator, metadataExtensionProvider);
            }
        }
    }
}
