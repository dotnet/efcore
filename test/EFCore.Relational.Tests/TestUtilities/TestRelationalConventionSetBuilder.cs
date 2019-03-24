// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Storage;

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
        {
            var typeMappingSource = new TestRelationalTypeMappingSource(
                TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>());

            return new TestRelationalConventionSetBuilder(
                    new ProviderConventionSetBuilderDependencies(
                        typeMappingSource,
                        null,
                        null,
                        null,
                        new FakeDiagnosticsLogger<DbLoggerCategory.Model>(),
                        null,
                        null),
                    new RelationalConventionSetBuilderDependencies(
                        typeMappingSource))
                .CreateConventionSet();
        }
    }
}
