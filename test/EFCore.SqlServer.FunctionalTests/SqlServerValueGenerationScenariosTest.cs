// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public class SqlServerValueGenerationScenariosTest
    {
        private const string DatabaseName = "SqlServerValueGenerationScenariosTest";

        // Positive cases

        [Fact]
        public void Insert_with_Identity_column()
        {
            using (var testStore = SqlServerTestStore.CreateInitialized(DatabaseName))
            {
                using (var context = new BlogContextIdentity(testStore.Name))
                {
                    context.Database.EnsureCreatedResiliently();

                    context.AddRange(
                        new Blog
                        {
                            Name = "One Unicorn"
                        }, new Blog
                        {
                            Name = "Two Unicorns"
                        });

                    context.SaveChanges();
                }

                using (var context = new BlogContextIdentity(testStore.Name))
                {
                    var blogs = context.Blogs.OrderBy(e => e.Id).ToList();

                    Assert.Equal(1, blogs[0].Id);
                    Assert.Equal(2, blogs[1].Id);
                }
            }
        }

        public class BlogContextIdentity : ContextBase
        {
            public BlogContextIdentity(string databaseName)
                : base(databaseName)
            {
            }
        }

        [SqlServerCondition(SqlServerCondition.SupportsSequences)]
        [ConditionalFact]
        public void Insert_with_sequence_HiLo()
        {
            using (var testStore = SqlServerTestStore.CreateInitialized(DatabaseName))
            {
                using (var context = new BlogContextHiLo(testStore.Name))
                {
                    context.Database.EnsureCreatedResiliently();

                    context.AddRange(
                        new Blog
                        {
                            Name = "One Unicorn"
                        }, new Blog
                        {
                            Name = "Two Unicorns"
                        });

                    context.SaveChanges();
                }

                using (var context = new BlogContextHiLo(testStore.Name))
                {
                    var blogs = context.Blogs.OrderBy(e => e.Id).ToList();

                    Assert.Equal(1, blogs[0].Id);
                    Assert.Equal(2, blogs[0].OtherId);
                    Assert.Equal(3, blogs[1].Id);
                    Assert.Equal(4, blogs[1].OtherId);
                }
            }
        }

        public class BlogContextHiLo : ContextBase
        {
            public BlogContextHiLo(string databaseName)
                : base(databaseName)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.ForSqlServerUseSequenceHiLo();

                modelBuilder.Entity<Blog>(
                    eb =>
                    {
                        eb.HasAlternateKey(
                            b => new
                            {
                                b.OtherId
                            });
                        eb.Property(b => b.OtherId).ValueGeneratedOnAdd();
                    });
            }
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsSequences)]
        public void Insert_with_default_value_from_sequence()
        {
            using (var testStore = SqlServerTestStore.CreateInitialized(DatabaseName))
            {
                using (var context = new BlogContextDefaultValue(testStore.Name))
                {
                    context.Database.EnsureCreatedResiliently();

                    context.AddRange(
                        new Blog
                        {
                            Name = "One Unicorn"
                        }, new Blog
                        {
                            Name = "Two Unicorns"
                        });

                    context.SaveChanges();
                }

                using (var context = new BlogContextDefaultValue(testStore.Name))
                {
                    var blogs = context.Blogs.OrderBy(e => e.Id).ToList();

                    Assert.Equal(77, blogs[0].Id);
                    Assert.Equal(78, blogs[1].Id);
                }

                using (var context = new BlogContextDefaultValueNoMigrations(testStore.Name))
                {
                    context.AddRange(
                        new Blog
                        {
                            Name = "One Unicorn"
                        }, new Blog
                        {
                            Name = "Two Unicorns"
                        });

                    context.SaveChanges();
                }

                using (var context = new BlogContextDefaultValueNoMigrations(testStore.Name))
                {
                    var blogs = context.Blogs.OrderBy(e => e.Id).ToList();

                    Assert.Equal(77, blogs[0].Id);
                    Assert.Equal(78, blogs[1].Id);
                    Assert.Equal(79, blogs[2].Id);
                    Assert.Equal(80, blogs[3].Id);
                }
            }
        }

        public class BlogContextDefaultValue : ContextBase
        {
            public BlogContextDefaultValue(string databaseName)
                : base(databaseName)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder
                    .HasSequence("MySequence")
                    .StartsAt(77);

                modelBuilder
                    .Entity<Blog>()
                    .Property(e => e.Id)
                    .HasDefaultValueSql("next value for MySequence");
            }
        }

        public class BlogContextDefaultValueNoMigrations : ContextBase
        {
            public BlogContextDefaultValueNoMigrations(string databaseName)
                : base(databaseName)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder
                    .Entity<Blog>()
                    .Property(e => e.Id)
                    .HasDefaultValue();
            }
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsSequences)]
        public void Insert_with_default_string_value_from_sequence()
        {
            using (var testStore = SqlServerTestStore.CreateInitialized(DatabaseName))
            {
                using (var context = new BlogContextStringDefaultValue(testStore.Name))
                {
                    context.Database.EnsureCreatedResiliently();

                    context.AddRange(
                        new BlogWithStringKey
                        {
                            Name = "One Unicorn"
                        }, new BlogWithStringKey
                        {
                            Name = "Two Unicorns"
                        });

                    context.SaveChanges();
                }

                using (var context = new BlogContextStringDefaultValue(testStore.Name))
                {
                    var blogs = context.StringyBlogs.OrderBy(e => e.Id).ToList();

                    Assert.Equal("i77", blogs[0].Id);
                    Assert.Equal("i78", blogs[1].Id);
                }
            }
        }

        public class BlogContextStringDefaultValue : ContextBase
        {
            public BlogContextStringDefaultValue(string databaseName)
                : base(databaseName)
            {
            }

            public DbSet<BlogWithStringKey> StringyBlogs { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder
                    .HasSequence("MyStrinySequence")
                    .StartsAt(77);

                modelBuilder
                    .Entity<BlogWithStringKey>()
                    .Property(e => e.Id)
                    .HasDefaultValueSql("'i' + CAST((NEXT VALUE FOR MyStrinySequence) AS VARCHAR(20))");
            }
        }

        public class BlogWithStringKey
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsSequences)]
        public void Insert_with_key_default_value_from_sequence()
        {
            using (var testStore = SqlServerTestStore.CreateInitialized(DatabaseName))
            {
                using (var context = new BlogContextKeyColumnWithDefaultValue(testStore.Name))
                {
                    context.Database.EnsureCreatedResiliently();

                    context.AddRange(
                        new Blog
                        {
                            Name = "One Unicorn"
                        }, new Blog
                        {
                            Name = "Two Unicorns"
                        });

                    context.SaveChanges();
                }

                using (var context = new BlogContextKeyColumnWithDefaultValue(testStore.Name))
                {
                    var blogs = context.Blogs.OrderBy(e => e.Id).ToList();

                    Assert.Equal(77, blogs[0].Id);
                    Assert.Equal(78, blogs[1].Id);
                }
            }
        }

        public class BlogContextKeyColumnWithDefaultValue : ContextBase
        {
            public BlogContextKeyColumnWithDefaultValue(string databaseName)
                : base(databaseName)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder
                    .HasSequence("MySequence")
                    .StartsAt(77);

                modelBuilder
                    .Entity<Blog>()
                    .Property(e => e.Id)
                    .HasDefaultValueSql("next value for MySequence")
                    .Metadata.BeforeSaveBehavior = PropertySaveBehavior.Throw;
            }
        }

        [Fact]
        public void Insert_with_explicit_non_default_keys()
        {
            using (var testStore = SqlServerTestStore.CreateInitialized(DatabaseName))
            {
                using (var context = new BlogContextNoKeyGeneration(testStore.Name))
                {
                    context.Database.EnsureCreatedResiliently();

                    context.AddRange(
                        new Blog
                        {
                            Id = 66,
                            Name = "One Unicorn"
                        }, new Blog
                        {
                            Id = 67,
                            Name = "Two Unicorns"
                        });

                    context.SaveChanges();
                }

                using (var context = new BlogContextNoKeyGeneration(testStore.Name))
                {
                    var blogs = context.Blogs.OrderBy(e => e.Id).ToList();

                    Assert.Equal(66, blogs[0].Id);
                    Assert.Equal(67, blogs[1].Id);
                }
            }
        }

        public class BlogContextNoKeyGeneration : ContextBase
        {
            public BlogContextNoKeyGeneration(string databaseName)
                : base(databaseName)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder
                    .Entity<Blog>()
                    .Property(e => e.Id)
                    .ValueGeneratedNever();
            }
        }

        [Fact]
        public void Insert_with_explicit_with_default_keys()
        {
            using (var testStore = SqlServerTestStore.CreateInitialized(DatabaseName))
            {
                using (var context = new BlogContextNoKeyGenerationNullableKey(testStore.Name))
                {
                    context.Database.EnsureCreatedResiliently();

                    context.AddRange(
                        new NullableKeyBlog
                        {
                            Id = 0,
                            Name = "One Unicorn"
                        },
                        new NullableKeyBlog
                        {
                            Id = 1,
                            Name = "Two Unicorns"
                        });

                    context.SaveChanges();
                }

                using (var context = new BlogContextNoKeyGenerationNullableKey(testStore.Name))
                {
                    var blogs = context.NullableKeyBlogs.OrderBy(e => e.Id).ToList();

                    Assert.Equal(0, blogs[0].Id);
                    Assert.Equal(1, blogs[1].Id);
                }
            }
        }

        public class BlogContextNoKeyGenerationNullableKey : ContextBase
        {
            public BlogContextNoKeyGenerationNullableKey(string databaseName)
                : base(databaseName)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder
                    .Entity<NullableKeyBlog>()
                    .Property(e => e.Id)
                    .ValueGeneratedNever();
            }
        }

        [Fact]
        public void Insert_with_non_key_default_value()
        {
            using (var testStore = SqlServerTestStore.CreateInitialized(DatabaseName))
            {
                using (var context = new BlogContextNonKeyDefaultValue(testStore.Name))
                {
                    context.Database.EnsureCreatedResiliently();

                    var blogs = new List<Blog>
                    {
                        new Blog
                        {
                            Name = "One Unicorn"
                        },
                        new Blog
                        {
                            Name = "Two Unicorns",
                            CreatedOn = new DateTime(1969, 8, 3, 0, 10, 0)
                        }
                    };

                    context.AddRange(blogs);

                    context.SaveChanges();

                    Assert.NotEqual(new DateTime(), blogs[0].CreatedOn);
                    Assert.NotEqual(new DateTime(), blogs[1].CreatedOn);
                    Assert.Null(blogs[0].OtherId);
                    Assert.Null(blogs[1].OtherId);
                }

                using (var context = new BlogContextNonKeyDefaultValue(testStore.Name))
                {
                    var blogs = context.Blogs.OrderBy(e => e.Name).ToList();

                    Assert.NotEqual(new DateTime(), blogs[0].CreatedOn);
                    Assert.Equal(new DateTime(1969, 8, 3, 0, 10, 0), blogs[1].CreatedOn);

                    blogs[0].CreatedOn = new DateTime(1973, 9, 3, 0, 10, 0);
                    blogs[1].Name = "Zwo Unicorns";

                    context.SaveChanges();
                }

                using (var context = new BlogContextNonKeyDefaultValue(testStore.Name))
                {
                    var blogs = context.Blogs.OrderBy(e => e.Name).ToList();

                    Assert.Equal(new DateTime(1969, 8, 3, 0, 10, 0), blogs[1].CreatedOn);
                    Assert.Equal(new DateTime(1973, 9, 3, 0, 10, 0), blogs[0].CreatedOn);
                }
            }
        }

        public class BlogContextNonKeyDefaultValue : ContextBase
        {
            public BlogContextNonKeyDefaultValue(string databaseName)
                : base(databaseName)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Blog>(
                    b =>
                    {
                        b.Property(e => e.CreatedOn)
                            .HasDefaultValueSql("getdate()");

                        b.Property(e => e.OtherId)
                            .HasDefaultValue(value: null);
                    });
            }
        }

        [Fact]
        public void Insert_with_non_key_default_value_readonly()
        {
            using (var testStore = SqlServerTestStore.CreateInitialized(DatabaseName))
            {
                using (var context = new BlogContextNonKeyReadOnlyDefaultValue(testStore.Name))
                {
                    context.Database.EnsureCreatedResiliently();

                    context.AddRange(
                        new Blog
                        {
                            Name = "One Unicorn"
                        },
                        new Blog
                        {
                            Name = "Two Unicorns"
                        });

                    context.SaveChanges();

                    Assert.NotEqual(new DateTime(), context.Blogs.ToList()[0].CreatedOn);
                }

                DateTime dateTime0;

                using (var context = new BlogContextNonKeyReadOnlyDefaultValue(testStore.Name))
                {
                    var blogs = context.Blogs.OrderBy(e => e.Id).ToList();

                    dateTime0 = blogs[0].CreatedOn;

                    Assert.NotEqual(new DateTime(), dateTime0);
                    Assert.NotEqual(new DateTime(), blogs[1].CreatedOn);

                    blogs[0].Name = "One Pegasus";
                    blogs[1].CreatedOn = new DateTime(1973, 9, 3, 0, 10, 0);

                    context.SaveChanges();
                }

                using (var context = new BlogContextNonKeyReadOnlyDefaultValue(testStore.Name))
                {
                    var blogs = context.Blogs.OrderBy(e => e.Id).ToList();

                    Assert.Equal(dateTime0, blogs[0].CreatedOn);
                    Assert.Equal(new DateTime(1973, 9, 3, 0, 10, 0), blogs[1].CreatedOn);
                }
            }
        }

        public class BlogContextNonKeyReadOnlyDefaultValue : ContextBase
        {
            public BlogContextNonKeyReadOnlyDefaultValue(string databaseName)
                : base(databaseName)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Blog>()
                    .Property(e => e.CreatedOn)
                    .HasDefaultValueSql("getdate()")
                    .Metadata.BeforeSaveBehavior = PropertySaveBehavior.Throw;
            }
        }

        [Fact]
        public void Insert_and_update_with_computed_column()
        {
            using (var testStore = SqlServerTestStore.CreateInitialized(DatabaseName))
            {
                using (var context = new BlogContextComputedColumn(testStore.Name))
                {
                    context.Database.EnsureCreatedResiliently();

                    var blog = context.Add(
                        new FullNameBlog
                        {
                            FirstName = "One",
                            LastName = "Unicorn"
                        }).Entity;

                    context.SaveChanges();

                    Assert.Equal("One Unicorn", blog.FullName);
                }

                using (var context = new BlogContextComputedColumn(testStore.Name))
                {
                    var blog = context.FullNameBlogs.Single();

                    Assert.Equal("One Unicorn", blog.FullName);

                    blog.LastName = "Pegasus";

                    context.SaveChanges();

                    Assert.Equal("One Pegasus", blog.FullName);
                }
            }
        }

        public class BlogContextComputedColumn : ContextBase
        {
            public BlogContextComputedColumn(string databaseName)
                : base(databaseName)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                var property = modelBuilder.Entity<FullNameBlog>()
                    .Property(e => e.FullName)
                    .HasComputedColumnSql("FirstName + ' ' + LastName")
                    .Metadata;

                property.BeforeSaveBehavior = PropertySaveBehavior.Throw;
                property.AfterSaveBehavior = PropertySaveBehavior.Throw;
            }
        }

        // #6044
        [Fact]
        public void Insert_and_update_with_computed_column_with_function()
        {
            using (var testStore = SqlServerTestStore.CreateInitialized(DatabaseName))
            {
                using (var context = new BlogContextComputedColumnWithFunction(testStore.Name))
                {
                    context.Database.ExecuteSqlCommand
                    (
                        @"CREATE FUNCTION
[dbo].[GetFullName](@First NVARCHAR(MAX), @Second NVARCHAR(MAX))
RETURNS NVARCHAR(MAX) WITH SCHEMABINDING AS BEGIN RETURN @First + @Second END");

                    context.GetService<IRelationalDatabaseCreator>().CreateTables();
                }

                using (var context = new BlogContextComputedColumnWithFunction(testStore.Name))
                {
                    var blog = context.Add(
                        new FullNameBlog
                        {
                            FirstName = "One",
                            LastName = "Unicorn"
                        }).Entity;

                    context.SaveChanges();

                    Assert.Equal("OneUnicorn", blog.FullName);
                }

                using (var context = new BlogContextComputedColumnWithFunction(testStore.Name))
                {
                    var blog = context.FullNameBlogs.Single();

                    Assert.Equal("OneUnicorn", blog.FullName);

                    blog.LastName = "Pegasus";

                    context.SaveChanges();

                    Assert.Equal("OnePegasus", blog.FullName);
                }
            }
        }

        public class BlogContextComputedColumnWithFunction : ContextBase
        {
            public BlogContextComputedColumnWithFunction(string databaseName)
                : base(databaseName)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<FullNameBlog>()
                    .Property(e => e.FullName)
                    .HasComputedColumnSql("[dbo].[GetFullName]([FirstName], [LastName])")
                    .Metadata.AfterSaveBehavior = PropertySaveBehavior.Throw;
            }
        }

        // #6044
        [Fact]
        public void Insert_and_update_with_computed_column_with_querying_function()
        {
            using (var testStore = SqlServerTestStore.CreateInitialized(DatabaseName))
            {
                using (var context = new BlogContextComputedColumn(testStore.Name))
                {
                    context.GetService<IRelationalDatabaseCreator>().CreateTables();

                    context.Database.ExecuteSqlCommand("ALTER TABLE dbo.FullNameBlogs DROP COLUMN FullName;");

                    context.Database.ExecuteSqlCommand(
                        @"CREATE FUNCTION [dbo].[GetFullName](@Id int)
RETURNS NVARCHAR(MAX) WITH SCHEMABINDING AS
BEGIN
    DECLARE @FullName NVARCHAR(MAX);
    SELECT @FullName = [FirstName] + [LastName] FROM [dbo].[FullNameBlogs] WHERE [Id] = @Id;
    RETURN @FullName
END");

                    context.Database.ExecuteSqlCommand("ALTER TABLE dbo.FullNameBlogs ADD FullName AS [dbo].[GetFullName]([Id]); ");
                }

                try
                {
                    using (var context = new BlogContextComputedColumn(testStore.Name))
                    {
                        var blog = context.Add(
                            new FullNameBlog
                            {
                                FirstName = "One",
                                LastName = "Unicorn"
                            }).Entity;

                        context.SaveChanges();

                        Assert.Equal("OneUnicorn", blog.FullName);
                    }

                    using (var context = new BlogContextComputedColumn(testStore.Name))
                    {
                        var blog = context.FullNameBlogs.Single();

                        Assert.Equal("OneUnicorn", blog.FullName);

                        blog.LastName = "Pegasus";

                        context.SaveChanges();

                        Assert.Equal("OnePegasus", blog.FullName);
                    }

                    using (var context = new BlogContextComputedColumn(testStore.Name))
                    {
                        var blog1 = context.Add(
                            new FullNameBlog
                            {
                                FirstName = "Hank",
                                LastName = "Unicorn"
                            }).Entity;
                        var blog2 = context.Add(
                            new FullNameBlog
                            {
                                FirstName = "Jeff",
                                LastName = "Unicorn"
                            }).Entity;

                        context.SaveChanges();

                        Assert.Equal("HankUnicorn", blog1.FullName);
                        Assert.Equal("JeffUnicorn", blog2.FullName);
                    }
                }
                finally
                {
                    using (var context = new BlogContextComputedColumn(testStore.Name))
                    {
                        context.Database.ExecuteSqlCommand("ALTER TABLE dbo.FullNameBlogs DROP COLUMN FullName;");
                        context.Database.ExecuteSqlCommand("DROP FUNCTION [dbo].[GetFullName];");
                    }
                }
            }
        }

        [Fact]
        public void Insert_with_client_generated_GUID_key()
        {
            using (var testStore = SqlServerTestStore.CreateInitialized(DatabaseName))
            {
                Guid afterSave;
                using (var context = new BlogContextClientGuidKey(testStore.Name))
                {
                    context.Database.EnsureCreatedResiliently();

                    var blog = context.Add(
                        new GuidBlog
                        {
                            Name = "One Unicorn"
                        }).Entity;

                    var beforeSave = blog.Id;
                    var beforeSaveNotId = blog.NotId;

                    Assert.NotEqual(default, beforeSave);
                    Assert.NotEqual(default, beforeSaveNotId);

                    context.SaveChanges();

                    afterSave = blog.Id;
                    var afterSaveNotId = blog.NotId;

                    Assert.Equal(beforeSave, afterSave);
                    Assert.Equal(beforeSaveNotId, afterSaveNotId);
                }

                using (var context = new BlogContextClientGuidKey(testStore.Name))
                {
                    Assert.Equal(afterSave, context.GuidBlogs.Single().Id);
                }
            }
        }

        public class BlogContextClientGuidKey : ContextBase
        {
            public BlogContextClientGuidKey(string databaseName)
                : base(databaseName)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<GuidBlog>(
                    eb =>
                    {
                        eb.HasAlternateKey(e => e.NotId);
                        eb.Property(e => e.NotId).ValueGeneratedOnAdd();
                    });
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.IsNotSqlAzure)]
        public void Insert_with_ValueGeneratedOnAdd_GUID_nonkey_property_throws()
        {
            using (var testStore = SqlServerTestStore.CreateInitialized(DatabaseName))
            {
                using (var context = new BlogContextClientGuidNonKey(testStore.Name))
                {
                    context.Database.EnsureCreatedResiliently();

                    var blog = context.Add(
                        new GuidBlog
                        {
                            Name = "One Unicorn"
                        }).Entity;

                    Assert.Equal(default, blog.NotId);

                    // No value set on a required column
                    Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                }
            }
        }

        public class BlogContextClientGuidNonKey : ContextBase
        {
            public BlogContextClientGuidNonKey(string databaseName)
                : base(databaseName)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<GuidBlog>().Property(e => e.NotId).ValueGeneratedOnAdd();
        }

        [Fact]
        public void Insert_with_server_generated_GUID_key()
        {
            using (var testStore = SqlServerTestStore.CreateInitialized(DatabaseName))
            {
                Guid afterSave;
                using (var context = new BlogContextServerGuidKey(testStore.Name))
                {
                    context.Database.EnsureCreatedResiliently();

                    var blog = context.Add(
                        new GuidBlog
                        {
                            Name = "One Unicorn"
                        }).Entity;

                    var beforeSave = blog.Id;
                    var beforeSaveNotId = blog.NotId;

                    Assert.NotEqual(default, beforeSave);
                    Assert.Equal(default, beforeSaveNotId);

                    context.SaveChanges();

                    afterSave = blog.Id;
                    var afterSaveNotId = blog.NotId;

                    Assert.NotEqual(default, afterSave);
                    Assert.NotEqual(default, afterSaveNotId);
                    Assert.NotEqual(beforeSave, afterSave);
                    Assert.NotEqual(beforeSaveNotId, afterSaveNotId);
                }

                using (var context = new BlogContextServerGuidKey(testStore.Name))
                {
                    Assert.Equal(afterSave, context.GuidBlogs.Single().Id);
                }
            }
        }

        public class BlogContextServerGuidKey : ContextBase
        {
            public BlogContextServerGuidKey(string databaseName)
                : base(databaseName)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder
                    .Entity<GuidBlog>(
                        eb =>
                        {
                            eb.Property(e => e.Id)
                                .HasDefaultValueSql("newsequentialid()");
                            eb.Property(e => e.NotId)
                                .HasDefaultValueSql("newsequentialid()");
                        });
            }
        }

        // Negative cases
        [Fact]
        public void Insert_with_explicit_non_default_keys_by_default()
        {
            using (var testStore = SqlServerTestStore.CreateInitialized(DatabaseName))
            {
                using (var context = new BlogContext(testStore.Name))
                {
                    context.Database.EnsureCreatedResiliently();

                    context.AddRange(
                        new Blog
                        {
                            Id = 1,
                            Name = "One Unicorn"
                        }, new Blog
                        {
                            Id = 2,
                            Name = "Two Unicorns"
                        });

                    // DbUpdateException : An error occurred while updating the entries. See the
                    // inner exception for details.
                    // SqlException : Cannot insert explicit value for identity column in table
                    // 'Blog' when IDENTITY_INSERT is set to OFF.
                    context.Database.CreateExecutionStrategy().Execute(
                        context, c =>
                            Assert.Throws<DbUpdateException>(() => c.SaveChanges()));
                }
            }
        }

        [Fact]
        public void Insert_with_explicit_default_keys()
        {
            using (var testStore = SqlServerTestStore.CreateInitialized(DatabaseName))
            {
                using (var context = new BlogContext(testStore.Name))
                {
                    context.Database.EnsureCreatedResiliently();

                    context.AddRange(
                        new Blog
                        {
                            Id = 0,
                            Name = "One Unicorn"
                        }, new Blog
                        {
                            Id = 1,
                            Name = "Two Unicorns"
                        });

                    // DbUpdateException : An error occurred while updating the entries. See the
                    // inner exception for details.
                    // SqlException : Cannot insert explicit value for identity column in table
                    // 'Blog' when IDENTITY_INSERT is set to OFF.
                    Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                }
            }
        }

        public class BlogContext : ContextBase
        {
            public BlogContext(string databaseName)
                : base(databaseName)
            {
            }
        }

        [Fact]
        public void Insert_with_implicit_default_keys()
        {
            using (var testStore = SqlServerTestStore.CreateInitialized(DatabaseName))
            {
                using (var context = new BlogContextSpecifyKeysUsingDefault(testStore.Name))
                {
                    context.Database.EnsureCreatedResiliently();

                    context.AddRange(
                        new Blog
                        {
                            Id = 0,
                            Name = "One Unicorn"
                        }, new Blog
                        {
                            Id = 1,
                            Name = "Two Unicorns"
                        });

                    context.SaveChanges();
                }

                using (var context = new BlogContextSpecifyKeysUsingDefault(testStore.Name))
                {
                    var blogs = context.Blogs.OrderBy(e => e.Id).ToList();

                    Assert.Equal(0, blogs[0].Id);
                    Assert.Equal(1, blogs[1].Id);
                }
            }
        }

        public class BlogContextSpecifyKeysUsingDefault : ContextBase
        {
            public BlogContextSpecifyKeysUsingDefault(string databaseName)
                : base(databaseName)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder
                    .Entity<Blog>()
                    .Property(e => e.Id)
                    .ValueGeneratedNever();
            }
        }

        [ConditionalFact]
        [SqlServerCondition(SqlServerCondition.SupportsSequences)]
        public void Insert_explicit_value_throws_when_readonly_sequence_before_save()
        {
            using (var testStore = SqlServerTestStore.CreateInitialized(DatabaseName))
            {
                using (var context = new BlogContextReadOnlySequenceKeyColumnWithDefaultValue(testStore.Name))
                {
                    context.Database.EnsureCreatedResiliently();

                    context.AddRange(
                        new Blog
                        {
                            Id = 1,
                            Name = "One Unicorn"
                        }, new Blog
                        {
                            Name = "Two Unicorns"
                        });

                    // The property 'Id' on entity type 'Blog' is defined to be read-only before it is
                    // saved, but its value has been set to something other than a temporary or default value.
                    Assert.Equal(
                        CoreStrings.PropertyReadOnlyBeforeSave("Id", "Blog"),
                        Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
                }
            }
        }

        public class BlogContextReadOnlySequenceKeyColumnWithDefaultValue : ContextBase
        {
            public BlogContextReadOnlySequenceKeyColumnWithDefaultValue(string databaseName)
                : base(databaseName)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.HasSequence("MySequence");

                modelBuilder
                    .Entity<Blog>()
                    .Property(e => e.Id)
                    .HasDefaultValueSql("next value for MySequence")
                    .Metadata.BeforeSaveBehavior = PropertySaveBehavior.Throw;
            }
        }

        [Fact]
        public void Insert_explicit_value_throws_when_readonly_before_save()
        {
            using (var testStore = SqlServerTestStore.CreateInitialized(DatabaseName))
            {
                using (var context = new BlogContextNonKeyReadOnlyDefaultValue(testStore.Name))
                {
                    context.Database.EnsureCreatedResiliently();

                    context.AddRange(
                        new Blog
                        {
                            Name = "One Unicorn"
                        },
                        new Blog
                        {
                            Name = "Two Unicorns",
                            CreatedOn = new DateTime(1969, 8, 3, 0, 10, 0)
                        });

                    // The property 'CreatedOn' on entity type 'Blog' is defined to be read-only before it is
                    // saved, but its value has been set to something other than a temporary or default value.
                    Assert.Equal(
                        CoreStrings.PropertyReadOnlyBeforeSave("CreatedOn", "Blog"),
                        Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
                }
            }
        }

        [Fact]
        public void Insert_explicit_value_into_computed_column()
        {
            using (var testStore = SqlServerTestStore.CreateInitialized(DatabaseName))
            {
                using (var context = new BlogContextComputedColumn(testStore.Name))
                {
                    context.Database.EnsureCreatedResiliently();

                    context.Add(
                        new FullNameBlog
                        {
                            FirstName = "One",
                            LastName = "Unicorn",
                            FullName = "Gerald"
                        });

                    // The property 'FullName' on entity type 'FullNameBlog' is defined to be read-only before it is
                    // saved, but its value has been set to something other than a temporary or default value.
                    Assert.Equal(
                        CoreStrings.PropertyReadOnlyBeforeSave("FullName", "FullNameBlog"),
                        Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
                }
            }
        }

        [Fact]
        public void Update_explicit_value_in_computed_column()
        {
            using (var testStore = SqlServerTestStore.CreateInitialized(DatabaseName))
            {
                using (var context = new BlogContextComputedColumn(testStore.Name))
                {
                    context.Database.EnsureCreatedResiliently();

                    context.Add(
                        new FullNameBlog
                        {
                            FirstName = "One",
                            LastName = "Unicorn"
                        });

                    context.SaveChanges();
                }

                using (var context = new BlogContextComputedColumn(testStore.Name))
                {
                    var blog = context.FullNameBlogs.Single();

                    blog.FullName = "The Gorilla";

                    // The property 'FullName' on entity type 'FullNameBlog' is defined to be read-only after it has been saved,
                    // but its value has been modified or marked as modified.
                    Assert.Equal(
                        CoreStrings.PropertyReadOnlyAfterSave("FullName", "FullNameBlog"),
                        Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
                }
            }
        }

        // Concurrency
        [Fact]
        public void Resolve_concurreny()
        {
            using (var testStore = SqlServerTestStore.CreateInitialized(DatabaseName))
            {
                using (var context = new BlogContextConcurrencyWithRowversion(testStore.Name))
                {
                    context.Database.EnsureCreatedResiliently();

                    var blog = context.Add(
                        new ConcurrentBlog
                        {
                            Name = "One Unicorn"
                        }).Entity;

                    context.SaveChanges();

                    using (var innerContext = new BlogContextConcurrencyWithRowversion(testStore.Name))
                    {
                        var updatedBlog = innerContext.ConcurrentBlogs.Single();
                        updatedBlog.Name = "One Pegasus";
                        innerContext.SaveChanges();
                        var currentTimestamp = updatedBlog.Timestamp.ToArray();

                        try
                        {
                            blog.Name = "One Earth Pony";
                            context.SaveChanges();
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            // Update origianal values (and optionally any current values)
                            // Would normally do this with just one method call
                            context.Entry(blog).Property(e => e.Id).OriginalValue = updatedBlog.Id;
                            context.Entry(blog).Property(e => e.Name).OriginalValue = updatedBlog.Name;
                            context.Entry(blog).Property(e => e.Timestamp).OriginalValue = updatedBlog.Timestamp;

                            context.SaveChanges();

                            Assert.NotEqual(blog.Timestamp, currentTimestamp);
                        }
                    }
                }
            }
        }

        public class BlogContextConcurrencyWithRowversion : ContextBase
        {
            public BlogContextConcurrencyWithRowversion(string databaseName)
                : base(databaseName)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<ConcurrentBlog>()
                    .Property(e => e.Timestamp)
                    .ValueGeneratedOnAddOrUpdate()
                    .IsConcurrencyToken();
            }
        }

        public class Blog
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public DateTime CreatedOn { get; set; }
            public int? OtherId { get; set; }
        }

        public class NullableKeyBlog
        {
            public int? Id { get; set; }
            public string Name { get; set; }
            public DateTime CreatedOn { get; set; }
        }

        public class FullNameBlog
        {
            public int Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string FullName { get; set; }
        }

        public class GuidBlog
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public Guid NotId { get; set; }
        }

        public class ConcurrentBlog
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public byte[] Timestamp { get; set; }
        }

        public abstract class ContextBase : DbContext
        {
            private readonly string _databaseName;

            protected ContextBase(string databaseName)
            {
                _databaseName = databaseName;
            }

            public DbSet<Blog> Blogs { get; set; }
            public DbSet<NullableKeyBlog> NullableKeyBlogs { get; set; }
            public DbSet<FullNameBlog> FullNameBlogs { get; set; }
            public DbSet<GuidBlog> GuidBlogs { get; set; }
            public DbSet<ConcurrentBlog> ConcurrentBlogs { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseSqlServer(SqlServerTestStore.CreateConnectionString(_databaseName), b => b.ApplyConfiguration());
        }
    }
}
