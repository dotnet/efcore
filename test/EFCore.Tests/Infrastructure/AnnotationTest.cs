// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Internal;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    public class AnnotationTest
    {
        [Fact]
        public void Members_check_arguments()
        {
            Assert.Equal(
                AbstractionsStrings.ArgumentIsEmpty("name"),
                Assert.Throws<ArgumentException>(() => new Annotation("", "Kake")).Message);
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
