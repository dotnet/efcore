// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
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
                => optionsBuilder
                    .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                    .UseInMemoryDatabase("9849");

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
            public ICollection<Motor> Motors { get; } = new HashSet<Motor>();
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
                => optionsBuilder
                    .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                    .UseInMemoryDatabase("3595");
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
                                on eVersion.RootEntityId equals eRoot.Id
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
                                on eVersion.RootEntityId equals eRoot.Id
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
                                on eVersion.RootEntityId equals eRoot.Id
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
                                on eVersion.RootEntityId equals eRoot.Id
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
                                on eVersion.RootEntityId equals eRoot.Id
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
                                on eVersion.RootEntityId equals eRoot.Id
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
                                on eVersion.RootEntityId equals eRoot.Id
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
                                on eVersion.RootEntityId equals eRoot.Id
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
                                on eVersion.RootEntityId equals eRoot.Id
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
                => optionsBuilder
                    .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                    .UseInMemoryDatabase("3101");

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Entity3101>().Property(e => e.Id).ValueGeneratedNever();
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
                        Posts = new List<Post5456> { new() { Comments = new List<Comment5456> { new(), new() } }, new() },
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
                => optionsBuilder
                    .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                    .UseInMemoryDatabase("5456");

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog5456>().Property(e => e.Id).ValueGeneratedNever();
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
                => optionsBuilder
                    .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                    .UseInMemoryDatabase("8282");
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

            public int Id { get; }
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
                => optionsBuilder
                    .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                    .UseInMemoryDatabase("19708");

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<CustomerView19708>().HasNoKey().ToInMemoryQuery(Build_Customers_Sql_View_InMemory());

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
                => optionsBuilder
                    .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                    .UseInMemoryDatabase("21768");
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
            private readonly List<OtherEntity21803> _otherEntities = new();

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
                => optionsBuilder
                    .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                    .UseInMemoryDatabase("21803");
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
                    .Select(
                        dtoOwner => new
                        {
                            dtoOwner.Id,
                            Owned2 = dtoOwner.Owned2 == null
                                ? null
                                : new { Other = dtoOwner.Owned2.Other == null ? null : new { dtoOwner.Owned2.Other.Id } },
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
            context.Owners.Add(
                new Owner20729
                {
                    Owned1 = new Owned120729(), Owned2 = new Owned220729(),
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
                => optionsBuilder
                    .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                    .UseInMemoryDatabase("20729");
        }

        #endregion

        #region Issue19253

        [ConditionalFact]
        public virtual void Concat_combines_nullability_of_entity_shapers()
        {
            using (CreateScratch<MyContext19253>(Seed19253, "19253"))
            {
                using var context = new MyContext19253();

                Expression<Func<A19253, string>> leftKeySelector = x => x.forkey;
                Expression<Func<B19253, string>> rightKeySelector = y => y.forkey;

                var query = context.A.GroupJoin(
                        context.B,
                        leftKeySelector,
                        rightKeySelector,
                        (left, rightg) => new { left, rightg })
                    .SelectMany(
                        r => r.rightg.DefaultIfEmpty(),
                        (x, y) => new JoinResult19253<A19253, B19253> { Left = x.left, Right = y })
                    .Concat(
                        context.B.GroupJoin(
                                context.A,
                                rightKeySelector,
                                leftKeySelector,
                                (right, leftg) => new { leftg, right })
                            .SelectMany(
                                l => l.leftg.DefaultIfEmpty(),
                                (x, y) => new JoinResult19253<A19253, B19253> { Left = y, Right = x.right })
                            .Where(z => z.Left.Equals(null)))
                    .ToList();

                Assert.Equal(3, query.Count);
            }
        }

        [ConditionalFact]
        public virtual void Union_combines_nullability_of_entity_shapers()
        {
            using (CreateScratch<MyContext19253>(Seed19253, "19253"))
            {
                using var context = new MyContext19253();

                Expression<Func<A19253, string>> leftKeySelector = x => x.forkey;
                Expression<Func<B19253, string>> rightKeySelector = y => y.forkey;

                var query = context.A.GroupJoin(
                        context.B,
                        leftKeySelector,
                        rightKeySelector,
                        (left, rightg) => new { left, rightg })
                    .SelectMany(
                        r => r.rightg.DefaultIfEmpty(),
                        (x, y) => new JoinResult19253<A19253, B19253> { Left = x.left, Right = y })
                    .Union(
                        context.B.GroupJoin(
                                context.A,
                                rightKeySelector,
                                leftKeySelector,
                                (right, leftg) => new { leftg, right })
                            .SelectMany(
                                l => l.leftg.DefaultIfEmpty(),
                                (x, y) => new JoinResult19253<A19253, B19253> { Left = y, Right = x.right })
                            .Where(z => z.Left.Equals(null)))
                    .ToList();

                Assert.Equal(3, query.Count);
            }
        }

        [ConditionalFact]
        public virtual void Except_combines_nullability_of_entity_shapers()
        {
            using (CreateScratch<MyContext19253>(Seed19253, "19253"))
            {
                using var context = new MyContext19253();

                Expression<Func<A19253, string>> leftKeySelector = x => x.forkey;
                Expression<Func<B19253, string>> rightKeySelector = y => y.forkey;

                var query = context.A.GroupJoin(
                        context.B,
                        leftKeySelector,
                        rightKeySelector,
                        (left, rightg) => new { left, rightg })
                    .SelectMany(
                        r => r.rightg.DefaultIfEmpty(),
                        (x, y) => new JoinResult19253<A19253, B19253> { Left = x.left, Right = y })
                    .Except(
                        context.B.GroupJoin(
                                context.A,
                                rightKeySelector,
                                leftKeySelector,
                                (right, leftg) => new { leftg, right })
                            .SelectMany(
                                l => l.leftg.DefaultIfEmpty(),
                                (x, y) => new JoinResult19253<A19253, B19253> { Left = y, Right = x.right }))
                    .ToList();

                Assert.Single(query);
            }
        }

        [ConditionalFact]
        public virtual void Intersect_combines_nullability_of_entity_shapers()
        {
            using (CreateScratch<MyContext19253>(Seed19253, "19253"))
            {
                using var context = new MyContext19253();

                Expression<Func<A19253, string>> leftKeySelector = x => x.forkey;
                Expression<Func<B19253, string>> rightKeySelector = y => y.forkey;

                var query = context.A.GroupJoin(
                        context.B,
                        leftKeySelector,
                        rightKeySelector,
                        (left, rightg) => new { left, rightg })
                    .SelectMany(
                        r => r.rightg.DefaultIfEmpty(),
                        (x, y) => new JoinResult19253<A19253, B19253> { Left = x.left, Right = y })
                    .Intersect(
                        context.B.GroupJoin(
                                context.A,
                                rightKeySelector,
                                leftKeySelector,
                                (right, leftg) => new { leftg, right })
                            .SelectMany(
                                l => l.leftg.DefaultIfEmpty(),
                                (x, y) => new JoinResult19253<A19253, B19253> { Left = y, Right = x.right }))
                    .ToList();

                Assert.Single(query);
            }
        }

        private class MyContext19253 : DbContext
        {
            public DbSet<A19253> A { get; set; }
            public DbSet<B19253> B { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                    .UseInMemoryDatabase("19253");
        }

        private class JoinResult19253<TLeft, TRight>
        {
            public TLeft Left { get; set; }

            public TRight Right { get; set; }
        }

        private class A19253
        {
            public int Id { get; set; }
            public string a { get; set; }
            public string a1 { get; set; }
            public string forkey { get; set; }
        }

        private class B19253
        {
            public int Id { get; set; }
            public string b { get; set; }
            public string b1 { get; set; }
            public string forkey { get; set; }
        }

        private static void Seed19253(MyContext19253 context)
        {
            var tmp_a = new A19253[]
            {
                new()
                {
                    a = "a0",
                    a1 = "a1",
                    forkey = "a"
                },
                new()
                {
                    a = "a2",
                    a1 = "a1",
                    forkey = "d"
                },
            };
            var tmp_b = new B19253[]
            {
                new()
                {
                    b = "b0",
                    b1 = "b1",
                    forkey = "a"
                },
                new()
                {
                    b = "b2",
                    b1 = "b1",
                    forkey = "c"
                },
            };
            context.A.AddRange(tmp_a);
            context.B.AddRange(tmp_b);

            context.SaveChanges();
        }

        #endregion

        #region Issue23285

        [ConditionalFact]
        public virtual void Owned_reference_on_base_with_hierarchy()
        {
            using (CreateScratch<MyContext23285>(Seed23285, "23285"))
            {
                using var context = new MyContext23285();

                var query = context.Table.ToList();

                var root = Assert.Single(query);
                Assert.True(root is ChildA23285);
            }
        }

        private static void Seed23285(MyContext23285 context)
        {
            context.Table.Add(new ChildA23285());

            context.SaveChanges();
        }

        [Owned]
        private class OwnedClass23285
        {
            public string A { get; set; }
            public string B { get; set; }
        }

        private class Root23285
        {
            public int Id { get; set; }
            public OwnedClass23285 OwnedProp { get; set; }
        }

        private class ChildA23285 : Root23285
        {
            public bool Prop { get; set; }
        }

        private class ChildB23285 : Root23285
        {
            public double Prop { get; set; }
        }

        private class MyContext23285 : DbContext
        {
            public DbSet<Root23285> Table { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                    .UseInMemoryDatabase("23285");

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<ChildA23285>().HasBaseType<Root23285>();
                modelBuilder.Entity<ChildB23285>().HasBaseType<Root23285>();
            }
        }

        #endregion

        #region Issue23687

        [ConditionalFact]
        public virtual void Owned_reference_with_composite_key()
        {
            using (CreateScratch<MyContext23687>(Seed23687, "23687"))
            {
                using var context = new MyContext23687();

                var query = context.Table.ToList();

                var root = Assert.Single(query);
                Assert.Equal("A", root.OwnedProp.A);
                Assert.Equal("B", root.OwnedProp.B);
            }
        }

        private static void Seed23687(MyContext23687 context)
        {
            context.Table.Add(
                new Root23687
                {
                    Id1 = 1,
                    Id2 = 11,
                    OwnedProp = new OwnedClass23687 { A = "A", B = "B" }
                });

            context.SaveChanges();
        }

        [Owned]
        private class OwnedClass23687
        {
            public string A { get; set; }
            public string B { get; set; }
        }

        private class Root23687
        {
            public int Id1 { get; set; }
            public int Id2 { get; set; }
            public OwnedClass23687 OwnedProp { get; set; }
        }

        private class MyContext23687 : DbContext
        {
            public DbSet<Root23687> Table { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                    .UseInMemoryDatabase("23687");

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Root23687>().HasKey(e => new { e.Id1, e.Id2 });
        }

        #endregion

        #region Issue23593

        [ConditionalFact]
        public virtual void Join_with_enum_as_key_selector()
        {
            using (CreateScratch<MyContext23593>(Seed23593, "23593"))
            {
                using var context = new MyContext23593();

                var query = from sm in context.StatusMaps
                            join sme in context.StatusMapEvents on sm.Id equals sme.Id
                            select sm;

                var result = Assert.Single(query);
                Assert.Equal(StatusMapCode23593.Two, result.Id);
            }
        }

        [ConditionalFact]
        public virtual void Join_with_enum_inside_anonymous_type_as_key_selector()
        {
            using (CreateScratch<MyContext23593>(Seed23593, "23593"))
            {
                using var context = new MyContext23593();

                var query = from sm in context.StatusMaps
                            join sme in context.StatusMapEvents on new { sm.Id } equals new { sme.Id }
                            select sm;

                var result = Assert.Single(query);
                Assert.Equal(StatusMapCode23593.Two, result.Id);
            }
        }

        [ConditionalFact]
        public virtual void Join_with_enum_inside_anonymous_type_with_other_property_as_key_selector()
        {
            using (CreateScratch<MyContext23593>(Seed23593, "23593"))
            {
                using var context = new MyContext23593();

                var query = from sm in context.StatusMaps
                            join sme in context.StatusMapEvents on new { sm.Id, A = 1 } equals new { sme.Id, A = 1 }
                            select sm;

                var result = Assert.Single(query);
                Assert.Equal(StatusMapCode23593.Two, result.Id);
            }
        }

        private static void Seed23593(MyContext23593 context)
        {
            context.Add(new StatusMap23593 { Id = StatusMapCode23593.One });
            context.Add(new StatusMap23593 { Id = StatusMapCode23593.Two });
            context.Add(new StatusMapEvent23593 { Id = StatusMapCode23593.Two });

            context.SaveChanges();
        }

        private enum StatusMapCode23593
        {
            One,
            Two,
            Three,
            Four
        }

        private class StatusMap23593
        {
            public StatusMapCode23593 Id { get; set; }
        }

        private class StatusMapEvent23593
        {
            public StatusMapCode23593 Id { get; set; }
        }

        private class MyContext23593 : DbContext
        {
            public DbSet<StatusMap23593> StatusMaps { get; set; }
            public DbSet<StatusMapEvent23593> StatusMapEvents { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                    .UseInMemoryDatabase("23593");
        }

        #endregion

        #region Issue23926

        [ConditionalFact]
        public virtual void Left_join_with_entity_with_enum_discriminator()
        {
            using (CreateScratch<MyContext23926>(Seed23926, "23926"))
            {
                using var context = new MyContext23926();

                var query = context.History.Select(e => e.User.Name).ToList();

                Assert.Equal(query, new[] { "UserA", "DerivedUserB", null });
            }
        }

        private static void Seed23926(MyContext23926 context)
        {
            context.Add(new History23926 { User = new User23926 { Name = "UserA" } });
            context.Add(new History23926 { User = new DerivedUser23926 { Name = "DerivedUserB" } });
            context.Add(new History23926 { User = null });

            context.SaveChanges();
        }

        private class History23926
        {
            public int Id { get; set; }
            public int? UserId { get; set; }
            public User23926 User { get; set; }
        }

        private class User23926
        {
            public int Id { get; set; }
            public UserTypes23926 Type { get; set; }
            public string Name { get; set; }
        }

        private enum UserTypes23926
        {
            User,
            DerivedUser
        }

        private class DerivedUser23926 : User23926
        {
            public string Value { get; set; }
        }

        private class MyContext23926 : DbContext
        {
            public DbSet<History23926> History { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                    .UseInMemoryDatabase("23926");

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<User23926>().HasDiscriminator(e => e.Type)
                    .HasValue<User23926>(UserTypes23926.User)
                    .HasValue<DerivedUser23926>(UserTypes23926.DerivedUser);
        }

        #endregion

        #region Issue18435

        [ConditionalFact]
        public virtual void Shared_owned_property_on_multiple_level_in_Select()
        {
            using (CreateScratch<MyContext18435>(Seed18435, "18435"))
            {
                using var context = new MyContext18435();

                var result = context.TestEntities
                    .Select(
                        x => new
                        {
                            x.Value,
                            A = x.Owned.First,
                            B = x.Owned.Second,
                            C = x.Child.Owned.First,
                            D = x.Child.Owned.Second
                        }).FirstOrDefault();

                Assert.Equal("test", result.Value);
                Assert.Equal(2, result.A);
                Assert.Equal(4, result.B);
                Assert.Equal(1, result.C);
                Assert.Equal(3, result.D);
            }
        }

        private static void Seed18435(MyContext18435 context)
        {
            context.Add(
                new RootEntity18435
                {
                    Value = "test",
                    Owned = new TestOwned18435
                    {
                        First = 2,
                        Second = 4,
                        AnotherValueType = "yay"
                    },
                    Child = new ChildEntity18435
                    {
                        Owned = new TestOwned18435
                        {
                            First = 1,
                            Second = 3,
                            AnotherValueType = "nay"
                        }
                    }
                });

            context.SaveChanges();
        }

        private class RootEntity18435
        {
            public int Id { get; set; }
            public string Value { get; set; }
            public TestOwned18435 Owned { get; set; }
            public ChildEntity18435 Child { get; set; }
        }

        private class ChildEntity18435
        {
            public int Id { get; set; }
            public string Value { get; set; }
            public TestOwned18435 Owned { get; set; }
        }

        [Owned]
        private class TestOwned18435
        {
            public int First { get; set; }
            public int Second { get; set; }
            public string AnotherValueType { get; set; }
        }

        private class MyContext18435 : DbContext
        {
            public DbSet<RootEntity18435> TestEntities { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                    .UseInMemoryDatabase("18435");
        }

        #endregion

        #region Issue19425

        [ConditionalFact(Skip = "Issue#19425")]
        public virtual void Non_nullable_cast_in_null_check()
        {
            using (CreateScratch<MyContext19425>(Seed19425, "19425"))
            {
                using var context = new MyContext19425();

                var query = (from foo in context.FooTable
                             select new { Bar = foo.Bar != null ? (Bar19425)foo.Bar : (Bar19425?)null }).ToList();

                Assert.Single(query);
            }
        }

        private static void Seed19425(MyContext19425 context)
        {
            context.FooTable.Add(new FooTable19425 { Id = 1, Bar = null });

            context.SaveChanges();
        }

        private enum Bar19425
        {
            value1,
            value2
        }

        private class FooTable19425
        {
            public int Id { get; set; }
            public byte? Bar { get; set; }
        }

        private class MyContext19425 : DbContext
        {
            public DbSet<FooTable19425> FooTable { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                    .UseInMemoryDatabase("19425");
        }

        #endregion

        #region Issue19667

        [ConditionalFact]
        public virtual void Property_access_on_nullable_converted_scalar_type()
        {
            using (CreateScratch<MyContext19667>(Seed19667, "19667"))
            {
                using var context = new MyContext19667();

                var query = context.Entities.OrderByDescending(e => e.Id).FirstOrDefault(p => p.Type.Date.Year == 2020);

                Assert.Equal(2, query.Id);
            }
        }

        private static void Seed19667(MyContext19667 context)
        {
            context.Entities.Add(new MyEntity19667 { Id = 1, Type = new MyType19667 { Date = new DateTime(2020, 1, 1) } });
            context.Entities.Add(new MyEntity19667 { Id = 2, Type = new MyType19667 { Date = new DateTime(2020, 1, 1).AddDays(1) } });

            context.SaveChanges();
        }

        private class MyEntity19667
        {
            public int Id { get; set; }

            public MyType19667 Type { get; set; }
        }

        [Owned]
        private class MyType19667
        {
            public DateTime Date { get; set; }
        }

        private class MyContext19667 : DbContext
        {
            public DbSet<MyEntity19667> Entities { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                    .UseInMemoryDatabase("19667");
        }

        #endregion

        #region Issue20359

        [ConditionalFact]
        public virtual void Changing_order_of_projection_in_anonymous_type_works()
        {
            using (CreateScratch<MyContext20359>(Seed20359, "20359"))
            {
                using var context = new MyContext20359();

                var result1 = (from r in context.Root
                               select new { r.B.BValue, r.A.Sub.AValue }).FirstOrDefault();

                var result2 = (from r in context.Root
                               select new
                               {
                                   r.A.Sub.AValue, r.B.BValue,
                               }).FirstOrDefault();

                Assert.Equal(result1.BValue, result2.BValue);
            }
        }

        private static void Seed20359(MyContext20359 context)
        {
            var root = new Root20359
            {
                A = new A20359 { Sub = new ASubClass20359 { AValue = "A Value" } }, B = new B20359 { BValue = "B Value" }
            };

            context.Add(root);

            context.SaveChanges();
        }

        private class A20359
        {
            public int Id { get; set; }

            public ASubClass20359 Sub { get; set; }
        }

        private class ASubClass20359
        {
            public string AValue { get; set; }
        }

        private class B20359
        {
            public string BValue { get; set; }
        }

        private class Root20359
        {
            public int Id { get; set; }

            public A20359 A { get; set; }
            public B20359 B { get; set; }
        }

        private class MyContext20359 : DbContext
        {
            public DbSet<Root20359> Root { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                    .UseInMemoryDatabase("20359");

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<A20359>(
                    builder =>
                    {
                        builder.OwnsOne(x => x.Sub);
                    });

                modelBuilder.Entity<Root20359>(
                    builder =>
                    {
                        builder.OwnsOne(x => x.B);
                    });
            }
        }

        #endregion

        #region Issue23360

        [ConditionalFact]
        public virtual void Union_with_different_property_name_using_same_anonymous_type()
        {
            using (CreateScratch<MyContext23360>(Seed23360, "23360"))
            {
                using var context = new MyContext23360();

                var userQuery = context.User
                    .Select(
                        u => new CommonSelectType23360
                        {
                            // 1. FirstName, 2. LastName
                            FirstName = u.Forename, LastName = u.Surname,
                        });

                var customerQuery = context.Customer
                    .Select(
                        c => new CommonSelectType23360
                        {
                            // 1. LastName, 2. FirstName
                            LastName = c.FamilyName, FirstName = c.GivenName,
                        });

                var result = userQuery.Union(customerQuery).ToList();

                Assert.Equal("Peter", result[0].FirstName);
                Assert.Equal("Smith", result[0].LastName);
                Assert.Equal("John", result[1].FirstName);
                Assert.Equal("Doe", result[1].LastName);
            }
        }

        private static void Seed23360(MyContext23360 context)
        {
            context.User.Add(
                new User23360
                {
                    Forename = "Peter", Surname = "Smith",
                });

            context.Customer.Add(
                new Customer23360
                {
                    GivenName = "John", FamilyName = "Doe",
                });

            context.SaveChanges();
        }

        private class User23360
        {
            [Key]
            public int Key { get; set; }

            public string Forename { get; set; }
            public string Surname { get; set; }
        }

        private class Customer23360
        {
            [Key]
            public int Key { get; set; }

            public string GivenName { get; set; }
            public string FamilyName { get; set; }
        }

        private class CommonSelectType23360
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }

        private class MyContext23360 : DbContext
        {
            public virtual DbSet<User23360> User { get; set; }
            public virtual DbSet<Customer23360> Customer { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                    .UseInMemoryDatabase("23360");
        }

        #endregion

        #region Issue18394

        [ConditionalFact]
        public virtual void Ordering_of_collection_result_is_correct()
        {
            using (CreateScratch<MyContext18394>(Seed18394, "18394"))
            {
                using var context = new MyContext18394();

                var myA = context.As
                    .Where(x => x.Id == 1)
                    .Select(
                        x => new ADto18394
                        {
                            Id = x.Id,
                            PropertyB = (x.PropertyB == null)
                                ? null
                                : new BDto18394
                                {
                                    Id = x.PropertyB.Id,
                                    PropertyCList = x.PropertyB.PropertyCList.Select(
                                        y => new CDto18394 { Id = y.Id, SomeText = y.SomeText }).ToList()
                                }
                        })
                    .FirstOrDefault();

                Assert.Equal("TestText", myA.PropertyB.PropertyCList.First().SomeText);
            }
        }

        private static void Seed18394(MyContext18394 context)
        {
            var a = new A18394 { PropertyB = new B18394 { PropertyCList = new List<C18394> { new() { SomeText = "TestText" } } } };
            context.As.Add(a);

            context.SaveChanges();
        }

        private class ADto18394
        {
            public int Id { get; set; }

            public BDto18394 PropertyB { get; set; }

            public int PropertyBId { get; set; }
        }

        private class BDto18394
        {
            public int Id { get; set; }

            public List<CDto18394> PropertyCList { get; set; }
        }

        private class CDto18394
        {
            public int Id { get; set; }

            public int CId { get; set; }

            public string SomeText { get; set; }
        }

        private class A18394
        {
            public int Id { get; set; }

            public B18394 PropertyB { get; set; }

            public int PropertyBId { get; set; }
        }

        private class B18394
        {
            public int Id { get; set; }

            public List<C18394> PropertyCList { get; set; }
        }

        private class C18394
        {
            public int Id { get; set; }

            public int BId { get; set; }

            public string SomeText { get; set; }

            public B18394 B { get; set; }
        }

        private class MyContext18394 : DbContext
        {
            public DbSet<A18394> As { get; set; }
            public DbSet<B18394> Bs { get; set; }
            public DbSet<C18394> Cs { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                    .UseInMemoryDatabase("18394");
        }

        #endregion

        #region Issue23934

        [ConditionalFact]
        public virtual void Owned_entity_indexes_are_maintained_properly()
        {
            using (CreateScratch<MyContext23934>(Seed23934, "23934"))
            {
                using var context = new MyContext23934();

                var criteria = new DateTime(2020, 1, 1);

                var data = context.Outers.Where(x => x.OwnedProp.At >= criteria || x.Inner.OwnedProp.At >= criteria).ToList();

                Assert.Single(data);
            }
        }

        private static void Seed23934(MyContext23934 context)
        {
            var inner = new Inner23934 { Id = Guid.NewGuid(), OwnedProp = new OwnedClass23934 { At = new DateTime(2020, 1, 1) } };

            var outer = new Outer23934
            {
                Id = Guid.NewGuid(),
                OwnedProp = new OwnedClass23934 { At = new DateTime(2020, 1, 1) },
                InnerId = inner.Id
            };

            context.Inners.Add(inner);
            context.Outers.Add(outer);

            context.SaveChanges();
        }

        private class Outer23934
        {
            public Guid Id { get; set; }
            public OwnedClass23934 OwnedProp { get; set; }
            public Guid InnerId { get; set; }
            public Inner23934 Inner { get; set; }
        }

        private class Inner23934
        {
            public Guid Id { get; set; }
            public OwnedClass23934 OwnedProp { get; set; }
        }

        [Owned]
        private class OwnedClass23934
        {
            public DateTime At { get; set; }
        }

        private class MyContext23934 : DbContext
        {
            public DbSet<Outer23934> Outers { get; set; }

            public DbSet<Inner23934> Inners { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                    .UseInMemoryDatabase("23934");
        }

        #endregion

        #region SharedHelper

        private static InMemoryTestStore CreateScratch<TContext>(Action<TContext> seed, string databaseName)
            where TContext : DbContext, new()
            => InMemoryTestStore.GetOrCreate(databaseName)
                .InitializeInMemory(null, () => new TContext(), c => seed((TContext)c));

        #endregion
    }
}
