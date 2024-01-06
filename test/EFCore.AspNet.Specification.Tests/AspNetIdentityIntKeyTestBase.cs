// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Microsoft.EntityFrameworkCore;

public abstract class AspNetIdentityIntKeyTestBase<TFixture>
    : AspNetIdentityTestBase<TFixture, IdentityDbContext<IdentityUser<int>, IdentityRole<int>, int>,
        IdentityUser<int>, IdentityRole<int>, int, IdentityUserClaim<int>, IdentityUserRole<int>, IdentityUserLogin<int>,
        IdentityRoleClaim<int>, IdentityUserToken<int>>
    where TFixture : AspNetIdentityTestBase<TFixture, IdentityDbContext<IdentityUser<int>, IdentityRole<int>, int>, IdentityUser<int>,
        IdentityRole<int>, int, IdentityUserClaim<int>, IdentityUserRole<int>, IdentityUserLogin<int>, IdentityRoleClaim<int>,
        IdentityUserToken<int>>.AspNetIdentityFixtureBase
{
    protected AspNetIdentityIntKeyTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    protected override List<EntityTypeMapping> ExpectedMappings
        =>
        [
            new EntityTypeMapping
            {
                Name = "Microsoft.AspNetCore.Identity.IdentityRole<int>",
                TableName = "AspNetRoles",
                PrimaryKey = "Key: IdentityRole<int>.Id PK",
                Properties =
                {
                    "Property: IdentityRole<int>.Id (int) Required PK AfterSave:Throw ValueGenerated.OnAdd",
                    "Property: IdentityRole<int>.ConcurrencyStamp (string) Concurrency",
                    "Property: IdentityRole<int>.Name (string) MaxLength(256)",
                    "Property: IdentityRole<int>.NormalizedName (string) Index MaxLength(256)",
                },
                Indexes = { "{'NormalizedName'} Unique", },
            },
            new EntityTypeMapping
            {
                Name = "Microsoft.AspNetCore.Identity.IdentityRoleClaim<int>",
                TableName = "AspNetRoleClaims",
                PrimaryKey = "Key: IdentityRoleClaim<int>.Id PK",
                Properties =
                {
                    "Property: IdentityRoleClaim<int>.Id (int) Required PK AfterSave:Throw ValueGenerated.OnAdd",
                    "Property: IdentityRoleClaim<int>.ClaimType (string)",
                    "Property: IdentityRoleClaim<int>.ClaimValue (string)",
                    "Property: IdentityRoleClaim<int>.RoleId (int) Required FK Index",
                },
                Indexes = { "{'RoleId'} ", },
                FKs = { "ForeignKey: IdentityRoleClaim<int> {'RoleId'} -> IdentityRole<int> {'Id'} Required Cascade", },
            },
            new EntityTypeMapping
            {
                Name = "Microsoft.AspNetCore.Identity.IdentityUser<int>",
                TableName = "AspNetUsers",
                PrimaryKey = "Key: IdentityUser<int>.Id PK",
                Properties =
                {
                    "Property: IdentityUser<int>.Id (int) Required PK AfterSave:Throw ValueGenerated.OnAdd",
                    "Property: IdentityUser<int>.AccessFailedCount (int) Required",
                    "Property: IdentityUser<int>.ConcurrencyStamp (string) Concurrency",
                    "Property: IdentityUser<int>.Email (string) MaxLength(256)",
                    "Property: IdentityUser<int>.EmailConfirmed (bool) Required",
                    "Property: IdentityUser<int>.LockoutEnabled (bool) Required",
                    "Property: IdentityUser<int>.LockoutEnd (DateTimeOffset?)",
                    "Property: IdentityUser<int>.NormalizedEmail (string) Index MaxLength(256)",
                    "Property: IdentityUser<int>.NormalizedUserName (string) Index MaxLength(256)",
                    "Property: IdentityUser<int>.PasswordHash (string)",
                    "Property: IdentityUser<int>.PhoneNumber (string)",
                    "Property: IdentityUser<int>.PhoneNumberConfirmed (bool) Required",
                    "Property: IdentityUser<int>.SecurityStamp (string)",
                    "Property: IdentityUser<int>.TwoFactorEnabled (bool) Required",
                    "Property: IdentityUser<int>.UserName (string) MaxLength(256)",
                },
                Indexes =
                {
                    "{'NormalizedEmail'} ", "{'NormalizedUserName'} Unique",
                },
            },
            new EntityTypeMapping
            {
                Name = "Microsoft.AspNetCore.Identity.IdentityUserClaim<int>",
                TableName = "AspNetUserClaims",
                PrimaryKey = "Key: IdentityUserClaim<int>.Id PK",
                Properties =
                {
                    "Property: IdentityUserClaim<int>.Id (int) Required PK AfterSave:Throw ValueGenerated.OnAdd",
                    "Property: IdentityUserClaim<int>.ClaimType (string)",
                    "Property: IdentityUserClaim<int>.ClaimValue (string)",
                    "Property: IdentityUserClaim<int>.UserId (int) Required FK Index",
                },
                Indexes = { "{'UserId'} ", },
                FKs = { "ForeignKey: IdentityUserClaim<int> {'UserId'} -> IdentityUser<int> {'Id'} Required Cascade", },
            },
            new EntityTypeMapping
            {
                Name = "Microsoft.AspNetCore.Identity.IdentityUserLogin<int>",
                TableName = "AspNetUserLogins",
                PrimaryKey = "Key: IdentityUserLogin<int>.LoginProvider, IdentityUserLogin<int>.ProviderKey PK",
                Properties =
                {
                    "Property: IdentityUserLogin<int>.LoginProvider (string) Required PK AfterSave:Throw",
                    "Property: IdentityUserLogin<int>.ProviderKey (string) Required PK AfterSave:Throw",
                    "Property: IdentityUserLogin<int>.ProviderDisplayName (string)",
                    "Property: IdentityUserLogin<int>.UserId (int) Required FK Index",
                },
                Indexes = { "{'UserId'} ", },
                FKs = { "ForeignKey: IdentityUserLogin<int> {'UserId'} -> IdentityUser<int> {'Id'} Required Cascade", },
            },
            new EntityTypeMapping
            {
                Name = "Microsoft.AspNetCore.Identity.IdentityUserRole<int>",
                TableName = "AspNetUserRoles",
                PrimaryKey = "Key: IdentityUserRole<int>.UserId, IdentityUserRole<int>.RoleId PK",
                Properties =
                {
                    "Property: IdentityUserRole<int>.UserId (int) Required PK FK AfterSave:Throw",
                    "Property: IdentityUserRole<int>.RoleId (int) Required PK FK Index AfterSave:Throw",
                },
                Indexes = { "{'RoleId'} ", },
                FKs =
                {
                    "ForeignKey: IdentityUserRole<int> {'RoleId'} -> IdentityRole<int> {'Id'} Required Cascade",
                    "ForeignKey: IdentityUserRole<int> {'UserId'} -> IdentityUser<int> {'Id'} Required Cascade",
                },
            },
            new EntityTypeMapping
            {
                Name = "Microsoft.AspNetCore.Identity.IdentityUserToken<int>",
                TableName = "AspNetUserTokens",
                PrimaryKey = "Key: IdentityUserToken<int>.UserId, IdentityUserToken<int>.LoginProvider, IdentityUserToken<int>.Name PK",
                Properties =
                {
                    "Property: IdentityUserToken<int>.UserId (int) Required PK FK AfterSave:Throw",
                    "Property: IdentityUserToken<int>.LoginProvider (string) Required PK AfterSave:Throw",
                    "Property: IdentityUserToken<int>.Name (string) Required PK AfterSave:Throw",
                    "Property: IdentityUserToken<int>.Value (string)",
                },
                FKs = { "ForeignKey: IdentityUserToken<int> {'UserId'} -> IdentityUser<int> {'Id'} Required Cascade", },
            }
        ];
}
