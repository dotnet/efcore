// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Microsoft.EntityFrameworkCore.Utilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class CompiledQueryCacheKeyGeneratorTest
    {
        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        public void It_creates_unique_query_cache_key()
        {
            using (var testStore = SqlServerTestStore.Create(nameof(CompiledQueryCacheKeyGeneratorTest)))
            {
                object key1, key2;
                Expression query;
                using (var context1 = new QueryKeyCacheContext(rowNumberPaging: true, connection: testStore.Connection))
                {
                    var generator = context1.GetService<ICompiledQueryCacheKeyGenerator>();
                    query = context1.Set<Poco1>().Skip(4).Take(10).Expression;
                    key1 = generator.GenerateCacheKey(query, false);
                }

                using (var context2 = new QueryKeyCacheContext(rowNumberPaging: false, connection: testStore.Connection))
                {
                    var generator = context2.GetService<ICompiledQueryCacheKeyGenerator>();
                    key2 = generator.GenerateCacheKey(query, false);
                }

                Assert.NotEqual(key1, key2);
            }
        }

        public class QueryKeyCacheContext : DbContext
        {
            private readonly bool _rowNumberPaging;
            private readonly DbConnection _connection;

            public QueryKeyCacheContext(bool rowNumberPaging, DbConnection connection)
            {
                _rowNumberPaging = rowNumberPaging;
                _connection = connection;
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.Entity<Poco1>();

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseSqlServer(
                    _connection, b =>
                        {
                            b.ApplyConfiguration();
                            if (_rowNumberPaging)
                            {
                                b.UseRowNumberForPaging();
                            }
                        });
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class Poco1
        {
            public int Id { get; set; }
        }
    }
}
