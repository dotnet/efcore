﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.SqlServer.Design.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    public class CSharpEntityTypeGeneratorTest : ModelCodeGeneratorTestBase
    {
        [ConditionalFact]
        public void KeylessAttribute_is_generated_for_key_less_entity()
        {
            Test(
                modelBuilder => modelBuilder.Entity("Vista").HasNoKey(),
                new ModelCodeGenerationOptions { UseDataAnnotations = true },
                code =>
                {
                    AssertFileContents(
                        @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace
{
    [Keyless]
    public partial class Vista
    {
    }
}
",
                        code.AdditionalFiles.Single(f => f.Path == "Vista.cs"));

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

        public virtual DbSet<Vista> Vista { get; set; }

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
                },
                model =>
                {
                    var entityType = model.FindEntityType("TestNamespace.Vista");
                    Assert.Null(entityType.FindPrimaryKey());
                });
        }

        [ConditionalFact]
        public void TableAttribute_is_generated_for_custom_name()
        {
            Test(
                modelBuilder =>
                {
                    modelBuilder.Entity(
                        "Vista",
                        b =>
                        {
                            b.ToTable("Vistas"); // Default name is "Vista" in the absence of pluralizer
                            b.Property<int>("Id");
                            b.HasKey("Id");
                        });
                },
                new ModelCodeGenerationOptions { UseDataAnnotations = true },
                code =>
                {
                    AssertFileContents(
                        @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace
{
    [Table(""Vistas"")]
    public partial class Vista
    {
        [Key]
        public int Id { get; set; }
    }
}
",
                        code.AdditionalFiles.Single(f => f.Path == "Vista.cs"));
                },
                model =>
                {
                    var entityType = model.FindEntityType("TestNamespace.Vista");
                    Assert.Equal("Vistas", entityType.GetTableName());
                    Assert.Null(entityType.GetSchema());
                });
        }

        [ConditionalFact]
        public void TableAttribute_is_not_generated_for_default_schema()
        {
            Test(
                modelBuilder =>
                {
                    modelBuilder.HasDefaultSchema("dbo");
                    modelBuilder.Entity(
                        "Vista",
                        b =>
                        {
                            b.ToTable("Vista", "dbo"); // Default name is "Vista" in the absence of pluralizer
                            b.Property<int>("Id");
                            b.HasKey("Id");
                        });
                },
                new ModelCodeGenerationOptions { UseDataAnnotations = true },
                code =>
                {
                    AssertFileContents(
                        @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace
{
    public partial class Vista
    {
        [Key]
        public int Id { get; set; }
    }
}
",
                        code.AdditionalFiles.Single(f => f.Path == "Vista.cs"));
                },
                model =>
                {
                    var entityType = model.FindEntityType("TestNamespace.Vista");
                    Assert.Equal("Vista", entityType.GetTableName());
                    Assert.Null(entityType.GetSchema()); // Takes through model default schema
                });
        }

        [ConditionalFact]
        public void TableAttribute_is_generated_for_non_default_schema()
        {
            Test(
                modelBuilder =>
                {
                    modelBuilder.HasDefaultSchema("dbo");
                    modelBuilder.Entity(
                        "Vista",
                        b =>
                        {
                            b.ToTable("Vista", "custom");
                            b.Property<int>("Id");
                            b.HasKey("Id");
                        });
                },
                new ModelCodeGenerationOptions { UseDataAnnotations = true },
                code =>
                {
                    AssertFileContents(
                        @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace
{
    [Table(""Vista"", Schema = ""custom"")]
    public partial class Vista
    {
        [Key]
        public int Id { get; set; }
    }
}
",
                        code.AdditionalFiles.Single(f => f.Path == "Vista.cs"));
                },
                model =>
                {
                    var entityType = model.FindEntityType("TestNamespace.Vista");
                    Assert.Equal("Vista", entityType.GetTableName());
                    Assert.Equal("custom", entityType.GetSchema());
                });
        }

        [ConditionalFact]
        public void TableAttribute_is_not_generated_for_views()
        {
            Test(
                modelBuilder => modelBuilder.Entity("Vista").ToView("Vistas", "dbo"),
                new ModelCodeGenerationOptions { UseDataAnnotations = true },
                code =>
                {
                    AssertFileContents(
                        @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace
{
    [Keyless]
    public partial class Vista
    {
    }
}
",
                        code.AdditionalFiles.Single(f => f.Path == "Vista.cs"));
                },
                model =>
                {
                    var entityType = model.FindEntityType("TestNamespace.Vista");
                    Assert.Equal("Vistas", entityType.GetViewName());
                    Assert.Null(entityType.GetTableName());
                    Assert.Equal("dbo", entityType.GetViewSchema());
                    Assert.Null(entityType.GetSchema());
                });
        }

        [ConditionalFact]
        public void IndexAttribute_is_generated_for_multiple_indexes_with_name_unique()
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
                            x.HasIndex(new[] { "B", "C" }, "IndexOnBAndC");
                            x.HasIndex("C");
                        }),
                new ModelCodeGenerationOptions { UseDataAnnotations = true },
                code =>
                {
                    AssertFileContents(
                        @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace
{
    [Index(nameof(C))]
    [Index(nameof(A), nameof(B), Name = ""IndexOnAAndB"", IsUnique = true)]
    [Index(nameof(B), nameof(C), Name = ""IndexOnBAndC"")]
    public partial class EntityWithIndexes
    {
        [Key]
        public int Id { get; set; }
        public int A { get; set; }
        public int B { get; set; }
        public int C { get; set; }
    }
}
",
                        code.AdditionalFiles.Single(f => f.Path == "EntityWithIndexes.cs"));
                },
                model =>
                {
                    var entityType = model.FindEntityType("TestNamespace.EntityWithIndexes");
                    var indexes = entityType.GetIndexes();
                    Assert.Collection(
                        indexes,
                        t => Assert.Null(t.Name),
                        t => Assert.Equal("IndexOnAAndB", t.Name),
                        t => Assert.Equal("IndexOnBAndC", t.Name));
                });
        }

        [ConditionalFact]
        public void Entity_with_indexes_generates_IndexAttribute_only_for_indexes_without_annotations()
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
                                .HasFilter("Filter SQL");
                        }),
                new ModelCodeGenerationOptions { UseDataAnnotations = true },
                code =>
                {
                    AssertFileContents(
                        @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace
{
    [Index(nameof(A), nameof(B), Name = ""IndexOnAAndB"", IsUnique = true)]
    public partial class EntityWithIndexes
    {
        [Key]
        public int Id { get; set; }
        public int A { get; set; }
        public int B { get; set; }
        public int C { get; set; }
    }
}
",
                        code.AdditionalFiles.Single(f => f.Path == "EntityWithIndexes.cs"));

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
                    .HasFilter(""Filter SQL"");

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
        public void KeyAttribute_is_generated_for_single_property_and_no_fluent_api()
        {
            Test(
                modelBuilder => modelBuilder
                    .Entity(
                        "Entity",
                        x =>
                        {
                            x.Property<int>("PrimaryKey");
                            x.HasKey("PrimaryKey");
                        }),
                new ModelCodeGenerationOptions { UseDataAnnotations = true },
                code =>
                {
                    AssertFileContents(
                        @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace
{
    public partial class Entity
    {
        [Key]
        public int PrimaryKey { get; set; }
    }
}
",
                        code.AdditionalFiles.Single(f => f.Path == "Entity.cs"));

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

        public virtual DbSet<Entity> Entity { get; set; }

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
            modelBuilder.Entity<Entity>(entity =>
            {
                entity.Property(e => e.PrimaryKey).UseIdentityColumn();
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
                    Assert.Equal("PrimaryKey", model.FindEntityType("TestNamespace.Entity").FindPrimaryKey().Properties[0].Name));
        }

        [ConditionalFact]
        public void KeyAttribute_is_generated_on_multiple_properties_but_configuring_using_fluent_api_for_composite_key()
        {
            Test(
                modelBuilder => modelBuilder
                    .Entity(
                        "Post",
                        x =>
                        {
                            x.Property<int>("Key");
                            x.Property<int>("Serial");
                            x.HasKey("Key", "Serial");
                        }),
                new ModelCodeGenerationOptions { UseDataAnnotations = true },
                code =>
                {
                    AssertFileContents(
                        @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace
{
    public partial class Post
    {
        [Key]
        public int Key { get; set; }
        [Key]
        public int Serial { get; set; }
    }
}
",
                        code.AdditionalFiles.Single(f => f.Path == "Post.cs"));

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

        public virtual DbSet<Post> Post { get; set; }

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
            modelBuilder.Entity<Post>(entity =>
            {
                entity.HasKey(e => new { e.Key, e.Serial });
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
                {
                    var postType = model.FindEntityType("TestNamespace.Post");
                    Assert.Equal(new[] { "Key", "Serial" }, postType.FindPrimaryKey().Properties.Select(p => p.Name));
                });
        }

        [ConditionalFact]
        public void Required_and_not_required_properties_without_nrt()
        {
            Test(
                modelBuilder => modelBuilder
                    .Entity(
                        "Entity",
                        x =>
                        {
                            x.Property<int>("Id");
                            x.Property<string>("RequiredString").IsRequired();
                            x.Property<string>("NonRequiredString");
                            x.Property<int>("RequiredInt");
                            x.Property<int?>("NonRequiredInt");
                        }),
                new ModelCodeGenerationOptions { UseDataAnnotations = true },
                code =>
                {
                    AssertFileContents(
                        @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace
{
    public partial class Entity
    {
        [Key]
        public int Id { get; set; }
        public int? NonRequiredInt { get; set; }
        public string NonRequiredString { get; set; }
        public int RequiredInt { get; set; }
        [Required]
        public string RequiredString { get; set; }
    }
}
",
                        code.AdditionalFiles.Single(f => f.Path == "Entity.cs"));
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
        public void Required_and_not_required_properties_with_nrt()
        {
            Test(
                modelBuilder => modelBuilder
                    .Entity(
                        "Entity",
                        x =>
                        {
                            x.Property<int>("Id");
                            x.Property<string>("RequiredString").IsRequired();
                            x.Property<string>("NonRequiredString");
                            x.Property<int>("RequiredInt");
                            x.Property<int?>("NonRequiredInt");
                        }),
                new ModelCodeGenerationOptions { UseDataAnnotations = true, UseNullableReferenceTypes = true},
                code =>
                {
                    AssertFileContents(
                        @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace
{
    public partial class Entity
    {
        [Key]
        public int Id { get; set; }
        public int? NonRequiredInt { get; set; }
        public string? NonRequiredString { get; set; }
        public int RequiredInt { get; set; }
        public string RequiredString { get; set; } = null!;
    }
}
",
                        code.AdditionalFiles.Single(f => f.Path == "Entity.cs"));
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
        public void Required_and_not_required_navigations_without_nrt()
        {
            Test(
                modelBuilder => modelBuilder
                    .Entity(
                        "Entity",
                        x =>
                        {
                            x.Property<int>("Id");

                            x.HasOne("Dependent1", "RequiredReferenceNavigation").WithMany("Entity").IsRequired();
                            x.HasOne("Dependent2", "OptionalReferenceNavigation").WithMany("Entity");
                            x.HasOne("Dependent3", "RequiredValueNavigation").WithMany("Entity").IsRequired();
                            x.HasOne("Dependent4", "OptionalValueNavigation").WithMany("Entity");
                        })
                    .Entity("Dependent1", x => x.Property<string>("Id"))
                    .Entity("Dependent2", x => x.Property<string>("Id"))
                    .Entity("Dependent3", x => x.Property<int>("Id"))
                    .Entity("Dependent4", x => x.Property<int>("Id")),
                new ModelCodeGenerationOptions { UseDataAnnotations = true },
                code =>
                {
                    AssertFileContents(
                        @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace
{
    public partial class Entity
    {
        [Key]
        public int Id { get; set; }
        public string OptionalReferenceNavigationId { get; set; }
        public int? OptionalValueNavigationId { get; set; }
        [Required]
        public string RequiredReferenceNavigationId { get; set; }
        public int RequiredValueNavigationId { get; set; }

        [ForeignKey(nameof(OptionalReferenceNavigationId))]
        [InverseProperty(nameof(Dependent2.Entity))]
        public virtual Dependent2 OptionalReferenceNavigation { get; set; }
        [ForeignKey(nameof(OptionalValueNavigationId))]
        [InverseProperty(nameof(Dependent4.Entity))]
        public virtual Dependent4 OptionalValueNavigation { get; set; }
        [ForeignKey(nameof(RequiredReferenceNavigationId))]
        [InverseProperty(nameof(Dependent1.Entity))]
        public virtual Dependent1 RequiredReferenceNavigation { get; set; }
        [ForeignKey(nameof(RequiredValueNavigationId))]
        [InverseProperty(nameof(Dependent3.Entity))]
        public virtual Dependent3 RequiredValueNavigation { get; set; }
    }
}
",
                        code.AdditionalFiles.Single(f => f.Path == "Entity.cs"));
                },
                model =>
                {
                    var entityType = model.FindEntityType("TestNamespace.Entity");

                    Assert.False(entityType.GetProperty("RequiredReferenceNavigationId").IsNullable);
                    Assert.True(entityType.GetProperty("OptionalReferenceNavigationId").IsNullable);
                    Assert.False(entityType.GetProperty("RequiredValueNavigationId").IsNullable);
                    Assert.True(entityType.GetProperty("OptionalValueNavigationId").IsNullable);

                    Assert.True(entityType.FindNavigation("RequiredReferenceNavigation")!.ForeignKey.IsRequired);
                    Assert.False(entityType.FindNavigation("OptionalReferenceNavigation")!.ForeignKey.IsRequired);
                    Assert.True(entityType.FindNavigation("RequiredValueNavigation")!.ForeignKey.IsRequired);
                    Assert.False(entityType.FindNavigation("OptionalValueNavigation")!.ForeignKey.IsRequired);
                });
        }

        [ConditionalFact]
        public void Required_and_not_required_reference_navigations_with_nrt()
        {
            Test(
                modelBuilder => modelBuilder
                    .Entity(
                        "Entity",
                        x =>
                        {
                            x.Property<int>("Id");

                            x.HasOne("Dependent1", "RequiredNavigationWithReferenceForeignKey").WithOne("Entity").IsRequired();
                            x.HasOne("Dependent2", "OptionalNavigationWithReferenceForeignKey").WithOne("Entity");
                            x.HasOne("Dependent3", "RequiredNavigationWithValueForeignKey").WithOne("Entity").IsRequired();
                            x.HasOne("Dependent4", "OptionalNavigationWithValueForeignKey").WithOne("Entity");
                        })
                    .Entity("Dependent1", x => x.Property<string>("Id"))
                    .Entity("Dependent2", x => x.Property<string>("Id"))
                    .Entity("Dependent3", x => x.Property<int>("Id"))
                    .Entity("Dependent4", x => x.Property<int>("Id")),
                new ModelCodeGenerationOptions { UseDataAnnotations = true, UseNullableReferenceTypes = true },
                code =>
                {
                    AssertFileContents(
                        @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace
{
    public partial class Entity
    {
        [Key]
        public int Id { get; set; }
        public string? OptionalNavigationWithReferenceForeignKeyId { get; set; }
        public int? OptionalNavigationWithValueForeignKeyId { get; set; }
        public string RequiredNavigationWithReferenceForeignKeyId { get; set; } = null!;
        public int RequiredNavigationWithValueForeignKeyId { get; set; }

        [ForeignKey(nameof(OptionalNavigationWithReferenceForeignKeyId))]
        [InverseProperty(nameof(Dependent2.Entity))]
        public virtual Dependent2? OptionalNavigationWithReferenceForeignKey { get; set; }
        [ForeignKey(nameof(OptionalNavigationWithValueForeignKeyId))]
        [InverseProperty(nameof(Dependent4.Entity))]
        public virtual Dependent4? OptionalNavigationWithValueForeignKey { get; set; }
        [ForeignKey(nameof(RequiredNavigationWithReferenceForeignKeyId))]
        [InverseProperty(nameof(Dependent1.Entity))]
        public virtual Dependent1 RequiredNavigationWithReferenceForeignKey { get; set; } = null!;
        [ForeignKey(nameof(RequiredNavigationWithValueForeignKeyId))]
        [InverseProperty(nameof(Dependent3.Entity))]
        public virtual Dependent3 RequiredNavigationWithValueForeignKey { get; set; } = null!;
    }
}
",
                        code.AdditionalFiles.Single(f => f.Path == "Entity.cs"));
                },
                model =>
                {
                    var entityType = model.FindEntityType("TestNamespace.Entity");

                    Assert.False(entityType.GetProperty("RequiredNavigationWithReferenceForeignKeyId").IsNullable);
                    Assert.True(entityType.GetProperty("OptionalNavigationWithReferenceForeignKeyId").IsNullable);
                    Assert.False(entityType.GetProperty("RequiredNavigationWithValueForeignKeyId").IsNullable);
                    Assert.True(entityType.GetProperty("OptionalNavigationWithValueForeignKeyId").IsNullable);

                    Assert.True(entityType.FindNavigation("RequiredNavigationWithReferenceForeignKey")!.ForeignKey.IsRequired);
                    Assert.False(entityType.FindNavigation("OptionalNavigationWithReferenceForeignKey")!.ForeignKey.IsRequired);
                    Assert.True(entityType.FindNavigation("RequiredNavigationWithValueForeignKey")!.ForeignKey.IsRequired);
                    Assert.False(entityType.FindNavigation("OptionalNavigationWithValueForeignKey")!.ForeignKey.IsRequired);
                });
        }

        [ConditionalFact]
        public void Required_and_not_required_collection_navigations_with_nrt()
        {
            Test(
                modelBuilder => modelBuilder
                    .Entity(
                        "Entity",
                        x =>
                        {
                            x.Property<int>("Id");

                            x.HasOne("Dependent1", "RequiredNavigationWithReferenceForeignKey").WithMany("Entity").IsRequired();
                            x.HasOne("Dependent2", "OptionalNavigationWithReferenceForeignKey").WithMany("Entity");
                            x.HasOne("Dependent3", "RequiredNavigationWithValueForeignKey").WithMany("Entity").IsRequired();
                            x.HasOne("Dependent4", "OptionalNavigationWithValueForeignKey").WithMany("Entity");
                        })
                    .Entity("Dependent1", x => x.Property<string>("Id"))
                    .Entity("Dependent2", x => x.Property<string>("Id"))
                    .Entity("Dependent3", x => x.Property<int>("Id"))
                    .Entity("Dependent4", x => x.Property<int>("Id")),
                new ModelCodeGenerationOptions { UseDataAnnotations = true, UseNullableReferenceTypes = true },
                code =>
                {
                    AssertFileContents(
                        @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace
{
    public partial class Entity
    {
        [Key]
        public int Id { get; set; }
        public string? OptionalNavigationWithReferenceForeignKeyId { get; set; }
        public int? OptionalNavigationWithValueForeignKeyId { get; set; }
        public string RequiredNavigationWithReferenceForeignKeyId { get; set; } = null!;
        public int RequiredNavigationWithValueForeignKeyId { get; set; }

        [ForeignKey(nameof(OptionalNavigationWithReferenceForeignKeyId))]
        [InverseProperty(nameof(Dependent2.Entity))]
        public virtual Dependent2? OptionalNavigationWithReferenceForeignKey { get; set; }
        [ForeignKey(nameof(OptionalNavigationWithValueForeignKeyId))]
        [InverseProperty(nameof(Dependent4.Entity))]
        public virtual Dependent4? OptionalNavigationWithValueForeignKey { get; set; }
        [ForeignKey(nameof(RequiredNavigationWithReferenceForeignKeyId))]
        [InverseProperty(nameof(Dependent1.Entity))]
        public virtual Dependent1 RequiredNavigationWithReferenceForeignKey { get; set; } = null!;
        [ForeignKey(nameof(RequiredNavigationWithValueForeignKeyId))]
        [InverseProperty(nameof(Dependent3.Entity))]
        public virtual Dependent3 RequiredNavigationWithValueForeignKey { get; set; } = null!;
    }
}
",
                        code.AdditionalFiles.Single(f => f.Path == "Entity.cs"));

                    for (var i = 1; i <= 4; i++)
                    {
                        Assert.Contains(
                            "public virtual ICollection<Entity> Entity { get; set; }",
                            code.AdditionalFiles.Single(f => f.Path == $"Dependent{i}.cs").Code);
                    }
                },
                model =>
                {
                    var entityType = model.FindEntityType("TestNamespace.Entity");

                    Assert.False(entityType.GetProperty("RequiredNavigationWithReferenceForeignKeyId").IsNullable);
                    Assert.True(entityType.GetProperty("OptionalNavigationWithReferenceForeignKeyId").IsNullable);
                    Assert.False(entityType.GetProperty("RequiredNavigationWithValueForeignKeyId").IsNullable);
                    Assert.True(entityType.GetProperty("OptionalNavigationWithValueForeignKeyId").IsNullable);

                    Assert.True(entityType.FindNavigation("RequiredNavigationWithReferenceForeignKey")!.ForeignKey.IsRequired);
                    Assert.False(entityType.FindNavigation("OptionalNavigationWithReferenceForeignKey")!.ForeignKey.IsRequired);
                    Assert.True(entityType.FindNavigation("RequiredNavigationWithValueForeignKey")!.ForeignKey.IsRequired);
                    Assert.False(entityType.FindNavigation("OptionalNavigationWithValueForeignKey")!.ForeignKey.IsRequired);
                });
        }

        [ConditionalFact]
        public void RequiredAttribute_is_not_generated_for_key_property()
        {
            Test(
                modelBuilder => modelBuilder
                    .Entity(
                        "Entity",
                        x =>
                        {
                            x.Property<string>("RequiredString");
                            x.HasKey("RequiredString");
                        }),
                new ModelCodeGenerationOptions { UseDataAnnotations = true },
                code =>
                {
                    AssertFileContents(
                        @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace
{
    public partial class Entity
    {
        [Key]
        public string RequiredString { get; set; }
    }
}
",
                        code.AdditionalFiles.Single(f => f.Path == "Entity.cs"));
                },
                model =>
                    Assert.False(model.FindEntityType("TestNamespace.Entity").GetProperty("RequiredString").IsNullable));
        }

        [ConditionalFact]
        public void ColumnAttribute_is_generated_for_property()
        {
            Test(
                modelBuilder => modelBuilder
                    .Entity(
                        "Entity",
                        x =>
                        {
                            x.Property<int>("Id");
                            x.Property<string>("A").HasColumnName("propertyA");
                            x.Property<string>("B").HasColumnType("nchar(10)");
                            x.Property<string>("C").HasColumnName("random").HasColumnType("varchar(200)");
                            x.Property<decimal>("D").HasColumnType("numeric(18, 2)");
                            x.Property<string>("E").HasMaxLength(100);
                        }),
                new ModelCodeGenerationOptions { UseDataAnnotations = true },
                code =>
                {
                    AssertFileContents(
                        @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace
{
    public partial class Entity
    {
        [Key]
        public int Id { get; set; }
        [Column(""propertyA"")]
        public string A { get; set; }
        [Column(TypeName = ""nchar(10)"")]
        public string B { get; set; }
        [Column(""random"", TypeName = ""varchar(200)"")]
        public string C { get; set; }
        [Column(TypeName = ""numeric(18, 2)"")]
        public decimal D { get; set; }
        [StringLength(100)]
        public string E { get; set; }
    }
}
",
                        code.AdditionalFiles.Single(f => f.Path == "Entity.cs"));

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

        public virtual DbSet<Entity> Entity { get; set; }

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
            modelBuilder.Entity<Entity>(entity =>
            {
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
                {
                    var entitType = model.FindEntityType("TestNamespace.Entity");
                    Assert.Equal("propertyA", entitType.GetProperty("A").GetColumnBaseName());
                    Assert.Equal("nchar(10)", entitType.GetProperty("B").GetColumnType());
                    Assert.Equal("random", entitType.GetProperty("C").GetColumnBaseName());
                    Assert.Equal("varchar(200)", entitType.GetProperty("C").GetColumnType());
                });
        }

        [ConditionalFact]
        public void MaxLengthAttribute_is_generated_for_property()
        {
            Test(
                modelBuilder => modelBuilder
                    .Entity(
                        "Entity",
                        x =>
                        {
                            x.Property<int>("Id");
                            x.Property<string>("A").HasMaxLength(34);
                            x.Property<byte[]>("B").HasMaxLength(10);
                        }),
                new ModelCodeGenerationOptions { UseDataAnnotations = true },
                code =>
                {
                    AssertFileContents(
                        @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace
{
    public partial class Entity
    {
        [Key]
        public int Id { get; set; }
        [StringLength(34)]
        public string A { get; set; }
        [MaxLength(10)]
        public byte[] B { get; set; }
    }
}
",
                        code.AdditionalFiles.Single(f => f.Path == "Entity.cs"));
                },
                model =>
                {
                    var entitType = model.FindEntityType("TestNamespace.Entity");
                    Assert.Equal(34, entitType.GetProperty("A").GetMaxLength());
                    Assert.Equal(10, entitType.GetProperty("B").GetMaxLength());
                });
        }

        [ConditionalFact]
        public void UnicodeAttribute_is_generated_for_property()
        {
            Test(
                modelBuilder => modelBuilder
                    .Entity(
                        "Entity",
                        x =>
                        {
                            x.Property<int>("Id");
                            x.Property<string>("A").HasMaxLength(34).IsUnicode();
                            x.Property<string>("B").HasMaxLength(34).IsUnicode(false);
                            x.Property<string>("C").HasMaxLength(34);
                        }),
                new ModelCodeGenerationOptions { UseDataAnnotations = true },
                code =>
                {
                    AssertFileContents(
                        @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace
{
    public partial class Entity
    {
        [Key]
        public int Id { get; set; }
        [StringLength(34)]
        [Unicode]
        public string A { get; set; }
        [StringLength(34)]
        [Unicode(false)]
        public string B { get; set; }
        [StringLength(34)]
        public string C { get; set; }
    }
}
",
                        code.AdditionalFiles.Single(f => f.Path == "Entity.cs"));
                },
                model =>
                {
                    var entitType = model.FindEntityType("TestNamespace.Entity");
                    Assert.True(entitType.GetProperty("A").IsUnicode());
                    Assert.False(entitType.GetProperty("B").IsUnicode());
                    Assert.Null(entitType.GetProperty("C").IsUnicode());
                });
        }

        [ConditionalFact]
        public void PrecisionAttribute_is_generated_for_property()
        {
            Test(
                   modelBuilder => modelBuilder
                       .Entity(
                           "Entity",
                           x =>
                           {
                               x.Property<int>("Id");
                               x.Property<decimal>("A").HasPrecision(10);
                               x.Property<decimal>("B").HasPrecision(14, 3);
                               x.Property<DateTime>("C").HasPrecision(5);
                               x.Property<DateTimeOffset>("D").HasPrecision(3);
                           }),
                   new ModelCodeGenerationOptions { UseDataAnnotations = true },
                   code =>
                   {
                       AssertFileContents(
                           @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace
{
    public partial class Entity
    {
        [Key]
        public int Id { get; set; }
        [Precision(10)]
        public decimal A { get; set; }
        [Precision(14, 3)]
        public decimal B { get; set; }
        [Precision(5)]
        public DateTime C { get; set; }
        [Precision(3)]
        public DateTimeOffset D { get; set; }
    }
}
",
                           code.AdditionalFiles.Single(f => f.Path == "Entity.cs"));
                   },
                   model =>
                   {
                       var entitType = model.FindEntityType("TestNamespace.Entity");
                       Assert.Equal(10, entitType.GetProperty("A").GetPrecision());
                       Assert.Equal(14, entitType.GetProperty("B").GetPrecision());
                       Assert.Equal(3, entitType.GetProperty("B").GetScale());
                       Assert.Equal(5, entitType.GetProperty("C").GetPrecision());
                       Assert.Equal(3, entitType.GetProperty("D").GetPrecision());
                   });
        }

        [ConditionalFact]
        public void Comments_are_generated()
        {
            Test(
                modelBuilder => modelBuilder
                    .Entity(
                        "Entity",
                        x =>
                        {
                            x.HasComment("Entity Comment");
                            x.Property<int>("Id").HasComment("Property Comment");
                        })
                    ,
                new ModelCodeGenerationOptions { UseDataAnnotations = true },
                code =>
                {
                    AssertFileContents(
                        @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace
{
    /// <summary>
    /// Entity Comment
    /// </summary>
    public partial class Entity
    {
        /// <summary>
        /// Property Comment
        /// </summary>
        [Key]
        public int Id { get; set; }
    }
}
",
                        code.AdditionalFiles.Single(f => f.Path == "Entity.cs"));
                },
                model => { });
        }

        [ConditionalFact]
        public void Comments_complex_are_generated()
        {
            Test(
                modelBuilder => modelBuilder
                    .Entity(
                        "Entity",
                        x =>
                        {
                            x.HasComment(@"Entity Comment
On multiple lines
With XML content <br/>");
                            x.Property<int>("Id").HasComment(@"Property Comment
On multiple lines
With XML content <br/>");
                        })
                    ,
                new ModelCodeGenerationOptions { UseDataAnnotations = true },
                code =>
                {
                    AssertFileContents(
                        @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace
{
    /// <summary>
    /// Entity Comment
    /// On multiple lines
    /// With XML content &lt;br/&gt;
    /// </summary>
    public partial class Entity
    {
        /// <summary>
        /// Property Comment
        /// On multiple lines
        /// With XML content &lt;br/&gt;
        /// </summary>
        [Key]
        public int Id { get; set; }
    }
}
",
                        code.AdditionalFiles.Single(f => f.Path == "Entity.cs"));
                },
                model => { });
        }

        [ConditionalFact]
        public void Properties_are_sorted_in_order_of_definition_in_table()
        {
            Test(
                modelBuilder => modelBuilder
                    .Entity(
                        "Entity",
                        x =>
                        {
                            // Order would be PK first and then rest alphabetically since they are all shadow
                            x.Property<int>("Id");
                            x.Property<string>("LastProperty");
                            x.Property<string>("FirstProperty");
                        }),
                new ModelCodeGenerationOptions { UseDataAnnotations = true },
                code =>
                {
                    AssertFileContents(
                        @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace
{
    public partial class Entity
    {
        [Key]
        public int Id { get; set; }
        public string FirstProperty { get; set; }
        public string LastProperty { get; set; }
    }
}
",
                        code.AdditionalFiles.Single(f => f.Path == "Entity.cs"));
                },
                model => { });
        }

        [ConditionalFact]
        public void Navigation_properties_are_sorted_after_properties_and_collection_are_initialized_in_ctor()
        {
            Test(
                modelBuilder => modelBuilder
                    .Entity(
                        "Person",
                        x => x.Property<int>("Id"))
                    .Entity(
                        "Contribution",
                        x => x.Property<int>("Id"))
                    .Entity(
                        "Post",
                        x =>
                        {
                            x.Property<int>("Id");

                            x.HasOne("Person", "Author").WithMany("Posts");
                            x.HasMany("Contribution", "Contributions").WithOne();
                        }),
                new ModelCodeGenerationOptions { UseDataAnnotations = true },
                code =>
                {
                    AssertFileContents(
                        @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace
{
    public partial class Post
    {
        public Post()
        {
            Contributions = new HashSet<Contribution>();
        }

        [Key]
        public int Id { get; set; }
        public int? AuthorId { get; set; }

        [ForeignKey(nameof(AuthorId))]
        [InverseProperty(nameof(Person.Posts))]
        public virtual Person Author { get; set; }
        public virtual ICollection<Contribution> Contributions { get; set; }
    }
}
",
                        code.AdditionalFiles.Single(f => f.Path == "Post.cs"));

                    AssertFileContents(
                        @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace
{
    public partial class Person
    {
        public Person()
        {
            Posts = new HashSet<Post>();
        }

        [Key]
        public int Id { get; set; }

        [InverseProperty(nameof(Post.Author))]
        public virtual ICollection<Post> Posts { get; set; }
    }
}
",
                        code.AdditionalFiles.Single(f => f.Path == "Person.cs"));
                },
                model =>
                {
                    var postType = model.FindEntityType("TestNamespace.Post");
                    var authorNavigation = postType.FindNavigation("Author");
                    Assert.True(authorNavigation.IsOnDependent);
                    Assert.Equal("TestNamespace.Person", authorNavigation.ForeignKey.PrincipalEntityType.Name);

                    var contributionsNav = postType.FindNavigation("Contributions");
                    Assert.False(contributionsNav.IsOnDependent);
                    Assert.Equal("TestNamespace.Contribution", contributionsNav.ForeignKey.DeclaringEntityType.Name);
                });
        }

        [ConditionalFact]
        public void ForeignKeyAttribute_is_generated_for_composite_fk()
        {
            Test(
                modelBuilder => modelBuilder
                    .Entity(
                        "Blog",
                        x =>
                        {
                            x.Property<int>("Id1");
                            x.Property<int>("Id2");
                            x.HasKey("Id1", "Id2");
                        })
                    .Entity(
                        "Post",
                        x =>
                        {
                            x.Property<int>("Id");

                            x.HasOne("Blog", "BlogNavigation").WithMany("Posts").HasForeignKey("BlogId1", "BlogId2");
                        }),
                new ModelCodeGenerationOptions { UseDataAnnotations = true },
                code =>
                {
                    AssertFileContents(
                        @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace
{
    public partial class Post
    {
        [Key]
        public int Id { get; set; }
        public int? BlogId1 { get; set; }
        public int? BlogId2 { get; set; }

        [ForeignKey(""BlogId1,BlogId2"")]
        [InverseProperty(nameof(Blog.Posts))]
        public virtual Blog BlogNavigation { get; set; }
    }
}
",
                        code.AdditionalFiles.Single(f => f.Path == "Post.cs"));

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

        public virtual DbSet<Blog> Blog { get; set; }
        public virtual DbSet<Post> Post { get; set; }

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
            modelBuilder.Entity<Blog>(entity =>
            {
                entity.HasKey(e => new { e.Id1, e.Id2 });
            });

            modelBuilder.Entity<Post>(entity =>
            {
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
                {
                    var postType = model.FindEntityType("TestNamespace.Post");
                    var blogNavigation = postType.FindNavigation("BlogNavigation");
                    Assert.Equal("TestNamespace.Blog", blogNavigation.ForeignKey.PrincipalEntityType.Name);
                    Assert.Equal(new[] { "BlogId1", "BlogId2" }, blogNavigation.ForeignKey.Properties.Select(p => p.Name));
                });
        }

        [ConditionalFact]
        public void ForeignKeyAttribute_InversePropertyAttribute_is_not_generated_for_alternate_key()
        {
            Test(
                modelBuilder => modelBuilder
                    .Entity(
                        "Blog",
                        x =>
                        {
                            x.Property<int>("Id");
                            x.Property<int>("Id1");
                            x.Property<int>("Id2");
                        })
                    .Entity(
                        "Post",
                        x =>
                        {
                            x.Property<int>("Id");

                            x.HasOne("Blog", "BlogNavigation").WithMany("Posts")
                                .HasPrincipalKey("Id1", "Id2")
                                .HasForeignKey("BlogId1", "BlogId2");
                        }),
                new ModelCodeGenerationOptions { UseDataAnnotations = true },
                code =>
                {
                    AssertFileContents(
                        @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace
{
    public partial class Post
    {
        [Key]
        public int Id { get; set; }
        public int? BlogId1 { get; set; }
        public int? BlogId2 { get; set; }

        public virtual Blog BlogNavigation { get; set; }
    }
}
",
                        code.AdditionalFiles.Single(f => f.Path == "Post.cs"));

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

        public virtual DbSet<Blog> Blog { get; set; }
        public virtual DbSet<Post> Post { get; set; }

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
            modelBuilder.Entity<Blog>(entity =>
            {
                entity.Property(e => e.Id).UseIdentityColumn();
            });

            modelBuilder.Entity<Post>(entity =>
            {
                entity.Property(e => e.Id).UseIdentityColumn();

                entity.HasOne(d => d.BlogNavigation)
                    .WithMany(p => p.Posts)
                    .HasPrincipalKey(p => new { p.Id1, p.Id2 })
                    .HasForeignKey(d => new { d.BlogId1, d.BlogId2 });
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
                {
                    var postType = model.FindEntityType("TestNamespace.Post");
                    var blogNavigation = postType.FindNavigation("BlogNavigation");
                    Assert.Equal("TestNamespace.Blog", blogNavigation.ForeignKey.PrincipalEntityType.Name);
                    Assert.Equal(new[] { "BlogId1", "BlogId2" }, blogNavigation.ForeignKey.Properties.Select(p => p.Name));
                    Assert.Equal(new[] { "Id1", "Id2" }, blogNavigation.ForeignKey.PrincipalKey.Properties.Select(p => p.Name));
                });
        }

        [ConditionalFact]
        public void InverseProperty_when_navigation_property_with_same_type_and_navigation_name()
        {
            Test(
                modelBuilder => modelBuilder
                    .Entity(
                        "Blog",
                        x => x.Property<int>("Id"))
                    .Entity(
                        "Post",
                        x =>
                        {
                            x.Property<int>("Id");
                            x.HasOne("Blog", "Blog").WithMany("Posts");
                        }),
                new ModelCodeGenerationOptions { UseDataAnnotations = true },
                code =>
                {
                    AssertFileContents(
                        @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace
{
    public partial class Post
    {
        [Key]
        public int Id { get; set; }
        public int? BlogId { get; set; }

        [ForeignKey(nameof(BlogId))]
        [InverseProperty(""Posts"")]
        public virtual Blog Blog { get; set; }
    }
}
",
                        code.AdditionalFiles.Single(f => f.Path == "Post.cs"));
                },
                model =>
                {
                    var postType = model.FindEntityType("TestNamespace.Post");
                    var blogNavigation = postType.FindNavigation("Blog");

                    var foreignKeyProperty = Assert.Single(blogNavigation.ForeignKey.Properties);
                    Assert.Equal("BlogId", foreignKeyProperty.Name);

                    var inverseNavigation = blogNavigation.Inverse;
                    Assert.Equal("TestNamespace.Blog", inverseNavigation.DeclaringEntityType.Name);
                    Assert.Equal("Posts", inverseNavigation.Name);
                });
        }

        [ConditionalFact]
        public void InverseProperty_when_navigation_property_with_same_type_and_property_name()
        {
            Test(
                modelBuilder => modelBuilder
                    .Entity(
                        "Blog",
                        x => x.Property<int>("Id"))
                    .Entity(
                        "Post",
                        x =>
                        {
                            x.Property<int>("Id");
                            x.HasOne("Blog", "BlogNavigation").WithMany("Posts").HasForeignKey("Blog");
                        }),
                new ModelCodeGenerationOptions { UseDataAnnotations = true },
                code =>
                {
                    AssertFileContents(
                        @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace
{
    public partial class Post
    {
        [Key]
        public int Id { get; set; }
        public int? Blog { get; set; }

        [ForeignKey(nameof(Blog))]
        [InverseProperty(""Posts"")]
        public virtual Blog BlogNavigation { get; set; }
    }
}
",
                        code.AdditionalFiles.Single(f => f.Path == "Post.cs"));
                },
                model =>
                {
                    var postType = model.FindEntityType("TestNamespace.Post");
                    var blogNavigation = postType.FindNavigation("BlogNavigation");

                    var foreignKeyProperty = Assert.Single(blogNavigation.ForeignKey.Properties);
                    Assert.Equal("Blog", foreignKeyProperty.Name);

                    var inverseNavigation = blogNavigation.Inverse;
                    Assert.Equal("TestNamespace.Blog", inverseNavigation.DeclaringEntityType.Name);
                    Assert.Equal("Posts", inverseNavigation.Name);
                });
        }

        [ConditionalFact]
        public void InverseProperty_when_navigation_property_with_same_type_and_other_navigation_name()
        {
            Test(
                modelBuilder => modelBuilder
                    .Entity(
                        "Blog",
                        x => x.Property<int>("Id"))
                    .Entity(
                        "Post",
                        x =>
                        {
                            x.Property<int>("Id");
                            x.HasOne("Blog", "Blog").WithMany("Posts");
                            x.HasOne("Blog", "OriginalBlog").WithMany("OriginalPosts").HasForeignKey("OriginalBlogId");
                        }),
                new ModelCodeGenerationOptions { UseDataAnnotations = true },
                code =>
                {
                    AssertFileContents(
                        @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace
{
    public partial class Post
    {
        [Key]
        public int Id { get; set; }
        public int? BlogId { get; set; }
        public int? OriginalBlogId { get; set; }

        [ForeignKey(nameof(BlogId))]
        [InverseProperty(""Posts"")]
        public virtual Blog Blog { get; set; }
        [ForeignKey(nameof(OriginalBlogId))]
        [InverseProperty(""OriginalPosts"")]
        public virtual Blog OriginalBlog { get; set; }
    }
}
",
                        code.AdditionalFiles.Single(f => f.Path == "Post.cs"));
                },
                model =>
                {
                    var postType = model.FindEntityType("TestNamespace.Post");

                    var blogNavigation = postType.FindNavigation("Blog");

                    var foreignKeyProperty = Assert.Single(blogNavigation.ForeignKey.Properties);
                    Assert.Equal("BlogId", foreignKeyProperty.Name);

                    var inverseNavigation = blogNavigation.Inverse;
                    Assert.Equal("TestNamespace.Blog", inverseNavigation.DeclaringEntityType.Name);
                    Assert.Equal("Posts", inverseNavigation.Name);

                    var originalBlogNavigation = postType.FindNavigation("OriginalBlog");

                    var originalForeignKeyProperty = Assert.Single(originalBlogNavigation.ForeignKey.Properties);
                    Assert.Equal("OriginalBlogId", originalForeignKeyProperty.Name);

                    var originalInverseNavigation = originalBlogNavigation.Inverse;
                    Assert.Equal("TestNamespace.Blog", originalInverseNavigation.DeclaringEntityType.Name);
                    Assert.Equal("OriginalPosts", originalInverseNavigation.Name);
                });
        }

        [ConditionalFact]
        public void Entity_with_custom_annotation()
        {
            Test(
                modelBuilder => modelBuilder
                    .Entity(
                        "EntityWithAnnotation",
                        x =>
                        {
                            x.HasAnnotation("Custom:EntityAnnotation", "first argument");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                        }),
                new ModelCodeGenerationOptions { UseDataAnnotations = true },
                code =>
                {
                    AssertFileContents(
                        @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace
{
    [CustomEntityDataAnnotation(""first argument"")]
    public partial class EntityWithAnnotation
    {
        [Key]
        public int Id { get; set; }
    }
}
",
                        code.AdditionalFiles.Single(f => f.Path == "EntityWithAnnotation.cs"));

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

        public virtual DbSet<EntityWithAnnotation> EntityWithAnnotation { get; set; }

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
            modelBuilder.Entity<EntityWithAnnotation>(entity =>
            {
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
                assertModel: null,
                skipBuild: true);
        }

        [ConditionalFact]
        public void Entity_property_with_custom_annotation()
        {
            Test(
                modelBuilder => modelBuilder
                    .Entity(
                        "EntityWithPropertyAnnotation",
                        x =>
                        {
                            x.Property<int>("Id")
                                .HasAnnotation("Custom:PropertyAnnotation", "first argument");
                            x.HasKey("Id");
                        }),
                new ModelCodeGenerationOptions { UseDataAnnotations = true },
                code =>
                {
                    AssertFileContents(
                        @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestNamespace
{
    public partial class EntityWithPropertyAnnotation
    {
        [Key]
        [CustomPropertyDataAnnotation(""first argument"")]
        public int Id { get; set; }
    }
}
",
                        code.AdditionalFiles.Single(f => f.Path == "EntityWithPropertyAnnotation.cs"));

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

        public virtual DbSet<EntityWithPropertyAnnotation> EntityWithPropertyAnnotation { get; set; }

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
            modelBuilder.Entity<EntityWithPropertyAnnotation>(entity =>
            {
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
                assertModel: null,
                skipBuild: true);
        }

        protected override void AddModelServices(IServiceCollection services)
        {
            services.Replace(ServiceDescriptor.Singleton<IRelationalAnnotationProvider, ModelAnnotationProvider>());
        }

        protected override void AddScaffoldingServices(IServiceCollection services)
        {
            services.Replace(ServiceDescriptor.Singleton<IAnnotationCodeGenerator, ModelAnnotationCodeGenerator>());
        }

        public class ModelAnnotationProvider : SqlServerAnnotationProvider
        {
            public ModelAnnotationProvider(RelationalAnnotationProviderDependencies dependencies)
                : base(dependencies)
            {
            }

            /// <inheritdoc />
            public override IEnumerable<IAnnotation> For(ITable table, bool designTime)
            {
                foreach (var annotation in base.For(table, designTime))
                {
                    yield return annotation;
                }

                var entityType = table.EntityTypeMappings.First().EntityType;

                foreach (var annotation in entityType.GetAnnotations().Where(a => a.Name == "Custom:EntityAnnotation"))
                {
                    yield return annotation;
                }
            }

            /// <inheritdoc />
            public override IEnumerable<IAnnotation> For(IColumn column, bool designTime)
            {
                foreach (var annotation in base.For(column, designTime))
                {
                    yield return annotation;
                }

                var properties = column.PropertyMappings.Select(m => m.Property);
                var annotations = properties.SelectMany(p => p.GetAnnotations()).GroupBy(a => a.Name).Select(g => g.First());

                foreach (var annotation in annotations.Where(a => a.Name == "Custom:PropertyAnnotation"))
                {
                    yield return annotation;
                }
            }
        }

        public class ModelAnnotationCodeGenerator : SqlServerAnnotationCodeGenerator
        {
            public ModelAnnotationCodeGenerator(AnnotationCodeGeneratorDependencies dependencies)
                : base(dependencies)
            {
            }

            protected override AttributeCodeFragment GenerateDataAnnotation(IEntityType entityType, IAnnotation annotation)
                => annotation.Name switch
                {
                    "Custom:EntityAnnotation" => new AttributeCodeFragment(
                        typeof(CustomEntityDataAnnotationAttribute), new object[] { annotation.Value as string }),
                    _ => base.GenerateDataAnnotation(entityType, annotation)
                };

            protected override AttributeCodeFragment GenerateDataAnnotation(IProperty property, IAnnotation annotation)
                => annotation.Name switch
                {
                    "Custom:PropertyAnnotation" => new AttributeCodeFragment(typeof(CustomPropertyDataAnnotationAttribute), new object[] {annotation.Value as string}),
                    _ => base.GenerateDataAnnotation(property, annotation)
                };
        }

        [AttributeUsage(AttributeTargets.Class)]
        public class CustomEntityDataAnnotationAttribute : Attribute
        {
            public CustomEntityDataAnnotationAttribute(string argument)
                => Argument = argument;

            public virtual string Argument { get; }
        }

        [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
        public class CustomPropertyDataAnnotationAttribute : Attribute
        {
            public CustomPropertyDataAnnotationAttribute(string argument)
                => Argument = argument;

            public virtual string Argument { get; }
        }
    }
}
