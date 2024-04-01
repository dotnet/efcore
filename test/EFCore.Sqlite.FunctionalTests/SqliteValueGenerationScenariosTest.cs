// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class SqliteValueGenerationScenariosTest
{
    [ConditionalFact]
    public void Insert_with_Identity_column()
    {
        using (var context = new BlogContextIdentity(nameof(Insert_with_Identity_column)))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            context.AddRange(
                new Blog { Name = "One Unicorn" }, new Blog { Name = "Two Unicorns" });

            context.SaveChanges();
        }

        using (var context = new BlogContextIdentity(nameof(Insert_with_Identity_column)))
        {
            var blogs = context.Blogs.OrderBy(e => e.Id).ToList();

            Assert.Equal(1, blogs[0].Id);
            Assert.Equal(2, blogs[1].Id);
        }
    }

    public class BlogContextIdentity(string databaseName) : ContextBase(databaseName);

    [ConditionalFact]
    public void Insert_uint_to_Identity_column_using_value_converter()
    {
        using (var context = new BlogContextUIntToIdentityUsingValueConverter(
                   nameof(Insert_uint_to_Identity_column_using_value_converter)))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            context.AddRange(
                new BlogWithUIntKey { Name = "One Unicorn" }, new BlogWithUIntKey { Name = "Two Unicorns" });

            context.SaveChanges();
        }

        using (var context = new BlogContextUIntToIdentityUsingValueConverter(
                   nameof(Insert_uint_to_Identity_column_using_value_converter)))
        {
            var blogs = context.UnsignedBlogs.OrderBy(e => e.Id).ToList();

            Assert.Equal((uint)1, blogs[0].Id);
            Assert.Equal((uint)2, blogs[1].Id);
        }
    }

    public class BlogContextUIntToIdentityUsingValueConverter(string databaseName) : ContextBase(databaseName)
    {
        public DbSet<BlogWithUIntKey> UnsignedBlogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .Entity<BlogWithUIntKey>()
                .Property(e => e.Id)
                .HasConversion<int>();
        }
    }

    public class BlogWithUIntKey
    {
        public uint Id { get; set; }
        public string Name { get; set; }
    }

    [ConditionalFact]
    public void Insert_int_enum_to_Identity_column()
    {
        using (var context = new BlogContextIntEnumToIdentity(nameof(Insert_int_enum_to_Identity_column)))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            context.AddRange(
                new BlogWithIntEnumKey { Name = "One Unicorn" }, new BlogWithIntEnumKey { Name = "Two Unicorns" });

            context.SaveChanges();
        }

        using (var context = new BlogContextIntEnumToIdentity(nameof(Insert_int_enum_to_Identity_column)))
        {
            var blogs = context.EnumBlogs.OrderBy(e => e.Id).ToList();

            Assert.Equal(1, (int)blogs[0].Id);
            Assert.Equal(2, (int)blogs[1].Id);
        }
    }

    public class BlogContextIntEnumToIdentity(string databaseName) : ContextBase(databaseName)
    {
        public DbSet<BlogWithIntEnumKey> EnumBlogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .Entity<BlogWithIntEnumKey>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd();
        }
    }

    public class BlogWithIntEnumKey
    {
        public IntKey Id { get; set; }
        public string Name { get; set; }
    }

    public enum IntKey;

    [ConditionalFact]
    public void Insert_ushort_enum_to_Identity_column()
    {
        using (var context = new BlogContextUShortEnumToIdentity(nameof(Insert_ushort_enum_to_Identity_column)))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            context.AddRange(
                new BlogWithUShortEnumKey { Name = "One Unicorn" }, new BlogWithUShortEnumKey { Name = "Two Unicorns" });

            context.SaveChanges();
        }

        using (var context = new BlogContextUShortEnumToIdentity(nameof(Insert_ushort_enum_to_Identity_column)))
        {
            var blogs = context.EnumBlogs.OrderBy(e => e.Id).ToList();

            Assert.Equal(1, (int)blogs[0].Id);
            Assert.Equal(2, (int)blogs[1].Id);
        }
    }

    public class BlogContextUShortEnumToIdentity(string databaseName) : ContextBase(databaseName)
    {
        public DbSet<BlogWithUShortEnumKey> EnumBlogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .Entity<BlogWithUShortEnumKey>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd();
        }
    }

    public class BlogWithUShortEnumKey
    {
        public UShortKey Id { get; set; }
        public string Name { get; set; }
    }

    public enum UShortKey : ushort;

    [ConditionalFact]
    public void Insert_string_to_Identity_column_using_value_converter()
    {
        using (var context = new BlogContextStringToIdentityUsingValueConverter(
                   nameof(Insert_string_to_Identity_column_using_value_converter)))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            context.AddRange(
                new BlogWithStringKey { Name = "One Unicorn" }, new BlogWithStringKey { Name = "Two Unicorns" });

            context.SaveChanges();
        }

        using (var context = new BlogContextStringToIdentityUsingValueConverter(
                   nameof(Insert_string_to_Identity_column_using_value_converter)))
        {
            var blogs = context.StringyBlogs.OrderBy(e => e.Id).ToList();

            Assert.Equal("1", blogs[0].Id);
            Assert.Equal("2", blogs[1].Id);
        }
    }

    public class BlogContextStringToIdentityUsingValueConverter(string databaseName) : ContextBase(databaseName)
    {
        public DbSet<BlogWithStringKey> StringyBlogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            Guid guid;
            modelBuilder
                .Entity<BlogWithStringKey>()
                .Property(e => e.Id)
                .HasValueGenerator<TemporaryStringValueGenerator>()
                .HasConversion(
                    v => Guid.TryParse(v, out guid)
                        ? default
                        : int.Parse(v),
                    v => v.ToString())
                .ValueGeneratedOnAdd();
        }
    }

    public class BlogWithStringKey
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    [ConditionalFact]
    public void Insert_with_explicit_non_default_keys()
    {
        using (var context = new BlogContextNoKeyGeneration(nameof(Insert_with_explicit_non_default_keys)))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            context.AddRange(
                new Blog { Id = 66, Name = "One Unicorn" }, new Blog { Id = 67, Name = "Two Unicorns" });

            context.SaveChanges();
        }

        using (var context = new BlogContextNoKeyGeneration(nameof(Insert_with_explicit_non_default_keys)))
        {
            var blogs = context.Blogs.OrderBy(e => e.Id).ToList();

            Assert.Equal(66, blogs[0].Id);
            Assert.Equal(67, blogs[1].Id);
        }
    }

    public class BlogContextNoKeyGeneration(string databaseName) : ContextBase(databaseName)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .Entity<Blog>()
                .Property(e => e.Id)
                .ValueGeneratedNever();
        }
    }

    [ConditionalFact]
    public void Insert_with_explicit_with_default_keys()
    {
        using (var context = new BlogContextNoKeyGenerationNullableKey(nameof(Insert_with_explicit_with_default_keys)))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            context.AddRange(
                new NullableKeyBlog { Id = 0, Name = "One Unicorn" },
                new NullableKeyBlog { Id = 1, Name = "Two Unicorns" });

            context.SaveChanges();
        }

        using (var context = new BlogContextNoKeyGenerationNullableKey(nameof(Insert_with_explicit_with_default_keys)))
        {
            var blogs = context.NullableKeyBlogs.OrderBy(e => e.Id).ToList();

            Assert.Equal(0, blogs[0].Id);
            Assert.Equal(1, blogs[1].Id);
        }
    }

    public class BlogContextNoKeyGenerationNullableKey(string databaseName) : ContextBase(databaseName)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .Entity<NullableKeyBlog>()
                .Property(e => e.Id)
                .ValueGeneratedNever();
        }
    }

    [ConditionalFact]
    public void Insert_with_non_key_default_value()
    {
        using (var context = new BlogContextNonKeyDefaultValue(nameof(Insert_with_non_key_default_value)))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var blogs = new List<Blog>
            {
                new() { Name = "One Unicorn" },
                new()
                {
                    Name = "Two Unicorns",
                    CreatedOn = new DateTime(1969, 8, 3, 0, 10, 0),
                    NeedsConverter = new NeedsConverter(111),
                }
            };

            context.AddRange(blogs);

            context.SaveChanges();

            Assert.NotEqual(new DateTime(), blogs[0].CreatedOn);
            Assert.NotEqual(new DateTime(), blogs[1].CreatedOn);
            Assert.Equal(111, blogs[1].NeedsConverter.Value);
        }

        using (var context = new BlogContextNonKeyDefaultValue(nameof(Insert_with_non_key_default_value)))
        {
            var blogs = context.Blogs.OrderBy(e => e.Name).ToList();
            Assert.Equal(3, blogs.Count);

            Assert.NotEqual(new DateTime(), blogs[0].CreatedOn);
            Assert.Equal(new DateTime(1969, 8, 3, 0, 10, 0), blogs[1].CreatedOn);
            Assert.Equal(new DateTime(1974, 8, 3, 0, 10, 0), blogs[2].CreatedOn);

            blogs[0].CreatedOn = new DateTime(1973, 9, 3, 0, 10, 0);

            blogs[1].Name = "X Unicorns";
            blogs[1].NeedsConverter = new NeedsConverter(222);

            blogs[2].Name = "Y Unicorns";
            blogs[2].NeedsConverter = new NeedsConverter(333);

            context.SaveChanges();
        }

        using (var context = new BlogContextNonKeyDefaultValue(nameof(Insert_with_non_key_default_value)))
        {
            var blogs = context.Blogs.OrderBy(e => e.Name).ToList();
            Assert.Equal(3, blogs.Count);

            Assert.Equal(new DateTime(1973, 9, 3, 0, 10, 0), blogs[0].CreatedOn);
            Assert.Equal(new DateTime(1969, 8, 3, 0, 10, 0), blogs[1].CreatedOn);
            Assert.Equal(222, blogs[1].NeedsConverter.Value);
            Assert.Equal(new DateTime(1974, 8, 3, 0, 10, 0), blogs[2].CreatedOn);
            Assert.Equal(333, blogs[2].NeedsConverter.Value);
        }
    }

    public class BlogContextNonKeyDefaultValue(string databaseName) : ContextBase(databaseName)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Blog>(
                b =>
                {
                    b.Property(e => e.CreatedOn).HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.HasData(
                        new Blog
                        {
                            Id = 9979,
                            Name = "W Unicorns",
                            CreatedOn = new DateTime(1974, 8, 3, 0, 10, 0),
                            NeedsConverter = new NeedsConverter(111),
                        });
                });
        }
    }

    [ConditionalFact]
    public void Insert_with_non_key_default_value_readonly()
    {
        using (var context = new BlogContextNonKeyReadOnlyDefaultValue(nameof(Insert_with_non_key_default_value_readonly)))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            context.AddRange(
                new Blog { Name = "One Unicorn" },
                new Blog { Name = "Two Unicorns" });

            context.SaveChanges();

            Assert.NotEqual(new DateTime(), context.Blogs.ToList()[0].CreatedOn);
        }

        DateTime dateTime0;

        using (var context = new BlogContextNonKeyReadOnlyDefaultValue(nameof(Insert_with_non_key_default_value_readonly)))
        {
            var blogs = context.Blogs.OrderBy(e => e.Id).ToList();

            dateTime0 = blogs[0].CreatedOn;

            Assert.NotEqual(new DateTime(), dateTime0);
            Assert.NotEqual(new DateTime(), blogs[1].CreatedOn);

            blogs[0].Name = "One Pegasus";
            blogs[1].CreatedOn = new DateTime(1973, 9, 3, 0, 10, 0);

            context.SaveChanges();
        }

        using (var context = new BlogContextNonKeyReadOnlyDefaultValue(nameof(Insert_with_non_key_default_value_readonly)))
        {
            var blogs = context.Blogs.OrderBy(e => e.Id).ToList();

            Assert.Equal(dateTime0, blogs[0].CreatedOn);
            Assert.Equal(new DateTime(1973, 9, 3, 0, 10, 0), blogs[1].CreatedOn);
        }
    }

    public class BlogContextNonKeyReadOnlyDefaultValue(string databaseName) : ContextBase(databaseName)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Blog>()
                .Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Throw);
        }
    }

    [ConditionalFact]
    public void Insert_with_client_generated_GUID_key()
    {
        Guid afterSave;
        using (var context = new BlogContextClientGuidKey(nameof(Insert_with_client_generated_GUID_key)))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var blog = context.Add(
                new GuidBlog { Name = "One Unicorn" }).Entity;

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

        using (var context = new BlogContextClientGuidKey(nameof(Insert_with_client_generated_GUID_key)))
        {
            Assert.Equal(afterSave, context.GuidBlogs.Single().Id);
        }
    }

    public class BlogContextClientGuidKey(string databaseName) : ContextBase(databaseName)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<GuidBlog>(
                eb =>
                {
                    eb.HasAlternateKey(e => e.NotId);
                    eb.Property(e => e.NotId).ValueGeneratedOnAdd();
                });
        }
    }

    public class BlogContextClientGuidNonKey(string databaseName) : ContextBase(databaseName)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<GuidBlog>().Property(e => e.NotId).ValueGeneratedOnAdd();
        }
    }

    [ConditionalFact]
    public void Insert_with_explicit_default_keys()
    {
        using var context = new BlogContext(nameof(Insert_with_explicit_default_keys));
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        context.AddRange(
            new Blog { Id = 0, Name = "One Unicorn" }, new Blog { Id = 1, Name = "Two Unicorns" });

        // DbUpdateException : An error occurred while updating the entries. See the
        // inner exception for details.
        // SqlException : Cannot insert explicit value for identity column in table
        // 'Blog' when IDENTITY_INSERT is set to OFF.
        var updateException = Assert.Throws<DbUpdateException>(() => context.SaveChanges());
        Assert.Single(updateException.Entries);
    }

    public class BlogContext(string databaseName) : ContextBase(databaseName);

    [ConditionalFact]
    public void Insert_with_implicit_default_keys()
    {
        using (var context = new BlogContextSpecifyKeysUsingDefault(nameof(Insert_with_implicit_default_keys)))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            context.AddRange(
                new Blog { Id = 0, Name = "One Unicorn" }, new Blog { Id = 1, Name = "Two Unicorns" });

            context.SaveChanges();
        }

        using (var context = new BlogContextSpecifyKeysUsingDefault(nameof(Insert_with_implicit_default_keys)))
        {
            var blogs = context.Blogs.OrderBy(e => e.Id).ToList();

            Assert.Equal(0, blogs[0].Id);
            Assert.Equal(1, blogs[1].Id);
        }
    }

    public class BlogContextSpecifyKeysUsingDefault(string databaseName) : ContextBase(databaseName)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .Entity<Blog>()
                .Property(e => e.Id)
                .ValueGeneratedNever();
        }
    }

    [ConditionalFact]
    public void Insert_explicit_value_throws_when_readonly_before_save()
    {
        using var context = new BlogContextNonKeyReadOnlyDefaultValue(nameof(Insert_explicit_value_throws_when_readonly_before_save));
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        context.AddRange(
            new Blog { Name = "One Unicorn" },
            new Blog { Name = "Two Unicorns", CreatedOn = new DateTime(1969, 8, 3, 0, 10, 0) });

        // The property 'CreatedOn' on entity type 'Blog' is defined to be read-only before it is
        // saved, but its value has been set to something other than a temporary or default value.
        Assert.Equal(
            CoreStrings.PropertyReadOnlyBeforeSave("CreatedOn", "Blog"),
            Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
    }

    public class Blog
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedOn { get; set; }
        public NeedsConverter NeedsConverter { get; set; }
        public int? OtherId { get; set; }
    }

    public class NeedsConverter(int value)
    {
        public int Value { get; } = value;

        public override bool Equals(object obj)
            => throw new InvalidOperationException();

        public override int GetHashCode()
            => throw new InvalidOperationException();
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Blog>()
                .Property(e => e.NeedsConverter)
                .HasConversion(
                    v => v.Value,
                    v => new NeedsConverter(v),
                    new ValueComparer<NeedsConverter>(
                        (l, r) => (l == null && r == null) || (l != null && r != null && l.Value == r.Value),
                        v => v.Value.GetHashCode(),
                        v => new NeedsConverter(v.Value)))
                .HasDefaultValue(new NeedsConverter(999));

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .EnableServiceProviderCaching(false)
                .UseSqlite($"DataSource = {_databaseName}.db");
    }
}
