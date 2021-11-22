// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.ValueGeneration;

public class BinaryValueGeneratorTest
{
    [ConditionalFact]
    public void Creates_GUID_arrays()
    {
        var generator = new BinaryValueGenerator();

        var values = new HashSet<Guid>();
        for (var i = 0; i < 100; i++)
        {
            var generatedValue = generator.Next(null);

            values.Add(new Guid(generatedValue));
        }

        Assert.Equal(100, values.Count);
    }

    [ConditionalFact]
    public void Generates_non_temp_values()
        => Assert.False(new BinaryValueGenerator().GeneratesTemporaryValues);
}
