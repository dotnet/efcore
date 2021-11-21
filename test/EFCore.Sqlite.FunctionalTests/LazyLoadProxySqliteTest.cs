// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore;

public class LazyLoadProxySqliteTest : LazyLoadProxyTestBase<LazyLoadProxySqliteTest.LoadSqliteFixture>
{
    public LazyLoadProxySqliteTest(LoadSqliteFixture fixture)
        : base(fixture)
    {
    }

    [ConditionalFact]
    public void IsLoaded_is_not_set_if_loading_principal_collection_fails()
    {
        using var context = Fixture.CreateContext();

        var principal = context.Set<Parent>().Single();
        Assert.False(context.Entry(principal).Collection(e => e.Children).IsLoaded);

        Fixture.Interceptor.Throw = true;

        Assert.Equal("Bang!", Assert.Throws<Exception>(() => principal.Children).Message);
        Assert.False(context.Entry(principal).Collection(e => e.Children).IsLoaded);

        Fixture.Interceptor.Throw = false;

        Assert.NotEmpty(principal.Children);
        Assert.True(context.Entry(principal).Collection(e => e.Children).IsLoaded);
    }

    [ConditionalFact]
    public void IsLoaded_is_not_set_if_loading_principal_single_reference_fails()
    {
        using var context = Fixture.CreateContext();

        var principal = context.Set<Parent>().Single();
        Assert.False(context.Entry(principal).Reference(e => e.Single).IsLoaded);

        Fixture.Interceptor.Throw = true;

        Assert.Equal("Bang!", Assert.Throws<Exception>(() => principal.Single).Message);
        Assert.False(context.Entry(principal).Reference(e => e.Single).IsLoaded);

        Fixture.Interceptor.Throw = false;

        Assert.NotNull(principal.Single);
        Assert.True(context.Entry(principal).Reference(e => e.Single).IsLoaded);
    }

    [ConditionalFact]
    public void IsLoaded_is_not_set_if_loading_many_to_many_collection_fails()
    {
        using var context = Fixture.CreateContext();

        var principal = context.Set<Parent>().Single();
        Assert.False(context.Entry(principal).Collection(e => e.ManyChildren).IsLoaded);

        Fixture.Interceptor.Throw = true;

        Assert.Equal("Bang!", Assert.Throws<Exception>(() => principal.ManyChildren).Message);
        Assert.False(context.Entry(principal).Collection(e => e.ManyChildren).IsLoaded);

        Fixture.Interceptor.Throw = false;

        Assert.NotEmpty(principal.ManyChildren);
        Assert.True(context.Entry(principal).Collection(e => e.ManyChildren).IsLoaded);
    }

    [ConditionalFact]
    public void IsLoaded_is_not_set_if_loading_dependent_single_reference_fails()
    {
        using var context = Fixture.CreateContext();

        var dependent = context.Set<Single>().OrderBy(e => e.Id).First();
        Assert.False(context.Entry(dependent).Reference(e => e.Parent).IsLoaded);

        Fixture.Interceptor.Throw = true;

        Assert.Equal("Bang!", Assert.Throws<Exception>(() => dependent.Parent).Message);
        Assert.False(context.Entry(dependent).Reference(e => e.Parent).IsLoaded);

        Fixture.Interceptor.Throw = false;

        Assert.NotNull(dependent.Parent);
        Assert.True(context.Entry(dependent).Reference(e => e.Parent).IsLoaded);
    }

    [ConditionalFact]
    public void IsLoaded_is_not_set_if_loading_dependent_collection_reference_fails()
    {
        using var context = Fixture.CreateContext();

        var dependent = context.Set<Child>().OrderBy(e => e.Id).First();
        Assert.False(context.Entry(dependent).Reference(e => e.Parent).IsLoaded);

        Fixture.Interceptor.Throw = true;

        Assert.Equal("Bang!", Assert.Throws<Exception>(() => dependent.Parent).Message);
        Assert.False(context.Entry(dependent).Reference(e => e.Parent).IsLoaded);

        Fixture.Interceptor.Throw = false;

        Assert.NotNull(dependent.Parent);
        Assert.True(context.Entry(dependent).Reference(e => e.Parent).IsLoaded);
    }

    public class ThrowingInterceptor : DbCommandInterceptor
    {
        public bool Throw { get; set; }

        public override InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result)
        {
            if (Throw)
            {
                throw new Exception("Bang!");
            }

            return base.ReaderExecuting(command, eventData, result);
        }
    }

    public class LoadSqliteFixture : LoadFixtureBase
    {
        public ThrowingInterceptor Interceptor { get; } = new();

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder.UseLazyLoadingProxies().AddInterceptors(Interceptor));

        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;
    }
}
