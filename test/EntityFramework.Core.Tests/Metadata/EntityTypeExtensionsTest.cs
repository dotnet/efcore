// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Metadata
{
    public class EntityTypeExtensionsTest
    {
        [Fact]
        public void Can_get_all_properties_and_navigations()
        {
            var typeMock = new Mock<IEntityType>();

            var property1 = Mock.Of<IProperty>();
            var property2 = Mock.Of<IProperty>();
            var navigation1 = Mock.Of<INavigation>();
            var navigation2 = Mock.Of<INavigation>();

            typeMock.Setup(m => m.GetProperties()).Returns(new List<IProperty> { property1, property2 });
            typeMock.Setup(m => m.GetNavigations()).Returns(new List<INavigation> { navigation1, navigation2 });

            Assert.Equal(
                new IPropertyBase[] { property1, property2, navigation1, navigation2 },
                typeMock.Object.GetPropertiesAndNavigations().ToArray());
        }

        [Fact]
        public void Can_get_referencing_foreign_keys()
        {
            var modelMock = new Mock<Model>();
            var entityType = new EntityType("Customer", modelMock.Object);

            entityType.GetReferencingForeignKeys();

            modelMock.Verify(m => m.GetReferencingForeignKeys(entityType), Times.Once());
        }

        [Fact]
        public void Can_get_proper_table_name_for_generic_entityType()
        {
            var entityType = new EntityType(typeof(A<int>), new Model());

            Assert.Equal(
                "A<int>",
                ((IEntityType)entityType).DisplayName());

        }

        public class A<T>
        {

        }
    }
}
