// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel;

// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Infrastructure;

public partial class ModelValidatorTest : ModelValidatorTestBase
{
    [ConditionalFact]
    public virtual void Detects_key_property_which_cannot_be_compared()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<WithNonComparableKey>(
            eb =>
            {
                eb.Property(e => e.Id);
                eb.HasKey(e => e.Id);
            });

        VerifyError(
            CoreStrings.NonComparableKeyType(nameof(WithNonComparableKey), nameof(WithNonComparableKey.Id), nameof(NotComparable)),
            modelBuilder);
    }

    protected class WithNonComparableKey
    {
        public NotComparable Id { get; set; }
    }

    [ConditionalFact]
    public virtual void Detects_noncomparable_key_property_with_comparer()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<WithNonComparableKey>(
            eb =>
            {
                eb.Property(e => e.Id).HasConversion(typeof(NotComparable), typeof(CustomValueComparer<NotComparable>));
                eb.HasKey(e => e.Id);
            });

        VerifyError(
            CoreStrings.NonComparableKeyType(nameof(WithNonComparableKey), nameof(WithNonComparableKey.Id), nameof(NotComparable)),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_noncomparable_key_property_with_provider_comparer()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<WithNonComparableKey>(
            eb =>
            {
                eb.Property(e => e.Id).HasConversion(
                    typeof(CastingConverter<NotComparable, NotComparable>), null, typeof(CustomValueComparer<NotComparable>));
                eb.HasKey(e => e.Id);
            });

        VerifyError(
            CoreStrings.NonComparableKeyTypes(
                nameof(WithNonComparableKey), nameof(WithNonComparableKey.Id), nameof(NotComparable), nameof(NotComparable)),
            modelBuilder);
    }

    public class CustomValueComparer<T> : ValueComparer<T> // Doesn't implement IComparer
    {
        public CustomValueComparer()
            : base(false)
        {
        }
    }

    [ConditionalFact]
    public virtual void Detects_unique_index_property_which_cannot_be_compared()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<WithNonComparableUniqueIndex>(
            eb =>
            {
                eb.HasIndex(e => e.Index).IsUnique();
            });

        VerifyError(
            CoreStrings.NonComparableKeyType(
                nameof(WithNonComparableUniqueIndex), nameof(WithNonComparableUniqueIndex.Index), nameof(NotComparable)),
            modelBuilder);
    }

    protected class WithNonComparableUniqueIndex
    {
        public int Id { get; set; }
        public NotComparable Index { get; set; }
    }

    [ConditionalFact]
    public virtual void Ignores_normal_property_which_cannot_be_compared()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<WithNonComparableNormalProperty>(
            eb =>
            {
                eb.Property(e => e.Id);
                eb.HasKey(e => e.Id);
                eb.Property(e => e.Foo);
            });

        Validate(modelBuilder);
    }

    protected class WithNonComparableNormalProperty
    {
        public int Id { get; set; }
        public NotComparable Foo { get; set; }
    }

    protected struct NotComparable;

    [ConditionalFact]
    public virtual void Detects_custom_converter_for_collection_type_without_comparer()
    {
        var modelBuilder = CreateConventionModelBuilder();

        IMutableProperty convertedProperty = null;
        modelBuilder.Entity<WithCollectionConversion>(
            eb =>
            {
                eb.Property(e => e.Id);
                convertedProperty = eb.Property(e => e.SomeStrings).Metadata;
                convertedProperty.SetValueConverter(
                    new ValueConverter<string[], string>(
                        v => string.Join(',', v),
                        v => v.Split(',', StringSplitOptions.None)));
            });

        VerifyWarning(
            CoreResources.LogCollectionWithoutComparer(
                new TestLogger<TestLoggingDefinitions>()).GenerateMessage("WithCollectionConversion", "SomeStrings"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Ignores_custom_converter_for_collection_type_with_comparer()
    {
        var modelBuilder = CreateConventionModelBuilder();

        IMutableProperty convertedProperty = null;
        modelBuilder.Entity<WithCollectionConversion>(
            eb =>
            {
                eb.Property(e => e.Id);
                convertedProperty = eb.Property(e => e.SomeStrings).Metadata;
                convertedProperty.SetValueConverter(
                    new ValueConverter<string[], string>(
                        v => string.Join(',', v),
                        v => v.Split(',', StringSplitOptions.None)));
            });

        convertedProperty.SetValueComparer(
            new ValueComparer<string[]>(
                (v1, v2) => v1.SequenceEqual(v2),
                v => v.GetHashCode()));

        Validate(modelBuilder);

        Assert.Empty(LoggerFactory.Log.Where(l => l.Level == LogLevel.Warning));
    }

    protected class WithCollectionConversion
    {
        public int Id { get; set; }
        public string[] SomeStrings { get; set; }
    }

    [ConditionalFact]
    public virtual void Throws_when_mapping_concrete_sealed_type_that_does_not_implement_IList()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<WithStringCollection>(
            eb =>
            {
                eb.Property(e => e.Id);
                eb.PrimitiveCollection(e => e.SomeString);
            });

        VerifyError(CoreStrings.BadListType("string", "IList<char>"), modelBuilder, sensitiveDataLoggingEnabled: false);
    }

    protected class WithStringCollection
    {
        public int Id { get; set; }
        public string SomeString { get; set; }
    }

    [ConditionalFact]
    public virtual void Throws_when_mapping_an_IReadOnlyCollection()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<WithReadOnlyCollection>(
            eb =>
            {
                eb.Property(e => e.Id);
                eb.PrimitiveCollection(e => e.Tags);
            });

        VerifyError(
            CoreStrings.ReadOnlyListType("IReadOnlyCollection<int>"),
            modelBuilder, sensitiveDataLoggingEnabled: false);
    }

    protected class WithReadOnlyCollection
    {
        public int Id { get; set; }
        public IReadOnlyCollection<int> Tags { get; set; }
    }

    [ConditionalFact]
    public virtual void Throws_when_mapping_an_IReadOnlyList()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<WithReadOnlyList>(
            eb =>
            {
                eb.Property(e => e.Id);
                eb.PrimitiveCollection(e => e.Tags);
            });

        VerifyError(
            CoreStrings.ReadOnlyListType("IReadOnlyList<char>"),
            modelBuilder, sensitiveDataLoggingEnabled: false);
    }

    protected class WithReadOnlyList
    {
        public int Id { get; set; }
        public IReadOnlyList<char> Tags { get; set; }
    }

    [ConditionalFact]
    public virtual void Does_not_throw_for_non_generic_collection()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<WithNonGenericCollection>(
            eb =>
            {
                eb.Property(e => e.Id);
                eb.PrimitiveCollection(e => e.Tags);
            });

        Validate(modelBuilder);
    }

    protected class MyCollection : IList<int>
    {
        private readonly List<int> _list = [];
        public IEnumerator<int> GetEnumerator() => _list.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public void Add(int item) => _list.Add(item);
        public void Clear() => _list.Clear();
        public bool Contains(int item) => _list.Contains(item);
        public void CopyTo(int[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);
        public bool Remove(int item) => _list.Remove(item);
        public int Count => _list.Count;
        public bool IsReadOnly => ((ICollection<int>)_list).IsReadOnly;
        public int IndexOf(int item) => _list.IndexOf(item);
        public void Insert(int index, int item) => _list.Insert(index, item);
        public void RemoveAt(int index) => _list.RemoveAt(index);
        public int this[int index]
        {
            get => _list[index];
            set => _list[index] = value;
        }
    }

    protected class WithNonGenericCollection
    {
        public int Id { get; set; }
        public MyCollection Tags { get; set; }
    }

    [ConditionalFact]
    public virtual void Ignores_binary_keys_and_strings_without_custom_comparer()
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        var model = modelBuilder.Model;

        var entityType = model.AddEntityType(typeof(WithStringAndBinaryKey));

        var keyProperty = entityType.AddProperty(nameof(WithStringAndBinaryKey.Id), typeof(byte[]));
        keyProperty.IsNullable = false;
        entityType.SetPrimaryKey(keyProperty);
        keyProperty.SetValueConverter(
            new ValueConverter<byte[], byte[]>(v => v, v => v));

        var stringProperty = entityType.AddProperty(nameof(WithStringAndBinaryKey.AString), typeof(string));
        stringProperty.SetValueConverter(
            new ValueConverter<string, string>(v => v, v => v));

        Validate(modelBuilder);

        Assert.Empty(LoggerFactory.Log.Where(l => l.Level == LogLevel.Warning));
    }

    protected class WithStringAndBinaryKey
    {
        public byte[] Id { get; set; }
        public string AString { get; set; }
    }

    [ConditionalFact]
    public virtual void Detects_filter_on_derived_type()
    {
        var modelBuilder = CreateConventionModelBuilder();
        var entityTypeA = modelBuilder.Entity<A>().Metadata;
        var entityTypeD = modelBuilder.Entity<D>().Metadata;

        entityTypeD.SetQueryFilter((Expression<Func<D, bool>>)(_ => true));

        VerifyError(
            CoreStrings.BadFilterDerivedType(entityTypeD.GetQueryFilter(), entityTypeD.DisplayName(), entityTypeA.DisplayName()),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_filter_on_owned_type()
    {
        var modelBuilder = CreateConventionModelBuilder();
        var queryFilter = (Expression<Func<ReferencedEntity, bool>>)(_ => true);
        modelBuilder.Entity<SampleEntity>()
            .OwnsOne(
                s => s.ReferencedEntity, eb =>
                {
                    eb.OwnedEntityType.SetQueryFilter(queryFilter);
                });

        VerifyError(CoreStrings.BadFilterOwnedType(queryFilter, nameof(ReferencedEntity)), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_on_shadow_key_created_explicitly()
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        var model = modelBuilder.Model;

        var entityType = model.AddEntityType(typeof(A));
        SetPrimaryKey(entityType);
        AddProperties(entityType);

        var keyProperty = ((IConventionEntityType)entityType).AddProperty("Key", typeof(int));
        ((IConventionEntityType)entityType).AddKey(keyProperty);

        VerifyWarning(
            CoreResources.LogShadowPropertyCreated(new TestLogger<TestLoggingDefinitions>()).GenerateMessage("A", "Key"), modelBuilder,
            LogLevel.Debug);
    }

    [ConditionalFact]
    public virtual void Passes_on_shadow_primary_key_created_by_convention_in_dependent_type()
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        var model = (IConventionModel)modelBuilder.Model;

        var entityType = model.AddEntityType(typeof(A));
        AddProperties((IMutableEntityType)entityType);
        entityType.AddProperty(nameof(A.Id), typeof(int));

        var keyProperty = entityType.AddProperty("Key", typeof(int));
        entityType.SetPrimaryKey(keyProperty);

        VerifyWarning(
            CoreResources.LogShadowPropertyCreated(new TestLogger<TestLoggingDefinitions>())
                .GenerateMessage("A", "Key"), modelBuilder, LogLevel.Debug);
    }

    [ConditionalFact]
    public virtual void Warns_on_uniquified_shadow_key_due_to_wrong_type()
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        var model = modelBuilder.Model;

        var principalType = model.AddEntityType(typeof(PrincipalOne));
        SetPrimaryKey(principalType);

        var dependentType = model.AddEntityType(typeof(DependentOne));
        SetPrimaryKey(dependentType);
        dependentType.AddProperty(DependentOne.PrincipalOneIdProperty);

        modelBuilder
            .Entity<PrincipalOne>()
            .HasMany(e => e.DependentsOnes)
            .WithOne(e => e.PrincipalOne);

        VerifyWarning(
            CoreResources.LogShadowForeignKeyPropertyCreated(
                new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                nameof(DependentOne),
                nameof(DependentOne.PrincipalOneId) + "1",
                nameof(DependentOne.PrincipalOneId)),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Warns_on_uniquified_shadow_key_due_to_unmapped_property()
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        var model = modelBuilder.Model;

        var principalType = model.AddEntityType(typeof(PrincipalTwo));
        SetPrimaryKey(principalType);

        var dependentType = model.AddEntityType(typeof(DependentTwo));
        SetPrimaryKey(dependentType);
        dependentType.AddIgnored(nameof(DependentTwo.PrincipalTwoId));

        modelBuilder
            .Entity<PrincipalTwo>()
            .HasMany(e => e.DependentsTwos)
            .WithOne(e => e.PrincipalTwo);

        VerifyWarning(
            CoreResources.LogShadowForeignKeyPropertyCreated(
                new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                nameof(DependentTwo),
                nameof(DependentTwo.PrincipalTwoId) + "1",
                nameof(DependentTwo.PrincipalTwoId)),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Warns_on_uniquified_shadow_key_due_to_use_in_another_relationship()
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        var model = modelBuilder.Model;

        var principalType = model.AddEntityType(typeof(PrincipalThree));
        SetPrimaryKey(principalType);

        var dependentType = model.AddEntityType(typeof(DependentThree));
        SetPrimaryKey(dependentType);
        dependentType.AddProperty(DependentThree.PrincipalThreeIdProperty);

        modelBuilder
            .Entity<PrincipalThree>()
            .HasMany(e => e.DependentsThreesA)
            .WithOne(e => e.PrincipalThreeA)
            .HasForeignKey(e => e.PrincipalThreeId);

        modelBuilder
            .Entity<PrincipalThree>()
            .HasMany(e => e.DependentsThreesB)
            .WithOne(e => e.PrincipalThreeB);

        VerifyWarning(
            CoreResources.LogShadowForeignKeyPropertyCreated(
                new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                nameof(DependentThree),
                nameof(DependentThree.PrincipalThreeId) + "1",
                nameof(DependentThree.PrincipalThreeId)),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Warns_on_double_uniquified_shadow_key_due_to_wrong_type()
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        var model = modelBuilder.Model;

        var principalType = model.AddEntityType(typeof(PrincipalFour));
        SetPrimaryKey(principalType);

        var dependentType = model.AddEntityType(typeof(DependentFour));
        SetPrimaryKey(dependentType);
        dependentType.AddProperty(DependentFour.PrincipalFourIdProperty);
        dependentType.AddProperty(DependentFour.PrincipalFourId1Property);

        modelBuilder
            .Entity<PrincipalFour>()
            .HasMany(e => e.DependentsFours)
            .WithOne(e => e.PrincipalFour);

        VerifyWarning(
            CoreResources.LogShadowForeignKeyPropertyCreated(
                new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                nameof(DependentFour),
                nameof(DependentFour.PrincipalFourId) + "2",
                nameof(DependentFour.PrincipalFourId)),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_shadow_key_referenced_by_foreign_key_by_convention()
    {
        var builder = CreateConventionlessModelBuilder();
        var modelBuilder = (InternalModelBuilder)builder.GetInfrastructure();
        var dependentEntityBuilder = modelBuilder.Entity(typeof(SampleEntityMinimal), ConfigurationSource.Convention);
        dependentEntityBuilder.Property(typeof(int), "Id", ConfigurationSource.Convention);
        dependentEntityBuilder.Ignore(nameof(SampleEntityMinimal.ReferencedEntity), ConfigurationSource.Explicit);

        dependentEntityBuilder.PrimaryKey(
            new List<string> { "Id" }, ConfigurationSource.Convention);

        var principalEntityBuilder = modelBuilder.Entity(typeof(ReferencedEntityMinimal), ConfigurationSource.Convention);
        principalEntityBuilder.Property(typeof(int), "Id", ConfigurationSource.Convention);
        principalEntityBuilder.PrimaryKey(
            new List<string> { "Id" }, ConfigurationSource.Convention);

        dependentEntityBuilder.Property(typeof(string), "Foo", ConfigurationSource.Convention);
        principalEntityBuilder.Property(typeof(string), "ReferencedFoo", ConfigurationSource.Convention);

        dependentEntityBuilder.HasRelationship(
            principalEntityBuilder.Metadata,
            dependentEntityBuilder.GetOrCreateProperties(
                new List<string> { "Foo" }, ConfigurationSource.Convention),
            principalEntityBuilder.HasKey(new[] { "ReferencedFoo" }, ConfigurationSource.Convention).Metadata,
            ConfigurationSource.Convention);

        VerifyError(
            CoreStrings.ReferencedShadowKey(
                typeof(SampleEntityMinimal).Name,
                typeof(ReferencedEntityMinimal).Name,
                "{'Foo' : string}",
                "{'Id' : int}"),
            builder);
    }

    [ConditionalFact]
    public virtual void Detects_a_null_primary_key()
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        modelBuilder.Entity<A>(
            b =>
            {
                b.Property(e => e.Id);
                b.Property(e => e.P0);
                b.Property(e => e.P1);
                b.Property(e => e.P2);
                b.Property(e => e.P3);
            });

        VerifyError(CoreStrings.EntityRequiresKey(nameof(A)), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_key_property_with_value_generated_on_update()
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        var model = modelBuilder.Model;
        var entityTypeA = model.AddEntityType(typeof(A));
        SetPrimaryKey(entityTypeA);
        AddProperties(entityTypeA);
        entityTypeA.FindPrimaryKey().Properties.Single().ValueGenerated = ValueGenerated.OnUpdate;

        VerifyError(CoreStrings.MutableKeyProperty(nameof(A.Id)), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_key_property_with_value_generated_on_add_or_update()
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        var model = modelBuilder.Model;

        var entityTypeA = model.AddEntityType(typeof(A));
        SetPrimaryKey(entityTypeA);
        AddProperties(entityTypeA);
        entityTypeA.FindPrimaryKey().Properties.Single().ValueGenerated = ValueGenerated.OnAddOrUpdate;

        VerifyError(CoreStrings.MutableKeyProperty(nameof(A.Id)), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_identifying_relationship_cycle()
    {
        var modelBuilder = base.CreateConventionModelBuilder();

        modelBuilder.Entity<C>().HasBaseType((string)null);
        modelBuilder.Entity<A>().HasOne<B>().WithOne().HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id).IsRequired();
        modelBuilder.Entity<A>().HasOne<C>().WithOne().HasForeignKey<C>(a => a.Id).HasPrincipalKey<A>(b => b.Id).IsRequired();
        modelBuilder.Entity<C>().HasOne<B>().WithOne().HasForeignKey<B>(a => a.Id).HasPrincipalKey<C>(b => b.Id).IsRequired();

        VerifyError(CoreStrings.IdentifyingRelationshipCycle("A -> B -> C"), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_on_relationship_cycle_for_property_configuration()
    {
        var modelBuilder = base.CreateConventionModelBuilder();

        modelBuilder.Entity<C>().HasBaseType((string)null);
        modelBuilder.Entity<D>().HasBaseType((string)null);
        modelBuilder.Entity<A>().HasOne<B>().WithOne().HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id).IsRequired();
        modelBuilder.Entity<A>().HasOne<C>().WithOne().HasForeignKey<C>(c => c.Id).HasPrincipalKey<A>(a => a.Id).IsRequired();
        modelBuilder.Entity<C>().HasOne<B>().WithOne().HasForeignKey<B>(b => b.Id).HasPrincipalKey<C>(c => c.Id).IsRequired();
        modelBuilder.Entity<D>().HasOne<B>().WithOne().HasForeignKey<D>(d => d.Id).HasPrincipalKey<B>(b => b.Id).IsRequired();

        var dId = modelBuilder.Model.FindEntityType(typeof(D)).FindProperty(nameof(D.Id));

        Assert.Null(dId.GetValueConverter());
        Assert.Null(dId.GetProviderClrType());
    }

    [ConditionalFact]
    public virtual void Passes_on_multiple_relationship_cycles_for_property_configuration()
    {
        var modelBuilder = base.CreateConventionModelBuilder();

        modelBuilder.Entity<C>().HasBaseType((string)null);
        modelBuilder.Entity<D>().HasBaseType((string)null);
        modelBuilder.Entity<A>().HasOne<B>().WithOne().HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id).IsRequired();
        modelBuilder.Entity<A>().HasOne<C>().WithOne().HasForeignKey<C>(c => c.Id).HasPrincipalKey<A>(a => a.Id).IsRequired();
        modelBuilder.Entity<C>().HasOne<B>().WithOne().HasForeignKey<B>(b => b.Id).HasPrincipalKey<C>(c => c.Id).IsRequired();
        modelBuilder.Entity<C>().HasOne<D>().WithOne().HasForeignKey<D>(d => d.Id).HasPrincipalKey<C>(c => c.Id).IsRequired();
        modelBuilder.Entity<D>().HasOne<E>().WithOne().HasForeignKey<E>(e => e.Id).HasPrincipalKey<D>(d => d.Id).IsRequired();
        modelBuilder.Entity<C>().HasOne<E>().WithOne().HasForeignKey<C>(c => c.Id).HasPrincipalKey<E>(e => e.Id).IsRequired();

        var aId = modelBuilder.Model.FindEntityType(typeof(A)).FindProperty(nameof(A.Id));

        Assert.Null(aId.GetValueConverter());
        Assert.Null(aId.GetProviderClrType());
    }

    [ConditionalFact]
    public virtual void Detects_conflicting_converter_and_provider_type_with_relationship_cycle()
    {
        var modelBuilder = base.CreateConventionModelBuilder();

        modelBuilder.Entity<C>().HasBaseType((string)null);
        modelBuilder.Entity<D>().HasBaseType((string)null);
        modelBuilder.Entity<A>().Property(b => b.Id).HasConversion<string>();
        modelBuilder.Entity<B>().Property(b => b.Id).HasConversion<CastingConverter<int, int>>();

        modelBuilder.Entity<B>().HasOne<C>().WithOne().HasForeignKey<B>(b => b.Id).HasPrincipalKey<C>(c => c.Id).IsRequired();
        modelBuilder.Entity<B>().HasOne<C>().WithOne().HasForeignKey<C>(c => c.Id).HasPrincipalKey<B>(b => b.Id).IsRequired();
        modelBuilder.Entity<A>().HasOne<D>().WithOne().HasForeignKey<D>(d => d.Id).HasPrincipalKey<A>(a => a.Id).IsRequired();
        modelBuilder.Entity<D>().HasOne<C>().WithOne().HasForeignKey<D>(d => d.Id).HasPrincipalKey<C>(c => c.Id).IsRequired();

        var dId = modelBuilder.Model.FindEntityType(typeof(D)).FindProperty(nameof(D.Id));

        Assert.Equal(CoreStrings.ConflictingRelationshipConversions("D", "Id", "string", "CastingConverter<int, int>"),
            Assert.Throws<InvalidOperationException>(dId.GetValueConverter).Message);
        Assert.Equal(CoreStrings.ConflictingRelationshipConversions("D", "Id", "string", "CastingConverter<int, int>"),
            Assert.Throws<InvalidOperationException>(dId.GetProviderClrType).Message);
    }

    [ConditionalFact]
    public virtual void Detects_conflicting_provider_types_with_relationship_cycle()
    {
        var modelBuilder = base.CreateConventionModelBuilder();

        modelBuilder.Entity<C>().HasBaseType((string)null);
        modelBuilder.Entity<D>().HasBaseType((string)null);
        modelBuilder.Entity<C>().Property(c => c.Id).HasConversion<long>();
        modelBuilder.Entity<A>().Property(a => a.Id).HasConversion<string>();

        modelBuilder.Entity<B>().HasOne<C>().WithOne().HasForeignKey<B>(b => b.Id).HasPrincipalKey<C>(c => c.Id).IsRequired();
        modelBuilder.Entity<B>().HasOne<C>().WithOne().HasForeignKey<C>(c => c.Id).HasPrincipalKey<B>(b => b.Id).IsRequired();
        modelBuilder.Entity<A>().HasOne<D>().WithOne().HasForeignKey<D>(d => d.Id).HasPrincipalKey<A>(a => a.Id).IsRequired();
        modelBuilder.Entity<D>().HasOne<C>().WithOne().HasForeignKey<D>(d => d.Id).HasPrincipalKey<C>(c => c.Id).IsRequired();

        var dId = modelBuilder.Model.FindEntityType(typeof(D)).FindProperty(nameof(D.Id));

        Assert.Equal(CoreStrings.ConflictingRelationshipConversions("D", "Id", "string", "long"),
            Assert.Throws<InvalidOperationException>(dId.GetValueConverter).Message);
        Assert.Equal(CoreStrings.ConflictingRelationshipConversions("D", "Id", "string", "long"),
            Assert.Throws<InvalidOperationException>(dId.GetProviderClrType).Message);
    }

    [ConditionalFact]
    public virtual void Detects_relationship_cycle_for_explicit_property_configuration()
    {
        var modelBuilder = base.CreateConventionModelBuilder();

        modelBuilder.Entity<C>().HasBaseType((string)null);
        modelBuilder.Entity<A>().HasOne<B>().WithOne().HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id).IsRequired();
        modelBuilder.Entity<A>().HasOne<C>().WithOne().HasForeignKey<C>(a => a.Id).HasPrincipalKey<A>(b => b.Id).IsRequired();
        modelBuilder.Entity<C>().HasOne<B>().WithOne().HasForeignKey<B>(a => a.Id).HasPrincipalKey<C>(b => b.Id).IsRequired();
        modelBuilder.Entity<D>().HasBaseType((string)null);
        modelBuilder.Entity<D>().HasOne<B>().WithOne().HasForeignKey<D>(a => a.Id).HasPrincipalKey<B>(b => b.Id).IsRequired();

        var aId = modelBuilder.Model.FindEntityType(typeof(A)).FindProperty(nameof(A.Id));
        aId.SetValueConverter((ValueConverter)null);
        aId.SetProviderClrType(null);

        var dId = modelBuilder.Model.FindEntityType(typeof(D)).FindProperty(nameof(D.Id));
        Assert.Null(dId.GetValueConverter());
        Assert.Null(dId.GetProviderClrType());

        VerifyError(
            CoreStrings.IdentifyingRelationshipCycle("A -> B -> C"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_on_multiple_relationship_paths()
    {
        var modelBuilder = base.CreateConventionModelBuilder();

        modelBuilder.Entity<A>();
        modelBuilder.Entity<B>();
        modelBuilder.Entity<C>().HasBaseType((string)null);
        modelBuilder.Entity<A>().HasOne<B>().WithOne().HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id).IsRequired();
        modelBuilder.Entity<A>().HasOne<C>().WithOne().HasForeignKey<A>(a => a.Id).HasPrincipalKey<C>(b => b.Id).IsRequired();
        modelBuilder.Entity<C>().HasOne<B>().WithOne().HasForeignKey<B>(a => a.Id).HasPrincipalKey<C>(b => b.Id).IsRequired();

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_on_redundant_foreign_key()
    {
        var modelBuilder = base.CreateConventionModelBuilder();

        modelBuilder.Entity<A>().HasOne<A>().WithOne().IsRequired().HasForeignKey<A>(a => a.Id).HasPrincipalKey<A>(b => b.Id);

        VerifyWarning(
            CoreResources.LogRedundantForeignKey(new TestLogger<TestLoggingDefinitions>()).GenerateMessage("{'Id'}", "A"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_on_escapable_foreign_key_cycles()
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        var model = modelBuilder.Model;

        var entityA = model.AddEntityType(typeof(A));
        SetPrimaryKey(entityA);
        AddProperties(entityA);
        var keyA1 = CreateKey(entityA);
        var keyA2 = CreateKey(entityA, startingPropertyIndex: 0, propertyCount: 2);

        var entityB = model.AddEntityType(typeof(B));
        SetPrimaryKey(entityB);
        AddProperties(entityB);

        entityB.AddIgnored(nameof(B.A));
        entityB.AddIgnored(nameof(B.AnotherA));
        entityB.AddIgnored(nameof(B.ManyAs));

        var keyB1 = CreateKey(entityB);
        var keyB2 = CreateKey(entityB, startingPropertyIndex: 1, propertyCount: 2);

        CreateForeignKey(keyA1, keyB1);
        CreateForeignKey(keyB1, keyA1);
        CreateForeignKey(keyA2, keyB2);

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_on_escapable_foreign_key_cycles_not_starting_at_hub()
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        var model = modelBuilder.Model;

        var entityA = model.AddEntityType(typeof(A));
        SetPrimaryKey(entityA);
        AddProperties(entityA);

        var keyA1 = CreateKey(entityA);
        var keyA2 = CreateKey(entityA, startingPropertyIndex: 1, propertyCount: 2);

        var entityB = model.AddEntityType(typeof(B));
        SetPrimaryKey(entityB);
        AddProperties(entityB);

        entityB.AddIgnored(nameof(B.A));
        entityB.AddIgnored(nameof(B.AnotherA));
        entityB.AddIgnored(nameof(B.ManyAs));

        var keyB1 = CreateKey(entityB);
        var keyB2 = CreateKey(entityB, startingPropertyIndex: 0, propertyCount: 2);

        CreateForeignKey(keyA1, keyB1);
        CreateForeignKey(keyB1, keyA1);
        CreateForeignKey(keyB2, keyA2);

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_on_foreign_key_cycle_with_one_GenerateOnAdd()
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        var model = modelBuilder.Model;

        var entityA = model.AddEntityType(typeof(A));
        SetPrimaryKey(entityA);
        AddProperties(entityA);

        var keyA = CreateKey(entityA);

        var entityB = model.AddEntityType(typeof(B));
        AddProperties(entityB);
        SetPrimaryKey(entityB);

        entityB.AddIgnored(nameof(B.A));
        entityB.AddIgnored(nameof(B.AnotherA));
        entityB.AddIgnored(nameof(B.ManyAs));

        var keyB = CreateKey(entityB);

        CreateForeignKey(keyA, keyB);
        CreateForeignKey(keyB, keyA);

        keyA.Properties[0].ValueGenerated = ValueGenerated.OnAdd;

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_on_double_reference_to_root_principal_property()
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        var model = modelBuilder.Model;

        var entityA = model.AddEntityType(typeof(A));
        SetPrimaryKey(entityA);
        AddProperties(entityA);

        var keyA1 = CreateKey(entityA);
        var keyA2 = CreateKey(entityA, startingPropertyIndex: 0, propertyCount: 2);

        var entityB = model.AddEntityType(typeof(B));
        SetPrimaryKey(entityB);
        AddProperties(entityB);

        entityB.AddIgnored(nameof(B.A));
        entityB.AddIgnored(nameof(B.AnotherA));
        entityB.AddIgnored(nameof(B.ManyAs));

        var keyB1 = CreateKey(entityB);
        var keyB2 = CreateKey(entityB, startingPropertyIndex: 0, propertyCount: 2);

        CreateForeignKey(keyA1, keyB1);
        CreateForeignKey(keyA2, keyB2);

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_on_diamond_path_to_root_principal_property()
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        var model = modelBuilder.Model;

        var entityA = model.AddEntityType(typeof(A));
        SetPrimaryKey(entityA);
        AddProperties(entityA);

        var keyA1 = CreateKey(entityA);
        var keyA2 = CreateKey(entityA, startingPropertyIndex: 0, propertyCount: 2);
        var keyA3 = CreateKey(entityA);
        var keyA4 = CreateKey(entityA, startingPropertyIndex: 2, propertyCount: 2);

        var entityB = model.AddEntityType(typeof(B));

        SetPrimaryKey(entityB);
        AddProperties(entityB);
        entityB.AddIgnored(nameof(B.A));
        entityB.AddIgnored(nameof(B.AnotherA));
        entityB.AddIgnored(nameof(B.ManyAs));

        var keyB1 = CreateKey(entityB);
        var keyB2 = CreateKey(entityB, startingPropertyIndex: 1, propertyCount: 2);

        CreateForeignKey(keyA1, keyB1);
        CreateForeignKey(keyA2, keyB2);

        CreateForeignKey(keyB1, keyA3);
        CreateForeignKey(keyB2, keyA4);

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_on_correct_inheritance()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<A>();
        modelBuilder.Entity<D>();

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_skipped_base_type()
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        var model = modelBuilder.Model;

        var entityA = model.AddEntityType(typeof(A));
        SetPrimaryKey(entityA);
        AddProperties(entityA);

        var entityD = modelBuilder.Entity<D>();
        entityD.HasBaseType<A>();

        var entityF = modelBuilder.Entity<F>();
        entityF.HasBaseType<A>();

        VerifyError(CoreStrings.InconsistentInheritance(nameof(F), nameof(A), nameof(D)), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_abstract_leaf_type()
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        var model = modelBuilder.Model;

        var entityA = model.AddEntityType(typeof(A));
        SetPrimaryKey(entityA);
        AddProperties(entityA);

        var entityAbstract = model.AddEntityType(typeof(Abstract));
        SetBaseType(entityAbstract, entityA);

        VerifyError(CoreStrings.AbstractLeafEntityType(entityAbstract.DisplayName()), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_generic_leaf_type()
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        var model = modelBuilder.Model;

        var entityAbstract = model.AddEntityType(typeof(Abstract));
        SetPrimaryKey(entityAbstract);
        AddProperties(entityAbstract);

        var entityGeneric = model.AddEntityType(typeof(Generic<>));
        SetBaseType(entityGeneric, entityAbstract);

        VerifyError(CoreStrings.AbstractLeafEntityType(entityGeneric.DisplayName()), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_on_valid_many_to_many_navigations()
    {
        var modelBuilder = CreateConventionModelBuilder();

        var model = modelBuilder.Model;
        var orderProductEntity = model.AddEntityType(typeof(OrderProduct));
        var orderEntity = model.FindEntityType(typeof(Order));
        var productEntity = model.FindEntityType(typeof(Product));
        var orderProductForeignKey = orderProductEntity
            .GetForeignKeys().Single(fk => fk.PrincipalEntityType == orderEntity);
        var productOrderForeignKey = orderProductEntity
            .GetForeignKeys().Single(fk => fk.PrincipalEntityType == productEntity);
        orderProductEntity.SetPrimaryKey(
            new[] { orderProductForeignKey.Properties.Single(), productOrderForeignKey.Properties.Single() });

        var productsNavigation = orderEntity.AddSkipNavigation(
            nameof(Order.Products), null, productEntity, true, false);
        productsNavigation.SetForeignKey(orderProductForeignKey);

        var ordersNavigation = productEntity.AddSkipNavigation(
            nameof(Product.Orders), null, orderEntity, true, false);
        ordersNavigation.SetForeignKey(productOrderForeignKey);

        productsNavigation.SetInverse(ordersNavigation);
        ordersNavigation.SetInverse(productsNavigation);

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_missing_foreign_key_for_skip_navigations()
    {
        var modelBuilder = CreateConventionModelBuilder();

        var model = modelBuilder.Model;
        var orderProductEntity = model.AddEntityType(typeof(OrderProduct));
        var orderEntity = model.FindEntityType(typeof(Order));
        var productEntity = model.FindEntityType(typeof(Product));
        var orderProductForeignKey = orderProductEntity
            .GetForeignKeys().Single(fk => fk.PrincipalEntityType == orderEntity);
        var productOrderForeignKey = orderProductEntity
            .GetForeignKeys().Single(fk => fk.PrincipalEntityType == productEntity);
        orderProductEntity.SetPrimaryKey(
            new[] { orderProductForeignKey.Properties.Single(), productOrderForeignKey.Properties.Single() });

        var productsNavigation = orderEntity.AddSkipNavigation(
            nameof(Order.Products), null, productEntity, true, false);

        VerifyError(
            CoreStrings.SkipNavigationNoForeignKey(nameof(Order.Products), nameof(Order)),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_missing_inverse_skip_navigations()
    {
        var modelBuilder = CreateConventionModelBuilder();

        var model = modelBuilder.Model;
        var orderProductEntity = model.AddEntityType(typeof(OrderProduct));
        var orderEntity = model.FindEntityType(typeof(Order));
        var productEntity = model.FindEntityType(typeof(Product));
        var orderProductForeignKey = orderProductEntity
            .GetForeignKeys().Single(fk => fk.PrincipalEntityType == orderEntity);
        var productOrderForeignKey = orderProductEntity
            .GetForeignKeys().Single(fk => fk.PrincipalEntityType == productEntity);
        orderProductEntity.SetPrimaryKey(
            new[] { orderProductForeignKey.Properties.Single(), productOrderForeignKey.Properties.Single() });

        var productsNavigation = orderEntity.AddSkipNavigation(
            nameof(Order.Products), null, productEntity, true, false);
        productsNavigation.SetForeignKey(orderProductForeignKey);

        VerifyError(
            CoreStrings.SkipNavigationNoInverse(nameof(Order.Products), nameof(Order)),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_nonCollection_skip_navigations()
    {
        var modelBuilder = CreateConventionModelBuilder();

        var model = modelBuilder.Model;
        var customerEntity = model.AddEntityType(typeof(Customer));
        var orderEntity = model.FindEntityType(typeof(Order));
        var orderDetailsEntity = model.FindEntityType(typeof(OrderDetails));
        new EntityTypeBuilder<OrderDetails>(orderDetailsEntity).Ignore(e => e.Customer);

        var productsNavigation = orderDetailsEntity.AddSkipNavigation(
            nameof(OrderDetails.Customer), null, customerEntity, false, false);
        orderDetailsEntity.RemoveIgnored(nameof(OrderDetails.Customer));

        VerifyError(
            CoreStrings.SkipNavigationNonCollection(nameof(OrderDetails.Customer), nameof(OrderDetails)),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_collection_complex_properties()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Ignore(typeof(Order));

        var model = modelBuilder.Model;
        var customerEntity = model.AddEntityType(typeof(Customer));
        customerEntity.AddComplexProperty(nameof(Customer.Orders), collection: true);

        VerifyError(
            CoreStrings.ComplexPropertyCollection(nameof(Customer), nameof(Customer.Orders)),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_shadow_complex_properties()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Ignore(typeof(Order));

        var model = modelBuilder.Model;
        var customerEntity = model.AddEntityType(typeof(Customer));
        customerEntity.AddComplexProperty("CustomerDetails", typeof(SponsorDetails), typeof(SponsorDetails));

        VerifyError(
            CoreStrings.ComplexPropertyShadow(nameof(Customer), "CustomerDetails"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_indexer_complex_properties()
    {
        var modelBuilder = CreateConventionModelBuilder();

        var model = modelBuilder.Model;
        var customerEntity = model.AddEntityType("Customer");
        customerEntity.AddComplexProperty(
            "CustomerDetails", typeof(SponsorDetails), customerEntity.FindIndexerPropertyInfo()!, typeof(SponsorDetails));

        VerifyError(
            CoreStrings.ComplexPropertyIndexer("Customer (Dictionary<string, object>)", "CustomerDetails"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_on_valid_owned_entity_types()
    {
        var builder = CreateConventionlessModelBuilder();
        var modelBuilder = (InternalModelBuilder)builder.GetInfrastructure();
        var entityTypeBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
        entityTypeBuilder.PrimaryKey(new[] { nameof(SampleEntity.Id) }, ConfigurationSource.Convention);
        entityTypeBuilder.Ignore(nameof(SampleEntity.Name), ConfigurationSource.Explicit);
        entityTypeBuilder.Ignore(nameof(SampleEntity.Number), ConfigurationSource.Explicit);
        entityTypeBuilder.Ignore(nameof(SampleEntity.OtherSamples), ConfigurationSource.Explicit);
        entityTypeBuilder.Ignore(nameof(SampleEntity.AnotherReferencedEntity), ConfigurationSource.Explicit);

        var ownershipBuilder = entityTypeBuilder.HasOwnership(
            typeof(ReferencedEntity), nameof(SampleEntity.ReferencedEntity), ConfigurationSource.Convention);

        var ownedTypeBuilder = ownershipBuilder.Metadata.DeclaringEntityType.Builder;
        ownedTypeBuilder.PrimaryKey(ownershipBuilder.Metadata.Properties.Select(p => p.Name).ToList(), ConfigurationSource.Convention);
        ownedTypeBuilder.Ignore(nameof(ReferencedEntity.Id), ConfigurationSource.Explicit);
        ownedTypeBuilder.Ignore(nameof(ReferencedEntity.SampleEntityId), ConfigurationSource.Explicit);

        Validate(builder);
    }

    [ConditionalFact]
    public virtual void Detects_entity_type_with_multiple_ownerships()
    {
        var builder = CreateConventionlessModelBuilder();
        var modelBuilder = (InternalModelBuilder)builder.GetInfrastructure();

        var entityTypeBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
        entityTypeBuilder.PrimaryKey(new[] { nameof(SampleEntity.Id) }, ConfigurationSource.Convention);
        entityTypeBuilder.Ignore(nameof(SampleEntity.Number), ConfigurationSource.Explicit);
        entityTypeBuilder.Ignore(nameof(SampleEntity.Name), ConfigurationSource.Explicit);
        entityTypeBuilder.Ignore(nameof(SampleEntity.OtherSamples), ConfigurationSource.Explicit);

        var ownershipBuilder = entityTypeBuilder.HasOwnership(
            typeof(ReferencedEntity), nameof(SampleEntity.ReferencedEntity), ConfigurationSource.Convention);

        var ownedTypeBuilder = ownershipBuilder.Metadata.DeclaringEntityType.Builder;
        ownedTypeBuilder.PrimaryKey(ownershipBuilder.Metadata.Properties.Select(p => p.Name).ToList(), ConfigurationSource.Convention);

        ownedTypeBuilder.HasRelationship(
                entityTypeBuilder.Metadata, null, nameof(SampleEntity.AnotherReferencedEntity), ConfigurationSource.Convention,
                setTargetAsPrincipal: true)
            .Metadata.IsOwnership = true;

        ownedTypeBuilder.Ignore(nameof(ReferencedEntity.Id), ConfigurationSource.Explicit);
        ownedTypeBuilder.Ignore(nameof(ReferencedEntity.SampleEntityId), ConfigurationSource.Explicit);

        VerifyError(
            CoreStrings.MultipleOwnerships(
                nameof(ReferencedEntity), "'SampleEntity.ReferencedEntity', 'SampleEntity.AnotherReferencedEntity'"),
            builder);
    }

    [ConditionalFact]
    public virtual void Detects_principal_owned_entity_type()
    {
        var builder = CreateConventionlessModelBuilder();
        var modelBuilder = (InternalModelBuilder)builder.GetInfrastructure();

        var entityTypeBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
        entityTypeBuilder.PrimaryKey(new[] { nameof(SampleEntity.Id) }, ConfigurationSource.Convention);
        entityTypeBuilder.Ignore(nameof(SampleEntity.Number), ConfigurationSource.Explicit);
        entityTypeBuilder.Ignore(nameof(SampleEntity.Name), ConfigurationSource.Explicit);
        entityTypeBuilder.Ignore(nameof(SampleEntity.OtherSamples), ConfigurationSource.Explicit);
        entityTypeBuilder.Ignore(nameof(SampleEntity.AnotherReferencedEntity), ConfigurationSource.Explicit);

        var ownershipBuilder = entityTypeBuilder.HasOwnership(
            typeof(ReferencedEntity), nameof(SampleEntity.ReferencedEntity), ConfigurationSource.Convention);

        var ownedTypeBuilder = ownershipBuilder.Metadata.DeclaringEntityType.Builder;
        ownedTypeBuilder.Ignore(nameof(ReferencedEntity.Id), ConfigurationSource.Explicit);
        ownedTypeBuilder.Ignore(nameof(ReferencedEntity.SampleEntityId), ConfigurationSource.Explicit);
        ownedTypeBuilder.PrimaryKey(ownershipBuilder.Metadata.Properties.Select(p => p.Name).ToList(), ConfigurationSource.Convention);

        var anotherEntityTypeBuilder = modelBuilder.Entity(typeof(AnotherSampleEntity), ConfigurationSource.Convention);
        anotherEntityTypeBuilder.PrimaryKey(new[] { nameof(AnotherSampleEntity.Id) }, ConfigurationSource.Convention);

        anotherEntityTypeBuilder.HasRelationship(
            ownedTypeBuilder.Metadata, nameof(AnotherSampleEntity.ReferencedEntity), ConfigurationSource.Convention,
            targetIsPrincipal: true);

        VerifyError(
            CoreStrings.PrincipalOwnedType(
                nameof(AnotherSampleEntity) + "." + nameof(AnotherSampleEntity.ReferencedEntity),
                nameof(ReferencedEntity),
                nameof(ReferencedEntity)),
            builder);
    }

    [ConditionalFact]
    public virtual void Detects_non_owner_navigation_to_owned_entity_type()
    {
        var builder = CreateConventionlessModelBuilder();
        var modelBuilder = (InternalModelBuilder)builder.GetInfrastructure();

        var entityTypeBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
        entityTypeBuilder.PrimaryKey(new[] { nameof(SampleEntity.Id) }, ConfigurationSource.Convention);
        entityTypeBuilder.Ignore(nameof(SampleEntity.Number), ConfigurationSource.Explicit);
        entityTypeBuilder.Ignore(nameof(SampleEntity.Name), ConfigurationSource.Explicit);
        entityTypeBuilder.Ignore(nameof(SampleEntity.OtherSamples), ConfigurationSource.Explicit);
        entityTypeBuilder.Ignore(nameof(SampleEntity.AnotherReferencedEntity), ConfigurationSource.Explicit);

        var ownershipBuilder = entityTypeBuilder.HasOwnership(
            typeof(ReferencedEntity), nameof(SampleEntity.ReferencedEntity), ConfigurationSource.Convention);

        var ownedTypeBuilder = ownershipBuilder.Metadata.DeclaringEntityType.Builder;
        ownedTypeBuilder.PrimaryKey(ownershipBuilder.Metadata.Properties.Select(p => p.Name).ToList(), ConfigurationSource.Convention);
        ownedTypeBuilder.Ignore(nameof(ReferencedEntity.Id), ConfigurationSource.Explicit);
        ownedTypeBuilder.Ignore(nameof(ReferencedEntity.SampleEntityId), ConfigurationSource.Explicit);

        var anotherEntityTypeBuilder = modelBuilder.Entity(typeof(AnotherSampleEntity), ConfigurationSource.Convention);
        anotherEntityTypeBuilder.PrimaryKey(new[] { nameof(AnotherSampleEntity.Id) }, ConfigurationSource.Convention);

        anotherEntityTypeBuilder.HasRelationship(
                ownedTypeBuilder.Metadata, nameof(AnotherSampleEntity.ReferencedEntity), ConfigurationSource.Convention)
            .HasEntityTypes(anotherEntityTypeBuilder.Metadata, ownedTypeBuilder.Metadata, ConfigurationSource.Convention);

        VerifyError(
            CoreStrings.InverseToOwnedType(
                nameof(AnotherSampleEntity), nameof(SampleEntity.ReferencedEntity), nameof(ReferencedEntity), nameof(SampleEntity)),
            builder);
    }

    [ConditionalFact]
    public virtual void Detects_derived_owned_entity_type()
    {
        var builder = CreateConventionlessModelBuilder();
        var modelBuilder = (InternalModelBuilder)builder.GetInfrastructure();

        var entityTypeBuilder = modelBuilder.Entity(typeof(B), ConfigurationSource.Convention);
        entityTypeBuilder.PrimaryKey(new[] { nameof(B.Id) }, ConfigurationSource.Convention);
        entityTypeBuilder.Property(typeof(int?), nameof(B.P0), ConfigurationSource.Explicit);
        entityTypeBuilder.Property(typeof(int?), nameof(B.P1), ConfigurationSource.Explicit);
        entityTypeBuilder.Property(typeof(int?), nameof(B.P2), ConfigurationSource.Explicit);
        entityTypeBuilder.Property(typeof(int?), nameof(B.P3), ConfigurationSource.Explicit);
        entityTypeBuilder.Ignore(nameof(B.AnotherA), ConfigurationSource.Explicit);
        entityTypeBuilder.Ignore(nameof(B.ManyAs), ConfigurationSource.Explicit);

        var ownershipBuilder = entityTypeBuilder.HasOwnership(typeof(D), nameof(B.A), ConfigurationSource.Convention);
        var ownedTypeBuilder = ownershipBuilder.Metadata.DeclaringEntityType.Builder;
        ownedTypeBuilder.PrimaryKey(ownershipBuilder.Metadata.Properties.Select(p => p.Name).ToList(), ConfigurationSource.Convention);
        ownedTypeBuilder.HasNoRelationship(ownershipBuilder.Metadata, ConfigurationSource.Convention);

        var baseOwnershipBuilder = entityTypeBuilder.HasOwnership(typeof(A), nameof(B.A), ConfigurationSource.Convention);
        var anotherEntityTypeBuilder = baseOwnershipBuilder.Metadata.DeclaringEntityType.Builder;
        anotherEntityTypeBuilder = modelBuilder.Entity(typeof(A), ConfigurationSource.Convention);
        anotherEntityTypeBuilder.PrimaryKey(new[] { nameof(A.Id) }, ConfigurationSource.Convention);
        anotherEntityTypeBuilder.Property(typeof(int?), nameof(A.P0), ConfigurationSource.Explicit);
        anotherEntityTypeBuilder.Property(typeof(int?), nameof(A.P1), ConfigurationSource.Explicit);
        anotherEntityTypeBuilder.Property(typeof(int?), nameof(A.P2), ConfigurationSource.Explicit);
        anotherEntityTypeBuilder.Property(typeof(int?), nameof(A.P3), ConfigurationSource.Explicit);

        Assert.NotNull(ownedTypeBuilder.HasBaseType(typeof(A), ConfigurationSource.DataAnnotation));

        VerifyError(CoreStrings.OwnedDerivedType(nameof(D)), builder);
    }

    [ConditionalFact]
    public virtual void Detects_owned_entity_type_without_ownership()
    {
        var builder = CreateConventionlessModelBuilder();
        var modelBuilder = (InternalModelBuilder)builder.GetInfrastructure();
        modelBuilder.Owned(typeof(A), ConfigurationSource.Convention);
        var aBuilder = modelBuilder.Entity(typeof(A), ConfigurationSource.Convention);
        aBuilder.Ignore(nameof(A.Id), ConfigurationSource.Explicit);
        aBuilder.Ignore(nameof(A.P0), ConfigurationSource.Explicit);
        aBuilder.Ignore(nameof(A.P1), ConfigurationSource.Explicit);
        aBuilder.Ignore(nameof(A.P2), ConfigurationSource.Explicit);
        aBuilder.Ignore(nameof(A.P3), ConfigurationSource.Explicit);

        VerifyError(CoreStrings.OwnerlessOwnedType(nameof(A)), builder);
    }

    [ConditionalFact]
    public virtual void Detects_ForeignKey_on_inherited_generated_key_property()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Abstract>().Property<int>("SomeId").ValueGeneratedOnAdd();
        modelBuilder.Entity<Abstract>().HasAlternateKey("SomeId");
        modelBuilder.Entity<Generic<int>>().HasOne<Abstract>().WithOne().HasForeignKey<Generic<int>>("SomeId");
        modelBuilder.Entity<Generic<string>>().Metadata.SetDiscriminatorValue("GenericString");

        VerifyError(
            CoreStrings.ForeignKeyPropertyInKey(
                "SomeId",
                "Generic<int>",
                "{'SomeId'}",
                nameof(Abstract)), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_for_ForeignKey_on_inherited_generated_key_property_abstract_base()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Abstract>().Property(e => e.Id).ValueGeneratedOnAdd();
        modelBuilder.Entity<Generic<int>>().HasOne<Abstract>().WithOne().HasForeignKey<Generic<int>>(e => e.Id);

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_for_ForeignKey_on_inherited_generated_composite_key_property()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Abstract>().Property<int>("SomeId").ValueGeneratedOnAdd();
        modelBuilder.Entity<Abstract>().Property<int>("SomeOtherId").ValueGeneratedOnAdd();
        modelBuilder.Entity<Abstract>().HasAlternateKey("SomeId", "SomeOtherId");
        modelBuilder.Entity<Generic<int>>().HasOne<Abstract>().WithOne().HasForeignKey<Generic<int>>("SomeId");
        modelBuilder.Entity<Generic<string>>();

        Validate(modelBuilder);
    }

    [ConditionalTheory]
    [InlineData(ChangeTrackingStrategy.ChangedNotifications)]
    [InlineData(ChangeTrackingStrategy.ChangingAndChangedNotifications)]
    [InlineData(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues)]
    public virtual void Detects_non_notifying_entities(ChangeTrackingStrategy changeTrackingStrategy)
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        var model = modelBuilder.Model;

        var entityType = model.AddEntityType(typeof(NonNotifyingEntity));
        var id = entityType.AddProperty("Id");
        entityType.SetPrimaryKey(id);

        model.SetChangeTrackingStrategy(changeTrackingStrategy);

        VerifyError(
            CoreStrings.ChangeTrackingInterfaceMissing("NonNotifyingEntity", changeTrackingStrategy, "INotifyPropertyChanged"),
            modelBuilder);
    }

    [ConditionalTheory]
    [InlineData(ChangeTrackingStrategy.ChangingAndChangedNotifications)]
    [InlineData(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues)]
    public virtual void Detects_changed_only_notifying_entities(ChangeTrackingStrategy changeTrackingStrategy)
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        var model = modelBuilder.Model;

        var entityType = model.AddEntityType(typeof(ChangedOnlyEntity));
        var id = entityType.AddProperty("Id");
        entityType.SetPrimaryKey(id);

        model.SetChangeTrackingStrategy(changeTrackingStrategy);

        VerifyError(
            CoreStrings.ChangeTrackingInterfaceMissing("ChangedOnlyEntity", changeTrackingStrategy, "INotifyPropertyChanging"),
            modelBuilder);
    }

    [ConditionalTheory]
    [InlineData(ChangeTrackingStrategy.Snapshot)]
    [InlineData(ChangeTrackingStrategy.ChangedNotifications)]
    [InlineData(ChangeTrackingStrategy.ChangingAndChangedNotifications)]
    [InlineData(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues)]
    public virtual void Passes_for_fully_notifying_entities(ChangeTrackingStrategy changeTrackingStrategy)
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        var model = modelBuilder.Model;

        var entityType = model.AddEntityType(typeof(FullNotificationEntity));
        var id = entityType.AddProperty("Id");
        entityType.SetPrimaryKey(id);

        model.SetChangeTrackingStrategy(changeTrackingStrategy);

        Validate(modelBuilder);
    }

    [ConditionalTheory]
    [InlineData(ChangeTrackingStrategy.Snapshot)]
    [InlineData(ChangeTrackingStrategy.ChangedNotifications)]
    public virtual void Passes_for_changed_only_entities_with_snapshot_or_changed_only_tracking(
        ChangeTrackingStrategy changeTrackingStrategy)
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        var model = modelBuilder.Model;

        var entityType = model.AddEntityType(typeof(ChangedOnlyEntity));
        var id = entityType.AddProperty("Id");
        entityType.SetPrimaryKey(id);

        model.SetChangeTrackingStrategy(changeTrackingStrategy);

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_for_non_notifying_entities_with_snapshot_tracking()
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        var model = modelBuilder.Model;

        var entityType = model.AddEntityType(typeof(NonNotifyingEntity));
        var id = entityType.AddProperty("Id");
        entityType.SetPrimaryKey(id);

        model.SetChangeTrackingStrategy(ChangeTrackingStrategy.Snapshot);

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_for_valid_seeds()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<A>().HasData(
            new A { Id = 1 });
        modelBuilder.Entity<D>().HasData(
            new D { Id = 2, P0 = 3 });

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_for_ignored_invalid_seeded_properties()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<EntityWithInvalidProperties>(
            eb =>
            {
                eb.Ignore(e => e.NotImplemented);

                eb.HasData(
                    new EntityWithInvalidProperties { Id = -1 });

                eb.HasData(
                    new
                    {
                        Id = -2,
                        NotImplemented = true,
                        Static = 1,
                        WriteOnly = 1,
                        ReadOnly = 1,
                        PrivateGetter = 1
                    });
            });

        Validate(modelBuilder);

        var data = modelBuilder.Model.GetEntityTypes().Single().GetSeedData();
        Assert.Equal(-1, data.First().Values.Single());
        Assert.Equal(-2, data.Last().Values.Single());
    }

    [ConditionalFact]
    public virtual void Detects_derived_seeds()
    {
        var modelBuilder = CreateConventionModelBuilder();

        Assert.Equal(
            CoreStrings.SeedDatumDerivedType(nameof(A), nameof(D)),
            Assert.Throws<InvalidOperationException>(
                () => modelBuilder.Entity<A>().HasData(
                    new D { Id = 2, P0 = 3 })).Message);
    }

    [ConditionalFact]
    public virtual void Detects_derived_seeds_for_owned_types()
    {
        var modelBuilder = CreateConventionModelBuilder();

        Assert.Equal(
            CoreStrings.SeedDatumDerivedType(nameof(A), nameof(D)),
            Assert.Throws<InvalidOperationException>(
                () => modelBuilder.Entity<B>()
                    .OwnsOne(
                        b => b.A, a => a.HasData(
                            new D { Id = 2, P0 = 3 }))
                    .OwnsOne(b => b.AnotherA)).Message);
    }

    [ConditionalFact]
    public virtual void Detects_missing_required_values_in_seeds()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<A>(
            e =>
            {
                e.Property(a => a.P0).IsRequired();
                e.HasData(
                    new A { Id = 1 });
            });

        VerifyError(
            CoreStrings.SeedDatumMissingValue(nameof(A), nameof(A.P0)),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_on_missing_required_store_generated_values_in_seeds()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<A>(
            e =>
            {
                e.Property(a => a.P0).IsRequired().ValueGeneratedOnAddOrUpdate();
                e.HasData(
                    new A { Id = 1 });
            });

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_missing_key_values_in_seeds()
    {
        var entity = new NonSignedIntegerKeyEntity();
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<NonSignedIntegerKeyEntity>(e => e.HasData(entity));

        Assert.Equal(
            ValueGenerated.OnAdd,
            modelBuilder.Model.FindEntityType(typeof(NonSignedIntegerKeyEntity)).FindProperty(nameof(NonSignedIntegerKeyEntity.Id))
                .ValueGenerated);
        VerifyError(
            CoreStrings.SeedDatumDefaultValue(nameof(NonSignedIntegerKeyEntity), nameof(NonSignedIntegerKeyEntity.Id), entity.Id),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_missing_signed_integer_key_values_in_seeds()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<A>(e => e.HasData(new A()));

        VerifyError(
            CoreStrings.SeedDatumSignedNumericValue(nameof(A), nameof(A.Id)),
            modelBuilder);
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public virtual void Detects_duplicate_seeds(bool sensitiveDataLoggingEnabled)
    {
        var modelBuilder = CreateConventionModelBuilder(sensitiveDataLoggingEnabled: sensitiveDataLoggingEnabled);
        modelBuilder.Entity<A>().HasData(
            new A { Id = 1 });
        modelBuilder.Entity<D>().HasData(
            new D { Id = 1 });

        VerifyError(
            sensitiveDataLoggingEnabled
                ? CoreStrings.SeedDatumDuplicateSensitive(nameof(D), $"{nameof(A.Id)}:1")
                : CoreStrings.SeedDatumDuplicate(nameof(D), $"{{'{nameof(A.Id)}'}}"),
            modelBuilder,
            sensitiveDataLoggingEnabled);
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public virtual void Detects_incompatible_values(bool sensitiveDataLoggingEnabled)
    {
        var modelBuilder = CreateConventionModelBuilder(sensitiveDataLoggingEnabled: sensitiveDataLoggingEnabled);
        modelBuilder.Entity<A>(
            e =>
            {
                e.HasData(
                    new { Id = 1, P0 = "invalid" });
            });

        VerifyError(
            sensitiveDataLoggingEnabled
                ? CoreStrings.SeedDatumIncompatibleValueSensitive(nameof(A), "invalid", nameof(A.P0), "int?")
                : CoreStrings.SeedDatumIncompatibleValue(nameof(A), nameof(A.P0), "int?"),
            modelBuilder,
            sensitiveDataLoggingEnabled);
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public virtual void Detects_reference_navigations_in_seeds(bool sensitiveDataLoggingEnabled)
    {
        var modelBuilder = CreateConventionModelBuilder(sensitiveDataLoggingEnabled: sensitiveDataLoggingEnabled);
        modelBuilder.Entity<SampleEntity>(
            e =>
            {
                e.HasData(
                    new SampleEntity { Id = 1, ReferencedEntity = new ReferencedEntity { Id = 2 } });
            });

        VerifyError(
            sensitiveDataLoggingEnabled
                ? CoreStrings.SeedDatumNavigationSensitive(
                    nameof(SampleEntity),
                    $"{nameof(SampleEntity.Id)}:1",
                    nameof(SampleEntity.ReferencedEntity),
                    nameof(ReferencedEntity),
                    $"{{'{nameof(ReferencedEntity.SampleEntityId)}'}}")
                : CoreStrings.SeedDatumNavigation(
                    nameof(SampleEntity),
                    nameof(SampleEntity.ReferencedEntity),
                    nameof(ReferencedEntity),
                    $"{{'{nameof(ReferencedEntity.SampleEntityId)}'}}"),
            modelBuilder,
            sensitiveDataLoggingEnabled);
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public virtual void Detects_reference_navigations_in_seeds2(bool sensitiveDataLoggingEnabled)
    {
        var modelBuilder = CreateConventionModelBuilder(sensitiveDataLoggingEnabled: sensitiveDataLoggingEnabled);
        modelBuilder.Entity<Order>(
            e =>
            {
                e.HasMany(o => o.Products)
                    .WithMany(p => p.Orders);
                e.HasData(
                    new Order { Id = 1, Products = new List<Product> { new() } });
            });

        VerifyError(
            sensitiveDataLoggingEnabled
                ? CoreStrings.SeedDatumNavigationSensitive(
                    nameof(Order),
                    $"{nameof(Order.Id)}:1",
                    nameof(Order.Products),
                    "OrderProduct (Dictionary<string, object>)",
                    "{'OrdersId'}")
                : CoreStrings.SeedDatumNavigation(
                    nameof(Order),
                    nameof(Order.Products),
                    "OrderProduct (Dictionary<string, object>)",
                    "{'OrdersId'}"),
            modelBuilder,
            sensitiveDataLoggingEnabled);
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public virtual void Detects_collection_navigations_in_seeds(bool sensitiveDataLoggingEnabled)
    {
        var modelBuilder = CreateConventionModelBuilder(sensitiveDataLoggingEnabled: sensitiveDataLoggingEnabled);
        modelBuilder.Entity<SampleEntity>(
            e =>
            {
                e.HasData(
                    new SampleEntity
                    {
                        Id = 1,
                        OtherSamples = new HashSet<SampleEntity>(
                            new[] { new SampleEntity { Id = 2 } })
                    });
            });

        VerifyError(
            sensitiveDataLoggingEnabled
                ? CoreStrings.SeedDatumNavigationSensitive(
                    nameof(SampleEntity),
                    $"{nameof(SampleEntity.Id)}:1",
                    nameof(SampleEntity.OtherSamples),
                    nameof(SampleEntity),
                    "{'SampleEntityId'}")
                : CoreStrings.SeedDatumNavigation(
                    nameof(SampleEntity),
                    nameof(SampleEntity.OtherSamples),
                    nameof(SampleEntity),
                    "{'SampleEntityId'}"),
            modelBuilder,
            sensitiveDataLoggingEnabled);
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public virtual void Detects_complex_properties_in_seeds(bool sensitiveDataLoggingEnabled)
    {
        var modelBuilder = CreateConventionModelBuilder(sensitiveDataLoggingEnabled: sensitiveDataLoggingEnabled);
        modelBuilder.Entity<SampleEntity>(
            e =>
            {
                e.HasData(
                    new SampleEntity { Id = 1, ReferencedEntity = new ReferencedEntity { Id = 2 } });
                e.ComplexProperty(s => s.ReferencedEntity).IsRequired();
            });

        VerifyError(
            sensitiveDataLoggingEnabled
                ? CoreStrings.SeedDatumComplexPropertySensitive(
                    nameof(SampleEntity),
                    $"{nameof(SampleEntity.Id)}:1",
                    nameof(SampleEntity.ReferencedEntity))
                : CoreStrings.SeedDatumComplexProperty(
                    nameof(SampleEntity),
                    nameof(SampleEntity.ReferencedEntity)),
            modelBuilder,
            sensitiveDataLoggingEnabled);
    }

    [ConditionalFact]
    public virtual void Throws_on_two_properties_sharing_a_field()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Customer>().Property(c => c.PartitionId).HasField("_name");

        VerifyError(
            CoreStrings.ConflictingFieldProperty(
                nameof(Customer), nameof(Customer.PartitionId), "_name", nameof(Customer), nameof(Customer.Name)),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Throws_on_property_using_a_field_mapped_as_another_property()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Customer>().Property(c => c.PartitionId).HasField("OtherName");
        modelBuilder.Entity<Customer>().Property(c => c.OtherName);

        VerifyError(
            CoreStrings.ConflictingFieldProperty(
                nameof(Customer), nameof(Customer.PartitionId), nameof(Customer.OtherName), nameof(Customer), nameof(Customer.OtherName)),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_missing_discriminator_property()
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        var model = modelBuilder.Model;

        var entityA = model.AddEntityType(typeof(A));
        SetPrimaryKey(entityA);
        AddProperties(entityA);

        var entityC = model.AddEntityType(typeof(C));
        entityC.BaseType = entityA;

        VerifyError(CoreStrings.NoDiscriminatorProperty(entityA.DisplayName()), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_incompatible_discriminator_value()
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        var model = modelBuilder.Model;

        var entityA = model.AddEntityType(typeof(A));
        SetPrimaryKey(entityA);
        AddProperties(entityA);

        var entityC = model.AddEntityType(typeof(C));
        SetBaseType(entityC, entityA);

        entityA.SetDiscriminatorProperty(entityA.AddProperty("D", typeof(int)));
        entityA.SetDiscriminatorValue("1");

        entityC.SetDiscriminatorValue(1);

        VerifyError(CoreStrings.DiscriminatorValueIncompatible("1", nameof(A), "int"), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_missing_discriminator_value_on_base()
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        var model = modelBuilder.Model;

        var entityA = model.AddEntityType(typeof(A));
        SetPrimaryKey(entityA);
        AddProperties(entityA);

        var entityC = model.AddEntityType(typeof(C));
        SetBaseType(entityC, entityA);

        entityA.SetDiscriminatorProperty(entityA.AddProperty("D", typeof(int)));
        entityA.RemoveDiscriminatorValue();

        entityC.SetDiscriminatorValue(1);

        VerifyError(CoreStrings.NoDiscriminatorValue(entityA.DisplayName()), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_missing_discriminator_value_on_leaf()
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        var model = modelBuilder.Model;

        var entityAbstract = model.AddEntityType(typeof(Abstract));
        SetPrimaryKey(entityAbstract);
        AddProperties(entityAbstract);

        var entityGeneric = model.AddEntityType(typeof(Generic<string>));
        SetBaseType(entityGeneric, entityAbstract);

        entityAbstract.SetDiscriminatorProperty(entityAbstract.AddProperty("D", typeof(int)));
        entityAbstract.SetDiscriminatorValue(0);

        entityGeneric.RemoveDiscriminatorValue();

        VerifyError(CoreStrings.NoDiscriminatorValue(entityGeneric.DisplayName()), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_missing_non_hierarchy_discriminator_value()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<A>().HasDiscriminator<byte>("ClassType");

        VerifyError(CoreStrings.NoDiscriminatorValue(typeof(A).Name), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_missing_non_string_discriminator_values()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<C>();
        modelBuilder.Entity<A>().HasDiscriminator<byte>("ClassType")
            .HasValue<A>(0)
            .HasValue<D>(1);

        VerifyError(CoreStrings.NoDiscriminatorValue(typeof(C).Name), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_duplicate_discriminator_values()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<A>().HasDiscriminator<byte>("ClassType")
            .HasValue<A>(1)
            .HasValue<C>(1)
            .HasValue<D>(2);

        VerifyError(CoreStrings.DuplicateDiscriminatorValue(typeof(C).Name, 1, typeof(A).Name), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Required_navigation_with_query_filter_on_one_side_issues_a_warning()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Customer>().HasMany(x => x.Orders).WithOne(x => x.Customer).IsRequired();
        modelBuilder.Entity<Customer>().HasQueryFilter(x => x.Id > 5);
        modelBuilder.Ignore<OrderDetails>();

        var message = CoreResources.LogPossibleIncorrectRequiredNavigationWithQueryFilterInteraction(
            CreateValidationLogger()).GenerateMessage(nameof(Customer), nameof(Order));

        VerifyWarning(message, modelBuilder);
    }

    [ConditionalFact]
    public virtual void Optional_navigation_with_query_filter_on_one_side_doesnt_issue_a_warning()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Customer>().HasMany(x => x.Orders).WithOne(x => x.Customer).IsRequired(false);
        modelBuilder.Entity<Customer>().HasQueryFilter(x => x.Id > 5);

        var message = CoreResources.LogPossibleIncorrectRequiredNavigationWithQueryFilterInteraction(
            CreateValidationLogger()).GenerateMessage(nameof(Customer), nameof(Order));

        VerifyLogDoesNotContain(message, modelBuilder);
    }

    [ConditionalFact]
    public virtual void Required_navigation_with_query_filter_on_both_sides_doesnt_issue_a_warning()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Customer>().HasMany(x => x.Orders).WithOne(x => x.Customer).IsRequired();
        modelBuilder.Entity<Customer>().HasQueryFilter(x => x.Id > 5);
        modelBuilder.Entity<Order>().HasQueryFilter(x => x.Customer.Id > 5);

        var message = CoreResources.LogPossibleIncorrectRequiredNavigationWithQueryFilterInteraction(
            CreateValidationLogger()).GenerateMessage(nameof(Customer), nameof(Order));

        VerifyLogDoesNotContain(message, modelBuilder);
    }

    [ConditionalFact]
    public virtual void Required_navigation_on_derived_type_with_query_filter_on_both_sides_doesnt_issue_a_warning()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Blog>().HasMany(x => x.PicturePosts).WithOne(x => x.Blog).IsRequired();
        modelBuilder.Entity<Blog>(e => e.HasQueryFilter(b => b.IsDeleted == false));
        modelBuilder.Entity<Post>(e => e.HasQueryFilter(p => p.IsDeleted == false));

        var message = CoreResources.LogPossibleIncorrectRequiredNavigationWithQueryFilterInteraction(
            CreateValidationLogger()).GenerateMessage(nameof(Blog), nameof(PicturePost));

        VerifyLogDoesNotContain(message, modelBuilder);
    }

    [ConditionalFact]
    public virtual void Required_navigation_targeting_derived_type_with_no_query_filter_issues_a_warning()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Post>(e => e.HasQueryFilter(p => p.IsDeleted == false).Ignore(e => e.Blog));
        modelBuilder.Entity<PicturePost>().HasMany(e => e.Pictures).WithOne(e => e.PicturePost).IsRequired();

        var message = CoreResources.LogPossibleIncorrectRequiredNavigationWithQueryFilterInteraction(
            CreateValidationLogger()).GenerateMessage(nameof(PicturePost), nameof(Picture));

        VerifyWarning(message, modelBuilder);
    }

    [ConditionalFact]
    public virtual void Required_navigation_on_owned_type_with_query_filter_on_owner_doesnt_issue_a_warning()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Blog>(
            e =>
            {
                e.Ignore(i => i.PicturePosts);
                e.HasQueryFilter(b => b.IsDeleted == false);
                e.OwnsMany(i => i.BlogOwnedEntities);
            });

        var message = CoreResources.LogPossibleIncorrectRequiredNavigationWithQueryFilterInteraction(
            CreateValidationLogger()).GenerateMessage(nameof(Blog), nameof(BlogOwnedEntity));

        VerifyLogDoesNotContain(message, modelBuilder);
    }

    [ConditionalFact]
    public virtual void Shared_type_inheritance_throws()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.SharedTypeEntity<A>("Shared1");
        modelBuilder.SharedTypeEntity<C>("Shared2").HasBaseType("Shared1");

        VerifyError(CoreStrings.SharedTypeDerivedType("Shared2 (C)"), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Seeding_keyless_entity_throws()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<KeylessSeed>(
            e =>
            {
                e.HasNoKey();
                e.HasData(
                    new KeylessSeed { Species = "Apple" });
            });

        VerifyError(CoreStrings.SeedKeylessEntity(nameof(KeylessSeed)), modelBuilder);
    }

    // INotify interfaces not really implemented; just marking the classes to test metadata construction
    protected class FullNotificationEntity : INotifyPropertyChanging, INotifyPropertyChanged
    {
        public int Id { get; set; }

#pragma warning disable 67
        public event PropertyChangingEventHandler PropertyChanging;
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 67
    }

    // INotify interfaces not really implemented; just marking the classes to test metadata construction
    protected class ChangedOnlyEntity : INotifyPropertyChanged
    {
        public int Id { get; set; }

#pragma warning disable 67
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 67
    }

    protected class NonNotifyingEntity
    {
        public int Id { get; set; }
    }

    protected class NonSignedIntegerKeyEntity
    {
        public uint Id { get; set; }
    }
}
