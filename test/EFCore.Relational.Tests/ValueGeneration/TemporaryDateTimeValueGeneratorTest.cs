// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;

namespace Microsoft.EntityFrameworkCore.ValueGeneration;

public class TemporaryDateTimeValueGeneratorTest
{
    [ConditionalFact]
    public void Can_create_values_for_DateTime_types()
    {
        var generator = new TemporaryDateTimeValueGenerator();

        Assert.Equal(new DateTime(1), generator.Next(null));
        Assert.Equal(new DateTime(2), generator.Next(null));
    }

    [ConditionalFact]
    public void Generates_temporary_values()
        => Assert.True(new TemporaryDateTimeValueGenerator().GeneratesTemporaryValues);
}
