// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.ValueGeneration;

public class GuidValueGeneratorTest
{
    [ConditionalFact]
    public void Creates_GUIDs()
    {
        var sequentialGuidIdentityGenerator = new GuidValueGenerator();

        var values = new HashSet<Guid>();
        for (var i = 0; i < 100; i++)
        {
            var generatedValue = sequentialGuidIdentityGenerator.Next(null);

            values.Add(generatedValue);
        }

        Assert.Equal(100, values.Count);
    }

    [ConditionalFact]
    public void Does_not_generate_temp_values()
        => Assert.False(new GuidValueGenerator().GeneratesTemporaryValues);
}
