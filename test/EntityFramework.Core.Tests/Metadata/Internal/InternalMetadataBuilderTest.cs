// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Xunit;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class InternalMetadataBuilderTest
    {
        private InternalMetadataBuilder<Model> CreateInternalMetadataBuilder()
        {
            return new InternalModelBuilder(new Model(), null);
        }

        [Fact]
        public void Can_only_override_lower_source_annotation()
        {
            var builder = CreateInternalMetadataBuilder();
            var metadata = builder.Metadata;

            Assert.True(builder.Annotation("Foo", "1", ConfigurationSource.Convention));
            Assert.True(builder.Annotation("Foo", "2", ConfigurationSource.DataAnnotation));

            Assert.Equal("2", metadata.Annotations.Single().Value);

            Assert.False(builder.Annotation("Foo", "1", ConfigurationSource.Convention));
            Assert.Equal("2", metadata.Annotations.Single().Value);
        }

        [Fact]
        public void Can_only_override_existing_annotation_explicitly()
        {
            var builder = CreateInternalMetadataBuilder();
            var metadata = builder.Metadata;
            metadata["Foo"] = "1";

            Assert.True(builder.Annotation("Foo", "1", ConfigurationSource.DataAnnotation));
            Assert.False(builder.Annotation("Foo", "2", ConfigurationSource.DataAnnotation));

            Assert.Equal("1", metadata.Annotations.Single().Value);

            Assert.True(builder.Annotation("Foo", "2", ConfigurationSource.Explicit));
            Assert.Equal("2", metadata.Annotations.Single().Value);
        }
    }
}
