// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal;

public class CandidateNamingServiceTest
{
    [Theory, InlineData("PascalCase", "PascalCase"), InlineData("camelCase", "CamelCase"), InlineData("snake-case", "SnakeCase"),
     InlineData("MixedCASE", "MixedCase"), InlineData("separated_by_underscores", "SeparatedByUnderscores"),
     InlineData("PascalCase_withUnderscore", "PascalCaseWithUnderscore"), InlineData("ALL_CAPS", "AllCaps"), InlineData(
         "numbers0Dont1Affect23Upper45Case678To9LowerCase10Boundary999", "Numbers0Dont1Affect23Upper45Case678To9LowerCase10Boundary999"),
     InlineData("We1!*~&%rdCh@r^act()0rs", "We1RdChRAct0rs")]
    public void Generates_candidate_identifiers(string input, string output)
        => Assert.Equal(
            output, new CandidateNamingService().GenerateCandidateIdentifier(
                new DatabaseTable { Database = new DatabaseModel(), Name = input }));

    [Theory, InlineData("‍🐶", ""), InlineData(" ", "")]
    public void Generates_column_candidate_identifiers(string input, string output)
        => Assert.Equal(
            output, new CandidateNamingService().GenerateCandidateIdentifier(
                new DatabaseColumn { Name = input }));

    [Fact]
    public void Dependent_end_navigation_name_does_not_become_empty_when_stripping_Id_leaves_no_base_name()
    {
        // Regression test: when every character before the "Id" suffix is non-alphanumeric (e.g. "_Id"),
        // stripping the suffix must not yield an empty navigation name (which would silently fall back to the
        // principal type name); the original property name is used instead.
        var modelBuilder = FakeRelationalTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Entity<NamingPrincipal>();
        modelBuilder.Entity<NamingDependent>(
            b =>
            {
                b.Property<int>("_Id");
                b.HasOne<NamingPrincipal>().WithMany().HasForeignKey("_Id");
            });

        var foreignKey = modelBuilder.Model.FindEntityType(typeof(NamingDependent))!.GetForeignKeys().Single();

        Assert.Equal("_Id", new CandidateNamingService().GetDependentEndCandidateNavigationPropertyName(foreignKey));
    }

    private class NamingPrincipal
    {
        public int Id { get; set; }
    }

    private class NamingDependent
    {
        public int Id { get; set; }
    }
}
