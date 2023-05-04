// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking;

public class TemporaryValuesTest
{
    [ConditionalFact]
    public void Set_temporary_values_for_normal_properties()
    {
        using (var context = new DefaultValuesContext())
        {
            var entity = new EntityWithNonIndexers();
            context.Add(entity);

            Assert.Equal(0, entity.ValueProperty);
            Assert.Null(entity.NullableValueProperty);
            Assert.Null(entity.ReferenceValueProperty);

            Assert.True(context.Entry(entity).Property(e => e.ValueProperty).CurrentValue < 0);
            Assert.True(context.Entry(entity).Property(e => e.NullableValueProperty).CurrentValue < 0);
            Assert.NotNull(context.Entry(entity).Property(e => e.ReferenceValueProperty).CurrentValue);

            Assert.True(context.Entry(entity).Property(e => e.ValueProperty).IsTemporary);
            Assert.True(context.Entry(entity).Property(e => e.NullableValueProperty).IsTemporary);
            Assert.True(context.Entry(entity).Property(e => e.ReferenceValueProperty).IsTemporary);

            entity.ValueProperty = 77;
            entity.NullableValueProperty = 77;
            entity.ReferenceValueProperty = "Seventy Seven";

            context.ChangeTracker.DetectChanges();

            Assert.Equal(77, entity.ValueProperty);
            Assert.Equal(77, entity.NullableValueProperty);
            Assert.Equal("Seventy Seven", entity.ReferenceValueProperty);

            Assert.Equal(77, context.Entry(entity).Property(e => e.ValueProperty).CurrentValue);
            Assert.Equal(77, context.Entry(entity).Property(e => e.NullableValueProperty).CurrentValue);
            Assert.Equal("Seventy Seven", context.Entry(entity).Property(e => e.ReferenceValueProperty).CurrentValue);

            Assert.False(context.Entry(entity).Property(e => e.ValueProperty).IsTemporary);
            Assert.False(context.Entry(entity).Property(e => e.NullableValueProperty).IsTemporary);
            Assert.False(context.Entry(entity).Property(e => e.ReferenceValueProperty).IsTemporary);

            context.Entry(entity).Property(e => e.ValueProperty).IsTemporary = true;
            context.Entry(entity).Property(e => e.NullableValueProperty).IsTemporary = true;
            context.Entry(entity).Property(e => e.ReferenceValueProperty).IsTemporary = true;

            Assert.Equal(77, entity.ValueProperty);
            Assert.Equal(77, entity.NullableValueProperty);
            Assert.Equal("Seventy Seven", entity.ReferenceValueProperty);

            Assert.Equal(77, context.Entry(entity).Property(e => e.ValueProperty).CurrentValue);
            Assert.Equal(77, context.Entry(entity).Property(e => e.NullableValueProperty).CurrentValue);
            Assert.Equal("Seventy Seven", context.Entry(entity).Property(e => e.ReferenceValueProperty).CurrentValue);

            Assert.True(context.Entry(entity).Property(e => e.ValueProperty).IsTemporary);
            Assert.True(context.Entry(entity).Property(e => e.NullableValueProperty).IsTemporary);
            Assert.True(context.Entry(entity).Property(e => e.ReferenceValueProperty).IsTemporary);
        }
    }

    [ConditionalFact]
    public void Set_temporary_values_for_indexer_properties()
    {
        using (var context = new DefaultValuesContext())
        {
            var entity1 = new EntityWithIndexerValueProperty();
            var entity2 = new EntityWithIndexerNullableValueProperty();
            var entity3 = new EntityWithIndexerReferenceProperty();

            context.AddRange(entity1, entity2, entity3);

            Assert.Equal(0, entity1["ValueProperty"]);
            Assert.Null(entity2["NullableValueProperty"]);
            Assert.Null(entity3["ReferenceValueProperty"]);

            Assert.True(context.Entry(entity1).Property<int>("ValueProperty").CurrentValue < 0);
            Assert.True(context.Entry(entity2).Property<int?>("NullableValueProperty").CurrentValue < 0);
            Assert.NotNull(context.Entry(entity3).Property<string>("ReferenceValueProperty").CurrentValue);

            Assert.True(context.Entry(entity1).Property<int>("ValueProperty").IsTemporary);
            Assert.True(context.Entry(entity2).Property<int?>("NullableValueProperty").IsTemporary);
            Assert.True(context.Entry(entity3).Property<string>("ReferenceValueProperty").IsTemporary);

            entity1["ValueProperty"] = 77;
            entity2["NullableValueProperty"] = 77;
            entity3["ReferenceValueProperty"] = "Seventy Seven";

            Assert.Equal(77, entity1["ValueProperty"]);
            Assert.Equal(77, entity2["NullableValueProperty"]);
            Assert.Equal("Seventy Seven", entity3["ReferenceValueProperty"]);

            Assert.Equal(77, context.Entry(entity1).Property<int>("ValueProperty").CurrentValue);
            Assert.Equal(77, context.Entry(entity2).Property<int?>("NullableValueProperty").CurrentValue);
            Assert.Equal("Seventy Seven", context.Entry(entity3).Property<string>("ReferenceValueProperty").CurrentValue);

            entity1["ValueProperty"] = 78;
            entity2["NullableValueProperty"] = 78;
            entity3["ReferenceValueProperty"] = "Seventy Eight";

            Assert.Equal(78, entity1["ValueProperty"]);
            Assert.Equal(78, entity2["NullableValueProperty"]);
            Assert.Equal("Seventy Eight", entity3["ReferenceValueProperty"]);

            Assert.Equal(78, context.Entry(entity1).Property<int>("ValueProperty").CurrentValue);
            Assert.Equal(78, context.Entry(entity2).Property<int?>("NullableValueProperty").CurrentValue);
            Assert.Equal("Seventy Eight", context.Entry(entity3).Property<string>("ReferenceValueProperty").CurrentValue);

            Assert.False(context.Entry(entity1).Property<int>("ValueProperty").IsTemporary);
            Assert.False(context.Entry(entity2).Property<int?>("NullableValueProperty").IsTemporary);
            Assert.False(context.Entry(entity3).Property<string>("ReferenceValueProperty").IsTemporary);

            context.Entry(entity1).Property<int>("ValueProperty").IsTemporary = true;
            context.Entry(entity2).Property<int?>("NullableValueProperty").IsTemporary = true;
            context.Entry(entity3).Property<string>("ReferenceValueProperty").IsTemporary = true;

            Assert.Equal(78, entity1["ValueProperty"]);
            Assert.Equal(78, entity2["NullableValueProperty"]);
            Assert.Equal("Seventy Eight", entity3["ReferenceValueProperty"]);

            Assert.Equal(78, context.Entry(entity1).Property<int>("ValueProperty").CurrentValue);
            Assert.Equal(78, context.Entry(entity2).Property<int?>("NullableValueProperty").CurrentValue);
            Assert.Equal("Seventy Eight", context.Entry(entity3).Property<string>("ReferenceValueProperty").CurrentValue);

            Assert.True(context.Entry(entity1).Property<int>("ValueProperty").IsTemporary);
            Assert.True(context.Entry(entity2).Property<int?>("NullableValueProperty").IsTemporary);
            Assert.True(context.Entry(entity3).Property<string>("ReferenceValueProperty").IsTemporary);
        }
    }

    [ConditionalFact]
    public void Set_temporary_values_for_indexer_properties_types_as_object()
    {
        using (var context = new DefaultValuesContext())
        {
            var entity = new EntityWithIndexersAsObject();

            context.Add(entity);

            Assert.Null(entity["ValueProperty"]);
            Assert.Null(entity["NullableValueProperty"]);
            Assert.Null(entity["ReferenceValueProperty"]);

            Assert.True(context.Entry(entity).Property<int>("ValueProperty").CurrentValue < 0);
            Assert.True(context.Entry(entity).Property<int?>("NullableValueProperty").CurrentValue < 0);
            Assert.NotNull(context.Entry(entity).Property<string>("ReferenceValueProperty").CurrentValue);

            Assert.True(context.Entry(entity).Property<int>("ValueProperty").IsTemporary);
            Assert.True(context.Entry(entity).Property<int?>("NullableValueProperty").IsTemporary);
            Assert.True(context.Entry(entity).Property<string>("ReferenceValueProperty").IsTemporary);

            entity["ValueProperty"] = 77;
            entity["NullableValueProperty"] = 77;
            entity["ReferenceValueProperty"] = "Seventy Seven";

            Assert.Equal(77, entity["ValueProperty"]);
            Assert.Equal(77, entity["NullableValueProperty"]);
            Assert.Equal("Seventy Seven", entity["ReferenceValueProperty"]);

            Assert.Equal(77, context.Entry(entity).Property<int>("ValueProperty").CurrentValue);
            Assert.Equal(77, context.Entry(entity).Property<int?>("NullableValueProperty").CurrentValue);
            Assert.Equal("Seventy Seven", context.Entry(entity).Property<string>("ReferenceValueProperty").CurrentValue);

            Assert.False(context.Entry(entity).Property<int>("ValueProperty").IsTemporary);
            Assert.False(context.Entry(entity).Property<int?>("NullableValueProperty").IsTemporary);
            Assert.False(context.Entry(entity).Property<string>("ReferenceValueProperty").IsTemporary);

            entity["ValueProperty"] = 78;
            entity["NullableValueProperty"] = 78;
            entity["ReferenceValueProperty"] = "Seventy Eight";

            Assert.Equal(78, entity["ValueProperty"]);
            Assert.Equal(78, entity["NullableValueProperty"]);
            Assert.Equal("Seventy Eight", entity["ReferenceValueProperty"]);

            Assert.Equal(78, context.Entry(entity).Property<int>("ValueProperty").CurrentValue);
            Assert.Equal(78, context.Entry(entity).Property<int?>("NullableValueProperty").CurrentValue);
            Assert.Equal("Seventy Eight", context.Entry(entity).Property<string>("ReferenceValueProperty").CurrentValue);

            Assert.False(context.Entry(entity).Property<int>("ValueProperty").IsTemporary);
            Assert.False(context.Entry(entity).Property<int?>("NullableValueProperty").IsTemporary);
            Assert.False(context.Entry(entity).Property<string>("ReferenceValueProperty").IsTemporary);

            context.Entry(entity).Property<int>("ValueProperty").IsTemporary = true;
            context.Entry(entity).Property<int?>("NullableValueProperty").IsTemporary = true;
            context.Entry(entity).Property<string>("ReferenceValueProperty").IsTemporary = true;

            Assert.Equal(78, entity["ValueProperty"]);
            Assert.Equal(78, entity["NullableValueProperty"]);
            Assert.Equal("Seventy Eight", entity["ReferenceValueProperty"]);

            Assert.Equal(78, context.Entry(entity).Property<int>("ValueProperty").CurrentValue);
            Assert.Equal(78, context.Entry(entity).Property<int?>("NullableValueProperty").CurrentValue);
            Assert.Equal("Seventy Eight", context.Entry(entity).Property<string>("ReferenceValueProperty").CurrentValue);

            Assert.True(context.Entry(entity).Property<int>("ValueProperty").IsTemporary);
            Assert.True(context.Entry(entity).Property<int?>("NullableValueProperty").IsTemporary);
            Assert.True(context.Entry(entity).Property<string>("ReferenceValueProperty").IsTemporary);
        }
    }

    private class EntityWithNonIndexers
    {
        public int Id { get; set; }

        public int ValueProperty { get; set; }
        public int? NullableValueProperty { get; set; }
        public string ReferenceValueProperty { get; set; }
    }

    private class EntityWithIndexerValueProperty
    {
        public int Id { get; set; }

        private readonly Dictionary<string, int> _values = new();

        public int this[string name]
        {
            get => _values.TryGetValue(name, out var value) ? value : default;
            set => _values[name] = value;
        }
    }

    private class EntityWithIndexerNullableValueProperty
    {
        public int Id { get; set; }

        private readonly Dictionary<string, int?> _values = new();

        public int? this[string name]
        {
            get => _values.TryGetValue(name, out var value) ? value : default;
            set => _values[name] = value;
        }
    }

    private class EntityWithIndexerReferenceProperty
    {
        public int Id { get; set; }

        private readonly Dictionary<string, string> _values = new();

        public string this[string name]
        {
            get => _values.TryGetValue(name, out var value) ? value : default;
            set => _values[name] = value;
        }
    }

    private class EntityWithIndexersAsObject
    {
        public int Id { get; set; }

        private readonly Dictionary<string, object> _values = new();

        public object this[string name]
        {
            get => _values.TryGetValue(name, out var value) ? value : default;
            set => _values[name] = value;
        }
    }

    private class DefaultValuesContext : DbContext
    {
        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                .UseInMemoryDatabase(GetType().FullName!);

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EntityWithNonIndexers>(
                b =>
                {
                    b.Property(e => e.ValueProperty)
                        .ValueGeneratedOnAdd()
                        .HasValueGenerator<TemporaryIntValueGenerator>();

                    b.Property(e => e.NullableValueProperty)
                        .ValueGeneratedOnAdd()
                        .HasValueGenerator<TemporaryIntValueGenerator>();

                    b.Property(e => e.ReferenceValueProperty)
                        .ValueGeneratedOnAdd()
                        .HasValueGenerator<TemporaryStringValueGenerator>();
                });

            modelBuilder.Entity<EntityWithIndexersAsObject>(
                b =>
                {
                    b.IndexerProperty<int>("ValueProperty")
                        .ValueGeneratedOnAdd()
                        .HasValueGenerator<TemporaryIntValueGenerator>();

                    b.IndexerProperty<int?>("NullableValueProperty")
                        .ValueGeneratedOnAdd()
                        .HasValueGenerator<TemporaryIntValueGenerator>();

                    b.IndexerProperty<string>("ReferenceValueProperty")
                        .ValueGeneratedOnAdd()
                        .HasValueGenerator<TemporaryStringValueGenerator>();
                });

            modelBuilder.Entity<EntityWithIndexerValueProperty>(
                b =>
                {
                    b.IndexerProperty<int>("ValueProperty")
                        .ValueGeneratedOnAdd()
                        .HasValueGenerator<TemporaryIntValueGenerator>();
                });

            modelBuilder.Entity<EntityWithIndexerNullableValueProperty>(
                b =>
                {
                    b.IndexerProperty<int?>("NullableValueProperty")
                        .ValueGeneratedOnAdd()
                        .HasValueGenerator<TemporaryIntValueGenerator>();
                });

            modelBuilder.Entity<EntityWithIndexerReferenceProperty>(
                b =>
                {
                    b.IndexerProperty<string>("ReferenceValueProperty")
                        .ValueGeneratedOnAdd()
                        .HasValueGenerator<TemporaryStringValueGenerator>();
                });
        }

        private class TemporaryStringValueGenerator : ValueGenerator<string>
        {
            public override bool GeneratesTemporaryValues
                => true;

            public override string Next(EntityEntry entry)
                => Guid.NewGuid().ToString();
        }
    }
}
