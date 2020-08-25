// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable MergeConditionalExpression
// ReSharper disable ConstantNullCoalescingCondition
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public class QueryBugsInMemoryTest : IClassFixture<InMemoryFixture>
    {
        #region Bug9849

        [ConditionalFact]
        public virtual void Include_throw_when_empty_9849()
        {
            using (CreateScratch<DatabaseContext>(_ => { }, "9849"))
            {
                using var context = new DatabaseContext();
                var results = context.VehicleInspections.Include(_ => _.Motors).ToList();

                Assert.Empty(results);
            }
        }

        [ConditionalFact]
        public virtual void Include_throw_when_empty_9849_2()
        {
            using (CreateScratch<DatabaseContext>(_ => { }, "9849"))
            {
                using var context = new DatabaseContext();
#pragma warning disable IDE1006 // Naming Styles
                var results = context.VehicleInspections.Include(_foo => _foo.Motors).ToList();
#pragma warning restore IDE1006 // Naming Styles

                Assert.Empty(results);
            }
        }

        [ConditionalFact]
        public virtual void Include_throw_when_empty_9849_3()
        {
            using (CreateScratch<DatabaseContext>(_ => { }, "9849"))
            {
                using var context = new DatabaseContext();
#pragma warning disable IDE1006 // Naming Styles
                var results = context.VehicleInspections.Include(__ => __.Motors).ToList();
#pragma warning restore IDE1006 // Naming Styles

                Assert.Empty(results);
            }
        }

        [ConditionalFact]
        public virtual void Include_throw_when_empty_9849_4()
        {
            using (CreateScratch<DatabaseContext>(_ => { }, "9849"))
            {
                using var context = new DatabaseContext();
#pragma warning disable IDE1006 // Naming Styles
                var results = context.VehicleInspections.Include(___ => ___.Motors).ToList();
#pragma warning restore IDE1006 // Naming Styles

                Assert.Empty(results);
            }
        }

        [ConditionalFact]
        public virtual void Include_throw_when_empty_9849_5()
        {
            using (CreateScratch<DatabaseContext>(_ => { }, "9849"))
            {
                using var context = new DatabaseContext();
                var results
                    = (from _ in context.VehicleInspections
                       join _f in context.Motors on _.Id equals _f.Id
                       join __ in context.VehicleInspections on _f.Id equals __.Id
                       select _).ToList();

                Assert.Empty(results);
            }
        }

        [ConditionalFact]
        public virtual void Include_throw_when_empty_9849_6()
        {
            using (CreateScratch<DatabaseContext>(_ => { }, "9849"))
            {
                using var context = new DatabaseContext();
#pragma warning disable IDE1006 // Naming Styles
                // Explicitly named variables these way. They verify parameter names generated in EF Core.
                var _ = 0L;
                var __ = 0L;
                var _f = 0L;
#pragma warning restore IDE1006 // Naming Styles

                var results
                    = (from v in context.VehicleInspections
                       where v.Id == _ || v.Id == __ || v.Id == _f
                       select _).ToList();

                Assert.Empty(results);
            }
        }

        private class DatabaseContext : DbContext
        {
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder
                    .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                    .UseInMemoryDatabase("9849");
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                var builder = modelBuilder.Entity<VehicleInspection>();

                builder.HasMany(i => i.Motors).WithOne(a => a.Inspection).HasForeignKey(i => i.VehicleInspectionId);
            }

            public DbSet<VehicleInspection> VehicleInspections { get; set; }
            public DbSet<Motor> Motors { get; set; }
        }

        private class VehicleInspection
        {
            public long Id { get; set; }
            public ICollection<Motor> Motors { get; set; } = new HashSet<Motor>();
        }

        private class Motor
        {
            public long Id { get; set; }
            public long VehicleInspectionId { get; set; }
            public VehicleInspection Inspection { get; set; }
        }

        #endregion

        #region Bug3595

        [ConditionalFact]
        public virtual void GroupBy_with_uninitialized_datetime_projection_3595()
        {
            using (CreateScratch<Context3595>(Seed3595, "3595"))
            {
                using var context = new Context3595();
                var q0 = from instance in context.Exams
                         join question in context.ExamQuestions
                             on instance.Id equals question.ExamId
                         where instance.Id != 3
                         group question by question.QuestionId
                         into gQuestions
                         select new { gQuestions.Key, MaxDate = gQuestions.Max(q => q.Modified) };

                var result = q0.ToList();

                Assert.Equal(default, result.Single().MaxDate);
            }
        }

        private static void Seed3595(Context3595 context)
        {
            var question = new Question3595();
            var examInstance = new Exam3595();
            var examInstanceQuestion = new ExamQuestion3595 { Question = question, Exam = examInstance };

            context.Add(question);
            context.Add(examInstance);
            context.Add(examInstanceQuestion);
            context.SaveChanges();
        }

        private abstract class Base3595
        {
            public DateTime Modified { get; set; }
        }

        private class Question3595 : Base3595
        {
            public int Id { get; set; }
        }

        private class Exam3595 : Base3595
        {
            public int Id { get; set; }
        }

        private class ExamQuestion3595 : Base3595
        {
            public int Id { get; set; }

            public int QuestionId { get; set; }
            public Question3595 Question { get; set; }

            public int ExamId { get; set; }
            public Exam3595 Exam { get; set; }
        }

        private class Context3595 : DbContext
        {
            public DbSet<Exam3595> Exams { get; set; }
            public DbSet<Question3595> Questions { get; set; }
            public DbSet<ExamQuestion3595> ExamQuestions { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder
                    .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                    .UseInMemoryDatabase("3595");
            }
        }

        #endregion

        #region Bug3101

        [ConditionalFact]
        public virtual void Repro3101_simple_coalesce1()
        {
            using (CreateScratch<MyContext3101>(Seed3101, "3101"))
            {
                using var ctx = new MyContext3101();
                var query = from eVersion in ctx.Entities
                            join eRoot in ctx.Entities.Include(e => e.Children).AsNoTracking()
                                on eVersion.RootEntityId equals (int?)eRoot.Id
                                into RootEntities
                            from eRootJoined in RootEntities.DefaultIfEmpty()
                            select eRootJoined ?? eVersion;

                Assert.Equal(3, query.ToList().Count);
            }
        }

        [ConditionalFact]
        public virtual void Repro3101_simple_coalesce2()
        {
            using (CreateScratch<MyContext3101>(Seed3101, "3101"))
            {
                using var ctx = new MyContext3101();
                var query = from eVersion in ctx.Entities
                            join eRoot in ctx.Entities.Include(e => e.Children)
                                on eVersion.RootEntityId equals (int?)eRoot.Id
                                into RootEntities
                            from eRootJoined in RootEntities.DefaultIfEmpty()
                            select eRootJoined ?? eVersion;

                var result = query.ToList();
                Assert.Equal(2, result.Count(e => e.Children.Count > 0));
            }
        }

        [ConditionalFact]
        public virtual void Repro3101_simple_coalesce3()
        {
            using (CreateScratch<MyContext3101>(Seed3101, "3101"))
            {
                using var ctx = new MyContext3101();
                var query = from eVersion in ctx.Entities.Include(e => e.Children)
                            join eRoot in ctx.Entities.Include(e => e.Children)
                                on eVersion.RootEntityId equals (int?)eRoot.Id
                                into RootEntities
                            from eRootJoined in RootEntities.DefaultIfEmpty()
                            select eRootJoined ?? eVersion;

                var result = query.ToList();

                Assert.True(result.All(e => e.Children.Count > 0));
            }
        }

        [ConditionalFact]
        public virtual void Repro3101_complex_coalesce1()
        {
            using (CreateScratch<MyContext3101>(Seed3101, "3101"))
            {
                using var ctx = new MyContext3101();
                var query = from eVersion in ctx.Entities.Include(e => e.Children)
                            join eRoot in ctx.Entities
                                on eVersion.RootEntityId equals (int?)eRoot.Id
                                into RootEntities
                            from eRootJoined in RootEntities.DefaultIfEmpty()
                            select new { One = 1, Coalesce = eRootJoined ?? eVersion };

                var result = query.ToList();
                Assert.True(result.All(e => e.Coalesce.Children.Count > 0));
            }
        }

        [ConditionalFact]
        public virtual void Repro3101_complex_coalesce2()
        {
            using (CreateScratch<MyContext3101>(Seed3101, "3101"))
            {
                using var ctx = new MyContext3101();
                var query = from eVersion in ctx.Entities
                            join eRoot in ctx.Entities.Include(e => e.Children)
                                on eVersion.RootEntityId equals (int?)eRoot.Id
                                into RootEntities
                            from eRootJoined in RootEntities.DefaultIfEmpty()
                            select new { Root = eRootJoined, Coalesce = eRootJoined ?? eVersion };

                var result = query.ToList();
                Assert.Equal(2, result.Count(e => e.Coalesce.Children.Count > 0));
            }
        }

        [ConditionalFact]
        public virtual void Repro3101_nested_coalesce1()
        {
            using (CreateScratch<MyContext3101>(Seed3101, "3101"))
            {
                using var ctx = new MyContext3101();
                var query = from eVersion in ctx.Entities
                            join eRoot in ctx.Entities.Include(e => e.Children)
                                on eVersion.RootEntityId equals (int?)eRoot.Id
                                into RootEntities
                            from eRootJoined in RootEntities.DefaultIfEmpty()
                            select new { One = 1, Coalesce = eRootJoined ?? (eVersion ?? eRootJoined) };

                var result = query.ToList();
                Assert.Equal(2, result.Count(e => e.Coalesce.Children.Count > 0));
            }
        }

        [ConditionalFact]
        public virtual void Repro3101_nested_coalesce2()
        {
            using (CreateScratch<MyContext3101>(Seed3101, "3101"))
            {
                using var ctx = new MyContext3101();
                var query = from eVersion in ctx.Entities.Include(e => e.Children)
                            join eRoot in ctx.Entities
                                on eVersion.RootEntityId equals (int?)eRoot.Id
                                into RootEntities
                            from eRootJoined in RootEntities.DefaultIfEmpty()
                            select new
                            {
                                One = eRootJoined,
                                Two = 2,
                                Coalesce = eRootJoined ?? (eVersion ?? eRootJoined)
                            };

                var result = query.ToList();
                Assert.True(result.All(e => e.Coalesce.Children.Count > 0));
            }
        }

        [ConditionalFact]
        public virtual void Repro3101_conditional()
        {
            using (CreateScratch<MyContext3101>(Seed3101, "3101"))
            {
                using var ctx = new MyContext3101();
                var query = from eVersion in ctx.Entities.Include(e => e.Children)
                            join eRoot in ctx.Entities
                                on eVersion.RootEntityId equals (int?)eRoot.Id
                                into RootEntities
                            from eRootJoined in RootEntities.DefaultIfEmpty()
#pragma warning disable IDE0029 // Use coalesce expression
                            select eRootJoined != null ? eRootJoined : eVersion;
#pragma warning restore IDE0029 // Use coalesce expression

                var result = query.ToList();
                Assert.True(result.All(e => e.Children.Count > 0));
            }
        }

        [ConditionalFact]
        public virtual void Repro3101_coalesce_tracking()
        {
            using (CreateScratch<MyContext3101>(Seed3101, "3101"))
            {
                using var ctx = new MyContext3101();
                var query = from eVersion in ctx.Entities
                            join eRoot in ctx.Entities
                                on eVersion.RootEntityId equals (int?)eRoot.Id
                                into RootEntities
                            from eRootJoined in RootEntities.DefaultIfEmpty()
                            select new
                            {
                                eRootJoined,
                                eVersion,
                                foo = eRootJoined ?? eVersion
                            };

                Assert.Equal(3, query.ToList().Count);

                Assert.True(ctx.ChangeTracker.Entries().Any());
            }
        }

        private static void Seed3101(MyContext3101 context)
        {
            var c11 = new Child3101 { Name = "c11" };
            var c12 = new Child3101 { Name = "c12" };
            var c13 = new Child3101 { Name = "c13" };
            var c21 = new Child3101 { Name = "c21" };
            var c22 = new Child3101 { Name = "c22" };
            var c31 = new Child3101 { Name = "c31" };
            var c32 = new Child3101 { Name = "c32" };

            context.Children.AddRange(c11, c12, c13, c21, c22, c31, c32);

            var e1 = new Entity3101 { Id = 1, Children = new[] { c11, c12, c13 } };
            var e2 = new Entity3101 { Id = 2, Children = new[] { c21, c22 } };
            var e3 = new Entity3101 { Id = 3, Children = new[] { c31, c32 } };

            e2.RootEntity = e1;

            context.Entities.AddRange(e1, e2, e3);
            context.SaveChanges();
        }

        private class MyContext3101 : DbContext
        {
            public DbSet<Entity3101> Entities { get; set; }

            public DbSet<Child3101> Children { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder
                    .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                    .UseInMemoryDatabase("3101");
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Entity3101>().Property(e => e.Id).ValueGeneratedNever();
            }
        }

        private class Entity3101
        {
            public Entity3101()
            {
                Children = new Collection<Child3101>();
            }

            public int Id { get; set; }

            public int? RootEntityId { get; set; }

            public Entity3101 RootEntity { get; set; }

            public ICollection<Child3101> Children { get; set; }
        }

        private class Child3101
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        #endregion

        #region Bug5456

        [ConditionalFact]
        public virtual void Repro5456_include_group_join_is_per_query_context()
        {
            using (CreateScratch<MyContext5456>(Seed5456, "5456"))
            {
                Parallel.For(
                    0, 10, i =>
                    {
                        using var ctx = new MyContext5456();
                        var result = ctx.Posts.Where(x => x.Blog.Id > 1).Include(x => x.Blog).ToList();

                        Assert.Equal(198, result.Count);
                    });
            }
        }

        [ConditionalFact]
        public virtual void Repro5456_include_group_join_is_per_query_context_async()
        {
            using (CreateScratch<MyContext5456>(Seed5456, "5456"))
            {
                Parallel.For(
                    0, 10, async i =>
                    {
                        using var ctx = new MyContext5456();
                        var result = await ctx.Posts.Where(x => x.Blog.Id > 1).Include(x => x.Blog).ToListAsync();

                        Assert.Equal(198, result.Count);
                    });
            }
        }

        [ConditionalFact]
        public virtual void Repro5456_multiple_include_group_join_is_per_query_context()
        {
            using (CreateScratch<MyContext5456>(Seed5456, "5456"))
            {
                Parallel.For(
                    0, 10, i =>
                    {
                        using var ctx = new MyContext5456();
                        var result = ctx.Posts.Where(x => x.Blog.Id > 1).Include(x => x.Blog).Include(x => x.Comments).ToList();

                        Assert.Equal(198, result.Count);
                    });
            }
        }

        [ConditionalFact]
        public virtual void Repro5456_multiple_include_group_join_is_per_query_context_async()
        {
            using (CreateScratch<MyContext5456>(Seed5456, "5456"))
            {
                Parallel.For(
                    0, 10, async i =>
                    {
                        using var ctx = new MyContext5456();
                        var result = await ctx.Posts.Where(x => x.Blog.Id > 1).Include(x => x.Blog).Include(x => x.Comments)
                            .ToListAsync();

                        Assert.Equal(198, result.Count);
                    });
            }
        }

        [ConditionalFact]
        public virtual void Repro5456_multi_level_include_group_join_is_per_query_context()
        {
            using (CreateScratch<MyContext5456>(Seed5456, "5456"))
            {
                Parallel.For(
                    0, 10, i =>
                    {
                        using var ctx = new MyContext5456();
                        var result = ctx.Posts.Where(x => x.Blog.Id > 1).Include(x => x.Blog).ThenInclude(b => b.Author).ToList();

                        Assert.Equal(198, result.Count);
                    });
            }
        }

        [ConditionalFact]
        public virtual void Repro5456_multi_level_include_group_join_is_per_query_context_async()
        {
            using (CreateScratch<MyContext5456>(Seed5456, "5456"))
            {
                Parallel.For(
                    0, 10, async i =>
                    {
                        using var ctx = new MyContext5456();
                        var result = await ctx.Posts.Where(x => x.Blog.Id > 1).Include(x => x.Blog).ThenInclude(b => b.Author)
                            .ToListAsync();

                        Assert.Equal(198, result.Count);
                    });
            }
        }

        private void Seed5456(MyContext5456 context)
        {
            for (var i = 0; i < 100; i++)
            {
                context.Add(
                    new Blog5456
                    {
                        Id = i + 1,
                        Posts = new List<Post5456>
                        {
                            new Post5456 { Comments = new List<Comment5456> { new Comment5456(), new Comment5456() } },
                            new Post5456()
                        },
                        Author = new Author5456()
                    });
            }

            context.SaveChanges();
        }

        private class MyContext5456 : DbContext
        {
            public DbSet<Blog5456> Blogs { get; set; }
            public DbSet<Post5456> Posts { get; set; }
            public DbSet<Comment5456> Comments { get; set; }
            public DbSet<Author5456> Authors { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder
                    .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                    .UseInMemoryDatabase("5456");
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Blog5456>().Property(e => e.Id).ValueGeneratedNever();
            }
        }

        private class Blog5456
        {
            public int Id { get; set; }
            public List<Post5456> Posts { get; set; }
            public Author5456 Author { get; set; }
        }

        private class Author5456
        {
            public int Id { get; set; }
            public List<Blog5456> Blogs { get; set; }
        }

        private class Post5456
        {
            public int Id { get; set; }
            public Blog5456 Blog { get; set; }
            public List<Comment5456> Comments { get; set; }
        }

        private class Comment5456
        {
            public int Id { get; set; }
            public Post5456 Blog { get; set; }
        }

        #endregion

        #region Bug8282

        [ConditionalFact]
        public virtual void Entity_passed_to_DTO_constructor_works()
        {
            using (CreateScratch<MyContext8282>(e => { }, "8282"))
            {
                using var context = new MyContext8282();
                var query = context.Entity.Select(e => new EntityDto8282(e)).ToList();

                Assert.Empty(query);
            }
        }

        private class MyContext8282 : DbContext
        {
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public DbSet<Entity8282> Entity { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder
                    .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                    .UseInMemoryDatabase("8282");
            }
        }

        private class Entity8282
        {
            public int Id { get; set; }
        }

        private class EntityDto8282
        {
            public EntityDto8282(Entity8282 entity)
            {
                Id = entity.Id;
            }

            public int Id { get; set; }
        }

        #endregion

        #region Bug19708

        [ConditionalFact]
        public virtual void GroupJoin_SelectMany_in_defining_query_is_flattened()
        {
            using (CreateScratch<MyContext19708>(Seed19708, "19708"))
            {
                using var context = new MyContext19708();

                var query = context.Set<CustomerView19708>().ToList();

                Assert.Collection(
                    query,
                    t => AssertCustomerView(t, 1, "First", 1, "FirstChild"),
                    t => AssertCustomerView(t, 2, "Second", 2, "SecondChild1"),
                    t => AssertCustomerView(t, 2, "Second", 3, "SecondChild2"),
                    t => AssertCustomerView(t, 3, "Third", null, ""));
            }

            static void AssertCustomerView(
                CustomerView19708 actual,
                int id,
                string name,
                int? customerMembershipId,
                string customerMembershipName)
            {
                Assert.Equal(id, actual.Id);
                Assert.Equal(name, actual.Name);
                Assert.Equal(customerMembershipId, actual.CustomerMembershipId);
                Assert.Equal(customerMembershipName, actual.CustomerMembershipName);
            }
        }

        private class MyContext19708 : DbContext
        {
            public DbSet<Customer19708> Customers { get; set; }
            public DbSet<CustomerMembership19708> CustomerMemberships { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder
                    .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                    .UseInMemoryDatabase("19708");
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<CustomerView19708>().HasNoKey().ToInMemoryQuery(Build_Customers_Sql_View_InMemory());
            }

            private Expression<Func<IQueryable<CustomerView19708>>> Build_Customers_Sql_View_InMemory()
            {
                Expression<Func<IQueryable<CustomerView19708>>> query = () =>
                    from customer in Customers
                    join customerMembership in CustomerMemberships on customer.Id equals customerMembership.CustomerId into
                        nullableCustomerMemberships
                    from customerMembership in nullableCustomerMemberships.DefaultIfEmpty()
                    select new CustomerView19708
                    {
                        Id = customer.Id,
                        Name = customer.Name,
                        CustomerMembershipId = customerMembership != null ? customerMembership.Id : default(int?),
                        CustomerMembershipName = customerMembership != null ? customerMembership.Name : ""
                    };
                return query;
            }
        }

        private static void Seed19708(MyContext19708 context)
        {
            var customer1 = new Customer19708 { Name = "First" };
            var customer2 = new Customer19708 { Name = "Second" };
            var customer3 = new Customer19708 { Name = "Third" };

            var customerMembership1 = new CustomerMembership19708 { Name = "FirstChild", Customer = customer1 };
            var customerMembership2 = new CustomerMembership19708 { Name = "SecondChild1", Customer = customer2 };
            var customerMembership3 = new CustomerMembership19708 { Name = "SecondChild2", Customer = customer2 };

            context.AddRange(customer1, customer2, customer3);
            context.AddRange(customerMembership1, customerMembership2, customerMembership3);

            context.SaveChanges();
        }

        private class Customer19708
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private class CustomerMembership19708
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int CustomerId { get; set; }
            public Customer19708 Customer { get; set; }
        }

        private class CustomerView19708
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int? CustomerMembershipId { get; set; }
            public string CustomerMembershipName { get; set; }
        }

        #endregion

        #region Issue21768

        [ConditionalFact]
        public virtual void Using_explicit_interface_implementation_as_navigation_works()
        {
            using (CreateScratch<MyContext21768>(t => { }, "21768"))
            {
                using var context = new MyContext21768();
                Expression<Func<IBook21768, BookViewModel21768>> projection = b => new BookViewModel21768
                {
                    FirstPage = b.FrontCover.Illustrations.FirstOrDefault(i => i.State >= IllustrationState21768.Approved) != null
                        ? new PageViewModel21768
                        {
                            Uri = b.FrontCover.Illustrations.FirstOrDefault(i => i.State >= IllustrationState21768.Approved).Uri
                        }
                        : null,
                };

                var result = context.Books.Where(b => b.Id == 1).Select(projection).SingleOrDefault();
            }
        }

        private class BookViewModel21768
        {
            public PageViewModel21768 FirstPage { get; set; }
        }

        private class PageViewModel21768
        {
            public string Uri { get; set; }
        }

        private interface IBook21768
        {
            public int Id { get; set; }

            public IBookCover21768 FrontCover { get; }
            public int FrontCoverId { get; set; }

            public IBookCover21768 BackCover { get; }
            public int BackCoverId { get; set; }
        }

        private interface IBookCover21768
        {
            public int Id { get; set; }
            public IEnumerable<ICoverIllustration21768> Illustrations { get; }
        }

        private interface ICoverIllustration21768
        {
            public int Id { get; set; }
            public IBookCover21768 Cover { get; }
            public int CoverId { get; set; }
            public string Uri { get; set; }
            public IllustrationState21768 State { get; set; }
        }

        private class Book21768 : IBook21768
        {
            public int Id { get; set; }

            public BookCover21768 FrontCover { get; set; }
            public int FrontCoverId { get; set; }

            public BookCover21768 BackCover { get; set; }
            public int BackCoverId { get; set; }

            IBookCover21768 IBook21768.FrontCover
                => FrontCover;

            IBookCover21768 IBook21768.BackCover
                => BackCover;
        }

        private class BookCover21768 : IBookCover21768
        {
            public int Id { get; set; }
            public ICollection<CoverIllustration21768> Illustrations { get; set; }

            IEnumerable<ICoverIllustration21768> IBookCover21768.Illustrations
                => Illustrations;
        }

        private class CoverIllustration21768 : ICoverIllustration21768
        {
            public int Id { get; set; }
            public BookCover21768 Cover { get; set; }
            public int CoverId { get; set; }
            public string Uri { get; set; }
            public IllustrationState21768 State { get; set; }

            IBookCover21768 ICoverIllustration21768.Cover
                => Cover;
        }

        private enum IllustrationState21768
        {
            New,
            PendingApproval,
            Approved,
            Printed
        }

        private class MyContext21768 : DbContext
        {
            public DbSet<Book21768> Books { get; set; }
            public DbSet<BookCover21768> BookCovers { get; set; }
            public DbSet<CoverIllustration21768> CoverIllustrations { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder
                    .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                    .UseInMemoryDatabase("21768");
            }
        }

        #endregion

        #region Issue21803

        [ConditionalFact]
        public virtual void Select_enumerable_navigation_backed_by_collection()
        {
            using (CreateScratch<MyContext21803>(Seed21803, "21803"))
            {
                using var context = new MyContext21803();

                var query = context.Set<AppEntity21803>().Select(appEntity => appEntity.OtherEntities);

                query.ToList();
            }
        }

        private static void Seed21803(MyContext21803 context)
        {
            var appEntity = new AppEntity21803();
            context.AddRange(
                new OtherEntity21803 { AppEntity = appEntity },
                new OtherEntity21803 { AppEntity = appEntity },
                new OtherEntity21803 { AppEntity = appEntity },
                new OtherEntity21803 { AppEntity = appEntity });

            context.SaveChanges();
        }

        private class AppEntity21803
        {
            private readonly List<OtherEntity21803> _otherEntities = new List<OtherEntity21803>();

            public int Id { get; private set; }

            public IEnumerable<OtherEntity21803> OtherEntities
                => _otherEntities;
        }

        private class OtherEntity21803
        {
            public int Id { get; private set; }
            public AppEntity21803 AppEntity { get; set; }
        }

        private class MyContext21803 : DbContext
        {
            public DbSet<AppEntity21803> Entities { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder
                    .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                    .UseInMemoryDatabase("21803");
            }
        }

        #endregion

        #region Issue20729

        [ConditionalFact]
        public virtual void Multiple_owned_references_at_same_level_maintains_valueBuffer_positions()
        {
            using (CreateScratch<MyContext20729>(Seed20729, "20729"))
            {
                using var context = new MyContext20729();

                var query = context.Set<Owner20729>()
                    .Select(dtoOwner => new
                    {
                        dtoOwner.Id,
                        Owned2 = dtoOwner.Owned2 == null ? null : new
                        {
                            Other = dtoOwner.Owned2.Other == null ? null : new { dtoOwner.Owned2.Other.Id }
                        }
                        ,
                        Owned1 = dtoOwner.Owned1 == null ? null : new { dtoOwner.Owned1.Value }

                    }
                    ).ToList();

                var owner = Assert.Single(query);
                Assert.NotNull(owner.Owned1);
                Assert.NotNull(owner.Owned2);
            }
        }

        private static void Seed20729(MyContext20729 context)
        {
            context.Owners.Add(new Owner20729
            {
                Owned1 = new Owned120729(),
                Owned2 = new Owned220729(),
            });

            context.SaveChanges();
        }

        private class Owner20729
        {
            public int Id { get; set; }
            public Owned120729 Owned1 { get; set; }
            public Owned220729 Owned2 { get; set; }
        }

        [Owned]
        private class Owned120729
        {
            public int Value { get; set; }
        }
        [Owned]
        private class Owned220729
        {
            public Other20729 Other { get; set; }
        }
        private class Other20729
        {
            public int Id { get; set; }
        }

        private class MyContext20729 : DbContext
        {
            public DbSet<Owner20729> Owners { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder
                    .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                    .UseInMemoryDatabase("20729");
            }
        }

        #endregion

        #region SharedHelper

        private static InMemoryTestStore CreateScratch<TContext>(Action<TContext> seed, string databaseName)
            where TContext : DbContext, new()
        {
            return InMemoryTestStore.GetOrCreate(databaseName)
                .InitializeInMemory(null, () => new TContext(), c => seed((TContext)c));
        }

        #endregion
    }
}
