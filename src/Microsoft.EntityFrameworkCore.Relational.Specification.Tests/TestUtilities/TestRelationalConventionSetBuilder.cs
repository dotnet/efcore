// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities
{
    public class TestRelationalConventionSetBuilder : RelationalConventionSetBuilder
    {
        public TestRelationalConventionSetBuilder(
            IRelationalTypeMapper typeMapper,
            IRelationalAnnotationProvider annotationProvider,
            ICurrentDbContext currentContext,
            IDbSetFinder setFinder)
            : base(typeMapper, annotationProvider, currentContext, setFinder)
        {
        }

        public static ConventionSet Build()
            => new TestRelationalConventionSetBuilder(new TestRelationalTypeMapper(), new TestAnnotationProvider(), null, null)
                .AddConventions(new CoreConventionSetBuilder().CreateConventionSet());
    }
}
