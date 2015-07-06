// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.InMemory.FunctionalTests
{
    public class DataAnnotationInMemoryTest : DataAnnotationTestBase<InMemoryTestStore, DataAnnotationInMemoryFixture>
    {
        public DataAnnotationInMemoryTest(DataAnnotationInMemoryFixture fixture)
            : base(fixture)
        {
        }

        public override void ConcurrencyCheckAttribute_throws_if_value_in_database_changed()
        {
            using (var context = CreateContext())
            {
                Assert.True(context.Model.GetEntityType(typeof(One)).GetProperty("RowVersion").IsConcurrencyToken);
            }
        }

        public override void MaxLengthAttribute_throws_while_inserting_value_longer_than_max_length()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(10, context.Model.GetEntityType(typeof(One)).GetProperty("MaxLengthProperty").GetMaxLength());
            }
        }

        public override void RequiredAttribute_throws_while_inserting_null_value()
        {
            using (var context = CreateContext())
            {
                Assert.False(context.Model.GetEntityType(typeof(One)).GetProperty("RequiredColumn").IsNullable);
            }
        }

        public override void StringLengthAttribute_throws_while_inserting_value_longer_than_max_length()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(16, context.Model.GetEntityType(typeof(Two)).GetProperty("Data").GetMaxLength());
            }
        }

        public override void TimestampAttribute_throws_if_value_in_database_changed()
        {
            using (var context = CreateContext())
            {
                Assert.True(context.Model.GetEntityType(typeof(Two)).GetProperty("Timestamp").IsConcurrencyToken);
            }
        }
    }
}
