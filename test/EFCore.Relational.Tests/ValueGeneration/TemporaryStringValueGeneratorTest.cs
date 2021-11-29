// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;

namespace Microsoft.EntityFrameworkCore.ValueGeneration;

public class TemporaryStringValueGeneratorTest
{
    [ConditionalFact]
    public void Creates_GUID_strings()
    {
        var generator = new TemporaryStringValueGenerator();

        var values = new HashSet<Guid>();
        for (var i = 0; i < 100; i++)
        {
            var generatedValue = generator.Next(null);

            values.Add(Guid.Parse(generatedValue));
        }

        Assert.Equal(100, values.Count);
    }

    [ConditionalFact]
    public void Generates_temp_values()
        => Assert.True(new TemporaryStringValueGenerator().GeneratesTemporaryValues);
}
