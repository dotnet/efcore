// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Update;

public abstract class ComplexCollectionJsonUpdateTestBase<TFixture>(TFixture fixture) : IClassFixture<TFixture>
    where TFixture : ComplexCollectionJsonUpdateTestBase<TFixture>.ComplexCollectionJsonUpdateFixtureBase, new()
{
    public TFixture Fixture { get; } = fixture;

    protected ComplexCollectionJsonContext CreateContext()
        => (ComplexCollectionJsonContext)Fixture.CreateContext();

    [Fact]
    public virtual Task Add_element_to_complex_collection_mapped_to_json()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var company = await context.Companies.OrderBy(c => c.Id).FirstAsync();

                var companyEntry = context.Entry(company);
                var budgetProperty = companyEntry.ComplexProperty(c => c.Department).Property(c => c.Budget);
                Assert.Equal(budgetProperty.CurrentValue, budgetProperty.OriginalValue);

                company.Contacts!.Add(new Contact { Name = "New Contact", PhoneNumbers = ["555-0000"] });

                Assert.Contains("Contacts (Complex: List<Contact>)", context.ChangeTracker.DebugView.LongView);
                Assert.Contains("Department (Complex: Department)", context.ChangeTracker.DebugView.LongView);
                Assert.Contains("Name: 'Initial Department'", context.ChangeTracker.DebugView.LongView);
                Assert.Contains("Employees (Complex: List<Employee>)", context.ChangeTracker.DebugView.LongView);

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                using (SuspendRecordingEvents())
                {
                    var company = await context.Companies.OrderBy(c => c.Id).FirstAsync();
                    Assert.Equal(3, company.Contacts!.Count);
                    Assert.Equal("New Contact", company.Contacts[2].Name);
                    Assert.Single(company.Contacts[2].PhoneNumbers);
                    Assert.Equal("555-0000", company.Contacts[2].PhoneNumbers[0]);
                }
            });

    [Fact]
    public virtual Task Remove_element_from_complex_collection_mapped_to_json()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var company = await context.Companies.OrderBy(c => c.Id).FirstAsync();

                company.Contacts!.RemoveAt(0);

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                using (SuspendRecordingEvents())
                {
                    var company = await context.Companies.OrderBy(c => c.Id).FirstAsync();
                    Assert.Single(company.Contacts!);
                    Assert.Equal("Second Contact", company.Contacts![0].Name);
                }
            });

    [Fact]
    public virtual Task Delete_complex_collection_owner_entity_mapped_to_json()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var company = await context.Companies.SingleAsync(c => c.Id == 1);

                context.Remove(company);

                ClearLog();

                await context.SaveChangesAsync();
            },
            async context =>
            {
                using (SuspendRecordingEvents())
                {
                    var exists = await context.Companies.AnyAsync(c => c.Id == 1);
                    Assert.False(exists);
                }
            });

    [Fact]
    public virtual Task Modify_element_in_complex_collection_mapped_to_json()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var company = await context.Companies.OrderBy(c => c.Id).FirstAsync();

                company.Contacts![0].Name = "First Contact - Modified";

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                using (SuspendRecordingEvents())
                {
                    var company = await context.Companies.OrderBy(c => c.Id).FirstAsync();
                    Assert.Equal("First Contact - Modified", company.Contacts![0].Name);
                }
            });

    [Fact]
    public virtual Task Move_elements_in_complex_collection_mapped_to_json()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var company = await context.Companies.OrderBy(c => c.Id).FirstAsync();

                var temp = company.Contacts![0];
                company.Contacts[0] = company.Contacts[1];
                company.Contacts[1] = temp;

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                using (SuspendRecordingEvents())
                {
                    var company = await context.Companies.OrderBy(c => c.Id).FirstAsync();
                    Assert.Equal("Second Contact", company.Contacts![0].Name);
                    Assert.Equal("First Contact", company.Contacts[1].Name);
                }
            });

    [Fact]
    public virtual Task Change_complex_collection_mapped_to_json_to_null_and_to_empty()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var company = await context.Companies.OrderBy(c => c.Id).FirstAsync();
                ClearLog();

                company.Contacts!.Clear();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                using (SuspendRecordingEvents())
                {
                    var company = await context.Companies.OrderBy(c => c.Id).FirstAsync();
                    Assert.NotNull(company.Contacts);
                    Assert.Empty(company.Contacts);
                    company.Contacts = null;
                }

                await context.SaveChangesAsync();
            },
            async context =>
            {
                using (SuspendRecordingEvents())
                {
                    var company = await context.Companies.OrderBy(c => c.Id).FirstAsync();
                    Assert.Null(company.Contacts);
                }
            });

    [Fact]
    public virtual Task Complex_collection_with_nested_complex_type_mapped_to_json()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var company = await context.Companies.OrderBy(c => c.Id).FirstAsync();

                company.Employees =
                [
                    new Employee
                    {
                        Name = "John Doe",
                        PhoneNumbers = ["555-1234", "555-5678"],
                        Address = new Address
                        {
                            Street = "123 Main St",
                            City = "Seattle",
                            PostalCode = "98101",
                            Country = "USA"
                        }
                    },
                    new Employee
                    {
                        Name = "Jane Smith",
                        PhoneNumbers = ["555-9876"],
                        Address = new Address
                        {
                            Street = "456 Oak Ave",
                            City = "Portland",
                            PostalCode = "97201",
                            Country = "USA"
                        }
                    }
                ];

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                using (SuspendRecordingEvents())
                {
                    var company = await context.Companies.OrderBy(c => c.Id).FirstAsync();
                    Assert.Equal(2, company.Employees!.Count);

                    var john = company.Employees[0];
                    Assert.Equal("John Doe", john.Name);
                    Assert.Equal("123 Main St", john.Address.Street);
                    Assert.Equal("Seattle", john.Address.City);

                    var jane = company.Employees[1];
                    Assert.Equal("Jane Smith", jane.Name);
                    Assert.Equal("456 Oak Ave", jane.Address.Street);
                    Assert.Equal("Portland", jane.Address.City);
                }
            });

    [Fact]
    public virtual Task Modify_multiple_complex_properties_mapped_to_json()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var company = await context.Companies.OrderBy(c => c.Id).FirstAsync();

                company.Contacts = [new Contact { Name = "Contact 1", PhoneNumbers = ["555-1111"] }];
                company.Department = new Department { Name = "Department A", Budget = 50000.00m };

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                using (SuspendRecordingEvents())
                {
                    var company = await context.Companies.OrderBy(c => c.Id).FirstAsync();
                    Assert.Single(company.Contacts!);
                    Assert.Equal("Contact 1", company.Contacts![0].Name);

                    Assert.NotNull(company.Department);
                    Assert.Equal("Department A", company.Department.Name);
                    Assert.Equal(50000.00m, company.Department.Budget);
                }
            });

    [Fact]
    public virtual Task Clear_complex_collection_mapped_to_json()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var company = await context.Companies.OrderBy(c => c.Id).FirstAsync();

                company.Contacts!.Clear();

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                using (SuspendRecordingEvents())
                {
                    var company = await context.Companies.OrderBy(c => c.Id).FirstAsync();
                    Assert.Empty(company.Contacts!);
                }
            });

    [Fact]
    public virtual Task Replace_entire_complex_collection_mapped_to_json()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var company = await context.Companies.OrderBy(c => c.Id).FirstAsync();

                company.Contacts =
                [
                    new Contact { Name = "Replacement Contact 1", PhoneNumbers = ["999-1111"] },
                    new Contact { Name = "Replacement Contact 2", PhoneNumbers = ["999-2222", "999-3333"] }
                ];

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                using (SuspendRecordingEvents())
                {
                    var company = await context.Companies.OrderBy(c => c.Id).FirstAsync();
                    Assert.Equal(2, company.Contacts!.Count);
                    Assert.Equal("Replacement Contact 1", company.Contacts[0].Name);
                    Assert.Equal("Replacement Contact 2", company.Contacts[1].Name);
                }
            });

    [Fact]
    public virtual Task Add_element_to_nested_complex_collection_mapped_to_json()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var company = await context.Companies.OrderBy(c => c.Id).FirstAsync();

                company.Employees![0].PhoneNumbers.Add("555-9999");

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                using (SuspendRecordingEvents())
                {
                    var company = await context.Companies.OrderBy(c => c.Id).FirstAsync();
                    var employee = company.Employees![0];
                    Assert.Equal(2, employee.PhoneNumbers.Count);
                    Assert.Equal("555-0001", employee.PhoneNumbers[0]);
                    Assert.Equal("555-9999", employee.PhoneNumbers[1]);
                }
            });

    [Fact]
    public virtual Task Modify_nested_complex_property_in_complex_collection_mapped_to_json()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var company = await context.Companies.OrderBy(c => c.Id).FirstAsync();

                company.Employees![0].Address.City = "Modified City";
                company.Employees[0].Address.PostalCode = "99999";

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                using (SuspendRecordingEvents())
                {
                    var company = await context.Companies.OrderBy(c => c.Id).FirstAsync();
                    var employee = company.Employees![0];
                    Assert.Equal("Modified City", employee.Address.City);
                    Assert.Equal("99999", employee.Address.PostalCode);
                    Assert.Equal("100 First St", employee.Address.Street); // Unchanged
                    Assert.Equal("USA", employee.Address.Country); // Unchanged
                }
            });

    [Fact]
    public virtual Task Set_complex_collection_to_null_mapped_to_json()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var company = await context.Companies.OrderBy(c => c.Id).FirstAsync();

                var companyEntry = context.Entry(company);
                var employeesProperty = companyEntry.ComplexCollection(c => c.Employees);
                Assert.Equal(
                    employeesProperty.CurrentValue,
                    employeesProperty.GetInfrastructure().GetOriginalValue(employeesProperty.Metadata));
                company.Employees = null;

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                using (SuspendRecordingEvents())
                {
                    var company = await context.Companies.OrderBy(c => c.Id).FirstAsync();
                    Assert.Null(company.Employees);
                }
            });

    [Fact]
    public virtual Task Set_null_complex_collection_to_non_empty_mapped_to_json()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var company = await context.Companies.OrderBy(c => c.Id).FirstAsync();
                company.Employees = null;
                await context.SaveChangesAsync();

                company.Employees =
                [
                    new Employee
                    {
                        Name = "New Employee",
                        PhoneNumbers = ["555-1111"],
                        Address = new Address
                        {
                            Street = "123 New St",
                            City = "New City",
                            PostalCode = "12345",
                            Country = "USA"
                        }
                    }
                ];

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                using (SuspendRecordingEvents())
                {
                    var company = await context.Companies.OrderBy(c => c.Id).FirstAsync();
                    Assert.NotNull(company.Employees);
                    Assert.Single(company.Employees);
                    Assert.Equal("New Employee", company.Employees[0].Name);
                }
            });

    [Fact]
    public virtual Task Replace_complex_collection_element_mapped_to_json()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var company = await context.Companies.OrderBy(c => c.Id).FirstAsync();

                company.Employees![0] = new Employee
                {
                    Name = "Replacement Employee",
                    PhoneNumbers = ["555-7777", "555-8888"],
                    Address = new Address
                    {
                        Street = "789 Replace St",
                        City = "Replace City",
                        PostalCode = "54321",
                        Country = "Canada"
                    }
                };

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                using (SuspendRecordingEvents())
                {
                    var company = await context.Companies.OrderBy(c => c.Id).FirstAsync();
                    var employee = company.Employees![0];
                    Assert.Equal("Replacement Employee", employee.Name);
                    Assert.Equal(2, employee.PhoneNumbers.Count);
                    Assert.Equal("555-7777", employee.PhoneNumbers[0]);
                    Assert.Equal("789 Replace St", employee.Address.Street);
                    Assert.Equal("Canada", employee.Address.Country);
                }
            });

    [Fact]
    public virtual Task Complex_collection_with_empty_nested_collections_mapped_to_json()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var company = await context.Companies.OrderBy(c => c.Id).FirstAsync();

                company.Employees!.Add(
                    new Employee
                    {
                        Name = "Employee No Phone",
                        PhoneNumbers = [], // Empty collection
                        Address = new Address
                        {
                            Street = "456 No Phone St",
                            City = "Quiet City",
                            PostalCode = "00000",
                            Country = "USA"
                        }
                    });

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                using (SuspendRecordingEvents())
                {
                    var company = await context.Companies.OrderBy(c => c.Id).FirstAsync();
                    Assert.Equal(2, company.Employees!.Count);
                    var employeeWithoutPhone = company.Employees[1];
                    Assert.Equal("Employee No Phone", employeeWithoutPhone.Name);
                    Assert.Empty(employeeWithoutPhone.PhoneNumbers);
                    Assert.Equal("Quiet City", employeeWithoutPhone.Address.City);
                }
            });

    [Fact]
    public virtual Task Set_complex_property_mapped_to_json_to_null()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var company = await context.Companies.OrderBy(c => c.Id).FirstAsync();
                company.Department = null;

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                using (SuspendRecordingEvents())
                {
                    var company = await context.Companies.OrderBy(c => c.Id).FirstAsync();
                    Assert.Null(company.Department);
                }
            });

    [Fact]
    public virtual Task Set_null_complex_property_to_non_null_mapped_to_json()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var company = await context.Companies.OrderBy(c => c.Id).FirstAsync();
                company.Department = null;
                await context.SaveChangesAsync();

                company.Department = new Department { Name = "New Department", Budget = 25000.00m };

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                using (SuspendRecordingEvents())
                {
                    var company = await context.Companies.OrderBy(c => c.Id).FirstAsync();
                    Assert.NotNull(company.Department);
                    Assert.Equal("New Department", company.Department.Name);
                    Assert.Equal(25000.00m, company.Department.Budget);
                }
            });

    [Fact]
    public virtual Task Replace_complex_property_mapped_to_json()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var company = await context.Companies.OrderBy(c => c.Id).FirstAsync();

                company.Department = new Department { Name = "Replacement Department", Budget = 99999.99m };

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                using (SuspendRecordingEvents())
                {
                    var company = await context.Companies.OrderBy(c => c.Id).FirstAsync();
                    Assert.NotNull(company.Department);
                    Assert.Equal("Replacement Department", company.Department.Name);
                    Assert.Equal(99999.99m, company.Department.Budget);
                }
            });

    [Fact]
    public virtual Task Grow_nested_sub_collection_in_complex_property_mapped_to_json()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var widget = await context.Set<WidgetWithDeepJson>().OrderBy(w => w.Id).FirstAsync();

                // Replace the entire JSON-mapped complex property with a structure where one nested
                // sub-collection (Inner) has grown from one element to two. The sibling sub-collection
                // (Others) being present in the type definition is required to trigger the regression.
                widget.Deep = new DeepData
                {
                    Mid = new MiddleData
                    {
                        Items =
                        [
                            new DeepItem
                            {
                                Title = "Item1",
                                Inner =
                                [
                                    new InnerEntry { Value = "inner-0" },
                                    new InnerEntry { Value = "inner-1" }
                                ],
                                Others = []
                            }
                        ]
                    }
                };

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                using (SuspendRecordingEvents())
                {
                    var widget = await context.Set<WidgetWithDeepJson>().OrderBy(w => w.Id).FirstAsync();
                    var item = Assert.Single(widget.Deep.Mid.Items);
                    Assert.Equal("Item1", item.Title);
                    Assert.Equal(2, item.Inner.Count);
                    Assert.Equal("inner-0", item.Inner[0].Value);
                    Assert.Equal("inner-1", item.Inner[1].Value);
                    Assert.Empty(item.Others);
                }
            });

    [Fact]
    public virtual Task Set_nullable_complex_property_with_nested_collection_to_null()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var entity = await context.Set<EntityWithNullableMeta>().OrderBy(e => e.Id).FirstAsync();

                // Setting Meta to null on an item whose Meta.Entries has 2+ elements used to throw
                // InvalidOperationException from DetectChanges when reindexing the orphaned entries.
                entity.Items[0].Meta = null;

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                using (SuspendRecordingEvents())
                {
                    var entity = await context.Set<EntityWithNullableMeta>().OrderBy(e => e.Id).FirstAsync();
                    Assert.Single(entity.Items);
                    Assert.Null(entity.Items[0].Meta);
                }
            });

    [Fact] // Issue #38625
    public virtual Task Complex_collection_absent_from_json_is_materialized_as_empty()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                // Simulate a row persisted before the Others sub-collection was added to the type,
                // so the key is absent from the stored document.
                await SetStoredDocumentAsync(
                    context, "Widgets", "Deep", id: 1, """{"Mid":{"Items":[{"Title":"Item1","Inner":[{"Value":"inner-0"}]}]}}""");

                // Tracking and no-tracking queries go through separate fixup paths.
                AssertMaterialized(await context.Set<WidgetWithDeepJson>().OrderBy(w => w.Id).FirstAsync());
                AssertMaterialized(await context.Set<WidgetWithDeepJson>().AsNoTracking().OrderBy(w => w.Id).FirstAsync());

                static void AssertMaterialized(WidgetWithDeepJson widget)
                {
                    var item = Assert.Single(widget.Deep.Mid.Items);

                    Assert.Equal("Item1", item.Title);
                    Assert.Equal("inner-0", Assert.Single(item.Inner).Value);

                    // A complex collection that is absent from the JSON document materializes as an empty collection, not null.
                    Assert.NotNull(item.Others);
                    Assert.Empty(item.Others);
                }
            });

    [Fact] // Issue #38625
    public virtual Task Complex_collection_absent_from_json_in_value_type_complex_type_is_materialized_as_empty()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                // A value type complex type takes the inlined fixup path rather than the invoked one
                await SetStoredDocumentAsync(context, "StructWidgets", "Data", id: 1, """{"Label":"S1"}""");

                var widget = await context.Set<StructWidget>().OrderBy(w => w.Id).FirstAsync();
                Assert.Equal("S1", widget.Data.Label);
                Assert.NotNull(widget.Data.Tags);
                Assert.Empty(widget.Data.Tags);

                var untrackedWidget = await context.Set<StructWidget>().AsNoTracking().OrderBy(w => w.Id).FirstAsync();
                Assert.NotNull(untrackedWidget.Data.Tags);
                Assert.Empty(untrackedWidget.Data.Tags);
            });

    [Fact] // Issue #38625
    public virtual Task Complex_collection_explicitly_null_in_json_is_materialized_as_null()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                // Unlike an absent key, an explicit null in the document is preserved.
                await SetStoredDocumentAsync(
                    context, "Widgets", "Deep", id: 1,
                    """{"Mid":{"Items":[{"Title":"Item1","Inner":[{"Value":"inner-0"}],"Others":null}]}}""");

                var widget = await context.Set<WidgetWithDeepJson>().OrderBy(w => w.Id).FirstAsync();
                Assert.Null(Assert.Single(widget.Deep.Mid.Items).Others);

                var untrackedWidget = await context.Set<WidgetWithDeepJson>().AsNoTracking().OrderBy(w => w.Id).FirstAsync();
                Assert.Null(Assert.Single(untrackedWidget.Deep.Mid.Items).Others);
            });

    [Fact] // Issue #38625
    public virtual Task Save_changes_after_loading_row_with_complex_collection_absent_from_json()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                await SetStoredDocumentAsync(
                    context, "Widgets", "Deep", id: 1, """{"Mid":{"Items":[{"Title":"Item1","Inner":[{"Value":"inner-0"}]}]}}""");

                var widget = await context.Set<WidgetWithDeepJson>().OrderBy(w => w.Id).FirstAsync();

                // Modifying an unrelated scalar must not make the row unsaveable.
                widget.Deep.Mid.Items[0].Title = "Item1-updated";

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                using (SuspendRecordingEvents())
                {
                    var widget = await context.Set<WidgetWithDeepJson>().OrderBy(w => w.Id).FirstAsync();
                    var item = Assert.Single(widget.Deep.Mid.Items);
                    Assert.Equal("Item1-updated", item.Title);
                    Assert.Empty(item.Others);
                }
            });

    [Fact] // Issue #38625
    public virtual Task Primitive_collection_absent_from_json_is_materialized_as_empty()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                // Simulate a row persisted before the Codes collection was added to the element type. The second element has the
                // key, so this also covers the read flag of one element not carrying over to the next.
                await SetStoredDocumentAsync(
                    context, "CodeWidgets", "Data", id: 1,
                    """{"Items":[{"Label":"C1"},{"Label":"C2","Codes":[7]},{"Label":"C3"}]}""");

                AssertMaterialized(await context.Set<CodeWidget>().OrderBy(w => w.Id).FirstAsync());
                AssertMaterialized(await context.Set<CodeWidget>().AsNoTracking().OrderBy(w => w.Id).FirstAsync());

                static void AssertMaterialized(CodeWidget widget)
                {
                    Assert.Equal(["C1", "C2", "C3"], widget.Data.Items.Select(i => i.Label));

                    // A primitive collection absent from the JSON document materializes as empty, not null; one that is present
                    // keeps its values.
                    Assert.NotNull(widget.Data.Items[0].Codes);
                    Assert.Empty(widget.Data.Items[0].Codes);
                    Assert.Equal([7], widget.Data.Items[1].Codes);
                    Assert.Empty(widget.Data.Items[2].Codes);

                    // Every collection kind the reader/writer can build comes back empty, whichever way it is created
                    Assert.All(widget.Data.Items, i => Assert.Empty(Assert.IsType<int[]>(i.ArrayCodes)));
                    Assert.All(widget.Data.Items, i => Assert.Empty(Assert.IsType<List<int>>(i.InterfaceCodes)));
                    Assert.All(widget.Data.Items, i => Assert.Empty(i.TextCodes));
                    Assert.All(widget.Data.Items, i => Assert.Empty(Assert.IsType<ReadOnlyCollection<int>>(i.ReadOnlyCodes)));

                    // An optional collection and one with its own reader/writer are left alone, so an absent key stays null
                    Assert.All(widget.Data.Items, i => Assert.Null(i.OptionalCodes));
                    Assert.All(widget.Data.Items, i => Assert.Null(i.CustomCodes));
                }
            });

    [Fact] // Issue #38625
    public virtual Task Primitive_collection_absent_from_json_in_owned_entity_is_materialized_as_empty()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                // JSON-mapped owned entities share the property read loop with complex types, so they are covered too.
                await SetStoredDocumentAsync(context, "OwnedCodeWidgets", "Owned", id: 1, """{"Label":"O1"}""");

                AssertMaterialized(await context.Set<OwnedCodeWidget>().OrderBy(w => w.Id).FirstAsync());
                AssertMaterialized(await context.Set<OwnedCodeWidget>().AsNoTracking().OrderBy(w => w.Id).FirstAsync());

                static void AssertMaterialized(OwnedCodeWidget widget)
                {
                    Assert.Equal("O1", widget.Owned.Label);
                    Assert.NotNull(widget.Owned.Codes);
                    Assert.Empty(widget.Owned.Codes);
                }
            });

    [Fact] // Issue #38625
    public virtual Task Save_changes_after_loading_row_with_primitive_collection_absent_from_json()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                // CustomCodes has its own reader/writer, so it is not synthesized and has to be present for the row to be saveable
                await SetStoredDocumentAsync(
                    context, "CodeWidgets", "Data", id: 1, """{"Items":[{"Label":"C1","CustomCodes":[9]}]}""");

                var widget = await context.Set<CodeWidget>().OrderBy(w => w.Id).FirstAsync();

                // Modifying an unrelated scalar must not make the row unsaveable.
                widget.Data.Items[0].Label = "C1-updated";

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                using (SuspendRecordingEvents())
                {
                    var widget = await context.Set<CodeWidget>().OrderBy(w => w.Id).FirstAsync();
                    var item = Assert.Single(widget.Data.Items);
                    Assert.Equal("C1-updated", item.Label);
                    Assert.Empty(item.Codes);
                }
            });

    [Fact] // Issue #38625
    public virtual Task Saving_null_required_primitive_collection_in_complex_collection_element_throws()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var widget = await context.Set<CodeWidget>().OrderBy(w => w.Id).FirstAsync();
                widget.Data.Items[0].Codes = null!;

                // The element's own entry reports the failure; without that, the containing property is looked up on the wrong
                // entry and the user gets an internal "property belongs to type" message instead.
                Assert.Equal(
                    CoreStrings.NullRequiredPrimitiveCollection(
                        "CodeWidget.Data#CodeData.Items#CodeItem", nameof(CodeItem.Codes)),
                    (await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync())).Message);
            });

    [Fact] // Issue #38625
    public virtual Task Saving_null_required_complex_collection_in_complex_collection_element_throws()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var widget = await context.Set<WidgetWithDeepJson>().OrderBy(w => w.Id).FirstAsync();
                widget.Deep.Mid.Items[0].Others = null!;

                Assert.Equal(
                    CoreStrings.NullRequiredComplexProperty(nameof(DeepItem), nameof(DeepItem.Others)),
                    (await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync())).Message);
            });

    private static async Task SetStoredDocumentAsync(DbContext context, string table, string column, int id, string json)
    {
        // Identifiers are delimited by the provider so that any provider can run this test; the document is inlined rather than
        // parameterized so that providers with a dedicated JSON store type need no cast. Braces are escaped for string.Format.
        var sqlGenerationHelper = context.GetService<ISqlGenerationHelper>();

        var literal = json.Replace("'", "''").Replace("{", "{{").Replace("}", "}}");

        var rowsAffected = await context.Database.ExecuteSqlRawAsync(
            $"UPDATE {Q(table)} SET {Q(column)} = '{literal}' WHERE {Q("Id")} = {id}");

        // Otherwise a mismatched table or column name would leave the seeded document in place and make the test vacuous.
        Assert.Equal(1, rowsAffected);

        string Q(string name)
            => sqlGenerationHelper.DelimitIdentifier(name);
    }

    protected virtual void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    protected virtual void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();

    protected virtual void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected virtual IDisposable SuspendRecordingEvents()
        => Fixture.TestSqlLoggerFactory.SuspendRecordingEvents();

    protected class ComplexCollectionJsonContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<CompanyWithComplexCollections> Companies { get; set; } = null!;
        public DbSet<WidgetWithDeepJson> Widgets { get; set; } = null!;
        public DbSet<StructWidget> StructWidgets { get; set; } = null!;
        public DbSet<CodeWidget> CodeWidgets { get; set; } = null!;
        public DbSet<OwnedCodeWidget> OwnedCodeWidgets { get; set; } = null!;
        public DbSet<EntityWithNullableMeta> EntitiesWithNullableMeta { get; set; } = null!;
    }

    protected class CompanyWithComplexCollections
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public List<Contact>? Contacts { get; set; }
        public List<Employee>? Employees { get; set; }
        public Department? Department { get; set; }
    }

    protected class Contact
    {
        public required string Name { get; set; }
        public List<string> PhoneNumbers { get; set; } = [];
    }

    protected class Employee
    {
        public required string Name { get; set; }
        public List<string> PhoneNumbers { get; set; } = [];
        public required Address Address { get; set; }
    }

    protected class Address
    {
        public required string Street { get; set; }
        public required string City { get; set; }
        public required string PostalCode { get; set; }
        public required string Country { get; set; }
    }

    protected class Department
    {
        public required string Name { get; set; }
        public decimal Budget { get; set; }
    }

    protected class WidgetWithDeepJson
    {
        public int Id { get; set; }
        public required DeepData Deep { get; set; }
    }

    protected class DeepData
    {
        public required MiddleData Mid { get; set; }
    }

    protected class MiddleData
    {
        public List<DeepItem> Items { get; set; } = [];
    }

    protected class DeepItem
    {
        public required string Title { get; set; }
        public List<InnerEntry> Inner { get; set; } = [];
        public List<InnerEntry> Others { get; set; } = [];
    }

    protected class InnerEntry
    {
        public required string Value { get; set; }
    }

    protected class CodeWidget
    {
        public int Id { get; set; }
        public required CodeData Data { get; set; }
    }

    protected class OwnedCodeWidget
    {
        public int Id { get; set; }
        public required OwnedCodes Owned { get; set; }
    }

    protected class OwnedCodes
    {
        public required string Label { get; set; }
        public List<int> Codes { get; set; } = [];
    }

    protected class CodeData
    {
        public List<CodeItem> Items { get; set; } = [];
    }

    // One property per way the collection to create is decided: a concrete type with a parameterless constructor, an array, a
    // type reachable only through the List<T> fallback, one the reader/writer builds from a list, and the two kinds that are
    // deliberately left alone.
    protected class CodeItem
    {
        public required string Label { get; set; }
        public List<int> Codes { get; set; } = [];

        public int[] ArrayCodes { get; set; } = [];

        public IList<int> InterfaceCodes { get; set; } = [];

        public List<string> TextCodes { get; set; } = [];

        // Read-only collections are created by the JSON reader/writer from a list rather than a parameterless constructor
        public ReadOnlyCollection<int> ReadOnlyCodes { get; set; } = new([]);

        // Optional, so an absent key legitimately means null and nothing is synthesized
        public List<int>? OptionalCodes { get; set; }

        // Configured with its own reader/writer below, so nothing is synthesized for it when the key is absent
        public List<int> CustomCodes { get; set; } = [];
    }

    protected class CodeListReaderWriter : JsonValueReaderWriter<List<int>>
    {
        private static readonly ConstructorInfo Constructor = typeof(CodeListReaderWriter).GetConstructor([])!;

        [Experimental(EFDiagnostics.PrecompiledQueryExperimental)]
        public override Expression ConstructorExpression
            => Expression.New(Constructor);

        public override List<int> FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
        {
            var values = new List<int>();
            while (manager.CurrentReader.TokenType != JsonTokenType.EndArray)
            {
                manager.MoveNext();
                if (manager.CurrentReader.TokenType == JsonTokenType.Number)
                {
                    values.Add(manager.CurrentReader.GetInt32());
                }
            }

            return values;
        }

        public override void ToJsonTyped(Utf8JsonWriter writer, List<int> value)
        {
            writer.WriteStartArray();
            foreach (var item in value)
            {
                writer.WriteNumberValue(item);
            }

            writer.WriteEndArray();
        }
    }

    protected class StructWidget
    {
        public int Id { get; set; }
        public StructData Data { get; set; }
    }

    protected struct StructData
    {
        public StructData()
        {
        }

        public required string Label { get; set; }
        public List<InnerEntry> Tags { get; set; } = [];
    }

    protected class EntityWithNullableMeta
    {
        public int Id { get; set; }
        public List<ItemWithMeta> Items { get; set; } = [];
    }

    protected class ItemWithMeta
    {
        public required string Name { get; set; }
        public OptionalMeta? Meta { get; set; }
    }

    protected class OptionalMeta
    {
        public List<MetaEntry> Entries { get; set; } = [];
    }

    protected class MetaEntry
    {
        public required string Value { get; set; }
    }

    public abstract class ComplexCollectionJsonUpdateFixtureBase : SharedStoreFixtureBase<DbContext>
    {
        protected override string StoreName
            => "ComplexCollectionJsonUpdateTest";

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override bool ShouldLogCategory(string logCategory)
            => logCategory == DbLoggerCategory.Update.Name;

        protected override Type ContextType
            => typeof(ComplexCollectionJsonContext);

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.Entity<CompanyWithComplexCollections>(b =>
            {
                b.Property(x => x.Id).ValueGeneratedNever();

                b.ComplexCollection(
                    x => x.Contacts, cb =>
                    {
                        cb.ToJson();
                        cb.PrimitiveCollection(c => c.PhoneNumbers);
                    });

                b.ComplexCollection(
                    x => x.Employees, cb =>
                    {
                        cb.ToJson();
                        cb.PrimitiveCollection(e => e.PhoneNumbers);
                        cb.ComplexProperty(e => e.Address);
                    });

                b.ComplexProperty(
                    x => x.Department, cb => cb.ToJson());
            });

            modelBuilder.Entity<WidgetWithDeepJson>(b =>
            {
                b.Property(x => x.Id).ValueGeneratedNever();

                b.ComplexProperty(
                    x => x.Deep, db =>
                    {
                        db.ToJson();
                        db.ComplexProperty(
                            d => d.Mid, mb =>
                                mb.ComplexCollection(
                                    m => m.Items, ib =>
                                    {
                                        ib.ComplexCollection(i => i.Inner);
                                        ib.ComplexCollection(i => i.Others);
                                    }));
                    });
            });

            modelBuilder.Entity<StructWidget>(b =>
            {
                b.Property(x => x.Id).ValueGeneratedNever();

                b.ComplexProperty(
                    x => x.Data, db =>
                    {
                        db.ToJson();
                        db.ComplexCollection(d => d.Tags);
                    });
            });

            modelBuilder.Entity<CodeWidget>(b =>
            {
                b.Property(x => x.Id).ValueGeneratedNever();

                b.ComplexProperty(
                    x => x.Data, db =>
                    {
                        db.ToJson();
                        db.ComplexCollection(
                            d => d.Items,
                            ib => ib.PrimitiveCollection(i => i.CustomCodes).Metadata
                                .SetJsonValueReaderWriterType(typeof(CodeListReaderWriter)));
                    });
            });

            modelBuilder.Entity<OwnedCodeWidget>(b =>
            {
                b.Property(x => x.Id).ValueGeneratedNever();

                b.OwnsOne(x => x.Owned, ob => ob.ToJson());
                b.Navigation(x => x.Owned).IsRequired();
            });

            modelBuilder.Entity<EntityWithNullableMeta>(b =>
            {
                b.Property(x => x.Id).ValueGeneratedNever();

                b.ComplexCollection(
                    x => x.Items, ib =>
                    {
                        ib.ToJson();
                        ib.ComplexProperty(
                            x => x.Meta, mb =>
                                mb.ComplexCollection(m => m.Entries));
                    });
            });
        }

        protected override Task SeedAsync(DbContext context)
        {
            var company = new CompanyWithComplexCollections
            {
                Id = 1,
                Name = "Test Company",
                Contacts =
                [
                    new Contact { Name = "First Contact", PhoneNumbers = ["555-1234", "555-5678"] },
                    new Contact { Name = "Second Contact", PhoneNumbers = ["555-9876", "555-5432"] }
                ],
                Employees =
                [
                    new Employee
                    {
                        Name = "Initial Employee",
                        PhoneNumbers = ["555-0001"],
                        Address = new Address
                        {
                            Street = "100 First St",
                            City = "Initial City",
                            PostalCode = "00001",
                            Country = "USA"
                        }
                    }
                ],
                Department = new Department { Name = "Initial Department", Budget = 10000.00m }
            };

            context.Add(company);

            var widget = new WidgetWithDeepJson
            {
                Id = 1,
                Deep = new DeepData
                {
                    Mid = new MiddleData
                    {
                        Items =
                        [
                            new DeepItem
                            {
                                Title = "Item1",
                                Inner = [new InnerEntry { Value = "inner-0" }],
                                Others = []
                            }
                        ]
                    }
                }
            };

            context.Add(widget);

            context.Add(new StructWidget { Id = 1, Data = new StructData { Label = "S1", Tags = [new InnerEntry { Value = "t-0" }] } });

            context.Add(
                new CodeWidget
                {
                    Id = 1,
                    Data = new CodeData
                    {
                        Items =
                        [
                            new CodeItem
                            {
                                Label = "C1",
                                Codes = [1, 2],
                                ArrayCodes = [4],
                                InterfaceCodes = [5],
                                TextCodes = ["six"],
                                ReadOnlyCodes = new([3]),
                                OptionalCodes = [7],
                                CustomCodes = [9]
                            }
                        ]
                    }
                });

            context.Add(new OwnedCodeWidget { Id = 1, Owned = new OwnedCodes { Label = "O1", Codes = [4] } });

            var entityWithNullableMeta = new EntityWithNullableMeta
            {
                Id = 1,
                Items =
                [
                    new ItemWithMeta
                    {
                        Name = "Item1",
                        Meta = new OptionalMeta
                        {
                            Entries =
                            [
                                new MetaEntry { Value = "entry-0" },
                                new MetaEntry { Value = "entry-1" }
                            ]
                        }
                    }
                ]
            };

            context.Add(entityWithNullableMeta);
            return context.SaveChangesAsync();
        }
    }
}
