// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class MigrationsTestBase2<TFixture> : IClassFixture<TFixture>
        where TFixture : MigrationsTestBase2<TFixture>.MigrationsFixtureBase2, new()
    {
        protected TFixture Fixture { get; }

        protected MigrationsTestBase2(TFixture fixture)
        {
            Fixture = fixture;
        }

        [ConditionalFact]
        public virtual Task CreateIndexOperation_with_filter_where_clause()
            => ExecuteIncremental(
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<string>("Name");
                    }),
                builder => builder.Entity("People").HasIndex("Name").HasFilter("[Name] IS NOT NULL"), // TODO: Quotes for other databases
                model => Assert.Equal("([Name] IS NOT NULL)", model.Tables.Single().Indexes.Single().Filter));  // TODO: Why parentheses?

        [ConditionalFact]
        public virtual Task CreateIndexOperation_with_filter_where_clause_and_is_unique()
            => ExecuteIncremental(
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<string>("Name");
                    }),
                builder => builder.Entity("People").HasIndex("Name").IsUnique()
                    .HasFilter("[Name] IS NOT NULL AND [Name] <> ''"), // TODO: Quotes
                model => Assert.Equal("([Name] IS NOT NULL AND [Name]<>'')", model.Tables.Single().Indexes.Single().Filter));  // TODO: Whitespace is gonna be difficult, provider-specific...

        [ConditionalFact]
        public virtual Task AddColumnOperation_with_defaultValue()
            => ExecuteIncremental(
                builder => builder.Entity("People").Property<int>("Id"),
                builder => builder.Entity("People").Property<string>("Name")
                    //                  .HasColumnType("varchar(30)")
                    .IsRequired()
                    .HasDefaultValue("John Doe"),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    Assert.Equal(2, table.Columns.Count);
                    var nameColumn = Assert.Single(table.Columns, c => c.Name == "Name");
//                    Assert.Equal("varchar(30)", nameColumn.StoreType);
                    Assert.False(nameColumn.IsNullable);
                    Assert.Equal("(N'John Doe')", nameColumn.DefaultValueSql); // TODO: No
                });

        [ConditionalFact]
        public virtual Task AddColumnOperation_with_defaultValueSql()
            => ExecuteIncremental(
                builder => builder.Entity("People").Property<int>("Id"),
                builder => builder.Entity("People").Property<DateTime?>("Birthday")
                    .HasColumnType("date")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP"),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    Assert.Equal(2, table.Columns.Count);
                    var nameColumn = Assert.Single(table.Columns, c => c.Name == "Birthday");
                    Assert.Equal("date", nameColumn.StoreType);
                    Assert.True(nameColumn.IsNullable);
                    Assert.Equal("(getdate())", nameColumn.DefaultValueSql);
                });

        [ConditionalFact]
        public virtual Task AddColumnOperation_without_column_type()
            => ExecuteIncremental(
                builder => builder.Entity("People").Property<int>("Id"),
                builder => builder.Entity("People").Property<string>("Name").IsRequired(),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var column = Assert.Single(table.Columns, c => c.Name == "Name");
                    Assert.Equal("nvarchar(max)", column.StoreType);
                    Assert.False(column.IsNullable);
                });

        [ConditionalFact]
        public virtual Task AddColumnOperation_with_ansi()
            => ExecuteIncremental(
                builder => builder.Entity("People").Property<int>("Id"),
                builder => builder.Entity("People").Property<string>("Name").IsUnicode(false),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var column = Assert.Single(table.Columns, c => c.Name == "Name");
                    Assert.Equal("varchar(max)", column.StoreType);
                    Assert.True(column.IsNullable);
                });

        // TODO: AddColumnOperation_with_unicode_overridden. In which scenarios do we need to do this?

        // TODO: AddColumnOperation_with_unicode_no_model. In which scenarios do we need to do this?

        [ConditionalFact]
        public virtual Task AddColumnOperation_with_fixed_length()
            => ExecuteIncremental(
                builder => builder.Entity("People").Property<int>("Id"),
                builder => builder.Entity("People").Property<string>("Name").IsFixedLength(),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var column = Assert.Single(table.Columns, c => c.Name == "Name");
                    Assert.Equal("nvarchar(max)", column.StoreType);
                });

        // TODO: AddColumnOperation_with_fixed_length_no_model

        [ConditionalFact]
        public virtual Task AddColumnOperation_with_maxLength()
            => ExecuteIncremental(
                builder => builder.Entity("People").Property<int>("Id"),
                builder => builder.Entity("People").Property<string>("Name").HasMaxLength(30),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var column = Assert.Single(table.Columns, c => c.Name == "Name");
                    Assert.Equal("nvarchar(30)", column.StoreType);
                });

        // TODO: AddColumnOperation_with_maxLength_overridden

        // TODO: AddColumnOperation_with_maxLength_no_model

        [ConditionalFact]
        public virtual Task AddColumnOperation_with_maxLength_on_derived()
            => ExecuteIncremental(
                builder =>
                {
                    builder.Entity("Person");
                    builder.Entity(
                        "SpecialPerson", e =>
                        {
                            e.HasBaseType("Person");
                            e.Property<string>("Name").HasMaxLength(30);
                        });

                    builder.Entity("MoreSpecialPerson").HasBaseType("SpecialPerson");
                },
                builder => builder.Entity("Person").Property<string>("Name").HasMaxLength(30),
                model =>
                {
                    var table = Assert.Single(model.Tables, t => t.Name == "Person");
                    var column = Assert.Single(table.Columns, c => c.Name == "Name");
                    Assert.Equal("nvarchar(30)", column.StoreType);
                });

        [ConditionalFact]
        public virtual Task AddColumnOperation_with_shared_column()
            => ExecuteIncremental(
                builder =>
                {
                    builder.Entity("Base").Property<int>("Id");
                    builder.Entity("Derived1").Property<string>("Foo");
                    builder.Entity("Derived2").Property<string>("Foo");
                },
                builder => builder.Entity("Base").Property<string>("Foo"),
                model =>
                {
                    // var table = Assert.Single(model.Tables);
                    // var column = Assert.Single(table.Columns, c => c.Name == "Name");
                    // Assert.Equal("nvarchar(30)", column.StoreType);
                });

        [ConditionalFact]
        public virtual Task AddForeignKeyOperation()
            => ExecuteIncremental(
                builder =>
                {
                    builder.Entity("Customers").Property<int>("Id");
                    builder.Entity(
                        "Orders", e =>
                        {
                            e.Property<int>("Id");
                            e.Property<int>("CustomerId");
                        });
                },
                builder => builder.Entity("Orders").HasOne("Customers").WithMany().HasForeignKey("CustomerId"),
                model =>
                {
                    var customersTable = Assert.Single(model.Tables, t => t.Name == "Customers");
                    var ordersTable = Assert.Single(model.Tables, t => t.Name == "Orders");
                    var foreignKey = ordersTable.ForeignKeys.Single();
                    Assert.Equal("FK_Orders_Customers_CustomerId", foreignKey.Name);
                    Assert.Equal(ReferentialAction.Cascade, foreignKey.OnDelete);
                    Assert.Same(customersTable, foreignKey.PrincipalTable);
                    Assert.Same(customersTable.Columns.Single(), Assert.Single(foreignKey.PrincipalColumns));
                    Assert.Equal("CustomerId", Assert.Single(foreignKey.Columns).Name);
                });

        [ConditionalFact]
        public virtual Task AddForeignKeyOperation_with_name()
            => ExecuteIncremental(
                builder =>
                {
                    builder.Entity("Customers").Property<int>("Id");
                    builder.Entity(
                        "Orders", e =>
                        {
                            e.Property<int>("Id");
                            e.Property<int>("CustomerId");
                        });
                },
                builder => builder.Entity("Orders").HasOne("Customers").WithMany().HasForeignKey("CustomerId").HasConstraintName("FK_Foo"),
                model =>
                {
                    var table = Assert.Single(model.Tables, t => t.Name == "Orders");
                    var foreignKey = table.ForeignKeys.Single();
                    Assert.Equal("FK_Foo", foreignKey.Name);
                });

        // TODO: AddForeignKeyOperation_without_principal_columns, how to generate the scenario via model diffing

        [ConditionalFact]
        public virtual Task AddPrimaryKeyOperation()
            => ExecuteIncremental(
                builder => builder.Entity("People").Property<string>("SomeField"),
                builder => builder.Entity("People").HasKey("SomeField"),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var primaryKey = table.PrimaryKey;
                    Assert.Same(table, primaryKey.Table);
                    Assert.Same(table.Columns.Single(), Assert.Single(primaryKey.Columns));
                    Assert.Equal("PK_People", primaryKey.Name);
                });

        [ConditionalFact]
        public virtual Task AddPrimaryKeyOperation_composite_with_name()
            => ExecuteIncremental(
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<string>("SomeField1");
                        e.Property<string>("SomeField2");
                    }),
                builder => builder.Entity("People").HasKey("SomeField1", "SomeField2").HasName("PK_Foo"),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var primaryKey = table.PrimaryKey;
                    Assert.Same(table, primaryKey.Table);
                    Assert.Collection(
                        primaryKey.Columns,
                        c => Assert.Same(table.Columns[0], c),
                        c => Assert.Same(table.Columns[1], c));
                    Assert.Equal("PK_Foo", primaryKey.Name);
                });

        [ConditionalFact]
        public virtual Task AddUniqueConstraintOperation()
            => ExecuteIncremental(
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<int>("AlternateKeyColumn");
                    }),
                builder => builder.Entity("People").HasAlternateKey("AlternateKeyColumn"),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var uniqueConstraint = table.UniqueConstraints.Single();
                    Assert.Same(table, uniqueConstraint.Table);
                    Assert.Same(table.Columns.Single(c => c.Name == "AlternateKeyColumn"), Assert.Single(uniqueConstraint.Columns));
                    Assert.Equal("AK_People_AlternateKeyColumn", uniqueConstraint.Name);
                });

        [ConditionalFact]
        public virtual Task AddUniqueConstraintOperation_composite_with_name()
            => ExecuteIncremental(
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<int>("AlternateKeyColumn1");
                        e.Property<int>("AlternateKeyColumn2");
                    }),
                builder => builder.Entity("People").HasAlternateKey("AlternateKeyColumn1", "AlternateKeyColumn2").HasName("AK_Foo"),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var uniqueConstraint = table.UniqueConstraints.Single();
                    Assert.Same(table, uniqueConstraint.Table);
                    Assert.Collection(
                        uniqueConstraint.Columns,
                        c => Assert.Same(table.Columns.Single(c => c.Name == "AlternateKeyColumn1"), c),
                        c => Assert.Same(table.Columns.Single(c => c.Name == "AlternateKeyColumn2"), c));
                    Assert.Equal("AK_Foo", uniqueConstraint.Name);
                });

        [ConditionalFact]
        public virtual Task CreateCheckConstraintOperation_with_name()
            => ExecuteIncremental(
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<int>("DriverLicense");
                    }),
                builder => builder.Entity("People").HasCheckConstraint("CK_Foo", "[DriverLicense] > 0"), // TODO: Quote
                model =>
                {
                    // TODO: no scaffolding support for check constraints, https://github.com/aspnet/EntityFrameworkCore/issues/15408
                });

        [ConditionalFact]
        public virtual Task AlterColumnOperation_name()
            => Execute(
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<int>("SomeColumn");
                    }),
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<int>("somecolumn");
                    }),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var column = Assert.Single(table.Columns, c => c.Name != "Id");
                    Assert.Equal("somecolumn", column.Name);
                });

        [ConditionalFact]
        public virtual Task AlterColumnOperation_type()
            => Execute(
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<int>("SomeColumn");
                    }),
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<long>("SomeColumn");
                    }),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var column = Assert.Single(table.Columns, c => c.Name != "Id");
                    Assert.Equal("bigint", column.StoreType); // TODO: store type name
                });

        [ConditionalFact]
        public virtual Task AlterColumnOperation_required()
            => Execute(
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<string>("SomeColumn").IsRequired(false);
                    }),
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<string>("SomeColumn").IsRequired(true);
                    }),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var column = Assert.Single(table.Columns, c => c.Name != "Id");
                    Assert.False(column.IsNullable);
                });

        // TODO: More AlterColumn scenarios

        [ConditionalFact]
        public virtual Task AlterSequenceOperation_all_settings()
            => Execute(
                builder => builder.HasSequence<int>("foo"),
                builder => builder.HasSequence<int>("foo")
                    .StartsAt(-3)
                    .IncrementsBy(2)
                    .HasMin(-5)
                    .HasMax(10)
                    .IsCyclic(),
                model =>
                {
                    var sequence = Assert.Single(model.Sequences);
                    Assert.Equal(-3, sequence.StartValue);
                    Assert.Equal(2, sequence.IncrementBy);
                    Assert.Equal(-5, sequence.MinValue);
                    Assert.Equal(10, sequence.MaxValue);
                    Assert.True(sequence.IsCyclic);
                });

        [ConditionalFact]
        public virtual Task AlterSequenceOperation_increment_by()
            => Execute(
                builder => builder.HasSequence<int>("foo"),
                builder => builder.HasSequence<int>("foo").IncrementsBy(2),
                model =>
                {
                    var sequence = Assert.Single(model.Sequences);
                    Assert.Equal(2, sequence.IncrementBy);
                });

        [ConditionalFact]
        public virtual Task RenameTableOperation()
            => Execute(
                builder => builder.Entity("People").Property<int>("Id"),
                builder => builder.Entity("people").Property<int>("Id"),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    Assert.Equal("people", table.Name);
                });

        [ConditionalFact]
        public virtual Task RenameTableOperation_schema()
            => Execute(
                builder => builder.Entity("People").ToTable("People", "dbo").Property<int>("Id"),
                builder => builder.Entity("People").ToTable("People", "dbo2").Property<int>("Id"),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    Assert.Equal("dbo2", table.Schema);
                    Assert.Equal("People", table.Name);
                });

        [ConditionalFact]
        public virtual Task CreateIndexOperation()
            => ExecuteIncremental(
                builder => builder.Entity(
                    "People", entityBuilder =>
                    {
                        entityBuilder.Property<int>("Id");
                        entityBuilder.Property<string>("FirstName");
                    }),
                builder => builder.Entity("People").HasIndex("FirstName"),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var index = Assert.Single(table.Indexes);
                    Assert.Same(table, index.Table);
                    Assert.Same(table.Columns.Single(c => c.Name == "FirstName"), Assert.Single(index.Columns));
                    Assert.Equal("IX_People_FirstName", index.Name);
                    Assert.False(index.IsUnique);
                    Assert.Null(index.Filter);
                });

        [ConditionalFact]
        public virtual Task CreateIndexOperation_unique()
            => ExecuteIncremental(
                builder => builder.Entity(
                    "People", entityBuilder =>
                    {
                        entityBuilder.Property<int>("Id");
                        entityBuilder.Property<string>("FirstName");
                        entityBuilder.Property<string>("LastName");
                    }),
                builder => builder.Entity("People").HasIndex("FirstName", "LastName").IsUnique(),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var index = Assert.Single(table.Indexes);
                    Assert.True(index.IsUnique);
                });

        [ConditionalFact]
        public virtual Task CreateIndexOperation_with_where_clauses()
            => ExecuteIncremental(
                builder => builder.Entity(
                    "People", entityBuilder =>
                    {
                        entityBuilder.Property<int>("Id");
                        entityBuilder.Property<int>("Age");
                    }),
                builder => builder.Entity("People").HasIndex("Age").HasFilter("[Age] > 18"), // TODO: Quote
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var index = Assert.Single(table.Indexes);
                    Assert.Same(table.Columns.Single(c => c.Name == "Age"), Assert.Single(index.Columns));
                    Assert.Equal("([Age]>(18))", index.Filter); // TODO: Assert non-null?
                });

        [ConditionalFact]
        public virtual Task CreateSequenceOperation()
            => Execute(
                builder => { },
                builder => builder.HasSequence<int>("TestSequence"),
                model =>
                {
                    var sequence = Assert.Single(model.Sequences);
                    Assert.Equal("TestSequence", sequence.Name);
                });

        [ConditionalFact]
        public virtual Task CreateSequenceOperation_all_settings()
            => Execute(
                builder => { },
                builder => builder.HasSequence<long>("TestSequence", "dbo2")
                    .StartsAt(3)
                    .IncrementsBy(2)
                    .HasMin(2)
                    .HasMax(916)
                    .IsCyclic(),
                model =>
                {
                    var sequence = Assert.Single(model.Sequences);
                    Assert.Equal("TestSequence", sequence.Name);
                    Assert.Equal("dbo2", sequence.Schema);
                    Assert.Equal(3, sequence.StartValue);
                    Assert.Equal(2, sequence.IncrementBy);
                    Assert.Equal(2, sequence.MinValue);
                    Assert.Equal(916, sequence.MaxValue);
                    Assert.True(sequence.IsCyclic);
                });

        [ConditionalFact]
        public virtual Task CreateTableOperation_all_settings()
            => ExecuteIncremental(
                builder => builder.Entity("Employers").Property<int>("Id"),
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.ToTable("People", "dbo2");

                        e.Property<int>("CustomId");
                        e.Property<int>("EmployerId")
                            .HasComment("Employer ID comment");
                        e.Property<string>("SSN")
                            .HasColumnType("char(11)") // TODO: Provider-specific type
                            .IsRequired(false);

                        e.HasKey("CustomId");
                        e.HasAlternateKey("SSN");
                        e.HasCheckConstraint("CK_SSN", "[SSN] > 0"); // TODO: Quote
                        e.HasOne("Employers").WithMany("People").HasForeignKey("EmployerId");
                    }),
                model =>
                {
                    var employersTable = Assert.Single(model.Tables, t => t.Name == "Employers");
                    var peopleTable = Assert.Single(model.Tables, t => t.Name == "People");

                    Assert.Equal("People", peopleTable.Name);
                    Assert.Equal("dbo2", peopleTable.Schema);

                    Assert.Collection(
                        peopleTable.Columns,
                        c =>
                        {
                            Assert.Equal("CustomId", c.Name);
                            Assert.False(c.IsNullable);
                            Assert.Equal("int", c.StoreType); // TODO: Provider-specific type
                            Assert.Null(c.Comment);
                        },
                        c =>
                        {
                            Assert.Equal("EmployerId", c.Name);
                            Assert.False(c.IsNullable);
                            Assert.Equal("int", c.StoreType); // TODO: Provider-specific type
                            Assert.Equal("Employer ID comment", c.Comment);
                        },
                        c =>
                        {
                            Assert.Equal("SSN", c.Name);
                            //Assert.True(c.IsNullable); // TODO: This fails!
                            Assert.Equal("char(11)", c.StoreType); // TODO: Provider-specific type
                            Assert.Null(c.Comment);
                        });

                    Assert.Same(
                        peopleTable.Columns.Single(c => c.Name == "CustomId"),
                        Assert.Single(peopleTable.PrimaryKey.Columns));
                    Assert.Same(
                        peopleTable.Columns.Single(c => c.Name == "SSN"),
                        Assert.Single(Assert.Single(peopleTable.UniqueConstraints).Columns));
                    // TODO: Need to scaffold check constraints, https://github.com/aspnet/EntityFrameworkCore/issues/15408

                    var foreignKey = Assert.Single(peopleTable.ForeignKeys);
                    Assert.Same(peopleTable, foreignKey.Table);
                    Assert.Same(peopleTable.Columns.Single(c => c.Name == "EmployerId"), Assert.Single(foreignKey.Columns));
                    Assert.Same(employersTable, foreignKey.PrincipalTable);
                    Assert.Same(employersTable.Columns.Single(), Assert.Single(foreignKey.PrincipalColumns));
                });

        [ConditionalFact]
        public virtual Task CreateTableOperation_no_key()
            => Execute(
                builder => { },
                builder => builder.Entity("Anonymous").Property<int>("SomeColumn"),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    Assert.Null(table.PrimaryKey);
                });

        [ConditionalFact]
        public virtual Task DropColumnOperation()
            => Execute(
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<int>("SomeColumn");
                    }),
                builder => builder.Entity("People").Property<int>("Id"),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    Assert.Equal("Id", Assert.Single(table.Columns).Name);
                });

        [ConditionalFact]
        public virtual Task DropForeignKeyOperation()
            => Execute(
                builder =>
                {
                    builder.Entity("Customers").Property<int>("Id");
                    builder.Entity(
                        "Orders", e =>
                        {
                            e.Property<int>("Id");
                            e.Property<int>("CustomerId");
                            e.HasOne("Customers").WithMany().HasForeignKey("CustomerId");
                        });
                },
                builder =>
                {
                    builder.Entity("Customers").Property<int>("Id");
                    builder.Entity(
                        "Orders", e =>
                        {
                            e.Property<int>("Id");
                            e.Property<int>("CustomerId");
                        });
                },
                model =>
                {
                    var customersTable = Assert.Single(model.Tables, t => t.Name == "Customers");
                    Assert.Empty(customersTable.ForeignKeys);
                });

        [ConditionalFact]
        public virtual Task DropIndexOperation()
            => Execute(
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<int>("SomeField");
                        e.HasIndex("SomeField");
                    }),
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<int>("SomeField");
                    }),
                model => Assert.Empty(Assert.Single(model.Tables).Indexes));

        [ConditionalFact]
        public virtual Task DropPrimaryKeyOperation()
            => Execute(
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<int>("SomeField");
                    }),
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("SomeField");
                    }),
                model => Assert.Null(Assert.Single(model.Tables).PrimaryKey));

        [ConditionalFact]
        public virtual Task DropSequenceOperation()
            => Execute(
                builder => builder.HasSequence("TestSequence"),
                builder => { },
                model => Assert.Empty(model.Sequences));

        [ConditionalFact]
        public virtual Task DropTableOperation()
            => Execute(
                builder => builder.Entity("People", e => e.Property<int>("Id")),
                builder => { },
                model => Assert.Empty(model.Tables));

        [ConditionalFact]
        public virtual Task DropUniqueConstraintOperation()
            => Execute(
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<int>("AlternateKeyColumn");
                        e.HasAlternateKey("AlternateKeyColumn");
                    }),
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<int>("AlternateKeyColumn");
                    }),
                model =>
                {
                    Assert.Empty(Assert.Single(model.Tables).UniqueConstraints);
                });

        [ConditionalFact]
        public virtual Task DropCheckConstraintOperation()
            => Execute(
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<int>("DriverLicense");
                        e.HasCheckConstraint("CK_Foo", "[DriverLicense] > 0"); // TODO: Quote
                    }),
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<int>("DriverLicense");
                    }),
                model =>
                {
                    // TODO: no scaffolding support for check constraints, https://github.com/aspnet/EntityFrameworkCore/issues/15408
                });

        // TODO: SqlOperation

        // TODO: Data tests

        private class Person
        {
            public int Id { get; set; }
            public string FullName { get; set; }
        }

        protected virtual Task ExecuteIncremental(
            Action<ModelBuilder> buildSourceAction,
            Action<ModelBuilder> buildTargetIncrementalAction,
            Action<DatabaseModel> modelAsserter)
            => Execute(
                buildSourceAction,
                modelBuilder =>
                {
                    buildSourceAction(modelBuilder);
                    buildTargetIncrementalAction(modelBuilder);
                },
                modelAsserter);

        protected virtual async Task Execute(
            Action<ModelBuilder> buildSourceAction,
            Action<ModelBuilder> buildTargetAction,
            Action<DatabaseModel> modelAsserter)
        {
            var context = Fixture.CreateContext();
            var serviceProvider = ((IInfrastructure<IServiceProvider>)context).Instance;
            var migrationsSqlGenerator = serviceProvider.GetRequiredService<IMigrationsSqlGenerator>();
            var modelDiffer = serviceProvider.GetRequiredService<IMigrationsModelDiffer>();
            var migrationsCommandExecutor = serviceProvider.GetRequiredService<IMigrationCommandExecutor>();
            var connection = serviceProvider.GetRequiredService<IRelationalConnection>();
            var databaseModelFactory = serviceProvider.GetRequiredService<IDatabaseModelFactory>();

            // Build the source and target models. Add current/latest product version if one wasn't set.
            var sourceModelBuilder = Fixture.TestHelpers.CreateConventionBuilder(skipValidation: true);
            buildSourceAction(sourceModelBuilder);
            if (sourceModelBuilder.Model.GetProductVersion() is null)
            {
                sourceModelBuilder.Model.SetProductVersion(ProductInfo.GetVersion());
            }
            var sourceModel = sourceModelBuilder.FinalizeModel();

            var targetModelBuilder = Fixture.TestHelpers.CreateConventionBuilder(skipValidation: true);
            buildTargetAction(targetModelBuilder);
            if (targetModelBuilder.Model.GetProductVersion() is null)
            {
                targetModelBuilder.Model.SetProductVersion(ProductInfo.GetVersion());
            }
            var targetModel = targetModelBuilder.FinalizeModel();

            try
            {
                using (Fixture.TestSqlLoggerFactory.SuspendRecordingEvents())
                {
                    // Apply migrations to get to the source state, and do a scaffolding snapshot for later comparison
                    await migrationsCommandExecutor.ExecuteNonQueryAsync(
                        migrationsSqlGenerator.Generate(modelDiffer.GetDifferences(null, sourceModel), sourceModel),
                        connection);
                }

                var sourceScaffoldedModel = databaseModelFactory.Create(
                    context.Database.GetDbConnection(),
                    new DatabaseModelFactoryOptions());

                // Apply migrations to get from source to target
                await migrationsCommandExecutor.ExecuteNonQueryAsync(
                    migrationsSqlGenerator.Generate(modelDiffer.GetDifferences(sourceModel, targetModel), targetModel),
                    connection);

                using var _ = Fixture.TestSqlLoggerFactory.SuspendRecordingEvents();

                // Reverse-engineer and execute the test-provided assertions on the resulting database model
                var targetScaffoldedModel = databaseModelFactory.Create(
                    context.Database.GetDbConnection(),
                    new DatabaseModelFactoryOptions());

                modelAsserter(targetScaffoldedModel);

                // Apply reverse migrations to go back to the source state

                await migrationsCommandExecutor.ExecuteNonQueryAsync(
                    migrationsSqlGenerator.Generate(modelDiffer.GetDifferences(targetModel, sourceModel), sourceModel),
                    connection);

                var sourceScaffoldedModel2 = databaseModelFactory.Create(
                    context.Database.GetDbConnection(),
                    new DatabaseModelFactoryOptions());

                // TODO: Complete all equality implementations in DatabaseModel and related types
                //Assert.Equal(sourceScaffoldedModel, sourceScaffoldedModel2);

                // Apply reverse migrations to go back to the initial empty state
                await migrationsCommandExecutor.ExecuteNonQueryAsync(
                    migrationsSqlGenerator.Generate(modelDiffer.GetDifferences(sourceModel, null)),
                    connection);

                var emptyModel = databaseModelFactory.Create(
                    context.Database.GetDbConnection(),
                    new DatabaseModelFactoryOptions());

                Assert.Empty(emptyModel.Tables);
                Assert.Empty(emptyModel.Sequences);
            }
            catch
            {
                try
                {
                    Fixture.TestStore.Clean(context);
                }
                catch
                {
                    // ignored, throw the original exception
                }

                throw;
            }
        }

        protected virtual void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        public abstract class MigrationsFixtureBase2 : SharedStoreFixtureBase<PoolableDbContext>
        {
            public abstract TestHelpers TestHelpers { get; }
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;
        }
    }
}
