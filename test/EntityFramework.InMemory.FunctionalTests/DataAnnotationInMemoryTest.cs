// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests;
using Xunit;

namespace Microsoft.Data.Entity.InMemory.FunctionalTests
{
    public class DataAnnotationInMemoryTest : DataAnnotationTestBase<InMemoryTestStore, DataAnnotationInMemoryFixture>
    {
        public DataAnnotationInMemoryTest(DataAnnotationInMemoryFixture fixture)
            : base(fixture)
        {
        }

        public override void RequiredAttribute_throws_while_inserting_null_value()
        {
            using (var context = CreateContext())
            {
                Assert.False(context.Model.EntityTypes[0].GetProperty("RequiredColumn").IsNullable);
            }
        }

        public override void ConcurrencyCheckAttribute_throws_if_value_in_database_changed()
        {
            using (var context = CreateContext())
            {
                Assert.True(context.Model.EntityTypes[0].GetProperty("RowVersion").IsConcurrencyToken);
            }
        }
    }
}
