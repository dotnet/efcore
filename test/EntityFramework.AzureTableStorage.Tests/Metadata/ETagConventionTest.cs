// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.AzureTableStorage.Metadata;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests.Metadata
{
    public class ETagConventionTest
    {
        private readonly ETagConvention _convention = new ETagConvention();

        [Fact]
        public void It_adds_etag()
        {
            var entityBuilder = CreateInternalEntityBuilder();

            _convention.Apply(entityBuilder);

            var etagProp = (IProperty)entityBuilder.Metadata.GetProperty("ETag");
            Assert.NotNull(etagProp);
            Assert.True(etagProp.IsShadowProperty);
            Assert.True(etagProp.IsConcurrencyToken);
        }

        [Fact]
        public void It_does_not_overwrite_etag_prop()
        {
            var entityBuilder = CreateInternalEntityBuilder();
            entityBuilder.Property(typeof(int), "ETag", ConfigurationSource.Convention);

            _convention.Apply(entityBuilder);

            var etagProp = (IProperty)entityBuilder.Metadata.GetProperty("ETag");
            Assert.NotNull(etagProp);
            Assert.True(etagProp.IsShadowProperty);
            Assert.True(etagProp.IsConcurrencyToken);
        }

        private InternalEntityBuilder CreateInternalEntityBuilder()
        {
            var modelBuilder = new InternalModelBuilder(new Model(), null);
            return modelBuilder.Entity("TestType", ConfigurationSource.Convention);
        }
    }
}
