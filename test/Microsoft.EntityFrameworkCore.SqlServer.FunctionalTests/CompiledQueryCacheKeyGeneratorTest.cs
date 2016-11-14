// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
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
                    var services = ((IInfrastructure<IServiceProvider>)context1).Instance.GetService<IDbContextServices>().DatabaseProviderServices;
                    query = context1.Set<Poco1>().Skip(4).Take(10).Expression;
                    var generator = services.CompiledQueryCacheKeyGenerator;
                    key1 = generator.GenerateCacheKey(query, false);
                }

                using (var context2 = new QueryKeyCacheContext(rowNumberPaging: false, connection: testStore.Connection))
                {
                    var services = ((IInfrastructure<IServiceProvider>)context2).Instance.GetService<IDbContextServices>().DatabaseProviderServices;
                    var generator = services.CompiledQueryCacheKeyGenerator;
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
