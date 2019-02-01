// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public class CompiledQueryCacheKeyGeneratorTest
    {
        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public void It_creates_unique_query_cache_key()
        {
            using (var testStore = SqlServerTestStore.GetOrCreateInitialized(nameof(CompiledQueryCacheKeyGeneratorTest)))
            {
                object key1, key2;
                Expression query;
                using (var context1 = new QueryKeyCacheContext(CreateOptions(testStore, rowNumberPaging: true)))
                {
                    var generator = context1.GetService<ICompiledQueryCacheKeyGenerator>();
                    query = context1.Set<Poco1>().Skip(4).Take(10).Expression;
                    key1 = generator.GenerateCacheKey(query, false);
                }

                using (var context2 = new QueryKeyCacheContext(CreateOptions(testStore, rowNumberPaging: false)))
                {
                    var generator = context2.GetService<ICompiledQueryCacheKeyGenerator>();
                    key2 = generator.GenerateCacheKey(query, false);
                }

                Assert.NotEqual(key1, key2);
            }
        }

        protected virtual DbContextOptions CreateOptions(SqlServerTestStore testStore, bool rowNumberPaging)
        {
            var builder = testStore.AddProviderOptions(new DbContextOptionsBuilder())
                .EnableSensitiveDataLogging()
                .EnableServiceProviderCaching(false)
                .ConfigureWarnings(
                    b => b.Default(WarningBehavior.Throw)
                        .Log(CoreEventId.SensitiveDataLoggingEnabledWarning)
                        .Log(CoreEventId.ManyServiceProvidersCreatedWarning));

            if (rowNumberPaging)
            {
                new SqlServerDbContextOptionsBuilder(builder).UseRowNumberForPaging();
            }

            return builder.Options;
        }

        public class QueryKeyCacheContext : DbContext
        {
            public QueryKeyCacheContext(DbContextOptions options)
                : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.Entity<Poco1>();
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class Poco1
        {
            // ReSharper disable once UnusedMember.Local
            public int Id { get; set; }
        }
    }
}
