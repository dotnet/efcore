// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Data.Entity.AzureTableStorage.Query;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Moq;
using Remotion.Linq;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests.Query
{
    public class QueryModelVisitorTests
    {
        [Fact]
        public void Simple_where_clause()
        {
            var scans = CountScans<Root>(set => set.Where(t => t.ID > 5));
            Assert.Equal(1, scans);
        }

        [Fact]
        public void Simple_projection()
        {
            var scans = CountScans<Root>(set => set.Select(s => s.ID));
            Assert.Equal(1, scans);
        }

        [Fact]
        public void Select_many_clause()
        {
            var scans = CountScans<Root, Branch>((rs, bs) =>
                from c in rs
                from e in bs
                where c.SHA1 == e.SHA1
                orderby c.ID, e.ID
                select new { c, e }
                );
            Assert.Equal(2, scans);
        }

        [Fact]
        public void Nested_select_statements()
        {
            var scans = CountScans<Root>(r =>
                from r1 in (from r2 in (from r3 in r select r3) select r2) select r1
                );
            Assert.Equal(1, scans);
        }

        [Fact]
        public void Subquery()
        {
            var scans = CountScans<Root>(r =>
                from r1 in r
                where (
                    from r2 in r1.Branches
                    where DateTime.IsLeapYear(r2.Timestamp.Year)
                    select r2).Any()
                select r1
                );
            Assert.Equal(1, scans);
        }

        private int CountScans<T>(Expression<Func<DbSet<T>, IQueryable>> expression) where T : class, new()
        {
            var query = expression.Compile()(new DbSet<T>(Mock.Of<DbContext>()));
            return CountQuery(query);
        }

        private int CountScans<T1, T2>(Expression<Func<DbSet<T1>, DbSet<T2>, IQueryable>> expression) where T1 : class, new() where T2 : class
        {
            var query = expression.Compile()(new DbSet<T1>(Mock.Of<DbContext>()), new DbSet<T2>(Mock.Of<DbContext>()));
            return CountQuery(query);
        }

        private int CountQuery(IQueryable query)
        {
            var queryModel = new EntityQueryProvider(new EntityQueryExecutor(Mock.Of<DbContext>())).GenerateQueryModel(query.Expression);

            return CountQueryModel(queryModel);
        }

        public int CountQueryModel(QueryModel queryModel)
        {
            var context = new AtsQueryCompilationContext(CreateModel());
            var visitor = context.CreateQueryModelVisitor();
            visitor.VisitQueryModel(queryModel);

            var counter = new EntityScanCounterVisitor(this);
            counter.VisitExpression(visitor.Expression);
            return counter.EntityScanCount;
        }

        public IModel CreateModel()
        {
            var model = new Model();
            var builder = new ModelBuilder(model);

            builder.Entity<Branch>(b =>
                {
                    b.Key(s => s.ID);
                    b.Property(s => s.RootID);
                    b.Property(s => s.SHA1);
                });

            builder.Entity<Root>(b =>
                {
                    b.Key(s => s.ID);
                    b.Property(s => s.SHA1);
                    b.OneToMany(e => e.Branches, e => e.Root);
                });

            return model;
        }
    }

    public class Root
    {
        public int ID { get; set; }
        public virtual ICollection<Branch> Branches { get; set; }
        public string SHA1 { get; set; }
    }

    public class Branch
    {
        public int ID { get; set; }
        public string SHA1 { get; set; }
        public int RootID { get; set; }
        public virtual Root Root { get; set; }

        public DateTimeOffset Timestamp { get; set; }
    }

    public class EntityScanCounterVisitor : ExpressionTreeVisitor
    {
        private readonly QueryModelVisitorTests _tester;

        public EntityScanCounterVisitor(QueryModelVisitorTests tester)
        {
            EntityScanCount = 0;
            _tester = tester;
        }

        public int EntityScanCount { get; private set; }

        protected override Expression VisitMethodCallExpression(MethodCallExpression expression)
        {
            if (expression.Method.DeclaringType == typeof(AtsQueryModelVisitor)
                && expression.Method.Name == "ExecuteSelectExpression")
            {
                EntityScanCount++;
            }
            return base.VisitMethodCallExpression(expression);
        }

        protected override Expression VisitSubQueryExpression(SubQueryExpression expression)
        {
            EntityScanCount += _tester.CountQueryModel(expression.QueryModel);
            return expression;
        }
    }
}
