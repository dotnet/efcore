// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Cosmos.ValueGeneration;

public class IdValueGeneratorTest
{
    [ConditionalFact]
    public void Generated_ids_do_not_clash()
    {
        var modelBuilder = CosmosTestHelpers.Instance.CreateConventionBuilder();

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
            Create(new BytesStructEntity { Id = new BytesStruct(null) }),
            Create(new BytesStructEntity { Id = new BytesStruct([]) }),
            Create(new BytesStructEntity { Id = new BytesStruct([1]) }),
            Create(new BytesStructEntity { Id = new BytesStruct([2, 2]) }),
        };

        Assert.Equal(ids.Count, new HashSet<string>(ids.Concat(ids)).Count);

        string Create<TEntity>(TEntity entity)
            where TEntity : class, new()
            => (string)CosmosTestHelpers.Instance.CreateInternalEntry(model, EntityState.Added, entity)
                [model.FindEntityType(typeof(TEntity)).FindProperty(StoreKeyConvention.DefaultIdPropertyName)];
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
