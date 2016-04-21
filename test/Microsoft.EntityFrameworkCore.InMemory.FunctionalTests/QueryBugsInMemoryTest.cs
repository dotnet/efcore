// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
