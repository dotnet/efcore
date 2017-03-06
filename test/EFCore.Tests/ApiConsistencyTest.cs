// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Tests
{
    public class ApiConsistencyTest : ApiConsistencyTestBase
    {
        public class SampleEntity
        {
        }

        [Fact]
        public void Fluent_api_methods_should_not_return_void()
        {
            var fluentApiTypes = new[]
            {
                typeof(ModelBuilder),
                typeof(CollectionNavigationBuilder),
                typeof(CollectionNavigationBuilder<SampleEntity, SampleEntity>),
                typeof(EntityTypeBuilder),
                typeof(EntityTypeBuilder<>),
                typeof(IndexBuilder),
                typeof(KeyBuilder),
                typeof(PropertyBuilder),
                typeof(PropertyBuilder<>),
                typeof(ReferenceCollectionBuilder),
                typeof(ReferenceCollectionBuilder<SampleEntity, SampleEntity>),
                typeof(ReferenceNavigationBuilder),
                typeof(ReferenceNavigationBuilder<SampleEntity, SampleEntity>),
                typeof(ReferenceReferenceBuilder),
                typeof(ReferenceReferenceBuilder<SampleEntity, SampleEntity>)
            };

            var voidMethods
                = from type in GetAllTypes(fluentApiTypes)
                  where type.GetTypeInfo().IsVisible
                  from method in type.GetMethods(PublicInstance)
                  where (method.DeclaringType == type)
                        && (method.ReturnType == typeof(void))
                  select type.Name + "." + method.Name;

            Assert.Equal("", string.Join(Environment.NewLine, voidMethods));
        }

        protected override Assembly TargetAssembly => typeof(EntityType).GetTypeInfo().Assembly;
    }
}
