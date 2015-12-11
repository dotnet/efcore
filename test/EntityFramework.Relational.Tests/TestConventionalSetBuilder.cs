// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata.Conventions;
using Microsoft.Data.Entity.Metadata.Conventions.Internal;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.Tests
{
    public class TestConventionalSetBuilder : RelationalConventionSetBuilder
    {
        public TestConventionalSetBuilder(IRelationalTypeMapper typeMapper)
            : base(typeMapper)
        {
        }

        public static ConventionSet Build()
            => new TestConventionalSetBuilder(new TestRelationalTypeMapper())
                .AddConventions(new CoreConventionSetBuilder().CreateConventionSet());
    }
}
