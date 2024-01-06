// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using IdentityServer4.EntityFramework;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Options;
using IdentityServer4.EntityFramework.Stores;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using IdentityServer4.Stores.Serialization;

namespace Microsoft.EntityFrameworkCore;

public abstract class PersistedGrantDbContextTestBase<TFixture> : IClassFixture<TFixture>
    where TFixture : PersistedGrantDbContextTestBase<TFixture>.PersistedGrantDbContextFixtureBase
{
    protected PersistedGrantDbContextTestBase(PersistedGrantDbContextFixtureBase fixture)
    {
        Fixture = fixture;
    }

    protected PersistedGrantDbContextFixtureBase Fixture { get; }

    [ConditionalFact]
    public async Task Can_call_PersistedGrantStore_GetAllAsync()
        => await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                await SaveGrants(context);
            },
            async context =>
            {
                var store = new PersistedGrantStore(context, new FakeLogger<PersistedGrantStore>());

                var results = (await store.GetAllAsync(
                    new PersistedGrantFilter
                    {
                        Type = "T1",
                        SessionId = "Se1",
                        SubjectId = "Su1"
                    })).ToList();

                Assert.Equal(2, results.Count);
            }
        );

    private static async Task SaveGrants(PersistedGrantDbContext context)
    {
        var store = new PersistedGrantStore(context, new FakeLogger<PersistedGrantStore>());
        await store.StoreAsync(
            new PersistedGrant
            {
                Key = "K1",
                Type = "T1",
                SubjectId = "Su1",
                SessionId = "Se1",
                ClientId = "C1",
                Description = "D1",
                CreationTime = DateTime.Now,
                Expiration = null,
                ConsumedTime = null,
                Data = "Data1"
            });
        await store.StoreAsync(
            new PersistedGrant
            {
                Key = "K2",
                Type = "T1",
                SubjectId = "Su1",
                SessionId = "Se1",
                ClientId = "C2",
                Description = "D2",
                CreationTime = DateTime.Now,
                Expiration = DateTime.Now + new TimeSpan(1, 0, 0, 0),
                ConsumedTime = null,
                Data = "Data2"
            });
        await store.StoreAsync(
            new PersistedGrant
            {
                Key = "K3",
                Type = "T2",
                SubjectId = "Su2",
                SessionId = "Se2",
                ClientId = "C1",
                Description = "D3",
                CreationTime = DateTime.Now,
                Expiration = null,
                ConsumedTime = null,
                Data = "Data3"
            });
    }

    [ConditionalFact]
    public async Task Can_call_PersistedGrantStore_GetAsync()
        => await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                await SaveGrants(context);
            },
            async context =>
            {
                var store = new PersistedGrantStore(context, new FakeLogger<PersistedGrantStore>());

                Assert.Equal("Data2", (await store.GetAsync("K2")).Data);
                Assert.Null(await store.GetAsync("???"));
            }
        );

    [ConditionalFact]
    public async Task Can_call_PersistedGrantStore_RemoveAsync()
        => await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                await SaveGrants(context);
            },
            async context =>
            {
                var store = new PersistedGrantStore(context, new FakeLogger<PersistedGrantStore>());

                await store.RemoveAsync("K2");
                Assert.Null(await store.GetAsync("K2"));
                await store.RemoveAsync("???");
            }
        );

    [ConditionalFact]
    public async Task Can_call_PersistedGrantStore_RemoveAllAsync()
        => await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                await SaveGrants(context);
            },
            async context =>
            {
                var store = new PersistedGrantStore(context, new FakeLogger<PersistedGrantStore>());

                await store.RemoveAllAsync(
                    new PersistedGrantFilter
                    {
                        Type = "T1",
                        SessionId = "Se1",
                        SubjectId = "Su1"
                    });

                Assert.Null(await store.GetAsync("K1"));
                Assert.Null(await store.GetAsync("K2"));
                Assert.NotNull(await store.GetAsync("K3"));
            }
        );

    [ConditionalFact]
    public async Task Can_call_TokenCleanupService_RemoveExpiredGrantsAsync()
        => await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                await SaveGrants(context);
                await SaveDevices(context);
            },
            async context =>
            {
                var service = new TokenCleanupService(new OperationalStoreOptions(), context, new FakeLogger<TokenCleanupService>());

                await service.RemoveExpiredGrantsAsync();
            }
        );

    [ConditionalFact]
    public async Task Can_call_DeviceFlowStore_FindByUserCodeAsync()
        => await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                await SaveDevices(context);
            },
            async context =>
            {
                var store = new DeviceFlowStore(context, new PersistentGrantSerializer(), new FakeLogger<DeviceFlowStore>());

                Assert.Equal("D2", (await store.FindByUserCodeAsync("U2")).Description);
            }
        );

    [ConditionalFact]
    public async Task Can_call_DeviceFlowStore_FindByDeviceCodeAsync_and_RemoveByDeviceCodeAsync()
        => await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                await SaveDevices(context);
            },
            async context =>
            {
                var store = new DeviceFlowStore(context, new PersistentGrantSerializer(), new FakeLogger<DeviceFlowStore>());

                Assert.Equal("D2", (await store.FindByDeviceCodeAsync("DC2")).Description);
                await store.RemoveByDeviceCodeAsync("DC2");
                Assert.Null(await store.FindByDeviceCodeAsync("DC2"));
                await store.RemoveByDeviceCodeAsync("DC2");
                Assert.Null(await store.FindByDeviceCodeAsync("DC2"));
            }
        );

    private static async Task SaveDevices(PersistedGrantDbContext context)
    {
        var store = new DeviceFlowStore(context, new PersistentGrantSerializer(), new FakeLogger<DeviceFlowStore>());

        await store.StoreDeviceAuthorizationAsync(
            "DC1", "U1", new DeviceCode
            {
                CreationTime = DateTime.Now,
                Lifetime = 100,
                ClientId = "C1",
                Description = "D1",
                IsOpenId = false,
                IsAuthorized = true,
                RequestedScopes = new List<string>(),
                AuthorizedScopes = new List<string>(),
                SessionId = "S1"
            });

        await store.StoreDeviceAuthorizationAsync(
            "DC2", "U2", new DeviceCode
            {
                CreationTime = DateTime.Now,
                Lifetime = 100,
                ClientId = "C2",
                Description = "D2",
                IsOpenId = false,
                IsAuthorized = true,
                RequestedScopes = new List<string>(),
                AuthorizedScopes = new List<string>(),
                SessionId = "S2"
            });

        await store.StoreDeviceAuthorizationAsync(
            "DC3", "U3", new DeviceCode
            {
                CreationTime = DateTime.Now,
                Lifetime = 100,
                ClientId = "C3",
                Description = "D3",
                IsOpenId = false,
                IsAuthorized = true,
                RequestedScopes = new List<string>(),
                AuthorizedScopes = new List<string>(),
                SessionId = "S3"
            });
    }

    [ConditionalFact]
    public void Can_build_PersistedGrantDbContext_model()
    {
        using (var context = CreateContext())
        {
            var entityTypeMappings = context.Model.GetEntityTypes().Select(e => new EntityTypeMapping(e)).ToList();

            EntityTypeMapping.AssertEqual(ExpectedMappings, entityTypeMappings);
        }
    }

    protected virtual List<EntityTypeMapping> ExpectedMappings
        =>
        [
            new EntityTypeMapping
            {
                Name = "IdentityServer4.EntityFramework.Entities.DeviceFlowCodes",
                TableName = "DeviceCodes",
                PrimaryKey = "Key: DeviceFlowCodes.UserCode PK",
                Properties =
                {
                    "Property: DeviceFlowCodes.UserCode (string) Required PK AfterSave:Throw MaxLength(200)",
                    "Property: DeviceFlowCodes.ClientId (string) Required MaxLength(200)",
                    "Property: DeviceFlowCodes.CreationTime (DateTime) Required",
                    "Property: DeviceFlowCodes.Data (string) Required MaxLength(50000)",
                    "Property: DeviceFlowCodes.Description (string) MaxLength(200)",
                    "Property: DeviceFlowCodes.DeviceCode (string) Required Index MaxLength(200)",
                    "Property: DeviceFlowCodes.Expiration (DateTime?) Required Index",
                    "Property: DeviceFlowCodes.SessionId (string) MaxLength(100)",
                    "Property: DeviceFlowCodes.SubjectId (string) MaxLength(200)",
                },
                Indexes =
                {
                    "{'DeviceCode'} Unique", "{'Expiration'} ",
                },
            },
            new EntityTypeMapping
            {
                Name = "IdentityServer4.EntityFramework.Entities.PersistedGrant",
                TableName = "PersistedGrants",
                PrimaryKey = "Key: PersistedGrant.Key PK",
                Properties =
                {
                    "Property: PersistedGrant.Key (string) Required PK AfterSave:Throw MaxLength(200)",
                    "Property: PersistedGrant.ClientId (string) Required Index MaxLength(200)",
                    "Property: PersistedGrant.ConsumedTime (DateTime?)",
                    "Property: PersistedGrant.CreationTime (DateTime) Required",
                    "Property: PersistedGrant.Data (string) Required MaxLength(50000)",
                    "Property: PersistedGrant.Description (string) MaxLength(200)",
                    "Property: PersistedGrant.Expiration (DateTime?) Index",
                    "Property: PersistedGrant.SessionId (string) Index MaxLength(100)",
                    "Property: PersistedGrant.SubjectId (string) Index MaxLength(200)",
                    "Property: PersistedGrant.Type (string) Required Index MaxLength(50)",
                },
                Indexes =
                {
                    "{'Expiration'} ",
                    "{'SubjectId', 'ClientId', 'Type'} ",
                    "{'SubjectId', 'SessionId', 'Type'} ",
                },
            }
        ];

    protected PersistedGrantDbContext CreateContext()
        => Fixture.CreateContext();

    protected virtual Task ExecuteWithStrategyInTransactionAsync(
        Func<PersistedGrantDbContext, Task> testOperation,
        Func<PersistedGrantDbContext, Task> nestedTestOperation1 = null,
        Func<PersistedGrantDbContext, Task> nestedTestOperation2 = null,
        Func<PersistedGrantDbContext, Task> nestedTestOperation3 = null)
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext, UseTransaction,
            testOperation, nestedTestOperation1, nestedTestOperation2, nestedTestOperation3);

    protected virtual void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    public abstract class PersistedGrantDbContextFixtureBase : SharedStoreFixtureBase<PersistedGrantDbContext>
    {
        protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
            => base.AddServices(serviceCollection)
                .AddSingleton<OperationalStoreOptions>();

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder)
                .EnableDetailedErrors()
                .EnableSensitiveDataLogging()
                .ConfigureWarnings(
                    b => b.Default(WarningBehavior.Throw)
                        .Log(CoreEventId.SensitiveDataLoggingEnabledWarning)
                        .Log(CoreEventId.PossibleUnintendedReferenceComparisonWarning));

        protected override bool UsePooling
            => false; // The IdentityServer ConfigurationDbContext has additional service dependencies
    }
}
