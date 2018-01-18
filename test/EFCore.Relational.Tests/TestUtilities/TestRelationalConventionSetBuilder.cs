// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;

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
                    new FallbackRelationalCoreTypeMapper(
                        TestServiceFactory.Instance.Create<CoreTypeMapperDependencies>(),
                        TestServiceFactory.Instance.Create<RelationalTypeMapperDependencies>(),
                        TestServiceFactory.Instance.Create<TestRelationalTypeMapper>()),
                    new FakeDiagnosticsLogger<DbLoggerCategory.Model>(),
                    null,
                    null))
                .AddConventions(
                    TestServiceFactory.Instance.Create<CoreConventionSetBuilder>()
                        .CreateConventionSet());
    }
}
