// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class QueryProviderTest
    {
        [Fact]
        public void Non_generic_ExecuteQuery_does_not_throw()
        {
            var context = new TestContext();
            Func<IQueryable<TestEntity>, int> func = Queryable.Count;
            IQueryable q = context.TestEntities;
            var expr = Expression.Call(null, func.GetMethodInfo(), q.Expression);
            Assert.Equal(0, q.Provider.Execute<int>(expr));
            Assert.Equal(0, (int)q.Provider.Execute(expr));
        }

        [Fact]
        public void Non_generic_ExecuteQuery_does_not_throw_incorrect_pattern()
        {
            var context = new TestContext();
            Func<IQueryable<TestEntity>, int> func = Queryable.Count;
            IQueryable q = context.TestEntities;
            var expr = Expression.Call(null, func.GetMethodInfo(), Expression.Constant(q));
            Assert.Equal(0, q.Provider.Execute<int>(expr));
            Assert.Equal(0, (int)q.Provider.Execute(expr));
        }

        #region Fixture

        private class TestEntity
        {
            public int Id { get; set; }
        }

        private class TestContext : DbContext
        {
            public DbSet<TestEntity> TestEntities { get; set; }

            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());
        }

        #endregion
    }
}
