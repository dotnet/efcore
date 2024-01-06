// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Microsoft.EntityFrameworkCore;

public abstract class AspNetIdentityDefaultTestBase<TFixture>
    : AspNetIdentityTestBase<TFixture, IdentityDbContext, IdentityUser, IdentityRole, string, IdentityUserClaim<string>,
        IdentityUserRole<string>, IdentityUserLogin<string>, IdentityRoleClaim<string>, IdentityUserToken<string>>
    where TFixture : AspNetIdentityTestBase<TFixture, IdentityDbContext, IdentityUser, IdentityRole, string, IdentityUserClaim<string>,
        IdentityUserRole<string>, IdentityUserLogin<string>, IdentityRoleClaim<string>, IdentityUserToken<string>>.AspNetIdentityFixtureBase
{
    protected AspNetIdentityDefaultTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    protected override List<EntityTypeMapping> ExpectedMappings
        =>
        [
            new EntityTypeMapping
            {
                Name = "Microsoft.AspNetCore.Identity.IdentityRole",
                TableName = "AspNetRoles",
                PrimaryKey = "Key: IdentityRole.Id PK",
                Properties =
                {
                    "Property: IdentityRole.Id (string) Required PK AfterSave:Throw",
                    "Property: IdentityRole.ConcurrencyStamp (string) Concurrency",
                    "Property: IdentityRole.Name (string) MaxLength(256)",
                    "Property: IdentityRole.NormalizedName (string) Index MaxLength(256)",
                },
                Indexes = { "{'NormalizedName'} Unique", },
            },
            new EntityTypeMapping
            {
                Name = "Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>",
                TableName = "AspNetRoleClaims",
                PrimaryKey = "Key: IdentityRoleClaim<string>.Id PK",
                Properties =
                {
                    "Property: IdentityRoleClaim<string>.Id (int) Required PK AfterSave:Throw ValueGenerated.OnAdd",
                    "Property: IdentityRoleClaim<string>.ClaimType (string)",
                    "Property: IdentityRoleClaim<string>.ClaimValue (string)",
                    "Property: IdentityRoleClaim<string>.RoleId (string) Required FK Index",
                },
                Indexes = { "{'RoleId'} ", },
                FKs = { "ForeignKey: IdentityRoleClaim<string> {'RoleId'} -> IdentityRole {'Id'} Required Cascade", },
            },
            new EntityTypeMapping
            {
                Name = "Microsoft.AspNetCore.Identity.IdentityUser",
                TableName = "AspNetUsers",
                PrimaryKey = "Key: IdentityUser.Id PK",
                Properties =
                {
                    "Property: IdentityUser.Id (string) Required PK AfterSave:Throw",
                    "Property: IdentityUser.AccessFailedCount (int) Required",
                    "Property: IdentityUser.ConcurrencyStamp (string) Concurrency",
                    "Property: IdentityUser.Email (string) MaxLength(256)",
                    "Property: IdentityUser.EmailConfirmed (bool) Required",
                    "Property: IdentityUser.LockoutEnabled (bool) Required",
                    "Property: IdentityUser.LockoutEnd (DateTimeOffset?)",
                    "Property: IdentityUser.NormalizedEmail (string) Index MaxLength(256)",
                    "Property: IdentityUser.NormalizedUserName (string) Index MaxLength(256)",
                    "Property: IdentityUser.PasswordHash (string)",
                    "Property: IdentityUser.PhoneNumber (string)",
                    "Property: IdentityUser.PhoneNumberConfirmed (bool) Required",
                    "Property: IdentityUser.SecurityStamp (string)",
                    "Property: IdentityUser.TwoFactorEnabled (bool) Required",
                    "Property: IdentityUser.UserName (string) MaxLength(256)",
                },
                Indexes =
                {
                    "{'NormalizedEmail'} ", "{'NormalizedUserName'} Unique",
                },
            },
            new EntityTypeMapping
            {
                Name = "Microsoft.AspNetCore.Identity.IdentityUserClaim<string>",
                TableName = "AspNetUserClaims",
                PrimaryKey = "Key: IdentityUserClaim<string>.Id PK",
                Properties =
                {
                    "Property: IdentityUserClaim<string>.Id (int) Required PK AfterSave:Throw ValueGenerated.OnAdd",
                    "Property: IdentityUserClaim<string>.ClaimType (string)",
                    "Property: IdentityUserClaim<string>.ClaimValue (string)",
                    "Property: IdentityUserClaim<string>.UserId (string) Required FK Index",
                },
                Indexes = { "{'UserId'} ", },
                FKs = { "ForeignKey: IdentityUserClaim<string> {'UserId'} -> IdentityUser {'Id'} Required Cascade", },
            },
            new EntityTypeMapping
            {
                Name = "Microsoft.AspNetCore.Identity.IdentityUserLogin<string>",
                TableName = "AspNetUserLogins",
                PrimaryKey = "Key: IdentityUserLogin<string>.LoginProvider, IdentityUserLogin<string>.ProviderKey PK",
                Properties =
                {
                    "Property: IdentityUserLogin<string>.LoginProvider (string) Required PK AfterSave:Throw",
                    "Property: IdentityUserLogin<string>.ProviderKey (string) Required PK AfterSave:Throw",
                    "Property: IdentityUserLogin<string>.ProviderDisplayName (string)",
                    "Property: IdentityUserLogin<string>.UserId (string) Required FK Index",
                },
                Indexes = { "{'UserId'} ", },
                FKs = { "ForeignKey: IdentityUserLogin<string> {'UserId'} -> IdentityUser {'Id'} Required Cascade", },
            },
            new EntityTypeMapping
            {
                Name = "Microsoft.AspNetCore.Identity.IdentityUserRole<string>",
                TableName = "AspNetUserRoles",
                PrimaryKey = "Key: IdentityUserRole<string>.UserId, IdentityUserRole<string>.RoleId PK",
                Properties =
                {
                    "Property: IdentityUserRole<string>.UserId (string) Required PK FK AfterSave:Throw",
                    "Property: IdentityUserRole<string>.RoleId (string) Required PK FK Index AfterSave:Throw",
                },
                Indexes = { "{'RoleId'} ", },
                FKs =
                {
                    "ForeignKey: IdentityUserRole<string> {'RoleId'} -> IdentityRole {'Id'} Required Cascade",
                    "ForeignKey: IdentityUserRole<string> {'UserId'} -> IdentityUser {'Id'} Required Cascade",
                },
            },
            new EntityTypeMapping
            {
                Name = "Microsoft.AspNetCore.Identity.IdentityUserToken<string>",
                TableName = "AspNetUserTokens",
                PrimaryKey =
                    "Key: IdentityUserToken<string>.UserId, IdentityUserToken<string>.LoginProvider, IdentityUserToken<string>.Name PK",
                Properties =
                {
                    "Property: IdentityUserToken<string>.UserId (string) Required PK FK AfterSave:Throw",
                    "Property: IdentityUserToken<string>.LoginProvider (string) Required PK AfterSave:Throw",
                    "Property: IdentityUserToken<string>.Name (string) Required PK AfterSave:Throw",
                    "Property: IdentityUserToken<string>.Value (string)",
                },
                FKs = { "ForeignKey: IdentityUserToken<string> {'UserId'} -> IdentityUser {'Id'} Required Cascade", },
            }
        ];
}
