// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Cosmos.ValueGeneration;

public class IdValueGeneratorTest
{
    [Fact]
    public void Generated_ids_do_not_clash()
    {
        var modelBuilder = CosmosTestHelpers.Instance.CreateConventionBuilder();

        modelBuilder.HasDiscriminatorInJsonIds();

        modelBuilder.Entity<Blog>().HasKey(p => new { p.OtherId, p.Id });
        modelBuilder.Entity<Post>().HasKey(p => new { p.OtherId, p.Id });

        modelBuilder.Entity<IntClassEntity>().Property(e => e.Id).HasConversion(IntClass.Converter);
        modelBuilder.Entity<IntStructEntity>().Property(e => e.Id).HasConversion(IntStruct.Converter);
        modelBuilder.Entity<BytesStructEntity>().Property(e => e.Id).HasConversion(BytesStruct.Converter);

        var model = modelBuilder.FinalizeModel();

        var ids = new List<string>
        {
            Create(new Blog { Id = 1, OtherId = 1 }),
            Create(new Post { Id = "1", OtherId = "1" }),
            Create(new Post { Id = "1", OtherId = "1|" }),
            Create(new Post { Id = "|1", OtherId = "1" }),
            Create(new IntClassEntity { Id = new IntClass(1) }),
            Create(new IntClassEntity { Id = new IntClass(2) }),
            Create(new IntStructEntity { Id = new IntStruct(1) }),
            Create(new IntStructEntity { Id = new IntStruct(2) }),
            Create(new BytesStructEntity { Id = new BytesStruct([]) }),
            Create(new BytesStructEntity { Id = new BytesStruct([1]) }),
            Create(new BytesStructEntity { Id = new BytesStruct([2, 2]) }),
        };

        Assert.Equal(ids.Count, new HashSet<string>(ids.Concat(ids)).Count);

        string Create<TEntity>(TEntity entity)
            where TEntity : class, new()
            => (string)CosmosTestHelpers.Instance.CreateInternalEntry(model, EntityState.Added, entity)
                [model.FindEntityType(typeof(TEntity)).FindProperty(CosmosJsonIdConvention.DefaultIdPropertyName)];
    }

    [Fact]
    public void Illegal_id_characters_are_not_escaped_by_default()
    {
        var modelBuilder = CosmosTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Entity<Post>().HasKey(p => new { p.OtherId, p.Id });
        var model = modelBuilder.FinalizeModel();

        foreach (var c in new[] { "/", "\\", "?", "#" })
        {
            var id = (string)CosmosTestHelpers.Instance.CreateInternalEntry(
                    model, EntityState.Added, new Post { Id = c, OtherId = "1" })
                [model.FindEntityType(typeof(Post)).FindProperty(CosmosJsonIdConvention.DefaultIdPropertyName)];

            Assert.Contains(c, id);
            Assert.DoesNotContain("^", id);
        }
    }

    [Fact]
    public void Ids_with_former_escape_sequences_do_not_collide()
    {
        var modelBuilder = CosmosTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Entity<Post>().HasKey(p => new { p.OtherId, p.Id });
        var model = modelBuilder.FinalizeModel();

        var pairs = new[] { ("/", "^2F"), ("\\", "^5C"), ("?", "^3F"), ("#", "^23") };
        foreach (var (raw, escaped) in pairs)
        {
            var id1 = Create(raw);
            var id2 = Create(escaped);

            Assert.NotEqual(id1, id2);
        }

        string Create(string value)
            => (string)CosmosTestHelpers.Instance.CreateInternalEntry(
                    model, EntityState.Added, new Post { Id = value, OtherId = "1" })
                [model.FindEntityType(typeof(Post)).FindProperty(CosmosJsonIdConvention.DefaultIdPropertyName)];
    }

    [Fact]
    public void Generated_ids_for_complex_type_key_do_not_clash()
    {
        var modelBuilder = CosmosTestHelpers.Instance.CreateConventionBuilder();

        modelBuilder.Entity<ComplexKeyEntity>(b =>
        {
            b.ComplexProperty(e => e.Id);
            b.HasKey(e => e.Id.Value);
            b.Property(e => e.Id.Value).ValueGeneratedNever();
        });

        var model = modelBuilder.FinalizeModel();

        var id1 = Create(new ComplexKeyEntity { Id = new ComplexKey { Value = 1 } });
        var id2 = Create(new ComplexKeyEntity { Id = new ComplexKey { Value = 2 } });

        Assert.NotEqual(id1, id2);

        string Create(ComplexKeyEntity entity)
            => (string)CosmosTestHelpers.Instance.CreateInternalEntry(model, EntityState.Added, entity)
                [model.FindEntityType(typeof(ComplexKeyEntity)).FindProperty(CosmosJsonIdConvention.DefaultIdPropertyName)];
    }

    [Fact]
    public void Generated_ids_for_complex_type_key_with_discriminator_do_not_clash()
    {
        var modelBuilder = CosmosTestHelpers.Instance.CreateConventionBuilder();

        modelBuilder.Entity<ComplexKeyEntity>(b =>
        {
            b.ComplexProperty(e => e.Id);
            b.HasKey(e => e.Id.Value);
            b.Property(e => e.Id.Value).ValueGeneratedNever();
            b.HasDiscriminatorInJsonId();
        });

        var model = modelBuilder.FinalizeModel();

        var id1 = Create(new ComplexKeyEntity { Id = new ComplexKey { Value = 1 } });
        var id2 = Create(new ComplexKeyEntity { Id = new ComplexKey { Value = 2 } });

        Assert.NotEqual(id1, id2);

        string Create(ComplexKeyEntity entity)
            => (string)CosmosTestHelpers.Instance.CreateInternalEntry(model, EntityState.Added, entity)
                [model.FindEntityType(typeof(ComplexKeyEntity)).FindProperty(CosmosJsonIdConvention.DefaultIdPropertyName)];
    }

    [Fact]
    public void Generated_ids_for_record_struct_complex_type_key_with_discriminator_do_not_clash()
    {
        var modelBuilder = CosmosTestHelpers.Instance.CreateConventionBuilder();

        modelBuilder.Entity<Customer>(b =>
        {
            b.ComplexProperty(e => e.Id);
            b.HasKey(e => e.Id.Value);
            b.Property(e => e.Id.Value).ValueGeneratedNever();
            b.HasDiscriminatorInJsonId();
        });

        var model = modelBuilder.FinalizeModel();

        var id1 = Create(new Customer { Id = new CustomerId(1), Name = "Alice" });
        var id2 = Create(new Customer { Id = new CustomerId(2), Name = "Bob" });

        Assert.NotEqual(id1, id2);

        string Create(Customer entity)
            => (string)CosmosTestHelpers.Instance.CreateInternalEntry(model, EntityState.Added, entity)
                [model.FindEntityType(typeof(Customer)).FindProperty(CosmosJsonIdConvention.DefaultIdPropertyName)];
    }

    [Fact]
    public void Generated_ids_for_nested_complex_type_key_with_discriminator_do_not_clash()
    {
        var modelBuilder = CosmosTestHelpers.Instance.CreateConventionBuilder();

        modelBuilder.Entity<Order>(b =>
        {
            b.ComplexProperty(e => e.Key, kb => kb.ComplexProperty(k => k.Inner));
            b.HasKey(e => e.Key.Inner.Value);
            b.Property(e => e.Key.Inner.Value).ValueGeneratedNever();
            b.HasDiscriminatorInJsonId();
        });

        var model = modelBuilder.FinalizeModel();

        var id1 = Create(new Order { Key = new OrderKey { Inner = new InnerKey { Value = 1 } }, Description = "First" });
        var id2 = Create(new Order { Key = new OrderKey { Inner = new InnerKey { Value = 2 } }, Description = "Second" });

        Assert.NotEqual(id1, id2);

        string Create(Order entity)
            => (string)CosmosTestHelpers.Instance.CreateInternalEntry(model, EntityState.Added, entity)
                [model.FindEntityType(typeof(Order)).FindProperty(CosmosJsonIdConvention.DefaultIdPropertyName)];
    }

    private class ComplexKeyEntity
    {
        public ComplexKey Id { get; set; }
    }

    private struct ComplexKey
    {
        public int Value { get; set; }
    }

    private readonly record struct CustomerId(int Value);

    private class Customer
    {
        public CustomerId Id { get; set; }
        public string Name { get; set; }
    }

    private class Order
    {
        public OrderKey Key { get; set; }
        public string Description { get; set; }
    }

    private class OrderKey
    {
        public InnerKey Inner { get; set; }
    }

    private class InnerKey
    {
        public int Value { get; set; }
    }

    private class Blog
    {
        public int Id { get; set; }
        public int OtherId { get; set; }
    }

    private class Post
    {
        public string Id { get; set; }
        public string OtherId { get; set; }
    }

    private class IntClassEntity
    {
        public IntClass Id { get; set; }
    }

    private class IntClass(int value)
    {
        public static readonly ValueConverter<IntClass, int> Converter
            = new(v => v.Value, v => new IntClass(v));

        private bool Equals(IntClass other)
            => other != null && Value == other.Value;

        public override bool Equals(object obj)
            => obj == this
                || obj?.GetType() == GetType()
                && Equals((IntClass)obj);

        public override int GetHashCode()
            => Value;

        public int Value { get; } = value;
    }

    private class IntStructEntity
    {
        public IntStruct Id { get; set; }
    }

    private struct IntStruct(int value)
    {
        public static readonly ValueConverter<IntStruct, int> Converter
            = new(v => v.Value, v => new IntStruct(v));

        public int Value { get; } = value;
    }

    private class BytesStructEntity
    {
        public BytesStruct Id { get; set; }
    }

    private struct BytesStruct(byte[] value)
    {
        public static readonly ValueConverter<BytesStruct, byte[]> Converter
            = new(v => v.Value, v => new BytesStruct(v));

        public byte[] Value { get; } = value;

        public bool Equals(BytesStruct other)
            => Value == null
                && other.Value == null
                || other.Value != null
                && Value?.SequenceEqual(other.Value) == true;

        public override int GetHashCode()
        {
            var code = new HashCode();
            if (Value != null)
            {
                foreach (var b in Value)
                {
                    code.Add(b);
                }
            }

            return code.ToHashCode();
        }
    }
}
