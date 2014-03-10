// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking
{
    public class EntityKeyTest
    {
        [Fact]
        public void Value_property_calls_template_method()
        {
            Assert.Equal("Kake", new ConcreteKey(new Mock<IEntityType>().Object).Value);
        }

        [Fact]
        public void Can_get_entity_type()
        {
            var entityType = new Mock<IEntityType>().Object;
            Assert.Equal(entityType, new ConcreteKey(entityType).EntityType);
        }

        public class ConcreteKey : EntityKey
        {
            public ConcreteKey(IEntityType entityType)
                : base(entityType)
            {
            }

            protected override object GetValue()
            {
                return "Kake";
            }
        }
    }
}
