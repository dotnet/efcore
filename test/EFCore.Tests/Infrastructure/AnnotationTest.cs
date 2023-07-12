// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.Infrastructure;

public class AnnotationTest
{
    [ConditionalFact]
    public void Members_check_arguments()
        => Assert.Equal(
            AbstractionsStrings.ArgumentIsEmpty("name"),
            Assert.Throws<ArgumentException>(() => new Annotation("", "Kake")).Message);

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
