// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Core.Metadata
{
    using Xunit;

    public class AnnotationFacts
    {
        [Fact]
        public void Can_create_annotation()
        {
            var annotation = new Annotation("Foo", "Bar");

            Assert.Equal("Foo", annotation.Name);
            Assert.Equal("Bar", annotation.Value);
        }
    }
}
