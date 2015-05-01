// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Reflection;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Builders;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class ApiConsistencyTest : ApiConsistencyTestBase
    {
        [Fact]
        public void Fluent_api_methods_should_not_return_void()
        {
            var fluentApiTypes = new[] { typeof(BasicModelBuilder) };

            var voidMethods
                = from type in GetAllTypes(fluentApiTypes)
                    where type.IsVisible
                    from method in type.GetMethods(PublicInstance)
                    where method.DeclaringType == type
                          && method.ReturnType == typeof(void)
                    select type.FullName + "." + method.Name;

            Assert.Equal("", string.Join("\r\n", voidMethods));
        }

        protected override Assembly TargetAssembly
        {
            get { return typeof(EntityType).Assembly; }
        }
    }
}
