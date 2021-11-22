// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using NameSpace1;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class SimpleQueryRelationalTestBase : SimpleQueryTestBase
    {
        protected TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected void ClearLog()
            => TestSqlLoggerFactory.Clear();

        protected void AssertSql(params string[] expected)
            => TestSqlLoggerFactory.AssertBaseline(expected);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Multiple_different_entity_type_from_different_namespaces(bool async)
        {
            var contextFactory = await InitializeAsync<Context23981>();
            using var context = contextFactory.CreateContext();
            //var good1 = context.Set<NameSpace1.TestQuery>().FromSqlRaw(@"SELECT 1 AS MyValue").ToList(); // OK
            //var good2 = context.Set<NameSpace2.TestQuery>().FromSqlRaw(@"SELECT 1 AS MyValue").ToList(); // OK
            var bad = context.Set<TestQuery>().FromSqlRaw(@"SELECT cast(null as int) AS MyValue").ToList(); // Exception
        }

        protected class Context23981 : DbContext
        {
            public Context23981(DbContextOptions options)
                : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                var mb = modelBuilder.Entity(typeof(TestQuery));

                mb.HasBaseType((Type)null);
                mb.HasNoKey();
                mb.ToTable((string)null);

                mb = modelBuilder.Entity(typeof(NameSpace2.TestQuery));

                mb.HasBaseType((Type)null);
                mb.HasNoKey();
                mb.ToTable((string)null);
            }
        }
    }
}

namespace NameSpace1
{
    public class TestQuery
    {
        public int? MyValue { get; set; }
    }
}

namespace NameSpace2
{
    public class TestQuery
    {
        public int MyValue { get; set; }
    }
}
