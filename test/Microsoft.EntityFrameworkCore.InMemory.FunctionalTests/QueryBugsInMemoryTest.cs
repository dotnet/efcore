// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.InMemory.FunctionalTests
{
    class QueryBugsInMemoryTest : IClassFixture<InMemoryFixture>
    {
        [Fact]
        public void GroupBy_with_uninitialized_datetime_projection_3595()
        {
            using (CreateScratch<Context3595>(Seed3595))
            {
                using (var context = new Context3595())
                {
                    var q0 = from instance in context.Exams
                             join question in context.ExamQuestions
                                on instance.Id equals question.ExamId
                             where instance.Id != 3
                             group question by question.QuestionId into gQuestions
                             select new
                             {
                                 gQuestions.Key,
                                 MaxDate = gQuestions.Max(q => q.Modified)
                             };

                    var result = q0.ToList();

                    Assert.Equal(default(DateTime), result.Single().MaxDate);
                }
            }
        }

        private static void Seed3595(Context3595 context)
        {
            var question = new Question3595();
            var examInstance = new Exam3595();
            var examInstanceQuestion = new ExamQuestion3595
            {
                Question = question,
                Exam = examInstance
            };

            context.Add(question);
            context.Add(examInstance);
            context.Add(examInstanceQuestion);
            context.SaveChanges();
        }

        public abstract class Base3595
        {
            public DateTime Modified { get; set; }
        }

        public class Question3595 : Base3595
        {
            public int Id { get; set; }
        }

        public class Exam3595 : Base3595
        {
            public int Id { get; set; }
        }

        public class ExamQuestion3595 : Base3595
        {
            public int Id { get; set; }

            public int QuestionId { get; set; }
            public Question3595 Question { get; set; }

            public int ExamId { get; set; }
            public Exam3595 Exam { get; set; }
        }

        public class Context3595 : DbContext
        {
            public DbSet<Exam3595> Exams { get; set; }
            public DbSet<Question3595> Questions { get; set; }
            public DbSet<ExamQuestion3595> ExamQuestions { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseInMemoryDatabase();
        }

        [Fact]
        public virtual void Repro3101_simple_coalesce1()
        {
            using (CreateScratch<MyContext3101>(Seed3101))
            {
                using (var ctx = new MyContext3101())
                {
                    var query = from eVersion in ctx.Entities
                                join eRoot in ctx.Entities.Include(e => e.Children).AsNoTracking()
                                on eVersion.RootEntityId equals (int?)eRoot.Id
                                into RootEntities
                                from eRootJoined in RootEntities.DefaultIfEmpty()
                                select eRootJoined ?? eVersion;

                    var result = query.ToList();
                }
            }
        }

        [Fact]
        public virtual void Repro3101_simple_coalesce2()
        {
            using (CreateScratch<MyContext3101>(Seed3101))
            {
                using (var ctx = new MyContext3101())
                {
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
        }

        [Fact]
        public virtual void Repro3101_simple_coalesce3()
        {
            using (CreateScratch<MyContext3101>(Seed3101))
            {
                using (var ctx = new MyContext3101())
                {
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
        }

        [Fact]
        public virtual void Repro3101_complex_coalesce1()
        {
            using (CreateScratch<MyContext3101>(Seed3101))
            {
                using (var ctx = new MyContext3101())
                {
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
        }

        [Fact]
        public virtual void Repro3101_complex_coalesce2()
        {
            using (CreateScratch<MyContext3101>(Seed3101))
            {
                using (var ctx = new MyContext3101())
                {
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
        }

        [Fact]
        public virtual void Repro3101_nested_coalesce1()
        {
            using (CreateScratch<MyContext3101>(Seed3101))
            {
                using (var ctx = new MyContext3101())
                {
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
        }

        [Fact]
        public virtual void Repro3101_nested_coalesce2()
        {
            using (CreateScratch<MyContext3101>(Seed3101))
            {
                using (var ctx = new MyContext3101())
                {
                    var query = from eVersion in ctx.Entities.Include(e => e.Children)
                                join eRoot in ctx.Entities
                                on eVersion.RootEntityId equals (int?)eRoot.Id
                                into RootEntities
                                from eRootJoined in RootEntities.DefaultIfEmpty()
                                select new { One = eRootJoined, Two = 2, Coalesce = eRootJoined ?? (eVersion ?? eRootJoined) };

                    var result = query.ToList();
                    Assert.True(result.All(e => e.Coalesce.Children.Count > 0));
                }
            }
        }

        [Fact]
        public virtual void Repro3101_conditional()
        {
            using (CreateScratch<MyContext3101>(Seed3101))
            {
                using (var ctx = new MyContext3101())
                {
                    var query = from eVersion in ctx.Entities.Include(e => e.Children)
                                join eRoot in ctx.Entities
                                on eVersion.RootEntityId equals (int?)eRoot.Id
                                into RootEntities
                                from eRootJoined in RootEntities.DefaultIfEmpty()
                                select eRootJoined != null ? eRootJoined : eVersion;

                    var result = query.ToList();
                    Assert.True(result.All(e => e.Children.Count > 0));
                }
            }
        }

        [Fact]
        public virtual void Repro3101_coalesce_tracking()
        {
            using (CreateScratch<MyContext3101>(Seed3101))
            {
                using (var ctx = new MyContext3101())
                {
                    var query = from eVersion in ctx.Entities
                                join eRoot in ctx.Entities
                                on eVersion.RootEntityId equals (int?)eRoot.Id
                                into RootEntities
                                from eRootJoined in RootEntities.DefaultIfEmpty()
                                select new { eRootJoined, eVersion, foo = eRootJoined ?? eVersion };

                    var result = query.ToList();

                    var foo = ctx.ChangeTracker.Entries().ToList();
                    Assert.True(ctx.ChangeTracker.Entries().Count() > 0);
                }
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

        public class MyContext3101 : DbContext
        {
            public DbSet<Entity3101> Entities { get; set; }

            public DbSet<Child3101> Children { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseInMemoryDatabase();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Entity3101>().Property(e => e.Id).ValueGeneratedNever();
            }
        }

        public class Entity3101
        {
            public Entity3101()
            {
                this.Children = new Collection<Child3101>();
            }

            public int Id { get; set; }

            public int? RootEntityId { get; set; }

            public Entity3101 RootEntity { get; set; }

            public ICollection<Child3101> Children { get; set; }
        }

        public class Child3101
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private static InMemoryTestStore CreateScratch<TContext>(Action<TContext> Seed)
            where TContext : DbContext, new()
            => InMemoryTestStore.CreateScratch(
                () =>
                    {
                        using (var context = new TContext())
                        {
                            Seed(context);
                        }
                    },
                () =>
                    {
                        using (var context = new Context3595())
                        {
                            context.GetInfrastructure().GetRequiredService<IInMemoryStoreSource>().GetGlobalStore().Clear();
                        }
                    });
    }
}
