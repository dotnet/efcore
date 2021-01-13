// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    public class AnnotationTest
    {
        [ConditionalFact]
        public void Members_check_arguments()
        {
            Assert.Equal(
                AbstractionsStrings.ArgumentIsEmpty("name"),
                Assert.Throws<ArgumentException>(() => new Annotation("", "Kake")).Message);
        }

        [ConditionalFact]
        public void Can_create_annotation()
        {
            var annotation = new Annotation("Foo", "Bar");

            Assert.Equal("Foo", annotation.Name);
            Assert.Equal("Bar", annotation.Value);
        }

        [ConditionalFact]
        public void NegativeNumberArguments_PrecisionAttribute_Throws()
        {
            Assert.Equal(
                AbstractionsStrings.ArgumentIsNegativeNumber("precision"),
                Assert.Throws<ArgumentException>(() => new PrecisionAttribute(-1)).Message);
            Assert.Equal(
                AbstractionsStrings.ArgumentIsNegativeNumber("scale"),
                Assert.Throws<ArgumentException>(() => new PrecisionAttribute(3, -2)).Message);
            Assert.Equal(
                AbstractionsStrings.ArgumentIsNegativeNumber("precision"),
                Assert.Throws<ArgumentException>(() => new PrecisionAttribute(-5, 4)).Message);
        }
    }
}
