// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider;

// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Migrations.Internal;

public class MigrationsModelDifferTest : MigrationsModelDifferTestBase
{
    [ConditionalFact]
    public void Model_differ_does_not_detect_views()
        => Execute(
            _ => { },
            modelBuilder => modelBuilder.Entity<TestKeylessType>().HasNoKey().ToView("Vista", "dbo"),
            result => Assert.Equal(0, result.Count));

    [ConditionalFact]
    public void Model_differ_does_not_detect_views_with_owned_types()
        => Execute(
            _ => { },
            target => target.Entity<Order>(
                x =>
                {
                    x.ToView("Orders");
                    x.OwnsOne(y => y.Shipping);
                }),
            upOperations => Assert.Equal(0, upOperations.Count));

    [ConditionalFact]
    public void Model_differ_does_not_detect_views_with_weak_types()
        => Execute(
            _ => { },
            target => target.Entity<Order>(
                x =>
                {
                    x.ToView("Orders");
                    x.OwnsOne(y => y.Billing);
                    x.OwnsOne(y => y.Shipping);
                }),
            upOperations => Assert.Equal(0, upOperations.Count));

    [ConditionalFact]
    public void Model_differ_does_not_detect_defining_queries()
    {
        DbContext context = null;
        Execute(
            _ => { },
#pragma warning disable CS0618 // Type or member is obsolete
            modelBuilder => modelBuilder.Entity<TestKeylessType>().HasNoKey().ToQuery(
                () => context.Set<TestKeylessType>().FromSqlRaw("SELECT * FROM Vista")),
#pragma warning restore CS0618 // Type or member is obsolete
            result => Assert.Empty(result));
    }

    [ConditionalFact]
    public void Model_differ_does_not_detect_queries()
        => Execute(
            _ => { },
            modelBuilder => modelBuilder.Entity<TestKeylessType>().HasNoKey().ToSqlQuery("SELECT * FROM Vista"),
            result => Assert.Empty(result));

    [ConditionalFact]
    public void Model_differ_detects_adding_store_type()
        => Execute(
            _ => { },
            modelBuilder =>
            {
                modelBuilder.Entity(
                    "Cat",
                    x => x.Property<short>("Id"));
            },
            modelBuilder =>
            {
                modelBuilder.Entity(
                    "Cat",
                    x => x.Property<short>("Id").HasConversion<int>());
            },
            upOps => Assert.Collection(
                upOps,
                o =>
                {
                    var m = Assert.IsType<AlterColumnOperation>(o);
                    Assert.Equal("Id", m.Name);
                    Assert.Equal("Cat", m.Table);
                    Assert.Same(typeof(int), m.ClrType);
                }),
            downOps => Assert.Collection(
                downOps,
                o =>
                {
                    var m = Assert.IsType<AlterColumnOperation>(o);
                    Assert.Equal("Id", m.Name);
                    Assert.Equal("Cat", m.Table);
                    Assert.Same(typeof(short), m.ClrType);
                }));

    [ConditionalFact]
    public void Model_differ_detects_adding_value_converter()
        => Execute(
            _ => { },
            modelBuilder =>
            {
                modelBuilder.Entity(
                    "Cat",
                    x => x.Property<short>("Id"));
            },
            modelBuilder =>
            {
                modelBuilder.Entity(
                    "Cat",
                    x => x.Property<short>("Id").HasConversion(v => (long)v, v => (short)v));
            },
            upOps => Assert.Collection(
                upOps,
                o =>
                {
                    var m = Assert.IsType<AlterColumnOperation>(o);
                    Assert.Equal("Id", m.Name);
                    Assert.Equal("Cat", m.Table);
                    Assert.Same(typeof(long), m.ClrType);
                }),
            downOps => Assert.Collection(
                downOps,
                o =>
                {
                    var m = Assert.IsType<AlterColumnOperation>(o);
                    Assert.Equal("Id", m.Name);
                    Assert.Equal("Cat", m.Table);
                    Assert.Same(typeof(short), m.ClrType);
                }));

    [ConditionalFact]
    public void Model_differ_detects_changing_store_type_with_conversions()
        => Execute(
            _ => { },
            modelBuilder =>
            {
                modelBuilder.Entity(
                    "Cat",
                    x => x.Property<short>("Id").HasConversion<int>());
            },
            modelBuilder =>
            {
                modelBuilder.Entity(
                    "Cat",
                    x => x.Property<short>("Id").HasConversion(v => (long)v, v => (short)v));
            },
            upOps => Assert.Collection(
                upOps,
                o =>
                {
                    var m = Assert.IsType<AlterColumnOperation>(o);
                    Assert.Equal("Id", m.Name);
                    Assert.Equal("Cat", m.Table);
                    Assert.Same(typeof(long), m.ClrType);
                }),
            downOps => Assert.Collection(
                downOps,
                o =>
                {
                    var m = Assert.IsType<AlterColumnOperation>(o);
                    Assert.Equal("Id", m.Name);
                    Assert.Equal("Cat", m.Table);
                    Assert.Same(typeof(int), m.ClrType);
                }));

    [ConditionalFact]
    public void Model_differ_breaks_foreign_key_cycles_in_create_table_operations()
        => Execute(
            _ => { },
            modelBuilder =>
            {
                modelBuilder.Entity(
                    "First",
                    x =>
                    {
                        x.Property<int>("ID");
                        x.Property<int>("FK");
                    });

                modelBuilder.Entity(
                    "Second",
                    x =>
                    {
                        x.Property<int>("ID");
                        x.Property<int>("FK");
                    });

                modelBuilder.Entity("First").HasOne("Second").WithMany().HasForeignKey("FK").HasPrincipalKey("ID");
                modelBuilder.Entity("Second").HasOne("First").WithMany().HasForeignKey("FK").HasPrincipalKey("ID");
            },
            result =>
            {
                Assert.Equal(5, result.Count);

                var createFirstTableOperation = Assert.IsType<CreateTableOperation>(result[0]);
                var createSecondTableOperation = Assert.IsType<CreateTableOperation>(result[1]);
                Assert.IsType<CreateIndexOperation>(result[2]);
                Assert.IsType<CreateIndexOperation>(result[3]);
                var addFkOperation = Assert.IsType<AddForeignKeyOperation>(result[4]);

                Assert.Empty(createFirstTableOperation.ForeignKeys);
                Assert.Single(createSecondTableOperation.ForeignKeys);
                Assert.Equal(createFirstTableOperation.Name, addFkOperation.Table);
            });

    [ConditionalFact]
    public void Model_differ_breaks_foreign_key_cycles_in_drop_table_operations()
        => Execute(
            modelBuilder =>
            {
                modelBuilder.Entity(
                    "Third",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<int>("FourthId");
                    });
                modelBuilder.Entity(
                    "Fourth",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<int>("ThirdId");
                    });

                modelBuilder.Entity("Third").HasOne("Fourth").WithMany().HasForeignKey("FourthId");
                modelBuilder.Entity("Fourth").HasOne("Third").WithMany().HasForeignKey("ThirdId");
            },
            _ => { },
            operations =>
            {
                Assert.Collection(
                    operations,
                    o => Assert.IsType<DropForeignKeyOperation>(o),
                    o => Assert.IsType<DropTableOperation>(o),
                    o => Assert.IsType<DropTableOperation>(o));
            });

    [ConditionalFact]
    public void Model_differ_breaks_double_foreign_key_cycles_in_create_table_operations()
        => Execute(
            _ => { },
            modelBuilder =>
            {
                modelBuilder
                    .Entity("Banks")
                    .Property<string>("Id");

                modelBuilder
                    .Entity("BankRegistrations")
                    .Property<string>("Id");

                modelBuilder
                    .Entity("BankProfiles")
                    .Property<string>("Id");

                modelBuilder
                    .Entity("Banks")
                    .HasOne("BankRegistrations")
                    .WithOne()
                    .HasForeignKey("Banks", "DefaultBankRegistrationId");

                modelBuilder
                    .Entity("Banks")
                    .HasOne("BankRegistrations")
                    .WithOne()
                    .HasForeignKey("Banks", "StagingBankRegistrationId");

                modelBuilder
                    .Entity("Banks")
                    .HasOne("BankProfiles")
                    .WithOne()
                    .HasForeignKey("Banks", "DefaultBankProfileId");

                modelBuilder
                    .Entity("Banks")
                    .HasOne("BankProfiles")
                    .WithOne()
                    .HasForeignKey("Banks", "StagingBankProfileId");

                modelBuilder
                    .Entity("BankRegistrations")
                    .HasOne("Banks")
                    .WithMany()
                    .HasForeignKey("BankId");

                modelBuilder
                    .Entity("BankProfiles")
                    .HasOne("Banks")
                    .WithMany()
                    .HasForeignKey("BankId");

                modelBuilder
                    .Entity("BankProfiles")
                    .HasOne("BankRegistrations")
                    .WithMany()
                    .HasForeignKey("BankRegistrationId");
            },
            result =>
            {
                Assert.Equal(3, result.OfType<CreateTableOperation>().Count());
                Assert.Equal(7, result.OfType<CreateIndexOperation>().Count());
                Assert.Equal(
                    7, result.OfType<CreateTableOperation>().SelectMany(t => t.ForeignKeys).Count()
                    + result.OfType<AddForeignKeyOperation>().Count());
            });

    [ConditionalFact]
    public void Model_differ_breaks_multiple_foreign_key_cycles_in_create_table_operations()
        => Execute(
            _ => { },
            modelBuilder =>
            {
                modelBuilder.Entity<Book>(
                    entity =>
                    {
                        entity.HasOne(d => d.Album)
                            .WithMany(p => p.Books);
                        entity.HasOne(d => d.User)
                            .WithMany(p => p.Books);
                    });
                modelBuilder.Entity<Album>(
                    entity =>
                    {
                        entity.HasOne(d => d.OwnerUser)
                            .WithMany(p => p.AlbumOwnerUsers);
                        entity.HasOne(d => d.Book)
                            .WithMany(p => p.Albums);
                    });
                modelBuilder.Entity<User>(
                    entity =>
                    {
                        entity.HasOne(d => d.Book)
                            .WithMany(p => p.Users);
                        entity.HasOne(d => d.ReaderGroup)
                            .WithMany(p => p.UserReaderGroups);
                    });
                modelBuilder.Entity<Group>(
                    entity =>
                    {
                        entity.HasOne(d => d.OwnerAlbum)
                            .WithMany(p => p.Groups);
                        entity.HasOne(d => d.OwnerUser)
                            .WithMany(p => p.Groups);
                    });
            },
            result =>
            {
                Assert.Equal(4, result.OfType<CreateTableOperation>().Count());
                Assert.Equal(8, result.OfType<CreateIndexOperation>().Count());
                Assert.Equal(
                    8, result.OfType<CreateTableOperation>().SelectMany(t => t.ForeignKeys).Count()
                    + result.OfType<AddForeignKeyOperation>().Count());
            });

    private class Book
    {
        public int Id { get; set; }

        public Album Album { get; set; }
        public User User { get; set; }
        public ICollection<Album> Albums { get; set; }
        public ICollection<User> Users { get; set; }
    }

    private class Album
    {
        public int Id { get; set; }

        public User OwnerUser { get; set; }
        public Book Book { get; set; }
        public ICollection<Book> Books { get; set; }
        public ICollection<Group> Groups { get; set; }
    }

    private class User
    {
        public int Id { get; set; }

        public Book Book { get; set; }
        public Group ReaderGroup { get; set; }
        public ICollection<Album> AlbumOwnerUsers { get; set; }
        public ICollection<Book> Books { get; set; }
        public ICollection<Group> Groups { get; set; }
    }

    private class Group
    {
        public int Id { get; set; }

        public Album OwnerAlbum { get; set; }
        public User OwnerUser { get; set; }
        public ICollection<User> UserReaderGroups { get; set; }
    }

    [ConditionalFact]
    public void Create_table()
        => Execute(
            _ => { },
            _ => { },
            modelBuilder => modelBuilder.Entity(
                "Node",
                x =>
                {
                    x.ToTable("Node", "dbo", tb => tb.HasCheckConstraint("CK_Node_SomeCheckConstraint", "[Id] > 10"));
                    x.Property<int>("Id");
                    x.Property<int>("AltId");
                    x.HasAlternateKey("AltId");
                    x.Property<int?>("ParentAltId");
                    x.HasOne("Node").WithMany().HasForeignKey("ParentAltId");
                    x.HasIndex("ParentAltId");
                }),
            upOps =>
            {
                Assert.Equal(3, upOps.Count);

                var ensureSchemaOperation = Assert.IsType<EnsureSchemaOperation>(upOps[0]);
                Assert.Equal("dbo", ensureSchemaOperation.Name);

                var createTableOperation = Assert.IsType<CreateTableOperation>(upOps[1]);
                Assert.Equal("Node", createTableOperation.Name);
                Assert.Equal("dbo", createTableOperation.Schema);
                Assert.Equal(3, createTableOperation.Columns.Count);
                Assert.Null(createTableOperation.Columns.First(o => o.Name == "AltId").DefaultValue);
                Assert.NotNull(createTableOperation.PrimaryKey);
                Assert.Single(createTableOperation.UniqueConstraints);
                var checkConstraint = createTableOperation.CheckConstraints.Single();
                Assert.Equal("CK_Node_SomeCheckConstraint", checkConstraint.Name);
                Assert.Equal("[Id] > 10", checkConstraint.Sql);
                Assert.Single(createTableOperation.ForeignKeys);

                Assert.IsType<CreateIndexOperation>(upOps[2]);
            },
            downOps => Assert.Collection(
                downOps,
                o =>
                {
                    var operation = Assert.IsType<DropTableOperation>(o);
                    Assert.Equal("Node", operation.Name);
                }));

    private class CreateTableEntity1
    {
        public int Id { get; set; }
        public int C { get; set; }
        public int B { get; set; }
        public int A { get; set; }
    }

    private class CreateTableEntity2
    {
        public int Id { get; set; }
        public int E { get; set; }
        public CreateTableEntity2B D { get; set; }
        public int A { get; set; }
    }

    private class CreateTableEntity2B
    {
        public int B { get; set; }
        public int C { get; set; }
    }

    [ConditionalFact]
    public void Create_table_columns_use_property_order()
        => Execute(
            _ => { },
            modelBuilder => modelBuilder.Entity<CreateTableEntity1>(),
            operations =>
            {
                var operation = Assert.IsType<CreateTableOperation>(Assert.Single(operations));
                Assert.Collection(
                    operation.Columns,
                    x => Assert.Equal("Id", x.Name),
                    x => Assert.Equal("C", x.Name),
                    x => Assert.Equal("B", x.Name),
                    x => Assert.Equal("A", x.Name));
            });

    [ConditionalFact]
    public void Create_table_columns_use_dependent_to_principal_and_key_order_when_shadow_fk()
        => Execute(
            _ => { },
            modelBuilder =>
            {
                modelBuilder.Entity<CreateTableEntity2>();
                modelBuilder.Entity<CreateTableEntity2B>().HasKey(
                    e => new { e.C, e.B });
            },
            operations =>
            {
                Assert.Equal(3, operations.Count);

                Assert.IsType<CreateTableOperation>(operations[0]);

                var operation = Assert.IsType<CreateTableOperation>(operations[1]);
                Assert.Equal("CreateTableEntity2", operation.Name);
                Assert.Collection(
                    operation.Columns,
                    x => Assert.Equal("Id", x.Name),
                    x => Assert.Equal("E", x.Name),
                    x => Assert.Equal("DC", x.Name),
                    x => Assert.Equal("DB", x.Name),
                    x => Assert.Equal("A", x.Name));

                Assert.IsType<CreateIndexOperation>(operations[2]);
            });

    [ConditionalFact]
    public void Create_table_columns_uses_defining_navigation_order()
        => Execute(
            _ => { },
            modelBuilder => modelBuilder.Entity<CreateTableEntity2>().OwnsOne(e => e.D),
            operations =>
            {
                var operation = Assert.IsType<CreateTableOperation>(Assert.Single(operations));
                Assert.Collection(
                    operation.Columns,
                    x => Assert.Equal("Id", x.Name),
                    x => Assert.Equal("E", x.Name),
                    x => Assert.Equal("D_B", x.Name),
                    x => Assert.Equal("D_C", x.Name),
                    x => Assert.Equal("A", x.Name));
            });

    [ConditionalFact]
    public void Create_table_columns_uses_principal_to_dependent_order_when_splitting()
        => Execute(
            _ => { },
            modelBuilder => modelBuilder.Entity<CreateTableEntity2B>().ToTable("CreateTableEntity2")
                .HasOne<CreateTableEntity2>().WithOne(x => x.D).HasForeignKey<CreateTableEntity2B>("Id"),
            operations =>
            {
                var operation = Assert.IsType<CreateTableOperation>(Assert.Single(operations));
                Assert.Collection(
                    operation.Columns,
                    x => Assert.Equal("Id", x.Name),
                    x => Assert.Equal("E", x.Name),
                    x => Assert.Equal("B", x.Name),
                    x => Assert.Equal("C", x.Name),
                    x => Assert.Equal("A", x.Name));
            });

    [ConditionalFact]
    public void Create_table_columns_groups_and_sorts_type_hierarchy()
        => Execute(
            _ => { },
            modelBuilder =>
            {
                modelBuilder.Entity("D").Property<int>("Id");
                modelBuilder.Entity("C").HasBaseType("D").Property<int>("C");
                modelBuilder.Entity("B").HasBaseType("D").Property<int>("B");
                modelBuilder.Entity("A").HasBaseType("B").Property<int>("A");
            },
            operations =>
            {
                var operation = Assert.IsType<CreateTableOperation>(Assert.Single(operations));
                Assert.Collection(
                    operation.Columns,
                    x => Assert.Equal("Id", x.Name),
                    x => Assert.Equal("Discriminator", x.Name),
                    x => Assert.Equal("B", x.Name),
                    x => Assert.Equal("A", x.Name),
                    x => Assert.Equal("C", x.Name));
            });

    [ConditionalFact]
    public void Create_table_columns_handles_aliased_columns()
        => Execute(
            _ => { },
            modelBuilder => modelBuilder.Entity<CreateTableEntity1>(
                b =>
                {
                    b.Property(e => e.C).HasColumnName("C");
                    b.Property(e => e.A).HasColumnName("C");
                }),
            operations =>
            {
                var operation = Assert.IsType<CreateTableOperation>(Assert.Single(operations));
                Assert.Collection(
                    operation.Columns,
                    x => Assert.Equal("Id", x.Name),
                    x => Assert.Equal("C", x.Name),
                    x => Assert.Equal("B", x.Name));
            });

    [ConditionalFact]
    public void Create_table_columns_handles_shadow_defining_navigation()
        => Execute(
            _ => { },
            modelBuilder => modelBuilder.Entity(
                "X",
                x =>
                {
                    x.Property<int>("Id");
                    x.OwnsOne("Y", "Y").Property<int>("A");
                }),
            operations =>
            {
                var operation = Assert.IsType<CreateTableOperation>(Assert.Single(operations));
                Assert.Collection(
                    operation.Columns,
                    x => Assert.Equal("Id", x.Name),
                    x => Assert.Equal("Y_A", x.Name));
            });

    [ConditionalFact]
    public void Create_table_columns_handles_shadow_principal_to_dependent_when_splitting()
        => Execute(
            _ => { },
            modelBuilder => modelBuilder
                .Entity("X", x => x.Property<int>("Id"))
                .Entity(
                    "Y",
                    x =>
                    {
                        x.ToTable("X");
                        x.Property<int>("A");
                        x.HasOne("X").WithOne("Y").HasForeignKey("Y", "Id");
                    }),
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<CreateTableOperation>(operations[0]);
                Assert.Collection(
                    operation.Columns,
                    x => Assert.Equal("Id", x.Name),
                    x => Assert.Equal("A", x.Name));
            });

    [ConditionalFact]
    public void Create_table_columns_handles_no_principal_to_dependent_when_splitting()
        => Execute(
            _ => { },
            modelBuilder => modelBuilder
                .Entity("X", x => x.Property<int>("Id"))
                .Entity(
                    "Y",
                    x =>
                    {
                        x.ToTable("X");
                        x.Property<int>("A");
                        x.HasOne("X").WithOne().HasForeignKey("Y", "Id");
                    }),
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<CreateTableOperation>(operations[0]);
                Assert.Collection(
                    operation.Columns,
                    x => Assert.Equal("Id", x.Name),
                    x => Assert.Equal("A", x.Name));
            });

    [ConditionalFact]
    public void Create_table_columns_handles_shadow_dependent_to_principal()
        => Execute(
            _ => { },
            modelBuilder => modelBuilder
                .Entity("X", x => x.Property<int>("Id"))
                .Entity(
                    "Y",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.HasOne("X", "X").WithMany("Ys").HasForeignKey("XId");
                    }),
            operations =>
            {
                Assert.Equal(3, operations.Count);

                Assert.IsType<CreateTableOperation>(operations[0]);

                var operation = Assert.IsType<CreateTableOperation>(operations[1]);
                Assert.Collection(
                    operation.Columns,
                    x => Assert.Equal("Id", x.Name),
                    x => Assert.Equal("XId", x.Name));

                Assert.IsType<CreateIndexOperation>(operations[2]);
            });

    [ConditionalFact]
    public void Create_table_columns_handles_no_dependent_to_principal()
        => Execute(
            _ => { },
            modelBuilder => modelBuilder
                .Entity("X", x => x.Property<int>("Id"))
                .Entity(
                    "Y",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.HasOne("X").WithMany().HasForeignKey("XId");
                    }),
            operations =>
            {
                Assert.Equal(3, operations.Count);

                Assert.IsType<CreateTableOperation>(operations[0]);

                var operation = Assert.IsType<CreateTableOperation>(operations[1]);
                Assert.Collection(
                    operation.Columns,
                    x => Assert.Equal("Id", x.Name),
                    x => Assert.Equal("XId", x.Name));

                Assert.IsType<CreateIndexOperation>(operations[2]);
            });

    [ConditionalFact]
    public void Create_table_columns_handles_self_referencing_one_to_many()
        => Execute(
            _ => { },
            modelBuilder => modelBuilder
                .Entity(
                    "X",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.HasOne("X").WithMany();
                    }),
            operations =>
            {
                Assert.Equal(2, operations.Count);

                var operation = Assert.IsType<CreateTableOperation>(operations[0]);
                Assert.Collection(
                    operation.Columns,
                    x => Assert.Equal("Id", x.Name),
                    x => Assert.Equal("XId", x.Name));

                Assert.IsType<CreateIndexOperation>(operations[1]);
            });

    [ConditionalFact]
    public void Create_table_columns_handles_self_referencing_one_to_one()
        => Execute(
            _ => { },
            modelBuilder => modelBuilder
                .Entity(
                    "X",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.HasOne("X").WithOne();
                    }),
            operations =>
            {
                Assert.Equal(2, operations.Count);

                var operation = Assert.IsType<CreateTableOperation>(operations[0]);
                Assert.Collection(
                    operation.Columns,
                    x => Assert.Equal("Id", x.Name),
                    x => Assert.Equal("XId", x.Name));

                Assert.IsType<CreateIndexOperation>(operations[1]);
            });

    [ConditionalFact]
    public void Create_table_columns_use_explicit_order()
        => Execute(
            _ => { },
            modelBuilder => modelBuilder.Entity<CreateTableEntity1>(
                b =>
                {
                    b.Property(e => e.C).HasColumnOrder(3);
                    b.Property(e => e.B).HasColumnOrder(1);
                    b.Property(e => e.A).HasColumnOrder(2);
                }),
            operations =>
            {
                var operation = Assert.IsType<CreateTableOperation>(Assert.Single(operations));
                Assert.Collection(
                    operation.Columns,
                    x =>
                    {
                        Assert.Equal("B", x.Name);
                        Assert.Null(operation.FindAnnotation(RelationalAnnotationNames.ColumnOrder));
                    },
                    x => Assert.Equal("A", x.Name),
                    x => Assert.Equal("C", x.Name),
                    x => Assert.Equal("Id", x.Name));
            });

    [ConditionalFact]
    public void Create_FK_to_excluded_principal()
        => Execute(
            common => common
                .Entity(
                    "X", x =>
                    {
                        x.Property<int>("Id");
                        x.ToTable("X", t => t.ExcludeFromMigrations());
                    })
                .Entity(
                    "Y",
                    x =>
                    {
                        x.Property<int>("Id");
                    }),
            _ => { },
            target => target
                .Entity(
                    "Y",
                    x =>
                    {
                        x.HasOne("X").WithMany().HasForeignKey("XId");
                    }),
            upOps => Assert.Collection(
                upOps,
                o =>
                {
                    var operation = Assert.IsType<AddColumnOperation>(o);
                    Assert.Equal("XId", operation.Name);
                },
                o =>
                {
                    var operation = Assert.IsType<CreateIndexOperation>(o);
                    Assert.Equal("IX_Y_XId", operation.Name);
                    Assert.Collection(
                        operation.Columns,
                        x => Assert.Equal("XId", x));
                },
                o =>
                {
                    var operation = Assert.IsType<AddForeignKeyOperation>(o);
                    Assert.Equal("FK_Y_X_XId", operation.Name);
                    Assert.Collection(
                        operation.Columns,
                        x => Assert.Equal("XId", x));
                }),
            downOps => Assert.Collection(
                downOps,
                o =>
                {
                    var operation = Assert.IsType<DropForeignKeyOperation>(o);
                    Assert.Equal("FK_Y_X_XId", operation.Name);
                },
                o =>
                {
                    var operation = Assert.IsType<DropIndexOperation>(o);
                    Assert.Equal("IX_Y_XId", operation.Name);
                },
                o =>
                {
                    var operation = Assert.IsType<DropColumnOperation>(o);
                    Assert.Equal("XId", operation.Name);
                }));

    [ConditionalFact]
    public void Create_table_no_key()
        => Execute(
            _ => { },
            _ => { },
            modelBuilder => modelBuilder.Entity("Anonymous").HasNoKey().Property<int>("Value"),
            upOps =>
            {
                Assert.Equal(1, upOps.Count);

                var operation = Assert.IsType<CreateTableOperation>(upOps[0]);
                Assert.Equal("Anonymous", operation.Name);
                Assert.Null(operation.PrimaryKey);
            },
            downOps =>
            {
                Assert.Equal(1, downOps.Count);

                var operation = Assert.IsType<DropTableOperation>(downOps[0]);
                Assert.Equal("Anonymous", operation.Name);
            });

    [ConditionalFact]
    public void Create_table_from_view()
        => Execute(
            _ => { },
            source => source.Entity("Material").ToView("Immaterial").HasNoKey().Property<int>("Value"),
            target => target.Entity("Material").HasNoKey().Property<int>("Value"),
            upOps =>
            {
                Assert.Equal(1, upOps.Count);

                var operation = Assert.IsType<CreateTableOperation>(upOps[0]);
                Assert.Equal("Material", operation.Name);
            },
            downOps =>
            {
                Assert.Equal(1, downOps.Count);

                var operation = Assert.IsType<DropTableOperation>(downOps[0]);
                Assert.Equal("Material", operation.Name);
            });

    [ConditionalFact]
    public void Alter_table_comment()
        => Execute(
            source => source.Entity(
                "MountainLion",
                x =>
                {
                    x.ToTable("MountainLion", "dbo", tb => tb.HasComment("Old comment"));
                    x.Property<int>("Id");
                }),
            target => target.Entity(
                "MountainLion",
                x =>
                {
                    x.ToTable("MountainLion", "dbo", tb => tb.HasComment("New comment"));
                    x.Property<int>("Id");
                }),
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<AlterTableOperation>(operations[0]);
                Assert.Equal("dbo", operation.Schema);
                Assert.Equal("MountainLion", operation.Name);
                Assert.Equal("New comment", operation.Comment);
                Assert.Equal("Old comment", operation.OldTable.Comment);
            });

    [ConditionalFact]
    public void Rename_table()
        => Execute(
            source => source.Entity("Cat").ToTable("Cat", "dbo").Property<int>("Id"),
            target => target.Entity(
                "Cat",
                x =>
                {
                    x.ToTable("Cats", "dbo");
                    x.Property<int>("Id");
                    x.HasKey("Id").HasName("PK_Cat");
                }),
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<RenameTableOperation>(operations[0]);
                Assert.Equal("Cat", operation.Name);
                Assert.Equal("dbo", operation.Schema);
                Assert.Equal("Cats", operation.NewName);
                Assert.Equal("dbo", operation.NewSchema);
            });

    [ConditionalFact]
    public void Rename_table_with_foreign_keys()
        => Execute(
            modelBuilder =>
            {
                modelBuilder
                    .HasAnnotation("ProductVersion", "6.0.0");

                modelBuilder.Entity(
                    "TableRename.Entity1", b =>
                    {
                        b.Property<int>("Id")
                            .ValueGeneratedOnAdd();

                        b.Property<int>("Entity2Id");

                        b.Property<int>("Entity3Id");

                        b.HasKey("Id");

                        b.HasIndex("Entity2Id");

                        b.HasIndex("Entity3Id");

                        b.ToTable("Entity1");
                    });

                modelBuilder.Entity(
                    "TableRename.Entity2", b =>
                    {
                        b.Property<int>("Id")
                            .ValueGeneratedOnAdd();

                        b.HasKey("Id");

                        b.ToTable("Entity2");
                    });

                modelBuilder.Entity(
                    "TableRename.Entity3", b =>
                    {
                        b.Property<int>("Id")
                            .ValueGeneratedOnAdd();

                        b.HasKey("Id");

                        b.ToTable("Entity3");
                    });

                modelBuilder.Entity(
                    "TableRename.Entity1", b =>
                    {
                        b.HasOne("TableRename.Entity2", "Entity2Navigation")
                            .WithMany("Entity1s")
                            .HasForeignKey("Entity2Id")
                            .OnDelete(DeleteBehavior.Cascade)
                            .IsRequired();

                        b.HasOne("TableRename.Entity3", "Entity3Navigation")
                            .WithMany("Entity1s")
                            .HasForeignKey("Entity3Id")
                            .OnDelete(DeleteBehavior.Cascade)
                            .IsRequired();
                    });
            },
            modelBuilder =>
            {
                modelBuilder.Entity(
                    "TableRename.Entity4", b =>
                    {
                        b.Property<int>("Id");

                        b.Property<int>("Entity2Id");

                        b.Property<int>("Entity3Id");
                    });

                modelBuilder.Entity(
                    "TableRename.Entity2", b =>
                    {
                        b.Property<int>("Id");
                    });

                modelBuilder.Entity(
                    "TableRename.Entity3", b =>
                    {
                        b.Property<int>("Id");
                    });

                modelBuilder.Entity(
                    "TableRename.Entity4", b =>
                    {
                        b.HasOne("TableRename.Entity2", "Entity2Navigation")
                            .WithMany("Entity4s")
                            .HasForeignKey("Entity2Id")
                            .OnDelete(DeleteBehavior.Cascade)
                            .IsRequired();

                        b.HasOne("TableRename.Entity3", "Entity3Navigation")
                            .WithMany("Entity4s")
                            .HasForeignKey("Entity3Id")
                            .OnDelete(DeleteBehavior.Cascade)
                            .IsRequired();
                    });
            },
            upOps => Assert.Collection(
                upOps,
                o =>
                {
                    var m = Assert.IsType<DropTableOperation>(o);
                    Assert.Equal("Entity1", m.Name);
                    Assert.Null(m.Schema);
                },
                o =>
                {
                    var m = Assert.IsType<CreateTableOperation>(o);
                    Assert.Equal("Entity4", m.Name);
                    Assert.Null(m.Schema);
                },
                o =>
                {
                    var m = Assert.IsType<CreateIndexOperation>(o);
                    Assert.Equal("Entity4", m.Table);
                    Assert.Equal("IX_Entity4_Entity2Id", m.Name);
                    Assert.Collection(
                        m.Columns,
                        v => Assert.Equal("Entity2Id", v));
                },
                o =>
                {
                    var m = Assert.IsType<CreateIndexOperation>(o);
                    Assert.Equal("Entity4", m.Table);
                    Assert.Equal("IX_Entity4_Entity3Id", m.Name);
                    Assert.Collection(
                        m.Columns,
                        v => Assert.Equal("Entity3Id", v));
                }), skipSourceConventions: true);

    [ConditionalFact]
    public void Move_table()
        => Execute(
            source => source.Entity("Person").ToTable("People", "dbo").Property<int>("Id"),
            target => target.Entity("Person").ToTable("People", "public").Property<int>("Id"),
            operations =>
            {
                Assert.Equal(2, operations.Count);

                var ensureSchemaOperation = Assert.IsType<EnsureSchemaOperation>(operations[0]);
                Assert.Equal("public", ensureSchemaOperation.Name);

                var renameTableOperation = Assert.IsType<RenameTableOperation>(operations[1]);
                Assert.Equal("People", renameTableOperation.Name);
                Assert.Equal("dbo", renameTableOperation.Schema);
                Assert.Equal("People", renameTableOperation.NewName);
                Assert.Equal("public", renameTableOperation.NewSchema);
            });

    [ConditionalFact]
    public void Rename_entity_type()
        => Execute(
            source => source.Entity(
                "Dog",
                x =>
                {
                    x.Property<int>("Id");
                    x.HasKey("Id").HasName("PK_Dog");
                }),
            target => target.Entity(
                "Doge",
                x =>
                {
                    x.ToTable("Dog");
                    x.Property<int>("Id");
                    x.HasKey("Id").HasName("PK_Dog");
                }),
            Assert.Empty);

    [ConditionalFact]
    public void Create_shared_table_with_two_types()
        => Execute(
            _ => { },
            _ => { },
            modelBuilder =>
            {
                modelBuilder.Entity(
                    "Cat",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<string>("MouseId");
                        x.ToTable("Animal");
                    });
                modelBuilder.Entity(
                    "Dog",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<string>("BoneId");
                        x.HasOne("Cat").WithOne().HasForeignKey("Dog", "Id");
                        x.ToTable("Animal");
                    });
            },
            upOps =>
            {
                Assert.Equal(1, upOps.Count);

                var createTableOperation = Assert.IsType<CreateTableOperation>(upOps[0]);
                Assert.Equal("Animal", createTableOperation.Name);
                Assert.Equal("Id", createTableOperation.PrimaryKey.Columns.Single());
                Assert.Equal(new[] { "Id", "MouseId", "BoneId" }, createTableOperation.Columns.Select(c => c.Name));
                Assert.Empty(createTableOperation.ForeignKeys);
                Assert.Empty(createTableOperation.UniqueConstraints);
                Assert.Empty(createTableOperation.CheckConstraints);
            },
            downOps =>
            {
                Assert.Equal(1, downOps.Count);

                var dropTableOperation = Assert.IsType<DropTableOperation>(downOps[0]);
                Assert.Equal("Animal", dropTableOperation.Name);
            });

    [ConditionalFact]
    public void Add_type_to_shared_table()
        => Execute(
            modelBuilder =>
            {
                modelBuilder.Entity(
                    "Cat",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<string>("MouseId");
                        x.ToTable("Animal");
                    });
            },
            _ => { },
            modelBuilder =>
            {
                modelBuilder.Entity(
                    "Dog",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<string>("BoneId");
                        x.HasOne("Cat").WithOne().HasForeignKey("Dog", "Id");
                        x.ToTable("Animal");
                    });
            },
            upOps =>
            {
                Assert.Equal(1, upOps.Count);

                var alterTableOperation = Assert.IsType<AddColumnOperation>(upOps[0]);
                Assert.Equal("BoneId", alterTableOperation.Name);
            },
            downOps =>
            {
                Assert.Equal(1, downOps.Count);

                var alterTableOperation = Assert.IsType<DropColumnOperation>(downOps[0]);
                Assert.Equal("BoneId", alterTableOperation.Name);
            });

    [ConditionalFact]
    public void Move_type_from_one_shared_table_to_another_with_seed_data()
        => Execute(
            modelBuilder =>
            {
                modelBuilder.Entity(
                    "Cat",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<string>("MouseId");
                        x.HasData(
                            new { Id = 42, MouseId = "Jerry" });
                    });
                modelBuilder.Entity(
                    "Dog",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<string>("BoneId");
                        x.HasData(
                            new { Id = 42, BoneId = "Brook" });
                    });
                modelBuilder.Entity(
                    "Animal",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<string>("HandlerId");
                        x.HasData(
                            new { Id = 42, HandlerId = "Brenda" });
                    });
            },
            modelBuilder =>
            {
                modelBuilder.Entity(
                    "Animal",
                    x =>
                    {
                        x.HasOne("Dog").WithOne().HasForeignKey("Dog", "Id");
                        x.ToTable("Dog");
                    });
            },
            modelBuilder =>
            {
                modelBuilder.Entity(
                    "Animal",
                    x =>
                    {
                        x.HasOne("Cat").WithOne().HasForeignKey("Cat", "Id");
                        x.ToTable("Cat");
                    });
            },
            upOps => Assert.Collection(
                upOps,
                o =>
                {
                    var m = Assert.IsType<DropColumnOperation>(o);
                    Assert.Equal("HandlerId", m.Name);
                    Assert.Equal("Dog", m.Table);
                },
                o =>
                {
                    var m = Assert.IsType<AddColumnOperation>(o);
                    Assert.Equal("HandlerId", m.Name);
                    Assert.Equal("Cat", m.Table);
                },
                o =>
                {
                    var m = Assert.IsType<UpdateDataOperation>(o);
                    Assert.Equal("Cat", m.Table);
                    AssertMultidimensionalArray(
                        m.KeyValues,
                        v => Assert.Equal(42, v));
                    AssertMultidimensionalArray(
                        m.Values,
                        v => Assert.Equal("Brenda", v));
                    Assert.Collection(
                        m.Columns,
                        v => Assert.Equal("HandlerId", v));
                }),
            downOps => Assert.Collection(
                downOps,
                o =>
                {
                    var m = Assert.IsType<DropColumnOperation>(o);
                    Assert.Equal("HandlerId", m.Name);
                    Assert.Equal("Cat", m.Table);
                },
                o =>
                {
                    var m = Assert.IsType<AddColumnOperation>(o);
                    Assert.Equal("HandlerId", m.Name);
                    Assert.Equal("Dog", m.Table);
                },
                o =>
                {
                    var m = Assert.IsType<UpdateDataOperation>(o);
                    Assert.Equal("Dog", m.Table);
                    AssertMultidimensionalArray(
                        m.KeyValues,
                        v => Assert.Equal(42, v));
                    AssertMultidimensionalArray(
                        m.Values,
                        v => Assert.Equal("Brenda", v));
                    Assert.Collection(
                        m.Columns,
                        v => Assert.Equal("HandlerId", v));
                }));

    [ConditionalFact]
    public void Can_split_entity_in_two_using_shared_table_with_seed_data()
        => Execute(
            _ => { },
            modelBuilder =>
            {
                modelBuilder.Entity(
                    "Animal",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<string>("MouseId");
                        x.Property<string>("BoneId");
                        x.HasData(
                            new
                            {
                                Id = 42,
                                MouseId = "1",
                                BoneId = "2"
                            });
                    });
            },
            modelBuilder =>
            {
                modelBuilder.Entity(
                    "Cat",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<string>("MouseId");
                        x.ToTable("Animal");
                        x.HasData(
                            new { Id = 42, MouseId = "1" });
                    });
                modelBuilder.Entity(
                    "Dog",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<string>("BoneId");
                        x.HasOne("Cat").WithOne().HasForeignKey("Dog", "Id");
                        x.ToTable("Animal");
                        x.HasData(
                            new { Id = 42, BoneId = "2" });
                    });
            },
            upOps => Assert.Equal(0, upOps.Count),
            downOps => Assert.Equal(0, downOps.Count));

    [ConditionalFact]
    public void Can_add_tables_with_entity_splitting_with_seed_data()
        => Execute(
            _ => { },
            _ => { },
            modelBuilder =>
            {
                modelBuilder.Entity(
                    "Animal",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<string>("MouseId");
                        x.Property<string>("BoneId");
                        x.HasData(
                            new
                            {
                                Id = 42,
                                MouseId = "1",
                                BoneId = "2"
                            });
                        x.SplitToTable(
                            "AnimalDetails", t =>
                            {
                                t.Property<string>("BoneId");
                            });
                    });
            },
            upOps => Assert.Collection(
                upOps,
                o =>
                {
                    var m = Assert.IsType<CreateTableOperation>(o);
                    Assert.Equal("Animal", m.Name);
                    Assert.Equal("Id", m.PrimaryKey.Columns.Single());
                    Assert.Equal(new[] { "Id", "MouseId" }, m.Columns.Select(c => c.Name));
                    Assert.Empty(m.ForeignKeys);
                },
                o =>
                {
                    var m = Assert.IsType<CreateTableOperation>(o);
                    Assert.Equal("AnimalDetails", m.Name);
                    Assert.Equal("Id", m.PrimaryKey.Columns.Single());
                    Assert.Equal(new[] { "Id", "BoneId" }, m.Columns.Select(c => c.Name));
                    var fk = m.ForeignKeys.Single();
                    Assert.Equal("Animal", fk.PrincipalTable);
                },
                o =>
                {
                    var m = Assert.IsType<InsertDataOperation>(o);
                    Assert.Equal("Animal", m.Table);
                    AssertMultidimensionalArray(
                        m.Values,
                        v => Assert.Equal(42, v),
                        v => Assert.Equal("1", v));
                    Assert.Collection(
                        m.Columns,
                        v => Assert.Equal("Id", v),
                        v => Assert.Equal("MouseId", v));
                },
                o =>
                {
                    var m = Assert.IsType<InsertDataOperation>(o);
                    Assert.Equal("AnimalDetails", m.Table);
                    AssertMultidimensionalArray(
                        m.Values,
                        v => Assert.Equal(42, v),
                        v => Assert.Equal("2", v));
                    Assert.Collection(
                        m.Columns,
                        v => Assert.Equal("Id", v),
                        v => Assert.Equal("BoneId", v));
                }),
            downOps => Assert.Collection(
                downOps,
                o =>
                {
                    var m = Assert.IsType<DropTableOperation>(o);
                    Assert.Equal("AnimalDetails", m.Name);
                },
                o =>
                {
                    var m = Assert.IsType<DropTableOperation>(o);
                    Assert.Equal("Animal", m.Name);
                }));

    [ConditionalFact]
    public void Add_owned_types()
        => Execute(
            _ => { },
            _ => { },
            modelBuilder =>
            {
                modelBuilder.Entity(
                    "Order",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.OwnsOne("Address", "ShippingAddress");
                        x.OwnsOne("Address", "BillingAddress");
                    });
            },
            upOps => Assert.Collection(
                upOps,
                o =>
                {
                    var m = Assert.IsType<CreateTableOperation>(o);
                    Assert.Equal("Order", m.Name);
                }),
            downOps => Assert.Collection(
                downOps,
                o =>
                {
                    var m = Assert.IsType<DropTableOperation>(o);
                    Assert.Equal("Order", m.Name);
                }));

    [ConditionalFact]
    public void Add_owned_type_with_seed_data()
        => Execute(
            modelBuilder =>
            {
                modelBuilder.Entity(
                    "Order",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.HasData(
                            new { Id = 42 });
                    });
            },
            _ => { },
            modelBuilder =>
            {
                modelBuilder.Entity(
                    "Order",
                    x =>
                    {
                        x.OwnsOne(
                            "Address", "ShippingAddress", s =>
                            {
                                s.Property<string>("Street");
                                s.Property<string>("City");
                                s.HasData(
                                    new
                                    {
                                        OrderId = 42,
                                        Street = "Lombard",
                                        City = "San Francisco"
                                    });
                            });
                        x.OwnsOne(
                            "Address", "BillingAddress", s =>
                            {
                                s.Property<string>("Street");
                                s.Property<string>("City");
                                s.HasData(
                                    new
                                    {
                                        OrderId = 42,
                                        Street = "Abbey Road",
                                        City = "London"
                                    });
                            });
                    });
            },
            upOps => Assert.Collection(
                upOps,
                o =>
                {
                    var m = Assert.IsType<AddColumnOperation>(o);
                    Assert.Equal("BillingAddress_City", m.Name);
                    Assert.Equal("Order", m.Table);
                },
                o =>
                {
                    var m = Assert.IsType<AddColumnOperation>(o);
                    Assert.Equal("BillingAddress_Street", m.Name);
                    Assert.Equal("Order", m.Table);
                },
                o =>
                {
                    var m = Assert.IsType<AddColumnOperation>(o);
                    Assert.Equal("ShippingAddress_City", m.Name);
                    Assert.Equal("Order", m.Table);
                },
                o =>
                {
                    var m = Assert.IsType<AddColumnOperation>(o);
                    Assert.Equal("ShippingAddress_Street", m.Name);
                    Assert.Equal("Order", m.Table);
                },
                o =>
                {
                    var m = Assert.IsType<UpdateDataOperation>(o);
                    AssertMultidimensionalArray(
                        m.KeyValues,
                        v => Assert.Equal(42, v));
                    AssertMultidimensionalArray(
                        m.Values,
                        v => Assert.Equal("London", v),
                        v => Assert.Equal("Abbey Road", v),
                        v => Assert.Equal("San Francisco", v),
                        v => Assert.Equal("Lombard", v));
                    Assert.Collection(
                        m.Columns,
                        v => Assert.Equal("BillingAddress_City", v),
                        v => Assert.Equal("BillingAddress_Street", v),
                        v => Assert.Equal("ShippingAddress_City", v),
                        v => Assert.Equal("ShippingAddress_Street", v));
                }),
            downOps => Assert.Collection(
                downOps,
                o =>
                {
                    var m = Assert.IsType<DropColumnOperation>(o);
                    Assert.Equal("BillingAddress_City", m.Name);
                    Assert.Equal("Order", m.Table);
                },
                o =>
                {
                    var m = Assert.IsType<DropColumnOperation>(o);
                    Assert.Equal("BillingAddress_Street", m.Name);
                    Assert.Equal("Order", m.Table);
                },
                o =>
                {
                    var m = Assert.IsType<DropColumnOperation>(o);
                    Assert.Equal("ShippingAddress_City", m.Name);
                    Assert.Equal("Order", m.Table);
                },
                o =>
                {
                    var m = Assert.IsType<DropColumnOperation>(o);
                    Assert.Equal("ShippingAddress_Street", m.Name);
                    Assert.Equal("Order", m.Table);
                }));

    [ConditionalFact]
    public void Rename_entity_type_with_seed_data()
        => Execute(
            _ => { },
            source => source.Entity(
                "EntityWithIdWrongName",
                x =>
                {
                    x.Property<int>("Id");
                    x.HasKey("Id").HasName("PK_EntityId");
                    x.HasData(
                        new { Id = 42 },
                        new { Id = 27 });
                }),
            target => target.Entity(
                "EntityWithId",
                x =>
                {
                    x.ToTable("EntityWithIdWrongName");
                    x.Property<int>("Id");
                    x.HasKey("Id").HasName("PK_EntityId");
                    x.HasData(
                        new { Id = 42 },
                        new { Id = 27 });
                }),
            Assert.Empty,
            Assert.Empty);

    [ConditionalFact]
    public void Add_column_with_computed_value()
        => Execute(
            source => source.Entity("Dragon").ToTable("Dragon", "dbo").Property<int>("Id"),
            target => target.Entity(
                "Dragon",
                x =>
                {
                    x.ToTable("Dragon", "dbo");
                    x.Property<int>("Id");
                    x.Property<string>("Name")
                        .HasColumnType("nvarchar(30)")
                        .IsRequired()
                        .HasComputedColumnSql("CreateDragonName()");
                }),
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<AddColumnOperation>(operations[0]);
                Assert.Equal("dbo", operation.Schema);
                Assert.Equal("Dragon", operation.Table);
                Assert.Equal("Name", operation.Name);
                Assert.Equal(typeof(string), operation.ClrType);
                Assert.Equal("nvarchar(30)", operation.ColumnType);
                Assert.False(operation.IsNullable);
                Assert.Equal("", operation.DefaultValue);
                Assert.Equal("CreateDragonName()", operation.ComputedColumnSql);
            });

    [ConditionalFact] // Issue #4501
    public void Add_column_ValueGeneratedOnAddOrUpdate_with_default_value_sql()
        => Execute(
            source => source.Entity("Dragon").ToTable("Dragon", "dbo").Property<int>("Id"),
            target => target.Entity(
                "Dragon",
                x =>
                {
                    x.ToTable("Dragon", "dbo");
                    x.Property<int>("Id");
                    x.Property<DateTime>("LastModified")
                        .HasDefaultValueSql("GETDATE()")
                        .ValueGeneratedOnAddOrUpdate();
                }),
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<AddColumnOperation>(operations[0]);
                Assert.Equal("dbo", operation.Schema);
                Assert.Equal("Dragon", operation.Table);
                Assert.Equal("LastModified", operation.Name);
                Assert.Equal(typeof(DateTime), operation.ClrType);
                Assert.Null(operation.ComputedColumnSql);
                Assert.Equal("GETDATE()", operation.DefaultValueSql);
            });

    [ConditionalFact]
    public void Add_column_ValueGeneratedOnUpdate_with_default_value_sql()
        => Execute(
            source => source.Entity("Dragon").ToTable("Dragon", "dbo").Property<int>("Id"),
            target => target.Entity(
                "Dragon",
                x =>
                {
                    x.ToTable("Dragon", "dbo");
                    x.Property<int>("Id");
                    x.Property<DateTime>("LastModified")
                        .HasDefaultValueSql("GETDATE()")
                        .ValueGeneratedOnUpdate();
                }),
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<AddColumnOperation>(operations[0]);
                Assert.Equal("dbo", operation.Schema);
                Assert.Equal("Dragon", operation.Table);
                Assert.Equal("LastModified", operation.Name);
                Assert.Equal(typeof(DateTime), operation.ClrType);
                Assert.Null(operation.ComputedColumnSql);
                Assert.Equal("GETDATE()", operation.DefaultValueSql);
            });

    [ConditionalTheory]
    [InlineData(typeof(int), 0)]
    [InlineData(typeof(int?), 0)]
    [InlineData(typeof(string), "")]
    [InlineData(typeof(byte[]), new byte[0])]
    [InlineData(typeof(SomeEnum), 0)]
    [InlineData(typeof(SomeEnum?), 0)]
    public void Add_column_not_null(Type type, object expectedDefault)
        => Execute(
            source => source.Entity("Robin").Property<int>("Id"),
            target => target.Entity(
                "Robin",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property(type, "Value").IsRequired();
                }),
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<AddColumnOperation>(operations[0]);
                Assert.Equal("Robin", operation.Table);
                Assert.Equal("Value", operation.Name);
                Assert.Equal(expectedDefault, operation.DefaultValue);
            });

    [ConditionalFact]
    public void Add_property_converted_to_nullable()
        => Execute(
            source => source.Entity("Capybara").Property<int>("Id"),
            target => target.Entity(
                "Capybara",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<SomeEnum>("SomeEnum")
                        .HasConversion(m => (int?)m, p => p.HasValue ? (SomeEnum)p.Value : default);
                }),
            operations =>
            {
                var operation = Assert.IsType<AddColumnOperation>(Assert.Single(operations));
                Assert.Equal(typeof(int), operation.ClrType);
            });

    [ConditionalFact]
    public void Add_column_with_seed_data()
        => Execute(
            _ => { },
            source => source.Entity(
                "Firefly",
                x =>
                {
                    x.ToTable("Firefly", "dbo");
                    x.Property<int>("Id");
                    x.HasData(
                        new { Id = 42 });
                }),
            target => target.Entity(
                "Firefly",
                x =>
                {
                    x.ToTable("Firefly", "dbo");
                    x.Property<int>("Id");
                    x.Property<string>("Name").HasColumnType("nvarchar(30)");
                    x.HasData(
                        new { Id = 42, Name = "Firefly 1" },
                        new { Id = 43, Name = "Firefly 2" });
                }),
            upOps => Assert.Collection(
                upOps,
                o =>
                {
                    var operation = Assert.IsType<AddColumnOperation>(o);
                    Assert.Equal("dbo", operation.Schema);
                    Assert.Equal("Firefly", operation.Table);
                    Assert.Equal("Name", operation.Name);
                },
                o =>
                {
                    var m = Assert.IsType<UpdateDataOperation>(o);
                    AssertMultidimensionalArray(
                        m.KeyValues,
                        v => Assert.Equal(42, v));
                    AssertMultidimensionalArray(
                        m.Values,
                        v => Assert.Equal("Firefly 1", v));
                },
                o =>
                {
                    var m = Assert.IsType<InsertDataOperation>(o);
                    AssertMultidimensionalArray(
                        m.Values,
                        v => Assert.Equal(43, v),
                        v => Assert.Equal("Firefly 2", v));
                }),
            downOps => Assert.Collection(
                downOps,
                o =>
                {
                    var m = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Null(m.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        m.KeyValues,
                        v => Assert.Equal(43, v));
                },
                o =>
                {
                    var operation = Assert.IsType<DropColumnOperation>(o);
                    Assert.Equal("dbo", operation.Schema);
                    Assert.Equal("Firefly", operation.Table);
                    Assert.Equal("Name", operation.Name);
                }));

    [ConditionalFact]
    public void Throws_on_null_keys_in_seed_data()
        => Assert.Equal(
            RelationalStrings.NullKeyValue(
                "dbo.Firefly",
                "Id"),
            Assert.Throws<InvalidOperationException>(
                () => Execute(
                    common => common.Entity(
                        "Firefly",
                        x =>
                        {
                            x.ToTable("Firefly", "dbo");
                            x.Property<int>("Id");
                            x.HasData(
                                new { Id = (int?)null });
                        }),
                    _ => { },
                    _ => { },
                    upOps => { },
                    downOps => { })).Message);

    [ConditionalFact]
    public void Throws_on_composite_null_keys_in_seed_data()
        => Assert.Equal(
            RelationalStrings.NullKeyValue(
                "dbo.Firefly",
                "Id"),
            Assert.Throws<InvalidOperationException>(
                () => Execute(
                    common => common.Entity(
                        "Firefly",
                        x =>
                        {
                            x.ToTable("Firefly", "dbo");
                            x.Property<int>("Id");
                            x.Property<string>("Name");
                            x.HasKey("Id", "Name");
                            x.HasData(
                                new { Id = (int?)null, Name = "Firefly 1" });
                        }),
                    _ => { },
                    _ => { },
                    upOps => { },
                    downOps => { })).Message);

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void Throws_on_duplicate_seed_data(bool enableSensitiveLogging)
        => Assert.Equal(
            enableSensitiveLogging
                ? RelationalStrings.DuplicateSeedDataSensitive(
                    "Firefly (Dictionary<string, object>)",
                    "{42}",
                    "dbo.Firefly")
                : RelationalStrings.DuplicateSeedData(
                    "Firefly (Dictionary<string, object>)",
                    "dbo.Firefly"),
            Assert.Throws<InvalidOperationException>(
                () => Execute(
                    common => common.Entity(
                        "Firefly",
                        x =>
                        {
                            x.ToTable("Firefly", "dbo");
                            x.Property<int>("Id");
                            x.HasData(
                                new { Id = 42 },
                                new { Id = 42 });
                        }),
                    _ => { },
                    _ => { },
                    upOps => { },
                    downOps => { },
                    _ => { },
                    enableSensitiveLogging: enableSensitiveLogging)).Message);

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void Throws_on_conflicting_seed_data(bool enableSensitiveLogging)
        => Assert.Equal(
            enableSensitiveLogging
                ? RelationalStrings.ConflictingSeedValuesSensitive(
                    "FireflyDetails (Dictionary<string, object>)",
                    "{42}",
                    "Firefly",
                    "Name",
                    "1",
                    "2")
                : RelationalStrings.ConflictingSeedValues(
                    "FireflyDetails (Dictionary<string, object>)",
                    "Firefly",
                    "Name"),
            Assert.Throws<InvalidOperationException>(
                () => Execute(
                    common =>
                    {
                        common.Entity(
                            "Firefly",
                            x =>
                            {
                                x.ToTable("Firefly");
                                x.Property<int>("Id");
                                x.Property<string>("Name");
                                x.HasData(
                                    new { Id = 42, Name = "1" });
                            });

                        common.Entity(
                            "FireflyDetails",
                            x =>
                            {
                                x.ToTable("Firefly");
                                x.Property<int>("Id");
                                x.Property<string>("Name");
                                x.HasOne("Firefly", null).WithOne().HasForeignKey("FireflyDetails", "Id");
                                x.HasData(
                                    new { Id = 42, Name = "2" });
                            });
                    },
                    _ => { },
                    _ => { },
                    upOps => { },
                    downOps => { },
                    _ => { },
                    enableSensitiveLogging: enableSensitiveLogging)).Message);

    [ConditionalFact]
    public void Add_column_with_order()
        => Execute(
            source => source.Entity("Peacock").Property<int>("Id"),
            target => target.Entity(
                "Peacock",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<string>("Name")
                        .HasColumnOrder(1);
                }),
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<AddColumnOperation>(operations[0]);
                Assert.Equal("Peacock", operation.Table);
                Assert.Equal("Name", operation.Name);
                Assert.Equal(typeof(string), operation.ClrType);
                Assert.Equal(1, operation[RelationalAnnotationNames.ColumnOrder]);
            });

    [ConditionalFact]
    public void Add_seed_data_with_non_writable_column_insert_operations_with_batching()
        => Execute(
            _ => { },
            source => source.Entity(
                "Firefly",
                x =>
                {
                    x.ToTable("Firefly", "dbo");
                    x.Property<int>("Id");
                    x.Property<string>("Name").HasColumnType("nvarchar(30)");
                    x.Property<byte[]>("Version").IsRowVersion();
                }),
            target => target.Entity(
                "Firefly",
                x =>
                {
                    x.ToTable("Firefly", "dbo");
                    x.Property<int>("Id");
                    x.Property<string>("Name").HasColumnType("nvarchar(30)");
                    x.Property<byte[]>("Version").IsRowVersion();
                    x.HasData(
                        new { Id = 42, Name = "Firefly 1" },
                        new { Id = 43, Name = "Firefly 2" },
                        new { Id = 44, Name = "Firefly 3" },
                        new { Id = 45, Name = "Firefly 4" });
                }),
            upOps => Assert.Collection(
                upOps,
                o =>
                {
                    var m = Assert.IsType<InsertDataOperation>(o);
                    Assert.Collection(
                        ToJaggedArray(m.Values),
                        r => Assert.Collection(
                            r,
                            v => Assert.Equal(42, v),
                            v => Assert.Equal("Firefly 1", v)),
                        r => Assert.Collection(
                            r,
                            v => Assert.Equal(43, v),
                            v => Assert.Equal("Firefly 2", v)),
                        r => Assert.Collection(
                            r,
                            v => Assert.Equal(44, v),
                            v => Assert.Equal("Firefly 3", v)),
                        r => Assert.Collection(
                            r,
                            v => Assert.Equal(45, v),
                            v => Assert.Equal("Firefly 4", v))
                    );
                }),
            downOps => Assert.Collection(
                downOps,
                o =>
                {
                    var m = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Null(m.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        m.KeyValues,
                        v => Assert.Equal(42, v));
                },
                o =>
                {
                    var m = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Null(m.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        m.KeyValues,
                        v => Assert.Equal(43, v));
                },
                o =>
                {
                    var m = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Null(m.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        m.KeyValues,
                        v => Assert.Equal(44, v));
                },
                o =>
                {
                    var m = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Null(m.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        m.KeyValues,
                        v => Assert.Equal(45, v));
                }),
            builderOptions => builderOptions.UseFakeRelational(a => a.MaxBatchSize(4)));

    private enum SomeEnum
    {
        Default,
        NonDefault
    }

    [ConditionalFact]
    public void Rename_column()
        => Execute(
            source => source.Entity(
                "Zebra",
                x =>
                {
                    x.ToTable("Zebra", "dbo");
                    x.Property<int>("Id");
                    x.Property<string>("Name").HasColumnType("nvarchar(30)");
                }),
            target => target.Entity(
                "Zebra",
                x =>
                {
                    x.ToTable("Zebra", "dbo");
                    x.Property<int>("Id");
                    x.Property<string>("Name").HasColumnName("ZebraName").HasColumnType("nvarchar(30)");
                }),
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<RenameColumnOperation>(operations[0]);
                Assert.Equal("dbo", operation.Schema);
                Assert.Equal("Zebra", operation.Table);
                Assert.Equal("Name", operation.Name);
                Assert.Equal("ZebraName", operation.NewName);
            });

    [ConditionalFact]
    public void Rename_column_with_seed_data()
        => Execute(
            _ => { },
            source => source.Entity(
                "EntityWithTwoProperties",
                x =>
                {
                    x.Property<int>("IdBeforeRename");
                    x.HasKey("IdBeforeRename");
                    x.Property<int>("Value1");
                    x.Property<string>("Value2");
                    x.HasData(
                        new
                        {
                            IdBeforeRename = 42,
                            Value1 = 32,
                            Value2 = "equal"
                        },
                        new
                        {
                            IdBeforeRename = 24,
                            Value1 = 72,
                            Value2 = "not equal1"
                        });
                }),
            target => target.Entity(
                "EntityWithTwoProperties",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<int>("Value1");
                    x.Property<string>("Value2");
                    x.HasData(
                        new
                        {
                            Id = 42,
                            Value1 = 27,
                            Value2 = "equal"
                        }, // modified
                        new
                        {
                            Id = 24,
                            Value1 = 99,
                            Value2 = "not equal2"
                        }); // modified
                }),
            upOps => Assert.Collection(
                upOps,
                o =>
                {
                    var operation = Assert.IsType<RenameColumnOperation>(o);
                    Assert.Null(operation.Schema);
                    Assert.Equal("EntityWithTwoProperties", operation.Table);
                    Assert.Equal("IdBeforeRename", operation.Name);
                    Assert.Equal("Id", operation.NewName);
                },
                o =>
                {
                    var m = Assert.IsType<UpdateDataOperation>(o);
                    AssertMultidimensionalArray(
                        m.KeyValues,
                        v => Assert.Equal(24, v));
                    AssertMultidimensionalArray(
                        m.Values,
                        v => Assert.Equal(99, v),
                        v => Assert.Equal("not equal2", v));
                },
                o =>
                {
                    var m = Assert.IsType<UpdateDataOperation>(o);
                    AssertMultidimensionalArray(
                        m.KeyValues,
                        v => Assert.Equal(42, v));
                    AssertMultidimensionalArray(
                        m.Values,
                        v => Assert.Equal(27, v));
                }),
            downOps => Assert.Collection(
                downOps,
                o =>
                {
                    var operation = Assert.IsType<RenameColumnOperation>(o);
                    Assert.Null(operation.Schema);
                    Assert.Equal("EntityWithTwoProperties", operation.Table);
                    Assert.Equal("Id", operation.Name);
                    Assert.Equal("IdBeforeRename", operation.NewName);
                },
                o =>
                {
                    var m = Assert.IsType<UpdateDataOperation>(o);
                    AssertMultidimensionalArray(
                        m.KeyValues,
                        v => Assert.Equal(24, v));
                    AssertMultidimensionalArray(
                        m.Values,
                        v => Assert.Equal(72, v),
                        v => Assert.Equal("not equal1", v));
                },
                o =>
                {
                    var m = Assert.IsType<UpdateDataOperation>(o);
                    AssertMultidimensionalArray(
                        m.KeyValues,
                        v => Assert.Equal(42, v));
                    AssertMultidimensionalArray(
                        m.Values,
                        v => Assert.Equal(32, v));
                }));

    [ConditionalFact]
    public void Rename_property()
        => Execute(
            source => source.Entity(
                "Buffalo",
                x =>
                {
                    x.ToTable("Buffalo", "dbo");
                    x.Property<int>("Id");
                    x.Property<string>("BuffaloName").HasColumnType("nvarchar(30)");
                }),
            target => target.Entity(
                "Buffalo",
                x =>
                {
                    x.ToTable("Buffalo", "dbo");
                    x.Property<int>("Id");
                    x.Property<string>("Name").HasColumnName("BuffaloName").HasColumnType("nvarchar(30)");
                }),
            Assert.Empty);

    [ConditionalFact]
    public void Rename_property_with_same_seed_data()
        => Execute(
            _ => { },
            target => target.Entity(
                "Zebra",
                x =>
                {
                    x.ToTable("Zebra", "dbo");
                    x.Property<int>("Id");
                    x.Property<string>("ZebraName").HasColumnType("nvarchar(30)");
                    x.HasData(
                        new { Id = 42, ZebraName = "equal" });
                }),
            source => source.Entity(
                "Zebra",
                x =>
                {
                    x.ToTable("Zebra", "dbo");
                    x.Property<int>("Id");
                    x.Property<string>("Name").HasColumnName("ZebraName").HasColumnType("nvarchar(30)");
                    x.HasData(
                        new { Id = 42, Name = "equal" });
                }),
            Assert.Empty,
            Assert.Empty);

    [ConditionalFact]
    public void Rename_property_and_column()
        => Execute(
            source => source.Entity("Buffalo").Property<int>("BuffaloId"),
            target => target.Entity("Buffalo").Property<int>("Id"),
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<RenameColumnOperation>(operations[0]);
                Assert.Equal("Buffalo", operation.Table);
                Assert.Equal("BuffaloId", operation.Name);
                Assert.Equal("Id", operation.NewName);
            });

    [ConditionalFact]
    public void Rename_property_and_column_when_snapshot()
        => Execute(
            source => source.Entity(
                typeof(Crab).FullName,
                x =>
                {
                    x.ToTable("Crab");

                    x.Property<string>("CrabId");

                    x.HasKey("CrabId");
                }),
            target => target.Entity<Crab>(),
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<RenameColumnOperation>(operations[0]);
                Assert.Equal("Crab", operation.Table);
                Assert.Equal("CrabId", operation.Name);
                Assert.Equal("Id", operation.NewName);
            },
            skipSourceConventions: true);

    [ConditionalFact]
    public void Add_table_sharing_to_TPT()
        => Execute(
            common =>
            {
                common.Entity(
                    "Order",
                    x =>
                    {
                        x.ToTable("Order");
                        x.Property<int>("Id");
                    });
                common.Entity(
                    "DetailedOrder",
                    x =>
                    {
                        x.ToTable("DetailedOrder");
                        x.HasBaseType("Order");
                        x.Property<string>("Description").HasColumnName("Description");
                    });
            },
            _ => { },
            target =>
            {
                target.Entity(
                    "OrderDetails",
                    x =>
                    {
                        x.ToTable("DetailedOrder");
                        x.Property<int>("Id");
                        x.Property<string>("Description").HasColumnName("Description");
                        x.Property<DateTime>("OrderDate");
                        x.HasOne("DetailedOrder", null).WithOne().HasForeignKey("OrderDetails", "Id");
                    });
            },
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<AddColumnOperation>(operations[0]);
                Assert.Null(operation.Schema);
                Assert.Equal("DetailedOrder", operation.Table);
                Assert.Equal("OrderDate", operation.Name);
            });

    [ConditionalFact]
    public void Add_table_sharing_to_TPC()
        => Execute(
            common =>
            {
                common.Entity(
                    "Order",
                    x =>
                    {
                        x.UseTpcMappingStrategy();
                        x.ToTable("Order");
                        x.Property<int>("Id");
                    });
                common.Entity(
                    "DetailedOrder",
                    x =>
                    {
                        x.ToTable("DetailedOrder");
                        x.HasBaseType("Order");
                        x.Property<string>("Description").HasColumnName("Description");
                    });
            },
            _ => { },
            target =>
            {
                target.Entity(
                    "OrderDetails",
                    x =>
                    {
                        x.ToTable("DetailedOrder");
                        x.Property<int>("Id");
                        x.Property<string>("Description").HasColumnName("Description");
                        x.Property<DateTime>("OrderDate");
                        x.HasOne("DetailedOrder", null).WithOne().HasForeignKey("OrderDetails", "Id");
                    });
            },
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<AddColumnOperation>(operations[0]);
                Assert.Null(operation.Schema);
                Assert.Equal("DetailedOrder", operation.Table);
                Assert.Equal("OrderDate", operation.Name);
            });

    [ConditionalFact]
    public void Rename_column_in_TPT_with_table_sharing_and_seed_data()
        => Execute(
            common =>
            {
                common.Entity(
                    "Order",
                    x =>
                    {
                        x.ToTable("Order");
                        x.Property<int>("Id");
                    });
                common.Entity(
                    "DetailedOrder",
                    x =>
                    {
                        x.ToTable("DetailedOrder");
                        x.HasBaseType("Order");
                        x.Property<string>("Description").HasColumnName("Description");
                        x.HasData(new { Id = 42, Description = "Order 1" });
                    });
                common.Entity(
                    "OrderDetails",
                    x =>
                    {
                        x.ToTable("DetailedOrder");
                        x.Property<int>("Id");
                        x.Property<string>("Description").HasColumnName("Description");
                        x.Property<DateTime>("OrderDate");
                        x.HasOne("DetailedOrder", null).WithOne().HasForeignKey("OrderDetails", "Id");
                        x.HasData(
                            new
                            {
                                Id = 42,
                                Description = "Order 1",
                                OrderDate = DateTime.MinValue
                            });
                    });
            },
            _ => { },
            target =>
            {
                target.Entity(
                    "DetailedOrder",
                    x =>
                    {
                        x.Property<string>("Description").HasColumnName("OrderDescription");
                    });
                target.Entity(
                    "OrderDetails",
                    x =>
                    {
                        x.Property<string>("Description").HasColumnName("OrderDescription");
                    });
            },
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<RenameColumnOperation>(operations[0]);
                Assert.Null(operation.Schema);
                Assert.Equal("DetailedOrder", operation.Table);
                Assert.Equal("Description", operation.Name);
                Assert.Equal("OrderDescription", operation.NewName);
            });

    private class Crab
    {
        public string Id { get; set; }
    }

    [ConditionalFact]
    public void Add_custom_value_generator()
        => Execute(
            source => source.Entity(
                "Toad",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<string>("Name");
                }),
            target => target.Entity(
                "Toad",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<string>("Name")
                        .HasValueGenerator<CustomValueGenerator>();
                }),
            operations => Assert.Equal(0, operations.Count));

    [ConditionalFact]
    public void Remove_custom_value_generator()
        => Execute(
            source => source.Entity(
                "Toad",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<string>("Name")
                        .HasValueGenerator<CustomValueGenerator>();
                }),
            target => target.Entity(
                "Toad",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<string>("Name");
                }),
            operations => Assert.Equal(0, operations.Count));

    private class CustomValueGenerator : ValueGenerator<string>
    {
        public override string Next(EntityEntry entry)
            => throw new NotImplementedException();

        public override bool GeneratesTemporaryValues
            => false;
    }

    [ConditionalFact]
    public void Alter_column_nullability()
        => Execute(
            source => source.Entity(
                "Bison",
                x =>
                {
                    x.ToTable("Bison", "dbo");
                    x.Property<int>("Id");
                    x.Property<string>("Name")
                        .HasColumnType("nvarchar(30)")
                        .IsRequired()
                        .HasDefaultValueSql("CreateBisonName()");
                }),
            target => target.Entity(
                "Bison",
                x =>
                {
                    x.ToTable("Bison", "dbo");
                    x.Property<int>("Id");
                    x.Property<string>("Name")
                        .HasColumnType("nvarchar(30)")
                        .IsRequired(false)
                        .HasDefaultValueSql("CreateBisonName()");
                }),
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<AlterColumnOperation>(operations[0]);
                Assert.Equal("dbo", operation.Schema);
                Assert.Equal("Bison", operation.Table);
                Assert.Equal("Name", operation.Name);
                Assert.Equal(typeof(string), operation.ClrType);
                Assert.Equal("nvarchar(30)", operation.ColumnType);
                Assert.True(operation.IsNullable);
                Assert.Null(operation.DefaultValue);
                Assert.Equal("CreateBisonName()", operation.DefaultValueSql);
            });

    [ConditionalFact]
    public void Alter_column_nullability_to_required()
        => Execute(
            source => source.Entity(
                "Bison",
                x =>
                {
                    x.ToTable("Bison", "dbo");
                    x.Property<int>("Id");
                    x.Property<string>("Name")
                        .HasColumnType("nvarchar(30)")
                        .IsRequired(false)
                        .HasDefaultValueSql("CreateBisonName()");
                }),
            target => target.Entity(
                "Bison",
                x =>
                {
                    x.ToTable("Bison", "dbo");
                    x.Property<int>("Id");
                    x.Property<string>("Name")
                        .HasColumnType("nvarchar(30)")
                        .IsRequired()
                        .HasDefaultValueSql("CreateBisonName()");
                }),
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<AlterColumnOperation>(operations[0]);
                Assert.Equal("dbo", operation.Schema);
                Assert.Equal("Bison", operation.Table);
                Assert.Equal("Name", operation.Name);
                Assert.Equal(typeof(string), operation.ClrType);
                Assert.Equal("nvarchar(30)", operation.ColumnType);
                Assert.False(operation.IsNullable);
                Assert.Equal(string.Empty, operation.DefaultValue);
                Assert.Equal("CreateBisonName()", operation.DefaultValueSql);
            });

    [ConditionalFact]
    public void Alter_column_type()
        => Execute(
            common => common.Entity(
                "Puma",
                x =>
                {
                    x.ToTable("Puma", "dbo");
                    x.Property<int>("Id");
                    x.Property<string>("Name")
                        .IsRequired()
                        .HasDefaultValueSql("CreatePumaName()");
                }),
            source => source.Entity(
                "Puma",
                x =>
                {
                    x.Property<string>("Name")
                        .HasColumnType("varchar(30)");
                }),
            target => target.Entity(
                "Puma",
                x =>
                {
                    x.Property<string>("Name")
                        .HasColumnType("varchar(450)");
                }),
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<AlterColumnOperation>(operations[0]);
                Assert.Equal("dbo", operation.Schema);
                Assert.Equal("Puma", operation.Table);
                Assert.Equal("Name", operation.Name);
                Assert.Equal(typeof(string), operation.ClrType);
                Assert.Equal("varchar(450)", operation.ColumnType);
                Assert.False(operation.IsNullable);
                Assert.Null(operation.DefaultValue);
                Assert.Equal("CreatePumaName()", operation.DefaultValueSql);
            });

    [ConditionalFact]
    public void Alter_column_type_with_seed_data()
        => Execute(
            common => common.Entity(
                "Puma",
                x =>
                {
                    x.ToTable("Puma", "dbo");
                    x.Property<int>("Id");
                }),
            source => source.Entity(
                "Puma",
                x =>
                {
                    x.Property<short>("ClawCount")
                        .HasColumnType("int");
                    x.HasData(
                        new { Id = 42, ClawCount = (short)20 });
                }),
            target => target.Entity(
                "Puma",
                x =>
                {
                    x.Property<int>("ClawCount");
                    x.HasData(
                        new { Id = 42, ClawCount = 20 });
                }),
            operations =>
            {
                Assert.Equal(2, operations.Count);
                Assert.IsType<AlterColumnOperation>(operations[0]); // Because the column type changed

                var operation = Assert.IsType<UpdateDataOperation>(operations[1]);
                Assert.Equal("dbo", operation.Schema);
                Assert.Equal("Puma", operation.Table);

                AssertMultidimensionalArray(
                    operation.KeyValues,
                    v => Assert.Equal(42, v));
                AssertMultidimensionalArray(
                    operation.Values,
                    v => Assert.Equal(20, v));
            });

    [ConditionalFact]
    public void Alter_key_column_type_with_seed_data()
        => Execute(
            _ => { },
            source => source.Entity(
                "Firefly",
                x =>
                {
                    x.ToTable("Firefly", "dbo");
                    x.Property<int>("Id");
                    x.HasData(
                        new { Id = 42 },
                        new { Id = 43 });
                }),
            target => target.Entity(
                "Firefly",
                x =>
                {
                    x.ToTable("Firefly", "dbo");
                    x.Property<string>("Id").HasColumnType("nvarchar(30)");
                    x.HasData(
                        new { Id = "42" });
                }),
            upOps => Assert.Collection(
                upOps,
                o =>
                {
                    var m = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Null(m.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        m.KeyValues,
                        v => Assert.Equal(42, v));
                },
                o =>
                {
                    var m = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Null(m.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        m.KeyValues,
                        v => Assert.Equal(43, v));
                },
                o =>
                {
                    var operation = Assert.IsType<AlterColumnOperation>(o);
                    Assert.Equal("dbo", operation.Schema);
                    Assert.Equal("Firefly", operation.Table);
                    Assert.Equal("Id", operation.Name);
                    Assert.Equal(typeof(string), operation.ClrType);
                    Assert.Equal(typeof(int), operation.OldColumn.ClrType);
                },
                o =>
                {
                    var m = Assert.IsType<InsertDataOperation>(o);
                    AssertMultidimensionalArray(
                        m.Values,
                        v => Assert.Equal("42", v));
                }),
            downOps => Assert.Collection(
                downOps,
                o =>
                {
                    var m = Assert.IsType<DeleteDataOperation>(o);
                    AssertMultidimensionalArray(
                        m.KeyValues,
                        v => Assert.Equal("42", v));
                },
                o =>
                {
                    var operation = Assert.IsType<AlterColumnOperation>(o);
                    Assert.Equal("dbo", operation.Schema);
                    Assert.Equal("Firefly", operation.Table);
                    Assert.Equal("Id", operation.Name);
                    Assert.Equal(typeof(int), operation.ClrType);
                    Assert.Equal(typeof(string), operation.OldColumn.ClrType);
                },
                o =>
                {
                    var m = Assert.IsType<InsertDataOperation>(o);
                    AssertMultidimensionalArray(
                        m.Values,
                        v => Assert.Equal(42, v),
                        v => Assert.Equal(43, v));
                }));

    [ConditionalFact]
    public void Alter_column_max_length()
        => Execute(
            source => source.Entity(
                "Toad",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<string>("Name");
                }),
            target => target.Entity(
                "Toad",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<string>("Name")
                        .HasMaxLength(30);
                }),
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<AlterColumnOperation>(operations[0]);
                Assert.Equal("Toad", operation.Table);
                Assert.Equal("Name", operation.Name);
                Assert.Equal(30, operation.MaxLength);
                Assert.True(operation.IsDestructiveChange);
            });

    [ConditionalFact]
    public void Alter_column_precision()
        => Execute(
            source => source.Entity(
                "Toad",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<decimal>("Salary");
                }),
            target => target.Entity(
                "Toad",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<decimal>("Salary")
                        .HasPrecision(10);
                }),
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<AlterColumnOperation>(operations[0]);
                Assert.Equal("Toad", operation.Table);
                Assert.Equal("Salary", operation.Name);
                Assert.Equal(10, operation.Precision);
                Assert.Null(operation.Scale);
                Assert.True(operation.IsDestructiveChange);
            });

    [ConditionalFact]
    public void Alter_column_precision_and_scale()
        => Execute(
            source => source.Entity(
                "Toad",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<decimal>("Salary");
                }),
            target => target.Entity(
                "Toad",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<decimal>("Salary")
                        .HasPrecision(17, 5);
                }),
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<AlterColumnOperation>(operations[0]);
                Assert.Equal("Toad", operation.Table);
                Assert.Equal("Salary", operation.Name);
                Assert.Equal(17, operation.Precision);
                Assert.Equal(5, operation.Scale);
                Assert.True(operation.IsDestructiveChange);
            });

    [ConditionalFact]
    public void Alter_column_unicode()
        => Execute(
            source => source.Entity(
                "Toad",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<string>("Name");
                }),
            target => target.Entity(
                "Toad",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<string>("Name")
                        .IsUnicode(false);
                }),
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<AlterColumnOperation>(operations[0]);
                Assert.Equal("Toad", operation.Table);
                Assert.Equal("Name", operation.Name);
                Assert.False(operation.IsUnicode);
                Assert.True(operation.IsDestructiveChange);
            });

    [ConditionalFact]
    public void Alter_column_fixed_length()
        => Execute(
            source => source.Entity(
                "Toad",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<string>("Name");
                }),
            target => target.Entity(
                "Toad",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<string>("Name")
                        .IsFixedLength();
                }),
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<AlterColumnOperation>(operations[0]);
                Assert.Equal("Toad", operation.Table);
                Assert.Equal("Name", operation.Name);
                Assert.True(operation.IsFixedLength);
                Assert.True(operation.IsDestructiveChange);
            });

    [ConditionalFact]
    public void Alter_column_default()
        => Execute(
            source => source.Entity(
                "Cougar",
                x =>
                {
                    x.ToTable("Cougar", "dbo");
                    x.Property<int>("Id");
                    x.Property<string>("Name")
                        .HasColumnType("nvarchar(30)")
                        .IsRequired()
                        .HasDefaultValue("Butch");
                }),
            target => target.Entity(
                "Cougar",
                x =>
                {
                    x.ToTable("Cougar", "dbo");
                    x.Property<int>("Id");
                    x.Property<string>("Name")
                        .HasColumnType("nvarchar(30)")
                        .IsRequired()
                        .HasDefaultValue("Cosmo");
                }),
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<AlterColumnOperation>(operations[0]);
                Assert.Equal("dbo", operation.Schema);
                Assert.Equal("Cougar", operation.Table);
                Assert.Equal("Name", operation.Name);
                Assert.Equal(typeof(string), operation.ClrType);
                Assert.Equal("nvarchar(30)", operation.ColumnType);
                Assert.False(operation.IsNullable);
                Assert.Equal("Cosmo", operation.DefaultValue);
                Assert.Null(operation.DefaultValueSql);
            });

    [ConditionalFact]
    public void No_alter_column_default_when_references_not_equal()
        => Execute(
            source => source.Entity(
                "Snake",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<byte[]>("Bytes")
                        .HasDefaultValue(new byte[] { 0 });
                }),
            target => target.Entity(
                "Snake",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<byte[]>("Bytes")
                        .HasDefaultValue(new byte[] { 0 });
                }),
            Assert.Empty);

    [ConditionalFact]
    public void Alter_column_default_expression()
        => Execute(
            source => source.Entity(
                "MountainLion",
                x =>
                {
                    x.ToTable("MountainLion", "dbo");
                    x.Property<int>("Id");
                    x.Property<string>("Name")
                        .HasColumnType("nvarchar(30)")
                        .IsRequired()
                        .HasDefaultValueSql("CreateMountainLionName()");
                }),
            target => target.Entity(
                "MountainLion",
                x =>
                {
                    x.ToTable("MountainLion", "dbo");
                    x.Property<int>("Id");
                    x.Property<string>("Name")
                        .HasColumnType("nvarchar(30)")
                        .IsRequired()
                        .HasDefaultValueSql("CreateCatamountName()");
                }),
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<AlterColumnOperation>(operations[0]);
                Assert.Equal("dbo", operation.Schema);
                Assert.Equal("MountainLion", operation.Table);
                Assert.Equal("Name", operation.Name);
                Assert.Equal(typeof(string), operation.ClrType);
                Assert.Equal("nvarchar(30)", operation.ColumnType);
                Assert.False(operation.IsNullable);
                Assert.Null(operation.DefaultValue);
                Assert.Equal("CreateCatamountName()", operation.DefaultValueSql);
            });

    [ConditionalFact]
    public void Alter_column_computed_expression()
        => Execute(
            source => source.Entity(
                "MountainLion",
                x =>
                {
                    x.ToTable("MountainLion", "dbo");
                    x.Property<int>("Id");
                    x.Property<string>("Name")
                        .HasColumnType("nvarchar(30)")
                        .IsRequired()
                        .HasComputedColumnSql("CreateMountainLionName()");
                }),
            target => target.Entity(
                "MountainLion",
                x =>
                {
                    x.ToTable("MountainLion", "dbo");
                    x.Property<int>("Id");
                    x.Property<string>("Name")
                        .HasColumnType("nvarchar(30)")
                        .IsRequired()
                        .HasComputedColumnSql("CreateCatamountName()");
                }),
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<AlterColumnOperation>(operations[0]);
                Assert.Equal("dbo", operation.Schema);
                Assert.Equal("MountainLion", operation.Table);
                Assert.Equal("Name", operation.Name);
                Assert.Equal(typeof(string), operation.ClrType);
                Assert.Equal("nvarchar(30)", operation.ColumnType);
                Assert.False(operation.IsNullable);
                Assert.Null(operation.DefaultValue);
                Assert.Equal("CreateCatamountName()", operation.ComputedColumnSql);
            });

    [ConditionalFact]
    public void Alter_column_comment()
        => Execute(
            source => source.Entity(
                "MountainLion",
                x =>
                {
                    x.ToTable("MountainLion", "dbo");
                    x.Property<int>("Id");
                    x.Property<string>("Name")
                        .HasColumnType("nvarchar(30)")
                        .HasComment("Old comment");
                }),
            target => target.Entity(
                "MountainLion",
                x =>
                {
                    x.ToTable("MountainLion", "dbo");
                    x.Property<int>("Id");
                    x.Property<string>("Name")
                        .HasColumnType("nvarchar(30)")
                        .HasComment("New comment");
                }),
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<AlterColumnOperation>(operations[0]);
                Assert.Equal("dbo", operation.Schema);
                Assert.Equal("MountainLion", operation.Table);
                Assert.Equal("Name", operation.Name);
                Assert.Equal(typeof(string), operation.ClrType);
                Assert.Equal("nvarchar(30)", operation.ColumnType);
                Assert.Equal("New comment", operation.Comment);
                Assert.Equal("Old comment", operation.OldColumn.Comment);
            });

    [ConditionalFact]
    public void Alter_column_order()
        => Execute(
            source => source.Entity(
                "Pangolin",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<string>("Name")
                        .HasColumnOrder(1);
                }),
            target => target.Entity(
                "Pangolin",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<string>("Name")
                        .HasColumnOrder(2);
                }),
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<AlterColumnOperation>(operations[0]);
                Assert.Equal("Pangolin", operation.Table);
                Assert.Equal("Name", operation.Name);
                Assert.Equal(2, operation[RelationalAnnotationNames.ColumnOrder]);
                Assert.Equal(1, operation.OldColumn[RelationalAnnotationNames.ColumnOrder]);
            });

    [ConditionalFact]
    public void Alter_column_but_not_order()
        => Execute(
            source => source.Entity(
                "Crane",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<string>("Name")
                        .HasColumnOrder(1);
                }),
            target => target.Entity(
                "Crane",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<string>("Name")
                        .HasColumnOrder(1)
                        .IsUnicode(false);
                }),
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<AlterColumnOperation>(operations[0]);
                Assert.Equal("Crane", operation.Table);
                Assert.Equal("Name", operation.Name);
                Assert.Null(operation.FindAnnotation(RelationalAnnotationNames.ColumnOrder));
                Assert.Null(operation.OldColumn.FindAnnotation(RelationalAnnotationNames.ColumnOrder));
            });

    [ConditionalFact]
    public void Add_unique_constraint()
        => Execute(
            source => source.Entity(
                "Flamingo",
                x =>
                {
                    x.ToTable("Flamingo", "dbo");
                    x.Property<int>("Id");
                    x.Property<int>("AlternateId");
                }),
            target => target.Entity(
                "Flamingo",
                x =>
                {
                    x.ToTable("Flamingo", "dbo");
                    x.Property<int>("Id");
                    x.Property<int>("AlternateId");
                    x.HasAlternateKey("AlternateId");
                }),
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<AddUniqueConstraintOperation>(operations[0]);
                Assert.Equal("dbo", operation.Schema);
                Assert.Equal("Flamingo", operation.Table);
                Assert.Equal("AK_Flamingo_AlternateId", operation.Name);
                Assert.Equal(new[] { "AlternateId" }, operation.Columns);
            });

    [ConditionalFact]
    public void Drop_unique_constraint()
        => Execute(
            source => source.Entity(
                "Penguin",
                x =>
                {
                    x.ToTable("Penguin", "dbo");
                    x.Property<int>("Id");
                    x.Property<int>("AlternateId");
                    x.HasAlternateKey("AlternateId");
                }),
            target => target.Entity(
                "Penguin",
                x =>
                {
                    x.ToTable("Penguin", "dbo");
                    x.Property<int>("Id");
                    x.Property<int>("AlternateId");
                }),
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<DropUniqueConstraintOperation>(operations[0]);
                Assert.Equal("dbo", operation.Schema);
                Assert.Equal("Penguin", operation.Table);
                Assert.Equal("AK_Penguin_AlternateId", operation.Name);
            });

    [ConditionalFact]
    public void Rename_unique_constraint()
        => Execute(
            source => source.Entity(
                "Pelican",
                x =>
                {
                    x.ToTable("Pelican", "dbo");
                    x.Property<int>("Id");
                    x.Property<int>("AlternateId");
                    x.HasAlternateKey("AlternateId");
                }),
            target => target.Entity(
                "Pelican",
                x =>
                {
                    x.ToTable("Pelican", "dbo");
                    x.Property<int>("Id");
                    x.Property<int>("AlternateId");
                    x.HasAlternateKey("AlternateId").HasName("AK_dbo.Pelican_AlternateId");
                }),
            operations =>
            {
                Assert.Equal(2, operations.Count);

                var dropOperation = Assert.IsType<DropUniqueConstraintOperation>(operations[0]);
                Assert.Equal("dbo", dropOperation.Schema);
                Assert.Equal("Pelican", dropOperation.Table);
                Assert.Equal("AK_Pelican_AlternateId", dropOperation.Name);

                var addOperation = Assert.IsType<AddUniqueConstraintOperation>(operations[1]);
                Assert.Equal("dbo", addOperation.Schema);
                Assert.Equal("Pelican", addOperation.Table);
                Assert.Equal("AK_dbo.Pelican_AlternateId", addOperation.Name);
                Assert.Equal(new[] { "AlternateId" }, addOperation.Columns);
            });

    [ConditionalFact]
    public void Alter_unique_constraint_columns()
        => Execute(
            source => source.Entity(
                "Rook",
                x =>
                {
                    x.ToTable("Rook", "dbo");
                    x.Property<int>("Id");
                    x.Property<int>("AlternateId");
                    x.HasAlternateKey("AlternateId");
                    x.Property<int>("AlternateRookId");
                }),
            target => target.Entity(
                "Rook",
                x =>
                {
                    x.ToTable("Rook", "dbo");
                    x.Property<int>("Id");
                    x.Property<int>("AlternateId");
                    x.Property<int>("AlternateRookId");
                    x.HasAlternateKey("AlternateRookId").HasName("AK_Rook_AlternateId");
                }),
            operations =>
            {
                Assert.Equal(2, operations.Count);

                var dropOperation = Assert.IsType<DropUniqueConstraintOperation>(operations[0]);
                Assert.Equal("dbo", dropOperation.Schema);
                Assert.Equal("Rook", dropOperation.Table);
                Assert.Equal("AK_Rook_AlternateId", dropOperation.Name);

                var addOperation = Assert.IsType<AddUniqueConstraintOperation>(operations[1]);
                Assert.Equal("dbo", addOperation.Schema);
                Assert.Equal("Rook", addOperation.Table);
                Assert.Equal("AK_Rook_AlternateId", addOperation.Name);
                Assert.Equal(new[] { "AlternateRookId" }, addOperation.Columns);
            });

    [ConditionalFact]
    public void Add_check_constraint()
        => Execute(
            source => source.Entity(
                "Flamingo",
                x =>
                {
                    x.ToTable("Flamingo", "dbo");
                    x.Property<int>("Id");
                    x.Property<int>("AlternateId");
                }),
            target => target.Entity(
                "Flamingo",
                x =>
                {
                    x.ToTable("Flamingo", "dbo", tb => tb.HasCheckConstraint("CK_Flamingo_AlternateId", "AlternateId > Id"));
                    x.Property<int>("Id");
                    x.Property<int>("AlternateId");
                }),
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<AddCheckConstraintOperation>(operations[0]);
                Assert.Equal("dbo", operation.Schema);
                Assert.Equal("Flamingo", operation.Table);
                Assert.Equal("CK_Flamingo_AlternateId", operation.Name);
                Assert.Equal("AlternateId > Id", operation.Sql);
            });

    [ConditionalFact]
    public void Drop_check_constraint()
        => Execute(
            source => source.Entity(
                "Penguin",
                x =>
                {
                    x.ToTable("Penguin", "dbo", tb => tb.HasCheckConstraint("CK_Penguin_AlternateId", "AlternateId > Id"));
                    x.Property<int>("Id");
                    x.Property<int>("AlternateId");
                }),
            target => target.Entity(
                "Penguin",
                x =>
                {
                    x.ToTable("Penguin", "dbo");
                    x.Property<int>("Id");
                    x.Property<int>("AlternateId");
                }),
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<DropCheckConstraintOperation>(operations[0]);
                Assert.Equal("dbo", operation.Schema);
                Assert.Equal("Penguin", operation.Table);
                Assert.Equal("CK_Penguin_AlternateId", operation.Name);
            });

    [ConditionalFact]
    public void Rename_check_constraint()
        => Execute(
            source => source.Entity(
                "Pelican",
                x =>
                {
                    x.ToTable("Pelican", "dbo", tb => tb.HasCheckConstraint("CK_Pelican_AlternateId", "AlternateId > Id"));
                    x.Property<int>("Id");
                    x.Property<int>("AlternateId");
                }),
            target => target.Entity(
                "Pelican",
                x =>
                {
                    x.ToTable(
                        "Pelican", "dbo", tb => tb.HasCheckConstraint("CK_Pelican_AlternateId", "AlternateId > Id").HasName("CK_Flamingo"));
                    x.Property<int>("Id");
                    x.Property<int>("AlternateId");
                }),
            operations =>
            {
                Assert.Equal(2, operations.Count);

                var dropOperation = Assert.IsType<DropCheckConstraintOperation>(operations[0]);
                Assert.Equal("dbo", dropOperation.Schema);
                Assert.Equal("Pelican", dropOperation.Table);
                Assert.Equal("CK_Pelican_AlternateId", dropOperation.Name);

                var createOperation = Assert.IsType<AddCheckConstraintOperation>(operations[1]);
                Assert.Equal("dbo", createOperation.Schema);
                Assert.Equal("Pelican", createOperation.Table);
                Assert.Equal("CK_Flamingo", createOperation.Name);
                Assert.Equal("AlternateId > Id", createOperation.Sql);
            });

    [ConditionalFact]
    public void Alter_check_constraint_expression()
        => Execute(
            source => source.Entity(
                "Rook",
                x =>
                {
                    x.ToTable("Rook", "dbo", tb => tb.HasCheckConstraint("CK_Rook_AlternateId", "AlternateId > Id"));
                    x.Property<int>("Id");
                    x.Property<int>("AlternateId");
                }),
            target => target.Entity(
                "Rook",
                x =>
                {
                    x.ToTable("Rook", "dbo", tb => tb.HasCheckConstraint("CK_Rook_AlternateId", "AlternateId < Id"));
                    x.Property<int>("Id");
                    x.Property<int>("AlternateId");
                }),
            operations =>
            {
                Assert.Equal(2, operations.Count);

                var dropOperation = Assert.IsType<DropCheckConstraintOperation>(operations[0]);
                Assert.Equal("dbo", dropOperation.Schema);
                Assert.Equal("Rook", dropOperation.Table);
                Assert.Equal("CK_Rook_AlternateId", dropOperation.Name);

                var createOperation = Assert.IsType<AddCheckConstraintOperation>(operations[1]);
                Assert.Equal("dbo", createOperation.Schema);
                Assert.Equal("Rook", createOperation.Table);
                Assert.Equal("CK_Rook_AlternateId", createOperation.Name);
                Assert.Equal("AlternateId < Id", createOperation.Sql);
            });

    [ConditionalFact]
    public void Add_primary_key()
        => Execute(
            _ => { },
            source => source.Entity("Puffin").ToTable("Puffin", "dbo").HasNoKey().Property<int>("Id"),
            target => target.Entity("Puffin").ToTable("Puffin", "dbo").Property<int>("Id"),
            upOps =>
            {
                Assert.Equal(1, upOps.Count);

                var operation = Assert.IsType<AddPrimaryKeyOperation>(upOps[0]);
                Assert.Equal("dbo", operation.Schema);
                Assert.Equal("Puffin", operation.Table);
                Assert.Equal("PK_Puffin", operation.Name);
                Assert.Equal(new[] { "Id" }, operation.Columns);
            },
            downOps =>
            {
                Assert.Equal(1, downOps.Count);

                var operation = Assert.IsType<DropPrimaryKeyOperation>(downOps[0]);
                Assert.Equal("dbo", operation.Schema);
                Assert.Equal("Puffin", operation.Table);
                Assert.Equal("PK_Puffin", operation.Name);
            },
            skipSourceConventions: true);

    [ConditionalFact]
    public void Rename_primary_key()
        => Execute(
            source => source.Entity("Puffin").ToTable("Puffin", "dbo").Property<int>("Id"),
            target => target.Entity(
                "Puffin",
                x =>
                {
                    x.ToTable("Puffin", "dbo");
                    x.Property<int>("Id");
                    x.HasKey("Id").HasName("PK_dbo.Puffin");
                }),
            operations =>
            {
                Assert.Equal(2, operations.Count);

                var dropOperation = Assert.IsType<DropPrimaryKeyOperation>(operations[0]);
                Assert.Equal("dbo", dropOperation.Schema);
                Assert.Equal("Puffin", dropOperation.Table);
                Assert.Equal("PK_Puffin", dropOperation.Name);

                var addOperation = Assert.IsType<AddPrimaryKeyOperation>(operations[1]);
                Assert.Equal("dbo", addOperation.Schema);
                Assert.Equal("Puffin", addOperation.Table);
                Assert.Equal("PK_dbo.Puffin", addOperation.Name);
                Assert.Equal(new[] { "Id" }, addOperation.Columns);
            });

    [ConditionalFact]
    public void Alter_primary_key_columns()
        => Execute(
            source => source.Entity(
                "Raven",
                x =>
                {
                    x.ToTable("Raven", "dbo");
                    x.Property<int>("Id");
                    x.HasKey("Id");
                    x.Property<int>("RavenId");
                }),
            target => target.Entity(
                "Raven",
                x =>
                {
                    x.ToTable("Raven", "dbo");
                    x.Property<int>("Id");
                    x.Property<int>("RavenId");
                    x.HasKey("RavenId");
                }),
            operations =>
            {
                Assert.Equal(2, operations.Count);

                var dropOperation = Assert.IsType<DropPrimaryKeyOperation>(operations[0]);
                Assert.Equal("dbo", dropOperation.Schema);
                Assert.Equal("Raven", dropOperation.Table);
                Assert.Equal("PK_Raven", dropOperation.Name);

                var addOperation = Assert.IsType<AddPrimaryKeyOperation>(operations[1]);
                Assert.Equal("dbo", addOperation.Schema);
                Assert.Equal("Raven", addOperation.Table);
                Assert.Equal("PK_Raven", addOperation.Name);
                Assert.Equal(new[] { "RavenId" }, addOperation.Columns);
            },
            skipSourceConventions: true);

    [ConditionalFact]
    public void Alter_primary_key_column_order_with_seed_data()
        => Execute(
            common => common.Entity(
                "Raven",
                x =>
                {
                    x.ToTable("Raven", "dbo");
                    x.Property<int>("Id");
                    x.Property<string>("RavenId");
                    x.HasData(
                        new { Id = 42, RavenId = "42" });
                }),
            source => source.Entity(
                "Raven",
                x => x.HasKey("Id", "RavenId")),
            target => target.Entity(
                "Raven",
                x => x.HasKey("RavenId", "Id")),
            operations =>
            {
                Assert.Equal(4, operations.Count);

                var dropOperation = Assert.IsType<DropPrimaryKeyOperation>(operations[0]);
                Assert.Equal("dbo", dropOperation.Schema);
                Assert.Equal("Raven", dropOperation.Table);
                Assert.Equal("PK_Raven", dropOperation.Name);

                var deleteDataOperation = Assert.IsType<DeleteDataOperation>(operations[1]);
                Assert.Null(deleteDataOperation.KeyColumnTypes);
                Assert.Equal(new[] { "Id", "RavenId" }, deleteDataOperation.KeyColumns);
                AssertMultidimensionalArray(
                    deleteDataOperation.KeyValues,
                    v => Assert.Equal(42, v),
                    v => Assert.Equal("42", v));

                var addOperation = Assert.IsType<AddPrimaryKeyOperation>(operations[2]);
                Assert.Equal("dbo", addOperation.Schema);
                Assert.Equal("Raven", addOperation.Table);
                Assert.Equal("PK_Raven", addOperation.Name);
                Assert.Equal(new[] { "RavenId", "Id" }, addOperation.Columns);

                var insertDataOperation = Assert.IsType<InsertDataOperation>(operations[3]);
                Assert.Equal("Raven", insertDataOperation.Table);
                Assert.Equal(new[] { "Id", "RavenId" }, insertDataOperation.Columns);
                AssertMultidimensionalArray(
                    insertDataOperation.Values,
                    v => Assert.Equal(42, v),
                    v => Assert.Equal("42", v));
            },
            skipSourceConventions: true);

    [ConditionalFact]
    public void Add_foreign_key()
        => Execute(
            common => common.Entity(
                "Amoeba",
                x =>
                {
                    x.ToTable("Amoeba", "dbo");
                    x.Property<int>("Id");
                    x.Property<int>("ParentId");
                }),
            _ => { },
            target => target.Entity(
                "Amoeba",
                x => x.HasOne("Amoeba").WithMany().HasForeignKey("ParentId")
            ),
            operations =>
            {
                Assert.Equal(2, operations.Count);

                var createIndexOperation = Assert.IsType<CreateIndexOperation>(operations[0]);
                Assert.Equal("dbo", createIndexOperation.Schema);
                Assert.Equal("Amoeba", createIndexOperation.Table);
                Assert.Equal("IX_Amoeba_ParentId", createIndexOperation.Name);
                Assert.Equal(new[] { "ParentId" }, createIndexOperation.Columns);

                var addFkOperation = Assert.IsType<AddForeignKeyOperation>(operations[1]);
                Assert.Equal("dbo", addFkOperation.Schema);
                Assert.Equal("Amoeba", addFkOperation.Table);
                Assert.Equal("FK_Amoeba_Amoeba_ParentId", addFkOperation.Name);
                Assert.Equal(new[] { "ParentId" }, addFkOperation.Columns);
                Assert.Equal("dbo", addFkOperation.PrincipalSchema);
                Assert.Equal("Amoeba", addFkOperation.PrincipalTable);
                Assert.Equal(new[] { "Id" }, addFkOperation.PrincipalColumns);
                Assert.Equal(ReferentialAction.Cascade, addFkOperation.OnDelete);
                Assert.Equal(ReferentialAction.NoAction, addFkOperation.OnUpdate);
            });

    [ConditionalFact]
    public void Add_optional_foreign_key()
        => Execute(
            source => source.Entity(
                "Amoeba",
                x =>
                {
                    x.ToTable("Amoeba", "dbo");
                    x.Property<int>("Id");
                    x.Property<int?>("ParentId");
                }),
            target => target.Entity(
                "Amoeba",
                x =>
                {
                    x.ToTable("Amoeba", "dbo");
                    x.Property<int>("Id");
                    x.Property<int?>("ParentId");
                    x.HasOne("Amoeba").WithMany().HasForeignKey("ParentId");
                }),
            operations =>
            {
                Assert.Equal(2, operations.Count);

                var createIndexOperation = Assert.IsType<CreateIndexOperation>(operations[0]);
                Assert.Equal("dbo", createIndexOperation.Schema);
                Assert.Equal("Amoeba", createIndexOperation.Table);
                Assert.Equal("IX_Amoeba_ParentId", createIndexOperation.Name);
                Assert.Equal(new[] { "ParentId" }, createIndexOperation.Columns);

                var addFkOperation = Assert.IsType<AddForeignKeyOperation>(operations[1]);
                Assert.Equal("dbo", addFkOperation.Schema);
                Assert.Equal("Amoeba", addFkOperation.Table);
                Assert.Equal("FK_Amoeba_Amoeba_ParentId", addFkOperation.Name);
                Assert.Equal(new[] { "ParentId" }, addFkOperation.Columns);
                Assert.Equal("dbo", addFkOperation.PrincipalSchema);
                Assert.Equal("Amoeba", addFkOperation.PrincipalTable);
                Assert.Equal(new[] { "Id" }, addFkOperation.PrincipalColumns);
                Assert.Equal(ReferentialAction.NoAction, addFkOperation.OnDelete);
                Assert.Equal(ReferentialAction.NoAction, addFkOperation.OnUpdate);
            });

    [ConditionalFact]
    public void Add_optional_foreign_key_with_cascade_delete()
        => Execute(
            source => source.Entity(
                "Amoeba",
                x =>
                {
                    x.ToTable("Amoeba", "dbo");
                    x.Property<int>("Id");
                    x.Property<int?>("ParentId");
                }),
            target => target.Entity(
                "Amoeba",
                x =>
                {
                    x.ToTable("Amoeba", "dbo");
                    x.Property<int>("Id");
                    x.Property<int?>("ParentId");
                    x.HasOne("Amoeba").WithMany().HasForeignKey("ParentId").OnDelete(DeleteBehavior.Cascade);
                }),
            operations =>
            {
                Assert.Equal(2, operations.Count);

                var createIndexOperation = Assert.IsType<CreateIndexOperation>(operations[0]);
                Assert.Equal("dbo", createIndexOperation.Schema);
                Assert.Equal("Amoeba", createIndexOperation.Table);
                Assert.Equal("IX_Amoeba_ParentId", createIndexOperation.Name);
                Assert.Equal(new[] { "ParentId" }, createIndexOperation.Columns);

                var addFkOperation = Assert.IsType<AddForeignKeyOperation>(operations[1]);
                Assert.Equal("dbo", addFkOperation.Schema);
                Assert.Equal("Amoeba", addFkOperation.Table);
                Assert.Equal("FK_Amoeba_Amoeba_ParentId", addFkOperation.Name);
                Assert.Equal(new[] { "ParentId" }, addFkOperation.Columns);
                Assert.Equal("dbo", addFkOperation.PrincipalSchema);
                Assert.Equal("Amoeba", addFkOperation.PrincipalTable);
                Assert.Equal(new[] { "Id" }, addFkOperation.PrincipalColumns);
                Assert.Equal(ReferentialAction.Cascade, addFkOperation.OnDelete);
                Assert.Equal(ReferentialAction.NoAction, addFkOperation.OnUpdate);
            });

    [ConditionalFact]
    public void Add_required_foreign_key_with_restrict()
        => Execute(
            source => source.Entity(
                "Amoeba",
                x =>
                {
                    x.ToTable("Amoeba", "dbo");
                    x.Property<int>("Id");
                    x.Property<int>("ParentId");
                }),
            target => target.Entity(
                "Amoeba",
                x =>
                {
                    x.ToTable("Amoeba", "dbo");
                    x.Property<int>("Id");
                    x.Property<int>("ParentId");
                    x.HasOne("Amoeba").WithMany().HasForeignKey("ParentId").OnDelete(DeleteBehavior.Restrict);
                }),
            operations =>
            {
                Assert.Equal(2, operations.Count);

                var createIndexOperation = Assert.IsType<CreateIndexOperation>(operations[0]);
                Assert.Equal("dbo", createIndexOperation.Schema);
                Assert.Equal("Amoeba", createIndexOperation.Table);
                Assert.Equal("IX_Amoeba_ParentId", createIndexOperation.Name);
                Assert.Equal(new[] { "ParentId" }, createIndexOperation.Columns);

                var addFkOperation = Assert.IsType<AddForeignKeyOperation>(operations[1]);
                Assert.Equal("dbo", addFkOperation.Schema);
                Assert.Equal("Amoeba", addFkOperation.Table);
                Assert.Equal("FK_Amoeba_Amoeba_ParentId", addFkOperation.Name);
                Assert.Equal(new[] { "ParentId" }, addFkOperation.Columns);
                Assert.Equal("dbo", addFkOperation.PrincipalSchema);
                Assert.Equal("Amoeba", addFkOperation.PrincipalTable);
                Assert.Equal(new[] { "Id" }, addFkOperation.PrincipalColumns);
                Assert.Equal(ReferentialAction.Restrict, addFkOperation.OnDelete);
                Assert.Equal(ReferentialAction.NoAction, addFkOperation.OnUpdate);
            });

    [ConditionalFact]
    public void Add_required_foreign_key_with_default()
        => Execute(
            source => source.Entity(
                "Amoeba",
                x =>
                {
                    x.ToTable("Amoeba", "dbo");
                    x.Property<int>("Id");
                    x.Property<int>("ParentId");
                }),
            target => target.Entity(
                "Amoeba",
                x =>
                {
                    x.ToTable("Amoeba", "dbo");
                    x.Property<int>("Id");
                    x.Property<int>("ParentId");
                    x.HasOne("Amoeba").WithMany().HasForeignKey("ParentId").OnDelete(DeleteBehavior.ClientSetNull);
                }),
            operations =>
            {
                Assert.Equal(2, operations.Count);

                var createIndexOperation = Assert.IsType<CreateIndexOperation>(operations[0]);
                Assert.Equal("dbo", createIndexOperation.Schema);
                Assert.Equal("Amoeba", createIndexOperation.Table);
                Assert.Equal("IX_Amoeba_ParentId", createIndexOperation.Name);
                Assert.Equal(new[] { "ParentId" }, createIndexOperation.Columns);

                var addFkOperation = Assert.IsType<AddForeignKeyOperation>(operations[1]);
                Assert.Equal("dbo", addFkOperation.Schema);
                Assert.Equal("Amoeba", addFkOperation.Table);
                Assert.Equal("FK_Amoeba_Amoeba_ParentId", addFkOperation.Name);
                Assert.Equal(new[] { "ParentId" }, addFkOperation.Columns);
                Assert.Equal("dbo", addFkOperation.PrincipalSchema);
                Assert.Equal("Amoeba", addFkOperation.PrincipalTable);
                Assert.Equal(new[] { "Id" }, addFkOperation.PrincipalColumns);
                Assert.Equal(ReferentialAction.NoAction, addFkOperation.OnDelete);
                Assert.Equal(ReferentialAction.NoAction, addFkOperation.OnUpdate);
            });

    [ConditionalFact]
    public void Add_optional_foreign_key_with_set_null()
        => Execute(
            source => source.Entity(
                "Amoeba",
                x =>
                {
                    x.ToTable("Amoeba", "dbo");
                    x.Property<int>("Id");
                    x.Property<int?>("ParentId");
                }),
            target => target.Entity(
                "Amoeba",
                x =>
                {
                    x.ToTable("Amoeba", "dbo");
                    x.Property<int>("Id");
                    x.Property<int?>("ParentId");
                    x.HasOne("Amoeba").WithMany().HasForeignKey("ParentId").OnDelete(DeleteBehavior.SetNull);
                }),
            operations =>
            {
                Assert.Equal(2, operations.Count);

                var createIndexOperation = Assert.IsType<CreateIndexOperation>(operations[0]);
                Assert.Equal("dbo", createIndexOperation.Schema);
                Assert.Equal("Amoeba", createIndexOperation.Table);
                Assert.Equal("IX_Amoeba_ParentId", createIndexOperation.Name);
                Assert.Equal(new[] { "ParentId" }, createIndexOperation.Columns);

                var addFkOperation = Assert.IsType<AddForeignKeyOperation>(operations[1]);
                Assert.Equal("dbo", addFkOperation.Schema);
                Assert.Equal("Amoeba", addFkOperation.Table);
                Assert.Equal("FK_Amoeba_Amoeba_ParentId", addFkOperation.Name);
                Assert.Equal(new[] { "ParentId" }, addFkOperation.Columns);
                Assert.Equal("dbo", addFkOperation.PrincipalSchema);
                Assert.Equal("Amoeba", addFkOperation.PrincipalTable);
                Assert.Equal(new[] { "Id" }, addFkOperation.PrincipalColumns);
                Assert.Equal(ReferentialAction.SetNull, addFkOperation.OnDelete);
                Assert.Equal(ReferentialAction.NoAction, addFkOperation.OnUpdate);
            });

    [ConditionalFact]
    public void Add_optional_foreign_key_with_restrict()
        => Execute(
            source => source.Entity(
                "Amoeba",
                x =>
                {
                    x.ToTable("Amoeba", "dbo");
                    x.Property<int>("Id");
                    x.Property<int?>("ParentId");
                }),
            target => target.Entity(
                "Amoeba",
                x =>
                {
                    x.ToTable("Amoeba", "dbo");
                    x.Property<int>("Id");
                    x.Property<int?>("ParentId");
                    x.HasOne("Amoeba").WithMany().HasForeignKey("ParentId").OnDelete(DeleteBehavior.Restrict);
                }),
            operations =>
            {
                Assert.Equal(2, operations.Count);

                var createIndexOperation = Assert.IsType<CreateIndexOperation>(operations[0]);
                Assert.Equal("dbo", createIndexOperation.Schema);
                Assert.Equal("Amoeba", createIndexOperation.Table);
                Assert.Equal("IX_Amoeba_ParentId", createIndexOperation.Name);
                Assert.Equal(new[] { "ParentId" }, createIndexOperation.Columns);

                var addFkOperation = Assert.IsType<AddForeignKeyOperation>(operations[1]);
                Assert.Equal("dbo", addFkOperation.Schema);
                Assert.Equal("Amoeba", addFkOperation.Table);
                Assert.Equal("FK_Amoeba_Amoeba_ParentId", addFkOperation.Name);
                Assert.Equal(new[] { "ParentId" }, addFkOperation.Columns);
                Assert.Equal("dbo", addFkOperation.PrincipalSchema);
                Assert.Equal("Amoeba", addFkOperation.PrincipalTable);
                Assert.Equal(new[] { "Id" }, addFkOperation.PrincipalColumns);
                Assert.Equal(ReferentialAction.Restrict, addFkOperation.OnDelete);
                Assert.Equal(ReferentialAction.NoAction, addFkOperation.OnUpdate);
            });

    [ConditionalFact]
    public void Remove_foreign_key()
        => Execute(
            source => source.Entity(
                "Anemone",
                x =>
                {
                    x.ToTable("Anemone", "dbo");
                    x.Property<int>("Id");
                    x.Property<int>("ParentId");
                    x.HasOne("Anemone").WithMany().HasForeignKey("ParentId");
                }),
            target => target.Entity(
                "Anemone",
                x =>
                {
                    x.ToTable("Anemone", "dbo");
                    x.Property<int>("Id");
                    x.Property<int>("ParentId");
                }),
            operations =>
            {
                Assert.Equal(2, operations.Count);

                var dropFkOperation = Assert.IsType<DropForeignKeyOperation>(operations[0]);
                Assert.Equal("dbo", dropFkOperation.Schema);
                Assert.Equal("Anemone", dropFkOperation.Table);
                Assert.Equal("FK_Anemone_Anemone_ParentId", dropFkOperation.Name);

                var dropIndexOperation = Assert.IsType<DropIndexOperation>(operations[1]);
                Assert.Equal("dbo", dropIndexOperation.Schema);
                Assert.Equal("Anemone", dropIndexOperation.Table);
                Assert.Equal("IX_Anemone_ParentId", dropIndexOperation.Name);
            });

    [ConditionalFact]
    public void Rename_foreign_key()
        => Execute(
            source => source.Entity(
                "Nematode",
                x =>
                {
                    x.ToTable("Nematode", "dbo");
                    x.Property<int>("Id");
                    x.Property<int>("ParentId");
                    x.HasOne("Nematode").WithMany().HasForeignKey("ParentId");
                }),
            target => target.Entity(
                "Nematode",
                x =>
                {
                    x.ToTable("Nematode", "dbo");
                    x.Property<int>("Id");
                    x.Property<int>("ParentId");
                    x.HasOne("Nematode").WithMany().HasForeignKey("ParentId").HasConstraintName("FK_Nematode_NematodeParent");
                }),
            operations =>
            {
                Assert.Equal(2, operations.Count);

                var dropOperation = Assert.IsType<DropForeignKeyOperation>(operations[0]);
                Assert.Equal("dbo", dropOperation.Schema);
                Assert.Equal("Nematode", dropOperation.Table);
                Assert.Equal("FK_Nematode_Nematode_ParentId", dropOperation.Name);

                var addOperation = Assert.IsType<AddForeignKeyOperation>(operations[1]);
                Assert.Equal("dbo", addOperation.Schema);
                Assert.Equal("Nematode", addOperation.Table);
                Assert.Equal("FK_Nematode_NematodeParent", addOperation.Name);
                Assert.Equal(new[] { "ParentId" }, addOperation.Columns);
                Assert.Equal("dbo", addOperation.PrincipalSchema);
                Assert.Equal("Nematode", addOperation.PrincipalTable);
                Assert.Equal(new[] { "Id" }, addOperation.PrincipalColumns);
            });

    [ConditionalFact]
    public void Alter_foreign_key_columns()
        => Execute(
            source => source.Entity(
                "Mushroom",
                x =>
                {
                    x.ToTable("Mushroom", "dbo");
                    x.Property<int>("Id");
                    x.Property<int>("ParentId1");
                    x.HasOne("Mushroom").WithMany().HasForeignKey("ParentId1");
                    x.Property<int>("ParentId2");
                }),
            target => target.Entity(
                "Mushroom",
                x =>
                {
                    x.ToTable("Mushroom", "dbo");
                    x.Property<int>("Id");
                    x.Property<int>("ParentId1");
                    x.Property<int>("ParentId2");
                    x.HasOne("Mushroom").WithMany().HasForeignKey("ParentId2").HasConstraintName("FK_Mushroom_Mushroom_ParentId1");
                }),
            operations =>
            {
                Assert.Equal(4, operations.Count);

                var dropFkOperation = Assert.IsType<DropForeignKeyOperation>(operations[0]);
                Assert.Equal("dbo", dropFkOperation.Schema);
                Assert.Equal("Mushroom", dropFkOperation.Table);
                Assert.Equal("FK_Mushroom_Mushroom_ParentId1", dropFkOperation.Name);

                var dropIndexOperation = Assert.IsType<DropIndexOperation>(operations[1]);
                Assert.Equal("dbo", dropIndexOperation.Schema);
                Assert.Equal("Mushroom", dropIndexOperation.Table);
                Assert.Equal("IX_Mushroom_ParentId1", dropIndexOperation.Name);

                var addIndexOperation = Assert.IsType<CreateIndexOperation>(operations[2]);
                Assert.Equal("dbo", addIndexOperation.Schema);
                Assert.Equal("Mushroom", addIndexOperation.Table);
                Assert.Equal("IX_Mushroom_ParentId2", addIndexOperation.Name);
                Assert.Equal(new[] { "ParentId2" }, addIndexOperation.Columns);

                var addFkOperation = Assert.IsType<AddForeignKeyOperation>(operations[3]);
                Assert.Equal("dbo", addFkOperation.Schema);
                Assert.Equal("Mushroom", addFkOperation.Table);
                Assert.Equal("FK_Mushroom_Mushroom_ParentId1", addFkOperation.Name);
                Assert.Equal(new[] { "ParentId2" }, addFkOperation.Columns);
                Assert.Equal("dbo", addFkOperation.PrincipalSchema);
                Assert.Equal("Mushroom", addFkOperation.PrincipalTable);
                Assert.Equal(new[] { "Id" }, addFkOperation.PrincipalColumns);
            });

    [ConditionalFact]
    public void Alter_foreign_key_cascade_delete()
        => Execute(
            source => source.Entity(
                "Mushroom",
                x =>
                {
                    x.ToTable("Mushroom", "dbo");
                    x.Property<int>("Id");
                    x.Property<int>("ParentId1");
                    x.HasOne("Mushroom").WithMany().HasForeignKey("ParentId1").OnDelete(DeleteBehavior.Restrict);
                    x.Property<int>("ParentId2");
                }),
            target => target.Entity(
                "Mushroom",
                x =>
                {
                    x.ToTable("Mushroom", "dbo");
                    x.Property<int>("Id");
                    x.Property<int>("ParentId1");
                    x.HasOne("Mushroom").WithMany().HasForeignKey("ParentId1").OnDelete(DeleteBehavior.Cascade);
                    x.Property<int>("ParentId2");
                }),
            operations =>
            {
                Assert.Equal(2, operations.Count);

                var dropOperation = Assert.IsType<DropForeignKeyOperation>(operations[0]);
                Assert.Equal("dbo", dropOperation.Schema);
                Assert.Equal("Mushroom", dropOperation.Table);
                Assert.Equal("FK_Mushroom_Mushroom_ParentId1", dropOperation.Name);

                var addOperation = Assert.IsType<AddForeignKeyOperation>(operations[1]);
                Assert.Equal("dbo", addOperation.Schema);
                Assert.Equal("Mushroom", addOperation.Table);
                Assert.Equal("FK_Mushroom_Mushroom_ParentId1", addOperation.Name);
                Assert.Equal(new[] { "ParentId1" }, addOperation.Columns);
                Assert.Equal("dbo", addOperation.PrincipalSchema);
                Assert.Equal("Mushroom", addOperation.PrincipalTable);
                Assert.Equal(new[] { "Id" }, addOperation.PrincipalColumns);
                Assert.Equal(ReferentialAction.Cascade, addOperation.OnDelete);
                Assert.Equal(ReferentialAction.NoAction, addOperation.OnUpdate);
            });

    [ConditionalFact]
    public void Alter_foreign_key_on_delete_from_ClientSetNull_to_NoAction()
        => Execute(
            source => source.Entity(
                "Mushroom",
                x =>
                {
                    x.ToTable("Mushroom", "dbo");
                    x.Property<int>("Id");
                    x.Property<int>("ParentId1");
                    x.HasOne("Mushroom").WithMany().HasForeignKey("ParentId1").OnDelete(DeleteBehavior.ClientSetNull);
                    x.Property<int>("ParentId2");
                }),
            target => target.Entity(
                "Mushroom",
                x =>
                {
                    x.ToTable("Mushroom", "dbo");
                    x.Property<int>("Id");
                    x.Property<int>("ParentId1");
                    x.HasOne("Mushroom").WithMany().HasForeignKey("ParentId1").OnDelete(DeleteBehavior.NoAction);
                    x.Property<int>("ParentId2");
                }),
            operations => Assert.Equal(0, operations.Count));

    [ConditionalFact]
    public void Alter_foreign_key_target()
        => Execute(
            source =>
            {
                source.Entity("Lion").ToTable("Lion", "odb").Property<int>("LionId");
                source.Entity("Tiger").ToTable("Tiger", "bod").Property<int>("TigerId");
                source.Entity(
                    "Liger",
                    x =>
                    {
                        x.ToTable("Liger", "dbo");
                        x.Property<int>("Id");
                        x.Property<int>("ParentId");
                        x.HasOne("Lion").WithMany().HasForeignKey("ParentId");
                    });
            },
            target =>
            {
                target.Entity("Lion").ToTable("Lion", "odb").Property<int>("LionId");
                target.Entity("Tiger").ToTable("Tiger", "bod").Property<int>("TigerId");
                target.Entity(
                    "Liger",
                    x =>
                    {
                        x.ToTable("Liger", "dbo");
                        x.Property<int>("Id");
                        x.Property<int>("ParentId");
                        x.HasOne("Tiger").WithMany().HasForeignKey("ParentId").HasConstraintName("FK_Liger_Lion_ParentId");
                    });
            },
            operations =>
            {
                Assert.Equal(2, operations.Count);

                var dropOperation = Assert.IsType<DropForeignKeyOperation>(operations[0]);
                Assert.Equal("dbo", dropOperation.Schema);
                Assert.Equal("Liger", dropOperation.Table);
                Assert.Equal("FK_Liger_Lion_ParentId", dropOperation.Name);

                var addOperation = Assert.IsType<AddForeignKeyOperation>(operations[1]);
                Assert.Equal("dbo", addOperation.Schema);
                Assert.Equal("Liger", addOperation.Table);
                Assert.Equal("FK_Liger_Lion_ParentId", addOperation.Name);
                Assert.Equal(new[] { "ParentId" }, addOperation.Columns);
                Assert.Equal("bod", addOperation.PrincipalSchema);
                Assert.Equal("Tiger", addOperation.PrincipalTable);
                Assert.Equal(new[] { "TigerId" }, addOperation.PrincipalColumns);
            });

    [ConditionalFact]
    public void Create_index()
        => Execute(
            source => source.Entity(
                "Hippo",
                x =>
                {
                    x.ToTable("Hippo", "dbo");
                    x.Property<int>("Id");
                    x.Property<int>("Value");
                }),
            target => target.Entity(
                "Hippo",
                x =>
                {
                    x.ToTable("Hippo", "dbo");
                    x.Property<int>("Id");
                    x.Property<int>("Value");
                    x.HasIndex("Value").IsUnique();
                }),
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<CreateIndexOperation>(operations[0]);
                Assert.Equal("dbo", operation.Schema);
                Assert.Equal("Hippo", operation.Table);
                Assert.Equal("IX_Hippo_Value", operation.Name);
                Assert.Equal(new[] { "Value" }, operation.Columns);
                Assert.True(operation.IsUnique);
            });

    [ConditionalFact]
    public void Drop_index()
        => Execute(
            source => source.Entity(
                "Horse",
                x =>
                {
                    x.ToTable("Horse", "dbo");
                    x.Property<int>("Id");
                    x.Property<int>("Value");
                    x.HasIndex("Value");
                }),
            target => target.Entity(
                "Horse",
                x =>
                {
                    x.ToTable("Horse", "dbo");
                    x.Property<int>("Id");
                    x.Property<int>("Value");
                }),
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<DropIndexOperation>(operations[0]);
                Assert.Equal("dbo", operation.Schema);
                Assert.Equal("Horse", operation.Table);
                Assert.Equal("IX_Horse_Value", operation.Name);
            });

    [ConditionalFact]
    public void Rename_index()
        => Execute(
            source => source.Entity(
                "Donkey",
                x =>
                {
                    x.ToTable("Donkey", "dbo");
                    x.Property<int>("Id");
                    x.Property<int>("Value");
                    x.HasIndex("Value");
                }),
            target => target.Entity(
                "Donkey",
                x =>
                {
                    x.ToTable("Donkey", "dbo");
                    x.Property<int>("Id");
                    x.Property<int>("Value");
                    x.HasIndex(["Value"], "IX_dbo.Donkey_Value");
                }),
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<RenameIndexOperation>(operations[0]);
                Assert.Equal("dbo", operation.Schema);
                Assert.Equal("Donkey", operation.Table);
                Assert.Equal("IX_Donkey_Value", operation.Name);
                Assert.Equal("IX_dbo.Donkey_Value", operation.NewName);
            });

    [ConditionalFact]
    public void Alter_index_columns()
        => Execute(
            source => source.Entity(
                "Muel",
                x =>
                {
                    x.ToTable("Muel", "dbo");
                    x.Property<int>("Id");
                    x.Property<int>("Value");
                    x.HasIndex("Value");
                    x.Property<int>("MuleValue");
                }),
            target => target.Entity(
                "Muel",
                x =>
                {
                    x.ToTable("Muel", "dbo");
                    x.Property<int>("Id");
                    x.Property<int>("Value");
                    x.Property<int>("MuleValue");
                    x.HasIndex(["MuleValue"], "IX_Muel_Value");
                }),
            operations =>
            {
                Assert.Equal(2, operations.Count);

                var dropOperation = Assert.IsType<DropIndexOperation>(operations[0]);
                Assert.Equal("dbo", dropOperation.Schema);
                Assert.Equal("Muel", dropOperation.Table);
                Assert.Equal("IX_Muel_Value", dropOperation.Name);

                var createOperation = Assert.IsType<CreateIndexOperation>(operations[1]);
                Assert.Equal("dbo", createOperation.Schema);
                Assert.Equal("Muel", createOperation.Table);
                Assert.Equal("IX_Muel_Value", createOperation.Name);
                Assert.Equal(new[] { "MuleValue" }, createOperation.Columns);
            });

    [ConditionalFact]
    public void Alter_index_uniqueness()
        => Execute(
            source => source.Entity(
                "Pony",
                x =>
                {
                    x.ToTable("Pony", "dbo");
                    x.Property<int>("Id");
                    x.Property<int>("Value");
                    x.HasIndex("Value").IsUnique(false);
                }),
            target => target.Entity(
                "Pony",
                x =>
                {
                    x.ToTable("Pony", "dbo");
                    x.Property<int>("Id");
                    x.Property<int>("Value");
                    x.HasIndex("Value").IsUnique();
                }),
            operations =>
            {
                Assert.Equal(2, operations.Count);

                var dropOperation = Assert.IsType<DropIndexOperation>(operations[0]);
                Assert.Equal("dbo", dropOperation.Schema);
                Assert.Equal("Pony", dropOperation.Table);
                Assert.Equal("IX_Pony_Value", dropOperation.Name);

                var createOperation = Assert.IsType<CreateIndexOperation>(operations[1]);
                Assert.Equal("dbo", createOperation.Schema);
                Assert.Equal("Pony", createOperation.Table);
                Assert.Equal("IX_Pony_Value", createOperation.Name);
                Assert.True(createOperation.IsUnique);
            });

    [ConditionalFact]
    public void Create_sequence()
        => Execute(
            _ => { },
            modelBuilder => modelBuilder.HasSequence<int>("Tango", "dbo")
                .StartsAt(2)
                .IncrementsBy(3)
                .HasMin(1)
                .HasMax(4)
                .IsCyclic()
                .UseCache(20),
            operations =>
            {
                Assert.Equal(2, operations.Count);

                Assert.IsType<EnsureSchemaOperation>(operations[0]);

                var operation = Assert.IsType<CreateSequenceOperation>(operations[1]);
                Assert.Equal("Tango", operation.Name);
                Assert.Equal("dbo", operation.Schema);
                Assert.Equal(typeof(int), operation.ClrType);
                Assert.Equal(2, operation.StartValue);
                Assert.Equal(3, operation.IncrementBy);
                Assert.Equal(1, operation.MinValue);
                Assert.Equal(4, operation.MaxValue);
                Assert.True(operation.IsCyclic);
                Assert.True(operation.IsCached);
                Assert.Equal(20, operation.CacheSize);
            });

    [ConditionalFact]
    public void Drop_sequence()
        => Execute(
            modelBuilder => modelBuilder.HasSequence("Bravo", "dbo"),
            _ => { },
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<DropSequenceOperation>(operations[0]);
                Assert.Equal("Bravo", operation.Name);
                Assert.Equal("dbo", operation.Schema);
            });

    [ConditionalFact]
    public void Rename_sequence()
        => Execute(
            source => source.HasSequence("Bravo", "dbo"),
            target => target.HasSequence("bravo", "dbo"),
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<RenameSequenceOperation>(operations[0]);
                Assert.Equal("Bravo", operation.Name);
                Assert.Equal("dbo", operation.Schema);
                Assert.Equal("bravo", operation.NewName);
                Assert.Equal("dbo", operation.NewSchema);
            });

    [ConditionalFact]
    public void Move_sequence()
        => Execute(
            source => source.HasSequence("Charlie", "dbo"),
            target => target.HasSequence("Charlie", "odb"),
            operations =>
            {
                Assert.Equal(2, operations.Count);

                Assert.IsType<EnsureSchemaOperation>(operations[0]);

                var operation = Assert.IsType<RenameSequenceOperation>(operations[1]);
                Assert.Equal("Charlie", operation.Name);
                Assert.Equal("dbo", operation.Schema);
                Assert.Equal("Charlie", operation.NewName);
                Assert.Equal("odb", operation.NewSchema);
            });

    [ConditionalFact]
    public void Alter_sequence_increment_by()
        => Execute(
            source => source.HasSequence<int>("Alpha", "dbo")
                .StartsAt(2)
                .IncrementsBy(3)
                .HasMin(1)
                .HasMax(4)
                .IsCyclic()
                .UseCache(20),
            source => source.HasSequence<int>("Alpha", "dbo")
                .StartsAt(2)
                .IncrementsBy(5)
                .HasMin(1)
                .HasMax(4)
                .IsCyclic()
                .UseCache(20),
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<AlterSequenceOperation>(operations[0]);
                Assert.Equal("Alpha", operation.Name);
                Assert.Equal("dbo", operation.Schema);
                Assert.Equal(5, operation.IncrementBy);
                Assert.Equal(1, operation.MinValue);
                Assert.Equal(4, operation.MaxValue);
                Assert.True(operation.IsCyclic);
                Assert.True(operation.IsCached);
                Assert.Equal(20, operation.CacheSize);
            },
            skipSourceConventions: true);

    [ConditionalFact]
    public void Alter_sequence_max_value()
        => Execute(
            source => source.HasSequence<int>("Echo", "dbo")
                .StartsAt(2)
                .IncrementsBy(3)
                .HasMin(1)
                .HasMax(4)
                .IsCyclic()
                .UseCache(20),
            source => source.HasSequence<int>("Echo", "dbo")
                .StartsAt(2)
                .IncrementsBy(3)
                .HasMin(1)
                .HasMax(5)
                .IsCyclic()
                .UseCache(20),
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<AlterSequenceOperation>(operations[0]);
                Assert.Equal("Echo", operation.Name);
                Assert.Equal("dbo", operation.Schema);
                Assert.Equal(3, operation.IncrementBy);
                Assert.Equal(1, operation.MinValue);
                Assert.Equal(5, operation.MaxValue);
                Assert.True(operation.IsCyclic);
                Assert.True(operation.IsCached);
                Assert.Equal(20, operation.CacheSize);
            },
            skipSourceConventions: true);

    [ConditionalFact]
    public void Alter_sequence_min_value()
        => Execute(
            source => source.HasSequence<int>("Delta", "dbo")
                .StartsAt(2)
                .IncrementsBy(3)
                .HasMin(1)
                .HasMax(4)
                .IsCyclic()
                .UseCache(20),
            source => source.HasSequence<int>("Delta", "dbo")
                .StartsAt(2)
                .IncrementsBy(3)
                .HasMin(5)
                .HasMax(4)
                .IsCyclic()
                .UseCache(20),
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<AlterSequenceOperation>(operations[0]);
                Assert.Equal("Delta", operation.Name);
                Assert.Equal("dbo", operation.Schema);
                Assert.Equal(3, operation.IncrementBy);
                Assert.Equal(5, operation.MinValue);
                Assert.Equal(4, operation.MaxValue);
                Assert.True(operation.IsCyclic);
                Assert.True(operation.IsCached);
                Assert.Equal(20, operation.CacheSize);
            });

    [ConditionalFact]
    public void Alter_sequence_cycle()
        => Execute(
            source => source.HasSequence<int>("Foxtrot", "dbo")
                .StartsAt(2)
                .IncrementsBy(3)
                .HasMin(1)
                .HasMax(4)
                .IsCyclic()
                .UseCache(20),
            source => source.HasSequence<int>("Foxtrot", "dbo")
                .StartsAt(2)
                .IncrementsBy(3)
                .HasMin(1)
                .HasMax(4)
                .IsCyclic(false)
                .UseCache(20),
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<AlterSequenceOperation>(operations[0]);
                Assert.Equal("Foxtrot", operation.Name);
                Assert.Equal("dbo", operation.Schema);
                Assert.Equal(3, operation.IncrementBy);
                Assert.Equal(1, operation.MinValue);
                Assert.Equal(4, operation.MaxValue);
                Assert.False(operation.IsCyclic);
                Assert.True(operation.IsCached);
                Assert.Equal(20, operation.CacheSize);
            },
            skipSourceConventions: true);


    [ConditionalFact]
    public void Alter_sequence_cache_size()
        => Execute(
            source => source.HasSequence<int>("Gamma", "dbo")
                .StartsAt(2)
                .IncrementsBy(3)
                .HasMin(1)
                .HasMax(4)
                .IsCyclic()
                .UseCache(20),
            source => source.HasSequence<int>("Gamma", "dbo")
                .StartsAt(2)
                .IncrementsBy(3)
                .HasMin(1)
                .HasMax(4)
                .IsCyclic()
                .UseCache(5),
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<AlterSequenceOperation>(operations[0]);
                Assert.Equal("Gamma", operation.Name);
                Assert.Equal("dbo", operation.Schema);
                Assert.Equal(3, operation.IncrementBy);
                Assert.Equal(1, operation.MinValue);
                Assert.Equal(4, operation.MaxValue);
                Assert.True(operation.IsCyclic);
                Assert.True(operation.IsCached);
                Assert.Equal(5, operation.CacheSize);
            });



    [ConditionalFact]
    public void Alter_sequence_default_cache_to_nocache()
        => Execute(
            source => source.HasSequence<int>("Eta", "dbo")
                .StartsAt(2)
                .IncrementsBy(3)
                .HasMin(1)
                .HasMax(4)
                .IsCyclic()
                .UseCache(),
            source => source.HasSequence<int>("Eta", "dbo")
                .StartsAt(2)
                .IncrementsBy(3)
                .HasMin(1)
                .HasMax(4)
                .IsCyclic()
                .UseNoCache(),
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<AlterSequenceOperation>(operations[0]);
                Assert.Equal("Eta", operation.Name);
                Assert.Equal("dbo", operation.Schema);
                Assert.Equal(3, operation.IncrementBy);
                Assert.Equal(1, operation.MinValue);
                Assert.Equal(4, operation.MaxValue);
                Assert.True(operation.IsCyclic);
                Assert.False(operation.IsCached);
                Assert.Null(operation.CacheSize);
            });

    [ConditionalFact]
    public void Alter_sequence_default_cache_to_cache()
    => Execute(
        source => source.HasSequence<int>("Theta", "dbo")
            .StartsAt(2)
            .IncrementsBy(3)
            .HasMin(1)
            .HasMax(4)
            .IsCyclic()
            .UseCache(),
        source => source.HasSequence<int>("Theta", "dbo")
            .StartsAt(2)
            .IncrementsBy(3)
            .HasMin(1)
            .HasMax(4)
            .IsCyclic()
            .UseCache(20),
        operations =>
        {
            Assert.Equal(1, operations.Count);

            var operation = Assert.IsType<AlterSequenceOperation>(operations[0]);
            Assert.Equal("Theta", operation.Name);
            Assert.Equal("dbo", operation.Schema);
            Assert.Equal(3, operation.IncrementBy);
            Assert.Equal(1, operation.MinValue);
            Assert.Equal(4, operation.MaxValue);
            Assert.True(operation.IsCyclic);
            Assert.True(operation.IsCached);
            Assert.Equal(20, operation.CacheSize);
        });



    [ConditionalFact]
    public void Alter_sequence_nocache_to_cache()
    => Execute(
        source => source.HasSequence<int>("Epsilon", "dbo")
            .StartsAt(2)
            .IncrementsBy(3)
            .HasMin(1)
            .HasMax(4)
            .IsCyclic()
            .UseNoCache(),
        source => source.HasSequence<int>("Epsilon", "dbo")
            .StartsAt(2)
            .IncrementsBy(3)
            .HasMin(1)
            .HasMax(4)
            .IsCyclic()
            .UseCache(20),
        operations =>
        {
            Assert.Equal(1, operations.Count);

            var operation = Assert.IsType<AlterSequenceOperation>(operations[0]);
            Assert.Equal("Epsilon", operation.Name);
            Assert.Equal("dbo", operation.Schema);
            Assert.Equal(3, operation.IncrementBy);
            Assert.Equal(1, operation.MinValue);
            Assert.Equal(4, operation.MaxValue);
            Assert.True(operation.IsCyclic);
            Assert.True(operation.IsCached);
            Assert.Equal(20, operation.CacheSize);
        });


    [ConditionalFact]
    public void Alter_sequence_nocache_to_default_cache()
    => Execute(
        source => source.HasSequence<int>("Kappa", "dbo")
            .StartsAt(2)
            .IncrementsBy(3)
            .HasMin(1)
            .HasMax(4)
            .IsCyclic()
            .UseNoCache(),
        source => source.HasSequence<int>("Kappa", "dbo")
            .StartsAt(2)
            .IncrementsBy(3)
            .HasMin(1)
            .HasMax(4)
            .IsCyclic()
            .UseCache(),
        operations =>
        {
            Assert.Equal(1, operations.Count);

            var operation = Assert.IsType<AlterSequenceOperation>(operations[0]);
            Assert.Equal("Kappa", operation.Name);
            Assert.Equal("dbo", operation.Schema);
            Assert.Equal(3, operation.IncrementBy);
            Assert.Equal(1, operation.MinValue);
            Assert.Equal(4, operation.MaxValue);
            Assert.True(operation.IsCyclic);
            Assert.True(operation.IsCached);
            Assert.Null(operation.CacheSize);
        });

    [ConditionalFact]
    public void Alter_sequence_cache_to_default_cache()
    => Execute(
        source => source.HasSequence<int>("Omicron", "dbo")
            .StartsAt(2)
            .IncrementsBy(3)
            .HasMin(1)
            .HasMax(4)
            .IsCyclic()
            .UseCache(20),
        source => source.HasSequence<int>("Omicron", "dbo")
            .StartsAt(2)
            .IncrementsBy(3)
            .HasMin(1)
            .HasMax(4)
            .IsCyclic()
            .UseCache(),
        operations =>
        {
            Assert.Equal(1, operations.Count);

            var operation = Assert.IsType<AlterSequenceOperation>(operations[0]);
            Assert.Equal("Omicron", operation.Name);
            Assert.Equal("dbo", operation.Schema);
            Assert.Equal(3, operation.IncrementBy);
            Assert.Equal(1, operation.MinValue);
            Assert.Equal(4, operation.MaxValue);
            Assert.True(operation.IsCyclic);
            Assert.True(operation.IsCached);
            Assert.Null(operation.CacheSize);
        });

    [ConditionalFact]
    public void Alter_sequence_cache_to_nocache()
    => Execute(
        source => source.HasSequence<int>("Phi", "dbo")
            .StartsAt(2)
            .IncrementsBy(3)
            .HasMin(1)
            .HasMax(4)
            .IsCyclic()
            .UseCache(20),
        source => source.HasSequence<int>("Phi", "dbo")
            .StartsAt(2)
            .IncrementsBy(3)
            .HasMin(1)
            .HasMax(4)
            .IsCyclic()
            .UseNoCache(),
        operations =>
        {
            Assert.Equal(1, operations.Count);

            var operation = Assert.IsType<AlterSequenceOperation>(operations[0]);
            Assert.Equal("Phi", operation.Name);
            Assert.Equal("dbo", operation.Schema);
            Assert.Equal(3, operation.IncrementBy);
            Assert.Equal(1, operation.MinValue);
            Assert.Equal(4, operation.MaxValue);
            Assert.True(operation.IsCyclic);
            Assert.False(operation.IsCached);
            Assert.Null(operation.CacheSize);
        });

    [ConditionalFact]
    public void Alter_sequence_type()
        => Execute(
            source => source.HasSequence<int>("Hotel", "dbo")
                .StartsAt(2)
                .IncrementsBy(3)
                .HasMin(1)
                .HasMax(4)
                .IsCyclic()
                .UseCache(20),
            source => source.HasSequence<long>("Hotel", "dbo")
                .StartsAt(2)
                .IncrementsBy(3)
                .HasMin(1)
                .HasMax(4)
                .IsCyclic()
                .UseCache(20),
            operations =>
            {
                Assert.Equal(2, operations.Count);

                var dropOperation = Assert.IsType<DropSequenceOperation>(operations[0]);
                Assert.Equal("Hotel", dropOperation.Name);
                Assert.Equal("dbo", dropOperation.Schema);

                var createOperation = Assert.IsType<CreateSequenceOperation>(operations[1]);
                Assert.Equal("Hotel", createOperation.Name);
                Assert.Equal("dbo", createOperation.Schema);
                Assert.Equal(typeof(long), createOperation.ClrType);
                Assert.Equal(2, createOperation.StartValue);
                Assert.Equal(3, createOperation.IncrementBy);
                Assert.Equal(1, createOperation.MinValue);
                Assert.Equal(4, createOperation.MaxValue);
                Assert.True(createOperation.IsCyclic);
                Assert.True(createOperation.IsCached);
                Assert.Equal(20, createOperation.CacheSize);
            },
            skipSourceConventions: true);

    [ConditionalFact]
    public void Alter_sequence_start()
        => Execute(
            source => source.HasSequence<int>("Golf", "dbo")
                .StartsAt(2)
                .IncrementsBy(3)
                .HasMin(1)
                .HasMax(4)
                .IsCyclic()
                .UseCache(20),
            source => source.HasSequence<int>("Golf", "dbo")
                .StartsAt(5)
                .IncrementsBy(3)
                .HasMin(1)
                .HasMax(4)
                .IsCyclic()
                .UseCache(20),
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<RestartSequenceOperation>(operations[0]);

                Assert.Equal("dbo", operation.Schema);
                Assert.Equal("Golf", operation.Name);
                Assert.Equal(5, operation.StartValue);
            },
            skipSourceConventions: true);

    [ConditionalFact]
    public void Restart_altered_sequence()
        => Execute(
            source => source.HasSequence<int>("Golf", "dbo")
                .StartsAt(2)
                .IncrementsBy(3)
                .HasMin(1)
                .HasMax(4)
                .IsCyclic()
                .UseCache(20),
            source => source.HasSequence<int>("Golf", "dbo")
                .StartsAt(5)
                .IncrementsBy(6)
                .HasMin(1)
                .HasMax(4)
                .IsCyclic()
                .UseCache(20),
            operations => Assert.Collection(
                operations,
                o => Assert.IsType<AlterSequenceOperation>(o),
                o => Assert.IsType<RestartSequenceOperation>(o)),
            skipSourceConventions: true);

    [ConditionalFact]
    public void Diff_IProperty_destructive_when_null_to_not_null()
        => Execute(
            source => source.Entity(
                "Lizard",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<int?>("Value");
                }),
            target => target.Entity(
                "Lizard",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<int>("Value");
                }),
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<AlterColumnOperation>(operations[0]);
                Assert.True(operation.IsDestructiveChange);
            });

    [ConditionalFact]
    public void Diff_IProperty_not_destructive_when_not_null_to_null()
        => Execute(
            source => source.Entity(
                "Frog",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<int>("Value");
                }),
            target => target.Entity(
                "Frog",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<int?>("Value");
                }),
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<AlterColumnOperation>(operations[0]);
                Assert.False(operation.IsDestructiveChange);
            });

    [ConditionalFact]
    public void Diff_IProperty_destructive_when_type_changed()
        => Execute(
            source => source.Entity(
                "Frog",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<int>("Value");
                }),
            target => target.Entity(
                "Frog",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<int>("Value").HasColumnType("bigint");
                }),
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<AlterColumnOperation>(operations[0]);
                Assert.True(operation.IsDestructiveChange);
            });

    [ConditionalFact]
    public void Sort_works_with_primary_keys_and_columns()
        => Execute(
            source => source.Entity("Jaguar").Property<int>("Id"),
            target => target.Entity(
                "Jaguar",
                x =>
                {
                    x.Property<string>("Name");
                    x.HasKey("Name");
                }),
            operations => Assert.Collection(
                operations,
                o => Assert.IsType<DropPrimaryKeyOperation>(o),
                o => Assert.IsType<DropColumnOperation>(o),
                o => Assert.IsType<AddColumnOperation>(o),
                o => Assert.IsType<AddPrimaryKeyOperation>(o)));

    [ConditionalFact]
    public void Sort_adds_unique_constraint_after_column()
        => Execute(
            source => source.Entity("Panther").Property<int>("Id"),
            target => target.Entity(
                "Panther",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<int>("AlternateId");
                    x.HasAlternateKey("AlternateId");
                }),
            operations => Assert.Collection(
                operations,
                o => Assert.IsType<AddColumnOperation>(o),
                o => Assert.IsType<AddUniqueConstraintOperation>(o)));

    [ConditionalFact]
    public void Sort_drops_unique_constraint_before_column()
        => Execute(
            source => source.Entity(
                "Bobcat",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<int>("AlternateId");
                    x.HasAlternateKey("AlternateId");
                }),
            target => target.Entity("Bobcat").Property<int>("Id"),
            operations => Assert.Collection(
                operations,
                o => Assert.IsType<DropUniqueConstraintOperation>(o),
                o => Assert.IsType<DropColumnOperation>(o)));

    [ConditionalFact]
    public void Sort_creates_index_after_column()
        => Execute(
            source => source.Entity("Coyote").Property<int>("Id"),
            target => target.Entity(
                "Coyote",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<int>("Value");
                    x.HasIndex("Value");
                }),
            operations => Assert.Collection(
                operations,
                o => Assert.IsType<AddColumnOperation>(o),
                o => Assert.IsType<CreateIndexOperation>(o)));

    [ConditionalFact]
    public void Sort_drops_index_before_column()
        => Execute(
            source => source.Entity(
                "Wolf",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<int>("Value");
                    x.HasIndex("Value");
                }),
            target => target.Entity("Wolf").Property<int>("Id"),
            operations => Assert.Collection(
                operations,
                o => Assert.IsType<DropIndexOperation>(o),
                o => Assert.IsType<DropColumnOperation>(o)));

    [ConditionalFact]
    public void Sort_adds_foreign_key_after_column()
        => Execute(
            source => source.Entity("Algae").Property<int>("Id"),
            target => target.Entity(
                "Algae",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<int>("ParentId");
                    x.HasOne("Algae").WithMany().HasForeignKey("ParentId");
                }),
            operations => Assert.Collection(
                operations,
                o => Assert.IsType<AddColumnOperation>(o),
                o => Assert.IsType<CreateIndexOperation>(o),
                o => Assert.IsType<AddForeignKeyOperation>(o)));

    [ConditionalFact]
    public void Sort_drops_foreign_key_before_column()
        => Execute(
            source => source.Entity(
                "Bacteria",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<int>("ParentId");
                    x.HasOne("Bacteria").WithMany().HasForeignKey("ParentId");
                }),
            target => target.Entity("Bacteria").Property<int>("Id"),
            operations => Assert.Collection(
                operations,
                o => Assert.IsType<DropForeignKeyOperation>(o),
                o => Assert.IsType<DropIndexOperation>(o),
                o => Assert.IsType<DropColumnOperation>(o)));

    [ConditionalFact]
    public void Sort_adds_foreign_key_after_target_table()
        => Execute(
            source => source.Entity(
                "Car",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<int>("MakerId");
                }),
            target =>
            {
                target.Entity("Maker").Property<int>("Id");
                target.Entity(
                    "Car",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<int>("MakerId");
                        x.HasOne("Maker").WithMany().HasForeignKey("MakerId");
                    });
            },
            operations => Assert.Collection(
                operations,
                o => Assert.IsType<CreateTableOperation>(o),
                o => Assert.IsType<CreateIndexOperation>(o),
                o => Assert.IsType<AddForeignKeyOperation>(o)));

    [ConditionalFact]
    public void Sort_drops_foreign_key_before_target_table()
        => Execute(
            source =>
            {
                source.Entity("Maker").Property<int>("Id");
                source.Entity(
                    "Boat",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<int>("MakerId");
                        x.HasOne("Maker").WithMany().HasForeignKey("MakerId");
                    });
            },
            target => target.Entity(
                "Boat",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<int>("MakerId");
                }),
            operations => Assert.Collection(
                operations,
                o => Assert.IsType<DropForeignKeyOperation>(o),
                o => Assert.IsType<DropTableOperation>(o),
                o => Assert.IsType<DropIndexOperation>(o)));

    [ConditionalFact]
    public void Sort_adds_foreign_key_after_target_column_and_unique_constraint()
        => Execute(
            source =>
            {
                source.Entity("Maker").Property<int>("Id");
                source.Entity(
                    "Airplane",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<int>("MakerId");
                    });
            },
            target =>
            {
                target.Entity(
                    "Maker",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<int>("AlternateId");
                    });
                target.Entity(
                    "Airplane",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<int>("MakerId");
                        x.HasOne("Maker").WithMany().HasForeignKey("MakerId").HasPrincipalKey("AlternateId");
                    });
            },
            operations => Assert.Collection(
                operations,
                o => Assert.IsType<AddColumnOperation>(o),
                o => Assert.IsType<AddUniqueConstraintOperation>(o),
                o => Assert.IsType<CreateIndexOperation>(o),
                o => Assert.IsType<AddForeignKeyOperation>(o)));

    [ConditionalFact]
    public void Sort_drops_foreign_key_before_target_column_and_unique_constraint()
        => Execute(
            source =>
            {
                source.Entity(
                    "Maker",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<int>("AlternateId");
                    });
                source.Entity(
                    "Submarine",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<int>("MakerId");
                        x.HasOne("Maker").WithMany().HasForeignKey("MakerId").HasPrincipalKey("AlternateId");
                    });
            },
            target =>
            {
                target.Entity("Maker").Property<int>("Id");
                target.Entity(
                    "Submarine",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<int>("MakerId");
                    });
            },
            operations => Assert.Collection(
                operations,
                o => Assert.IsType<DropForeignKeyOperation>(o),
                o => Assert.IsType<DropIndexOperation>(o),
                o => Assert.IsType<DropUniqueConstraintOperation>(o),
                o => Assert.IsType<DropColumnOperation>(o)));

    [ConditionalFact]
    public void Sort_creates_tables_in_topologic_order()
        => Execute(
            _ => { },
            modelBuilder =>
            {
                modelBuilder.Entity("Maker").Property<int>("Id");
                modelBuilder.Entity(
                    "Helicopter",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<int>("MakerId");
                        x.HasOne("Maker").WithMany().HasForeignKey("MakerId");
                    });
            },
            operations =>
            {
                Assert.Equal(3, operations.Count);

                var operation1 = Assert.IsType<CreateTableOperation>(operations[0]);
                Assert.Equal("Maker", operation1.Name);

                var operation2 = Assert.IsType<CreateTableOperation>(operations[1]);
                Assert.Equal("Helicopter", operation2.Name);

                var operation3 = Assert.IsType<CreateIndexOperation>(operations[2]);
                Assert.Equal("IX_Helicopter_MakerId", operation3.Name);
            });

    [ConditionalFact]
    public void Sort_drops_tables_in_topologic_order()
        => Execute(
            modelBuilder =>
            {
                modelBuilder.Entity("Maker").Property<int>("Id");
                modelBuilder.Entity(
                    "Glider",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<int>("MakerId");
                        x.HasOne("Maker").WithMany().HasForeignKey("MakerId");
                    });
            },
            _ => { },
            operations =>
            {
                Assert.Equal(2, operations.Count);

                var operation1 = Assert.IsType<DropTableOperation>(operations[0]);
                Assert.Equal("Glider", operation1.Name);

                var operation2 = Assert.IsType<DropTableOperation>(operations[1]);
                Assert.Equal("Maker", operation2.Name);
            });

    [ConditionalFact]
    public void Rename_column_with_primary_key()
        => Execute(
            source => source.Entity("Hornet").Property<int>("Id"),
            target => target.Entity("Hornet").Property<int>("Id").HasColumnName("HornetId"),
            operations =>
            {
                Assert.Equal(1, operations.Count);

                Assert.IsType<RenameColumnOperation>(operations[0]);
            });

    [ConditionalFact]
    public void Rename_column_with_unique_constraint()
        => Execute(
            source => source.Entity(
                "Wasp",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<string>("Name");
                    x.HasAlternateKey("Name");
                }),
            target => target.Entity(
                "Wasp",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<string>("Name").HasColumnName("WaspName");
                    x.HasAlternateKey("Name");
                }),
            operations =>
            {
                Assert.Equal(3, operations.Count);

                Assert.IsType<DropUniqueConstraintOperation>(operations[0]);
                Assert.IsType<RenameColumnOperation>(operations[1]);
                Assert.IsType<AddUniqueConstraintOperation>(operations[2]);
            });

    [ConditionalFact]
    public void Rename_column_with_index()
        => Execute(
            source => source.Entity(
                "Bee",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<string>("Name");
                    x.HasIndex("Name");
                }),
            target => target.Entity(
                "Bee",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<string>("Name").HasColumnName("BeeName");
                    x.HasIndex("Name");
                }),
            operations =>
            {
                Assert.Equal(2, operations.Count);

                Assert.IsType<RenameColumnOperation>(operations[0]);
                Assert.IsType<RenameIndexOperation>(operations[1]);
            });

    [ConditionalFact]
    public void Rename_table_with_unique_constraint()
        => Execute(
            source => source.Entity(
                "Fly",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<string>("Name");
                    x.HasAlternateKey("Name");
                }),
            target => target.Entity(
                "Fly",
                x =>
                {
                    x.ToTable("Flies");
                    x.Property<int>("Id");
                    x.HasKey("Id").HasName("PK_Fly");
                    x.Property<string>("Name");
                    x.HasAlternateKey("Name").HasName("AK_Fly_Name");
                }),
            operations =>
            {
                Assert.Equal(1, operations.Count);

                Assert.IsType<RenameTableOperation>(operations[0]);
            });

    [ConditionalFact]
    public void Rename_table_with_index()
        => Execute(
            source => source.Entity(
                "Gnat",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<string>("Name");
                    x.HasIndex("Name");
                }),
            target => target.Entity(
                "Gnat",
                x =>
                {
                    x.ToTable("Gnats");
                    x.Property<int>("Id");
                    x.HasKey("Id").HasName("PK_Gnat");
                    x.Property<string>("Name");
                    x.HasIndex(["Name"], "IX_Gnat_Name");
                }),
            operations =>
            {
                Assert.Equal(1, operations.Count);

                Assert.IsType<RenameTableOperation>(operations[0]);
            });

    [ConditionalFact]
    public void Rename_entity_type_with_primary_key_and_unique_constraint()
        => Execute(
            source => source.Entity(
                "Grasshopper",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<string>("Name");
                    x.HasAlternateKey("Name");
                }),
            target => target.Entity(
                "grasshopper",
                x =>
                {
                    x.Property<int>("Id");
                    x.HasKey("Id").HasName("PK_Grasshopper");
                    x.Property<string>("Name");
                    x.HasAlternateKey("Name").HasName("AK_Grasshopper_Name");
                }),
            operations =>
            {
                Assert.Equal(1, operations.Count);

                Assert.IsType<RenameTableOperation>(operations[0]);
            });

    [ConditionalFact]
    public void Rename_entity_type_with_index()
        => Execute(
            source => source.Entity(
                "Cricket",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<string>("Name");
                    x.HasIndex("Name");
                }),
            target => target.Entity(
                "cricket",
                x =>
                {
                    x.Property<int>("Id");
                    x.HasKey("Id").HasName("PK_Cricket");
                    x.Property<string>("Name");
                    x.HasIndex("Name").HasDatabaseName("IX_Cricket_Name");
                }),
            operations =>
            {
                Assert.Equal(1, operations.Count);

                Assert.IsType<RenameTableOperation>(operations[0]);
            });

    [ConditionalFact]
    public void Rename_column_with_foreign_key()
        => Execute(
            source => source.Entity(
                "Yeast",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<int>("ParentId");
                    x.HasOne("Yeast").WithMany().HasForeignKey("ParentId");
                }),
            target => target.Entity(
                "Yeast",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<int>("ParentId").HasColumnName("ParentYeastId");
                    x.HasOne("Yeast").WithMany().HasForeignKey("ParentId");
                }),
            operations =>
            {
                Assert.Equal(4, operations.Count);

                Assert.IsType<DropForeignKeyOperation>(operations[0]);
                Assert.IsType<RenameColumnOperation>(operations[1]);
                Assert.IsType<RenameIndexOperation>(operations[2]);
                Assert.IsType<AddForeignKeyOperation>(operations[3]);
            });

    [ConditionalFact]
    public void Rename_column_with_referencing_foreign_key()
        => Execute(
            source => source.Entity(
                "Mucor",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<int>("ParentId");
                    x.HasOne("Mucor").WithMany().HasForeignKey("ParentId");
                }),
            target => target.Entity(
                "Mucor",
                x =>
                {
                    x.Property<int>("Id").HasColumnName("MucorId");
                    x.Property<int>("ParentId");
                    x.HasOne("Mucor").WithMany().HasForeignKey("ParentId");
                }),
            operations =>
            {
                Assert.Equal(1, operations.Count);
                Assert.IsType<RenameColumnOperation>(operations[0]);
            });

    [ConditionalFact]
    public void Add_column_with_foreign_key()
        => Execute(
            source => source.Entity(
                "Yeast",
                x =>
                {
                    x.Property<string>("Id").HasColumnType("ansi_string_fixed(127)");
                }),
            target => target.Entity(
                "Yeast",
                x =>
                {
                    x.Property<string>("Id").HasColumnType("ansi_string_fixed(127)");
                    x.Property<string>("ParentId").IsFixedLength(false);
                    x.HasOne("Yeast").WithMany().HasForeignKey("ParentId");
                }),
            operations => Assert.Collection(
                operations, o =>
                {
                    var operation = Assert.IsType<AddColumnOperation>(o);
                    Assert.Equal("ParentId", operation.Name);
                    Assert.Equal("ansi_string_fixed(127)", operation.ColumnType);
                }, o =>
                {
                    var operation = Assert.IsType<CreateIndexOperation>(o);
                    Assert.Equal(new[] { "ParentId" }, operation.Columns);
                }, o =>
                {
                    var operation = Assert.IsType<AddForeignKeyOperation>(o);
                    Assert.Equal(new[] { "ParentId" }, operation.Columns);
                }
            ));

    [ConditionalFact]
    public void Change_principal_column_facets()
        => Execute(
            source => source.Entity(
                "Yeast",
                x =>
                {
                    x.Property<string>("Id");
                    x.Property<string>("ParentId");
                    x.HasOne("Yeast").WithMany().HasForeignKey("ParentId");
                }),
            target => target.Entity(
                "Yeast",
                x =>
                {
                    x.Property<string>("Id").HasMaxLength(127).IsFixedLength().IsUnicode(false);
                    x.Property<string>("ParentId").IsFixedLength(false);
                    x.HasOne("Yeast").WithMany().HasForeignKey("ParentId");
                }),
            operations => Assert.Collection(
                operations, o =>
                {
                    var operation = Assert.IsType<AlterColumnOperation>(o);
                    Assert.Equal("ParentId", operation.Name);
                    Assert.Equal("ansi_string(127)", operation.ColumnType);
                }, o =>
                {
                    var operation = Assert.IsType<AlterColumnOperation>(o);
                    Assert.Equal("Id", operation.Name);
                    Assert.Equal("ansi_string_fixed(127)", operation.ColumnType);
                }
            ));

    [ConditionalFact]
    public void Rename_table_with_foreign_key()
        => Execute(
            source =>
            {
                source.Entity("Zebra").Property<int>("Id");
                source.Entity(
                    "Zonkey",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<int>("ParentId");
                        x.HasOne("Zebra").WithMany().HasForeignKey("ParentId");
                    });
            },
            target =>
            {
                target.Entity("Zebra").Property<int>("Id");
                target.Entity(
                    "Zonkey",
                    x =>
                    {
                        x.ToTable("Zonkeys");
                        x.Property<int>("Id");
                        x.HasKey("Id").HasName("PK_Zonkey");
                        x.Property<int>("ParentId");
                        x.HasOne("Zebra").WithMany().HasForeignKey("ParentId").HasConstraintName("FK_Zonkey_Zebra_ParentId");
                        x.HasIndex("ParentId").HasDatabaseName("IX_Zonkey_ParentId");
                    });
            },
            operations =>
            {
                Assert.Equal(1, operations.Count);
                Assert.IsType<RenameTableOperation>(operations[0]);
            });

    [ConditionalFact]
    public void Rename_table_with_referencing_foreign_key()
        => Execute(
            source =>
            {
                source.Entity("Jaguar").Property<int>("Id");
                source.Entity(
                    "Jaglion",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<int>("ParentId");
                        x.HasOne("Jaguar").WithMany().HasForeignKey("ParentId");
                    });
            },
            target =>
            {
                target.Entity(
                    "Jaguar",
                    x =>
                    {
                        x.ToTable("Jaguars");
                        x.Property<int>("Id");
                        x.HasKey("Id").HasName("PK_Jaguar");
                    });
                target.Entity(
                    "Jaglion",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<int>("ParentId");
                        x.HasOne("Jaguar").WithMany().HasForeignKey("ParentId")
                            .HasConstraintName("FK_Jaglion_Jaguar_ParentId");
                    });
            },
            operations =>
            {
                Assert.Equal(1, operations.Count);
                Assert.IsType<RenameTableOperation>(operations[0]);
            });

    [ConditionalFact]
    public void Create_table_with_property_on_subtype()
        => Execute(
            _ => { },
            modelBuilder =>
            {
                modelBuilder.Entity("Animal").Property<int>("Id");
                modelBuilder.Entity("Fish").HasBaseType("Animal").Property<string>("Name");
            },
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<CreateTableOperation>(operations[0]);
                Assert.Equal("Animal", operation.Name);
                Assert.Equal(3, operation.Columns.Count);

                Assert.Contains(operation.Columns, c => c.Name == "Name");
            });

    [ConditionalFact]
    public void Create_table_with_required_property_on_subtype()
        => Execute(
            _ => { },
            modelBuilder =>
            {
                modelBuilder.Entity("Animal").Property<int>("Id");
                modelBuilder.Entity("Whale").HasBaseType("Animal").Property<int>("Value");
            },
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<CreateTableOperation>(operations[0]);
                Assert.Equal("Animal", operation.Name);
                Assert.Equal(3, operation.Columns.Count);

                Assert.True(operation.Columns.First(c => c.Name == "Value").IsNullable);
            });

    [ConditionalFact]
    public void Add_property_on_subtype()
        => Execute(
            source =>
            {
                source.Entity("Animal").ToTable("Animal", "dbo").Property<int>("Id");
                source.Entity("Shark").HasBaseType("Animal");
            },
            target =>
            {
                target.Entity("Animal").ToTable("Animal", "dbo").Property<int>("Id");
                target.Entity("Shark").HasBaseType("Animal").Property<string>("Name");
            },
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<AddColumnOperation>(operations[0]);
                Assert.Equal("dbo", operation.Schema);
                Assert.Equal("Animal", operation.Table);
                Assert.Equal("Name", operation.Name);
            });

    [ConditionalFact]
    public void Add_required_property_on_subtype()
        => Execute(
            source =>
            {
                source.Entity("Animal").Property<int>("Id");
                source.Entity("Marlin").HasBaseType("Animal");
            },
            target =>
            {
                target.Entity("Animal").Property<int>("Id");
                target.Entity("Marlin").HasBaseType("Animal").Property<int>("Value");
            },
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<AddColumnOperation>(operations[0]);
                Assert.Equal("Value", operation.Name);
                Assert.Equal("Value", operation.Name);
                Assert.True(operation.IsNullable);
            });

    [ConditionalFact]
    public void Remove_property_on_subtype()
        => Execute(
            source =>
            {
                source.Entity("Animal").ToTable("Animal", "dbo").Property<int>("Id");
                source.Entity("Blowfish").HasBaseType("Animal").Property<string>("Name");
            },
            target =>
            {
                target.Entity("Animal").ToTable("Animal", "dbo").Property<int>("Id");
                target.Entity("Blowfish").HasBaseType("Animal");
            },
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<DropColumnOperation>(operations[0]);
                Assert.Equal("dbo", operation.Schema);
                Assert.Equal("Animal", operation.Table);
                Assert.Equal("Name", operation.Name);
            });

    [ConditionalFact]
    public void Alter_property_on_subtype()
        => Execute(
            source =>
            {
                source.Entity("Animal").ToTable("Animal", "dbo").Property<int>("Id");
                source.Entity("Barracuda").HasBaseType("Animal").Property<string>("Name");
            },
            target =>
            {
                target.Entity("Animal").ToTable("Animal", "dbo").Property<int>("Id");
                target.Entity("Barracuda").HasBaseType("Animal").Property<string>("Name")
                    .HasColumnType("varchar(30)");
            },
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<AlterColumnOperation>(operations[0]);
                Assert.Equal("dbo", operation.Schema);
                Assert.Equal("Animal", operation.Table);
                Assert.Equal("Name", operation.Name);
                Assert.Equal("varchar(30)", operation.ColumnType);
            });

    [ConditionalFact]
    public void Create_index_on_subtype()
        => Execute(
            source =>
            {
                source.Entity("Animal").ToTable("Animal", "dbo").Property<int>("Id");
                source.Entity("Minnow").HasBaseType("Animal").Property<string>("Name");
            },
            target =>
            {
                target.Entity("Animal").ToTable("Animal", "dbo").Property<int>("Id");
                target.Entity(
                    "Minnow",
                    x =>
                    {
                        x.HasBaseType("Animal");
                        x.Property<string>("Name");
                        x.HasIndex("Name");
                    });
            },
            operations =>
            {
                Assert.Equal(2, operations.Count);
                Assert.IsType<AlterColumnOperation>(operations[0]); // Because index property has different type mapping

                var operation = Assert.IsType<CreateIndexOperation>(operations[1]);
                Assert.Equal("dbo", operation.Schema);
                Assert.Equal("Animal", operation.Table);
                Assert.Equal("IX_Animal_Name", operation.Name);
                Assert.Equal(new[] { "Name" }, operation.Columns);
            });

    [ConditionalFact]
    public void Alter_index_on_subtype()
        => Execute(
            source =>
            {
                source.Entity("Animal").ToTable("Animal", "dbo").Property<int>("Id");
                source.Entity(
                    "Pike",
                    x =>
                    {
                        x.HasBaseType("Animal");
                        x.Property<string>("Name");
                        x.HasIndex("Name");
                    });
            },
            target =>
            {
                target.Entity("Animal").ToTable("Animal", "dbo").Property<int>("Id");
                target.Entity(
                    "Pike",
                    x =>
                    {
                        x.HasBaseType("Animal");
                        x.Property<string>("Name");
                        x.HasIndex("Name").HasDatabaseName("IX_Animal_Pike_Name");
                    });
            },
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<RenameIndexOperation>(operations[0]);
                Assert.Equal("dbo", operation.Schema);
                Assert.Equal("Animal", operation.Table);
                Assert.Equal("IX_Animal_Name", operation.Name);
                Assert.Equal("IX_Animal_Pike_Name", operation.NewName);
            });

    [ConditionalFact]
    public void Drop_index_on_subtype()
        => Execute(
            source =>
            {
                source.Entity("Animal").ToTable("Animal", "dbo").Property<int>("Id");
                source.Entity(
                    "Catfish",
                    x =>
                    {
                        x.HasBaseType("Animal");
                        x.Property<string>("Name");
                        x.HasIndex("Name");
                    });
            },
            target =>
            {
                target.Entity("Animal").ToTable("Animal", "dbo").Property<int>("Id");
                target.Entity("Catfish").HasBaseType("Animal").Property<string>("Name");
            },
            operations =>
            {
                Assert.Equal(2, operations.Count);
                Assert.IsType<AlterColumnOperation>(operations[1]); // Because index property has different type mapping

                var operation = Assert.IsType<DropIndexOperation>(operations[0]);
                Assert.Equal("dbo", operation.Schema);
                Assert.Equal("Animal", operation.Table);
                Assert.Equal("IX_Animal_Name", operation.Name);
            });

    [ConditionalFact]
    public void Create_table_with_foreign_key_on_base_type()
        => Execute(
            _ => { },
            modelBuilder =>
            {
                modelBuilder.Entity("Person").Property<int>("Id");
                modelBuilder.Entity(
                    "Animal",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<int>("HandlerId");
                        x.HasOne("Person").WithMany().HasForeignKey("HandlerId");
                    });
                modelBuilder.Entity("Wyvern").HasBaseType("Animal");
            },
            operations =>
            {
                Assert.Equal(3, operations.Count);
                Assert.IsType<CreateTableOperation>(operations[0]);

                var createTableOperation = Assert.IsType<CreateTableOperation>(operations[1]);
                Assert.Equal("Animal", createTableOperation.Name);
                Assert.Single(createTableOperation.ForeignKeys);

                var addForeignKeyOperation = createTableOperation.ForeignKeys[0];
                Assert.Equal("FK_Animal_Person_HandlerId", addForeignKeyOperation.Name);
                Assert.Equal(new[] { "HandlerId" }, addForeignKeyOperation.Columns);
                Assert.Equal("Person", addForeignKeyOperation.PrincipalTable);
                Assert.Equal(new[] { "Id" }, addForeignKeyOperation.PrincipalColumns);

                var createIndexOperation = Assert.IsType<CreateIndexOperation>(operations[2]);
                Assert.Equal("Animal", createIndexOperation.Table);
                Assert.Equal("IX_Animal_HandlerId", createIndexOperation.Name);
                Assert.Equal(new[] { "HandlerId" }, createIndexOperation.Columns);
            });

    [ConditionalFact]
    public void Create_table_with_foreign_key_on_subtype()
        => Execute(
            _ => { },
            modelBuilder =>
            {
                modelBuilder.Entity("Person").Property<int>("Id");
                modelBuilder.Entity("Animal").Property<int>("Id");
                modelBuilder.Entity(
                    "Stag",
                    x =>
                    {
                        x.HasBaseType("Animal");
                        x.Property<int>("HandlerId");
                        x.HasOne("Person").WithMany().HasForeignKey("HandlerId");
                    });
            },
            operations =>
            {
                Assert.Equal(3, operations.Count);

                Assert.IsType<CreateTableOperation>(operations[0]);

                var createTableOperation = Assert.IsType<CreateTableOperation>(operations[1]);
                Assert.Equal("Animal", createTableOperation.Name);
                Assert.Single(createTableOperation.ForeignKeys);

                var addForeignKeyOperation = createTableOperation.ForeignKeys[0];
                Assert.Equal("FK_Animal_Person_HandlerId", addForeignKeyOperation.Name);
                Assert.Equal(new[] { "HandlerId" }, addForeignKeyOperation.Columns);
                Assert.Equal("Person", addForeignKeyOperation.PrincipalTable);
                Assert.Equal(new[] { "Id" }, addForeignKeyOperation.PrincipalColumns);

                var createIndexOperation = Assert.IsType<CreateIndexOperation>(operations[2]);
                Assert.Equal("Animal", createIndexOperation.Table);
                Assert.Equal("IX_Animal_HandlerId", createIndexOperation.Name);
                Assert.Equal(new[] { "HandlerId" }, createIndexOperation.Columns);
            });

    [ConditionalFact]
    public void Create_table_with_foreign_key_to_subtype()
        => Execute(
            _ => { },
            modelBuilder =>
            {
                modelBuilder.Entity("Animal").Property<int>("Id");
                modelBuilder.Entity("DomesticAnimal").HasBaseType("Animal");
                modelBuilder.Entity(
                    "Person",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<int>("PetId");
                        x.HasOne("DomesticAnimal").WithMany().HasForeignKey("PetId");
                    });
            },
            operations =>
            {
                Assert.Equal(3, operations.Count);

                Assert.IsType<CreateTableOperation>(operations[0]);

                var createTableOperation = Assert.IsType<CreateTableOperation>(operations[1]);
                Assert.Equal("Person", createTableOperation.Name);
                Assert.Single(createTableOperation.ForeignKeys);

                var addForeignKeyOperation = createTableOperation.ForeignKeys[0];
                Assert.Equal("FK_Person_Animal_PetId", addForeignKeyOperation.Name);
                Assert.Equal(new[] { "PetId" }, addForeignKeyOperation.Columns);
                Assert.Equal("Animal", addForeignKeyOperation.PrincipalTable);
                Assert.Equal(new[] { "Id" }, addForeignKeyOperation.PrincipalColumns);

                var createIndexOperation = Assert.IsType<CreateIndexOperation>(operations[2]);
                Assert.Equal("Person", createIndexOperation.Table);
                Assert.Equal("IX_Person_PetId", createIndexOperation.Name);
                Assert.Equal(new[] { "PetId" }, createIndexOperation.Columns);
            });

    [ConditionalFact]
    public void Create_table_with_selfReferencing_foreign_key_in_hierarchy()
        => Execute(
            _ => { },
            modelBuilder =>
            {
                modelBuilder.Entity("Animal").Property<int>("Id");
                modelBuilder.Entity(
                    "Predator",
                    x =>
                    {
                        x.HasBaseType("Animal");
                        x.Property<int>("PreyId");
                        x.HasOne("Animal").WithMany().HasForeignKey("PreyId");
                    });
            },
            operations =>
            {
                Assert.Equal(2, operations.Count);

                var createTableOperation = Assert.IsType<CreateTableOperation>(operations[0]);
                Assert.Single(createTableOperation.ForeignKeys);

                var addForeignKeyOperation = createTableOperation.ForeignKeys[0];
                Assert.Equal("FK_Animal_Animal_PreyId", addForeignKeyOperation.Name);
                Assert.Equal(new[] { "PreyId" }, addForeignKeyOperation.Columns);
                Assert.Equal("Animal", addForeignKeyOperation.PrincipalTable);
                Assert.Equal(new[] { "Id" }, addForeignKeyOperation.PrincipalColumns);

                var createIndexOperation = Assert.IsType<CreateIndexOperation>(operations[1]);
                Assert.Equal("Animal", createIndexOperation.Table);
                Assert.Equal("IX_Animal_PreyId", createIndexOperation.Name);
                Assert.Equal(new[] { "PreyId" }, createIndexOperation.Columns);
            });

    [ConditionalFact]
    public void Create_table_with_seed_data()
        => Execute(
            _ => { },
            _ => { },
            target => target.Entity(
                "Zebra",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<string>("Name").HasColumnType("nvarchar(30)");
                    x.HasData(
                        new { Id = 42, Name = "equal" });
                }),
            upOps => Assert.Collection(
                upOps,
                o =>
                {
                    var operation = Assert.IsType<CreateTableOperation>(o);
                    Assert.Null(operation.Schema);
                    Assert.Equal("Zebra", operation.Name);
                },
                o =>
                {
                    var operation = Assert.IsType<InsertDataOperation>(o);
                    Assert.Null(operation.Schema);
                    Assert.Equal("Zebra", operation.Table);
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Equal(42, v),
                        v => Assert.Equal("equal", v));
                }),
            downOps => Assert.Collection(
                downOps,
                o =>
                {
                    var operation = Assert.IsType<DropTableOperation>(o);
                    Assert.Null(operation.Schema);
                    Assert.Equal("Zebra", operation.Name);
                }));

    [ConditionalFact]
    public void Add_subtype_with_shared_column_with_seed_data()
        => Execute(
            modelBuilder =>
            {
                modelBuilder.Entity("Animal").Property<int>("Id");
                modelBuilder.Entity(
                    "Cat", x =>
                    {
                        x.HasBaseType("Animal").Property<string>("BreederId").HasColumnName("BreederId");
                        x.HasData(
                            new { Id = 42, BreederId = "42" });
                    });
            },
            _ => { },
            modelBuilder =>
            {
                modelBuilder.Entity(
                    "Dog", x =>
                    {
                        x.HasBaseType("Animal").Property<string>("BreederId").HasColumnName("BreederId");
                        x.HasData(
                            new { Id = 43, BreederId = "43" });
                    });
            },
            upOps => Assert.Collection(
                upOps,
                o =>
                {
                    var m = Assert.IsType<InsertDataOperation>(o);
                    AssertMultidimensionalArray(
                        m.Values,
                        v => Assert.Equal(43, v),
                        v => Assert.Equal("43", v),
                        v => Assert.Equal("Dog", v));
                }),
            downOps => Assert.Collection(
                downOps,
                o =>
                {
                    var m = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Null(m.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        m.KeyValues,
                        v => Assert.Equal(43, v));
                }));

    [ConditionalFact]
    public void Change_TPH_to_TPT_with_FKs_and_seed_data()
        => Execute(
            modelBuilder =>
            {
                modelBuilder.Entity(
                    "Animal", x =>
                    {
                        x.Property<int>("Id");
                        x.Property<int?>("MouseId");

                        x.HasOne("Mouse").WithMany().HasForeignKey("MouseId");
                    });
                modelBuilder.Entity(
                    "Cat", x =>
                    {
                        x.HasBaseType("Animal");
                        x.Property<int?>("PreyId").HasColumnName("PreyId");

                        x.HasOne("Animal").WithMany().HasForeignKey("PreyId");
                        x.HasData(
                            new { Id = 11, MouseId = 31 });
                    });
                modelBuilder.Entity(
                    "Dog", x =>
                    {
                        x.HasBaseType("Animal");
                        x.Property<int?>("PreyId").HasColumnName("PreyId");

                        x.HasOne("Animal").WithMany().HasForeignKey("PreyId");
                        x.HasData(
                            new { Id = 21, PreyId = 31 });
                    });
                modelBuilder.Entity(
                    "Mouse", x =>
                    {
                        x.HasBaseType("Animal");

                        x.HasData(
                            new { Id = 31 });
                    });
            },
            source =>
            {
                source.Entity(
                    "Animal", x =>
                    {
                        x.Property<string>("Discriminator").Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Save);
                    });
                source.Entity(
                    "Cat", x =>
                    {
                        x.HasData(
                            new { Id = 12, MouseId = 32 },
                            new { Id = 13 });
                    });
                source.Entity(
                    "Dog", x =>
                    {
                        x.HasData(
                            new { Id = 22, PreyId = 32 });
                    });
                source.Entity(
                    "Mouse", x =>
                    {
                        x.HasData(
                            new { Id = 32 });
                    });
                source.Entity(
                    "UnrelatedDog", x =>
                    {
                        x.ToTable("Dogs");
                        x.Property<int>("Id");
                        x.Property<int?>("PreyId");
                    });
            },
            target =>
            {
                target.Entity(
                    "Cat", x =>
                    {
                        x.ToTable("Cats");
                        x.HasData(
                            new { Id = 12 },
                            new { Id = 13, MouseId = 32 });
                    });
                target.Entity(
                    "Dog", x =>
                    {
                        x.ToTable("Dogs");
                        x.HasOne("Animal", null).WithOne().HasForeignKey("Dog", "Id")
                            .HasConstraintName("FK_Dogs_Animal");
                        x.HasData(
                            new { Id = 22, PreyId = 33 },
                            new { Id = 23 });
                    });
                target.Entity(
                    "Mouse", x =>
                    {
                        x.ToTable("Mice");
                        x.HasData(
                            new { Id = 33 });
                    });
            },
            upOps => Assert.Collection(
                upOps,
                o =>
                {
                    var operation = Assert.IsType<DropForeignKeyOperation>(o);
                    Assert.Equal("FK_Animal_Animal_MouseId", operation.Name);
                    Assert.Equal("Animal", operation.Table);
                },
                o =>
                {
                    var operation = Assert.IsType<DropForeignKeyOperation>(o);
                    Assert.Equal("FK_Animal_Animal_PreyId", operation.Name);
                    Assert.Equal("Animal", operation.Table);
                },
                o =>
                {
                    var operation = Assert.IsType<DropIndexOperation>(o);
                    Assert.Equal("IX_Animal_PreyId", operation.Name);
                    Assert.Equal("Animal", operation.Table);
                },
                o =>
                {
                    var operation = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(32, v));
                },
                o =>
                {
                    var operation = Assert.IsType<DropColumnOperation>(o);
                    Assert.Equal("Discriminator", operation.Name);
                    Assert.Equal("Animal", operation.Table);
                },
                o =>
                {
                    var operation = Assert.IsType<DropColumnOperation>(o);
                    Assert.Equal("PreyId", operation.Name);
                    Assert.Equal("Animal", operation.Table);
                },
                o =>
                {
                    var operation = Assert.IsType<CreateTableOperation>(o);
                    Assert.Equal("Cats", operation.Name);
                    Assert.Collection(
                        operation.Columns,
                        c =>
                        {
                            Assert.Equal("Id", c.Name);
                            Assert.Equal("default_int_mapping", c.ColumnType);
                            Assert.Equal("Cats", c.Table);
                            Assert.False(c.IsNullable);
                            Assert.False(c.IsRowVersion);
                            Assert.Null(c.IsUnicode);
                            Assert.Null(c.IsFixedLength);
                            Assert.Null(c.MaxLength);
                            Assert.Null(c.Precision);
                            Assert.Null(c.Scale);
                            Assert.Null(c.DefaultValue);
                            Assert.Null(c.DefaultValueSql);
                            Assert.Null(c.ComputedColumnSql);
                            Assert.Null(c.IsStored);
                            Assert.Null(c.Comment);
                            Assert.Null(c.Collation);
                        },
                        c =>
                        {
                            Assert.Equal("PreyId", c.Name);
                            Assert.Equal("default_int_mapping", c.ColumnType);
                            Assert.Equal("Cats", c.Table);
                            Assert.True(c.IsNullable);
                            Assert.False(c.IsRowVersion);
                            Assert.Null(c.IsUnicode);
                            Assert.Null(c.IsFixedLength);
                            Assert.Null(c.MaxLength);
                            Assert.Null(c.Precision);
                            Assert.Null(c.Scale);
                            Assert.Null(c.DefaultValue);
                            Assert.Null(c.DefaultValueSql);
                            Assert.Null(c.ComputedColumnSql);
                            Assert.Null(c.IsStored);
                            Assert.Null(c.Comment);
                            Assert.Null(c.Collation);
                        });

                    var pk = operation.PrimaryKey;
                    Assert.Equal("PK_Cats", pk.Name);
                    Assert.Equal("Cats", pk.Table);
                    Assert.Equal(new[] { "Id" }, pk.Columns);

                    Assert.Collection(
                        operation.ForeignKeys,
                        fk =>
                        {
                            Assert.Equal("FK_Cats_Animal_Id", fk.Name);
                            Assert.Equal("Cats", fk.Table);
                            Assert.Equal("Animal", fk.PrincipalTable);
                            Assert.Equal(new[] { "Id" }, fk.Columns);
                            Assert.Equal(new[] { "Id" }, fk.PrincipalColumns);
                            Assert.Equal(ReferentialAction.Cascade, fk.OnDelete);
                        },
                        fk =>
                        {
                            Assert.Equal("FK_Cats_Animal_PreyId", fk.Name);
                            Assert.Equal("Cats", fk.Table);
                            Assert.Equal("Animal", fk.PrincipalTable);
                            Assert.Equal(new[] { "PreyId" }, fk.Columns);
                            Assert.Equal(new[] { "Id" }, fk.PrincipalColumns);
                            Assert.Equal(ReferentialAction.NoAction, fk.OnDelete);
                        });

                    Assert.Empty(operation.UniqueConstraints);
                    Assert.Null(operation.Comment);
                    Assert.Empty(operation.CheckConstraints);
                },
                o =>
                {
                    var operation = Assert.IsType<CreateTableOperation>(o);
                    Assert.Equal("Mice", operation.Name);
                    Assert.Collection(
                        operation.Columns,
                        c =>
                        {
                            Assert.Equal("Id", c.Name);
                            Assert.Equal("default_int_mapping", c.ColumnType);
                            Assert.Equal("Mice", c.Table);
                            Assert.False(c.IsNullable);
                            Assert.False(c.IsRowVersion);
                            Assert.Null(c.IsUnicode);
                            Assert.Null(c.IsFixedLength);
                            Assert.Null(c.MaxLength);
                            Assert.Null(c.Precision);
                            Assert.Null(c.Scale);
                            Assert.Null(c.DefaultValue);
                            Assert.Null(c.DefaultValueSql);
                            Assert.Null(c.ComputedColumnSql);
                            Assert.Null(c.IsStored);
                            Assert.Null(c.Comment);
                            Assert.Null(c.Collation);
                        });

                    var pk = operation.PrimaryKey;
                    Assert.Equal("PK_Mice", pk.Name);
                    Assert.Equal("Mice", pk.Table);
                    Assert.Equal(new[] { "Id" }, pk.Columns);

                    var fk = operation.ForeignKeys.Single();
                    Assert.Equal("FK_Mice_Animal_Id", fk.Name);
                    Assert.Equal("Mice", fk.Table);
                    Assert.Equal("Animal", fk.PrincipalTable);
                    Assert.Equal(new[] { "Id" }, fk.Columns);
                    Assert.Equal(new[] { "Id" }, fk.PrincipalColumns);
                    Assert.Equal(ReferentialAction.Cascade, fk.OnDelete);

                    Assert.Empty(operation.UniqueConstraints);
                    Assert.Null(operation.Comment);
                    Assert.Empty(operation.CheckConstraints);
                },
                o =>
                {
                    var operation = Assert.IsType<UpdateDataOperation>(o);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(12, v));

                    Assert.Equal(new[] { "MouseId" }, operation.Columns);
                    Assert.Null(operation.ColumnTypes);
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Null(v));
                },
                o =>
                {
                    var operation = Assert.IsType<UpdateDataOperation>(o);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(13, v));

                    Assert.Equal(new[] { "MouseId" }, operation.Columns);
                    Assert.Null(operation.ColumnTypes);
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Equal(32, v));
                },
                o =>
                {
                    var operation = Assert.IsType<InsertDataOperation>(o);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal(new[] { "Id", "MouseId" }, operation.Columns);
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Equal(23, v),
                        v => Assert.Null(v),
                        v => Assert.Equal(33, v),
                        v => Assert.Null(v));
                },
                o =>
                {
                    var operation = Assert.IsType<InsertDataOperation>(o);
                    Assert.Equal("Cats", operation.Table);
                    Assert.Equal(new[] { "Id", "PreyId" }, operation.Columns);
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Equal(11, v),
                        v => Assert.Null(v),
                        v => Assert.Equal(12, v),
                        v => Assert.Null(v),
                        v => Assert.Equal(13, v),
                        v => Assert.Null(v));
                },
                o =>
                {
                    var operation = Assert.IsType<InsertDataOperation>(o);
                    Assert.Equal("Dogs", operation.Table);

                    Assert.Equal(new[] { "Id", "PreyId" }, operation.Columns);
                    Assert.Null(operation.ColumnTypes);
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Equal(21, v),
                        v => Assert.Equal(31, v));
                },
                o =>
                {
                    var operation = Assert.IsType<InsertDataOperation>(o);
                    Assert.Equal("Mice", operation.Table);
                    Assert.Equal(new[] { "Id" }, operation.Columns);
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Equal(31, v));
                },
                o =>
                {
                    var operation = Assert.IsType<InsertDataOperation>(o);
                    Assert.Equal("Dogs", operation.Table);

                    Assert.Equal(new[] { "Id", "PreyId" }, operation.Columns);
                    Assert.Null(operation.ColumnTypes);
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Equal(22, v),
                        v => Assert.Equal(33, v),
                        v => Assert.Equal(23, v),
                        v => Assert.Null(v));
                },
                o =>
                {
                    var operation = Assert.IsType<InsertDataOperation>(o);
                    Assert.Equal("Mice", operation.Table);
                    Assert.Equal(new[] { "Id" }, operation.Columns);
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Equal(33, v));
                },
                o =>
                {
                    var operation = Assert.IsType<CreateIndexOperation>(o);
                    Assert.Equal("IX_Dogs_PreyId", operation.Name);
                    Assert.Equal("Dogs", operation.Table);
                    Assert.Equal(new[] { "PreyId" }, operation.Columns);
                },
                o =>
                {
                    var operation = Assert.IsType<CreateIndexOperation>(o);
                    Assert.Equal("IX_Cats_PreyId", operation.Name);
                    Assert.Equal("Cats", operation.Table);
                    Assert.Equal(new[] { "PreyId" }, operation.Columns);
                },
                o =>
                {
                    var operation = Assert.IsType<AddForeignKeyOperation>(o);
                    Assert.Equal("FK_Animal_Mice_MouseId", operation.Name);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal("Mice", operation.PrincipalTable);
                    Assert.Equal(new[] { "MouseId" }, operation.Columns);
                    Assert.Equal(new[] { "Id" }, operation.PrincipalColumns);
                    Assert.Equal(ReferentialAction.NoAction, operation.OnDelete);
                },
                o =>
                {
                    var operation = Assert.IsType<AddForeignKeyOperation>(o);
                    Assert.Equal("FK_Dogs_Animal", operation.Name);
                    Assert.Equal("Dogs", operation.Table);
                    Assert.Equal("Animal", operation.PrincipalTable);
                    Assert.Equal(new[] { "Id" }, operation.Columns);
                    Assert.Equal(new[] { "Id" }, operation.PrincipalColumns);
                    Assert.Equal(ReferentialAction.Cascade, operation.OnDelete);
                },
                o =>
                {
                    var operation = Assert.IsType<AddForeignKeyOperation>(o);
                    Assert.Equal("FK_Dogs_Animal_PreyId", operation.Name);
                    Assert.Equal("Dogs", operation.Table);
                    Assert.Equal("Animal", operation.PrincipalTable);
                    Assert.Equal(new[] { "PreyId" }, operation.Columns);
                    Assert.Equal(new[] { "Id" }, operation.PrincipalColumns);
                    Assert.Equal(ReferentialAction.NoAction, operation.OnDelete);
                }),
            downOps => Assert.Collection(
                downOps,
                o =>
                {
                    var operation = Assert.IsType<DropForeignKeyOperation>(o);
                    Assert.Equal("FK_Animal_Mice_MouseId", operation.Name);
                    Assert.Equal("Animal", operation.Table);
                },
                o =>
                {
                    var operation = Assert.IsType<DropForeignKeyOperation>(o);
                    Assert.Equal("FK_Dogs_Animal", operation.Name);
                    Assert.Equal("Dogs", operation.Table);
                },
                o =>
                {
                    var operation = Assert.IsType<DropForeignKeyOperation>(o);
                    Assert.Equal("FK_Dogs_Animal_PreyId", operation.Name);
                    Assert.Equal("Dogs", operation.Table);
                },
                o =>
                {
                    var operation = Assert.IsType<DropTableOperation>(o);
                    Assert.Equal("Cats", operation.Name);
                },
                o =>
                {
                    var operation = Assert.IsType<DropTableOperation>(o);
                    Assert.Equal("Mice", operation.Name);
                },
                o =>
                {
                    var operation = Assert.IsType<DropIndexOperation>(o);
                    Assert.Equal("IX_Dogs_PreyId", operation.Name);
                    Assert.Equal("Dogs", operation.Table);
                },
                o =>
                {
                    var operation = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Equal("Dogs", operation.Table);

                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(21, v));
                },
                o =>
                {
                    var operation = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Equal("Dogs", operation.Table);

                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(22, v));
                },
                o =>
                {
                    var operation = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Equal("Dogs", operation.Table);

                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(23, v));
                },
                o =>
                {
                    var operation = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(23, v));
                },
                o =>
                {
                    var operation = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Equal("Animal", operation.Table);

                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(33, v));
                },
                o =>
                {
                    var operation = Assert.IsType<AddColumnOperation>(o);
                    Assert.Equal("Discriminator", operation.Name);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal(typeof(string), operation.ClrType);
                },
                o =>
                {
                    var operation = Assert.IsType<AddColumnOperation>(o);
                    Assert.Equal("PreyId", operation.Name);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal(typeof(int), operation.ClrType);
                    Assert.True(operation.IsNullable);
                },
                o =>
                {
                    var operation = Assert.IsType<UpdateDataOperation>(o);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(11, v));

                    Assert.Equal(new[] { "Discriminator", "PreyId" }, operation.Columns);
                    Assert.Null(operation.ColumnTypes);
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Equal("Cat", v),
                        v => Assert.Null(v));
                },
                o =>
                {
                    var operation = Assert.IsType<UpdateDataOperation>(o);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(12, v));

                    Assert.Equal(new[] { "Discriminator", "MouseId", "PreyId" }, operation.Columns);
                    Assert.Null(operation.ColumnTypes);
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Equal("Cat", v),
                        v => Assert.Equal(32, v),
                        v => Assert.Null(v));
                },
                o =>
                {
                    var operation = Assert.IsType<UpdateDataOperation>(o);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(13, v));

                    Assert.Equal(new[] { "Discriminator", "MouseId", "PreyId" }, operation.Columns);
                    Assert.Null(operation.ColumnTypes);
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Equal("Cat", v),
                        v => Assert.Null(v),
                        v => Assert.Null(v));
                },
                o =>
                {
                    var operation = Assert.IsType<UpdateDataOperation>(o);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(21, v));

                    Assert.Equal(new[] { "Discriminator", "PreyId" }, operation.Columns);
                    Assert.Null(operation.ColumnTypes);
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Equal("Dog", v),
                        v => Assert.Equal(31, v));
                },
                o =>
                {
                    var operation = Assert.IsType<UpdateDataOperation>(o);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(22, v));

                    Assert.Equal(new[] { "Discriminator", "PreyId" }, operation.Columns);
                    Assert.Null(operation.ColumnTypes);
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Equal("Dog", v),
                        v => Assert.Equal(32, v));
                },
                o =>
                {
                    var operation = Assert.IsType<UpdateDataOperation>(o);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(31, v));

                    Assert.Equal(new[] { "Discriminator" }, operation.Columns);
                    Assert.Null(operation.ColumnTypes);
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Equal("Mouse", v));
                },
                o =>
                {
                    var operation = Assert.IsType<InsertDataOperation>(o);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal(new[] { "Id", "Discriminator", "MouseId" }, operation.Columns);
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Equal(32, v),
                        v => Assert.Equal("Mouse", v),
                        v => Assert.Null(v));
                },
                o =>
                {
                    var operation = Assert.IsType<CreateIndexOperation>(o);
                    Assert.Equal("IX_Animal_PreyId", operation.Name);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal(new[] { "PreyId" }, operation.Columns);
                },
                o =>
                {
                    var operation = Assert.IsType<AddForeignKeyOperation>(o);
                    Assert.Equal("FK_Animal_Animal_MouseId", operation.Name);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal("Animal", operation.PrincipalTable);
                    Assert.Equal(new[] { "MouseId" }, operation.Columns);
                    Assert.Equal(new[] { "Id" }, operation.PrincipalColumns);
                    Assert.Equal(ReferentialAction.NoAction, operation.OnDelete);
                },
                o =>
                {
                    var operation = Assert.IsType<AddForeignKeyOperation>(o);
                    Assert.Equal("FK_Animal_Animal_PreyId", operation.Name);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal("Animal", operation.PrincipalTable);
                    Assert.Equal(new[] { "PreyId" }, operation.Columns);
                    Assert.Equal(new[] { "Id" }, operation.PrincipalColumns);
                    Assert.Equal(ReferentialAction.NoAction, operation.OnDelete);
                }));

    [ConditionalFact]
    public void Change_TPH_to_TPT_with_FKs_and_seed_data_readonly_discriminator()
        => Execute(
            modelBuilder =>
            {
                modelBuilder.Entity(
                    "Animal", x =>
                    {
                        x.Property<int>("Id");
                        x.Property<int?>("MouseId");

                        x.HasOne("Mouse").WithMany().HasForeignKey("MouseId");
                    });
                modelBuilder.Entity(
                    "Cat", x =>
                    {
                        x.HasBaseType("Animal");
                        x.Property<int?>("PreyId").HasColumnName("PreyId");

                        x.HasOne("Animal").WithMany().HasForeignKey("PreyId");
                        x.HasData(
                            new { Id = 11, MouseId = 31 });
                    });
                modelBuilder.Entity(
                    "Dog", x =>
                    {
                        x.HasBaseType("Animal");
                        x.Property<int?>("PreyId").HasColumnName("PreyId");

                        x.HasOne("Animal").WithMany().HasForeignKey("PreyId");
                        x.HasData(
                            new { Id = 21, PreyId = 31 });
                    });
                modelBuilder.Entity(
                    "Mouse", x =>
                    {
                        x.HasBaseType("Animal");

                        x.HasData(
                            new { Id = 31 });
                    });
            },
            source =>
            {
                source.Entity(
                    "Cat", x =>
                    {
                        x.HasData(
                            new { Id = 12, MouseId = 32 },
                            new { Id = 13 });
                    });
                source.Entity(
                    "Dog", x =>
                    {
                        x.HasData(
                            new { Id = 22, PreyId = 32 });
                    });
                source.Entity(
                    "Mouse", x =>
                    {
                        x.HasData(
                            new { Id = 32 });
                    });
                source.Entity(
                    "UnrelatedDog", x =>
                    {
                        x.ToTable("Dogs");
                        x.Property<int>("Id");
                        x.Property<int?>("PreyId");
                    });
            },
            target =>
            {
                target.Entity(
                    "Cat", x =>
                    {
                        x.ToTable("Cats");
                        x.HasData(
                            new { Id = 12 },
                            new { Id = 13, MouseId = 32 });
                    });
                target.Entity(
                    "Dog", x =>
                    {
                        x.ToTable("Dogs");
                        x.HasData(
                            new { Id = 22, PreyId = 33 },
                            new { Id = 23 });
                    });
                target.Entity(
                    "Mouse", x =>
                    {
                        x.ToTable("Mice");
                        x.HasData(
                            new { Id = 33 });
                    });
            },
            upOps => Assert.Collection(
                upOps,
                o =>
                {
                    var operation = Assert.IsType<DropForeignKeyOperation>(o);
                    Assert.Equal("FK_Animal_Animal_MouseId", operation.Name);
                    Assert.Equal("Animal", operation.Table);
                },
                o =>
                {
                    var operation = Assert.IsType<DropForeignKeyOperation>(o);
                    Assert.Equal("FK_Animal_Animal_PreyId", operation.Name);
                    Assert.Equal("Animal", operation.Table);
                },
                o =>
                {
                    var operation = Assert.IsType<DropIndexOperation>(o);
                    Assert.Equal("IX_Animal_PreyId", operation.Name);
                    Assert.Equal("Animal", operation.Table);
                },
                o =>
                {
                    var operation = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(32, v));
                },
                o =>
                {
                    var operation = Assert.IsType<DropColumnOperation>(o);
                    Assert.Equal("Discriminator", operation.Name);
                    Assert.Equal("Animal", operation.Table);
                },
                o =>
                {
                    var operation = Assert.IsType<DropColumnOperation>(o);
                    Assert.Equal("PreyId", operation.Name);
                    Assert.Equal("Animal", operation.Table);
                },
                o =>
                {
                    var operation = Assert.IsType<CreateTableOperation>(o);
                    Assert.Equal("Cats", operation.Name);
                    Assert.Collection(
                        operation.Columns,
                        c =>
                        {
                            Assert.Equal("Id", c.Name);
                            Assert.Equal("default_int_mapping", c.ColumnType);
                            Assert.Equal("Cats", c.Table);
                            Assert.False(c.IsNullable);
                            Assert.False(c.IsRowVersion);
                            Assert.Null(c.IsUnicode);
                            Assert.Null(c.IsFixedLength);
                            Assert.Null(c.MaxLength);
                            Assert.Null(c.Precision);
                            Assert.Null(c.Scale);
                            Assert.Null(c.DefaultValue);
                            Assert.Null(c.DefaultValueSql);
                            Assert.Null(c.ComputedColumnSql);
                            Assert.Null(c.IsStored);
                            Assert.Null(c.Comment);
                            Assert.Null(c.Collation);
                        },
                        c =>
                        {
                            Assert.Equal("PreyId", c.Name);
                            Assert.Equal("default_int_mapping", c.ColumnType);
                            Assert.Equal("Cats", c.Table);
                            Assert.True(c.IsNullable);
                            Assert.False(c.IsRowVersion);
                            Assert.Null(c.IsUnicode);
                            Assert.Null(c.IsFixedLength);
                            Assert.Null(c.MaxLength);
                            Assert.Null(c.Precision);
                            Assert.Null(c.Scale);
                            Assert.Null(c.DefaultValue);
                            Assert.Null(c.DefaultValueSql);
                            Assert.Null(c.ComputedColumnSql);
                            Assert.Null(c.IsStored);
                            Assert.Null(c.Comment);
                            Assert.Null(c.Collation);
                        });

                    var pk = operation.PrimaryKey;
                    Assert.Equal("PK_Cats", pk.Name);
                    Assert.Equal("Cats", pk.Table);
                    Assert.Equal(new[] { "Id" }, pk.Columns);

                    Assert.Collection(
                        operation.ForeignKeys,
                        fk =>
                        {
                            Assert.Equal("FK_Cats_Animal_Id", fk.Name);
                            Assert.Equal("Cats", fk.Table);
                            Assert.Equal("Animal", fk.PrincipalTable);
                            Assert.Equal(new[] { "Id" }, fk.Columns);
                            Assert.Equal(new[] { "Id" }, fk.PrincipalColumns);
                            Assert.Equal(ReferentialAction.Cascade, fk.OnDelete);
                        },
                        fk =>
                        {
                            Assert.Equal("FK_Cats_Animal_PreyId", fk.Name);
                            Assert.Equal("Cats", fk.Table);
                            Assert.Equal("Animal", fk.PrincipalTable);
                            Assert.Equal(new[] { "PreyId" }, fk.Columns);
                            Assert.Equal(new[] { "Id" }, fk.PrincipalColumns);
                            Assert.Equal(ReferentialAction.NoAction, fk.OnDelete);
                        });

                    Assert.Empty(operation.UniqueConstraints);
                    Assert.Null(operation.Comment);
                    Assert.Empty(operation.CheckConstraints);
                },
                o =>
                {
                    var operation = Assert.IsType<CreateTableOperation>(o);
                    Assert.Equal("Mice", operation.Name);
                    Assert.Collection(
                        operation.Columns,
                        c =>
                        {
                            Assert.Equal("Id", c.Name);
                            Assert.Equal("default_int_mapping", c.ColumnType);
                            Assert.Equal("Mice", c.Table);
                            Assert.False(c.IsNullable);
                            Assert.False(c.IsRowVersion);
                            Assert.Null(c.IsUnicode);
                            Assert.Null(c.IsFixedLength);
                            Assert.Null(c.MaxLength);
                            Assert.Null(c.Precision);
                            Assert.Null(c.Scale);
                            Assert.Null(c.DefaultValue);
                            Assert.Null(c.DefaultValueSql);
                            Assert.Null(c.ComputedColumnSql);
                            Assert.Null(c.IsStored);
                            Assert.Null(c.Comment);
                            Assert.Null(c.Collation);
                        });

                    var pk = operation.PrimaryKey;
                    Assert.Equal("PK_Mice", pk.Name);
                    Assert.Equal("Mice", pk.Table);
                    Assert.Equal(new[] { "Id" }, pk.Columns);

                    var fk = operation.ForeignKeys.Single();
                    Assert.Equal("FK_Mice_Animal_Id", fk.Name);
                    Assert.Equal("Mice", fk.Table);
                    Assert.Equal("Animal", fk.PrincipalTable);
                    Assert.Equal(new[] { "Id" }, fk.Columns);
                    Assert.Equal(new[] { "Id" }, fk.PrincipalColumns);
                    Assert.Equal(ReferentialAction.Cascade, fk.OnDelete);

                    Assert.Empty(operation.UniqueConstraints);
                    Assert.Null(operation.Comment);
                    Assert.Empty(operation.CheckConstraints);
                },
                o =>
                {
                    var operation = Assert.IsType<UpdateDataOperation>(o);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(12, v));

                    Assert.Equal(new[] { "MouseId" }, operation.Columns);
                    Assert.Null(operation.ColumnTypes);
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Null(v));
                },
                o =>
                {
                    var operation = Assert.IsType<UpdateDataOperation>(o);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(13, v));

                    Assert.Equal(new[] { "MouseId" }, operation.Columns);
                    Assert.Null(operation.ColumnTypes);
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Equal(32, v));
                },
                o =>
                {
                    var operation = Assert.IsType<InsertDataOperation>(o);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal(new[] { "Id", "MouseId" }, operation.Columns);
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Equal(23, v),
                        v => Assert.Null(v),
                        v => Assert.Equal(33, v),
                        v => Assert.Null(v));
                },
                o =>
                {
                    var operation = Assert.IsType<InsertDataOperation>(o);
                    Assert.Equal("Cats", operation.Table);
                    Assert.Equal(new[] { "Id", "PreyId" }, operation.Columns);
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Equal(11, v),
                        v => Assert.Null(v),
                        v => Assert.Equal(12, v),
                        v => Assert.Null(v),
                        v => Assert.Equal(13, v),
                        v => Assert.Null(v));
                },
                o =>
                {
                    var operation = Assert.IsType<InsertDataOperation>(o);
                    Assert.Equal("Dogs", operation.Table);

                    Assert.Equal(new[] { "Id", "PreyId" }, operation.Columns);
                    Assert.Null(operation.ColumnTypes);
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Equal(21, v),
                        v => Assert.Equal(31, v));
                },
                o =>
                {
                    var operation = Assert.IsType<InsertDataOperation>(o);
                    Assert.Equal("Mice", operation.Table);
                    Assert.Equal(new[] { "Id" }, operation.Columns);
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Equal(31, v));
                },
                o =>
                {
                    var operation = Assert.IsType<InsertDataOperation>(o);
                    Assert.Equal("Dogs", operation.Table);

                    Assert.Equal(new[] { "Id", "PreyId" }, operation.Columns);
                    Assert.Null(operation.ColumnTypes);
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Equal(22, v),
                        v => Assert.Equal(33, v),
                        v => Assert.Equal(23, v),
                        v => Assert.Null(v));
                },
                o =>
                {
                    var operation = Assert.IsType<InsertDataOperation>(o);
                    Assert.Equal("Mice", operation.Table);
                    Assert.Equal(new[] { "Id" }, operation.Columns);
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Equal(33, v));
                },
                o =>
                {
                    var operation = Assert.IsType<CreateIndexOperation>(o);
                    Assert.Equal("IX_Dogs_PreyId", operation.Name);
                    Assert.Equal("Dogs", operation.Table);
                    Assert.Equal(new[] { "PreyId" }, operation.Columns);
                },
                o =>
                {
                    var operation = Assert.IsType<CreateIndexOperation>(o);
                    Assert.Equal("IX_Cats_PreyId", operation.Name);
                    Assert.Equal("Cats", operation.Table);
                    Assert.Equal(new[] { "PreyId" }, operation.Columns);
                },
                o =>
                {
                    var operation = Assert.IsType<AddForeignKeyOperation>(o);
                    Assert.Equal("FK_Animal_Mice_MouseId", operation.Name);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal("Mice", operation.PrincipalTable);
                    Assert.Equal(new[] { "MouseId" }, operation.Columns);
                    Assert.Equal(new[] { "Id" }, operation.PrincipalColumns);
                    Assert.Equal(ReferentialAction.NoAction, operation.OnDelete);
                },
                o =>
                {
                    var operation = Assert.IsType<AddForeignKeyOperation>(o);
                    Assert.Equal("FK_Dogs_Animal_Id", operation.Name);
                    Assert.Equal("Dogs", operation.Table);
                    Assert.Equal("Animal", operation.PrincipalTable);
                    Assert.Equal(new[] { "Id" }, operation.Columns);
                    Assert.Equal(new[] { "Id" }, operation.PrincipalColumns);
                    Assert.Equal(ReferentialAction.Cascade, operation.OnDelete);
                },
                o =>
                {
                    var operation = Assert.IsType<AddForeignKeyOperation>(o);
                    Assert.Equal("FK_Dogs_Animal_PreyId", operation.Name);
                    Assert.Equal("Dogs", operation.Table);
                    Assert.Equal("Animal", operation.PrincipalTable);
                    Assert.Equal(new[] { "PreyId" }, operation.Columns);
                    Assert.Equal(new[] { "Id" }, operation.PrincipalColumns);
                    Assert.Equal(ReferentialAction.NoAction, operation.OnDelete);
                }),
            downOps => Assert.Collection(
                downOps,
                o =>
                {
                    var operation = Assert.IsType<DropForeignKeyOperation>(o);
                    Assert.Equal("FK_Animal_Mice_MouseId", operation.Name);
                    Assert.Equal("Animal", operation.Table);
                },
                o =>
                {
                    var operation = Assert.IsType<DropForeignKeyOperation>(o);
                    Assert.Equal("FK_Dogs_Animal_Id", operation.Name);
                    Assert.Equal("Dogs", operation.Table);
                },
                o =>
                {
                    var operation = Assert.IsType<DropForeignKeyOperation>(o);
                    Assert.Equal("FK_Dogs_Animal_PreyId", operation.Name);
                    Assert.Equal("Dogs", operation.Table);
                },
                o =>
                {
                    var operation = Assert.IsType<DropTableOperation>(o);
                    Assert.Equal("Cats", operation.Name);
                },
                o =>
                {
                    var operation = Assert.IsType<DropTableOperation>(o);
                    Assert.Equal("Mice", operation.Name);
                },
                o =>
                {
                    var operation = Assert.IsType<DropIndexOperation>(o);
                    Assert.Equal("IX_Dogs_PreyId", operation.Name);
                    Assert.Equal("Dogs", operation.Table);
                },
                o =>
                {
                    var operation = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(11, v));
                },
                o =>
                {
                    var operation = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(12, v));
                },
                o =>
                {
                    var operation = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(13, v));
                },
                o =>
                {
                    var operation = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Equal("Dogs", operation.Table);

                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(21, v));
                },
                o =>
                {
                    var operation = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Equal("Dogs", operation.Table);

                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(22, v));
                },
                o =>
                {
                    var operation = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Equal("Dogs", operation.Table);

                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(23, v));
                },
                o =>
                {
                    var operation = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(21, v));
                },
                o =>
                {
                    var operation = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(22, v));
                },
                o =>
                {
                    var operation = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(23, v));
                },
                o =>
                {
                    var operation = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(31, v));
                },
                o =>
                {
                    var operation = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Equal("Animal", operation.Table);

                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(33, v));
                },
                o =>
                {
                    var operation = Assert.IsType<AddColumnOperation>(o);
                    Assert.Equal("Discriminator", operation.Name);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal(typeof(string), operation.ClrType);
                },
                o =>
                {
                    var operation = Assert.IsType<AddColumnOperation>(o);
                    Assert.Equal("PreyId", operation.Name);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal(typeof(int), operation.ClrType);
                },
                o =>
                {
                    var operation = Assert.IsType<InsertDataOperation>(o);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal(new[] { "Id", "Discriminator", "MouseId", "PreyId" }, operation.Columns);
                    Assert.Null(operation.ColumnTypes);
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Equal(13, v),
                        v => Assert.Equal("Cat", v),
                        v => Assert.Null(v),
                        v => Assert.Null(v));
                },
                o =>
                {
                    var operation = Assert.IsType<InsertDataOperation>(o);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal(new[] { "Id", "Discriminator", "MouseId" }, operation.Columns);
                    Assert.Null(operation.ColumnTypes);
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Equal(31, v),
                        v => Assert.Equal("Mouse", v),
                        v => Assert.Null(v),
                        v => Assert.Equal(32, v),
                        v => Assert.Equal("Mouse", v),
                        v => Assert.Null(v));
                },
                o =>
                {
                    var operation = Assert.IsType<InsertDataOperation>(o);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal(new[] { "Id", "Discriminator", "MouseId", "PreyId" }, operation.Columns);
                    Assert.Null(operation.ColumnTypes);
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Equal(11, v),
                        v => Assert.Equal("Cat", v),
                        v => Assert.Equal(31, v),
                        v => Assert.Null(v),
                        v => Assert.Equal(12, v),
                        v => Assert.Equal("Cat", v),
                        v => Assert.Equal(32, v),
                        v => Assert.Null(v),
                        v => Assert.Equal(21, v),
                        v => Assert.Equal("Dog", v),
                        v => Assert.Null(v),
                        v => Assert.Equal(31, v),
                        v => Assert.Equal(22, v),
                        v => Assert.Equal("Dog", v),
                        v => Assert.Null(v),
                        v => Assert.Equal(32, v));
                },
                o =>
                {
                    var operation = Assert.IsType<CreateIndexOperation>(o);
                    Assert.Equal("IX_Animal_PreyId", operation.Name);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal(new[] { "PreyId" }, operation.Columns);
                },
                o =>
                {
                    var operation = Assert.IsType<AddForeignKeyOperation>(o);
                    Assert.Equal("FK_Animal_Animal_MouseId", operation.Name);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal("Animal", operation.PrincipalTable);
                    Assert.Equal(new[] { "MouseId" }, operation.Columns);
                    Assert.Equal(new[] { "Id" }, operation.PrincipalColumns);
                    Assert.Equal(ReferentialAction.NoAction, operation.OnDelete);
                },
                o =>
                {
                    var operation = Assert.IsType<AddForeignKeyOperation>(o);
                    Assert.Equal("FK_Animal_Animal_PreyId", operation.Name);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal("Animal", operation.PrincipalTable);
                    Assert.Equal(new[] { "PreyId" }, operation.Columns);
                    Assert.Equal(new[] { "Id" }, operation.PrincipalColumns);
                    Assert.Equal(ReferentialAction.NoAction, operation.OnDelete);
                }));

    [ConditionalFact]
    public void Change_TPH_to_TPC_with_FKs_and_seed_data()
        => Execute(
            modelBuilder =>
            {
                modelBuilder.Entity(
                    "Animal", x =>
                    {
                        x.Property<int>("Id");
                        x.Property<int?>("MouseId");

                        x.HasOne("Mouse").WithMany().HasForeignKey("MouseId");
                    });
                modelBuilder.Entity(
                    "Cat", x =>
                    {
                        x.HasBaseType("Animal");
                        x.Property<int?>("PreyId").HasColumnName("PreyId");

                        x.HasOne("Animal").WithMany().HasForeignKey("PreyId");
                        x.HasData(
                            new { Id = 11, MouseId = 31 });
                    });
                modelBuilder.Entity(
                    "Dog", x =>
                    {
                        x.HasBaseType("Animal");
                        x.Property<int?>("PreyId").HasColumnName("PreyId");

                        x.HasOne("Animal").WithMany().HasForeignKey("PreyId");
                        x.HasData(
                            new { Id = 21, PreyId = 31 });
                    });
                modelBuilder.Entity(
                    "Mouse", x =>
                    {
                        x.HasBaseType("Animal");

                        x.HasData(
                            new { Id = 31 });
                    });
            },
            source =>
            {
                source.Entity(
                    "Animal", x =>
                    {
                        x.Property<string>("Discriminator").Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Save);
                    });
                source.Entity(
                    "Cat", x =>
                    {
                        x.HasData(
                            new { Id = 12, MouseId = 32 },
                            new { Id = 13 });
                    });
                source.Entity(
                    "Dog", x =>
                    {
                        x.HasData(
                            new { Id = 22, PreyId = 32 });
                    });
                source.Entity(
                    "Mouse", x =>
                    {
                        x.HasData(
                            new { Id = 32 });
                    });
                source.Entity(
                    "UnrelatedDog", x =>
                    {
                        x.ToTable("Dogs");
                        x.Property<int>("Id");
                        x.Property<int?>("PreyId");
                    });
            },
            target =>
            {
                target.Entity(
                    "Animal", x =>
                    {
                        x.UseTpcMappingStrategy();
                    });
                target.Entity(
                    "Cat", x =>
                    {
                        x.ToTable("Cats");
                        x.HasData(
                            new { Id = 12 },
                            new { Id = 13, MouseId = 32 });
                    });
                target.Entity(
                    "Dog", x =>
                    {
                        x.ToTable("Dogs");
                        x.HasOne("Animal", null).WithOne().HasForeignKey("Dog", "Id")
                            .HasConstraintName("FK_Dogs_Animal");
                        x.HasData(
                            new { Id = 22, PreyId = 33 },
                            new { Id = 23 });
                    });
                target.Entity(
                    "Mouse", x =>
                    {
                        x.ToTable("Mice");
                        x.HasData(
                            new { Id = 33 });
                    });
            },
            upOps => Assert.Collection(
                upOps,
                o =>
                {
                    var operation = Assert.IsType<DropForeignKeyOperation>(o);
                    Assert.Equal("FK_Animal_Animal_MouseId", operation.Name);
                    Assert.Equal("Animal", operation.Table);
                },
                o =>
                {
                    var operation = Assert.IsType<DropForeignKeyOperation>(o);
                    Assert.Equal("FK_Animal_Animal_PreyId", operation.Name);
                    Assert.Equal("Animal", operation.Table);
                },
                o =>
                {
                    var operation = Assert.IsType<DropIndexOperation>(o);
                    Assert.Equal("IX_Animal_PreyId", operation.Name);
                    Assert.Equal("Animal", operation.Table);
                },
                o =>
                {
                    var operation = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(11, v));
                },
                o =>
                {
                    var operation = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(12, v));
                },
                o =>
                {
                    var operation = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(13, v));
                },
                o =>
                {
                    var operation = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(21, v));
                },
                o =>
                {
                    var operation = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(22, v));
                },
                o =>
                {
                    var operation = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(31, v));
                },
                o =>
                {
                    var operation = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(32, v));
                },
                o =>
                {
                    var operation = Assert.IsType<DropColumnOperation>(o);
                    Assert.Equal("Discriminator", operation.Name);
                    Assert.Equal("Animal", operation.Table);
                },
                o =>
                {
                    var operation = Assert.IsType<DropColumnOperation>(o);
                    Assert.Equal("PreyId", operation.Name);
                    Assert.Equal("Animal", operation.Table);
                },
                o =>
                {
                    var operation = Assert.IsType<AddColumnOperation>(o);
                    Assert.Equal("MouseId", operation.Name);
                    Assert.Equal("Dogs", operation.Table);
                    Assert.Equal(typeof(int), operation.ClrType);
                },
                o =>
                {
                    var operation = Assert.IsType<CreateTableOperation>(o);
                    Assert.Equal("Mice", operation.Name);
                    Assert.Collection(
                        operation.Columns,
                        c =>
                        {
                            Assert.Equal("Id", c.Name);
                            Assert.Equal("default_int_mapping", c.ColumnType);
                            Assert.Equal("Mice", c.Table);
                            Assert.False(c.IsNullable);
                            Assert.False(c.IsRowVersion);
                            Assert.Null(c.IsUnicode);
                            Assert.Null(c.IsFixedLength);
                            Assert.Null(c.MaxLength);
                            Assert.Null(c.Precision);
                            Assert.Null(c.Scale);
                            Assert.Null(c.DefaultValue);
                            Assert.Null(c.DefaultValueSql);
                            Assert.Null(c.ComputedColumnSql);
                            Assert.Null(c.IsStored);
                            Assert.Null(c.Comment);
                            Assert.Null(c.Collation);
                        },
                        c =>
                        {
                            Assert.Equal("MouseId", c.Name);
                            Assert.Equal("default_int_mapping", c.ColumnType);
                            Assert.Equal("Mice", c.Table);
                            Assert.True(c.IsNullable);
                        });

                    var pk = operation.PrimaryKey;
                    Assert.Equal("PK_Mice", pk.Name);
                    Assert.Equal("Mice", pk.Table);
                    Assert.Equal(new[] { "Id" }, pk.Columns);

                    var fk = operation.ForeignKeys.Single();
                    Assert.Equal("FK_Mice_Mice_MouseId", fk.Name);
                    Assert.Equal("Mice", fk.Table);
                    Assert.Equal("Mice", fk.PrincipalTable);
                    Assert.Equal(new[] { "MouseId" }, fk.Columns);
                    Assert.Equal(new[] { "Id" }, fk.PrincipalColumns);
                    Assert.Equal(ReferentialAction.NoAction, fk.OnDelete);

                    Assert.Empty(operation.UniqueConstraints);
                    Assert.Null(operation.Comment);
                    Assert.Empty(operation.CheckConstraints);
                },
                o =>
                {
                    var operation = Assert.IsType<CreateTableOperation>(o);
                    Assert.Equal("Cats", operation.Name);
                    Assert.Collection(
                        operation.Columns,
                        c =>
                        {
                            Assert.Equal("Id", c.Name);
                            Assert.Equal("default_int_mapping", c.ColumnType);
                            Assert.Equal("Cats", c.Table);
                            Assert.False(c.IsNullable);
                        },
                        c =>
                        {
                            Assert.Equal("MouseId", c.Name);
                            Assert.Equal("default_int_mapping", c.ColumnType);
                            Assert.Equal("Cats", c.Table);
                            Assert.True(c.IsNullable);
                        },
                        c =>
                        {
                            Assert.Equal("PreyId", c.Name);
                            Assert.Equal("default_int_mapping", c.ColumnType);
                            Assert.Equal("Cats", c.Table);
                            Assert.True(c.IsNullable);
                        });

                    var pk = operation.PrimaryKey;
                    Assert.Equal("PK_Cats", pk.Name);
                    Assert.Equal("Cats", pk.Table);
                    Assert.Equal(new[] { "Id" }, pk.Columns);

                    Assert.Collection(
                        operation.ForeignKeys,
                        fk =>
                        {
                            Assert.Equal("FK_Cats_Mice_MouseId", fk.Name);
                            Assert.Equal("Cats", fk.Table);
                            Assert.Equal("Mice", fk.PrincipalTable);
                            Assert.Equal(new[] { "MouseId" }, fk.Columns);
                            Assert.Equal(new[] { "Id" }, fk.PrincipalColumns);
                            Assert.Equal(ReferentialAction.NoAction, fk.OnDelete);
                        });

                    Assert.Empty(operation.UniqueConstraints);
                    Assert.Null(operation.Comment);
                    Assert.Empty(operation.CheckConstraints);
                },
                o =>
                {
                    var operation = Assert.IsType<InsertDataOperation>(o);
                    Assert.Equal("Cats", operation.Table);
                    Assert.Equal(new[] { "Id", "MouseId", "PreyId" }, operation.Columns);
                    Assert.Null(operation.ColumnTypes);
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Equal(12, v),
                        v => Assert.Null(v),
                        v => Assert.Null(v),
                        v => Assert.Equal(13, v),
                        v => Assert.Equal(32, v),
                        v => Assert.Null(v));
                },
                o =>
                {
                    var operation = Assert.IsType<InsertDataOperation>(o);
                    Assert.Equal("Dogs", operation.Table);
                    Assert.Equal(new[] { "Id", "MouseId", "PreyId" }, operation.Columns);
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Equal(21, v),
                        v => Assert.Null(v),
                        v => Assert.Equal(31, v),
                        v => Assert.Equal(22, v),
                        v => Assert.Null(v),
                        v => Assert.Equal(33, v),
                        v => Assert.Equal(23, v),
                        v => Assert.Null(v),
                        v => Assert.Null(v));
                },
                o =>
                {
                    var operation = Assert.IsType<InsertDataOperation>(o);
                    Assert.Equal("Mice", operation.Table);
                    Assert.Equal(new[] { "Id", "MouseId" }, operation.Columns);
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Equal(31, v),
                        v => Assert.Null(v),
                        v => Assert.Equal(33, v),
                        v => Assert.Null(v));
                },
                o =>
                {
                    var operation = Assert.IsType<InsertDataOperation>(o);
                    Assert.Equal("Cats", operation.Table);
                    Assert.Equal(new[] { "Id", "MouseId", "PreyId" }, operation.Columns);
                    Assert.Null(operation.ColumnTypes);
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Equal(11, v),
                        v => Assert.Equal(31, v),
                        v => Assert.Null(v));
                },
                o =>
                {
                    var operation = Assert.IsType<CreateIndexOperation>(o);
                    Assert.Equal("IX_Dogs_MouseId", operation.Name);
                    Assert.Equal("Dogs", operation.Table);
                    Assert.Equal(new[] { "MouseId" }, operation.Columns);
                },
                o =>
                {
                    var operation = Assert.IsType<CreateIndexOperation>(o);
                    Assert.Equal("IX_Dogs_PreyId", operation.Name);
                    Assert.Equal("Dogs", operation.Table);
                    Assert.Equal(new[] { "PreyId" }, operation.Columns);
                },
                o =>
                {
                    var operation = Assert.IsType<CreateIndexOperation>(o);
                    Assert.Equal("IX_Cats_MouseId", operation.Name);
                    Assert.Equal("Cats", operation.Table);
                    Assert.Equal(new[] { "MouseId" }, operation.Columns);
                },
                o =>
                {
                    var operation = Assert.IsType<CreateIndexOperation>(o);
                    Assert.Equal("IX_Cats_PreyId", operation.Name);
                    Assert.Equal("Cats", operation.Table);
                    Assert.Equal(new[] { "PreyId" }, operation.Columns);
                },
                o =>
                {
                    var operation = Assert.IsType<CreateIndexOperation>(o);
                    Assert.Equal("IX_Mice_MouseId", operation.Name);
                    Assert.Equal("Mice", operation.Table);
                    Assert.Equal(new[] { "MouseId" }, operation.Columns);
                },
                o =>
                {
                    var operation = Assert.IsType<AddForeignKeyOperation>(o);
                    Assert.Equal("FK_Animal_Mice_MouseId", operation.Name);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal("Mice", operation.PrincipalTable);
                    Assert.Equal(new[] { "MouseId" }, operation.Columns);
                    Assert.Equal(new[] { "Id" }, operation.PrincipalColumns);
                    Assert.Equal(ReferentialAction.NoAction, operation.OnDelete);
                },
                o =>
                {
                    var operation = Assert.IsType<AddForeignKeyOperation>(o);
                    Assert.Equal("FK_Dogs_Mice_MouseId", operation.Name);
                    Assert.Equal("Dogs", operation.Table);
                    Assert.Equal("Mice", operation.PrincipalTable);
                    Assert.Equal(new[] { "MouseId" }, operation.Columns);
                    Assert.Equal(new[] { "Id" }, operation.PrincipalColumns);
                    Assert.Equal(ReferentialAction.NoAction, operation.OnDelete);
                }),
            downOps => Assert.Collection(
                downOps,
                o =>
                {
                    var operation = Assert.IsType<DropForeignKeyOperation>(o);
                    Assert.Equal("FK_Animal_Mice_MouseId", operation.Name);
                    Assert.Equal("Animal", operation.Table);
                },
                o =>
                {
                    var operation = Assert.IsType<DropForeignKeyOperation>(o);
                    Assert.Equal("FK_Dogs_Mice_MouseId", operation.Name);
                    Assert.Equal("Dogs", operation.Table);
                },
                o =>
                {
                    var operation = Assert.IsType<DropTableOperation>(o);
                    Assert.Equal("Cats", operation.Name);
                },
                o =>
                {
                    var operation = Assert.IsType<DropTableOperation>(o);
                    Assert.Equal("Mice", operation.Name);
                },
                o =>
                {
                    var operation = Assert.IsType<DropIndexOperation>(o);
                    Assert.Equal("IX_Dogs_MouseId", operation.Name);
                    Assert.Equal("Dogs", operation.Table);
                },
                o =>
                {
                    var operation = Assert.IsType<DropIndexOperation>(o);
                    Assert.Equal("IX_Dogs_PreyId", operation.Name);
                    Assert.Equal("Dogs", operation.Table);
                },
                o =>
                {
                    var operation = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Equal("Dogs", operation.Table);

                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(21, v));
                },
                o =>
                {
                    var operation = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Equal("Dogs", operation.Table);

                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(22, v));
                },
                o =>
                {
                    var operation = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Equal("Dogs", operation.Table);

                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(23, v));
                },
                o =>
                {
                    var operation = Assert.IsType<DropColumnOperation>(o);
                    Assert.Equal("MouseId", operation.Name);
                    Assert.Equal("Dogs", operation.Table);
                },
                o =>
                {
                    var operation = Assert.IsType<AddColumnOperation>(o);
                    Assert.Equal("Discriminator", operation.Name);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal(typeof(string), operation.ClrType);
                },
                o =>
                {
                    var operation = Assert.IsType<AddColumnOperation>(o);
                    Assert.Equal("PreyId", operation.Name);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal(typeof(int), operation.ClrType);
                },
                o =>
                {
                    var operation = Assert.IsType<InsertDataOperation>(o);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal(new[] { "Id", "Discriminator", "MouseId", "PreyId" }, operation.Columns);
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Equal(13, v),
                        v => Assert.Equal("Cat", v),
                        v => Assert.Null(v),
                        v => Assert.Null(v));
                },
                o =>
                {
                    var operation = Assert.IsType<InsertDataOperation>(o);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal(new[] { "Id", "Discriminator", "MouseId" }, operation.Columns);
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Equal(31, v),
                        v => Assert.Equal("Mouse", v),
                        v => Assert.Null(v),
                        v => Assert.Equal(32, v),
                        v => Assert.Equal("Mouse", v),
                        v => Assert.Null(v));
                },
                o =>
                {
                    var operation = Assert.IsType<InsertDataOperation>(o);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal(new[] { "Id", "Discriminator", "MouseId", "PreyId" }, operation.Columns);
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Equal(11, v),
                        v => Assert.Equal("Cat", v),
                        v => Assert.Equal(31, v),
                        v => Assert.Null(v),
                        v => Assert.Equal(12, v),
                        v => Assert.Equal("Cat", v),
                        v => Assert.Equal(32, v),
                        v => Assert.Null(v),
                        v => Assert.Equal(21, v),
                        v => Assert.Equal("Dog", v),
                        v => Assert.Null(v),
                        v => Assert.Equal(31, v),
                        v => Assert.Equal(22, v),
                        v => Assert.Equal("Dog", v),
                        v => Assert.Null(v),
                        v => Assert.Equal(32, v));
                },
                o =>
                {
                    var operation = Assert.IsType<CreateIndexOperation>(o);
                    Assert.Equal("IX_Animal_PreyId", operation.Name);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal(new[] { "PreyId" }, operation.Columns);
                },
                o =>
                {
                    var operation = Assert.IsType<AddForeignKeyOperation>(o);
                    Assert.Equal("FK_Animal_Animal_MouseId", operation.Name);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal("Animal", operation.PrincipalTable);
                    Assert.Equal(new[] { "MouseId" }, operation.Columns);
                    Assert.Equal(new[] { "Id" }, operation.PrincipalColumns);
                    Assert.Equal(ReferentialAction.NoAction, operation.OnDelete);
                },
                o =>
                {
                    var operation = Assert.IsType<AddForeignKeyOperation>(o);
                    Assert.Equal("FK_Animal_Animal_PreyId", operation.Name);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal("Animal", operation.PrincipalTable);
                    Assert.Equal(new[] { "PreyId" }, operation.Columns);
                    Assert.Equal(new[] { "Id" }, operation.PrincipalColumns);
                    Assert.Equal(ReferentialAction.NoAction, operation.OnDelete);
                }));

    [ConditionalFact]
    public void Change_TPT_to_TPC_with_FKs_and_seed_data()
        => Execute(
            modelBuilder =>
            {
                modelBuilder.Entity(
                    "Animal", x =>
                    {
                        x.Property<int>("Id");
                        x.Property<int?>("MouseId");

                        x.HasOne("Mouse").WithMany().HasForeignKey("MouseId");
                    });
                modelBuilder.Entity(
                    "Cat", x =>
                    {
                        x.HasBaseType("Animal");
                        x.Property<int?>("PreyId").HasColumnName("PreyId");

                        x.HasOne("Animal").WithMany().HasForeignKey("PreyId");
                        x.HasData(
                            new { Id = 11, MouseId = 31 });
                    });
                modelBuilder.Entity(
                    "Dog", x =>
                    {
                        x.HasBaseType("Animal");
                        x.Property<int?>("PreyId").HasColumnName("PreyId");

                        x.HasOne("Animal").WithMany().HasForeignKey("PreyId");
                        x.HasData(
                            new { Id = 21, PreyId = 31 });
                    });
                modelBuilder.Entity(
                    "Mouse", x =>
                    {
                        x.HasBaseType("Animal");

                        x.HasData(
                            new { Id = 31 });
                    });
            },
            source =>
            {
                source.Entity(
                    "Animal", x =>
                    {
                        x.UseTptMappingStrategy();
                    });
                source.Entity(
                    "Cat", x =>
                    {
                        x.ToTable("Cats");
                        x.HasData(
                            new { Id = 12, MouseId = 32 },
                            new { Id = 13 });
                    });
                source.Entity(
                    "Dog", x =>
                    {
                        x.ToTable("Dogs");
                        x.HasData(
                            new { Id = 22, PreyId = 32 });
                    });
                source.Entity(
                    "Mouse", x =>
                    {
                        x.ToTable("Mice");
                        x.HasData(
                            new { Id = 32 });
                    });
                source.Entity(
                    "UnrelatedDog", x =>
                    {
                        x.ToTable("Dogs");
                        x.Property<int>("Id");
                        x.Property<int?>("PreyId");
                    });
            },
            target =>
            {
                target.Entity(
                    "Animal", x =>
                    {
                        x.UseTpcMappingStrategy();
                    });
                target.Entity(
                    "Cat", x =>
                    {
                        x.ToTable("Cats");
                        x.HasData(
                            new { Id = 12 },
                            new { Id = 13, MouseId = 32 });
                    });
                target.Entity(
                    "Dog", x =>
                    {
                        x.ToTable("Dogs");
                        x.HasOne("Animal", null).WithOne().HasForeignKey("Dog", "Id")
                            .HasConstraintName("FK_Dogs_Animal");
                        x.HasData(
                            new { Id = 22, PreyId = 33 },
                            new { Id = 23 });
                    });
                target.Entity(
                    "Mouse", x =>
                    {
                        x.ToTable("Mice");
                        x.HasData(
                            new { Id = 33 });
                    });
            },
            upOps => Assert.Collection(
                upOps,
                o =>
                {
                    var operation = Assert.IsType<DropForeignKeyOperation>(o);
                    Assert.Equal("FK_Cats_Animal_Id", operation.Name);
                    Assert.Equal("Cats", operation.Table);
                },
                o =>
                {
                    var operation = Assert.IsType<DropForeignKeyOperation>(o);
                    Assert.Equal("FK_Cats_Animal_PreyId", operation.Name);
                    Assert.Equal("Cats", operation.Table);
                },
                o =>
                {
                    var operation = Assert.IsType<DropForeignKeyOperation>(o);
                    Assert.Equal("FK_Dogs_Animal_Id", operation.Name);
                    Assert.Equal("Dogs", operation.Table);
                },
                o =>
                {
                    var operation = Assert.IsType<DropForeignKeyOperation>(o);
                    Assert.Equal("FK_Dogs_Animal_PreyId", operation.Name);
                    Assert.Equal("Dogs", operation.Table);
                },
                o =>
                {
                    var operation = Assert.IsType<DropForeignKeyOperation>(o);
                    Assert.Equal("FK_Mice_Animal_Id", operation.Name);
                    Assert.Equal("Mice", operation.Table);
                },
                o =>
                {
                    var operation = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(11, v));
                },
                o =>
                {
                    var operation = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(12, v));
                },
                o =>
                {
                    var operation = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(13, v));
                },
                o =>
                {
                    var operation = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(21, v));
                },
                o =>
                {
                    var operation = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(22, v));
                },
                o =>
                {
                    var operation = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(31, v));
                },
                o =>
                {
                    var operation = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Equal("Mice", operation.Table);
                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(32, v));
                },
                o =>
                {
                    var operation = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(32, v));
                },
                o =>
                {
                    var operation = Assert.IsType<AddColumnOperation>(o);
                    Assert.Equal("MouseId", operation.Name);
                    Assert.Equal("Mice", operation.Table);
                },
                o =>
                {
                    var operation = Assert.IsType<AddColumnOperation>(o);
                    Assert.Equal("MouseId", operation.Name);
                    Assert.Equal("Dogs", operation.Table);
                },
                o =>
                {
                    var operation = Assert.IsType<AddColumnOperation>(o);
                    Assert.Equal("MouseId", operation.Name);
                    Assert.Equal("Cats", operation.Table);
                },
                o =>
                {
                    var operation = Assert.IsType<UpdateDataOperation>(o);
                    Assert.Equal("Cats", operation.Table);
                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(11, v));

                    Assert.Equal(new[] { "MouseId" }, operation.Columns);
                    Assert.Null(operation.ColumnTypes);
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Equal(31, v));
                },
                o =>
                {
                    var operation = Assert.IsType<UpdateDataOperation>(o);
                    Assert.Equal("Cats", operation.Table);
                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(12, v));

                    Assert.Equal(new[] { "MouseId" }, operation.Columns);
                    Assert.Null(operation.ColumnTypes);
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Null(v));
                },
                o =>
                {
                    var operation = Assert.IsType<UpdateDataOperation>(o);
                    Assert.Equal("Cats", operation.Table);
                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(13, v));

                    Assert.Equal(new[] { "MouseId" }, operation.Columns);
                    Assert.Null(operation.ColumnTypes);
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Equal(32, v));
                },
                o =>
                {
                    var operation = Assert.IsType<UpdateDataOperation>(o);
                    Assert.Equal("Dogs", operation.Table);
                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(21, v));

                    Assert.Equal(new[] { "MouseId" }, operation.Columns);
                    Assert.Null(operation.ColumnTypes);
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Null(v));
                },
                o =>
                {
                    var operation = Assert.IsType<UpdateDataOperation>(o);
                    Assert.Equal("Dogs", operation.Table);
                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(22, v));

                    Assert.Equal(new[] { "MouseId", "PreyId" }, operation.Columns);
                    Assert.Null(operation.ColumnTypes);
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Null(v),
                        v => Assert.Equal(33, v));
                },
                o =>
                {
                    var operation = Assert.IsType<InsertDataOperation>(o);
                    Assert.Equal("Dogs", operation.Table);

                    Assert.Equal(new[] { "Id", "MouseId", "PreyId" }, operation.Columns);
                    Assert.Null(operation.ColumnTypes);
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Equal(23, v),
                        v => Assert.Null(v),
                        v => Assert.Null(v));
                },
                o =>
                {
                    var operation = Assert.IsType<UpdateDataOperation>(o);
                    Assert.Equal("Mice", operation.Table);
                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(31, v));

                    Assert.Equal(new[] { "MouseId" }, operation.Columns);
                    Assert.Null(operation.ColumnTypes);
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Null(v));
                },
                o =>
                {
                    var operation = Assert.IsType<InsertDataOperation>(o);
                    Assert.Equal("Mice", operation.Table);
                    Assert.Equal(new[] { "Id", "MouseId" }, operation.Columns);
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Equal(33, v),
                        v => Assert.Null(v));
                },
                o =>
                {
                    var operation = Assert.IsType<CreateIndexOperation>(o);
                    Assert.Equal("IX_Mice_MouseId", operation.Name);
                    Assert.Equal("Mice", operation.Table);
                    Assert.Equal(new[] { "MouseId" }, operation.Columns);
                },
                o =>
                {
                    var operation = Assert.IsType<CreateIndexOperation>(o);
                    Assert.Equal("IX_Dogs_MouseId", operation.Name);
                    Assert.Equal("Dogs", operation.Table);
                    Assert.Equal(new[] { "MouseId" }, operation.Columns);
                },
                o =>
                {
                    var operation = Assert.IsType<CreateIndexOperation>(o);
                    Assert.Equal("IX_Cats_MouseId", operation.Name);
                    Assert.Equal("Cats", operation.Table);
                    Assert.Equal(new[] { "MouseId" }, operation.Columns);
                },
                o =>
                {
                    var operation = Assert.IsType<AddForeignKeyOperation>(o);
                    Assert.Equal("FK_Cats_Mice_MouseId", operation.Name);
                    Assert.Equal("Cats", operation.Table);
                    Assert.Equal("Mice", operation.PrincipalTable);
                    Assert.Equal(new[] { "MouseId" }, operation.Columns);
                    Assert.Equal(new[] { "Id" }, operation.PrincipalColumns);
                    Assert.Equal(ReferentialAction.NoAction, operation.OnDelete);
                },
                o =>
                {
                    var operation = Assert.IsType<AddForeignKeyOperation>(o);
                    Assert.Equal("FK_Dogs_Mice_MouseId", operation.Name);
                    Assert.Equal("Dogs", operation.Table);
                    Assert.Equal("Mice", operation.PrincipalTable);
                    Assert.Equal(new[] { "MouseId" }, operation.Columns);
                    Assert.Equal(new[] { "Id" }, operation.PrincipalColumns);
                    Assert.Equal(ReferentialAction.NoAction, operation.OnDelete);
                },
                o =>
                {
                    var operation = Assert.IsType<AddForeignKeyOperation>(o);
                    Assert.Equal("FK_Mice_Mice_MouseId", operation.Name);
                    Assert.Equal("Mice", operation.Table);
                    Assert.Equal("Mice", operation.PrincipalTable);
                    Assert.Equal(new[] { "MouseId" }, operation.Columns);
                    Assert.Equal(new[] { "Id" }, operation.PrincipalColumns);
                    Assert.Equal(ReferentialAction.NoAction, operation.OnDelete);
                }),
            downOps => Assert.Collection(
                downOps,
                o =>
                {
                    var operation = Assert.IsType<DropForeignKeyOperation>(o);
                    Assert.Equal("FK_Cats_Mice_MouseId", operation.Name);
                    Assert.Equal("Cats", operation.Table);
                },
                o =>
                {
                    var operation = Assert.IsType<DropForeignKeyOperation>(o);
                    Assert.Equal("FK_Dogs_Mice_MouseId", operation.Name);
                    Assert.Equal("Dogs", operation.Table);
                },
                o =>
                {
                    var operation = Assert.IsType<DropForeignKeyOperation>(o);
                    Assert.Equal("FK_Mice_Mice_MouseId", operation.Name);
                    Assert.Equal("Mice", operation.Table);
                },
                o =>
                {
                    var operation = Assert.IsType<DropIndexOperation>(o);
                    Assert.Equal("IX_Mice_MouseId", operation.Name);
                    Assert.Equal("Mice", operation.Table);
                },
                o =>
                {
                    var operation = Assert.IsType<DropIndexOperation>(o);
                    Assert.Equal("IX_Dogs_MouseId", operation.Name);
                    Assert.Equal("Dogs", operation.Table);
                },
                o =>
                {
                    var operation = Assert.IsType<DropIndexOperation>(o);
                    Assert.Equal("IX_Cats_MouseId", operation.Name);
                    Assert.Equal("Cats", operation.Table);
                },
                o =>
                {
                    var operation = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Equal("Dogs", operation.Table);

                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(23, v));
                },
                o =>
                {
                    var operation = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Equal("Mice", operation.Table);

                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(33, v));
                },
                o =>
                {
                    var operation = Assert.IsType<DropColumnOperation>(o);
                    Assert.Equal("MouseId", operation.Name);
                    Assert.Equal("Mice", operation.Table);
                },
                o =>
                {
                    var operation = Assert.IsType<DropColumnOperation>(o);
                    Assert.Equal("MouseId", operation.Name);
                    Assert.Equal("Dogs", operation.Table);
                },
                o =>
                {
                    var operation = Assert.IsType<DropColumnOperation>(o);
                    Assert.Equal("MouseId", operation.Name);
                    Assert.Equal("Cats", operation.Table);
                },
                o =>
                {
                    var operation = Assert.IsType<InsertDataOperation>(o);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal(new[] { "Id", "MouseId" }, operation.Columns);
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Equal(11, v),
                        v => Assert.Equal(31, v),
                        v => Assert.Equal(13, v),
                        v => Assert.Null(v),
                        v => Assert.Equal(21, v),
                        v => Assert.Null(v),
                        v => Assert.Equal(22, v),
                        v => Assert.Null(v),
                        v => Assert.Equal(31, v),
                        v => Assert.Null(v),
                        v => Assert.Equal(32, v),
                        v => Assert.Null(v));
                },
                o =>
                {
                    var operation = Assert.IsType<UpdateDataOperation>(o);
                    Assert.Equal("Dogs", operation.Table);
                    Assert.Equal(new[] { "Id" }, operation.KeyColumns);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(22, v));

                    Assert.Equal(new[] { "PreyId" }, operation.Columns);
                    Assert.Null(operation.ColumnTypes);
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Equal(32, v));
                },
                o =>
                {
                    var operation = Assert.IsType<InsertDataOperation>(o);
                    Assert.Equal("Mice", operation.Table);
                    Assert.Equal(new[] { "Id" }, operation.Columns);
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Equal(32, v));
                },
                o =>
                {
                    var operation = Assert.IsType<InsertDataOperation>(o);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Equal(new[] { "Id", "MouseId" }, operation.Columns);
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Equal(12, v),
                        v => Assert.Equal(32, v));
                },
                o =>
                {
                    var operation = Assert.IsType<AddForeignKeyOperation>(o);
                    Assert.Equal("FK_Cats_Animal_Id", operation.Name);
                    Assert.Equal("Cats", operation.Table);
                    Assert.Equal("Animal", operation.PrincipalTable);
                    Assert.Equal(new[] { "Id" }, operation.Columns);
                    Assert.Equal(new[] { "Id" }, operation.PrincipalColumns);
                    Assert.Equal(ReferentialAction.Cascade, operation.OnDelete);
                },
                o =>
                {
                    var operation = Assert.IsType<AddForeignKeyOperation>(o);
                    Assert.Equal("FK_Cats_Animal_PreyId", operation.Name);
                    Assert.Equal("Cats", operation.Table);
                    Assert.Equal("Animal", operation.PrincipalTable);
                    Assert.Equal(new[] { "PreyId" }, operation.Columns);
                    Assert.Equal(new[] { "Id" }, operation.PrincipalColumns);
                    Assert.Equal(ReferentialAction.NoAction, operation.OnDelete);
                },
                o =>
                {
                    var operation = Assert.IsType<AddForeignKeyOperation>(o);
                    Assert.Equal("FK_Dogs_Animal_Id", operation.Name);
                    Assert.Equal("Dogs", operation.Table);
                    Assert.Equal("Animal", operation.PrincipalTable);
                    Assert.Equal(new[] { "Id" }, operation.Columns);
                    Assert.Equal(new[] { "Id" }, operation.PrincipalColumns);
                    Assert.Equal(ReferentialAction.Cascade, operation.OnDelete);
                },
                o =>
                {
                    var operation = Assert.IsType<AddForeignKeyOperation>(o);
                    Assert.Equal("FK_Dogs_Animal_PreyId", operation.Name);
                    Assert.Equal("Dogs", operation.Table);
                    Assert.Equal("Animal", operation.PrincipalTable);
                    Assert.Equal(new[] { "PreyId" }, operation.Columns);
                    Assert.Equal(new[] { "Id" }, operation.PrincipalColumns);
                    Assert.Equal(ReferentialAction.NoAction, operation.OnDelete);
                },
                o =>
                {
                    var operation = Assert.IsType<AddForeignKeyOperation>(o);
                    Assert.Equal("FK_Mice_Animal_Id", operation.Name);
                    Assert.Equal("Mice", operation.Table);
                    Assert.Equal("Animal", operation.PrincipalTable);
                    Assert.Equal(new[] { "Id" }, operation.Columns);
                    Assert.Equal(new[] { "Id" }, operation.PrincipalColumns);
                    Assert.Equal(ReferentialAction.Cascade, operation.OnDelete);
                }));

    [ConditionalFact]
    public void Change_TPT_to_TPC_with_excluded_base()
        => Execute(
            common =>
            {
                common.Entity(
                    "Order",
                    x =>
                    {
                        x.ToTable("Order", t => t.ExcludeFromMigrations());
                        x.Property<int>("Id");
                        x.Property<string>("Address");
                    });
                common.Entity(
                    "DetailedOrder",
                    x =>
                    {
                        x.ToTable("DetailedOrder");
                        x.HasBaseType("Order");
                        x.Property<string>("Description").HasColumnName("Description");
                    });
            },
            _ => { },
            target =>
            {
                target.Entity("Order").UseTpcMappingStrategy();
            },
            upOperations =>
            {
                Assert.Equal(2, upOperations.Count);

                var dropForeignKeyOperation = Assert.IsType<DropForeignKeyOperation>(upOperations[0]);
                Assert.Null(dropForeignKeyOperation.Schema);
                Assert.Equal("DetailedOrder", dropForeignKeyOperation.Table);
                Assert.Equal("FK_DetailedOrder_Order_Id", dropForeignKeyOperation.Name);

                var addColumnOperation = Assert.IsType<AddColumnOperation>(upOperations[1]);
                Assert.Null(addColumnOperation.Schema);
                Assert.Equal("DetailedOrder", addColumnOperation.Table);
                Assert.Equal("Address", addColumnOperation.Name);
            },
            downOperations =>
            {
                Assert.Equal(2, downOperations.Count);

                var dropColumnOperation = Assert.IsType<DropColumnOperation>(downOperations[0]);
                Assert.Null(dropColumnOperation.Schema);
                Assert.Equal("DetailedOrder", dropColumnOperation.Table);
                Assert.Equal("Address", dropColumnOperation.Name);

                var addForeignKeyOperation = Assert.IsType<AddForeignKeyOperation>(downOperations[1]);
                Assert.Null(addForeignKeyOperation.Schema);
                Assert.Equal("DetailedOrder", addForeignKeyOperation.Table);
                Assert.Equal("FK_DetailedOrder_Order_Id", addForeignKeyOperation.Name);
            });

    [ConditionalFact]
    public void Add_foreign_key_on_base_type()
        => Execute(
            modelBuilder =>
            {
                modelBuilder.Entity("Person").Property<int>("Id");
                modelBuilder.Entity(
                    "Animal",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<int>("HandlerId");
                    });
                modelBuilder.Entity("Drakee").HasBaseType("Animal");
            },
            modelBuilder =>
            {
                modelBuilder.Entity("Person").Property<int>("Id");
                modelBuilder.Entity(
                    "Animal",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<int>("HandlerId");
                        x.HasOne("Person").WithMany().HasForeignKey("HandlerId");
                    });
                modelBuilder.Entity("Drakee").HasBaseType("Animal");
            },
            operations =>
            {
                Assert.Equal(2, operations.Count);

                var createIndexOperation = Assert.IsType<CreateIndexOperation>(operations[0]);
                Assert.Equal("Animal", createIndexOperation.Table);
                Assert.Equal("IX_Animal_HandlerId", createIndexOperation.Name);
                Assert.Equal(new[] { "HandlerId" }, createIndexOperation.Columns);

                var addFkOperation = Assert.IsType<AddForeignKeyOperation>(operations[1]);
                Assert.Equal("Animal", addFkOperation.Table);
                Assert.Equal("FK_Animal_Person_HandlerId", addFkOperation.Name);
                Assert.Equal(new[] { "HandlerId" }, addFkOperation.Columns);
                Assert.Equal("Person", addFkOperation.PrincipalTable);
                Assert.Equal(new[] { "Id" }, addFkOperation.PrincipalColumns);
            });

    [ConditionalFact]
    public void Add_shared_foreign_key_on_subtypes()
        => Execute(
            common =>
            {
                common.Entity("Person").Property<int>("Id");
                common.Entity("Animal").Property<int>("Id");
                common.Entity("GameAnimal").HasBaseType("Animal").Property<int>("HunterId").HasColumnName("HunterId");
                common.Entity("EndangeredAnimal").HasBaseType("Animal").Property<int>("HunterId").HasColumnName("HunterId");
            },
            source => { },
            target =>
            {
                target.Entity(
                    "GameAnimal",
                    x =>
                    {
                        x.HasOne("Person").WithMany().HasForeignKey("HunterId").HasConstraintName("FK_Animal_Person_HunterId");
                        x.HasIndex("HunterId").HasDatabaseName("IX_Animal_HunterId");
                    });
                target.Entity(
                    "EndangeredAnimal",
                    x =>
                    {
                        x.HasOne("Person").WithMany().HasForeignKey("HunterId").HasConstraintName("FK_Animal_Person_HunterId");
                        x.HasIndex("HunterId").HasDatabaseName("IX_Animal_HunterId");
                    });
            },
            operations =>
            {
                Assert.Equal(2, operations.Count);

                var createIndexOperation = Assert.IsType<CreateIndexOperation>(operations[0]);
                Assert.Equal("Animal", createIndexOperation.Table);
                Assert.Equal("IX_Animal_HunterId", createIndexOperation.Name);
                Assert.Equal(new[] { "HunterId" }, createIndexOperation.Columns);

                var addFkOperation = Assert.IsType<AddForeignKeyOperation>(operations[1]);
                Assert.Equal("Animal", addFkOperation.Table);
                Assert.Equal("FK_Animal_Person_HunterId", addFkOperation.Name);
                Assert.Equal(new[] { "HunterId" }, addFkOperation.Columns);
                Assert.Equal("Person", addFkOperation.PrincipalTable);
                Assert.Equal(new[] { "Id" }, addFkOperation.PrincipalColumns);
            });

    [ConditionalFact]
    public void Add_shared_property_with_foreign_key_on_subtypes()
        => Execute(
            common =>
            {
                common.Entity("Person").Property<int>("Id");
                common.Entity("Animal").Property<int>("Id");
                common.Entity("GameAnimal").HasBaseType("Animal").Property<int>("HunterId").HasColumnName("HunterId");
                common.Entity(
                    "GameAnimal",
                    x =>
                    {
                        x.HasOne("Person").WithMany().HasForeignKey("HunterId").HasConstraintName("FK_Animal_Person_HunterId");
                        x.HasIndex("HunterId").HasDatabaseName("IX_Animal_HunterId");
                    });
            },
            source => { },
            target =>
            {
                target.Entity("EndangeredAnimal").HasBaseType("Animal").Property<int>("HunterId").HasColumnName("HunterId");
                target.Entity(
                    "EndangeredAnimal",
                    x =>
                    {
                        x.HasOne("Person").WithMany().HasForeignKey("HunterId").HasConstraintName("FK_Animal_Person_HunterId");
                        x.HasIndex("HunterId").HasDatabaseName("IX_Animal_HunterId");
                    });
            },
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<AlterColumnOperation>(operations[0]);
                Assert.Equal("Animal", operation.Table);
                Assert.Equal("Discriminator", operation.Name);
                Assert.Equal(typeof(string), operation.ClrType);
                Assert.Equal("just_string(21)", operation.ColumnType);
                Assert.Equal(21, operation.MaxLength);
            });

    [ConditionalFact]
    public void Add_foreign_key_to_subtype()
        => Execute(
            source =>
            {
                source.Entity("Animal").Property<int>("Id");
                source.Entity("TrophyAnimal").HasBaseType("Animal");
                source.Entity(
                    "Person",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<int>("TrophyId");
                    });
            },
            target =>
            {
                target.Entity("Animal").Property<int>("Id");
                target.Entity("TrophyAnimal").HasBaseType("Animal");
                target.Entity(
                    "Person",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<int>("TrophyId");
                        x.HasOne("TrophyAnimal").WithMany().HasForeignKey("TrophyId");
                    });
            },
            operations =>
            {
                Assert.Equal(2, operations.Count);

                var createIndexOperation = Assert.IsType<CreateIndexOperation>(operations[0]);
                Assert.Equal("Person", createIndexOperation.Table);
                Assert.Equal("IX_Person_TrophyId", createIndexOperation.Name);
                Assert.Equal(new[] { "TrophyId" }, createIndexOperation.Columns);

                var addFkOperation = Assert.IsType<AddForeignKeyOperation>(operations[1]);
                Assert.Equal("Person", addFkOperation.Table);
                Assert.Equal("FK_Person_Animal_TrophyId", addFkOperation.Name);
                Assert.Equal(new[] { "TrophyId" }, addFkOperation.Columns);
                Assert.Equal("Animal", addFkOperation.PrincipalTable);
                Assert.Equal(new[] { "Id" }, addFkOperation.PrincipalColumns);
            });

    [ConditionalFact]
    public void Drop_foreign_key_on_subtype()
        => Execute(
            source =>
            {
                source.Entity("Person").Property<int>("Id");
                source.Entity("Animal").Property<int>("Id");
                source.Entity(
                    "MountAnimal",
                    x =>
                    {
                        x.HasBaseType("Animal");
                        x.Property<int>("RiderId");
                        x.HasOne("Person").WithMany().HasForeignKey("RiderId");
                    });
            },
            target =>
            {
                target.Entity("Person").Property<int>("Id");
                target.Entity("Animal").Property<int>("Id");
                target.Entity("MountAnimal").HasBaseType("Animal").Property<int>("RiderId");
            },
            operations =>
            {
                Assert.Equal(2, operations.Count);

                var dropFkOperation = Assert.IsType<DropForeignKeyOperation>(operations[0]);
                Assert.Equal("Animal", dropFkOperation.Table);
                Assert.Equal("FK_Animal_Person_RiderId", dropFkOperation.Name);

                var dropIndexOperation = Assert.IsType<DropIndexOperation>(operations[1]);
                Assert.Equal("Animal", dropIndexOperation.Table);
                Assert.Equal("IX_Animal_RiderId", dropIndexOperation.Name);
            });

    [ConditionalFact]
    public void Create_shared_table_with_two_entity_types()
        => Execute(
            _ => { },
            modelBuilder =>
            {
                modelBuilder.Entity("Order").ToTable("Orders").Property<int>("Id");
                modelBuilder.Entity(
                    "OrderDetails", eb =>
                    {
                        eb.Property<int>("Id");
                        eb.Property<DateTime>("Time");
                        eb.HasOne("Order").WithOne().HasForeignKey("OrderDetails", "Id");
                        eb.ToTable("Orders");
                    });
            },
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var createTableOperation = Assert.IsType<CreateTableOperation>(operations[0]);
                Assert.Equal(2, createTableOperation.Columns.Count);
                var timeColumn = createTableOperation.Columns[1];
                Assert.Equal("Time", timeColumn.Name);
                Assert.True(timeColumn.IsNullable);
            });

    [ConditionalFact]
    public void Create_shared_table_with_required_dependent()
        => Execute(
            _ => { },
            modelBuilder =>
            {
                modelBuilder.Entity("Order").ToTable("Orders").Property<int>("Id");
                modelBuilder.Entity(
                    "OrderDetails", eb =>
                    {
                        eb.Property<int>("Id");
                        eb.Property<DateTime>("Time");
                        eb.HasOne("Order").WithOne("OrderDetails").HasForeignKey("OrderDetails", "Id");
                        eb.ToTable("Orders");
                    });
                modelBuilder.Entity("Order").Navigation("OrderDetails").IsRequired();
            },
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var createTableOperation = Assert.IsType<CreateTableOperation>(operations[0]);
                Assert.Equal(2, createTableOperation.Columns.Count);
                var timeColumn = createTableOperation.Columns[1];
                Assert.Equal("Time", timeColumn.Name);
                Assert.False(timeColumn.IsNullable);
            });

    [ConditionalFact]
    public void Create_shared_table_with_inheritance_and_three_entity_types()
        => Execute(
            _ => { },
            modelBuilder =>
            {
                modelBuilder.Entity("OrderBase").ToTable("Orders").Property<int>("Id");
                modelBuilder.Entity("Order").HasBaseType("OrderBase").ToTable("Orders");
                modelBuilder.Entity(
                    "OrderDetails",
                    eb =>
                    {
                        eb.Property<int>("Id");
                        eb.Property<DateTime>("Time");
                        eb.HasOne("Order").WithOne().HasForeignKey("OrderDetails", "Id");
                        eb.ToTable("Orders");
                    });
            },
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var createTableOperation = Assert.IsType<CreateTableOperation>(operations[0]);
                Assert.Equal(3, createTableOperation.Columns.Count);
                var timeColumn = createTableOperation.Columns[2];
                Assert.Equal("Time", timeColumn.Name);
                Assert.True(timeColumn.IsNullable);
            });

    [ConditionalFact]
    public void Split_out_subtype_with_seed_data()
        => Execute(
            common =>
            {
                common.Entity<Animal>(
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<string>("Name");
                        x.Property<int>("Discriminator");

                        x.HasDiscriminator<int>("Discriminator")
                            .HasValue(1)
                            .HasValue<Eagle>(2);

                        x.ToTable("Animal", "dbo");
                        x.HasData(
                            new { Id = 42 });
                    });

                common.Entity<Eagle>(
                    x =>
                    {
                        x.HasBaseType<Animal>();
                        x.HasData(
                            new { Id = 41 });
                    });
            },
            source => source.Entity<Animal>(
                x =>
                {
                    x.HasData(
                        new Animal { Id = 43, Name = "Bob" });
                }),
            target => target.Entity<Shark>(
                x =>
                {
                    x.HasBaseType<Animal>();

                    x.HasDiscriminator<int>("Discriminator")
                        .HasValue(3);

                    x.HasData(
                        new Shark { Id = 43, Name = "Bob" });
                }),
            upOps => Assert.Collection(
                upOps,
                o =>
                {
                    var operation = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Null(operation.KeyColumnTypes);
                    Assert.Collection(
                        operation.KeyColumns,
                        v => Assert.Equal("Id", v));
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(43, v));
                },
                o =>
                {
                    var operation = Assert.IsType<InsertDataOperation>(o);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Collection(
                        operation.Columns,
                        v => Assert.Equal("Id", v),
                        v => Assert.Equal("Discriminator", v),
                        v => Assert.Equal("Name", v));
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Equal(43, v),
                        v => Assert.Equal(3, v),
                        v => Assert.Equal("Bob", v));
                }),
            downOps => Assert.Collection(
                downOps,
                o =>
                {
                    var operation = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Collection(
                        operation.KeyColumns,
                        v => Assert.Equal("Id", v));
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(43, v));
                },
                o =>
                {
                    var operation = Assert.IsType<InsertDataOperation>(o);
                    Assert.Equal("Animal", operation.Table);
                    Assert.Collection(
                        operation.Columns,
                        v => Assert.Equal("Id", v),
                        v => Assert.Equal("Discriminator", v),
                        v => Assert.Equal("Name", v));
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Equal(43, v),
                        v => Assert.Equal(1, v),
                        v => Assert.Equal("Bob", v));
                }));

    private class Animal
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    private class Eagle : Animal;

    private class Shark : Animal;

    [ConditionalFact]
    public void Add_column_to_renamed_table()
        => Execute(
            source => source.Entity("Table").ToTable("Table", "old").Property<int>("Id"),
            target => target
                .Entity(
                    "Table",
                    x =>
                    {
                        x.ToTable("RenamedTable", "new");
                        x.Property<int>("Id");
                        x.HasKey("Id").HasName("PK_Table");
                        x.Property<string>("Value");
                    }),
            operations =>
            {
                Assert.Equal(3, operations.Count);

                Assert.IsType<EnsureSchemaOperation>(operations[0]);

                Assert.IsType<RenameTableOperation>(operations[1]);

                var addColumnOperation = Assert.IsType<AddColumnOperation>(operations[2]);
                Assert.Equal("new", addColumnOperation.Schema);
                Assert.Equal("RenamedTable", addColumnOperation.Table);
                Assert.Equal("Value", addColumnOperation.Name);
            });

    [ConditionalFact]
    public void Add_foreign_key_to_renamed_table()
        => Execute(
            source => source
                .Entity("ReferencedTable", x => x.Property<int>("Id"))
                .Entity(
                    "Table",
                    x =>
                    {
                        x.ToTable("Table", "old");
                        x.Property<int>("Id");
                        x.Property<int>("ForeignId");
                        x.HasIndex("ForeignId");
                    }),
            target => target
                .Entity("ReferencedTable", x => x.Property<int>("Id"))
                .Entity(
                    "Table",
                    x =>
                    {
                        x.ToTable("RenamedTable", "new");
                        x.Property<int>("Id");
                        x.HasKey("Id").HasName("PK_Table");
                        x.Property<int>("ForeignId");
                        x.HasIndex("ForeignId").HasDatabaseName("IX_Table_ForeignId");
                        x.HasOne("ReferencedTable").WithMany().HasForeignKey("ForeignId");
                    }),
            operations =>
            {
                Assert.Equal(3, operations.Count);

                Assert.IsType<EnsureSchemaOperation>(operations[0]);

                Assert.IsType<RenameTableOperation>(operations[1]);

                var addForeignKeyOperation = Assert.IsType<AddForeignKeyOperation>(operations[2]);
                Assert.Equal("new", addForeignKeyOperation.Schema);
                Assert.Equal("RenamedTable", addForeignKeyOperation.Table);
                Assert.Equal("FK_RenamedTable_ReferencedTable_ForeignId", addForeignKeyOperation.Name);
            });

    [ConditionalFact]
    public void Add_foreign_key_to_renamed_column()
        => Execute(
            source => source
                .Entity("ReferencedTable", x => x.Property<int>("Id"))
                .Entity(
                    "Table",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<int>("ForeignId");
                        x.HasIndex("ForeignId");
                    }),
            target => target
                .Entity("ReferencedTable", x => x.Property<int>("Id"))
                .Entity(
                    "Table",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.HasKey("Id").HasName("PK_Table");
                        x.Property<int>("ForeignId").HasColumnName("RenamedForeignId");
                        x.HasIndex("ForeignId").HasDatabaseName("IX_Table_ForeignId");
                        x.HasOne("ReferencedTable").WithMany().HasForeignKey("ForeignId");
                    }),
            operations =>
            {
                Assert.Equal(2, operations.Count);

                Assert.IsType<RenameColumnOperation>(operations[0]);

                var addForeignKeyOperation = Assert.IsType<AddForeignKeyOperation>(operations[1]);
                Assert.Equal("FK_Table_ReferencedTable_RenamedForeignId", addForeignKeyOperation.Name);
                Assert.Equal(new[] { "RenamedForeignId" }, addForeignKeyOperation.Columns);
            });

    [ConditionalFact]
    public void Add_foreign_key_referencing_renamed_table()
        => Execute(
            source => source
                .Entity("ReferencedTable", x => x.ToTable("ReferencedTable", "old").Property<int>("Id"))
                .Entity(
                    "Table",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<int>("ForeignId");
                        x.HasIndex("ForeignId");
                    }),
            target => target
                .Entity(
                    "ReferencedTable",
                    x =>
                    {
                        x.ToTable("RenamedReferencedTable", "new");
                        x.Property<int>("Id");
                        x.HasKey("Id").HasName("PK_ReferencedTable");
                    })
                .Entity(
                    "Table",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<int>("ForeignId");
                        x.HasOne("ReferencedTable").WithMany().HasForeignKey("ForeignId");
                    }),
            operations =>
            {
                Assert.Equal(3, operations.Count);

                Assert.IsType<EnsureSchemaOperation>(operations[0]);

                Assert.IsType<RenameTableOperation>(operations[1]);

                var addForeignKeyOperation = Assert.IsType<AddForeignKeyOperation>(operations[2]);
                Assert.Equal("new", addForeignKeyOperation.PrincipalSchema);
                Assert.Equal("RenamedReferencedTable", addForeignKeyOperation.PrincipalTable);
                Assert.Equal("FK_Table_RenamedReferencedTable_ForeignId", addForeignKeyOperation.Name);
            });

    [ConditionalFact]
    public void Add_foreign_key_referencing_renamed_column_with_seed_data()
        => Execute(
            common => common
                .Entity("ReferencedTable", x => x.Property<int>("Id"))
                .Entity(
                    "Table",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<int>("ForeignId");
                        x.HasIndex("ForeignId");
                    }),
            source => source
                .Entity(
                    "Table",
                    x =>
                    {
                        x.HasIndex("ForeignId");
                        x.HasData(
                            new { Id = 43 });
                    }),
            target => target
                .Entity(
                    "ReferencedTable", x =>
                    {
                        x.Property<int>("Id").HasColumnName("ReferencedTableId");
                        x.HasData(
                            new { Id = 42 });
                    })
                .Entity(
                    "Table",
                    x =>
                    {
                        x.HasOne("ReferencedTable").WithMany().HasForeignKey("ForeignId");
                        x.HasData(
                            new { Id = 43, ForeignId = 42 });
                    }),
            upOps => Assert.Collection(
                upOps,
                o =>
                {
                    var operation = Assert.IsType<RenameColumnOperation>(o);
                    Assert.Equal("ReferencedTable", operation.Table);
                    Assert.Equal("Id", operation.Name);
                    Assert.Equal("ReferencedTableId", operation.NewName);
                },
                o =>
                {
                    var operation = Assert.IsType<InsertDataOperation>(o);
                    Assert.Equal("ReferencedTable", operation.Table);
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Equal(42, v));
                },
                o =>
                {
                    var operation = Assert.IsType<UpdateDataOperation>(o);
                    Assert.Equal("Table", operation.Table);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(43, v));
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Equal(42, v));
                },
                o =>
                {
                    var operation = Assert.IsType<AddForeignKeyOperation>(o);
                    Assert.Equal(new[] { "ReferencedTableId" }, operation.PrincipalColumns);
                    Assert.Equal("FK_Table_ReferencedTable_ForeignId", operation.Name);
                }),
            downOps => Assert.Collection(
                downOps,
                o =>
                {
                    var operation = Assert.IsType<DropForeignKeyOperation>(o);
                    Assert.Equal("FK_Table_ReferencedTable_ForeignId", operation.Name);
                },
                o =>
                {
                    var operation = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Equal("ReferencedTable", operation.Table);
                    Assert.Collection(
                        operation.KeyColumns,
                        v => Assert.Equal("ReferencedTableId", v));
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(42, v));
                },
                o =>
                {
                    var operation = Assert.IsType<RenameColumnOperation>(o);
                    Assert.Equal("ReferencedTable", operation.Table);
                    Assert.Equal("ReferencedTableId", operation.Name);
                    Assert.Equal("Id", operation.NewName);
                },
                o =>
                {
                    var operation = Assert.IsType<UpdateDataOperation>(o);
                    Assert.Equal("Table", operation.Table);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(43, v));
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Equal(0, v));
                }));

    [ConditionalFact]
    public void Create_table_with_foreign_key_referencing_renamed_table()
        => Execute(
            source => source.Entity("ReferencedTable").ToTable("ReferencedTable", "old").Property<int>("Id"),
            target => target
                .Entity(
                    "ReferencedTable",
                    x =>
                    {
                        x.ToTable("RenamedReferencedTable", "new");
                        x.Property<int>("Id");
                        x.HasKey("Id").HasName("PK_ReferencedTable");
                    })
                .Entity(
                    "Table",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<int>("ForeignId");
                        x.HasOne("ReferencedTable").WithMany().HasForeignKey("ForeignId");
                    }),
            operations =>
            {
                Assert.Equal(4, operations.Count);

                Assert.IsType<EnsureSchemaOperation>(operations[0]);

                Assert.IsType<RenameTableOperation>(operations[1]);

                var createTableOperation = Assert.IsType<CreateTableOperation>(operations[2]);
                Assert.Single(createTableOperation.ForeignKeys);

                var addForeignKeyOperation = createTableOperation.ForeignKeys[0];
                Assert.Equal("new", addForeignKeyOperation.PrincipalSchema);
                Assert.Equal("RenamedReferencedTable", addForeignKeyOperation.PrincipalTable);
                Assert.Equal("FK_Table_RenamedReferencedTable_ForeignId", addForeignKeyOperation.Name);

                Assert.IsType<CreateIndexOperation>(operations[3]);
            });

    [ConditionalFact]
    public void Create_table_with_foreign_key_referencing_renamed_column()
        => Execute(
            source => source.Entity("ReferencedTable").Property<int>("Id"),
            target => target
                .Entity("ReferencedTable", x => x.Property<int>("Id").HasColumnName("ReferencedTableId"))
                .Entity(
                    "Table",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<int>("ForeignId");
                        x.HasOne("ReferencedTable").WithMany().HasForeignKey("ForeignId");
                    }),
            operations =>
            {
                Assert.Equal(3, operations.Count);

                Assert.IsType<RenameColumnOperation>(operations[0]);

                var createTableOperation = Assert.IsType<CreateTableOperation>(operations[1]);
                Assert.Single(createTableOperation.ForeignKeys);

                var addForeignKeyOperation = createTableOperation.ForeignKeys[0];
                Assert.Equal(new[] { "ReferencedTableId" }, addForeignKeyOperation.PrincipalColumns);
                Assert.Equal("FK_Table_ReferencedTable_ForeignId", addForeignKeyOperation.Name);

                Assert.IsType<CreateIndexOperation>(operations[2]);
            });

    [ConditionalFact]
    public void Rename_primary_key_on_renamed_table()
        => Execute(
            source => source.Entity("Table").ToTable("Table", "old").Property<int>("Id"),
            target => target.Entity("Table").ToTable("RenamedTable", "new").Property<int>("Id"),
            operations =>
            {
                Assert.Equal(4, operations.Count);

                var dropPrimaryKeyOperation = Assert.IsType<DropPrimaryKeyOperation>(operations[0]);
                Assert.Equal("old", dropPrimaryKeyOperation.Schema);
                Assert.Equal("Table", dropPrimaryKeyOperation.Table);
                Assert.Equal("PK_Table", dropPrimaryKeyOperation.Name);

                Assert.IsType<EnsureSchemaOperation>(operations[1]);

                Assert.IsType<RenameTableOperation>(operations[2]);

                var addPrimaryKeyOperation = Assert.IsType<AddPrimaryKeyOperation>(operations[3]);
                Assert.Equal("new", addPrimaryKeyOperation.Schema);
                Assert.Equal("RenamedTable", addPrimaryKeyOperation.Table);
                Assert.Equal("PK_RenamedTable", addPrimaryKeyOperation.Name);
            });

    [ConditionalFact]
    public void Rename_primary_key_on_renamed_column()
        => Execute(
            source => source.Entity("Table").Property<int>("Id"),
            target => target.Entity(
                "Table",
                x =>
                {
                    x.Property<int>("Id").HasColumnName("RenamedId");
                    x.HasKey("Id").HasName("PK_Table_Renamed");
                }),
            operations =>
            {
                Assert.Equal(3, operations.Count);

                Assert.IsType<DropPrimaryKeyOperation>(operations[0]);

                Assert.IsType<RenameColumnOperation>(operations[1]);

                var addPrimaryKeyOperation = Assert.IsType<AddPrimaryKeyOperation>(operations[2]);
                Assert.Equal(new[] { "RenamedId" }, addPrimaryKeyOperation.Columns);
                Assert.Equal("PK_Table_Renamed", addPrimaryKeyOperation.Name);
            });

    [ConditionalFact]
    public void Add_alternate_key_to_renamed_table()
        => Execute(
            source => source.Entity(
                "Table",
                x =>
                {
                    x.ToTable("Table", "old");
                    x.Property<int>("Id");
                    x.Property<int>("AlternateId");
                }),
            target => target.Entity(
                "Table",
                x =>
                {
                    x.ToTable("RenamedTable", "new");
                    x.Property<int>("Id");
                    x.HasKey("Id").HasName("PK_Table");
                    x.Property<int>("AlternateId");
                    x.HasAlternateKey("AlternateId");
                }),
            operations =>
            {
                Assert.Equal(3, operations.Count);

                Assert.IsType<EnsureSchemaOperation>(operations[0]);

                Assert.IsType<RenameTableOperation>(operations[1]);

                var addUniqueConstraintOperation = Assert.IsType<AddUniqueConstraintOperation>(operations[2]);
                Assert.Equal("new", addUniqueConstraintOperation.Schema);
                Assert.Equal("RenamedTable", addUniqueConstraintOperation.Table);
                Assert.Equal("AK_RenamedTable_AlternateId", addUniqueConstraintOperation.Name);
            });

    [ConditionalFact]
    public void Add_alternate_key_to_renamed_column()
        => Execute(
            source => source.Entity(
                "Table",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<int>("AlternateId");
                }),
            target => target.Entity(
                "Table",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<int>("AlternateId").HasColumnName("RenamedAlternateId");
                    x.HasAlternateKey("AlternateId");
                }),
            operations =>
            {
                Assert.Equal(2, operations.Count);

                Assert.IsType<RenameColumnOperation>(operations[0]);

                var addUniqueConstraintOperation = Assert.IsType<AddUniqueConstraintOperation>(operations[1]);
                Assert.Equal(new[] { "RenamedAlternateId" }, addUniqueConstraintOperation.Columns);
                Assert.Equal("AK_Table_RenamedAlternateId", addUniqueConstraintOperation.Name);
            });

    [ConditionalFact]
    public void Alter_column_on_renamed_table()
        => Execute(
            source => source.Entity(
                "Table",
                x =>
                {
                    x.ToTable("Table", "old");
                    x.Property<int>("Id");
                    x.Property<string>("Value");
                }),
            target => target.Entity(
                "Table",
                x =>
                {
                    x.ToTable("RenamedTable", "new");
                    x.Property<int>("Id");
                    x.HasKey("Id").HasName("PK_Table");
                    x.Property<string>("Value").IsRequired();
                }),
            operations =>
            {
                Assert.Equal(3, operations.Count);

                Assert.IsType<EnsureSchemaOperation>(operations[0]);

                Assert.IsType<RenameTableOperation>(operations[1]);

                var alterColumnOperation = Assert.IsType<AlterColumnOperation>(operations[2]);
                Assert.Equal("new", alterColumnOperation.Schema);
                Assert.Equal("RenamedTable", alterColumnOperation.Table);
                Assert.Equal("Value", alterColumnOperation.Name);
            });

    [ConditionalFact]
    public void Alter_renamed_column()
        => Execute(
            source => source.Entity(
                "Table",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<string>("Value");
                }),
            target => target.Entity(
                "Table",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<string>("Value").HasColumnName("RenamedValue").IsRequired();
                }),
            operations =>
            {
                Assert.Equal(2, operations.Count);

                Assert.IsType<RenameColumnOperation>(operations[0]);

                var alterColumnOperation = Assert.IsType<AlterColumnOperation>(operations[1]);
                Assert.Equal("Table", alterColumnOperation.Table);
                Assert.Equal("RenamedValue", alterColumnOperation.Name);
            });

    [ConditionalFact]
    public void Alter_renamed_sequence()
        => Execute(
            source => source.HasSequence("Sequence", "old"),
            target => target.HasSequence("Sequence", "new").IncrementsBy(2),
            operations =>
            {
                Assert.Equal(3, operations.Count);

                Assert.IsType<EnsureSchemaOperation>(operations[0]);

                Assert.IsType<RenameSequenceOperation>(operations[1]);

                var alterSequenceOperation = Assert.IsType<AlterSequenceOperation>(operations[2]);
                Assert.Equal("new", alterSequenceOperation.Schema);
                Assert.Equal("Sequence", alterSequenceOperation.Name);
            },
            skipSourceConventions: true);

    [ConditionalFact]
    public void Create_index_on_renamed_table()
        => Execute(
            source => source.Entity(
                "Table",
                x =>
                {
                    x.ToTable("Table", "old");
                    x.Property<int>("Id");
                    x.Property<int>("Value");
                }),
            target => target.Entity(
                "Table",
                x =>
                {
                    x.ToTable("RenamedTable", "new");
                    x.Property<int>("Id");
                    x.HasKey("Id").HasName("PK_Table");
                    x.Property<int>("Value");
                    x.HasIndex("Value");
                }),
            operations =>
            {
                Assert.Equal(3, operations.Count);

                Assert.IsType<EnsureSchemaOperation>(operations[0]);

                Assert.IsType<RenameTableOperation>(operations[1]);

                var createIndexOperation = Assert.IsType<CreateIndexOperation>(operations[2]);
                Assert.Equal("new", createIndexOperation.Schema);
                Assert.Equal("RenamedTable", createIndexOperation.Table);
                Assert.Equal("IX_RenamedTable_Value", createIndexOperation.Name);
            });

    [ConditionalFact]
    public void Create_index_on_renamed_column()
        => Execute(
            source => source.Entity(
                "Table",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<int>("Value");
                }),
            target => target.Entity(
                "Table",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<int>("Value").HasColumnName("RenamedValue");
                    x.HasIndex("Value");
                }),
            operations =>
            {
                Assert.Equal(2, operations.Count);

                Assert.IsType<RenameColumnOperation>(operations[0]);

                var createIndexOperation = Assert.IsType<CreateIndexOperation>(operations[1]);
                Assert.Equal(new[] { "RenamedValue" }, createIndexOperation.Columns);
                Assert.Equal("IX_Table_RenamedValue", createIndexOperation.Name);
            });

    [ConditionalFact]
    public void Drop_column_on_renamed_table()
        => Execute(
            source => source
                .Entity(
                    "Table",
                    x =>
                    {
                        x.ToTable("Table", "old");
                        x.Property<int>("Id");
                        x.Property<string>("Value");
                    }),
            target => target
                .Entity(
                    "Table",
                    x =>
                    {
                        x.ToTable("RenamedTable", "new");
                        x.Property<int>("Id");
                        x.HasKey("Id").HasName("PK_Table");
                    }),
            operations =>
            {
                Assert.Equal(3, operations.Count);

                var dropColumnOperation = Assert.IsType<DropColumnOperation>(operations[0]);
                Assert.Equal("old", dropColumnOperation.Schema);
                Assert.Equal("Table", dropColumnOperation.Table);
                Assert.Equal("Value", dropColumnOperation.Name);

                Assert.IsType<EnsureSchemaOperation>(operations[1]);

                Assert.IsType<RenameTableOperation>(operations[2]);
            });

    [ConditionalFact]
    public void Drop_foreign_key_on_renamed_table()
        => Execute(
            source => source
                .Entity("ReferencedTable", x => x.Property<int>("Id"))
                .Entity(
                    "Table",
                    x =>
                    {
                        x.ToTable("Table", "old");
                        x.Property<int>("Id");
                        x.Property<int>("ForeignId");
                        x.HasIndex("ForeignId");
                        x.HasOne("ReferencedTable").WithMany().HasForeignKey("ForeignId");
                    }),
            target => target
                .Entity("ReferencedTable", x => x.Property<int>("Id"))
                .Entity(
                    "Table",
                    x =>
                    {
                        x.ToTable("RenamedTable", "new");
                        x.Property<int>("Id");
                        x.HasKey("Id").HasName("PK_Table");
                        x.Property<int>("ForeignId");
                        x.HasIndex("ForeignId").HasDatabaseName("IX_Table_ForeignId");
                    }),
            operations =>
            {
                Assert.Equal(3, operations.Count);

                var dropForeignKeyOperation = Assert.IsType<DropForeignKeyOperation>(operations[0]);
                Assert.Equal("old", dropForeignKeyOperation.Schema);
                Assert.Equal("Table", dropForeignKeyOperation.Table);
                Assert.Equal("FK_Table_ReferencedTable_ForeignId", dropForeignKeyOperation.Name);

                Assert.IsType<EnsureSchemaOperation>(operations[1]);

                Assert.IsType<RenameTableOperation>(operations[2]);
            });

    [ConditionalFact]
    public void Drop_alternate_key_on_renamed_table()
        => Execute(
            source => source.Entity(
                "Table",
                x =>
                {
                    x.ToTable("Table", "old");
                    x.Property<int>("Id");
                    x.Property<int>("AlternateId");
                    x.HasAlternateKey("AlternateId");
                }),
            target => target.Entity(
                "Table",
                x =>
                {
                    x.ToTable("RenamedTable", "new");
                    x.Property<int>("Id");
                    x.HasKey("Id").HasName("PK_Table");
                    x.Property<int>("AlternateId");
                }),
            operations =>
            {
                Assert.Equal(3, operations.Count);

                var dropUniqueConstraintOperation = Assert.IsType<DropUniqueConstraintOperation>(operations[0]);
                Assert.Equal("old", dropUniqueConstraintOperation.Schema);
                Assert.Equal("Table", dropUniqueConstraintOperation.Table);
                Assert.Equal("AK_Table_AlternateId", dropUniqueConstraintOperation.Name);

                Assert.IsType<EnsureSchemaOperation>(operations[1]);

                Assert.IsType<RenameTableOperation>(operations[2]);
            });

    [ConditionalFact]
    public void Drop_index_on_renamed_table()
        => Execute(
            source => source.Entity(
                "Table",
                x =>
                {
                    x.ToTable("Table", "old");
                    x.Property<int>("Id");
                    x.Property<int>("Value");
                    x.HasIndex("Value");
                }),
            target => target.Entity(
                "Table",
                x =>
                {
                    x.ToTable("RenamedTable", "new");
                    x.Property<int>("Id");
                    x.HasKey("Id").HasName("PK_Table");
                    x.Property<int>("Value");
                }),
            operations =>
            {
                Assert.Equal(3, operations.Count);

                var dropIndexOperation = Assert.IsType<DropIndexOperation>(operations[0]);
                Assert.Equal("old", dropIndexOperation.Schema);
                Assert.Equal("Table", dropIndexOperation.Table);
                Assert.Equal("IX_Table_Value", dropIndexOperation.Name);

                Assert.IsType<EnsureSchemaOperation>(operations[1]);

                Assert.IsType<RenameTableOperation>(operations[2]);
            });

    [ConditionalFact]
    public void Restart_renamed_sequence()
        => Execute(
            source => source.HasSequence("Sequence", "old"),
            target => target.HasSequence("Sequence", "new").StartsAt(2),
            operations =>
            {
                Assert.Equal(3, operations.Count);

                Assert.IsType<EnsureSchemaOperation>(operations[0]);

                Assert.IsType<RenameSequenceOperation>(operations[1]);

                var alterSequenceOperation = Assert.IsType<RestartSequenceOperation>(operations[2]);
                Assert.Equal("new", alterSequenceOperation.Schema);
                Assert.Equal("Sequence", alterSequenceOperation.Name);
            });

    [ConditionalFact]
    public void Rename_column_on_renamed_table()
        => Execute(
            source => source
                .Entity(
                    "Table",
                    x =>
                    {
                        x.ToTable("Table", "old");
                        x.Property<int>("Id");
                        x.Property<string>("Value");
                    }),
            target => target
                .Entity(
                    "Table",
                    x =>
                    {
                        x.ToTable("RenamedTable", "new");
                        x.Property<int>("Id");
                        x.HasKey("Id").HasName("PK_Table");
                        x.Property<string>("Value").HasColumnName("RenamedValue");
                    }),
            operations =>
            {
                Assert.Equal(3, operations.Count);

                Assert.IsType<EnsureSchemaOperation>(operations[0]);

                Assert.IsType<RenameTableOperation>(operations[1]);

                var renameColumnOperation = Assert.IsType<RenameColumnOperation>(operations[2]);
                Assert.Equal("new", renameColumnOperation.Schema);
                Assert.Equal("RenamedTable", renameColumnOperation.Table);
                Assert.Equal("RenamedValue", renameColumnOperation.NewName);
            });

    [ConditionalFact]
    public void Rename_index_on_renamed_table()
        => Execute(
            source => source.Entity(
                "Table",
                x =>
                {
                    x.ToTable("Table", "old");
                    x.Property<int>("Id");
                    x.Property<int>("Value");
                    x.HasIndex("Value");
                }),
            target => target.Entity(
                "Table",
                x =>
                {
                    x.ToTable("RenamedTable", "new");
                    x.Property<int>("Id");
                    x.HasKey("Id").HasName("PK_Table");
                    x.Property<int>("Value");
                    x.HasIndex("Value");
                }),
            operations =>
            {
                Assert.Equal(3, operations.Count);

                Assert.IsType<EnsureSchemaOperation>(operations[0]);

                Assert.IsType<RenameTableOperation>(operations[1]);

                var renameIndexOperation = Assert.IsType<RenameIndexOperation>(operations[2]);
                Assert.Equal("new", renameIndexOperation.Schema);
                Assert.Equal("RenamedTable", renameIndexOperation.Table);
                Assert.Equal("IX_RenamedTable_Value", renameIndexOperation.NewName);
            });

    [ConditionalFact]
    public void Add_alternate_key_on_added_column()
        => Execute(
            source => source.Entity("Table").Property<int>("Id"),
            target => target
                .Entity(
                    "Table",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<int>("AlternateId");
                        x.HasAlternateKey("AlternateId");
                    }),
            operations =>
            {
                Assert.Equal(2, operations.Count);

                Assert.IsType<AddColumnOperation>(operations[0]);
                Assert.IsType<AddUniqueConstraintOperation>(operations[1]);
            });

    [ConditionalFact]
    public void Add_foreign_key_referencing_added_alternate_key_with_seed_data()
        => Execute(
            common => common
                .Entity(
                    "Table",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<int>("AlternateId");
                    }),
            source => { },
            target => target
                .Entity(
                    "Table",
                    x =>
                    {
                        x.HasAlternateKey("AlternateId");
                        x.HasData(
                            new { Id = 42, AlternateId = 4242 });
                    })
                .Entity(
                    "ReferencingTable",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<int>("ReferencedAlternateId");
                        x.HasOne("Table").WithMany()
                            .HasForeignKey("ReferencedAlternateId")
                            .HasPrincipalKey("AlternateId");
                        x.HasData(
                            new { Id = 43, ReferencedAlternateId = 4242 });
                    }),
            upOps => Assert.Collection(
                upOps,
                o =>
                {
                    var operation = Assert.IsType<AddUniqueConstraintOperation>(o);
                    Assert.Equal("Table", operation.Table);
                    Assert.Equal(new[] { "AlternateId" }, operation.Columns);
                },
                o =>
                {
                    var operation = Assert.IsType<CreateTableOperation>(o);
                    Assert.Equal("ReferencingTable", operation.Name);
                    Assert.Single(operation.ForeignKeys);
                },
                o =>
                {
                    var operation = Assert.IsType<InsertDataOperation>(o);
                    Assert.Equal("Table", operation.Table);
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Equal(42, v),
                        v => Assert.Equal(4242, v));
                },
                o =>
                {
                    var operation = Assert.IsType<InsertDataOperation>(o);
                    Assert.Equal("ReferencingTable", operation.Table);
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Equal(43, v),
                        v => Assert.Equal(4242, v));
                },
                o =>
                {
                    var operation = Assert.IsType<CreateIndexOperation>(o);
                    Assert.Equal("ReferencingTable", operation.Table);
                    Assert.Equal(new[] { "ReferencedAlternateId" }, operation.Columns);
                }),
            downOps => Assert.Collection(
                downOps,
                o =>
                {
                    var operation = Assert.IsType<DropTableOperation>(o);
                    Assert.Equal("ReferencingTable", operation.Name);
                },
                o =>
                {
                    var operation = Assert.IsType<DropUniqueConstraintOperation>(o);
                    Assert.Equal("Table", operation.Table);
                },
                o =>
                {
                    var operation = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Equal("Table", operation.Table);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(42, v));
                }));

    [ConditionalFact]
    public void Update_AK_seed_value_with_a_referencing_foreign_key()
        => Execute(
            common => common
                .Entity(
                    "ReferencedTable", x =>
                    {
                        x.Property<int>("Id");
                        x.Property<int>("AlternateId");
                        x.HasAlternateKey("AlternateId");
                    })
                .Entity(
                    "Table",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<int>("ForeignId");
                        x.HasIndex("ForeignId");
                        x.HasOne("ReferencedTable").WithMany().HasForeignKey("ForeignId").IsRequired();
                        x.HasData(
                            new { Id = 43, ForeignId = 42 });
                    }),
            source => source
                .Entity(
                    "ReferencedTable", x =>
                    {
                        x.HasData(
                            new { Id = 42, AlternateId = 4242 });
                    }),
            target => target
                .Entity(
                    "ReferencedTable", x =>
                    {
                        x.HasData(
                            new { Id = 42, AlternateId = 4343 });
                    }),
            upOps => Assert.Collection(
                upOps,
                o =>
                {
                    var operation = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Equal("ReferencedTable", operation.Table);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(42, v));
                },
                o =>
                {
                    var operation = Assert.IsType<InsertDataOperation>(o);
                    Assert.Equal("ReferencedTable", operation.Table);
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Equal(42, v),
                        v => Assert.Equal(4343, v));
                }),
            downOps => Assert.Collection(
                downOps,
                o =>
                {
                    var operation = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Equal("ReferencedTable", operation.Table);
                    Assert.Null(operation.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        operation.KeyValues,
                        v => Assert.Equal(42, v));
                },
                o =>
                {
                    var operation = Assert.IsType<InsertDataOperation>(o);
                    Assert.Equal("ReferencedTable", operation.Table);
                    Assert.Null(operation.ColumnTypes);
                    AssertMultidimensionalArray(
                        operation.Values,
                        v => Assert.Equal(42, v),
                        v => Assert.Equal(4242, v));
                }));

    [ConditionalFact]
    public void SeedData_with_guid_AK_and_multiple_owned_types()
        => Execute(
            target =>
            {
                target.Entity<SomeEntity>(
                    builder =>
                    {
                        builder.HasAlternateKey(x => x.Guid);
                        builder.Property(x => x.Id).ValueGeneratedNever();

                        var data = new[] { new SomeEntity(1L, new Guid("74520CF7-0C78-447C-8FE0-ED97A16A13F5")) };

                        var owned = data.Select(x => new { SomeEntityId = x.Id, }).ToArray();

                        builder.OwnsOne(x => x.OwnedEntity).HasData(owned);
                        builder.HasData(data);
                    });

                target.Entity<ApplicationUser>(
                    builder =>
                    {
                        builder.HasAlternateKey(x => x.Guid);

                        var data = new[] { new ApplicationUser { Id = 12345, Guid = new Guid("4C85B629-732A-4724-AA33-6E8108134BAE") } };

                        var owned = data.Select(x => new { ApplicationUserId = x.Id, }).ToArray();

                        builder.OwnsOne(x => x.OwnedEntity).HasData(owned);
                        builder.HasData(data);
                    });
            },
            target => { },
            source => { },
            Assert.Empty,
            Assert.Empty);

    protected class SomeEntity(long id, Guid guid)
    {
        public virtual SomeOwnedEntity OwnedEntity { get; } = new();

        public Guid Guid { get; protected set; } = guid;

        public long Id { get; protected set; } = id;
    }

    protected class ApplicationUser
    {
        private readonly SomeOwnedEntity _ownedEntity;

        public ApplicationUser()
        {
            _ownedEntity = null!;
        }

        public virtual long Id { get; set; }

        public virtual SomeOwnedEntity OwnedEntity
            => _ownedEntity;

        public Guid Guid { get; set; }
    }

    protected class SomeOwnedEntity;

    [ConditionalFact]
    public void SeedData_and_PK_rename()
        => Execute(
            _ => { },
            source => source.Entity(
                "EntityWithTwoProperties",
                x =>
                {
                    x.Property<int>("Key");
                    x.Property<int>("Value1");
                    x.Property<string>("Value2");
                    x.HasKey("Key");
                }),
            target => target.Entity(
                "EntityWithTwoProperties",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<int>("Value1");
                    x.Property<string>("Value2");
                    x.HasData(
                        new { Id = 42, Value1 = 32 });
                }),
            upOps => Assert.Collection(
                upOps,
                o =>
                {
                    var operation = Assert.IsType<RenameColumnOperation>(o);
                    Assert.Equal("EntityWithTwoProperties", operation.Table);
                    Assert.Equal("Key", operation.Name);
                    Assert.Equal("Id", operation.NewName);
                },
                o =>
                {
                    var m = Assert.IsType<InsertDataOperation>(o);
                    Assert.Equal(new[] { "Id", "Value1", "Value2" }, m.Columns);
                    Assert.Null(m.ColumnTypes);
                    AssertMultidimensionalArray(
                        m.Values,
                        v => Assert.Equal(42, v),
                        v => Assert.Equal(32, v),
                        Assert.Null);
                }),
            downOps => Assert.Collection(
                downOps,
                o =>
                {
                    var m = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Equal(new[] { "Id" }, m.KeyColumns);
                    Assert.Null(m.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        m.KeyValues,
                        v => Assert.Equal(42, v));
                },
                o =>
                {
                    var operation = Assert.IsType<RenameColumnOperation>(o);
                    Assert.Equal("EntityWithTwoProperties", operation.Table);
                    Assert.Equal("Id", operation.Name);
                    Assert.Equal("Key", operation.NewName);
                }));

    [ConditionalFact]
    public void SeedData_and_change_PK_type()
        => Execute(
            _ => { },
            source => source.Entity(
                "EntityWithTwoProperties",
                x =>
                {
                    x.Property<string>("Key");
                    x.Property<int>("Value1");
                    x.Property<string>("Value2");
                    x.HasKey("Key");
                }),
            target => target.Entity(
                "EntityWithTwoProperties",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<int>("Value1");
                    x.Property<string>("Value2");
                    x.HasData(
                        new { Id = 42, Value1 = 32 });
                }),
            upOps => Assert.Collection(
                upOps,
                o =>
                {
                    var operation = Assert.IsType<DropPrimaryKeyOperation>(o);
                    Assert.Equal("EntityWithTwoProperties", operation.Table);
                    Assert.Equal("PK_EntityWithTwoProperties", operation.Name);
                },
                o =>
                {
                    var operation = Assert.IsType<DropColumnOperation>(o);
                    Assert.Equal("EntityWithTwoProperties", operation.Table);
                    Assert.Equal("Key", operation.Name);
                },
                o =>
                {
                    var operation = Assert.IsType<AddColumnOperation>(o);
                    Assert.Equal("EntityWithTwoProperties", operation.Table);
                    Assert.Equal("Id", operation.Name);
                },
                o =>
                {
                    var operation = Assert.IsType<AddPrimaryKeyOperation>(o);
                    Assert.Equal("EntityWithTwoProperties", operation.Table);
                    Assert.Equal("PK_EntityWithTwoProperties", operation.Name);
                    Assert.Equal(new[] { "Id" }, operation.Columns);
                },
                o =>
                {
                    var m = Assert.IsType<InsertDataOperation>(o);
                    AssertMultidimensionalArray(
                        m.Values,
                        v => Assert.Equal(42, v),
                        v => Assert.Equal(32, v),
                        Assert.Null);
                }),
            downOps => Assert.Collection(
                downOps,
                o =>
                {
                    var operation = Assert.IsType<DropPrimaryKeyOperation>(o);
                    Assert.Equal("EntityWithTwoProperties", operation.Table);
                    Assert.Equal("PK_EntityWithTwoProperties", operation.Name);
                },
                o =>
                {
                    var m = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Equal(new[] { "Id" }, m.KeyColumns);
                    Assert.Equal(new[] { "default_int_mapping" }, m.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        m.KeyValues,
                        v => Assert.Equal(42, v));
                },
                o =>
                {
                    var operation = Assert.IsType<DropColumnOperation>(o);
                    Assert.Equal("EntityWithTwoProperties", operation.Table);
                    Assert.Equal("Id", operation.Name);
                },
                o =>
                {
                    var operation = Assert.IsType<AddColumnOperation>(o);
                    Assert.Equal("EntityWithTwoProperties", operation.Table);
                    Assert.Equal("Key", operation.Name);
                },
                o =>
                {
                    var operation = Assert.IsType<AddPrimaryKeyOperation>(o);
                    Assert.Equal("EntityWithTwoProperties", operation.Table);
                    Assert.Equal("PK_EntityWithTwoProperties", operation.Name);
                    Assert.Equal(new[] { "Key" }, operation.Columns);
                }));

    [ConditionalFact]
    public void SeedData_binary_change()
        => Execute(
            _ => { },
            source => source.Entity(
                "EntityWithTwoProperties",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<byte[]>("Value1");
                    x.HasData(
                        new { Id = 42, Value1 = new byte[] { 2, 1 } });
                }),
            target => target.Entity(
                "EntityWithTwoProperties",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<byte[]>("Value1");
                    x.HasData(
                        new { Id = 42, Value1 = new byte[] { 1, 2 } });
                }),
            upOps => Assert.Collection(
                upOps,
                o =>
                {
                    var m = Assert.IsType<UpdateDataOperation>(o);
                    AssertMultidimensionalArray(
                        m.KeyValues,
                        v => Assert.Equal(42, v));
                    AssertMultidimensionalArray(
                        m.Values,
                        v => Assert.Equal(new byte[] { 1, 2 }, v));
                }),
            downOps => Assert.Collection(
                downOps,
                o =>
                {
                    var m = Assert.IsType<UpdateDataOperation>(o);
                    AssertMultidimensionalArray(
                        m.KeyValues,
                        v => Assert.Equal(42, v));
                    AssertMultidimensionalArray(
                        m.Values,
                        v => Assert.Equal(new byte[] { 2, 1 }, v));
                }));

    [ConditionalFact]
    public void SeedData_binary_change_custom_comparer()
        => Execute(
            source => source.Entity(
                "EntityWithTwoProperties",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<byte[]>("Value1").HasConversion(typeof(byte[]), null, new RightmostValueComparer());
                }),
            source => source.Entity(
                "EntityWithTwoProperties",
                x =>
                {
                    x.HasData(
                        new { Id = 42, Value1 = new byte[] { 0, 1 } });
                }),
            target => target.Entity(
                "EntityWithTwoProperties",
                x =>
                {
                    x.HasData(
                        new { Id = 42, Value1 = new byte[] { 1 } });
                }),
            upOps => Assert.Empty(upOps),
            downOps => Assert.Empty(downOps));

    private class RightmostValueComparer : ValueComparer<byte[]>
    {
        public RightmostValueComparer()
            : base(false)
        {
        }

        public override bool Equals(byte[] left, byte[] right)
            => object.Equals(left[^1], right[^1]);
    }

    [ConditionalFact]
    public void SeedData_binary_no_change()
        => Execute(
            _ => { },
            source => source.Entity(
                "EntityWithTwoProperties",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<byte[]>("Value1");
                    x.HasData(
                        new { Id = 42, Value1 = new byte[] { 1, 2 } });
                }),
            target => target.Entity(
                "EntityWithTwoProperties",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<byte[]>("Value1");
                    x.HasData(
                        new { Id = 42, Value1 = new byte[] { 1, 2 } });
                }),
            upOps => Assert.Empty(upOps),
            downOps => Assert.Empty(downOps));

    [ConditionalFact]
    public void SeedData_update_with_table_rename()
        => Execute(
            _ => { },
            source => source.Entity(
                "EntityWithTwoProperties",
                x =>
                {
                    x.ToTable("Cat", "dbo");
                    x.Property<int>("Id");
                    x.HasKey("Id").HasName("PK_Cat");
                    x.Property<int>("Value1");
                    x.Property<string>("Value2");
                    x.HasData(
                        new
                        {
                            Id = 42,
                            Value1 = 32,
                            Value2 = "equal"
                        }, // modified
                        new
                        {
                            Id = 24,
                            Value1 = 72,
                            Value2 = "not equal1"
                        }); // modified
                }),
            target => target.Entity(
                "EntityWithTwoProperties",
                x =>
                {
                    x.ToTable("Cats", "dbo");
                    x.Property<int>("Id");
                    x.HasKey("Id").HasName("PK_Cat");
                    x.Property<int>("Value1");
                    x.Property<string>("Value2");
                    x.HasData(
                        new
                        {
                            Id = 42,
                            Value1 = 27,
                            Value2 = "equal"
                        }, // modified
                        new
                        {
                            Id = 24,
                            Value1 = 99,
                            Value2 = "not equal2"
                        }); // modified
                }),
            upOps => Assert.Collection(
                upOps,
                o =>
                {
                    var operation = Assert.IsType<RenameTableOperation>(o);
                    Assert.Equal("Cat", operation.Name);
                    Assert.Equal("dbo", operation.Schema);
                    Assert.Equal("Cats", operation.NewName);
                    Assert.Equal("dbo", operation.NewSchema);
                },
                o =>
                {
                    var m = Assert.IsType<UpdateDataOperation>(o);
                    Assert.Equal("Cats", m.Table);
                    Assert.Equal("dbo", m.Schema);
                    AssertMultidimensionalArray(
                        m.KeyValues,
                        v => Assert.Equal(24, v));
                    AssertMultidimensionalArray(
                        m.Values,
                        v => Assert.Equal(99, v),
                        v => Assert.Equal("not equal2", v));
                },
                o =>
                {
                    var m = Assert.IsType<UpdateDataOperation>(o);
                    Assert.Equal("Cats", m.Table);
                    Assert.Equal("dbo", m.Schema);
                    AssertMultidimensionalArray(
                        m.KeyValues,
                        v => Assert.Equal(42, v));
                    AssertMultidimensionalArray(
                        m.Values,
                        v => Assert.Equal(27, v));
                }),
            downOps => Assert.Collection(
                downOps,
                o =>
                {
                    var operation = Assert.IsType<RenameTableOperation>(o);
                    Assert.Equal("Cats", operation.Name);
                    Assert.Equal("dbo", operation.Schema);
                    Assert.Equal("Cat", operation.NewName);
                    Assert.Equal("dbo", operation.NewSchema);
                },
                o =>
                {
                    var m = Assert.IsType<UpdateDataOperation>(o);
                    Assert.Equal("Cat", m.Table);
                    Assert.Equal("dbo", m.Schema);
                    AssertMultidimensionalArray(
                        m.KeyValues,
                        v => Assert.Equal(24, v));
                    AssertMultidimensionalArray(
                        m.Values,
                        v => Assert.Equal(72, v),
                        v => Assert.Equal("not equal1", v));
                },
                o =>
                {
                    var m = Assert.IsType<UpdateDataOperation>(o);
                    Assert.Equal("Cat", m.Table);
                    Assert.Equal("dbo", m.Schema);
                    AssertMultidimensionalArray(
                        m.KeyValues,
                        v => Assert.Equal(42, v));
                    AssertMultidimensionalArray(
                        m.Values,
                        v => Assert.Equal(32, v));
                }));

    [ConditionalFact]
    public void SeedData_nonkey_refactoring_value_conversion()
        => Execute(
            common => common.Entity(
                "EntityWithOneProperty",
                x => x.Property<int>("Id")),
            source => source.Entity(
                "EntityWithOneProperty",
                x =>
                {
                    x.Property<string>("Value1").IsRequired();
                    x.HasData(
                        new { Id = 42, Value1 = "32" });
                }),
            target => target.Entity(
                "EntityWithOneProperty",
                x =>
                {
                    x.Property<int>("Value1")
                        .HasConversion(e => e.ToString(), e => int.Parse(e));
                    x.HasData(
                        new { Id = 42, Value1 = 32 });
                }),
            Assert.Empty,
            Assert.Empty);

    [ConditionalFact]
    public void SeedData_nonkey_refactoring_value_conversion_to_value_type()
        => Execute(
            common => common.Entity(
                "EntityWithOneProperty",
                x => x.Property<int>("Id")),
            source => source.Entity(
                "EntityWithOneProperty",
                x =>
                {
                    x.Property<DateTime>("Value1");
                    x.HasData(
                        new { Id = 42, Value1 = new DateTime() });
                }),
            target => target.Entity(
                "EntityWithOneProperty",
                x =>
                {
                    x.Property<byte[]>("Value1")
                        .IsRequired()
                        .HasConversion(e => new DateTime(), e => Array.Empty<byte>());
                    x.HasData(
                        new { Id = 42, Value1 = Array.Empty<byte>() });
                }),
            Assert.Empty,
            Assert.Empty);

    [ConditionalFact]
    public void SeedData_nonkey_refactoring_value_conversion_to_value_type_store_generated()
        => Execute(
            common => common.Entity(
                "EntityWithOneProperty",
                x => x.Property<int>("Id")),
            source => source.Entity(
                "EntityWithOneProperty",
                x =>
                {
                    x.Property<DateTime>("Value1")
                        .ValueGeneratedOnAddOrUpdate()
                        .IsConcurrencyToken();
                    x.HasData(
                        new { Id = 42, Value1 = DateTime.Now });
                }),
            target => target.Entity(
                "EntityWithOneProperty",
                x =>
                {
                    x.Property<byte[]>("Value1")
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .IsConcurrencyToken()
                        .HasConversion(e => new DateTime(), e => Array.Empty<byte>());
                    x.HasData(
                        new { Id = 42, Value1 = Array.Empty<byte>() });
                }),
            Assert.Empty,
            Assert.Empty);

    [ConditionalFact]
    public void SeedData_nonkey_refactoring_value_conversion_with_structural_provider_type()
        => Execute(
            common => common.Entity(
                "EntityWithOneProperty",
                x => x.Property<int>("Id")),
            source => source.Entity(
                "EntityWithOneProperty",
                x =>
                {
                    x.Property<int>("Value1")
                        .IsRequired()
                        .HasConversion(e => new[] { e }, e => e[0]);
                    x.HasData(
                        new { Id = 42, Value1 = 32 });
                }),
            target => target.Entity(
                "EntityWithOneProperty",
                x =>
                {
                    x.Property<string>("Value1")
                        .IsRequired()
                        .HasConversion(e => new[] { int.Parse(e) }, e => e[0].ToString());
                    x.HasData(
                        new { Id = 42, Value1 = "32" });
                }),
            Assert.Empty,
            Assert.Empty);

    [ConditionalFact]
    public void SeedData_key_refactoring_value_conversion()
        => Execute(
            common => common.Entity(
                "EntityWithOneProperty",
                x => x.Property<int>("Value1")),
            source => source.Entity(
                "EntityWithOneProperty",
                x =>
                {
                    x.Property<string>("Id");
                    x.HasData(
                        new { Id = "42", Value1 = 32 });
                }),
            target => target.Entity(
                "EntityWithOneProperty",
                x =>
                {
                    x.Property<int>("Id")
                        .HasConversion(e => e.ToString(), e => int.Parse(e));
                    x.HasData(
                        new { Id = 42, Value1 = 32 });
                }),
            Assert.Empty,
            Assert.Empty);

    [ConditionalFact] // Issue #29985
    public void SeedData_value_conversion_nullable_datetime()
        => Execute(
            common => common.Entity(
                "EntityWithOneProperty",
                x =>
                {
                    x.Property<int>("Id");
                    x.HasData(new { Id = 42 });
                }),
            source => source.Entity(
                "EntityWithOneProperty",
                x =>
                {
                    x.Property<DateTime?>("Value1")
                        .HasColumnType("datetime2")
                        .HasConversion(
                            p => p,
                            p => p != null ? DateTime.SpecifyKind(p.Value, DateTimeKind.Utc) : null);
                }),
            target => target.Entity(
                "EntityWithOneProperty",
                x =>
                {
                    x.Property<DateTime?>("Value1")
                        .HasColumnType("datetime2");
                }),
            Assert.Empty,
            Assert.Empty);

    [ConditionalFact]
    public void SeedData_change_enum_conversion()
        => Execute(
            common => common.Entity(
                "EntityWithEnumProperty",
                x =>
                {
                    x.ToTable("EntityWithEnumProperty", "schema");
                    x.Property<int>("Id");
                    x.HasKey("Id");
                    x.Property<SomeEnum?>("Enum").HasDefaultValue(SomeEnum.Default);
                    x.HasData(
                        new { Id = 1, Enum = SomeEnum.NonDefault });
                }),
            _ => { },
            target => target.Entity(
                "EntityWithEnumProperty",
                x =>
                {
                    x.Property<SomeEnum?>("Enum")
                        .HasConversion(e => e.ToString(), e => (SomeEnum)Enum.Parse(typeof(SomeEnum), e));
                }),
            upOps => Assert.Collection(
                upOps,
                o =>
                {
                    var operation = Assert.IsType<AlterColumnOperation>(o);
                    Assert.Equal("Enum", operation.Name);
                    Assert.Equal("EntityWithEnumProperty", operation.Table);
                    Assert.Equal("schema", operation.Schema);
                    Assert.Equal(typeof(string), operation.ClrType);
                    Assert.Equal(nameof(SomeEnum.Default), operation.DefaultValue);
                },
                o =>
                {
                    var m = Assert.IsType<UpdateDataOperation>(o);
                    Assert.Equal("EntityWithEnumProperty", m.Table);
                    Assert.Equal("schema", m.Schema);
                    AssertMultidimensionalArray(
                        m.KeyValues,
                        v => Assert.Equal(1, v));
                    AssertMultidimensionalArray(
                        m.Values,
                        v => Assert.Equal(nameof(SomeEnum.NonDefault), v));
                }),
            downOps => Assert.Collection(
                downOps,
                o =>
                {
                    var operation = Assert.IsType<AlterColumnOperation>(o);
                    Assert.Equal("Enum", operation.Name);
                    Assert.Equal("EntityWithEnumProperty", operation.Table);
                    Assert.Equal("schema", operation.Schema);
                    Assert.Equal(typeof(int), operation.ClrType);
                    Assert.Equal((int)SomeEnum.Default, operation.DefaultValue);
                },
                o =>
                {
                    var m = Assert.IsType<UpdateDataOperation>(o);
                    Assert.Equal("EntityWithEnumProperty", m.Table);
                    Assert.Equal("schema", m.Schema);
                    AssertMultidimensionalArray(
                        m.KeyValues,
                        v => Assert.Equal(1, v));
                    AssertMultidimensionalArray(
                        m.Values,
                        v => Assert.Equal((int)SomeEnum.NonDefault, v));
                }));

    [ConditionalFact]
    public void SeedData_change_with_default()
        => Execute(
            common => common.Entity(
                "EntityWithEnumProperty",
                x =>
                {
                    x.ToTable("EntityWithEnumProperty", "schema");
                    x.Property<int>("Id");
                    x.HasKey("Id");
                    x.Property<SomeEnum>("Enum").HasDefaultValue(SomeEnum.Default);
                }),
            source => source.Entity(
                "EntityWithEnumProperty",
                x =>
                {
                    x.HasData(
                        new { Id = 1, Enum = SomeEnum.NonDefault });
                }),
            target => target.Entity(
                "EntityWithEnumProperty",
                x =>
                {
                    x.HasData(
                        new { Id = 1, Enum = SomeEnum.Default });
                }),
            upOps => Assert.Collection(
                upOps,
                o =>
                {
                    var m = Assert.IsType<UpdateDataOperation>(o);
                    Assert.Equal("EntityWithEnumProperty", m.Table);
                    Assert.Equal("schema", m.Schema);
                    AssertMultidimensionalArray(
                        m.KeyValues,
                        v => Assert.Equal(1, v));
                    AssertMultidimensionalArray(
                        m.Values,
                        v => Assert.Equal((int)SomeEnum.Default, v));
                }),
            downOps => Assert.Collection(
                downOps,
                o =>
                {
                    var m = Assert.IsType<UpdateDataOperation>(o);
                    Assert.Equal("EntityWithEnumProperty", m.Table);
                    Assert.Equal("schema", m.Schema);
                    AssertMultidimensionalArray(
                        m.KeyValues,
                        v => Assert.Equal(1, v));
                    AssertMultidimensionalArray(
                        m.Values,
                        v => Assert.Equal((int)SomeEnum.NonDefault, v));
                }));

    [ConditionalFact]
    public void SeedData_no_change_enum_key()
        => Execute(
            _ => { },
            source => source.Entity(
                "EntityWithEnumKey",
                x =>
                {
                    x.ToTable("EntityWithEnumKey", "schema");
                    x.Property<int>("Enum");
                    x.HasKey("Enum");
                    x.HasData(
                        new { Enum = 1 });
                }),
            target => target.Entity(
                "EntityWithEnumKey",
                x =>
                {
                    x.ToTable("EntityWithEnumKey", "schema");
                    x.Property<SomeEnum>("Enum");
                    x.HasKey("Enum");
                    x.HasData(
                        new { Enum = SomeEnum.NonDefault });
                }),
            Assert.Empty,
            Assert.Empty);

    [ConditionalFact]
    public void SeedData_all_operations()
        => Execute(
            _ => { },
            source => source.Entity(
                "EntityWithTwoProperties",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<int>("Value1");
                    x.Property<string>("Value2");
                    x.HasData(
                        new
                        {
                            Id = 99999,
                            Value1 = 0,
                            Value2 = ""
                        }, // deleted
                        new
                        {
                            Id = 42,
                            Value1 = 32,
                            Value2 = "equal",
                            InvalidProperty = "is ignored"
                        }, // modified
                        new
                        {
                            Id = 8,
                            Value1 = 100,
                            Value2 = "equal"
                        }, // unchanged
                        new
                        {
                            Id = 24,
                            Value1 = 72,
                            Value2 = "not equal1"
                        }); // modified
                }),
            target => target.Entity(
                "EntityWithTwoProperties",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<int>("Value1");
                    x.Property<string>("Value2");
                    x.HasData(
                        new
                        {
                            Id = 11111,
                            Value1 = 0,
                            Value2 = ""
                        }, // added
                        new
                        {
                            Id = 11112,
                            Value1 = 1,
                            Value2 = "new"
                        }, // added
                        new
                        {
                            Id = 42,
                            Value1 = 27,
                            Value2 = "equal",
                            InvalidProperty = "is ignored here too"
                        }, // modified
                        new
                        {
                            Id = 8,
                            Value1 = 100,
                            Value2 = "equal"
                        }, // unchanged
                        new
                        {
                            Id = 24,
                            Value1 = 99,
                            Value2 = "not equal2"
                        }); // modified
                }),
            upOps => Assert.Collection(
                upOps,
                o =>
                {
                    var m = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Null(m.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        m.KeyValues,
                        v => Assert.Equal(99999, v));
                },
                o =>
                {
                    var m = Assert.IsType<UpdateDataOperation>(o);
                    Assert.Null(m.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        m.KeyValues,
                        v => Assert.Equal(24, v));
                    AssertMultidimensionalArray(
                        m.Values,
                        v => Assert.Equal(99, v),
                        v => Assert.Equal("not equal2", v));
                },
                o =>
                {
                    var m = Assert.IsType<UpdateDataOperation>(o);
                    Assert.Null(m.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        m.KeyValues,
                        v => Assert.Equal(42, v));
                    AssertMultidimensionalArray(
                        m.Values,
                        v => Assert.Equal(27, v));
                },
                o =>
                {
                    var m = Assert.IsType<InsertDataOperation>(o);
                    Assert.Null(m.ColumnTypes);
                    AssertMultidimensionalArray(
                        m.Values,
                        v => Assert.Equal(11111, v),
                        v => Assert.Equal(0, v),
                        v => Assert.Equal("", v),
                        v => Assert.Equal(11112, v),
                        v => Assert.Equal(1, v),
                        v => Assert.Equal("new", v));
                }),
            downOps => Assert.Collection(
                downOps,
                o =>
                {
                    var m = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Null(m.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        m.KeyValues,
                        v => Assert.Equal(11111, v));
                },
                o =>
                {
                    var m = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Null(m.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        m.KeyValues,
                        v => Assert.Equal(11112, v));
                },
                o =>
                {
                    var m = Assert.IsType<UpdateDataOperation>(o);
                    Assert.Null(m.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        m.KeyValues,
                        v => Assert.Equal(24, v));
                    AssertMultidimensionalArray(
                        m.Values,
                        v => Assert.Equal(72, v),
                        v => Assert.Equal("not equal1", v));
                },
                o =>
                {
                    var m = Assert.IsType<UpdateDataOperation>(o);
                    Assert.Null(m.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        m.KeyValues,
                        v => Assert.Equal(42, v));
                    AssertMultidimensionalArray(
                        m.Values,
                        v => Assert.Equal(32, v));
                },
                o =>
                {
                    var m = Assert.IsType<InsertDataOperation>(o);
                    Assert.Null(m.ColumnTypes);
                    AssertMultidimensionalArray(
                        m.Values,
                        v => Assert.Equal(99999, v),
                        v => Assert.Equal(0, v),
                        v => Assert.Equal("", v));
                }));

    [ConditionalFact]
    public void SeedData_with_timestamp_column()
        => Execute(
            _ => { },
            source => source.Entity(
                "EntityWithTimeStamp",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<string>("Value");
                    x.Property<int>("DefaultValue").HasDefaultValue(42);
                    x.Property<int>("DefaultValueSql").HasDefaultValueSql("1");
                    x.Property<int>("ComputedValueSql").HasComputedColumnSql("5");
                    x.Property<byte[]>("TimeStamp").IsRowVersion();
                    x.HasData(
                        new { Id = 11, Value = "Value" }, //Modified
                        new
                        {
                            Id = 12,
                            Value = "Value",
                            DefaultValue = 5,
                            DefaultValueSql = 5,
                            ComputedValueSql = 5
                        }, //Modified
                        new { Id = 21, Value = "Deleted" }); //Deleted
                }),
            target => target.Entity(
                "EntityWithTimeStamp",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<string>("Value");
                    x.Property<int>("DefaultValue").HasDefaultValue(42);
                    x.Property<int>("DefaultValueSql").HasDefaultValueSql("1");
                    x.Property<int>("ComputedValueSql").HasComputedColumnSql("5");
                    x.Property<byte[]>("TimeStamp").IsRowVersion();
                    x.HasData(
                        new { Id = 11, Value = "Modified" }, //Modified
                        new
                        {
                            Id = 12,
                            Value = "Modified",
                            DefaultValue = 6,
                            DefaultValueSql = 6,
                            ComputedValueSql = 5
                        }, //Modified
                        new { Id = 31, Value = "Added" }, //Added
                        new
                        {
                            Id = 32,
                            Value = "DefaultValuesProvided",
                            DefaultValue = 42,
                            DefaultValueSql = 42,
                            ComputedValueSql = 42
                        }); //Added
                }),
            upOps => Assert.Collection(
                upOps,
                o =>
                {
                    var m = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Null(m.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        m.KeyValues,
                        v => Assert.Equal(21, v));
                },
                o =>
                {
                    var m = Assert.IsType<UpdateDataOperation>(o);
                    Assert.Null(m.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        m.KeyValues,
                        v => Assert.Equal(11, v));
                    AssertMultidimensionalArray(
                        m.Values,
                        v => Assert.Equal("Modified", v));
                },
                o =>
                {
                    var m = Assert.IsType<UpdateDataOperation>(o);
                    Assert.Null(m.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        m.KeyValues,
                        v => Assert.Equal(12, v));
                    AssertMultidimensionalArray(
                        m.Values,
                        v => Assert.Equal(6, v),
                        v => Assert.Equal(6, v),
                        v => Assert.Equal("Modified", v));
                },
                o =>
                {
                    var m = Assert.IsType<InsertDataOperation>(o);
                    Assert.Null(m.ColumnTypes);
                    AssertMultidimensionalArray(
                        m.Values,
                        v => Assert.Equal(31, v),
                        v => Assert.Equal("Added", v));
                },
                o =>
                {
                    var m = Assert.IsType<InsertDataOperation>(o);
                    Assert.Null(m.ColumnTypes);
                    AssertMultidimensionalArray(
                        m.Values,
                        v => Assert.Equal(32, v),
                        v => Assert.Equal(42, v),
                        v => Assert.Equal(42, v),
                        v => Assert.Equal("DefaultValuesProvided", v));
                }),
            downOps => Assert.Collection(
                downOps,
                o =>
                {
                    var m = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Null(m.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        m.KeyValues,
                        v => Assert.Equal(31, v));
                },
                o =>
                {
                    var m = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Null(m.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        m.KeyValues,
                        v => Assert.Equal(32, v));
                },
                o =>
                {
                    var m = Assert.IsType<UpdateDataOperation>(o);
                    AssertMultidimensionalArray(
                        m.KeyValues,
                        v => Assert.Equal(11, v));
                    AssertMultidimensionalArray(
                        m.Values,
                        v => Assert.Equal("Value", v));
                },
                o =>
                {
                    var m = Assert.IsType<UpdateDataOperation>(o);
                    AssertMultidimensionalArray(
                        m.KeyValues,
                        v => Assert.Equal(12, v));
                    AssertMultidimensionalArray(
                        m.Values,
                        v => Assert.Equal(5, v),
                        v => Assert.Equal(5, v),
                        v => Assert.Equal("Value", v));
                },
                o =>
                {
                    var m = Assert.IsType<InsertDataOperation>(o);
                    AssertMultidimensionalArray(
                        m.Values,
                        v => Assert.Equal(21, v),
                        v => Assert.Equal("Deleted", v));
                }));

    [ConditionalFact]
    public void SeedData_with_shadow_navigation_properties()
        => SeedData_with_navigation_properties(
            target =>
            {
                target.Entity(
                    "Blog",
                    x =>
                    {
                        x.Property<int>("BlogId");
                        x.Property<string>("Url");
                        x.HasData(
                            new { BlogId = 32, Url = "updated.url" },
                            new { BlogId = 38, Url = "newblog.url" },
                            new { BlogId = 316, Url = "nowitexists.blog" });
                    });
                target.Entity(
                    "Post",
                    x =>
                    {
                        x.Property<int>("PostId");
                        x.Property<string>("Title");
                        x.HasOne("Blog", "Blog")
                            .WithMany("Posts")
                            .HasForeignKey("BlogId")
                            .OnDelete(DeleteBehavior.Cascade);
                        x.HasData(
                            new
                            {
                                PostId = 416,
                                Title = "Post To Non-existent BlogId",
                                BlogId = 316
                            },
                            new
                            {
                                PostId = 545,
                                Title = "Updated Title",
                                BlogId = 38
                            },
                            new
                            {
                                PostId = 546,
                                Title = "New Post",
                                BlogId = 32
                            });
                    });
            });

    [ConditionalFact]
    public void SeedData_with_CLR_navigation_properties()
        => SeedData_with_navigation_properties(
            target =>
            {
                target.Entity<Blog>(
                    x =>
                    {
                        x.Property<int>("BlogId");
                        x.Property<string>("Url");
                        x.HasData(
                            new { BlogId = 32, Url = "updated.url" },
                            new { BlogId = 38, Url = "newblog.url" },
                            new { BlogId = 316, Url = "nowitexists.blog" });
                    });
                target.Entity<Post>(
                    x =>
                    {
                        x.Property<int>("PostId");
                        x.Property<string>("Title");
                        x.HasOne(p => p.Blog)
                            .WithMany("Posts")
                            .HasForeignKey("BlogId")
                            .OnDelete(DeleteBehavior.Cascade);
                        x.HasData(
                            new
                            {
                                PostId = 416,
                                Title = "Post To Non-existent BlogId",
                                BlogId = 316
                            },
                            new
                            {
                                PostId = 545,
                                Title = "Updated Title",
                                BlogId = 38
                            },
                            new
                            {
                                PostId = 546,
                                Title = "New Post",
                                BlogId = 32
                            });
                    });
            });

    private void SeedData_with_navigation_properties(Action<ModelBuilder> buildTargetAction)
        => Execute(
            _ => { },
            source =>
            {
                source.Entity(
                    "Blog",
                    x =>
                    {
                        x.Property<int>("BlogId");
                        x.Property<string>("Url");
                        x.HasData(
                            new { BlogId = 32, Url = "original.url" });
                    });
                source.Entity(
                    "Post",
                    x =>
                    {
                        x.Property<int>("PostId");
                        x.Property<string>("Title");
                        x.HasOne("Blog", "Blog")
                            .WithMany("Posts")
                            .HasForeignKey("BlogId")
                            .OnDelete(DeleteBehavior.Cascade);
                        x.HasData(
                            new
                            {
                                PostId = 545,
                                Title = "Original Title",
                                BlogId = 32
                            },
                            new
                            {
                                PostId = 416,
                                Title = "Post To Non-existent BlogId",
                                BlogId = 316
                            },
                            new
                            {
                                PostId = 390,
                                Title = "Post To Be Removed",
                                BlogId = 32
                            });
                    });
            },
            buildTargetAction,
            upOps => Assert.Collection(
                upOps,
                o =>
                {
                    var m = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Equal("Post", m.Table);
                    Assert.Null(m.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        m.KeyValues,
                        v => Assert.Equal(390, v));
                },
                o =>
                {
                    var m = Assert.IsType<UpdateDataOperation>(o);
                    Assert.Equal("Blog", m.Table);
                    Assert.Null(m.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        m.KeyValues,
                        v => Assert.Equal(32, v));
                    AssertMultidimensionalArray(
                        m.Values,
                        v => Assert.Equal("updated.url", v));
                },
                o =>
                {
                    var m = Assert.IsType<InsertDataOperation>(o);
                    Assert.Equal("Blog", m.Table);
                    AssertMultidimensionalArray(
                        m.Values,
                        v => Assert.Equal(38, v),
                        v => Assert.Equal("newblog.url", v),
                        v => Assert.Equal(316, v),
                        v => Assert.Equal("nowitexists.blog", v));
                },
                o =>
                {
                    var m = Assert.IsType<UpdateDataOperation>(o);
                    Assert.Equal("Post", m.Table);
                    AssertMultidimensionalArray(
                        m.KeyValues,
                        v => Assert.Equal(545, v));
                    AssertMultidimensionalArray(
                        m.Values,
                        v => Assert.Equal(38, v),
                        v => Assert.Equal("Updated Title", v));
                },
                o =>
                {
                    var m = Assert.IsType<InsertDataOperation>(o);
                    Assert.Equal("Post", m.Table);
                    AssertMultidimensionalArray(
                        m.Values,
                        v => Assert.Equal(546, v),
                        v => Assert.Equal(32, v),
                        v => Assert.Equal("New Post", v));
                }),
            downOps => Assert.Collection(
                downOps,
                o =>
                {
                    var m = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Equal("Blog", m.Table);
                    Assert.Null(m.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        m.KeyValues,
                        v => Assert.Equal(38, v));
                },
                o =>
                {
                    var m = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Equal("Blog", m.Table);
                    AssertMultidimensionalArray(
                        m.KeyValues,
                        v => Assert.Equal(316, v));
                },
                o =>
                {
                    var m = Assert.IsType<DeleteDataOperation>(o);
                    Assert.Equal("Post", m.Table);
                    Assert.Null(m.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        m.KeyValues,
                        v => Assert.Equal(546, v));
                },
                o =>
                {
                    var m = Assert.IsType<UpdateDataOperation>(o);
                    Assert.Equal("Blog", m.Table);
                    Assert.Null(m.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        m.KeyValues,
                        v => Assert.Equal(32, v));
                    AssertMultidimensionalArray(
                        m.Values,
                        v => Assert.Equal("original.url", v));
                },
                o =>
                {
                    var m = Assert.IsType<UpdateDataOperation>(o);
                    Assert.Equal("Post", m.Table);
                    Assert.Null(m.KeyColumnTypes);
                    AssertMultidimensionalArray(
                        m.KeyValues,
                        v => Assert.Equal(545, v));
                    AssertMultidimensionalArray(
                        m.Values,
                        v => Assert.Equal(32, v),
                        v => Assert.Equal("Original Title", v));
                },
                o =>
                {
                    var m = Assert.IsType<InsertDataOperation>(o);
                    Assert.Equal("Post", m.Table);
                    Assert.Null(m.ColumnTypes);
                    AssertMultidimensionalArray(
                        m.Values,
                        v => Assert.Equal(390, v),
                        v => Assert.Equal(32, v),
                        v => Assert.Equal("Post To Be Removed", v));
                }));

    private class OldOrder
    {
        public int Id { get; set; }

        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }

        public Address Billing { get; set; }
    }

    private class Order
    {
        private readonly int _secretId;

        public Order()
        {
        }

        public Order(int secretId)
        {
            _secretId = secretId;
        }

        public int Id { get; set; }

        public Address Billing { get; set; }
        public Address Shipping { get; set; }
    }

    private class Customer
    {
        public int Id { get; set; }

        public Address Mailing { get; set; }
        public ICollection<Order> Orders { get; set; }
    }

    private class Address
    {
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
    }

    [ConditionalFact]
    public void Add_property_on_owned_type()
        => Execute(
            common => common.Entity<Order>(
                x =>
                {
                    x.OwnsOne(y => y.Billing);
                    x.OwnsOne(y => y.Shipping);
                }),
            source => source.Entity<Order>().OwnsOne(y => y.Shipping).Ignore("AddressLine2"),
            target => { },
            upOperations =>
            {
                var operation = Assert.IsType<AddColumnOperation>(Assert.Single(upOperations));
                Assert.Equal("Order", operation.Table);
                Assert.Equal("Shipping_AddressLine2", operation.Name);
            },
            downOperations =>
            {
                var operation = Assert.IsType<DropColumnOperation>(Assert.Single(downOperations));
                Assert.Equal("Order", operation.Table);
                Assert.Equal("Shipping_AddressLine2", operation.Name);
            });

    [ConditionalFact]
    public void Add_ownership()
        => Execute(
            common => { },
            source => source.Entity<OldOrder>().ToTable("Order").Ignore(x => x.AddressLine1)
                .Ignore(x => x.AddressLine2).OwnsOne(y => y.Billing),
            target => target.Entity<Order>(
                x =>
                {
                    x.OwnsOne(y => y.Billing);
                    x.OwnsOne(y => y.Shipping);
                }),
            upOperations =>
            {
                Assert.Equal(2, upOperations.Count);

                var operation1 = Assert.IsType<AddColumnOperation>(upOperations[0]);
                Assert.Equal("Order", operation1.Table);
                Assert.Equal("Shipping_AddressLine1", operation1.Name);

                var operation2 = Assert.IsType<AddColumnOperation>(upOperations[1]);
                Assert.Equal("Order", operation2.Table);
                Assert.Equal("Shipping_AddressLine2", operation2.Name);
            },
            downOperations =>
            {
                Assert.Equal(2, downOperations.Count);

                var operation1 = Assert.IsType<DropColumnOperation>(downOperations[0]);
                Assert.Equal("Order", operation1.Table);
                Assert.Equal("Shipping_AddressLine1", operation1.Name);

                var operation2 = Assert.IsType<DropColumnOperation>(downOperations[1]);
                Assert.Equal("Order", operation2.Table);
                Assert.Equal("Shipping_AddressLine2", operation2.Name);
            });

    [ConditionalFact]
    public void Add_type_with_additional_ownership()
        => Execute(
            source => source
                .Entity<Customer>()
                .Ignore(c => c.Orders)
                .OwnsOne(y => y.Mailing),
            target => target
                .Entity<Order>(
                    x =>
                    {
                        x.OwnsOne(y => y.Billing);
                        x.OwnsOne(y => y.Shipping);
                    })
                .Entity<Customer>()
                .Ignore(c => c.Orders)
                .OwnsOne(y => y.Mailing),
            operations =>
            {
                var operation = Assert.IsType<CreateTableOperation>(Assert.Single(operations));
                Assert.Equal("Order", operation.Name);
            });

    [ConditionalFact]
    public void Add_type_with_ownership_SeedData()
        => Execute(
            common => common.Ignore<Customer>(),
            _ => { },
            target => target
                .Entity<Order>(
                    x =>
                    {
                        x.Property<int>("_secretId");
                        x.HasData(
                            new Order(42) { Id = 1 });
                        x.OwnsOne(y => y.Billing).HasData(
                            new { OrderId = 1, AddressLine1 = "billing" });
                        x.OwnsOne(y => y.Shipping).HasData(
                            new { OrderId = 1, AddressLine2 = "shipping" });
                    }),
            upOps => Assert.Collection(
                upOps,
                o =>
                {
                    var m = Assert.IsType<CreateTableOperation>(o);
                    Assert.Equal("Order", m.Name);
                },
                o =>
                {
                    var m = Assert.IsType<InsertDataOperation>(o);
                    Assert.Equal("Order", m.Table);
                    AssertMultidimensionalArray(
                        m.Values,
                        v => Assert.Equal(1, v),
                        v => Assert.Equal(42, v),
                        v => Assert.Equal("billing", v),
                        Assert.Null,
                        Assert.Null,
                        v => Assert.Equal("shipping", v));
                }),
            downOps => Assert.Collection(
                downOps,
                o =>
                {
                    var m = Assert.IsType<DropTableOperation>(o);
                    Assert.Equal("Order", m.Name);
                }),
            skipSourceConventions: true);

    [ConditionalFact]
    public void SeedData_type_with_ownership_no_changes()
        => Execute(
            common =>
            {
                common.Ignore<Customer>();
                common.Entity<Order>(
                    x =>
                    {
                        x.Property<int>("_secretId");
                        x.HasData(
                            new Order(42) { Id = 1 });
                        x.OwnsOne(y => y.Billing).HasData(
                            new { OrderId = 1, AddressLine1 = "billing" });
                        x.OwnsOne(y => y.Shipping).HasData(
                            new { OrderId = 1, AddressLine2 = "shipping" });
                    });
            },
            _ => { },
            _ => { },
            Assert.Empty,
            Assert.Empty);

    [ConditionalFact]
    public void SeedData_type_with_owned_collection_no_changes()
        => Execute(
            common =>
            {
                common.Entity<Customer>(
                    c =>
                    {
                        c.Ignore(x => x.Mailing);

                        c.HasKey(x => x.Id);
                        c.HasData(new Customer { Id = 1 });

                        c.OwnsMany(
                            y => y.Orders, x =>
                            {
                                x.Ignore(o => o.Billing);
                                x.Ignore(o => o.Shipping);

                                x.WithOwner()
                                    .HasForeignKey("CustomerId");

                                x.HasKey("CustomerId", "Id");
                                x.HasData(new { Id = 2, CustomerId = 1 });
                            });
                    });
            },
            _ => { },
            _ => { },
            Assert.Empty,
            Assert.Empty,
            skipSourceConventions: true);

    [ConditionalFact]
    public void SeedData_type_with_excluded_owned_collection()
        => Execute(
            common =>
            {
                common.Entity<Customer>(
                    c =>
                    {
                        c.Ignore(x => x.Mailing);

                        c.HasKey(x => x.Id);
                        c.HasData(new Customer { Id = 1 });

                        c.OwnsMany(
                            y => y.Orders, x =>
                            {
                                x.Ignore(o => o.Billing);
                                x.Ignore(o => o.Shipping);

                                x.WithOwner()
                                    .HasForeignKey("CustomerId");

                                x.HasKey("CustomerId", "Id");
                                x.HasData(new { Id = 2, CustomerId = 1 });
                            });
                    });
            },
            _ => { },
            target =>
            {
                target.Entity<Customer>(
                    c =>
                    {
                        c.OwnsMany(
                            y => y.Orders, x =>
                            {
                                x.ToTable("Order", t => t.ExcludeFromMigrations());
                            });
                        c.ToTable("Customer", t => t.ExcludeFromMigrations());
                    });
            },
            Assert.Empty,
            Assert.Empty,
            skipSourceConventions: true);

    public class Parent
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public IEnumerable<Child> Children { get; set; }
    }

    public class Child
    {
        public Guid Id { get; set; }
        public string ChildName { get; set; }
    }

    [ConditionalFact]
    public void Old_style_ownership_to_new_style()
        => Execute(
            common =>
            {
                common.Entity(
                    "Order", b =>
                    {
                        b.Property<int>("Id")
                            .ValueGeneratedOnAdd();

                        b.HasKey("Id");

                        b.ToTable("Order");
                    });
            },
            source =>
            {
                source.HasAnnotation("ProductVersion", "2.2.6-servicing-10079");
                source.Entity(
                    "Order", b =>
                    {
                        b.OwnsOne(
                            "OrderInfo", "OrderInfo", b1 =>
                            {
                                b1.Property<int>("OrderId")
                                    .ValueGeneratedOnAdd();

                                b1.HasKey("OrderId");

                                b1.ToTable("Order");

                                b1.HasOne("Order", "Order")
                                    .WithOne("OrderInfo")
                                    .HasForeignKey("OrderInfo", "OrderId")
                                    .OnDelete(DeleteBehavior.Cascade);

                                b1.OwnsOne(
                                    "Address", "ShippingAddress", b2 =>
                                    {
                                        b2.Property<int>("OrderId")
                                            .ValueGeneratedOnAdd();

                                        b2.HasKey("OrderId");

                                        b2.ToTable("Order");

                                        b2.HasOne("OrderInfo")
                                            .WithOne("ShippingAddress")
                                            .HasForeignKey("Address", "OrderId")
                                            .OnDelete(DeleteBehavior.Cascade);
                                    });

                                b1.OwnsOne(
                                    "Address", "BillingAddress", b2 =>
                                    {
                                        b2.Property<int>("OrderId")
                                            .ValueGeneratedOnAdd();

                                        b2.HasKey("OrderId");

                                        b2.ToTable("Order");

                                        b2.HasOne("OrderInfo")
                                            .WithOne("BillingAddress")
                                            .HasForeignKey("Address", "OrderId")
                                            .OnDelete(DeleteBehavior.Cascade);
                                    });
                            });
                    });
            },
            target =>
            {
                target.Entity(
                    "Order", b =>
                    {
                        b.OwnsOne(
                            "OrderInfo", "OrderInfo", b1 =>
                            {
                                b1.Property<int>("OrderId")
                                    .ValueGeneratedOnAdd();

                                b1.HasKey("OrderId");

                                b1.ToTable("Order");

                                b1.WithOwner("Order")
                                    .HasForeignKey("OrderId");

                                b1.OwnsOne(
                                    "Address", "ShippingAddress", b2 =>
                                    {
                                        b2.Property<int>("OrderId")
                                            .ValueGeneratedOnAdd();

                                        b2.HasKey("OrderId");

                                        b2.ToTable("Order");

                                        b2.WithOwner()
                                            .HasForeignKey("OrderId");
                                    });

                                b1.OwnsOne(
                                    "Address", "BillingAddress", b2 =>
                                    {
                                        b2.Property<int>("OrderId")
                                            .ValueGeneratedOnAdd();

                                        b2.HasKey("OrderId");

                                        b2.ToTable("Order");

                                        b2.WithOwner()
                                            .HasForeignKey("OrderId");
                                    });
                            });
                    });
            },
            Assert.Empty,
            Assert.Empty);

    [ConditionalFact]
    public void Move_properties_to_owned_type()
        => Execute(
            source => source.Ignore<Address>().Entity<OldOrder>(),
            target => target.Entity<OldOrder>().Ignore(x => x.AddressLine1).Ignore(x => x.AddressLine2)
                .OwnsOne(y => y.Billing),
            operations =>
            {
                Assert.Equal(2, operations.Count);

                var operation1 = Assert.IsType<RenameColumnOperation>(operations[0]);
                Assert.Equal("OldOrder", operation1.Table);
                Assert.Equal("AddressLine2", operation1.Name);
                Assert.Equal("Billing_AddressLine2", operation1.NewName);

                var operation2 = Assert.IsType<RenameColumnOperation>(operations[1]);
                Assert.Equal("OldOrder", operation2.Table);
                Assert.Equal("AddressLine1", operation2.Name);
                Assert.Equal("Billing_AddressLine1", operation2.NewName);
            });

    [ConditionalFact]
    public void Move_properties_to_owned_type_with_existing_ownership()
        => Execute(
            source => source.Entity<OldOrder>().ToTable("Order").OwnsOne(o => o.Billing),
            target => target.Entity<Order>(
                x =>
                {
                    x.OwnsOne(o => o.Billing);
                    x.OwnsOne(o => o.Shipping);
                }),
            operations =>
            {
                Assert.Equal(2, operations.Count);

                var operation1 = Assert.IsType<RenameColumnOperation>(operations[0]);
                Assert.Equal("Order", operation1.Table);
                Assert.Equal("AddressLine2", operation1.Name);
                Assert.Equal("Shipping_AddressLine2", operation1.NewName);

                var operation2 = Assert.IsType<RenameColumnOperation>(operations[1]);
                Assert.Equal("Order", operation2.Table);
                Assert.Equal("AddressLine1", operation2.Name);
                Assert.Equal("Shipping_AddressLine1", operation2.NewName);
            });

    [ConditionalFact]
    public void Rename_property_on_owned_type_and_add_similar_to_owner()
        => Execute(
            source => source.Entity<Order>(
                x =>
                {
                    x.OwnsOne(o => o.Billing).Property<int>("OldZip");
                    x.Ignore(o => o.Shipping);
                }),
            target => target.Entity<Order>(
                x =>
                {
                    x.Property<int>("NotZip");
                    x.OwnsOne(o => o.Billing).Property<int>("NewZip");
                    x.Ignore(o => o.Shipping);
                }),
            operations =>
            {
                Assert.Equal(2, operations.Count);

                var operation1 = Assert.IsType<RenameColumnOperation>(operations[0]);
                Assert.Equal("Order", operation1.Table);
                Assert.Equal("Billing_OldZip", operation1.Name);
                Assert.Equal("Billing_NewZip", operation1.NewName);

                var operation2 = Assert.IsType<AddColumnOperation>(operations[1]);
                Assert.Equal("Order", operation2.Table);
                Assert.Equal("NotZip", operation2.Name);
            });

    [ConditionalFact]
    public void Rename_property_on_owning_type_and_add_similar_to_owned()
        => Execute(
            source => source.Entity<Order>(
                x =>
                {
                    x.Property<DateTime>("OldDate");
                    x.OwnsOne(o => o.Billing);
                    x.Ignore(o => o.Shipping);
                }),
            target => target.Entity<Order>(
                x =>
                {
                    x.Property<DateTime>("NewDate");
                    x.OwnsOne(o => o.Billing).Property<DateTime>("AnotherDate");
                    x.Ignore(o => o.Shipping);
                }),
            operations =>
            {
                Assert.Equal(2, operations.Count);

                var operation1 = Assert.IsType<RenameColumnOperation>(operations[0]);
                Assert.Equal("Order", operation1.Table);
                Assert.Equal("OldDate", operation1.Name);
                Assert.Equal("NewDate", operation1.NewName);

                var operation2 = Assert.IsType<AddColumnOperation>(operations[1]);
                Assert.Equal("Order", operation2.Table);
                Assert.Equal("Billing_AnotherDate", operation2.Name);
            });

    [ConditionalFact]
    public void Rename_property_on_dependent_and_add_similar_to_principal_with_shared_table()
        => Execute(
            common => common
                .Entity<OldOrder>(x => x.HasOne(o => o.Billing).WithOne().HasForeignKey<Address>("Id"))
                .Entity<Address>().ToTable("OldOrder"),
            source => source
                .Entity<OldOrder>(x => { })
                .Entity<Address>().Property<int>("OldZip"),
            target => target
                .Entity<OldOrder>(x => x.Property<int>("NotZip"))
                .Entity<Address>().Property<int>("NewZip"),
            operations =>
            {
                Assert.Equal(2, operations.Count);

                var operation1 = Assert.IsType<RenameColumnOperation>(operations[0]);
                Assert.Equal("OldOrder", operation1.Table);
                Assert.Equal("OldZip", operation1.Name);
                Assert.Equal("NewZip", operation1.NewName);

                var operation2 = Assert.IsType<AddColumnOperation>(operations[1]);
                Assert.Equal("OldOrder", operation2.Table);
                Assert.Equal("NotZip", operation2.Name);
            });

    [ConditionalFact]
    public void Rename_property_on_subtype_and_add_similar_to_base()
        => Execute(
            source => source
                .Entity("AddressBase", x => x.Property<int>("Id"))
                .Entity("Address").HasBaseType("AddressBase").Property<int>("OldZip"),
            target => target
                .Entity(
                    "AddressBase",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<int>("NotZip");
                    })
                .Entity("Address").HasBaseType("AddressBase").Property<int>("NewZip"),
            operations =>
            {
                Assert.Equal(2, operations.Count);

                var operation1 = Assert.IsType<RenameColumnOperation>(operations[0]);
                Assert.Equal("AddressBase", operation1.Table);
                Assert.Equal("OldZip", operation1.Name);
                Assert.Equal("NewZip", operation1.NewName);

                var operation2 = Assert.IsType<AddColumnOperation>(operations[1]);
                Assert.Equal("AddressBase", operation2.Table);
                Assert.Equal("NotZip", operation2.Name);
            });

    [ConditionalFact]
    public void Owner_pk_properties_appear_before_owned_pk_which_preserves_annotations()
        => Execute(
            _ => { },
            target => target.Entity<Customer13300>(
                builder =>
                {
                    builder.OwnsOne(
                        o => o.Created,
                        sa => sa.Property(p => p.Reason).HasMaxLength(255).IsUnicode(false));

                    builder.Property(x => x.TenantId).IsRequired();
                    builder.HasKey(x => new { x.TenantId, x.ProviderKey });
                    builder.Property(x => x.ProviderKey).HasMaxLength(50).IsUnicode(false);
                }),
            operations =>
            {
                var createTableOperation = Assert.IsType<CreateTableOperation>(Assert.Single(operations));

                Assert.Collection(
                    createTableOperation.Columns,
                    c =>
                    {
                        Assert.Equal("TenantId", c.Name);
                        Assert.False(c.IsNullable);
                    },
                    c =>
                    {
                        Assert.Equal("ProviderKey", c.Name);
                        Assert.Equal(50, c.MaxLength);
                        Assert.False(c.IsUnicode);
                    },
                    c => Assert.Equal("DisplayName", c.Name),
                    c =>
                    {
                        Assert.Equal("Created_Reason", c.Name);
                        Assert.Equal(255, c.MaxLength);
                        Assert.False(c.IsUnicode);
                    }
                );
            });

    [ConditionalFact]
    public void Alter_database_collation()
        => Execute(
            source => source.UseCollation("Some collation"),
            target => target.UseCollation("Some other collation"),
            operations =>
            {
                Assert.Equal(1, operations.Count);

                var operation = Assert.IsType<AlterDatabaseOperation>(operations[0]);
                Assert.Equal("Some other collation", operation.Collation);
                Assert.Equal("Some collation", operation.OldDatabase.Collation);
            });

    public class Customer13300 : ProviderTenantEntity13300
    {
        public string DisplayName { get; set; }
    }

    public abstract class ProviderTenantEntity13300 : TenantEntity13300
    {
        public string ProviderKey { get; set; }
    }

    public abstract class TenantEntity13300
    {
        public Guid TenantId { get; set; }
        public ReferencePoint13300 Created { get; set; } = new();
    }

    public class ReferencePoint13300
    {
        public string Reason { get; set; }
    }

    [ConditionalFact]
    public void Primary_key_properties_are_sorted_first()
        => Execute(
            _ => { },
            target =>
            {
                target.Entity<Principal>();
                target.Entity<Dependent>(
                    b =>
                    {
                        b.Property<int>("ShadowPk");
                        b.Property<int>("AnotherShadowProperty");
                        b.HasKey("Id1", "Id2", "Id3", "ShadowPk");
                    });
            },
            operations =>
            {
                var dependentTableCreation
                    = (CreateTableOperation)operations.Single(o => o is CreateTableOperation { Name: "Dependent" });

                Assert.Collection(
                    dependentTableCreation.Columns,
                    c => Assert.Equal("Id3", c.Name),
                    c => Assert.Equal("Id2", c.Name),
                    c => Assert.Equal("Id1", c.Name),
                    c => Assert.Equal("ShadowPk", c.Name),
                    c => Assert.Equal("Value", c.Name),
                    c => Assert.Equal("AnotherShadowProperty", c.Name),
                    c => Assert.Equal("RealFkNavigationId", c.Name),
                    c => Assert.Equal("ShadowFkNavigationId", c.Name));
            });

    public abstract class Base
    {
        public int? RealFkNavigationId { get; set; }
        public Principal ShadowFkNavigation { get; set; }
        public Principal RealFkNavigation { get; set; }
        public int Id3 { get; set; }
    }

    public class Dependent : Base
    {
        public int Id2 { get; set; }
        public int Id1 { get; set; }
        public string Value { get; set; }
    }

    public class Principal
    {
        public int Id { get; set; }
    }

    private class Blog
    {
        private readonly Action<object, string> _loader;
        private ICollection<Post> _posts;

        public Blog()
        {
        }

        private Blog(Action<object, string> lazyLoader)
        {
            _loader = lazyLoader;
        }

        public int BlogId { get; set; }
        public string Url { get; set; }

        public ICollection<Post> Posts
        {
            get => _loader.Load(this, ref _posts);
            set => _posts = value;
        }
    }

    private class Post
    {
        private readonly ILazyLoader _loader;
        private Blog _blog;

        public Post()
        {
        }

        private Post(ILazyLoader loader)
        {
            _loader = loader;
        }

        public int PostId { get; set; }
        public string Title { get; set; }
        public int? BlogId { get; set; }

        public Blog Blog
        {
            get => _loader.Load(this, ref _blog);
            set => _blog = value;
        }
    }

    [ConditionalFact]
    public void Create_table_handles_same_name_but_different_schemas_and_identifying_relationship()
        => Execute(
            _ => { },
            modelBuilder => modelBuilder
                .Entity(
                    "Entity1",
                    x =>
                    {
                        x.ToTable("Entity");

                        x.Property<int>("Id");
                        x.Property<string>("Property1");
                    })
                .Entity(
                    "Entity2",
                    x =>
                    {
                        x.ToTable("Entity", "other");

                        x.Property<int>("Id");
                        x.Property<string>("Property2");

                        x.HasOne("Entity1", null).WithMany().HasForeignKey("Id");
                    }),
            operations =>
            {
                Assert.Equal(3, operations.Count);

                Assert.IsType<EnsureSchemaOperation>(operations[0]);

                var operation1 = Assert.IsType<CreateTableOperation>(operations[1]);
                Assert.Equal("Entity", operation1.Name);
                Assert.Null(operation1.Schema);
                Assert.Collection(
                    operation1.Columns,
                    x => Assert.Equal("Id", x.Name),
                    x => Assert.Equal("Property1", x.Name));

                var operation2 = Assert.IsType<CreateTableOperation>(operations[2]);
                Assert.Equal("Entity", operation2.Name);
                Assert.Equal("other", operation2.Schema);
                Assert.Collection(
                    operation2.Columns,
                    x => Assert.Equal("Id", x.Name),
                    x => Assert.Equal("Property2", x.Name));
            });

    [ConditionalFact]
    public void Construction_of_shadow_values_buffer_account_for_shadow_navigations_1()
        => Execute(
            modelBuilder => modelBuilder
                .Entity(
                    "User",
                    b =>
                    {
                        b.Property<int>("Id");
                        b.ToTable("Users");
                        b.HasData(new { Id = 1 });
                    })
                .Entity(
                    "BaseType",
                    b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("Discriminator");
                        b.Property<int>("UserId");
                        b.ToTable("Type");
                        b.HasDiscriminator<string>("Discriminator").HasValue("BaseType");
                    })
                .Entity(
                    "DerivedType",
                    b =>
                    {
                        b.HasBaseType("BaseType");
                        b.Property<int>("LevelId");
                        b.HasDiscriminator().HasValue("DerivedType");
                        b.HasData(
                            new
                            {
                                Id = 1,
                                UserId = 1,
                                LevelId = 1
                            });
                    })
                .Entity("BaseType")
                .HasOne("User", "User")
                .WithMany()
                .HasForeignKey("UserId"),
            modelBuilder => modelBuilder
                .Entity(
                    "User",
                    b =>
                    {
                        b.Property<int>("Id");
                        b.ToTable("Users");
                        b.HasData(new { Id = 1 });
                    })
                .Entity(
                    "BaseType",
                    b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("Discriminator");
                        b.Property<int>("UserId");
                        b.ToTable("Type");
                        b.HasDiscriminator<string>("Discriminator").HasValue("BaseType");
                    })
                .Entity(
                    "DerivedType",
                    b =>
                    {
                        b.HasBaseType("BaseType");
                        b.Property<int>("LevelId");
                        b.HasDiscriminator().HasValue("DerivedType");
                        b.HasData(
                            new
                            {
                                Id = 1,
                                UserId = 1,
                                LevelId = 1
                            });
                    })
                .Entity("BaseType")
                .HasOne("User", "User")
                .WithMany()
                .HasForeignKey("UserId"),
            ops => { });

    [ConditionalFact]
    public void Construction_of_shadow_values_buffer_account_for_shadow_navigations_2()
        => Execute(
            modelBuilder => modelBuilder
                .Entity(
                    "User",
                    b =>
                    {
                        b.Property<int>("Id");
                        b.ToTable("Users");
                        b.HasData(new { Id = 1 });
                    })
                .Entity(
                    "BaseType",
                    b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("Discriminator");
                        b.Property<int>("UserId");
                        b.ToTable("Type");
                        b.HasDiscriminator<string>("Discriminator").HasValue("BaseType");
                    })
                .Entity(
                    "DerivedType",
                    b =>
                    {
                        b.HasBaseType("BaseType");
                        b.Property<int>("Level1Id");
                        b.Property<double>("Level2Id");
                        b.HasDiscriminator().HasValue("DerivedType");
                        b.HasData(
                            new
                            {
                                Id = 1,
                                UserId = 1,
                                Level1Id = 1,
                                Level2Id = 1.0
                            });
                    })
                .Entity("BaseType")
                .HasOne("User", "User")
                .WithMany()
                .HasForeignKey("UserId"),
            modelBuilder => modelBuilder
                .Entity(
                    "User",
                    b =>
                    {
                        b.Property<int>("Id");
                        b.ToTable("Users");
                        b.HasData(new { Id = 1 });
                    })
                .Entity(
                    "BaseType",
                    b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("Discriminator");
                        b.Property<int>("UserId");
                        b.ToTable("Type");
                        b.HasDiscriminator<string>("Discriminator").HasValue("BaseType");
                    })
                .Entity(
                    "DerivedType",
                    b =>
                    {
                        b.HasBaseType("BaseType");
                        b.Property<int>("Level1Id");
                        b.Property<double>("Level2Id");
                        b.HasDiscriminator().HasValue("DerivedType");
                        b.HasData(
                            new
                            {
                                Id = 1,
                                UserId = 1,
                                Level1Id = 1,
                                Level2Id = 1.0
                            });
                    })
                .Entity("BaseType")
                .HasOne("User", "User")
                .WithMany()
                .HasForeignKey("UserId"),
            ops => { });

    private class TestKeylessType
    {
        public string Something { get; set; }
    }

    private static IQueryable<TestKeylessType> GetCountByYear(int id)
        => throw new NotImplementedException();

    [ConditionalFact]
    public void Model_differ_does_not_detect_entity_type_mapped_to_TVF()
        => Execute(
            _ => { },
            modelBuilder =>
            {
                var function = modelBuilder.HasDbFunction(
                    typeof(MigrationsModelDifferTest).GetMethod(
                        nameof(GetCountByYear),
                        BindingFlags.NonPublic | BindingFlags.Static)).Metadata;

                modelBuilder.Entity<TestKeylessType>().ToFunction(function.ModelName);
            },
            result => Assert.Equal(0, result.Count),
            skipSourceConventions: true);

    protected override TestHelpers TestHelpers
        => FakeRelationalTestHelpers.Instance;
}
