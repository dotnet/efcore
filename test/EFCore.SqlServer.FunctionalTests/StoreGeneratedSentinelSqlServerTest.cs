// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class StoreGeneratedSentinelSqlServerTest(StoreGeneratedSentinelSqlServerTest.StoreGeneratedSentinelSqlServerFixture fixture) : StoreGeneratedSqlServerTestBase<
    StoreGeneratedSentinelSqlServerTest.StoreGeneratedSentinelSqlServerFixture>(fixture)
{
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
                    b.Property(e => e.Id).HasSentinel(IntSentinel);
                    b.Property(e => e.NotStoreGenerated).HasSentinel(StringSentinel);
                    b.Property(e => e.Identity).HasSentinel(StringSentinel);
                    b.Property(e => e.IdentityReadOnlyBeforeSave).HasSentinel(StringSentinel);
                    b.Property(e => e.IdentityReadOnlyAfterSave).HasSentinel(StringSentinel);
                    b.Property(e => e.AlwaysIdentity).HasSentinel(StringSentinel);
                    b.Property(e => e.AlwaysIdentityReadOnlyBeforeSave).HasSentinel(StringSentinel);
                    b.Property(e => e.AlwaysIdentityReadOnlyAfterSave).HasSentinel(StringSentinel);
                    b.Property(e => e.Computed).HasSentinel(StringSentinel);
                    b.Property(e => e.ComputedReadOnlyBeforeSave).HasSentinel(StringSentinel);
                    b.Property(e => e.ComputedReadOnlyAfterSave).HasSentinel(StringSentinel);
                    b.Property(e => e.AlwaysComputed).HasSentinel(StringSentinel);
                    b.Property(e => e.AlwaysComputedReadOnlyBeforeSave).HasSentinel(StringSentinel);
                    b.Property(e => e.AlwaysComputedReadOnlyAfterSave).HasSentinel(StringSentinel);
                });

            modelBuilder.Entity<Anais>(
                b =>
                {
                    b.Property(e => e.Id).HasSentinel(IntSentinel);
                    b.Property(e => e.Never).HasSentinel(StringSentinel);
                    b.Property(e => e.NeverUseBeforeUseAfter).HasSentinel(StringSentinel);
                    b.Property(e => e.NeverIgnoreBeforeUseAfter).HasSentinel(StringSentinel);
                    b.Property(e => e.NeverThrowBeforeUseAfter).HasSentinel(StringSentinel);
                    b.Property(e => e.NeverUseBeforeIgnoreAfter).HasSentinel(StringSentinel);
                    b.Property(e => e.NeverIgnoreBeforeIgnoreAfter).HasSentinel(StringSentinel);
                    b.Property(e => e.NeverThrowBeforeIgnoreAfter).HasSentinel(StringSentinel);
                    b.Property(e => e.NeverUseBeforeThrowAfter).HasSentinel(StringSentinel);
                    b.Property(e => e.NeverIgnoreBeforeThrowAfter).HasSentinel(StringSentinel);
                    b.Property(e => e.NeverThrowBeforeThrowAfter).HasSentinel(StringSentinel);

                    b.Property(e => e.OnAdd).HasSentinel(StringSentinel);
                    b.Property(e => e.OnAddUseBeforeUseAfter).HasSentinel(StringSentinel);
                    b.Property(e => e.OnAddIgnoreBeforeUseAfter).HasSentinel(StringSentinel);
                    b.Property(e => e.OnAddThrowBeforeUseAfter).HasSentinel(StringSentinel);
                    b.Property(e => e.OnAddUseBeforeIgnoreAfter).HasSentinel(StringSentinel);
                    b.Property(e => e.OnAddIgnoreBeforeIgnoreAfter).HasSentinel(StringSentinel);
                    b.Property(e => e.OnAddThrowBeforeIgnoreAfter).HasSentinel(StringSentinel);
                    b.Property(e => e.OnAddUseBeforeThrowAfter).HasSentinel(StringSentinel);
                    b.Property(e => e.OnAddIgnoreBeforeThrowAfter).HasSentinel(StringSentinel);
                    b.Property(e => e.OnAddThrowBeforeThrowAfter).HasSentinel(StringSentinel);

                    b.Property(e => e.OnAddOrUpdate).HasSentinel(StringSentinel);
                    b.Property(e => e.OnAddOrUpdateUseBeforeUseAfter).HasSentinel(StringSentinel);
                    b.Property(e => e.OnAddOrUpdateIgnoreBeforeUseAfter).HasSentinel(StringSentinel);
                    b.Property(e => e.OnAddOrUpdateThrowBeforeUseAfter).HasSentinel(StringSentinel);
                    b.Property(e => e.OnAddOrUpdateUseBeforeIgnoreAfter).HasSentinel(StringSentinel);
                    b.Property(e => e.OnAddOrUpdateIgnoreBeforeIgnoreAfter).HasSentinel(StringSentinel);
                    b.Property(e => e.OnAddOrUpdateThrowBeforeIgnoreAfter).HasSentinel(StringSentinel);
                    b.Property(e => e.OnAddOrUpdateUseBeforeThrowAfter).HasSentinel(StringSentinel);
                    b.Property(e => e.OnAddOrUpdateIgnoreBeforeThrowAfter).HasSentinel(StringSentinel);
                    b.Property(e => e.OnAddOrUpdateThrowBeforeThrowAfter).HasSentinel(StringSentinel);

                    b.Property(e => e.OnUpdate).HasSentinel(StringSentinel);
                    b.Property(e => e.OnUpdateUseBeforeUseAfter).HasSentinel(StringSentinel);
                    b.Property(e => e.OnUpdateIgnoreBeforeUseAfter).HasSentinel(StringSentinel);
                    b.Property(e => e.OnUpdateThrowBeforeUseAfter).HasSentinel(StringSentinel);
                    b.Property(e => e.OnUpdateUseBeforeIgnoreAfter).HasSentinel(StringSentinel);
                    b.Property(e => e.OnUpdateIgnoreBeforeIgnoreAfter).HasSentinel(StringSentinel);
                    b.Property(e => e.OnUpdateThrowBeforeIgnoreAfter).HasSentinel(StringSentinel);
                    b.Property(e => e.OnUpdateUseBeforeThrowAfter).HasSentinel(StringSentinel);
                    b.Property(e => e.OnUpdateIgnoreBeforeThrowAfter).HasSentinel(StringSentinel);
                    b.Property(e => e.OnUpdateThrowBeforeThrowAfter).HasSentinel(StringSentinel);
                });

            modelBuilder.Entity<WithBackingFields>(
                b =>
                {
                    b.Property(e => e.Id).HasSentinel(NullableIntSentinel);
                    b.Property(e => e.NullableAsNonNullable).HasSentinel(NullableIntSentinel);
                    b.Property(e => e.NonNullableAsNullable).HasSentinel(IntSentinel);
                });

            modelBuilder.Entity<WithNoBackingFields>(
                b =>
                {
                    b.Property(e => e.Id).HasSentinel(IntSentinel);
                    b.Property(e => e.TrueDefault).HasSentinel(BoolSentinel);
                    b.Property(e => e.NonZeroDefault).HasSentinel(IntSentinel);
                    b.Property(e => e.FalseDefault).HasSentinel(BoolSentinel);
                    b.Property(e => e.ZeroDefault).HasSentinel(IntSentinel);
                });

            modelBuilder.Entity<WithNullableBackingFields>(
                b =>
                {
                    b.Property(e => e.Id).HasSentinel(NullableIntSentinel);
                    b.Property(e => e.NullableBackedBoolTrueDefault).HasSentinel(NullableBoolSentinel);
                    b.Property(e => e.NullableBackedIntNonZeroDefault).HasSentinel(NullableIntSentinel);
                    b.Property(e => e.NullableBackedBoolFalseDefault).HasSentinel(NullableBoolSentinel);
                    b.Property(e => e.NullableBackedIntZeroDefault).HasSentinel(NullableIntSentinel);
                });

            modelBuilder.Entity<WithObjectBackingFields>(
                b =>
                {
                    b.Property(e => e.Id).HasSentinel(NullableIntSentinel);
                    b.Property(e => e.NullableBackedBoolTrueDefault).HasSentinel(NullableBoolSentinel);
                    b.Property(e => e.NullableBackedIntNonZeroDefault).HasSentinel(NullableIntSentinel);
                    b.Property(e => e.NullableBackedBoolFalseDefault).HasSentinel(NullableBoolSentinel);
                    b.Property(e => e.NullableBackedIntZeroDefault).HasSentinel(NullableIntSentinel);
                });

            modelBuilder.Entity<NonStoreGenDependent>().Property(e => e.Id).HasSentinel(IntSentinel);

            modelBuilder.Entity<CompositePrincipal>().Property(e => e.Id).HasSentinel(IntSentinel);
            modelBuilder.Entity<CompositeDependent>().Property(e => e.PrincipalId).HasSentinel(IntSentinel);

            modelBuilder.Entity<WrappedIntHiLoClassPrincipal>().Property(e => e.Id).HasSentinel(WrappedIntHiLoKeyClassSentinel);
            modelBuilder.Entity<WrappedIntHiLoStructPrincipal>().Property(e => e.Id).HasSentinel(WrappedIntHiLoKeyStructSentinel);
            modelBuilder.Entity<WrappedIntHiLoRecordPrincipal>().Property(e => e.Id).HasSentinel(WrappedIntHiLoKeyRecordSentinel);

            modelBuilder.Entity<IntToString>().Property(e => e.Id).HasSentinel(IntSentinel);
            modelBuilder.Entity<GuidToString>().Property(e => e.Id).HasSentinel(GuidSentinel);
            modelBuilder.Entity<GuidToBytes>().Property(e => e.Id).HasSentinel(GuidSentinel);
            modelBuilder.Entity<ShortToBytes>().Property(e => e.Id).HasSentinel(ShortSentinel);

            modelBuilder.Entity<Darwin>(
                b =>
                {
                    b.Property(e => e.Id).HasSentinel(NullableIntSentinel);
                    b.Property(e => e.Name).HasSentinel(StringSentinel);
                });

            modelBuilder.Entity<Species>(
                b =>
                {
                    b.Property(e => e.Id).HasSentinel(IntSentinel);
                    b.Property(e => e.Name).HasSentinel(StringSentinel);
                });

            modelBuilder.Entity<OptionalProduct>().Property(e => e.Id).HasSentinel(IntSentinel);
            modelBuilder.Entity<StoreGenPrincipal>().Property(e => e.Id).HasSentinel(IntSentinel);

            modelBuilder.Entity<WrappedIntClassPrincipal>(
                entity =>
                {
                    entity.Property(e => e.Id).HasSentinel(WrappedIntKeyClassSentinel);
                    entity.Property(e => e.NonKey).HasSentinel(WrappedIntClassSentinel);
                });

            modelBuilder.Entity<WrappedIntStructPrincipal>(
                entity =>
                {
                    entity.Property(e => e.Id).HasSentinel(WrappedIntKeyStructSentinel);
                    entity.Property(e => e.NonKey).HasSentinel(WrappedIntStructSentinel);
                });

            modelBuilder.Entity<WrappedIntRecordPrincipal>(
                entity =>
                {
                    entity.Property(e => e.Id).HasSentinel(WrappedIntKeyRecordSentinel);
                    entity.Property(e => e.NonKey).HasSentinel(WrappedIntRecordSentinel);
                });

            modelBuilder.Entity<LongToIntPrincipal>().Property(e => e.Id).HasSentinel(LongSentinel);
            modelBuilder.Entity<LongToDecimalPrincipal>().Property(e => e.Id).HasSentinel(LongToDecimalPrincipalSentinel);

            modelBuilder.Entity<WrappedGuidClassPrincipal>(
                entity =>
                {
                    entity.Property(e => e.Id).HasSentinel(WrappedGuidKeyClassSentinel);
                    entity.Property(e => e.NonKey).HasSentinel(WrappedGuidClassSentinel);
                });

            modelBuilder.Entity<WrappedGuidStructPrincipal>(
                entity =>
                {
                    entity.Property(e => e.Id).HasSentinel(WrappedGuidKeyStructSentinel);
                    entity.Property(e => e.NonKey).HasSentinel(WrappedGuidStructSentinel);
                });

            modelBuilder.Entity<WrappedGuidRecordPrincipal>(
                entity =>
                {
                    entity.Property(e => e.Id).HasSentinel(WrappedGuidKeyRecordSentinel);
                    entity.Property(e => e.NonKey).HasSentinel(WrappedGuidRecordSentinel);
                });

            modelBuilder.Entity<WrappedStringClassPrincipal>(
                entity =>
                {
                    entity.Property(e => e.Id).HasSentinel(WrappedStringKeyClassSentinel);
                    entity.Property(e => e.NonKey).HasSentinel(WrappedStringClassSentinel);
                });

            modelBuilder.Entity<WrappedStringStructPrincipal>(
                entity =>
                {
                    entity.Property(e => e.Id).HasSentinel(WrappedStringKeyStructSentinel);
                    entity.Property(e => e.NonKey).HasSentinel(WrappedStringStructSentinel);
                });

            modelBuilder.Entity<WrappedStringRecordPrincipal>(
                entity =>
                {
                    entity.Property(e => e.Id).HasSentinel(WrappedStringKeyRecordSentinel);
                    entity.Property(e => e.NonKey).HasSentinel(WrappedStringRecordSentinel);
                });

            modelBuilder.Entity<WrappedUriClassPrincipal>(
                entity =>
                {
                    entity.Property(e => e.Id).HasSentinel(WrappedUriKeyClassSentinel);
                    entity.Property(e => e.NonKey).HasSentinel(WrappedUriClassSentinel);
                });

            modelBuilder.Entity<WrappedUriStructPrincipal>(
                entity =>
                {
                    entity.Property(e => e.Id).HasSentinel(WrappedUriKeyStructSentinel);
                    entity.Property(e => e.NonKey).HasSentinel(WrappedUriStructSentinel);
                });

            modelBuilder.Entity<WrappedUriRecordPrincipal>(
                entity =>
                {
                    entity.Property(e => e.Id).HasSentinel(WrappedUriKeyRecordSentinel);
                    entity.Property(e => e.NonKey).HasSentinel(WrappedUriRecordSentinel);
                });

            modelBuilder.Entity<UriPrincipal>().Property(e => e.Id).HasSentinel(UriSentinel);
            ;
            modelBuilder.Entity<EnumPrincipal>().Property(e => e.Id).HasSentinel(KeyEnumSentinel);
            ;
            modelBuilder.Entity<GuidAsStringPrincipal>().Property(e => e.Id).HasSentinel(GuidAsStringSentinel);
            modelBuilder.Entity<StringAsGuidPrincipal>().Property(e => e.Id).HasSentinel(StringAsGuidSentinel);
        }
    }
}
