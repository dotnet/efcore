// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.ValueGeneration;

public class SequentialGuidValueGeneratorTest
{
    [ConditionalFact]
    public void Can_get_next_values()
    {
        var sequentialGuidIdentityGenerator = new SequentialGuidValueGenerator();

        var values = new HashSet<Guid>();
        for (var i = 0; i < 100; i++)
        {
            var generatedValue = sequentialGuidIdentityGenerator.Next(null);

            values.Add(generatedValue);
        }

        // Check all generated values are different--functional test checks ordering on SQL Server
        Assert.Equal(100, values.Count);
    }

    [ConditionalFact]
    public void Does_not_generate_temp_values()
        => Assert.False(new SequentialGuidValueGenerator().GeneratesTemporaryValues);
}
