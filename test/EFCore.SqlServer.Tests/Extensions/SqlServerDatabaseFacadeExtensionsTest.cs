// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider;

namespace Microsoft.EntityFrameworkCore;

public class SqlServerDatabaseFacadeExtensionsTest
{
    [ConditionalFact]
    public void Returns_appropriate_name()
        => Assert.Equal(
            typeof(SqlServerConnection).Assembly.GetName().Name,
            new DatabaseProvider<SqlServerOptionsExtension>(new DatabaseProviderDependencies()).Name);

    [ConditionalFact]
    public void Is_configured_when_configuration_contains_associated_extension()
    {
        var optionsBuilder = new DbContextOptionsBuilder();
        optionsBuilder.UseSqlServer("Database=Crunchie");

        Assert.True(
            new DatabaseProvider<SqlServerOptionsExtension>(new DatabaseProviderDependencies()).IsConfigured(optionsBuilder.Options));
    }

    [ConditionalFact]
    public void Is_not_configured_when_configuration_does_not_contain_associated_extension()
    {
        var optionsBuilder = new DbContextOptionsBuilder();

        Assert.False(
            new DatabaseProvider<SqlServerOptionsExtension>(new DatabaseProviderDependencies()).IsConfigured(optionsBuilder.Options));
    }

    [ConditionalFact]
    public void Default_value_for_CommandTimeout_is_null_and_can_be_changed_including_setting_to_null()
    {
        using var context = new TimeoutContext();
        Assert.Null(context.Database.GetCommandTimeout());

        context.Database.SetCommandTimeout(77);
        Assert.Equal(77, context.Database.GetCommandTimeout());

        context.Database.SetCommandTimeout(null);
        Assert.Null(context.Database.GetCommandTimeout());

        context.Database.SetCommandTimeout(TimeSpan.FromSeconds(66));
        Assert.Equal(66, context.Database.GetCommandTimeout());
    }

    [ConditionalFact]
    public void Setting_CommandTimeout_to_infinite_sets_to_zero()
    {
        using var context = new TimeoutContext();

        context.Database.SetCommandTimeout(Timeout.InfiniteTimeSpan);
        Assert.Equal(0, context.Database.GetCommandTimeout());
    }

    [ConditionalFact]
    public void Setting_CommandTimeout_to_negative_value_throws()
    {
        Assert.Throws<InvalidOperationException>(
            () => new DbContextOptionsBuilder().UseSqlServer(
                "No=LoveyDovey",
                b => b.CommandTimeout(-55)));

        using var context = new TimeoutContext();
        Assert.Null(context.Database.GetCommandTimeout());

        Assert.Throws<ArgumentException>(
            () => context.Database.SetCommandTimeout(-3));
        Assert.Throws<ArgumentException>(
            () => context.Database.SetCommandTimeout(TimeSpan.FromSeconds(-3)));

        Assert.Throws<ArgumentException>(
            () => context.Database.SetCommandTimeout(-99));
        Assert.Throws<ArgumentException>(
            () => context.Database.SetCommandTimeout(TimeSpan.FromSeconds(-99)));

        Assert.Throws<ArgumentException>(
            () => context.Database.SetCommandTimeout(TimeSpan.FromSeconds(uint.MaxValue)));
    }

    public class TimeoutContext : DbContext
    {
        public TimeoutContext()
        {
        }

        public TimeoutContext(int? commandTimeout)
        {
            Database.SetCommandTimeout(commandTimeout);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInternalServiceProvider(SqlServerFixture.DefaultServiceProvider)
                .UseSqlServer(new FakeDbConnection("A=B"));
    }

    [ConditionalFact]
    public void IsSqlServer_when_using_OnConfiguring()
    {
        using var context = new SqlServerOnConfiguringContext();
        Assert.True(context.Database.IsSqlServer());
    }

    [ConditionalFact]
    public void IsSqlServer_in_OnModelCreating_when_using_OnConfiguring()
    {
        using var context = new SqlServerOnModelContext();
        var _ = context.Model; // Trigger context initialization
        Assert.True(context.IsSqlServerSet);
    }

    [ConditionalFact]
    public void IsRelational_in_OnModelCreating_when_using_OnConfiguring()
    {
        using var context = new RelationalOnModelContext();
        var _ = context.Model; // Trigger context initialization
        Assert.True(context.IsSqlServerSet);
    }

    [ConditionalFact]
    public void IsSqlServer_in_constructor_when_using_OnConfiguring()
    {
        using var context = new SqlServerConstructorContext();
        var _ = context.Model; // Trigger context initialization
        Assert.True(context.IsSqlServerSet);
    }

    [ConditionalFact]
    public void Cannot_use_IsSqlServer_in_OnConfiguring()
    {
        using var context = new SqlServerUseInOnConfiguringContext();
        Assert.Equal(
            CoreStrings.RecursiveOnConfiguring,
            Assert.Throws<InvalidOperationException>(
                () =>
                {
                    var _ = context.Model; // Trigger context initialization
                }).Message);
    }

    [ConditionalFact]
    public void IsSqlServer_when_using_constructor()
    {
        using var context = new ProviderContext(
            new DbContextOptionsBuilder()
                .UseInternalServiceProvider(SqlServerFixture.DefaultServiceProvider)
                .UseSqlServer("Database=Maltesers").Options);
        Assert.True(context.Database.IsSqlServer());
    }

    [ConditionalFact]
    public void IsSqlServer_in_OnModelCreating_when_using_constructor()
    {
        using var context = new ProviderOnModelContext(
            new DbContextOptionsBuilder()
                .UseInternalServiceProvider(SqlServerFixture.DefaultServiceProvider)
                .UseSqlServer("Database=Maltesers").Options);
        var _ = context.Model; // Trigger context initialization
        Assert.True(context.IsSqlServerSet);
    }

    [ConditionalFact]
    public void IsSqlServer_in_constructor_when_using_constructor()
    {
        using var context = new ProviderConstructorContext(
            new DbContextOptionsBuilder()
                .UseInternalServiceProvider(SqlServerFixture.DefaultServiceProvider)
                .UseSqlServer("Database=Maltesers").Options);
        var _ = context.Model; // Trigger context initialization
        Assert.True(context.IsSqlServerSet);
    }

    [ConditionalFact]
    public void Cannot_use_IsSqlServer_in_OnConfiguring_with_constructor()
    {
        using var context = new ProviderUseInOnConfiguringContext(
            new DbContextOptionsBuilder()
                .UseInternalServiceProvider(SqlServerFixture.DefaultServiceProvider)
                .UseSqlServer("Database=Maltesers").Options);
        Assert.Equal(
            CoreStrings.RecursiveOnConfiguring,
            Assert.Throws<InvalidOperationException>(
                () =>
                {
                    var _ = context.Model; // Trigger context initialization
                }).Message);
    }

    [ConditionalFact]
    public void Not_IsSqlServer_when_using_different_provider()
    {
        using var context = new ProviderContext(
            new DbContextOptionsBuilder()
                .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                .UseInMemoryDatabase("Maltesers").Options);
        Assert.False(context.Database.IsSqlServer());
    }

    private class ProviderContext : DbContext
    {
        protected ProviderContext()
        {
        }

        public ProviderContext(DbContextOptions options)
            : base(options)
        {
        }

        public bool? IsSqlServerSet { get; protected set; }
    }

    private class SqlServerOnConfiguringContext : ProviderContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInternalServiceProvider(SqlServerFixture.DefaultServiceProvider)
                .UseSqlServer("Database=Maltesers");
    }

    private class SqlServerOnModelContext : SqlServerOnConfiguringContext
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => IsSqlServerSet = Database.IsSqlServer();
    }

    private class RelationalOnModelContext : SqlServerOnConfiguringContext
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => IsSqlServerSet = Database.IsRelational();
    }

    private class SqlServerConstructorContext : SqlServerOnConfiguringContext
    {
        public SqlServerConstructorContext()
        {
            IsSqlServerSet = Database.IsSqlServer();
        }
    }

    private class SqlServerUseInOnConfiguringContext : SqlServerOnConfiguringContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            IsSqlServerSet = Database.IsSqlServer();
        }
    }

    private class ProviderOnModelContext(DbContextOptions options) : ProviderContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => IsSqlServerSet = Database.IsSqlServer();
    }

    private class ProviderConstructorContext : ProviderContext
    {
        public ProviderConstructorContext(DbContextOptions options)
            : base(options)
        {
            IsSqlServerSet = Database.IsSqlServer();
        }
    }

    private class ProviderUseInOnConfiguringContext(DbContextOptions options) : ProviderContext(options)
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => IsSqlServerSet = Database.IsSqlServer();
    }
}
