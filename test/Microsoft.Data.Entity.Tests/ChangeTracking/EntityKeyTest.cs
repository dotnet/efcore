// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class EntityKeyTest
    {
        [Fact]
        public void Value_property_calls_template_method()
        {
            Assert.Equal("Kake", new ConcreteKey().Value);
        }

        public class ConcreteKey : EntityKey
        {
            protected override object GetValue()
            {
                return "Kake";
            }
        }
    }
}
