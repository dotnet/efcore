// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ModelBuilding
{
    public class CosmosModelBuilderGenericTest : ModelBuilderGenericTest
    {
        public class CosmosGenericNonRelationship : GenericNonRelationship
        {
            public override void Properties_can_set_row_version()
            {
                // Fails due to ETags
            }

            public override void Properties_can_be_made_concurrency_tokens()
            {
                // Fails due to ETags
            }

            public override void Properties_specified_by_string_are_shadow_properties_unless_already_known_to_be_CLR_properties()
            {
                // Fails due to extra shadow properties
            }

            protected override void Mapping_throws_for_non_ignored_array()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<OneDee>();

                var model = modelBuilder.FinalizeModel();
                var entityType = model.FindEntityType(typeof(OneDee));

                var property = entityType.FindProperty(nameof(OneDee.One));
                Assert.Null(property.GetProviderClrType());
                Assert.NotNull(property.FindTypeMapping());
            }

            [ConditionalFact]
            public override void Properties_can_have_provider_type_set_for_type()
            {
                var modelBuilder = CreateModelBuilder(c => c.Properties<string>().HaveConversion<byte[]>());

                modelBuilder.Entity<Quarks>(
                    b =>
                    {
                        b.Property(e => e.Up);
                        b.Property(e => e.Down);
                        b.Property<int>("Charm");
                        b.Property<string>("Strange");
                        b.Property<string>("__id").HasConversion((Type)null);
                    });

                var model = modelBuilder.FinalizeModel();
                var entityType = (IReadOnlyEntityType)model.FindEntityType(typeof(Quarks));

                Assert.Null(entityType.FindProperty("Up").GetProviderClrType());
                Assert.Same(typeof(byte[]), entityType.FindProperty("Down").GetProviderClrType());
                Assert.Null(entityType.FindProperty("Charm").GetProviderClrType());
                Assert.Same(typeof(byte[]), entityType.FindProperty("Strange").GetProviderClrType());
            }

            [ConditionalFact]
            public virtual void Partition_key_is_added_to_the_keys()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<Customer>()
                    .Ignore(b => b.Details)
                    .Ignore(b => b.Orders)
                    .HasPartitionKey(b => b.AlternateKey)
                    .Property(b => b.AlternateKey).HasConversion<string>();

                var model = modelBuilder.FinalizeModel();

                var entity = model.FindEntityType(typeof(Customer));

                Assert.Equal(
                    new[] { nameof(Customer.Id), nameof(Customer.AlternateKey) },
                    entity.FindPrimaryKey().Properties.Select(p => p.Name));
                Assert.Equal(
                    new[] { StoreKeyConvention.DefaultIdPropertyName, nameof(Customer.AlternateKey) },
                    entity.GetKeys().First(k => k != entity.FindPrimaryKey()).Properties.Select(p => p.Name));

                var idProperty = entity.FindProperty(StoreKeyConvention.DefaultIdPropertyName);
                Assert.Single(idProperty.GetContainingKeys());
                Assert.NotNull(idProperty.GetValueGeneratorFactory());
            }

            [ConditionalFact]
            public virtual void Partition_key_is_added_to_the_alternate_key_if_primary_key_contains_id()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<Customer>().HasKey(StoreKeyConvention.DefaultIdPropertyName);
                modelBuilder.Entity<Customer>()
                    .Ignore(b => b.Details)
                    .Ignore(b => b.Orders)
                    .HasPartitionKey(b => b.AlternateKey)
                    .Property(b => b.AlternateKey).HasConversion<string>();

                var model = modelBuilder.FinalizeModel();

                var entity = model.FindEntityType(typeof(Customer));

                Assert.Equal(
                    new[] { StoreKeyConvention.DefaultIdPropertyName },
                    entity.FindPrimaryKey().Properties.Select(p => p.Name));
                Assert.Equal(
                    new[] { StoreKeyConvention.DefaultIdPropertyName, nameof(Customer.AlternateKey) },
                    entity.GetKeys().First(k => k != entity.FindPrimaryKey()).Properties.Select(p => p.Name));
            }

            [ConditionalFact]
            public virtual void No_id_property_created_if_another_property_mapped_to_id()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<Customer>()
                    .Property(c => c.Name)
                    .ToJsonProperty(StoreKeyConvention.IdPropertyJsonName);
                modelBuilder.Entity<Customer>()
                    .Ignore(b => b.Details)
                    .Ignore(b => b.Orders);

                var model = modelBuilder.FinalizeModel();

                var entity = model.FindEntityType(typeof(Customer));

                Assert.Null(entity.FindProperty(StoreKeyConvention.DefaultIdPropertyName));
                Assert.Single(entity.GetKeys().Where(k => k != entity.FindPrimaryKey()));

                var idProperty = entity.GetDeclaredProperties()
                    .Single(p => p.GetJsonPropertyName() == StoreKeyConvention.IdPropertyJsonName);
                Assert.Single(idProperty.GetContainingKeys());
                Assert.NotNull(idProperty.GetValueGeneratorFactory());
            }

            [ConditionalFact]
            public virtual void No_id_property_created_if_another_property_mapped_to_id_in_pk()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<Customer>()
                    .Property(c => c.Name)
                    .ToJsonProperty(StoreKeyConvention.IdPropertyJsonName);
                modelBuilder.Entity<Customer>()
                    .Ignore(c => c.Details)
                    .Ignore(c => c.Orders)
                    .HasKey(c => c.Name);

                var model = modelBuilder.FinalizeModel();

                var entity = model.FindEntityType(typeof(Customer));

                Assert.Null(entity.FindProperty(StoreKeyConvention.DefaultIdPropertyName));
                Assert.Empty(entity.GetKeys().Where(k => k != entity.FindPrimaryKey()));

                var idProperty = entity.GetDeclaredProperties()
                    .Single(p => p.GetJsonPropertyName() == StoreKeyConvention.IdPropertyJsonName);
                Assert.Single(idProperty.GetContainingKeys());
                Assert.Null(idProperty.GetValueGeneratorFactory());
            }

            [ConditionalFact]
            public virtual void No_alternate_key_is_created_if_primary_key_contains_id()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<Customer>().HasKey(StoreKeyConvention.DefaultIdPropertyName);
                modelBuilder.Entity<Customer>()
                    .Ignore(b => b.Details)
                    .Ignore(b => b.Orders);

                var model = modelBuilder.FinalizeModel();

                var entity = model.FindEntityType(typeof(Customer));

                Assert.Equal(
                    new[] { StoreKeyConvention.DefaultIdPropertyName },
                    entity.FindPrimaryKey().Properties.Select(p => p.Name));
                Assert.Empty(entity.GetKeys().Where(k => k != entity.FindPrimaryKey()));

                var idProperty = entity.FindProperty(StoreKeyConvention.DefaultIdPropertyName);
                Assert.Single(idProperty.GetContainingKeys());
                Assert.Null(idProperty.GetValueGeneratorFactory());
            }

            [ConditionalFact]
            public virtual void No_alternate_key_is_created_if_primary_key_contains_id_and_partition_key()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<Customer>().HasKey(nameof(Customer.AlternateKey), StoreKeyConvention.DefaultIdPropertyName);
                modelBuilder.Entity<Customer>()
                    .Ignore(b => b.Details)
                    .Ignore(b => b.Orders)
                    .HasPartitionKey(b => b.AlternateKey)
                    .Property(b => b.AlternateKey).HasConversion<string>();

                var model = modelBuilder.FinalizeModel();

                var entity = model.FindEntityType(typeof(Customer));

                Assert.Equal(
                    new[] { nameof(Customer.AlternateKey), StoreKeyConvention.DefaultIdPropertyName },
                    entity.FindPrimaryKey().Properties.Select(p => p.Name));
                Assert.Empty(entity.GetKeys().Where(k => k != entity.FindPrimaryKey()));
            }

            [ConditionalFact]
            public virtual void No_alternate_key_is_created_if_id_is_partition_key()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<Customer>().HasKey(nameof(Customer.AlternateKey));
                modelBuilder.Entity<Customer>()
                    .Ignore(b => b.Details)
                    .Ignore(b => b.Orders)
                    .HasPartitionKey(b => b.AlternateKey)
                    .Property(b => b.AlternateKey).HasConversion<string>().ToJsonProperty("id");

                var model = modelBuilder.FinalizeModel();

                var entity = model.FindEntityType(typeof(Customer));

                Assert.Equal(
                    new[] { nameof(Customer.AlternateKey) },
                    entity.FindPrimaryKey().Properties.Select(p => p.Name));
                Assert.Empty(entity.GetKeys().Where(k => k != entity.FindPrimaryKey()));
            }

            protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder> configure = null)
                => CreateTestModelBuilder(CosmosTestHelpers.Instance, configure);
        }

        public class CosmosGenericInheritance : GenericInheritance
        {
            public override void Can_set_and_remove_base_type()
            {
                // Fails due to presence of __jObject
            }

            public override void Base_type_can_be_discovered_after_creating_foreign_keys_on_derived()
            {
                // Base discovered as owned
            }

            public override void Base_types_are_mapped_correctly_if_discovered_last()
            {
                // Base discovered as owned
            }

            public override void Relationships_on_derived_types_are_discovered_first_if_base_is_one_sided()
            {
                // Base discovered as owned
            }

            protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder> configure = null)
                => CreateTestModelBuilder(CosmosTestHelpers.Instance, configure);
        }

        public class CosmosGenericOneToMany : GenericOneToMany
        {
            [ConditionalFact(Skip = "#25279")]
            public override void Keyless_type_discovered_before_referenced_entity_type_does_not_leave_temp_id()
            {
                base.Keyless_type_discovered_before_referenced_entity_type_does_not_leave_temp_id();
            }

            public override void Navigation_to_shared_type_is_not_discovered_by_convention()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<CollectionNavigationToSharedType>();

                var model = modelBuilder.FinalizeModel();

                var principal = model.FindEntityType(typeof(CollectionNavigationToSharedType));
                var owned = principal.FindNavigation(nameof(CollectionNavigationToSharedType.Navigation)).TargetEntityType;
                Assert.True(owned.IsOwned());
                Assert.True(owned.HasSharedClrType);
                Assert.Equal("CollectionNavigationToSharedType.Navigation#Dictionary<string, object>",
                    owned.DisplayName());
            }

            protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder> configure = null)
                => CreateTestModelBuilder(CosmosTestHelpers.Instance, configure);
        }

        public class CosmosGenericManyToOne : GenericManyToOne
        {
            protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder> configure = null)
                => CreateTestModelBuilder(CosmosTestHelpers.Instance, configure);
        }

        public class CosmosGenericOneToOne : GenericOneToOne
        {
            public override void Navigation_to_shared_type_is_not_discovered_by_convention()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<ReferenceNavigationToSharedType>();

                var model = modelBuilder.FinalizeModel();

                var principal = model.FindEntityType(typeof(ReferenceNavigationToSharedType));
                var owned = principal.FindNavigation(nameof(ReferenceNavigationToSharedType.Navigation)).TargetEntityType;
                Assert.True(owned.IsOwned());
                Assert.True(owned.HasSharedClrType);
                Assert.Equal("ReferenceNavigationToSharedType.Navigation#Dictionary<string, object>",
                    owned.DisplayName());
            }

            protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder> configure = null)
                => CreateTestModelBuilder(CosmosTestHelpers.Instance, configure);
        }

        public class CosmosGenericManyToMany : GenericManyToMany
        {
            [ConditionalFact]
            public virtual void Can_use_shared_type_as_join_entity_with_partition_keys()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Ignore<OneToManyNavPrincipal>();
                modelBuilder.Ignore<OneToOneNavPrincipal>();

                modelBuilder.Entity<ManyToManyNavPrincipal>(mb =>
                {
                    mb.Property<string>("PartitionId");
                    mb.HasPartitionKey("PartitionId");
                });

                modelBuilder.Entity<NavDependent>(mb =>
                {
                    mb.Property<string>("PartitionId");
                    mb.HasPartitionKey("PartitionId");
                });

                modelBuilder.Entity<ManyToManyNavPrincipal>()
                    .HasMany(e => e.Dependents)
                    .WithMany(e => e.ManyToManyPrincipals)
                    .UsingEntity<Dictionary<string, object>>(
                        "JoinType",
                        e => e.HasOne<NavDependent>().WithMany().HasForeignKey("DependentId", "PartitionId"),
                        e => e.HasOne<ManyToManyNavPrincipal>().WithMany().HasForeignKey("PrincipalId", "PartitionId"),
                        e =>
                        {
                            e.HasPartitionKey("PartitionId");
                        });

                var model = modelBuilder.FinalizeModel();

                var joinType = model.FindEntityType("JoinType");
                Assert.NotNull(joinType);
                Assert.Equal(2, joinType.GetForeignKeys().Count());
                Assert.Equal(3, joinType.FindPrimaryKey().Properties.Count);
                Assert.Equal(6, joinType.GetProperties().Count());
                Assert.Equal("DbContext", joinType.GetContainer());
                Assert.Equal("PartitionId", joinType.GetPartitionKeyPropertyName());
                Assert.Equal("PartitionId", joinType.FindPrimaryKey().Properties.Last().Name);
            }

            [ConditionalFact]
            public virtual void Can_use_implicit_join_entity_with_partition_keys()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Ignore<OneToManyNavPrincipal>();
                modelBuilder.Ignore<OneToOneNavPrincipal>();

                modelBuilder.Entity<ManyToManyNavPrincipal>(mb =>
                {
                    mb.Ignore(e => e.Dependents);
                    mb.Property<string>("PartitionId");
                    mb.HasPartitionKey("PartitionId");
                });

                modelBuilder.Entity<NavDependent>(mb =>
                {
                    mb.Property<string>("PartitionId");
                    mb.HasPartitionKey("PartitionId");
                });

                modelBuilder.Entity<ManyToManyNavPrincipal>()
                    .HasMany(e => e.Dependents)
                    .WithMany(e => e.ManyToManyPrincipals);

                var model = modelBuilder.FinalizeModel();

                var joinType = model.FindEntityType("ManyToManyNavPrincipalNavDependent");
                Assert.NotNull(joinType);
                Assert.Equal(2, joinType.GetForeignKeys().Count());
                Assert.Equal(3, joinType.FindPrimaryKey().Properties.Count);
                Assert.Equal(6, joinType.GetProperties().Count());
                Assert.Equal("DbContext", joinType.GetContainer());
                Assert.Equal("PartitionId", joinType.GetPartitionKeyPropertyName());
                Assert.Equal("PartitionId", joinType.FindPrimaryKey().Properties.Last().Name);
            }

            [ConditionalFact]
            public virtual void Can_use_implicit_join_entity_with_partition_keys_changed()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Ignore<OneToManyNavPrincipal>();
                modelBuilder.Ignore<OneToOneNavPrincipal>();

                modelBuilder.Entity<ManyToManyNavPrincipal>(mb =>
                {
                    mb.Property<string>("PartitionId");
                    mb.HasPartitionKey("PartitionId");
                });

                modelBuilder.Entity<NavDependent>(mb =>
                {
                    mb.Property<string>("PartitionId");
                    mb.HasPartitionKey("PartitionId");
                });

                modelBuilder.Entity<ManyToManyNavPrincipal>(mb =>
                {
                    mb.Property<string>("Partition2Id");
                    mb.HasPartitionKey("Partition2Id");
                });

                modelBuilder.Entity<NavDependent>(mb =>
                {
                    mb.Property<string>("Partition2Id");
                    mb.HasPartitionKey("Partition2Id");
                });

                var model = modelBuilder.FinalizeModel();

                var joinType = model.FindEntityType("ManyToManyNavPrincipalNavDependent");
                Assert.NotNull(joinType);
                Assert.Equal(2, joinType.GetForeignKeys().Count());
                Assert.Equal(3, joinType.FindPrimaryKey().Properties.Count);
                Assert.Equal(6, joinType.GetProperties().Count());
                Assert.Equal("DbContext", joinType.GetContainer());
                Assert.Equal("Partition2Id", joinType.GetPartitionKeyPropertyName());
                Assert.Equal("Partition2Id", joinType.FindPrimaryKey().Properties.Last().Name);
            }

            public override void Join_type_is_automatically_configured_by_convention()
            {
                // Many-to-many not configured by convention on Cosmos
            }

            public override void Throws_for_ForeignKeyAttribute_on_navigation()
            {
                // Many-to-many not configured by convention on Cosmos
            }

            protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder> configure = null)
                => CreateTestModelBuilder(CosmosTestHelpers.Instance, configure);
        }

        public class CosmosGenericOwnedTypes : GenericOwnedTypes
        {
            [ConditionalFact(Skip = "#25279")]
            public override void Can_configure_owned_type_collection_with_one_call_afterwards()
            {
                base.Can_configure_owned_type_collection_with_one_call_afterwards();
            }

            public override void Deriving_from_owned_type_throws()
            {
                // On Cosmos the base type starts as owned
            }

            public override void Configuring_base_type_as_owned_throws()
            {
                // On Cosmos the base type starts as owned
            }

            [ConditionalFact]
            public virtual void Reference_type_is_discovered_as_owned()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<OneToOneOwnerWithField>(
                    e =>
                    {
                        e.Property(p => p.Id);
                        e.Property(p => p.AlternateKey);
                        e.Property(p => p.Description);
                        e.HasKey(p => p.Id);
                    });

                var model = modelBuilder.FinalizeModel();

                var owner = model.FindEntityType(typeof(OneToOneOwnerWithField));
                Assert.Equal(typeof(OneToOneOwnerWithField).FullName, owner.Name);
                var ownership = owner.FindNavigation(nameof(OneToOneOwnerWithField.OwnedDependent)).ForeignKey;
                Assert.True(ownership.IsOwnership);
                Assert.Equal(nameof(OneToOneOwnerWithField.OwnedDependent), ownership.PrincipalToDependent.Name);
                Assert.Equal(nameof(OneToOneOwnedWithField.OneToOneOwner), ownership.DependentToPrincipal.Name);
                Assert.Equal(nameof(OneToOneOwnerWithField.Id), ownership.PrincipalKey.Properties.Single().Name);
                var owned = ownership.DeclaringEntityType;
                Assert.Single(owned.GetForeignKeys());
                Assert.NotNull(model.FindEntityType(typeof(OneToOneOwnedWithField)));
                Assert.Equal(1, model.GetEntityTypes().Count(e => e.ClrType == typeof(OneToOneOwnedWithField)));
            }

            protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder> configure = null)
                => CreateTestModelBuilder(CosmosTestHelpers.Instance, configure);
        }
    }
}
