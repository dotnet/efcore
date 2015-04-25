// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data;
using System.Data.Common;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Relational.Update;
using Moq;
using Moq.Protected;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests.Model
{
    public class RelationalTypeMappingTest
    {
        [Fact]
        public void CreateParameter_creates_parameter_with_current_values()
        {
            var parameterMock = new Mock<DbParameter>();
            var commandMock = new Mock<DbCommand>();
            commandMock
                .Protected()
                .Setup<DbParameter>("CreateDbParameter")
                .Returns(parameterMock.Object);

            var columnModificationMock = new Mock<ColumnModification>(
                CreateInternalEntryMock().Object,
                Mock.Of<IProperty>(),
                Mock.Of<IRelationalPropertyExtensions>(),
                new ParameterNameGenerator(),
                false, false, false, false);

            columnModificationMock.Setup(m => m.Property).Returns(new Mock<IProperty>().Object);
            columnModificationMock.Setup(m => m.ParameterName).Returns("p");

            var parameter = new RelationalTypeMapping("s", DbType.String).CreateParameter(commandMock.Object, columnModificationMock.Object, useOriginalValue: false);

            Assert.Same(parameterMock.Object, parameter);
            columnModificationMock.Verify(m => m.Value, Times.Once);
            columnModificationMock.Verify(m => m.OriginalValue, Times.Never);
        }

        [Fact]
        public void CreateParameter_creates_parameter_with_original_values()
        {
            var parameterMock = new Mock<DbParameter>();
            var commandMock = new Mock<DbCommand>();
            commandMock
                .Protected()
                .Setup<DbParameter>("CreateDbParameter")
                .Returns(parameterMock.Object);

            var columnModificationMock = new Mock<ColumnModification>(
                CreateInternalEntryMock().Object,
                Mock.Of<IProperty>(),
                Mock.Of<IRelationalPropertyExtensions>(),
                new ParameterNameGenerator(),
                false, false, false, false);

            columnModificationMock.Setup(m => m.Property).Returns(new Mock<IProperty>().Object);
            columnModificationMock.Setup(m => m.OriginalParameterName).Returns("op");

            var parameter = new RelationalTypeMapping("s", DbType.String).CreateParameter(commandMock.Object, columnModificationMock.Object, useOriginalValue: true);

            Assert.Same(parameterMock.Object, parameter);
            columnModificationMock.Verify(m => m.Value, Times.Never);
            columnModificationMock.Verify(m => m.OriginalValue, Times.Once);
        }

        private static Mock<InternalEntityEntry> CreateInternalEntryMock()
        {
            var entityTypeMock = new Mock<IEntityType>();
            entityTypeMock.Setup(e => e.GetProperties()).Returns(new IProperty[0]);

            var internalEntryMock = new Mock<InternalEntityEntry>(
                Mock.Of<IStateManager>(), entityTypeMock.Object, Mock.Of<IEntityEntryMetadataServices>());
            return internalEntryMock;
        }
    }
}
