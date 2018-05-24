// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class CandidateNamingServiceTest
    {
        [Theory]
        [InlineData("PascalCase", "PascalCase")]
        [InlineData("camelCase", "CamelCase")]
        [InlineData("snake-case", "SnakeCase")]
        [InlineData("MixedCASE", "MixedCase")]
        [InlineData("separated_by_underscores", "SeparatedByUnderscores")]
        [InlineData("PascalCase_withUnderscore", "PascalCaseWithUnderscore")]
        [InlineData("ALL_CAPS", "AllCaps")]
        [InlineData("numbers0Dont1Affect23Upper45Case678To9LowerCase10Boundary999", "Numbers0Dont1Affect23Upper45Case678To9LowerCase10Boundary999")]
        [InlineData("We1!*~&%rdCh@r^act()0rs", "We1RdChRAct0rs")]
        public void Generates_candidate_identifiers(string input, string output)
        {
            Assert.Equal(
                output, new CandidateNamingService().GenerateCandidateIdentifier(
                    new DatabaseTable
                    {
                        Name = input
                    }));
        }
    }
}
