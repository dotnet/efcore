// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

public class IndexTest
{
    [Fact]
    public void Throws_when_model_is_readonly()
    {
        var model = CreateModel();
        var entityType = model.AddEntityType("E");
        var property = entityType.AddProperty("P", typeof(int));
        var index = entityType.AddIndex([property]);

        model.FinalizeModel();

        Assert.Equal(
            CoreStrings.ModelReadOnly,
            Assert.Throws<InvalidOperationException>(() => entityType.AddIndex([property])).Message);

        Assert.Equal(
            CoreStrings.ModelReadOnly,
            Assert.Throws<InvalidOperationException>(() => entityType.AddIndex([property], "Name")).Message);

        Assert.Equal(
            CoreStrings.ModelReadOnly,
            Assert.Throws<InvalidOperationException>(() => entityType.RemoveIndex(index)).Message);

        Assert.Equal(
            CoreStrings.ModelReadOnly,
            Assert.Throws<InvalidOperationException>(() => index.IsUnique = false).Message);
    }

    [Fact]
    public void Gets_expected_default_values()
    {
        var entityType = ((IConventionModel)CreateModel()).AddEntityType(typeof(Customer));
        var property1 = entityType.AddProperty(Customer.IdProperty);
        var property2 = entityType.AddProperty(Customer.NameProperty);

        var index = entityType.AddIndex([property1, property2]);

        Assert.True(new[] { property1, property2 }.SequenceEqual(index.Properties));
        Assert.False(index.IsUnique);
        Assert.Equal(ConfigurationSource.Convention, index.GetConfigurationSource());
    }

    [Fact]
    public void Can_set_unique()
    {
        var entityType = CreateModel().AddEntityType(typeof(Customer));
        var property1 = entityType.AddProperty(Customer.IdProperty);
        var property2 = entityType.AddProperty(Customer.NameProperty);

        var index = entityType.AddIndex([property1, property2]);
        index.IsUnique = true;

        Assert.True(new[] { property1, property2 }.SequenceEqual(index.Properties));
        Assert.True(index.IsUnique);
    }

    [Fact]
    public void IsDescending_all_ascending_is_normalized_to_null()
    {
        var entityType = CreateModel().AddEntityType(typeof(Customer));
        var property1 = entityType.AddProperty(Customer.IdProperty);
        var property2 = entityType.AddProperty(Customer.NameProperty);

        var index = entityType.AddIndex([property1, property2]);
        index.IsDescending = [false, false];

        Assert.True(new[] { property1, property2 }.SequenceEqual(index.Properties));
        Assert.Null(index.IsDescending);
    }

    [Fact]
    public void IsDescending_all_descending_is_normalized_to_empty()
    {
        var entityType = CreateModel().AddEntityType(typeof(Customer));
        var property1 = entityType.AddProperty(Customer.IdProperty);
        var property2 = entityType.AddProperty(Customer.NameProperty);

        var index = entityType.AddIndex([property1, property2]);
        index.IsDescending = [true, true];

        Assert.True(new[] { property1, property2 }.SequenceEqual(index.Properties));
        Assert.Equal([], index.IsDescending);
    }

    [Fact]
    public void IsDescending_invalid_number_of_columns_throws()
    {
        var entityType = CreateModel().AddEntityType(typeof(Customer));
        var property1 = entityType.AddProperty(Customer.IdProperty);
        var property2 = entityType.AddProperty(Customer.NameProperty);

        var index = entityType.AddIndex([property1, property2]);
        var exception = Assert.Throws<ArgumentException>(() => index.IsDescending = [true]);
        Assert.Equal(
            CoreStrings.InvalidNumberOfIndexSortOrderValues("{'Id', 'Name'}", 1, 2) + " (Parameter 'descending')",
            exception.Message);
    }

    private static IMutableModel CreateModel()
        => new Model();

    private class Customer
    {
        public static readonly PropertyInfo IdProperty = typeof(Customer).GetProperty("Id");
        public static readonly PropertyInfo NameProperty = typeof(Customer).GetProperty("Name");

        public int Id { get; set; }
        public string Name { get; set; }
    }

    private class Order
    {
        public static readonly PropertyInfo IdProperty = typeof(Order).GetProperty("Id");

        public int Id { get; set; }
    }

    private sealed class Blog
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public List<Post> Posts { get; set; } = [];
        public Address Owner { get; set; }
    }

    private sealed class Post
    {
        public string Title { get; set; }
        public int Rating { get; set; }
        public List<Comment> Comments { get; set; } = [];
    }

    private sealed class Comment
    {
        public string Text { get; set; }
    }

    private sealed class Address
    {
        public string City { get; set; }
        public string Country { get; set; }
    }

    private static ModelBuilder CreateComplexModelBuilder()
    {
        var modelBuilder = new ModelBuilder();
        modelBuilder.Entity<Blog>(b =>
        {
            b.Property(e => e.Title);

            b.ComplexProperty(e => e.Owner, cb =>
            {
                cb.Property(a => a.City);
                cb.Property(a => a.Country);
            });

            b.ComplexCollection(e => e.Posts, cb =>
            {
                cb.Property(p => p.Title);
                cb.Property(p => p.Rating);
                cb.ComplexCollection(p => p.Comments, ccb => ccb.Property(c => c.Text));
            });
        });

        return modelBuilder;
    }

    [Theory]
    [InlineData("Posts[")]                  // unterminated bracket
    [InlineData("Posts[abc].Title")]        // non-numeric index
    [InlineData("Posts[-1].Title")]         // negative index
    [InlineData("Posts[].")]                // empty trailing segment
    [InlineData(".Title")]                  // empty leading segment
    [InlineData("[0].Title")]               // bracket at start of segment
    [InlineData("")]                        // empty path
    public void MatchComplexPath_rejects_invalid_path(string path)
    {
        Assert.Null(InternalTypeBaseBuilder.MatchComplexPath(path));
    }

    [Theory]
    [InlineData("Posts[].Title")]
    [InlineData("Posts[*].Title")]
    public void MatchComplexPath_accepts_all_elements_syntaxes(string path)
    {
        var parsed = InternalTypeBaseBuilder.MatchComplexPath(path);
        Assert.NotNull(parsed);
        Assert.Equal(["Posts", "Title"], parsed.Value.MemberNames);
        Assert.Equal([true, false], parsed.Value.IsCollection);
        Assert.Equal([null], parsed.Value.CollectionIndices);
    }

    [Fact]
    public void MatchComplexPath_preserves_collection_flag_on_leaf()
    {
        var parsed = InternalTypeBaseBuilder.MatchComplexPath("Posts[]");
        Assert.NotNull(parsed);
        Assert.Equal(["Posts"], parsed.Value.MemberNames);
        Assert.Equal([true], parsed.Value.IsCollection);
        Assert.Equal([null], parsed.Value.CollectionIndices);
    }

    [Fact]
    public void MatchComplexPath_preserves_indexer_on_leaf()
    {
        var parsed = InternalTypeBaseBuilder.MatchComplexPath("Posts[3]");
        Assert.NotNull(parsed);
        Assert.Equal(["Posts"], parsed.Value.MemberNames);
        Assert.Equal([true], parsed.Value.IsCollection);
        Assert.Equal([3], parsed.Value.CollectionIndices);
    }

    [Fact]
    public void MatchComplexPath_single_scalar_member_emits_single_flag()
    {
        var parsed = InternalTypeBaseBuilder.MatchComplexPath("Title");
        Assert.NotNull(parsed);
        Assert.Equal(["Title"], parsed.Value.MemberNames);
        Assert.Equal([false], parsed.Value.IsCollection);
        Assert.Null(parsed.Value.CollectionIndices);
    }

    [Fact]
    public void FindIndex_with_collection_indices_returns_matching_json_path_index()
    {
        // (Properties, CollectionIndices) form the full identity of an unnamed JSON-path index.
        // FindIndex(properties, CI) must locate it; the no-CI overload should not, because that overload
        // looks up the "plain" (CI=null) index identity.
        var modelBuilder = CreateComplexModelBuilder();
        var entityType = (EntityType)modelBuilder.Entity<Blog>().Metadata;
        var titleProp = (PropertyBase)entityType.FindComplexProperty("Posts")!.ComplexType.FindProperty("Title")!;
        entityType.AddIndex(
            [titleProp],
            [[0]],
            ConfigurationSource.Explicit);

        var found = entityType.FindIndex([titleProp], [[0]]);
        Assert.NotNull(found);
        Assert.Equal([0], Assert.Single(found.CollectionIndices!));

        Assert.Null(entityType.FindIndex([titleProp], [[null]]));
        Assert.Null(entityType.FindIndex([titleProp]));
    }

    [Fact]
    public void FindIndex_without_collection_indices_does_not_return_json_path_index()
    {
        // JSON-path indexes (those with non-null CollectionIndices) are addressable only via the
        // (properties, CI) overload of FindIndex. The plain (properties) overload must not return
        // them.
        var modelBuilder = CreateComplexModelBuilder();
        var entityType = (EntityType)modelBuilder.Entity<Blog>().Metadata;
        var titleProp = (PropertyBase)entityType.FindComplexProperty("Posts")!.ComplexType.FindProperty("Title")!;
        entityType.AddIndex(
            [titleProp],
            [[0]],
            ConfigurationSource.Explicit);

        Assert.Null(entityType.FindIndex([titleProp]));

        var foundJson = entityType.FindIndex([titleProp], [[0]]);
        Assert.NotNull(foundJson);
        Assert.Equal([0], Assert.Single(foundJson.CollectionIndices!));
    }

    [Fact]
    public void AddIndex_unnamed_with_different_collection_indices_does_not_throw_duplicate()
    {
        // Adding two unnamed indexes with the same Properties but different CollectionIndices via the
        // internal AddIndex API succeeds because their UnnamedIndexKey identities differ.
        var modelBuilder = CreateComplexModelBuilder();
        var entityType = (EntityType)modelBuilder.Entity<Blog>().Metadata;
        var titleProp = (PropertyBase)entityType.FindComplexProperty("Posts")!.ComplexType.FindProperty("Title")!;

        entityType.AddIndex(
            [titleProp],
            [[0]],
            ConfigurationSource.Explicit);

        entityType.AddIndex(
            [titleProp],
            [[1]],
            ConfigurationSource.Explicit);

        Assert.Equal(2, entityType.GetIndexes().Count());
    }

    [Fact]
    public void NormalizeCollectionIndices_throws_when_entry_length_exceeds_collection_count()
    {
        var modelBuilder = CreateComplexModelBuilder();
        var entityType = (EntityType)modelBuilder.Model.FindEntityType(typeof(Blog))!;

        // Posts is a single complex collection, so the entry for a leaf inside it should have exactly 1 element.
        // Providing 2 elements should throw.
        var titleProp = (PropertyBase)entityType.FindComplexProperty("Posts")!.ComplexType.FindProperty("Title")!;
        var tooManyIndices = new IReadOnlyList<int?>[] { [null, null] };

        var ex = Assert.Throws<ArgumentException>(
            () => new Index(
                [titleProp], tooManyIndices, entityType, ConfigurationSource.Explicit));

        Assert.Contains(CoreStrings.InvalidCollectionIndicesEntryLength("Title", "{'" + titleProp.Name + "'}", 2, 1), ex.Message);
    }

    [Fact]
    public void NormalizeCollectionIndices_throws_when_entry_length_is_zero_for_collection_property()
    {
        var modelBuilder = CreateComplexModelBuilder();
        var entityType = (EntityType)modelBuilder.Model.FindEntityType(typeof(Blog))!;

        // Posts is a single complex collection, so providing an empty entry (0 elements) should throw.
        var titleProp = (PropertyBase)entityType.FindComplexProperty("Posts")!.ComplexType.FindProperty("Title")!;
        var emptyIndices = new IReadOnlyList<int?>[] { Array.Empty<int?>() };

        var ex = Assert.Throws<ArgumentException>(
            () => new Index(
                [titleProp], emptyIndices, entityType, ConfigurationSource.Explicit));

        Assert.Contains(CoreStrings.InvalidCollectionIndicesEntryLength("Title", "{'" + titleProp.Name + "'}", 0, 1), ex.Message);
    }

    [Fact]
    public void NormalizeCollectionIndices_throws_when_non_null_entry_for_non_collection_property()
    {
        var modelBuilder = CreateComplexModelBuilder();
        var entityType = (EntityType)modelBuilder.Model.FindEntityType(typeof(Blog))!;

        // Owner.City is NOT inside a complex collection, so the entry should be null (0 collection segments).
        // Providing a non-null entry with 1 element should throw.
        var cityProp = (PropertyBase)entityType.FindComplexProperty("Owner")!.ComplexType.FindProperty("City")!;
        var wrongIndices = new IReadOnlyList<int?>[] { [null] };

        var ex = Assert.Throws<ArgumentException>(
            () => new Index(
                [cityProp], wrongIndices, entityType, ConfigurationSource.Explicit));

        Assert.Contains(CoreStrings.InvalidCollectionIndicesEntryLength("City", "{'" + cityProp.Name + "'}", 1, 0), ex.Message);
    }

    [Fact]
    public void NormalizeCollectionIndices_accepts_correct_entry_length_for_collection_property()
    {
        var modelBuilder = CreateComplexModelBuilder();
        var entityType = (EntityType)modelBuilder.Model.FindEntityType(typeof(Blog))!;

        var titleProp = (PropertyBase)entityType.FindComplexProperty("Posts")!.ComplexType.FindProperty("Title")!;
        var correctIndices = new IReadOnlyList<int?>[] { [null] };

        var index = new Index(
            [titleProp], correctIndices, entityType, ConfigurationSource.Explicit);

        Assert.NotNull(index.CollectionIndices);
        Assert.Equal([null], Assert.Single(index.CollectionIndices));
    }

    [Fact]
    public void NormalizeCollectionIndices_counts_leaf_complex_collection_property()
    {
        // When the indexed leaf itself is a complex collection (string path "Posts[]" / "Posts[3]"),
        // CollectionIndices must carry a single entry for that leaf.
        var modelBuilder = CreateComplexModelBuilder();
        var entityType = (EntityType)modelBuilder.Model.FindEntityType(typeof(Blog))!;
        var postsProp = (PropertyBase)entityType.FindComplexProperty("Posts")!;

        var index = new Index(
            [postsProp], [[3]], entityType, ConfigurationSource.Explicit);

        Assert.NotNull(index.CollectionIndices);
        Assert.Equal([3], Assert.Single(index.CollectionIndices));
    }

    [Fact]
    public void NormalizeCollectionIndices_throws_when_entry_length_mismatch_for_leaf_complex_collection_property()
    {
        var modelBuilder = CreateComplexModelBuilder();
        var entityType = (EntityType)modelBuilder.Model.FindEntityType(typeof(Blog))!;
        var postsProp = (PropertyBase)entityType.FindComplexProperty("Posts")!;
        var tooManyIndices = new IReadOnlyList<int?>[] { [null, null] };

        var ex = Assert.Throws<ArgumentException>(
            () => new Index(
                [postsProp], tooManyIndices, entityType, ConfigurationSource.Explicit));

        Assert.Contains(CoreStrings.InvalidCollectionIndicesEntryLength("Posts", "{'" + postsProp.Name + "'}", 2, 1), ex.Message);
    }

    [Fact]
    public void GetOrCreateProperties_returns_existing_complex_property_as_leaf_for_string_path()
    {
        var modelBuilder = CreateComplexModelBuilder();
        var entityTypeBuilder = ((EntityType)modelBuilder.Entity<Blog>().Metadata).Builder;

        var properties = entityTypeBuilder.GetOrCreateProperties(
            [["Owner"]],
            collection: null,
            ConfigurationSource.Explicit);

        Assert.NotNull(properties);
        var leaf = Assert.Single(properties);
        Assert.IsType<ComplexProperty>(leaf);
        Assert.Equal("Owner", leaf.Name);
    }

    [Fact]
    public void GetOrCreateProperties_returns_existing_complex_collection_as_leaf_for_string_path()
    {
        var modelBuilder = CreateComplexModelBuilder();
        var entityTypeBuilder = ((EntityType)modelBuilder.Entity<Blog>().Metadata).Builder;

        var properties = entityTypeBuilder.GetOrCreateProperties(
            [["Posts"]],
            collection: null,
            ConfigurationSource.Explicit);

        Assert.NotNull(properties);
        var leaf = Assert.Single(properties);
        var leafComplex = Assert.IsType<ComplexProperty>(leaf);
        Assert.True(leafComplex.IsCollection);
        Assert.Equal("Posts", leaf.Name);
    }

    [Fact]
    public void GetOrCreateProperties_returns_existing_complex_property_as_leaf_for_member_chain()
    {
        var modelBuilder = CreateComplexModelBuilder();
        var entityTypeBuilder = ((EntityType)modelBuilder.Entity<Blog>().Metadata).Builder;
        var ownerMember = typeof(Blog).GetProperty(nameof(Blog.Owner))!;

        var properties = entityTypeBuilder.GetOrCreateProperties(
            [[ownerMember]],
            collection: null,
            ConfigurationSource.Explicit);

        Assert.NotNull(properties);
        var leaf = Assert.Single(properties);
        Assert.IsType<ComplexProperty>(leaf);
        Assert.Equal("Owner", leaf.Name);
    }

    [Fact]
    public void GetOrCreateProperties_returns_existing_complex_collection_as_leaf_for_member_chain()
    {
        var modelBuilder = CreateComplexModelBuilder();
        var entityTypeBuilder = ((EntityType)modelBuilder.Entity<Blog>().Metadata).Builder;
        var postsMember = typeof(Blog).GetProperty(nameof(Blog.Posts))!;

        var properties = entityTypeBuilder.GetOrCreateProperties(
            [[postsMember]],
            collection: null,
            ConfigurationSource.Explicit);

        Assert.NotNull(properties);
        var leaf = Assert.Single(properties);
        var leafComplex = Assert.IsType<ComplexProperty>(leaf);
        Assert.True(leafComplex.IsCollection);
        Assert.Equal("Posts", leaf.Name);
    }

    [Fact]
    public void MatchComplexPath_parses_nested_complex_collections()
    {
        var parsed = InternalTypeBaseBuilder.MatchComplexPath("Posts[0].Comments[1].Text");
        Assert.NotNull(parsed);
        Assert.Equal(["Posts", "Comments", "Text"], parsed.Value.MemberNames);
        Assert.Equal([true, true, false], parsed.Value.IsCollection);
        Assert.Equal([0, 1], parsed.Value.CollectionIndices);
    }

    [Fact]
    public void MatchComplexPath_parses_nested_complex_collections_all_elements()
    {
        var parsed = InternalTypeBaseBuilder.MatchComplexPath("Posts[].Comments[].Text");
        Assert.NotNull(parsed);
        Assert.Equal(["Posts", "Comments", "Text"], parsed.Value.MemberNames);
        Assert.Equal([true, true, false], parsed.Value.IsCollection);
        Assert.Equal([null, null], parsed.Value.CollectionIndices);
    }

    [Fact]
    public void MatchComplexPath_parses_mixed_indexer_and_wildcard()
    {
        var parsed = InternalTypeBaseBuilder.MatchComplexPath("Posts[2].Comments[*].Text");
        Assert.NotNull(parsed);
        Assert.Equal([2, null], parsed.Value.CollectionIndices);
    }

    [Fact]
    public void NormalizeCollectionIndices_counts_nested_complex_collections_in_path()
    {
        var modelBuilder = CreateComplexModelBuilder();
        var entityType = (EntityType)modelBuilder.Model.FindEntityType(typeof(Blog))!;
        var textProp = (PropertyBase)entityType.FindComplexProperty("Posts")!.ComplexType
            .FindComplexProperty("Comments")!.ComplexType.FindProperty("Text")!;

        // Two collection traversals (Posts, Comments) precede Text — entry must have 2 elements.
        var index = new Index(
            [textProp], [[0, 1]], entityType, ConfigurationSource.Explicit);

        Assert.NotNull(index.CollectionIndices);
        Assert.Equal([0, 1], Assert.Single(index.CollectionIndices));
    }

    [Fact]
    public void NormalizeCollectionIndices_throws_for_nested_collections_when_entry_length_is_wrong()
    {
        var modelBuilder = CreateComplexModelBuilder();
        var entityType = (EntityType)modelBuilder.Model.FindEntityType(typeof(Blog))!;
        var textProp = (PropertyBase)entityType.FindComplexProperty("Posts")!.ComplexType
            .FindComplexProperty("Comments")!.ComplexType.FindProperty("Text")!;

        var ex = Assert.Throws<ArgumentException>(
            () => new Index([textProp], [[0]], entityType, ConfigurationSource.Explicit));

        Assert.Contains(
            CoreStrings.InvalidCollectionIndicesEntryLength("Text", "{'" + textProp.Name + "'}", 1, 2),
            ex.Message);
    }

    [Fact]
    public void Detach_and_reattach_preserves_index_with_collection_indices()
    {
        // Reattachment goes through InternalIndexBuilder.RequiresComplexReattach when the index targets
        // properties inside a complex chain or carries collection indices. Verify the round-trip preserves
        // Properties, CollectionIndices, Name, and IsUnique.
        var modelBuilder = CreateComplexModelBuilder();
        var entityType = (EntityType)modelBuilder.Model.FindEntityType(typeof(Blog))!;
        var titleProp = (PropertyBase)entityType.FindComplexProperty("Posts")!.ComplexType.FindProperty("Title")!;

        var original = entityType.AddIndex([titleProp], [[5]], "IX_Reattach", ConfigurationSource.Explicit);
        original.SetIsUnique(true, ConfigurationSource.Explicit);

        var detached = InternalEntityTypeBuilder.DetachIndex(original);
        Assert.Null(entityType.FindIndex("IX_Reattach"));

        var reattached = detached.Attach(entityType.Builder);
        Assert.NotNull(reattached);

        var newIndex = entityType.FindIndex("IX_Reattach")!;
        Assert.Same(reattached.Metadata, newIndex);
        Assert.Equal("IX_Reattach", newIndex.Name);
        Assert.True(newIndex.IsUnique);
        Assert.Same(titleProp, Assert.Single(newIndex.Properties));
        Assert.NotNull(newIndex.CollectionIndices);
        Assert.Equal([5], Assert.Single(newIndex.CollectionIndices!));
    }

    [Fact]
    public void Detach_and_reattach_preserves_unnamed_index_with_collection_indices()
    {
        var modelBuilder = CreateComplexModelBuilder();
        var entityType = (EntityType)modelBuilder.Model.FindEntityType(typeof(Blog))!;
        var titleProp = (PropertyBase)entityType.FindComplexProperty("Posts")!.ComplexType.FindProperty("Title")!;

        var original = entityType.AddIndex([titleProp], [[null]], ConfigurationSource.Explicit);

        var detached = InternalEntityTypeBuilder.DetachIndex(original);
        Assert.Null(entityType.FindIndex([titleProp], [[null]]));

        var reattached = detached.Attach(entityType.Builder);
        Assert.NotNull(reattached);

        var newIndex = entityType.FindIndex([titleProp], [[null]])!;
        Assert.Same(reattached.Metadata, newIndex);
        Assert.Null(newIndex.Name);
        Assert.NotNull(newIndex.CollectionIndices);
        Assert.Equal(new int?[] { null }, Assert.Single(newIndex.CollectionIndices!));
    }

    [Fact]
    public void Detach_and_reattach_preserves_index_through_nested_complex_collections()
    {
        var modelBuilder = CreateComplexModelBuilder();
        var entityType = (EntityType)modelBuilder.Model.FindEntityType(typeof(Blog))!;
        var textProp = (PropertyBase)entityType.FindComplexProperty("Posts")!.ComplexType
            .FindComplexProperty("Comments")!.ComplexType.FindProperty("Text")!;

        var original = entityType.AddIndex([textProp], [[0, 1]], "IX_NestedReattach", ConfigurationSource.Explicit);

        var detached = InternalEntityTypeBuilder.DetachIndex(original);
        var reattached = detached.Attach(entityType.Builder);
        Assert.NotNull(reattached);

        var newIndex = entityType.FindIndex("IX_NestedReattach")!;
        Assert.Same(reattached.Metadata, newIndex);
        Assert.Same(textProp, Assert.Single(newIndex.Properties));
        Assert.NotNull(newIndex.CollectionIndices);
        Assert.Equal([0, 1], Assert.Single(newIndex.CollectionIndices!));
    }

    [Fact]
    public void Detach_and_reattach_preserves_index_on_leaf_complex_collection_property()
    {
        // RequiresComplexReattach writes chainFlags[depth] = false even when the leaf is itself a
        // complex collection (because the leaf is the indexed property, not a traversal step). The
        // leaf still resolves correctly through FindMember on the rebuilt entity type, and the
        // single CollectionIndices entry for the leaf must round-trip.
        var modelBuilder = CreateComplexModelBuilder();
        var entityType = (EntityType)modelBuilder.Model.FindEntityType(typeof(Blog))!;
        var postsProp = (PropertyBase)entityType.FindComplexProperty("Posts")!;

        var original = entityType.AddIndex([postsProp], [[7]], "IX_LeafCollectionReattach", ConfigurationSource.Explicit);

        var detached = InternalEntityTypeBuilder.DetachIndex(original);
        var reattached = detached.Attach(entityType.Builder);
        Assert.NotNull(reattached);

        var newIndex = entityType.FindIndex("IX_LeafCollectionReattach")!;
        Assert.Same(reattached.Metadata, newIndex);
        var leaf = Assert.Single(newIndex.Properties);
        var leafComplex = Assert.IsType<ComplexProperty>(leaf);
        Assert.True(leafComplex.IsCollection);
        Assert.Equal("Posts", leaf.Name);
        Assert.NotNull(newIndex.CollectionIndices);
        Assert.Equal([7], Assert.Single(newIndex.CollectionIndices!));
    }

    [Fact]
    public void Detach_and_reattach_preserves_unnamed_index_on_leaf_complex_collection_property()
    {
        var modelBuilder = CreateComplexModelBuilder();
        var entityType = (EntityType)modelBuilder.Model.FindEntityType(typeof(Blog))!;
        var postsProp = (PropertyBase)entityType.FindComplexProperty("Posts")!;

        var original = entityType.AddIndex([postsProp], [[null]], ConfigurationSource.Explicit);

        var detached = InternalEntityTypeBuilder.DetachIndex(original);
        var reattached = detached.Attach(entityType.Builder);
        Assert.NotNull(reattached);

        var newIndex = entityType.FindIndex([postsProp], [[null]])!;
        Assert.Same(reattached.Metadata, newIndex);
        Assert.Null(newIndex.Name);
        Assert.Same(postsProp, Assert.Single(newIndex.Properties));
        Assert.NotNull(newIndex.CollectionIndices);
        Assert.Equal(new int?[] { null }, Assert.Single(newIndex.CollectionIndices!));
    }

    [Fact]
    public void HasIndex_synthesizes_trailing_wildcard_when_leaf_is_complex_collection()
    {
        // "Posts" addresses a complex collection leaf — the trailing `[]` is optional.
        // The builder should produce a single wildcard collection-indices entry equivalent to `Posts[]`.
        var modelBuilder = CreateComplexModelBuilder();
        var entityTypeBuilder = ((EntityType)modelBuilder.Entity<Blog>().Metadata).Builder;

        var indexBuilder = entityTypeBuilder.HasIndex(["Posts"], ConfigurationSource.Explicit);

        Assert.NotNull(indexBuilder);
        var index = indexBuilder.Metadata;
        Assert.Equal("Posts", Assert.Single(index.Properties).Name);
        Assert.NotNull(index.CollectionIndices);
        Assert.Equal(new int?[] { null }, Assert.Single(index.CollectionIndices!));
    }

    [Fact]
    public void HasIndex_appends_trailing_wildcard_when_parent_collection_is_indexed_and_leaf_is_omitted()
    {
        // "Posts[0].Comments" indexes the entire Comments collection of Posts[0].
        // The parser produces one indexer entry ([0]) for the parent Posts collection; the leaf
        // Comments collection has no `[]` suffix, so HasIndex must append a wildcard to make the
        // collection-indices entry length match the path's collection depth (2).
        var modelBuilder = CreateComplexModelBuilder();
        var entityTypeBuilder = ((EntityType)modelBuilder.Entity<Blog>().Metadata).Builder;

        var indexBuilder = entityTypeBuilder.HasIndex(["Posts[0].Comments"], ConfigurationSource.Explicit);

        Assert.NotNull(indexBuilder);
        var index = indexBuilder.Metadata;
        var leaf = Assert.Single(index.Properties);
        var leafComplex = Assert.IsType<ComplexProperty>(leaf);
        Assert.True(leafComplex.IsCollection);
        Assert.Equal("Comments", leaf.Name);
        Assert.NotNull(index.CollectionIndices);
        Assert.Equal(new int?[] { 0, null }, Assert.Single(index.CollectionIndices!));
    }

    [Fact]
    public void HasIndex_does_not_modify_collection_indices_when_leaf_indexer_is_explicit()
    {
        // When the caller explicitly provided the leaf indexer (`Posts[3]`), HasIndex must not append
        // a wildcard — the parsed entry already has the correct length.
        var modelBuilder = CreateComplexModelBuilder();
        var entityTypeBuilder = ((EntityType)modelBuilder.Entity<Blog>().Metadata).Builder;

        var indexBuilder = entityTypeBuilder.HasIndex(["Posts[3]"], ConfigurationSource.Explicit);

        Assert.NotNull(indexBuilder);
        var index = indexBuilder.Metadata;
        Assert.NotNull(index.CollectionIndices);
        Assert.Equal(new int?[] { 3 }, Assert.Single(index.CollectionIndices!));
    }

    private class InheritedBase
    {
        public int Id { get; set; }
    }

    private sealed class InheritedDerived : InheritedBase
    {
        public string OnlyOnDerived { get; set; } = null!;
    }

    [Fact]
    public void FindMember_used_by_GetOrCreateProperties_does_not_match_derived_entity_type_members()
    {
        // Index resolution must only see members reachable from `this` (this type + base types), not
        // members declared on derived types. Verify the lookup primitives behave that way: FindMember
        // returns null while FindMembersInHierarchy still finds the derived-type member.
        var modelBuilder = new ModelBuilder();
        modelBuilder.Entity<InheritedBase>();
        modelBuilder.Entity<InheritedDerived>(
            b =>
            {
                b.HasBaseType<InheritedBase>();
                b.Property(d => d.OnlyOnDerived);
            });

        var baseType = (EntityType)modelBuilder.Model.FindEntityType(typeof(InheritedBase))!;

        Assert.Null(baseType.FindMember(nameof(InheritedDerived.OnlyOnDerived)));
        Assert.NotNull(baseType.FindMembersInHierarchy(nameof(InheritedDerived.OnlyOnDerived)).FirstOrDefault());
    }
}
