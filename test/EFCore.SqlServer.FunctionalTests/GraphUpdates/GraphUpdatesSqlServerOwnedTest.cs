// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class GraphUpdatesSqlServerOwnedTest(GraphUpdatesSqlServerOwnedTest.SqlServerFixture fixture)
    : GraphUpdatesSqlServerTestBase<GraphUpdatesSqlServerOwnedTest.SqlServerFixture>(fixture)
{
    // No owned types
    public override Task Update_root_by_collection_replacement_of_inserted_first_level(bool async)
        => Task.CompletedTask;

    // No owned types
    public override Task Mark_explicitly_set_dependent_appropriately_with_any_inheritance_and_stable_generator(bool async, bool useAdd)
        => Task.CompletedTask;

    // No owned types
    public override Task Mark_explicitly_set_stable_dependent_appropriately(bool async, bool useAdd)
        => Task.CompletedTask;

    // No owned types
    public override Task Mark_explicitly_set_stable_dependent_appropriately_when_deep_in_graph(bool async, bool useAdd)
        => Task.CompletedTask;

    // No owned types
    public override Task Update_root_by_collection_replacement_of_deleted_first_level(bool async)
        => Task.CompletedTask;

    // No owned types
    public override Task Update_root_by_collection_replacement_of_inserted_second_level(bool async)
        => Task.CompletedTask;

    // No owned types
    public override Task Update_root_by_collection_replacement_of_deleted_second_level(bool async)
        => Task.CompletedTask;

    // No owned types
    public override Task Update_root_by_collection_replacement_of_inserted_first_level_level(bool async)
        => Task.CompletedTask;

    // No owned types
    public override Task Update_root_by_collection_replacement_of_deleted_third_level(bool async)
        => Task.CompletedTask;

    // No owned types
    public override Task Sever_relationship_that_will_later_be_deleted(bool async)
        => Task.CompletedTask;

    // No owned types
    public override Task Alternate_key_over_foreign_key_doesnt_bypass_delete_behavior(bool async)
        => Task.CompletedTask;

    // No owned types
    public override Task Shadow_skip_navigation_in_base_class_is_handled(bool async)
        => Task.CompletedTask;

    // Owned dependents are always loaded
    public override Task Required_one_to_one_are_cascade_deleted_in_store(
        CascadeTiming? cascadeDeleteTiming,
        CascadeTiming? deleteOrphansTiming)
        => Task.CompletedTask;

    public override Task Required_one_to_one_with_alternate_key_are_cascade_deleted_in_store(
        CascadeTiming? cascadeDeleteTiming,
        CascadeTiming? deleteOrphansTiming)
        => Task.CompletedTask;

    // No owned types
    public override Task Can_insert_when_composite_FK_has_default_value_for_one_part(bool async)
        => Task.CompletedTask;

    public override Task Required_one_to_one_relationships_are_one_to_one(CascadeTiming? deleteOrphansTiming)
        => Task.CompletedTask;

    public override Task Required_one_to_one_with_AK_relationships_are_one_to_one(CascadeTiming? deleteOrphansTiming)
        => Task.CompletedTask;

    // No owned types
    public override Task Can_insert_when_bool_PK_in_composite_key_has_sentinel_value(bool async, bool initialValue)
        => Task.CompletedTask;

    // No owned types
    public override Task Can_insert_when_int_PK_in_composite_key_has_sentinel_value(bool async, int initialValue)
        => Task.CompletedTask;

    // No owned types
    public override Task Can_insert_when_nullable_bool_PK_in_composite_key_has_sentinel_value(bool async, bool? initialValue)
        => Task.CompletedTask;

    // No owned types
    public override Task Throws_for_single_property_bool_key_with_default_value_generation(bool async, bool initialValue)
        => Task.CompletedTask;

    // No owned types
    public override Task Throws_for_single_property_nullable_bool_key_with_default_value_generation(bool async, bool? initialValue)
        => Task.CompletedTask;

    protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    public class SqlServerFixture : GraphUpdatesSqlServerFixtureBase
    {
        protected override string StoreName
            => "GraphOwnedUpdatesTest";

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.Entity<OwnerRoot>(
                b =>
                {
                    b.OwnsOne(e => e.OptionalSingle).OwnsOne(e => e.Single);
                    b.OwnsOne(e => e.RequiredSingle).OwnsOne(e => e.Single);
                    b.OwnsMany(e => e.OptionalChildren).OwnsMany(e => e.Children);
                    b.OwnsMany(e => e.RequiredChildren).OwnsMany(e => e.Children);
                });

            modelBuilder.Entity<Root>(
                b =>
                {
                    b.Property(e => e.AlternateId).ValueGeneratedOnAdd();

                    // TODO: Owned inheritance support #9630
                    b.HasMany(e => e.RequiredChildren)
                        .WithOne(e => e.Parent)
                        .HasForeignKey(e => e.ParentId);

                    modelBuilder.Entity<Required1>()
                        .HasMany(e => e.Children)
                        .WithOne(e => e.Parent)
                        .HasForeignKey(e => e.ParentId);

                    modelBuilder.Entity<Required1Derived>();
                    modelBuilder.Entity<Required1MoreDerived>();
                    modelBuilder.Entity<Required2Derived>();
                    modelBuilder.Entity<Required2MoreDerived>();

                    b.HasMany(e => e.OptionalChildren)
                        .WithOne(e => e.Parent)
                        .HasForeignKey(e => e.ParentId)
                        .OnDelete(DeleteBehavior.SetNull);

                    b.OwnsOne(
                        e => e.RequiredSingle, r =>
                        {
                            r.WithOwner(e => e.Root)
                                .HasForeignKey(e => e.Id);

                            r.OwnsOne(e => e.Single)
                                .WithOwner(e => e.Back)
                                .HasForeignKey(e => e.Id);
                        });

                    b.HasOne(e => e.OptionalSingle)
                        .WithOne(e => e.Root)
                        .HasForeignKey<OptionalSingle1>(e => e.RootId)
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne(e => e.OptionalSingleDerived)
                        .WithOne(e => e.DerivedRoot)
                        .HasForeignKey<OptionalSingle1Derived>(e => e.DerivedRootId)
                        .OnDelete(DeleteBehavior.ClientSetNull);

                    b.HasOne(e => e.OptionalSingleMoreDerived)
                        .WithOne(e => e.MoreDerivedRoot)
                        .HasForeignKey<OptionalSingle1MoreDerived>(e => e.MoreDerivedRootId)
                        .OnDelete(DeleteBehavior.ClientSetNull);

                    // TODO: Owned inheritance support #9630
                    b.HasOne(e => e.RequiredNonPkSingle)
                        .WithOne(e => e.Root)
                        .HasForeignKey<RequiredNonPkSingle1>(e => e.RootId);

                    b.HasOne(e => e.RequiredNonPkSingleDerived)
                        .WithOne(e => e.DerivedRoot)
                        .HasForeignKey<RequiredNonPkSingle1Derived>(e => e.DerivedRootId)
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne(e => e.RequiredNonPkSingleMoreDerived)
                        .WithOne(e => e.MoreDerivedRoot)
                        .HasForeignKey<RequiredNonPkSingle1MoreDerived>(e => e.MoreDerivedRootId)
                        .OnDelete(DeleteBehavior.Restrict);

                    // TODO: Owned inheritance support #9630
                    b.HasMany(e => e.RequiredChildrenAk)
                        .WithOne(e => e.Parent)
                        .HasPrincipalKey(e => e.AlternateId)
                        .HasForeignKey(e => e.ParentId);

                    modelBuilder.Entity<RequiredAk1>(
                        b =>
                        {
                            b.Property(e => e.AlternateId)
                                .ValueGeneratedOnAdd();

                            b.HasMany(e => e.Children)
                                .WithOne(e => e.Parent)
                                .HasPrincipalKey(e => e.AlternateId)
                                .HasForeignKey(e => e.ParentId);

                            b.HasMany(e => e.CompositeChildren)
                                .WithOne(e => e.Parent)
                                .HasPrincipalKey(
                                    e => new { e.Id, e.AlternateId })
                                .HasForeignKey(
                                    e => new { e.ParentId, e.ParentAlternateId });
                        });

                    modelBuilder.Entity<RequiredAk2>()
                        .Property(e => e.AlternateId)
                        .ValueGeneratedOnAdd();

                    modelBuilder.Entity<RequiredAk2Derived>();
                    modelBuilder.Entity<RequiredAk2MoreDerived>();

                    modelBuilder.Entity<RequiredAk1Derived>();
                    modelBuilder.Entity<RequiredAk1MoreDerived>();

                    b.HasMany(e => e.OptionalChildrenAk)
                        .WithOne(e => e.Parent)
                        .HasPrincipalKey(e => e.AlternateId)
                        .HasForeignKey(e => e.ParentId)
                        .OnDelete(DeleteBehavior.SetNull);

                    b.OwnsOne(
                        e => e.RequiredSingleAk, r =>
                        {
                            r.WithOwner(e => e.Root)
                                .HasPrincipalKey(e => e.AlternateId)
                                .HasForeignKey(e => e.RootId);

                            r.HasKey(e => e.Id);

                            r.Property(e => e.AlternateId)
                                .ValueGeneratedOnAdd();

                            r.OwnsOne(
                                e => e.Single, r2 =>
                                {
                                    r2.WithOwner(e => e.Back)
                                        .HasForeignKey(e => e.BackId)
                                        .HasPrincipalKey(e => e.AlternateId);

                                    r2.HasKey(e => e.Id);

                                    r2.Property(e => e.Id)
                                        .ValueGeneratedOnAdd();

                                    r2.Property(e => e.AlternateId)
                                        .ValueGeneratedOnAdd();

                                    r2.ToTable("RequiredSingleAk2");
                                });

                            r.OwnsOne(
                                e => e.SingleComposite, r2 =>
                                {
                                    r2.WithOwner(e => e.Back)
                                        .HasForeignKey(e => new { e.BackId, e.BackAlternateId })
                                        .HasPrincipalKey(e => new { e.Id, e.AlternateId });

                                    r2.HasKey(e => e.Id);

                                    r2.ToTable("RequiredSingleComposite2");
                                });

                            // Table splitting using AK is not supported #23208
                            r.ToTable("RequiredSingleAk1");
                        });

                    b.HasOne(e => e.OptionalSingleAk)
                        .WithOne(e => e.Root)
                        .HasPrincipalKey<Root>(e => e.AlternateId)
                        .HasForeignKey<OptionalSingleAk1>(e => e.RootId)
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne(e => e.OptionalSingleAkDerived)
                        .WithOne(e => e.DerivedRoot)
                        .HasPrincipalKey<Root>(e => e.AlternateId)
                        .HasForeignKey<OptionalSingleAk1Derived>(e => e.DerivedRootId)
                        .OnDelete(DeleteBehavior.ClientSetNull);

                    b.HasOne(e => e.OptionalSingleAkMoreDerived)
                        .WithOne(e => e.MoreDerivedRoot)
                        .HasPrincipalKey<Root>(e => e.AlternateId)
                        .HasForeignKey<OptionalSingleAk1MoreDerived>(e => e.MoreDerivedRootId)
                        .OnDelete(DeleteBehavior.ClientSetNull);

                    b.HasOne(e => e.RequiredNonPkSingleAk)
                        .WithOne(e => e.Root)
                        .HasPrincipalKey<Root>(e => e.AlternateId)
                        .HasForeignKey<RequiredNonPkSingleAk1>(e => e.RootId);

                    b.HasOne(e => e.RequiredNonPkSingleAkDerived)
                        .WithOne(e => e.DerivedRoot)
                        .HasPrincipalKey<Root>(e => e.AlternateId)
                        .HasForeignKey<RequiredNonPkSingleAk1Derived>(e => e.DerivedRootId)
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne(e => e.RequiredNonPkSingleAkMoreDerived)
                        .WithOne(e => e.MoreDerivedRoot)
                        .HasPrincipalKey<Root>(e => e.AlternateId)
                        .HasForeignKey<RequiredNonPkSingleAk1MoreDerived>(e => e.MoreDerivedRootId)
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasMany(e => e.RequiredCompositeChildren)
                        .WithOne(e => e.Parent)
                        .HasPrincipalKey(e => e.AlternateId)
                        .HasForeignKey(e => e.ParentAlternateId);
                });

            modelBuilder.Entity<Optional1>(
                b =>
                {
                    b.HasMany(e => e.Children)
                        .WithOne(e => e.Parent)
                        .HasForeignKey(e => e.ParentId)
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasMany(e => e.CompositeChildren)
                        .WithOne(e => e.Parent2)
                        .HasForeignKey(e => new { e.Parent2Id });
                });

            modelBuilder.Entity<Optional1Derived>();
            modelBuilder.Entity<Optional1MoreDerived>();
            modelBuilder.Entity<Optional2Derived>();
            modelBuilder.Entity<Optional2MoreDerived>();

            modelBuilder.Entity<OptionalSingle1>()
                .HasOne(e => e.Single)
                .WithOne(e => e.Back)
                .HasForeignKey<OptionalSingle2>(e => e.BackId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<OptionalSingle2>(
                b =>
                {
                    b.HasDiscriminator(e => e.Disc)
                        .HasValue<OptionalSingle2>(new MyDiscriminator(1))
                        .HasValue<OptionalSingle2Derived>(new MyDiscriminator(2))
                        .HasValue<OptionalSingle2MoreDerived>(new MyDiscriminator(3));

                    b.Property(e => e.Disc)
                        .HasConversion(
                            v => v.Value,
                            v => new MyDiscriminator(v),
                            new ValueComparer<MyDiscriminator>(
                                (l, r) => l.Value == r.Value,
                                v => v.Value.GetHashCode(),
                                v => new MyDiscriminator(v.Value)))
                        .Metadata
                        .SetAfterSaveBehavior(PropertySaveBehavior.Save);
                });

            modelBuilder.Entity<RequiredNonPkSingle1>()
                .HasOne(e => e.Single)
                .WithOne(e => e.Back)
                .HasForeignKey<RequiredNonPkSingle2>(e => e.BackId);

            modelBuilder.Entity<RequiredNonPkSingle2Derived>();
            modelBuilder.Entity<RequiredNonPkSingle2MoreDerived>();

            modelBuilder.Entity<RequiredNonPkSingleAk1>(
                b =>
                {
                    b.Property(e => e.AlternateId)
                        .ValueGeneratedOnAdd();

                    b.HasOne(e => e.Single)
                        .WithOne(e => e.Back)
                        .HasForeignKey<RequiredNonPkSingleAk2>(e => e.BackId)
                        .HasPrincipalKey<RequiredNonPkSingleAk1>(e => e.AlternateId);
                });

            modelBuilder.Entity<OptionalAk1>(
                b =>
                {
                    b.Property(e => e.AlternateId)
                        .ValueGeneratedOnAdd();

                    b.HasMany(e => e.Children)
                        .WithOne(e => e.Parent)
                        .HasPrincipalKey(e => e.AlternateId)
                        .HasForeignKey(e => e.ParentId)
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasMany(e => e.CompositeChildren)
                        .WithOne(e => e.Parent)
                        .HasPrincipalKey(
                            e => new { e.Id, e.AlternateId })
                        .HasForeignKey(
                            e => new { e.ParentId, e.ParentAlternateId });
                });

            modelBuilder.Entity<OptionalAk1Derived>();
            modelBuilder.Entity<OptionalAk1MoreDerived>();

            modelBuilder.Entity<OptionalAk2>()
                .Property(e => e.AlternateId)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<OptionalAk2Derived>();
            modelBuilder.Entity<OptionalAk2MoreDerived>();

            modelBuilder.Entity<RequiredNonPkSingleAk2>()
                .Property(e => e.AlternateId)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<RequiredNonPkSingleAk2Derived>();
            modelBuilder.Entity<RequiredNonPkSingleAk2MoreDerived>();

            modelBuilder.Entity<OptionalSingleAk1>(
                b =>
                {
                    b.Property(e => e.AlternateId)
                        .ValueGeneratedOnAdd();

                    b.HasOne(e => e.Single)
                        .WithOne(e => e.Back)
                        .HasForeignKey<OptionalSingleAk2>(e => e.BackId)
                        .HasPrincipalKey<OptionalSingleAk1>(e => e.AlternateId)
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne(e => e.SingleComposite)
                        .WithOne(e => e.Back)
                        .HasForeignKey<OptionalSingleComposite2>(
                            e => new { e.BackId, e.ParentAlternateId })
                        .HasPrincipalKey<OptionalSingleAk1>(
                            e => new { e.Id, e.AlternateId });
                });

            modelBuilder.Entity<OptionalSingleAk2>()
                .Property(e => e.AlternateId)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<OptionalSingleAk2Derived>();
            modelBuilder.Entity<OptionalSingleAk2MoreDerived>();

            modelBuilder.Entity<RequiredComposite1>(
                eb =>
                {
                    eb.Property(e => e.Id).ValueGeneratedNever();

                    eb.HasKey(
                        e => new { e.Id, e.ParentAlternateId });

                    eb.HasMany(e => e.CompositeChildren)
                        .WithOne(e => e.Parent)
                        .HasPrincipalKey(
                            e => new { e.Id, e.ParentAlternateId })
                        .HasForeignKey(
                            e => new { e.ParentId, e.ParentAlternateId });
                });

            modelBuilder.Entity<OptionalOverlapping2>(
                eb =>
                {
                    eb.Property(e => e.Id).ValueGeneratedNever();

                    eb.HasKey(
                        e => new { e.Id, e.ParentAlternateId });

                    eb.HasOne(e => e.Root)
                        .WithMany()
                        .HasPrincipalKey(e => e.AlternateId)
                        .HasForeignKey(e => e.ParentAlternateId);
                });

            modelBuilder.Entity<BadCustomer>();
            modelBuilder.Entity<BadOrder>();

            modelBuilder.Entity<QuestTask>();

            modelBuilder.Entity<QuizTask>()
                .HasMany(qt => qt.Choices)
                .WithOne()
                .HasForeignKey(tc => tc.QuestTaskId);

            modelBuilder.Entity<HiddenAreaTask>()
                .HasMany(hat => hat.Choices)
                .WithOne()
                .HasForeignKey(tc => tc.QuestTaskId);

            modelBuilder.Entity<TaskChoice>();
            modelBuilder.Entity<ParentAsAChild>();
            modelBuilder.Entity<ChildAsAParent>();

            modelBuilder.Entity<Poost>();
            modelBuilder.Entity<Bloog>();

            modelBuilder.Entity<Produce>()
                .HasIndex(e => e.BarCode)
                .IsUnique();

            modelBuilder.Entity<SharedFkRoot>(
                builder =>
                {
                    builder.HasMany(x => x.Dependants).WithOne(x => x.Root)
                        .HasForeignKey(x => new { x.RootId })
                        .HasPrincipalKey(x => x.Id)
                        .OnDelete(DeleteBehavior.Cascade);

                    builder.HasMany(x => x.Parents).WithOne(x => x.Root)
                        .HasForeignKey(x => new { x.RootId })
                        .HasPrincipalKey(x => x.Id)
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity<SharedFkParent>(
                builder =>
                {
                    builder.HasOne(x => x.Dependant).WithOne(x => x!.Parent).IsRequired(false)
                        .HasForeignKey<SharedFkParent>(x => new { x.RootId, x.DependantId })
                        .HasPrincipalKey<SharedFkDependant>(x => new { x.RootId, x.Id })
                        .OnDelete(DeleteBehavior.ClientSetNull);
                });

            modelBuilder.Entity<SharedFkDependant>();

            modelBuilder.Entity<Owner>();

            modelBuilder.Entity<OwnerWithKeyedCollection>(
                b =>
                {
                    b.Navigation(e => e.Owned).IsRequired();
                    b.Navigation(e => e.OwnedWithKey).IsRequired();

                    b.OwnsMany(
                        e => e.OwnedCollectionPrivateKey,
                        b => b.HasKey("OwnerWithKeyedCollectionId", "PrivateKey"));
                });

            modelBuilder
                .Entity<OwnerWithNonCompositeOwnedCollection>()
                .OwnsMany(e => e.Owned, owned => owned.HasKey("Id"));

            modelBuilder.Entity<OwnerNoKeyGeneration>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();

                    b.OwnsOne(
                        e => e.Owned,
                        b => b.Property("OwnerNoKeyGenerationId").ValueGeneratedNever());
                    b.OwnsMany(
                        e => e.OwnedCollection,
                        b =>
                        {
                            b.Property<int>("OwnedNoKeyGenerationId").ValueGeneratedNever();
                            b.Property("OwnerNoKeyGenerationId").ValueGeneratedNever();
                        });
                });

            modelBuilder.Entity<Provider>().HasData(
                new Provider { Id = "prov1" },
                new Provider { Id = "prov2" });

            modelBuilder.Entity<Partner>().HasData(
                new Partner { Id = "partner1" });

            modelBuilder.Entity<ProviderContract>(
                b =>
                {
                    b.HasOne(p => p.Partner).WithMany().IsRequired().HasForeignKey("PartnerId");
                    b.HasOne<Provider>().WithMany().IsRequired().HasForeignKey("ProviderId");

                    b.HasDiscriminator<string>("ProviderId")
                        .HasValue<ProviderContract1>("prov1")
                        .HasValue<ProviderContract2>("prov2");
                });

            modelBuilder.Entity<EventDescriptorZ>(
                b =>
                {
                    b.Property<long>("EntityZId");
                    b.HasOne(e => e.EntityZ).WithMany().HasForeignKey("EntityZId").IsRequired();
                });

            modelBuilder.Entity<City>();

            modelBuilder.Entity<AccessState>(
                b =>
                {
                    b.Property(e => e.AccessStateId).ValueGeneratedNever();
                    b.HasData(new AccessState { AccessStateId = 1 });
                });

            modelBuilder.Entity<Cruiser>(
                b =>
                {
                    b.Property(e => e.IdUserState).HasDefaultValue(1);
                    b.HasOne(e => e.UserState).WithMany(e => e.Users).HasForeignKey(e => e.IdUserState);
                });

            modelBuilder.Entity<AccessStateWithSentinel>(
                b =>
                {
                    b.Property(e => e.AccessStateWithSentinelId).ValueGeneratedNever();
                    b.HasData(new AccessStateWithSentinel { AccessStateWithSentinelId = 1 });
                });

            modelBuilder.Entity<CruiserWithSentinel>(
                b =>
                {
                    b.Property(e => e.IdUserState).HasDefaultValue(1).HasSentinel(667);
                    b.HasOne(e => e.UserState).WithMany(e => e.Users).HasForeignKey(e => e.IdUserState);
                });

            modelBuilder.Entity<StringKeyAndIndexParent>(
                b =>
                {
                    b.HasAlternateKey(e => e.AlternateId);
                    b.OwnsOne(
                        x => x.Child, b =>
                        {
                            b.WithOwner(e => e.Parent).HasForeignKey(e => e.ParentId);
                        });
                });
        }
    }
}
