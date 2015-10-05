// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.FunctionalTests.TestUtilities.Xunit;
using Microsoft.Data.Entity.Internal;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class SqlServerValueGenerationScenariosTest
    {
        // Positive cases

        public class IdentityColumn : TestBase<IdentityColumn.BlogContext>
        {
            [Fact]
            public void Insert_with_Identity_column()
            {
                using (var context = new BlogContext())
                {
                    context.AddRange(new Blog { Name = "One Unicorn" }, new Blog { Name = "Two Unicorns" });

                    context.SaveChanges();
                }

                using (var context = new BlogContext())
                {
                    var blogs = context.Blogs.OrderBy(e => e.Id).ToList();

                    Assert.Equal(1, blogs[0].Id);
                    Assert.Equal(2, blogs[1].Id);
                }
            }

            public class BlogContext : ContextBase
            {
            }
        }

        [SqlServerCondition(SqlServerCondition.SupportsSequences)]
        public class SequenceHiLo : TestBase<SequenceHiLo.BlogContext>
        {
            [ConditionalFact]
            public void Insert_with_sequence_HiLo()
            {
                using (var context = new BlogContext())
                {
                    context.AddRange(new Blog { Name = "One Unicorn" }, new Blog { Name = "Two Unicorns" });

                    context.SaveChanges();
                }

                using (var context = new BlogContext())
                {
                    var blogs = context.Blogs.OrderBy(e => e.Id).ToList();

                    Assert.Equal(1, blogs[0].Id);
                    Assert.Equal(2, blogs[1].Id);
                }
            }

            public class BlogContext : ContextBase
            {
                protected override void OnModelCreating(ModelBuilder modelBuilder) 
                    => modelBuilder.ForSqlServerUseSequenceHiLo();
            }
        }

        public class IdentityColumnWithDefaultValue : TestBase<IdentityColumnWithDefaultValue.BlogContext>
        {
            [ConditionalFact]
            [SqlServerCondition(SqlServerCondition.SupportsSequences)]
            public void Insert_with_default_value_from_sequence()
            {
                using (var context = new BlogContext())
                {
                    context.AddRange(new Blog { Name = "One Unicorn" }, new Blog { Name = "Two Unicorns" });

                    context.SaveChanges();
                }

                using (var context = new BlogContext())
                {
                    var blogs = context.Blogs.OrderBy(e => e.Id).ToList();

                    Assert.Equal(77, blogs[0].Id);
                    Assert.Equal(78, blogs[1].Id);
                }
            }

            public class BlogContext : ContextBase
            {
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
        }

        public class ReadOnlyIdentityColumnWithDefaultValue : TestBase<ReadOnlyIdentityColumnWithDefaultValue.BlogContext>
        {
            [ConditionalFact]
            [SqlServerCondition(SqlServerCondition.SupportsSequences)]
            public void Insert_with_default_value_from_sequence()
            {
                using (var context = new BlogContext())
                {
                    context.AddRange(new Blog { Name = "One Unicorn" }, new Blog { Name = "Two Unicorns" });

                    context.SaveChanges();
                }

                using (var context = new BlogContext())
                {
                    var blogs = context.Blogs.OrderBy(e => e.Id).ToList();

                    Assert.Equal(77, blogs[0].Id);
                    Assert.Equal(78, blogs[1].Id);
                }
            }

            public class BlogContext : ContextBase
            {
                protected override void OnModelCreating(ModelBuilder modelBuilder)
                {
                    modelBuilder
                        .HasSequence("MySequence")
                        .StartsAt(77);

                    // TODO: Nested closure for Metadata
                    modelBuilder
                        .Entity<Blog>()
                        .Property(e => e.Id)
                        .HasDefaultValueSql("next value for MySequence")
                        .Metadata.IsReadOnlyBeforeSave = true;
                }
            }
        }

        public class NoKeyGeneration : TestBase<NoKeyGeneration.BlogContext>
        {
            [Fact]
            public void Insert_with_explicit_non_sentinel_keys()
            {
                using (var context = new BlogContext())
                {
                    context.AddRange(new Blog { Id = 66, Name = "One Unicorn" }, new Blog { Id = 67, Name = "Two Unicorns" });

                    context.SaveChanges();
                }

                using (var context = new BlogContext())
                {
                    var blogs = context.Blogs.OrderBy(e => e.Id).ToList();

                    Assert.Equal(66, blogs[0].Id);
                    Assert.Equal(67, blogs[1].Id);
                }
            }

            public class BlogContext : ContextBase
            {
                protected override void OnModelCreating(ModelBuilder modelBuilder)
                {
                    modelBuilder
                        .Entity<Blog>()
                        .Property(e => e.Id)
                        .ValueGeneratedNever();
                }
            }
        }

        public class NoKeyGenerationChangeSentinel : TestBase<NoKeyGenerationChangeSentinel.BlogContext>
        {
            [Fact]
            public void Insert_with_explicit_with_sentinel_keys()
            {
                using (var context = new BlogContext())
                {
                    context.AddRange(new Blog { Id = 0, Name = "One Unicorn" }, new Blog { Id = 1, Name = "Two Unicorns" });

                    context.SaveChanges();
                }

                using (var context = new BlogContext())
                {
                    var blogs = context.Blogs.OrderBy(e => e.Id).ToList();

                    Assert.Equal(0, blogs[0].Id);
                    Assert.Equal(1, blogs[1].Id);
                }
            }

            public class BlogContext : ContextBase
            {
                protected override void OnModelCreating(ModelBuilder modelBuilder)
                {
                    // TODO: Rename sentinel value API
                    modelBuilder
                        .Entity<Blog>()
                        .Property(e => e.Id)
                        .ValueGeneratedNever()
                        .Metadata.SentinelValue = -1;
                }
            }
        }

        public class NoKeyGenerationNullableKey : TestBase<NoKeyGenerationNullableKey.BlogContext>
        {
            [Fact]
            public void Insert_with_explicit_with_sentinel_keys()
            {
                using (var context = new BlogContext())
                {
                    context.AddRange(
                        new NullableKeyBlog { Id = 0, Name = "One Unicorn" },
                        new NullableKeyBlog { Id = 1, Name = "Two Unicorns" });

                    context.SaveChanges();
                }

                using (var context = new BlogContext())
                {
                    var blogs = context.NullableKeyBlogs.OrderBy(e => e.Id).ToList();

                    Assert.Equal(0, blogs[0].Id);
                    Assert.Equal(1, blogs[1].Id);
                }
            }

            public class BlogContext : ContextBase
            {
                protected override void OnModelCreating(ModelBuilder modelBuilder)
                {
                    modelBuilder
                        .Entity<NullableKeyBlog>()
                        .Property(e => e.Id)
                        .ValueGeneratedNever();
                }
            }
        }

        public class NonKeyDefaultValue : TestBase<NonKeyDefaultValue.BlogContext>
        {
            [Fact]
            public void Insert_with_non_key_default_value_when_sentinel()
            {
                using (var context = new BlogContext())
                {
                    var blogs = new List<Blog>
                    {
                        new Blog { Name = "One Unicorn" },
                        new Blog { Name = "Two Unicorns", CreatedOn = new DateTime(1969, 8, 3, 0, 10, 0) }
                    };

                    context.AddRange(blogs);

                    context.SaveChanges();

                    Assert.NotEqual(new DateTime(), blogs[0].CreatedOn);
                    Assert.NotEqual(new DateTime(), blogs[1].CreatedOn);
                }

                using (var context = new BlogContext())
                {
                    var blogs = context.Blogs.OrderBy(e => e.Name).ToList();

                    Assert.NotEqual(new DateTime(), blogs[0].CreatedOn);
                    Assert.Equal(new DateTime(1969, 8, 3, 0, 10, 0), blogs[1].CreatedOn);

                    blogs[0].CreatedOn = new DateTime(1973, 9, 3, 0, 10, 0);
                    blogs[1].Name = "Zwo Unicorns";

                    context.SaveChanges();
                }

                using (var context = new BlogContext())
                {
                    var blogs = context.Blogs.OrderBy(e => e.Name).ToList();

                    Assert.Equal(new DateTime(1969, 8, 3, 0, 10, 0), blogs[1].CreatedOn);
                    Assert.Equal(new DateTime(1973, 9, 3, 0, 10, 0), blogs[0].CreatedOn);
                }
            }

            public class BlogContext : ContextBase
            {
                protected override void OnModelCreating(ModelBuilder modelBuilder)
                {
                    modelBuilder.Entity<Blog>()
                        .Property(e => e.CreatedOn)
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("getdate()");
                }
            }
        }

        public class NonKeyReadOnlyDefaultValue : TestBase<NonKeyReadOnlyDefaultValue.BlogContext>
        {
            [Fact]
            public void Insert_with_non_key_default_value()
            {
                using (var context = new BlogContext())
                {
                    context.AddRange(
                        new Blog { Name = "One Unicorn" },
                        new Blog { Name = "Two Unicorns" });

                    context.SaveChanges();

                    Assert.NotEqual(new DateTime(), context.Blogs.ToList()[0].CreatedOn);
                }

                DateTime dateTime0;

                using (var context = new BlogContext())
                {
                    var blogs = context.Blogs.OrderBy(e => e.Id).ToList();

                    dateTime0 = blogs[0].CreatedOn;

                    Assert.NotEqual(new DateTime(), dateTime0);
                    Assert.NotEqual(new DateTime(), blogs[1].CreatedOn);

                    blogs[0].Name = "One Pegasus";
                    blogs[1].CreatedOn = new DateTime(1973, 9, 3, 0, 10, 0);

                    context.SaveChanges();
                }

                using (var context = new BlogContext())
                {
                    var blogs = context.Blogs.OrderBy(e => e.Id).ToList();

                    Assert.Equal(dateTime0, blogs[0].CreatedOn);
                    Assert.Equal(new DateTime(1973, 9, 3, 0, 10, 0), blogs[1].CreatedOn);
                }
            }

            public class BlogContext : ContextBase
            {
                protected override void OnModelCreating(ModelBuilder modelBuilder)
                {
                    modelBuilder.Entity<Blog>()
                        .Property(e => e.CreatedOn)
                        .HasDefaultValueSql("getdate()")
                        .Metadata.IsReadOnlyBeforeSave = true;
                }
            }
        }

        public class ComputedColumn : TestBase<ComputedColumn.BlogContext>
        {
            [Fact]
            public void Insert_and_update_with_computed_column()
            {
                using (var context = new BlogContext())
                {
                    var blog = context.Add(new FullNameBlog { FirstName = "One", LastName = "Unicorn" }).Entity;

                    context.SaveChanges();

                    Assert.Equal("One Unicorn", blog.FullName);
                }

                using (var context = new BlogContext())
                {
                    var blog = context.FullNameBlogs.Single();

                    Assert.Equal("One Unicorn", blog.FullName);

                    blog.LastName = "Pegasus";

                    context.SaveChanges();

                    Assert.Equal("One Pegasus", blog.FullName);
                }
            }

            public class BlogContext : ContextBase
            {
                protected override void OnModelCreating(ModelBuilder modelBuilder)
                {
                    modelBuilder.Entity<FullNameBlog>()
                        .Property(e => e.FullName)
                        .HasComputedColumnSql("FirstName + ' ' + LastName");
                }
            }
        }

        public class ClientGuidKey : TestBase<ClientGuidKey.BlogContext>
        {
            [Fact]
            public void Insert_with_client_generated_GUID_key()
            {
                Guid afterSave;

                using (var context = new BlogContext())
                {
                    var blog = context.Add(new GuidBlog { Name = "One Unicorn" }).Entity;

                    var beforeSave = blog.Id;

                    context.SaveChanges();

                    afterSave = blog.Id;

                    Assert.Equal(beforeSave, afterSave);
                }

                using (var context = new BlogContext())
                {
                    Assert.Equal(afterSave, context.GuidBlogs.Single().Id);
                }
            }

            public class BlogContext : ContextBase
            {
            }
        }

        public class ServerGuidKey : TestBase<ServerGuidKey.BlogContext>
        {
            [Fact]
            public void Insert_with_server_generated_GUID_key()
            {
                Guid afterSave;

                using (var context = new BlogContext())
                {
                    var blog = context.Add(new GuidBlog { Name = "One Unicorn" }).Entity;

                    var beforeSave = blog.Id;

                    context.SaveChanges();

                    afterSave = blog.Id;

                    Assert.NotEqual(beforeSave, afterSave);
                }

                using (var context = new BlogContext())
                {
                    Assert.Equal(afterSave, context.GuidBlogs.Single().Id);
                }
            }

            public class BlogContext : ContextBase
            {
                protected override void OnModelCreating(ModelBuilder modelBuilder)
                {
                    modelBuilder
                        .Entity<GuidBlog>()
                        .Property(e => e.Id)
                        .HasDefaultValueSql("newsequentialid()");
                }
            }
        }

        // Negative cases

        public class DoNothingButSpecifyKeys : TestBase<DoNothingButSpecifyKeys.BlogContext>
        {
            [Fact]
            public void Insert_with_explicit_non_sentinel_keys()
            {
                using (var context = new BlogContext())
                {
                    context.AddRange(new Blog { Id = 1, Name = "One Unicorn" }, new Blog { Id = 2, Name = "Two Unicorns" });

                    // DbUpdateException : An error occurred while updating the entries. See the
                    // inner exception for details.
                    // SqlException : Cannot insert explicit value for identity column in table 
                    // 'Blog' when IDENTITY_INSERT is set to OFF.
                    Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                }
            }

            public class BlogContext : ContextBase
            {
            }
        }

        public class DoNothingButSpecifyKeysUsingSentinel : TestBase<DoNothingButSpecifyKeysUsingSentinel.BlogContext>
        {
            [Fact]
            public void Insert_with_explicit_sentinel_keys()
            {
                using (var context = new BlogContext())
                {
                    context.AddRange(new Blog { Id = 0, Name = "One Unicorn" }, new Blog { Id = 1, Name = "Two Unicorns" });

                    // DbUpdateException : An error occurred while updating the entries. See the
                    // inner exception for details.
                    // SqlException : Cannot insert explicit value for identity column in table 
                    // 'Blog' when IDENTITY_INSERT is set to OFF.
                    Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                }
            }

            public class BlogContext : ContextBase
            {
            }
        }

        public class SpecifyKeysUsingSentinel : TestBase<SpecifyKeysUsingSentinel.BlogContext>
        {
            [Fact]
            public void Insert_with_explicit_sentinel_keys()
            {
                using (var context = new BlogContext())
                {
                    context.AddRange(new Blog { Id = 0, Name = "One Unicorn" }, new Blog { Id = 1, Name = "Two Unicorns" });

                    // The property 'Id' on entity type 'Blog' has a temporary value while attempting to change
                    // the entity's state to 'Unchanged'. Either set a permanent value explicitly or ensure
                    // that the database is configured to generate values for this property.
                    Assert.Equal(
                        CoreStrings.TempValuePersists("Id", "Blog", "Unchanged"),
                        Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
                }
            }

            public class BlogContext : ContextBase
            {
                protected override void OnModelCreating(ModelBuilder modelBuilder)
                {
                    modelBuilder
                        .Entity<Blog>()
                        .Property(e => e.Id)
                        .ValueGeneratedNever();
                }
            }
        }

        public class ReadOnlyIdentityColumnWithDefaultValueThrows : TestBase<ReadOnlyIdentityColumnWithDefaultValueThrows.BlogContext>
        {
            [ConditionalFact]
            [SqlServerCondition(SqlServerCondition.SupportsSequences)]
            public void Insert_explicit_value_throws_when_readonly_before_save()
            {
                using (var context = new BlogContext())
                {
                    context.AddRange(new Blog { Id = 1, Name = "One Unicorn" }, new Blog { Name = "Two Unicorns" });

                    // The property 'Id' on entity type 'Blog' is defined to be read-only before it is 
                    // saved, but its value has been set to something other than a temporary or default value.
                    Assert.Equal(
                        CoreStrings.PropertyReadOnlyBeforeSave("Id", "Blog"),
                        Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
                }
            }

            public class BlogContext : ContextBase
            {
                protected override void OnModelCreating(ModelBuilder modelBuilder)
                {
                    modelBuilder.HasSequence("MySequence");

                    modelBuilder
                        .Entity<Blog>()
                        .Property(e => e.Id)
                        .HasDefaultValueSql("next value for MySequence")
                        .Metadata.IsReadOnlyBeforeSave = true;
                }
            }
        }

        public class NonKeyReadOnlyDefaultValueThrows : TestBase<NonKeyReadOnlyDefaultValueThrows.BlogContext>
        {
            [Fact]
            public void Insert_explicit_value_throws_when_readonly_before_save()
            {
                using (var context = new BlogContext())
                {
                    context.AddRange(
                        new Blog { Name = "One Unicorn" },
                        new Blog { Name = "Two Unicorns", CreatedOn = new DateTime(1969, 8, 3, 0, 10, 0) });

                    // The property 'CreatedOn' on entity type 'Blog' is defined to be read-only before it is 
                    // saved, but its value has been set to something other than a temporary or default value.
                    Assert.Equal(
                        CoreStrings.PropertyReadOnlyBeforeSave("CreatedOn", "Blog"),
                        Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
                }
            }

            public class BlogContext : ContextBase
            {
                protected override void OnModelCreating(ModelBuilder modelBuilder)
                {
                    modelBuilder.Entity<Blog>()
                        .Property(e => e.CreatedOn)
                        .HasDefaultValueSql("getdate()")
                        .Metadata.IsReadOnlyBeforeSave = true;
                }
            }
        }

        public class ComputedColumnInsertValue : TestBase<ComputedColumnInsertValue.BlogContext>
        {
            [Fact]
            public void Insert_explicit_value_into_computed_column()
            {
                using (var context = new BlogContext())
                {
                    context.Add(new FullNameBlog { FirstName = "One", LastName = "Unicorn", FullName = "Gerald" });

                    // The property 'FullName' on entity type 'FullNameBlog' is defined to be read-only before it is 
                    // saved, but its value has been set to something other than a temporary or default value.
                    Assert.Equal(
                        CoreStrings.PropertyReadOnlyBeforeSave("FullName", "FullNameBlog"),
                        Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
                }
            }

            public class BlogContext : ContextBase
            {
                protected override void OnModelCreating(ModelBuilder modelBuilder)
                {
                    modelBuilder.Entity<FullNameBlog>()
                        .Property(e => e.FullName)
                        .HasComputedColumnSql("FirstName + ' ' + LastName");
                }
            }
        }

        public class ComputedColumnUpdateValue : TestBase<ComputedColumnUpdateValue.BlogContext>
        {
            [Fact]
            public void Update_explicit_value_in_computed_column()
            {
                using (var context = new BlogContext())
                {
                    context.Add(new FullNameBlog { FirstName = "One", LastName = "Unicorn" });

                    context.SaveChanges();
                }

                using (var context = new BlogContext())
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

            public class BlogContext : ContextBase
            {
                protected override void OnModelCreating(ModelBuilder modelBuilder)
                {
                    modelBuilder.Entity<FullNameBlog>()
                        .Property(e => e.FullName)
                        .HasComputedColumnSql("FirstName + ' ' + LastName");
                }
            }
        }

        // Concurrency

        public class ConcurrencyWithRowversion : TestBase<ConcurrencyWithRowversion.BlogContext>
        {
            [Fact]
            public void Resolve_concurreny()
            {
                using (var context = new BlogContext())
                {
                    var blog = context.Add(new ConcurrentBlog { Name = "One Unicorn" }).Entity;

                    context.SaveChanges();

                    using (var innerContext = new BlogContext())
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

            public class BlogContext : ContextBase
            {
                protected override void OnModelCreating(ModelBuilder modelBuilder)
                {
                    modelBuilder.Entity<ConcurrentBlog>()
                        .Property(e => e.Timestamp)
                        .ValueGeneratedOnAddOrUpdate()
                        .IsConcurrencyToken();
                }
            }
        }

        public class Blog
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public DateTime CreatedOn { get; set; }
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
        }

        public class ConcurrentBlog
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public byte[] Timestamp { get; set; }
        }

        public abstract class ContextBase : DbContext
        {
            public DbSet<Blog> Blogs { get; set; }
            public DbSet<NullableKeyBlog> NullableKeyBlogs { get; set; }
            public DbSet<FullNameBlog> FullNameBlogs { get; set; }
            public DbSet<GuidBlog> GuidBlogs { get; set; }
            public DbSet<ConcurrentBlog> ConcurrentBlogs { get; set; }

            protected ContextBase()
            {
                Database.EnsureCreated();
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                var name = GetType().FullName.Substring((GetType().Namespace + nameof(SqlServerValueGenerationScenariosTest)).Length + 2);
                optionsBuilder.UseSqlServer(SqlServerTestStore.CreateConnectionString(name));
            }
        }

        public class TestBase<TContext>
            where TContext : ContextBase, new()
        {
            public TestBase()
            {
                using (var context = new TContext())
                {
                    context.Database.EnsureDeleted();
                }
            }
        }
    }
}
