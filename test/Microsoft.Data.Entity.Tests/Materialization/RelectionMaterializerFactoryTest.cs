// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Materialization;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Materialization
{
    public class RelectionMaterializerFactoryTest
    {
        [Fact]
        public void Can_create_materializer()
        {
            var reflectionMaterializerFactory = new ReflectionMaterializerFactory();

            Assert.IsType<ReflectionMaterializer>(reflectionMaterializerFactory.CreateMaterializer(new EntityType()));
        }
    }
}
