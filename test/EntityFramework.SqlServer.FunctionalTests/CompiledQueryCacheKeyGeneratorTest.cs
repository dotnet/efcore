// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class CompiledQueryCacheKeyGeneratorTest
    {
        [Fact]
        public void It_creates_unique_query_cache_key()
        {
            object key1, key2;
            Expression query;
            using (var context1 = new QueryKeyCacheContext(rowNumberPaging: true))
            {
                var services = ((IAccessor<IServiceProvider>)context1).Service.GetService<IDbContextServices>().DatabaseProviderServices;
                query = context1.Set<Poco1>().Skip(4).Take(10).Expression;
                var generator = services.CompiledQueryCacheKeyGenerator;
                key1 = generator.GenerateCacheKey(query, false);
            }

            using (var context2 = new QueryKeyCacheContext(rowNumberPaging: false))
            {
                var services = ((IAccessor<IServiceProvider>)context2).Service.GetService<IDbContextServices>().DatabaseProviderServices;
                var generator = services.CompiledQueryCacheKeyGenerator;
                key2 = generator.GenerateCacheKey(query, false);
            }

            Assert.NotEqual(key1, key2);
        }

        public class QueryKeyCacheContext : DbContext
        {
            private readonly bool _rowNumberPaging;

            public QueryKeyCacheContext(bool rowNumberPaging)
            {
                _rowNumberPaging = rowNumberPaging;
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.Entity<Poco1>();

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                var optionBuilder = optionsBuilder.UseSqlServer(SqlServerTestStore.CreateScratch().Connection);
                if (_rowNumberPaging)
                {
                    optionBuilder.UseRowNumberForPaging();
                }
            }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class Poco1
        {
            public int Id { get; set; }
        }
    }
}
