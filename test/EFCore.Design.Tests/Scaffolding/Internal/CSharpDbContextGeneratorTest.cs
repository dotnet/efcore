// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Design.Internal;
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

#nullable disable

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

#nullable disable

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

#nullable disable

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
        public void Plugins_work()
        {
            var services = new ServiceCollection()
                .AddEntityFrameworkDesignTimeServices();
            new SqlServerDesignTimeServices().ConfigureDesignTimeServices(services);
            services.AddSingleton<IProviderCodeGeneratorPlugin, TestCodeGeneratorPlugin>();

            var generator = services
                .BuildServiceProvider()
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
                code => Assert.Contains(".ToView(\"Vista\")", code.ContextFile.Code),
                model => Assert.NotNull(
                    model.FindEntityType("TestNamespace.Vista").FindAnnotation(RelationalAnnotationNames.ViewDefinitionSql)));
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

#nullable disable

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

#nullable disable

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

#nullable disable

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

#nullable disable

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

        private class TestCodeGeneratorPlugin : ProviderCodeGeneratorPlugin
        {
            public override MethodCallCodeFragment GenerateProviderOptions()
                => new MethodCallCodeFragment("SetProviderOption");

            public override MethodCallCodeFragment GenerateContextOptions()
                => new MethodCallCodeFragment("SetContextOption");
        }
    }
}
