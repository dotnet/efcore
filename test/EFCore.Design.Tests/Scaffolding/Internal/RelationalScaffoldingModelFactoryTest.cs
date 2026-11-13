// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Internal;

public class RelationalScaffoldingModelFactoryTest
{
    private readonly IScaffoldingModelFactory _factory;
    private readonly TestOperationReporter _reporter;

    private static readonly DatabaseModel Database;
    private static readonly DatabaseTable Table;
    private static readonly DatabaseColumn IdColumn;
    private static readonly DatabasePrimaryKey IdPrimaryKey;

    static RelationalScaffoldingModelFactoryTest()
    {
        Database = new DatabaseModel();
        Table = new DatabaseTable { Database = Database, Name = "Foo" };
        IdColumn = new DatabaseColumn
        {
            Table = Table,
            Name = "Id",
            StoreType = "int"
        };
        IdPrimaryKey = new DatabasePrimaryKey
        {
            Table = Table,
            Name = "IdPrimaryKey",
            Columns = { IdColumn }
        };
    }

    public RelationalScaffoldingModelFactoryTest()
    {
        _reporter = new TestOperationReporter();

        var assembly = typeof(RelationalScaffoldingModelFactoryTest).Assembly;
        _factory = new DesignTimeServicesBuilder(assembly, assembly, _reporter, [])
            .CreateServiceCollection("Microsoft.EntityFrameworkCore.SqlServer")
            .AddSingleton<IScaffoldingModelFactory, FakeScaffoldingModelFactory>()
            .BuildServiceProvider(validateScopes: true)
            .GetRequiredService<IScaffoldingModelFactory>();

        _reporter.Clear();
    }

    [Fact]
    public void Capitalize_DatabaseName()
    {
        var database = new DatabaseModel { DatabaseName = "northwind" };
        var model = _factory.Create(database, new ModelReverseEngineerOptions { UseDatabaseNames = false });
        Assert.Equal("Northwind", model.GetDatabaseName());
    }

    [Fact]
    public void Creates_entity_types()
    {
        var info = new DatabaseModel
        {
            Tables =
            {
                new DatabaseTable
                {
                    Database = Database,
                    Name = "tableWithSchema",
                    Schema = "public",
                    Columns = { IdColumn },
                    PrimaryKey = IdPrimaryKey
                },
                new DatabaseTable
                {
                    Database = Database,
                    Name = "noSchema",
                    Columns = { IdColumn },
                    PrimaryKey = IdPrimaryKey
                },
                new DatabaseTable { Database = Database, Name = "noPrimaryKey" },
                new DatabaseView { Database = Database, Name = "view" }
            }
        };
        var model = _factory.Create(info, new ModelReverseEngineerOptions());
        Assert.Collection(
            model.GetEntityTypes().OrderBy(t => t.Name),
            vwtable =>
            {
                Assert.Equal("noPrimaryKey", vwtable.GetTableName());
                Assert.Empty(vwtable.GetKeys());
            },
            table =>
            {
                Assert.Equal("noSchema", table.GetTableName());
                Assert.Null(table.GetSchema());
            },
            pgtable =>
            {
                Assert.Equal("tableWithSchema", pgtable.GetTableName());
                Assert.Equal("public", pgtable.GetSchema());
            },
            view =>
            {
                Assert.Equal("view", view.GetViewName());
                Assert.Null(view.GetTableName());
                Assert.NotNull(view.FindAnnotation(RelationalAnnotationNames.ViewDefinitionSql));
            }
        );
    }

    [Fact]
    public void Creates_entity_types_case_insensitive()
    {
        var info = new DatabaseModel
        {
            Tables =
            {
                new DatabaseTable
                {
                    Database = Database,
                    Name = "TestTable",
                    Columns = { IdColumn },
                    PrimaryKey = IdPrimaryKey
                },
                new DatabaseTable
                {
                    Database = Database,
                    Name = "TESTTABLE",
                    Columns = { IdColumn },
                    PrimaryKey = IdPrimaryKey
                }
            }
        };
        var model = _factory.Create(info, new ModelReverseEngineerOptions());
        Assert.Equal(2, model.GetEntityTypes().Select(et => et.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Theory, InlineData("PascalCase"), InlineData("camelCase"), InlineData("snake-case"), InlineData("MixedCASE"),
     InlineData("separated_by_underscores"), InlineData("PascalCase_withUnderscore"), InlineData("ALL_CAPS"),
     InlineData("numbers0Dont1Affect23Upper45Case678To9LowerCase10Boundary999"), InlineData("We1!*~&%rdCh@r^act()0rs")]
    public void Get_DatabaseName(string expectedValue)
    {
        var options = new ModelReverseEngineerOptions { UseDatabaseNames = true };

        var database = new DatabaseModel { DatabaseName = expectedValue };
        var model = _factory.Create(database, options);
        Assert.Equal(expectedValue, model.GetDatabaseName());
    }

    [Fact]
    public void Loads_column_types()
    {
        var info = new DatabaseModel
        {
            Tables =
            {
                new DatabaseTable
                {
                    Database = Database,
                    Name = "Jobs",
                    Columns =
                    {
                        IdColumn,
                        new DatabaseColumn
                        {
                            Table = Table,
                            Name = "occupation",
                            StoreType = "nvarchar(max)",
                            DefaultValueSql = "\"dev\""
                        },
                        new DatabaseColumn
                        {
                            Table = Table,
                            Name = "salary",
                            StoreType = "int",
                            IsNullable = true
                        },
                        new DatabaseColumn
                        {
                            Table = Table,
                            Name = "modified",
                            StoreType = "nvarchar(max)",
                            IsNullable = false,
                            ValueGenerated = ValueGenerated.OnAddOrUpdate
                        },
                        new DatabaseColumn
                        {
                            Table = Table,
                            Name = "created",
                            StoreType = "nvarchar(max)",
                            ValueGenerated = ValueGenerated.OnAdd
                        },
                        new DatabaseColumn
                        {
                            Table = Table,
                            Name = "current",
                            StoreType = "nvarchar(max)",
                            ComputedColumnSql = "compute_this()"
                        }
                    },
                    PrimaryKey = IdPrimaryKey
                }
            }
        };

        var entityType =
            (EntityType)_factory.Create(info, new ModelReverseEngineerOptions { NoPluralize = true }).FindEntityType("Jobs")!;

        Assert.Collection(
            entityType.GetProperties(),
            pk =>
            {
                Assert.Equal("Id", pk.Name);
                Assert.Equal(typeof(int), pk.ClrType);
            },
            col1 =>
            {
                Assert.Equal("created", col1.GetColumnName());
                Assert.Equal(ValueGenerated.OnAdd, col1.ValueGenerated);
            },
            col2 =>
            {
                Assert.Equal("Current", col2.Name);
                Assert.Equal(typeof(string), col2.ClrType);
                Assert.Equal("compute_this()", col2.GetComputedColumnSql());
            },
            col3 =>
            {
                Assert.Equal("modified", col3.GetColumnName());
                Assert.Equal(ValueGenerated.OnAddOrUpdate, col3.ValueGenerated);
            },
            col4 =>
            {
                Assert.Equal("occupation", col4.GetColumnName());
                Assert.Equal(typeof(string), col4.ClrType);
                Assert.False(col4.IsColumnNullable());
                Assert.Null(col4.GetMaxLength());
                Assert.Equal("\"dev\"", col4.GetDefaultValueSql());
            },
            col5 =>
            {
                Assert.Equal("Salary", col5.Name);
                Assert.Equal(typeof(int?), col5.ClrType);
                Assert.True(col5.IsColumnNullable());
                Assert.Null(col5.GetDefaultValue());
            });
    }

    [Fact]
    public void Use_database_names_for_columns()
    {
        var info = new DatabaseModel
        {
            Tables =
            {
                new DatabaseTable
                {
                    Database = Database,
                    Name = "NaturalProducts",
                    Columns =
                    {
                        IdColumn,
                        new DatabaseColumn
                        {
                            Table = Table,
                            Name = "ProductSKU",
                            StoreType = "nvarchar(max)"
                        },
                        new DatabaseColumn
                        {
                            Table = Table,
                            Name = "supplierID",
                            StoreType = "nvarchar(max)"
                        },
                        new DatabaseColumn
                        {
                            Table = Table,
                            Name = "Vendor_Discount",
                            StoreType = "nvarchar(max)"
                        }
                    },
                    PrimaryKey = IdPrimaryKey
                }
            }
        };

        var entityType = _factory
            .Create(info, new ModelReverseEngineerOptions { UseDatabaseNames = true, NoPluralize = true })
            .FindEntityType("NaturalProducts")!;

        Assert.Collection(
            entityType.GetProperties(),
            pk => Assert.Equal("Id", pk.Name),
            col1 => Assert.Equal("ProductSKU", col1.Name),
            col2 => Assert.Equal("Vendor_Discount", col2.Name),
            col3 => Assert.Equal("supplierID", col3.Name));
    }

    [Fact]
    public void Do_not_use_database_names_for_columns()
    {
        var info = new DatabaseModel
        {
            Tables =
            {
                new DatabaseTable
                {
                    Database = Database,
                    Name = "NaturalProducts",
                    Columns =
                    {
                        IdColumn,
                        new DatabaseColumn
                        {
                            Table = Table,
                            Name = "ProductSKU",
                            StoreType = "nvarchar(max)"
                        },
                        new DatabaseColumn
                        {
                            Table = Table,
                            Name = "supplierID",
                            StoreType = "nvarchar(max)"
                        },
                        new DatabaseColumn
                        {
                            Table = Table,
                            Name = "Vendor_Discount",
                            StoreType = "nvarchar(max)"
                        }
                    },
                    PrimaryKey = IdPrimaryKey
                }
            }
        };

        var entityType = _factory.Create(info, new ModelReverseEngineerOptions { NoPluralize = true })
            .FindEntityType("NaturalProducts")!;

        Assert.Collection(
            entityType.GetProperties(),
            pk => Assert.Equal("Id", pk.Name),
            col1 => Assert.Equal("ProductSku", col1.Name),
            col2 => Assert.Equal("SupplierId", col2.Name),
            col3 => Assert.Equal("VendorDiscount", col3.Name));
    }

    [Theory, InlineData("nvarchar(450)", null), InlineData("datetime2(4)", null), InlineData("DateTime2(4)", "DateTime2(4)")]
    public void Column_type_annotation(string storeType, string? expectedColumnType)
    {
        var column = new DatabaseColumn
        {
            Table = Table,
            Name = "Col",
            StoreType = storeType
        };

        var info = new DatabaseModel
        {
            Tables =
            {
                new DatabaseTable
                {
                    Database = Database,
                    Name = "A",
                    Columns = { column },
                    PrimaryKey = new DatabasePrimaryKey
                    {
                        Table = Table,
                        Name = "PK_Foo",
                        Columns = { column }
                    }
                }
            }
        };

        var property = (Property)_factory.Create(info, new ModelReverseEngineerOptions()).FindEntityType("A")!.FindProperty("Col")!;

        Assert.Equal(expectedColumnType, property.GetConfiguredColumnType());
    }

    [Fact]
    public void Column_ordinal_annotation()
    {
        var col1 = new DatabaseColumn
        {
            Table = Table,
            Name = "Col1",
            StoreType = "nvarchar(max)"
        };
        var info = new DatabaseModel
        {
            Tables =
            {
                new DatabaseTable
                {
                    Database = Database,
                    Name = "A",
                    Columns =
                    {
                        col1,
                        new DatabaseColumn
                        {
                            Table = Table,
                            Name = "Col2",
                            StoreType = "nvarchar(max)"
                        },
                        new DatabaseColumn
                        {
                            Table = Table,
                            Name = "Col3",
                            StoreType = "nvarchar(max)"
                        }
                    },
                    PrimaryKey = new DatabasePrimaryKey
                    {
                        Table = Table,
                        Name = "PK_Foo",
                        Columns = { col1 }
                    }
                }
            }
        };

        var entityTypeA = _factory.Create(info, new ModelReverseEngineerOptions()).FindEntityType("A")!;
        var property1 = (Property)entityTypeA.FindProperty("Col1")!;
        var property2 = (Property)entityTypeA.FindProperty("Col2")!;
        var property3 = (Property)entityTypeA.FindProperty("Col3")!;

        Assert.Equal(0, property1.GetColumnOrder());
        Assert.Equal(1, property2.GetColumnOrder());
        Assert.Equal(2, property3.GetColumnOrder());
    }

    [Theory, InlineData("cheese"), InlineData(null)]
    public void Unmappable_column_type(string? StoreType)
    {
        var info = new DatabaseModel
        {
            Tables =
            {
                new DatabaseTable
                {
                    Database = Database,
                    Name = "E",
                    Columns = { IdColumn },
                    PrimaryKey = IdPrimaryKey
                }
            }
        };

        info.Tables.First().Columns.Add(
            new DatabaseColumn
            {
                Table = info.Tables.First(),
                Name = "Coli",
                StoreType = StoreType
            });

        Assert.Single(_factory.Create(info, new ModelReverseEngineerOptions()).FindEntityType("E")!.GetProperties());

        var (level, message) = _reporter.Messages.Single();
        Assert.Equal(LogLevel.Warning, level);
        Assert.Equal(DesignStrings.CannotFindTypeMappingForColumn("E.Coli", StoreType), message);
    }

    [Theory, InlineData(new[] { "Id" }, 1), InlineData(new[] { "Id", "AltId" }, 2)]
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
    public void Primary_key(string[] keyProps, int length)
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
    {
        var info = new DatabaseModel
        {
            Tables =
            {
                new DatabaseTable
                {
                    Database = Database,
                    Name = "PkTable",
                    PrimaryKey = new DatabasePrimaryKey { Table = Table, Name = "MyPk" }
                }
            }
        };
        foreach (var column in keyProps.Select(k => new DatabaseColumn
        {
            Table = Table,
            Name = k,
            StoreType = "int"
        }))
        {
            info.Tables[0].Columns.Add(column);
            info.Tables[0].PrimaryKey!.Columns.Add(column);
        }

        var model = (EntityType)_factory.Create(info, new ModelReverseEngineerOptions()).GetEntityTypes().Single();

        Assert.Equal("MyPk", model.FindPrimaryKey()!.GetName());
        Assert.Equal(keyProps, model.FindPrimaryKey()!.Properties.Select(p => p.GetColumnName()).ToArray());
    }

    [Fact]
    public void Unique_constraint()
    {
        var myColumn = new DatabaseColumn
        {
            Table = Table,
            Name = "MyColumn",
            StoreType = "int"
        };

        var databaseModel = new DatabaseModel
        {
            Tables =
            {
                new DatabaseTable
                {
                    Database = Database,
                    Name = "MyTable",
                    Columns = { IdColumn, myColumn },
                    PrimaryKey = IdPrimaryKey,
                    UniqueConstraints =
                    {
                        new DatabaseUniqueConstraint
                        {
                            Table = Table,
                            Name = "MyUniqueConstraint",
                            Columns = { myColumn }
                        }
                    }
                }
            }
        };

        var entityType = (EntityType)_factory.Create(databaseModel, new ModelReverseEngineerOptions()).GetEntityTypes().Single();
        var index = entityType.GetIndexes().Single();

        Assert.True(index.IsUnique);
        Assert.Equal("MyUniqueConstraint", index.GetDatabaseName());
        Assert.Same(entityType.FindProperty("MyColumn"), index.Properties.Single());
    }

    [Fact]
    public void Unique_constraint_without_name()
    {
        var myColumn = new DatabaseColumn
        {
            Table = Table,
            Name = "MyColumn",
            StoreType = "int"
        };

        var databaseModel = new DatabaseModel
        {
            Tables =
            {
                new DatabaseTable
                {
                    Database = Database,
                    Name = "MyTable",
                    Columns = { IdColumn, myColumn },
                    PrimaryKey = IdPrimaryKey,
                    UniqueConstraints = { new DatabaseUniqueConstraint { Table = Table, Columns = { myColumn } } }
                }
            }
        };

        var entityType = (EntityType)_factory.Create(databaseModel, new ModelReverseEngineerOptions()).GetEntityTypes().Single();
        var index = entityType.GetIndexes().Single();

        Assert.True(index.IsUnique);
        Assert.Equal("IX_MyTable_MyColumn", index.GetDatabaseName());
        Assert.Same(entityType.FindProperty("MyColumn"), index.Properties.Single());
    }

    [Fact]
    public void Unique_constraint_with_empty_string_name()
    {
        var myColumn = new DatabaseColumn
        {
            Table = Table,
            Name = "MyColumn",
            StoreType = "int"
        };

        var databaseModel = new DatabaseModel
        {
            Tables =
            {
                new DatabaseTable
                {
                    Database = Database,
                    Name = "MyTable",
                    Columns = { IdColumn, myColumn },
                    PrimaryKey = IdPrimaryKey,
                    UniqueConstraints =
                    {
                        new DatabaseUniqueConstraint
                        {
                            Table = Table,
                            Name = "",
                            Columns = { myColumn }
                        }
                    }
                }
            }
        };

        var entityType = (EntityType)_factory.Create(databaseModel, new ModelReverseEngineerOptions()).GetEntityTypes().Single();
        var index = entityType.GetIndexes().Single();

        Assert.True(index.IsUnique);
        Assert.Equal("IX_MyTable_MyColumn", index.GetDatabaseName());
        Assert.Same(entityType.FindProperty("MyColumn"), index.Properties.Single());
    }

    [Fact]
    public void Indexes_and_alternate_keys()
    {
        var c1 = new DatabaseColumn
        {
            Table = Table,
            Name = "C1",
            StoreType = "int"
        };
        var table = new DatabaseTable
        {
            Database = Database,
            Name = "T",
            Columns =
            {
                c1,
                new DatabaseColumn
                {
                    Table = Table,
                    Name = "C2",
                    StoreType = "int"
                },
                new DatabaseColumn
                {
                    Table = Table,
                    Name = "C3",
                    StoreType = "int"
                }
            },
            PrimaryKey = new DatabasePrimaryKey
            {
                Table = Table,
                Name = "PK_Foo",
                Columns = { c1 }
            }
        };
        table.Indexes.Add(
            new DatabaseIndex
            {
                Table = Table,
                Name = "IDX_C1",
                Columns = { table.Columns.ElementAt(0) },
                IsUnique = false
            });
        table.Indexes.Add(
            new DatabaseIndex
            {
                Table = Table,
                Name = "IDX_C2",
                Columns = { table.Columns.ElementAt(1) },
                IsUnique = true
            });
        table.Indexes.Add(
            new DatabaseIndex
            {
                Table = Table,
                Name = "",
                Columns = { table.Columns.ElementAt(2) },
                IsUnique = true
            });
        table.Indexes.Add(
            new DatabaseIndex
            {
                Table = Table,
                Name = "IDX_C2_C1",
                Columns = { table.Columns.ElementAt(1), table.Columns.ElementAt(0) },
                IsUnique = false
            });
        table.Indexes.Add(
            new DatabaseIndex
            {
                Table = Table,
                Columns = { table.Columns.ElementAt(1), table.Columns.ElementAt(2) },
                IsUnique = false
            });
        table.Indexes.Add(
            new DatabaseIndex
            {
                Table = Table,
                Name = "UNQ_C3_C1",
                Columns = { table.Columns.ElementAt(2), table.Columns.ElementAt(0) },
                IsUnique = true
            });

        var info = new DatabaseModel { Tables = { table } };

        var entityType = _factory.Create(info, new ModelReverseEngineerOptions()).GetEntityTypes().Single();

        Assert.Collection(
            entityType.GetIndexes(),
            t =>
            {
                Assert.True(t.IsUnique);
                Assert.Null(t.Name);
                Assert.Equal("IX_T_C3", t.GetDatabaseName());
                Assert.Same(entityType.FindProperty("C3"), t.Properties.Single());
            },
            t =>
            {
                Assert.False(t.IsUnique);
                Assert.Null(t.Name);
                Assert.Equal("IX_T_C2_C3", t.GetDatabaseName());
                Assert.Equal(new[] { "C2", "C3" }, t.Properties.Select(c => c.Name).ToArray());
            },
            t =>
            {
                Assert.False(t.IsUnique);
                Assert.Equal("IDX_C1", t.Name);
                Assert.Equal("IDX_C1", t.GetDatabaseName());
                Assert.Same(entityType.FindProperty("C1"), t.Properties.Single());
            },
            t =>
            {
                Assert.True(t.IsUnique);
                Assert.Equal("IDX_C2", t.Name);
                Assert.Equal("IDX_C2", t.GetDatabaseName());
                Assert.Same(entityType.FindProperty("C2"), t.Properties.Single());
            },
            t =>
            {
                Assert.False(t.IsUnique);
                Assert.Equal("IDX_C2_C1", t.Name);
                Assert.Equal("IDX_C2_C1", t.GetDatabaseName());
                Assert.Equal(new[] { "C2", "C1" }, t.Properties.Select(c => c.Name).ToArray());
            },
            t =>
            {
                Assert.True(t.IsUnique);
                Assert.Equal("UNQ_C3_C1", t.Name);
                Assert.Equal("UNQ_C3_C1", t.GetDatabaseName());
                Assert.Equal(new[] { "C3", "C1" }, t.Properties.Select(c => c.Name).ToArray());
            }
        );

        // unique indexes should not cause alternate keys if not used by foreign keys
        Assert.Equal(0, entityType.GetKeys().Count(k => !k.IsPrimaryKey()));
    }

    [Fact]
    public void Foreign_key()
    {
        var parentTable = new DatabaseTable
        {
            Database = Database,
            Name = "Parent",
            Columns = { IdColumn },
            PrimaryKey = IdPrimaryKey
        };
        var childrenTable = new DatabaseTable
        {
            Database = Database,
            Name = "Children",
            Columns =
            {
                IdColumn,
                new DatabaseColumn
                {
                    Table = Table,
                    Name = "ParentId",
                    StoreType = "int",
                    IsNullable = true
                }
            },
            PrimaryKey = IdPrimaryKey
        };
        childrenTable.ForeignKeys.Add(
            new DatabaseForeignKey
            {
                Table = childrenTable,
                Name = "FK_Foo",
                Columns = { childrenTable.Columns.ElementAt(1) },
                PrincipalTable = parentTable,
                PrincipalColumns = { parentTable.Columns.ElementAt(0) },
                OnDelete = ReferentialAction.Cascade
            });

        var model = _factory.Create(
            new DatabaseModel { Tables = { parentTable, childrenTable } },
            new ModelReverseEngineerOptions { NoPluralize = true });

        var parent = (EntityType)model.FindEntityType("Parent")!;

        var children = (EntityType)model.FindEntityType("Children")!;

        Assert.NotEmpty(parent.GetReferencingForeignKeys());
        var fk = Assert.Single(children.GetForeignKeys());
        Assert.False(fk.IsUnique);
        Assert.Equal(DeleteBehavior.Cascade, fk.DeleteBehavior);

        var principalKey = fk.PrincipalKey;

        Assert.Same(parent, principalKey.DeclaringEntityType);
        Assert.Same(parent.GetProperties().First(), principalKey.Properties[0]);
    }

    [Theory]
    [InlineData("tinyint", "smallint", typeof(byte))]
    [InlineData("tinyint", "int", typeof(byte))]
    [InlineData("tinyint", "bigint", typeof(byte))]
    [InlineData("smallint", "int", typeof(short))]
    [InlineData("smallint", "bigint", typeof(short))]
    [InlineData("int", "bigint", typeof(int))]
    [InlineData("int", "decimal(10,0)", typeof(int))]
    [InlineData("bigint", "decimal(18,0)", typeof(long))]
    [InlineData("tinyint", "decimal(3,0)", typeof(byte))]
    [InlineData("int", "bigint", typeof(int?), true)]
    public void Foreign_key_with_wider_dependent_type_uses_principal_clr_type(
        string principalStoreType,
        string dependentStoreType,
        Type expectedClrType,
        bool dependentNullable = false)
    {
        var (parent, children, _) = CreateForeignKey(principalStoreType, dependentStoreType, dependentNullable);

        var model = Scaffold(parent, children);

        Assert.Empty(_reporter.Messages);

        var fk = Assert.Single(model.FindEntityType("Children")!.GetForeignKeys());
        Assert.Equal("Parent", fk.PrincipalEntityType.Name);
        AssertProperty(Assert.Single(fk.Properties), expectedClrType, dependentStoreType);
        AssertProperty(Assert.Single(fk.PrincipalKey.Properties), expectedClrType.UnwrapNullableType(), principalStoreType);
    }

    [Theory]
    [InlineData("bigint", "smallint", "short", "long")]
    [InlineData("bigint", "tinyint", "byte", "long")]
    [InlineData("int", "smallint", "short", "int")]
    [InlineData("uniqueidentifier", "varchar(36)", "string", "Guid")]
    [InlineData("bit", "int", "int", "bool")]
    [InlineData("float", "bigint", "long", "double")]
    [InlineData("decimal(18, 2)", "bigint", "long", "decimal")]
    [InlineData("decimal(19, 0)", "bigint", "long", "decimal")]
    [InlineData("float", "decimal(18,2)", "decimal", "double")]
    [InlineData("nvarchar(10)", "decimal(10,0)", "decimal", "string")]
    [InlineData("int", "float", "double", "int")]
    [InlineData("decimal(10, 0)", "int", "int", "decimal")]
    [InlineData("decimal(2, 0)", "tinyint", "byte", "decimal")]
    [InlineData("decimal(5, 0)", "smallint", "short", "decimal")]
    public void Foreign_key_with_unsafe_mismatched_types_is_skipped(
        string principalStoreType,
        string dependentStoreType,
        string dependentClrType,
        string principalClrType)
    {
        var (parent, children, foreignKey) = CreateForeignKey(principalStoreType, dependentStoreType);

        var model = Scaffold(parent, children);

        AssertTypeMismatchWarning(foreignKey, $"{{'ParentId' : {dependentClrType}}}", $"{{'Id' : {principalClrType}}}");

        var property = model.FindEntityType("Children")!.FindProperty("ParentId")!;
        Assert.Empty(property.GetContainingForeignKeys());
        Assert.Equal(dependentClrType, property.ClrType.ShortDisplayName());
        Assert.Equal(dependentStoreType, property.GetColumnType());
    }

    [Theory]
    [InlineData("decimal(18, 0)", "bigint")]
    [InlineData("decimal(18)", "bigint")]
    [InlineData("numeric(18,0)", "bigint")]
    [InlineData("decimal", "bigint")] // The default is decimal(18, 0) on SQL Server
    [InlineData("decimal(9, 0)", "int")]
    [InlineData("decimal(4, 0)", "smallint")]
    [InlineData("decimal(2, 0)", "int")]
    public void Foreign_key_to_decimal_principal_with_integer_values_that_fit_uses_principal_clr_type(
        string principalStoreType,
        string dependentStoreType)
    {
        var (parent, children, _) = CreateForeignKey(principalStoreType, dependentStoreType);

        var model = Scaffold(parent, children);

        Assert.Empty(_reporter.Messages);

        var fk = Assert.Single(model.FindEntityType("Children")!.GetForeignKeys());
        AssertProperty(Assert.Single(fk.Properties), typeof(decimal), dependentStoreType);
    }

    [Fact]
    public void Foreign_key_with_mismatched_types_and_default_value_is_skipped()
    {
        var (parent, children, foreignKey) = CreateForeignKey("int", "bigint");
        children.Columns[1].DefaultValue = 0L;

        Scaffold(parent, children);

        AssertTypeMismatchWarning(foreignKey, "{'ParentId' : long}", "{'Id' : int}");
    }

    [Fact]
    public void Foreign_key_with_mismatched_types_from_row_version_column_is_skipped()
    {
        var (parent, children, foreignKey) = CreateForeignKey("int", "bigint");
        children.Columns[1].ValueGenerated = ValueGenerated.OnAddOrUpdate;
        children.Columns[1][ScaffoldingAnnotationNames.ConcurrencyToken] = true;

        Scaffold(parent, children);

        AssertTypeMismatchWarning(foreignKey, "{'ParentId' : long}", "{'Id' : int}");
    }

    [Fact]
    public void Foreign_key_from_column_without_store_type_is_skipped()
    {
        var (parent, children, foreignKey) = CreateForeignKey("int", null);

        var model = Scaffold(parent, children);

        AssertForeignKeyWarning(DesignStrings.ForeignKeyScaffoldErrorPropertyNotFound(foreignKey.DisplayName(), "ParentId"));

        var entityType = model.FindEntityType("Children")!;
        Assert.Empty(entityType.GetForeignKeys());
        Assert.Null(entityType.FindProperty("ParentId"));
    }

    [Fact]
    public void Foreign_key_to_column_without_store_type_is_skipped()
    {
        var (parent, children, foreignKey) = CreateForeignKey(null, "bigint");

        var model = Scaffold(parent, children);

        AssertForeignKeyWarning(DesignStrings.ForeignKeyScaffoldErrorPrincipalTableScaffoldingError(foreignKey.DisplayName(), parent.DisplayName()));

        var entityType = model.FindEntityType("Children")!;
        Assert.Empty(entityType.GetForeignKeys());
        AssertProperty(entityType.FindProperty("ParentId")!, typeof(long), "bigint");
    }

    [Fact]
    public void Self_referencing_foreign_key_with_mismatched_types_uses_principal_clr_type()
    {
        var table = CreateTable("Nodes", [Column("Id", "int"), Column("ParentId", "bigint", nullable: true)]);
        AddForeignKey(table, table, [table.Columns[1]], [table.Columns[0]]);

        var model = Scaffold(table);

        Assert.Empty(_reporter.Messages);

        var fk = Assert.Single(model.FindEntityType("Nodes")!.GetForeignKeys());
        AssertProperty(Assert.Single(fk.Properties), typeof(int?), "bigint");
    }

    [Fact]
    public void Foreign_key_from_primary_key_column_with_mismatched_types_uses_principal_clr_type()
    {
        var parent = CreateTable("Parent", [Column("Id", "int")]);
        var children = CreateTable("Children", [Column("ParentId", "bigint")]);
        AddForeignKey(children, parent, [children.Columns[0]], [parent.Columns[0]]);

        var model = Scaffold(parent, children);

        Assert.Empty(_reporter.Messages);

        var entityType = model.FindEntityType("Children")!;
        var fk = Assert.Single(entityType.GetForeignKeys());
        AssertProperty(Assert.Single(fk.Properties), typeof(int), "bigint");
        Assert.Equal(typeof(int), Assert.Single(entityType.FindPrimaryKey()!.Properties).ClrType);
        Assert.True(fk.IsUnique);
    }

    [Fact]
    public void Foreign_key_from_unique_index_column_with_mismatched_types_uses_principal_clr_type()
    {
        var (parent, children, _) = CreateForeignKey("int", "bigint");
        children.Indexes.Add(
            new DatabaseIndex
            {
                Table = children,
                Name = "IX_ParentId",
                Columns = { children.Columns[1] },
                IsUnique = true
            });

        var model = Scaffold(parent, children);

        Assert.Empty(_reporter.Messages);

        var entityType = model.FindEntityType("Children")!;
        var fk = Assert.Single(entityType.GetForeignKeys());
        AssertProperty(Assert.Single(fk.Properties), typeof(int), "bigint");
        Assert.Equal(typeof(int), Assert.Single(Assert.Single(entityType.GetIndexes()).Properties).ClrType);
        Assert.True(fk.IsUnique);
    }

    [Theory, InlineData(false), InlineData(true)]
    public void Foreign_key_to_unique_principal_columns_with_mismatched_types_uses_principal_clr_type(bool uniqueConstraint)
    {
        var parent = CreateTable("Parent", [Column("Id", "int"), Column("Code", "int")]);
        if (uniqueConstraint)
        {
            parent.UniqueConstraints.Add(
                new DatabaseUniqueConstraint
                {
                    Table = parent,
                    Name = "UQ_Code",
                    Columns = { parent.Columns[1] }
                });
        }
        else
        {
            parent.Indexes.Add(
                new DatabaseIndex
                {
                    Table = parent,
                    Name = "IX_Code",
                    Columns = { parent.Columns[1] },
                    IsUnique = true
                });
        }

        var children = CreateTable("Children", [Column("Id", "int"), Column("ParentCode", "bigint")]);
        AddForeignKey(children, parent, [children.Columns[1]], [parent.Columns[1]]);

        var model = Scaffold(parent, children);

        Assert.Empty(_reporter.Messages);

        var fk = Assert.Single(model.FindEntityType("Children")!.GetForeignKeys());
        AssertProperty(Assert.Single(fk.Properties), typeof(int), "bigint");
    }

    [Theory, InlineData(false), InlineData(true)]
    public void Foreign_key_to_non_key_principal_columns_does_not_change_dependent_type(bool nonUniqueIndex)
    {
        var parent = CreateTable("Parent", [Column("Id", "int"), Column("NotPkId", "int")]);
        if (nonUniqueIndex)
        {
            parent.Indexes.Add(
                new DatabaseIndex
                {
                    Table = parent,
                    Name = "IX_NotPkId",
                    Columns = { parent.Columns[1] }
                });
        }

        var children = CreateTable("Children", [Column("Id", "int"), Column("ParentId", "bigint")]);
        var foreignKey = AddForeignKey(children, parent, [children.Columns[1]], [parent.Columns[1]]);

        var model = Scaffold(parent, children);

        var (level, message) = Assert.Single(_reporter.Messages);
        Assert.Equal(LogLevel.Warning, level);
        Assert.Equal(
            DesignStrings.ForeignKeyScaffoldErrorPrincipalKeyNotFound(foreignKey.DisplayName(), "NotPkId", "Parent"),
            message);

        var entityType = model.FindEntityType("Children")!;
        Assert.Empty(entityType.GetForeignKeys());
        AssertProperty(entityType.FindProperty("ParentId")!, typeof(long), "bigint");
    }

    [Fact]
    public void Foreign_key_to_nullable_unique_index_with_mismatched_types_uses_principal_clr_type()
    {
        var parent = CreateTable("Parent", [Column("Id", "int"), Column("Code", "int", nullable: true)]);
        parent.Indexes.Add(
            new DatabaseIndex
            {
                Table = parent,
                Name = "IX_Code",
                Columns = { parent.Columns[1] },
                IsUnique = true
            });
        var children = CreateTable("Children", [Column("Id", "int"), Column("ParentCode", "bigint")]);
        var foreignKey = AddForeignKey(children, parent, [children.Columns[1]], [parent.Columns[1]]);

        var model = Scaffold(parent, children);

        var (level, message) = Assert.Single(_reporter.Messages);
        Assert.Equal(LogLevel.Warning, level);
        Assert.Equal(
            DesignStrings.ForeignKeyPrincipalEndContainsNullableColumns(foreignKey.DisplayName(), "IX_Code", "Parent.Code"),
            message);

        var fk = Assert.Single(model.FindEntityType("Children")!.GetForeignKeys());
        AssertProperty(Assert.Single(fk.Properties), typeof(int), "bigint");
        Assert.False(fk.PrincipalKey.Properties[0].IsNullable);
        Assert.Equal(2, model.FindEntityType("Parent")!.GetKeys().Count());
    }

    [Fact]
    public void Foreign_key_with_incompatible_types_to_nullable_unique_index_leaves_principal_unchanged()
    {
        var parent = CreateTable("Parent", [Column("Id", "int"), Column("Code", "nvarchar(10)", nullable: true)]);
        parent.Indexes.Add(
            new DatabaseIndex
            {
                Table = parent,
                Name = "IX_Code",
                Columns = { parent.Columns[1] },
                IsUnique = true
            });
        var children = CreateTable("Children", [Column("Id", "int"), Column("ParentCode", "int")]);
        var foreignKey = AddForeignKey(children, parent, [children.Columns[1]], [parent.Columns[1]]);

        var model = Scaffold(parent, children);

        // Only the type mismatch is reported, since the principal is not changed for a foreign key that is skipped.
        AssertTypeMismatchWarning(foreignKey, "{'ParentCode' : int}", "{'Code' : string}");

        Assert.Empty(model.FindEntityType("Children")!.GetForeignKeys());

        var parentType = model.FindEntityType("Parent")!;
        Assert.True(parentType.FindProperty("Code")!.IsNullable);
        Assert.Single(parentType.GetKeys());
    }

    [Fact]
    public void Foreign_keys_from_same_column_to_principals_with_same_clr_type_use_principal_clr_type()
    {
        var parent = CreateTable("Parent", [Column("Id", "int")]);
        var otherParent = CreateTable("OtherParent", [Column("Id", "int")]);
        var children = CreateTable("Children", [Column("Id", "int"), Column("ParentId", "bigint")]);
        AddForeignKey(children, parent, [children.Columns[1]], [parent.Columns[0]], "FK_Parent");
        AddForeignKey(children, otherParent, [children.Columns[1]], [otherParent.Columns[0]], "FK_OtherParent");

        var model = Scaffold(parent, otherParent, children);

        Assert.Empty(_reporter.Messages);

        var entityType = model.FindEntityType("Children")!;
        Assert.Equal(2, entityType.GetForeignKeys().Count());
        AssertProperty(entityType.FindProperty("ParentId")!, typeof(int), "bigint");
    }

    [Fact]
    public void Duplicate_foreign_keys_with_mismatched_types_use_principal_clr_type_and_report_the_duplicate()
    {
        var (parent, children, _) = CreateForeignKey("int", "bigint");
        var duplicate = AddForeignKey(children, parent, [children.Columns[1]], [parent.Columns[0]], "FK_Duplicate");

        var model = Scaffold(parent, children);

        var (level, message) = Assert.Single(_reporter.Messages);
        Assert.Equal(LogLevel.Warning, level);
        Assert.Equal(DesignStrings.ForeignKeyWithSameFacetsExists(duplicate.DisplayName(), "FK_Foo"), message);

        var fk = Assert.Single(model.FindEntityType("Children")!.GetForeignKeys());
        AssertProperty(Assert.Single(fk.Properties), typeof(int), "bigint");
    }

    [Fact]
    public void Duplicate_foreign_keys_with_unsafe_mismatched_types_are_both_skipped()
    {
        var (parent, children, foreignKey) = CreateForeignKey("bigint", "smallint");
        var duplicate = AddForeignKey(children, parent, [children.Columns[1]], [parent.Columns[0]], "FK_Duplicate");

        Scaffold(parent, children);

        Assert.Equal(2, _reporter.Messages.Count);
        Assert.Equal(
            [
                DesignStrings.ForeignKeyScaffoldErrorTypeMismatch(foreignKey.DisplayName(), "{'ParentId' : short}", "{'Id' : long}"),
                DesignStrings.ForeignKeyScaffoldErrorTypeMismatch(duplicate.DisplayName(), "{'ParentId' : short}", "{'Id' : long}")
            ],
            _reporter.Messages.Select(m => m.Message));
    }

    [Fact]
    public void Foreign_key_with_mismatched_types_to_principals_with_different_types_is_skipped()
    {
        var parent = CreateTable("Parent", [Column("Id", "int")]);
        var otherParent = CreateTable("OtherParent", [Column("Id", "bigint")]);
        var children = CreateTable("Children", [Column("Id", "int"), Column("ParentId", "bigint")]);
        var mismatchedForeignKey = AddForeignKey(children, parent, [children.Columns[1]], [parent.Columns[0]], "FK_Parent");
        AddForeignKey(children, otherParent, [children.Columns[1]], [otherParent.Columns[0]], "FK_OtherParent");

        var model = Scaffold(parent, otherParent, children);

        AssertTypeMismatchWarning(mismatchedForeignKey, "{'ParentId' : long}", "{'Id' : int}");

        var fk = Assert.Single(model.FindEntityType("Children")!.GetForeignKeys());
        Assert.Equal("OtherParent", fk.PrincipalEntityType.Name);
        AssertProperty(Assert.Single(fk.Properties), typeof(long), "bigint");
    }

    [Fact]
    public void Foreign_key_with_mismatched_types_from_column_referenced_by_another_foreign_key_is_skipped()
    {
        var parent = CreateTable("Parent", [Column("Id", "int")]);
        var children = CreateTable("Children", [Column("ParentId", "bigint")]);
        var mismatchedForeignKey = AddForeignKey(children, parent, [children.Columns[0]], [parent.Columns[0]]);
        var grandchildren = CreateTable("Grandchildren", [Column("Id", "int"), Column("ChildId", "bigint")]);
        AddForeignKey(grandchildren, children, [grandchildren.Columns[1]], [children.Columns[0]], "FK_Child");

        var model = Scaffold(parent, children, grandchildren);

        // The Children key column is referenced by Grandchildren, so it keeps its own type and the Parent foreign key is skipped.
        AssertTypeMismatchWarning(mismatchedForeignKey, "{'ParentId' : long}", "{'Id' : int}");

        Assert.Empty(model.FindEntityType("Children")!.GetForeignKeys());

        var fk = Assert.Single(model.FindEntityType("Grandchildren")!.GetForeignKeys());
        AssertProperty(Assert.Single(fk.Properties), typeof(long), "bigint");
    }

    [Fact]
    public void Foreign_keys_sharing_columns_are_all_skipped_when_one_cannot_change_the_shared_column()
    {
        var parent = CreateCompositeParent("Parent");
        var thirdParent = CreateCompositeParent("ThirdParent");
        var otherParent = CreateTable("OtherParent", [Column("Id", "bigint")]);
        var children = CreateTable(
            "Children", [Column("Id", "int"), Column("A", "bigint"), Column("B", "bigint"), Column("C", "bigint")]);

        // A references principals with different CLR types, so it can't be changed. That makes the foreign key on (A, B)
        // unresolvable, so B can't be changed either, and then neither can C because the foreign key on (B, C) is
        // unresolvable. The foreign key on (B, C) comes first, so it is only found to be unresolvable on a later pass.
        var thirdForeignKey = AddForeignKey(
            children, thirdParent, [children.Columns[2], children.Columns[3]], [.. thirdParent.Columns], "FK_Third");
        var compositeForeignKey = AddForeignKey(
            children, parent, [children.Columns[1], children.Columns[2]], [.. parent.Columns], "FK_Composite");
        var otherForeignKey = AddForeignKey(children, otherParent, [children.Columns[1]], [otherParent.Columns[0]], "FK_Other");
        otherParent.Columns[0].StoreType = "decimal(18, 2)";

        var model = Scaffold(parent, otherParent, thirdParent, children);

        Assert.All(_reporter.Messages, m => Assert.Equal(LogLevel.Warning, m.Level));
        Assert.Equal(
            [
                DesignStrings.ForeignKeyScaffoldErrorTypeMismatch(
                    thirdForeignKey.DisplayName(), "{'B' : long, 'C' : long}", "{'Id1' : int, 'Id2' : int}"),
                DesignStrings.ForeignKeyScaffoldErrorTypeMismatch(
                    compositeForeignKey.DisplayName(), "{'A' : long, 'B' : long}", "{'Id1' : int, 'Id2' : int}"),
                DesignStrings.ForeignKeyScaffoldErrorTypeMismatch(otherForeignKey.DisplayName(), "{'A' : long}", "{'Id' : decimal}")
            ],
            _reporter.Messages.Select(m => m.Message));

        var entityType = model.FindEntityType("Children")!;
        Assert.Empty(entityType.GetForeignKeys());
        AssertProperty(entityType.FindProperty("A")!, typeof(long), "bigint");
        AssertProperty(entityType.FindProperty("B")!, typeof(long), "bigint");
        AssertProperty(entityType.FindProperty("C")!, typeof(long), "bigint");
    }

    [Fact]
    public void Composite_foreign_key_with_some_mismatched_types_uses_principal_clr_types()
    {
        var parent = CreateCompositeParent("Parent");
        var children = CreateTable("Children", [Column("Id", "int"), Column("ParentId1", "bigint"), Column("ParentId2", "int")]);
        AddForeignKey(children, parent, [children.Columns[1], children.Columns[2]], [.. parent.Columns]);

        var model = Scaffold(parent, children);

        Assert.Empty(_reporter.Messages);

        var fk = Assert.Single(model.FindEntityType("Children")!.GetForeignKeys());
        AssertProperty(fk.Properties[0], typeof(int), "bigint");
        AssertProperty(fk.Properties[1], typeof(int), "int");
    }

    [Fact]
    public void Composite_foreign_key_with_unsafe_mismatched_column_is_skipped_without_changing_other_columns()
    {
        var parent = CreateCompositeParent("Parent");
        var children = CreateTable(
            "Children", [Column("Id", "int"), Column("ParentId1", "bigint"), Column("ParentId2", "uniqueidentifier")]);
        var foreignKey = AddForeignKey(children, parent, [children.Columns[1], children.Columns[2]], [.. parent.Columns]);

        var model = Scaffold(parent, children);

        AssertTypeMismatchWarning(foreignKey, "{'ParentId1' : long, 'ParentId2' : Guid}", "{'Id1' : int, 'Id2' : int}");

        var entityType = model.FindEntityType("Children")!;
        Assert.Empty(entityType.GetForeignKeys());
        AssertProperty(entityType.FindProperty("ParentId1")!, typeof(long), "bigint");
    }

    [Theory, InlineData(true, true), InlineData(true, false), InlineData(false, true)]
    public void Composite_foreign_key_with_nullable_columns_is_not_narrowed(bool firstNullable, bool secondNullable)
    {
        // A composite foreign key with a null component doesn't constrain its other components, so they can hold
        // values that don't exist in the principal column and the type can't be narrowed to the principal type.
        var parent = CreateCompositeParent("Parent");
        var children = CreateTable(
            "Children",
            [Column("Id", "int"), Column("ParentId1", "bigint", firstNullable), Column("ParentId2", "bigint", secondNullable)]);
        var foreignKey = AddForeignKey(children, parent, [children.Columns[1], children.Columns[2]], [.. parent.Columns]);

        var model = Scaffold(parent, children);

        AssertTypeMismatchWarning(
            foreignKey,
            $"{{'ParentId1' : long{(firstNullable ? "?" : "")}, 'ParentId2' : long{(secondNullable ? "?" : "")}}}",
            "{'Id1' : int, 'Id2' : int}");

        Assert.Empty(model.FindEntityType("Children")!.GetForeignKeys());
    }

    [Fact]
    public void Composite_foreign_key_with_unmappable_dependent_column_is_skipped_without_changing_other_columns()
    {
        var parent = CreateCompositeParent("Parent");
        var children = CreateTable("Children", [Column("Id", "int"), Column("ParentId1", "bigint"), Column("ParentId2", null)]);
        var foreignKey = AddForeignKey(children, parent, [children.Columns[1], children.Columns[2]], [.. parent.Columns]);

        var model = Scaffold(parent, children);

        AssertForeignKeyWarning(DesignStrings.ForeignKeyScaffoldErrorPropertyNotFound(foreignKey.DisplayName(), "ParentId2"));

        var entityType = model.FindEntityType("Children")!;
        Assert.Empty(entityType.GetForeignKeys());
        AssertProperty(entityType.FindProperty("ParentId1")!, typeof(long), "bigint");
        Assert.Null(entityType.FindProperty("ParentId2"));
    }

    [Fact]
    public void Composite_foreign_key_with_unmappable_principal_column_is_skipped_without_changing_other_columns()
    {
        var parent = CreateCompositeParent("Parent");
        parent.Columns[1].StoreType = null;
        var children = CreateTable("Children", [Column("Id", "int"), Column("ParentId1", "bigint"), Column("ParentId2", "int")]);
        var foreignKey = AddForeignKey(children, parent, [children.Columns[1], children.Columns[2]], [.. parent.Columns]);

        var model = Scaffold(parent, children);

        AssertForeignKeyWarning(DesignStrings.ForeignKeyScaffoldErrorPrincipalTableScaffoldingError(foreignKey.DisplayName(), parent.DisplayName()));

        var entityType = model.FindEntityType("Children")!;
        Assert.Empty(entityType.GetForeignKeys());
        AssertProperty(entityType.FindProperty("ParentId1")!, typeof(long), "bigint");
    }

    [Fact]
    public void Composite_foreign_key_with_unmappable_columns_at_the_same_position_is_skipped_without_changing_other_columns()
    {
        var parent = CreateCompositeParent("Parent");
        parent.Columns[1].StoreType = null;
        var children = CreateTable("Children", [Column("Id", "int"), Column("ParentId1", "bigint"), Column("ParentId2", null)]);
        var foreignKey = AddForeignKey(children, parent, [children.Columns[1], children.Columns[2]], [.. parent.Columns]);

        var model = Scaffold(parent, children);

        AssertForeignKeyWarning(DesignStrings.ForeignKeyScaffoldErrorPropertyNotFound(foreignKey.DisplayName(), "ParentId2"));

        var entityType = model.FindEntityType("Children")!;
        Assert.Empty(entityType.GetForeignKeys());
        AssertProperty(entityType.FindProperty("ParentId1")!, typeof(long), "bigint");
    }

    [Fact]
    public void Composite_foreign_key_with_principal_columns_in_a_different_order_does_not_change_dependent_types()
    {
        var parent = CreateCompositeParent("Parent");
        var children = CreateTable("Children", [Column("Id", "int"), Column("ParentId1", "bigint"), Column("ParentId2", "bigint")]);
        var foreignKey = AddForeignKey(
            children, parent, [children.Columns[1], children.Columns[2]], [parent.Columns[1], parent.Columns[0]]);

        var model = Scaffold(parent, children);

        AssertForeignKeyWarning(
            DesignStrings.ForeignKeyScaffoldErrorPrincipalKeyNotFound(
                foreignKey.DisplayName(), string.Join(System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator, "Id2", "Id1"), "Parent"));

        var entityType = model.FindEntityType("Children")!;
        Assert.Empty(entityType.GetForeignKeys());
        AssertProperty(entityType.FindProperty("ParentId1")!, typeof(long), "bigint");
        AssertProperty(entityType.FindProperty("ParentId2")!, typeof(long), "bigint");
    }

    [Fact]
    public void Foreign_keys_from_same_column_to_decimal_principals_are_skipped_unless_all_principals_are_integers()
    {
        var parent = CreateTable("Parent", [Column("Id", "decimal(18, 0)")]);
        var otherParent = CreateTable("OtherParent", [Column("Id", "decimal(18, 2)")]);
        var children = CreateTable("Children", [Column("Id", "int"), Column("ParentId", "bigint")]);
        var foreignKey = AddForeignKey(children, parent, [children.Columns[1]], [parent.Columns[0]], "FK_Parent");
        var otherForeignKey = AddForeignKey(children, otherParent, [children.Columns[1]], [otherParent.Columns[0]], "FK_OtherParent");

        var model = Scaffold(parent, otherParent, children);

        Assert.Equal(
            [
                DesignStrings.ForeignKeyScaffoldErrorTypeMismatch(foreignKey.DisplayName(), "{'ParentId' : long}", "{'Id' : decimal}"),
                DesignStrings.ForeignKeyScaffoldErrorTypeMismatch(
                    otherForeignKey.DisplayName(), "{'ParentId' : long}", "{'Id' : decimal}")
            ],
            _reporter.Messages.Select(m => m.Message));

        var entityType = model.FindEntityType("Children")!;
        Assert.Empty(entityType.GetForeignKeys());
        AssertProperty(entityType.FindProperty("ParentId")!, typeof(long), "bigint");
    }

    [Fact]
    public void Foreign_key_with_missing_principal_table_does_not_change_dependent_type()
    {
        var (parent, children, foreignKey) = CreateForeignKey("int", "bigint");
        foreignKey.PrincipalTable = null!;
        foreignKey.PrincipalColumns.Clear();

        var model = Scaffold(parent, children);

        var (level, message) = Assert.Single(_reporter.Messages);
        Assert.Equal(LogLevel.Warning, level);
        Assert.Equal(DesignStrings.ForeignKeyScaffoldErrorPrincipalTableNotFound(foreignKey.DisplayName()), message);
        AssertProperty(model.FindEntityType("Children")!.FindProperty("ParentId")!, typeof(long), "bigint");
    }

    [Fact]
    public void Foreign_key_with_different_numbers_of_dependent_and_principal_columns_does_not_change_dependent_types()
    {
        var parent = CreateTable("Parent", [Column("Id", "int")]);
        var children = CreateTable("Children", [Column("Id", "int"), Column("ParentId1", "bigint"), Column("ParentId2", "bigint")]);
        var foreignKey = AddForeignKey(children, parent, [children.Columns[1], children.Columns[2]], [parent.Columns[0]]);

        var model = Scaffold(parent, children);

        AssertTypeMismatchWarning(foreignKey, "{'ParentId1' : long, 'ParentId2' : long}", "{'Id' : int}");

        var entityType = model.FindEntityType("Children")!;
        Assert.Empty(entityType.GetForeignKeys());
        AssertProperty(entityType.FindProperty("ParentId1")!, typeof(long), "bigint");
        AssertProperty(entityType.FindProperty("ParentId2")!, typeof(long), "bigint");
    }

    [Fact]
    public void Foreign_key_types_are_resolved_each_time_the_model_is_created()
    {
        var (parent, children, _) = CreateForeignKey("int", "bigint");
        var database = new DatabaseModel { Tables = { parent, children } };

        for (var i = 0; i < 2; i++)
        {
            var model = _factory.Create(database, new ModelReverseEngineerOptions { NoPluralize = true });

            Assert.Empty(_reporter.Messages);

            var fk = Assert.Single(model.FindEntityType("Children")!.GetForeignKeys());
            Assert.Equal(typeof(int), Assert.Single(fk.Properties).ClrType);
        }

        // A model without the foreign key must not inherit the changed type from the earlier calls.
        children.ForeignKeys.Clear();
        var modelWithoutForeignKey = _factory.Create(database, new ModelReverseEngineerOptions { NoPluralize = true });
        Assert.Equal(typeof(long), modelWithoutForeignKey.FindEntityType("Children")!.FindProperty("ParentId")!.ClrType);
    }

    [Theory]
    [InlineData(typeof(sbyte), true)]
    [InlineData(typeof(byte), true)]
    [InlineData(typeof(short), true)]
    [InlineData(typeof(ushort), true)]
    [InlineData(typeof(int), true)]
    [InlineData(typeof(uint), true)]
    [InlineData(typeof(ulong), false)]
    public void Sqlite_foreign_key_to_principal_with_clr_type_hint_uses_principal_clr_type(Type hint, bool scaffolded)
    {
        // The dependent column is INTEGER, which is long unless the data indicates a smaller type, so it can hold any
        // value of the principal column unless the principal is ulong.
        var parent = CreateTable("Parent", [Column("Id", "INTEGER")]);
        parent.Columns[0][ScaffoldingAnnotationNames.ClrType] = hint;
        var children = CreateTable("Children", [Column("Id", "INTEGER"), Column("ParentId", "INTEGER")]);
        var foreignKey = AddForeignKey(children, parent, [children.Columns[1]], [parent.Columns[0]]);

        var (factory, reporter) = CreateFactory("Microsoft.EntityFrameworkCore.Sqlite");
        var model = ScaffoldUsing(factory, parent, children);

        var entityType = model.FindEntityType("Children")!;
        if (scaffolded)
        {
            Assert.Empty(reporter.Messages);
            AssertProperty(Assert.Single(Assert.Single(entityType.GetForeignKeys()).Properties), hint, "INTEGER");
        }
        else
        {
            var (level, message) = Assert.Single(reporter.Messages);
            Assert.Equal(LogLevel.Warning, level);
            Assert.Equal(
                DesignStrings.ForeignKeyScaffoldErrorTypeMismatch(foreignKey.DisplayName(), "{'ParentId' : long}", "{'Id' : ulong}"),
                message);
            Assert.Empty(entityType.GetForeignKeys());
        }
    }

    [Fact]
    public void Sqlite_foreign_key_from_column_with_clr_type_hint_uses_principal_clr_type()
    {
        // The hint is derived from the data in the column, and the store types are the same, so any value
        // in the principal column can be held by the foreign key column.
        var parent = CreateTable("Parent", [Column("Id", "INTEGER")]);
        var children = CreateTable("Children", [Column("Id", "INTEGER"), Column("ParentId", "INTEGER")]);
        children.Columns[1][ScaffoldingAnnotationNames.ClrType] = typeof(int);
        AddForeignKey(children, parent, [children.Columns[1]], [parent.Columns[0]]);

        var (factory, reporter) = CreateFactory("Microsoft.EntityFrameworkCore.Sqlite");
        var model = ScaffoldUsing(factory, parent, children);

        Assert.Empty(reporter.Messages);

        var fk = Assert.Single(model.FindEntityType("Children")!.GetForeignKeys());
        AssertProperty(Assert.Single(fk.Properties), typeof(long), "INTEGER");
    }

    [Theory, InlineData(false), InlineData(true)]
    public void Sqlite_composite_foreign_key_with_nullable_columns_is_not_narrowed(bool dependentHint)
    {
        // The principal columns are int because of the data in them, but a nullable composite foreign key
        // doesn't constrain the other columns, so they can hold any INTEGER value.
        var parent = CreateCompositeParent("Parent", "INTEGER");
        parent.Columns[0][ScaffoldingAnnotationNames.ClrType] = typeof(int);
        parent.Columns[1][ScaffoldingAnnotationNames.ClrType] = typeof(int);
        var children = CreateTable(
            "Children", [Column("Id", "INTEGER"), Column("ParentId1", "INTEGER", true), Column("ParentId2", "INTEGER", true)]);
        if (dependentHint)
        {
            children.Columns[1][ScaffoldingAnnotationNames.ClrType] = typeof(long);
            children.Columns[2][ScaffoldingAnnotationNames.ClrType] = typeof(long);
        }

        var foreignKey = AddForeignKey(children, parent, [children.Columns[1], children.Columns[2]], [.. parent.Columns]);

        var (factory, reporter) = CreateFactory("Microsoft.EntityFrameworkCore.Sqlite");
        var model = ScaffoldUsing(factory, parent, children);

        var (level, message) = Assert.Single(reporter.Messages);
        Assert.Equal(LogLevel.Warning, level);
        Assert.Equal(
            DesignStrings.ForeignKeyScaffoldErrorTypeMismatch(
                foreignKey.DisplayName(), "{'ParentId1' : long?, 'ParentId2' : long?}", "{'Id1' : int, 'Id2' : int}"),
            message);
        Assert.Empty(model.FindEntityType("Children")!.GetForeignKeys());
    }

    [Fact]
    public void Sqlite_composite_foreign_key_with_nullable_columns_is_widened_when_the_store_type_matches_the_principal()
    {
        // The dependent columns have an int hint, but the principal columns are INTEGER (long). The store types are
        // the same, so widening the dependent columns to long is safe even though the foreign key is nullable.
        var parent = CreateCompositeParent("Parent", "INTEGER");
        var children = CreateTable(
            "Children", [Column("Id", "INTEGER"), Column("ParentId1", "INTEGER", true), Column("ParentId2", "INTEGER", true)]);
        children.Columns[1][ScaffoldingAnnotationNames.ClrType] = typeof(int);
        children.Columns[2][ScaffoldingAnnotationNames.ClrType] = typeof(int);
        AddForeignKey(children, parent, [children.Columns[1], children.Columns[2]], [.. parent.Columns]);

        var (factory, reporter) = CreateFactory("Microsoft.EntityFrameworkCore.Sqlite");
        var model = ScaffoldUsing(factory, parent, children);

        Assert.Empty(reporter.Messages);

        var fk = Assert.Single(model.FindEntityType("Children")!.GetForeignKeys());
        Assert.Equal([typeof(long?), typeof(long?)], fk.Properties.Select(p => p.ClrType));
    }

    private IModel Scaffold(params DatabaseTable[] tables)
        => ScaffoldUsing(_factory, tables);

    private static IModel ScaffoldUsing(IScaffoldingModelFactory factory, params DatabaseTable[] tables)
    {
        var database = new DatabaseModel();
        foreach (var table in tables)
        {
            database.Tables.Add(table);
        }

        return factory.Create(database, new ModelReverseEngineerOptions { NoPluralize = true });
    }

    private static (IScaffoldingModelFactory Factory, TestOperationReporter Reporter) CreateFactory(string provider)
    {
        var reporter = new TestOperationReporter();

        var assembly = typeof(RelationalScaffoldingModelFactoryTest).Assembly;
        var factory = new DesignTimeServicesBuilder(assembly, assembly, reporter, [])
            .CreateServiceCollection(provider)
            .AddSingleton<IScaffoldingModelFactory, FakeScaffoldingModelFactory>()
            .BuildServiceProvider(validateScopes: true)
            .GetRequiredService<IScaffoldingModelFactory>();

        reporter.Clear();

        return (factory, reporter);
    }

    private static DatabaseColumn Column(string name, string? storeType, bool nullable = false)
        => new()
        {
            Name = name,
            StoreType = storeType,
            IsNullable = nullable
        };

    private static DatabaseTable CreateTable(string name, DatabaseColumn[] columns, DatabaseColumn[]? primaryKeyColumns = null)
    {
        var table = new DatabaseTable { Database = Database, Name = name };
        foreach (var column in columns)
        {
            column.Table = table;
            table.Columns.Add(column);
        }

        table.PrimaryKey = new DatabasePrimaryKey { Table = table, Name = "PK_" + name };
        foreach (var column in primaryKeyColumns ?? [columns[0]])
        {
            table.PrimaryKey.Columns.Add(column);
        }

        return table;
    }

    private static DatabaseTable CreateCompositeParent(string name, string storeType = "int")
    {
        DatabaseColumn[] columns = [Column("Id1", storeType), Column("Id2", storeType)];
        return CreateTable(name, columns, columns);
    }

    private static DatabaseForeignKey AddForeignKey(
        DatabaseTable dependent,
        DatabaseTable principal,
        DatabaseColumn[] dependentColumns,
        DatabaseColumn[] principalColumns,
        string name = "FK_Foo")
    {
        var foreignKey = new DatabaseForeignKey
        {
            Table = dependent,
            Name = name,
            PrincipalTable = principal
        };
        foreach (var column in dependentColumns)
        {
            foreignKey.Columns.Add(column);
        }

        foreach (var column in principalColumns)
        {
            foreignKey.PrincipalColumns.Add(column);
        }

        dependent.ForeignKeys.Add(foreignKey);
        return foreignKey;
    }

    // Parent(Id <principalStoreType>) and Children(Id int, ParentId <dependentStoreType>) with a foreign key between them.
    private static (DatabaseTable Parent, DatabaseTable Children, DatabaseForeignKey ForeignKey) CreateForeignKey(
        string? principalStoreType,
        string? dependentStoreType,
        bool dependentNullable = false)
    {
        var parent = CreateTable("Parent", [Column("Id", principalStoreType)]);
        var children = CreateTable("Children", [Column("Id", "int"), Column("ParentId", dependentStoreType, dependentNullable)]);
        var foreignKey = AddForeignKey(children, parent, [children.Columns[1]], [parent.Columns[0]]);

        return (parent, children, foreignKey);
    }

    private static void AssertProperty(IReadOnlyProperty property, Type expectedClrType, string expectedColumnType)
    {
        Assert.Equal(expectedClrType, property.ClrType);
        Assert.Equal(expectedColumnType, property.GetColumnType());
    }

    // The warnings for the columns that can't be mapped are reported by other code, so only check for the foreign key warning.
    private void AssertForeignKeyWarning(string expectedMessage)
    {
        Assert.All(_reporter.Messages, m => Assert.Equal(LogLevel.Warning, m.Level));
        Assert.Contains(expectedMessage, _reporter.Messages.Select(m => m.Message));
    }

    private void AssertTypeMismatchWarning(DatabaseForeignKey foreignKey, string dependentProperties, string principalProperties)
    {
        var (level, message) = Assert.Single(_reporter.Messages);
        Assert.Equal(LogLevel.Warning, level);
        Assert.Equal(
            DesignStrings.ForeignKeyScaffoldErrorTypeMismatch(foreignKey.DisplayName(), dependentProperties, principalProperties),
            message);
    }

    [Fact]
    public void Foreign_key_from_keyless_table()
    {
        var databaseModel = new DatabaseModel();
        var masterTable = new DatabaseTable { Database = databaseModel, Name = "Master" };
        var idColumn = new DatabaseColumn
        {
            Table = masterTable,
            Name = "Id",
            StoreType = "int"
        };
        masterTable.Columns.Add(idColumn);
        masterTable.PrimaryKey = new DatabasePrimaryKey
        {
            Table = masterTable,
            Name = null,
            Columns = { idColumn }
        };
        databaseModel.Tables.Add(masterTable);
        var detailTable = new DatabaseTable { Database = databaseModel, Name = "Detail" };
        var masterIdColumn = new DatabaseColumn
        {
            Table = detailTable,
            Name = "MasterId",
            StoreType = "int"
        };
        detailTable.Columns.Add(masterIdColumn);
        detailTable.ForeignKeys.Add(
            new DatabaseForeignKey
            {
                Table = detailTable,
                Name = null,
                Columns = { masterIdColumn },
                PrincipalTable = masterTable,
                PrincipalColumns = { idColumn }
            });
        databaseModel.Tables.Add(detailTable);

        var model = _factory.Create(databaseModel, new ModelReverseEngineerOptions());

        var detail = model.FindEntityType("Detail")!;
        var foreignKey = Assert.Single(detail.GetForeignKeys());
        Assert.Equal("Master", foreignKey.DependentToPrincipal!.Name);
        Assert.Null(foreignKey.PrincipalToDependent);
    }

    [Fact]
    public void Foreign_key_to_unique_constraint()
    {
        var keyColumn = new DatabaseColumn
        {
            Table = Table,
            Name = "Key",
            StoreType = "int",
            IsNullable = false
        };

        var parentTable = new DatabaseTable
        {
            Database = Database,
            Name = "Parent",
            Columns = { IdColumn, keyColumn },
            PrimaryKey = IdPrimaryKey
        };

        parentTable.UniqueConstraints.Add(
            new DatabaseUniqueConstraint
            {
                Table = parentTable,
                Name = "AK_Foo",
                Columns = { keyColumn }
            });

        var childrenTable = new DatabaseTable
        {
            Database = Database,
            Name = "Children",
            Columns = { IdColumn },
            PrimaryKey = IdPrimaryKey
        };

        childrenTable.ForeignKeys.Add(
            new DatabaseForeignKey
            {
                Table = childrenTable,
                Name = "FK_Foo",
                Columns = { childrenTable.Columns.ElementAt(0) },
                PrincipalTable = parentTable,
                PrincipalColumns = { parentTable.Columns.ElementAt(1) },
                OnDelete = ReferentialAction.Cascade,
            });

        var model = _factory.Create(
            new DatabaseModel { Tables = { parentTable, childrenTable } },
            new ModelReverseEngineerOptions { NoPluralize = true });

        var parent = (EntityType)model.FindEntityType("Parent")!;

        var children = (EntityType)model.FindEntityType("Children")!;

        Assert.NotEmpty(parent.GetReferencingForeignKeys());
        var fk = Assert.Single(children.GetForeignKeys());
        Assert.True(fk.IsUnique);
        Assert.Equal(DeleteBehavior.Cascade, fk.DeleteBehavior);

        var principalKey = fk.PrincipalKey;

        Assert.Same(parent, principalKey.DeclaringEntityType);
        Assert.Same(parent.GetProperties().First(p => p.Name == "Key"), principalKey.Properties[0]);
    }

    [Fact]
    public void Unique_foreign_key()
    {
        var parentTable = new DatabaseTable
        {
            Database = Database,
            Name = "Parent",
            Columns = { IdColumn },
            PrimaryKey = IdPrimaryKey
        };
        var childrenTable = new DatabaseTable
        {
            Database = Database,
            Name = "Children",
            Columns = { IdColumn },
            PrimaryKey = IdPrimaryKey
        };
        childrenTable.ForeignKeys.Add(
            new DatabaseForeignKey
            {
                Table = childrenTable,
                Name = "FK_Foo",
                Columns = { childrenTable.Columns.ElementAt(0) },
                PrincipalTable = parentTable,
                PrincipalColumns = { parentTable.Columns.ElementAt(0) },
                OnDelete = ReferentialAction.NoAction
            });

        var model = _factory.Create(
            new DatabaseModel { Tables = { parentTable, childrenTable } },
            new ModelReverseEngineerOptions { NoPluralize = true });

        var children = (EntityType)model.FindEntityType("Children")!;

        var fk = Assert.Single(children.GetForeignKeys());
        Assert.True(fk.IsUnique);
        Assert.Equal(DeleteBehavior.ClientSetNull, fk.DeleteBehavior);
    }

    [Fact]
    public void Composite_foreign_key()
    {
        var ida = new DatabaseColumn
        {
            Table = Table,
            Name = "Id_A",
            StoreType = "int"
        };
        var idb = new DatabaseColumn
        {
            Table = Table,
            Name = "Id_B",
            StoreType = "int"
        };
        var parentTable = new DatabaseTable
        {
            Database = Database,
            Name = "Parent",
            Columns = { ida, idb },
            PrimaryKey = new DatabasePrimaryKey
            {
                Table = Table,
                Name = "PK_Foo",
                Columns = { ida, idb }
            }
        };
        var childrenTable = new DatabaseTable
        {
            Database = Database,
            Name = "Children",
            Columns =
            {
                IdColumn,
                new DatabaseColumn
                {
                    Table = Table,
                    Name = "ParentId_A",
                    StoreType = "int"
                },
                new DatabaseColumn
                {
                    Table = Table,
                    Name = "ParentId_B",
                    StoreType = "int"
                }
            },
            PrimaryKey = IdPrimaryKey
        };
        childrenTable.ForeignKeys.Add(
            new DatabaseForeignKey
            {
                Table = childrenTable,
                Name = "FK_Foo",
                Columns = { childrenTable.Columns.ElementAt(1), childrenTable.Columns.ElementAt(2) },
                PrincipalTable = parentTable,
                PrincipalColumns = { parentTable.Columns.ElementAt(0), parentTable.Columns.ElementAt(1) },
                OnDelete = ReferentialAction.SetNull
            });

        var model = _factory.Create(
            new DatabaseModel { Tables = { parentTable, childrenTable } },
            new ModelReverseEngineerOptions { NoPluralize = true });

        var parent = (EntityType)model.FindEntityType("Parent")!;

        var children = (EntityType)model.FindEntityType("Children")!;

        Assert.NotEmpty(parent.GetReferencingForeignKeys());

        var fk = Assert.Single(children.GetForeignKeys());
        Assert.False(fk.IsUnique);
        Assert.Equal(DeleteBehavior.SetNull, fk.DeleteBehavior);

        var principalKey = fk.PrincipalKey;

        Assert.Equal("Parent", principalKey.DeclaringEntityType.Name);
        Assert.Equal("IdA", principalKey.Properties[0].Name);
        Assert.Equal("IdB", principalKey.Properties[1].Name);
    }

    [Fact]
    public void It_loads_self_referencing_foreign_key()
    {
        var table = new DatabaseTable
        {
            Database = Database,
            Name = "ItemsList",
            Columns =
            {
                IdColumn,
                new DatabaseColumn
                {
                    Table = Table,
                    Name = "ParentId",
                    StoreType = "int",
                    IsNullable = false
                }
            },
            PrimaryKey = IdPrimaryKey
        };
        table.ForeignKeys.Add(
            new DatabaseForeignKey
            {
                Table = table,
                Name = "FK_Foo",
                Columns = { table.Columns.ElementAt(1) },
                PrincipalTable = table,
                PrincipalColumns = { table.Columns.ElementAt(0) }
            });

        var model = _factory.Create(
            new DatabaseModel { Tables = { table } },
            new ModelReverseEngineerOptions());
        var list = model.FindEntityType("ItemsList")!;

        Assert.NotEmpty(list.GetReferencingForeignKeys());
        Assert.NotEmpty(list.GetForeignKeys());

        var principalKey = list.FindForeignKeys(list.FindProperty("ParentId")!).Single().PrincipalKey;
        Assert.Equal("ItemsList", principalKey.DeclaringEntityType.Name);
        Assert.Equal("Id", principalKey.Properties[0].Name);
    }

    [Fact]
    public void It_logs_warning_for_bad_foreign_key()
    {
        var parentTable = new DatabaseTable
        {
            Database = Database,
            Name = "Parent",
            Columns =
            {
                IdColumn,
                new DatabaseColumn
                {
                    Table = Table,
                    Name = "NotPkId",
                    StoreType = "int"
                }
            },
            PrimaryKey = IdPrimaryKey
        };
        var childrenTable = new DatabaseTable
        {
            Database = Database,
            Name = "Children",
            Columns =
            {
                IdColumn,
                new DatabaseColumn
                {
                    Table = Table,
                    Name = "ParentId",
                    StoreType = "int"
                }
            },
            PrimaryKey = IdPrimaryKey
        };
        childrenTable.ForeignKeys.Add(
            new DatabaseForeignKey
            {
                Table = childrenTable,
                Name = "FK_Foo",
                Columns = { childrenTable.Columns.ElementAt(1) },
                PrincipalTable = parentTable,
                PrincipalColumns = { parentTable.Columns.ElementAt(1) }
            });

        _factory.Create(
            new DatabaseModel { Tables = { parentTable, childrenTable } },
            new ModelReverseEngineerOptions());

        var (level, message) = _reporter.Messages.Single();
        Assert.Equal(LogLevel.Warning, level);
        Assert.Equal(
            DesignStrings.ForeignKeyScaffoldErrorPrincipalKeyNotFound(
                childrenTable.ForeignKeys.ElementAt(0).DisplayName(), "NotPkId", "Parent"), message);
    }

    [Fact]
    public void It_logs_warning_for_duplicate_foreign_key()
    {
        var parentTable = new DatabaseTable
        {
            Database = Database,
            Name = "Parent",
            Columns = { IdColumn },
            PrimaryKey = IdPrimaryKey
        };
        var childrenTable = new DatabaseTable
        {
            Database = Database,
            Name = "Children",
            Columns =
            {
                IdColumn,
                new DatabaseColumn
                {
                    Table = Table,
                    Name = "ParentId",
                    StoreType = "int"
                }
            },
            PrimaryKey = IdPrimaryKey
        };
        childrenTable.ForeignKeys.Add(
            new DatabaseForeignKey
            {
                Table = childrenTable,
                Name = "FK_Foo",
                Columns = { childrenTable.Columns.ElementAt(1) },
                PrincipalTable = parentTable,
                PrincipalColumns = { parentTable.Columns.ElementAt(0) }
            });
        childrenTable.ForeignKeys.Add(
            new DatabaseForeignKey
            {
                Table = childrenTable,
                Name = "FK_Another_Foo",
                Columns = { childrenTable.Columns.ElementAt(1) },
                PrincipalTable = parentTable,
                PrincipalColumns = { parentTable.Columns.ElementAt(0) }
            });

        _factory.Create(
            new DatabaseModel { Tables = { parentTable, childrenTable } },
            new ModelReverseEngineerOptions());

        var (level, message) = _reporter.Messages.Single();
        Assert.Equal(LogLevel.Warning, level);
        Assert.Equal(
            DesignStrings.ForeignKeyWithSameFacetsExists(childrenTable.ForeignKeys.ElementAt(1).DisplayName(), "FK_Foo"), message);
    }

    [Fact]
    public void Unique_nullable_index_unused_by_foreign_key()
    {
        var table = new DatabaseTable
        {
            Database = Database,
            Name = "Friends",
            Columns =
            {
                IdColumn,
                new DatabaseColumn
                {
                    Table = Table,
                    Name = "BuddyId",
                    StoreType = "int",
                    IsNullable = true
                }
            },
            PrimaryKey = IdPrimaryKey
        };
        table.Indexes.Add(
            new DatabaseIndex
            {
                Table = Table,
                Name = "IX_Foo",
                IsUnique = true,
                Columns = { table.Columns.ElementAt(1) }
            });
        table.ForeignKeys.Add(
            new DatabaseForeignKey
            {
                Table = table,
                Name = "FK_Foo",
                Columns = { table.Columns.ElementAt(1) },
                PrincipalTable = table,
                PrincipalColumns = { table.Columns.ElementAt(0) }
            });

        var model = _factory.Create(
            new DatabaseModel { Tables = { table } },
            new ModelReverseEngineerOptions { NoPluralize = true }).FindEntityType("Friends")!;

        var buddyIdProperty = model.FindProperty("BuddyId");
        Assert.NotNull(buddyIdProperty);
        Assert.True(buddyIdProperty.IsNullable);

        var fk = Assert.Single(model.GetForeignKeys());

        Assert.True(fk.IsUnique);
        Assert.DoesNotContain(model.GetKeys(), k => !k.IsPrimaryKey());
        Assert.Equal(model.FindPrimaryKey(), fk.PrincipalKey);
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("([BuddyId] IS NOT NULL)", true)]
    [InlineData("[BuddyId] IS NOT NULL", true)]
    [InlineData("(([BuddyId] IS NOT NULL))", true)]
    [InlineData("([buddyid] is not null)", true)]
    [InlineData("\"BuddyId\" IS NOT NULL", true)]
    [InlineData("`BuddyId` IS NOT NULL", true)]
    [InlineData("BuddyId IS NOT NULL", true)]
    [InlineData("buddyid IS NOT NULL AND \"BuddyId\" IS NOT NULL", true)]
    [InlineData("IsActive IS NOT NULL", false)]
    [InlineData("BuddyId IS NOT NULL AND IsActive IS NOT NULL", false)]
    [InlineData("BuddyId IS NOT NULL OR IsActive", false)]
    [InlineData("([BuddyId] IS NOT NULL AND [BuddyId] IS NOT NULL)", true)]
    [InlineData("([IsActive]=(1))", false)]
    [InlineData("([OtherId] IS NOT NULL)", false)]
    [InlineData("([BuddyId] IS NOT NULL AND [OtherId] IS NOT NULL)", false)]
    [InlineData("([BuddyId] IS NOT NULL AND [IsActive]=(1))", false)]
    [InlineData("([BuddyId] IS NOT NULL OR [IsActive]=(1))", false)]
    [InlineData("([BuddyId]>(10))", false)]
    [InlineData("([BuddyId] IS NULL)", false)]
    [InlineData("Filter SQL", false)]
    public void Unique_index_with_filter_makes_foreign_key_unique_only_for_null_guard(string? filter, bool expectedUnique)
    {
        var table = new DatabaseTable
        {
            Database = Database,
            Name = "Friends",
            Columns =
            {
                IdColumn,
                new DatabaseColumn
                {
                    Table = Table,
                    Name = "BuddyId",
                    StoreType = "int",
                    IsNullable = true
                },
                new DatabaseColumn
                {
                    Table = Table,
                    Name = "IsActive",
                    StoreType = "bit"
                }
            },
            PrimaryKey = IdPrimaryKey
        };
        table.Indexes.Add(
            new DatabaseIndex
            {
                Table = Table,
                Name = "IX_Foo",
                IsUnique = true,
                Filter = filter,
                Columns = { table.Columns.ElementAt(1) }
            });
        table.ForeignKeys.Add(
            new DatabaseForeignKey
            {
                Table = table,
                Name = "FK_Foo",
                Columns = { table.Columns.ElementAt(1) },
                PrincipalTable = table,
                PrincipalColumns = { table.Columns.ElementAt(0) }
            });

        var model = _factory.Create(
            new DatabaseModel { Tables = { table } },
            new ModelReverseEngineerOptions { NoPluralize = true }).FindEntityType("Friends")!;

        var fk = Assert.Single(model.GetForeignKeys());
        Assert.Equal(expectedUnique, fk.IsUnique);

        // The index itself is still scaffolded as unique, with its filter.
        var index = Assert.Single(model.GetIndexes());
        Assert.True(index.IsUnique);
    }

    [Theory]
    [InlineData("[A] IS NOT NULL AND [B] IS NOT NULL", true)]
    [InlineData("([A] IS NOT NULL AND [B] IS NOT NULL)", true)]
    [InlineData("[A] IS NOT NULL", true)]
    [InlineData("[B] IS NOT NULL", true)]
    [InlineData("[A] IS NOT NULL AND [C] IS NOT NULL", false)]
    [InlineData("[A] IS NOT NULL AND [B]=1", false)]
    [InlineData("[A]=1", false)]
    public void Composite_foreign_key_with_filtered_unique_index(string filter, bool expectedUnique)
        => Assert.Equal(expectedUnique, ScaffoldedForeignKeyIsUnique(["A", "B"], (["A", "B"], true, filter)));

    [Theory]
    [InlineData("buddy_id", "([buddy_id] IS NOT NULL)", true)]
    [InlineData("buddy_id", "([BuddyId] IS NOT NULL)", false)]
    [InlineData("we]ird", "([we]]ird] IS NOT NULL)", true)]
    [InlineData("we]ird", "([we]ird] IS NOT NULL)", false)]
    [InlineData("a\"b", "\"a\"\"b\" IS NOT NULL", true)]
    [InlineData("a\"b", "\"a\" IS NOT NULL", false)]
    [InlineData("c`d", "`c``d` IS NOT NULL", true)]
    [InlineData("c`d", "`c` IS NOT NULL", false)]
    public void Filter_is_matched_against_database_column_names(string columnName, string filter, bool expectedUnique)
        => Assert.Equal(expectedUnique, ScaffoldedForeignKeyIsUnique([columnName], ([columnName], true, filter)));

    [Theory]
    [InlineData("PersonId", "personid", "[PersonId] IS NOT NULL", true)]
    [InlineData("PersonId", "personid", "[personid] IS NOT NULL", false)]
    [InlineData("PersonId", "personid", "[PERSONID] IS NOT NULL", false)]
    [InlineData("PersonId", "personid", "[PersonId] IS NOT NULL AND [personid] IS NOT NULL", false)]
    [InlineData("personid", "PersonId", "[personid] IS NOT NULL", true)]
    [InlineData("personid", "PersonId", "[PersonId] IS NOT NULL", false)]
    [InlineData("\u00e4", "\u00c4", "\"\u00e4\" IS NOT NULL", true)]
    [InlineData("\u00e4", "\u00c4", "\"\u00c4\" IS NOT NULL", false)]
    public void Filter_column_is_resolved_exactly_when_columns_differ_only_by_case(
        string foreignKeyColumn,
        string otherColumn,
        string filter,
        bool expectedUnique)
        => Assert.Equal(
            expectedUnique,
            ScaffoldedForeignKeyIsUnique([foreignKeyColumn], [otherColumn], ([foreignKeyColumn], true, filter)));

    [Theory]
    [InlineData("NULL", "NULL IS NOT NULL", false)]
    [InlineData("NULL", "null IS NOT NULL", false)]
    [InlineData("NULL", "(NULL IS NOT NULL)", false)]
    [InlineData("NULL", "NULL IS NOT NULL AND NULL IS NOT NULL", false)]
    [InlineData("NULL", "\"NULL\" IS NOT NULL", true)]
    [InlineData("NULL", "[NULL] IS NOT NULL", true)]
    [InlineData("NULL", "`NULL` IS NOT NULL", true)]
    [InlineData("Nullable", "Nullable IS NOT NULL", true)]
    public void Bare_NULL_in_filter_is_not_a_column_name(string columnName, string filter, bool expectedUnique)
        => Assert.Equal(expectedUnique, ScaffoldedForeignKeyIsUnique([columnName], ([columnName], true, filter)));

    [Fact]
    public void Foreign_key_is_unique_if_any_matching_unique_index_is_not_restricted()
    {
        Assert.True(ScaffoldedForeignKeyIsUnique(["A"], (["A"], true, "[B]=1"), (["A"], true, null)));
        Assert.True(ScaffoldedForeignKeyIsUnique(["A"], (["A"], true, null), (["A"], true, "[B]=1")));
        Assert.True(ScaffoldedForeignKeyIsUnique(["A"], (["A"], true, "[B]=1"), (["A"], true, "[A] IS NOT NULL")));
        Assert.False(ScaffoldedForeignKeyIsUnique(["A"], (["A"], true, "[B]=1"), (["A"], false, null)));
    }

    [Fact]
    public void Adversarial_index_filters_are_handled_quickly()
    {
        // Long or pathological filters must neither hang nor throw; unrecognized ones are treated as restricting.
        foreach (var (filter, expectedUnique) in new[]
        {
            (new string(' ', 100_000) + "x", false),
            (new string('(', 100_000) + "[A] IS NOT NULL" + new string(')', 100_000), true),
            ("[A] IS NOT NULL" + new string(')', 100_000) + "x", false),
            ("[A] IS NOT NULL" + string.Concat(Enumerable.Repeat(" AND [A] IS NOT NULL", 5_000)), true),
            ("[A] IS NOT NULL" + string.Concat(Enumerable.Repeat(" AND [A] IS NOT NULL", 5_000)) + " AND x", false),
            ("[" + new string(']', 100_000), false)
        })
        {
            Assert.Equal(expectedUnique, ScaffoldedForeignKeyIsUnique(["A"], (["A"], true, filter)));
        }
    }

    private static bool ScaffoldedForeignKeyIsUnique(
        string[] foreignKeyColumns,
        params (string[] Columns, bool IsUnique, string? Filter)[] indexes)
        => ScaffoldedForeignKeyIsUnique(foreignKeyColumns, [], indexes);

    private static bool ScaffoldedForeignKeyIsUnique(
        string[] foreignKeyColumns,
        string[] otherColumns,
        params (string[] Columns, bool IsUnique, string? Filter)[] indexes)
    {
        var principal = new DatabaseTable { Database = Database, Name = "Principal" };
        var principalColumns = foreignKeyColumns
            .Select((_, i) => new DatabaseColumn { Table = principal, Name = "P" + i, StoreType = "int" })
            .ToList();
        foreach (var column in principalColumns)
        {
            principal.Columns.Add(column);
        }

        var principalKey = new DatabasePrimaryKey { Table = principal, Name = "PK_Principal" };
        foreach (var column in principalColumns)
        {
            principalKey.Columns.Add(column);
        }

        principal.PrimaryKey = principalKey;

        var dependent = new DatabaseTable { Database = Database, Name = "Dependent" };
        var id = new DatabaseColumn { Table = dependent, Name = "Id", StoreType = "int" };
        dependent.Columns.Add(id);
        dependent.PrimaryKey = new DatabasePrimaryKey { Table = dependent, Name = "PK_Dependent", Columns = { id } };

        var columns = foreignKeyColumns
            .Select(name => new DatabaseColumn { Table = dependent, Name = name, StoreType = "int", IsNullable = true })
            .ToList();
        foreach (var column in columns)
        {
            dependent.Columns.Add(column);
        }

        foreach (var name in otherColumns)
        {
            dependent.Columns.Add(new DatabaseColumn { Table = dependent, Name = name, StoreType = "int", IsNullable = true });
        }

        var i = 0;
        foreach (var (indexColumns, isUnique, filter) in indexes)
        {
            var index = new DatabaseIndex
            {
                Table = dependent,
                Name = "IX_" + i++,
                IsUnique = isUnique,
                Filter = filter
            };
            foreach (var name in indexColumns)
            {
                index.Columns.Add(columns.Single(c => c.Name == name));
            }

            dependent.Indexes.Add(index);
        }

        var foreignKey = new DatabaseForeignKey { Table = dependent, Name = "FK_Dependent", PrincipalTable = principal };
        foreach (var column in columns)
        {
            foreignKey.Columns.Add(column);
        }

        foreach (var column in principalColumns)
        {
            foreignKey.PrincipalColumns.Add(column);
        }

        dependent.ForeignKeys.Add(foreignKey);

        var model = new RelationalScaffoldingModelFactoryTest()._factory.Create(
            new DatabaseModel { Tables = { principal, dependent } },
            new ModelReverseEngineerOptions { NoPluralize = true });

        return Assert.Single(model.FindEntityType("Dependent")!.GetForeignKeys()).IsUnique;
    }

    [Fact]
    public void Unique_nullable_index_used_by_foreign_key()
    {
        var table = new DatabaseTable
        {
            Database = Database,
            Name = "Friends",
            Columns =
            {
                IdColumn,
                new DatabaseColumn
                {
                    Table = Table,
                    Name = "BuddyId",
                    StoreType = "int",
                    IsNullable = true
                }
            },
            PrimaryKey = IdPrimaryKey
        };
        table.Indexes.Add(
            new DatabaseIndex
            {
                Table = Table,
                Name = "FriendsNameUniqueIndex",
                Columns = { table.Columns.ElementAt(1) },
                IsUnique = true
            });
        table.ForeignKeys.Add(
            new DatabaseForeignKey
            {
                Table = table,
                Name = "FK_Foo",
                Columns = { table.Columns.ElementAt(1) },
                PrincipalTable = table,
                PrincipalColumns = { table.Columns.ElementAt(1) }
            });

        var model = _factory.Create(
            new DatabaseModel { Tables = { table } },
            new ModelReverseEngineerOptions { NoPluralize = true }).FindEntityType("Friends")!;

        var buddyIdProperty = model.FindProperty("BuddyId");
        Assert.NotNull(buddyIdProperty);
        Assert.False(buddyIdProperty.IsNullable);

        var fk = Assert.Single(model.GetForeignKeys());

        Assert.True(fk.IsUnique);
        var alternateKey = model.GetKeys().Single(k => !k.IsPrimaryKey());
        Assert.Equal(alternateKey, fk.PrincipalKey);

        var (level, message) = _reporter.Messages.Single();
        Assert.Equal(LogLevel.Warning, level);
        Assert.Equal(
            DesignStrings.ForeignKeyPrincipalEndContainsNullableColumns(
                table.ForeignKeys.ElementAt(0).DisplayName(), "FriendsNameUniqueIndex", "Friends.BuddyId"), message);
    }

    [Fact]
    public void Unique_index_composite_foreign_key()
    {
        var ida = new DatabaseColumn
        {
            Table = Table,
            Name = "Id_A",
            StoreType = "int"
        };
        var idb = new DatabaseColumn
        {
            Table = Table,
            Name = "Id_B",
            StoreType = "int"
        };
        var parentTable = new DatabaseTable
        {
            Database = Database,
            Name = "Parent",
            Columns = { ida, idb },
            PrimaryKey = new DatabasePrimaryKey
            {
                Table = Table,
                Name = "PK_Foo",
                Columns = { ida, idb }
            }
        };
        var childrenTable = new DatabaseTable
        {
            Database = Database,
            Name = "Children",
            Columns =
            {
                IdColumn,
                new DatabaseColumn
                {
                    Table = Table,
                    Name = "ParentId_A",
                    StoreType = "int"
                },
                new DatabaseColumn
                {
                    Table = Table,
                    Name = "ParentId_B",
                    StoreType = "int"
                }
            },
            PrimaryKey = IdPrimaryKey
        };
        childrenTable.Indexes.Add(
            new DatabaseIndex
            {
                Table = Table,
                Name = "IX_Foo",
                IsUnique = true,
                Columns = { childrenTable.Columns.ElementAt(1), childrenTable.Columns.ElementAt(2) }
            });
        childrenTable.ForeignKeys.Add(
            new DatabaseForeignKey
            {
                Table = childrenTable,
                Name = "FK_Foo",
                Columns = { childrenTable.Columns.ElementAt(1), childrenTable.Columns.ElementAt(2) },
                PrincipalTable = parentTable,
                PrincipalColumns = { parentTable.Columns.ElementAt(0), parentTable.Columns.ElementAt(1) }
            });

        var model = _factory.Create(
            new DatabaseModel { Tables = { parentTable, childrenTable } },
            new ModelReverseEngineerOptions { NoPluralize = true });
        var parent = model.FindEntityType("Parent")!;
        var children = model.FindEntityType("Children")!;

        var fk = Assert.Single(children.GetForeignKeys());

        Assert.True(fk.IsUnique);
        Assert.Equal(parent.FindPrimaryKey(), fk.PrincipalKey);
    }

    [Fact]
    public void Index_descending()
    {
        var table = new DatabaseTable
        {
            Database = Database,
            Name = "SomeTable",
            Columns =
            {
                new DatabaseColumn
                {
                    Table = Table,
                    Name = "X",
                    StoreType = "int"
                },
                new DatabaseColumn
                {
                    Table = Table,
                    Name = "Y",
                    StoreType = "int"
                },
                new DatabaseColumn
                {
                    Table = Table,
                    Name = "Z",
                    StoreType = "int"
                }
            }
        };

        table.Indexes.Add(
            new DatabaseIndex
            {
                Table = Table,
                Name = "IX_unspecified",
                Columns =
                {
                    table.Columns[0],
                    table.Columns[1],
                    table.Columns[2]
                }
            });

        table.Indexes.Add(
            new DatabaseIndex
            {
                Table = Table,
                Name = "IX_all_ascending",
                Columns =
                {
                    table.Columns[0],
                    table.Columns[1],
                    table.Columns[2]
                },
                IsDescending =
                {
                    false,
                    false,
                    false
                }
            });

        table.Indexes.Add(
            new DatabaseIndex
            {
                Table = Table,
                Name = "IX_all_descending",
                Columns =
                {
                    table.Columns[0],
                    table.Columns[1],
                    table.Columns[2]
                },
                IsDescending =
                {
                    true,
                    true,
                    true
                }
            });

        table.Indexes.Add(
            new DatabaseIndex
            {
                Table = Table,
                Name = "IX_mixed",
                Columns =
                {
                    table.Columns[0],
                    table.Columns[1],
                    table.Columns[2]
                },
                IsDescending =
                {
                    false,
                    true,
                    false
                }
            });

        var model = _factory.Create(
            new DatabaseModel { Tables = { table } },
            new ModelReverseEngineerOptions { NoPluralize = true });

        var entityType = model.FindEntityType("SomeTable")!;

        var unspecifiedIndex = Assert.Single(entityType.GetIndexes(), i => i.Name == "IX_unspecified");
        Assert.Null(unspecifiedIndex.IsDescending);

        var allAscendingIndex = Assert.Single(entityType.GetIndexes(), i => i.Name == "IX_all_ascending");
        Assert.Null(allAscendingIndex.IsDescending);

        var allDescendingIndex = Assert.Single(entityType.GetIndexes(), i => i.Name == "IX_all_descending");
        Assert.Equal([], allDescendingIndex.IsDescending);

        var mixedIndex = Assert.Single(entityType.GetIndexes(), i => i.Name == "IX_mixed");
        Assert.Equal([false, true, false], mixedIndex.IsDescending);
    }

    [Fact]
    public void Unique_names()
    {
        var info = new DatabaseModel
        {
            Tables =
            {
                new DatabaseTable
                {
                    Database = Database,
                    Name = "E F",
                    Columns =
                    {
                        IdColumn,
                        new DatabaseColumn
                        {
                            Table = Table,
                            Name = "San itized",
                            StoreType = "int"
                        },
                        new DatabaseColumn
                        {
                            Table = Table,
                            Name = "San+itized",
                            StoreType = "int"
                        }
                    },
                    PrimaryKey = IdPrimaryKey
                },
                new DatabaseTable
                {
                    Database = Database,
                    Name = "E+F",
                    Columns = { IdColumn },
                    PrimaryKey = IdPrimaryKey
                }
            }
        };

        var model = _factory.Create(info, new ModelReverseEngineerOptions());

        Assert.Collection(
            model.GetEntityTypes().Cast<EntityType>(),
            ef1 =>
            {
                Assert.Equal("E F", ef1.GetTableName());
                Assert.Equal("EF", ef1.Name);
                Assert.Collection(
                    ef1.GetProperties(),
                    id => Assert.Equal("Id", id.Name),
                    s1 =>
                    {
                        Assert.Equal("SanItized", s1.Name);
                        Assert.Equal("San itized", s1.GetColumnName());
                    },
                    s2 =>
                    {
                        Assert.Equal("SanItized1", s2.Name);
                        Assert.Equal("San+itized", s2.GetColumnName());
                    });
            },
            ef2 =>
            {
                Assert.Equal("E+F", ef2.GetTableName());
                Assert.Equal("EF1", ef2.Name);
                var id = Assert.Single(ef2.GetProperties());
                Assert.Equal("Id", id.Name);
                Assert.Equal("Id", id.GetColumnName());
            });
    }

    [Fact]
    public void Sequences()
    {
        var info = new DatabaseModel
        {
            Sequences =
            {
                new DatabaseSequence
                {
                    Database = Database,
                    Name = "CountByThree",
                    IncrementBy = 3,
                    ["CustomAnnotation"] = "Hello there"
                }
            }
        };

        var model = _factory.Create(info, new ModelReverseEngineerOptions());

        Assert.Collection(
            model.GetSequences(), first =>
            {
                Assert.NotNull(first);
                Assert.Equal("CountByThree", first.Name);
                Assert.Equal(3, first.IncrementBy);
                Assert.Null(first.Schema);
                Assert.Null(first.MaxValue);
                Assert.Null(first.MinValue);
                Assert.False(first.IsCyclic);
            });
    }

    [Fact]
    public void DbSet_annotation_is_set()
    {
        var info = new DatabaseModel
        {
            Tables =
            {
                new DatabaseTable
                {
                    Database = Database,
                    Name = "Blog",
                    Columns = { IdColumn },
                    PrimaryKey = IdPrimaryKey
                }
            }
        };

        var model = _factory.Create(info, new ModelReverseEngineerOptions { NoPluralize = true });
        Assert.Equal("Blog", model.GetEntityTypes().Single().GetDbSetName());
    }

    [Fact]
    public void Pluralization_of_entity_and_DbSet()
    {
        var info = new DatabaseModel
        {
            Tables =
            {
                new DatabaseTable
                {
                    Database = Database,
                    Name = "Blog",
                    Columns = { IdColumn },
                    PrimaryKey = IdPrimaryKey
                },
                new DatabaseTable
                {
                    Database = Database,
                    Name = "Posts",
                    Columns = { IdColumn },
                    PrimaryKey = IdPrimaryKey
                }
            }
        };

        var model = _factory.Create(info, new ModelReverseEngineerOptions());

        Assert.Collection(
            model.GetEntityTypes().OrderBy(t => t.Name).Cast<EntityType>(),
            entity =>
            {
                Assert.Equal("Blog", entity.GetTableName());
                Assert.Equal("Blog", entity.Name);
                Assert.Equal("Blogs", entity.GetDbSetName());
            },
            entity =>
            {
                Assert.Equal("Posts", entity.GetTableName());
                Assert.Equal("Post", entity.Name);
                Assert.Equal("Posts", entity.GetDbSetName());
            }
        );
    }

    [Fact]
    public void Pluralization_of_entity_and_DbSet_noPluralize()
    {
        var info = new DatabaseModel
        {
            Tables =
            {
                new DatabaseTable
                {
                    Database = Database,
                    Name = "Blog",
                    Columns = { IdColumn },
                    PrimaryKey = IdPrimaryKey
                },
                new DatabaseTable
                {
                    Database = Database,
                    Name = "Posts",
                    Columns = { IdColumn },
                    PrimaryKey = IdPrimaryKey
                }
            }
        };

        var model = _factory.Create(info, new ModelReverseEngineerOptions { NoPluralize = true });

        Assert.Collection(
            model.GetEntityTypes().OrderBy(t => t.Name).Cast<EntityType>(),
            entity =>
            {
                Assert.Equal("Blog", entity.GetTableName());
                Assert.Equal("Blog", entity.Name);
                Assert.Equal("Blog", entity.GetDbSetName());
            },
            entity =>
            {
                Assert.Equal("Posts", entity.GetTableName());
                Assert.Equal("Posts", entity.Name);
                Assert.Equal("Posts", entity.GetDbSetName());
            }
        );

        model = _factory.Create(info, new ModelReverseEngineerOptions { UseDatabaseNames = true, NoPluralize = true });

        Assert.Collection(
            model.GetEntityTypes().OrderBy(t => t.Name).Cast<EntityType>(),
            entity =>
            {
                Assert.Equal("Blog", entity.GetTableName());
                Assert.Equal("Blog", entity.Name);
                Assert.Equal("Blog", entity.GetDbSetName());
            },
            entity =>
            {
                Assert.Equal("Posts", entity.GetTableName());
                Assert.Equal("Posts", entity.Name);
                Assert.Equal("Posts", entity.GetDbSetName());
            }
        );
    }

    [Fact]
    public void Pluralization_of_collection_navigations()
    {
        var blogTable = new DatabaseTable
        {
            Database = Database,
            Name = "Blog",
            Columns = { IdColumn },
            PrimaryKey = IdPrimaryKey
        };
        var postTable = new DatabaseTable
        {
            Database = Database,
            Name = "Post",
            Columns =
            {
                IdColumn,
                new DatabaseColumn
                {
                    Table = Table,
                    Name = "BlogId",
                    StoreType = "int",
                    IsNullable = true
                }
            },
            PrimaryKey = IdPrimaryKey
        };

        postTable.ForeignKeys.Add(
            new DatabaseForeignKey
            {
                Table = postTable,
                Name = "FK_Foo",
                Columns = { postTable.Columns.ElementAt(1) },
                PrincipalTable = blogTable,
                PrincipalColumns = { blogTable.Columns.ElementAt(0) },
                OnDelete = ReferentialAction.Cascade
            });

        var info = new DatabaseModel { Tables = { blogTable, postTable } };

        var model = _factory.Create(info, new ModelReverseEngineerOptions());

        Assert.Collection(
            model.GetEntityTypes().OrderBy(t => t.Name).Cast<EntityType>(),
            entity =>
            {
                Assert.Equal("Blog", entity.Name);
                Assert.Equal("Posts", entity.GetNavigations().Single().Name);
            },
            entity =>
            {
                Assert.Equal("Post", entity.Name);
                Assert.Equal("Blog", entity.GetNavigations().Single().Name);
            }
        );
    }

    [Fact]
    public void Pluralization_of_collection_navigations_noPluralize()
    {
        var blogTable = new DatabaseTable
        {
            Database = Database,
            Name = "Blog",
            Columns = { IdColumn },
            PrimaryKey = IdPrimaryKey
        };
        var postTable = new DatabaseTable
        {
            Database = Database,
            Name = "Post",
            Columns =
            {
                IdColumn,
                new DatabaseColumn
                {
                    Table = Table,
                    Name = "BlogId",
                    StoreType = "int",
                    IsNullable = true
                }
            },
            PrimaryKey = IdPrimaryKey
        };

        postTable.ForeignKeys.Add(
            new DatabaseForeignKey
            {
                Table = postTable,
                Name = "FK_Foo",
                Columns = { postTable.Columns.ElementAt(1) },
                PrincipalTable = blogTable,
                PrincipalColumns = { blogTable.Columns.ElementAt(0) },
                OnDelete = ReferentialAction.Cascade
            });

        var info = new DatabaseModel { Tables = { blogTable, postTable } };

        var model = _factory.Create(info, new ModelReverseEngineerOptions { NoPluralize = true });

        Assert.Collection(
            model.GetEntityTypes().OrderBy(t => t.Name).Cast<EntityType>(),
            entity =>
            {
                Assert.Equal("Blog", entity.Name);
                Assert.Equal("Post", entity.GetNavigations().Single().Name);
            },
            entity =>
            {
                Assert.Equal("Post", entity.Name);
                Assert.Equal("Blog", entity.GetNavigations().Single().Name);
            }
        );
    }

    [Fact]
    public void Not_null_bool_column_with_unparsed_default_value_is_made_nullable()
    {
        var dbModel = new DatabaseModel
        {
            Tables =
            {
                new DatabaseTable
                {
                    Database = Database,
                    Name = "Table",
                    Columns =
                    {
                        IdColumn,
                        new DatabaseColumn
                        {
                            Table = Table,
                            Name = "NonNullBoolWithDefault",
                            StoreType = "bit",
                            DefaultValueSql = "Default",
                            IsNullable = false
                        },
                        new DatabaseColumn
                        {
                            Table = Table,
                            Name = "NonNullBoolWithoutDefault",
                            StoreType = "bit",
                            IsNullable = false
                        }
                    },
                    PrimaryKey = IdPrimaryKey
                }
            }
        };

        var model = _factory.Create(dbModel, new ModelReverseEngineerOptions());

        var columns = model.FindEntityType("Table")!.GetProperties().ToList();

        Assert.Equal(typeof(bool), columns.First(c => c.Name == "NonNullBoolWithoutDefault").ClrType);
        Assert.False(columns.First(c => c.Name == "NonNullBoolWithoutDefault").IsNullable);
        Assert.Equal(typeof(bool?), columns.First(c => c.Name == "NonNullBoolWithDefault").ClrType);
        Assert.False(columns.First(c => c.Name == "NonNullBoolWithDefault").IsNullable);
        Assert.Equal("Default", columns.First(c => c.Name == "NonNullBoolWithDefault")[RelationalAnnotationNames.DefaultValueSql]);
    }

    [Fact]
    public void Not_null_bool_column_with_parsed_default_value_is_not_made_nullable()
    {
        var dbModel = new DatabaseModel
        {
            Tables =
            {
                new DatabaseTable
                {
                    Database = Database,
                    Name = "Table",
                    Columns =
                    {
                        IdColumn,
                        new DatabaseColumn
                        {
                            Table = Table,
                            Name = "NonNullBoolWithDefault",
                            StoreType = "bit",
                            DefaultValueSql = "1",
                            DefaultValue = true,
                            IsNullable = false
                        },
                        new DatabaseColumn
                        {
                            Table = Table,
                            Name = "NonNullBoolWithoutDefault",
                            StoreType = "bit",
                            IsNullable = false
                        }
                    },
                    PrimaryKey = IdPrimaryKey
                }
            }
        };

        var model = _factory.Create(dbModel, new ModelReverseEngineerOptions());

        var columns = model.FindEntityType("Table")!.GetProperties().ToList();
        var columnWithDefault = columns.First(c => c.Name == "NonNullBoolWithDefault");
        var columnWithoutDefault = columns.First(c => c.Name == "NonNullBoolWithoutDefault");

        Assert.Equal(typeof(bool), columnWithoutDefault.ClrType);
        Assert.False(columnWithoutDefault.IsNullable);
        Assert.Equal(typeof(bool), columnWithDefault.ClrType);
        Assert.False(columnWithDefault.IsNullable);
        Assert.Equal("1", columnWithDefault[RelationalAnnotationNames.DefaultValueSql]);
        Assert.Equal(true, columnWithDefault[RelationalAnnotationNames.DefaultValue]);
        Assert.Null(columnWithoutDefault[RelationalAnnotationNames.DefaultValueSql]);
        Assert.Null(columnWithoutDefault[RelationalAnnotationNames.DefaultValue]);

        Assert.Empty(_reporter.Messages);
    }

    [Fact]
    public void Nullable_column_with_default_value_sql_does_not_generate_warning()
    {
        var dbModel = new DatabaseModel
        {
            Tables =
            {
                new DatabaseTable
                {
                    Database = Database,
                    Name = "Table",
                    Columns =
                    {
                        IdColumn,
                        new DatabaseColumn
                        {
                            Table = Table,
                            Name = "NullBoolWithDefault",
                            StoreType = "bit",
                            DefaultValueSql = "Default",
                            IsNullable = true
                        }
                    },
                    PrimaryKey = IdPrimaryKey
                }
            }
        };

        var model = _factory.Create(dbModel, new ModelReverseEngineerOptions());

        var columns = model.FindEntityType("Table")!.GetProperties().ToList();

        Assert.Equal(typeof(bool?), columns.First(c => c.Name == "NullBoolWithDefault").ClrType);
        Assert.True(columns.First(c => c.Name == "NullBoolWithDefault").IsNullable);
        Assert.Equal("Default", columns.First(c => c.Name == "NullBoolWithDefault")[RelationalAnnotationNames.DefaultValueSql]);

        Assert.Empty(_reporter.Messages);
    }

    [Fact]
    public void Correct_arguments_to_scaffolding_typemapper()
    {
        var principalPkColumn = new DatabaseColumn
        {
            Table = Table,
            Name = "PrimaryKey",
            StoreType = "nvarchar(450)"
        };
        var principalAkColumn = new DatabaseColumn
        {
            Table = Table,
            Name = "AlternateKey",
            StoreType = "nvarchar(450)"
        };
        var principalIndexColumn = new DatabaseColumn
        {
            Table = Table,
            Name = "Index",
            StoreType = "nvarchar(450)"
        };
        var rowversionColumn = new DatabaseColumn
        {
            Table = Table,
            Name = "Rowversion",
            StoreType = "rowversion",
            ValueGenerated = ValueGenerated.OnAddOrUpdate,
            [ScaffoldingAnnotationNames.ConcurrencyToken] = true
        };
        var clrTypeColumn = new DatabaseColumn
        {
            Table = Table,
            Name = "ClrType",
            StoreType = "char(36)",
            [ScaffoldingAnnotationNames.ClrType] = typeof(Guid)
        };

        var principalTable = new DatabaseTable
        {
            Database = Database,
            Name = "Principal",
            Columns =
            {
                principalPkColumn,
                principalAkColumn,
                principalIndexColumn,
                rowversionColumn,
                clrTypeColumn
            },
            PrimaryKey = new DatabasePrimaryKey
            {
                Table = Table,
                Name = "PK_Foo",
                Columns = { principalPkColumn }
            },
            UniqueConstraints =
            {
                new DatabaseUniqueConstraint
                {
                    Table = Table,
                    Name = "AK_Foo",
                    Columns = { principalAkColumn }
                }
            },
            Indexes =
            {
                new DatabaseIndex
                {
                    Table = Table,
                    Name = "IX_Foo",
                    Columns = { principalIndexColumn }
                }
            }
        };

        var dependentIdColumn = new DatabaseColumn
        {
            Table = Table,
            Name = "Id",
            StoreType = "int"
        };
        var dependentFkColumn = new DatabaseColumn
        {
            Table = Table,
            Name = "BlogAlternateKey",
            StoreType = "nvarchar(450)"
        };

        var dependentTable = new DatabaseTable
        {
            Database = Database,
            Name = "Dependent",
            Columns = { dependentIdColumn, dependentFkColumn },
            PrimaryKey = new DatabasePrimaryKey
            {
                Table = Table,
                Name = "PK_Foo",
                Columns = { dependentIdColumn }
            },
            Indexes =
            {
                new DatabaseIndex
                {
                    Table = Table,
                    Name = "IX_Foo",
                    Columns = { dependentFkColumn }
                }
            },
            ForeignKeys =
            {
                new DatabaseForeignKey
                {
                    Table = Table,
                    Name = "FK_Foo",
                    Columns = { dependentFkColumn },
                    PrincipalTable = principalTable,
                    PrincipalColumns = { principalAkColumn }
                }
            }
        };

        var dbModel = new DatabaseModel { Tables = { principalTable, dependentTable } };

        var model = _factory.Create(dbModel, new ModelReverseEngineerOptions());

        Assert.Null(model.FindEntityType("Principal")!.FindProperty("PrimaryKey")!.GetConfiguredColumnType());
        Assert.Null(model.FindEntityType("Principal")!.FindProperty("AlternateKey")!.GetConfiguredColumnType());
        Assert.Null(model.FindEntityType("Principal")!.FindProperty("Index")!.GetConfiguredColumnType());
        Assert.Null(model.FindEntityType("Principal")!.FindProperty("Rowversion")!.GetConfiguredColumnType());
        Assert.Equal(typeof(Guid), model.FindEntityType("Principal")!.FindProperty("ClrType")!.ClrType);
        Assert.Null(model.FindEntityType("Dependent")!.FindProperty("BlogAlternateKey")!.GetConfiguredColumnType());
    }

    [Fact]
    public void Unmapped_column_is_ignored()
    {
        var columnWithUnknownType = new DatabaseColumn
        {
            Table = Table,
            Name = "ColumnWithUnknownStoreType",
            StoreType = "unknown_type"
        };

        var dbModel = new DatabaseModel
        {
            Tables =
            {
                new DatabaseTable
                {
                    Database = Database,
                    Name = "Table",
                    Columns = { IdColumn, columnWithUnknownType },
                    PrimaryKey = IdPrimaryKey
                }
            }
        };

        var model = _factory.Create(dbModel, new ModelReverseEngineerOptions());

        var columns = model.FindEntityType("Table")!.GetProperties().ToList();

        Assert.Single(columns);
    }

    [Fact]
    public void Column_and_table_comments()
    {
        var database = new DatabaseModel
        {
            Tables =
            {
                new DatabaseTable
                {
                    Database = Database,
                    Name = "Table",
                    Comment = "A table",
                    Columns =
                    {
                        IdColumn,
                        new DatabaseColumn
                        {
                            Table = Table,
                            Name = "Column",
                            StoreType = "int",
                            Comment = "An int column"
                        }
                    }
                }
            }
        };

        var model = _factory.Create(database, new ModelReverseEngineerOptions());

        var table = model.FindEntityType("Table")!;
        Assert.Equal("A table", table.GetComment());

        var column = model.FindEntityType("Table")!.GetProperty("Column");
        Assert.Equal("An int column", column.GetComment());
    }

    [Fact]
    public void Database_collation()
    {
        var database = new DatabaseModel { Collation = "SomeDatabaseCollation" };

        var model = _factory.Create(database, new ModelReverseEngineerOptions());
        Assert.Equal("SomeDatabaseCollation", model.GetCollation());
    }

    [Fact]
    public void Column_collation()
    {
        var database = new DatabaseModel
        {
            Tables =
            {
                new DatabaseTable
                {
                    Database = Database,
                    Name = "Table",
                    Columns =
                    {
                        IdColumn,
                        new DatabaseColumn
                        {
                            Table = Table,
                            Name = "Column",
                            StoreType = "int",
                            Collation = "SomeColumnCollation"
                        }
                    }
                }
            }
        };

        var model = _factory.Create(database, new ModelReverseEngineerOptions());

        var column = model.FindEntityType("Table")!.GetProperty("Column");
        Assert.Equal("SomeColumnCollation", column.GetCollation());
    }

    [Theory, InlineData(false, false, false), InlineData(false, false, true), InlineData(false, true, false),
     InlineData(false, true, true), InlineData(true, false, false), InlineData(true, false, true), InlineData(true, true, false),
     InlineData(true, true, true)]
    public void UseDatabaseNames_and_NoPluralize_work_together(
        bool useDatabaseNames,
        bool noPluralize,
        bool pluralTables)
    {
        var userTableName = pluralTables ? "users" : "user";
        var postTableName = pluralTables ? "posts" : "post";
        var databaseModel = new DatabaseModel
        {
            Tables =
            {
                new DatabaseTable
                {
                    Name = userTableName,
                    Columns = { new DatabaseColumn { Name = "id", StoreType = "int" } },
                    PrimaryKey = new DatabasePrimaryKey { Columns = { new DatabaseColumnRef("id") } }
                },
                new DatabaseTable
                {
                    Name = postTableName,
                    Columns =
                    {
                        new DatabaseColumn { Name = "id", StoreType = "int" },
                        new DatabaseColumn { Name = "author_id", StoreType = "int" }
                    },
                    PrimaryKey = new DatabasePrimaryKey { Columns = { new DatabaseColumnRef("id") } },
                    ForeignKeys =
                    {
                        new DatabaseForeignKey
                        {
                            PrincipalTable = new DatabaseTableRef(userTableName),
                            Columns = { new DatabaseColumnRef("author_id") },
                            PrincipalColumns = { new DatabaseColumnRef("id") }
                        }
                    }
                }
            }
        };

        var model = _factory.Create(
            databaseModel,
            new ModelReverseEngineerOptions { UseDatabaseNames = useDatabaseNames, NoPluralize = noPluralize });

        var user = Assert.Single(model.GetEntityTypes(), e => e.GetTableName() == userTableName);
        var id = Assert.Single(user.GetProperties(), p => p.GetColumnName() == "id");
        var foreignKey = Assert.Single(user.GetReferencingForeignKeys());
        if (useDatabaseNames && noPluralize)
        {
            Assert.Equal(userTableName, user.Name);
            Assert.Equal(userTableName, user[ScaffoldingAnnotationNames.DbSetName]);
            Assert.Equal("id", id.Name);
            Assert.Equal(postTableName, foreignKey.PrincipalToDependent!.Name);
            Assert.Equal("author_id", Assert.Single(foreignKey.Properties).Name);
            Assert.Equal("author", foreignKey.DependentToPrincipal!.Name);
        }
        else if (useDatabaseNames)
        {
            Assert.Equal("user", user.Name);
            Assert.Equal("users", user[ScaffoldingAnnotationNames.DbSetName]);
            Assert.Equal("id", id.Name);
            Assert.Equal("posts", foreignKey.PrincipalToDependent!.Name);
            Assert.Equal("author_id", Assert.Single(foreignKey.Properties).Name);
            Assert.Equal("author", foreignKey.DependentToPrincipal!.Name);
        }
        else if (noPluralize)
        {
            if (pluralTables)
            {
                Assert.Equal("Users", user.Name);
                Assert.Equal("Users", user[ScaffoldingAnnotationNames.DbSetName]);
                Assert.Equal("Id", id.Name);
                Assert.Equal("Posts", foreignKey.PrincipalToDependent!.Name);
                Assert.Equal("AuthorId", Assert.Single(foreignKey.Properties).Name);
                Assert.Equal("Author", foreignKey.DependentToPrincipal!.Name);
            }
            else
            {
                Assert.Equal("User", user.Name);
                Assert.Equal("User", user[ScaffoldingAnnotationNames.DbSetName]);
                Assert.Equal("Id", id.Name);
                Assert.Equal("Post", foreignKey.PrincipalToDependent!.Name);
                Assert.Equal("AuthorId", Assert.Single(foreignKey.Properties).Name);
                Assert.Equal("Author", foreignKey.DependentToPrincipal!.Name);
            }
        }
        else
        {
            Assert.Equal("User", user.Name);
            Assert.Equal("Users", user[ScaffoldingAnnotationNames.DbSetName]);
            Assert.Equal("Id", id.Name);
            Assert.Equal("Posts", foreignKey.PrincipalToDependent!.Name);
            Assert.Equal("AuthorId", Assert.Single(foreignKey.Properties).Name);
            Assert.Equal("Author", foreignKey.DependentToPrincipal!.Name);
        }
    }

    [Fact]
    public void Scaffold_skip_navigation_for_many_to_many_join_table_ef6()
    {
        var database = new DatabaseModel
        {
            Tables =
            {
                new DatabaseTable
                {
                    Name = "Blogs",
                    Columns = { new DatabaseColumn { Name = "Id", StoreType = "int" } },
                    PrimaryKey = new DatabasePrimaryKey { Columns = { new DatabaseColumnRef("Id") } }
                },
                new DatabaseTable
                {
                    Name = "Posts",
                    Columns = { new DatabaseColumn { Name = "Id", StoreType = "int" } },
                    PrimaryKey = new DatabasePrimaryKey { Columns = { new DatabaseColumnRef("Id") } }
                },
                new DatabaseTable
                {
                    Name = "PostBlogs",
                    Columns =
                    {
                        new DatabaseColumn { Name = "Post_Id", StoreType = "int" },
                        new DatabaseColumn { Name = "Blog_Id", StoreType = "int" }
                    },
                    PrimaryKey =
                        new DatabasePrimaryKey { Columns = { new DatabaseColumnRef("Post_Id"), new DatabaseColumnRef("Blog_Id") } },
                    ForeignKeys =
                    {
                        new DatabaseForeignKey
                        {
                            Name = "Post_Blogs_Source",
                            Columns = { new DatabaseColumnRef("Post_Id") },
                            PrincipalTable = new DatabaseTableRef("Posts"),
                            PrincipalColumns = { new DatabaseColumnRef("Id") },
                            OnDelete = ReferentialAction.Cascade
                        },
                        new DatabaseForeignKey
                        {
                            Name = "Post_Blogs_Target",
                            Columns = { new DatabaseColumnRef("Blog_Id") },
                            PrincipalTable = new DatabaseTableRef("Blogs"),
                            PrincipalColumns = { new DatabaseColumnRef("Id") },
                            OnDelete = ReferentialAction.Cascade
                        }
                    }
                }
            }
        };

        var model = _factory.Create(database, new ModelReverseEngineerOptions());

        Assert.Collection(
            model.GetEntityTypes().OrderBy(e => e.Name),
            t1 =>
            {
                Assert.Equal("Blog", t1.Name);
                Assert.Equal("Blogs", t1.GetTableName());
                Assert.Empty(t1.GetDeclaredForeignKeys());
                var skipNavigation = Assert.Single(t1.GetSkipNavigations());
                Assert.Equal("Posts", skipNavigation.Name);
                Assert.Equal("Blogs", skipNavigation.Inverse.Name);
                Assert.Equal("PostBlog", skipNavigation.JoinEntityType.Name);
                Assert.Equal("Post_Blogs_Target", skipNavigation.ForeignKey.GetConstraintName());
            },
            t2 =>
            {
                Assert.Equal("Post", t2.Name);
                Assert.Equal("Posts", t2.GetTableName());
                Assert.Empty(t2.GetDeclaredForeignKeys());
                var skipNavigation = Assert.Single(t2.GetSkipNavigations());
                Assert.Equal("Blogs", skipNavigation.Name);
                Assert.Equal("Posts", skipNavigation.Inverse.Name);
                Assert.Equal("PostBlog", skipNavigation.JoinEntityType.Name);
                Assert.Equal("Post_Blogs_Source", skipNavigation.ForeignKey.GetConstraintName());
            },
            t3 =>
            {
                Assert.Equal("PostBlog", t3.Name);
                Assert.Equal("PostBlogs", t3.GetTableName());
                Assert.Collection(
                    t3.GetForeignKeys().OrderBy(fk => fk.GetConstraintName()),
                    fk1 =>
                    {
                        Assert.Equal("Post_Blogs_Source", fk1.GetConstraintName());
                        var property = Assert.Single(fk1.Properties);
                        Assert.Equal("PostId", property.Name);
                        Assert.Equal("Post_Id", property.GetColumnName(StoreObjectIdentifier.Table(t3.GetTableName()!)));
                        Assert.Equal("Post", fk1.PrincipalEntityType.Name);
                        Assert.Equal(DeleteBehavior.Cascade, fk1.DeleteBehavior);
                    },
                    fk2 =>
                    {
                        Assert.Equal("Post_Blogs_Target", fk2.GetConstraintName());
                        var property = Assert.Single(fk2.Properties);
                        Assert.Equal("BlogId", property.Name);
                        Assert.Equal("Blog_Id", property.GetColumnName(StoreObjectIdentifier.Table(t3.GetTableName()!)));
                        Assert.Equal("Blog", fk2.PrincipalEntityType.Name);
                        Assert.Equal(DeleteBehavior.Cascade, fk2.DeleteBehavior);
                    });
            });
    }

    [Fact]
    public void Scaffold_skip_navigation_for_many_to_many_join_table_basic()
    {
        var database = new DatabaseModel
        {
            Tables =
            {
                new DatabaseTable
                {
                    Name = "Blogs",
                    Columns = { new DatabaseColumn { Name = "Id", StoreType = "int" } },
                    PrimaryKey = new DatabasePrimaryKey { Columns = { new DatabaseColumnRef("Id") } }
                },
                new DatabaseTable
                {
                    Name = "Posts",
                    Columns = { new DatabaseColumn { Name = "Id", StoreType = "int" } },
                    PrimaryKey = new DatabasePrimaryKey { Columns = { new DatabaseColumnRef("Id") } }
                },
                new DatabaseTable
                {
                    Name = "BlogPosts",
                    Columns =
                    {
                        new DatabaseColumn { Name = "BlogId", StoreType = "int" },
                        new DatabaseColumn { Name = "PostId", StoreType = "int" }
                    },
                    PrimaryKey =
                        new DatabasePrimaryKey { Columns = { new DatabaseColumnRef("BlogId"), new DatabaseColumnRef("PostId") } },
                    ForeignKeys =
                    {
                        new DatabaseForeignKey
                        {
                            Columns = { new DatabaseColumnRef("BlogId") },
                            PrincipalColumns = { new DatabaseColumnRef("Id") },
                            PrincipalTable = new DatabaseTableRef("Blogs"),
                        },
                        new DatabaseForeignKey
                        {
                            Columns = { new DatabaseColumnRef("PostId") },
                            PrincipalColumns = { new DatabaseColumnRef("Id") },
                            PrincipalTable = new DatabaseTableRef("Posts"),
                        }
                    }
                }
            }
        };

        var model = _factory.Create(database, new ModelReverseEngineerOptions());

        Assert.Collection(
            model.GetEntityTypes().OrderBy(e => e.Name),
            t1 =>
            {
                Assert.Empty(t1.GetNavigations());
                var skipNavigation = Assert.Single(t1.GetSkipNavigations());
                Assert.Equal("Posts", skipNavigation.Name);
                Assert.Equal("Blogs", skipNavigation.Inverse.Name);
            },
            t2 =>
            {
                Assert.Empty(t2.GetNavigations());
                Assert.Equal(2, t2.GetForeignKeys().Count());
            },
            t3 =>
            {
                Assert.Empty(t3.GetNavigations());
                var skipNavigation = Assert.Single(t3.GetSkipNavigations());
                Assert.Equal("Blogs", skipNavigation.Name);
                Assert.Equal("Posts", skipNavigation.Inverse.Name);
            });
    }

    [Fact]
    public void Scaffold_skip_navigation_for_many_to_many_join_table_unique_constraint()
    {
        var database = new DatabaseModel
        {
            Tables =
            {
                new DatabaseTable
                {
                    Name = "Blogs",
                    Columns =
                    {
                        new DatabaseColumn { Name = "Id", StoreType = "int" },
                        new DatabaseColumn { Name = "Key", StoreType = "int" }
                    },
                    PrimaryKey = new DatabasePrimaryKey { Columns = { new DatabaseColumnRef("Id") } },
                    UniqueConstraints = { new DatabaseUniqueConstraint { Columns = { new DatabaseColumnRef("Key") } } }
                },
                new DatabaseTable
                {
                    Name = "Posts",
                    Columns = { new DatabaseColumn { Name = "Id", StoreType = "int" } },
                    PrimaryKey = new DatabasePrimaryKey { Columns = { new DatabaseColumnRef("Id") } }
                },
                new DatabaseTable
                {
                    Name = "BlogPosts",
                    Columns =
                    {
                        new DatabaseColumn { Name = "BlogKey", StoreType = "int" },
                        new DatabaseColumn { Name = "PostId", StoreType = "int" }
                    },
                    PrimaryKey =
                        new DatabasePrimaryKey { Columns = { new DatabaseColumnRef("BlogKey"), new DatabaseColumnRef("PostId") } },
                    ForeignKeys =
                    {
                        new DatabaseForeignKey
                        {
                            Columns = { new DatabaseColumnRef("BlogKey") },
                            PrincipalColumns = { new DatabaseColumnRef("Key") },
                            PrincipalTable = new DatabaseTableRef("Blogs"),
                        },
                        new DatabaseForeignKey
                        {
                            Columns = { new DatabaseColumnRef("PostId") },
                            PrincipalColumns = { new DatabaseColumnRef("Id") },
                            PrincipalTable = new DatabaseTableRef("Posts"),
                        }
                    }
                }
            }
        };

        var model = _factory.Create(database, new ModelReverseEngineerOptions());

        Assert.Collection(
            model.GetEntityTypes().OrderBy(e => e.Name),
            t1 =>
            {
                Assert.Empty(t1.GetNavigations());
                var skipNavigation = Assert.Single(t1.GetSkipNavigations());
                Assert.Equal("Posts", skipNavigation.Name);
                Assert.Equal("BlogKeys", skipNavigation.Inverse.Name);
            },
            t2 =>
            {
                Assert.Empty(t2.GetNavigations());
                Assert.Equal(2, t2.GetForeignKeys().Count());
                var fk = Assert.Single(t2.FindDeclaredForeignKeys([t2.GetProperty("BlogKey")]));
                Assert.False(fk.PrincipalKey.IsPrimaryKey());
            },
            t3 =>
            {
                Assert.Empty(t3.GetNavigations());
                var skipNavigation = Assert.Single(t3.GetSkipNavigations());
                Assert.Equal("BlogKeys", skipNavigation.Name);
                Assert.Equal("Posts", skipNavigation.Inverse.Name);
            });
    }

    [Fact]
    public void Scaffold_skip_navigation_for_many_to_many_join_table_self_ref()
    {
        var database = new DatabaseModel
        {
            Tables =
            {
                new DatabaseTable
                {
                    Name = "Products",
                    Columns = { new DatabaseColumn { Name = "Id", StoreType = "int" } },
                    PrimaryKey = new DatabasePrimaryKey { Columns = { new DatabaseColumnRef("Id") } }
                },
                new DatabaseTable
                {
                    Name = "RelatedProducts",
                    Columns =
                    {
                        new DatabaseColumn { Name = "Id", StoreType = "int" },
                        new DatabaseColumn { Name = "ProductId", StoreType = "int" }
                    },
                    PrimaryKey =
                        new DatabasePrimaryKey { Columns = { new DatabaseColumnRef("Id"), new DatabaseColumnRef("ProductId") } },
                    ForeignKeys =
                    {
                        new DatabaseForeignKey
                        {
                            Columns = { new DatabaseColumnRef("Id") },
                            PrincipalColumns = { new DatabaseColumnRef("Id") },
                            PrincipalTable = new DatabaseTableRef("Products"),
                        },
                        new DatabaseForeignKey
                        {
                            Columns = { new DatabaseColumnRef("ProductId") },
                            PrincipalColumns = { new DatabaseColumnRef("Id") },
                            PrincipalTable = new DatabaseTableRef("Products"),
                        }
                    }
                },
                new DatabaseTable
                {
                    Name = "SubProducts",
                    Columns =
                    {
                        new DatabaseColumn { Name = "Id", StoreType = "int" },
                        new DatabaseColumn { Name = "ProductId", StoreType = "int" }
                    },
                    PrimaryKey =
                        new DatabasePrimaryKey { Columns = { new DatabaseColumnRef("Id"), new DatabaseColumnRef("ProductId") } },
                    ForeignKeys =
                    {
                        new DatabaseForeignKey
                        {
                            Columns = { new DatabaseColumnRef("Id") },
                            PrincipalColumns = { new DatabaseColumnRef("Id") },
                            PrincipalTable = new DatabaseTableRef("Products"),
                        },
                        new DatabaseForeignKey
                        {
                            Columns = { new DatabaseColumnRef("ProductId") },
                            PrincipalColumns = { new DatabaseColumnRef("Id") },
                            PrincipalTable = new DatabaseTableRef("Products"),
                        }
                    }
                }
            }
        };

        var model = _factory.Create(database, new ModelReverseEngineerOptions());

        Assert.Collection(
            model.GetEntityTypes().OrderBy(e => e.Name),
            t1 =>
            {
                Assert.Empty(t1.GetNavigations());
                Assert.Collection(
                    t1.GetSkipNavigations(),
                    s => Assert.Equal("Ids", s.Name),
                    s => Assert.Equal("IdsNavigation", s.Name),
                    s => Assert.Equal("Products", s.Name),
                    s => Assert.Equal("ProductsNavigation", s.Name));
            },
            t2 =>
            {
                Assert.Empty(t2.GetNavigations());
                Assert.Equal(2, t2.GetForeignKeys().Count());
            },
            t2 =>
            {
                Assert.Empty(t2.GetNavigations());
                Assert.Equal(2, t2.GetForeignKeys().Count());
            });
    }

    [Fact]
    public void Scaffold_skip_navigation_for_many_to_many_join_table_self_ref_with_colliding_navigation_names()
    {
        var database = new DatabaseModel
        {
            Tables =
            {
                new DatabaseTable
                {
                    Name = "Products",
                    Columns = { new DatabaseColumn { Name = "Id", StoreType = "int" } },
                    PrimaryKey = new DatabasePrimaryKey { Columns = { new DatabaseColumnRef("Id") } }
                },
                new DatabaseTable
                {
                    Name = "RelatedProducts",
                    Columns =
                    {
                        new DatabaseColumn { Name = "Product_Id", StoreType = "int" },
                        new DatabaseColumn { Name = "ProductId", StoreType = "int" }
                    },
                    PrimaryKey = new DatabasePrimaryKey
                    {
                        Columns = { new DatabaseColumnRef("Product_Id"), new DatabaseColumnRef("ProductId") }
                    },
                    ForeignKeys =
                    {
                        new DatabaseForeignKey
                        {
                            Columns = { new DatabaseColumnRef("Product_Id") },
                            PrincipalColumns = { new DatabaseColumnRef("Id") },
                            PrincipalTable = new DatabaseTableRef("Products"),
                        },
                        new DatabaseForeignKey
                        {
                            Columns = { new DatabaseColumnRef("ProductId") },
                            PrincipalColumns = { new DatabaseColumnRef("Id") },
                            PrincipalTable = new DatabaseTableRef("Products"),
                        }
                    }
                }
            }
        };

        var model = _factory.Create(database, new ModelReverseEngineerOptions());

        var product = model.GetEntityTypes().Single(e => e.Name == "Product");
        var skipNavigationNames = product.GetSkipNavigations().Select(n => n.Name).ToList();

        Assert.Equal(2, skipNavigationNames.Count);
        Assert.Equal(skipNavigationNames.Count, skipNavigationNames.Distinct().Count());
    }

    [Fact]
    public void Scaffold_skip_navigation_for_many_to_many_join_table_composite_fk()
    {
        var database = new DatabaseModel
        {
            Tables =
            {
                new DatabaseTable
                {
                    Name = "AnnualValue",
                    Columns =
                    {
                        new DatabaseColumn { Name = "LearnAimRef", StoreType = "varchar(8)" },
                        new DatabaseColumn { Name = "EffectiveFrom", StoreType = "date" }
                    },
                    PrimaryKey =
                        new DatabasePrimaryKey
                        {
                            Columns = { new DatabaseColumnRef("LearnAimRef"), new DatabaseColumnRef("EffectiveFrom") }
                        }
                },
                new DatabaseTable
                {
                    Name = "AcademicYear_Lookup",
                    Columns =
                    {
                        new DatabaseColumn { Name = "AcademicYear", StoreType = "varchar(4)" },
                        new DatabaseColumn
                        {
                            Name = "AcademicYearDesc",
                            StoreType = "varchar(150)",
                            IsNullable = true
                        },
                        new DatabaseColumn
                        {
                            Name = "AcademicYearDesc2",
                            StoreType = "varchar(100)",
                            IsNullable = true
                        }
                    },
                    PrimaryKey = new DatabasePrimaryKey { Columns = { new DatabaseColumnRef("AcademicYear") } }
                },
                new DatabaseTable
                {
                    Name = "AnnualValue_AcademicYear_Mapping",
                    Columns =
                    {
                        new DatabaseColumn { Name = "AcademicYear", StoreType = "varchar(4)" },
                        new DatabaseColumn { Name = "LearnAimRef", StoreType = "varchar(8)" },
                        new DatabaseColumn { Name = "EffectiveFrom", StoreType = "date" }
                    },
                    PrimaryKey =
                        new DatabasePrimaryKey
                        {
                            Columns =
                            {
                                new DatabaseColumnRef("AcademicYear"),
                                new DatabaseColumnRef("LearnAimRef"),
                                new DatabaseColumnRef("EffectiveFrom")
                            }
                        },
                    ForeignKeys =
                    {
                        new DatabaseForeignKey
                        {
                            Columns = { new DatabaseColumnRef("LearnAimRef"), new DatabaseColumnRef("EffectiveFrom") },
                            PrincipalColumns = { new DatabaseColumnRef("LearnAimRef"), new DatabaseColumnRef("EffectiveFrom") },
                            PrincipalTable = new DatabaseTableRef("AnnualValue"),
                            OnDelete = ReferentialAction.Cascade
                        },
                        new DatabaseForeignKey
                        {
                            Columns = { new DatabaseColumnRef("AcademicYear") },
                            PrincipalColumns = { new DatabaseColumnRef("AcademicYear") },
                            PrincipalTable = new DatabaseTableRef("AcademicYear_Lookup"),
                            OnDelete = ReferentialAction.Cascade
                        }
                    }
                }
            }
        };

        var model = _factory.Create(database, new ModelReverseEngineerOptions());

        Assert.Collection(
            model.GetEntityTypes().OrderBy(e => e.Name),
            t1 =>
            {
                // AcademicYearLookup
                Assert.Equal("AcademicYearLookup", t1.Name);
                Assert.Collection(
                    t1.GetProperties().Select(p => p.Name).OrderBy(n => n),
                    p => Assert.Equal("AcademicYear", p),
                    p => Assert.Equal("AcademicYearDesc", p),
                    p => Assert.Equal("AcademicYearDesc2", p));
                Assert.Empty(t1.GetNavigations());
                var skipNavigation = Assert.Single(t1.GetSkipNavigations());
                Assert.Equal("AnnualValues", skipNavigation.Name);
            },
            t2 =>
            {
                // AnnualValue
                Assert.Equal("AnnualValue", t2.Name);
                Assert.Collection(
                    t2.GetProperties().Select(p => p.Name).OrderBy(n => n),
                    p => Assert.Equal("EffectiveFrom", p),
                    p => Assert.Equal("LearnAimRef", p));
                Assert.Empty(t2.GetNavigations());
                var skipNavigation = Assert.Single(t2.GetSkipNavigations());
                Assert.Equal("AcademicYears", skipNavigation.Name);
            },
            t3 =>
            {
                // AnnualValueAcademicYearMapping (join table)
                Assert.Equal("AnnualValueAcademicYearMapping", t3.Name);
                Assert.Collection(
                    t3.GetProperties().Select(p => p.Name).OrderBy(n => n),
                    p => Assert.Equal("AcademicYear", p),
                    p => Assert.Equal("EffectiveFrom", p),
                    p => Assert.Equal("LearnAimRef", p));
                Assert.Empty(t3.GetNavigations());
                Assert.Equal(2, t3.GetForeignKeys().Count());
            });
    }

    [Fact]
    public void Fk_property_ending_in_guid_navigation_name()
    {
        var blogTable = new DatabaseTable
        {
            Database = Database,
            Name = "Blog",
            Columns = { IdColumn },
            PrimaryKey = IdPrimaryKey
        };
        var postTable = new DatabaseTable
        {
            Database = Database,
            Name = "Post",
            Columns =
            {
                IdColumn,
                new DatabaseColumn
                {
                    Table = Table,
                    Name = "BlogGuid",
                    StoreType = "int",
                    IsNullable = true
                }
            },
            PrimaryKey = IdPrimaryKey
        };

        postTable.ForeignKeys.Add(
            new DatabaseForeignKey
            {
                Table = postTable,
                Name = "FK_Foo",
                Columns = { postTable.Columns.ElementAt(1) },
                PrincipalTable = blogTable,
                PrincipalColumns = { blogTable.Columns.ElementAt(0) },
                OnDelete = ReferentialAction.Cascade
            });

        var info = new DatabaseModel { Tables = { blogTable, postTable } };

        var model = _factory.Create(info, new ModelReverseEngineerOptions());

        Assert.Collection(
            model.GetEntityTypes().OrderBy(t => t.Name).Cast<EntityType>(),
            entity =>
            {
                Assert.Equal("Blog", entity.Name);
                Assert.Equal("Posts", entity.GetNavigations().Single().Name);
            },
            entity =>
            {
                Assert.Equal("Post", entity.Name);
                Assert.Equal("Blog", entity.GetNavigations().Single().Name);
            }
        );
    }

    [Fact]
    public void Composite_fk_property_ending_in_guid_navigation_name()
    {
        var blogTable = new DatabaseTable
        {
            Database = Database,
            Name = "Blog",
            Columns =
            {
                IdColumn,
                new DatabaseColumn
                {
                    Table = Table,
                    Name = "BlogGuid1",
                    StoreType = "int",
                    IsNullable = false
                },
                new DatabaseColumn
                {
                    Table = Table,
                    Name = "BlogGuid2",
                    StoreType = "int",
                    IsNullable = false
                }
            },
            PrimaryKey = IdPrimaryKey
        };
        var postTable = new DatabaseTable
        {
            Database = Database,
            Name = "Post",
            Columns =
            {
                IdColumn,
                new DatabaseColumn
                {
                    Table = Table,
                    Name = "BlogGuid1",
                    StoreType = "int",
                    IsNullable = true
                },
                new DatabaseColumn
                {
                    Table = Table,
                    Name = "BlogGuid2",
                    StoreType = "int",
                    IsNullable = true
                }
            },
            PrimaryKey = IdPrimaryKey
        };

        blogTable.UniqueConstraints.Add(
            new DatabaseUniqueConstraint
            {
                Table = blogTable,
                Name = "AK_Foo",
                Columns = { blogTable.Columns.ElementAt(1), blogTable.Columns.ElementAt(2) }
            });

        postTable.ForeignKeys.Add(
            new DatabaseForeignKey
            {
                Table = postTable,
                Name = "FK_Foo",
                Columns = { postTable.Columns.ElementAt(1), postTable.Columns.ElementAt(2) },
                PrincipalTable = blogTable,
                PrincipalColumns = { blogTable.Columns.ElementAt(1), blogTable.Columns.ElementAt(2) },
                OnDelete = ReferentialAction.Cascade
            });

        var info = new DatabaseModel { Tables = { blogTable, postTable } };

        var model = _factory.Create(info, new ModelReverseEngineerOptions());

        Assert.Collection(
            model.GetEntityTypes().OrderBy(t => t.Name).Cast<EntityType>(),
            entity =>
            {
                Assert.Equal("Blog", entity.Name);
                Assert.Equal("Posts", entity.GetNavigations().Single().Name);
            },
            entity =>
            {
                Assert.Equal("Post", entity.Name);
                Assert.Equal("Blog", entity.GetNavigations().Single().Name);
            }
        );
    }

    [Fact]
    public void Unusual_navigation_name() // Issue #14278
    {
        var bookDetailsTable = new DatabaseTable { Database = Database, Name = "Book_Details" };

        bookDetailsTable.Columns.Add(
            new DatabaseColumn
            {
                Table = bookDetailsTable,
                Name = "ID",
                StoreType = "int"
            });

        bookDetailsTable.Columns.Add(
            new DatabaseColumn
            {
                Table = bookDetailsTable,
                Name = "Book_Name",
                StoreType = "nvarchar(50)"
            });

        bookDetailsTable.Columns.Add(
            new DatabaseColumn
            {
                Table = bookDetailsTable,
                Name = "Student_Id",
                StoreType = "int"
            });

        bookDetailsTable.PrimaryKey = new DatabasePrimaryKey
        {
            Table = bookDetailsTable,
            Name = "PK_Book_Details",
            Columns = { bookDetailsTable.Columns.Single(c => c.Name == "ID") }
        };

        var studentDetailsTable = new DatabaseTable { Database = Database, Name = "Student_Details" };

        studentDetailsTable.Columns.Add(
            new DatabaseColumn
            {
                Table = studentDetailsTable,
                Name = "ID",
                StoreType = "int"
            });

        studentDetailsTable.Columns.Add(
            new DatabaseColumn
            {
                Table = studentDetailsTable,
                Name = "Student_Name",
                StoreType = "nvarchar(256)"
            });

        studentDetailsTable.PrimaryKey = new DatabasePrimaryKey
        {
            Table = studentDetailsTable,
            Name = "PK_Student_Details",
            Columns = { studentDetailsTable.Columns.Single(c => c.Name == "ID") }
        };

        bookDetailsTable.ForeignKeys.Add(
            new DatabaseForeignKey
            {
                Table = bookDetailsTable,
                Name = "FK_Foo",
                Columns = { bookDetailsTable.Columns.Single(c => c.Name == "Student_Id") },
                PrincipalTable = studentDetailsTable,
                PrincipalColumns = { studentDetailsTable.Columns.Single(c => c.Name == "ID") },
                OnDelete = ReferentialAction.Cascade
            });

        var info = new DatabaseModel { Tables = { bookDetailsTable, studentDetailsTable } };

        var model = _factory.Create(info, new ModelReverseEngineerOptions());

        Assert.Collection(
            model.GetEntityTypes().OrderBy(t => t.Name).Cast<EntityType>(),
            entity =>
            {
                Assert.Equal("BookDetail", entity.Name);
                Assert.Equal("Student", entity.GetNavigations().Single().Name);
            },
            entity =>
            {
                Assert.Equal("StudentDetail", entity.Name);
                Assert.Equal("BookDetails", entity.GetNavigations().Single().Name);
            }
        );

        model = _factory.Create(info, new ModelReverseEngineerOptions { UseDatabaseNames = true });

        Assert.Collection(
            model.GetEntityTypes().OrderBy(t => t.Name).Cast<EntityType>(),
            entity =>
            {
                Assert.Equal("Book_Detail", entity.Name);
                Assert.Equal("Student", entity.GetNavigations().Single().Name);
            },
            entity =>
            {
                Assert.Equal("Student_Detail", entity.Name);
                Assert.Equal("Book_Details", entity.GetNavigations().Single().Name);
            }
        );
    }

    [Fact]
    public void Interesting_navigation_name() // Issue #27832
    {
        var seasonTable = new DatabaseTable { Database = Database, Name = "TmTvSeason" };

        seasonTable.Columns.Add(
            new DatabaseColumn
            {
                Table = seasonTable,
                Name = "Id",
                StoreType = "int"
            });

        seasonTable.Columns.Add(
            new DatabaseColumn
            {
                Table = seasonTable,
                Name = "ShowId",
                StoreType = "int"
            });

        seasonTable.Columns.Add(
            new DatabaseColumn
            {
                Table = seasonTable,
                Name = "Name",
                StoreType = "nvarchar(300)"
            });

        seasonTable.PrimaryKey = new DatabasePrimaryKey
        {
            Table = seasonTable,
            Name = "PK_TmTvSeason",
            Columns = { seasonTable.Columns.Single(c => c.Name == "ShowId"), seasonTable.Columns.Single(c => c.Name == "Id") }
        };

        var episodeTable = new DatabaseTable { Database = Database, Name = "TmTvEpisode" };

        episodeTable.Columns.Add(
            new DatabaseColumn
            {
                Table = episodeTable,
                Name = "Id",
                StoreType = "int"
            });

        episodeTable.Columns.Add(
            new DatabaseColumn
            {
                Table = episodeTable,
                Name = "SeasonId",
                StoreType = "int"
            });

        episodeTable.Columns.Add(
            new DatabaseColumn
            {
                Table = episodeTable,
                Name = "ShowId",
                StoreType = "int"
            });

        episodeTable.Columns.Add(
            new DatabaseColumn
            {
                Table = episodeTable,
                Name = "Name",
                StoreType = "nvarchar(300)"
            });

        episodeTable.PrimaryKey = new DatabasePrimaryKey
        {
            Table = episodeTable,
            Name = "PK_TmTvEpisode",
            Columns =
            {
                episodeTable.Columns.Single(c => c.Name == "ShowId"),
                episodeTable.Columns.Single(c => c.Name == "SeasonId"),
                episodeTable.Columns.Single(c => c.Name == "Id")
            }
        };

        episodeTable.ForeignKeys.Add(
            new DatabaseForeignKey
            {
                Table = episodeTable,
                Name = "FK_TmTvEpisode_TmTvSeason",
                Columns = { episodeTable.Columns.Single(c => c.Name == "ShowId"), episodeTable.Columns.Single(c => c.Name == "SeasonId") },
                PrincipalTable = seasonTable,
                PrincipalColumns = { seasonTable.Columns.Single(c => c.Name == "ShowId"), seasonTable.Columns.Single(c => c.Name == "Id") },
                OnDelete = ReferentialAction.Cascade
            });

        var info = new DatabaseModel { Tables = { seasonTable, episodeTable } };

        var model = _factory.Create(info, new ModelReverseEngineerOptions());
        AssertNavigations();

        model = _factory.Create(info, new ModelReverseEngineerOptions { UseDatabaseNames = true });
        AssertNavigations();

        void AssertNavigations()
            => Assert.Collection(
                model.GetEntityTypes().OrderBy(t => t.Name).Cast<EntityType>(),
                entity =>
                {
                    Assert.Equal("TmTvEpisode", entity.Name);
                    Assert.Equal("TmTvSeason", entity.GetNavigations().Single().Name);
                },
                entity =>
                {
                    Assert.Equal("TmTvSeason", entity.Name);
                    Assert.Equal("TmTvEpisodes", entity.GetNavigations().Single().Name);
                }
            );
    }

    [Fact]
    public void Navigation_name_from_composite_FK() // Issue #32685
    {
        var itemCategoryTable = new DatabaseTable { Database = Database, Name = "ItemCategory" };

        itemCategoryTable.Columns.Add(
            new DatabaseColumn
            {
                Table = itemCategoryTable,
                Name = "Name",
                StoreType = "VARCHAR (25)",
                IsNullable = false
            });

        itemCategoryTable.Columns.Add(
            new DatabaseColumn
            {
                Table = itemCategoryTable,
                Name = "Description",
                StoreType = "NVARCHAR (512)",
                IsNullable = true
            });

        itemCategoryTable.PrimaryKey = new DatabasePrimaryKey
        {
            Table = itemCategoryTable,
            Name = "PK_ItemCategory",
            Columns = { itemCategoryTable.Columns.Single(c => c.Name == "Name") }
        };

        var itemTable = new DatabaseTable { Database = Database, Name = "Item" };

        itemTable.Columns.Add(
            new DatabaseColumn
            {
                Table = itemTable,
                Name = "Name",
                StoreType = "VARCHAR (40)",
                IsNullable = false
            });

        itemTable.Columns.Add(
            new DatabaseColumn
            {
                Table = itemTable,
                Name = "Description",
                StoreType = "NVARCHAR (512)",
                IsNullable = true
            });

        itemTable.Columns.Add(
            new DatabaseColumn
            {
                Table = itemTable,
                Name = "CategoryName",
                StoreType = "VARCHAR (25)",
                IsNullable = false
            });

        itemTable.PrimaryKey = new DatabasePrimaryKey
        {
            Table = itemTable,
            Name = "PK_Item",
            Columns = { itemTable.Columns.Single(c => c.Name == "Name"), itemTable.Columns.Single(c => c.Name == "CategoryName") }
        };

        var someTable = new DatabaseTable { Database = Database, Name = "SomeTable" };

        someTable.Columns.Add(
            new DatabaseColumn
            {
                Table = someTable,
                Name = "Id",
                StoreType = "int",
                IsNullable = false
            });

        someTable.Columns.Add(
            new DatabaseColumn
            {
                Table = someTable,
                Name = "DetailItemName",
                StoreType = "VARCHAR (40)",
                IsNullable = false
            });

        someTable.Columns.Add(
            new DatabaseColumn
            {
                Table = someTable,
                Name = "DetailItemCategoryName",
                StoreType = "VARCHAR (25)",
                IsNullable = false
            });

        someTable.Columns.Add(
            new DatabaseColumn
            {
                Table = someTable,
                Name = "CategoryName",
                StoreType = "VARCHAR (25)",
                IsNullable = false
            });

        someTable.PrimaryKey = new DatabasePrimaryKey
        {
            Table = someTable,
            Name = "PK_SomeTable",
            Columns = { someTable.Columns.Single(c => c.Name == "Id") }
        };

        someTable.ForeignKeys.Add(
            new DatabaseForeignKey
            {
                Table = itemTable,
                Name = "FK_Item_ItemCategory",
                Columns = { someTable.Columns.Single(c => c.Name == "CategoryName") },
                PrincipalTable = itemCategoryTable,
                PrincipalColumns = { itemTable.Columns.Single(c => c.Name == "Name") },
            });

        someTable.ForeignKeys.Add(
            new DatabaseForeignKey
            {
                Table = someTable,
                Name = "FK_SomeTable_DetailItem",
                Columns =
                {
                    someTable.Columns.Single(c => c.Name == "DetailItemName"),
                    someTable.Columns.Single(c => c.Name == "DetailItemCategoryName")
                },
                PrincipalTable = itemTable,
                PrincipalColumns =
                {
                    itemTable.Columns.Single(c => c.Name == "Name"), itemTable.Columns.Single(c => c.Name == "CategoryName")
                },
            });

        var info = new DatabaseModel
        {
            Tables =
            {
                itemTable,
                someTable,
                itemCategoryTable
            }
        };

        var model = _factory.Create(info, new ModelReverseEngineerOptions());

        Assert.Collection(
            model.GetEntityTypes().OrderBy(t => t.Name).Cast<EntityType>(),
            entity =>
            {
                Assert.Equal("Item", entity.Name);
                Assert.Equal("SomeTables", entity.GetNavigations().Single().Name);
            },
            entity =>
            {
                Assert.Equal("ItemCategory", entity.Name);
                Assert.Equal("SomeTables", entity.GetNavigations().Single().Name);
            },
            entity =>
            {
                Assert.Equal("SomeTable", entity.Name);
                Assert.Collection(
                    entity.GetNavigations().OrderBy(t => t.Name),
                    navigation => Assert.Equal("CategoryNameNavigation", navigation.Name),
                    navigation => Assert.Equal("Item", navigation.Name));
            }
        );

        model = _factory.Create(info, new ModelReverseEngineerOptions { UseDatabaseNames = true });

        Assert.Collection(
            model.GetEntityTypes().OrderBy(t => t.Name).Cast<EntityType>(),
            entity =>
            {
                Assert.Equal("Item", entity.Name);
                Assert.Equal("SomeTables", entity.GetNavigations().Single().Name);
            },
            entity =>
            {
                Assert.Equal("ItemCategory", entity.Name);
                Assert.Equal("SomeTables", entity.GetNavigations().Single().Name);
            },
            entity =>
            {
                Assert.Equal("SomeTable", entity.Name);
                Assert.Collection(
                    entity.GetNavigations().OrderBy(t => t.Name),
                    navigation => Assert.Equal("CategoryNameNavigation", navigation.Name),
                    navigation => Assert.Equal("Item", navigation.Name));
            }
        );
    }

    [Fact]
    public void Computed_column_when_sql_unknown()
    {
        var database = new DatabaseModel
        {
            Tables =
            {
                new DatabaseTable
                {
                    Database = Database,
                    Name = "Table",
                    Columns =
                    {
                        IdColumn,
                        new DatabaseColumn
                        {
                            Table = Table,
                            Name = "Column",
                            StoreType = "int",
                            ComputedColumnSql = string.Empty
                        }
                    }
                }
            }
        };

        var model = _factory.Create(database, new ModelReverseEngineerOptions());

        var column = model.FindEntityType("Table")!.GetProperty("Column");
        Assert.Empty(column.GetComputedColumnSql()!);
    }
}
