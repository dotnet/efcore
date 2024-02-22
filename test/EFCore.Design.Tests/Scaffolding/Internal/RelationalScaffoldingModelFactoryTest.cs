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

    [ConditionalFact]
    public void Capitalize_DatabaseName()
    {
        var database = new DatabaseModel { DatabaseName = "northwind" };
        var model = _factory.Create(database, new ModelReverseEngineerOptions { UseDatabaseNames = false });
        Assert.Equal("Northwind", model.GetDatabaseName());
    }

    [ConditionalFact]
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

    [ConditionalFact]
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

    [ConditionalTheory]
    [InlineData("PascalCase")]
    [InlineData("camelCase")]
    [InlineData("snake-case")]
    [InlineData("MixedCASE")]
    [InlineData("separated_by_underscores")]
    [InlineData("PascalCase_withUnderscore")]
    [InlineData("ALL_CAPS")]
    [InlineData("numbers0Dont1Affect23Upper45Case678To9LowerCase10Boundary999")]
    [InlineData("We1!*~&%rdCh@r^act()0rs")]
    public void Get_DatabaseName(string expectedValue)
    {
        var options = new ModelReverseEngineerOptions { UseDatabaseNames = true };

        var database = new DatabaseModel { DatabaseName = expectedValue };
        var model = _factory.Create(database, options);
        Assert.Equal(expectedValue, model.GetDatabaseName());
    }

    [ConditionalFact]
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
            (EntityType)_factory.Create(info, new ModelReverseEngineerOptions { NoPluralize = true }).FindEntityType("Jobs");

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

    [ConditionalFact]
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
            .FindEntityType("NaturalProducts");

        Assert.Collection(
            entityType.GetProperties(),
            pk => Assert.Equal("Id", pk.Name),
            col1 => Assert.Equal("ProductSKU", col1.Name),
            col2 => Assert.Equal("Vendor_Discount", col2.Name),
            col3 => Assert.Equal("supplierID", col3.Name));
    }

    [ConditionalFact]
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
            .FindEntityType("NaturalProducts");

        Assert.Collection(
            entityType.GetProperties(),
            pk => Assert.Equal("Id", pk.Name),
            col1 => Assert.Equal("ProductSku", col1.Name),
            col2 => Assert.Equal("SupplierId", col2.Name),
            col3 => Assert.Equal("VendorDiscount", col3.Name));
    }

    [ConditionalTheory]
    [InlineData("nvarchar(450)", null)]
    [InlineData("datetime2(4)", null)]
    [InlineData("DateTime2(4)", "DateTime2(4)")]
    public void Column_type_annotation(string storeType, string expectedColumnType)
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

        var property = (Property)_factory.Create(info, new ModelReverseEngineerOptions()).FindEntityType("A").FindProperty("Col");

        Assert.Equal(expectedColumnType, property.GetConfiguredColumnType());
    }

    [ConditionalFact]
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

        var entityTypeA = _factory.Create(info, new ModelReverseEngineerOptions()).FindEntityType("A");
        var property1 = (Property)entityTypeA.FindProperty("Col1");
        var property2 = (Property)entityTypeA.FindProperty("Col2");
        var property3 = (Property)entityTypeA.FindProperty("Col3");

        Assert.Equal(0, property1.GetColumnOrder());
        Assert.Equal(1, property2.GetColumnOrder());
        Assert.Equal(2, property3.GetColumnOrder());
    }

    [ConditionalTheory]
    [InlineData("cheese")]
    [InlineData(null)]
    public void Unmappable_column_type(string StoreType)
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

        Assert.Single(_factory.Create(info, new ModelReverseEngineerOptions()).FindEntityType("E").GetProperties());

        var (level, message) = _reporter.Messages.Single();
        Assert.Equal(LogLevel.Warning, level);
        Assert.Equal(DesignStrings.CannotFindTypeMappingForColumn("E.Coli", StoreType), message);
    }

    [ConditionalTheory]
    [InlineData(new[] { "Id" }, 1)]
    [InlineData(new[] { "Id", "AltId" }, 2)]
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
        foreach (var column in keyProps.Select(
                     k => new DatabaseColumn
                     {
                         Table = Table,
                         Name = k,
                         StoreType = "int"
                     }))
        {
            info.Tables[0].Columns.Add(column);
            info.Tables[0].PrimaryKey.Columns.Add(column);
        }

        var model = (EntityType)_factory.Create(info, new ModelReverseEngineerOptions()).GetEntityTypes().Single();

        Assert.Equal("MyPk", model.FindPrimaryKey().GetName());
        Assert.Equal(keyProps, model.FindPrimaryKey().Properties.Select(p => p.GetColumnName()).ToArray());
    }

    [ConditionalFact]
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

    [ConditionalFact]
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

    [ConditionalFact]
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

    [ConditionalFact]
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

    [ConditionalFact]
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

        var parent = (EntityType)model.FindEntityType("Parent");

        var children = (EntityType)model.FindEntityType("Children");

        Assert.NotEmpty(parent.GetReferencingForeignKeys());
        var fk = Assert.Single(children.GetForeignKeys());
        Assert.False(fk.IsUnique);
        Assert.Equal(DeleteBehavior.Cascade, fk.DeleteBehavior);

        var principalKey = fk.PrincipalKey;

        Assert.Same(parent, principalKey.DeclaringEntityType);
        Assert.Same(parent.GetProperties().First(), principalKey.Properties[0]);
    }

    [ConditionalFact]
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

        var detail = model.FindEntityType("Detail");
        var foreignKey = Assert.Single(detail.GetForeignKeys());
        Assert.Equal("Master", foreignKey.DependentToPrincipal.Name);
        Assert.Null(foreignKey.PrincipalToDependent);
    }

    [ConditionalFact]
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

        var parent = (EntityType)model.FindEntityType("Parent");

        var children = (EntityType)model.FindEntityType("Children");

        Assert.NotEmpty(parent.GetReferencingForeignKeys());
        var fk = Assert.Single(children.GetForeignKeys());
        Assert.True(fk.IsUnique);
        Assert.Equal(DeleteBehavior.Cascade, fk.DeleteBehavior);

        var principalKey = fk.PrincipalKey;

        Assert.Same(parent, principalKey.DeclaringEntityType);
        Assert.Same(parent.GetProperties().First(p => p.Name == "Key"), principalKey.Properties[0]);
    }

    [ConditionalFact]
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

        var children = (EntityType)model.FindEntityType("Children");

        var fk = Assert.Single(children.GetForeignKeys());
        Assert.True(fk.IsUnique);
        Assert.Equal(DeleteBehavior.ClientSetNull, fk.DeleteBehavior);
    }

    [ConditionalFact]
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

        var parent = (EntityType)model.FindEntityType("Parent");

        var children = (EntityType)model.FindEntityType("Children");

        Assert.NotEmpty(parent.GetReferencingForeignKeys());

        var fk = Assert.Single(children.GetForeignKeys());
        Assert.False(fk.IsUnique);
        Assert.Equal(DeleteBehavior.SetNull, fk.DeleteBehavior);

        var principalKey = fk.PrincipalKey;

        Assert.Equal("Parent", principalKey.DeclaringEntityType.Name);
        Assert.Equal("IdA", principalKey.Properties[0].Name);
        Assert.Equal("IdB", principalKey.Properties[1].Name);
    }

    [ConditionalFact]
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
        var list = model.FindEntityType("ItemsList");

        Assert.NotEmpty(list.GetReferencingForeignKeys());
        Assert.NotEmpty(list.GetForeignKeys());

        var principalKey = list.FindForeignKeys(list.FindProperty("ParentId")).Single().PrincipalKey;
        Assert.Equal("ItemsList", principalKey.DeclaringEntityType.Name);
        Assert.Equal("Id", principalKey.Properties[0].Name);
    }

    [ConditionalFact]
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

    [ConditionalFact]
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

    [ConditionalFact]
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
            new ModelReverseEngineerOptions { NoPluralize = true }).FindEntityType("Friends");

        var buddyIdProperty = model.FindProperty("BuddyId");
        Assert.NotNull(buddyIdProperty);
        Assert.True(buddyIdProperty.IsNullable);

        var fk = Assert.Single(model.GetForeignKeys());

        Assert.True(fk.IsUnique);
        Assert.Empty(model.GetKeys().Where(k => !k.IsPrimaryKey()));
        Assert.Equal(model.FindPrimaryKey(), fk.PrincipalKey);
    }

    [ConditionalFact]
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
            new ModelReverseEngineerOptions { NoPluralize = true }).FindEntityType("Friends");

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

    [ConditionalFact]
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
        var parent = model.FindEntityType("Parent");
        var children = model.FindEntityType("Children");

        var fk = Assert.Single(children.GetForeignKeys());

        Assert.True(fk.IsUnique);
        Assert.Equal(parent.FindPrimaryKey(), fk.PrincipalKey);
    }

    [ConditionalFact]
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
        Assert.Equal(new[] { false, true, false }, mixedIndex.IsDescending);
    }

    [ConditionalFact]
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

    [ConditionalFact]
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
                    IncrementBy = 3
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
                Assert.True(first.IsCached);
                Assert.Null(first.CacheSize);
            });
    }

    [ConditionalFact]
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

    [ConditionalFact]
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

    [ConditionalFact]
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

    [ConditionalFact]
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

    [ConditionalFact]
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

    [ConditionalFact]
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

        var columns = model.FindEntityType("Table").GetProperties().ToList();

        Assert.Equal(typeof(bool), columns.First(c => c.Name == "NonNullBoolWithoutDefault").ClrType);
        Assert.False(columns.First(c => c.Name == "NonNullBoolWithoutDefault").IsNullable);
        Assert.Equal(typeof(bool?), columns.First(c => c.Name == "NonNullBoolWithDefault").ClrType);
        Assert.False(columns.First(c => c.Name == "NonNullBoolWithDefault").IsNullable);
        Assert.Equal("Default", columns.First(c => c.Name == "NonNullBoolWithDefault")[RelationalAnnotationNames.DefaultValueSql]);
    }

    [ConditionalFact]
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

    [ConditionalFact]
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

        var columns = model.FindEntityType("Table").GetProperties().ToList();

        Assert.Equal(typeof(bool?), columns.First(c => c.Name == "NullBoolWithDefault").ClrType);
        Assert.True(columns.First(c => c.Name == "NullBoolWithDefault").IsNullable);
        Assert.Equal("Default", columns.First(c => c.Name == "NullBoolWithDefault")[RelationalAnnotationNames.DefaultValueSql]);

        Assert.Empty(_reporter.Messages);
    }

    [ConditionalFact]
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

        Assert.Null(model.FindEntityType("Principal").FindProperty("PrimaryKey").GetConfiguredColumnType());
        Assert.Null(model.FindEntityType("Principal").FindProperty("AlternateKey").GetConfiguredColumnType());
        Assert.Null(model.FindEntityType("Principal").FindProperty("Index").GetConfiguredColumnType());
        Assert.Null(model.FindEntityType("Principal").FindProperty("Rowversion").GetConfiguredColumnType());
        Assert.Equal(typeof(Guid), model.FindEntityType("Principal").FindProperty("ClrType").ClrType);
        Assert.Null(model.FindEntityType("Dependent").FindProperty("BlogAlternateKey").GetConfiguredColumnType());
    }

    [ConditionalFact]
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

        var columns = model.FindEntityType("Table").GetProperties().ToList();

        Assert.Single(columns);
    }

    [ConditionalFact]
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

        var table = model.FindEntityType("Table");
        Assert.Equal("A table", table.GetComment());

        var column = model.FindEntityType("Table").GetProperty("Column");
        Assert.Equal("An int column", column.GetComment());
    }

    [ConditionalFact]
    public void Database_collation()
    {
        var database = new DatabaseModel { Collation = "SomeDatabaseCollation" };

        var model = _factory.Create(database, new ModelReverseEngineerOptions());
        Assert.Equal("SomeDatabaseCollation", model.GetCollation());
    }

    [ConditionalFact]
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

        var column = model.FindEntityType("Table").GetProperty("Column");
        Assert.Equal("SomeColumnCollation", column.GetCollation());
    }

    [ConditionalTheory]
    [InlineData(false, false, false)]
    [InlineData(false, false, true)]
    [InlineData(false, true, false)]
    [InlineData(false, true, true)]
    [InlineData(true, false, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    [InlineData(true, true, true)]
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

        var user = Assert.Single(model.GetEntityTypes().Where(e => e.GetTableName() == userTableName));
        var id = Assert.Single(user.GetProperties().Where(p => p.GetColumnName() == "id"));
        var foreignKey = Assert.Single(user.GetReferencingForeignKeys());
        if (useDatabaseNames && noPluralize)
        {
            Assert.Equal(userTableName, user.Name);
            Assert.Equal(userTableName, user[ScaffoldingAnnotationNames.DbSetName]);
            Assert.Equal("id", id.Name);
            Assert.Equal(postTableName, foreignKey.PrincipalToDependent.Name);
            Assert.Equal("author_id", Assert.Single(foreignKey.Properties).Name);
            Assert.Equal("author", foreignKey.DependentToPrincipal.Name);
        }
        else if (useDatabaseNames)
        {
            Assert.Equal("user", user.Name);
            Assert.Equal("users", user[ScaffoldingAnnotationNames.DbSetName]);
            Assert.Equal("id", id.Name);
            Assert.Equal("posts", foreignKey.PrincipalToDependent.Name);
            Assert.Equal("author_id", Assert.Single(foreignKey.Properties).Name);
            Assert.Equal("author", foreignKey.DependentToPrincipal.Name);
        }
        else if (noPluralize)
        {
            if (pluralTables)
            {
                Assert.Equal("Users", user.Name);
                Assert.Equal("Users", user[ScaffoldingAnnotationNames.DbSetName]);
                Assert.Equal("Id", id.Name);
                Assert.Equal("Posts", foreignKey.PrincipalToDependent.Name);
                Assert.Equal("AuthorId", Assert.Single(foreignKey.Properties).Name);
                Assert.Equal("Author", foreignKey.DependentToPrincipal.Name);
            }
            else
            {
                Assert.Equal("User", user.Name);
                Assert.Equal("User", user[ScaffoldingAnnotationNames.DbSetName]);
                Assert.Equal("Id", id.Name);
                Assert.Equal("Post", foreignKey.PrincipalToDependent.Name);
                Assert.Equal("AuthorId", Assert.Single(foreignKey.Properties).Name);
                Assert.Equal("Author", foreignKey.DependentToPrincipal.Name);
            }
        }
        else
        {
            Assert.Equal("User", user.Name);
            Assert.Equal("Users", user[ScaffoldingAnnotationNames.DbSetName]);
            Assert.Equal("Id", id.Name);
            Assert.Equal("Posts", foreignKey.PrincipalToDependent.Name);
            Assert.Equal("AuthorId", Assert.Single(foreignKey.Properties).Name);
            Assert.Equal("Author", foreignKey.DependentToPrincipal.Name);
        }
    }

    [ConditionalFact]
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
                        Assert.Equal("Post_Id", property.GetColumnName(StoreObjectIdentifier.Table(t3.GetTableName())));
                        Assert.Equal("Post", fk1.PrincipalEntityType.Name);
                        Assert.Equal(DeleteBehavior.Cascade, fk1.DeleteBehavior);
                    },
                    fk2 =>
                    {
                        Assert.Equal("Post_Blogs_Target", fk2.GetConstraintName());
                        var property = Assert.Single(fk2.Properties);
                        Assert.Equal("BlogId", property.Name);
                        Assert.Equal("Blog_Id", property.GetColumnName(StoreObjectIdentifier.Table(t3.GetTableName())));
                        Assert.Equal("Blog", fk2.PrincipalEntityType.Name);
                        Assert.Equal(DeleteBehavior.Cascade, fk2.DeleteBehavior);
                    });
            });
    }

    [ConditionalFact]
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

    [ConditionalFact]
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
                var fk = Assert.Single(t2.FindDeclaredForeignKeys(new[] { t2.GetProperty("BlogKey") }));
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

    [ConditionalFact]
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

    [ConditionalFact]
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

    [ConditionalFact]
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

    [ConditionalFact]
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

    [ConditionalFact]
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

    [ConditionalFact]
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
                Columns = { someTable.Columns.Single(c => c.Name == "DetailItemName"), someTable.Columns.Single(c => c.Name == "DetailItemCategoryName") },
                PrincipalTable = itemTable,
                PrincipalColumns = { itemTable.Columns.Single(c => c.Name == "Name"), itemTable.Columns.Single(c => c.Name == "CategoryName") },
            });

        var info = new DatabaseModel { Tables = { itemTable, someTable, itemCategoryTable } };

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

    [ConditionalFact]
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

        var column = model.FindEntityType("Table").GetProperty("Column");
        Assert.Empty(column.GetComputedColumnSql());
    }
}
