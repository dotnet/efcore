// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Microsoft.EntityFrameworkCore;

public abstract class AspNetIdentityCustomTypesDefaultTestBase<TFixture>
    : AspNetIdentityTestBase<TFixture, CustomTypesIdentityContext, CustomUserString, CustomRoleString, string, CustomUserClaimString,
        CustomUserRoleString, CustomUserLoginString, CustomRoleClaimString, CustomUserTokenString>
    where TFixture : AspNetIdentityTestBase<TFixture, CustomTypesIdentityContext, CustomUserString, CustomRoleString, string,
        CustomUserClaimString, CustomUserRoleString, CustomUserLoginString, CustomRoleClaimString, CustomUserTokenString>.
    AspNetIdentityFixtureBase
{
    protected AspNetIdentityCustomTypesDefaultTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    [ConditionalFact]
    public async Task Can_lazy_load_User_navigations()
    {
        var userId = "";

        await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var user = new CustomUserString { NormalizedUserName = "wendy" };
                await CreateUser(context, user);
                userId = user.Id;
            },
            async context =>
            {
                var user = await context.Users.SingleAsync(u => u.Id == userId);

                Assert.Equal(3, user.Claims.Count);
                Assert.Equal(2, user.Logins.Count);
                Assert.Equal(1, user.Tokens.Count);
                Assert.Equal(2, user.UserRoles.Count);
            });
    }

    [ConditionalFact]
    public async Task Can_lazy_load_Role_navigations()
        => await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                await CreateUser(context, new CustomUserString { NormalizedUserName = "wendy" });
            },
            async context =>
            {
                var role = await context.Roles.OrderBy(e => e.NormalizedName).FirstAsync();

                Assert.Equal(2, role.RoleClaims.Count);
                Assert.Equal(1, role.UserRoles.Count);
            });

    [ConditionalFact]
    public async Task Can_lazy_load_User_navigations_many_to_many()
    {
        var userId = "";

        await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var user = new CustomUserString { NormalizedUserName = "wendy" };
                await CreateUser(context, user);
                userId = user.Id;
            },
            async context =>
            {
                var user = await context.Users.SingleAsync(u => u.Id == userId);

                Assert.Equal(2, user.Roles.Count);
            });
    }

    [ConditionalFact]
    public async Task Can_lazy_load_Role_navigations_many_to_many()
        => await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                await CreateUser(context, new CustomUserString { NormalizedUserName = "wendy" });
            },
            async context =>
            {
                var role = await context.Roles.OrderBy(e => e.NormalizedName).FirstAsync();

                Assert.Equal(1, role.Users.Count);
            });

    [ConditionalFact]
    public async Task Can_lazy_load_UserRole_navigations()
        => await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                await CreateUser(context, new CustomUserString { NormalizedUserName = "wendy" });
            },
            async context =>
            {
                var userRole = await context.UserRoles.OrderBy(e => e.Role.Name).FirstAsync();

                Assert.NotNull(userRole.Role);
                Assert.NotNull(userRole.User);
            });

    [ConditionalFact]
    public async Task Can_lazy_load_UserClaim_navigations()
        => await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                await CreateUser(context, new CustomUserString { NormalizedUserName = "wendy" });
            },
            async context =>
            {
                var userClaim = await context.UserClaims.OrderBy(e => e.ClaimType).ThenBy(e => e.ClaimValue).FirstAsync();
                Assert.NotNull(userClaim.User);
            });

    [ConditionalFact]
    public async Task Can_lazy_load_UserLogin_navigations()
        => await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                await CreateUser(context, new CustomUserString { NormalizedUserName = "wendy" });
            },
            async context =>
            {
                var userLogin = await context.UserLogins.OrderBy(e => e.LoginProvider).FirstAsync();
                Assert.NotNull(userLogin.User);
            });

    [ConditionalFact]
    public async Task Can_lazy_load_RoleClaim_navigations()
        => await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                await CreateUser(context, new CustomUserString { NormalizedUserName = "wendy" });
            },
            async context =>
            {
                var roleClaim = await context.RoleClaims.OrderBy(e => e.Role.Name).FirstAsync();
                Assert.NotNull(roleClaim.Role);
            });

    [ConditionalFact]
    public async Task Can_lazy_load_UserToken_navigations()
        => await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                await CreateUser(context, new CustomUserString { NormalizedUserName = "wendy" });
            },
            async context =>
            {
                var userToken = await context.UserTokens.OrderBy(e => e.Name).FirstAsync();
                Assert.NotNull(userToken.User);
            });

    protected override List<EntityTypeMapping> ExpectedMappings
        =>
        [
            new EntityTypeMapping
            {
                Name = "Microsoft.EntityFrameworkCore.CustomRoleClaimString",
                TableName = "MyRoleClaims",
                PrimaryKey = "Key: CustomRoleClaimString.Id PK",
                Properties =
                {
                    "Property: CustomRoleClaimString.Id (int) Required PK AfterSave:Throw ValueGenerated.OnAdd",
                    "Property: CustomRoleClaimString.ClaimType (string)",
                    "Property: CustomRoleClaimString.ClaimValue (string)",
                    "Property: CustomRoleClaimString.RoleId (string) Required FK Index",
                },
                Indexes = { "{'RoleId'} ", },
                FKs =
                {
                    "ForeignKey: CustomRoleClaimString {'RoleId'} -> CustomRoleString {'Id'} Required Cascade ToDependent: RoleClaims ToPrincipal: Role",
                },
                Navigations =
                {
                    "Navigation: CustomRoleClaimString.Role (CustomRoleString) ToPrincipal CustomRoleString Inverse: RoleClaims PropertyAccessMode.Field",
                },
            },
            new EntityTypeMapping
            {
                Name = "Microsoft.EntityFrameworkCore.CustomRoleString",
                TableName = "MyRoles",
                PrimaryKey = "Key: CustomRoleString.Id PK",
                Properties =
                {
                    "Property: CustomRoleString.Id (string) Required PK AfterSave:Throw",
                    "Property: CustomRoleString.ConcurrencyStamp (string) Concurrency",
                    "Property: CustomRoleString.Name (string) MaxLength(256)",
                    "Property: CustomRoleString.NormalizedName (string) Index MaxLength(256)",
                },
                Indexes = { "{'NormalizedName'} Unique", },
                Navigations =
                {
                    "Navigation: CustomRoleString.RoleClaims (ICollection<CustomRoleClaimString>) Collection ToDependent CustomRoleClaimString Inverse: Role PropertyAccessMode.Field",
                    "Navigation: CustomRoleString.UserRoles (ICollection<CustomUserRoleString>) Collection ToDependent CustomUserRoleString Inverse: Role PropertyAccessMode.Field",
                },
                SkipNavigations =
                {
                    "SkipNavigation: CustomRoleString.Users (ICollection<CustomUserString>) CollectionCustomUserString Inverse: Roles PropertyAccessMode.Field"
                }
            },
            new EntityTypeMapping
            {
                Name = "Microsoft.EntityFrameworkCore.CustomUserClaimString",
                TableName = "MyUserClaims",
                PrimaryKey = "Key: CustomUserClaimString.Id PK",
                Properties =
                {
                    "Property: CustomUserClaimString.Id (int) Required PK AfterSave:Throw ValueGenerated.OnAdd",
                    "Property: CustomUserClaimString.ClaimType (string)",
                    "Property: CustomUserClaimString.ClaimValue (string)",
                    "Property: CustomUserClaimString.UserId (string) Required FK Index",
                },
                Indexes = { "{'UserId'} ", },
                FKs =
                {
                    "ForeignKey: CustomUserClaimString {'UserId'} -> CustomUserString {'Id'} Required Cascade ToDependent: Claims ToPrincipal: User",
                },
                Navigations =
                {
                    "Navigation: CustomUserClaimString.User (CustomUserString) ToPrincipal CustomUserString Inverse: Claims PropertyAccessMode.Field",
                },
            },
            new EntityTypeMapping
            {
                Name = "Microsoft.EntityFrameworkCore.CustomUserLoginString",
                TableName = "MyUserLogins",
                PrimaryKey = "Key: CustomUserLoginString.LoginProvider, CustomUserLoginString.ProviderKey PK",
                Properties =
                {
                    "Property: CustomUserLoginString.LoginProvider (string) Required PK AfterSave:Throw",
                    "Property: CustomUserLoginString.ProviderKey (string) Required PK AfterSave:Throw",
                    "Property: CustomUserLoginString.ProviderDisplayName (string)",
                    "Property: CustomUserLoginString.UserId (string) Required FK Index",
                },
                Indexes = { "{'UserId'} ", },
                FKs =
                {
                    "ForeignKey: CustomUserLoginString {'UserId'} -> CustomUserString {'Id'} Required Cascade ToDependent: Logins ToPrincipal: User",
                },
                Navigations =
                {
                    "Navigation: CustomUserLoginString.User (CustomUserString) ToPrincipal CustomUserString Inverse: Logins PropertyAccessMode.Field",
                },
            },
            new EntityTypeMapping
            {
                Name = "Microsoft.EntityFrameworkCore.CustomUserRoleString",
                TableName = "MyUserRoles",
                PrimaryKey = "Key: CustomUserRoleString.UserId, CustomUserRoleString.RoleId PK",
                Properties =
                {
                    "Property: CustomUserRoleString.UserId (string) Required PK FK AfterSave:Throw",
                    "Property: CustomUserRoleString.RoleId (string) Required PK FK Index AfterSave:Throw",
                },
                Indexes = { "{'RoleId'} ", },
                FKs =
                {
                    "ForeignKey: CustomUserRoleString {'RoleId'} -> CustomRoleString {'Id'} Required Cascade ToDependent: UserRoles ToPrincipal: Role",
                    "ForeignKey: CustomUserRoleString {'UserId'} -> CustomUserString {'Id'} Required Cascade ToDependent: UserRoles ToPrincipal: User",
                },
                Navigations =
                {
                    "Navigation: CustomUserRoleString.Role (CustomRoleString) ToPrincipal CustomRoleString Inverse: UserRoles PropertyAccessMode.Field",
                    "Navigation: CustomUserRoleString.User (CustomUserString) ToPrincipal CustomUserString Inverse: UserRoles PropertyAccessMode.Field",
                }
            },
            new EntityTypeMapping
            {
                Name = "Microsoft.EntityFrameworkCore.CustomUserString",
                TableName = "MyUsers",
                PrimaryKey = "Key: CustomUserString.Id PK",
                Properties =
                {
                    "Property: CustomUserString.Id (string) Required PK AfterSave:Throw",
                    "Property: CustomUserString.AccessFailedCount (int) Required",
                    "Property: CustomUserString.ConcurrencyStamp (string) Concurrency",
                    "Property: CustomUserString.CustomTag (string)",
                    "Property: CustomUserString.Email (string) MaxLength(128)",
                    "Property: CustomUserString.EmailConfirmed (bool) Required",
                    "Property: CustomUserString.LockoutEnabled (bool) Required",
                    "Property: CustomUserString.LockoutEnd (DateTimeOffset?)",
                    "Property: CustomUserString.NormalizedEmail (string) Index MaxLength(128)",
                    "Property: CustomUserString.NormalizedUserName (string) Index MaxLength(128)",
                    "Property: CustomUserString.PasswordHash (string)",
                    "Property: CustomUserString.PhoneNumber (string)",
                    "Property: CustomUserString.PhoneNumberConfirmed (bool) Required",
                    "Property: CustomUserString.SecurityStamp (string)",
                    "Property: CustomUserString.TwoFactorEnabled (bool) Required",
                    "Property: CustomUserString.UserName (string) MaxLength(128)",
                },
                Indexes =
                {
                    "{'NormalizedEmail'} ", "{'NormalizedUserName'} Unique",
                },
                Navigations =
                {
                    "Navigation: CustomUserString.Claims (ICollection<CustomUserClaimString>) Collection ToDependent CustomUserClaimString Inverse: User PropertyAccessMode.Field",
                    "Navigation: CustomUserString.Logins (ICollection<CustomUserLoginString>) Collection ToDependent CustomUserLoginString Inverse: User PropertyAccessMode.Field",
                    "Navigation: CustomUserString.Tokens (ICollection<CustomUserTokenString>) Collection ToDependent CustomUserTokenString Inverse: User PropertyAccessMode.Field",
                    "Navigation: CustomUserString.UserRoles (ICollection<CustomUserRoleString>) Collection ToDependent CustomUserRoleString Inverse: User PropertyAccessMode.Field",
                },
                SkipNavigations =
                {
                    "SkipNavigation: CustomUserString.Roles (ICollection<CustomRoleString>) CollectionCustomRoleString Inverse: Users PropertyAccessMode.Field"
                }
            },
            new EntityTypeMapping
            {
                Name = "Microsoft.EntityFrameworkCore.CustomUserTokenString",
                TableName = "MyUserTokens",
                PrimaryKey = "Key: CustomUserTokenString.UserId, CustomUserTokenString.LoginProvider, CustomUserTokenString.Name PK",
                Properties =
                {
                    "Property: CustomUserTokenString.UserId (string) Required PK FK AfterSave:Throw",
                    "Property: CustomUserTokenString.LoginProvider (string) Required PK AfterSave:Throw MaxLength(128)",
                    "Property: CustomUserTokenString.Name (string) Required PK AfterSave:Throw MaxLength(128)",
                    "Property: CustomUserTokenString.Value (string)",
                },
                FKs =
                {
                    "ForeignKey: CustomUserTokenString {'UserId'} -> CustomUserString {'Id'} Required Cascade ToDependent: Tokens ToPrincipal: User",
                },
                Navigations =
                {
                    "Navigation: CustomUserTokenString.User (CustomUserString) ToPrincipal CustomUserString Inverse: Tokens PropertyAccessMode.Field",
                },
            }
        ];
}

public class CustomTypesIdentityContext(DbContextOptions options) : IdentityDbContext<CustomUserString, CustomRoleString, string, CustomUserClaimString,
    CustomUserRoleString,
    CustomUserLoginString, CustomRoleClaimString, CustomUserTokenString>(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("notdbo");

        modelBuilder.Entity<CustomUserString>(
            b =>
            {
                b.HasMany(e => e.Roles)
                    .WithMany(e => e.Users)
                    .UsingEntity<CustomUserRoleString>(
                        j => j.HasOne(e => e.Role).WithMany(e => e.UserRoles).HasForeignKey(e => e.RoleId),
                        j => j.HasOne(e => e.User).WithMany(e => e.UserRoles).HasForeignKey(e => e.RoleId));

                b.HasMany(e => e.Claims).WithOne(e => e.User).HasForeignKey(uc => uc.UserId).IsRequired();
                b.HasMany(e => e.Logins).WithOne(e => e.User).HasForeignKey(ul => ul.UserId).IsRequired();
                b.HasMany(e => e.Tokens).WithOne(e => e.User).HasForeignKey(ut => ut.UserId).IsRequired();
                b.HasMany(e => e.UserRoles).WithOne(e => e.User).HasForeignKey(ur => ur.UserId).IsRequired();
                b.ToTable("MyUsers");
                b.Property(u => u.UserName).HasMaxLength(128);
                b.Property(u => u.NormalizedUserName).HasMaxLength(128);
                b.Property(u => u.Email).HasMaxLength(128);
                b.Property(u => u.NormalizedEmail).HasMaxLength(128);
            });

        modelBuilder.Entity<CustomRoleString>(
            b =>
            {
                b.HasMany(e => e.UserRoles).WithOne(e => e.Role).HasForeignKey(ur => ur.RoleId).IsRequired();
                b.HasMany(e => e.RoleClaims).WithOne(e => e.Role).HasForeignKey(rc => rc.RoleId).IsRequired();
                b.ToTable("MyRoles");
            });

        modelBuilder.Entity<CustomUserClaimString>(
            b =>
            {
                b.ToTable("MyUserClaims");
            });

        modelBuilder.Entity<CustomUserLoginString>(
            b =>
            {
                b.ToTable("MyUserLogins");
            });

        modelBuilder.Entity<CustomUserTokenString>(
            b =>
            {
                b.Property(t => t.LoginProvider).HasMaxLength(128);
                b.Property(t => t.Name).HasMaxLength(128);
                b.ToTable("MyUserTokens");
            });

        modelBuilder.Entity<CustomRoleClaimString>(
            b =>
            {
                b.ToTable("MyRoleClaims");
            });

        modelBuilder.Entity<CustomUserRoleString>(
            b =>
            {
                b.ToTable("MyUserRoles");
            });
    }
}

public class CustomUserString : IdentityUser<string>
{
    public CustomUserString()
    {
        Id = Guid.NewGuid().ToString();
    }

    public string CustomTag { get; set; }

    public virtual ICollection<CustomRoleString> Roles { get; set; }

    public virtual ICollection<CustomUserClaimString> Claims { get; set; }
    public virtual ICollection<CustomUserLoginString> Logins { get; set; }
    public virtual ICollection<CustomUserTokenString> Tokens { get; set; }
    public virtual ICollection<CustomUserRoleString> UserRoles { get; set; }
}

public class CustomRoleString : IdentityRole<string>
{
    public CustomRoleString()
    {
        Id = Guid.NewGuid().ToString();
    }

    public virtual ICollection<CustomUserString> Users { get; set; }

    public virtual ICollection<CustomUserRoleString> UserRoles { get; set; }
    public virtual ICollection<CustomRoleClaimString> RoleClaims { get; set; }
}

public class CustomUserRoleString : IdentityUserRole<string>
{
    public virtual CustomUserString User { get; set; }
    public virtual CustomRoleString Role { get; set; }
}

public class CustomUserClaimString : IdentityUserClaim<string>
{
    public virtual CustomUserString User { get; set; }
}

public class CustomUserLoginString : IdentityUserLogin<string>
{
    public virtual CustomUserString User { get; set; }
}

public class CustomRoleClaimString : IdentityRoleClaim<string>
{
    public virtual CustomRoleString Role { get; set; }
}

public class CustomUserTokenString : IdentityUserToken<string>
{
    public virtual CustomUserString User { get; set; }
}
