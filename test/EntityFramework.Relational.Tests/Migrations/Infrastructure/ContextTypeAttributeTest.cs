// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests.Migrations.Infrastructure
{
    public class ContextTypeAttributeTest
    {
        [Fact]
        public void Create_attribute()
        {
            var attribute = new ContextTypeAttribute(typeof(MyContext));

            Assert.Same(typeof(MyContext), attribute.ContextType);
        }

        public class MyContext : DbContext
        {
        }
    }
}
