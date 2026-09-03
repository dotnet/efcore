// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Local
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

public class PropertyDiscoveryConventionTest
{
    private class BaseWithPrivates
    {
        public int Id { get; private set; }

        private string _code;

        // ReSharper disable once ConvertToAutoProperty
        public string Code
        {
            get => _code;
            private set => _code = value;
        }

        public string Description { get; private set; }

        public void SetInformation(string code, string description)
        {
            Code = code;
            Description = description;
        }
    }

    private class DerivedWithoutPrivates : BaseWithPrivates;

    private class WithPrivatesContext : DbContext
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public DbSet<DerivedWithoutPrivates> Entities { get; set; }

        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                .UseInMemoryDatabase(nameof(WithPrivatesContext));
    }

    [ConditionalFact]
    public void Properties_with_private_setters_on_unmapped_base_types_are_discovered()
    {
        using var context = new WithPrivatesContext();
        var model = context.Model;

        Assert.Single(model.GetEntityTypes());

        var entityType = (IRuntimeEntityType)model.FindEntityType(typeof(DerivedWithoutPrivates));

        Assert.Equal(3, entityType.PropertyCount);

        var idProperty = entityType.FindProperty(nameof(BaseWithPrivates.Id));
        Assert.NotNull(idProperty.PropertyInfo);
        Assert.NotNull(idProperty.FieldInfo);

        var codeProperty = entityType.FindProperty(nameof(BaseWithPrivates.Code));
        Assert.NotNull(codeProperty.PropertyInfo);
        Assert.NotNull(codeProperty.FieldInfo);

        var descriptionProperty = entityType.FindProperty(nameof(BaseWithPrivates.Description));
        Assert.NotNull(descriptionProperty.PropertyInfo);
        Assert.NotNull(descriptionProperty.FieldInfo);

        var entity = new DerivedWithoutPrivates();
        entity.SetInformation("Foo!", "Bar!");
        context.Add(entity);

        context.SaveChanges();
    }

    [ConditionalFact]
    public void Can_save_and_query_using_entities_with_private_setters_on_base_types()
    {
        int id;
        using (var context = new WithPrivatesContext())
        {
            var entity = new DerivedWithoutPrivates();
            entity.SetInformation("Foo!", "Bar!");
            context.Add(entity);

            context.SaveChanges();

            id = entity.Id;
        }

        using (var context = new WithPrivatesContext())
        {
            var entity = context.Entities.Single(e => e.Id == id);

            Assert.Equal("Foo!", entity.Code);
            Assert.Equal("Bar!", entity.Description);

            Assert.Equal(id, context.Entry(entity).Property(e => e.Id).CurrentValue);
            Assert.Equal("Foo!", context.Entry(entity).Property(e => e.Code).CurrentValue);
            Assert.Equal("Bar!", context.Entry(entity).Property(e => e.Description).CurrentValue);

            context.Entry(entity).Property(e => e.Code).CurrentValue = "Foooo!";
            context.Entry(entity).Property(e => e.Description).CurrentValue = "Barrr!";

            Assert.Equal("Foo!", context.Entry(entity).Property(e => e.Code).OriginalValue);
            Assert.Equal("Bar!", context.Entry(entity).Property(e => e.Description).OriginalValue);
            Assert.Equal("Foooo!", context.Entry(entity).Property(e => e.Code).CurrentValue);
            Assert.Equal("Barrr!", context.Entry(entity).Property(e => e.Description).CurrentValue);
        }
    }

    private class EntityWithInvalidProperties
    {
        public static int Static { get; set; }

        public int WriteOnly
        {
            // ReSharper disable once ValueParameterNotUsed
            set { }
        }

        public int ReadOnly { get; }

        public int PrivateGetter { private get; set; }

        public int this[int index]
        {
            get => 0;
            // ReSharper disable once ValueParameterNotUsed
            set { }
        }
    }

    [ConditionalFact]
    public void IsValidProperty_returns_false_when_invalid()
    {
        var entityBuilder = CreateInternalEntityBuilder<EntityWithInvalidProperties>();

        RunConvention(entityBuilder);

        Assert.Empty(entityBuilder.Metadata.GetProperties());
    }

    private class EntityWithEveryPrimitive
    {
        public bool Boolean { get; set; }
        public byte Byte { get; set; }
        public byte[] ByteArray { get; set; }
        public char Char { get; set; }
        public DateTime DateTime { get; set; }
        public DateTimeOffset DateTimeOffset { get; set; }
        public decimal Decimal { get; set; }
        public double Double { get; set; }
        public Enum1 Enum { get; set; }
        public Guid Guid { get; set; }
        public short Int16 { get; set; }
        public int Int32 { get; set; }
        public long Int64 { get; set; }
        public bool? NullableBoolean { get; set; }
        public byte? NullableByte { get; set; }
        public char? NullableChar { get; set; }
        public DateTime? NullableDateTime { get; set; }
        public DateTimeOffset? NullableDateTimeOffset { get; set; }
        public decimal? NullableDecimal { get; set; }
        public double? NullableDouble { get; set; }
        public Enum1? NullableEnum { get; set; }
        public Guid? NullableGuid { get; set; }
        public short? NullableInt16 { get; set; }
        public int? NullableInt32 { get; set; }
        public long? NullableInt64 { get; set; }
        public sbyte? NullableSByte { get; set; }
        public float? NullableSingle { get; set; }
        public TimeSpan? NullableTimeSpan { get; set; }
        public ushort? NullableUInt16 { get; set; }
        public uint? NullableUInt32 { get; set; }
        public ulong? NullableUInt64 { get; set; }
        public int PrivateSetter { get; private set; }
        public sbyte SByte { get; set; }
        public float Single { get; set; }
        public string String { get; set; }
        public TimeSpan TimeSpan { get; set; }
        public ushort UInt16 { get; set; }
        public uint UInt32 { get; set; }
        public ulong UInt64 { get; set; }
    }

    private enum Enum1
    {
        Default
    }

    [ConditionalFact]
    public void IsPrimitiveProperty_returns_true_when_supported_type()
    {
        var entityBuilder = CreateInternalEntityBuilder<EntityWithEveryPrimitive>();

        RunConvention(entityBuilder);

        Assert.Equal(
            typeof(EntityWithEveryPrimitive)
                .GetRuntimeProperties()
                .Select(p => p.Name),
            entityBuilder.Metadata.GetProperties().Select(p => p.Name));
    }

    private class EntityWithNoPrimitives
    {
        public object Object { get; set; }
    }

    [ConditionalFact]
    public void IsPrimitiveProperty_returns_false_when_unsupported_type()
    {
        var entityBuilder = CreateInternalEntityBuilder<EntityWithNoPrimitives>();

        RunConvention(entityBuilder);

        Assert.Empty(entityBuilder.Metadata.GetProperties());
    }

    private void RunConvention(InternalEntityTypeBuilder entityTypeBuilder)
    {
        var context = new ConventionContext<IConventionEntityTypeBuilder>(entityTypeBuilder.Metadata.Model.ConventionDispatcher);

        new PropertyDiscoveryConvention(CreateDependencies())
            .ProcessEntityTypeAdded(entityTypeBuilder, context);

        Assert.False(context.ShouldStopProcessing());
    }

    private ProviderConventionSetBuilderDependencies CreateDependencies()
        => InMemoryTestHelpers.Instance.CreateContextServices().GetRequiredService<ProviderConventionSetBuilderDependencies>();

    private InternalEntityTypeBuilder CreateInternalEntityBuilder<T>()
    {
        var modelBuilder = new InternalModelBuilder(new Model());
        var entityBuilder = modelBuilder.Entity(typeof(T), ConfigurationSource.Convention);

        return entityBuilder;
    }
}
