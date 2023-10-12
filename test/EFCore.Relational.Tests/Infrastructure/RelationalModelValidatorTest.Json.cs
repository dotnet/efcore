// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Infrastructure;

public partial class RelationalModelValidatorTest
{
    [ConditionalFact]
    public void Throw_when_non_json_entity_is_the_owner_of_json_entity_ref_ref()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityBasic>(
            b =>
            {
                b.OwnsOne(
                    x => x.OwnedReference, bb =>
                    {
                        bb.Ignore(x => x.NestedCollection);
                        bb.OwnsOne(x => x.NestedReference, bbb => bbb.ToJson("reference_reference"));
                    });
                b.Ignore(x => x.OwnedCollection);
            });

        VerifyError(
            RelationalStrings.JsonEntityOwnedByNonJsonOwnedType(
                nameof(ValidatorJsonOwnedRoot), nameof(ValidatorJsonEntityBasic)),
            modelBuilder);
    }

    [ConditionalFact]
    public void Throw_when_non_json_entity_is_the_owner_of_json_entity_ref_col()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityBasic>(
            b =>
            {
                b.OwnsOne(
                    x => x.OwnedReference, bb =>
                    {
                        bb.OwnsMany(x => x.NestedCollection, bbb => bbb.ToJson("reference_collection"));
                        bb.Ignore(x => x.NestedReference);
                    });
                b.Ignore(x => x.OwnedCollection);
            });

        VerifyError(
            RelationalStrings.JsonEntityOwnedByNonJsonOwnedType(
                nameof(ValidatorJsonOwnedRoot), nameof(ValidatorJsonEntityBasic)),
            modelBuilder);
    }

    [ConditionalFact]
    public void Throw_when_non_json_entity_is_the_owner_of_json_entity_col_ref()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityBasic>(
            b =>
            {
                b.OwnsMany(
                    x => x.OwnedCollection, bb =>
                    {
                        bb.Ignore(x => x.NestedCollection);
                        bb.OwnsOne(x => x.NestedReference, bbb => bbb.ToJson("collection_reference"));
                    });
                b.Ignore(x => x.OwnedReference);
            });

        VerifyError(
            RelationalStrings.JsonEntityOwnedByNonJsonOwnedType(
                nameof(ValidatorJsonOwnedRoot), nameof(ValidatorJsonOwnedRoot)),
            modelBuilder);
    }

    [ConditionalFact]
    public void Throw_when_non_json_entity_is_the_owner_of_json_entity_col_col()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityBasic>(
            b =>
            {
                b.OwnsMany(
                    x => x.OwnedCollection, bb =>
                    {
                        bb.Ignore(x => x.NestedReference);
                        bb.OwnsMany(x => x.NestedCollection, bbb => bbb.ToJson("collection_collection"));
                    });
                b.Ignore(x => x.OwnedReference);
            });

        VerifyError(
            RelationalStrings.JsonEntityOwnedByNonJsonOwnedType(
                nameof(ValidatorJsonOwnedRoot), nameof(ValidatorJsonOwnedRoot)),
            modelBuilder);
    }

    [ConditionalFact]
    public void Throw_when_json_entity_references_another_non_json_entity_via_reference()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityReferencedEntity>();
        modelBuilder.Entity<ValidatorJsonEntityJsonReferencingRegularEntity>(
            b =>
            {
                b.OwnsOne(
                    x => x.Owned, bb =>
                    {
                        bb.ToJson("reference");
                        bb.HasOne(x => x.Reference).WithOne().HasForeignKey<ValidatorJsonOwnedReferencingRegularEntity>(x => x.Fk);
                    });
            });

        VerifyError(
            RelationalStrings.JsonEntityReferencingRegularEntity(nameof(ValidatorJsonOwnedReferencingRegularEntity)),
            modelBuilder);
    }

    [ConditionalFact]
    public void Tpt_not_supported_for_owner_of_json_entity_on_base()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityInheritanceBase>(
            b =>
            {
                b.ToTable("Table1");
                b.OwnsOne(
                    x => x.ReferenceOnBase, bb =>
                    {
                        bb.ToJson("reference");
                    });
            });

        modelBuilder.Entity<ValidatorJsonEntityInheritanceDerived>(
            b =>
            {
                b.HasBaseType<ValidatorJsonEntityInheritanceBase>();
                b.ToTable("Table2");
                b.Ignore(x => x.ReferenceOnDerived);
                b.Ignore(x => x.CollectionOnDerived);
            });

        VerifyError(
            RelationalStrings.JsonEntityWithNonTphInheritanceOnOwner(nameof(ValidatorJsonEntityInheritanceBase)),
            modelBuilder);
    }

    [ConditionalFact]
    public void Tpt_not_supported_for_owner_of_json_entity_on_derived()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityInheritanceBase>(
            b =>
            {
                b.ToTable("Table1");
                b.Ignore(x => x.ReferenceOnBase);
            });

        modelBuilder.Entity<ValidatorJsonEntityInheritanceDerived>(
            b =>
            {
                b.ToTable("Table2");
                b.OwnsOne(x => x.ReferenceOnDerived, bb => bb.ToJson("reference"));
                b.Ignore(x => x.CollectionOnDerived);
            });

        VerifyError(
            RelationalStrings.JsonEntityWithNonTphInheritanceOnOwner(nameof(ValidatorJsonEntityInheritanceBase)),
            modelBuilder);
    }

    [ConditionalFact]
    public void Tpt_not_supported_for_owner_of_json_entity_mapping_strategy_explicitly_defined()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityInheritanceBase>(
            b =>
            {
                b.UseTptMappingStrategy();
                b.OwnsOne(
                    x => x.ReferenceOnBase, bb =>
                    {
                        bb.ToJson("reference");
                    });
            });

        modelBuilder.Entity<ValidatorJsonEntityInheritanceDerived>(
            b =>
            {
                b.HasBaseType<ValidatorJsonEntityInheritanceBase>();
                b.Ignore(x => x.ReferenceOnDerived);
                b.Ignore(x => x.CollectionOnDerived);
            });

        VerifyError(
            RelationalStrings.JsonEntityWithNonTphInheritanceOnOwner(nameof(ValidatorJsonEntityInheritanceBase)),
            modelBuilder);
    }

    [ConditionalFact]
    public void Tpt_not_supported_for_owner_of_json_entity_same_table_names_different_schemas()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityInheritanceBase>(
            b =>
            {
                b.ToTable("Table", "mySchema1");
                b.Ignore(x => x.ReferenceOnBase);
            });

        modelBuilder.Entity<ValidatorJsonEntityInheritanceDerived>(
            b =>
            {
                b.ToTable("Table", "mySchema2");
                b.OwnsOne(x => x.ReferenceOnDerived, bb => bb.ToJson("reference"));
                b.Ignore(x => x.CollectionOnDerived);
            });

        VerifyError(
            RelationalStrings.JsonEntityWithNonTphInheritanceOnOwner(nameof(ValidatorJsonEntityInheritanceBase)),
            modelBuilder);
    }

    [ConditionalFact]
    public void Tpc_not_supported_for_owner_of_json_entity()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityInheritanceBase>().UseTpcMappingStrategy();
        modelBuilder.Entity<ValidatorJsonEntityInheritanceAbstract>();
        modelBuilder.Entity<ValidatorJsonEntityInheritanceBase>(b => b.Ignore(x => x.ReferenceOnBase));

        modelBuilder.Entity<ValidatorJsonEntityInheritanceDerived>(
            b =>
            {
                b.OwnsOne(x => x.ReferenceOnDerived, bb => bb.ToJson("reference"));
                b.Ignore(x => x.CollectionOnDerived);
            });

        VerifyError(
            RelationalStrings.JsonEntityWithNonTphInheritanceOnOwner(nameof(ValidatorJsonEntityInheritanceBase)),
            modelBuilder);
    }

    [ConditionalFact]
    public void Json_entity_not_mapped_to_table_or_a_view_is_not_supported()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityBasic>(
            b =>
            {
                b.ToTable((string)null);
                b.OwnsOne(
                    x => x.OwnedReference, bb =>
                    {
                        bb.ToJson("reference");
                        bb.Ignore(x => x.NestedReference);
                        bb.Ignore(x => x.NestedCollection);
                    });
                b.Ignore(x => x.OwnedCollection);
            });

        VerifyError(
            RelationalStrings.JsonEntityWithOwnerNotMappedToTableOrView(nameof(ValidatorJsonEntityBasic)),
            modelBuilder);
    }

    [ConditionalFact]
    public void Json_multiple_json_entities_mapped_to_the_same_column()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntitySideBySide>(
            b =>
            {
                b.OwnsOne(x => x.Reference1, bb => bb.ToJson("json"));
                b.OwnsOne(x => x.Reference2, bb => bb.ToJson("json"));
                b.Ignore(x => x.Collection1);
                b.Ignore(x => x.Collection2);
            });

        VerifyError(
            RelationalStrings.JsonEntityMultipleRootsMappedToTheSameJsonColumn(
                "json", nameof(ValidatorJsonEntitySideBySide)),
            modelBuilder);
    }

    [ConditionalFact]
    public void Json_entity_with_defalt_value_on_a_property()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityBasic>(
            b =>
            {
                b.Ignore(x => x.OwnedCollection);
                b.OwnsOne(
                    x => x.OwnedReference, bb =>
                    {
                        bb.Ignore(x => x.NestedReference);
                        bb.Ignore(x => x.NestedCollection);
                        bb.ToJson("json");
                        bb.Property(x => x.Name).HasDefaultValue("myDefault");
                    });
            });

        VerifyError(RelationalStrings.JsonEntityWithDefaultValueSetOnItsProperty("ValidatorJsonOwnedRoot", "Name"), modelBuilder);
    }

    [ConditionalFact]
    public void Json_entity_with_table_splitting_throws()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityBasic>(
            b =>
            {
                b.ToTable("SharedTable");
                b.OwnsOne(
                    x => x.OwnedReference, bb =>
                    {
                        bb.ToJson("json");
                        bb.OwnsOne(x => x.NestedReference);
                        bb.OwnsMany(x => x.NestedCollection);
                    });
                b.Ignore(x => x.OwnedCollection);
            });

        modelBuilder.Entity<ValidatorJsonEntityTableSplitting>(
            b =>
            {
                b.ToTable("SharedTable");
                b.HasOne(x => x.Link).WithOne().HasForeignKey<ValidatorJsonEntityBasic>(x => x.Id);
            });

        VerifyError(
            RelationalStrings.JsonEntityWithTableSplittingIsNotSupported,
            modelBuilder);
    }

    [ConditionalFact]
    public void Json_entity_with_explicit_ordinal_key_on_collection_throws()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityExplicitOrdinal>(
            b =>
            {
                b.OwnsMany(
                    x => x.OwnedCollection, bb =>
                    {
                        bb.ToJson("json");
                        bb.HasKey(x => x.Ordinal);
                    });
            });

        VerifyError(RelationalStrings.JsonEntityWithExplicitlyConfiguredOrdinalKey("ValidatorJsonOwnedExplicitOrdinal"), modelBuilder);
    }

    [ConditionalFact]
    public void Json_entity_with_key_having_json_property_name_configured_explicitly_throws()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityExplicitOrdinal>(
            b =>
            {
                b.OwnsMany(
                    x => x.OwnedCollection, bb =>
                    {
                        bb.ToJson("json");
                        bb.HasKey(x => x.Ordinal);
                        bb.Property(x => x.Ordinal).HasJsonPropertyName("Foo");
                    });
            });

        VerifyError(
            RelationalStrings.JsonEntityWithExplicitlyConfiguredJsonPropertyNameOnKey(
                nameof(ValidatorJsonOwnedExplicitOrdinal.Ordinal), nameof(ValidatorJsonOwnedExplicitOrdinal)),
            modelBuilder);
    }

    [ConditionalFact]
    public void Json_entity_with_multiple_properties_mapped_to_same_json_name()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityBasic>(
            b =>
            {
                b.OwnsOne(
                    x => x.OwnedReference, bb =>
                    {
                        bb.Property(x => x.Name).HasJsonPropertyName("Foo");
                        bb.Property(x => x.Number).HasJsonPropertyName("Foo");
                        bb.ToJson("reference");
                        bb.Ignore(x => x.NestedReference);
                        bb.Ignore(x => x.NestedCollection);
                    });
                b.Ignore(x => x.OwnedCollection);
            });

        VerifyError(
            RelationalStrings.JsonEntityWithMultiplePropertiesMappedToSameJsonProperty("ValidatorJsonOwnedRoot", "Foo"),
            modelBuilder);
    }

    [ConditionalFact]
    public void Json_entity_with_property_and_navigation_mapped_to_same_json_name()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityBasic>(
            b =>
            {
                b.OwnsOne(
                    x => x.OwnedReference, bb =>
                    {
                        bb.Property(x => x.Name);
                        bb.Property(x => x.Number);
                        bb.ToJson("reference");
                        bb.OwnsOne(x => x.NestedReference, bbb => bbb.HasJsonPropertyName("Name"));
                        bb.Ignore(x => x.NestedCollection);
                    });
                b.Ignore(x => x.OwnedCollection);
            });

        VerifyError(
            RelationalStrings.JsonEntityWithMultiplePropertiesMappedToSameJsonProperty(
                nameof(ValidatorJsonOwnedRoot), nameof(ValidatorJsonOwnedRoot.Name)),
            modelBuilder);
    }

    [ConditionalFact]
    public void Json_on_base_and_derived_mapped_to_same_column_throws()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityInheritanceBase>().OwnsOne(x => x.ReferenceOnBase, b => b.ToJson("jsonColumn"));
        modelBuilder.Entity<ValidatorJsonEntityInheritanceDerived>(
            b =>
            {
                b.HasBaseType<ValidatorJsonEntityInheritanceBase>();
                b.OwnsOne(x => x.ReferenceOnDerived, bb => bb.ToJson("jsonColumn"));
                b.Ignore(x => x.CollectionOnDerived);
            });

        VerifyError(
            RelationalStrings.JsonEntityMultipleRootsMappedToTheSameJsonColumn(
                "jsonColumn", nameof(ValidatorJsonEntityInheritanceBase)),
            modelBuilder);
    }

    [ConditionalFact]
    public void Json_entity_mapped_to_different_view_than_its_root_aggregate()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityBasic>(
            b =>
            {
                b.ToView("MyView");
                b.OwnsOne(
                    x => x.OwnedReference, bb =>
                    {
                        bb.ToJson();
                        bb.ToView("MyOtherView");
                        bb.Ignore(x => x.NestedReference);
                        bb.Ignore(x => x.NestedCollection);
                    });
                b.Ignore(x => x.OwnedCollection);
            });

        VerifyError(
            RelationalStrings.JsonEntityMappedToDifferentTableOrViewThanOwner(
                nameof(ValidatorJsonOwnedRoot), "MyOtherView", nameof(ValidatorJsonEntityBasic), "MyView"),
            modelBuilder);
    }

    [ConditionalFact]
    public void Json_entity_mapped_to_different_view_than_its_parent()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityBasic>(
            b =>
            {
                b.ToView("MyView");
                b.OwnsOne(
                    x => x.OwnedReference, bb =>
                    {
                        bb.ToJson();
                        bb.ToView("MyView");
                        bb.OwnsMany(x => x.NestedCollection, bbb => bbb.ToView("MyOtherView"));
                        bb.Ignore(x => x.NestedReference);
                    });
                b.Ignore(x => x.OwnedCollection);
            });

        VerifyError(
            RelationalStrings.JsonEntityMappedToDifferentTableOrViewThanOwner(
                nameof(ValidatorJsonOwnedBranch), "MyOtherView", nameof(ValidatorJsonOwnedRoot), "MyView"),
            modelBuilder);
    }

    [ConditionalFact]
    public void Json_entity_mapped_to_different_table_than_its_parent()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityBasic>(
            b =>
            {
                b.ToTable("MyTable");
                b.OwnsOne(
                    x => x.OwnedReference, bb =>
                    {
                        bb.ToJson();
                        bb.ToTable("MyJsonTable");
                        bb.Ignore(x => x.NestedCollection);
                        bb.Ignore(x => x.NestedReference);
                    });
                b.Ignore(x => x.OwnedCollection);
            });

        VerifyError(
            RelationalStrings.JsonEntityMappedToDifferentTableOrViewThanOwner(
                nameof(ValidatorJsonOwnedRoot), "MyJsonTable", nameof(ValidatorJsonEntityBasic), "MyTable"),
            modelBuilder);
    }

    [ConditionalFact]
    public void Json_entity_mapped_to_a_view_but_its_parent_is_mapped_to_a_table()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityBasic>(
            b =>
            {
                b.ToTable("MyTable");
                b.OwnsOne(
                    x => x.OwnedReference, bb =>
                    {
                        bb.ToJson();
                        bb.ToView("MyJsonView");
                        bb.Ignore(x => x.NestedCollection);
                        bb.Ignore(x => x.NestedReference);
                    });
                b.Ignore(x => x.OwnedCollection);
            });

        VerifyError(
            RelationalStrings.JsonEntityMappedToDifferentTableOrViewThanOwner(
                nameof(ValidatorJsonOwnedRoot), "MyJsonView", nameof(ValidatorJsonEntityBasic), "MyTable"),
            modelBuilder);
    }

    [ConditionalFact]
    public void Json_entity_mapped_to_a_table_but_its_parent_is_mapped_to_a_view()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityBasic>(
            b =>
            {
                b.ToView("MyView");
                b.OwnsOne(
                    x => x.OwnedReference, bb =>
                    {
                        bb.ToJson();
                        bb.ToTable("MyJsonTable");
                        bb.Ignore(x => x.NestedCollection);
                        bb.Ignore(x => x.NestedReference);
                    });
                b.Ignore(x => x.OwnedCollection);
            });

        VerifyError(
            RelationalStrings.JsonEntityMappedToDifferentTableOrViewThanOwner(
                nameof(ValidatorJsonOwnedRoot), "MyJsonTable", nameof(ValidatorJsonEntityBasic), "MyView"),
            modelBuilder);
    }

    [ConditionalFact]
    public void SeedData_on_json_entity_throws_meaningful_exception()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityBasic>(
            b =>
            {
                b.HasData(new { Id = 1, OwnedReference = new { Name = "foo", Number = 1 } });
                b.OwnsOne(
                    x => x.OwnedReference, bb =>
                    {
                        bb.ToJson();
                        bb.Ignore(x => x.NestedCollection);
                        bb.Ignore(x => x.NestedReference);
                    });
                b.Ignore(x => x.OwnedCollection);
            });

        VerifyError(
            RelationalStrings.HasDataNotSupportedForEntitiesMappedToJson(nameof(ValidatorJsonEntityBasic)),
            modelBuilder);
    }

    [ConditionalFact]
    public void SeedData_on_entity_with_json_navigation_throws_meaningful_exception()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityBasic>(
            b =>
            {
                b.HasData(new { Id = 1 });
                b.OwnsMany(
                    x => x.OwnedCollection, bb =>
                    {
                        bb.ToJson();
                        bb.HasData(
                            new { Name = "foo", Number = 1 },
                            new { Name = "bar", Number = 2 });
                        bb.Ignore(x => x.NestedCollection);
                        bb.Ignore(x => x.NestedReference);
                    });
                b.Ignore(x => x.OwnedReference);
            });

        VerifyError(
            RelationalStrings.HasDataNotSupportedForEntitiesMappedToJson(nameof(ValidatorJsonOwnedRoot)),
            modelBuilder);
    }

    private class ValidatorJsonEntityBasic
    {
        public int Id { get; set; }
        public ValidatorJsonOwnedRoot OwnedReference { get; set; }
        public List<ValidatorJsonOwnedRoot> OwnedCollection { get; set; }
    }

    private abstract class ValidatorJsonEntityInheritanceAbstract : ValidatorJsonEntityInheritanceBase
    {
        public Guid Guid { get; set; }
    }

    private class ValidatorJsonEntityInheritanceBase
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ValidatorJsonOwnedBranch ReferenceOnBase { get; set; }
    }

    private class ValidatorJsonEntityInheritanceDerived : ValidatorJsonEntityInheritanceAbstract
    {
        public bool Switch { get; set; }

        public ValidatorJsonOwnedBranch ReferenceOnDerived { get; set; }

        public List<ValidatorJsonOwnedBranch> CollectionOnDerived { get; set; }
    }

    private class ValidatorJsonOwnedRoot
    {
        public string Name { get; set; }
        public int Number { get; }

        public ValidatorJsonOwnedBranch NestedReference { get; }
        public List<ValidatorJsonOwnedBranch> NestedCollection { get; }
    }

    private class ValidatorJsonOwnedBranch
    {
        public double Number { get; set; }
    }

    private class ValidatorJsonEntityExplicitOrdinal
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public List<ValidatorJsonOwnedExplicitOrdinal> OwnedCollection { get; set; }
    }

    private class ValidatorJsonOwnedExplicitOrdinal
    {
        public int Ordinal { get; set; }
        public DateTime Date { get; set; }
    }

    private class ValidatorJsonEntityJsonReferencingRegularEntity
    {
        public int Id { get; set; }
        public ValidatorJsonOwnedReferencingRegularEntity Owned { get; set; }
    }

    private class ValidatorJsonOwnedReferencingRegularEntity
    {
        public string Foo { get; set; }

        public int? Fk { get; }
        public ValidatorJsonEntityReferencedEntity Reference { get; }
    }

    private class ValidatorJsonEntityReferencedEntity
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
    }

    private class ValidatorJsonEntitySideBySide
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ValidatorJsonOwnedBranch Reference1 { get; set; }
        public ValidatorJsonOwnedBranch Reference2 { get; set; }
        public List<ValidatorJsonOwnedBranch> Collection1 { get; set; }
        public List<ValidatorJsonOwnedBranch> Collection2 { get; set; }
    }

    private class ValidatorJsonEntityTableSplitting
    {
        public int Id { get; set; }
        public ValidatorJsonEntityBasic Link { get; set; }
    }
}
