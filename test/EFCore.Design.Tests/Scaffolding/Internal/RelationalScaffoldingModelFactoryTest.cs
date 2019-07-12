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

        private static DatabaseColumn IdColumn => new DatabaseColumn
        {
            Name = "Id",
            StoreType = "int"
        };

        private static readonly DatabasePrimaryKey IdPrimaryKey = new DatabasePrimaryKey
        {
            Columns =
            {
                IdColumn
            }
        };

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
                    new DatabaseTable
                    {
                        Name = "tableWithSchema",
                        Schema = "public",
                        Columns =
                        {
                            IdColumn
                        },
                        PrimaryKey = IdPrimaryKey
                    },
                    new DatabaseTable
                    {
                        Name = "noSchema",
                        Columns =
                        {
                            IdColumn
                        },
                        PrimaryKey = IdPrimaryKey
                    },
                    new DatabaseTable
                    {
                        Name = "noPrimaryKey"
                    }
                }
            };
            var model = _factory.Create(info, false);
            Assert.Collection(
                model.GetEntityTypes().OrderBy(t => t.Name).Cast<EntityType>(),
                vwtable =>
                {
                    Assert.Equal("noPrimaryKey", vwtable.GetTableName());
                    Assert.Equal(0, vwtable.GetKeys().Count());
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
                    new DatabaseTable
                    {
                        Name = "TestTable",
                        Columns =
                        {
                            IdColumn
                        },
                        PrimaryKey = IdPrimaryKey
                    },
                    new DatabaseTable
                    {
                        Name = "TESTTABLE",
                        Columns =
                        {
                            IdColumn
                        },
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
                    new DatabaseTable
                    {
                        Name = "Jobs",
                        Columns =
                        {
                            IdColumn,
                            new DatabaseColumn
                            {
                                Name = "occupation",
                                StoreType = "nvarchar(max)",
                                DefaultValueSql = "\"dev\""
                            },
                            new DatabaseColumn
                            {
                                Name = "salary",
                                StoreType = "int",
                                IsNullable = true
                            },
                            new DatabaseColumn
                            {
                                Name = "modified",
                                StoreType = "nvarchar(max)",
                                IsNullable = false,
                                ValueGenerated = ValueGenerated.OnAddOrUpdate
                            },
                            new DatabaseColumn
                            {
                                Name = "created",
                                StoreType = "nvarchar(max)",
                                ValueGenerated = ValueGenerated.OnAdd
                            },
                            new DatabaseColumn
                            {
                                Name = "current",
                                StoreType = "nvarchar(max)",
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
                    new DatabaseTable
                    {
                        Name = "NaturalProducts",
                        Columns =
                        {
                            IdColumn,
                            new DatabaseColumn
                            {
                                Name = "ProductSKU",
                                StoreType = "nvarchar(max)"
                            },
                            new DatabaseColumn
                            {
                                Name = "supplierID",
                                StoreType = "nvarchar(max)"
                            },
                            new DatabaseColumn
                            {
                                Name = "Vendor_Discount",
                                StoreType = "nvarchar(max)"
                            }
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
                    new DatabaseTable
                    {
                        Name = "NaturalProducts",
                        Columns =
                        {
                            IdColumn,
                            new DatabaseColumn
                            {
                                Name = "ProductSKU",
                                StoreType = "nvarchar(max)"
                            },
                            new DatabaseColumn
                            {
                                Name = "supplierID",
                                StoreType = "nvarchar(max)"
                            },
                            new DatabaseColumn
                            {
                                Name = "Vendor_Discount",
                                StoreType = "nvarchar(max)"
                            }
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
            var column = new DatabaseColumn
            {
                Name = "Col",
                StoreType = StoreType
            };

            var info = new DatabaseModel
            {
                Tables =
                {
                    new DatabaseTable
                    {
                        Name = "A",
                        Columns =
                        {
                            column
                        },
                        PrimaryKey = new DatabasePrimaryKey
                        {
                            Columns =
                            {
                                column
                            }
                        }
                    }
                }
            };

            var property = (Property)_factory.Create(info, false).FindEntityType("A").FindProperty("Col");

            Assert.Equal(expectedColumnType, property.GetColumnType());
        }

        [ConditionalFact]
        public void Column_ordinal_annotation()
        {
            var col1 = new DatabaseColumn
            {
                Name = "Col1",
                StoreType = "nvarchar(max)"
            };
            var info = new DatabaseModel
            {
                Tables =
                {
                    new DatabaseTable
                    {
                        Name = "A",
                        Columns =
                        {
                            col1,
                            new DatabaseColumn
                            {
                                Name = "Col2",
                                StoreType = "nvarchar(max)"
                            },
                            new DatabaseColumn
                            {
                                Name = "Col3",
                                StoreType = "nvarchar(max)"
                            }
                        },
                        PrimaryKey = new DatabasePrimaryKey
                        {
                            Columns =
                            {
                                col1
                            }
                        }
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
                    new DatabaseTable
                    {
                        Name = "E",
                        Columns =
                        {
                            IdColumn
                        },
                        PrimaryKey = IdPrimaryKey
                    }
                }
            };

            info.Tables.First()
                .Columns.Add(
                    new DatabaseColumn
                    {
                        Table = info.Tables.First(),
                        Name = "Coli",
                        StoreType = StoreType
                    });

            Assert.Single(_factory.Create(info, false).FindEntityType("E").GetProperties());
            Assert.Single(_reporter.Messages, t => t.Contains(DesignStrings.CannotFindTypeMappingForColumn("E.Coli", StoreType)));
        }

        [ConditionalTheory]
        [InlineData(new[] { "Id" }, 1)]
        [InlineData(new[] { "Id", "AltId" }, 2)]
        public void Primary_key(string[] keyProps, int length)

        {
            var info = new DatabaseModel
            {
                Tables =
                {
                    new DatabaseTable
                    {
                        Name = "PkTable",
                        PrimaryKey = new DatabasePrimaryKey { Name = "MyPk" }
                    }
                }
            };
            foreach (var column in keyProps.Select(
                k => new DatabaseColumn
                {
                    Name = k,
                    StoreType = "int"
                }))
            {
                info.Tables[0].Columns.Add(column);
                info.Tables[0].PrimaryKey.Columns.Add(column);
            }

            var model = (EntityType)_factory.Create(info, false).GetEntityTypes().Single();

            Assert.Equal(model.FindPrimaryKey().GetName(), "MyPk");
            Assert.Equal(keyProps, model.FindPrimaryKey().Properties.Select(p => p.GetColumnName()).ToArray());
        }

        [ConditionalFact]
        public void Unique_constraint()
        {
            var myColumn = new DatabaseColumn
            {
                Name = "MyColumn",
                StoreType = "int"
            };

            var databaseModel = new DatabaseModel
            {
                Tables =
                {
                    new DatabaseTable
                    {
                        Name = "MyTable",
                        Columns =
                        {
                            IdColumn,
                            myColumn
                        },
                        PrimaryKey = IdPrimaryKey,
                        UniqueConstraints =
                        {
                            new DatabaseUniqueConstraint
                            {
                                Name = "MyUniqueConstraint",
                                Columns = { myColumn }
                            }
                        }
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
            var c1 = new DatabaseColumn
            {
                Name = "C1",
                StoreType = "int"
            };
            var table = new DatabaseTable
            {
                Name = "T",
                Columns =
                {
                    c1,
                    new DatabaseColumn
                    {
                        Name = "C2",
                        StoreType = "int"
                    },
                    new DatabaseColumn
                    {
                        Name = "C3",
                        StoreType = "int"
                    }
                },
                PrimaryKey = new DatabasePrimaryKey
                {
                    Columns =
                    {
                        c1
                    }
                }
            };
            table.Indexes.Add(
                new DatabaseIndex
                {
                    Name = "IDX_C1",
                    Columns =
                    {
                        table.Columns.ElementAt(0)
                    },
                    IsUnique = false
                });
            table.Indexes.Add(
                new DatabaseIndex
                {
                    Name = "UNQ_C2",
                    Columns =
                    {
                        table.Columns.ElementAt(1)
                    },
                    IsUnique = true
                });
            table.Indexes.Add(
                new DatabaseIndex
                {
                    Name = "IDX_C2_C1",
                    Columns =
                    {
                        table.Columns.ElementAt(1),
                        table.Columns.ElementAt(0)
                    },
                    IsUnique = false
                });
            table.Indexes.Add(
                new DatabaseIndex
                {
                    /*Name ="UNQ_C3_C1",*/
                    Columns =
                    {
                        table.Columns.ElementAt(2),
                        table.Columns.ElementAt(0)
                    },
                    IsUnique = true
                });

            var info = new DatabaseModel
            {
                Tables =
                {
                    table
                }
            };

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
            var parentTable = new DatabaseTable
            {
                Name = "Parent",
                Columns =
                {
                    IdColumn
                },
                PrimaryKey = IdPrimaryKey
            };
            var childrenTable = new DatabaseTable
            {
                Name = "Children",
                Columns =
                {
                    IdColumn,
                    new DatabaseColumn
                    {
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
                    PrincipalTable = parentTable,
                    OnDelete = ReferentialAction.Cascade,
                    Columns =
                    {
                        childrenTable.Columns.ElementAt(1)
                    },
                    PrincipalColumns =
                    {
                        parentTable.Columns.ElementAt(0)
                    }
                });

            var model = _factory.Create(
                new DatabaseModel
                {
                    Tables =
                    {
                        parentTable,
                        childrenTable
                    }
                },
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
        public void Foreign_key_to_unique_constraint()
        {
            var keyColumn = new DatabaseColumn
            {
                Name = "Key",
                StoreType = "int",
                IsNullable = false
            };

            var parentTable = new DatabaseTable
            {
                Name = "Parent",
                Columns =
                {
                    IdColumn,
                    keyColumn
                },
                PrimaryKey = IdPrimaryKey
            };

            parentTable.UniqueConstraints.Add(
                new DatabaseUniqueConstraint
                {
                    Table = parentTable,
                    Columns =
                    {
                        keyColumn
                    }
                });

            var childrenTable = new DatabaseTable
            {
                Name = "Children",
                Columns =
                {
                    IdColumn
                },
                PrimaryKey = IdPrimaryKey
            };

            childrenTable.ForeignKeys.Add(
                new DatabaseForeignKey
                {
                    Table = childrenTable,
                    PrincipalTable = parentTable,
                    OnDelete = ReferentialAction.Cascade,
                    Columns =
                    {
                        childrenTable.Columns.ElementAt(0)
                    },
                    PrincipalColumns =
                    {
                        parentTable.Columns.ElementAt(1)
                    }
                });

            var model = _factory.Create(
                new DatabaseModel
                {
                    Tables =
                    {
                        parentTable,
                        childrenTable
                    }
                },
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
            var parentTable = new DatabaseTable
            {
                Name = "Parent",
                Columns =
                {
                    IdColumn
                },
                PrimaryKey = IdPrimaryKey
            };
            var childrenTable = new DatabaseTable
            {
                Name = "Children",
                Columns =
                {
                    IdColumn
                },
                PrimaryKey = IdPrimaryKey
            };
            childrenTable.ForeignKeys.Add(
                new DatabaseForeignKey
                {
                    Table = childrenTable,
                    PrincipalTable = parentTable,
                    OnDelete = ReferentialAction.NoAction,
                    Columns =
                    {
                        childrenTable.Columns.ElementAt(0)
                    },
                    PrincipalColumns =
                    {
                        parentTable.Columns.ElementAt(0)
                    }
                });

            var model = _factory.Create(
                new DatabaseModel
                {
                    Tables =
                    {
                        parentTable,
                        childrenTable
                    }
                },
                false);

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
                Name = "Id_A",
                StoreType = "int"
            };
            var idb = new DatabaseColumn
            {
                Name = "Id_B",
                StoreType = "int"
            };
            var parentTable = new DatabaseTable
            {
                Name = "Parent",
                Columns =
                {
                    ida,
                    idb
                },
                PrimaryKey = new DatabasePrimaryKey
                {
                    Columns =
                    {
                        ida,
                        idb
                    }
                }
            };
            var childrenTable = new DatabaseTable
            {
                Name = "Children",
                Columns =
                {
                    IdColumn,
                    new DatabaseColumn
                    {
                        Name = "ParentId_A",
                        StoreType = "int"
                    },
                    new DatabaseColumn
                    {
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
                    PrincipalTable = parentTable,
                    OnDelete = ReferentialAction.SetNull,
                    Columns =
                    {
                        childrenTable.Columns.ElementAt(1),
                        childrenTable.Columns.ElementAt(2)
                    },
                    PrincipalColumns =
                    {
                        parentTable.Columns.ElementAt(0),
                        parentTable.Columns.ElementAt(1)
                    }
                });

            var model = _factory.Create(
                new DatabaseModel
                {
                    Tables =
                    {
                        parentTable,
                        childrenTable
                    }
                },
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
            var table = new DatabaseTable
            {
                Name = "ItemsList",
                Columns =
                {
                    IdColumn,
                    new DatabaseColumn
                    {
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
                    PrincipalTable = table,
                    Columns =
                    {
                        table.Columns.ElementAt(1)
                    },
                    PrincipalColumns =
                    {
                        table.Columns.ElementAt(0)
                    }
                });

            var model = _factory.Create(
                new DatabaseModel
                {
                    Tables =
                    {
                        table
                    }
                },
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
            var parentTable = new DatabaseTable
            {
                Name = "Parent",
                Columns =
                {
                    IdColumn,
                    new DatabaseColumn
                    {
                        Name = "NotPkId",
                        StoreType = "int"
                    }
                },
                PrimaryKey = IdPrimaryKey
            };
            var childrenTable = new DatabaseTable
            {
                Name = "Children",
                Columns =
                {
                    IdColumn,
                    new DatabaseColumn
                    {
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
                    PrincipalTable = parentTable,
                    Columns =
                    {
                        childrenTable.Columns.ElementAt(1)
                    },
                    PrincipalColumns =
                    {
                        parentTable.Columns.ElementAt(1)
                    }
                });

            _factory.Create(
                new DatabaseModel
                {
                    Tables =
                    {
                        parentTable,
                        childrenTable
                    }
                },
                false);

            Assert.Single(
                _reporter.Messages, t => t.Contains(
                    "warn: " +
                    DesignStrings.ForeignKeyScaffoldErrorPrincipalKeyNotFound(
                        childrenTable.ForeignKeys.ElementAt(0).DisplayName(), "NotPkId", "Parent")));
        }

        [ConditionalFact]
        public void Unique_nullable_index_unused_by_foreign_key()
        {
            var table = new DatabaseTable
            {
                Name = "Friends",
                Columns =
                {
                    IdColumn,
                    new DatabaseColumn
                    {
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
                    Columns =
                    {
                        table.Columns.ElementAt(1)
                    },
                    IsUnique = true
                });
            table.ForeignKeys.Add(
                new DatabaseForeignKey
                {
                    Table = table,
                    PrincipalTable = table,
                    Columns =
                    {
                        table.Columns.ElementAt(1)
                    },
                    PrincipalColumns =
                    {
                        table.Columns.ElementAt(0)
                    }
                });

            var model = _factory.Create(
                new DatabaseModel
                {
                    Tables =
                    {
                        table
                    }
                },
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
            var table = new DatabaseTable
            {
                Name = "Friends",
                Columns =
                {
                    IdColumn,
                    new DatabaseColumn
                    {
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
                    Name = "FriendsNameUniqueIndex",
                    Columns =
                    {
                        table.Columns.ElementAt(1)
                    },
                    IsUnique = true
                });
            table.ForeignKeys.Add(
                new DatabaseForeignKey
                {
                    Table = table,
                    PrincipalTable = table,
                    Columns =
                    {
                        table.Columns.ElementAt(1)
                    },
                    PrincipalColumns =
                    {
                        table.Columns.ElementAt(1)
                    }
                });

            var model = _factory.Create(
                new DatabaseModel
                {
                    Tables =
                    {
                        table
                    }
                },
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
                    "warn: " +
                    DesignStrings.ForeignKeyPrincipalEndContainsNullableColumns(
                        table.ForeignKeys.ElementAt(0).DisplayName(), "FriendsNameUniqueIndex", "Friends.BuddyId")));
        }

        [ConditionalFact]
        public void Unique_index_composite_foreign_key()
        {
            var ida = new DatabaseColumn
            {
                Name = "Id_A",
                StoreType = "int"
            };
            var idb = new DatabaseColumn
            {
                Name = "Id_B",
                StoreType = "int"
            };
            var parentTable = new DatabaseTable
            {
                Name = "Parent",
                Columns =
                {
                    ida,
                    idb
                },
                PrimaryKey = new DatabasePrimaryKey
                {
                    Columns =
                    {
                        ida,
                        idb
                    }
                }
            };
            var childrenTable = new DatabaseTable
            {
                Name = "Children",
                Columns =
                {
                    IdColumn,
                    new DatabaseColumn
                    {
                        Name = "ParentId_A",
                        StoreType = "int"
                    },
                    new DatabaseColumn
                    {
                        Name = "ParentId_B",
                        StoreType = "int"
                    }
                },
                PrimaryKey = IdPrimaryKey
            };
            childrenTable.Indexes.Add(
                new DatabaseIndex
                {
                    IsUnique = true,
                    Columns =
                    {
                        childrenTable.Columns.ElementAt(1),
                        childrenTable.Columns.ElementAt(2)
                    }
                });
            childrenTable.ForeignKeys.Add(
                new DatabaseForeignKey
                {
                    Table = childrenTable,
                    PrincipalTable = parentTable,
                    Columns =
                    {
                        childrenTable.Columns.ElementAt(1),
                        childrenTable.Columns.ElementAt(2)
                    },
                    PrincipalColumns =
                    {
                        parentTable.Columns.ElementAt(0),
                        parentTable.Columns.ElementAt(1)
                    }
                });

            var model = _factory.Create(
                new DatabaseModel
                {
                    Tables =
                    {
                        parentTable,
                        childrenTable
                    }
                },
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
                    new DatabaseTable
                    {
                        Name = "E F",
                        Columns =
                        {
                            IdColumn,
                            new DatabaseColumn
                            {
                                Name = "San itized",
                                StoreType = "int"
                            },
                            new DatabaseColumn
                            {
                                Name = "San+itized",
                                StoreType = "int"
                            }
                        },
                        PrimaryKey = IdPrimaryKey
                    },
                    new DatabaseTable
                    {
                        Name = "E+F",
                        Columns =
                        {
                            IdColumn
                        },
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
            var info = new DatabaseModel
            {
                Sequences =
                {
                    new DatabaseSequence
                    {
                        Name = "CountByThree",
                        IncrementBy = 3
                    }
                }
            };

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
                    new DatabaseTable
                    {
                        Name = "Blog",
                        Columns =
                        {
                            IdColumn
                        },
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
                    new DatabaseTable
                    {
                        Name = "Blog",
                        Columns =
                        {
                            IdColumn
                        },
                        PrimaryKey = IdPrimaryKey
                    },
                    new DatabaseTable
                    {
                        Name = "Posts",
                        Columns =
                        {
                            IdColumn
                        },
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
            var blogTable = new DatabaseTable
            {
                Name = "Blog",
                Columns =
                {
                    IdColumn
                },
                PrimaryKey = IdPrimaryKey
            };
            var postTable = new DatabaseTable
            {
                Name = "Post",
                Columns =
                {
                    IdColumn,
                    new DatabaseColumn
                    {
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
                    PrincipalTable = blogTable,
                    OnDelete = ReferentialAction.Cascade,
                    Columns =
                    {
                        postTable.Columns.ElementAt(1)
                    },
                    PrincipalColumns =
                    {
                        blogTable.Columns.ElementAt(0)
                    }
                });

            var info = new DatabaseModel
            {
                Tables =
                {
                    blogTable,
                    postTable
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
                    new DatabaseTable
                    {
                        Name = "Table",
                        Columns =
                        {
                            IdColumn,
                            new DatabaseColumn
                            {
                                Name = "NonNullBoolWithDefault",
                                StoreType = "bit",
                                DefaultValueSql = "Default",
                                IsNullable = false
                            },
                            new DatabaseColumn
                            {
                                Name = "NonNullBoolWithoutDefault",
                                StoreType = "bit",
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
                    new DatabaseTable
                    {
                        Name = "Table",
                        Columns =
                        {
                            IdColumn,
                            new DatabaseColumn
                            {
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
            var principalPkColumn = new DatabaseColumn
            {
                Name = "PrimaryKey",
                StoreType = "nvarchar(450)"
            };
            var principalAkColumn = new DatabaseColumn
            {
                Name = "AlternateKey",
                StoreType = "nvarchar(450)"
            };
            var principalIndexColumn = new DatabaseColumn
            {
                Name = "Index",
                StoreType = "nvarchar(450)"
            };
            var rowversionColumn = new DatabaseColumn
            {
                Name = "Rowversion",
                StoreType = "rowversion",
                ValueGenerated = ValueGenerated.OnAddOrUpdate,
                [ScaffoldingAnnotationNames.ConcurrencyToken] = true
            };

            var principalTable = new DatabaseTable
            {
                Name = "Principal",
                Columns =
                {
                    principalPkColumn,
                    principalAkColumn,
                    principalIndexColumn,
                    rowversionColumn
                },
                PrimaryKey = new DatabasePrimaryKey
                {
                    Columns =
                    {
                        principalPkColumn
                    }
                },
                UniqueConstraints =
                {
                    new DatabaseUniqueConstraint
                    {
                        Columns =
                        {
                            principalAkColumn
                        }
                    }
                },
                Indexes =
                {
                    new DatabaseIndex
                    {
                        Columns =
                        {
                            principalIndexColumn
                        }
                    }
                }
            };

            var dependentIdColumn = new DatabaseColumn
            {
                Name = "Id",
                StoreType = "int"
            };
            var dependentFkColumn = new DatabaseColumn
            {
                Name = "BlogAlternateKey",
                StoreType = "nvarchar(450)"
            };

            var dependentTable = new DatabaseTable
            {
                Name = "Dependent",
                Columns =
                {
                    dependentIdColumn,
                    dependentFkColumn
                },
                PrimaryKey = new DatabasePrimaryKey
                {
                    Columns =
                    {
                        dependentIdColumn
                    }
                },
                Indexes =
                {
                    new DatabaseIndex
                    {
                        Columns =
                        {
                            dependentFkColumn
                        }
                    }
                },
                ForeignKeys =
                {
                    new DatabaseForeignKey
                    {
                        Columns =
                        {
                            dependentFkColumn
                        },
                        PrincipalTable = principalTable,
                        PrincipalColumns =
                        {
                            principalAkColumn
                        }
                    }
                }
            };

            var dbModel = new DatabaseModel
            {
                Tables =
                {
                    principalTable,
                    dependentTable
                }
            };

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
            var columnWithUnknownType = new DatabaseColumn
            {
                Name = "ColumnWithUnknownStoreType",
                StoreType = "unknown_type"
            };

            var dbModel = new DatabaseModel
            {
                Tables =
                {
                    new DatabaseTable
                    {
                        Name = "Table",
                        Columns =
                        {
                            IdColumn,
                            columnWithUnknownType
                        },
                        PrimaryKey = IdPrimaryKey
                    }
                }
            };

            var model = _factory.Create(dbModel, false);

            var columns = model.FindEntityType("Table").GetProperties().ToList();

            Assert.Equal(1, columns.Count);
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
                        Name = "Table",
                        Comment = "A table",
                        Columns =
                        {
                            IdColumn,
                            new DatabaseColumn
                            {
                                Name = "Column",
                                StoreType = "int",
                                Comment = "An int column"
                            }
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
                    ? name[0..^1]
                    : name;
            }
        }
    }
}
