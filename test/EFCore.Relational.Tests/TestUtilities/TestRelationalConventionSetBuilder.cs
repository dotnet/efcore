// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Tests.TestUtilities
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
                        new TestRelationalTypeMapper(new RelationalTypeMapperDependencies()), new TestAnnotationProvider(), null, null))
                .AddConventions(new CoreConventionSetBuilder().CreateConventionSet());
    }
}
