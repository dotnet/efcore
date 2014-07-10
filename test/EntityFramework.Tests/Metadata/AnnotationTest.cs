// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Xunit;

namespace Microsoft.Data.Entity.Metadata
{
    public class AnnotationTest
    {
        [Fact]
        public void Members_check_arguments()
        {
            Assert.Equal(
                Strings.FormatArgumentIsEmpty("name"),
                Assert.Throws<ArgumentException>(() => new Annotation("", "Kake")).Message);

            Assert.Equal(
                Strings.FormatArgumentIsEmpty("value"),
                Assert.Throws<ArgumentException>(() => new Annotation("Lie", "")).Message);
        }

        [Fact]
        public void Can_create_annotation()
        {
            var annotation = new Annotation("Foo", "Bar");

            Assert.Equal("Foo", annotation.Name);
            Assert.Equal("Bar", annotation.Value);
        }
    }
}
