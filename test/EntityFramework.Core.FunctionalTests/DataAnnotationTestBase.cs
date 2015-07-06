// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Xunit;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class DataAnnotationTestBase<TTestStore, TFixture> : IClassFixture<TFixture>, IDisposable
        where TTestStore : TestStore
        where TFixture : DataAnnotationFixtureBase<TTestStore>, new()
    {
        protected DataAnnotationContext CreateContext()
        {
            return Fixture.CreateContext(TestStore);
        }

        protected DataAnnotationTestBase(TFixture fixture)
        {
            Fixture = fixture;

            TestStore = Fixture.CreateTestStore();
        }

        protected TFixture Fixture { get; }

        protected TTestStore TestStore { get; }

        public void Dispose()
        {
            TestStore.Dispose();
        }

        [Fact]
        public virtual void ConcurrencyCheckAttribute_throws_if_value_in_database_changed()
        {
            using (var context = CreateContext())
            {
                var clientRow = context.Ones.First(r => r.UniqueNo == 1);
                clientRow.RowVersion = new Guid("00000000-0000-0000-0002-000000000001");
                clientRow.RequiredColumn = "ChangedData";

                using (var innerContext = CreateContext())
                {
                    var storeRow = innerContext.Ones.First(r => r.UniqueNo == 1);
                    storeRow.RowVersion = new Guid("00000000-0000-0000-0003-000000000001");
                    storeRow.RequiredColumn = "ModifiedData";

                    innerContext.SaveChanges();
                }

                Assert.Throws<DbUpdateConcurrencyException>(() => context.SaveChanges());
            }
        }

        [Fact]
        public virtual void DatabaseGeneratedAttribute_autogenerates_values_when_set_to_identity()
        {
            using (var context = CreateContext())
            {
                context.Ones.Add(new One { RequiredColumn = "Third", RowVersion = new Guid("00000000-0000-0000-0000-000000000003") });

                context.SaveChanges();
            }
        }

        [Fact]
        public virtual void MaxLengthAttribute_throws_while_inserting_value_longer_than_max_length()
        {
            using (var context = CreateContext())
            {
                context.Ones.Add(new One { RequiredColumn = "ValidString", RowVersion = new Guid("00000000-0000-0000-0000-000000000001"), MaxLengthProperty = "Short" });

                context.SaveChanges();
            }

            using (var context = CreateContext())
            {
                context.Ones.Add(new One { RequiredColumn = "ValidString", RowVersion = new Guid("00000000-0000-0000-0000-000000000002"), MaxLengthProperty = "VeryVeryVeryVeryVeryVeryLongString" });

                Assert.Equal("An error occurred while updating the entries. See the inner exception for details.",
                    Assert.Throws<DbUpdateException>(() => context.SaveChanges()).Message);
            }
        }

        [Fact]
        public virtual void NotMappedAttribute_ignored_entityType()
        {
            using (var context = CreateContext())
            {
                Assert.False(context.Model.EntityTypes.Any(e => e.Name == typeof(C).FullName));
            }
        }

        [Fact]
        public virtual void RequiredAttribute_throws_while_inserting_null_value()
        {
            using (var context = CreateContext())
            {
                context.Ones.Add(new One { RequiredColumn = "ValidString", RowVersion = new Guid("00000000-0000-0000-0000-000000000001") });

                context.SaveChanges();
            }

            using (var context = CreateContext())
            {
                context.Ones.Add(new One { RequiredColumn = null, RowVersion = new Guid("00000000-0000-0000-0000-000000000002") });

                Assert.Equal("An error occurred while updating the entries. See the inner exception for details.",
                    Assert.Throws<DbUpdateException>(() => context.SaveChanges()).Message);
            }
        }

        [Fact]
        public virtual void StringLengthAttribute_throws_while_inserting_value_longer_than_max_length()
        {
            using (var context = CreateContext())
            {
                context.Twos.Add(new Two { Data = "ValidString" });

                context.SaveChanges();
            }

            using (var context = CreateContext())
            {
                context.Twos.Add(new Two { Data = "ValidButLongString" });

                Assert.Equal("An error occurred while updating the entries. See the inner exception for details.",
                    Assert.Throws<DbUpdateException>(() => context.SaveChanges()).Message);
            }
        }

        [Fact]
        public virtual void TimestampAttribute_throws_if_value_in_database_changed()
        {
            using (var context = CreateContext())
            {
                var clientRow = context.Twos.First(r => r.Id == 1);
                clientRow.Data = "ChangedData";

                using (var innerContext = CreateContext())
                {
                    var storeRow = innerContext.Twos.First(r => r.Id == 1);
                    storeRow.Data = "ModifiedData";

                    innerContext.SaveChanges();
                }

                Assert.Throws<DbUpdateConcurrencyException>(() => context.SaveChanges());
            }
        }
    }
}