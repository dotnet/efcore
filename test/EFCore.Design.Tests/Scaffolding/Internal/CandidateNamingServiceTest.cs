// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal;

public class CandidateNamingServiceTest
{
    [ConditionalTheory]
    [InlineData("PascalCase", "PascalCase")]
    [InlineData("camelCase", "CamelCase")]
    [InlineData("snake-case", "SnakeCase")]
    [InlineData("MixedCASE", "MixedCase")]
    [InlineData("separated_by_underscores", "SeparatedByUnderscores")]
    [InlineData("PascalCase_withUnderscore", "PascalCaseWithUnderscore")]
    [InlineData("ALL_CAPS", "AllCaps")]
    [InlineData(
        "numbers0Dont1Affect23Upper45Case678To9LowerCase10Boundary999", "Numbers0Dont1Affect23Upper45Case678To9LowerCase10Boundary999")]
    [InlineData("We1!*~&%rdCh@r^act()0rs", "We1RdChRAct0rs")]
    public void Generates_candidate_identifiers(string input, string output)
        => Assert.Equal(
            output, new CandidateNamingService().GenerateCandidateIdentifier(
                new DatabaseTable { Database = new DatabaseModel(), Name = input }));

    [ConditionalTheory]
    [InlineData("‍🐶", "")]
    [InlineData(" ", "")]
    public void Generates_column_candidate_identifiers(string input, string output)
        => Assert.Equal(
            output, new CandidateNamingService().GenerateCandidateIdentifier(
                new DatabaseColumn { Name = input }));
}
