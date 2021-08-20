// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class SharedTypeQueryRelationalTestBase : SharedTypeQueryTestBase
    {
        protected TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected void ClearLog() => TestSqlLoggerFactory.Clear();

        protected void AssertSql(params string[] expected) => TestSqlLoggerFactory.AssertBaseline(expected);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Can_use_shared_type_entity_type_in_query_filter_with_from_sql(bool async)
        {
            var contextFactory = await InitializeAsync<MyContextRelational24601>(
                seed: c => c.Seed());

            using var context = contextFactory.CreateContext();
            var query = context.Set<ViewQuery24601>();
            var result = async
                ? await query.ToListAsync()
                : query.ToList();

            Assert.Empty(result);
        }

        protected class MyContextRelational24601 : MyContext24601
        {
            public MyContextRelational24601(DbContextOptions options)
                : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);
                modelBuilder.Entity<ViewQuery24601>()
                    .HasQueryFilter(e => Set<Dictionary<string, object>>("STET")
                        .FromSqlRaw("Select * from STET").Select(i => (string)i["Value"]).Contains(e.Value));
            }
        }
    }
}
