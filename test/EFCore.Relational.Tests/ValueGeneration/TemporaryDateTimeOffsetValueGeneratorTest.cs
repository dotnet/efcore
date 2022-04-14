// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;

namespace Microsoft.EntityFrameworkCore.ValueGeneration;

public class TemporaryDateTimeOffsetValueGeneratorTest
{
    [ConditionalFact]
    public void Can_create_values_for_DateTime_types()
    {
        var generator = new TemporaryDateTimeOffsetValueGenerator();
        Assert.Equal(new DateTimeOffset(1, TimeSpan.Zero), generator.Next(null));
        Assert.Equal(new DateTimeOffset(2, TimeSpan.Zero), generator.Next(null));
    }

    [ConditionalFact]
    public void Generates_temporary_values()
        => Assert.True(new TemporaryDateTimeOffsetValueGenerator().GeneratesTemporaryValues);
}
