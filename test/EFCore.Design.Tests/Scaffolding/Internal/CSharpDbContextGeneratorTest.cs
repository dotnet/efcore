// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    public class CSharpDbContextGeneratorTest : ModelCodeGeneratorTestBase
    {
        [ConditionalFact]
        public void Empty_model()
        {
            Test(
                modelBuilder => { },
                new ModelCodeGenerationOptions(),
                code =>
                {
                    AssertFileContents(
                        @"using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace TestNamespace
{
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
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning "
                        + DesignStrings.SensitiveInformationWarning
                        + @"
                optionsBuilder.UseSqlServer(""Initial Catalog=TestDatabase"");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
",
                        code.ContextFile);

                    Assert.Empty(code.AdditionalFiles);
                },
                model => Assert.Empty(model.GetEntityTypes()));
        }

        [ConditionalFact]
        public void SuppressConnectionStringWarning_works()
        {
            Test(
                modelBuilder => { },
                new ModelCodeGenerationOptions { SuppressConnectionStringWarning = true },
                code =>
                {
                    AssertFileContents(
                        @"using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace TestNamespace
{
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
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(""Initial Catalog=TestDatabase"");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
",
                        code.ContextFile);

                    Assert.Empty(code.AdditionalFiles);
                },
                model => Assert.Empty(model.GetEntityTypes()));
        }

        [ConditionalFact]
        public void SuppressOnConfiguring_works()
        {
            Test(
                modelBuilder => { },
                new ModelCodeGenerationOptions { SuppressOnConfiguring = true },
                code =>
                {
                    AssertFileContents(
                        @"using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace TestNamespace
{
    public partial class TestDbContext : DbContext
    {
        public TestDbContext()
        {
        }

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
}
",
                        code.ContextFile);

                    Assert.Empty(code.AdditionalFiles);
                },
                null);
        }

        [ConditionalFact]
        public void DbSets_without_nrt()
        {
            Test(
                modelBuilder => modelBuilder.Entity("Entity"),
                new ModelCodeGenerationOptions
                {
                    UseNullableReferenceTypes = false,
                    SuppressConnectionStringWarning = true,
                    SuppressOnConfiguring = true
                },
                code =>
                {
                    Assert.Contains("DbSet<Entity> Entity { get; set; }" + Environment.NewLine, code.ContextFile.Code);
                },
                null);
        }

        [ConditionalFact]
        public void DbSets_with_nrt()
        {
            Test(
                modelBuilder => modelBuilder.Entity("Entity"),
                new ModelCodeGenerationOptions
                {
                    UseNullableReferenceTypes = true,
                    SuppressConnectionStringWarning = true,
                    SuppressOnConfiguring = true
                },
                code =>
                {
                    Assert.Contains("DbSet<Entity> Entity { get; set; } = null!;", code.ContextFile.Code);
                },
                null);
        }

        [ConditionalFact]
        public void Required_options_to_GenerateModel_are_not_null()
        {
            var generator = CreateServices()
                .AddSingleton<IProviderCodeGeneratorPlugin, TestCodeGeneratorPlugin>()
                .BuildServiceProvider(validateScopes: true)
                .GetRequiredService<IModelCodeGenerator>();

            Assert.StartsWith(
                CoreStrings.ArgumentPropertyNull(nameof(ModelCodeGenerationOptions.ContextName), "options"),
                Assert.Throws<ArgumentException>(
                    () =>
                        generator.GenerateModel(
                            new Model(),
                            new ModelCodeGenerationOptions
                            {
                                ContextName = null,
                                ConnectionString = "Initial Catalog=TestDatabase"
                            })).Message);

            Assert.StartsWith(
                CoreStrings.ArgumentPropertyNull(nameof(ModelCodeGenerationOptions.ConnectionString), "options"),
                Assert.Throws<ArgumentException>(
                    () =>
                        generator.GenerateModel(
                            new Model(),
                            new ModelCodeGenerationOptions
                            {
                                ContextName = "TestDbContext",
                                ConnectionString = null
                            })).Message);
        }

        [ConditionalFact]
        public void Plugins_work()
        {
            var generator = CreateServices()
                .AddSingleton<IProviderCodeGeneratorPlugin, TestCodeGeneratorPlugin>()
                .BuildServiceProvider(validateScopes: true)
                .GetRequiredService<IModelCodeGenerator>();

            var scaffoldedModel = generator.GenerateModel(
                new Model(),
                new ModelCodeGenerationOptions
                {
                    SuppressConnectionStringWarning = true,
                    ModelNamespace = "TestNamespace",
                    ContextName = "TestDbContext",
                    ConnectionString = "Initial Catalog=TestDatabase"
                });

            Assert.Contains(
                @"optionsBuilder.UseSqlServer(""Initial Catalog=TestDatabase"", x => x.SetProviderOption()).SetContextOption();",
                scaffoldedModel.ContextFile.Code);
        }

        [ConditionalFact]
        public void IsRequired_is_generated_for_ref_property_without_nrt()
        {
            Test(
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
                code => {
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
        }

        [ConditionalFact]
        public void IsRequired_is_not_generated_for_ref_property_with_nrt()
        {
            Test(
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
                code => {
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
        }

        [ConditionalFact]
        public void Comments_use_fluent_api()
        {
            Test(
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
        }

        [ConditionalFact]
        public void Entity_comments_use_fluent_api()
        {
            Test(
                modelBuilder => modelBuilder.Entity(
                    "Entity",
                    x =>
                    {
                        x.HasComment("An entity comment");
                    }),
                new ModelCodeGenerationOptions(),
                code => Assert.Contains(
                    ".HasComment(\"An entity comment\")",
                    code.ContextFile.Code),
                model => Assert.Equal(
                    "An entity comment",
                    model.FindEntityType("TestNamespace.Entity").GetComment()));
        }

        [ConditionalFact]
        public void Views_work()
        {
            Test(
                modelBuilder => modelBuilder.Entity("Vista").ToView("Vista"),
                new ModelCodeGenerationOptions { UseDataAnnotations = true },
                code => Assert.Contains("entity.ToView(\"Vista\");", code.ContextFile.Code),
                model => {
                    var entityType = model.FindEntityType("TestNamespace.Vista");

                    Assert.NotNull(entityType.FindAnnotation(RelationalAnnotationNames.ViewDefinitionSql));
                    Assert.Equal("Vista", entityType.GetViewName());
                    Assert.Null(entityType.GetViewSchema());
                    Assert.Null(entityType.GetTableName());
                    Assert.Null(entityType.GetSchema());
                });
        }

        [ConditionalFact]
        public void ModelInDifferentNamespaceDbContext_works()
        {
            var modelGenerationOptions = new ModelCodeGenerationOptions
            {
                ContextNamespace = "TestNamespace", ModelNamespace = "AnotherNamespaceOfModel"
            };

            const string entityInAnotherNamespaceTypeName = "EntityInAnotherNamespace";

            Test(
                modelBuilder => modelBuilder.Entity(entityInAnotherNamespaceTypeName)
                , modelGenerationOptions
                , code => Assert.Contains(string.Concat("using ", modelGenerationOptions.ModelNamespace, ";"), code.ContextFile.Code)
                , model => Assert.NotNull(model.FindEntityType(string.Concat(modelGenerationOptions.ModelNamespace, ".", entityInAnotherNamespaceTypeName)))
            );
        }

        [ConditionalFact]
        public void ModelSameNamespaceDbContext_works()
        {
            var modelGenerationOptions = new ModelCodeGenerationOptions { ContextNamespace = "TestNamespace" };

            const string entityInAnotherNamespaceTypeName = "EntityInAnotherNamespace";

            Test(
                modelBuilder => modelBuilder.Entity(entityInAnotherNamespaceTypeName)
                , modelGenerationOptions
                , code => Assert.DoesNotContain(string.Concat("using ", modelGenerationOptions.ModelNamespace, ";"), code.ContextFile.Code)
                , model => Assert.NotNull(model.FindEntityType(string.Concat(modelGenerationOptions.ModelNamespace, ".", entityInAnotherNamespaceTypeName)))
            );
        }

        [ConditionalFact]
        public void ValueGenerated_works()
        {
            Test(
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
                    Assert.Contains(
                        @$"Property(e => e.ValueGeneratedOnAdd){Environment.NewLine}                    .ValueGeneratedOnAdd()",
                        code.ContextFile.Code);
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
        }

        [ConditionalFact]
        public void HasPrecision_works()
        {
            Test(
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
        }

        [ConditionalFact]
        public void Collation_works()
        {
            Test(
                modelBuilder => modelBuilder.Entity("Entity").Property<string>("UseCollation").UseCollation("Some Collation"),
                new ModelCodeGenerationOptions(),
                code => Assert.Contains("Property(e => e.UseCollation).UseCollation(\"Some Collation\")", code.ContextFile.Code),
                model =>
                {
                    var entity = model.FindEntityType("TestNamespace.Entity");
                    Assert.Equal("Some Collation", entity.GetProperty("UseCollation").GetCollation());
                });
        }

        [ConditionalFact]
        public void ComputedColumnSql_works()
        {
            Test(
                modelBuilder => modelBuilder.Entity("Entity").Property<string>("ComputedColumn").HasComputedColumnSql("1 + 2"),
                new ModelCodeGenerationOptions(),
                code => Assert.Contains(".HasComputedColumnSql(\"1 + 2\")", code.ContextFile.Code),
                model =>
                {
                    var entity = model.FindEntityType("TestNamespace.Entity");
                    Assert.Equal("1 + 2", entity.GetProperty("ComputedColumn").GetComputedColumnSql());
                });
        }

        [ConditionalFact]
        public void IsUnicode_works()
        {
            Test(
                modelBuilder => {
                    modelBuilder.Entity("Entity").Property<string>("UnicodeColumn").IsUnicode();
                    modelBuilder.Entity("Entity").Property<string>("NonUnicodeColumn").IsUnicode(false);
                },
                new ModelCodeGenerationOptions(),
                code => {
                    Assert.Contains("Property(e => e.UnicodeColumn).IsUnicode()", code.ContextFile.Code);
                    Assert.Contains("Property(e => e.NonUnicodeColumn).IsUnicode(false)", code.ContextFile.Code);
                },
                model =>
                {
                    var entity = model.FindEntityType("TestNamespace.Entity");
                    Assert.True(entity.GetProperty("UnicodeColumn").IsUnicode());
                    Assert.False(entity.GetProperty("NonUnicodeColumn").IsUnicode());
                });
        }

        [ConditionalFact]
        public void ComputedColumnSql_works_stored()
        {
            Test(
                modelBuilder => modelBuilder.Entity("Entity").Property<string>("ComputedColumn")
                    .HasComputedColumnSql("1 + 2", stored: true),
                new ModelCodeGenerationOptions(),
                code => Assert.Contains(".HasComputedColumnSql(\"1 + 2\", true)", code.ContextFile.Code),
                model =>
                {
                    var entity = model.FindEntityType("TestNamespace.Entity");
                    Assert.True(entity.GetProperty("ComputedColumn").GetIsStored());
                });
        }

        [ConditionalFact]
        public void ComputedColumnSql_works_unspecified()
        {
            Test(
                modelBuilder => modelBuilder.Entity("Entity").Property<string>("ComputedColumn").HasComputedColumnSql(),
                new ModelCodeGenerationOptions(),
                code => Assert.Contains(".HasComputedColumnSql()", code.ContextFile.Code),
                model =>
                {
                    var entity = model.FindEntityType("TestNamespace.Entity");
                    Assert.Empty(entity.GetProperty("ComputedColumn").GetComputedColumnSql());
                });
        }

        [ConditionalFact]
        public void DefaultValue_works_unspecified()
        {
            Test(
                modelBuilder => modelBuilder.Entity("Entity").Property<string>("DefaultedColumn").HasDefaultValue(),
                new ModelCodeGenerationOptions(),
                code => Assert.Contains(".HasDefaultValue()", code.ContextFile.Code),
                model =>
                {
                    var entity = model.FindEntityType("TestNamespace.Entity");
                    Assert.Equal(DBNull.Value, entity.GetProperty("DefaultedColumn").GetDefaultValue());
                });
        }

        [ConditionalFact]
        public void DefaultValueSql_works_unspecified()
        {
            Test(
                modelBuilder => modelBuilder.Entity("Entity").Property<string>("DefaultedColumn").HasDefaultValueSql(),
                new ModelCodeGenerationOptions(),
                code => Assert.Contains(".HasDefaultValueSql()", code.ContextFile.Code),
                model =>
                {
                    var entity = model.FindEntityType("TestNamespace.Entity");
                    Assert.Empty(entity.GetProperty("DefaultedColumn").GetDefaultValueSql());
                });
        }

        [ConditionalFact]
        public void Entity_with_indexes_and_use_data_annotations_false_always_generates_fluent_API()
        {
            Test(
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
                            x.HasIndex(new[] { "A", "B" }, "IndexOnAAndB")
                                .IsUnique();
                            x.HasIndex(new[] { "B", "C" }, "IndexOnBAndC")
                                .HasFilter("Filter SQL")
                                .HasAnnotation("AnnotationName", "AnnotationValue");
                        }),
                new ModelCodeGenerationOptions { UseDataAnnotations = false },
                code =>
                {
                    AssertFileContents(
                        @"using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace TestNamespace
{
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
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning "
                        + DesignStrings.SensitiveInformationWarning
                        + @"
                optionsBuilder.UseSqlServer(""Initial Catalog=TestDatabase"");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EntityWithIndexes>(entity =>
            {
                entity.HasIndex(e => new { e.A, e.B }, ""IndexOnAAndB"")
                    .IsUnique();

                entity.HasIndex(e => new { e.B, e.C }, ""IndexOnBAndC"")
                    .HasFilter(""Filter SQL"")
                    .HasAnnotation(""AnnotationName"", ""AnnotationValue"");

                entity.Property(e => e.Id).UseIdentityColumn();
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
",
                        code.ContextFile);
                },
                model =>
                    Assert.Equal(2, model.FindEntityType("TestNamespace.EntityWithIndexes").GetIndexes().Count()));
        }

        [ConditionalFact]
        public void Entity_with_indexes_and_use_data_annotations_true_generates_fluent_API_only_for_indexes_with_annotations()
        {
            Test(
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
                            x.HasIndex(new[] { "A", "B" }, "IndexOnAAndB")
                                .IsUnique();
                            x.HasIndex(new[] { "B", "C" }, "IndexOnBAndC")
                                .HasFilter("Filter SQL")
                                .HasAnnotation("AnnotationName", "AnnotationValue");
                        }),
                new ModelCodeGenerationOptions { UseDataAnnotations = true },
                code =>
                {
                    AssertFileContents(
                        @"using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace TestNamespace
{
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
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning "
                        + DesignStrings.SensitiveInformationWarning
                        + @"
                optionsBuilder.UseSqlServer(""Initial Catalog=TestDatabase"");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EntityWithIndexes>(entity =>
            {
                entity.HasIndex(e => new { e.B, e.C }, ""IndexOnBAndC"")
                    .HasFilter(""Filter SQL"")
                    .HasAnnotation(""AnnotationName"", ""AnnotationValue"");

                entity.Property(e => e.Id).UseIdentityColumn();
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
",
                        code.ContextFile);
                },
                model =>
                    Assert.Equal(2, model.FindEntityType("TestNamespace.EntityWithIndexes").GetIndexes().Count()));
        }

        [ConditionalFact]
        public void Entity_lambda_uses_correct_identifiers()
        {
            Test(
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
                        @"using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace TestNamespace
{
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
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning "
                        + DesignStrings.SensitiveInformationWarning
                        + @"
                optionsBuilder.UseSqlServer(""Initial Catalog=TestDatabase"");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DependentEntity>(entity =>
            {
                entity.HasIndex(e => e.DependentId, ""IX_DependentEntity_DependentId"")
                    .IsUnique();

                entity.Property(e => e.Id).UseIdentityColumn();

                entity.HasOne(d => d.NavigationToPrincipal)
                    .WithOne(p => p.NavigationToDependent)
                    .HasPrincipalKey<PrincipalEntity>(p => p.PrincipalId)
                    .HasForeignKey<DependentEntity>(d => d.DependentId);
            });

            modelBuilder.Entity<PrincipalEntity>(entity =>
            {
                entity.HasKey(e => e.AlternateId);

                entity.Property(e => e.AlternateId).UseIdentityColumn();
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
",
                        code.ContextFile);
                },
                model => { });
        }

        [ConditionalFact]
        public void Column_type_is_not_scaffolded_as_annotation()
        {
            Test(
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
                        @"using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace TestNamespace
{
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
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning "
                        + DesignStrings.SensitiveInformationWarning
                        + @"
                optionsBuilder.UseSqlServer(""Initial Catalog=TestDatabase"");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.Property(e => e.Id).UseIdentityColumn();

                entity.Property(e => e.HireDate)
                    .HasColumnType(""date"")
                    .HasColumnName(""hiring_date"");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
",
                        code.ContextFile);
                },
                model =>
                    Assert.Equal("date", model.FindEntityType("TestNamespace.Employee").GetProperty("HireDate").GetConfiguredColumnType()));
        }

        [ConditionalFact]
        public void Is_fixed_length_annotation_should_be_scaffolded_without_optional_parameter()
        {
             Test(
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
                    Assert.Equal(true, model.FindEntityType("TestNamespace.Employee").GetProperty("Name").IsFixedLength()));
        }

        [ConditionalFact]
        public void Global_namespace_works()
        {
            Test(
                modelBuilder => modelBuilder.Entity("MyEntity"),
                new ModelCodeGenerationOptions
                {
                    ModelNamespace = string.Empty
                },
                code =>
                {
                    Assert.DoesNotContain("namespace ", code.ContextFile.Code);
                    Assert.DoesNotContain("namespace ", Assert.Single(code.AdditionalFiles).Code);
                },
                model =>
                {
                    Assert.NotNull(model.FindEntityType("MyEntity"));
                });
        }

        [ConditionalFact]
        public void Global_namespace_works_just_context()
        {
            Test(
                modelBuilder => modelBuilder.Entity("MyEntity"),
                new ModelCodeGenerationOptions
                {
                    ModelNamespace = "TestNamespace",
                    ContextNamespace = string.Empty
                },
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
        }

        [ConditionalFact]
        public void Global_namespace_works_just_model()
        {
            Test(
                modelBuilder => modelBuilder.Entity("MyEntity"),
                new ModelCodeGenerationOptions
                {
                    ModelNamespace = string.Empty,
                    ContextNamespace = "TestNamespace"
                },
                code =>
                {
                    Assert.Contains("namespace TestNamespace", code.ContextFile.Code);
                    Assert.DoesNotContain("namespace ", Assert.Single(code.AdditionalFiles).Code);
                },
                model =>
                {
                    Assert.NotNull(model.FindEntityType("MyEntity"));
                });
        }

        private class TestCodeGeneratorPlugin : ProviderCodeGeneratorPlugin
        {
            public override MethodCallCodeFragment GenerateProviderOptions()
                => new("SetProviderOption");

            public override MethodCallCodeFragment GenerateContextOptions()
                => new("SetContextOption");
        }
    }
}
