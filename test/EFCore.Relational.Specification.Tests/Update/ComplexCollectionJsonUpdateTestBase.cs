// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Update;

public abstract class ComplexCollectionJsonUpdateTestBase<TFixture>(TFixture fixture) : IClassFixture<TFixture>
    where TFixture : ComplexCollectionJsonUpdateTestBase<TFixture>.ComplexCollectionJsonUpdateFixtureBase, new()
{
    public TFixture Fixture { get; } = fixture;

    protected ComplexCollectionJsonContext CreateContext()
        => (ComplexCollectionJsonContext)Fixture.CreateContext();

    [ConditionalFact]
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

    [ConditionalFact]
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

    [ConditionalFact]
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

    [ConditionalFact]
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

    [ConditionalFact]
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

    [ConditionalFact]
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

    [ConditionalFact]
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

    [ConditionalFact]
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

    [ConditionalFact]
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

    [ConditionalFact]
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

    [ConditionalFact]
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

    [ConditionalFact]
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

    [ConditionalFact]
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

    [ConditionalFact]
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

    [ConditionalFact]
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

    [ConditionalFact]
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

    [ConditionalFact]
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

    [ConditionalFact]
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
            => modelBuilder.Entity<CompanyWithComplexCollections>(b =>
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
                    x => x.Department, cb =>
                    {
                        cb.ToJson();
                    });
            });

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
            return context.SaveChangesAsync();
        }
    }
}
