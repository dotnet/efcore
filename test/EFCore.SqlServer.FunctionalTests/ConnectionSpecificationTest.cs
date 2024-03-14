// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class ConnectionSpecificationTest
{
    [ConditionalFact]
    public async Task Can_specify_no_connection_string_in_OnConfiguring()
    {
        var serviceProvider
            = new ServiceCollection()
                .AddDbContext<NoneInOnConfiguringContext>()
                .BuildServiceProvider(validateScopes: true);

        using (await SqlServerTestStore.GetNorthwindStoreAsync())
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<NoneInOnConfiguringContext>();

            context.Database.SetConnectionString(SqlServerNorthwindTestStoreFactory.NorthwindConnectionString);

            Assert.True(await context.Customers.AnyAsync());
        }
    }

    [ConditionalFact]
    public async Task Can_specify_no_connection_string_in_OnConfiguring_with_default_service_provider()
    {
        using (await SqlServerTestStore.GetNorthwindStoreAsync())
        {
            using var context = new NoneInOnConfiguringContext();

            context.Database.SetConnectionString(SqlServerNorthwindTestStoreFactory.NorthwindConnectionString);

            Assert.True(await context.Customers.AnyAsync());
        }
    }

    [ConditionalFact]
    public async Task Throws_if_context_used_with_no_connection_or_connection_string()
    {
        using (await SqlServerTestStore.GetNorthwindStoreAsync())
        {
            using var context = new NoneInOnConfiguringContext();

            await Assert.ThrowsAsync<InvalidOperationException>(() => context.Customers.AnyAsync());
        }
    }

    private class NoneInOnConfiguringContext : NorthwindContextBase
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .EnableServiceProviderCaching(false)
                .UseSqlServer(b => b.ApplyConfiguration());
    }

    [ConditionalFact]
    public async Task Can_specify_connection_string_in_OnConfiguring()
    {
        var serviceProvider
            = new ServiceCollection()
                .AddDbContext<StringInOnConfiguringContext>()
                .BuildServiceProvider(validateScopes: true);

        using (await SqlServerTestStore.GetNorthwindStoreAsync())
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<StringInOnConfiguringContext>();
            Assert.True(await context.Customers.AnyAsync());
        }
    }

    [ConditionalFact]
    public async Task Can_specify_connection_string_in_OnConfiguring_with_default_service_provider()
    {
        using (await SqlServerTestStore.GetNorthwindStoreAsync())
        {
            using var context = new StringInOnConfiguringContext();
            Assert.True(await context.Customers.AnyAsync());
        }
    }

    private class StringInOnConfiguringContext : NorthwindContextBase
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .EnableServiceProviderCaching(false)
                .UseSqlServer(SqlServerNorthwindTestStoreFactory.NorthwindConnectionString, b => b.ApplyConfiguration());
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Can_specify_no_connection_in_OnConfiguring(bool contextOwnsConnection)
    {
        var serviceProvider
            = new ServiceCollection()
                .AddScoped(p => new SqlConnection(SqlServerNorthwindTestStoreFactory.NorthwindConnectionString))
                .AddDbContext<NoneInOnConfiguringContext>().BuildServiceProvider(validateScopes: true);

        SqlConnection connection;

        using (await SqlServerTestStore.GetNorthwindStoreAsync())
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<NoneInOnConfiguringContext>();

            connection = new SqlConnection(SqlServerNorthwindTestStoreFactory.NorthwindConnectionString);
            context.Database.SetDbConnection(connection, contextOwnsConnection);

            Assert.True(await context.Customers.AnyAsync());
        }

        if (contextOwnsConnection)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() => connection.OpenAsync()); // Disposed
        }
        else
        {
            await connection.OpenAsync();
            await connection.CloseAsync();
            await connection.DisposeAsync();
        }
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Can_specify_no_connection_in_OnConfiguring_with_default_service_provider(bool contextOwnsConnection)
    {
        SqlConnection connection;

        using (await SqlServerTestStore.GetNorthwindStoreAsync())
        {
            using var context = new NoneInOnConfiguringContext();

            connection = new SqlConnection(SqlServerNorthwindTestStoreFactory.NorthwindConnectionString);
            context.Database.SetDbConnection(connection, contextOwnsConnection);

            Assert.True(await context.Customers.AnyAsync());
        }

        if (contextOwnsConnection)
        {
            Assert.Throws<InvalidOperationException>(() => connection.Open()); // Disposed
        }
        else
        {
            connection.Open();
            connection.Close();
            connection.Dispose();
        }
    }

    [ConditionalFact]
    public async Task Can_specify_connection_in_OnConfiguring()
    {
        var serviceProvider
            = new ServiceCollection()
                .AddScoped(p => new SqlConnection(SqlServerNorthwindTestStoreFactory.NorthwindConnectionString))
                .AddDbContext<ConnectionInOnConfiguringContext>().BuildServiceProvider(validateScopes: true);

        using (await SqlServerTestStore.GetNorthwindStoreAsync())
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ConnectionInOnConfiguringContext>();
            Assert.True(await context.Customers.AnyAsync());
        }
    }

    [ConditionalFact]
    public async Task Can_specify_connection_in_OnConfiguring_with_default_service_provider()
    {
        using (await SqlServerTestStore.GetNorthwindStoreAsync())
        {
            using var connection = new SqlConnection(SqlServerNorthwindTestStoreFactory.NorthwindConnectionString);
            using var context = new ConnectionInOnConfiguringContext(connection);

            Assert.True(await context.Customers.AnyAsync());
        }
    }

    [ConditionalFact]
    public async Task Can_specify_owned_connection_in_OnConfiguring()
    {
        var serviceProvider
            = new ServiceCollection()
                .AddSingleton(_ => new SqlConnection(SqlServerNorthwindTestStoreFactory.NorthwindConnectionString))
                .AddDbContext<OwnedConnectionInOnConfiguringContext>().BuildServiceProvider(validateScopes: true);

        SqlConnection connection;

        using (await SqlServerTestStore.GetNorthwindStoreAsync())
        {
            connection = serviceProvider.GetRequiredService<SqlConnection>();

            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<OwnedConnectionInOnConfiguringContext>();
            Assert.True(await context.Customers.AnyAsync());
        }

        Assert.Throws<InvalidOperationException>(() => connection.Open()); // Disposed
    }

    [ConditionalFact]
    public async Task Can_specify_owned_connection_in_OnConfiguring_with_default_service_provider()
    {
        SqlConnection connection;

        using (await SqlServerTestStore.GetNorthwindStoreAsync())
        {
            connection = new SqlConnection(SqlServerNorthwindTestStoreFactory.NorthwindConnectionString);
            using var context = new OwnedConnectionInOnConfiguringContext(connection);

            Assert.True(await context.Customers.AnyAsync());
        }

        Assert.Throws<InvalidOperationException>(() => connection.Open()); // Disposed
    }

    [ConditionalFact]
    public async Task Can_specify_then_change_connection()
    {
        var connection = new SqlConnection(SqlServerNorthwindTestStoreFactory.NorthwindConnectionString);

        var serviceProvider
            = new ServiceCollection()
                .AddScoped(p => connection)
                .AddDbContext<ConnectionInOnConfiguringContext>().BuildServiceProvider(validateScopes: true);

        using (await SqlServerTestStore.GetNorthwindStoreAsync())
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ConnectionInOnConfiguringContext>();

            Assert.Same(connection, context.Database.GetDbConnection());
            Assert.True(await context.Customers.AnyAsync());

            using var newConnection = new SqlConnection(SqlServerNorthwindTestStoreFactory.NorthwindConnectionString);
            context.Database.SetDbConnection(newConnection);

            Assert.Same(newConnection, context.Database.GetDbConnection());
            Assert.True(await context.Customers.AnyAsync());
        }
    }

    [ConditionalFact]
    public async Task Cannot_change_connection_when_open_and_owned()
    {
        var connection = new SqlConnection(SqlServerNorthwindTestStoreFactory.NorthwindConnectionString);

        var serviceProvider
            = new ServiceCollection()
                .AddScoped(p => connection)
                .AddDbContext<OwnedConnectionInOnConfiguringContext>().BuildServiceProvider(validateScopes: true);

        using (await SqlServerTestStore.GetNorthwindStoreAsync())
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<OwnedConnectionInOnConfiguringContext>();

            context.Database.OpenConnection();
            Assert.Same(connection, context.Database.GetDbConnection());
            Assert.True(await context.Customers.AnyAsync());

            using var newConnection = new SqlConnection(SqlServerNorthwindTestStoreFactory.NorthwindConnectionString);

            Assert.Equal(
                RelationalStrings.CannotChangeWhenOpen,
                Assert.Throws<InvalidOperationException>(() => context.Database.SetDbConnection(newConnection)).Message);
        }
    }

    [ConditionalFact]
    public async Task Can_change_connection_when_open_and_not_owned()
    {
        var connection = new SqlConnection(SqlServerNorthwindTestStoreFactory.NorthwindConnectionString);

        var serviceProvider
            = new ServiceCollection()
                .AddScoped(p => connection)
                .AddDbContext<ConnectionInOnConfiguringContext>().BuildServiceProvider(validateScopes: true);

        using (await SqlServerTestStore.GetNorthwindStoreAsync())
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ConnectionInOnConfiguringContext>();

            context.Database.OpenConnection();
            Assert.Same(connection, context.Database.GetDbConnection());
            Assert.True(await context.Customers.AnyAsync());

            using var newConnection = new SqlConnection(SqlServerNorthwindTestStoreFactory.NorthwindConnectionString);
            context.Database.SetDbConnection(newConnection);

            Assert.Same(newConnection, context.Database.GetDbConnection());
            Assert.True(await context.Customers.AnyAsync());
        }
    }

    private class ConnectionInOnConfiguringContext(SqlConnection connection) : NorthwindContextBase
    {
        private readonly SqlConnection _connection = connection;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .EnableServiceProviderCaching(false)
                .UseSqlServer(_connection, b => b.ApplyConfiguration());

        public override void Dispose()
        {
            _connection.Dispose();
            base.Dispose();
        }
    }

    private class OwnedConnectionInOnConfiguringContext(SqlConnection connection) : NorthwindContextBase
    {
        private readonly SqlConnection _connection = connection;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .EnableServiceProviderCaching(false)
                .UseSqlServer(_connection, contextOwnsConnection: true, b => b.ApplyConfiguration());
    }

    [ConditionalFact]
    public async Task Throws_if_no_connection_found_in_config_without_UseSqlServer()
    {
        var serviceProvider
            = new ServiceCollection()
                .AddDbContext<NoUseSqlServerContext>().BuildServiceProvider(validateScopes: true);

        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<NoUseSqlServerContext>();
        Assert.Equal(
            CoreStrings.NoProviderConfigured,
            (await Assert.ThrowsAsync<InvalidOperationException>(() => context.Customers.AnyAsync())).Message);
    }

    [ConditionalFact]
    public async Task Throws_if_no_config_without_UseSqlServer()
    {
        var serviceProvider
            = new ServiceCollection()
                .AddDbContext<NoUseSqlServerContext>().BuildServiceProvider(validateScopes: true);

        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<NoUseSqlServerContext>();
        Assert.Equal(
            CoreStrings.NoProviderConfigured,
            (await Assert.ThrowsAsync<InvalidOperationException>(() => context.Customers.AnyAsync())).Message);
    }

    private class NoUseSqlServerContext : NorthwindContextBase
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.EnableServiceProviderCaching(false);
    }

    [ConditionalFact]
    public async Task Can_depend_on_DbContextOptions()
    {
        var serviceProvider
            = new ServiceCollection()
                .AddScoped(p => new SqlConnection(SqlServerNorthwindTestStoreFactory.NorthwindConnectionString))
                .AddDbContext<OptionsContext>()
                .BuildServiceProvider(validateScopes: true);

        using (await SqlServerTestStore.GetNorthwindStoreAsync())
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<OptionsContext>();
            Assert.True(await context.Customers.AnyAsync());
        }
    }

    [ConditionalFact]
    public async Task Can_depend_on_DbContextOptions_with_default_service_provider()
    {
        using (await SqlServerTestStore.GetNorthwindStoreAsync())
        {
            using var connection = new SqlConnection(SqlServerNorthwindTestStoreFactory.NorthwindConnectionString);

            using var context = new OptionsContext(
                new DbContextOptions<OptionsContext>(),
                connection);

            Assert.True(await context.Customers.AnyAsync());
        }
    }

    private class OptionsContext(DbContextOptions<OptionsContext> options, SqlConnection connection) : NorthwindContextBase(options)
    {
        private readonly SqlConnection _connection = connection;
        private readonly DbContextOptions<OptionsContext> _options = options;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            Assert.Same(_options, optionsBuilder.Options);

            optionsBuilder
                .EnableServiceProviderCaching(false)
                .UseSqlServer(_connection, b => b.ApplyConfiguration());

            Assert.NotSame(_options, optionsBuilder.Options);
        }

        public override void Dispose()
        {
            _connection.Dispose();
            base.Dispose();
        }
    }

    [ConditionalFact]
    public async Task Can_depend_on_non_generic_options_when_only_one_context()
    {
        var serviceProvider
            = new ServiceCollection()
                .AddDbContext<NonGenericOptionsContext>()
                .BuildServiceProvider(validateScopes: true);

        using (await SqlServerTestStore.GetNorthwindStoreAsync())
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<NonGenericOptionsContext>();
            Assert.True(await context.Customers.AnyAsync());
        }
    }

    [ConditionalFact]
    public async Task Can_depend_on_non_generic_options_when_only_one_context_with_default_service_provider()
    {
        using (await SqlServerTestStore.GetNorthwindStoreAsync())
        {
            using var context = new NonGenericOptionsContext(new DbContextOptions<DbContext>());
            Assert.True(await context.Customers.AnyAsync());
        }
    }

    private class NonGenericOptionsContext(DbContextOptions options) : NorthwindContextBase(options)
    {
        private readonly DbContextOptions _options = options;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            Assert.Same(_options, optionsBuilder.Options);

            optionsBuilder
                .EnableServiceProviderCaching(false)
                .UseSqlServer(SqlServerNorthwindTestStoreFactory.NorthwindConnectionString, b => b.ApplyConfiguration());

            Assert.NotSame(_options, optionsBuilder.Options);
        }
    }

    [ConditionalTheory]
    [InlineData("MyConnectionString", "name=MyConnectionString")]
    [InlineData("ConnectionStrings:DefaultConnection", "name=ConnectionStrings:DefaultConnection")]
    [InlineData("ConnectionStrings:DefaultConnection", " NamE   =   ConnectionStrings:DefaultConnection  ")]
    public async Task Can_use_AddDbContext_and_get_connection_string_from_config(string key, string connectionString)
    {
        var configBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string> { { key, SqlServerNorthwindTestStoreFactory.NorthwindConnectionString } });

        var serviceProvider
            = new ServiceCollection()
                .AddSingleton<IConfiguration>(configBuilder.Build())
                .AddDbContext<UseConfigurationContext>(
                    b => b.UseSqlServer(connectionString).EnableServiceProviderCaching(false))
                .BuildServiceProvider(validateScopes: true);

        using (await SqlServerTestStore.GetNorthwindStoreAsync())
        {
            using var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
            using var context = serviceScope.ServiceProvider.GetRequiredService<UseConfigurationContext>();
            Assert.True(await context.Customers.AnyAsync());
        }
    }

    private class UseConfigurationContext(DbContextOptions options) : NorthwindContextBase(options);

    private class NorthwindContextBase : DbContext
    {
        protected NorthwindContextBase()
        {
        }

        protected NorthwindContextBase(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Customer>(
                b =>
                {
                    b.HasKey(c => c.CustomerID);
                    b.ToTable("Customers");
                });
    }

    private class Customer
    {
        public string CustomerID { get; set; }

        // ReSharper disable UnusedMember.Local
        public string CompanyName { get; set; }

        public string Fax { get; set; }
        // ReSharper restore UnusedMember.Local
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Can_use_an_existing_closed_connection_test(bool openConnection)
    {
        var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkSqlServer()
            .BuildServiceProvider(validateScopes: true);

        using var store = await SqlServerTestStore.GetNorthwindStoreAsync();
        store.CloseConnection();

        var openCount = 0;
        var closeCount = 0;
        var disposeCount = 0;

        using var connection = new SqlConnection(store.ConnectionString);
        if (openConnection)
        {
            await connection.OpenAsync();
        }

        connection.StateChange += (_, a) =>
        {
            switch (a.CurrentState)
            {
                case ConnectionState.Open:
                    openCount++;
                    break;
                case ConnectionState.Closed:
                    closeCount++;
                    break;
            }
        };
        connection.Disposed += (_, __) => disposeCount++;

        using (var context = new NorthwindContext(serviceProvider, connection))
        {
            Assert.Equal(91, await context.Customers.CountAsync());
        }

        if (openConnection)
        {
            Assert.Equal(ConnectionState.Open, connection.State);
            Assert.Equal(0, openCount);
            Assert.Equal(0, closeCount);
        }
        else
        {
            Assert.Equal(ConnectionState.Closed, connection.State);
            Assert.Equal(1, openCount);
            Assert.Equal(1, closeCount);
        }

        Assert.Equal(0, disposeCount);
    }

    private class NorthwindContext(IServiceProvider serviceProvider, SqlConnection connection) : DbContext
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly SqlConnection _connection = connection;

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public DbSet<Customer> Customers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseSqlServer(_connection, b => b.ApplyConfiguration())
                .UseInternalServiceProvider(_serviceProvider);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Customer>(
                b =>
                {
                    b.HasKey(c => c.CustomerID);
                    b.ToTable("Customers");
                });
    }
}
