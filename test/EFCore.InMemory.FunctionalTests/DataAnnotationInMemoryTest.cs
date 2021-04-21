// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class DataAnnotationInMemoryTest : DataAnnotationTestBase<DataAnnotationInMemoryTest.DataAnnotationInMemoryFixture>
    {
        public DataAnnotationInMemoryTest(DataAnnotationInMemoryFixture fixture)
            : base(fixture)
        {
        }

        public override void ConcurrencyCheckAttribute_throws_if_value_in_database_changed()
        {
            using var context = CreateContext();
            Assert.True(context.Model.FindEntityType(typeof(One)).FindProperty("RowVersion").IsConcurrencyToken);
        }

        public override void MaxLengthAttribute_throws_while_inserting_value_longer_than_max_length()
        {
            using var context = CreateContext();
            Assert.Equal(10, context.Model.FindEntityType(typeof(One)).FindProperty("MaxLengthProperty").GetMaxLength());
        }

        public override void RequiredAttribute_for_navigation_throws_while_inserting_null_value()
        {
            using var context = CreateContext();
            Assert.True(
                context.Model.FindEntityType(typeof(BookDetails)).FindNavigation(nameof(BookDetails.AnotherBook)).ForeignKey
                    .IsRequired);
        }

        public override void RequiredAttribute_for_property_throws_while_inserting_null_value()
        {
            using var context = CreateContext();
            Assert.False(context.Model.FindEntityType(typeof(One)).FindProperty("RequiredColumn").IsNullable);
        }

        public override void StringLengthAttribute_throws_while_inserting_value_longer_than_max_length()
        {
            using var context = CreateContext();
            Assert.Equal(16, context.Model.FindEntityType(typeof(Two)).FindProperty("Data").GetMaxLength());
        }

        public override void TimestampAttribute_throws_if_value_in_database_changed()
        {
            using var context = CreateContext();
            Assert.True(context.Model.FindEntityType(typeof(Two)).FindProperty("Timestamp").IsConcurrencyToken);
        }

        public class DataAnnotationInMemoryFixture : DataAnnotationFixtureBase
        {
            public static readonly string DatabaseName = "DataAnnotations";

            protected override ITestStoreFactory TestStoreFactory
                => InMemoryTestStoreFactory.Instance;

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => base.AddOptions(builder).ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));
        }
    }
}
