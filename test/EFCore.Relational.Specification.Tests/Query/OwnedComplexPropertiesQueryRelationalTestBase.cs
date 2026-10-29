// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class OwnedComplexPropertiesQueryRelationalTestBase(NonSharedFixture fixture)
    : OwnedComplexPropertiesQueryTestBase(fixture)
{
    protected TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected void AssertSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected);

    [Fact]
    public virtual async Task Owned_reference_separate_table()
    {
        var contextFactory = await InitializeNonSharedTest<Context>(
            onModelCreating: mb => mb.Entity<Owner>(b =>
            {
                b.Ignore(e => e.OwnedCollection);
                b.OwnsOne(
                    e => e.Owned, ob =>
                    {
                        ob.ToTable("Owned");
                        ob.ComplexProperty(o => o.Address);
                    });
            }),
            seed: Seed);

        using var context = contextFactory.CreateDbContext();
        var owners = await context.Owners.OrderBy(e => e.Id).ToListAsync();

        AssertOwners(owners);
    }

    [Fact]
    public virtual async Task Owned_reference_separate_table_with_key_on_complex_property()
    {
        var contextFactory = await InitializeNonSharedTest<Context>(
            onModelCreating: mb => mb.Entity<Owner>(b =>
            {
                b.Ignore(e => e.OwnedCollection);
                b.OwnsOne(
                    e => e.Owned, ob =>
                    {
                        ob.ToTable("Owned");
                        ob.ComplexProperty(o => o.Address);
                        ob.HasKey("Address.Country.Code");
                    });
            }),
            seed: Seed);

        using var context = contextFactory.CreateDbContext();
        var owners = await context.Owners.OrderBy(e => e.Id).ToListAsync();

        AssertOwners(owners);
    }

    [Fact]
    public virtual async Task Owned_reference_with_json_complex_property()
    {
        var contextFactory = await InitializeNonSharedTest<Context>(
            onModelCreating: mb => mb.Entity<Owner>(b =>
            {
                b.Ignore(e => e.OwnedCollection);
                b.OwnsOne(e => e.Owned, ob => ob.ComplexProperty(o => o.Address, ab => ab.ToJson()));
            }),
            seed: Seed);

        using var context = contextFactory.CreateDbContext();
        var owners = await context.Owners.OrderBy(e => e.Id).ToListAsync();

        AssertOwners(owners);
    }

    [Fact]
    public virtual async Task Owned_reference_json()
    {
        var contextFactory = await InitializeNonSharedTest<Context>(
            onModelCreating: mb => mb.Entity<Owner>(b =>
            {
                b.Ignore(e => e.OwnedCollection);
                b.OwnsOne(
                    e => e.Owned, ob =>
                    {
                        ob.ToJson();
                        ob.ComplexProperty(o => o.Address);
                    });
            }),
            seed: Seed);

        using var context = contextFactory.CreateDbContext();
        var owners = await context.Owners.OrderBy(e => e.Id).ToListAsync();

        AssertOwners(owners);
    }

    [Fact]
    public virtual async Task Owned_reference_in_subquery()
    {
        var contextFactory = await InitializeNonSharedTest<Context>(
            onModelCreating: ConfigureOwnedReference,
            seed: Seed);

        using var context = contextFactory.CreateDbContext();
        var owneds = await context.Owners.AsNoTracking()
            .OrderBy(e => e.Id)
            .Take(5)
            .Where(e => e.Id > 1)
            .Select(e => e.Owned)
            .ToListAsync();

        var owned = Assert.Single(owneds);
        Assert.Equal("Berlin", owned.Address.City);
        Assert.Equal("DE", owned.Address.Country.Code);
    }

    [Fact]
    public virtual async Task Owned_reference_with_json_complex_property_in_subquery()
    {
        var contextFactory = await InitializeNonSharedTest<Context>(
            onModelCreating: mb => mb.Entity<Owner>(b =>
            {
                b.Ignore(e => e.OwnedCollection);
                b.OwnsOne(e => e.Owned, ob => ob.ComplexProperty(o => o.Address, ab => ab.ToJson()));
            }),
            seed: Seed);

        using var context = contextFactory.CreateDbContext();
        var owneds = await context.Owners.AsNoTracking()
            .OrderBy(e => e.Id)
            .Take(5)
            .Where(e => e.Id > 1)
            .Select(e => e.Owned)
            .ToListAsync();

        var owned = Assert.Single(owneds);
        Assert.Equal("Berlin", owned.Address.City);
        Assert.Equal("DE", owned.Address.Country.Code);
    }

    [Fact]
    public virtual async Task Owned_reference_with_distinct()
    {
        var contextFactory = await InitializeNonSharedTest<Context>(
            onModelCreating: ConfigureOwnedReference,
            seed: Seed);

        using var context = contextFactory.CreateDbContext();
        var owners = await context.Owners.Distinct().OrderBy(e => e.Id).ToListAsync();

        AssertOwners(owners);
    }

    [Fact]
    public virtual async Task Owned_reference_in_union()
    {
        var contextFactory = await InitializeNonSharedTest<Context>(
            onModelCreating: ConfigureOwnedReference,
            seed: Seed);

        using var context = contextFactory.CreateDbContext();
        var owners = await context.Owners.Where(e => e.Id == 1)
            .Union(context.Owners.Where(e => e.Id == 2))
            .OrderBy(e => e.Id)
            .ToListAsync();

        AssertOwners(owners);
    }

    [Fact]
    public virtual Task Owned_reference_on_TPT_owner()
        => AssertOwnedReferencesWithDerivedOwner(b => b.UseTptMappingStrategy());

    [Fact]
    public virtual Task Owned_reference_on_TPC_owner()
        => AssertOwnedReferencesWithDerivedOwner(
            b =>
            {
                b.UseTpcMappingStrategy();
                b.OwnsOne(e => e.Owned, ob => ob.ToTable("Owned"));
            },
            b => b.ConfigureWarnings(w => w.Log(RelationalEventId.ForeignKeyTpcPrincipalWarning)));

    [Fact]
    public virtual async Task Owned_reference_entity_splitting()
    {
        var contextFactory = await InitializeNonSharedTest<Context>(
            onModelCreating: mb =>
            {
                mb.Ignore<Owner>();
                mb.Entity<OwnerOfSplit>(b => b.OwnsOne(
                    e => e.Owned, ob =>
                    {
                        ob.SplitToTable("SplitOwnedDescription", t => t.Property(o => o.Description));
                        ob.ComplexProperty(o => o.Address);
                    }));
            },
            seed: context =>
            {
                context.AddRange(
                    new OwnerOfSplit
                    {
                        Id = 1,
                        Owned = new SplitOwned
                        {
                            Name = "Owned1",
                            Description = "Description1",
                            Address = CreateOwned("", "1 Main St", "Sydney", "Australia", "AU").Address
                        }
                    },
                    new OwnerOfSplit
                    {
                        Id = 2,
                        Owned = new SplitOwned
                        {
                            Name = "Owned2",
                            Description = "Description2",
                            Address = CreateOwned("", "2 Hauptstrasse", "Berlin", "Germany", "DE").Address
                        }
                    });

                return context.SaveChangesAsync();
            });

        using var context = contextFactory.CreateDbContext();
        var owners = await context.Set<OwnerOfSplit>().OrderBy(e => e.Id).ToListAsync();

        Assert.Collection(
            owners,
            o =>
            {
                Assert.Equal("Description1", o.Owned.Description);
                Assert.Equal("Sydney", o.Owned.Address.City);
                Assert.Equal("AU", o.Owned.Address.Country.Code);
            },
            o =>
            {
                Assert.Equal("Description2", o.Owned.Description);
                Assert.Equal("Berlin", o.Owned.Address.City);
                Assert.Equal("DE", o.Owned.Address.Country.Code);
            });
    }

    [Fact]
    public virtual Task Update_complex_property_of_owned_reference_separate_table()
        => AssertUpdateOwnedReference(
            mb => mb.Entity<Owner>(b =>
            {
                b.Ignore(e => e.OwnedCollection);
                b.OwnsOne(
                    e => e.Owned, ob =>
                    {
                        ob.ToTable("Owned");
                        ob.ComplexProperty(o => o.Address);
                    });
            }),
            nested: true);

    [Fact]
    public virtual Task Update_json_complex_property_of_owned_reference()
        => AssertUpdateOwnedReference(
            mb => mb.Entity<Owner>(b =>
            {
                b.Ignore(e => e.OwnedCollection);
                b.OwnsOne(e => e.Owned, ob => ob.ComplexProperty(o => o.Address, ab => ab.ToJson()));
            }),
            nested: false);

    [Fact]
    public virtual Task Update_complex_property_of_json_owned_reference()
        => AssertUpdateOwnedReference(ConfigureJsonOwnedReference, nested: false);

    [Fact]
    public virtual Task Update_complex_properties_of_json_owned_reference()
        => AssertUpdateOwnedReference(ConfigureJsonOwnedReference, nested: true);

    [Fact]
    public virtual Task Update_complex_property_and_nested_owned_reference_with_same_json_name()
        => AssertUpdateComplexPropertyAndNestedOwnedReference(configureJsonNames: false);

    [Fact]
    public virtual Task Update_complex_property_and_nested_owned_reference_with_same_configured_json_name()
        => AssertUpdateComplexPropertyAndNestedOwnedReference(configureJsonNames: true);

    [Fact]
    public virtual async Task Complex_property_of_owned_reference_after_pushdown()
    {
        var contextFactory = await InitializeNonSharedTest<Context>(
            onModelCreating: ConfigureOwnedReference,
            seed: Seed);

        using var context = contextFactory.CreateDbContext();
        var owners = await context.Owners
            .Where(e => e.Owned.Name != "X")
            .OrderBy(e => e.Id)
            .Take(5)
            .Where(e => e.Owned.Address.City == "Berlin")
            .ToListAsync();

        Assert.Equal(2, Assert.Single(owners).Id);
    }

    [Fact]
    public virtual async Task Filter_on_json_complex_property_of_owned_reference_in_left_joined_subquery()
    {
        var contextFactory = await InitializeNonSharedTest<Context>(
            onModelCreating: mb => mb.Entity<Owner>(b =>
            {
                b.Ignore(e => e.OwnedCollection);
                b.OwnsOne(e => e.Owned, ob => ob.ComplexProperty(o => o.Address, ab => ab.ToJson()));
            }),
            seed: Seed);

        using var context = contextFactory.CreateDbContext();

        // The owner of the second row has no next owner, so its city is null and isn't Berlin
        var ids = await (from o in context.Owners
                         join n in context.Owners.OrderBy(e => e.Id).Take(5) on o.Id + 1 equals n.Id into next
                         from n in next.DefaultIfEmpty()
                         where n!.Owned.Address.City != "Berlin"
                         select o.Id).ToListAsync();

        Assert.Equal([2], ids);
    }

    [Fact]
    public virtual async Task Nested_complex_property_of_optional_owned_reference_after_pushdown()
    {
        var contextFactory = await InitializeNonSharedTest<Context>(
            onModelCreating: mb =>
            {
                mb.Ignore<Owner>();
                mb.Entity<OwnerOfOptional>(b => b.OwnsOne(
                    e => e.Holder, ob =>
                    {
                        ob.Property(o => o.Note).IsRequired();
                        ob.ComplexProperty(o => o.Address);
                    }));
            },
            seed: context =>
            {
                context.AddRange(
                    new OwnerOfOptional
                    {
                        Id = 1,
                        Holder = new AddressHolder
                        {
                            Note = "Note1",
                            Address = CreateOwned("", "1 Main St", "Sydney", "Australia", "AU").Address
                        }
                    },
                    new OwnerOfOptional
                    {
                        Id = 2,
                        Holder = new AddressHolder
                        {
                            Note = "Note2",
                            Address = CreateOwned("", "2 Hauptstrasse", "Berlin", "Germany", "DE").Address
                        }
                    },
                    new OwnerOfOptional { Id = 3 });

                return context.SaveChangesAsync();
            });

        using var context = contextFactory.CreateDbContext();
        var countries = await context.Set<OwnerOfOptional>()
            .OrderBy(e => e.Id)
            .Take(5)
            .Where(e => e.Id > 0)
            .Select(e => (string?)e.Holder!.Address.Country.Name)
            .ToListAsync();

        Assert.Equal(["Australia", "Germany", null], countries);

        var codes = await context.Set<OwnerOfOptional>()
            .AsNoTracking()
            .OrderBy(e => e.Id)
            .Take(5)
            .Distinct()
            .Where(e => e.Holder!.Address.Country.Code != "DE")
            .OrderBy(e => e.Id)
            .Select(e => (string?)e.Holder!.Address.Country.Code)
            .ToListAsync();

        Assert.Equal(["AU", null], codes);
    }

    [Fact]
    public virtual async Task Remove_optional_owned_reference_with_json_complex_property()
    {
        var contextFactory = await InitializeNonSharedTest<Context>(
            onModelCreating: mb =>
            {
                mb.Ignore<Owner>();
                mb.Entity<OwnerOfOptional>(b => b.OwnsOne(
                    e => e.Holder, ob =>
                    {
                        ob.Property(o => o.Note).IsRequired();
                        ob.ComplexProperty(o => o.Address, ab => ab.ToJson());
                    }));
            },
            seed: context =>
            {
                context.AddRange(
                    new OwnerOfOptional
                    {
                        Id = 1,
                        Holder = new AddressHolder
                        {
                            Note = "Note1",
                            Address = CreateOwned("", "1 Main St", "Sydney", "Australia", "AU").Address
                        }
                    },
                    new OwnerOfOptional
                    {
                        Id = 2,
                        Holder = new AddressHolder
                        {
                            Note = "Note2",
                            Address = CreateOwned("", "2 Hauptstrasse", "Berlin", "Germany", "DE").Address
                        }
                    });

                return context.SaveChangesAsync();
            });

        using (var context = contextFactory.CreateDbContext())
        {
            (await context.Set<OwnerOfOptional>().SingleAsync(e => e.Id == 1)).Holder = null;
            context.Add(new OwnerOfOptional { Id = 3 });

            ClearLog();
            await context.SaveChangesAsync();
        }

        using (var context = contextFactory.CreateDbContext())
        {
            var addresses = await context.Set<OwnerOfOptional>().OrderBy(e => e.Id).Select(e => e.Holder!.Address).ToListAsync();

            Assert.Collection(
                addresses,
                Assert.Null,
                a => Assert.Equal("Berlin", a.City),
                Assert.Null);
        }
    }

    [Fact]
    public virtual async Task ExecuteUpdate_complex_property_of_owned_reference()
    {
        var contextFactory = await InitializeNonSharedTest<Context>(
            onModelCreating: ConfigureOwnedReference,
            seed: Seed);

        using (var context = contextFactory.CreateDbContext())
        {
            ClearLog();
            await context.Owners.Where(e => e.Id == 1).ExecuteUpdateAsync(s => s.SetProperty(e => e.Owned.Address.City, "Melbourne"));
        }

        using (var context = contextFactory.CreateDbContext())
        {
            Assert.Equal("Melbourne", (await context.Owners.SingleAsync(e => e.Id == 1)).Owned.Address.City);
        }
    }

    [Fact]
    public virtual async Task ExecuteUpdate_complex_property_of_json_owned_reference()
    {
        var contextFactory = await InitializeNonSharedTest<Context>(
            onModelCreating: ConfigureJsonOwnedReference,
            seed: Seed);

        using var context = contextFactory.CreateDbContext();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => context.Owners.ExecuteUpdateAsync(s => s.SetProperty(e => e.Owned.Address.City, "Melbourne")));

        Assert.Equal(
            RelationalStrings.ExecuteOperationOnOwnedJsonIsNotSupported("ExecuteUpdate", "Owned"),
            Assert.IsType<InvalidOperationException>(exception.InnerException).Message);
    }

    [Fact]
    public virtual async Task Json_complex_properties_on_same_owned_type_under_two_navigations_throws()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => InitializeNonSharedTest<Context>(
                onModelCreating: mb =>
                {
                    mb.Entity<Owner>(b =>
                    {
                        b.Ignore(e => e.OwnedCollection);
                        b.OwnsOne(e => e.Owned, ob => ob.ComplexProperty(o => o.Address, ab => ab.ToJson("Address")));
                    });
                    // The default names are prefixed with the navigations, so they only collide when named explicitly
                    mb.Entity<SpecialOwner>(b => b.OwnsOne(
                        e => e.DerivedOwned, ob => ob.ComplexProperty(o => o.Address, ab => ab.ToJson("Address"))));
                }));

        Assert.Equal(
            RelationalStrings.ComplexPropertiesSharedJsonColumn("Owner.Owned#Owned.Address", "SpecialOwner.DerivedOwned#Owned.Address", "Address", "Owners"),
            exception.Message);
    }

    [Fact]
    public virtual async Task Update_scalar_and_complex_collection_of_json_owned_reference()
    {
        var contextFactory = await InitializeTaggedOwners(ob => ob.ToJson(), configureTag: null, ConfigureTagConverters);

        using (var context = contextFactory.CreateDbContext())
        {
            var owner = await context.Set<OwnerWithTag>().SingleAsync(e => e.Id == 1);
            owner.Owned.Tag.Label = "Uno";
            owner.Owned.Tags.Add(new Tag { Id = new TagId(11), Label = "Eleven", Code = new TagCode("E") });

            ClearLog();
            await context.SaveChangesAsync();
        }

        using (var context = contextFactory.CreateDbContext())
        {
            var owner = await context.Set<OwnerWithTag>().SingleAsync(e => e.Id == 1);

            Assert.Equal("Uno", owner.Owned.Tag.Label);
            Assert.Equal(["Ten", "Eleven"], owner.Owned.Tags.Select(t => t.Label));
        }
    }

    [Fact]
    public virtual Task Set_nullable_complex_property_of_json_owned_reference()
        => AssertUpdateNullableComplexProperty(
            initialValue: null, o => o.Outer = new OuterComplex { Inner = new InnerComplex { Value = "New" } }, expectedValue: "New");

    [Fact]
    public virtual Task Clear_nullable_complex_property_of_json_owned_reference()
        => AssertUpdateNullableComplexProperty(initialValue: "Old", o => o.Outer = null, expectedValue: null);

    [Fact]
    public virtual Task Update_property_in_nullable_complex_property_of_json_owned_reference()
        => AssertUpdateNullableComplexProperty(initialValue: "Old", o => o.Outer!.Inner.Value = "New", expectedValue: "New");

    [Fact(Skip = "Issue #35701")]
    public virtual async Task Optional_owned_reference_with_null_scalars_and_complex_property()
    {
        var contextFactory = await InitializeNonSharedTest<Context>(
            onModelCreating: mb =>
            {
                mb.Ignore<Owner>();
                mb.Entity<OwnerOfOptional>(b => b.OwnsOne(e => e.Holder, ob => ob.ComplexProperty(o => o.Address)));
            },
            seed: context =>
            {
                context.Add(
                    new OwnerOfOptional
                    {
                        Id = 1,
                        Holder = new AddressHolder { Address = CreateOwned("", "1 Main St", "Sydney", "Australia", "AU").Address }
                    });

                return context.SaveChangesAsync();
            });

        using var context = contextFactory.CreateDbContext();
        var owner = await context.Set<OwnerOfOptional>().SingleAsync();

        Assert.Null(owner.Holder!.Note);
        Assert.Equal("Sydney", owner.Holder.Address.City);
    }

    [Fact]
    public virtual Task Value_converter_in_complex_type_on_json_owned_reference()
        => AssertValueConverterInComplexType(ob => ob.ToJson(), configureTag: null);

    [Fact]
    public virtual Task Value_converter_in_json_complex_type_on_owned_reference()
        => AssertValueConverterInComplexType(configureOwned: null, tb => tb.ToJson());

    [Fact]
    public virtual Task Value_comparer_in_complex_type_on_json_owned_reference()
        => AssertValueComparerInComplexType(ob => ob.ToJson(), configureTag: null);

    [Fact]
    public virtual Task Value_comparer_in_json_complex_type_on_owned_reference()
        => AssertValueComparerInComplexType(configureOwned: null, tb => tb.ToJson());

    [Fact]
    public virtual Task Json_value_reader_writer_in_complex_type_on_json_owned_reference()
        => AssertJsonValueReaderWriterInComplexType(ob => ob.ToJson(), configureTag: null);

    [Fact]
    public virtual Task Json_value_reader_writer_in_json_complex_type_on_owned_reference()
        => AssertJsonValueReaderWriterInComplexType(configureOwned: null, tb => tb.ToJson());

    [Fact(Skip = "Issue #35701")]
    public override Task Optional_owned_reference_with_only_complex_property()
        => base.Optional_owned_reference_with_only_complex_property();

    [Fact]
    public virtual async Task Delete_owner_with_owned_complex_property_separate_table()
    {
        var contextFactory = await InitializeNonSharedTest<Context>(
            onModelCreating: mb => mb.Entity<Owner>(b =>
            {
                b.Ignore(e => e.OwnedCollection);
                b.OwnsOne(
                    e => e.Owned, ob =>
                    {
                        ob.ToTable("Owned");
                        ob.ComplexProperty(o => o.Address);
                    });
            }),
            seed: Seed);

        using (var context = contextFactory.CreateDbContext())
        {
            context.Remove(await context.Owners.SingleAsync(e => e.Id == 1));

            ClearLog();
            await context.SaveChangesAsync();
        }

        using (var context = contextFactory.CreateDbContext())
        {
            Assert.Equal(2, (await context.Owners.SingleAsync()).Id);
        }
    }

    protected override void ConfigureOwnedCollection(OwnedNavigationBuilder<Owner, Owned> builder)
        => builder.HasKey("OwnerId", nameof(Owned.Name));

    protected override void ConfigureTagCollection(ComplexCollectionBuilder<Tag> builder)
    {
        if (!builder.Metadata.DeclaringType.IsMappedToJson())
        {
            builder.ToJson();
        }
    }

    private async Task AssertJsonValueReaderWriterInComplexType(
        Action<OwnedNavigationBuilder<OwnerWithTag, TaggedOwned>>? configureOwned,
        Action<ComplexPropertyBuilder<Tag>>? configureTag)
    {
        var contextFactory = await InitializeTaggedOwners(
            configureOwned,
            configureTag,
            ConfigureTagConverters,
            onModelCreating: mb =>
            {
                var owned = mb.Model.FindEntityType(typeof(OwnerWithTag))!.FindNavigation(nameof(OwnerWithTag.Owned))!.TargetEntityType;
                foreach (var complexProperty in owned.GetComplexProperties())
                {
                    complexProperty.ComplexType.FindProperty(nameof(Tag.Code))!.SetJsonValueReaderWriterType(typeof(TagCodeReaderWriter));
                }
            });

        using (var context = contextFactory.CreateDbContext())
        {
            var owners = await context.Set<OwnerWithTag>().OrderBy(e => e.Id).ToListAsync();
            Assert.Equal([new TagCode("A"), new TagCode("B")], owners.Select(o => o.Owned.Tag.Code));
            Assert.Equal([new TagCode("A"), new TagCode("B")], owners.Select(o => o.Owned.Tags.Single().Code));

            owners[0].Owned.Tag.Code = new TagCode("C");

            ClearLog();
            await context.SaveChangesAsync();
        }

        using (var context = contextFactory.CreateDbContext())
        {
            var owner = await context.Set<OwnerWithTag>().SingleAsync(e => e.Id == 1);
            Assert.Equal(new TagCode("C"), owner.Owned.Tag.Code);
        }
    }

    protected class TagCodeReaderWriter : JsonValueReaderWriter<TagCode>
    {
        private static readonly ConstructorInfo Constructor = typeof(TagCodeReaderWriter).GetConstructor([])!;

        [Experimental(EFDiagnostics.PrecompiledQueryExperimental)]
        public override Expression ConstructorExpression
            => Expression.New(Constructor);

        public override TagCode FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
            => new(manager.CurrentReader.GetString()!.TrimStart('#'));

        public override void ToJsonTyped(Utf8JsonWriter writer, TagCode value)
            => writer.WriteStringValue("#" + value.Value);
    }

    // Address.City and the nested City navigation share a JSON name at different depths of the owned document
    private async Task AssertUpdateComplexPropertyAndNestedOwnedReference(bool configureJsonNames)
    {
        var contextFactory = await InitializeNonSharedTest<Context>(
            onModelCreating: mb =>
            {
                mb.Ignore<Owner>();
                mb.Entity<OwnerOfNested>(b => b.OwnsOne(
                    e => e.Owned, ob =>
                    {
                        ob.ToJson();
                        ob.ComplexProperty(
                            o => o.Address, ab =>
                            {
                                if (configureJsonNames)
                                {
                                    ab.Property(a => a.City).HasJsonPropertyName("Place");
                                }
                            });
                        ob.OwnsOne(
                            o => o.City, cb =>
                            {
                                if (configureJsonNames)
                                {
                                    cb.HasJsonPropertyName("Place");
                                }
                            });
                    }));
            },
            seed: context =>
            {
                context.Add(
                    new OwnerOfNested
                    {
                        Id = 1,
                        Owned = new NestedOwned
                        {
                            Address = CreateOwned("", "1 Main St", "Sydney", "Australia", "AU").Address,
                            City = new City { Name = "Sydney" }
                        }
                    });

                return context.SaveChangesAsync();
            });

        using (var context = contextFactory.CreateDbContext())
        {
            var owner = await context.Set<OwnerOfNested>().SingleAsync();
            owner.Owned.Address.City = "Melbourne";
            owner.Owned.City.Name = "Brisbane";

            ClearLog();
            await context.SaveChangesAsync();
        }

        using (var context = contextFactory.CreateDbContext())
        {
            var owner = await context.Set<OwnerOfNested>().SingleAsync();

            Assert.Equal("Melbourne", owner.Owned.Address.City);
            Assert.Equal("Brisbane", owner.Owned.City.Name);
        }
    }

    // Changing a nullable complex property between null and a value changes its JSON parent, so it can't be patched by property
    private async Task AssertUpdateNullableComplexProperty(
        string? initialValue,
        Action<OwnedWithNullableComplex> change,
        string? expectedValue)
    {
        var contextFactory = await InitializeNonSharedTest<Context>(
            onModelCreating: mb =>
            {
                mb.Ignore<Owner>();
                mb.Entity<OwnerOfNullableComplex>(b => b.OwnsOne(
                    e => e.Owned, ob =>
                    {
                        ob.ToJson();
                        ob.ComplexProperty(o => o.Outer, cb => cb.ComplexProperty(c => c.Inner));
                    }));
            },
            seed: context =>
            {
                context.Add(
                    new OwnerOfNullableComplex
                    {
                        Id = 1,
                        Owned = new OwnedWithNullableComplex
                        {
                            Name = "Owned",
                            Outer = initialValue == null ? null : new OuterComplex { Inner = new InnerComplex { Value = initialValue } }
                        }
                    });

                return context.SaveChangesAsync();
            });

        using (var context = contextFactory.CreateDbContext())
        {
            change((await context.Set<OwnerOfNullableComplex>().SingleAsync()).Owned);

            ClearLog();
            await context.SaveChangesAsync();
        }

        using (var context = contextFactory.CreateDbContext())
        {
            var owned = (await context.Set<OwnerOfNullableComplex>().SingleAsync()).Owned;

            Assert.Equal("Owned", owned.Name);
            Assert.Equal(expectedValue, owned.Outer?.Inner.Value);
        }
    }

    private static void ConfigureJsonOwnedReference(ModelBuilder modelBuilder)
        => modelBuilder.Entity<Owner>(b =>
        {
            b.Ignore(e => e.OwnedCollection);
            b.OwnsOne(
                e => e.Owned, ob =>
                {
                    ob.ToJson();
                    ob.ComplexProperty(o => o.Address);
                });
        });

    public class OwnerOfNested
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public NestedOwned Owned { get; set; } = null!;
    }

    public class NestedOwned
    {
        public Address Address { get; set; } = null!;
        public City City { get; set; } = null!;
    }

    public class City
    {
        public string Name { get; set; } = null!;
    }

    public class OwnerOfNullableComplex
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public OwnedWithNullableComplex Owned { get; set; } = null!;
    }

    public class OwnedWithNullableComplex
    {
        public string Name { get; set; } = null!;
        public OuterComplex? Outer { get; set; }
    }

    public class OuterComplex
    {
        public InnerComplex Inner { get; set; } = null!;
    }

    public class InnerComplex
    {
        public string Value { get; set; } = null!;
    }

    public class OwnerOfSplit
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public SplitOwned Owned { get; set; } = null!;
    }

    public class SplitOwned
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public Address Address { get; set; } = null!;
    }
}
