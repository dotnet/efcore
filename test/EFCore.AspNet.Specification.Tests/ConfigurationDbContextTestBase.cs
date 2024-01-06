// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Options;
using IdentityServer4.EntityFramework.Stores;

namespace Microsoft.EntityFrameworkCore;

public abstract class ConfigurationDbContextTestBase<TFixture> : IClassFixture<TFixture>
    where TFixture : ConfigurationDbContextTestBase<TFixture>.ConfigurationDbContextFixtureBase
{
    protected ConfigurationDbContextTestBase(ConfigurationDbContextFixtureBase fixture)
    {
        Fixture = fixture;
    }

    protected ConfigurationDbContextFixtureBase Fixture { get; }

    [ConditionalFact(
        Skip =
            "VerificationException : Method System.Linq.Enumerable.MaxFloat: type argument 'System.Char' violates the constraint of type parameter 'T'.")]
    public async Task Can_call_ResourceStore_FindApiScopesByNameAsync()
        => await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                await SaveApiScopes(context);
            },
            async context =>
            {
                var store = new ResourceStore(context, new FakeLogger<ResourceStore>());

                Assert.Equal(2, (await store.FindApiScopesByNameAsync(new[] { "ApiScope2", "ApiScope1" })).Count());
            }
        );

    private static async Task SaveApiScopes(ConfigurationDbContext context)
    {
        context.AddRange(
            new ApiScope
            {
                Name = "ApiScope1",
                DisplayName = "ApiScope 1",
                Description = "ApiScope 1",
                Required = true,
                Emphasize = true,
                UserClaims = [],
                Properties = [],
            },
            new ApiScope
            {
                Name = "ApiScope2",
                DisplayName = "ApiScope 2",
                Description = "ApiScope 2",
                Required = true,
                Emphasize = true,
                UserClaims = [],
                Properties = [],
            },
            new ApiScope
            {
                Name = "ApiScope3",
                DisplayName = "ApiScope 3",
                Description = "ApiScope 3",
                Required = true,
                Emphasize = true,
                UserClaims = [],
                Properties = [],
            });

        await context.SaveChangesAsync();
    }

    [ConditionalFact(
        Skip =
            "VerificationException : Method System.Linq.Enumerable.MaxFloat: type argument 'System.Char' violates the constraint of type parameter 'T'.")]
    public async Task Can_call_ClientStore_FindClientByIdAsync()
        => await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                context.AddRange(
                    new Client
                    {
                        ClientId = "C1", Description = "D1",
                    },
                    new Client
                    {
                        ClientId = "C2", Description = "D2",
                    },
                    new Client
                    {
                        ClientId = "C3", Description = "D3",
                    });

                await context.SaveChangesAsync();
            },
            async context =>
            {
                var store = new ClientStore(context, new FakeLogger<ClientStore>());

                Assert.Equal("D2", (await store.FindClientByIdAsync("C2")).Description);
            }
        );

    [ConditionalFact(
        Skip =
            "VerificationException : Method System.Linq.Enumerable.MaxFloat: type argument 'System.Char' violates the constraint of type parameter 'T'.")]
    public async Task Can_call_ResourceStore_FindIdentityResourcesByScopeNameAsync()
        => await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                await SaveIdentityResources(context);
            },
            async context =>
            {
                var store = new ResourceStore(context, new FakeLogger<ResourceStore>());

                Assert.Equal(
                    2, (await store.FindIdentityResourcesByScopeNameAsync(new[] { "IdentityResource2", "IdentityResource1" })).Count());
            }
        );

    [ConditionalFact(
        Skip =
            "VerificationException : Method System.Linq.Enumerable.MaxFloat: type argument 'System.Char' violates the constraint of type parameter 'T'.")]
    public async Task Can_call_ResourceStore_FindApiResourcesByScopeNameAsync()
        => await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                await SaveApiResources(context);
            },
            async context =>
            {
                var store = new ResourceStore(context, new FakeLogger<ResourceStore>());

                Assert.Equal(2, (await store.FindApiResourcesByScopeNameAsync(new[] { "S1", "S4" })).Count());
            }
        );

    [ConditionalFact(
        Skip =
            "VerificationException : Method System.Linq.Enumerable.MaxFloat: type argument 'System.Char' violates the constraint of type parameter 'T'.")]
    public async Task Can_call_ResourceStore_GetAllResourcesAsync()
        => await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                await SaveIdentityResources(context);
                await SaveApiScopes(context);
                await SaveApiResources(context);
            },
            async context =>
            {
                var store = new ResourceStore(context, new FakeLogger<ResourceStore>());

                var resources = await store.GetAllResourcesAsync();

                Assert.Equal(3, resources.ApiResources.Count);
                Assert.Equal(3, resources.ApiScopes.Count);
                Assert.Equal(3, resources.IdentityResources.Count);
            }
        );

    private static async Task SaveIdentityResources(ConfigurationDbContext context)
    {
        context.AddRange(
            new IdentityResource
            {
                Name = "IdentityResource1",
                DisplayName = "IdentityResource 1",
                Description = "IdentityResource 1",
                Required = true,
                Emphasize = true,
            },
            new IdentityResource
            {
                Name = "IdentityResource2",
                DisplayName = "IdentityResource 2",
                Description = "IdentityResource 2",
                Required = true,
                Emphasize = true
            },
            new IdentityResource
            {
                Name = "IdentityResource3",
                DisplayName = "IdentityResource 3",
                Description = "IdentityResource 3",
                Required = true,
                Emphasize = true
            });

        await context.SaveChangesAsync();
    }

    private static async Task SaveApiResources(ConfigurationDbContext context)
    {
        context.AddRange(
            new ApiResource
            {
                Name = "ApiResource1",
                DisplayName = "ApiResource 1",
                Description = "ApiResource 1",
                Scopes = [new() { Scope = "S1" }, new() { Scope = "S2" }]
            },
            new ApiResource
            {
                Name = "ApiResource2",
                DisplayName = "ApiResource 2",
                Description = "ApiResource 2",
                Scopes = [new() { Scope = "S4" }, new() { Scope = "S5" }]
            },
            new ApiResource
            {
                Name = "ApiResource3",
                DisplayName = "ApiResource 3",
                Description = "ApiResource 3"
            });

        await context.SaveChangesAsync();
    }

    [ConditionalFact(
        Skip =
            "VerificationException : Method System.Linq.Enumerable.MaxFloat: type argument 'System.Char' violates the constraint of type parameter 'T'.")]
    public async Task Can_call_ResourceStore_FindApiResourcesByNameAsync()
        => await ExecuteWithStrategyInTransactionAsync(
            SaveApiResources,
            async context =>
            {
                var store = new ResourceStore(context, new FakeLogger<ResourceStore>());

                Assert.Equal(2, (await store.FindApiResourcesByNameAsync(new[] { "ApiResource2", "ApiResource1" })).Count());
            }
        );

    [ConditionalFact]
    public void Can_build_ConfigurationDbContext_model()
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
                Name = "IdentityServer4.EntityFramework.Entities.ApiResource",
                TableName = "ApiResources",
                PrimaryKey = "Key: ApiResource.Id PK",
                Properties =
                {
                    "Property: ApiResource.Id (int) Required PK AfterSave:Throw ValueGenerated.OnAdd",
                    "Property: ApiResource.AllowedAccessTokenSigningAlgorithms (string) MaxLength(100)",
                    "Property: ApiResource.Created (DateTime) Required",
                    "Property: ApiResource.Description (string) MaxLength(1000)",
                    "Property: ApiResource.DisplayName (string) MaxLength(200)",
                    "Property: ApiResource.Enabled (bool) Required",
                    "Property: ApiResource.LastAccessed (DateTime?)",
                    "Property: ApiResource.Name (string) Required Index MaxLength(200)",
                    "Property: ApiResource.NonEditable (bool) Required",
                    "Property: ApiResource.ShowInDiscoveryDocument (bool) Required",
                    "Property: ApiResource.Updated (DateTime?)",
                },
                Indexes = { "{'Name'} Unique", },
                Navigations =
                {
                    "Navigation: ApiResource.Properties (List<ApiResourceProperty>) Collection ToDependent ApiResourceProperty Inverse: ApiResource",
                    "Navigation: ApiResource.Scopes (List<ApiResourceScope>) Collection ToDependent ApiResourceScope Inverse: ApiResource",
                    "Navigation: ApiResource.Secrets (List<ApiResourceSecret>) Collection ToDependent ApiResourceSecret Inverse: ApiResource",
                    "Navigation: ApiResource.UserClaims (List<ApiResourceClaim>) Collection ToDependent ApiResourceClaim Inverse: ApiResource",
                },
            },
            new EntityTypeMapping
            {
                Name = "IdentityServer4.EntityFramework.Entities.ApiResourceClaim",
                TableName = "ApiResourceClaims",
                PrimaryKey = "Key: ApiResourceClaim.Id PK",
                Properties =
                {
                    "Property: ApiResourceClaim.Id (int) Required PK AfterSave:Throw ValueGenerated.OnAdd",
                    "Property: ApiResourceClaim.ApiResourceId (int) Required FK Index",
                    "Property: ApiResourceClaim.Type (string) Required MaxLength(200)",
                },
                Indexes = { "{'ApiResourceId'} ", },
                FKs =
                {
                    "ForeignKey: ApiResourceClaim {'ApiResourceId'} -> ApiResource {'Id'} Required Cascade ToDependent: UserClaims ToPrincipal: ApiResource",
                },
                Navigations = { "Navigation: ApiResourceClaim.ApiResource (ApiResource) ToPrincipal ApiResource Inverse: UserClaims", },
            },
            new EntityTypeMapping
            {
                Name = "IdentityServer4.EntityFramework.Entities.ApiResourceProperty",
                TableName = "ApiResourceProperties",
                PrimaryKey = "Key: ApiResourceProperty.Id PK",
                Properties =
                {
                    "Property: ApiResourceProperty.Id (int) Required PK AfterSave:Throw ValueGenerated.OnAdd",
                    "Property: ApiResourceProperty.ApiResourceId (int) Required FK Index",
                    "Property: ApiResourceProperty.Key (string) Required MaxLength(250)",
                    "Property: ApiResourceProperty.Value (string) Required MaxLength(2000)",
                },
                Indexes = { "{'ApiResourceId'} ", },
                FKs =
                {
                    "ForeignKey: ApiResourceProperty {'ApiResourceId'} -> ApiResource {'Id'} Required Cascade ToDependent: Properties ToPrincipal: ApiResource",
                },
                Navigations = { "Navigation: ApiResourceProperty.ApiResource (ApiResource) ToPrincipal ApiResource Inverse: Properties", },
            },
            new EntityTypeMapping
            {
                Name = "IdentityServer4.EntityFramework.Entities.ApiResourceScope",
                TableName = "ApiResourceScopes",
                PrimaryKey = "Key: ApiResourceScope.Id PK",
                Properties =
                {
                    "Property: ApiResourceScope.Id (int) Required PK AfterSave:Throw ValueGenerated.OnAdd",
                    "Property: ApiResourceScope.ApiResourceId (int) Required FK Index",
                    "Property: ApiResourceScope.Scope (string) Required MaxLength(200)",
                },
                Indexes = { "{'ApiResourceId'} ", },
                FKs =
                {
                    "ForeignKey: ApiResourceScope {'ApiResourceId'} -> ApiResource {'Id'} Required Cascade ToDependent: Scopes ToPrincipal: ApiResource",
                },
                Navigations = { "Navigation: ApiResourceScope.ApiResource (ApiResource) ToPrincipal ApiResource Inverse: Scopes", },
            },
            new EntityTypeMapping
            {
                Name = "IdentityServer4.EntityFramework.Entities.ApiResourceSecret",
                TableName = "ApiResourceSecrets",
                PrimaryKey = "Key: ApiResourceSecret.Id PK",
                Properties =
                {
                    "Property: ApiResourceSecret.Id (int) Required PK AfterSave:Throw ValueGenerated.OnAdd",
                    "Property: ApiResourceSecret.ApiResourceId (int) Required FK Index",
                    "Property: ApiResourceSecret.Created (DateTime) Required",
                    "Property: ApiResourceSecret.Description (string) MaxLength(1000)",
                    "Property: ApiResourceSecret.Expiration (DateTime?)",
                    "Property: ApiResourceSecret.Type (string) Required MaxLength(250)",
                    "Property: ApiResourceSecret.Value (string) Required MaxLength(4000)",
                },
                Indexes = { "{'ApiResourceId'} ", },
                FKs =
                {
                    "ForeignKey: ApiResourceSecret {'ApiResourceId'} -> ApiResource {'Id'} Required Cascade ToDependent: Secrets ToPrincipal: ApiResource",
                },
                Navigations = { "Navigation: ApiResourceSecret.ApiResource (ApiResource) ToPrincipal ApiResource Inverse: Secrets", },
            },
            new EntityTypeMapping
            {
                Name = "IdentityServer4.EntityFramework.Entities.ApiScope",
                TableName = "ApiScopes",
                PrimaryKey = "Key: ApiScope.Id PK",
                Properties =
                {
                    "Property: ApiScope.Id (int) Required PK AfterSave:Throw ValueGenerated.OnAdd",
                    "Property: ApiScope.Description (string) MaxLength(1000)",
                    "Property: ApiScope.DisplayName (string) MaxLength(200)",
                    "Property: ApiScope.Emphasize (bool) Required",
                    "Property: ApiScope.Enabled (bool) Required",
                    "Property: ApiScope.Name (string) Required Index MaxLength(200)",
                    "Property: ApiScope.Required (bool) Required",
                    "Property: ApiScope.ShowInDiscoveryDocument (bool) Required",
                },
                Indexes = { "{'Name'} Unique", },
                Navigations =
                {
                    "Navigation: ApiScope.Properties (List<ApiScopeProperty>) Collection ToDependent ApiScopeProperty Inverse: Scope",
                    "Navigation: ApiScope.UserClaims (List<ApiScopeClaim>) Collection ToDependent ApiScopeClaim Inverse: Scope",
                },
            },
            new EntityTypeMapping
            {
                Name = "IdentityServer4.EntityFramework.Entities.ApiScopeClaim",
                TableName = "ApiScopeClaims",
                PrimaryKey = "Key: ApiScopeClaim.Id PK",
                Properties =
                {
                    "Property: ApiScopeClaim.Id (int) Required PK AfterSave:Throw ValueGenerated.OnAdd",
                    "Property: ApiScopeClaim.ScopeId (int) Required FK Index",
                    "Property: ApiScopeClaim.Type (string) Required MaxLength(200)",
                },
                Indexes = { "{'ScopeId'} ", },
                FKs =
                {
                    "ForeignKey: ApiScopeClaim {'ScopeId'} -> ApiScope {'Id'} Required Cascade ToDependent: UserClaims ToPrincipal: Scope",
                },
                Navigations = { "Navigation: ApiScopeClaim.Scope (ApiScope) ToPrincipal ApiScope Inverse: UserClaims", },
            },
            new EntityTypeMapping
            {
                Name = "IdentityServer4.EntityFramework.Entities.ApiScopeProperty",
                TableName = "ApiScopeProperties",
                PrimaryKey = "Key: ApiScopeProperty.Id PK",
                Properties =
                {
                    "Property: ApiScopeProperty.Id (int) Required PK AfterSave:Throw ValueGenerated.OnAdd",
                    "Property: ApiScopeProperty.Key (string) Required MaxLength(250)",
                    "Property: ApiScopeProperty.ScopeId (int) Required FK Index",
                    "Property: ApiScopeProperty.Value (string) Required MaxLength(2000)",
                },
                Indexes = { "{'ScopeId'} ", },
                FKs =
                {
                    "ForeignKey: ApiScopeProperty {'ScopeId'} -> ApiScope {'Id'} Required Cascade ToDependent: Properties ToPrincipal: Scope",
                },
                Navigations = { "Navigation: ApiScopeProperty.Scope (ApiScope) ToPrincipal ApiScope Inverse: Properties", },
            },
            new EntityTypeMapping
            {
                Name = "IdentityServer4.EntityFramework.Entities.Client",
                TableName = "Clients",
                PrimaryKey = "Key: Client.Id PK",
                Properties =
                {
                    "Property: Client.Id (int) Required PK AfterSave:Throw ValueGenerated.OnAdd",
                    "Property: Client.AbsoluteRefreshTokenLifetime (int) Required",
                    "Property: Client.AccessTokenLifetime (int) Required",
                    "Property: Client.AccessTokenType (int) Required",
                    "Property: Client.AllowAccessTokensViaBrowser (bool) Required",
                    "Property: Client.AllowOfflineAccess (bool) Required",
                    "Property: Client.AllowPlainTextPkce (bool) Required",
                    "Property: Client.AllowRememberConsent (bool) Required",
                    "Property: Client.AllowedIdentityTokenSigningAlgorithms (string) MaxLength(100)",
                    "Property: Client.AlwaysIncludeUserClaimsInIdToken (bool) Required",
                    "Property: Client.AlwaysSendClientClaims (bool) Required",
                    "Property: Client.AuthorizationCodeLifetime (int) Required",
                    "Property: Client.BackChannelLogoutSessionRequired (bool) Required",
                    "Property: Client.BackChannelLogoutUri (string) MaxLength(2000)",
                    "Property: Client.ClientClaimsPrefix (string) MaxLength(200)",
                    "Property: Client.ClientId (string) Required Index MaxLength(200)",
                    "Property: Client.ClientName (string) MaxLength(200)",
                    "Property: Client.ClientUri (string) MaxLength(2000)",
                    "Property: Client.ConsentLifetime (int?)",
                    "Property: Client.Created (DateTime) Required",
                    "Property: Client.Description (string) MaxLength(1000)",
                    "Property: Client.DeviceCodeLifetime (int) Required",
                    "Property: Client.EnableLocalLogin (bool) Required",
                    "Property: Client.Enabled (bool) Required",
                    "Property: Client.FrontChannelLogoutSessionRequired (bool) Required",
                    "Property: Client.FrontChannelLogoutUri (string) MaxLength(2000)",
                    "Property: Client.IdentityTokenLifetime (int) Required",
                    "Property: Client.IncludeJwtId (bool) Required",
                    "Property: Client.LastAccessed (DateTime?)",
                    "Property: Client.LogoUri (string) MaxLength(2000)",
                    "Property: Client.NonEditable (bool) Required",
                    "Property: Client.PairWiseSubjectSalt (string) MaxLength(200)",
                    "Property: Client.ProtocolType (string) Required MaxLength(200)",
                    "Property: Client.RefreshTokenExpiration (int) Required",
                    "Property: Client.RefreshTokenUsage (int) Required",
                    "Property: Client.RequireClientSecret (bool) Required",
                    "Property: Client.RequireConsent (bool) Required",
                    "Property: Client.RequirePkce (bool) Required",
                    "Property: Client.RequireRequestObject (bool) Required",
                    "Property: Client.SlidingRefreshTokenLifetime (int) Required",
                    "Property: Client.UpdateAccessTokenClaimsOnRefresh (bool) Required",
                    "Property: Client.Updated (DateTime?)",
                    "Property: Client.UserCodeType (string) MaxLength(100)",
                    "Property: Client.UserSsoLifetime (int?)",
                },
                Indexes = { "{'ClientId'} Unique", },
                Navigations =
                {
                    "Navigation: Client.AllowedCorsOrigins (List<ClientCorsOrigin>) Collection ToDependent ClientCorsOrigin Inverse: Client",
                    "Navigation: Client.AllowedGrantTypes (List<ClientGrantType>) Collection ToDependent ClientGrantType Inverse: Client",
                    "Navigation: Client.AllowedScopes (List<ClientScope>) Collection ToDependent ClientScope Inverse: Client",
                    "Navigation: Client.Claims (List<ClientClaim>) Collection ToDependent ClientClaim Inverse: Client",
                    "Navigation: Client.ClientSecrets (List<ClientSecret>) Collection ToDependent ClientSecret Inverse: Client",
                    "Navigation: Client.IdentityProviderRestrictions (List<ClientIdPRestriction>) Collection ToDependent ClientIdPRestriction Inverse: Client",
                    "Navigation: Client.PostLogoutRedirectUris (List<ClientPostLogoutRedirectUri>) Collection ToDependent ClientPostLogoutRedirectUri Inverse: Client",
                    "Navigation: Client.Properties (List<ClientProperty>) Collection ToDependent ClientProperty Inverse: Client",
                    "Navigation: Client.RedirectUris (List<ClientRedirectUri>) Collection ToDependent ClientRedirectUri Inverse: Client",
                },
            },
            new EntityTypeMapping
            {
                Name = "IdentityServer4.EntityFramework.Entities.ClientClaim",
                TableName = "ClientClaims",
                PrimaryKey = "Key: ClientClaim.Id PK",
                Properties =
                {
                    "Property: ClientClaim.Id (int) Required PK AfterSave:Throw ValueGenerated.OnAdd",
                    "Property: ClientClaim.ClientId (int) Required FK Index",
                    "Property: ClientClaim.Type (string) Required MaxLength(250)",
                    "Property: ClientClaim.Value (string) Required MaxLength(250)",
                },
                Indexes = { "{'ClientId'} ", },
                FKs = { "ForeignKey: ClientClaim {'ClientId'} -> Client {'Id'} Required Cascade ToDependent: Claims ToPrincipal: Client", },
                Navigations = { "Navigation: ClientClaim.Client (Client) ToPrincipal Client Inverse: Claims", },
            },
            new EntityTypeMapping
            {
                Name = "IdentityServer4.EntityFramework.Entities.ClientCorsOrigin",
                TableName = "ClientCorsOrigins",
                PrimaryKey = "Key: ClientCorsOrigin.Id PK",
                Properties =
                {
                    "Property: ClientCorsOrigin.Id (int) Required PK AfterSave:Throw ValueGenerated.OnAdd",
                    "Property: ClientCorsOrigin.ClientId (int) Required FK Index",
                    "Property: ClientCorsOrigin.Origin (string) Required MaxLength(150)",
                },
                Indexes = { "{'ClientId'} ", },
                FKs =
                {
                    "ForeignKey: ClientCorsOrigin {'ClientId'} -> Client {'Id'} Required Cascade ToDependent: AllowedCorsOrigins ToPrincipal: Client",
                },
                Navigations = { "Navigation: ClientCorsOrigin.Client (Client) ToPrincipal Client Inverse: AllowedCorsOrigins", },
            },
            new EntityTypeMapping
            {
                Name = "IdentityServer4.EntityFramework.Entities.ClientGrantType",
                TableName = "ClientGrantTypes",
                PrimaryKey = "Key: ClientGrantType.Id PK",
                Properties =
                {
                    "Property: ClientGrantType.Id (int) Required PK AfterSave:Throw ValueGenerated.OnAdd",
                    "Property: ClientGrantType.ClientId (int) Required FK Index",
                    "Property: ClientGrantType.GrantType (string) Required MaxLength(250)",
                },
                Indexes = { "{'ClientId'} ", },
                FKs =
                {
                    "ForeignKey: ClientGrantType {'ClientId'} -> Client {'Id'} Required Cascade ToDependent: AllowedGrantTypes ToPrincipal: Client",
                },
                Navigations = { "Navigation: ClientGrantType.Client (Client) ToPrincipal Client Inverse: AllowedGrantTypes", },
            },
            new EntityTypeMapping
            {
                Name = "IdentityServer4.EntityFramework.Entities.ClientIdPRestriction",
                TableName = "ClientIdPRestrictions",
                PrimaryKey = "Key: ClientIdPRestriction.Id PK",
                Properties =
                {
                    "Property: ClientIdPRestriction.Id (int) Required PK AfterSave:Throw ValueGenerated.OnAdd",
                    "Property: ClientIdPRestriction.ClientId (int) Required FK Index",
                    "Property: ClientIdPRestriction.Provider (string) Required MaxLength(200)",
                },
                Indexes = { "{'ClientId'} ", },
                FKs =
                {
                    "ForeignKey: ClientIdPRestriction {'ClientId'} -> Client {'Id'} Required Cascade ToDependent: IdentityProviderRestrictions ToPrincipal: Client",
                },
                Navigations =
                {
                    "Navigation: ClientIdPRestriction.Client (Client) ToPrincipal Client Inverse: IdentityProviderRestrictions",
                },
            },
            new EntityTypeMapping
            {
                Name = "IdentityServer4.EntityFramework.Entities.ClientPostLogoutRedirectUri",
                TableName = "ClientPostLogoutRedirectUris",
                PrimaryKey = "Key: ClientPostLogoutRedirectUri.Id PK",
                Properties =
                {
                    "Property: ClientPostLogoutRedirectUri.Id (int) Required PK AfterSave:Throw ValueGenerated.OnAdd",
                    "Property: ClientPostLogoutRedirectUri.ClientId (int) Required FK Index",
                    "Property: ClientPostLogoutRedirectUri.PostLogoutRedirectUri (string) Required MaxLength(2000)",
                },
                Indexes = { "{'ClientId'} ", },
                FKs =
                {
                    "ForeignKey: ClientPostLogoutRedirectUri {'ClientId'} -> Client {'Id'} Required Cascade ToDependent: PostLogoutRedirectUris ToPrincipal: Client",
                },
                Navigations =
                {
                    "Navigation: ClientPostLogoutRedirectUri.Client (Client) ToPrincipal Client Inverse: PostLogoutRedirectUris",
                },
            },
            new EntityTypeMapping
            {
                Name = "IdentityServer4.EntityFramework.Entities.ClientProperty",
                TableName = "ClientProperties",
                PrimaryKey = "Key: ClientProperty.Id PK",
                Properties =
                {
                    "Property: ClientProperty.Id (int) Required PK AfterSave:Throw ValueGenerated.OnAdd",
                    "Property: ClientProperty.ClientId (int) Required FK Index",
                    "Property: ClientProperty.Key (string) Required MaxLength(250)",
                    "Property: ClientProperty.Value (string) Required MaxLength(2000)",
                },
                Indexes = { "{'ClientId'} ", },
                FKs =
                {
                    "ForeignKey: ClientProperty {'ClientId'} -> Client {'Id'} Required Cascade ToDependent: Properties ToPrincipal: Client",
                },
                Navigations = { "Navigation: ClientProperty.Client (Client) ToPrincipal Client Inverse: Properties", },
            },
            new EntityTypeMapping
            {
                Name = "IdentityServer4.EntityFramework.Entities.ClientRedirectUri",
                TableName = "ClientRedirectUris",
                PrimaryKey = "Key: ClientRedirectUri.Id PK",
                Properties =
                {
                    "Property: ClientRedirectUri.Id (int) Required PK AfterSave:Throw ValueGenerated.OnAdd",
                    "Property: ClientRedirectUri.ClientId (int) Required FK Index",
                    "Property: ClientRedirectUri.RedirectUri (string) Required MaxLength(2000)",
                },
                Indexes = { "{'ClientId'} ", },
                FKs =
                {
                    "ForeignKey: ClientRedirectUri {'ClientId'} -> Client {'Id'} Required Cascade ToDependent: RedirectUris ToPrincipal: Client",
                },
                Navigations = { "Navigation: ClientRedirectUri.Client (Client) ToPrincipal Client Inverse: RedirectUris", },
            },
            new EntityTypeMapping
            {
                Name = "IdentityServer4.EntityFramework.Entities.ClientScope",
                TableName = "ClientScopes",
                PrimaryKey = "Key: ClientScope.Id PK",
                Properties =
                {
                    "Property: ClientScope.Id (int) Required PK AfterSave:Throw ValueGenerated.OnAdd",
                    "Property: ClientScope.ClientId (int) Required FK Index",
                    "Property: ClientScope.Scope (string) Required MaxLength(200)",
                },
                Indexes = { "{'ClientId'} ", },
                FKs =
                {
                    "ForeignKey: ClientScope {'ClientId'} -> Client {'Id'} Required Cascade ToDependent: AllowedScopes ToPrincipal: Client",
                },
                Navigations = { "Navigation: ClientScope.Client (Client) ToPrincipal Client Inverse: AllowedScopes", },
            },
            new EntityTypeMapping
            {
                Name = "IdentityServer4.EntityFramework.Entities.ClientSecret",
                TableName = "ClientSecrets",
                PrimaryKey = "Key: ClientSecret.Id PK",
                Properties =
                {
                    "Property: ClientSecret.Id (int) Required PK AfterSave:Throw ValueGenerated.OnAdd",
                    "Property: ClientSecret.ClientId (int) Required FK Index",
                    "Property: ClientSecret.Created (DateTime) Required",
                    "Property: ClientSecret.Description (string) MaxLength(2000)",
                    "Property: ClientSecret.Expiration (DateTime?)",
                    "Property: ClientSecret.Type (string) Required MaxLength(250)",
                    "Property: ClientSecret.Value (string) Required MaxLength(4000)",
                },
                Indexes = { "{'ClientId'} ", },
                FKs =
                {
                    "ForeignKey: ClientSecret {'ClientId'} -> Client {'Id'} Required Cascade ToDependent: ClientSecrets ToPrincipal: Client",
                },
                Navigations = { "Navigation: ClientSecret.Client (Client) ToPrincipal Client Inverse: ClientSecrets", },
            },
            new EntityTypeMapping
            {
                Name = "IdentityServer4.EntityFramework.Entities.IdentityResource",
                TableName = "IdentityResources",
                PrimaryKey = "Key: IdentityResource.Id PK",
                Properties =
                {
                    "Property: IdentityResource.Id (int) Required PK AfterSave:Throw ValueGenerated.OnAdd",
                    "Property: IdentityResource.Created (DateTime) Required",
                    "Property: IdentityResource.Description (string) MaxLength(1000)",
                    "Property: IdentityResource.DisplayName (string) MaxLength(200)",
                    "Property: IdentityResource.Emphasize (bool) Required",
                    "Property: IdentityResource.Enabled (bool) Required",
                    "Property: IdentityResource.Name (string) Required Index MaxLength(200)",
                    "Property: IdentityResource.NonEditable (bool) Required",
                    "Property: IdentityResource.Required (bool) Required",
                    "Property: IdentityResource.ShowInDiscoveryDocument (bool) Required",
                    "Property: IdentityResource.Updated (DateTime?)",
                },
                Indexes = { "{'Name'} Unique", },
                Navigations =
                {
                    "Navigation: IdentityResource.Properties (List<IdentityResourceProperty>) Collection ToDependent IdentityResourceProperty Inverse: IdentityResource",
                    "Navigation: IdentityResource.UserClaims (List<IdentityResourceClaim>) Collection ToDependent IdentityResourceClaim Inverse: IdentityResource",
                },
            },
            new EntityTypeMapping
            {
                Name = "IdentityServer4.EntityFramework.Entities.IdentityResourceClaim",
                TableName = "IdentityResourceClaims",
                PrimaryKey = "Key: IdentityResourceClaim.Id PK",
                Properties =
                {
                    "Property: IdentityResourceClaim.Id (int) Required PK AfterSave:Throw ValueGenerated.OnAdd",
                    "Property: IdentityResourceClaim.IdentityResourceId (int) Required FK Index",
                    "Property: IdentityResourceClaim.Type (string) Required MaxLength(200)",
                },
                Indexes = { "{'IdentityResourceId'} ", },
                FKs =
                {
                    "ForeignKey: IdentityResourceClaim {'IdentityResourceId'} -> IdentityResource {'Id'} Required Cascade ToDependent: UserClaims ToPrincipal: IdentityResource",
                },
                Navigations =
                {
                    "Navigation: IdentityResourceClaim.IdentityResource (IdentityResource) ToPrincipal IdentityResource Inverse: UserClaims",
                },
            },
            new EntityTypeMapping
            {
                Name = "IdentityServer4.EntityFramework.Entities.IdentityResourceProperty",
                TableName = "IdentityResourceProperties",
                PrimaryKey = "Key: IdentityResourceProperty.Id PK",
                Properties =
                {
                    "Property: IdentityResourceProperty.Id (int) Required PK AfterSave:Throw ValueGenerated.OnAdd",
                    "Property: IdentityResourceProperty.IdentityResourceId (int) Required FK Index",
                    "Property: IdentityResourceProperty.Key (string) Required MaxLength(250)",
                    "Property: IdentityResourceProperty.Value (string) Required MaxLength(2000)",
                },
                Indexes = { "{'IdentityResourceId'} ", },
                FKs =
                {
                    "ForeignKey: IdentityResourceProperty {'IdentityResourceId'} -> IdentityResource {'Id'} Required Cascade ToDependent: Properties ToPrincipal: IdentityResource",
                },
                Navigations =
                {
                    "Navigation: IdentityResourceProperty.IdentityResource (IdentityResource) ToPrincipal IdentityResource Inverse: Properties",
                },
            }
        ];

    protected ConfigurationDbContext CreateContext()
        => Fixture.CreateContext();

    protected virtual Task ExecuteWithStrategyInTransactionAsync(
        Func<ConfigurationDbContext, Task> testOperation,
        Func<ConfigurationDbContext, Task> nestedTestOperation1 = null,
        Func<ConfigurationDbContext, Task> nestedTestOperation2 = null,
        Func<ConfigurationDbContext, Task> nestedTestOperation3 = null)
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext, UseTransaction,
            testOperation, nestedTestOperation1, nestedTestOperation2, nestedTestOperation3);

    protected virtual void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    public abstract class ConfigurationDbContextFixtureBase : SharedStoreFixtureBase<ConfigurationDbContext>
    {
        protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
            => base.AddServices(serviceCollection)
                .AddSingleton<ConfigurationStoreOptions>();

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
