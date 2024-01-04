// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class TestRelationalConventionSetBuilder(
    ProviderConventionSetBuilderDependencies dependencies,
    RelationalConventionSetBuilderDependencies relationalDependencies) : RelationalConventionSetBuilder(dependencies, relationalDependencies)
{
    public static ConventionSet Build()
        => ConventionSet.CreateConventionSet(FakeRelationalTestHelpers.Instance.CreateContext());
}
