// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class TestRelationalConventionSetBuilder : RelationalConventionSetBuilder
    {
        public TestRelationalConventionSetBuilder(RelationalConventionSetBuilderDependencies dependencies)
            : base(dependencies)
        {
        }

        public static ConventionSet Build()
            => new TestRelationalConventionSetBuilder(
                new RelationalConventionSetBuilderDependencies(
                    TestServiceFactory.Instance.Create<TestRelationalTypeMapper>(),
                    new FakeDiagnosticsLogger<DbLoggerCategory.Model>(),
                    null,
                    null))
                .AddConventions(
                    TestServiceFactory.Instance.Create<CoreConventionSetBuilder>()
                        .CreateConventionSet());
    }
}
