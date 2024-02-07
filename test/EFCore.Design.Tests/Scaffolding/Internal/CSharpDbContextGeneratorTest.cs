// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CustomTestNamespace;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Design.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    public class CSharpDbContextGeneratorTest(ModelCodeGeneratorTestFixture fixture, ITestOutputHelper output) : ModelCodeGeneratorTestBase(fixture, output)
    {
        [ConditionalFact]
        public Task Empty_model()
            => TestAsync(
                modelBuilder => { },
                new ModelCodeGenerationOptions(),
                code =>
                {
                    AssertFileContents(
                        $$"""
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class TestDbContext : DbContext
{
    public TestDbContext()
    {
    }

    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning {{DesignStrings.SensitiveInformationWarning}}
        => optionsBuilder.UseSqlServer("Initial Catalog=TestDatabase");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
""",
                        code.ContextFile);

                    Assert.Empty(code.AdditionalFiles);
                },
                model => Assert.Empty(model.GetEntityTypes()));

        [ConditionalFact]
        public Task SuppressConnectionStringWarning_works()
            => TestAsync(
                modelBuilder => { },
                new ModelCodeGenerationOptions { SuppressConnectionStringWarning = true },
                code =>
                {
                    AssertFileContents(
                        """
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class TestDbContext : DbContext
{
    public TestDbContext()
    {
    }

    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Initial Catalog=TestDatabase");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
""",
                        code.ContextFile);

                    Assert.Empty(code.AdditionalFiles);
                },
                model => Assert.Empty(model.GetEntityTypes()));

        [ConditionalFact]
        public Task SuppressOnConfiguring_works()
            => TestAsync(
                modelBuilder => { },
                new ModelCodeGenerationOptions { SuppressOnConfiguring = true },
                code =>
                {
                    AssertFileContents(
                        """
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
""",
                        code.ContextFile);

                    Assert.Empty(code.AdditionalFiles);
                },
                null);

        [ConditionalFact]
        public Task DbSets_without_nrt()
            => TestAsync(
                modelBuilder => modelBuilder.Entity("Entity"),
                new ModelCodeGenerationOptions
                {
                    UseNullableReferenceTypes = false,
                    SuppressConnectionStringWarning = true,
                    SuppressOnConfiguring = true
                },
                code =>
                {
                    Assert.Contains("DbSet<Entity> Entity { get; set; }", code.ContextFile.Code);
                    Assert.DoesNotContain("DbSet<Entity> Entity { get; set; } = null!;", code.ContextFile.Code);
                },
                null);

        [ConditionalFact]
        public Task DbSets_with_nrt()
            => TestAsync(
                modelBuilder => modelBuilder.Entity("Entity"),
                new ModelCodeGenerationOptions
                {
                    UseNullableReferenceTypes = true,
                    SuppressConnectionStringWarning = true,
                    SuppressOnConfiguring = true
                },
                code =>
                {
                    Assert.Contains("DbSet<Entity> Entity { get; set; }", code.ContextFile.Code);
                    Assert.DoesNotContain("DbSet<Entity> Entity { get; set; } = null!;", code.ContextFile.Code);
                },
                null);

        [ConditionalFact]
        public void Required_options_to_GenerateModel_are_not_null()
        {
            var generator = CreateServices()
                .AddSingleton<IProviderCodeGeneratorPlugin, TestCodeGeneratorPlugin>()
                .BuildServiceProvider(validateScopes: true)
                .GetServices<IModelCodeGenerator>()
                .Last(g => g is CSharpModelGenerator);

            Assert.StartsWith(
                CoreStrings.ArgumentPropertyNull(nameof(ModelCodeGenerationOptions.ContextName), "options"),
                Assert.Throws<ArgumentException>(
                        () =>
                            generator.GenerateModel(
                                new Model(),
                                new ModelCodeGenerationOptions { ContextName = null, ConnectionString = "Initial Catalog=TestDatabase" }))
                    .Message);

            Assert.StartsWith(
                CoreStrings.ArgumentPropertyNull(nameof(ModelCodeGenerationOptions.ConnectionString), "options"),
                Assert.Throws<ArgumentException>(
                    () =>
                        generator.GenerateModel(
                            new Model(),
                            new ModelCodeGenerationOptions { ContextName = "TestDbContext", ConnectionString = null })).Message);
        }

        [ConditionalFact]
        public void Plugins_work()
        {
            var generator = CreateServices()
                .AddSingleton<IProviderCodeGeneratorPlugin, TestCodeGeneratorPlugin>()
                .BuildServiceProvider(validateScopes: true)
                .GetServices<IModelCodeGenerator>()
                .Last(g => g is CSharpModelGenerator);

            var scaffoldedModel = generator.GenerateModel(
                new Model(),
                new ModelCodeGenerationOptions
                {
                    SuppressConnectionStringWarning = true,
                    ModelNamespace = "TestNamespace",
                    ContextName = "TestDbContext",
                    ConnectionString = "Initial Catalog=TestDatabase"
                });

            AssertContains(
                """
optionsBuilder
            .UseSqlServer("Initial Catalog=TestDatabase", x => x.SetProviderOption())
            .SetContextOption();
""",
                scaffoldedModel.ContextFile.Code);
        }

        [ConditionalFact]
        public Task IsRequired_is_generated_for_ref_property_without_nrt()
            => TestAsync(
                modelBuilder =>
                {
                    modelBuilder.Entity(
                        "Entity", x =>
                        {
                            x.Property<string>("RequiredString").IsRequired();
                            x.Property<string>("NonRequiredString");
                            x.Property<int>("RequiredInt");
                            x.Property<int?>("NonRequiredInt");
                        });
                },
                new ModelCodeGenerationOptions { UseNullableReferenceTypes = false },
                code =>
                {
                    Assert.Contains("Property(e => e.RequiredString).IsRequired()", code.ContextFile.Code);
                    Assert.DoesNotContain("NotRequiredString", code.ContextFile.Code);
                    Assert.DoesNotContain("RequiredInt", code.ContextFile.Code);
                    Assert.DoesNotContain("NotRequiredInt", code.ContextFile.Code);
                },
                model =>
                {
                    var entityType = model.FindEntityType("TestNamespace.Entity");
                    Assert.False(entityType.GetProperty("RequiredString").IsNullable);
                    Assert.True(entityType.GetProperty("NonRequiredString").IsNullable);
                    Assert.False(entityType.GetProperty("RequiredInt").IsNullable);
                    Assert.True(entityType.GetProperty("NonRequiredInt").IsNullable);
                });

        [ConditionalFact]
        public Task IsRequired_is_not_generated_for_ref_property_with_nrt()
            => TestAsync(
                modelBuilder =>
                {
                    modelBuilder.Entity(
                        "Entity", x =>
                        {
                            x.Property<string>("RequiredString").IsRequired();
                            x.Property<string>("NonRequiredString");
                            x.Property<int>("RequiredInt");
                            x.Property<int?>("NonRequiredInt");
                        });
                },
                new ModelCodeGenerationOptions { UseNullableReferenceTypes = true },
                code =>
                {
                    Assert.DoesNotContain("RequiredString", code.ContextFile.Code);
                    Assert.DoesNotContain("NotRequiredString", code.ContextFile.Code);
                    Assert.DoesNotContain("RequiredInt", code.ContextFile.Code);
                    Assert.DoesNotContain("NotRequiredInt", code.ContextFile.Code);
                },
                model =>
                {
                    var entityType = model.FindEntityType("TestNamespace.Entity");
                    Assert.False(entityType.GetProperty("RequiredString").IsNullable);
                    Assert.True(entityType.GetProperty("NonRequiredString").IsNullable);
                    Assert.False(entityType.GetProperty("RequiredInt").IsNullable);
                    Assert.True(entityType.GetProperty("NonRequiredInt").IsNullable);
                });

        [ConditionalFact]
        public Task Comments_use_fluent_api()
            => TestAsync(
                modelBuilder => modelBuilder.Entity(
                    "Entity",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<int>("Property")
                            .HasComment("An int property");
                    }),
                new ModelCodeGenerationOptions(),
                code => Assert.Contains(
                    ".HasComment(\"An int property\")",
                    code.ContextFile.Code),
                model => Assert.Equal(
                    "An int property",
                    model.FindEntityType("TestNamespace.Entity").GetProperty("Property").GetComment()));

        [ConditionalFact]
        public Task Entity_comments_use_fluent_api()
            => TestAsync(
                modelBuilder => modelBuilder.Entity(
                    "Entity",
                    x =>
                    {
                        x.ToTable(tb => tb.HasComment("An entity comment"));
                    }),
                new ModelCodeGenerationOptions(),
                code => Assert.Contains(
                    ".HasComment(\"An entity comment\")",
                    code.ContextFile.Code),
                model => Assert.Equal(
                    "An entity comment",
                    model.FindEntityType("TestNamespace.Entity").GetComment()));

        [ConditionalFact]
        public Task Views_work()
            => TestAsync(
                modelBuilder => modelBuilder.Entity("Vista").ToView("Vista"),
                new ModelCodeGenerationOptions { UseDataAnnotations = true },
                code => Assert.Contains(".ToView(\"Vista\")", code.ContextFile.Code),
                model =>
                {
                    var entityType = model.FindEntityType("TestNamespace.Vista");

                    Assert.NotNull(entityType.FindAnnotation(RelationalAnnotationNames.ViewDefinitionSql));
                    Assert.Equal("Vista", entityType.GetViewName());
                    Assert.Null(entityType.GetViewSchema());
                    Assert.Null(entityType.GetTableName());
                    Assert.Null(entityType.GetSchema());
                });

        [ConditionalFact]
        public Task ModelInDifferentNamespaceDbContext_works()
        {
            var modelGenerationOptions = new ModelCodeGenerationOptions
            {
                ContextNamespace = "TestNamespace", ModelNamespace = "AnotherNamespaceOfModel"
            };

            const string entityInAnotherNamespaceTypeName = "EntityInAnotherNamespace";

            return TestAsync(
                modelBuilder => modelBuilder.Entity(entityInAnotherNamespaceTypeName)
                , modelGenerationOptions
                , code => Assert.Contains(string.Concat("using ", modelGenerationOptions.ModelNamespace, ";"), code.ContextFile.Code)
                , model => Assert.NotNull(model.FindEntityType(string.Concat(modelGenerationOptions.ModelNamespace, ".", entityInAnotherNamespaceTypeName)))
            );
        }

        [ConditionalFact]
        public Task ModelSameNamespaceDbContext_works()
        {
            var modelGenerationOptions = new ModelCodeGenerationOptions { ContextNamespace = "TestNamespace" };

            const string entityInAnotherNamespaceTypeName = "EntityInAnotherNamespace";

            return TestAsync(
                modelBuilder => modelBuilder.Entity(entityInAnotherNamespaceTypeName)
                , modelGenerationOptions
                , code => Assert.DoesNotContain(string.Concat("using ", modelGenerationOptions.ModelNamespace, ";"), code.ContextFile.Code)
                , model => Assert.NotNull(model.FindEntityType(string.Concat(modelGenerationOptions.ModelNamespace, ".", entityInAnotherNamespaceTypeName)))
            );
        }

        [ConditionalFact]
        public Task ValueGenerated_works()
            => TestAsync(
                modelBuilder => modelBuilder.Entity(
                    "Entity",
                    x =>
                    {
                        x.Property<int>("ValueGeneratedOnAdd").ValueGeneratedOnAdd();
                        x.Property<int>("ValueGeneratedOnAddOrUpdate").ValueGeneratedOnAddOrUpdate();
                        x.Property<int>("ConcurrencyToken").IsConcurrencyToken();
                        x.Property<int>("ValueGeneratedOnUpdate").ValueGeneratedOnUpdate();
                        x.Property<int>("ValueGeneratedNever").ValueGeneratedNever();
                    }),
                new ModelCodeGenerationOptions(),
                code =>
                {
                    Assert.Contains("Property(e => e.ValueGeneratedOnAdd).ValueGeneratedOnAdd()", code.ContextFile.Code);
                    Assert.Contains("Property(e => e.ValueGeneratedOnAddOrUpdate).ValueGeneratedOnAddOrUpdate()", code.ContextFile.Code);
                    Assert.Contains("Property(e => e.ConcurrencyToken).IsConcurrencyToken()", code.ContextFile.Code);
                    Assert.Contains("Property(e => e.ValueGeneratedOnUpdate).ValueGeneratedOnUpdate()", code.ContextFile.Code);
                    Assert.Contains("Property(e => e.ValueGeneratedNever).ValueGeneratedNever()", code.ContextFile.Code);
                },
                model =>
                {
                    var entity = model.FindEntityType("TestNamespace.Entity");
                    Assert.Equal(ValueGenerated.OnAdd, entity.GetProperty("ValueGeneratedOnAdd").ValueGenerated);
                    Assert.Equal(ValueGenerated.OnAddOrUpdate, entity.GetProperty("ValueGeneratedOnAddOrUpdate").ValueGenerated);
                    Assert.True(entity.GetProperty("ConcurrencyToken").IsConcurrencyToken);
                    Assert.Equal(ValueGenerated.OnUpdate, entity.GetProperty("ValueGeneratedOnUpdate").ValueGenerated);
                    Assert.Equal(ValueGenerated.Never, entity.GetProperty("ValueGeneratedNever").ValueGenerated);
                });

        [ConditionalFact]
        public Task HasPrecision_works()
            => TestAsync(
                modelBuilder => modelBuilder.Entity(
                    "Entity",
                    x =>
                    {
                        x.Property<decimal>("HasPrecision").HasPrecision(12);
                        x.Property<decimal>("HasPrecisionAndScale").HasPrecision(14, 7);
                    }),
                new ModelCodeGenerationOptions(),
                code =>
                {
                    Assert.Contains("Property(e => e.HasPrecision).HasPrecision(12)", code.ContextFile.Code);
                    Assert.Contains("Property(e => e.HasPrecisionAndScale).HasPrecision(14, 7)", code.ContextFile.Code);
                },
                model =>
                {
                    var entity = model.FindEntityType("TestNamespace.Entity");
                    Assert.Equal(12, entity.GetProperty("HasPrecision").GetPrecision());
                    Assert.Null(entity.GetProperty("HasPrecision").GetScale());
                    Assert.Equal(14, entity.GetProperty("HasPrecisionAndScale").GetPrecision());
                    Assert.Equal(7, entity.GetProperty("HasPrecisionAndScale").GetScale());
                });

        [ConditionalFact]
        public Task Collation_works()
            => TestAsync(
                modelBuilder => modelBuilder.Entity("Entity").Property<string>("UseCollation").UseCollation("Some Collation"),
                new ModelCodeGenerationOptions(),
                code => Assert.Contains("Property(e => e.UseCollation).UseCollation(\"Some Collation\")", code.ContextFile.Code),
                model =>
                {
                    var entity = model.FindEntityType("TestNamespace.Entity");
                    Assert.Equal("Some Collation", entity.GetProperty("UseCollation").GetCollation());
                });

        [ConditionalFact]
        public Task ComputedColumnSql_works()
            => TestAsync(
                modelBuilder => modelBuilder.Entity("Entity").Property<string>("ComputedColumn").HasComputedColumnSql("1 + 2"),
                new ModelCodeGenerationOptions(),
                code => Assert.Contains(".HasComputedColumnSql(\"1 + 2\")", code.ContextFile.Code),
                model =>
                {
                    var entity = model.FindEntityType("TestNamespace.Entity");
                    Assert.Equal("1 + 2", entity.GetProperty("ComputedColumn").GetComputedColumnSql());
                });

        [ConditionalFact]
        public Task Column_with_default_value_only_uses_default_value()
            => TestAsync(
                serviceProvider => serviceProvider.GetService<IScaffoldingModelFactory>().Create(
                    BuildModelWithColumn("nvarchar(max)", null, "Hot"), new ModelReverseEngineerOptions()),
                new ModelCodeGenerationOptions(),
                code => Assert.Contains(".HasDefaultValue(\"Hot\")", code.ContextFile.Code),
                model =>
                {
                    var property = model.FindEntityType("TestNamespace.Table")!.GetProperty("Column");
                    Assert.Equal("Hot", property.GetDefaultValue());
                    Assert.Null(property.FindAnnotation(RelationalAnnotationNames.DefaultValueSql));
                });

        [ConditionalFact]
        public Task Column_with_default_value_sql_only_uses_default_value_sql()
            => TestAsync(
                serviceProvider => serviceProvider.GetService<IScaffoldingModelFactory>().Create(
                    BuildModelWithColumn("nvarchar(max)", "('Hot')", null), new ModelReverseEngineerOptions()),
                new ModelCodeGenerationOptions(),
                code => Assert.Contains(".HasDefaultValueSql(\"('Hot')\")", code.ContextFile.Code),
                model =>
                {
                    var property = model.FindEntityType("TestNamespace.Table")!.GetProperty("Column");
                    Assert.Equal("('Hot')", property.GetDefaultValueSql());
                    Assert.Null(property.FindAnnotation(RelationalAnnotationNames.DefaultValue));
                });

        [ConditionalFact]
        public Task Column_with_default_value_sql_and_default_value_uses_default_value()
            => TestAsync(
                serviceProvider => serviceProvider.GetService<IScaffoldingModelFactory>().Create(
                    BuildModelWithColumn("nvarchar(max)", "('Hot')", "Hot"), new ModelReverseEngineerOptions()),
                new ModelCodeGenerationOptions(),
                code => Assert.Contains(".HasDefaultValue(\"Hot\")", code.ContextFile.Code),
                model =>
                {
                    var property = model.FindEntityType("TestNamespace.Table")!.GetProperty("Column");
                    Assert.Equal("Hot", property.GetDefaultValue());
                    Assert.Null(property.FindAnnotation(RelationalAnnotationNames.DefaultValueSql));
                });

        [ConditionalFact]
        public Task Column_with_default_value_sql_and_default_value_where_value_is_CLR_default_uses_neither()
            => TestAsync(
                serviceProvider => serviceProvider.GetService<IScaffoldingModelFactory>().Create(
                    BuildModelWithColumn("int", "((0))", 0), new ModelReverseEngineerOptions()),
                new ModelCodeGenerationOptions(),
                code => Assert.DoesNotContain("HasDefaultValue", code.ContextFile.Code),
                model =>
                {
                    var property = model.FindEntityType("TestNamespace.Table")!.GetProperty("Column");
                    Assert.Null(property.FindAnnotation(RelationalAnnotationNames.DefaultValue));
                    Assert.Null(property.FindAnnotation(RelationalAnnotationNames.DefaultValueSql));
                });

        [ConditionalFact]
        public Task IsUnicode_works()
            => TestAsync(
                modelBuilder =>
                {
                    modelBuilder.Entity("Entity").Property<string>("UnicodeColumn").IsUnicode();
                    modelBuilder.Entity("Entity").Property<string>("NonUnicodeColumn").IsUnicode(false);
                },
                new ModelCodeGenerationOptions(),
                code =>
                {
                    Assert.Contains("Property(e => e.UnicodeColumn).IsUnicode()", code.ContextFile.Code);
                    Assert.Contains("Property(e => e.NonUnicodeColumn).IsUnicode(false)", code.ContextFile.Code);
                },
                model =>
                {
                    var entity = model.FindEntityType("TestNamespace.Entity");
                    Assert.True(entity.GetProperty("UnicodeColumn").IsUnicode());
                    Assert.False(entity.GetProperty("NonUnicodeColumn").IsUnicode());
                });

        [ConditionalFact]
        public Task ComputedColumnSql_works_stored()
            => TestAsync(
                modelBuilder => modelBuilder.Entity("Entity").Property<string>("ComputedColumn")
                    .HasComputedColumnSql("1 + 2", stored: true),
                new ModelCodeGenerationOptions(),
                code => Assert.Contains(".HasComputedColumnSql(\"1 + 2\", true)", code.ContextFile.Code),
                model =>
                {
                    var entity = model.FindEntityType("TestNamespace.Entity");
                    Assert.True(entity.GetProperty("ComputedColumn").GetIsStored());
                });

        [ConditionalFact]
        public Task ComputedColumnSql_works_unspecified()
            => TestAsync(
                modelBuilder => modelBuilder.Entity("Entity").Property<string>("ComputedColumn").HasComputedColumnSql(),
                new ModelCodeGenerationOptions(),
                code => Assert.Contains(".HasComputedColumnSql()", code.ContextFile.Code),
                model =>
                {
                    var entity = model.FindEntityType("TestNamespace.Entity");
                    Assert.Empty(entity.GetProperty("ComputedColumn").GetComputedColumnSql());
                });

        [ConditionalFact]
        public Task DefaultValue_works_unspecified()
            => TestAsync(
                modelBuilder => modelBuilder.Entity("Entity").Property<string>("DefaultedColumn").HasDefaultValue(),
                new ModelCodeGenerationOptions(),
                code => Assert.Contains(".HasDefaultValue()", code.ContextFile.Code),
                model =>
                {
                    var entity = model.FindEntityType("TestNamespace.Entity");
                    Assert.Equal(DBNull.Value, entity.GetProperty("DefaultedColumn").GetDefaultValue());
                });

        [ConditionalFact]
        public Task DefaultValueSql_works_unspecified()
            => TestAsync(
                modelBuilder => modelBuilder.Entity("Entity").Property<string>("DefaultedColumn").HasDefaultValueSql(),
                new ModelCodeGenerationOptions(),
                code => Assert.Contains(".HasDefaultValueSql()", code.ContextFile.Code),
                model =>
                {
                    var entity = model.FindEntityType("TestNamespace.Entity");
                    Assert.Empty(entity.GetProperty("DefaultedColumn").GetDefaultValueSql());
                });

        [ConditionalFact]
        public Task Entity_with_indexes_and_use_data_annotations_false_always_generates_fluent_API()
            => TestAsync(
                modelBuilder => modelBuilder
                    .Entity(
                        "EntityWithIndexes",
                        x =>
                        {
                            x.Property<int>("Id");
                            x.Property<int>("A");
                            x.Property<int>("B");
                            x.Property<int>("C");
                            x.HasKey("Id");
                            x.HasIndex(["A", "B"], "IndexOnAAndB")
                                .IsUnique()
                                .IsDescending(false, true);
                            x.HasIndex(["B", "C"], "IndexOnBAndC")
                                .HasFilter("Filter SQL")
                                .HasAnnotation("AnnotationName", "AnnotationValue");
                        }),
                new ModelCodeGenerationOptions { UseDataAnnotations = false },
                code =>
                {
                    AssertFileContents(
                        $$"""
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class TestDbContext : DbContext
{
    public TestDbContext()
    {
    }

    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<EntityWithIndexes> EntityWithIndexes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning {{DesignStrings.SensitiveInformationWarning}}
        => optionsBuilder.UseSqlServer("Initial Catalog=TestDatabase");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EntityWithIndexes>(entity =>
        {
            entity.HasIndex(e => new { e.A, e.B }, "IndexOnAAndB")
                .IsUnique()
                .IsDescending(false, true);

            entity.HasIndex(e => new { e.B, e.C }, "IndexOnBAndC")
                .HasFilter("Filter SQL")
                .HasAnnotation("AnnotationName", "AnnotationValue");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
""",
                        code.ContextFile);
                },
                model =>
                    Assert.Equal(2, model.FindEntityType("TestNamespace.EntityWithIndexes").GetIndexes().Count()));

        [ConditionalFact]
        public Task Entity_with_indexes_and_use_data_annotations_true_generates_fluent_API_only_for_indexes_with_annotations()
            => TestAsync(
                modelBuilder => modelBuilder
                    .Entity(
                        "EntityWithIndexes",
                        x =>
                        {
                            x.Property<int>("Id");
                            x.Property<int>("A");
                            x.Property<int>("B");
                            x.Property<int>("C");
                            x.HasKey("Id");
                            x.HasIndex(["A", "B"], "IndexOnAAndB")
                                .IsUnique()
                                .IsDescending(false, true);
                            x.HasIndex(["B", "C"], "IndexOnBAndC")
                                .HasFilter("Filter SQL")
                                .HasAnnotation("AnnotationName", "AnnotationValue");
                        }),
                new ModelCodeGenerationOptions { UseDataAnnotations = true },
                code =>
                {
                    AssertFileContents(
                        $$"""
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class TestDbContext : DbContext
{
    public TestDbContext()
    {
    }

    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<EntityWithIndexes> EntityWithIndexes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning {{DesignStrings.SensitiveInformationWarning}}
        => optionsBuilder.UseSqlServer("Initial Catalog=TestDatabase");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EntityWithIndexes>(entity =>
        {
            entity.HasIndex(e => new { e.B, e.C }, "IndexOnBAndC")
                .HasFilter("Filter SQL")
                .HasAnnotation("AnnotationName", "AnnotationValue");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
""",
                        code.ContextFile);
                },
                model =>
                    Assert.Equal(2, model.FindEntityType("TestNamespace.EntityWithIndexes").GetIndexes().Count()));

        [ConditionalFact]
        public Task Indexes_with_descending()
            => TestAsync(
                modelBuilder => modelBuilder
                    .Entity(
                        "EntityWithIndexes",
                        x =>
                        {
                            x.Property<int>("Id");
                            x.Property<int>("X");
                            x.Property<int>("Y");
                            x.Property<int>("Z");
                            x.HasKey("Id");
                            x.HasIndex(["X", "Y", "Z"], "IX_unspecified");
                            x.HasIndex(["X", "Y", "Z"], "IX_empty")
                                .IsDescending();
                            x.HasIndex(["X", "Y", "Z"], "IX_all_ascending")
                                .IsDescending(false, false, false);
                            x.HasIndex(["X", "Y", "Z"], "IX_all_descending")
                                .IsDescending(true, true, true);
                            x.HasIndex(["X", "Y", "Z"], "IX_mixed")
                                .IsDescending(false, true, false);
                        }),
                new ModelCodeGenerationOptions { UseDataAnnotations = false },
                code =>
                {
                    AssertFileContents(
                        $$"""
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class TestDbContext : DbContext
{
    public TestDbContext()
    {
    }

    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<EntityWithIndexes> EntityWithIndexes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning {{DesignStrings.SensitiveInformationWarning}}
        => optionsBuilder.UseSqlServer("Initial Catalog=TestDatabase");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EntityWithIndexes>(entity =>
        {
            entity.HasIndex(e => new { e.X, e.Y, e.Z }, "IX_all_ascending");

            entity.HasIndex(e => new { e.X, e.Y, e.Z }, "IX_all_descending").IsDescending();

            entity.HasIndex(e => new { e.X, e.Y, e.Z }, "IX_empty").IsDescending();

            entity.HasIndex(e => new { e.X, e.Y, e.Z }, "IX_mixed").IsDescending(false, true, false);

            entity.HasIndex(e => new { e.X, e.Y, e.Z }, "IX_unspecified");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
""",
                        code.ContextFile);
                },
                model =>
                {
                    var entityType = model.FindEntityType("TestNamespace.EntityWithIndexes")!;
                    Assert.Equal(5, entityType.GetIndexes().Count());

                    var unspecifiedIndex = Assert.Single(entityType.GetIndexes(), i => i.Name == "IX_unspecified");
                    Assert.Null(unspecifiedIndex.IsDescending);

                    var emptyIndex = Assert.Single(entityType.GetIndexes(), i => i.Name == "IX_empty");
                    Assert.Equal([], emptyIndex.IsDescending);

                    var allAscendingIndex = Assert.Single(entityType.GetIndexes(), i => i.Name == "IX_all_ascending");
                    Assert.Null(allAscendingIndex.IsDescending);

                    var allDescendingIndex = Assert.Single(entityType.GetIndexes(), i => i.Name == "IX_all_descending");
                    Assert.Equal([], allDescendingIndex.IsDescending);

                    var mixedIndex = Assert.Single(entityType.GetIndexes(), i => i.Name == "IX_mixed");
                    Assert.Equal(new[] { false, true, false }, mixedIndex.IsDescending);
                });

        [ConditionalFact]
        public Task Entity_lambda_uses_correct_identifiers()
            => TestAsync(
                modelBuilder =>
                {
                    modelBuilder.Entity(
                        "PrincipalEntity", b =>
                        {
                            b.Property<int>("Id");
                            b.Property<int>("PrincipalId");
                            b.Property<int>("AlternateId");
                            b.HasKey("AlternateId");
                        });
                    modelBuilder.Entity(
                        "DependentEntity", b =>
                        {
                            b.Property<int>("Id");
                            b.Property<int>("DependentId");
                            b.HasOne("PrincipalEntity", "NavigationToPrincipal")
                                .WithOne("NavigationToDependent")
                                .HasForeignKey("DependentEntity", "DependentId")
                                .HasPrincipalKey("PrincipalEntity", "PrincipalId");
                        });
                },
                new ModelCodeGenerationOptions { UseDataAnnotations = false },
                code =>
                {
                    AssertFileContents(
                        $$"""
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class TestDbContext : DbContext
{
    public TestDbContext()
    {
    }

    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<DependentEntity> DependentEntity { get; set; }

    public virtual DbSet<PrincipalEntity> PrincipalEntity { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning {{DesignStrings.SensitiveInformationWarning}}
        => optionsBuilder.UseSqlServer("Initial Catalog=TestDatabase");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DependentEntity>(entity =>
        {
            entity.HasIndex(e => e.DependentId, "IX_DependentEntity_DependentId").IsUnique();

            entity.HasOne(d => d.NavigationToPrincipal).WithOne(p => p.NavigationToDependent)
                .HasPrincipalKey<PrincipalEntity>(p => p.PrincipalId)
                .HasForeignKey<DependentEntity>(d => d.DependentId);
        });

        modelBuilder.Entity<PrincipalEntity>(entity =>
        {
            entity.HasKey(e => e.AlternateId);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
""",
                        code.ContextFile);
                },
                model => { });

        [ConditionalFact]
        public Task Column_type_is_not_scaffolded_as_annotation()
            => TestAsync(
                modelBuilder => modelBuilder
                    .Entity(
                        "Employee",
                        x =>
                        {
                            x.Property<int>("Id");
                            x.Property<DateTime>("HireDate").HasColumnType("date").HasColumnName("hiring_date");
                        }),
                new ModelCodeGenerationOptions { UseDataAnnotations = false },
                code =>
                {
                    AssertFileContents(
                        $$"""
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class TestDbContext : DbContext
{
    public TestDbContext()
    {
    }

    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Employee> Employee { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning {{DesignStrings.SensitiveInformationWarning}}
        => optionsBuilder.UseSqlServer("Initial Catalog=TestDatabase");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.Property(e => e.HireDate)
                .HasColumnType("date")
                .HasColumnName("hiring_date");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
""",
                        code.ContextFile);
                },
                model =>
                    Assert.Equal("date", model.FindEntityType("TestNamespace.Employee").GetProperty("HireDate").GetConfiguredColumnType()));

        [ConditionalFact]
        public Task Is_fixed_length_annotation_should_be_scaffolded_without_optional_parameter()
            => TestAsync(
                modelBuilder => modelBuilder
                    .Entity(
                        "Employee",
                        x =>
                        {
                            x.Property<int>("Id");
                            x.Property<string>("Name").HasMaxLength(5).IsFixedLength();
                        }),
                new ModelCodeGenerationOptions { UseDataAnnotations = false },
                code => Assert.Contains(".IsFixedLength()", code.ContextFile.Code),
                model =>
                    Assert.True(model.FindEntityType("TestNamespace.Employee").GetProperty("Name").IsFixedLength()));

        [ConditionalFact]
        public Task Global_namespace_works()
            => TestAsync(
                modelBuilder => modelBuilder.Entity("MyEntity"),
                new ModelCodeGenerationOptions { ModelNamespace = string.Empty },
                code =>
                {
                    AssertFileContents(
                        $$"""
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

public partial class TestDbContext : DbContext
{
    public TestDbContext()
    {
    }

    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<MyEntity> MyEntity { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning {{DesignStrings.SensitiveInformationWarning}}
        => optionsBuilder.UseSqlServer("Initial Catalog=TestDatabase");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MyEntity>(entity =>
        {
            entity.HasNoKey();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
""",
                        code.ContextFile);

                    Assert.DoesNotContain("namespace ", Assert.Single(code.AdditionalFiles).Code);
                },
                model =>
                {
                    Assert.NotNull(model.FindEntityType("MyEntity"));
                });

        [ConditionalFact]
        public Task Global_namespace_works_just_context()
            => TestAsync(
                modelBuilder => modelBuilder.Entity("MyEntity"),
                new ModelCodeGenerationOptions { ModelNamespace = "TestNamespace", ContextNamespace = string.Empty },
                code =>
                {
                    Assert.Contains("using TestNamespace;", code.ContextFile.Code);
                    Assert.DoesNotContain("namespace ", code.ContextFile.Code);
                    Assert.Contains("namespace TestNamespace", Assert.Single(code.AdditionalFiles).Code);
                },
                model =>
                {
                    Assert.NotNull(model.FindEntityType("TestNamespace.MyEntity"));
                });

        [ConditionalFact]
        public Task Global_namespace_works_just_model()
            => TestAsync(
                modelBuilder => modelBuilder.Entity("MyEntity"),
                new ModelCodeGenerationOptions { ModelNamespace = string.Empty, ContextNamespace = "TestNamespace" },
                code =>
                {
                    Assert.Contains("namespace TestNamespace", code.ContextFile.Code);
                    Assert.DoesNotContain("namespace ", Assert.Single(code.AdditionalFiles).Code);
                },
                model =>
                {
                    Assert.NotNull(model.FindEntityType("MyEntity"));
                });

        [ConditionalFact]
        public Task Fluent_calls_in_custom_namespaces_work()
            => TestAsync(
                modelBuilder => TestModelBuilderExtensions.TestFluentApiCall(modelBuilder),
                new ModelCodeGenerationOptions { SuppressOnConfiguring = true },
                code =>
                {
                    AssertFileContents(
                        """
using System;
using System.Collections.Generic;
using CustomTestNamespace;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.TestFluentApiCall();

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
""",
                        code.ContextFile);

                    Assert.Empty(code.AdditionalFiles);
                },
                model => Assert.Empty(model.GetEntityTypes()),
                skipBuild: true);

        [ConditionalFact]
        public async Task Temporal_table_works()
            // Shadow properties. Issue #26007.
            => Assert.Equal(
                SqlServerStrings.TemporalPeriodPropertyMustBeInShadowState("Customer", "PeriodStart"),
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () =>
                        TestAsync(
                            modelBuilder => modelBuilder.Entity(
                                "Customer", e =>
                                {
                                    e.Property<int>("Id");
                                    e.Property<string>("Name");
                                    e.HasKey("Id");
                                    e.ToTable(tb => tb.IsTemporal());
                                }),
                            new ModelCodeGenerationOptions { UseDataAnnotations = false },
                            code =>
                            {
                                AssertFileContents(
                                    $$"""
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class TestDbContext : DbContext
{
    public TestDbContext()
    {
    }

    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Customer> Customer { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning {{DesignStrings.SensitiveInformationWarning}}
        => optionsBuilder.UseSqlServer("Initial Catalog=TestDatabase");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable(tb => tb.IsTemporal(ttb =>
                    {
                        ttb.UseHistoryTable("CustomerHistory");
                        ttb
                            .HasPeriodStart("PeriodStart")
                            .HasColumnName("PeriodStart");
                        ttb
                            .HasPeriodEnd("PeriodEnd")
                            .HasColumnName("PeriodEnd");
                    }));
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
""",
                                    code.ContextFile);
                            },
                            model =>
                            {
                                // TODO
                            }))).Message);

        [ConditionalFact]
        public Task Sequences_work()
            => TestAsync(
                modelBuilder => modelBuilder.HasSequence<int>("EvenNumbers", "dbo")
                    .StartsAt(2)
                    .IncrementsBy(2)
                    .HasMin(2)
                    .HasMax(100)
                    .IsCyclic()
                    .UseCache(20),
                new ModelCodeGenerationOptions(),
                code => AssertContains(
                    """
.HasSequence<int>("EvenNumbers", "dbo")
            .StartsAt(2L)
            .IncrementsBy(2)
            .HasMin(2L)
            .HasMax(100L)
            .IsCyclic()
            .UseCache(20);
""",
                    code.ContextFile.Code),
                model =>
                {
                    var sequence = model.FindSequence("EvenNumbers", "dbo");
                    Assert.NotNull(sequence);

                    Assert.Equal(typeof(int), sequence.Type);
                    Assert.Equal(2, sequence.StartValue);
                    Assert.Equal(2, sequence.IncrementBy);
                    Assert.Equal(2, sequence.MinValue);
                    Assert.Equal(100, sequence.MaxValue);
                    Assert.True(sequence.IsCyclic);
                    Assert.True(sequence.IsCached);
                    Assert.Equal(20, sequence.CacheSize);
                });

        [ConditionalFact]
        public Task Trigger_works()
            => TestAsync(
                modelBuilder => modelBuilder
                    .Entity(
                        "Employee",
                        x =>
                        {
                            x.Property<int>("Id");
                            x.ToTable(
                                tb =>
                                {
                                    tb.HasTrigger("Trigger1");
                                    tb.HasTrigger("Trigger2");
                                });
                        }),
                new ModelCodeGenerationOptions { UseDataAnnotations = false },
                code =>
                {
                    AssertFileContents(
                        $$"""
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace;

public partial class TestDbContext : DbContext
{
    public TestDbContext()
    {
    }

    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Employee> Employee { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning {{DesignStrings.SensitiveInformationWarning}}
        => optionsBuilder.UseSqlServer("Initial Catalog=TestDatabase");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>(entity =>
        {
            entity
                .ToTable(tb =>
                {
                    tb.HasTrigger("Trigger1");
                    tb.HasTrigger("Trigger2");
                })
                .HasAnnotation("SqlServer:UseSqlOutputClause", false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
""",
                        code.ContextFile);
                },
                model =>
                {
                    var entityType = model.FindEntityType("TestNamespace.Employee")!;
                    var triggers = entityType.GetDeclaredTriggers();

                    Assert.Collection(
                        triggers,
                        t => Assert.Equal("Trigger1", t.GetDatabaseName()),
                        t => Assert.Equal("Trigger2", t.GetDatabaseName()));
                });

        [ConditionalFact]
        public Task ValueGenerationStrategy_works_when_none()
            => TestAsync(
                modelBuilder => modelBuilder.Entity(
                    "Channel",
                    x =>
                    {
                        x.Property<int>("Id")
                            .Metadata.SetValueGenerationStrategy(SqlServerValueGenerationStrategy.None);
                    }),
                new ModelCodeGenerationOptions(),
                code =>
                {
                    AssertFileContents(
                        $$"""
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace TestNamespace;

public partial class TestDbContext : DbContext
{
    public TestDbContext()
    {
    }

    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Channel> Channel { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning {{DesignStrings.SensitiveInformationWarning}}
        => optionsBuilder.UseSqlServer("Initial Catalog=TestDatabase");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Channel>(entity =>
        {
            entity.Property(e => e.Id).HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
""",
                        code.ContextFile);
                },
                model =>
                {
                    var entityType = Assert.Single(model.GetEntityTypes());
                    var property = Assert.Single(entityType.GetProperties());
                    Assert.Equal(SqlServerValueGenerationStrategy.None, property.GetValueGenerationStrategy());
                });

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public Task ColumnOrder_is_ignored(bool useDataAnnotations)
            => TestAsync(
                modelBuilder => modelBuilder.Entity("Entity").Property<string>("Property").HasColumnOrder(1),
                new ModelCodeGenerationOptions { UseDataAnnotations = useDataAnnotations },
                code =>
                {
                    Assert.DoesNotContain(".HasColumnOrder(1)", code.ContextFile.Code);
                    Assert.DoesNotContain("[Column(Order = 1)]", code.AdditionalFiles[0].Code);
                },
                model =>
                {
                    var entity = model.FindEntityType("TestNamespace.Entity");
                    Assert.Null(entity.GetProperty("Property").GetColumnOrder());
                });

        protected override IServiceCollection AddModelServices(IServiceCollection services)
            => services.Replace(ServiceDescriptor.Singleton<IRelationalAnnotationProvider, TestModelAnnotationProvider>());

        protected override IServiceCollection AddScaffoldingServices(IServiceCollection services)
            => services.Replace(ServiceDescriptor.Singleton<IAnnotationCodeGenerator, TestModelAnnotationCodeGenerator>());

        private class TestModelAnnotationProvider(RelationalAnnotationProviderDependencies dependencies) : SqlServerAnnotationProvider(dependencies)
        {
            public override IEnumerable<IAnnotation> For(IRelationalModel database, bool designTime)
            {
                foreach (var annotation in base.For(database, designTime))
                {
                    yield return annotation;
                }

                if (database["Test:TestModelAnnotation"] is string annotationValue)
                {
                    yield return new Annotation("Test:TestModelAnnotation", annotationValue);
                }
            }
        }

        private class TestModelAnnotationCodeGenerator(AnnotationCodeGeneratorDependencies dependencies) : SqlServerAnnotationCodeGenerator(dependencies)
        {
            private static readonly MethodInfo _testFluentApiCallMethodInfo
                = typeof(TestModelBuilderExtensions).GetRuntimeMethod(
                    nameof(TestModelBuilderExtensions.TestFluentApiCall), [typeof(ModelBuilder)])!;

            protected override MethodCallCodeFragment GenerateFluentApi(IModel model, IAnnotation annotation)
                => annotation.Name switch
                {
                    "Test:TestModelAnnotation" => new MethodCallCodeFragment(_testFluentApiCallMethodInfo),
                    _ => base.GenerateFluentApi(model, annotation)
                };
        }

        private class TestCodeGeneratorPlugin : ProviderCodeGeneratorPlugin
        {
            private static readonly MethodInfo _setProviderOptionMethodInfo
                = typeof(TestCodeGeneratorPlugin).GetRuntimeMethod(
                    nameof(SetProviderOption), [typeof(SqlServerDbContextOptionsBuilder)]);

            private static readonly MethodInfo _setContextOptionMethodInfo
                = typeof(TestCodeGeneratorPlugin).GetRuntimeMethod(
                    nameof(SetContextOption), [typeof(DbContextOptionsBuilder)]);

            public override MethodCallCodeFragment GenerateProviderOptions()
                => new(_setProviderOptionMethodInfo);

            public override MethodCallCodeFragment GenerateContextOptions()
                => new(_setContextOptionMethodInfo);

            public static SqlServerDbContextOptionsBuilder SetProviderOption(SqlServerDbContextOptionsBuilder optionsBuilder)
                => throw new NotSupportedException();

            public static SqlServerDbContextOptionsBuilder SetContextOption(DbContextOptionsBuilder optionsBuilder)
                => throw new NotSupportedException();
        }
    }
}

namespace CustomTestNamespace
{
    internal static class TestModelBuilderExtensions
    {
        public static ModelBuilder TestFluentApiCall(ModelBuilder modelBuilder)
        {
            modelBuilder.Model.SetAnnotation("Test:TestModelAnnotation", "foo");

            return modelBuilder;
        }
    }
}
