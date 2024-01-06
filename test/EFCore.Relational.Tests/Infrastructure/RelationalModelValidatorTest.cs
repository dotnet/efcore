// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Xunit.Sdk;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Infrastructure;

public partial class RelationalModelValidatorTest : ModelValidatorTest
{
    public override void Detects_key_property_which_cannot_be_compared()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<WithNonComparableKey>(
            eb =>
            {
                eb.Property(e => e.Id);
                eb.HasKey(e => e.Id);
            });

        VerifyError(
            CoreStrings.PropertyNotMapped(nameof(NotComparable), nameof(WithNonComparableKey), nameof(WithNonComparableKey.Id)),
            modelBuilder);
    }

    public override void Detects_noncomparable_key_property_with_comparer()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<WithNonComparableKey>(
            eb =>
            {
                eb.Property(e => e.Id).HasConversion(typeof(NotComparable), typeof(CustomValueComparer<NotComparable>));
                eb.HasKey(e => e.Id);
            });

        VerifyError(
            CoreStrings.PropertyNotMapped(nameof(NotComparable), nameof(WithNonComparableKey), nameof(WithNonComparableKey.Id)),
            modelBuilder);
    }

    public override void Detects_noncomparable_key_property_with_provider_comparer()
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
            CoreStrings.PropertyNotMapped(nameof(NotComparable), nameof(WithNonComparableKey), nameof(WithNonComparableKey.Id)),
            modelBuilder);
    }

    public override void Detects_unique_index_property_which_cannot_be_compared()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<WithNonComparableUniqueIndex>(
            eb =>
            {
                eb.HasIndex(e => e.Index).IsUnique();
            });

        VerifyError(
            CoreStrings.PropertyNotMapped(
                nameof(NotComparable), nameof(WithNonComparableUniqueIndex), nameof(WithNonComparableUniqueIndex.Index)),
            modelBuilder);
    }

    public override void Ignores_normal_property_which_cannot_be_compared()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<WithNonComparableNormalProperty>(
            eb =>
            {
                eb.Property(e => e.Id);
                eb.HasKey(e => e.Id);
                eb.Property(e => e.Foo);
            });

        VerifyError(
            CoreStrings.PropertyNotMapped(
                nameof(NotComparable), nameof(WithNonComparableNormalProperty), nameof(WithNonComparableNormalProperty.Foo)),
            modelBuilder);
    }

    public override void Detects_missing_discriminator_property()
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        var model = modelBuilder.Model;

        var entityA = model.AddEntityType(typeof(A));
        SetPrimaryKey(entityA);
        AddProperties(entityA);

        var entityC = model.AddEntityType(typeof(C));
        entityC.BaseType = entityA;

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Ignores_bool_with_default_value_false()
    {
        var modelBuilder = CreateConventionModelBuilder();
        var model = modelBuilder.Model;

        var entityType = model.AddEntityType(typeof(E));
        entityType.FindProperty("ImNot")!.SetDefaultValue(false);
        entityType.FindProperty("ImNotUsed")!.SetDefaultValue(false);

        var property = entityType.FindProperty("ImBool")!;
        property.SetDefaultValue(false);
        property.ValueGenerated = ValueGenerated.OnAdd;

        Validate(modelBuilder);

        Assert.DoesNotContain(LoggerFactory.Log, l => l.Level == LogLevel.Warning);
    }

    [ConditionalFact]
    public virtual void Bool_with_true_default_value_okay_because_sentinel_set_to_true()
    {
        var modelBuilder = CreateConventionModelBuilder();
        var model = modelBuilder.Model;

        var entityType = model.AddEntityType(typeof(E));
        entityType.FindProperty("ImNot")!.SetDefaultValue(true);
        entityType.FindProperty("ImNotUsed")!.SetDefaultValue(true);

        var property = entityType.FindProperty("ImBool")!;
        property.SetDefaultValue(true);
        property.ValueGenerated = ValueGenerated.OnAdd;

        Assert.True((bool)property.Sentinel!);

        Assert.DoesNotContain(LoggerFactory.Log, l => l.Level == LogLevel.Warning);
    }

    [ConditionalFact] // Issue #28509
    public virtual void Bool_with_default_value_and_nullable_backing_field_is_fine()
    {
        var modelBuilder = CreateConventionModelBuilder();
        var model = modelBuilder.Model;

        var entityType = model.AddEntityType(typeof(E2));
        var property = entityType.FindProperty("ImBool")!;
        property.SetField("_imBool");
        property.SetDefaultValue(true);
        property.ValueGenerated = ValueGenerated.OnAdd;

        Assert.DoesNotContain(LoggerFactory.Log, l => l.Level == LogLevel.Warning);
    }

    [ConditionalFact]
    public virtual void Detects_bool_with_default_expression()
    {
        var modelBuilder = CreateConventionModelBuilder();
        var model = modelBuilder.Model;

        var entityType = model.AddEntityType(typeof(E));
        entityType.FindProperty("ImNot")!.SetDefaultValueSql("TRUE");
        var property = entityType.FindProperty("ImBool")!;
        property.SetDefaultValueSql("TRUE");
        property.ValueGenerated = ValueGenerated.OnAddOrUpdate;

        VerifyWarning(
            RelationalResources.LogBoolWithDefaultWarning(new TestLogger<TestRelationalLoggingDefinitions>())
                .GenerateMessage("bool", "ImBool", "E", "False", "bool"), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Ignores_enum_with_default_value_matching_CLR_default()
    {
        var modelBuilder = CreateConventionModelBuilder();
        var model = modelBuilder.Model;

        var entityType = model.AddEntityType(typeof(WithEnum));
        var defaultEnum = entityType.FindProperty(nameof(WithEnum.EnumWithDefaultConstraint))!;
        defaultEnum!.SetDefaultValue(X.A);
        defaultEnum.Sentinel = X.A;
        var nullableEnum = entityType.FindProperty(nameof(WithEnum.NullableEnum))!;
        nullableEnum.SetDefaultValue(X.B);
        nullableEnum.Sentinel = X.B;

        Validate(modelBuilder);

        Assert.DoesNotContain(LoggerFactory.Log, l => l.Level == LogLevel.Warning);
    }

    [ConditionalFact]
    public virtual void Detects_enum_with_database_default_not_set_to_CLR_default()
    {
        var modelBuilder = CreateConventionModelBuilder();
        var model = modelBuilder.Model;

        var entityType = model.AddEntityType(typeof(WithEnum));
        entityType.FindProperty(nameof(WithEnum.EnumWithDefaultConstraint))!.SetDefaultValue(X.B);
        entityType.FindProperty(nameof(WithEnum.NullableEnum))!.SetDefaultValue(X.B);

        Validate(modelBuilder);

        VerifyWarning(
            RelationalResources.LogBoolWithDefaultWarning(new TestLogger<TestRelationalLoggingDefinitions>())
                .GenerateMessage("X", "EnumWithDefaultConstraint", "WithEnum", "0", "X"), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Enum_with_database_default_not_set_to_CLR_default_okay_if_sentinel_set()
    {
        var modelBuilder = CreateConventionModelBuilder();
        var model = modelBuilder.Model;

        var entityType = model.AddEntityType(typeof(WithEnum));
        var property = entityType.FindProperty(nameof(WithEnum.EnumWithDefaultConstraint))!;
        property.SetDefaultValue(X.B);
        property.Sentinel = X.B;
        entityType.FindProperty(nameof(WithEnum.NullableEnum))!.SetDefaultValue(X.B);

        Validate(modelBuilder);

        Assert.DoesNotContain(LoggerFactory.Log, l => l.Level == LogLevel.Warning);
    }

    [ConditionalFact]
    public virtual void Enum_with_database_default_not_set_to_CLR_default_and_nullable_backing_field_is_fine()
    {
        var modelBuilder = CreateConventionModelBuilder();
        var model = modelBuilder.Model;

        var entityType = model.AddEntityType(typeof(WithEnum2));
        entityType.FindProperty(nameof(WithEnum2.EnumWithDefaultConstraint))!.SetDefaultValue(X.B);

        Validate(modelBuilder);

        Assert.DoesNotContain(LoggerFactory.Log, l => l.Level == LogLevel.Warning);
    }

    [ConditionalFact]
    public virtual void Detects_enum_with_default_expression()
    {
        var modelBuilder = CreateConventionModelBuilder();
        var model = modelBuilder.Model;

        var entityType = model.AddEntityType(typeof(WithEnum));
        entityType.FindProperty(nameof(WithEnum.EnumWithDefaultConstraint))!.SetDefaultValueSql("SQL");
        entityType.FindProperty(nameof(WithEnum.NullableEnum))!.SetDefaultValueSql("SQL");

        Validate(modelBuilder);

        VerifyWarning(
            RelationalResources.LogBoolWithDefaultWarning(new TestLogger<TestRelationalLoggingDefinitions>())
                .GenerateMessage("X", "EnumWithDefaultConstraint", "WithEnum", "0", "X"), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_primary_key_with_default_value()
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        var model = modelBuilder.Model;

        var entityA = model.AddEntityType(typeof(A));
        SetPrimaryKey(entityA);
        entityA.FindProperty("Id")!.SetDefaultValue(1);
        AddProperties(entityA);

        entityA.FindProperty("Id")!.SetDefaultValue(1);

        VerifyWarning(
            RelationalResources.LogKeyHasDefaultValue(
                new TestLogger<TestRelationalLoggingDefinitions>()).GenerateMessage("Id", "A"), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_alternate_key_with_default_value()
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        var model = modelBuilder.Model;

        var entityA = model.AddEntityType(typeof(A));
        SetPrimaryKey(entityA);
        entityA.AddProperty(nameof(A.P1), typeof(int?));
        entityA.AddProperty(nameof(A.P2), typeof(int?));
        entityA.AddProperty(nameof(A.P3), typeof(int?));

        var property = entityA.AddProperty("P0", typeof(int?));
        property.IsNullable = false;
        entityA.AddKey(new[] { property });
        property.SetDefaultValue(1);

        VerifyWarning(
            RelationalResources.LogKeyHasDefaultValue(new TestLogger<TestRelationalLoggingDefinitions>()).GenerateMessage("P0", "A"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_duplicate_table_names_without_identifying_relationship()
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        var model = modelBuilder.Model;

        var entityA = model.AddEntityType(typeof(A));
        SetPrimaryKey(entityA);
        AddProperties(entityA);

        var entityB = model.AddEntityType(typeof(B));
        SetPrimaryKey(entityB);
        AddProperties(entityB);
        entityB.AddIgnored(nameof(B.A));
        entityB.AddIgnored(nameof(B.AnotherA));
        entityB.AddIgnored(nameof(B.ManyAs));

        entityA.SetTableName("Table");
        entityA.SetSchema("Schema");
        entityB.SetTableName("Table");
        entityB.SetSchema("Schema");

        VerifyError(
            RelationalStrings.IncompatibleTableNoRelationship(
                "Schema.Table", entityB.DisplayName(), entityA.DisplayName()),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_duplicate_table_names_when_no_key()
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        var model = modelBuilder.Model;

        var entityA = model.AddEntityType(typeof(A));
        entityA.AddProperty("Id", typeof(int));
        entityA.IsKeyless = true;
        AddProperties(entityA);

        var entityB = model.AddEntityType(typeof(B));
        entityB.AddProperty("Id", typeof(int));
        entityB.IsKeyless = true;
        AddProperties(entityB);
        entityB.AddIgnored(nameof(B.A));
        entityB.AddIgnored(nameof(B.AnotherA));
        entityB.AddIgnored(nameof(B.ManyAs));

        entityA.SetTableName("Table");
        entityB.SetTableName("Table");

        VerifyError(
            RelationalStrings.IncompatibleTableNoRelationship(
                "Table", entityB.DisplayName(), entityA.DisplayName()),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_duplicate_view_names_without_identifying_relationship()
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        var model = modelBuilder.Model;

        var entityA = model.AddEntityType(typeof(A));
        SetPrimaryKey(entityA);
        AddProperties(entityA);

        var entityB = model.AddEntityType(typeof(B));
        SetPrimaryKey(entityB);
        AddProperties(entityB);
        entityB.AddIgnored(nameof(B.A));
        entityB.AddIgnored(nameof(B.AnotherA));
        entityB.AddIgnored(nameof(B.ManyAs));

        entityA.SetViewName("Table");
        entityA.SetViewSchema("Schema");
        entityB.SetViewName("Table");
        entityB.SetViewSchema("Schema");

        VerifyError(
            RelationalStrings.IncompatibleViewNoRelationship(
                "Schema.Table", entityB.DisplayName(), entityA.DisplayName()),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_duplicate_view_names_when_no_key()
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        var model = modelBuilder.Model;

        var entityA = model.AddEntityType(typeof(A));
        entityA.AddProperty("Id", typeof(int));
        entityA.IsKeyless = true;
        AddProperties(entityA);

        var entityB = model.AddEntityType(typeof(B));
        entityB.AddProperty("Id", typeof(int));
        entityB.IsKeyless = true;
        AddProperties(entityB);
        entityB.AddIgnored(nameof(B.A));
        entityB.AddIgnored(nameof(B.AnotherA));
        entityB.AddIgnored(nameof(B.ManyAs));

        entityA.SetViewName("Table");
        entityB.SetViewName("Table");

        VerifyError(
            RelationalStrings.IncompatibleViewNoRelationship(
                "Table", entityB.DisplayName(), entityA.DisplayName()),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_for_duplicate_table_names_in_different_schema()
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        var model = modelBuilder.Model;

        var entityA = model.AddEntityType(typeof(A));
        SetPrimaryKey(entityA);
        AddProperties(entityA);

        var entityB = model.AddEntityType(typeof(B));
        SetPrimaryKey(entityB);
        AddProperties(entityB);
        entityB.AddIgnored(nameof(B.A));
        entityB.AddIgnored(nameof(B.AnotherA));
        entityB.AddIgnored(nameof(B.ManyAs));
        entityA.SetTableName("Table");
        entityA.SetSchema("SchemaA");
        entityB.SetTableName("Table");
        entityB.SetSchema("SchemaB");

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_for_duplicate_table_names_for_inherited_entities()
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        var model = modelBuilder.Model;

        var entityA = model.AddEntityType(typeof(A));
        SetPrimaryKey(entityA);
        AddProperties(entityA);

        var entityC = model.AddEntityType(typeof(C));
        SetBaseType(entityC, entityA);

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_incompatible_primary_keys_with_shared_table()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<A>().HasOne<B>().WithOne(b => b.A).HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id).IsRequired();
        modelBuilder.Entity<A>().HasKey(a => a.Id).HasName("Key");
        modelBuilder.Entity<A>().ToTable("Table");
        modelBuilder.Entity<B>().ToTable("Table");

        VerifyError(
            RelationalStrings.IncompatibleTableKeyNameMismatch(
                "Table", nameof(B), nameof(A), "PK_Table", "{'Id'}", "Key", "{'Id'}"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_incompatible_comments_with_shared_table()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<A>().HasOne<B>().WithOne(b => b.A).HasPrincipalKey<A>(a => a.Id).HasForeignKey<B>(b => b.Id).IsRequired();
        modelBuilder.Entity<A>().ToTable("Table", tb => tb.HasComment("My comment"));
        modelBuilder.Entity<B>().ToTable("Table", tb => tb.HasComment("my comment"));

        VerifyError(
            RelationalStrings.IncompatibleTableCommentMismatch(
                "Table", nameof(A), nameof(B), "My comment", "my comment"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_on_null_comments()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<A>().HasOne<B>().WithOne(b => b.A).HasPrincipalKey<A>(a => a.Id).HasForeignKey<B>(b => b.Id).IsRequired();
        modelBuilder.Entity<A>().ToTable("Table", tb => tb.HasComment("My comment"));
        modelBuilder.Entity<B>().ToTable("Table");

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_incompatible_primary_key_columns_with_shared_table()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<A>().HasOne<B>().WithOne(b => b.A).HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id).IsRequired();
        modelBuilder.Entity<A>().Property(a => a.Id).ValueGeneratedNever().HasColumnName("Key");
        modelBuilder.Entity<A>().ToTable("Table");
        modelBuilder.Entity<B>().Property(a => a.Id).ValueGeneratedNever().HasColumnName(nameof(B.Id));
        modelBuilder.Entity<B>().ToTable("Table");

        VerifyError(
            RelationalStrings.DuplicateKeyColumnMismatch(
                "{'Id'}", nameof(B), "{'Id'}", nameof(A), "Table", "PK_Table", "{'Id'}", "{'Key'}"), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_on_shared_columns_with_shared_table()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<A>().HasOne<B>().WithOne(b => b.A).HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id).IsRequired();
        modelBuilder.Entity<A>().Property(a => a.P0).HasColumnName(nameof(A.P0));
        modelBuilder.Entity<A>().Property(a => a.P3).HasColumnName(nameof(A.P3))
            .HasConversion(e => (long?)e, e => (int?)e);
        modelBuilder.Entity<A>().Property(a => a.P1).IsRequired();
        modelBuilder.Entity<A>().ToTable("Table");
        modelBuilder.Entity<B>().Property(b => b.P0).HasColumnName(nameof(A.P0)).HasColumnType("someInt");
        modelBuilder.Entity<B>().Property(b => b.P3).HasColumnName(nameof(A.P3))
            .HasConversion(e => (long)e, e => (int?)e);
        modelBuilder.Entity<B>().ToTable("Table");

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Throws_on_nullable_shared_columns_with_shared_table_with_dependents()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<A>().HasOne<B>().WithOne().HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id).IsRequired();
        modelBuilder.Entity<A>().Property(a => a.P0).HasColumnName(nameof(A.P0));
        modelBuilder.Entity<A>().ToTable("Table");
        modelBuilder.Entity<B>().Property(b => b.P0).HasColumnName(nameof(A.P0)).HasColumnType("someInt");
        modelBuilder.Entity<B>().ToTable("Table");

        VerifyError(RelationalStrings.OptionalDependentWithDependentWithoutIdentifyingProperty(nameof(A)), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Warns_on_no_required_columns_with_shared_table()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<Owner>().OwnsOne(e => e.Owned);

        var definition =
            RelationalResources.LogOptionalDependentWithoutIdentifyingProperty(new TestLogger<TestRelationalLoggingDefinitions>());
        VerifyWarning(definition.GenerateMessage(nameof(OwnedEntity)), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_incompatible_shared_columns_in_shared_table_with_different_data_types()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<A>().HasOne<B>().WithOne(b => b.A).HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id).IsRequired();
        modelBuilder.Entity<A>().Property(a => a.P0).HasColumnName(nameof(A.P0)).HasColumnType("someInt");
        modelBuilder.Entity<A>().ToTable("Table");
        modelBuilder.Entity<B>().Property(b => b.P0).HasColumnName(nameof(A.P0)).HasColumnType("default_int_mapping");
        modelBuilder.Entity<B>().ToTable("Table");

        VerifyError(
            RelationalStrings.DuplicateColumnNameDataTypeMismatch(
                nameof(A), nameof(A.P0), nameof(B), nameof(B.P0), nameof(B.P0), "Table", "someInt", "default_int_mapping"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_properties_mapped_to_the_same_column_within_hierarchy()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<A>().Property(a => a.P0).HasColumnName(nameof(A.P0));
        modelBuilder.Entity<C>().Property<int?>("PC").HasColumnName(nameof(A.P0));

        VerifyError(
            RelationalStrings.DuplicateColumnNameSameHierarchy(
                nameof(A), nameof(A.P0), nameof(C), "PC", nameof(A.P0), nameof(A)),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_for_incompatible_shared_columns_in_shared_table_with_different_provider_types()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<A>().HasOne<B>().WithOne(b => b.A).HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id).IsRequired();
        modelBuilder.Entity<A>().Property(a => a.P0).HasColumnName(nameof(A.P0)).HasColumnType("someInt").HasConversion<long>();
        modelBuilder.Entity<A>().ToTable("Table");
        modelBuilder.Entity<B>().Property(b => b.P0).HasColumnName(nameof(A.P0)).HasColumnType("someInt");
        modelBuilder.Entity<B>().ToTable("Table");

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_incompatible_shared_columns_in_shared_table_with_different_provider_types_for_unique_indexes()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<A>().HasOne<B>().WithOne(b => b.A).HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id).IsRequired();
        modelBuilder.Entity<A>().Property(a => a.P0).HasColumnName(nameof(A.P0)).HasColumnType("someInt").HasConversion<long>();
        modelBuilder.Entity<A>().ToTable("Table");
        modelBuilder.Entity<B>().Property(b => b.P0).HasColumnName(nameof(A.P0)).HasColumnType("someInt");
        modelBuilder.Entity<B>().ToTable("Table");
        modelBuilder.Entity<A>().HasIndex(a => a.P0).IsUnique();

        VerifyError(
            RelationalStrings.DuplicateColumnNameProviderTypeMismatch(
                nameof(A), nameof(A.P0), nameof(B), nameof(B.P0), nameof(B.P0), "Table", "long", "int"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_incompatible_shared_columns_in_shared_table_with_different_provider_types_for_keys()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<A>().HasOne<B>().WithOne(b => b.A).HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id).IsRequired();
        modelBuilder.Entity<A>().Property(a => a.P0).HasColumnName(nameof(A.P0)).HasColumnType("someInt").HasConversion<long>();
        modelBuilder.Entity<A>().ToTable("Table");
        modelBuilder.Entity<B>().Property(b => b.P0).HasColumnName(nameof(A.P0)).HasColumnType("someInt");
        modelBuilder.Entity<B>().ToTable("Table");
        modelBuilder.Entity<A>().HasAlternateKey(a => a.P0);

        VerifyError(
            RelationalStrings.DuplicateColumnNameProviderTypeMismatch(
                nameof(A), nameof(A.P0), nameof(B), nameof(B.P0), nameof(B.P0), "Table", "long", "int"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_incompatible_shared_columns_in_shared_table_with_different_provider_types_for_foreign_keys()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<A>().HasOne<B>().WithOne(b => b.A).HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id).IsRequired();
        modelBuilder.Entity<A>().Property(a => a.P0).HasColumnName(nameof(A.P0)).HasColumnType("someInt").HasConversion<long>();
        modelBuilder.Entity<A>().ToTable("Table");
        modelBuilder.Entity<B>().Property(b => b.P0).HasColumnName(nameof(A.P0)).HasColumnType("someInt");
        modelBuilder.Entity<B>().ToTable("Table");
        modelBuilder.Entity<A>().HasOne<B>().WithOne().HasForeignKey<A>(a => a.P0).HasPrincipalKey<B>(b => b.Id);

        VerifyError(
            RelationalStrings.DuplicateColumnNameProviderTypeMismatch(
                nameof(A), nameof(A.P0), nameof(B), nameof(B.P0), nameof(B.P0), "Table", "long", "int"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_incompatible_shared_check_constraints_with_shared_table()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<A>().HasOne<B>().WithOne(b => b.A).HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id).IsRequired();
        modelBuilder.Entity<A>().ToTable("Table", tb => tb.HasCheckConstraint("SomeCK", "Id > 0").HasName("CK_Table_SomeCK"));
        modelBuilder.Entity<B>().ToTable("Table", tb => tb.HasCheckConstraint("SomeOtherCK", "Id > 10").HasName("CK_Table_SomeCK"));

        VerifyError(
            RelationalStrings.DuplicateCheckConstraintSqlMismatch(
                "SomeOtherCK", nameof(B), "SomeCK", nameof(A), "CK_Table_SomeCK"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_for_incompatible_uniquified_check_constraints_with_shared_table()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<A>().HasOne<B>().WithOne(b => b.A).HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id).IsRequired();
        modelBuilder.Entity<A>().ToTable("Table", tb => tb.HasCheckConstraint("CK_Table_SomeCK", "Id > 0"));
        modelBuilder.Entity<B>().ToTable("Table", tb => tb.HasCheckConstraint("CK_Table_SomeCK", "Id > 10"));

        var model = Validate(modelBuilder);

        Assert.Equal("CK_Table_SomeCK1", model.FindEntityType(typeof(A)).GetCheckConstraints().Single().Name);
        Assert.Equal("CK_Table_SomeCK", model.FindEntityType(typeof(B)).GetCheckConstraints().Single().Name);
    }

    [ConditionalFact]
    public virtual void Passes_for_compatible_shared_check_constraints_with_shared_table()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<A>().HasOne<B>().WithOne(b => b.A).HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id).IsRequired();
        modelBuilder.Entity<A>().ToTable("Table", tb => tb.HasCheckConstraint("CK_Table_SomeCK", "Id > 0"));
        modelBuilder.Entity<B>().ToTable("Table", tb => tb.HasCheckConstraint("CK_Table_SomeCK", "Id > 0"));

        var model = Validate(modelBuilder);

        Assert.Equal("CK_Table_SomeCK", model.FindEntityType(typeof(A)).GetCheckConstraints().Single().Name);
        Assert.Equal("CK_Table_SomeCK", model.FindEntityType(typeof(B)).GetCheckConstraints().Single().Name);
    }

    [ConditionalFact]
    public virtual void Detects_multiple_shared_table_roots()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<A>().HasOne<B>().WithOne().HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id).IsRequired();
        modelBuilder.Entity<A>().ToTable("Table");
        modelBuilder.Entity<A>().HasOne<C>().WithOne().HasForeignKey<A>(a => a.Id).HasPrincipalKey<C>(b => b.Id).IsRequired();
        modelBuilder.Entity<C>().HasBaseType((string)null).ToTable("Table");
        modelBuilder.Entity<B>().ToTable("Table");

        VerifyError(
            RelationalStrings.IncompatibleTableNoRelationship("Table", nameof(C), nameof(B)),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_shared_table_root_cycle()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<A>().HasOne<B>().WithOne().HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id).IsRequired();
        modelBuilder.Entity<A>().ToTable("Table");
        modelBuilder.Entity<A>().HasOne<B>().WithOne().HasForeignKey<B>(a => a.Id).HasPrincipalKey<A>(b => b.Id).IsRequired();
        modelBuilder.Entity<B>().ToTable("Table");

        VerifyError(CoreStrings.IdentifyingRelationshipCycle("A -> B"), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_for_compatible_shared_table()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<A>().HasOne<B>().WithOne().HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id).IsRequired();

        modelBuilder.Entity<A>().ToTable("Table").Property(e => e.P0).IsRequired();
        modelBuilder.Entity<B>(
            b =>
            {
                b.ToTable("Table");
                b.Property(bb => bb.Id)
                    .HasColumnName("Key")
                    .HasColumnType("someInt")
                    .HasDefaultValueSql("NEXT value");

                b.HasKey(bb => bb.Id)
                    .HasName("Key");
            });

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_for_compatible_excluded_shared_table_inverted()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<A>().HasOne<B>().WithOne().HasPrincipalKey<A>(a => a.Id).HasForeignKey<B>(b => b.Id).IsRequired();
        modelBuilder.Entity<A>().ToTable("Table", t => t.ExcludeFromMigrations());
        modelBuilder.Entity<B>().ToTable("Table", t => t.ExcludeFromMigrations());

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_for_compatible_excluded_shared_table_owned()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<B>().OwnsOne(b => b.A);
        modelBuilder.Entity<B>().ToTable("Table", t => t.ExcludeFromMigrations());

        var model = Validate(modelBuilder);

        var b = model.FindEntityType(typeof(B));
        Assert.Equal("Table", b.GetTableName());
        Assert.True(b.IsTableExcludedFromMigrations());
    }

    [ConditionalFact]
    public virtual void Passes_for_compatible_excluded_table_derived()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<A>().ToTable("Table", t => t.ExcludeFromMigrations());
        modelBuilder.Entity<C>();

        var model = Validate(modelBuilder);

        var c = model.FindEntityType(typeof(C));
        Assert.Equal("Table", c.GetTableName());
        Assert.True(c.IsTableExcludedFromMigrations());
    }

    [ConditionalFact]
    public virtual void Detect_partially_excluded_shared_table()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<A>().HasOne<B>().WithOne().HasPrincipalKey<A>(a => a.Id).HasForeignKey<B>(b => b.Id).IsRequired();
        modelBuilder.Entity<A>().ToTable("Table", t => t.ExcludeFromMigrations());
        modelBuilder.Entity<B>().ToTable("Table");

        VerifyError(
            RelationalStrings.IncompatibleTableExcludedMismatch(
                nameof(Table), nameof(A), nameof(B)),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_entity_splitting_on_base_type()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>().ToView("Animal").SplitToView("AnimalDetails", s => s.Property(a => a.Name));
        modelBuilder.Entity<Cat>().ToView("Cat");

        VerifyError(
            RelationalStrings.EntitySplittingHierarchy(nameof(Animal), "AnimalDetails"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_entity_splitting_on_derived_type()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>().ToTable("Animal");
        modelBuilder.Entity<Cat>().ToTable("Cat").SplitToTable("CatDetails", s => s.Property(a => a.Name));

        VerifyError(
            RelationalStrings.EntitySplittingHierarchy(nameof(Cat), "CatDetails"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_entity_splitting_with_unmapped_main()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>().SplitToView("AnimalDetails", s => s.Property(a => a.Name));

        VerifyError(
            RelationalStrings.EntitySplittingUnmappedMainFragment(nameof(Animal), "AnimalDetails", "View"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_entity_splitting_to_with_conflicting_main()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>().ToTable("Animal").SplitToTable("Animal", s => s.Property(a => a.Name));

        VerifyError(
            RelationalStrings.EntitySplittingConflictingMainFragment(nameof(Animal), "Animal"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_entity_splitting_with_unmapped_PK()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>().SplitToTable("AnimalDetails", s => s.Property(a => a.Id).HasColumnName(null));

        VerifyError(
            RelationalStrings.EntitySplittingMissingPrimaryKey(nameof(Animal), "AnimalDetails"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_entity_splitting_without_properties()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>().SplitToTable("AnimalDetails", s => { });

        VerifyError(
            RelationalStrings.EntitySplittingMissingProperties(nameof(Animal), "AnimalDetails"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_entity_splitting_to_table_with_all_properties()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>().SplitToTable(
            "AnimalDetails", s =>
            {
                s.Property(a => a.Name);
                s.Property("FavoritePersonId");
            });

        VerifyError(
            RelationalStrings.EntitySplittingMissingPropertiesMainFragment(nameof(Animal), "Animal"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_entity_splitting_to_view_with_all_properties()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>().ToView("Animal").SplitToView(
            "AnimalDetails", s =>
            {
                s.Property(a => a.Name);
                s.Property("FavoritePersonId");
            });

        VerifyError(
            RelationalStrings.EntitySplittingMissingPropertiesMainFragment(nameof(Animal), "Animal"),
            modelBuilder);
    }

    [ConditionalFact]
    public void Detects_entity_splitting_with_optional_table_splitting_without_required_properties()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<Order>(
            cb =>
            {
                cb.Ignore(c => c.Customer);

                cb.ToTable("Order");

                cb.SplitToTable(
                    "OrderDetails", tb =>
                    {
                        tb.Property(c => c.PartitionId);
                    });

                cb.OwnsOne(
                    c => c.OrderDetails, db =>
                    {
                        db.ToTable("Order");

                        db.Property<string>("OtherAddress");
                        db.SplitToTable(
                            "Details", tb =>
                            {
                                tb.Property("OtherAddress");
                            });
                    });
            });

        VerifyError(
            RelationalStrings.EntitySplittingMissingRequiredPropertiesOptionalDependent(
                nameof(OrderDetails), "Order", ".Navigation(p => p.OrderDetails).IsRequired()"),
            modelBuilder);
    }

    [ConditionalFact]
    public void Detects_entity_splitting_with_partial_table_splitting()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<Order>(
            cb =>
            {
                cb.Ignore(c => c.Customer);

                cb.ToTable("Order");

                cb.SplitToTable(
                    "OrderDetails", tb =>
                    {
                        tb.Property(c => c.PartitionId);
                    });

                cb.OwnsOne(
                    c => c.OrderDetails, db =>
                    {
                        db.ToTable("Details");

                        db.Property<string>("OtherAddress");
                        db.SplitToTable(
                            "Order", tb =>
                            {
                                tb.Property("OtherAddress");
                            });
                    });
                cb.Navigation(c => c.OrderDetails).IsRequired();
            });

        VerifyError(
            RelationalStrings.EntitySplittingUnmatchedMainTableSplitting(nameof(OrderDetails), "Order", nameof(Order), "Order"),
            modelBuilder);
    }

    [ConditionalFact]
    public void Detects_entity_splitting_with_reverse_table_splitting()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<Order>(
            cb =>
            {
                cb.Ignore(c => c.Customer);

                cb.ToTable("Order");

                cb.SplitToTable(
                    "OrderDetails", tb =>
                    {
                        tb.Property(c => c.PartitionId);
                    });

                cb.OwnsOne(
                    c => c.OrderDetails, db =>
                    {
                        db.ToTable("OrderDetails");

                        db.Property<string>("OtherAddress");
                        db.SplitToTable(
                            "Order", tb =>
                            {
                                tb.Property("OtherAddress");
                            });
                    });
                cb.Navigation(c => c.OrderDetails).IsRequired();
            });

        VerifyError(
            RelationalStrings.EntitySplittingUnmatchedMainTableSplitting(nameof(OrderDetails), "Order", nameof(Order), "Order"),
            modelBuilder);
    }

    [ConditionalFact]
    public void Detects_unnamed_index_properties_mapped_to_different_fragments_in_entity_splitting()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<Cat>()
            .ToTable("Cats")
            .SplitToTable("Animals", s => s.Property(c => c.Name));
        modelBuilder.Entity<Cat>().HasIndex(nameof(Animal.Name), nameof(Cat.Identity));

        var definition = RelationalResources
            .LogUnnamedIndexPropertiesMappedToNonOverlappingTables(
                new TestLogger<TestRelationalLoggingDefinitions>());
        VerifyWarning(
            definition.GenerateMessage(
                nameof(Cat),
                "{'Name', 'Identity'}",
                nameof(Animal.Name),
                "{'Animals'}",
                nameof(Cat.Identity),
                "{'Cats'}"),
            modelBuilder,
            LogLevel.Error);
    }

    [ConditionalFact]
    public void Detects_unnamed_key_properties_mapped_to_different_fragments_in_entity_splitting()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<Cat>()
            .ToTable("Cats")
            .SplitToTable("Animals", s => s.Property(c => c.Name));
        modelBuilder.Entity<Cat>().HasAlternateKey(nameof(Animal.Name), nameof(Cat.Identity));

        var definition = RelationalResources
            .LogKeyPropertiesNotMappedToTable(new TestLogger<TestRelationalLoggingDefinitions>());
        VerifyWarning(
            definition.GenerateMessage(
                "{'Name', 'Identity'}",
                nameof(Cat),
                "Cats"),
            modelBuilder,
            LogLevel.Error);
    }

    [ConditionalFact]
    public virtual void Detects_unmapped_foreign_keys_in_entity_splitting()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Person>(
            pb =>
            {
                pb.HasKey(p => new { p.Id, p.Name });
            });
        modelBuilder.Entity<Cat>().ToTable("Cat")
            .HasOne<Person>().WithMany()
            .HasForeignKey("FavoritePersonId", "FavoritePersonName");

        modelBuilder.Entity<Cat>()
            .SplitToTable("Animals", s => s.Property("FavoritePersonName"));

        var definition =
            RelationalResources.LogForeignKeyPropertiesMappedToUnrelatedTables(new TestLogger<TestRelationalLoggingDefinitions>());
        VerifyWarning(
            definition.GenerateMessage(
                l => l.Log(
                    definition.Level,
                    definition.EventId,
                    definition.MessageFormat,
                    "{'FavoritePersonId', 'FavoritePersonName'}",
                    nameof(Cat),
                    nameof(Person),
                    "{'FavoritePersonId', 'FavoritePersonName'}",
                    nameof(Cat),
                    "{'Id', 'Name'}",
                    nameof(Person))),
            modelBuilder,
            LogLevel.Error);
    }

    [ConditionalFact]
    public virtual void Detects_duplicate_columns_in_derived_types_with_different_types()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>();

        modelBuilder.Entity<Cat>().Property(c => c.Type).HasColumnName("Type").HasColumnType("someInt");
        modelBuilder.Entity<Dog>().Property(d => d.Type).HasColumnName("Type").HasColumnType("default_int_mapping");

        VerifyError(
            RelationalStrings.DuplicateColumnNameDataTypeMismatch(
                nameof(Cat), nameof(Cat.Type), nameof(Dog), nameof(Dog.Type), nameof(Cat.Type), nameof(Animal), "someInt",
                "default_int_mapping"), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_duplicate_column_names_within_hierarchy_with_different_MaxLength()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>();

        modelBuilder.Entity<Cat>().Property(c => c.Breed).HasColumnName("Breed").HasMaxLength(30);
        modelBuilder.Entity<Dog>().Property(d => d.Breed).HasColumnName("Breed").HasMaxLength(15);

        VerifyError(
            RelationalStrings.DuplicateColumnNameMaxLengthMismatch(
                nameof(Cat), nameof(Cat.Breed), nameof(Dog), nameof(Dog.Breed), nameof(Cat.Breed), nameof(Animal), "30",
                "15"), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_duplicate_column_names_within_hierarchy_with_different_IsUnicode()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>();

        modelBuilder.Entity<Cat>().Property(c => c.Breed).HasColumnName("Breed").IsUnicode();
        modelBuilder.Entity<Dog>().Property(d => d.Breed).HasColumnName("Breed");

        VerifyError(
            RelationalStrings.DuplicateColumnNameUnicodenessMismatch(
                nameof(Cat), nameof(Cat.Breed), nameof(Dog), nameof(Dog.Breed), nameof(Cat.Breed), nameof(Animal)), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_duplicate_column_names_within_hierarchy_with_different_IsFixedLength()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>();

        modelBuilder.Entity<Cat>().Property(c => c.Breed).HasColumnName("Breed").IsFixedLength();
        modelBuilder.Entity<Dog>().Property(d => d.Breed).HasColumnName("Breed");

        VerifyError(
            RelationalStrings.DuplicateColumnNameFixedLengthMismatch(
                nameof(Cat), nameof(Cat.Breed), nameof(Dog), nameof(Dog.Breed), nameof(Cat.Breed), nameof(Animal)), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_duplicate_column_names_within_hierarchy_with_different_IsConcurrencyToken()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>();

        modelBuilder.Entity<Cat>().Property(c => c.Breed).HasColumnName("Breed").IsConcurrencyToken();
        modelBuilder.Entity<Dog>().Property(d => d.Breed).HasColumnName("Breed");

        VerifyError(
            RelationalStrings.DuplicateColumnNameConcurrencyTokenMismatch(
                nameof(Cat), nameof(Cat.Breed),
                nameof(Dog), nameof(Dog.Breed),
                nameof(Cat.Breed), nameof(Animal)),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_duplicate_column_names_within_hierarchy_with_different_ComputedColumnSql()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>();
        modelBuilder.Entity<Cat>().Property(c => c.Breed).HasColumnName("Breed").HasComputedColumnSql("1");
        modelBuilder.Entity<Dog>().Property(c => c.Breed).HasColumnName("Breed");

        VerifyError(
            RelationalStrings.DuplicateColumnNameComputedSqlMismatch(
                nameof(Cat), nameof(Cat.Breed), nameof(Dog), nameof(Dog.Breed), nameof(Cat.Breed), nameof(Animal), "1", ""),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_duplicate_column_names_within_hierarchy_with_different_stored_setting()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>();
        modelBuilder.Entity<Cat>().Property(c => c.Breed).HasColumnName("Breed").HasComputedColumnSql("1", true);
        modelBuilder.Entity<Dog>().Property(c => c.Breed).HasColumnName("Breed").HasComputedColumnSql("1");

        VerifyError(
            RelationalStrings.DuplicateColumnNameIsStoredMismatch(
                nameof(Cat), nameof(Cat.Breed), nameof(Dog), nameof(Dog.Breed), nameof(Cat.Breed), nameof(Animal), "True", ""),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_duplicate_column_names_within_hierarchy_with_different_DefaultValue()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>();
        modelBuilder.Entity<Cat>().Property(c => c.Breed).HasColumnName("Breed").HasDefaultValueSql("1");
        modelBuilder.Entity<Dog>().Property(c => c.Breed).HasColumnName("Breed").HasDefaultValue("1");

        VerifyError(
            RelationalStrings.DuplicateColumnNameDefaultSqlMismatch(
                nameof(Cat), nameof(Cat.Breed), nameof(Dog), nameof(Dog.Breed), nameof(Cat.Breed), nameof(Animal), "NULL", "1"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_duplicate_column_names_within_hierarchy_with_different_DefaultValueSql()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>();
        modelBuilder.Entity<Cat>().Property(c => c.Breed).HasColumnName("Breed").HasDefaultValueSql("1");
        modelBuilder.Entity<Dog>().Property(c => c.Breed).HasColumnName("Breed");

        VerifyError(
            RelationalStrings.DuplicateColumnNameDefaultSqlMismatch(
                nameof(Cat), nameof(Cat.Breed), nameof(Dog), nameof(Dog.Breed), nameof(Cat.Breed), nameof(Animal), "1", ""),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_duplicate_column_names_with_different_column_nullability()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<A>().HasOne<B>().WithOne(b => b.A).HasForeignKey<B>(a => a.Id).HasPrincipalKey<A>(b => b.Id);
        modelBuilder.Entity<A>().HasOne<G>().WithOne(g => g.A).HasForeignKey<G>(a => a.Id).HasPrincipalKey<A>(b => b.Id)
            .Metadata.IsRequiredDependent = true;
        modelBuilder.Entity<A>().ToTable("Table").Ignore(a => a.P0);
        modelBuilder.Entity<B>().ToTable("Table").Property(b => b.P0).HasColumnName(nameof(A.P0));
        modelBuilder.Entity<G>().ToTable("Table").Property(g => g.P0).HasColumnName(nameof(A.P0)).IsRequired();

        VerifyError(
            RelationalStrings.DuplicateColumnNameNullabilityMismatch(
                nameof(B), nameof(B.P0), nameof(G), nameof(G.P0), nameof(A.P0), "Table"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_on_duplicate_column_names_within_hierarchy_with_same_column_nullability()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>();
        modelBuilder.Entity<Cat>().Property<int>("OtherId").HasColumnName("OtherId");
        modelBuilder.Entity<Dog>().Property<int?>("OtherId").HasColumnName("OtherId");

        var model = Validate(modelBuilder);

        var column = model.FindEntityType(typeof(Cat)).FindProperty("OtherId").GetTableColumnMappings().Single().Column;

        Assert.Equal(2, column.PropertyMappings.Count());
        Assert.True(column.IsNullable);
        Assert.Null(column.DefaultValue);
    }

    [ConditionalFact]
    public virtual void Detects_duplicate_column_names_within_hierarchy_with_different_comments()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>();
        modelBuilder.Entity<Cat>().Property(c => c.Breed).HasColumnName("Breed").HasComment("My comment");
        modelBuilder.Entity<Dog>().Property(c => c.Breed).HasColumnName("Breed");

        VerifyError(
            RelationalStrings.DuplicateColumnNameCommentMismatch(
                nameof(Cat), nameof(Cat.Breed), nameof(Dog), nameof(Dog.Breed), nameof(Cat.Breed), nameof(Animal), "My comment", ""),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_duplicate_column_names_within_hierarchy_with_different_collations()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>();
        modelBuilder.Entity<Cat>().Property(c => c.Breed).HasColumnName("Breed").UseCollation("UTF8");
        modelBuilder.Entity<Dog>().Property(c => c.Breed).HasColumnName("Breed");

        VerifyError(
            RelationalStrings.DuplicateColumnNameCollationMismatch(
                nameof(Cat), nameof(Cat.Breed), nameof(Dog), nameof(Dog.Breed), nameof(Cat.Breed), nameof(Animal), "UTF8", ""),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_duplicate_column_names_within_hierarchy_with_different_orders()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>();
        modelBuilder.Entity<Cat>().Property(c => c.Breed).HasColumnName("Breed").HasColumnOrder(0);
        modelBuilder.Entity<Dog>().Property(c => c.Breed).HasColumnName("Breed");

        VerifyError(
            RelationalStrings.DuplicateColumnNameOrderMismatch(
                nameof(Cat), nameof(Cat.Breed), nameof(Dog), nameof(Dog.Breed), nameof(Cat.Breed), nameof(Animal), 0, null),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_duplicate_column_names_within_hierarchy_with_different_precision()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>();
        modelBuilder.Entity<Cat>().Property(c => c.Breed).HasColumnName("Breed").HasPrecision(1);
        modelBuilder.Entity<Dog>().Property(c => c.Breed).HasColumnName("Breed");

        VerifyError(
            RelationalStrings.DuplicateColumnNamePrecisionMismatch(
                nameof(Cat), nameof(Cat.Breed), nameof(Dog), nameof(Dog.Breed), nameof(Cat.Breed), nameof(Animal), "", "1"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_duplicate_column_names_within_hierarchy_with_different_scale()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>();
        modelBuilder.Entity<Cat>().Property(c => c.Breed).HasColumnName("Breed").HasPrecision(1, 2);
        modelBuilder.Entity<Dog>().Property(c => c.Breed).HasColumnName("Breed").HasPrecision(1);

        VerifyError(
            RelationalStrings.DuplicateColumnNameScaleMismatch(
                nameof(Cat), nameof(Cat.Breed), nameof(Dog), nameof(Dog.Breed), nameof(Cat.Breed), nameof(Animal), "", "2"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_for_compatible_duplicate_column_names_within_hierarchy()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>();
        modelBuilder.Entity<Cat>(
            eb =>
            {
                eb.Ignore(e => e.Type);
                eb.Property(c => c.Breed).HasMaxLength(25);
                eb.Property(c => c.Breed).HasColumnName("BreedName");
                eb.Property(c => c.Breed).HasDefaultValue("None");
                eb.Property<bool>("Selected").HasDefaultValue(false);
            });
        modelBuilder.Entity<Dog>(
            eb =>
            {
                eb.Ignore(e => e.Type);
                eb.Property(c => c.Breed).HasMaxLength(25);
                eb.Property(c => c.Breed).HasColumnName("BreedName");
                eb.Property(c => c.Breed).HasDefaultValue("None");
                eb.Property<string>("Selected").IsRequired().HasDefaultValue("false").HasConversion<bool>();
            });

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_for_shared_columns()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>().Property(a => a.Id).HasMaxLength(20).HasPrecision(15, 10).IsUnicode();
        modelBuilder.Entity<Cat>().OwnsOne(a => a.FavoritePerson);
        modelBuilder.Entity<Dog>().Ignore(d => d.FavoritePerson);

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_duplicate_foreignKey_names_within_hierarchy_on_different_tables()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>();
        modelBuilder.Entity<Cat>().HasOne<Person>().WithMany().HasForeignKey("FriendId").HasConstraintName("FK");
        modelBuilder.Entity<Dog>().HasOne<Person>().WithMany().HasForeignKey("FriendId").HasConstraintName("FK");

        modelBuilder.Entity<Cat>().ToTable("Cats");
        modelBuilder.Entity<Dog>().ToTable("Dogs");

        // Should throw. Issue #23144.
        Assert.Contains(
            "No exception was thrown",
            Assert.Throws<ThrowsException>(
                () => VerifyError(
                    RelationalStrings.DuplicateForeignKeyTableMismatch(
                        "{'FriendId'}", nameof(Dog),
                        "{'FriendId'}", nameof(Cat),
                        "FK",
                        "Cats",
                        "Dogs"),
                    modelBuilder)).Message);
    }

    [ConditionalFact]
    public virtual void Detects_duplicate_foreignKey_names_within_hierarchy_with_different_principal_tables()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>();
        modelBuilder.Entity<Cat>().HasOne<Person>().WithMany().HasForeignKey("FriendId").HasConstraintName("FK");
        modelBuilder.Entity<Dog>().HasOne<Animal>().WithMany().HasForeignKey("FriendId").HasConstraintName("FK");

        VerifyError(
            RelationalStrings.DuplicateForeignKeyPrincipalTableMismatch(
                "{'FriendId'}", nameof(Dog),
                "{'FriendId'}", nameof(Cat),
                nameof(Animal), "FK",
                nameof(Animal),
                nameof(Person)),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_duplicate_foreignKey_names_within_hierarchy_with_different_column_count()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>().Property<int>("FriendId");
        modelBuilder.Entity<Animal>().Property<string>("Shadow");
        modelBuilder.Entity<Cat>().HasOne<Person>().WithMany().HasForeignKey("FriendId", "Shadow").HasPrincipalKey(
            p => new { p.Id, p.Name }).HasConstraintName("FK");
        modelBuilder.Entity<Dog>().HasOne<Person>().WithMany().HasForeignKey("FriendId").HasConstraintName("FK");
        modelBuilder.Entity<Person>().Property(e => e.Id).ValueGeneratedNever();

        VerifyError(
            RelationalStrings.DuplicateForeignKeyColumnMismatch(
                "{'FriendId'}", nameof(Dog),
                "{'FriendId', 'Shadow'}", nameof(Cat),
                nameof(Animal), "FK",
                "{'FriendId'}",
                "{'FriendId', 'Shadow'}"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_duplicate_foreignKey_names_within_hierarchy_with_different_column_order()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<Animal>();
        modelBuilder.Entity<Cat>(
            et =>
            {
                et.Property(c => c.Breed).HasColumnName("Breed");
                et.HasOne<Person>().WithMany()
                    .HasForeignKey(
                        c => new { c.Name, c.Breed })
                    .HasPrincipalKey(
                        p => new { p.Name, p.FavoriteBreed })
                    .HasConstraintName("FK");
            });
        modelBuilder.Entity<Dog>(
            et =>
            {
                et.Property(c => c.Breed).HasColumnName("Breed");
                et.HasOne<Person>().WithMany()
                    .HasForeignKey(
                        d => new { d.Breed, d.Name })
                    .HasPrincipalKey(
                        p => new { p.FavoriteBreed, p.Name })
                    .HasConstraintName("FK");
            });

        VerifyError(
            RelationalStrings.DuplicateForeignKeyColumnMismatch(
                "{'" + nameof(Dog.Breed) + "', '" + nameof(Dog.Name) + "'}", nameof(Dog),
                "{'" + nameof(Cat.Name) + "', '" + nameof(Cat.Breed) + "'}", nameof(Cat),
                nameof(Animal), "FK",
                "{'" + nameof(Dog.Breed) + "', '" + nameof(Dog.Name) + "'}",
                "{'" + nameof(Cat.Name) + "', '" + nameof(Cat.Breed) + "'}"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_duplicate_foreignKey_names_within_hierarchy_mapped_to_different_columns()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>();
        modelBuilder.Entity<Cat>().HasOne<Person>().WithMany().HasForeignKey(
            c => new { c.Name, c.Breed }).HasPrincipalKey(
            p => new { p.Name, p.FavoriteBreed }).HasConstraintName("FK");
        modelBuilder.Entity<Dog>().HasOne<Person>().WithMany().HasForeignKey(
            d => new { d.Name, d.Breed }).HasPrincipalKey(
            p => new { p.Name, p.FavoriteBreed }).HasConstraintName("FK");
        modelBuilder.Entity<Dog>().Property(d => d.Breed).HasColumnName("DogBreed");

        VerifyError(
            RelationalStrings.DuplicateForeignKeyColumnMismatch(
                "{'" + nameof(Dog.Name) + "', '" + nameof(Dog.Breed) + "'}", nameof(Dog),
                "{'" + nameof(Cat.Name) + "', '" + nameof(Cat.Breed) + "'}", nameof(Cat),
                nameof(Animal), "FK",
                "{'" + nameof(Dog.Name) + "', 'DogBreed'}",
                "{'" + nameof(Cat.Name) + "', '" + nameof(Cat.Breed) + "'}"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_duplicate_foreignKey_names_within_hierarchy_referencing_different_columns()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>();
        modelBuilder.Entity<Cat>()
            .HasOne<Person>().WithMany()
            .HasForeignKey(c => c.Name).HasPrincipalKey(p => p.Name).HasConstraintName("FK");
        modelBuilder.Entity<Dog>()
            .HasOne<Person>().WithMany()
            .HasForeignKey(d => d.Name).HasPrincipalKey(p => p.FavoriteBreed).HasConstraintName("FK");
        modelBuilder.Entity<Dog>().Property(d => d.Breed).HasColumnName("DogBreed");

        VerifyError(
            RelationalStrings.DuplicateForeignKeyPrincipalColumnMismatch(
                "{'" + nameof(Dog.Name) + "'}", nameof(Dog),
                "{'" + nameof(Cat.Name) + "'}", nameof(Cat),
                nameof(Animal), "FK",
                "{'" + nameof(Person.FavoriteBreed) + "'}",
                "{'" + nameof(Person.Name) + "'}"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_duplicate_foreignKey_names_within_hierarchy_with_different_uniqueness()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>();
        var fk1 = modelBuilder.Entity<Cat>().HasOne<Person>().WithMany().HasForeignKey(c => c.Name).HasPrincipalKey(p => p.Name)
            .HasConstraintName("FK_Animal_Person_Name").Metadata;
        var fk2 = modelBuilder.Entity<Dog>().HasOne<Person>().WithOne().HasForeignKey<Dog>(d => d.Name)
            .HasPrincipalKey<Person>(p => p.Name)
            .HasConstraintName("FK_Animal_Person_Name").Metadata;

        VerifyError(
            RelationalStrings.DuplicateForeignKeyUniquenessMismatch(
                "{'" + nameof(Dog.Name) + "'}", nameof(Dog),
                "{'" + nameof(Cat.Name) + "'}", nameof(Cat),
                nameof(Animal), "FK_Animal_Person_Name"),
            modelBuilder);

        var index1 = fk1.DeclaringEntityType.GetDeclaredIndexes().Single();
        var index2 = fk2.DeclaringEntityType.GetDeclaredIndexes().Single();
        Assert.NotSame(index1, index2);
        Assert.NotEqual(index1.GetDatabaseName(), index2.GetDatabaseName());
    }

    [ConditionalFact]
    public virtual void Detects_duplicate_foreignKey_names_within_hierarchy_with_different_delete_behavior()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>();
        modelBuilder.Entity<Cat>().HasOne<Person>().WithMany().HasForeignKey(c => c.Name).HasPrincipalKey(p => p.Name)
            .OnDelete(DeleteBehavior.Cascade).HasConstraintName("FK_Animal_Person_Name");
        modelBuilder.Entity<Dog>().HasOne<Person>().WithMany().HasForeignKey(d => d.Name).HasPrincipalKey(p => p.Name)
            .OnDelete(DeleteBehavior.SetNull).HasConstraintName("FK_Animal_Person_Name");

        VerifyError(
            RelationalStrings.DuplicateForeignKeyDeleteBehaviorMismatch(
                "{'" + nameof(Dog.Name) + "'}", nameof(Dog),
                "{'" + nameof(Cat.Name) + "'}", nameof(Cat),
                nameof(Animal), "FK_Animal_Person_Name",
                DeleteBehavior.SetNull, DeleteBehavior.Cascade),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_for_incompatible_foreignKeys_within_hierarchy()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>();
        var fk1 = modelBuilder.Entity<Cat>().HasOne<Person>().WithMany().HasForeignKey(c => c.Name).HasPrincipalKey(p => p.Name)
            .OnDelete(DeleteBehavior.Cascade).Metadata;
        var fk2 = modelBuilder.Entity<Dog>().HasOne<Person>().WithMany().HasForeignKey(d => d.Name).HasPrincipalKey(p => p.Name)
            .OnDelete(DeleteBehavior.SetNull).Metadata;

        Validate(modelBuilder);

        Assert.Equal("FK_Animal_Person_Name", fk1.GetConstraintName());
        Assert.Equal("FK_Animal_Person_Name1", fk2.GetConstraintName());

        var index1 = fk1.DeclaringEntityType.GetDeclaredIndexes().Single();
        var index2 = fk2.DeclaringEntityType.GetDeclaredIndexes().Single();
        Assert.NotSame(index1, index2);
        Assert.Equal(index1.GetDatabaseName(), index2.GetDatabaseName());
    }

    [ConditionalFact]
    public virtual void Passes_for_incompatible_foreignKeys_within_hierarchy_when_one_name_configured_explicitly()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>();
        var fk1 = modelBuilder.Entity<Cat>().HasOne<Person>().WithMany().HasForeignKey(c => c.Name).HasPrincipalKey(p => p.Name)
            .OnDelete(DeleteBehavior.Cascade).HasConstraintName("FK_Animal_Person_Name").Metadata;
        var fk2 = modelBuilder.Entity<Dog>().HasOne<Person>().WithMany().HasForeignKey(d => d.Name).HasPrincipalKey(p => p.Name)
            .OnDelete(DeleteBehavior.SetNull).Metadata;

        Validate(modelBuilder);

        Assert.Equal("FK_Animal_Person_Name", fk1.GetConstraintName());
        Assert.Equal("FK_Animal_Person_Name1", fk2.GetConstraintName());

        var index1 = fk1.DeclaringEntityType.GetDeclaredIndexes().Single();
        var index2 = fk2.DeclaringEntityType.GetDeclaredIndexes().Single();
        Assert.NotSame(index1, index2);
        Assert.Equal(index1.GetDatabaseName(), index2.GetDatabaseName());
    }

    [ConditionalFact]
    public virtual void Passes_for_compatible_duplicate_foreignKey_names_within_hierarchy()
    {
        var modelBuilder = CreateConventionModelBuilder();
        IReadOnlyForeignKey fk1 = null;
        IReadOnlyForeignKey fk2 = null;

        modelBuilder.Entity<Animal>();
        modelBuilder.Entity<Cat>(
            et =>
            {
                et.Property(c => c.Breed).HasColumnName("Breed");
                fk1 = et
                    .HasOne(a => a.FavoritePerson)
                    .WithMany()
                    .HasForeignKey(
                        c => new { c.Name, c.Breed })
                    .HasPrincipalKey(
                        p => new { p.Name, p.FavoriteBreed })
                    .Metadata;
            });
        modelBuilder.Entity<Dog>(
            et =>
            {
                et.Property(c => c.Breed).HasColumnName("Breed");
                fk2 = et
                    .HasOne(a => (Employee)a.FavoritePerson)
                    .WithMany()
                    .HasForeignKey(
                        c => new { c.Name, c.Breed })
                    .HasPrincipalKey(
                        p => new { p.Name, p.FavoriteBreed })
                    .Metadata;
            });

        Validate(modelBuilder);

        Assert.NotSame(fk1, fk2);
        Assert.Equal(fk1.GetConstraintName(), fk2.GetConstraintName());

        var index1 = fk1.DeclaringEntityType.GetDeclaredIndexes().Single();
        var index2 = fk2.DeclaringEntityType.GetDeclaredIndexes().Single();
        Assert.NotSame(index1, index2);
        Assert.Equal(index1.GetDatabaseName(), index2.GetDatabaseName());
    }

    [ConditionalFact]
    public virtual void Passes_for_compatible_duplicate_foreignKey_names_within_hierarchy_name_configured_explicitly()
    {
        var modelBuilder = CreateConventionModelBuilder();
        IReadOnlyForeignKey fk1 = null;
        IReadOnlyForeignKey fk2 = null;

        modelBuilder.Entity<Animal>();
        modelBuilder.Entity<Cat>(
            et =>
            {
                et.Property(c => c.Breed).HasColumnName("Breed");
                fk1 = et
                    .HasOne<Person>()
                    .WithMany()
                    .HasForeignKey(
                        c => new { c.Name, c.Breed })
                    .HasPrincipalKey(
                        p => new { p.Name, p.FavoriteBreed })
                    .HasConstraintName("FK")
                    .Metadata;
            });
        modelBuilder.Entity<Dog>(
            et =>
            {
                et.Property(c => c.Breed).HasColumnName("Breed");
                fk2 = et
                    .HasOne<Employee>()
                    .WithMany()
                    .HasForeignKey(
                        c => new { c.Name, c.Breed })
                    .HasPrincipalKey(
                        p => new { p.Name, p.FavoriteBreed })
                    .HasConstraintName("FK")
                    .Metadata;
            });

        Validate(modelBuilder);

        Assert.NotSame(fk1, fk2);
        Assert.Equal(fk1.GetConstraintName(), fk2.GetConstraintName());

        var index1 = fk1.DeclaringEntityType.GetDeclaredIndexes().Single();
        var index2 = fk2.DeclaringEntityType.GetDeclaredIndexes().Single();
        Assert.NotSame(index1, index2);
        Assert.Equal(index1.GetDatabaseName(), index2.GetDatabaseName());
    }

    [ConditionalFact]
    public virtual void Detects_duplicate_index_names_within_hierarchy_with_different_column_count()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>().Property<int>("Shadow");
        modelBuilder.Entity<Cat>().HasIndex(nameof(Cat.Name), "Shadow").HasDatabaseName("IX");
        modelBuilder.Entity<Dog>().HasIndex(d => d.Name).HasDatabaseName("IX");

        VerifyError(
            RelationalStrings.DuplicateIndexColumnMismatch(
                "{'" + nameof(Dog.Name) + "'}", nameof(Dog),
                "{'" + nameof(Cat.Name) + "', 'Shadow'}", nameof(Cat),
                nameof(Animal), "IX",
                "{'" + nameof(Dog.Name) + "'}",
                "{'" + nameof(Cat.Name) + "', 'Shadow'}"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_duplicate_index_names_within_hierarchy_with_different_column_order()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<Animal>();
        modelBuilder.Entity<Cat>(
            et =>
            {
                et.Property(c => c.Breed).HasColumnName("Breed");
                et.HasIndex(
                    c => new { c.Name, c.Breed }).HasDatabaseName("IX");
            });
        modelBuilder.Entity<Dog>(
            et =>
            {
                et.Property(c => c.Breed).HasColumnName("Breed");
                et.HasIndex(
                    d => new { d.Breed, d.Name }).HasDatabaseName("IX");
            });

        VerifyError(
            RelationalStrings.DuplicateIndexColumnMismatch(
                "{'" + nameof(Dog.Breed) + "', '" + nameof(Dog.Name) + "'}", nameof(Dog),
                "{'" + nameof(Cat.Name) + "', '" + nameof(Cat.Breed) + "'}", nameof(Cat),
                nameof(Animal), "IX",
                "{'" + nameof(Dog.Breed) + "', '" + nameof(Dog.Name) + "'}",
                "{'" + nameof(Cat.Name) + "', '" + nameof(Cat.Breed) + "'}"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_duplicate_index_names_within_hierarchy_mapped_to_different_columns()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>();
        modelBuilder.Entity<Cat>().HasIndex(
            c => new { c.Name, c.Breed }).HasDatabaseName("IX");
        modelBuilder.Entity<Dog>().HasIndex(
            d => new { d.Name, d.Breed }).HasDatabaseName("IX");
        modelBuilder.Entity<Dog>().Property(d => d.Breed).HasColumnName("DogBreed");

        VerifyError(
            RelationalStrings.DuplicateIndexColumnMismatch(
                "{'" + nameof(Dog.Name) + "', '" + nameof(Dog.Breed) + "'}", nameof(Dog),
                "{'" + nameof(Cat.Name) + "', '" + nameof(Cat.Breed) + "'}", nameof(Cat),
                nameof(Animal), "IX",
                "{'" + nameof(Dog.Name) + "', 'DogBreed'}",
                "{'" + nameof(Cat.Name) + "', '" + nameof(Cat.Breed) + "'}"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_duplicate_index_names_within_hierarchy_with_different_uniqueness()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>();
        modelBuilder.Entity<Cat>().HasIndex(c => c.Name).IsUnique().HasDatabaseName("IX_Animal_Name");
        modelBuilder.Entity<Dog>().HasIndex(d => d.Name).IsUnique(false).HasDatabaseName("IX_Animal_Name");

        VerifyError(
            RelationalStrings.DuplicateIndexUniquenessMismatch(
                "{'" + nameof(Dog.Name) + "'}", nameof(Dog),
                "{'" + nameof(Cat.Name) + "'}", nameof(Cat),
                nameof(Animal), "IX_Animal_Name"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_duplicate_index_names_within_hierarchy_with_different_sort_orders()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>();
        modelBuilder.Entity<Cat>().HasIndex(c => c.Name).HasDatabaseName("IX_Animal_Name")
            .IsDescending(true);
        modelBuilder.Entity<Dog>().HasIndex(d => d.Name).HasDatabaseName("IX_Animal_Name")
            .IsDescending(false);

        VerifyError(
            RelationalStrings.DuplicateIndexSortOrdersMismatch(
                "{'" + nameof(Dog.Name) + "'}", nameof(Dog),
                "{'" + nameof(Cat.Name) + "'}", nameof(Cat),
                nameof(Animal), "IX_Animal_Name"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_duplicate_index_names_within_hierarchy_with_different_filters()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>();
        modelBuilder.Entity<Cat>().HasIndex(c => c.Name).HasFilter("Foo").HasDatabaseName("IX_Animal_Name");
        modelBuilder.Entity<Dog>().HasIndex(d => d.Name).HasFilter("Bar").HasDatabaseName("IX_Animal_Name");

        VerifyError(
            RelationalStrings.DuplicateIndexFiltersMismatch(
                "{'" + nameof(Dog.Name) + "'}", nameof(Dog),
                "{'" + nameof(Cat.Name) + "'}", nameof(Cat),
                nameof(Animal), "IX_Animal_Name",
                "Bar", "Foo"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_for_incompatible_indexes_within_hierarchy_when_one_name_configured_explicitly()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>();
        var index1 = modelBuilder.Entity<Cat>().HasIndex(c => c.Name).IsUnique().HasDatabaseName("IX_Animal_Name").Metadata;
        var index2 = modelBuilder.Entity<Dog>().HasIndex(d => d.Name).IsUnique(false).Metadata;

        Validate(modelBuilder);

        Assert.Equal("IX_Animal_Name", index1.GetDatabaseName());
        Assert.Equal("IX_Animal_Name1", index2.GetDatabaseName());
    }

    [ConditionalFact]
    public virtual void Passes_for_compatible_duplicate_index_names_within_hierarchy()
    {
        var modelBuilder = CreateConventionModelBuilder();
        IMutableIndex index1 = null;
        IMutableIndex index2 = null;
        modelBuilder.Entity<Animal>();
        modelBuilder.Entity<Cat>(
            et =>
            {
                et.Property(c => c.Breed).HasColumnName("Breed");
                index1 = et.HasIndex(c => c.Breed, "IX_Animal_Breed").Metadata;
            });
        modelBuilder.Entity<Dog>(
            et =>
            {
                et.Property(c => c.Breed).HasColumnName("Breed");
                index2 = et.HasIndex(c => c.Breed, "IX_Animal_Breed").Metadata;
            });

        Validate(modelBuilder);

        Assert.NotSame(index1, index2);
        Assert.Equal(index1.GetDatabaseName(), index2.GetDatabaseName());
    }

    [ConditionalFact]
    public virtual void Passes_for_indexes_on_related_types_mapped_to_different_tables()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<PropertyBase>();
        modelBuilder.Entity<Property>();

        Validate(modelBuilder);
    }

    [Table("Objects")]
    private abstract class PropertyBase
    {
        public int Id { get; set; }

        public Organization Organization { get; set; }
    }

    private class Organization
    {
        public int Id { get; set; }
    }

    [Table("Properties")]
    private class Property : PropertyBase
    {
        public PropertyDetails Details { get; set; } = null!;
    }

    [Owned]
    private class PropertyDetails
    {
        public Address Address { get; set; }
    }

    private class Address
    {
        public int Id { get; set; }
    }

    [ConditionalFact]
    public virtual void Passes_with_missing_concurrency_token_on_the_base_type_without_convention()
    {
        var modelBuilder = CreateModelBuilderWithoutConvention<TableSharingConcurrencyTokenConvention>();
        modelBuilder.Entity<Person>().ToTable(nameof(Animal))
            .Property<byte[]>("Version").IsRowVersion().HasColumnName("Version");
        modelBuilder.Entity<Animal>().HasOne(a => a.FavoritePerson).WithOne().HasForeignKey<Person>(p => p.Id);
        modelBuilder.Entity<Cat>()
            .Property<byte[]>("Version").IsRowVersion().HasColumnName("Version");

        var model = Validate(modelBuilder);

        var animalType = model.FindEntityType(typeof(Animal))!;
        Assert.Null(animalType.GetDeclaredProperties().SingleOrDefault(p => p.IsConcurrencyToken));
    }

    [ConditionalFact]
    public virtual void Detects_missing_concurrency_token_on_the_sharing_type_without_convention()
    {
        var modelBuilder = CreateModelBuilderWithoutConvention<TableSharingConcurrencyTokenConvention>();
        modelBuilder.Entity<Person>().ToTable(nameof(Animal));
        modelBuilder.Entity<Animal>().HasOne(a => a.FavoritePerson).WithOne().HasForeignKey<Person>(p => p.Id);
        modelBuilder.Entity<Animal>().Property<byte[]>("Version").IsRowVersion().HasColumnName("Version");

        VerifyError(
            RelationalStrings.MissingConcurrencyColumn(nameof(Person), "Version", nameof(Animal)),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_for_missing_concurrency_token_property_on_the_base_type()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Person>().ToTable(nameof(Animal))
            .Property<byte[]>("Version").IsRowVersion().HasColumnName("Version");
        modelBuilder.Entity<Animal>().HasOne(a => a.FavoritePerson).WithOne().HasForeignKey<Person>(p => p.Id);
        modelBuilder.Entity<Cat>()
            .Property<byte[]>("Version").IsRowVersion().HasColumnName("Version");

        var model = Validate(modelBuilder);

        var animalType = model.FindEntityType(typeof(Animal))!;
        Assert.Null(animalType.GetDeclaredProperties().SingleOrDefault(p => p.IsConcurrencyToken));
    }

    [ConditionalFact]
    public virtual void Passes_for_missing_concurrency_token_property_on_the_base_type_when_derived_is_sharing()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Person>().ToTable(nameof(Animal))
            .Property<byte[]>("Version").IsRowVersion().HasColumnName("Version");
        modelBuilder.Entity<Animal>().Ignore(p => p.FavoritePerson);
        modelBuilder.Entity<Cat>().HasOne<Person>().WithOne().HasForeignKey<Person>(p => p.Id);
        modelBuilder.Entity<Cat>()
            .Property<byte[]>("Version").IsRowVersion().HasColumnName("Version");

        var model = Validate(modelBuilder);

        var animalType = model.FindEntityType(typeof(Animal));
        Assert.Empty(animalType.GetProperties().Where(p => p.IsConcurrencyToken));
    }

    [ConditionalFact]
    public virtual void Passes_for_missing_concurrency_token_property_on_the_sharing_type()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Person>().ToTable(nameof(Animal));
        modelBuilder.Entity<Animal>().HasOne(a => a.FavoritePerson).WithOne().HasForeignKey<Person>(p => p.Id);
        modelBuilder.Entity<Animal>().Property<ulong>("Version")
            .HasConversion<byte[]>().IsRowVersion();

        var model = Validate(modelBuilder);

        var personType = model.FindEntityType(typeof(Person))!;
        var concurrencyProperty = personType.GetDeclaredProperties().Single(p => p.IsConcurrencyToken);
        Assert.Equal("Version", concurrencyProperty.GetColumnName());
        Assert.Equal(typeof(ulong), concurrencyProperty.ClrType);
    }

    [ConditionalFact]
    public virtual void Passes_for_explicitly_mapped_concurrency_tokens_with_table_sharing()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Person>().ToTable(nameof(Animal))
            .Property<byte[]>("Version").IsRowVersion();
        modelBuilder.Entity<Animal>()
            .HasOne(a => a.FavoritePerson).WithOne().HasForeignKey<Person>(p => p.Id);
        modelBuilder.Entity<Animal>()
            .Property<byte[]>("Version").IsRowVersion();
        modelBuilder.Entity<Cat>();
        modelBuilder.Entity<Dog>();

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_for_missing_concurrency_token_on_owner()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>();
        modelBuilder.Entity<Cat>().OwnsOne(
            a => a.FavoritePerson,
            pb => pb.Property<byte[]>("Version").IsRowVersion().HasColumnName("Version"));
        modelBuilder.Entity<Dog>().OwnsOne(
            a => a.FavoritePerson);

        var model = Validate(modelBuilder);

        var animalType = model.FindEntityType(typeof(Animal))!;
        Assert.Null(animalType.GetDeclaredProperties().SingleOrDefault(p => p.IsConcurrencyToken));

        var dogType = model.FindEntityType(typeof(Dog))!;
        Assert.Null(dogType.GetDeclaredProperties().SingleOrDefault(p => p.IsConcurrencyToken));
    }

    [ConditionalFact]
    public virtual void Passes_for_explicitly_mapped_concurrency_tokens_with_owned()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>().Property<byte[]>("Version").IsRowVersion();
        modelBuilder.Entity<Cat>().OwnsOne(
            a => a.FavoritePerson,
            pb => pb.Property<byte[]>("Version").IsRowVersion());
        modelBuilder.Entity<Dog>().Ignore(d => d.FavoritePerson);

        var model = Validate(modelBuilder);

        var dogType = model.FindEntityType(typeof(Dog))!;
        Assert.Null(dogType.GetDeclaredProperties().SingleOrDefault(p => p.IsConcurrencyToken));
    }

    [ConditionalFact]
    public virtual void Passes_for_non_hierarchical_model()
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        var model = modelBuilder.Model;

        var entityA = model.AddEntityType(typeof(A));
        SetPrimaryKey(entityA);
        AddProperties(entityA);

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_for_missing_discriminator_value_for_abstract_class()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Abstract>();
        modelBuilder.Entity<A>().HasDiscriminator<byte>("ClassType")
            .HasValue<A>(0)
            .HasValue<C>(1)
            .HasValue<D>(2)
            .HasValue<Generic<string>>(3);

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_derived_entity_type_mapped_to_a_different_SQL_query()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>().ToSqlQuery("sql");
        modelBuilder.Entity<Cat>().ToSqlQuery("sql2");

        VerifyError(
            RelationalStrings.InvalidMappedSqlQueryDerivedType(nameof(Cat), nameof(Animal)),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_for_TPT()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>();
        modelBuilder.Entity<Cat>().ToTable("Cat").ToView("Cat");

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_for_unconfigured_entity_type_in_TPT()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>();
        modelBuilder.Entity<Cat>().ToTable("Cat");
        modelBuilder.Entity<Dog>();

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_clashing_entity_types_in_view_TPT()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>();
        modelBuilder.Entity<Cat>().ToTable("Cat").ToView("Cat");
        modelBuilder.Entity<Dog>().ToTable("Dog").ToView("Cat");

        VerifyError(
            RelationalStrings.NonTphViewClash(nameof(Dog), nameof(Cat), "Cat"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_table_and_view_TPT_mismatch()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>().ToTable("Animal").ToView("Animal");
        modelBuilder.Entity<Cat>().ToTable("Animal").ToView("Cat");

        VerifyError(
            RelationalStrings.NonTphTableClash(nameof(Cat), nameof(Animal), "Animal"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_TPT_with_discriminator()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>().HasDiscriminator<int>("Discriminator");
        modelBuilder.Entity<Cat>().ToTable("Cat");

        VerifyError(
            RelationalStrings.TphTableMismatch(nameof(Cat), nameof(Cat), nameof(Animal), nameof(Animal)),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_view_TPT_with_discriminator()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>().ToView("Animal").HasDiscriminator<int>("Discriminator");
        modelBuilder.Entity<Cat>().ToView("Cat");

        VerifyError(
            RelationalStrings.TphViewMismatch(nameof(Cat), nameof(Cat), nameof(Animal), nameof(Animal)),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_TPT_with_keyless_entity_type()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>().HasNoKey();
        modelBuilder.Entity<Cat>().ToTable("Cat");

        VerifyError(
            RelationalStrings.KeylessMappingStrategy("TPT", nameof(Animal)),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_on_valid_table_sharing_with_TPT()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<Animal>()
            .Ignore(a => a.FavoritePerson);

        modelBuilder.Entity<Cat>(
            x =>
            {
                x.ToTable("Cat");
                x.HasOne(c => c.FavoritePerson).WithOne().HasForeignKey<Person>(c => c.Id);
            });

        modelBuilder.Entity<Person>().ToTable("Cat");

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_linking_relationship_on_derived_type_in_TPT()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<Animal>()
            .Ignore(a => a.FavoritePerson);

        modelBuilder.Entity<Cat>(
            x =>
            {
                x.ToTable("Cat");
                x.HasOne(c => c.FavoritePerson).WithOne().HasForeignKey<Cat>(c => c.Id);
            });

        modelBuilder.Entity<Person>().ToTable("Cat");

        VerifyError(
            RelationalStrings.IncompatibleTableDerivedRelationship(
                "Cat", "Cat", "Person"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_linking_relationship_on_derived_type_in_TPT_views()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<Animal>()
            .Ignore(a => a.FavoritePerson)
            .ToView("Animal");

        modelBuilder.Entity<Cat>(
            x =>
            {
                x.ToView("Cat");
                x.HasOne(c => c.FavoritePerson).WithOne().HasForeignKey<Cat>(c => c.Id);
            });

        modelBuilder.Entity<Person>().ToView("Cat");

        VerifyError(
            RelationalStrings.IncompatibleViewDerivedRelationship(
                "Cat", "Cat", "Person"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_unmapped_foreign_keys_in_TPT()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>()
            .Ignore(a => a.FavoritePerson)
            .Property<int>("FavoritePersonId");
        modelBuilder.Entity<Cat>().ToTable("Cat")
            .HasOne<Person>().WithMany()
            .HasForeignKey("FavoritePersonId");

        var definition =
            RelationalResources.LogForeignKeyPropertiesMappedToUnrelatedTables(new TestLogger<TestRelationalLoggingDefinitions>());
        VerifyWarning(
            definition.GenerateMessage(
                l => l.Log(
                    definition.Level,
                    definition.EventId,
                    definition.MessageFormat,
                    "{'FavoritePersonId'}", nameof(Cat), nameof(Person), "{'FavoritePersonId'}", nameof(Cat), "{'Id'}",
                    nameof(Person))),
            modelBuilder,
            LogLevel.Error);
    }

    [ConditionalFact]
    public virtual void Passes_for_ToTable_for_abstract_class()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Abstract>().ToTable("Abstract");
        modelBuilder.Entity<A>();
        modelBuilder.Entity<Generic<string>>();

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_for_abstract_class_TPC()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Abstract>();
        modelBuilder.Entity<A>().UseTpcMappingStrategy();
        modelBuilder.Entity<Generic<string>>().ToTable("G");

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_for_view_TPC()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>().ToTable((string)null).UseTpcMappingStrategy();
        modelBuilder.Entity<Cat>().ToView("Cat");

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_invalid_MappingStrategy()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>().HasAnnotation(RelationalAnnotationNames.MappingStrategy, "TTT");
        modelBuilder.Entity<Cat>();

        VerifyError(
            RelationalStrings.InvalidMappingStrategy("TTT", nameof(Animal)),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_MappingStrategy_on_derived_types()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Cat>().HasBaseType((string)null);
        modelBuilder.Entity<Animal>();
        modelBuilder.Entity<Cat>().ToTable("Cat").ToView("Cat").UseTpcMappingStrategy().HasBaseType(typeof(Animal));

        VerifyError(
            RelationalStrings.DerivedStrategy(nameof(Cat), "TPC"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_ToTable_for_abstract_class_TPC()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Abstract>().ToTable("Abstract", "dbo").UseTpcMappingStrategy();
        modelBuilder.Entity<Generic<string>>();

        VerifyError(
            RelationalStrings.AbstractTpc(nameof(Abstract), "dbo.Abstract"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_ToView_for_abstract_class_TPC()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Abstract>().ToView("Abstract").UseTpcMappingStrategy();
        modelBuilder.Entity<Generic<string>>();

        VerifyError(
            RelationalStrings.AbstractTpc(nameof(Abstract), "Abstract"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_ToFunction_for_abstract_class_TPC()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Abstract>().ToFunction("Abstract").UseTpcMappingStrategy();
        modelBuilder.Entity<Generic<string>>();

        VerifyError(
            RelationalStrings.AbstractTpc(nameof(Abstract), "Abstract"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_UsingStoredProcedure_for_abstract_class_TPC()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Abstract>().InsertUsingStoredProcedure("Insert", s => s.HasParameter(s => s.Id))
            .UseTpcMappingStrategy();
        modelBuilder.Entity<Generic<string>>();

        VerifyError(
            RelationalStrings.AbstractTpc(nameof(Abstract), "Insert"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_clashing_entity_types_in_views_TPC()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>().UseTpcMappingStrategy();
        modelBuilder.Entity<Cat>().ToTable("Cat").ToView("Cat");
        modelBuilder.Entity<Dog>().ToTable("Dog").ToView("Cat");

        VerifyError(
            RelationalStrings.NonTphViewClash(nameof(Dog), nameof(Cat), "Cat"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_table_and_view_TPC_mismatch()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>().UseTpcMappingStrategy().ToTable("Animal").ToView("Animal");
        modelBuilder.Entity<Cat>().ToTable("Animal").ToView("Cat");

        VerifyError(
            RelationalStrings.NonTphTableClash(nameof(Cat), nameof(Animal), "Animal"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_on_TPC_with_keyless_entity_type()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>().UseTpcMappingStrategy().HasNoKey();
        modelBuilder.Entity<Cat>().ToTable("Cat");

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_view_TPC_with_discriminator()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>().ToView("Animal").UseTpcMappingStrategy().HasDiscriminator<int>("Discriminator");
        modelBuilder.Entity<Cat>().ToView("Cat");

        VerifyError(
            RelationalStrings.NonTphMappingStrategy("TPC", nameof(Animal)),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_store_generated_PK_in_TPC()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<Animal>(
            b =>
            {
                b.UseTpcMappingStrategy();
                b.Property(e => e.Id).ValueGeneratedOnAdd();
            });

        modelBuilder.Entity<Cat>();

        var definition =
            RelationalResources.LogTpcStoreGeneratedIdentity(new TestLogger<TestRelationalLoggingDefinitions>());
        VerifyWarning(
            definition.GenerateMessage(nameof(Animal), nameof(Animal.Id)),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_table_sharing_with_TPC_on_dependent()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<Animal>()
            .ToTable("Animal")
            .HasOne(c => c.FavoritePerson).WithOne().HasForeignKey<Person>(c => c.Id);

        modelBuilder.Entity<Person>().ToTable("Animal").UseTpcMappingStrategy();
        modelBuilder.Entity<Employee>().ToTable("Employee");

        VerifyError(
            RelationalStrings.TpcTableSharingDependent("Person", "Animal", "Employee", "Employee"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_on_valid_view_sharing_with_TPC()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<Animal>()
            .UseTpcMappingStrategy()
            .ToView("Animal")
            .Ignore(a => a.FavoritePerson);

        modelBuilder.Entity<Cat>(
            x =>
            {
                x.ToView("Cat");
                x.HasOne(c => c.FavoritePerson).WithOne().HasForeignKey<Person>(c => c.Id);
            });

        modelBuilder.Entity<Person>().ToView("Cat");

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_view_sharing_on_base_with_TPC()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<Animal>()
            .UseTpcMappingStrategy()
            .ToView("Animal")
            .HasOne(c => c.FavoritePerson).WithOne().HasForeignKey<Person>(c => c.Id);

        modelBuilder.Entity<Cat>(x => x.ToView("Cat"));

        modelBuilder.Entity<Person>().ToView("Animal");

        VerifyError(
            RelationalStrings.TpcTableSharing("Person", "Animal", "Animal"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_owned_table_sharing_on_abstract_class_with_TPC()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<LivingBeing>()
            .UseTpcMappingStrategy()
            .OwnsOne(b => b.Details);

        modelBuilder.Entity<Animal>();

        VerifyError(
            RelationalStrings.UnmappedNonTPHOwner("LivingBeing", "Details", "OwnedEntity", "Table"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_owned_view_sharing_on_abstract_class_with_TPT()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<LivingBeing>()
            .UseTptMappingStrategy()
            .OwnsOne(b => b.Details, ob =>
            {
                ob.ToTable((string)null);
            });

        modelBuilder.Entity<Animal>()
            .ToView("Animal");

        VerifyError(
            RelationalStrings.UnmappedNonTPHOwner("LivingBeing", "Details", "OwnedEntity", "View"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_linking_relationship_on_derived_type_in_TPC()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<Animal>()
            .UseTpcMappingStrategy()
            .Ignore(a => a.FavoritePerson);

        modelBuilder.Entity<Cat>(
            x =>
            {
                x.ToTable("Cat");
                x.HasOne(c => c.FavoritePerson).WithOne().HasForeignKey<Cat>(c => c.Id);
            });

        modelBuilder.Entity<Person>().ToTable("Cat");

        VerifyError(
            RelationalStrings.IncompatibleTableDerivedRelationship(
                "Cat", "Cat", "Person"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_linking_relationship_on_derived_type_in_TPC_views()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<Animal>()
            .UseTpcMappingStrategy()
            .Ignore(a => a.FavoritePerson)
            .ToView("Animal");

        modelBuilder.Entity<Cat>(
            x =>
            {
                x.ToView("Cat");
                x.HasOne(c => c.FavoritePerson).WithOne().HasForeignKey<Cat>(c => c.Id);
            });

        modelBuilder.Entity<Person>().ToView("Cat");

        VerifyError(
            RelationalStrings.IncompatibleViewDerivedRelationship(
                "Cat", "Cat", "Person"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_unmapped_foreign_keys_in_TPC()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>().UseTpcMappingStrategy()
            .Property(a => a.Id).ValueGeneratedNever();
        modelBuilder.Entity<Cat>().ToTable("Cat");
        modelBuilder.Entity<Person>()
            .HasOne<Animal>().WithOne(a => a.FavoritePerson)
            .HasForeignKey<Person>(p => p.FavoriteBreed)
            .HasPrincipalKey<Animal>(a => a.Name);

        var definition =
            RelationalResources.LogForeignKeyTpcPrincipal(new TestLogger<TestRelationalLoggingDefinitions>());
        VerifyWarning(
            definition.GenerateMessage(
                l => l.Log(
                    definition.Level,
                    definition.EventId,
                    definition.MessageFormat,
                    "{'FavoriteBreed'}", nameof(Person), nameof(Animal), nameof(Animal), nameof(Animal), nameof(Person),
                    nameof(Animal))),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_for_valid_table_overrides()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>();
        var property = modelBuilder.Entity<Dog>().Property(a => a.Identity).GetInfrastructure();
        modelBuilder.Entity<Dog>().ToTable("Dog");
        property.HasColumnName("DogName", StoreObjectIdentifier.Table("Dog"));

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_invalid_table_overrides()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>();
        var property = modelBuilder.Entity<Dog>().Property(a => a.Identity).GetInfrastructure();
        property.HasColumnName("DogName", StoreObjectIdentifier.Table("Dog"));

        VerifyError(
            RelationalStrings.TableOverrideMismatch("Dog.Identity", "Dog"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_column_override_on_an_inherited_property_with_TPT()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<Animal>()
            .Ignore(a => a.FavoritePerson);

        modelBuilder.Entity<Cat>().ToTable("Cat", b => b.Property(c => c.Name).HasColumnName("Name"));

        VerifyError(
            RelationalStrings.TableOverrideMismatch("Animal.Name", "Cat"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_for_valid_view_overrides()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>();
        var property = modelBuilder.Entity<Dog>().Property(a => a.Identity).GetInfrastructure();
        modelBuilder.Entity<Dog>().ToView("Dog");
        property.HasColumnName("DogName", StoreObjectIdentifier.View("Dog"));

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_invalid_view_overrides()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>();
        var property = modelBuilder.Entity<Dog>().Property(a => a.Identity).GetInfrastructure();
        property.HasColumnName("DogName", StoreObjectIdentifier.View("Dog"));

        VerifyError(
            RelationalStrings.ViewOverrideMismatch("Dog.Identity", "Dog"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_invalid_sql_query_overrides()
    {
        var modelBuilder = CreateConventionModelBuilder();
        var property = modelBuilder.Entity<Animal>().Property(a => a.Name).GetInfrastructure();
        property.HasColumnName("DogName", StoreObjectIdentifier.SqlQuery("Dog"));

        VerifyError(
            RelationalStrings.SqlQueryOverrideMismatch("Animal.Name", "Dog"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_invalid_stored_procedure_overrides()
    {
        var modelBuilder = CreateConventionModelBuilder();
        var property = modelBuilder.Entity<Animal>().Property(a => a.Name).GetInfrastructure();
        property.HasColumnName("DogName", StoreObjectIdentifier.InsertStoredProcedure("Dog"));

        VerifyError(
            RelationalStrings.StoredProcedureOverrideMismatch("Animal.Name", "Dog"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_invalid_function_overrides()
    {
        var modelBuilder = CreateConventionModelBuilder();
        var property = modelBuilder.Entity<Animal>().Property(a => a.Name).GetInfrastructure();
        property.HasColumnName("DogName", StoreObjectIdentifier.DbFunction("Dog"));

        VerifyError(
            RelationalStrings.FunctionOverrideMismatch("Animal.Name", "Dog"),
            modelBuilder);
    }

    [ConditionalFact]
    public void Detects_function_with_invalid_return_type()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.HasDbFunction(TestMethods.MethodCMi);

        VerifyError(
            RelationalStrings.DbFunctionInvalidReturnType(
                "Microsoft.EntityFrameworkCore.Infrastructure.RelationalModelValidatorTest+TestMethods.MethodC()",
                typeof(TestMethods).ShortDisplayName()),
            modelBuilder);
    }

    [ConditionalFact]
    public void Detects_function_with_unmapped_return_type()
    {
        var modelBuilder = CreateConventionlessModelBuilder();

        var methodInfo
            = typeof(TestMethods)
                .GetRuntimeMethod(nameof(TestMethods.MethodA), []);

        modelBuilder.HasDbFunction(methodInfo);

        VerifyError(
            RelationalStrings.DbFunctionInvalidReturnEntityType(
                "Microsoft.EntityFrameworkCore.Infrastructure.RelationalModelValidatorTest+TestMethods.MethodA()",
                typeof(IQueryable<TestMethods>).ShortDisplayName(),
                typeof(TestMethods).ShortDisplayName()),
            modelBuilder);
    }

    [ConditionalFact]
    public void Detects_function_with_invalid_parameter_type()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.HasDbFunction(TestMethods.MethodDMi);

        VerifyError(
            RelationalStrings.DbFunctionInvalidParameterType(
                "methods",
                "Microsoft.EntityFrameworkCore.Infrastructure.RelationalModelValidatorTest+TestMethods.MethodD(Microsoft.EntityFrameworkCore.Infrastructure.RelationalModelValidatorTest+TestMethods)",
                typeof(TestMethods).ShortDisplayName()),
            modelBuilder);
    }

    [ConditionalFact]
    public void Passes_for_valid_entity_type_mapped_to_function()
    {
        var modelBuilder = CreateConventionModelBuilder();

        var methodInfo
            = typeof(TestMethods)
                .GetRuntimeMethod(nameof(TestMethods.MethodA), []);

        var function = modelBuilder.HasDbFunction(methodInfo).Metadata;

        modelBuilder.Entity<TestMethods>().HasNoKey().ToFunction(function.ModelName);

        var model = Validate(modelBuilder);

        Assert.Single(model.GetEntityTypes());
        Assert.Single(model.GetDbFunctions());
    }

    [ConditionalFact]
    public void Detects_entity_type_mapped_to_non_existent_function()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<TestMethods>().HasNoKey().ToFunction("NonExistent");

        modelBuilder.Model.RemoveDbFunction("NonExistent");

        VerifyError(
            RelationalStrings.MappedFunctionNotFound(nameof(TestMethods), "NonExistent"),
            modelBuilder);
    }

    [ConditionalFact]
    public void Detects_entity_type_mapped_to_a_scalar_function()
    {
        var modelBuilder = CreateConventionModelBuilder();

        var function = modelBuilder.HasDbFunction(TestMethods.MethodEMi).Metadata;

        modelBuilder.Entity<TestMethods>().HasNoKey().ToFunction(function.ModelName);

        VerifyError(
            RelationalStrings.InvalidMappedFunctionUnmatchedReturn(
                nameof(TestMethods),
                "Microsoft.EntityFrameworkCore.Infrastructure.RelationalModelValidatorTest+TestMethods.MethodE()",
                "int",
                nameof(TestMethods)),
            modelBuilder);
    }

    [ConditionalFact]
    public void Detects_entity_type_mapped_to_a_different_type()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<Animal>().HasNoKey().ToFunction(TestMethods.MethodAMi);
        modelBuilder.Entity<TestMethods>().HasNoKey();

        VerifyError(
            RelationalStrings.InvalidMappedFunctionUnmatchedReturn(
                nameof(Animal),
                "Microsoft.EntityFrameworkCore.Infrastructure.RelationalModelValidatorTest+TestMethods.MethodA()",
                typeof(IQueryable<TestMethods>).ShortDisplayName(),
                nameof(Animal)),
            modelBuilder);
    }

    [ConditionalFact]
    public void Detects_entity_type_mapped_to_a_function_with_parameters()
    {
        var modelBuilder = CreateConventionModelBuilder();

        ((IConventionEntityType)modelBuilder.Entity<TestMethods>().HasNoKey().Metadata)
            .Builder.ToFunction(TestMethods.MethodBMi);

        VerifyError(
            RelationalStrings.InvalidMappedFunctionWithParameters(
                nameof(TestMethods),
                "Microsoft.EntityFrameworkCore.Infrastructure.RelationalModelValidatorTest+TestMethods.MethodB(int)",
                "{'id'}"),
            modelBuilder);
    }

    [ConditionalFact]
    public void Detects_multiple_entity_types_mapped_to_the_same_function()
    {
        var modelBuilder = CreateConventionModelBuilder();

        var function = modelBuilder.HasDbFunction(TestMethods.MethodAMi).Metadata;

        modelBuilder.Entity<DerivedTestMethods>(
            db =>
            {
                db.HasBaseType((string)null);
                db.OwnsOne(d => d.SomeTestMethods).ToFunction(function.ModelName);
                db.OwnsOne(d => d.OtherTestMethods).ToFunction(function.ModelName);
            });

        VerifyError(
            RelationalStrings.DbFunctionInvalidIQueryableOwnedReturnType(
                "Microsoft.EntityFrameworkCore.Infrastructure.RelationalModelValidatorTest+TestMethods.MethodA()",
                nameof(TestMethods)),
            modelBuilder);
    }

    [ConditionalFact]
    public void Detects_derived_entity_type_mapped_to_a_function()
    {
        var modelBuilder = CreateConventionModelBuilder();

        var function = modelBuilder.HasDbFunction(BaseTestMethods.MethodAMi).Metadata;

        modelBuilder.Entity<BaseTestMethods>().ToFunction(function.ModelName).HasNoKey();
        modelBuilder.Entity<TestMethods>().ToFunction(function.ModelName);

        VerifyError(
            RelationalStrings.InvalidMappedFunctionDerivedType(
                nameof(TestMethods),
                "Microsoft.EntityFrameworkCore.Infrastructure.RelationalModelValidatorTest+BaseTestMethods.MethodA()",
                nameof(BaseTestMethods)),
            modelBuilder);
    }

    [ConditionalFact]
    public void Detects_derived_entity_type_mapped_to_a_different_function()
    {
        var modelBuilder = CreateConventionModelBuilder();

        var function = modelBuilder.HasDbFunction(BaseTestMethods.MethodAMi).Metadata;
        var function2 = modelBuilder.HasDbFunction(TestMethods.MethodAMi).Metadata;

        modelBuilder.Entity<BaseTestMethods>().ToFunction(function.ModelName).HasNoKey();
        modelBuilder.Entity<TestMethods>().ToFunction(function2.ModelName);

        VerifyError(
            RelationalStrings.TphDbFunctionMismatch(
                nameof(TestMethods),
                "Microsoft.EntityFrameworkCore.Infrastructure.RelationalModelValidatorTest+TestMethods.MethodA()",
                nameof(BaseTestMethods),
                "Microsoft.EntityFrameworkCore.Infrastructure.RelationalModelValidatorTest+BaseTestMethods.MethodA()"),
            modelBuilder);
    }

    [ConditionalFact]
    public void Detects_multiple_entity_types_mapped_to_the_same_stored_procedure()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<DerivedTestMethods>(
            db =>
            {
                db.HasBaseType((string)null);
                db.OwnsOne(d => d.SomeTestMethods).DeleteUsingStoredProcedure(
                    "Delete",
                    s => s.HasOriginalValueParameter("DerivedTestMethodsId"));
                db.OwnsOne(d => d.OtherTestMethods).DeleteUsingStoredProcedure(
                    "Delete",
                    s => s.HasOriginalValueParameter("DerivedTestMethodsId"));
            });

        VerifyError(
            RelationalStrings.StoredProcedureTableSharing(
                "DerivedTestMethods.OtherTestMethods#TestMethods",
                "DerivedTestMethods.SomeTestMethods#TestMethods",
                "Delete"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_keyless_entity_type_mapped_to_a_stored_procedure()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>()
            .Ignore(a => a.FavoritePerson)
            .HasNoKey()
            .InsertUsingStoredProcedure(
                s => s
                    .HasParameter(c => c.Id)
                    .HasParameter(c => c.Name));

        VerifyError(
            RelationalStrings.StoredProcedureKeyless(nameof(Animal), "Animal_Insert"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_tableless_entity_type_mapped_to_some_stored_procedures()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>()
            .Ignore(a => a.FavoritePerson)
            .ToTable((string)null)
            .InsertUsingStoredProcedure(s => s.HasParameter(c => c.Id).HasParameter(c => c.Name))
            .UpdateUsingStoredProcedure(s => s.HasOriginalValueParameter(c => c.Id).HasParameter(c => c.Name))
            .Property(a => a.Id).ValueGeneratedNever();

        VerifyError(
            RelationalStrings.StoredProcedureUnmapped(nameof(Animal)),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_derived_entity_type_mapped_to_a_stored_procedure_in_TPH()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>().HasDiscriminator<int>("Discriminator");
        modelBuilder.Entity<Cat>().UpdateUsingStoredProcedure("Update", s => s.HasParameter(c => c.Breed));

        VerifyError(
            RelationalStrings.TphStoredProcedureMismatch(nameof(Cat), "Update", nameof(Animal), ""),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_derived_entity_type_mapped_to_a_different_stored_procedure_in_TPH()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>().UpdateUsingStoredProcedure("Update", s => s.HasParameter(c => c.Id))
            .HasDiscriminator<string>("Discriminator");
        modelBuilder.Entity<Cat>().UpdateUsingStoredProcedure("Update2", s => s.HasParameter(c => c.Breed));

        VerifyError(
            RelationalStrings.TphStoredProcedureMismatch(nameof(Cat), "Update2", nameof(Animal), "Update"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_derived_entity_type_mapped_to_a_different_stored_procedure_instance_in_TPH()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>().UpdateUsingStoredProcedure("Update", s => s.HasParameter(c => c.Id));
        modelBuilder.Entity<Cat>().UpdateUsingStoredProcedure("Update", s => s.HasParameter(c => c.Breed));

        VerifyError(
            RelationalStrings.StoredProcedureTphDuplicate(nameof(Cat), nameof(Animal), "Update"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_missing_generated_stored_procedure_parameters_in_TPH()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>()
            .UpdateUsingStoredProcedure(
                "Update", s => s
                    .HasOriginalValueParameter(a => a.Id, p => p.HasName("MyId"))
                    .HasParameter(a => a.Name)
                    .HasParameter((Cat c) => c.Breed)
                    .HasResultColumn(a => a.Name))
            .Property(a => a.Name).ValueGeneratedOnUpdate().Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Save);
        modelBuilder.Entity<Cat>();

        VerifyError(
            RelationalStrings.StoredProcedurePropertiesNotMapped(nameof(Animal), "Update", "{'FavoritePersonId', 'Identity'}"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_unmatched_stored_procedure_parameters_in_TPH()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>()
            .UpdateUsingStoredProcedure("Update", s => s.HasParameter("Missing"))
            .Property(a => a.Name).ValueGeneratedOnUpdate();

        VerifyError(
            RelationalStrings.StoredProcedureParameterNotFound("Missing", nameof(Animal), "Update"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_non_key_delete_stored_procedure_params_in_TPH()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>()
            .DeleteUsingStoredProcedure(
                s => s
                    .HasOriginalValueParameter(a => a.Id)
                    .HasOriginalValueParameter(a => a.Name))
            .Property(a => a.Name).ValueGeneratedOnUpdate();

        VerifyError(
            RelationalStrings.StoredProcedureDeleteNonKeyProperty(nameof(Animal), nameof(Animal.Name), "Animal_Delete"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_unmatched_stored_procedure_result_columns_in_TPH()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>()
            .UpdateUsingStoredProcedure("Update", "dbo", s => s.HasResultColumn("Missing"));

        VerifyError(
            RelationalStrings.StoredProcedureResultColumnNotFound("Missing", nameof(Animal), "dbo.Update"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_duplicate_parameter()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>()
            .InsertUsingStoredProcedure(
                s => s
                    .HasParameter(a => a.Id, p => p.IsOutput())
                    .HasRowsAffectedParameter(c => c.HasName("Id"))
                    .HasParameter("FavoritePersonId"));

        VerifyError(
            RelationalStrings.StoredProcedureDuplicateParameterName("Id", "Animal_Insert"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_duplicate_result_column()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>()
            .InsertUsingStoredProcedure(
                s => s
                    .HasResultColumn(a => a.Id, c => c.HasName("Id"))
                    .HasRowsAffectedResultColumn(c => c.HasName("Id"))
                    .HasParameter("FavoritePersonId"));

        VerifyError(
            RelationalStrings.StoredProcedureDuplicateResultColumnName("Id", "Animal_Insert"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_non_generated_insert_stored_procedure_result_columns_in_TPH()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>()
            .InsertUsingStoredProcedure(s => s.HasResultColumn(a => a.Name));
        modelBuilder.Entity<Cat>();

        VerifyError(
            RelationalStrings.StoredProcedureResultColumnNotGenerated(nameof(Animal), nameof(Animal.Name), "Animal_Insert"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_non_generated_update_stored_procedure_result_columns_in_TPT()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>()
            .Ignore(a => a.FavoritePerson)
            .UpdateUsingStoredProcedure(
                s => s
                    .HasOriginalValueParameter(a => a.Id)
                    .HasParameter(a => a.Name))
            .UseTptMappingStrategy();
        modelBuilder.Entity<Cat>()
            .UpdateUsingStoredProcedure("Update", s => s.HasResultColumn(c => c.Breed));

        VerifyError(
            RelationalStrings.StoredProcedureResultColumnNotGenerated(nameof(Cat), nameof(Cat.Breed), "Update"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_non_generated_insert_stored_procedure_output_parameter_in_TPC()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>()
            .InsertUsingStoredProcedure(s => s.HasParameter(a => a.Name, p => p.IsOutput()))
            .UseTpcMappingStrategy();
        modelBuilder.Entity<Cat>();

        VerifyError(
            RelationalStrings.StoredProcedureOutputParameterNotGenerated(nameof(Animal), nameof(Animal.Name), "Animal_Insert"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_non_generated_update_stored_procedure_input_output_parameter()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>()
            .Ignore(a => a.FavoritePerson)
            .UpdateUsingStoredProcedure(
                s => s
                    .HasOriginalValueParameter(a => a.Id)
                    .HasParameter(a => a.Name, p => p.IsInputOutput()));

        VerifyError(
            RelationalStrings.StoredProcedureOutputParameterNotGenerated(nameof(Animal), nameof(Animal.Name), "Animal_Update"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_delete_stored_procedure_result_columns_in_TPH()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>()
            .DeleteUsingStoredProcedure("Delete", s => s.HasResultColumn(a => a.Name))
            .Property(a => a.Name).ValueGeneratedOnUpdate();

        VerifyError(
            RelationalStrings.StoredProcedureResultColumnDelete(nameof(Animal), nameof(Animal.Name), "Delete"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_generated_properties_mapped_to_result_and_parameter()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>()
            .UpdateUsingStoredProcedure(
                s => s
                    .HasOriginalValueParameter(a => a.Id)
                    .HasParameter(a => a.Name, p => p.IsInputOutput())
                    .HasResultColumn(a => a.Name))
            .Property(a => a.Name).ValueGeneratedOnUpdate().Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Save);
        ;

        VerifyError(
            RelationalStrings.StoredProcedureResultColumnParameterConflict(nameof(Animal), nameof(Animal.Name), "Animal_Update"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_generated_properties_mapped_to_original_and_current_parameter()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>()
            .UpdateUsingStoredProcedure(
                s => s
                    .HasOriginalValueParameter(a => a.Id)
                    .HasParameter(a => a.Name, p => p.IsOutput())
                    .HasOriginalValueParameter(a => a.Name, p => p.IsInputOutput().HasName("OriginalName")))
            .Property(a => a.Name).ValueGeneratedOnUpdate();

        VerifyError(
            RelationalStrings.StoredProcedureOutputParameterConflict(nameof(Animal), nameof(Animal.Name), "Animal_Update"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_original_value_parameter_on_insert_stored_procedure()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>()
            .InsertUsingStoredProcedure(
                s => s
                    .HasParameter(a => a.Id)
                    .HasOriginalValueParameter(a => a.Name));

        VerifyError(
            RelationalStrings.StoredProcedureOriginalValueParameterOnInsert(nameof(Animal.Name) + "_Original", "Animal_Insert"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_current_value_parameter_on_delete_stored_procedure()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>()
            .DeleteUsingStoredProcedure(s => s.HasParameter(a => a.Id));

        VerifyError(
            RelationalStrings.StoredProcedureCurrentValueParameterOnDelete(nameof(Animal.Id), "Animal_Delete"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_unmapped_concurrency_token()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>()
            .UpdateUsingStoredProcedure(
                s => s
                    .HasOriginalValueParameter(a => a.Id)
                    .HasParameter("FavoritePersonId")
                    .HasParameter(a => a.Name, p => p.IsOutput())
                    .HasRowsAffectedReturnValue())
            .Property(a => a.Name).IsRowVersion();

        VerifyWarning(
            RelationalResources.LogStoredProcedureConcurrencyTokenNotMapped(new TestLogger<TestRelationalLoggingDefinitions>())
                .GenerateMessage(nameof(Animal), "Animal_Update", nameof(Animal.Name)), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_rows_affected_with_result_columns()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>()
            .UpdateUsingStoredProcedure(
                s => s
                    .HasOriginalValueParameter(a => a.Id)
                    .HasParameter("FavoritePersonId")
                    .HasResultColumn(a => a.Name)
                    .HasRowsAffectedReturnValue())
            .Property(a => a.Name).ValueGeneratedOnUpdate();

        VerifyError(
            RelationalStrings.StoredProcedureRowsAffectedWithResultColumns(nameof(Animal), "Animal_Update"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_rows_affected_on_insert_stored_procedure()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>()
            .InsertUsingStoredProcedure(
                s => s
                    .HasParameter(a => a.Id, pb => pb.IsOutput())
                    .HasParameter("FavoritePersonId")
                    .HasParameter(a => a.Name)
                    .HasRowsAffectedReturnValue());

        VerifyError(
            RelationalStrings.StoredProcedureRowsAffectedForInsert("Animal_Insert"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_stored_procedure_input_parameter_for_insert_non_save_property()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>()
            .InsertUsingStoredProcedure(
                "Insert",
                s => s
                    .HasParameter(a => a.Id, pb => pb.IsOutput())
                    .HasParameter("FavoritePersonId")
                    .HasParameter(a => a.Name))
            .Property(b => b.Name).Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);

        VerifyError(
            RelationalStrings.StoredProcedureInputParameterForInsertNonSaveProperty(
                nameof(Animal.Name), "Insert", nameof(Animal.Name), nameof(Animal), nameof(PropertySaveBehavior.Ignore)),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_stored_procedure_input_parameter_for_update_non_save_property()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>()
            .UpdateUsingStoredProcedure(
                "Update",
                s => s
                    .HasOriginalValueParameter(a => a.Id)
                    .HasParameter("FavoritePersonId")
                    .HasParameter(a => a.Name))
            .Property(b => b.Name).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

        VerifyError(
            RelationalStrings.StoredProcedureInputParameterForUpdateNonSaveProperty(
                nameof(Animal.Name), "Update", nameof(Animal.Name), nameof(Animal), nameof(PropertySaveBehavior.Ignore)),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_for_stored_procedure_without_parameter_for_insert_non_save_property()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>()
            .InsertUsingStoredProcedure(
                "Insert",
                s => s
                    .HasParameter(a => a.Id, pb => pb.IsOutput())
                    .HasParameter("FavoritePersonId"))
            .Property(b => b.Name).Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_for_stored_procedure_without_parameter_for_update_non_save_property()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>()
            .UpdateUsingStoredProcedure(
                "Update",
                s => s
                    .HasOriginalValueParameter(a => a.Id)
                    .HasParameter("FavoritePersonId"))
            .Property(b => b.Name).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_on_valid_UsingDeleteStoredProcedure_in_TPT()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>()
            .UseTptMappingStrategy()
            .DeleteUsingStoredProcedure("Delete", s => s.HasOriginalValueParameter(a => a.Id))
            .Property(a => a.Name).ValueGeneratedOnUpdate();
        modelBuilder.Entity<Cat>()
            .DeleteUsingStoredProcedure(s => s.HasOriginalValueParameter(a => a.Id));

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_on_derived_entity_type_mapped_to_a_stored_procedure_in_TPT()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>().UseTptMappingStrategy();
        modelBuilder.Entity<Cat>().UpdateUsingStoredProcedure(
            "Update", s => s
                .HasOriginalValueParameter(c => c.Id)
                .HasParameter(c => c.Breed)
                .HasParameter(c => c.Identity));

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_on_derived_entity_type_not_mapped_to_a_stored_procedure_in_TPT()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>()
            .UseTptMappingStrategy()
            .UpdateUsingStoredProcedure(
                "Update", s => s
                    .HasOriginalValueParameter(a => a.Id, p => p.HasName("MyId"))
                    .HasParameter(a => a.Name)
                    .HasParameter("FavoritePersonId")
                    .HasResultColumn(a => a.Name))
            .Property(a => a.Name).ValueGeneratedOnUpdate().Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Save);
        ;
        modelBuilder.Entity<Cat>();

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_missing_generated_stored_procedure_parameters_in_TPT()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>()
            .UseTptMappingStrategy()
            .UpdateUsingStoredProcedure(
                "Update", s => s
                    .HasOriginalValueParameter(a => a.Id, p => p.HasName("MyId"))
                    .HasParameter(a => a.Name)
                    .HasParameter("FavoritePersonId")
                    .HasResultColumn(a => a.Name))
            .Property(a => a.Name).ValueGeneratedOnUpdate().Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Save);
        ;
        modelBuilder.Entity<Cat>()
            .UpdateUsingStoredProcedure(s => s.HasParameter(c => c.Breed));

        VerifyError(
            RelationalStrings.StoredProcedurePropertiesNotMapped(nameof(Cat), "Cat_Update", "{'Identity', 'Id'}"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_missing_stored_procedure_parameters_for_abstract_properties_in_TPT()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<Abstract>().UseTptMappingStrategy();
        modelBuilder.Entity<Generic<string>>()
            .UpdateUsingStoredProcedure(
                "Update", s => s
                    .HasOriginalValueParameter(a => a.Id));

        VerifyError(
            RelationalStrings.StoredProcedurePropertiesNotMapped("Generic<string>", "Update", "{'P0', 'P1', 'P2', 'P3'}"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_unmatched_stored_procedure_parameters_in_TPT()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>()
            .UseTptMappingStrategy()
            .UpdateUsingStoredProcedure("Update", s => s.HasOriginalValueParameter((Cat c) => c.Breed));
        modelBuilder.Entity<Cat>();

        VerifyError(
            RelationalStrings.StoredProcedureParameterNotFound(nameof(Cat.Breed), nameof(Animal), "Update"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_unmatched_stored_procedure_result_columns_in_TPT()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>()
            .UpdateUsingStoredProcedure("Update", "dbo", s => s.HasResultColumn((Cat c) => c.Breed))
            .UseTptMappingStrategy();
        modelBuilder.Entity<Cat>();

        VerifyError(
            RelationalStrings.StoredProcedureResultColumnNotFound(nameof(Cat.Breed), nameof(Animal), "dbo.Update"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_InsertUsingStoredProcedure_without_a_name()
    {
        var modelBuilder = CreateConventionModelBuilder();
        var entityType = (IConventionEntityType)modelBuilder.Entity<Abstract>().UseTpcMappingStrategy().Metadata;
        modelBuilder.Entity<Generic<string>>();
        entityType.SetInsertStoredProcedure();

        VerifyError(
            RelationalStrings.StoredProcedureNoName(nameof(Abstract), "InsertStoredProcedure"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_missing_generated_stored_procedure_parameters()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>()
            .UpdateUsingStoredProcedure(
                "Update", s => s
                    .HasOriginalValueParameter(a => a.Id, p => p.HasName("MyId"))
                    .HasParameter(a => a.Name)
                    .HasParameter("FavoritePersonId")
                    .HasParameter(a => a.Name))
            .Property(a => a.Name).ValueGeneratedOnUpdate().Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Save);

        VerifyError(
            RelationalStrings.StoredProcedureGeneratedPropertiesNotMapped(
                nameof(Animal),
                "Update", "{'Name'}"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_missing_generated_stored_procedure_parameters_in_TPC()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>()
            .UseTpcMappingStrategy()
            .UpdateUsingStoredProcedure(
                "Update", s => s
                    .HasOriginalValueParameter(a => a.Id, p => p.HasName("MyId"))
                    .HasParameter(a => a.Name)
                    .HasParameter("FavoritePersonId")
                    .HasResultColumn(a => a.Name))
            .Property(a => a.Name).ValueGeneratedOnUpdate().Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Save);
        modelBuilder.Entity<Cat>()
            .UpdateUsingStoredProcedure(
                s => s
                    .HasResultColumn(a => a.Name)
                    .HasParameter(c => c.Breed)
                    .HasParameter(a => a.Name));

        VerifyError(
            RelationalStrings.StoredProcedurePropertiesNotMapped(nameof(Cat), "Cat_Update", "{'Identity', 'Id', 'FavoritePersonId'}"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_unmatched_stored_procedure_parameters_in_TPC()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>()
            .UseTpcMappingStrategy()
            .UpdateUsingStoredProcedure("Update", s => s.HasParameter((Cat c) => c.Breed));
        modelBuilder.Entity<Cat>();

        VerifyError(
            RelationalStrings.StoredProcedureParameterNotFound(nameof(Cat.Breed), nameof(Animal), "Update"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_unmatched_stored_procedure_result_columns_in_TPC()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>()
            .UpdateUsingStoredProcedure("Update", "dbo", s => s.HasResultColumn((Cat c) => c.Breed))
            .UseTpcMappingStrategy();
        modelBuilder.Entity<Cat>();

        VerifyError(
            RelationalStrings.StoredProcedureResultColumnNotFound(nameof(Cat.Breed), nameof(Animal), "dbo.Update"),
            modelBuilder);
    }

    [ConditionalFact]
    public void Passes_for_unnamed_index_with_all_properties_not_mapped_to_any_table()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<Animal>().ToTable((string)null);
        modelBuilder.Entity<Animal>().HasIndex(nameof(Animal.Id), nameof(Animal.Name));

        var definition = RelationalResources
            .LogUnnamedIndexAllPropertiesNotToMappedToAnyTable(
                new TestLogger<TestRelationalLoggingDefinitions>());
        VerifyWarning(
            definition.GenerateMessage(
                nameof(Animal),
                "{'Id', 'Name'}"),
            modelBuilder);
    }

    [ConditionalFact]
    public void Passes_for_named_index_with_all_properties_not_mapped_to_any_table()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<Animal>().ToTable((string)null);
        modelBuilder.Entity<Animal>()
            .HasIndex(
                [nameof(Animal.Id), nameof(Animal.Name)],
                "IX_AllPropertiesNotMapped");

        var definition = RelationalResources
            .LogNamedIndexAllPropertiesNotToMappedToAnyTable(
                new TestLogger<TestRelationalLoggingDefinitions>());
        VerifyWarning(
            definition.GenerateMessage(
                "IX_AllPropertiesNotMapped",
                nameof(Animal),
                "{'Id', 'Name'}"),
            modelBuilder);
    }

    [ConditionalFact]
    public void Passes_for_mix_of_index_properties_declared_and_inherited_TPT()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<Animal>().ToTable((string)null);
        modelBuilder.Entity<Cat>().ToTable("Cats")
            .HasIndex(
                [nameof(Cat.Identity), nameof(Animal.Name)],
                "IX_MixOfMappedAndUnmappedProperties");

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public void Detects_mix_of_index_property_mapped_and_not_mapped_to_any_table_mapped_first()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<Animal>();
        modelBuilder.Entity<Cat>().ToTable((string)null)
            .HasIndex(nameof(Animal.Name), nameof(Cat.Identity));

        var definition = RelationalResources
            .LogUnnamedIndexPropertiesBothMappedAndNotMappedToTable(
                new TestLogger<TestRelationalLoggingDefinitions>());
        VerifyWarning(
            definition.GenerateMessage(
                nameof(Cat),
                "{'Name', 'Identity'}",
                "Identity"),
            modelBuilder,
            LogLevel.Error);
    }

    [ConditionalFact]
    public void Detects_mix_of_index_property_mapped_and_not_mapped_to_any_table_unmapped_first()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<Animal>();
        modelBuilder.Entity<Cat>().ToTable((string)null)
            .HasIndex(
                [nameof(Cat.Identity), nameof(Animal.Name)],
                "IX_MixOfMappedAndUnmappedProperties");

        var definition = RelationalResources
            .LogNamedIndexPropertiesBothMappedAndNotMappedToTable(
                new TestLogger<TestRelationalLoggingDefinitions>());
        VerifyWarning(
            definition.GenerateMessage(
                "IX_MixOfMappedAndUnmappedProperties",
                nameof(Cat),
                "{'Identity', 'Name'}",
                "Identity"),
            modelBuilder,
            LogLevel.Error);
    }

    [ConditionalFact]
    public void Passes_for_index_properties_mapped_to_same_table_in_TPT_hierarchy()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<Animal>().ToTable("Animals");
        modelBuilder.Entity<Cat>().ToTable("Cats")
            .HasIndex(nameof(Animal.Id), nameof(Cat.Identity));

        Validate(modelBuilder);

        Assert.Empty(
            LoggerFactory.Log
                .Where(l => l.Level != LogLevel.Trace && l.Level != LogLevel.Debug));
    }

    [ConditionalFact]
    public void Detects_unnamed_index_properties_mapped_to_different_tables_in_TPT_hierarchy()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<Animal>().ToTable("Animals");
        modelBuilder.Entity<Cat>().ToTable("Cats");
        modelBuilder.Entity<Cat>().HasIndex(nameof(Animal.Name), nameof(Cat.Identity));

        var definition = RelationalResources
            .LogUnnamedIndexPropertiesMappedToNonOverlappingTables(
                new TestLogger<TestRelationalLoggingDefinitions>());
        VerifyWarning(
            definition.GenerateMessage(
                nameof(Cat),
                "{'Name', 'Identity'}",
                nameof(Animal.Name),
                "{'Animals'}",
                nameof(Cat.Identity),
                "{'Cats'}"),
            modelBuilder,
            LogLevel.Error);
    }

    [ConditionalFact]
    public void Detects_named_index_properties_mapped_to_different_tables_in_TPT_hierarchy()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<Animal>().ToTable("Animals");
        modelBuilder.Entity<Cat>().ToTable("Cats");
        modelBuilder.Entity<Cat>()
            .HasIndex(
                [nameof(Animal.Name), nameof(Cat.Identity)],
                "IX_MappedToDifferentTables");

        var definition = RelationalResources
            .LogNamedIndexPropertiesMappedToNonOverlappingTables(
                new TestLogger<TestRelationalLoggingDefinitions>());
        VerifyWarning(
            definition.GenerateMessage(
                l => l.Log(
                    definition.Level,
                    definition.EventId,
                    definition.MessageFormat,
                    "IX_MappedToDifferentTables",
                    nameof(Cat),
                    "{'Name', 'Identity'}",
                    nameof(Animal.Name),
                    "{'Animals'}",
                    nameof(Cat.Identity),
                    "{'Cats'}")),
            modelBuilder,
            LogLevel.Error);
    }

    [ConditionalFact]
    public virtual void Non_TPH_as_a_result_of_DbFunction_throws()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<A>().ToTable("A").HasNoDiscriminator();
        modelBuilder.Entity<C>().ToTable("C");

        modelBuilder.HasDbFunction(TestMethods.MethodFMi);

        VerifyError(
            RelationalStrings.TableValuedFunctionNonTph(
                TestMethods.MethodFMi.DeclaringType.FullName + "." + TestMethods.MethodFMi.Name + "()", "C"), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_for_relational_override_without_inheritance()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Person>(
            e =>
            {
                e.ToTable("foo");
                e.Property(p => p.Name).Metadata.SetColumnName("bar", StoreObjectIdentifier.Table("foo"));
            });

        Validate(modelBuilder);

        Assert.DoesNotContain(LoggerFactory.Log, l => l.Level == LogLevel.Warning);
    }

    [ConditionalFact]
    public virtual void Detects_duplicate_column_orders()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>(
            x =>
            {
                x.Property(a => a.Id).HasColumnOrder(0);
                x.Property(a => a.Name).HasColumnOrder(0);
            });

        var definition = RelationalResources.LogDuplicateColumnOrders(new TestLogger<TestRelationalLoggingDefinitions>());
        VerifyWarning(definition.GenerateMessage("Animal", "'Id', 'Name'"), modelBuilder, LogLevel.Error);
    }

    [ConditionalFact]
    public virtual void Throws_when_non_tph_entity_type_short_names_are_not_unique()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<TpcBase>().UseTpcMappingStrategy();
        modelBuilder.Entity<Outer.TpcDerived>().ToTable("TpcDerived1");
        modelBuilder.Entity<Outer2.TpcDerived>().ToTable("TpcDerived2");

        VerifyError(
            RelationalStrings.EntityShortNameNotUnique(
                "Microsoft.EntityFrameworkCore.Infrastructure.RelationalModelValidatorTest+Outer2+TpcDerived",
                "TpcDerived",
                "Microsoft.EntityFrameworkCore.Infrastructure.RelationalModelValidatorTest+Outer+TpcDerived"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Throws_when_non_tph_entity_type_discriminator_set_to_non_string()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<TpcBase>().UseTpcMappingStrategy();
        modelBuilder.Entity<Outer.TpcDerived>().ToTable("TpcDerived1");
        modelBuilder.Entity<Outer2.TpcDerived>().ToTable("TpcDerived2");
        modelBuilder.Entity<TpcBase>().Metadata.SetDiscriminatorProperty(null);
        modelBuilder.Entity<Outer2.TpcDerived>().Metadata.SetDiscriminatorValue(1);

        VerifyError(RelationalStrings.NonTphDiscriminatorValueNotString(1, "TpcDerived"), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Store_generated_in_composite_key()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<CarbonComposite>(
            b =>
            {
                b.HasKey(e => new { e.Id1, e.Id2 });
                b.Property(e => e.Id2).ValueGeneratedOnAdd();
            });

        Validate(modelBuilder);

        var entityType = modelBuilder.Model.FindEntityType(typeof(CarbonComposite))!;
        var keyProperties = entityType.FindPrimaryKey()!.Properties;
        Assert.Equal(2, keyProperties.Count);
        Assert.Equal(nameof(CarbonComposite.Id1), keyProperties[0].Name);
        Assert.Equal(nameof(CarbonComposite.Id2), keyProperties[1].Name);
        Assert.Equal(ValueGenerated.Never, keyProperties[0].ValueGenerated);
        Assert.Equal(ValueGenerated.OnAdd, keyProperties[1].ValueGenerated);
    }

    [ConditionalFact]
    public void Detects_trigger_on_TPH_non_root()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<Animal>();
        modelBuilder.Entity<Cat>().ToTable(tb => tb.HasTrigger("SomeTrigger"));

        VerifyWarning(RelationalResources.LogTriggerOnNonRootTphEntity(new TestLogger<TestRelationalLoggingDefinitions>())
            .GenerateMessage("Cat", "Animal"), modelBuilder);
    }

    private class TpcBase
    {
        public int Id { get; set; }
    }

    private class Outer
    {
        public class TpcDerived : TpcBase
        {
            public string Value { get; set; }
        }
    }

    private class Outer2
    {
        public class TpcDerived : TpcBase
        {
            public string Value { get; set; }
        }
    }

    protected override void SetBaseType(IMutableEntityType entityType, IMutableEntityType baseEntityType)
    {
        base.SetBaseType(entityType, baseEntityType);

        baseEntityType.SetDiscriminatorProperty(baseEntityType.AddProperty("Discriminator", typeof(string)));
        baseEntityType.SetDiscriminatorValue(baseEntityType.Name);
        entityType.SetDiscriminatorValue(entityType.Name);
    }

    public class TestDecimalToLongConverter : ValueConverter<decimal, long>
    {
        private static readonly Expression<Func<decimal, long>> convertToProviderExpression = d => (long)(d * 100);
        private static readonly Expression<Func<long, decimal>> convertFromProviderExpression = l => l / 100m;

        public TestDecimalToLongConverter()
            : base(convertToProviderExpression, convertFromProviderExpression)
        {
        }
    }

    public class TestDecimalToDecimalConverter : ValueConverter<decimal, decimal>
    {
        private static readonly Expression<Func<decimal, decimal>> convertToProviderExpression = d => d * 100m;
        private static readonly Expression<Func<decimal, decimal>> convertFromProviderExpression = l => l / 100m;

        public TestDecimalToDecimalConverter()
            : base(convertToProviderExpression, convertFromProviderExpression)
        {
        }
    }

    private class BaseTestMethods
    {
        public static readonly MethodInfo MethodAMi = typeof(BaseTestMethods).GetTypeInfo().GetDeclaredMethod(nameof(MethodA));

        public static IQueryable<BaseTestMethods> MethodA()
            => throw new NotImplementedException();
    }

    private class DerivedTestMethods : TestMethods
    {
        public int Id { get; set; }
        public TestMethods SomeTestMethods { get; set; }
        public TestMethods OtherTestMethods { get; set; }
    }

    private class TestMethods : BaseTestMethods
    {
        public static new readonly MethodInfo MethodAMi = typeof(TestMethods).GetTypeInfo().GetDeclaredMethod(nameof(MethodA));
        public static readonly MethodInfo MethodBMi = typeof(TestMethods).GetTypeInfo().GetDeclaredMethod(nameof(MethodB));
        public static readonly MethodInfo MethodCMi = typeof(TestMethods).GetTypeInfo().GetDeclaredMethod(nameof(MethodC));
        public static readonly MethodInfo MethodDMi = typeof(TestMethods).GetTypeInfo().GetDeclaredMethod(nameof(MethodD));
        public static readonly MethodInfo MethodEMi = typeof(TestMethods).GetTypeInfo().GetDeclaredMethod(nameof(MethodE));
        public static readonly MethodInfo MethodFMi = typeof(TestMethods).GetTypeInfo().GetDeclaredMethod(nameof(MethodF));

        public static new IQueryable<TestMethods> MethodA()
            => throw new NotImplementedException();

        public static IQueryable<TestMethods> MethodB(int id)
            => throw new NotImplementedException();

        public static TestMethods MethodC()
            => throw new NotImplementedException();

        public static int MethodD(TestMethods methods)
            => throw new NotImplementedException();

        public static int MethodE()
            => throw new NotImplementedException();

        public static IQueryable<C> MethodF()
            => throw new NotImplementedException();
    }

    protected virtual TestHelpers.TestModelBuilder CreateModelBuilderWithoutConvention<T>(bool sensitiveDataLoggingEnabled = false)
        => TestHelpers.CreateConventionBuilder(
            CreateModelLogger(sensitiveDataLoggingEnabled), CreateValidationLogger(sensitiveDataLoggingEnabled),
            modelConfigurationBuilder => ConventionSet.Remove(
                modelConfigurationBuilder.ConventionSet.ModelFinalizingConventions,
                typeof(T)));

    protected override TestHelpers TestHelpers
        => FakeRelationalTestHelpers.Instance;
}
