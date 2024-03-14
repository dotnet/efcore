// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class KeysWithConvertersCosmosTest(KeysWithConvertersCosmosTest.KeysWithConvertersCosmosFixture fixture) : KeysWithConvertersTestBase<KeysWithConvertersCosmosTest.KeysWithConvertersCosmosFixture>(fixture)
{
    [ConditionalFact(Skip = "Issue=#16920 (Include)")]
    public override Task Can_insert_and_read_back_with_struct_key_and_optional_dependents()
        => base.Can_insert_and_read_back_with_struct_key_and_optional_dependents();

    [ConditionalFact(Skip = "Issue=#16920 (Include)")]
    public override Task Can_insert_and_read_back_with_comparable_struct_key_and_optional_dependents()
        => base.Can_insert_and_read_back_with_comparable_struct_key_and_optional_dependents();

    [ConditionalFact(Skip = "Issue=#16920 (Include)")]
    public override Task Can_insert_and_read_back_with_generic_comparable_struct_key_and_optional_dependents()
        => base.Can_insert_and_read_back_with_generic_comparable_struct_key_and_optional_dependents();

    [ConditionalFact(Skip = "Issue=#16920 (Include)")]
    public override Task Can_insert_and_read_back_with_struct_key_and_required_dependents()
        => base.Can_insert_and_read_back_with_struct_key_and_required_dependents();

    [ConditionalFact(Skip = "Issue=#16920 (Include)")]
    public override Task Can_insert_and_read_back_with_comparable_struct_key_and_required_dependents()
        => base.Can_insert_and_read_back_with_comparable_struct_key_and_required_dependents();

    [ConditionalFact(Skip = "Issue=#16920 (Include)")]
    public override Task Can_insert_and_read_back_with_generic_comparable_struct_key_and_required_dependents()
        => base.Can_insert_and_read_back_with_generic_comparable_struct_key_and_required_dependents();

    [ConditionalFact(Skip = "Issue=#16920 (Include)")]
    public override Task Can_insert_and_read_back_with_class_key_and_optional_dependents()
        => base.Can_insert_and_read_back_with_class_key_and_optional_dependents();

    [ConditionalFact(Skip = "Issue=#16920 (Include)")]
    public override Task Can_insert_and_read_back_with_comparable_class_key_and_optional_dependents()
        => base.Can_insert_and_read_back_with_comparable_class_key_and_optional_dependents();

    [ConditionalFact(Skip = "Issue=#16920 (Include)")]
    public override Task Can_insert_and_read_back_with_comparable_struct_binary_key_and_optional_dependents()
        => base.Can_insert_and_read_back_with_comparable_struct_binary_key_and_optional_dependents();

    [ConditionalFact(Skip = "Issue=#16920 (Include)")]
    public override Task Can_insert_and_read_back_with_comparable_struct_binary_key_and_required_dependents()
        => base.Can_insert_and_read_back_with_comparable_struct_binary_key_and_required_dependents();

    [ConditionalFact(Skip = "Issue=#16920 (Include)")]
    public override Task Can_insert_and_read_back_with_generic_comparable_struct_binary_key_and_optional_dependents()
        => base.Can_insert_and_read_back_with_generic_comparable_struct_binary_key_and_optional_dependents();

    [ConditionalFact(Skip = "Issue=#16920 (Include)")]
    public override Task Can_insert_and_read_back_with_generic_comparable_struct_binary_key_and_required_dependents()
        => base.Can_insert_and_read_back_with_generic_comparable_struct_binary_key_and_required_dependents();

    [ConditionalFact(Skip = "Issue=#16920 (Include)")]
    public override Task Can_insert_and_read_back_with_structural_struct_binary_key_and_optional_dependents()
        => base.Can_insert_and_read_back_with_structural_struct_binary_key_and_optional_dependents();

    [ConditionalFact(Skip = "Issue=#16920 (Include)")]
    public override Task Can_insert_and_read_back_with_structural_struct_binary_key_and_required_dependents()
        => base.Can_insert_and_read_back_with_structural_struct_binary_key_and_required_dependents();

    [ConditionalFact(Skip = "Issue=#16920 (Include)")]
    public override Task Can_insert_and_read_back_with_struct_binary_key_and_optional_dependents()
        => base.Can_insert_and_read_back_with_struct_binary_key_and_optional_dependents();

    [ConditionalFact(Skip = "Issue=#16920 (Include)")]
    public override Task Can_insert_and_read_back_with_struct_binary_key_and_required_dependents()
        => base.Can_insert_and_read_back_with_struct_binary_key_and_required_dependents();

    [ConditionalFact(Skip = "Issue=#16920 (Include)")]
    public override Task Can_insert_and_read_back_with_comparable_struct_binary_key_and_optional_dependents_with_shadow_FK()
        => base.Can_insert_and_read_back_with_comparable_struct_binary_key_and_optional_dependents_with_shadow_FK();

    [ConditionalFact(Skip = "Issue=#16920 (Include)")]
    public override Task Can_insert_and_read_back_with_comparable_struct_binary_key_and_required_dependents_with_shadow_FK()
        => base.Can_insert_and_read_back_with_comparable_struct_binary_key_and_required_dependents_with_shadow_FK();

    [ConditionalFact(Skip = "Issue=#16920 (Include)")]
    public override Task Can_insert_and_read_back_with_comparable_struct_key_and_optional_dependents_with_shadow_FK()
        => base.Can_insert_and_read_back_with_comparable_struct_key_and_optional_dependents_with_shadow_FK();

    [ConditionalFact(Skip = "Issue=#16920 (Include)")]
    public override Task Can_insert_and_read_back_with_comparable_struct_key_and_required_dependents_with_shadow_FK()
        => base.Can_insert_and_read_back_with_comparable_struct_key_and_required_dependents_with_shadow_FK();

    [ConditionalFact(Skip = "Issue=#16920 (Include)")]
    public override Task Can_insert_and_read_back_with_generic_comparable_struct_binary_key_and_optional_dependents_with_shadow_FK()
        => base.Can_insert_and_read_back_with_generic_comparable_struct_binary_key_and_optional_dependents_with_shadow_FK();

    [ConditionalFact(Skip = "Issue=#16920 (Include)")]
    public override Task Can_insert_and_read_back_with_generic_comparable_struct_binary_key_and_required_dependents_with_shadow_FK()
        => base.Can_insert_and_read_back_with_generic_comparable_struct_binary_key_and_required_dependents_with_shadow_FK();

    [ConditionalFact(Skip = "Issue=#16920 (Include)")]
    public override Task Can_insert_and_read_back_with_generic_comparable_struct_key_and_optional_dependents_with_shadow_FK()
        => base.Can_insert_and_read_back_with_generic_comparable_struct_key_and_optional_dependents_with_shadow_FK();

    [ConditionalFact(Skip = "Issue=#16920 (Include)")]
    public override Task Can_insert_and_read_back_with_generic_comparable_struct_key_and_required_dependents_with_shadow_FK()
        => base.Can_insert_and_read_back_with_generic_comparable_struct_key_and_required_dependents_with_shadow_FK();

    [ConditionalFact(Skip = "Issue=#16920 (Include)")]
    public override Task Can_insert_and_read_back_with_struct_binary_key_and_optional_dependents_with_shadow_FK()
        => base.Can_insert_and_read_back_with_struct_binary_key_and_optional_dependents_with_shadow_FK();

    [ConditionalFact(Skip = "Issue=#16920 (Include)")]
    public override Task Can_insert_and_read_back_with_struct_key_and_optional_dependents_with_shadow_FK()
        => base.Can_insert_and_read_back_with_struct_key_and_optional_dependents_with_shadow_FK();

    [ConditionalFact(Skip = "Issue=#16920 (Include)")]
    public override Task Can_insert_and_read_back_with_struct_binary_key_and_required_dependents_with_shadow_FK()
        => base.Can_insert_and_read_back_with_struct_binary_key_and_required_dependents_with_shadow_FK();

    [ConditionalFact(Skip = "Issue=#16920 (Include)")]
    public override Task Can_insert_and_read_back_with_struct_key_and_required_dependents_with_shadow_FK()
        => base.Can_insert_and_read_back_with_struct_key_and_required_dependents_with_shadow_FK();

    [ConditionalFact(Skip = "Issue=#16920 (Include)")]
    public override Task Can_insert_and_read_back_with_structural_struct_binary_key_and_optional_dependents_with_shadow_FK()
        => base.Can_insert_and_read_back_with_structural_struct_binary_key_and_optional_dependents_with_shadow_FK();

    [ConditionalFact(Skip = "Issue=#16920 (Include)")]
    public override Task Can_insert_and_read_back_with_structural_struct_binary_key_and_required_dependents_with_shadow_FK()
        => base.Can_insert_and_read_back_with_structural_struct_binary_key_and_required_dependents_with_shadow_FK();

    [ConditionalFact(Skip = "Issue=#26239")]
    public override Task Can_insert_and_read_back_with_bare_class_key_and_optional_dependents()
        => base.Can_insert_and_read_back_with_bare_class_key_and_optional_dependents();

    [ConditionalFact(Skip = "Issue=#26239")]
    public override Task Can_insert_and_read_back_with_class_key_and_optional_dependents_with_shadow_FK()
        => base.Can_insert_and_read_back_with_class_key_and_optional_dependents_with_shadow_FK();

    [ConditionalFact(Skip = "Issue=#26239")]
    public override Task Can_insert_and_read_back_with_bare_class_key_and_optional_dependents_with_shadow_FK()
        => base.Can_insert_and_read_back_with_bare_class_key_and_optional_dependents_with_shadow_FK();

    [ConditionalFact(Skip = "Issue=#26239")]
    public override Task Can_insert_and_read_back_with_comparable_class_key_and_optional_dependents_with_shadow_FK()
        => base.Can_insert_and_read_back_with_comparable_class_key_and_optional_dependents_with_shadow_FK();

    [ConditionalFact(Skip = "Issue=#26239")]
    public override Task Can_insert_and_read_back_with_enumerable_class_key_and_optional_dependents()
        => base.Can_insert_and_read_back_with_enumerable_class_key_and_optional_dependents();

    public class KeysWithConvertersCosmosFixture : KeysWithConvertersFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => CosmosTestStoreFactory.Instance;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.Entity<IntStructKeyPrincipal>(
                b => { b.Property(e => e.Id).HasConversion(IntStructKey.Converter); });

            modelBuilder.Entity<IntStructKeyOptionalDependent>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(IntStructKey.Converter);
                    b.Property(e => e.PrincipalId).HasConversion(IntStructKey.Converter);
                });

            modelBuilder.Entity<IntStructKeyRequiredDependent>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(IntStructKey.Converter);
                    b.Property(e => e.PrincipalId).HasConversion(IntStructKey.Converter);
                });

            // modelBuilder.Entity<IntClassKeyPrincipal>(
            //     b =>
            //         {
            //             b.Property(e => e.Id).HasConversion(IntClassKey.Converter);
            //             b.HasMany(e => e.OptionalDependents).WithOne(e => e.Principal).HasForeignKey(e => e.PrincipalId);
            //             b.HasMany(e => e.RequiredDependents).WithOne(e => e.Principal).HasForeignKey(e => e.PrincipalId);
            //         });
            //
            // modelBuilder.Entity<IntClassKeyOptionalDependent>(
            //     b =>
            //         {
            //             b.Property(e => e.Id).HasConversion(IntClassKey.Converter);
            //             b.Property(e => e.PrincipalId).HasConversion(IntClassKey.Converter);
            //         });
            //
            // modelBuilder.Entity<IntClassKeyRequiredDependent>(
            //     b =>
            //         {
            //             b.Property(e => e.Id).HasConversion(IntClassKey.Converter);
            //             b.Property(e => e.PrincipalId).HasConversion(IntClassKey.Converter);
            //         });

            // modelBuilder.Entity<BareIntClassKeyPrincipal>(
            //     b => { b.Property(e => e.Id).HasConversion(BareIntClassKey.Converter, BareIntClassKey.Comparer); });
            //
            // modelBuilder.Entity<BareIntClassKeyOptionalDependent>(
            //     b =>
            //         {
            //             b.Property(e => e.Id).HasConversion(BareIntClassKey.Converter, BareIntClassKey.Comparer);
            //             b.Property(e => e.PrincipalId).HasConversion(BareIntClassKey.Converter, BareIntClassKey.Comparer);
            //         });
            //
            // modelBuilder.Entity<BareIntClassKeyRequiredDependent>(
            //     b =>
            //         {
            //             b.Property(e => e.Id).HasConversion(BareIntClassKey.Converter, BareIntClassKey.Comparer);
            //             b.Property(e => e.PrincipalId).HasConversion(BareIntClassKey.Converter, BareIntClassKey.Comparer);
            //         });

            modelBuilder.Entity<ComparableIntStructKeyPrincipal>(
                b => { b.Property(e => e.Id).HasConversion(ComparableIntStructKey.Converter); });

            modelBuilder.Entity<ComparableIntStructKeyOptionalDependent>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(ComparableIntStructKey.Converter);
                    b.Property(e => e.PrincipalId).HasConversion(ComparableIntStructKey.Converter);
                });

            modelBuilder.Entity<ComparableIntStructKeyRequiredDependent>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(ComparableIntStructKey.Converter);
                    b.Property(e => e.PrincipalId).HasConversion(ComparableIntStructKey.Converter);
                });

            modelBuilder.Entity<GenericComparableIntStructKeyPrincipal>(
                b => { b.Property(e => e.Id).HasConversion(GenericComparableIntStructKey.Converter); });

            modelBuilder.Entity<GenericComparableIntStructKeyOptionalDependent>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(GenericComparableIntStructKey.Converter);
                    b.Property(e => e.PrincipalId).HasConversion(GenericComparableIntStructKey.Converter);
                });

            modelBuilder.Entity<GenericComparableIntStructKeyRequiredDependent>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(GenericComparableIntStructKey.Converter);
                    b.Property(e => e.PrincipalId).HasConversion(GenericComparableIntStructKey.Converter);
                });

            modelBuilder.Entity<StructuralComparableBytesStructKeyPrincipal>(
                b => { b.Property(e => e.Id).HasConversion(StructuralComparableBytesStructKey.Converter); });

            modelBuilder.Entity<StructuralComparableBytesStructKeyOptionalDependent>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(StructuralComparableBytesStructKey.Converter);
                    b.Property(e => e.PrincipalId).HasConversion(StructuralComparableBytesStructKey.Converter);
                });

            modelBuilder.Entity<StructuralComparableBytesStructKeyRequiredDependent>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(StructuralComparableBytesStructKey.Converter);
                    b.Property(e => e.PrincipalId).HasConversion(StructuralComparableBytesStructKey.Converter);
                });

            modelBuilder.Entity<BytesStructKeyPrincipal>(
                b => { b.Property(e => e.Id).HasConversion(BytesStructKey.Converter); });

            modelBuilder.Entity<BytesStructKeyOptionalDependent>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(BytesStructKey.Converter);
                    b.Property(e => e.PrincipalId).HasConversion(BytesStructKey.Converter);
                });

            modelBuilder.Entity<BytesStructKeyRequiredDependent>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(BytesStructKey.Converter);
                    b.Property(e => e.PrincipalId).HasConversion(BytesStructKey.Converter);
                });

            modelBuilder.Entity<ComparableBytesStructKeyPrincipal>(
                b => { b.Property(e => e.Id).HasConversion(ComparableBytesStructKey.Converter); });

            modelBuilder.Entity<ComparableBytesStructKeyOptionalDependent>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(ComparableBytesStructKey.Converter);
                    b.Property(e => e.PrincipalId).HasConversion(ComparableBytesStructKey.Converter);
                });

            modelBuilder.Entity<ComparableBytesStructKeyRequiredDependent>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(ComparableBytesStructKey.Converter);
                    b.Property(e => e.PrincipalId).HasConversion(ComparableBytesStructKey.Converter);
                });

            modelBuilder.Entity<GenericComparableBytesStructKeyPrincipal>(
                b => { b.Property(e => e.Id).HasConversion(GenericComparableBytesStructKey.Converter); });

            modelBuilder.Entity<GenericComparableBytesStructKeyOptionalDependent>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(GenericComparableBytesStructKey.Converter);
                    b.Property(e => e.PrincipalId).HasConversion(GenericComparableBytesStructKey.Converter);
                });

            modelBuilder.Entity<GenericComparableBytesStructKeyRequiredDependent>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(GenericComparableBytesStructKey.Converter);
                    b.Property(e => e.PrincipalId).HasConversion(GenericComparableBytesStructKey.Converter);
                });

            // modelBuilder.Entity<ComparableIntClassKeyPrincipal>(
            //     b => { b.Property(e => e.Id).HasConversion(ComparableIntClassKey.Converter); });
            //
            // modelBuilder.Entity<ComparableIntClassKeyOptionalDependent>(
            //     b =>
            //         {
            //             b.Property(e => e.Id).HasConversion(ComparableIntClassKey.Converter);
            //             b.Property(e => e.PrincipalId).HasConversion(ComparableIntClassKey.Converter);
            //         });
            //
            // modelBuilder.Entity<ComparableIntClassKeyRequiredDependent>(
            //     b =>
            //         {
            //             b.Property(e => e.Id).HasConversion(ComparableIntClassKey.Converter);
            //             b.Property(e => e.PrincipalId).HasConversion(ComparableIntClassKey.Converter);
            //         });
            //
            // modelBuilder.Entity<GenericComparableIntClassKeyPrincipal>(
            //     b => { b.Property(e => e.Id).HasConversion(GenericComparableIntClassKey.Converter); });
            //
            // modelBuilder.Entity<GenericComparableIntClassKeyOptionalDependent>(
            //     b =>
            //         {
            //             b.Property(e => e.Id).HasConversion(GenericComparableIntClassKey.Converter);
            //             b.Property(e => e.PrincipalId).HasConversion(GenericComparableIntClassKey.Converter);
            //         });
            //
            // modelBuilder.Entity<GenericComparableIntClassKeyRequiredDependent>(
            //     b =>
            //         {
            //             b.Property(e => e.Id).HasConversion(GenericComparableIntClassKey.Converter);
            //             b.Property(e => e.PrincipalId).HasConversion(GenericComparableIntClassKey.Converter);
            //         });

            modelBuilder.Entity<BaseEntity>(
                entity =>
                {
                    entity.HasKey(e => e.Name);

                    entity.Property(p => p.Name)
                        .HasConversion(
                            p => p.Value,
                            p => new Key(p),
                            new ValueComparer<Key>(
                                (l, r) => (l == null && r == null) || (l != null && r != null && l.Value == r.Value),
                                v => v == null ? 0 : v.Value.GetHashCode()));

                    entity.OwnsOne(p => p.Text);
                    entity.Navigation(p => p.Text).IsRequired();
                });

            modelBuilder.Entity<IntStructKeyPrincipalShadow>(
                b => { b.Property(e => e.Id).HasConversion(IntStructKey.Converter); });

            modelBuilder.Entity<IntStructKeyOptionalDependentShadow>(
                b => { b.Property(e => e.Id).HasConversion(IntStructKey.Converter); });

            modelBuilder.Entity<IntStructKeyRequiredDependentShadow>(
                b => { b.Property(e => e.Id).HasConversion(IntStructKey.Converter); });

            // modelBuilder.Entity<IntClassKeyPrincipalShadow>(
            //     b => { b.Property(e => e.Id).HasConversion(IntClassKey.Converter); });
            //
            // modelBuilder.Entity<IntClassKeyOptionalDependentShadow>(
            //     b => { b.Property(e => e.Id).HasConversion(IntClassKey.Converter); });
            //
            // modelBuilder.Entity<IntClassKeyRequiredDependentShadow>(
            //     b => { b.Property(e => e.Id).HasConversion(IntClassKey.Converter); });
            //
            // modelBuilder.Entity<BareIntClassKeyPrincipalShadow>(
            //     b => { b.Property(e => e.Id).HasConversion(BareIntClassKey.Converter, BareIntClassKey.Comparer); });
            //
            // modelBuilder.Entity<BareIntClassKeyOptionalDependentShadow>(
            //     b => { b.Property(e => e.Id).HasConversion(BareIntClassKey.Converter, BareIntClassKey.Comparer); });
            //
            // modelBuilder.Entity<BareIntClassKeyRequiredDependentShadow>(
            //     b => { b.Property(e => e.Id).HasConversion(BareIntClassKey.Converter, BareIntClassKey.Comparer); });

            modelBuilder.Entity<ComparableIntStructKeyPrincipalShadow>(
                b => { b.Property(e => e.Id).HasConversion(ComparableIntStructKey.Converter); });

            modelBuilder.Entity<ComparableIntStructKeyOptionalDependentShadow>(
                b => { b.Property(e => e.Id).HasConversion(ComparableIntStructKey.Converter); });

            modelBuilder.Entity<ComparableIntStructKeyRequiredDependentShadow>(
                b => { b.Property(e => e.Id).HasConversion(ComparableIntStructKey.Converter); });

            modelBuilder.Entity<GenericComparableIntStructKeyPrincipalShadow>(
                b => { b.Property(e => e.Id).HasConversion(GenericComparableIntStructKey.Converter); });

            modelBuilder.Entity<GenericComparableIntStructKeyOptionalDependentShadow>(
                b => { b.Property(e => e.Id).HasConversion(GenericComparableIntStructKey.Converter); });

            modelBuilder.Entity<GenericComparableIntStructKeyRequiredDependentShadow>(
                b => { b.Property(e => e.Id).HasConversion(GenericComparableIntStructKey.Converter); });

            modelBuilder.Entity<StructuralComparableBytesStructKeyPrincipalShadow>(
                b => { b.Property(e => e.Id).HasConversion(StructuralComparableBytesStructKey.Converter); });

            modelBuilder.Entity<StructuralComparableBytesStructKeyOptionalDependentShadow>(
                b => { b.Property(e => e.Id).HasConversion(StructuralComparableBytesStructKey.Converter); });

            modelBuilder.Entity<StructuralComparableBytesStructKeyRequiredDependentShadow>(
                b => { b.Property(e => e.Id).HasConversion(StructuralComparableBytesStructKey.Converter); });

            modelBuilder.Entity<BytesStructKeyPrincipalShadow>(
                b => { b.Property(e => e.Id).HasConversion(BytesStructKey.Converter); });

            modelBuilder.Entity<BytesStructKeyOptionalDependentShadow>(
                b => { b.Property(e => e.Id).HasConversion(BytesStructKey.Converter); });

            modelBuilder.Entity<BytesStructKeyRequiredDependentShadow>(
                b => { b.Property(e => e.Id).HasConversion(BytesStructKey.Converter); });

            modelBuilder.Entity<ComparableBytesStructKeyPrincipalShadow>(
                b => { b.Property(e => e.Id).HasConversion(ComparableBytesStructKey.Converter); });

            modelBuilder.Entity<ComparableBytesStructKeyOptionalDependentShadow>(
                b => { b.Property(e => e.Id).HasConversion(ComparableBytesStructKey.Converter); });

            modelBuilder.Entity<ComparableBytesStructKeyRequiredDependentShadow>(
                b => { b.Property(e => e.Id).HasConversion(ComparableBytesStructKey.Converter); });

            modelBuilder.Entity<GenericComparableBytesStructKeyPrincipalShadow>(
                b => { b.Property(e => e.Id).HasConversion(GenericComparableBytesStructKey.Converter); });

            modelBuilder.Entity<GenericComparableBytesStructKeyOptionalDependentShadow>(
                b => { b.Property(e => e.Id).HasConversion(GenericComparableBytesStructKey.Converter); });

            modelBuilder.Entity<GenericComparableBytesStructKeyRequiredDependentShadow>(
                b => { b.Property(e => e.Id).HasConversion(GenericComparableBytesStructKey.Converter); });

            // modelBuilder.Entity<ComparableIntClassKeyPrincipalShadow>(
            //     b => { b.Property(e => e.Id).HasConversion(ComparableIntClassKey.Converter); });
            //
            // modelBuilder.Entity<ComparableIntClassKeyOptionalDependentShadow>(
            //     b => { b.Property(e => e.Id).HasConversion(ComparableIntClassKey.Converter); });
            //
            // modelBuilder.Entity<ComparableIntClassKeyRequiredDependentShadow>(
            //     b => { b.Property(e => e.Id).HasConversion(ComparableIntClassKey.Converter); });
            //
            // modelBuilder.Entity<GenericComparableIntClassKeyPrincipalShadow>(
            //     b => { b.Property(e => e.Id).HasConversion(GenericComparableIntClassKey.Converter); });
            //
            // modelBuilder.Entity<GenericComparableIntClassKeyOptionalDependentShadow>(
            //     b => { b.Property(e => e.Id).HasConversion(GenericComparableIntClassKey.Converter); });
            //
            // modelBuilder.Entity<GenericComparableIntClassKeyRequiredDependentShadow>(
            //     b => { b.Property(e => e.Id).HasConversion(GenericComparableIntClassKey.Converter); });

            modelBuilder.Entity<OwnerIntStructKey>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(IntStructKey.Converter);
                    b.OwnsOne(e => e.Owned);
                });

            modelBuilder.Entity<OwnerBytesStructKey>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(BytesStructKey.Converter);
                    b.OwnsOne(e => e.Owned);
                });

            modelBuilder.Entity<OwnerComparableIntStructKey>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(ComparableIntStructKey.Converter);
                    b.OwnsOne(e => e.Owned);
                });

            modelBuilder.Entity<OwnerComparableBytesStructKey>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(ComparableBytesStructKey.Converter);
                    b.OwnsOne(e => e.Owned);
                });

            modelBuilder.Entity<OwnerGenericComparableIntStructKey>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(GenericComparableIntStructKey.Converter);
                    b.OwnsOne(e => e.Owned);
                });

            modelBuilder.Entity<OwnerGenericComparableBytesStructKey>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(GenericComparableBytesStructKey.Converter);
                    b.OwnsOne(e => e.Owned);
                });

            modelBuilder.Entity<OwnerStructuralComparableBytesStructKey>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(StructuralComparableBytesStructKey.Converter);
                    b.OwnsOne(e => e.Owned);
                });

            modelBuilder.Entity<OwnerIntClassKey>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(IntClassKey.Converter);
                    b.OwnsOne(e => e.Owned);
                });

            modelBuilder.Entity<OwnerBareIntClassKey>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(BareIntClassKey.Converter, BareIntClassKey.Comparer);
                    b.OwnsOne(e => e.Owned);
                });

            modelBuilder.Entity<OwnerComparableIntClassKey>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(ComparableIntClassKey.Converter);
                    b.OwnsOne(e => e.Owned);
                });

            modelBuilder.Entity<OwnerGenericComparableIntClassKey>(
                b =>
                {
                    b.Property(e => e.Id).HasConversion(GenericComparableIntClassKey.Converter);
                    b.OwnsOne(e => e.Owned);
                });

            modelBuilder.Ignore<EnumerableClassKeyPrincipal>();
            modelBuilder.Ignore<EnumerableClassKeyOptionalDependent>();
            modelBuilder.Ignore<EnumerableClassKeyRequiredDependent>();
        }

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder.ConfigureWarnings(w => w.Ignore(CoreEventId.MappedEntityTypeIgnoredWarning)));
    }
}
