// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.AzureTableStorage.Metadata;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests.Metadata
{
    public class ETagConventionTest
    {
        public ETagConvention _convention = new ETagConvention();

        [Fact]
        public void It_adds_etag()
        {
            var entityType = new EntityType("TestType");
            _convention.Apply(entityType);
            var etagProp = entityType.GetProperty("ETag");
            Assert.NotNull(etagProp);
            Assert.True(etagProp.IsShadowProperty);
            Assert.True(etagProp.IsConcurrencyToken);
        }

        [Fact]
        public void It_does_not_overwrite_etag_prop()
        {
            var entityType = new EntityType("TestType");
            entityType.AddProperty("ETag", typeof(int), false, false);

            _convention.Apply(entityType);
            var etagProp = entityType.GetProperty("ETag");
            Assert.NotNull(etagProp);
            Assert.False(etagProp.IsShadowProperty);
            Assert.False(etagProp.IsConcurrencyToken);
        }
    }
}
