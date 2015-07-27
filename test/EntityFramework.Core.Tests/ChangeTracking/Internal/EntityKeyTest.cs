// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking.Internal
{
    public class EntityKeyTest
    {
        [Fact]
        public void Value_property_calls_template_method()
        {
            Assert.Equal("Kake", new ConcreteKey(null).Value);
        }

        public class ConcreteKey : EntityKey
        {
            public ConcreteKey(IKey key)
                : base(key)
            {
            }

            protected override object GetValue() => "Kake";
        }
    }
}
