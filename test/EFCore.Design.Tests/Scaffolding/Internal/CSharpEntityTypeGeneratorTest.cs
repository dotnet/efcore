// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
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

#nullable disable

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

#nullable disable

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

#nullable disable

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

#nullable disable

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

#nullable disable

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

#nullable disable

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

#nullable disable

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

#nullable disable

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

#nullable disable

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
        public void RequiredAttribute_is_generated_for_property()
        {
            Test(
                modelBuilder => modelBuilder
                    .Entity(
                        "Entity",
                        x =>
                        {
                            x.Property<int>("Id");
                            x.Property<string>("RequiredString").IsRequired();
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

#nullable disable

namespace TestNamespace
{
    public partial class Entity
    {
        [Key]
        public int Id { get; set; }
        [Required]
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

#nullable disable

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

#nullable disable

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

#nullable disable

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

#nullable disable

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

#nullable disable

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

#nullable disable

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

#nullable disable

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

#nullable disable

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

#nullable disable

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

#nullable disable

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

#nullable disable

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
    }
}
