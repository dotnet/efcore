// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;

namespace Microsoft.EntityFrameworkCore;

[CosmosCondition(CosmosCondition.DoesNotUseTokenCredential)]
public class AddHocFullTextSearchCosmosTest(NonSharedFixture fixture) : NonSharedModelTestBase(fixture), IClassFixture<NonSharedFixture>
{
    protected override string StoreName
        => "AdHocFullTextSearchTests";

    protected override ITestStoreFactory TestStoreFactory
        => CosmosTestStoreFactory.Instance;

    #region CompositeFullTextIndex

    [ConditionalFact]
    public async Task Validate_composite_full_text_index_throws()
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => InitializeAsync<ContextCompositeFullTextIndex>())).Message;

        Assert.Equal(
            CosmosStrings.CompositeFullTextIndex(
                nameof(ContextCompositeFullTextIndex.Entity),
                "Name,Number"),
            message);
    }

    protected class ContextCompositeFullTextIndex(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Entity> Entities { get; set; } = null!;

        public class Entity
        {
            public int Id { get; set; }
            public string PartitionKey { get; set; } = null!;
            public string Name { get; set; } = null!;
            public int Number { get; set; }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Entity>(b =>
            {
                b.ToContainer("Entities");
                b.HasPartitionKey(x => x.PartitionKey);
                b.Property(x => x.Name).EnableFullTextSearch();
                b.Property(x => x.Number).EnableFullTextSearch();
                b.HasIndex(x => new { x.Name, x.Number }).IsFullTextIndex();
            });
    }

    #endregion

    #region FullTextPropertyOnCollectionNavigation

    [ConditionalFact]
    public async Task Validate_full_text_property_on_collection_navigation_container_creation()
    {
        var message = (await Assert.ThrowsAsync<NotSupportedException>(
            () => InitializeAsync<ContextFullTextPropertyOnCollectionNavigation>())).Message;

        Assert.Equal(
            CosmosStrings.CreatingContainerWithFullTextOrVectorOnCollectionNotSupported("/Collection"),
            message);
    }

    protected class ContextFullTextPropertyOnCollectionNavigation(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Entity> Entities { get; set; } = null!;

        public class Entity
        {
            public int Id { get; set; }
            public string PartitionKey { get; set; } = null!;
            public List<Json> Collection { get; set; } = null!;
        }

        public class Json
        {
            public string Name { get; set; } = null!;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Entity>(b =>
            {
                b.ToContainer("Entities");
                b.HasPartitionKey(x => x.PartitionKey);
                b.OwnsMany(x => x.Collection, bb =>
                {
                    bb.Property(x => x.Name).EnableFullTextSearch();
                    bb.HasIndex(x => x.Name).IsFullTextIndex();
                });
            });
    }

    #endregion

    #region FullTextOnNonStringProperty

    [ConditionalFact]
    public async Task Validate_full_text_on_non_string_property()
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => InitializeAsync<ContextFullTextOnNonStringProperty>())).Message;

        Assert.Equal(
            CosmosStrings.FullTextSearchConfiguredForUnsupportedPropertyType(
                nameof(ContextFullTextOnNonStringProperty.Entity),
                nameof(ContextFullTextOnNonStringProperty.Entity.Number),
                typeof(int).Name),
            message);
    }

    protected class ContextFullTextOnNonStringProperty(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Entity> Entities { get; set; } = null!;

        public class Entity
        {
            public int Id { get; set; }
            public string PartitionKey { get; set; } = null!;
            public int Number { get; set; }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Entity>(b =>
            {
                b.ToContainer("Entities");
                b.HasPartitionKey(x => x.PartitionKey);
                b.Property(x => x.Number).EnableFullTextSearch();
                b.HasIndex(x => x.Number).IsFullTextIndex();
            });
    }

    #endregion

    #region SettingDefaultFullTextSearchLanguage

    [ConditionalFact]
    public async Task Set_unsupported_full_text_search_default_language()
    {
        var exception = (await Assert.ThrowsAsync<CosmosException>(
            () => InitializeAsync<ContextSettingDefaultFullTextSearchLanguage>()));

        Assert.Contains("The Full Text Policy contains an unsupported language xx-YY.", exception.Message);
    }

    protected class ContextSettingDefaultFullTextSearchLanguage(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Entity> Entities { get; set; } = null!;

        public class Entity
        {
            public int Id { get; set; }
            public string PartitionKey { get; set; } = null!;
            public string Name { get; set; } = null!;
            public int Number { get; set; }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Entity>(b =>
            {
                b.ToContainer("Entities").HasDefaultFullTextLanguage("xx-YY");
                b.HasPartitionKey(x => x.PartitionKey);
                b.Property(x => x.Name).EnableFullTextSearch();
                b.HasIndex(x => x.Name).IsFullTextIndex();
            });
    }

    #endregion

    #region DefaultFullTextSearchLanguageMismatch

    [ConditionalFact]
    public async Task Set_different_full_text_search_default_language_for_the_same_container()
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => InitializeAsync<ContextDefaultFullTextSearchLanguageMismatch>())).Message;

        Assert.Equal(
            CosmosStrings.FullTextSearchDefaultLanguageMismatch(
                "pl-PL",
                nameof(ContextDefaultFullTextSearchLanguageMismatch.Entity1),
                nameof(ContextDefaultFullTextSearchLanguageMismatch.Entity2),
                "en-US",
                "Entities"), message);
    }

    protected class ContextDefaultFullTextSearchLanguageMismatch(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Entity1> Entities1 { get; set; } = null!;
        public DbSet<Entity2> Entities2 { get; set; } = null!;

        public class Entity1
        {
            public int Id { get; set; }
            public string PartitionKey { get; set; } = null!;
            public string Name { get; set; } = null!;
            public int Number { get; set; }
        }

        public class Entity2
        {
            public int Id { get; set; }
            public string PartitionKey { get; set; } = null!;
            public string Name { get; set; } = null!;
            public int Number { get; set; }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entity1>(b =>
            {
                b.ToContainer("Entities").HasDefaultFullTextLanguage("pl-PL");
                b.HasPartitionKey(x => x.PartitionKey);
                b.Property(x => x.Name).EnableFullTextSearch();
                b.HasIndex(x => x.Name).IsFullTextIndex();
            });

            modelBuilder.Entity<Entity2>(b =>
            {
                b.ToContainer("Entities").HasDefaultFullTextLanguage("en-US");
                b.HasPartitionKey(x => x.PartitionKey);
                b.Property(x => x.Name).EnableFullTextSearch();
                b.HasIndex(x => x.Name).IsFullTextIndex();
            });
        }
    }

    #endregion

    #region DefaultFullTextSearchLanguageNoMismatchWhenNotSpecified

    [ConditionalFact]
    public async Task Explicitly_setting_default_full_text_language_doesnt_clash_with_not_setting_it_on_other_entity_for_the_same_container()
    {
        var exception = (await Assert.ThrowsAsync<CosmosException>(
            () => InitializeAsync<ContextDefaultFullTextSearchLanguageNoMismatchWhenNotSpecified>()));

        Assert.Contains("The Full Text Policy contains an unsupported language xx-YY.", exception.Message);
    }

    protected class ContextDefaultFullTextSearchLanguageNoMismatchWhenNotSpecified(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Entity1> Entities1 { get; set; } = null!;
        public DbSet<Entity2> Entities2 { get; set; } = null!;
        public DbSet<Entity2> Entities3 { get; set; } = null!;

        public class Entity1
        {
            public int Id { get; set; }
            public string PartitionKey { get; set; } = null!;
            public string Name { get; set; } = null!;
            public int Number { get; set; }
        }

        public class Entity2
        {
            public int Id { get; set; }
            public string PartitionKey { get; set; } = null!;
            public string Name { get; set; } = null!;
            public int Number { get; set; }
        }

        public class Entity3
        {
            public int Id { get; set; }
            public string PartitionKey { get; set; } = null!;
            public string Name { get; set; } = null!;
            public int Number { get; set; }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entity1>(b =>
            {
                b.ToContainer("Entities");
                b.HasPartitionKey(x => x.PartitionKey);
                b.Property(x => x.Name).EnableFullTextSearch();
                b.HasIndex(x => x.Name).IsFullTextIndex();
            });

            modelBuilder.Entity<Entity2>(b =>
            {
                b.ToContainer("Entities").HasDefaultFullTextLanguage("xx-YY");
                b.HasPartitionKey(x => x.PartitionKey);
                b.Property(x => x.Name).EnableFullTextSearch();
                b.HasIndex(x => x.Name).IsFullTextIndex();
            });

            modelBuilder.Entity<Entity3>(b =>
            {
                b.ToContainer("Entities");
                b.HasPartitionKey(x => x.PartitionKey);
                b.Property(x => x.Name).EnableFullTextSearch();
                b.HasIndex(x => x.Name).IsFullTextIndex();
            });
        }
    }

    #endregion

    #region DefaultFullTextSearchLanguageUsedWhenPropertyDoesntSpecifyOneExplicitly

    [ConditionalFact]
    public async Task Default_full_text_language_is_used_for_full_text_properties_if_they_dont_specify_language_themselves()
    {
        var exception = (await Assert.ThrowsAsync<CosmosException>(
            () => InitializeAsync<ContextDefaultFullTextSearchLanguageUsedWhenPropertyDoesntSpecifyOneExplicitly>()));

        Assert.Contains("The Full Text Policy contains an unsupported language xx-YY.", exception.Message);
    }

    protected class ContextDefaultFullTextSearchLanguageUsedWhenPropertyDoesntSpecifyOneExplicitly(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Entity> Entities { get; set; } = null!;

        public class Entity
        {
            public int Id { get; set; }
            public string PartitionKey { get; set; } = null!;
            public string Name { get; set; } = null!;
            public int Number { get; set; }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Entity>(b =>
        {
            b.ToContainer("Entities").HasDefaultFullTextLanguage("xx-YY");
            b.HasPartitionKey(x => x.PartitionKey);
            b.Property(x => x.Name).EnableFullTextSearch();
            b.HasIndex(x => x.Name).IsFullTextIndex();
        });
    }

    #endregion

    #region ExplicitFullTextLanguageOverridesTheDefault

    [ConditionalFact]
    public async Task Explicitly_setting_full_text_language_overrides_default()
    {
        var exception = (await Assert.ThrowsAsync<CosmosException>(
            () => InitializeAsync<ContextExplicitFullTextLanguageOverridesTheDefault>()));

        Assert.Contains("The Full Text Policy contains an unsupported language xx-YY.", exception.Message);
    }

    protected class ContextExplicitFullTextLanguageOverridesTheDefault(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Entity> Entities { get; set; } = null!;

        public class Entity
        {
            public int Id { get; set; }
            public string PartitionKey { get; set; } = null!;
            public string Name { get; set; } = null!;
            public int Number { get; set; }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Entity>(b =>
        {
            b.ToContainer("Entities").HasDefaultFullTextLanguage("en-US");
            b.HasPartitionKey(x => x.PartitionKey);
            b.Property(x => x.Name).EnableFullTextSearch("xx-YY");
            b.HasIndex(x => x.Name).IsFullTextIndex();
        });
    }

    #endregion

    #region EnableThenDisable

    [ConditionalFact]
    public async Task Enable_full_text_search_for_property_then_disable_it()
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => InitializeAsync<ContextEnableThenDisable>())).Message;

        Assert.Equal(
            CosmosStrings.FullTextIndexOnNonFullTextProperty(
                nameof(ContextEnableThenDisable.Entity),
                nameof(ContextEnableThenDisable.Entity.Name),
                nameof(CosmosPropertyBuilderExtensions.EnableFullTextSearch)),
            message);
    }

    protected class ContextEnableThenDisable(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Entity> Entities { get; set; } = null!;

        public class Entity
        {
            public int Id { get; set; }
            public string PartitionKey { get; set; } = null!;
            public string Name { get; set; } = null!;
            public int Number { get; set; }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Entity>(b =>
            {
                b.ToContainer("Entities");
                b.HasPartitionKey(x => x.PartitionKey);
                b.Property(x => x.Name).EnableFullTextSearch("de-DE");
                b.Property(x => x.Name).EnableFullTextSearch(language: null, enabled: false);
                b.HasIndex(x => x.Name).IsFullTextIndex();
            });
    }

    #endregion
}
