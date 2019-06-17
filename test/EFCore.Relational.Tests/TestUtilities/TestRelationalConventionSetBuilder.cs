// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class TestRelationalConventionSetBuilder : RelationalConventionSetBuilder
    {
        public TestRelationalConventionSetBuilder(
            ProviderConventionSetBuilderDependencies dependencies,
            RelationalConventionSetBuilderDependencies relationalDependencies)
            : base(dependencies, relationalDependencies)
        {
        }

        public static ConventionSet Build()
            => ConventionSet.CreateConventionSet(RelationalTestHelpers.Instance.CreateContext());
    }
}
