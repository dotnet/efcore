// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.



// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.Infrastructure;

public partial class RelationalModelValidatorTest
{
    [ConditionalFact]
    public void Throws_when_added_property_is_not_mapped_to_store()
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        var entityTypeBuilder = modelBuilder.Entity(typeof(NonPrimitiveAsPropertyEntity));
        entityTypeBuilder.Property(typeof(Tuple<long>), "LongProperty");
        entityTypeBuilder.Ignore(nameof(NonPrimitiveAsPropertyEntity.Property));

        Assert.Equal(
            CoreStrings.PropertyNotMapped(
                typeof(Tuple<long>).ShortDisplayName(),
                typeof(NonPrimitiveAsPropertyEntity).ShortDisplayName(),
                "LongProperty"),
            Assert.Throws<InvalidOperationException>(() => Validate(modelBuilder)).Message);
    }

    [ConditionalFact]
    public void Throws_when_added_property_is_not_mapped_to_store_even_if_configured_to_use_column_type()
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        var entityTypeBuilder = modelBuilder.Entity(typeof(NonPrimitiveNonNavigationAsPropertyEntity));
        entityTypeBuilder.Property(typeof(Tuple<long>), "LongProperty")
            .HasColumnType("some_int_mapping");

        Assert.Equal(
            RelationalStrings.PropertyNotMapped(
                typeof(Tuple<long>).ShortDisplayName(),
                typeof(NonPrimitiveNonNavigationAsPropertyEntity).ShortDisplayName(),
                "LongProperty",
                "some_int_mapping"),
            Assert.Throws<InvalidOperationException>(() => Validate(modelBuilder)).Message);
    }

    [ConditionalFact]
    public override void Detects_non_list_complex_collection()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<WithReadOnlyCollection>(eb =>
        {
            eb.Property(e => e.Id);
            eb.ComplexCollection(
                e => e.Tags,
                cb =>
                {
                    cb.ToJson();
                    cb.Property(p => p.Key).IsRequired();
                });
        });

        VerifyError(
            CoreStrings.NonListCollection(
                nameof(WithReadOnlyCollection), nameof(WithReadOnlyCollection.Tags), "IReadOnlyCollection<JsonbField>",
                "IList<JsonbField>"),
            modelBuilder);
    }

    [ConditionalFact]
    public void Throws_when_complex_collection_is_not_mapped_to_json()
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        var entityTypeBuilder = modelBuilder.Entity<ComplexCollectionEntity>();
        entityTypeBuilder.Property(e => e.Id);
        entityTypeBuilder.HasKey(e => e.Id);

        var complexCollectionBuilder = entityTypeBuilder.ComplexCollection(e => e.Tags);

        Assert.Equal(
            RelationalStrings.ComplexCollectionNotMappedToJson(
                typeof(ComplexCollectionEntity).Name,
                nameof(ComplexCollectionEntity.Tags)),
            Assert.Throws<InvalidOperationException>(() => Validate(modelBuilder)).Message);
    }

    protected class ComplexCollectionEntity
    {
        public int Id { get; set; }
        public List<ComplexTag> Tags { get; set; } = [];
    }

    protected class ComplexTag
    {
        public string Name { get; set; } = "";
        public int Value { get; set; }
    }
}
