// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Tests
{
    public class TestConventionalSetBuilder : RelationalConventionSetBuilder
    {
        public TestConventionalSetBuilder(IRelationalTypeMapper typeMapper,
            ICurrentDbContext currentContext,
            IDbSetFinder setFinder)
            : base(typeMapper, currentContext, setFinder)
        {
        }

        public static ConventionSet Build()
            => new TestConventionalSetBuilder(new TestRelationalTypeMapper(), null, null)
                .AddConventions(new CoreConventionSetBuilder().CreateConventionSet());
    }
}
