// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class FailedLoadingTest : IClassFixture<FailedLoadingTest.FailedLoadingFixture>
    {
        public FailedLoadingTest(FailedLoadingFixture fixture)
            => Fixture = fixture;

        public FailedLoadingFixture Fixture { get; }

        [ConditionalFact]
        public void IsLoaded_is_not_set_if_loading_principal_collection_fails()
        {
            using var context = Fixture.CreateContext();

            var principal = context.Set<PrincipalProxy>().Single();
            Assert.False(context.Entry(principal).Collection(e => e.Dependents).IsLoaded);

            Fixture.Interceptor.Throw = true;

            Assert.Equal("Bang!", Assert.Throws<Exception>(() => principal.Dependents).Message);
            Assert.False(context.Entry(principal).Collection(e => e.Dependents).IsLoaded);

            Fixture.Interceptor.Throw = false;

            Assert.Single(principal.Dependents);
            Assert.True(context.Entry(principal).Collection(e => e.Dependents).IsLoaded);
        }

        [ConditionalFact]
        public void IsLoaded_is_not_set_if_loading_principal_single_reference_fails()
        {
            using var context = Fixture.CreateContext();

            var principal = context.Set<PrincipalProxy>().Single();
            Assert.False(context.Entry(principal).Reference(e => e.SingleDependent).IsLoaded);

            Fixture.Interceptor.Throw = true;

            Assert.Equal("Bang!", Assert.Throws<Exception>(() => principal.SingleDependent).Message);
            Assert.False(context.Entry(principal).Reference(e => e.SingleDependent).IsLoaded);

            Fixture.Interceptor.Throw = false;

            Assert.NotNull(principal.SingleDependent);
            Assert.True(context.Entry(principal).Reference(e => e.SingleDependent).IsLoaded);
        }

        [ConditionalFact]
        public void IsLoaded_is_not_set_if_loading_many_to_many_collection_fails()
        {
            using var context = Fixture.CreateContext();

            var principal = context.Set<PrincipalProxy>().Single();
            Assert.False(context.Entry(principal).Collection(e => e.ManyDependents).IsLoaded);

            Fixture.Interceptor.Throw = true;

            Assert.Equal("Bang!", Assert.Throws<Exception>(() => principal.ManyDependents).Message);
            Assert.False(context.Entry(principal).Collection(e => e.ManyDependents).IsLoaded);

            Fixture.Interceptor.Throw = false;

            Assert.Single(principal.ManyDependents);
            Assert.True(context.Entry(principal).Collection(e => e.ManyDependents).IsLoaded);
        }

        [ConditionalFact]
        public void IsLoaded_is_not_set_if_loading_dependent_single_reference_fails()
        {
            using var context = Fixture.CreateContext();

            var principal = context.Set<DependentProxy>().Single();
            Assert.False(context.Entry(principal).Reference(e => e.SinglePrincipal).IsLoaded);

            Fixture.Interceptor.Throw = true;

            Assert.Equal("Bang!", Assert.Throws<Exception>(() => principal.SinglePrincipal).Message);
            Assert.False(context.Entry(principal).Reference(e => e.SinglePrincipal).IsLoaded);

            Fixture.Interceptor.Throw = false;

            Assert.NotNull(principal.SinglePrincipal);
            Assert.True(context.Entry(principal).Reference(e => e.SinglePrincipal).IsLoaded);
        }

        [ConditionalFact]
        public void IsLoaded_is_not_set_if_loading_dependent_collection_reference_fails()
        {
            using var context = Fixture.CreateContext();

            var principal = context.Set<DependentProxy>().Single();
            Assert.False(context.Entry(principal).Reference(e => e.Principal).IsLoaded);

            Fixture.Interceptor.Throw = true;

            Assert.Equal("Bang!", Assert.Throws<Exception>(() => principal.Principal).Message);
            Assert.False(context.Entry(principal).Reference(e => e.Principal).IsLoaded);

            Fixture.Interceptor.Throw = false;

            Assert.NotNull(principal.Principal);
            Assert.True(context.Entry(principal).Reference(e => e.Principal).IsLoaded);
        }

        public class PrincipalProxy
        {
            public int Id { get; set; }

            public virtual ICollection<DependentProxy> Dependents { get; } = new List<DependentProxy>();
            public virtual DependentProxy SingleDependent { get; set; }
            public virtual ICollection<DependentProxy> ManyDependents { get; } = new List<DependentProxy>();
        }

        public class DependentProxy
        {
            public int Id { get; set; }

            public virtual PrincipalProxy Principal { get; set; }
            public virtual PrincipalProxy SinglePrincipal { get; set; }
            public virtual ICollection<PrincipalProxy> ManyPrincipals { get; } = new List<PrincipalProxy>();
        }

        public class ThrowingInterceptor : DbCommandInterceptor
        {
            public bool Throw { get; set; }

            public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
            {
                if (Throw)
                {
                    throw new Exception("Bang!");
                }

                return base.ReaderExecuting(command, eventData, result);
            }
        }

        public class FailedLoadingFixture : SharedStoreFixtureBase<PoolableDbContext>
        {
            public ThrowingInterceptor Interceptor { get; } = new ThrowingInterceptor();

            protected override ITestStoreFactory TestStoreFactory
                => SqliteTestStoreFactory.Instance;

            protected override string StoreName
                => "FailedLoading";

            protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
                => base.AddServices(serviceCollection.AddEntityFrameworkProxies());

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => base.AddOptions(builder.UseLazyLoadingProxies().AddInterceptors(Interceptor));

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                modelBuilder.Entity<PrincipalProxy>(
                    b =>
                    {
                        b.HasMany(e => e.Dependents).WithOne(e => e.Principal);
                        b.HasOne(e => e.SingleDependent).WithOne(e => e.SinglePrincipal).HasForeignKey<DependentProxy>("SinglePrincipalId");
                        b.HasMany(e => e.ManyDependents).WithMany(e => e.ManyPrincipals);
                    });
            }

            protected override void Seed(PoolableDbContext context)
            {
                var dependent = new DependentProxy();

                context.Add(new PrincipalProxy
                {
                    Dependents = { dependent },
                    ManyDependents = { dependent },
                    SingleDependent = dependent
                });

                context.SaveChanges();
            }
        }
    }
}
