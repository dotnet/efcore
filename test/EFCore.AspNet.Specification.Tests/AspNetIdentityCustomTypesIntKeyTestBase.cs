// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Microsoft.EntityFrameworkCore;

public abstract class AspNetIdentityCustomTypesIntKeyTestBase<TFixture>
    : AspNetIdentityTestBase<TFixture, CustomTypesIdentityContextInt, CustomUserInt, CustomRoleInt, int, CustomUserClaimInt,
        CustomUserRoleInt, CustomUserLoginInt, CustomRoleClaimInt, CustomUserTokenInt>
    where TFixture : AspNetIdentityTestBase<TFixture, CustomTypesIdentityContextInt, CustomUserInt, CustomRoleInt, int, CustomUserClaimInt,
        CustomUserRoleInt, CustomUserLoginInt, CustomRoleClaimInt, CustomUserTokenInt>.AspNetIdentityFixtureBase
{
    protected AspNetIdentityCustomTypesIntKeyTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    [ConditionalFact]
    public async Task Can_use_navigation_properties_on_User()
    {
        var userId = 0;

        await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var user = new CustomUserInt { NormalizedUserName = "wendy" };
                await CreateUser(context, user);
                userId = user.Id;
            },
            async context =>
            {
                var user = await context.Users
                    .Include(e => e.Claims)
                    .Include(e => e.Logins)
                    .Include(e => e.Tokens)
                    .Include(e => e.UserRoles)
                    .SingleAsync(u => u.Id == userId);

                Assert.Equal(3, user.Claims.Count);
                Assert.Equal(2, user.Logins.Count);
                Assert.Equal(1, user.Tokens.Count);
                Assert.Equal(2, user.UserRoles.Count);
            });
    }

    protected override List<EntityTypeMapping> ExpectedMappings
        =>
        [
            new EntityTypeMapping
            {
                Name = "Microsoft.EntityFrameworkCore.CustomRoleClaimInt",
                TableName = "AspNetRoleClaims",
                PrimaryKey = "Key: CustomRoleClaimInt.Id PK",
                Properties =
                {
                    "Property: CustomRoleClaimInt.Id (int) Required PK AfterSave:Throw ValueGenerated.OnAdd",
                    "Property: CustomRoleClaimInt.ClaimType (string)",
                    "Property: CustomRoleClaimInt.ClaimValue (string)",
                    "Property: CustomRoleClaimInt.RoleId (int) Required FK Index",
                },
                Indexes = { "{'RoleId'} ", },
                FKs = { "ForeignKey: CustomRoleClaimInt {'RoleId'} -> CustomRoleInt {'Id'} Required Cascade", },
            },
            new EntityTypeMapping
            {
                Name = "Microsoft.EntityFrameworkCore.CustomRoleInt",
                TableName = "AspNetRoles",
                PrimaryKey = "Key: CustomRoleInt.Id PK",
                Properties =
                {
                    "Property: CustomRoleInt.Id (int) Required PK AfterSave:Throw ValueGenerated.OnAdd",
                    "Property: CustomRoleInt.ConcurrencyStamp (string) Concurrency",
                    "Property: CustomRoleInt.Name (string) MaxLength(256)",
                    "Property: CustomRoleInt.NormalizedName (string) Index MaxLength(256)",
                },
                Indexes = { "{'NormalizedName'} Unique", },
            },
            new EntityTypeMapping
            {
                Name = "Microsoft.EntityFrameworkCore.CustomUserClaimInt",
                TableName = "AspNetUserClaims",
                PrimaryKey = "Key: CustomUserClaimInt.Id PK",
                Properties =
                {
                    "Property: CustomUserClaimInt.Id (int) Required PK AfterSave:Throw ValueGenerated.OnAdd",
                    "Property: CustomUserClaimInt.ClaimType (string)",
                    "Property: CustomUserClaimInt.ClaimValue (string)",
                    "Property: CustomUserClaimInt.UserId (int) Required FK Index",
                },
                Indexes = { "{'UserId'} ", },
                FKs = { "ForeignKey: CustomUserClaimInt {'UserId'} -> CustomUserInt {'Id'} Required Cascade ToDependent: Claims", },
            },
            new EntityTypeMapping
            {
                Name = "Microsoft.EntityFrameworkCore.CustomUserInt",
                TableName = "AspNetUsers",
                PrimaryKey = "Key: CustomUserInt.Id PK",
                Properties =
                {
                    "Property: CustomUserInt.Id (int) Required PK AfterSave:Throw ValueGenerated.OnAdd",
                    "Property: CustomUserInt.AccessFailedCount (int) Required",
                    "Property: CustomUserInt.ConcurrencyStamp (string) Concurrency",
                    "Property: CustomUserInt.CustomTag (string)",
                    "Property: CustomUserInt.Email (string) MaxLength(256)",
                    "Property: CustomUserInt.EmailConfirmed (bool) Required",
                    "Property: CustomUserInt.LockoutEnabled (bool) Required",
                    "Property: CustomUserInt.LockoutEnd (DateTimeOffset?)",
                    "Property: CustomUserInt.NormalizedEmail (string) Index MaxLength(256)",
                    "Property: CustomUserInt.NormalizedUserName (string) Index MaxLength(256)",
                    "Property: CustomUserInt.PasswordHash (string)",
                    "Property: CustomUserInt.PhoneNumber (string)",
                    "Property: CustomUserInt.PhoneNumberConfirmed (bool) Required",
                    "Property: CustomUserInt.SecurityStamp (string)",
                    "Property: CustomUserInt.TwoFactorEnabled (bool) Required",
                    "Property: CustomUserInt.UserName (string) MaxLength(256)",
                },
                Indexes =
                {
                    "{'NormalizedEmail'} ", "{'NormalizedUserName'} Unique",
                },
                Navigations =
                {
                    "Navigation: CustomUserInt.Claims (ICollection<CustomUserClaimInt>) Collection ToDependent CustomUserClaimInt",
                    "Navigation: CustomUserInt.Logins (ICollection<CustomUserLoginInt>) Collection ToDependent CustomUserLoginInt",
                    "Navigation: CustomUserInt.Tokens (ICollection<CustomUserTokenInt>) Collection ToDependent CustomUserTokenInt",
                    "Navigation: CustomUserInt.UserRoles (ICollection<CustomUserRoleInt>) Collection ToDependent CustomUserRoleInt",
                },
            },
            new EntityTypeMapping
            {
                Name = "Microsoft.EntityFrameworkCore.CustomUserLoginInt",
                TableName = "AspNetUserLogins",
                PrimaryKey = "Key: CustomUserLoginInt.LoginProvider, CustomUserLoginInt.ProviderKey PK",
                Properties =
                {
                    "Property: CustomUserLoginInt.LoginProvider (string) Required PK AfterSave:Throw",
                    "Property: CustomUserLoginInt.ProviderKey (string) Required PK AfterSave:Throw",
                    "Property: CustomUserLoginInt.ProviderDisplayName (string)",
                    "Property: CustomUserLoginInt.UserId (int) Required FK Index",
                },
                Indexes = { "{'UserId'} ", },
                FKs = { "ForeignKey: CustomUserLoginInt {'UserId'} -> CustomUserInt {'Id'} Required Cascade ToDependent: Logins", },
            },
            new EntityTypeMapping
            {
                Name = "Microsoft.EntityFrameworkCore.CustomUserRoleInt",
                TableName = "AspNetUserRoles",
                PrimaryKey = "Key: CustomUserRoleInt.UserId, CustomUserRoleInt.RoleId PK",
                Properties =
                {
                    "Property: CustomUserRoleInt.UserId (int) Required PK FK AfterSave:Throw",
                    "Property: CustomUserRoleInt.RoleId (int) Required PK FK Index AfterSave:Throw",
                },
                Indexes = { "{'RoleId'} ", },
                FKs =
                {
                    "ForeignKey: CustomUserRoleInt {'RoleId'} -> CustomRoleInt {'Id'} Required Cascade",
                    "ForeignKey: CustomUserRoleInt {'UserId'} -> CustomUserInt {'Id'} Required Cascade ToDependent: UserRoles",
                },
            },
            new EntityTypeMapping
            {
                Name = "Microsoft.EntityFrameworkCore.CustomUserTokenInt",
                TableName = "AspNetUserTokens",
                PrimaryKey = "Key: CustomUserTokenInt.UserId, CustomUserTokenInt.LoginProvider, CustomUserTokenInt.Name PK",
                Properties =
                {
                    "Property: CustomUserTokenInt.UserId (int) Required PK FK AfterSave:Throw",
                    "Property: CustomUserTokenInt.LoginProvider (string) Required PK AfterSave:Throw",
                    "Property: CustomUserTokenInt.Name (string) Required PK AfterSave:Throw",
                    "Property: CustomUserTokenInt.Value (string)",
                },
                FKs = { "ForeignKey: CustomUserTokenInt {'UserId'} -> CustomUserInt {'Id'} Required Cascade ToDependent: Tokens", },
            }
        ];
}

public class CustomTypesIdentityContextInt(DbContextOptions options) : IdentityDbContext<CustomUserInt, CustomRoleInt, int, CustomUserClaimInt, CustomUserRoleInt,
    CustomUserLoginInt, CustomRoleClaimInt, CustomUserTokenInt>(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CustomUserInt>(
            b =>
            {
                b.HasMany(e => e.Claims).WithOne().HasForeignKey(uc => uc.UserId).IsRequired();
                b.HasMany(e => e.Logins).WithOne().HasForeignKey(ul => ul.UserId).IsRequired();
                b.HasMany(e => e.Tokens).WithOne().HasForeignKey(ut => ut.UserId).IsRequired();
                b.HasMany(e => e.UserRoles).WithOne().HasForeignKey(ur => ur.UserId).IsRequired();
            });
    }
}

public class CustomUserInt : IdentityUser<int>
{
    public string CustomTag { get; set; }
    public virtual ICollection<CustomUserClaimInt> Claims { get; set; }
    public virtual ICollection<CustomUserLoginInt> Logins { get; set; }
    public virtual ICollection<CustomUserTokenInt> Tokens { get; set; }
    public virtual ICollection<CustomUserRoleInt> UserRoles { get; set; }
}

public class CustomRoleInt : IdentityRole<int>;

public class CustomUserClaimInt : IdentityUserClaim<int>;

public class CustomUserRoleInt : IdentityUserRole<int>;

public class CustomUserLoginInt : IdentityUserLogin<int>;

public class CustomRoleClaimInt : IdentityRoleClaim<int>;

public class CustomUserTokenInt : IdentityUserToken<int>;
