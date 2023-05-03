// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

namespace Microsoft.EntityFrameworkCore;

public class StoreGeneratedSentinelSqlServerTest : StoreGeneratedSqlServerTestBase<
    StoreGeneratedSentinelSqlServerTest.StoreGeneratedSentinelSqlServerFixture>
{
    public StoreGeneratedSentinelSqlServerTest(StoreGeneratedSentinelSqlServerFixture fixture)
        : base(fixture)
    {
    }

    public class StoreGeneratedSentinelSqlServerFixture : StoreGeneratedSqlServerFixtureBase
    {
        public override Guid GuidSentinel { get; } = new("91B22A0D-99F4-4AD4-930F-6590AFD30FDD");

        public override int IntSentinel
            => -1;

        public override short ShortSentinel
            => -1;

        public override long LongSentinel
            => -1;

        public override int? NullableIntSentinel
            => -1;

        public override bool BoolSentinel
            => true;

        public override bool? NullableBoolSentinel
            => true;

        public override string? StringSentinel
            => "Sentinel";

        public override Uri? UriSentinel { get; } = new(@"http://localhost/");

        public override KeyEnum KeyEnumSentinel
            => (KeyEnum)(-1);

        public override string? StringAsGuidSentinel
            => "91B22A0D-99F4-4AD4-930F-6590AFD30FDD";

        public override Guid GuidAsStringSentinel { get; } = new("91B22A0D-99F4-4AD4-930F-6590AFD30FDD");
        public override WrappedIntKeyClass? WrappedIntKeyClassSentinel { get; } = new() { Value = -1 };
        public override WrappedIntClass? WrappedIntClassSentinel { get; } = new() { Value = -1 };
        public override WrappedIntKeyStruct WrappedIntKeyStructSentinel { get; } = new() { Value = -1 };
        public override WrappedIntStruct WrappedIntStructSentinel { get; } = new() { Value = -1 };
        public override WrappedIntKeyRecord? WrappedIntKeyRecordSentinel { get; } = new() { Value = -1 };
        public override WrappedIntRecord? WrappedIntRecordSentinel { get; } = new() { Value = -1 };
        public override WrappedStringKeyClass? WrappedStringKeyClassSentinel { get; } = new() { Value = "Sentinel" };
        public override WrappedStringClass? WrappedStringClassSentinel { get; } = new() { Value = "Sentinel" };
        public override WrappedStringKeyStruct WrappedStringKeyStructSentinel { get; } = new() { Value = "Sentinel" };
        public override WrappedStringStruct WrappedStringStructSentinel { get; } = new() { Value = "Sentinel" };
        public override WrappedStringKeyRecord? WrappedStringKeyRecordSentinel { get; } = new() { Value = "Sentinel" };
        public override WrappedStringRecord? WrappedStringRecordSentinel { get; } = new() { Value = "Sentinel" };

        public override WrappedGuidKeyClass? WrappedGuidKeyClassSentinel { get; } =
            new() { Value = new Guid("2567F02C-CBA8-4105-9000-387D12B505FF") };

        public override WrappedGuidClass? WrappedGuidClassSentinel { get; } =
            new() { Value = new Guid("2567F02C-CBA8-4105-9000-387D12B505FF") };

        public override WrappedGuidKeyStruct WrappedGuidKeyStructSentinel { get; } =
            new() { Value = new Guid("2567F02C-CBA8-4105-9000-387D12B505FF") };

        public override WrappedGuidStruct WrappedGuidStructSentinel { get; } =
            new() { Value = new Guid("2567F02C-CBA8-4105-9000-387D12B505FF") };

        public override WrappedGuidKeyRecord? WrappedGuidKeyRecordSentinel { get; } =
            new() { Value = new Guid("2567F02C-CBA8-4105-9000-387D12B505FF") };

        public override WrappedGuidRecord? WrappedGuidRecordSentinel { get; } =
            new() { Value = new Guid("2567F02C-CBA8-4105-9000-387D12B505FF") };

        public override WrappedUriKeyClass? WrappedUriKeyClassSentinel { get; } = new() { Value = new Uri(@"http://localhost/") };
        public override WrappedUriClass? WrappedUriClassSentinel { get; } = new() { Value = new Uri(@"http://localhost/") };
        public override WrappedUriKeyStruct WrappedUriKeyStructSentinel { get; } = new() { Value = new Uri(@"http://localhost/") };
        public override WrappedUriStruct WrappedUriStructSentinel { get; } = new() { Value = new Uri(@"http://localhost/") };
        public override WrappedUriKeyRecord? WrappedUriKeyRecordSentinel { get; } = new() { Value = new Uri(@"http://localhost/") };
        public override WrappedUriRecord? WrappedUriRecordSentinel { get; } = new() { Value = new Uri(@"http://localhost/") };

        public override long LongToDecimalPrincipalSentinel
            => -1;

        public override WrappedIntHiLoKeyClass? WrappedIntHiLoKeyClassSentinel { get; } = new() { Value = -1 };
        public override WrappedIntHiLoKeyStruct WrappedIntHiLoKeyStructSentinel { get; } = new() { Value = -1 };
        public override WrappedIntHiLoKeyRecord? WrappedIntHiLoKeyRecordSentinel { get; } = new() { Value = -1 };

        protected override string StoreName
            => "StoreGeneratedTestSentinel";

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<Gumball>(
                b =>
                {
                    b.Property(e => e.Id).Metadata.Sentinel = IntSentinel;
                    b.Property(e => e.NotStoreGenerated).Metadata.Sentinel = StringSentinel;
                    b.Property(e => e.Identity).Metadata.Sentinel = StringSentinel;
                    b.Property(e => e.IdentityReadOnlyBeforeSave).Metadata.Sentinel = StringSentinel;
                    b.Property(e => e.IdentityReadOnlyAfterSave).Metadata.Sentinel = StringSentinel;
                    b.Property(e => e.AlwaysIdentity).Metadata.Sentinel = StringSentinel;
                    b.Property(e => e.AlwaysIdentityReadOnlyBeforeSave).Metadata.Sentinel = StringSentinel;
                    b.Property(e => e.AlwaysIdentityReadOnlyAfterSave).Metadata.Sentinel = StringSentinel;
                    b.Property(e => e.Computed).Metadata.Sentinel = StringSentinel;
                    b.Property(e => e.ComputedReadOnlyBeforeSave).Metadata.Sentinel = StringSentinel;
                    b.Property(e => e.ComputedReadOnlyAfterSave).Metadata.Sentinel = StringSentinel;
                    b.Property(e => e.AlwaysComputed).Metadata.Sentinel = StringSentinel;
                    b.Property(e => e.AlwaysComputedReadOnlyBeforeSave).Metadata.Sentinel = StringSentinel;
                    b.Property(e => e.AlwaysComputedReadOnlyAfterSave).Metadata.Sentinel = StringSentinel;
                });

            modelBuilder.Entity<Anais>(
                b =>
                {
                    b.Property(e => e.Id).Metadata.Sentinel = IntSentinel;
                    b.Property(e => e.Never).Metadata.Sentinel = StringSentinel;
                    b.Property(e => e.NeverUseBeforeUseAfter).Metadata.Sentinel = StringSentinel;
                    b.Property(e => e.NeverIgnoreBeforeUseAfter).Metadata.Sentinel = StringSentinel;
                    b.Property(e => e.NeverThrowBeforeUseAfter).Metadata.Sentinel = StringSentinel;
                    b.Property(e => e.NeverUseBeforeIgnoreAfter).Metadata.Sentinel = StringSentinel;
                    b.Property(e => e.NeverIgnoreBeforeIgnoreAfter).Metadata.Sentinel = StringSentinel;
                    b.Property(e => e.NeverThrowBeforeIgnoreAfter).Metadata.Sentinel = StringSentinel;
                    b.Property(e => e.NeverUseBeforeThrowAfter).Metadata.Sentinel = StringSentinel;
                    b.Property(e => e.NeverIgnoreBeforeThrowAfter).Metadata.Sentinel = StringSentinel;
                    b.Property(e => e.NeverThrowBeforeThrowAfter).Metadata.Sentinel = StringSentinel;

                    b.Property(e => e.OnAdd).Metadata.Sentinel = StringSentinel;
                    b.Property(e => e.OnAddUseBeforeUseAfter).Metadata.Sentinel = StringSentinel;
                    b.Property(e => e.OnAddIgnoreBeforeUseAfter).Metadata.Sentinel = StringSentinel;
                    b.Property(e => e.OnAddThrowBeforeUseAfter).Metadata.Sentinel = StringSentinel;
                    b.Property(e => e.OnAddUseBeforeIgnoreAfter).Metadata.Sentinel = StringSentinel;
                    b.Property(e => e.OnAddIgnoreBeforeIgnoreAfter).Metadata.Sentinel = StringSentinel;
                    b.Property(e => e.OnAddThrowBeforeIgnoreAfter).Metadata.Sentinel = StringSentinel;
                    b.Property(e => e.OnAddUseBeforeThrowAfter).Metadata.Sentinel = StringSentinel;
                    b.Property(e => e.OnAddIgnoreBeforeThrowAfter).Metadata.Sentinel = StringSentinel;
                    b.Property(e => e.OnAddThrowBeforeThrowAfter).Metadata.Sentinel = StringSentinel;

                    b.Property(e => e.OnAddOrUpdate).Metadata.Sentinel = StringSentinel;
                    b.Property(e => e.OnAddOrUpdateUseBeforeUseAfter).Metadata.Sentinel = StringSentinel;
                    b.Property(e => e.OnAddOrUpdateIgnoreBeforeUseAfter).Metadata.Sentinel = StringSentinel;
                    b.Property(e => e.OnAddOrUpdateThrowBeforeUseAfter).Metadata.Sentinel = StringSentinel;
                    b.Property(e => e.OnAddOrUpdateUseBeforeIgnoreAfter).Metadata.Sentinel = StringSentinel;
                    b.Property(e => e.OnAddOrUpdateIgnoreBeforeIgnoreAfter).Metadata.Sentinel = StringSentinel;
                    b.Property(e => e.OnAddOrUpdateThrowBeforeIgnoreAfter).Metadata.Sentinel = StringSentinel;
                    b.Property(e => e.OnAddOrUpdateUseBeforeThrowAfter).Metadata.Sentinel = StringSentinel;
                    b.Property(e => e.OnAddOrUpdateIgnoreBeforeThrowAfter).Metadata.Sentinel = StringSentinel;
                    b.Property(e => e.OnAddOrUpdateThrowBeforeThrowAfter).Metadata.Sentinel = StringSentinel;

                    b.Property(e => e.OnUpdate).Metadata.Sentinel = StringSentinel;
                    b.Property(e => e.OnUpdateUseBeforeUseAfter).Metadata.Sentinel = StringSentinel;
                    b.Property(e => e.OnUpdateIgnoreBeforeUseAfter).Metadata.Sentinel = StringSentinel;
                    b.Property(e => e.OnUpdateThrowBeforeUseAfter).Metadata.Sentinel = StringSentinel;
                    b.Property(e => e.OnUpdateUseBeforeIgnoreAfter).Metadata.Sentinel = StringSentinel;
                    b.Property(e => e.OnUpdateIgnoreBeforeIgnoreAfter).Metadata.Sentinel = StringSentinel;
                    b.Property(e => e.OnUpdateThrowBeforeIgnoreAfter).Metadata.Sentinel = StringSentinel;
                    b.Property(e => e.OnUpdateUseBeforeThrowAfter).Metadata.Sentinel = StringSentinel;
                    b.Property(e => e.OnUpdateIgnoreBeforeThrowAfter).Metadata.Sentinel = StringSentinel;
                    b.Property(e => e.OnUpdateThrowBeforeThrowAfter).Metadata.Sentinel = StringSentinel;
                });

            modelBuilder.Entity<WithBackingFields>(
                b =>
                {
                    b.Property(e => e.Id).Metadata.Sentinel = NullableIntSentinel;
                    b.Property(e => e.NullableAsNonNullable).Metadata.Sentinel = NullableIntSentinel;
                    b.Property(e => e.NonNullableAsNullable).Metadata.Sentinel = IntSentinel;
                });

            modelBuilder.Entity<WithNullableBackingFields>(
                b =>
                {
                    b.Property(e => e.Id).Metadata.Sentinel = NullableIntSentinel;
                    b.Property(e => e.NullableBackedBoolTrueDefault).Metadata.Sentinel = NullableBoolSentinel;
                    b.Property(e => e.NullableBackedIntNonZeroDefault).Metadata.Sentinel = NullableIntSentinel;
                    b.Property(e => e.NullableBackedBoolFalseDefault).Metadata.Sentinel = NullableBoolSentinel;
                    b.Property(e => e.NullableBackedIntZeroDefault).Metadata.Sentinel = NullableIntSentinel;
                });

            modelBuilder.Entity<WithObjectBackingFields>(
                b =>
                {
                    b.Property(e => e.Id).Metadata.Sentinel = NullableIntSentinel;
                    b.Property(e => e.NullableBackedBoolTrueDefault).Metadata.Sentinel = NullableBoolSentinel;
                    b.Property(e => e.NullableBackedIntNonZeroDefault).Metadata.Sentinel = NullableIntSentinel;
                    b.Property(e => e.NullableBackedBoolFalseDefault).Metadata.Sentinel = NullableBoolSentinel;
                    b.Property(e => e.NullableBackedIntZeroDefault).Metadata.Sentinel = NullableIntSentinel;
                });

            modelBuilder.Entity<NonStoreGenDependent>().Property(e => e.Id).Metadata.Sentinel = IntSentinel;

            modelBuilder.Entity<CompositePrincipal>().Property(e => e.Id).Metadata.Sentinel = IntSentinel;
            modelBuilder.Entity<CompositeDependent>().Property(e => e.PrincipalId).Metadata.Sentinel = IntSentinel;

            modelBuilder.Entity<WrappedIntHiLoClassPrincipal>().Property(e => e.Id).Metadata.Sentinel = WrappedIntHiLoKeyClassSentinel;
            modelBuilder.Entity<WrappedIntHiLoStructPrincipal>().Property(e => e.Id).Metadata.Sentinel = WrappedIntHiLoKeyStructSentinel;
            modelBuilder.Entity<WrappedIntHiLoRecordPrincipal>().Property(e => e.Id).Metadata.Sentinel = WrappedIntHiLoKeyRecordSentinel;

            modelBuilder.Entity<IntToString>().Property(e => e.Id).Metadata.Sentinel = IntSentinel;
            modelBuilder.Entity<GuidToString>().Property(e => e.Id).Metadata.Sentinel = GuidSentinel;
            modelBuilder.Entity<GuidToBytes>().Property(e => e.Id).Metadata.Sentinel = GuidSentinel;
            modelBuilder.Entity<ShortToBytes>().Property(e => e.Id).Metadata.Sentinel = ShortSentinel;

            modelBuilder.Entity<Darwin>(
                b =>
                {
                    b.Property(e => e.Id).Metadata.Sentinel = NullableIntSentinel;
                    b.Property(e => e.Name).Metadata.Sentinel = StringSentinel;
                });

            modelBuilder.Entity<Species>(
                b =>
                {
                    b.Property(e => e.Id).Metadata.Sentinel = IntSentinel;
                    b.Property(e => e.Name).Metadata.Sentinel = StringSentinel;
                });

            modelBuilder.Entity<OptionalProduct>().Property(e => e.Id).Metadata.Sentinel = IntSentinel;
            modelBuilder.Entity<StoreGenPrincipal>().Property(e => e.Id).Metadata.Sentinel = IntSentinel;

            modelBuilder.Entity<WrappedIntClassPrincipal>(
                entity =>
                {
                    entity.Property(e => e.Id).Metadata.Sentinel = WrappedIntKeyClassSentinel;
                    entity.Property(e => e.NonKey).Metadata.Sentinel = WrappedIntClassSentinel;
                });

            modelBuilder.Entity<WrappedIntStructPrincipal>(
                entity =>
                {
                    entity.Property(e => e.Id).Metadata.Sentinel = WrappedIntKeyStructSentinel;
                    entity.Property(e => e.NonKey).Metadata.Sentinel = WrappedIntStructSentinel;
                });

            modelBuilder.Entity<WrappedIntRecordPrincipal>(
                entity =>
                {
                    entity.Property(e => e.Id).Metadata.Sentinel = WrappedIntKeyRecordSentinel;
                    entity.Property(e => e.NonKey).Metadata.Sentinel = WrappedIntRecordSentinel;
                });

            modelBuilder.Entity<LongToIntPrincipal>().Property(e => e.Id).Metadata.Sentinel = LongSentinel;
            modelBuilder.Entity<LongToDecimalPrincipal>().Property(e => e.Id).Metadata.Sentinel = LongToDecimalPrincipalSentinel;

            modelBuilder.Entity<WrappedGuidClassPrincipal>(
                entity =>
                {
                    entity.Property(e => e.Id).Metadata.Sentinel = WrappedGuidKeyClassSentinel;
                    entity.Property(e => e.NonKey).Metadata.Sentinel = WrappedGuidClassSentinel;
                });

            modelBuilder.Entity<WrappedGuidStructPrincipal>(
                entity =>
                {
                    entity.Property(e => e.Id).Metadata.Sentinel = WrappedGuidKeyStructSentinel;
                    entity.Property(e => e.NonKey).Metadata.Sentinel = WrappedGuidStructSentinel;
                });

            modelBuilder.Entity<WrappedGuidRecordPrincipal>(
                entity =>
                {
                    entity.Property(e => e.Id).Metadata.Sentinel = WrappedGuidKeyRecordSentinel;
                    entity.Property(e => e.NonKey).Metadata.Sentinel = WrappedGuidRecordSentinel;
                });

            modelBuilder.Entity<WrappedStringClassPrincipal>(
                entity =>
                {
                    entity.Property(e => e.Id).Metadata.Sentinel = WrappedStringKeyClassSentinel;
                    entity.Property(e => e.NonKey).Metadata.Sentinel = WrappedStringClassSentinel;
                });

            modelBuilder.Entity<WrappedStringStructPrincipal>(
                entity =>
                {
                    entity.Property(e => e.Id).Metadata.Sentinel = WrappedStringKeyStructSentinel;
                    entity.Property(e => e.NonKey).Metadata.Sentinel = WrappedStringStructSentinel;
                });

            modelBuilder.Entity<WrappedStringRecordPrincipal>(
                entity =>
                {
                    entity.Property(e => e.Id).Metadata.Sentinel = WrappedStringKeyRecordSentinel;
                    entity.Property(e => e.NonKey).Metadata.Sentinel = WrappedStringRecordSentinel;
                });

            modelBuilder.Entity<WrappedUriClassPrincipal>(
                entity =>
                {
                    entity.Property(e => e.Id).Metadata.Sentinel = WrappedUriKeyClassSentinel;
                    entity.Property(e => e.NonKey).Metadata.Sentinel = WrappedUriClassSentinel;
                });

            modelBuilder.Entity<WrappedUriStructPrincipal>(
                entity =>
                {
                    entity.Property(e => e.Id).Metadata.Sentinel = WrappedUriKeyStructSentinel;
                    entity.Property(e => e.NonKey).Metadata.Sentinel = WrappedUriStructSentinel;
                });

            modelBuilder.Entity<WrappedUriRecordPrincipal>(
                entity =>
                {
                    entity.Property(e => e.Id).Metadata.Sentinel = WrappedUriKeyRecordSentinel;
                    entity.Property(e => e.NonKey).Metadata.Sentinel = WrappedUriRecordSentinel;
                });

            modelBuilder.Entity<UriPrincipal>().Property(e => e.Id).Metadata.Sentinel = UriSentinel;
            ;
            modelBuilder.Entity<EnumPrincipal>().Property(e => e.Id).Metadata.Sentinel = KeyEnumSentinel;
            ;
            modelBuilder.Entity<GuidAsStringPrincipal>().Property(e => e.Id).Metadata.Sentinel = GuidAsStringSentinel;
            modelBuilder.Entity<StringAsGuidPrincipal>().Property(e => e.Id).Metadata.Sentinel = StringAsGuidSentinel;
        }
    }
}
