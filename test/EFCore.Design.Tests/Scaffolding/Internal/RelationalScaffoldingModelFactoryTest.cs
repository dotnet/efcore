// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Design.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Internal
{
    public class RelationalDatabaseModelFactoryTest
    {
        private readonly IScaffoldingModelFactory _factory;
        private readonly TestOperationReporter _reporter;

        private static readonly DatabaseModel Database;
        private static readonly DatabaseTable Table;
        private static readonly DatabaseColumn IdColumn;
        private static readonly DatabasePrimaryKey IdPrimaryKey;

        static RelationalDatabaseModelFactoryTest()
        {
            Database = new DatabaseModel();
            Table = new DatabaseTable(Database, "Foo");
            IdColumn = new DatabaseColumn(Table, "Id", "int");
            IdPrimaryKey = new DatabasePrimaryKey(Table, "IdPrimaryKey") { Columns = { IdColumn } };
        }

        public RelationalDatabaseModelFactoryTest()
        {
            _reporter = new TestOperationReporter();

            var services = new ServiceCollection()
                .AddEntityFrameworkDesignTimeServices(_reporter)
                .AddSingleton<IScaffoldingModelFactory, FakeScaffoldingModelFactory>();
            new SqlServerDesignTimeServices().ConfigureDesignTimeServices(services);

            _factory = services
                .BuildServiceProvider()
                .GetRequiredService<IScaffoldingModelFactory>();
        }

        [ConditionalFact]
        public void Creates_entity_types()
        {
            var info = new DatabaseModel
            {
                Tables =
                {
                    new DatabaseTable(Database, "tableWithSchema")
                    {
                        Schema = "public",
                        Columns = { IdColumn },
                        PrimaryKey = IdPrimaryKey
                    },
                    new DatabaseTable(Database, "noSchema")
                    {
                        Columns = { IdColumn },
                        PrimaryKey = IdPrimaryKey
                    },
                    new DatabaseTable(Database, "noPrimaryKey"),
                    new DatabaseView(Database, "view")
                }
            };
            var model = _factory.Create(info, false);
            Assert.Collection(
                model.GetEntityTypes().OrderBy(t => t.Name).Cast<EntityType>(),
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
                    Assert.NotNull(view.FindAnnotation(RelationalAnnotationNames.ViewDefinition));
                }
            );
            Assert.Empty(model.GetEntityTypeErrors().Values);
        }

        [ConditionalFact]
        public void Creates_entity_types_case_insensitive()
        {
            var info = new DatabaseModel
            {
                Tables =
                {
                    new DatabaseTable(Database, "TestTable")
                    {
                        Columns = { IdColumn },
                        PrimaryKey = IdPrimaryKey
                    },
                    new DatabaseTable(Database, "TESTTABLE")
                    {
                        Columns = { IdColumn },
                        PrimaryKey = IdPrimaryKey
                    }
                }
            };
            var model = _factory.Create(info, false);
            Assert.Equal(2, model.GetEntityTypes().Select(et => et.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        }

        [ConditionalFact]
        public void Loads_column_types()
        {
            var info = new DatabaseModel
            {
                Tables =
                {
                    new DatabaseTable(Database, "Jobs")
                    {
                        Columns =
                        {
                            IdColumn,
                            new DatabaseColumn(Table, "occupation", "nvarchar(max)")
                            {
                                DefaultValueSql = "\"dev\""
                            },
                            new DatabaseColumn(Table, "salary", "int")
                            {
                                IsNullable = true
                            },
                            new DatabaseColumn(Table, "modified", "nvarchar(max)")
                            {
                                IsNullable = false,
                                ValueGenerated = ValueGenerated.OnAddOrUpdate
                            },
                            new DatabaseColumn(Table, "created", "nvarchar(max)")
                            {
                                ValueGenerated = ValueGenerated.OnAdd
                            },
                            new DatabaseColumn(Table, "current", "nvarchar(max)")
                            {
                                ComputedColumnSql = "compute_this()"
                            }
                        },
                        PrimaryKey = IdPrimaryKey
                    }
                }
            };

            var entityType = (EntityType)_factory.Create(info, false).FindEntityType("Jobs");

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
                    new DatabaseTable(Database, "NaturalProducts")
                    {
                        Columns =
                        {
                            IdColumn,
                            new DatabaseColumn(Table, "ProductSKU", "nvarchar(max)"),
                            new DatabaseColumn(Table, "supplierID", "nvarchar(max)"),
                            new DatabaseColumn(Table, "Vendor_Discount", "nvarchar(max)")
                        },
                        PrimaryKey = IdPrimaryKey
                    }
                }
            };

            var entityType = _factory.Create(info, useDatabaseNames: true).FindEntityType("NaturalProducts");

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
                    new DatabaseTable(Database, "NaturalProducts")
                    {
                        Columns =
                        {
                            IdColumn,
                            new DatabaseColumn(Table, "ProductSKU", "nvarchar(max)"),
                            new DatabaseColumn(Table, "supplierID", "nvarchar(max)"),
                            new DatabaseColumn(Table, "Vendor_Discount", "nvarchar(max)")
                        },
                        PrimaryKey = IdPrimaryKey
                    }
                }
            };

            var entityType = _factory.Create(info, useDatabaseNames: false).FindEntityType("NaturalProducts");

            Assert.Collection(
                entityType.GetProperties(),
                pk => Assert.Equal("Id", pk.Name),
                col1 => Assert.Equal("ProductSku", col1.Name),
                col2 => Assert.Equal("SupplierId", col2.Name),
                col3 => Assert.Equal("VendorDiscount", col3.Name));
        }

        [ConditionalTheory]
        [InlineData("nvarchar(450)", null)]
        [InlineData("datetime2(4)", "datetime2(4)")]
        public void Column_type_annotation(string StoreType, string expectedColumnType)
        {
            var column = new DatabaseColumn(Table, "Col", StoreType);

            var info = new DatabaseModel
            {
                Tables =
                {
                    new DatabaseTable(Database, "A")
                    {
                        Columns = { column },
                        PrimaryKey = new DatabasePrimaryKey(Table, "PK_Foo") { Columns = { column } }
                    }
                }
            };

            var property = (Property)_factory.Create(info, false).FindEntityType("A").FindProperty("Col");

            Assert.Equal(expectedColumnType, property.GetColumnType());
        }

        [ConditionalFact]
        public void Column_ordinal_annotation()
        {
            var col1 = new DatabaseColumn(Table, "Col1", "nvarchar(max)");
            var info = new DatabaseModel
            {
                Tables =
                {
                    new DatabaseTable(Database, "A")
                    {
                        Columns =
                        {
                            col1,
                            new DatabaseColumn(Table, "Col2", "nvarchar(max)"),
                            new DatabaseColumn(Table, "Col3", "nvarchar(max)")
                        },
                        PrimaryKey = new DatabasePrimaryKey(Table, "PK_Foo") { Columns = { col1 } }
                    }
                }
            };

            var entityTypeA = _factory.Create(info, false).FindEntityType("A");
            var property1 = (Property)entityTypeA.FindProperty("Col1");
            var property2 = (Property)entityTypeA.FindProperty("Col2");
            var property3 = (Property)entityTypeA.FindProperty("Col3");

            Assert.Equal(0, property1.GetColumnOrdinal());
            Assert.Equal(1, property2.GetColumnOrdinal());
            Assert.Equal(2, property3.GetColumnOrdinal());
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
                    new DatabaseTable(Database, "E")
                    {
                        Columns = { IdColumn },
                        PrimaryKey = IdPrimaryKey
                    }
                }
            };

            info.Tables.First().Columns.Add(new DatabaseColumn(info.Tables.First(), "Coli", StoreType));

            Assert.Single(_factory.Create(info, false).FindEntityType("E").GetProperties());
            Assert.Single(_reporter.Messages, t => t.Contains(DesignStrings.CannotFindTypeMappingForColumn("E.Coli", StoreType)));
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
                Tables = { new DatabaseTable(Database, "PkTable") { PrimaryKey = new DatabasePrimaryKey(Table, "MyPk") } }
            };
            foreach (var column in keyProps.Select(k => new DatabaseColumn(Table, k, "int")))
            {
                info.Tables[0].Columns.Add(column);
                info.Tables[0].PrimaryKey.Columns.Add(column);
            }

            var model = (EntityType)_factory.Create(info, false).GetEntityTypes().Single();

            Assert.Equal("MyPk", model.FindPrimaryKey().GetName());
            Assert.Equal(keyProps, model.FindPrimaryKey().Properties.Select(p => p.GetColumnName()).ToArray());
        }

        [ConditionalFact]
        public void Unique_constraint()
        {
            var myColumn = new DatabaseColumn(Table, "MyColumn", "int");

            var databaseModel = new DatabaseModel
            {
                Tables =
                {
                    new DatabaseTable(Database, "MyTable")
                    {
                        Columns = { IdColumn, myColumn },
                        PrimaryKey = IdPrimaryKey,
                        UniqueConstraints = { new DatabaseUniqueConstraint(Table, "MyUniqueConstraint") { Columns = { myColumn } } }
                    }
                }
            };

            var entityType = (EntityType)_factory.Create(databaseModel, false).GetEntityTypes().Single();
            var index = entityType.GetIndexes().Single();

            Assert.True(index.IsUnique);
            Assert.Equal("MyUniqueConstraint", index.GetName());
            Assert.Same(entityType.FindProperty("MyColumn"), index.Properties.Single());
        }

        [ConditionalFact]
        public void Indexes_and_alternate_keys()
        {
            var c1 = new DatabaseColumn(Table, "C1", "int");
            var table = new DatabaseTable(Database, "T")
            {
                Columns =
                {
                    c1,
                    new DatabaseColumn(Table, "C2", "int"),
                    new DatabaseColumn(Table, "C3", "int")
                },
                PrimaryKey = new DatabasePrimaryKey(Table, "PK_Foo") { Columns = { c1 } }
            };
            table.Indexes.Add(
                new DatabaseIndex(Table, "IDX_C1")
                {
                    Columns = { table.Columns.ElementAt(0) },
                    IsUnique = false
                });
            table.Indexes.Add(
                new DatabaseIndex(Table, "UNQ_C2")
                {
                    Columns = { table.Columns.ElementAt(1) },
                    IsUnique = true
                });
            table.Indexes.Add(
                new DatabaseIndex(Table, "IDX_C2_C1")
                {
                    Columns = { table.Columns.ElementAt(1), table.Columns.ElementAt(0) },
                    IsUnique = false
                });
            table.Indexes.Add(
                new DatabaseIndex(Table, "UNQ_C3_C1")
                {
                    Columns = { table.Columns.ElementAt(2), table.Columns.ElementAt(0) }, IsUnique = true
                });

            var info = new DatabaseModel { Tables = { table } };

            var entityType = (EntityType)_factory.Create(info, false).GetEntityTypes().Single();

            Assert.Collection(
                entityType.GetIndexes(),
                indexColumn1 =>
                {
                    Assert.False(indexColumn1.IsUnique);
                    Assert.Equal("IDX_C1", indexColumn1.GetName());
                    Assert.Same(entityType.FindProperty("C1"), indexColumn1.Properties.Single());
                },
                uniqueColumn2 =>
                {
                    Assert.True(uniqueColumn2.IsUnique);
                    Assert.Same(entityType.FindProperty("C2"), uniqueColumn2.Properties.Single());
                },
                indexColumn2Column1 =>
                {
                    Assert.False(indexColumn2Column1.IsUnique);
                    Assert.Equal(new[] { "C2", "C1" }, indexColumn2Column1.Properties.Select(c => c.Name).ToArray());
                },
                uniqueColumn3Column1 =>
                {
                    Assert.True(uniqueColumn3Column1.IsUnique);
                    Assert.Equal(new[] { "C3", "C1" }, uniqueColumn3Column1.Properties.Select(c => c.Name).ToArray());
                }
            );

            // unique indexes should not cause alternate keys if not used by foreign keys
            Assert.Equal(0, entityType.GetKeys().Count(k => !k.IsPrimaryKey()));
        }

        [ConditionalFact]
        public void Foreign_key()
        {
            var parentTable = new DatabaseTable(Database, "Parent")
            {
                Columns = { IdColumn },
                PrimaryKey = IdPrimaryKey
            };
            var childrenTable = new DatabaseTable(Database, "Children")
            {
                Columns =
                {
                    IdColumn,
                    new DatabaseColumn(Table, "ParentId", "int") { IsNullable = true }
                },
                PrimaryKey = IdPrimaryKey
            };
            childrenTable.ForeignKeys.Add(
                new DatabaseForeignKey(childrenTable, "FK_Foo", parentTable)
                {
                    OnDelete = ReferentialAction.Cascade,
                    Columns = { childrenTable.Columns.ElementAt(1) },
                    PrincipalColumns = { parentTable.Columns.ElementAt(0) }
                });

            var model = _factory.Create(
                new DatabaseModel { Tables = { parentTable, childrenTable } },
                false);

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
            var masterTable = new DatabaseTable(databaseModel, "Master");
            var idColumn = new DatabaseColumn(masterTable, "Id", "int");
            masterTable.Columns.Add(idColumn);
            masterTable.PrimaryKey = new DatabasePrimaryKey(masterTable, null)
            {
                Columns = { idColumn }
            };
            databaseModel.Tables.Add(masterTable);
            var detailTable = new DatabaseTable(databaseModel, "Detail");
            var masterIdColumn = new DatabaseColumn(detailTable, "MasterId", "int");
            detailTable.Columns.Add(masterIdColumn);
            detailTable.ForeignKeys.Add(
                new DatabaseForeignKey(detailTable, null, masterTable)
                {
                    Columns = { masterIdColumn },
                    PrincipalColumns = { idColumn }
                });
            databaseModel.Tables.Add(detailTable);

            var model = _factory.Create(databaseModel, useDatabaseNames: false);

            var detail = model.FindEntityType("Detail");
            var foreignKey = Assert.Single(detail.GetForeignKeys());
            Assert.Equal("Master", foreignKey.DependentToPrincipal.Name);
            Assert.Null(foreignKey.PrincipalToDependent);
        }

        [ConditionalFact]
        public void Foreign_key_to_unique_constraint()
        {
            var keyColumn = new DatabaseColumn(Table, "Key", "int") { IsNullable = false };

            var parentTable = new DatabaseTable(Database, "Parent")
            {
                Columns = { IdColumn, keyColumn },
                PrimaryKey = IdPrimaryKey
            };

            parentTable.UniqueConstraints.Add(
                new DatabaseUniqueConstraint(parentTable, "AK_Foo") { Columns = { keyColumn } });

            var childrenTable = new DatabaseTable(Database, "Children")
            {
                Columns = { IdColumn },
                PrimaryKey = IdPrimaryKey
            };

            childrenTable.ForeignKeys.Add(
                new DatabaseForeignKey(childrenTable, "FK_Foo", parentTable)
                {
                    OnDelete = ReferentialAction.Cascade,
                    Columns = { childrenTable.Columns.ElementAt(0) },
                    PrincipalColumns = { parentTable.Columns.ElementAt(1) }
                });

            var model = _factory.Create(
                new DatabaseModel { Tables = { parentTable, childrenTable } },
                false);

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
            var parentTable = new DatabaseTable(Database, "Parent")
            {
                Columns = { IdColumn },
                PrimaryKey = IdPrimaryKey
            };
            var childrenTable = new DatabaseTable(Database, "Children")
            {
                Columns = { IdColumn },
                PrimaryKey = IdPrimaryKey
            };
            childrenTable.ForeignKeys.Add(
                new DatabaseForeignKey(childrenTable, "FK_Foo", parentTable)
                {
                    OnDelete = ReferentialAction.NoAction,
                    Columns = { childrenTable.Columns.ElementAt(0) },
                    PrincipalColumns = { parentTable.Columns.ElementAt(0) }
                });

            var model = _factory.Create(
                new DatabaseModel { Tables = { parentTable, childrenTable } },
                false);

            var children = (EntityType)model.FindEntityType("Children");

            var fk = Assert.Single(children.GetForeignKeys());
            Assert.True(fk.IsUnique);
            Assert.Equal(DeleteBehavior.ClientSetNull, fk.DeleteBehavior);
        }

        [ConditionalFact]
        public void Composite_foreign_key()
        {
            var ida = new DatabaseColumn(Table, "Id_A", "int");
            var idb = new DatabaseColumn(Table, "Id_B", "int");
            var parentTable = new DatabaseTable(Database, "Parent")
            {
                Columns = { ida, idb },
                PrimaryKey = new DatabasePrimaryKey(Table, "PK_Foo") { Columns = { ida, idb } }
            };
            var childrenTable = new DatabaseTable(Database, "Children")
            {
                Columns =
                {
                    IdColumn,
                    new DatabaseColumn(Table, "ParentId_A", "int"),
                    new DatabaseColumn(Table, "ParentId_B", "int")
                },
                PrimaryKey = IdPrimaryKey
            };
            childrenTable.ForeignKeys.Add(
                new DatabaseForeignKey(childrenTable, "FK_Foo", parentTable)
                {
                    OnDelete = ReferentialAction.SetNull,
                    Columns = { childrenTable.Columns.ElementAt(1), childrenTable.Columns.ElementAt(2) },
                    PrincipalColumns = { parentTable.Columns.ElementAt(0), parentTable.Columns.ElementAt(1) }
                });

            var model = _factory.Create(
                new DatabaseModel { Tables = { parentTable, childrenTable } },
                false);

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
            var table = new DatabaseTable(Database, "ItemsList")
            {
                Columns =
                {
                    IdColumn,
                    new DatabaseColumn(Table, "ParentId", "int") { IsNullable = false }
                },
                PrimaryKey = IdPrimaryKey
            };
            table.ForeignKeys.Add(
                new DatabaseForeignKey(table, "FK_Foo", table)
                {
                    Columns = { table.Columns.ElementAt(1) },
                    PrincipalColumns = { table.Columns.ElementAt(0) }
                });

            var model = _factory.Create(
                new DatabaseModel { Tables = { table } },
                false);
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
            var parentTable = new DatabaseTable(Database, "Parent")
            {
                Columns = { IdColumn, new DatabaseColumn(Table, "NotPkId", "int") },
                PrimaryKey = IdPrimaryKey
            };
            var childrenTable = new DatabaseTable(Database, "Children")
            {
                Columns = { IdColumn, new DatabaseColumn(Table, "ParentId", "int") },
                PrimaryKey = IdPrimaryKey
            };
            childrenTable.ForeignKeys.Add(
                new DatabaseForeignKey(childrenTable, "FK_Foo", parentTable)
                {
                    Columns = { childrenTable.Columns.ElementAt(1) },
                    PrincipalColumns = { parentTable.Columns.ElementAt(1) }
                });

            _factory.Create(
                new DatabaseModel { Tables = { parentTable, childrenTable } },
                false);

            Assert.Single(
                _reporter.Messages, t => t.Contains(
                    "warn: "
                    + DesignStrings.ForeignKeyScaffoldErrorPrincipalKeyNotFound(
                        childrenTable.ForeignKeys.ElementAt(0).DisplayName(), "NotPkId", "Parent")));
        }

        [ConditionalFact]
        public void Unique_nullable_index_unused_by_foreign_key()
        {
            var table = new DatabaseTable(Database, "Friends")
            {
                Columns =
                {
                    IdColumn,
                    new DatabaseColumn(Table, "BuddyId", "int") { IsNullable = true }
                },
                PrimaryKey = IdPrimaryKey
            };
            table.Indexes.Add(
                new DatabaseIndex(Table, "IX_Foo") { Columns = { table.Columns.ElementAt(1) }, IsUnique = true });
            table.ForeignKeys.Add(
                new DatabaseForeignKey(table, "FK_Foo", table)
                {
                    Columns = { table.Columns.ElementAt(1) },
                    PrincipalColumns = { table.Columns.ElementAt(0) }
                });

            var model = _factory.Create(
                new DatabaseModel { Tables = { table } },
                false).FindEntityType("Friends");

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
            var table = new DatabaseTable(Database, "Friends")
            {
                Columns =
                {
                    IdColumn,
                    new DatabaseColumn(Table, "BuddyId", "int") { IsNullable = true }
                },
                PrimaryKey = IdPrimaryKey
            };
            table.Indexes.Add(
                new DatabaseIndex(Table, "FriendsNameUniqueIndex")
                {
                    Columns = { table.Columns.ElementAt(1) },
                    IsUnique = true
                });
            table.ForeignKeys.Add(
                new DatabaseForeignKey(table, "FK_Foo", table)
                {
                    Columns = { table.Columns.ElementAt(1) },
                    PrincipalColumns = { table.Columns.ElementAt(1) }
                });

            var model = _factory.Create(
                new DatabaseModel { Tables = { table } },
                false).FindEntityType("Friends");

            var buddyIdProperty = model.FindProperty("BuddyId");
            Assert.NotNull(buddyIdProperty);
            Assert.False(buddyIdProperty.IsNullable);

            var fk = Assert.Single(model.GetForeignKeys());

            Assert.True(fk.IsUnique);
            var alternateKey = model.GetKeys().Single(k => !k.IsPrimaryKey());
            Assert.Equal(alternateKey, fk.PrincipalKey);

            Assert.Single(
                _reporter.Messages, t => t.Contains(
                    "warn: "
                    + DesignStrings.ForeignKeyPrincipalEndContainsNullableColumns(
                        table.ForeignKeys.ElementAt(0).DisplayName(), "FriendsNameUniqueIndex", "Friends.BuddyId")));
        }

        [ConditionalFact]
        public void Unique_index_composite_foreign_key()
        {
            var ida = new DatabaseColumn(Table, "Id_A", "int");
            var idb = new DatabaseColumn(Table, "Id_B", "int");
            var parentTable = new DatabaseTable(Database, "Parent")
            {
                Columns = { ida, idb },
                PrimaryKey = new DatabasePrimaryKey(Table, "PK_Foo") { Columns = { ida, idb } }
            };
            var childrenTable = new DatabaseTable(Database, "Children")
            {
                Columns =
                {
                    IdColumn,
                    new DatabaseColumn(Table, "ParentId_A", "int"),
                    new DatabaseColumn(Table, "ParentId_B", "int")
                },
                PrimaryKey = IdPrimaryKey
            };
            childrenTable.Indexes.Add(
                new DatabaseIndex(Table, "IX_Foo")
                {
                    IsUnique = true, Columns = { childrenTable.Columns.ElementAt(1), childrenTable.Columns.ElementAt(2) }
                });
            childrenTable.ForeignKeys.Add(
                new DatabaseForeignKey(childrenTable, "FK_Foo", parentTable)
                {
                    Columns = { childrenTable.Columns.ElementAt(1), childrenTable.Columns.ElementAt(2) },
                    PrincipalColumns = { parentTable.Columns.ElementAt(0), parentTable.Columns.ElementAt(1) }
                });

            var model = _factory.Create(
                new DatabaseModel { Tables = { parentTable, childrenTable } },
                false);
            var parent = model.FindEntityType("Parent");
            var children = model.FindEntityType("Children");

            var fk = Assert.Single(children.GetForeignKeys());

            Assert.True(fk.IsUnique);
            Assert.Equal(parent.FindPrimaryKey(), fk.PrincipalKey);
        }

        [ConditionalFact]
        public void Unique_names()
        {
            var info = new DatabaseModel
            {
                Tables =
                {
                    new DatabaseTable(Database, "E F")
                    {
                        Columns =
                        {
                            IdColumn,
                            new DatabaseColumn(Table, "San itized", "int"),
                            new DatabaseColumn(Table, "San+itized", "int")
                        },
                        PrimaryKey = IdPrimaryKey
                    },
                    new DatabaseTable(Database, "E+F")
                    {
                        Columns = { IdColumn },
                        PrimaryKey = IdPrimaryKey
                    }
                }
            };

            var model = _factory.Create(info, false);

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
            var info = new DatabaseModel { Sequences = { new DatabaseSequence(Database, "CountByThree") { IncrementBy = 3 } } };

            var model = _factory.Create(info, false);

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

        [ConditionalFact]
        public void DbSet_annotation_is_set()
        {
            var info = new DatabaseModel
            {
                Tables =
                {
                    new DatabaseTable(Database, "Blog")
                    {
                        Columns = { IdColumn },
                        PrimaryKey = IdPrimaryKey
                    }
                }
            };

            var model = _factory.Create(info, false);
            Assert.Equal("Blog", model.GetEntityTypes().Single().GetDbSetName());
        }

        [ConditionalFact]
        public void Pluralization_of_entity_and_DbSet()
        {
            var info = new DatabaseModel
            {
                Tables =
                {
                    new DatabaseTable(Database, "Blog")
                    {
                        Columns = { IdColumn },
                        PrimaryKey = IdPrimaryKey
                    },
                    new DatabaseTable(Database, "Posts")
                    {
                        Columns = { IdColumn },
                        PrimaryKey = IdPrimaryKey
                    }
                }
            };

            var services = new ServiceCollection()
                .AddEntityFrameworkDesignTimeServices(_reporter)
                .AddSingleton<IPluralizer, FakePluralizer>()
                .AddSingleton<IScaffoldingModelFactory, FakeScaffoldingModelFactory>();
            new SqlServerDesignTimeServices().ConfigureDesignTimeServices(services);

            var factory = services
                .BuildServiceProvider()
                .GetRequiredService<IScaffoldingModelFactory>();

            var model = factory.Create(info, false);

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

            model = factory.Create(info, true);

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
            var blogTable = new DatabaseTable(Database, "Blog")
            {
                Columns = { IdColumn },
                PrimaryKey = IdPrimaryKey
            };
            var postTable = new DatabaseTable(Database, "Post")
            {
                Columns =
                {
                    IdColumn,
                    new DatabaseColumn(Table, "BlogId", "int") { IsNullable = true }
                },
                PrimaryKey = IdPrimaryKey
            };

            postTable.ForeignKeys.Add(
                new DatabaseForeignKey(postTable, "FK_Foo", blogTable)
                {
                    OnDelete = ReferentialAction.Cascade,
                    Columns = { postTable.Columns.ElementAt(1) },
                    PrincipalColumns = { blogTable.Columns.ElementAt(0) }
                });

            var info = new DatabaseModel { Tables = { blogTable, postTable } };

            var services = new ServiceCollection()
                .AddEntityFrameworkDesignTimeServices(_reporter)
                .AddSingleton<IPluralizer, FakePluralizer>()
                .AddSingleton<IScaffoldingModelFactory, FakeScaffoldingModelFactory>();
            new SqlServerDesignTimeServices().ConfigureDesignTimeServices(services);

            var factory = services
                .BuildServiceProvider()
                .GetRequiredService<IScaffoldingModelFactory>();

            var model = factory.Create(info, false);

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
        public void Not_null_bool_column_with_default_value_is_made_nullable()
        {
            var dbModel = new DatabaseModel
            {
                Tables =
                {
                    new DatabaseTable(Database, "Table")
                    {
                        Columns =
                        {
                            IdColumn,
                            new DatabaseColumn(Table, "NonNullBoolWithDefault", "bit")
                            {
                                DefaultValueSql = "Default",
                                IsNullable = false
                            },
                            new DatabaseColumn(Table, "NonNullBoolWithoutDefault", "bit")
                            {
                                IsNullable = false
                            }
                        },
                        PrimaryKey = IdPrimaryKey
                    }
                }
            };

            var model = _factory.Create(dbModel, false);

            var columns = model.FindEntityType("Table").GetProperties().ToList();

            Assert.Equal(typeof(bool), columns.First(c => c.Name == "NonNullBoolWithoutDefault").ClrType);
            Assert.False(columns.First(c => c.Name == "NonNullBoolWithoutDefault").IsNullable);
            Assert.Equal(typeof(bool?), columns.First(c => c.Name == "NonNullBoolWithDefault").ClrType);
            Assert.False(columns.First(c => c.Name == "NonNullBoolWithDefault").IsNullable);
            Assert.Equal("Default", columns.First(c => c.Name == "NonNullBoolWithDefault")[RelationalAnnotationNames.DefaultValueSql]);
        }

        [ConditionalFact]
        public void Nullable_column_with_default_value_sql_does_not_generate_warning()
        {
            var dbModel = new DatabaseModel
            {
                Tables =
                {
                    new DatabaseTable(Database, "Table")
                    {
                        Columns =
                        {
                            IdColumn,
                            new DatabaseColumn(Table, "NullBoolWithDefault", "bit")
                            {
                                DefaultValueSql = "Default",
                                IsNullable = true
                            }
                        },
                        PrimaryKey = IdPrimaryKey
                    }
                }
            };

            var model = _factory.Create(dbModel, false);

            var columns = model.FindEntityType("Table").GetProperties().ToList();

            Assert.Equal(typeof(bool?), columns.First(c => c.Name == "NullBoolWithDefault").ClrType);
            Assert.True(columns.First(c => c.Name == "NullBoolWithDefault").IsNullable);
            Assert.Equal("Default", columns.First(c => c.Name == "NullBoolWithDefault")[RelationalAnnotationNames.DefaultValueSql]);

            Assert.Empty(_reporter.Messages);
        }

        [ConditionalFact]
        public void Correct_arguments_to_scaffolding_typemapper()
        {
            var principalPkColumn = new DatabaseColumn(Table, "PrimaryKey", "nvarchar(450)");
            var principalAkColumn = new DatabaseColumn(Table, "AlternateKey", "nvarchar(450)");
            var principalIndexColumn = new DatabaseColumn(Table, "Index", "nvarchar(450)");
            var rowversionColumn = new DatabaseColumn(Table, "Rowversion", "rowversion")
            {
                ValueGenerated = ValueGenerated.OnAddOrUpdate,
                [ScaffoldingAnnotationNames.ConcurrencyToken] = true
            };

            var principalTable = new DatabaseTable(Database, "Principal")
            {
                Columns =
                {
                    principalPkColumn,
                    principalAkColumn,
                    principalIndexColumn,
                    rowversionColumn
                },
                PrimaryKey = new DatabasePrimaryKey(Table, "PK_Foo") { Columns = { principalPkColumn } },
                UniqueConstraints = { new DatabaseUniqueConstraint(Table, "AK_Foo") { Columns = { principalAkColumn } } },
                Indexes = { new DatabaseIndex(Table, "IX_Foo") { Columns = { principalIndexColumn } } }
            };

            var dependentIdColumn = new DatabaseColumn(Table, "Id", "int");
            var dependentFkColumn = new DatabaseColumn(Table, "BlogAlternateKey", "nvarchar(450)");

            var dependentTable = new DatabaseTable(Database, "Dependent")
            {
                Columns = { dependentIdColumn, dependentFkColumn },
                PrimaryKey = new DatabasePrimaryKey(Table, "PK_Foo") { Columns = { dependentIdColumn } },
                Indexes = { new DatabaseIndex(Table, "IX_Foo") { Columns = { dependentFkColumn } } },
                ForeignKeys =
                {
                    new DatabaseForeignKey(Table, "FK_Foo", principalTable)
                    {
                        Columns = { dependentFkColumn },
                        PrincipalColumns = { principalAkColumn }
                    }
                }
            };

            var dbModel = new DatabaseModel { Tables = { principalTable, dependentTable } };

            var model = _factory.Create(dbModel, false);

            Assert.Null(model.FindEntityType("Principal").FindProperty("PrimaryKey").GetColumnType());
            Assert.Null(model.FindEntityType("Principal").FindProperty("AlternateKey").GetColumnType());
            Assert.Null(model.FindEntityType("Principal").FindProperty("Index").GetColumnType());
            Assert.Null(model.FindEntityType("Principal").FindProperty("Rowversion").GetColumnType());
            Assert.Null(model.FindEntityType("Dependent").FindProperty("BlogAlternateKey").GetColumnType());
        }

        [ConditionalFact]
        public void Unmapped_column_is_ignored()
        {
            var columnWithUnknownType = new DatabaseColumn(Table, "ColumnWithUnknownStoreType", "unknown_type");

            var dbModel = new DatabaseModel
            {
                Tables =
                {
                    new DatabaseTable(Database, "Table")
                    {
                        Columns = { IdColumn, columnWithUnknownType },
                        PrimaryKey = IdPrimaryKey
                    }
                }
            };

            var model = _factory.Create(dbModel, false);

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
                    new DatabaseTable(Database, "Table")
                    {
                        Comment = "A table",
                        Columns =
                        {
                            IdColumn,
                            new DatabaseColumn(Table, "Column", "int") { Comment = "An int column" }
                        }
                    }
                }
            };

            var model = _factory.Create(database, useDatabaseNames: false);

            var table = model.FindEntityType("Table");
            Assert.Equal("A table", table.GetComment());

            var column = model.FindEntityType("Table").GetProperty("Column");
            Assert.Equal("An int column", column.GetComment());
        }

        public class FakePluralizer : IPluralizer
        {
            public string Pluralize(string name)
            {
                return name.EndsWith("s")
                    ? name
                    : name + "s";
            }

            public string Singularize(string name)
            {
                return name.EndsWith("s")
                    ? name[..^1]
                    : name;
            }
        }
    }
}
