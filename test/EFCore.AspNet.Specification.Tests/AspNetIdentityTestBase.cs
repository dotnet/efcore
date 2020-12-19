// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class
        AspNetIdentityTestBase<TFixture, TContext, TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim,
            TUserToken> : IClassFixture<TFixture>
        where TFixture : AspNetIdentityTestBase<TFixture, TContext, TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim,
            TUserToken>.AspNetIdentityFixtureBase
        where TUser : IdentityUser<TKey>, new()
        where TRole : IdentityRole<TKey>, new()
        where TKey : IEquatable<TKey>
        where TUserClaim : IdentityUserClaim<TKey>, new()
        where TUserRole : IdentityUserRole<TKey>, new()
        where TUserLogin : IdentityUserLogin<TKey>, new()
        where TUserToken : IdentityUserToken<TKey>, new()
        where TRoleClaim : IdentityRoleClaim<TKey>, new()
        where TContext : IdentityUserContext<TUser, TKey, TUserClaim, TUserLogin, TUserToken>
    {
        protected AspNetIdentityTestBase(TFixture fixture)
            => Fixture = fixture;

        [ConditionalFact]
        public void Can_build_identity_model()
        {
            using (var context = CreateContext())
            {
                var entityTypeMappings = context.Model.GetEntityTypes().Select(e => new EntityTypeMapping(e)).ToList();

                EntityTypeMapping.AssertEqual(ExpectedMappings, entityTypeMappings);
            }
        }

        protected abstract List<EntityTypeMapping> ExpectedMappings { get; }

        [ConditionalFact]
        public async Task Can_call_UserStore_FindByNameAsync()
        {
            var user = new TUser { NormalizedUserName = "wendy" };

            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    await CreateUser(context, user);
                },
                async context =>
                {
                    using var userStore =
                        new UserStore<TUser, TRole, TContext, TKey, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim>(context);

                    Assert.Equal(user.Id, (await userStore.FindByNameAsync("wendy")).Id);
                });
        }

        [ConditionalFact]
        public async Task Can_call_UserStore_FindByEmailAsync()
        {
            var user = new TUser { NormalizedEmail = "wendy@example.com" };

            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    await CreateUser(context, user);
                },
                async context =>
                {
                    using var userStore =
                        new UserStore<TUser, TRole, TContext, TKey, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim>(context);

                    Assert.Equal(user.Id, (await userStore.FindByEmailAsync("wendy@example.com")).Id);
                });
        }

        [ConditionalFact]
        public async Task Can_call_UserStore_GetRolesAsync()
        {
            var user = new TUser { NormalizedEmail = "wendy@example.com" };

            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    await CreateUser(context, user);
                },
                async context =>
                {
                    using var userStore =
                        new UserStore<TUser, TRole, TContext, TKey, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim>(context);

                    var roles = await userStore.GetRolesAsync(user);
                    Assert.Equal(2, roles.Count);
                    Assert.Contains("Admin", roles);
                    Assert.Contains("Moderator", roles);
                });
        }

        [ConditionalFact]
        public async Task Can_call_UserStore_ReplaceClaimAsync()
        {
            var user = new TUser { NormalizedEmail = "wendy@example.com" };

            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    await CreateUser(context, user);
                },
                async context =>
                {
                    using var userStore =
                        new UserStore<TUser, TRole, TContext, TKey, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim>(context);

                    await userStore.ReplaceClaimAsync(user, new Claim("T1", "V2"), new Claim("T1", "V4"));

                    await context.SaveChangesAsync();
                },
                async context =>
                {
                    using var userStore =
                        new UserStore<TUser, TRole, TContext, TKey, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim>(context);

                    var claims = (await userStore.GetClaimsAsync(user)).OrderBy(e => e.Type).ThenBy(e => e.Value).ToList();
                    Assert.Equal(3, claims.Count);
                    Assert.Equal("T1", claims[0].Type);
                    Assert.Equal("V1", claims[0].Value);
                    Assert.Equal("T1", claims[1].Type);
                    Assert.Equal("V4", claims[1].Value);
                    Assert.Equal("T2", claims[2].Type);
                    Assert.Equal("V3", claims[2].Value);
                });
        }

        [ConditionalFact]
        public async Task Can_call_UserStore_RemoveClaimsAsync()
        {
            var user = new TUser { NormalizedEmail = "wendy@example.com" };

            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    await CreateUser(context, user);
                },
                async context =>
                {
                    using var userStore =
                        new UserStore<TUser, TRole, TContext, TKey, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim>(context);

                    await userStore.RemoveClaimsAsync(user, new[] { new Claim("T1", "V1"), new Claim("T2", "V3") });

                    await context.SaveChangesAsync();
                },
                async context =>
                {
                    using var userStore =
                        new UserStore<TUser, TRole, TContext, TKey, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim>(context);

                    var claims = (await userStore.GetClaimsAsync(user)).OrderBy(e => e.Type).ThenBy(e => e.Value).ToList();
                    Assert.Equal(1, claims.Count);
                    Assert.Equal("T1", claims[0].Type);
                    Assert.Equal("V2", claims[0].Value);
                });
        }

        [ConditionalFact]
        public async Task Can_call_UserStore_GetLoginsAsync()
        {
            var user = new TUser { NormalizedEmail = "wendy@example.com" };

            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    await CreateUser(context, user);
                },
                async context =>
                {
                    using var userStore =
                        new UserStore<TUser, TRole, TContext, TKey, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim>(context);

                    var logins = (await userStore.GetLoginsAsync(user)).OrderBy(e => e.LoginProvider).ToList();
                    Assert.Equal(2, logins.Count);
                    Assert.Equal("ISCABBS", logins[0].LoginProvider);
                    Assert.Equal("Local", logins[1].LoginProvider);
                });
        }

        [ConditionalFact]
        public async Task Can_call_UserStore_GetUsersForClaimAsync()
        {
            var user = new TUser { NormalizedEmail = "wendy@example.com" };

            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    await CreateUser(context, user);
                },
                async context =>
                {
                    using var userStore =
                        new UserStore<TUser, TRole, TContext, TKey, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim>(context);

                    var users = await userStore.GetUsersForClaimAsync(new Claim("T1", "V1"));
                    Assert.Equal(1, users.Count);
                    Assert.Equal("wendy@example.com", users[0].NormalizedEmail);
                });
        }

        [ConditionalFact]
        public async Task Can_call_UserStore_GetUsersInRoleAsync()
        {
            var user = new TUser { NormalizedEmail = "wendy@example.com" };

            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    await CreateUser(context, user);
                },
                async context =>
                {
                    using var userStore =
                        new UserStore<TUser, TRole, TContext, TKey, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim>(context);

                    var users = await userStore.GetUsersInRoleAsync("admin");
                    Assert.Equal(1, users.Count);
                    Assert.Equal("wendy@example.com", users[0].NormalizedEmail);
                });
        }

        [ConditionalFact]
        public async Task Can_call_UserOnlyStore_FindByNameAsync()
        {
            var user = new TUser { NormalizedUserName = "wendy" };

            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    await CreateUser(context, user);
                },
                async context =>
                {
                    using var userStore = new UserOnlyStore<TUser, TContext, TKey, TUserClaim, TUserLogin, TUserToken>(context);

                    Assert.Equal(user.Id, (await userStore.FindByNameAsync("wendy")).Id);
                });
        }

        [ConditionalFact]
        public async Task Can_call_UserOnlyStore_FindByEmailAsync()
        {
            var user = new TUser { NormalizedEmail = "wendy@example.com" };

            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    await CreateUser(context, user);
                },
                async context =>
                {
                    using var userStore = new UserOnlyStore<TUser, TContext, TKey, TUserClaim, TUserLogin, TUserToken>(context);

                    Assert.Equal(user.Id, (await userStore.FindByEmailAsync("wendy@example.com")).Id);
                });
        }

        [ConditionalFact]
        public async Task Can_call_UserOnlyStore_GetClaimsAsync()
        {
            var user = new TUser { NormalizedEmail = "wendy@example.com" };

            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    await CreateUser(context, user);
                },
                async context =>
                {
                    using var userStore = new UserOnlyStore<TUser, TContext, TKey, TUserClaim, TUserLogin, TUserToken>(context);

                    var claims = (await userStore.GetClaimsAsync(user)).OrderBy(e => e.Type).ThenBy(e => e.Value).ToList();
                    Assert.Equal(3, claims.Count);
                    Assert.Equal("T1", claims[0].Type);
                    Assert.Equal("V1", claims[0].Value);
                    Assert.Equal("T1", claims[1].Type);
                    Assert.Equal("V2", claims[1].Value);
                    Assert.Equal("T2", claims[2].Type);
                    Assert.Equal("V3", claims[2].Value);
                });
        }

        [ConditionalFact]
        public async Task Can_call_UserOnlyStore_ReplaceClaimAsync()
        {
            var user = new TUser { NormalizedEmail = "wendy@example.com" };

            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    await CreateUser(context, user);
                },
                async context =>
                {
                    using var userStore = new UserOnlyStore<TUser, TContext, TKey, TUserClaim, TUserLogin, TUserToken>(context);

                    await userStore.ReplaceClaimAsync(user, new Claim("T1", "V2"), new Claim("T1", "V4"));

                    await context.SaveChangesAsync();
                },
                async context =>
                {
                    using var userStore = new UserOnlyStore<TUser, TContext, TKey, TUserClaim, TUserLogin, TUserToken>(context);

                    var claims = (await userStore.GetClaimsAsync(user)).OrderBy(e => e.Type).ThenBy(e => e.Value).ToList();
                    Assert.Equal(3, claims.Count);
                    Assert.Equal("T1", claims[0].Type);
                    Assert.Equal("V1", claims[0].Value);
                    Assert.Equal("T1", claims[1].Type);
                    Assert.Equal("V4", claims[1].Value);
                    Assert.Equal("T2", claims[2].Type);
                    Assert.Equal("V3", claims[2].Value);
                });
        }

        [ConditionalFact]
        public async Task Can_call_UserOnlyStore_RemoveClaimsAsync()
        {
            var user = new TUser { NormalizedEmail = "wendy@example.com" };

            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    await CreateUser(context, user);
                },
                async context =>
                {
                    using var userStore = new UserOnlyStore<TUser, TContext, TKey, TUserClaim, TUserLogin, TUserToken>(context);

                    await userStore.RemoveClaimsAsync(user, new[] { new Claim("T1", "V1"), new Claim("T2", "V3") });

                    await context.SaveChangesAsync();
                },
                async context =>
                {
                    using var userStore = new UserOnlyStore<TUser, TContext, TKey, TUserClaim, TUserLogin, TUserToken>(context);

                    var claims = (await userStore.GetClaimsAsync(user)).OrderBy(e => e.Type).ThenBy(e => e.Value).ToList();
                    Assert.Equal(1, claims.Count);
                    Assert.Equal("T1", claims[0].Type);
                    Assert.Equal("V2", claims[0].Value);
                });
        }

        [ConditionalFact]
        public async Task Can_call_UserOnlyStore_GetLoginsAsync()
        {
            var user = new TUser { NormalizedEmail = "wendy@example.com" };

            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    await CreateUser(context, user);
                },
                async context =>
                {
                    using var userStore = new UserOnlyStore<TUser, TContext, TKey, TUserClaim, TUserLogin, TUserToken>(context);

                    var logins = (await userStore.GetLoginsAsync(user)).OrderBy(e => e.LoginProvider).ToList();
                    Assert.Equal(2, logins.Count);
                    Assert.Equal("ISCABBS", logins[0].LoginProvider);
                    Assert.Equal("Local", logins[1].LoginProvider);
                });
        }

        [ConditionalFact]
        public async Task Can_call_UserOnlyStore_GetUsersForClaimAsync()
        {
            var user = new TUser { NormalizedEmail = "wendy@example.com" };

            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    await CreateUser(context, user);
                },
                async context =>
                {
                    using var userStore = new UserOnlyStore<TUser, TContext, TKey, TUserClaim, TUserLogin, TUserToken>(context);

                    var users = await userStore.GetUsersForClaimAsync(new Claim("T1", "V1"));
                    Assert.Equal(1, users.Count);
                    Assert.Equal("wendy@example.com", users[0].NormalizedEmail);
                });
        }

        [ConditionalFact]
        public async Task Can_call_RoleStore_GetClaimsAsync()
        {
            var user = new TUser { NormalizedEmail = "wendy@example.com" };

            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    await CreateUser(context, user);
                },
                async context =>
                {
                    using var roleStore = new RoleStore<TRole, TContext, TKey, TUserRole, TRoleClaim>(context);
                    var adminRole = roleStore.Roles.Single(r => r.NormalizedName == "admin");

                    var claims = (await roleStore.GetClaimsAsync(adminRole)).OrderBy(e => e.Type).ThenBy(e => e.Value).ToList();
                    Assert.Equal(2, claims.Count);
                    Assert.Equal("AC1", claims[0].Type);
                    Assert.Equal("V1", claims[0].Value);
                    Assert.Equal("AC2", claims[1].Type);
                    Assert.Equal("V1", claims[1].Value);
                });
        }

        protected static async Task CreateUser(TContext context, TUser user)
        {
            using var userStore =
                new UserStore<TUser, TRole, TContext, TKey, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim>(context);
            using var roleStore = new RoleStore<TRole, TContext, TKey, TUserRole, TRoleClaim>(context);

            await userStore.CreateAsync(user);
            await userStore.AddClaimsAsync(user, new[] { new Claim("T1", "V1"), new Claim("T1", "V2"), new Claim("T2", "V3") });

            var adminRole = new TRole { NormalizedName = "admin", Name = "Admin" };
            await roleStore.CreateAsync(adminRole);
            await userStore.AddToRoleAsync(user, "admin");
            await roleStore.AddClaimAsync(adminRole, new Claim("AC1", "V1"));
            await roleStore.AddClaimAsync(adminRole, new Claim("AC2", "V1"));

            var moderatorRole = new TRole { NormalizedName = "moderator", Name = "Moderator" };
            await roleStore.CreateAsync(moderatorRole);
            await userStore.AddToRoleAsync(user, "moderator");
            await roleStore.AddClaimAsync(moderatorRole, new Claim("MC1", "V1"));
            await roleStore.AddClaimAsync(moderatorRole, new Claim("MC2", "V1"));

            await userStore.AddLoginAsync(user, new UserLoginInfo("ISCABBS", "DrDave", "SSHFTW"));
            await userStore.AddLoginAsync(user, new UserLoginInfo("Local", "EekyBear", "PPS"));

            await userStore.SetTokenAsync(user, "ISCABBS", "DrDave", "SSHFTW", CancellationToken.None);

            await context.SaveChangesAsync();
        }

        protected TFixture Fixture { get; }

        public abstract class AspNetIdentityFixtureBase
            : SharedStoreFixtureBase<TContext>
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
            }

            protected override void Seed(TContext context)
            {
            }
        }

        protected TContext CreateContext()
            => Fixture.CreateContext();

        protected virtual Task ExecuteWithStrategyInTransactionAsync(
            Func<TContext, Task> testOperation,
            Func<TContext, Task> nestedTestOperation1 = null,
            Func<TContext, Task> nestedTestOperation2 = null,
            Func<TContext, Task> nestedTestOperation3 = null)
            => TestHelpers.ExecuteWithStrategyInTransactionAsync(
                CreateContext, UseTransaction,
                testOperation, nestedTestOperation1, nestedTestOperation2, nestedTestOperation3);

        protected virtual void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
            => facade.UseTransaction(transaction.GetDbTransaction());

        protected class EntityTypeMapping
        {
            public EntityTypeMapping()
            {
            }

            public EntityTypeMapping(IEntityType entityType)
            {
                Name = entityType.Name;
                TableName = entityType.GetTableName();
                PrimaryKey = entityType.FindPrimaryKey()!.ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

                Properties.AddRange(
                    entityType.GetProperties()
                        .Select(p => p.ToDebugString(MetadataDebugStringOptions.SingleLineDefault)));

                Indexes.AddRange(
                    entityType.GetIndexes().Select(i => $"{i.Properties.Format()} {(i.IsUnique ? "Unique" : "")}"));

                FKs.AddRange(
                    entityType.GetForeignKeys().Select(f => f.ToDebugString(MetadataDebugStringOptions.SingleLineDefault)));

                Navigations.AddRange(
                    entityType.GetNavigations().Select(n => n.ToDebugString(MetadataDebugStringOptions.SingleLineDefault)));

                SkipNavigations.AddRange(
                    entityType.GetSkipNavigations().Select(n => n.ToDebugString(MetadataDebugStringOptions.SingleLineDefault)));
            }

            public string Name { get; set; }
            public string TableName { get; set; }
            public string PrimaryKey { get; set; }
            public List<string> Properties { get; } = new();
            public List<string> Indexes { get; } = new();
            public List<string> FKs { get; } = new();
            public List<string> Navigations { get; } = new();
            public List<string> SkipNavigations { get; } = new();

            public override string ToString()
            {
                var builder = new StringBuilder();

                builder.AppendLine("new()");
                builder.AppendLine("{");

                builder.AppendLine($"    Name = \"{Name}\",");
                builder.AppendLine($"    TableName = \"{TableName}\",");
                builder.AppendLine($"    PrimaryKey = \"{PrimaryKey}\",");

                builder.AppendLine("    Properties =");
                builder.AppendLine("    {");
                foreach (var property in Properties)
                {
                    builder.AppendLine($"        \"{property}\",");
                }

                builder.AppendLine("    },");

                if (Indexes.Any())
                {
                    builder.AppendLine("    Indexes =");
                    builder.AppendLine("    {");
                    foreach (var index in Indexes)
                    {
                        builder.AppendLine($"        \"{index}\",");
                    }

                    builder.AppendLine("    },");
                }

                if (FKs.Any())
                {
                    builder.AppendLine("    FKs =");
                    builder.AppendLine("    {");
                    foreach (var fk in FKs)
                    {
                        builder.AppendLine($"        \"{fk}\",");
                    }

                    builder.AppendLine("    },");
                }

                if (Navigations.Any())
                {
                    builder.AppendLine("    Navigations =");
                    builder.AppendLine("    {");
                    foreach (var navigation in Navigations)
                    {
                        builder.AppendLine($"        \"{navigation}\",");
                    }

                    builder.AppendLine("    },");
                }

                builder.AppendLine("},");

                return builder.ToString();
            }

            public static void AssertEqual(IReadOnlyList<EntityTypeMapping> expected, IReadOnlyList<EntityTypeMapping> actual)
            {
                Assert.Equal(expected.Count, actual.Count);
                for (var i = 0; i < expected.Count; i++)
                {
                    var e = expected[i];
                    var a = actual[i];

                    Assert.Equal(e.Name, a.Name);
                    Assert.Equal(e.TableName, a.TableName);
                    Assert.Equal(e.PrimaryKey, a.PrimaryKey);
                    Assert.Equal(e.Properties, a.Properties);
                    Assert.Equal(e.Indexes, a.Indexes);
                    Assert.Equal(e.FKs, a.FKs);
                    Assert.Equal(e.Navigations, a.Navigations);
                    Assert.Equal(e.SkipNavigations, a.SkipNavigations);
                }
            }

            public static string Serialize(IEnumerable<EntityTypeMapping> mappings)
            {
                var builder = new StringBuilder();
                foreach (var mapping in mappings)
                {
                    builder.Append(mapping);
                }

                return builder.ToString();
            }
        }
    }
}
