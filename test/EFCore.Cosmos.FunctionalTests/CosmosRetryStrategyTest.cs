// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;
using Microsoft.Azure.Cosmos;

namespace Microsoft.EntityFrameworkCore;

#nullable disable

[CosmosCondition(CosmosCondition.IsNotLinuxEmulator)]
public class CosmosRetryTest(CosmosRetryTest.CosmosRetryFixture fixture)
    : IClassFixture<CosmosRetryTest.CosmosRetryFixture>, IAsyncLifetime
{
    private const string DatabaseName = nameof(CosmosRetryTest);

    protected CosmosRetryFixture Fixture { get; } = fixture;

    [ConditionalTheory, InlineData(false), InlineData(true)]
    public async Task Retry_for_create_stores_document(bool transactionalBatch)
    {
        var customer = new Customer { Id = 42, Name = "Theon" };
        using (var context = CreateContext(transactionalBatch))
        {
            context.Add(customer);

            try
            {
                Fixture.RequestHandler.Reset();
                Fixture.RequestHandler.ShouldFailNextRequest = true;
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is CosmosException { StatusCode: HttpStatusCode.Conflict })
            {
                // Ignored, because the request is actually executed and the error is only a mock,
                // the document was already created and we get a conflict
            }

            Assert.Equal(2, Fixture.RequestHandler.RequestCount);
        }

        using (var context = CreateContext(transactionalBatch))
        {
            var customerFromStore = await context.Set<Customer>().SingleAsync();
            Assert.Equal(42, customerFromStore.Id);
            Assert.Equal("Theon", customerFromStore.Name);
        }
    }

    [ConditionalTheory, InlineData(false), InlineData(true)]
    public async Task Retry_for_update_stores_document(bool transactionalBatch)
    {
        var customer = new Customer { Id = 42, Name = "Theon" };

        using (var context = CreateContext(transactionalBatch))
        {
            context.Add(customer);
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext(transactionalBatch))
        {
            var customerFromStore = await context.Set<Customer>().SingleAsync();
            customerFromStore.Name = "Theon Greyjoy";

            Fixture.RequestHandler.Reset();
            Fixture.RequestHandler.ShouldFailNextRequest = true;
            await context.SaveChangesAsync();
            Assert.Equal(2, Fixture.RequestHandler.RequestCount);
        }

        using (var context = CreateContext(transactionalBatch))
        {
            var customerFromStore = await context.Set<Customer>().SingleAsync();
            Assert.Equal("Theon Greyjoy", customerFromStore.Name);
        }
    }

    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    private RetryStrategyContext CreateContext(bool transactionalBatch)
    {
        var context = Fixture.CreateContext();
        if (transactionalBatch)
        {
            context.Database.AutoTransactionBehavior = AutoTransactionBehavior.Always;
        }
        return context;
    }

    public async Task InitializeAsync()
    {
        Fixture.RequestHandler.Reset();
        using var context = Fixture.CreateContext();
        context.RemoveRange(await context.Customers.ToListAsync());
        await context.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
    }

    public class CosmosRetryFixture : SharedStoreFixtureBase<RetryStrategyContext>
    {
        protected override string StoreName
            => DatabaseName;

        protected override ITestStoreFactory TestStoreFactory
            => CosmosTestStoreFactory.Instance;

        public TooManyRequestsHandler RequestHandler { get; } = new TooManyRequestsHandler();

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder)
                .UseCosmos(b => b.HttpClientFactory(() => new HttpClient(RequestHandler)));
    }

    public class RetryStrategyContext(DbContextOptions options) : PoolableDbContext(options)
    {
        public DbSet<Customer> Customers { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
            => builder.Entity<Customer>(
                b => b.HasPartitionKey(c => c.Id));
    }

    public class TooManyRequestsHandler : DelegatingHandler
    {
        public TooManyRequestsHandler()
            : base(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            })
        {
        }

        public int RequestCount { get; private set; }

        public bool ShouldFailNextRequest { get; set; }

        public void Reset()
        {
            ShouldFailNextRequest = false;
            RequestCount = 0;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestCount++;
            var response = await base.SendAsync(request, cancellationToken);
            if (ShouldFailNextRequest)
            {
                ShouldFailNextRequest = false;

                response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
                response.Headers.Add("x-ms-retry-after-ms", "1");
            }
            return response;

        }
    }
}
