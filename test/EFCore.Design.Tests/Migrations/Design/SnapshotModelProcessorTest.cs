// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Migrations.Internal
{
    public class SnapshotModelProcessorTest
    {
        [ConditionalFact]
        public void Updates_provider_annotations_on_model()
        {
            var builder = new ModelBuilder(new ConventionSet());

            var model = builder.Model;
            ((Model)model).SetProductVersion("1.1.2");

            var entityType = builder.Entity<Blog>().Metadata;
            var property = builder.Entity<Blog>().Property(e => e.Id).Metadata;
            var key = builder.Entity<Blog>().HasKey(e => e.Id).Metadata;

            builder.Entity<Post>().Property(e => e.BlogId);
            var foreignKey = builder.Entity<Blog>().HasMany(e => e.Posts).WithOne(e => e.Blog).HasForeignKey(e => e.BlogId).Metadata;
            var nav1 = foreignKey.DependentToPrincipal;
            var nav2 = foreignKey.PrincipalToDependent;

            var index = builder.Entity<Post>().HasIndex(e => e.BlogId).Metadata;

            AddAnnotations(model);
            AddAnnotations(entityType);
            AddAnnotations(property);
            AddAnnotations(key);
            AddAnnotations(foreignKey);
            AddAnnotations(nav1);
            AddAnnotations(nav2);
            AddAnnotations(index);

            var reporter = new TestOperationReporter();

            new SnapshotModelProcessor(reporter).Process(model);

            AssertAnnotations(model);
            AssertAnnotations(entityType);
            AssertAnnotations(property);
            AssertAnnotations(key);
            AssertAnnotations(foreignKey);
            AssertAnnotations(nav1);
            AssertAnnotations(nav2);
            AssertAnnotations(index);

            Assert.Empty(reporter.Messages);
        }

        [ConditionalFact]
        public void Warns_for_conflicting_annotations()
        {
            var model = new Model();
            model.SetProductVersion("1.1.2");
            model["Unicorn:DefaultSchema"] = "Value1";
            model["Hippo:DefaultSchema"] = "Value2";

            Assert.Equal(3, model.GetAnnotations().Count());

            var reporter = new TestOperationReporter();

            new SnapshotModelProcessor(reporter).Process(model);

            Assert.Equal("warn: " + DesignStrings.MultipleAnnotationConflict("DefaultSchema"), reporter.Messages.Single());
            Assert.Equal(2, model.GetAnnotations().Count());

            var actual = (string)model["Relational:DefaultSchema"];
            Assert.True(actual == "Value1" || actual == "Value2");
        }

        [ConditionalFact]
        public void Warns_for_conflicting_annotations_one_relational()
        {
            var model = new Model();
            model.SetProductVersion("1.1.2");
            model["Unicorn:DefaultSchema"] = "Value1";
            model["Relational:DefaultSchema"] = "Value2";

            Assert.Equal(3, model.GetAnnotations().Count());

            var reporter = new TestOperationReporter();

            new SnapshotModelProcessor(reporter).Process(model);

            Assert.Equal("warn: " + DesignStrings.MultipleAnnotationConflict("DefaultSchema"), reporter.Messages.Single());
            Assert.Equal(2, model.GetAnnotations().Count());

            var actual = (string)model["Relational:DefaultSchema"];
            Assert.True(actual == "Value1" || actual == "Value2");
        }

        [ConditionalFact]
        public void Does_not_warn_for_duplicate_non_conflicting_annotations()
        {
            var model = new ModelBuilder(new ConventionSet()).Model;
            model.SetProductVersion("1.1.2");
            model["Unicorn:DefaultSchema"] = "Value";
            model["Hippo:DefaultSchema"] = "Value";

            Assert.Equal(3, model.GetAnnotations().Count());

            var reporter = new TestOperationReporter();

            new SnapshotModelProcessor(reporter).Process(model);

            Assert.Empty(reporter.Messages);

            Assert.Equal(2, model.GetAnnotations().Count());
            Assert.Equal("Value", (string)model["Relational:DefaultSchema"]);
        }

        [ConditionalFact]
        public void Does_not_process_non_v1_models()
        {
            var model = new Model();
            model.SetProductVersion("2.0.0");
            model["Unicorn:DefaultSchema"] = "Value";

            Assert.Equal(2, model.GetAnnotations().Count());

            var reporter = new TestOperationReporter();

            new SnapshotModelProcessor(reporter).Process(model);

            Assert.Empty(reporter.Messages);

            Assert.Equal(2, model.GetAnnotations().Count());
            Assert.Equal("Value", (string)model["Unicorn:DefaultSchema"]);
        }

        [ConditionalFact]
        public void Sets_owned_type_keys()
        {
            var builder = new ModelBuilder(new ConventionSet());

            var model = builder.Model;
            ((Model)model).SetProductVersion("2.1.0");

            builder.Entity<Blog>(
                b =>
                {
                    b.Property(e => e.Id);
                    b.HasKey(e => e.Id);

                    b.OwnsOne(e => e.Details).WithOwner().HasForeignKey(e => e.BlogId);
                });

            var reporter = new TestOperationReporter();
            new SnapshotModelProcessor(reporter).Process(model);

            Assert.Empty(reporter.Messages);
            Assert.Equal(
                nameof(BlogDetails.BlogId),
                model.FindEntityType(typeof(Blog)).FindNavigation(nameof(Blog.Details)).GetTargetType().FindPrimaryKey().Properties.Single()
                    .Name);
        }

        [ConditionalTheory]
        [InlineData(typeof(OwnershipModelSnapshot2_0))]
        [InlineData(typeof(OwnershipModelSnapshot2_1))]
        [InlineData(typeof(OwnershipModelSnapshot2_2))]
        [InlineData(typeof(OwnershipModelSnapshot3_0))]
        public void Can_diff_against_older_ownership_model(Type snapshotType)
        {
            using var db = new Ownership.OwnershipContext();
            var differ = db.GetService<IMigrationsModelDiffer>();
            var snapshot = (ModelSnapshot)Activator.CreateInstance(snapshotType);
            var reporter = new TestOperationReporter();
            var processor = new SnapshotModelProcessor(reporter);

            var differences = differ.GetDifferences(processor.Process(snapshot.Model), db.Model);

            Assert.Empty(differences);
        }

        private void AddAnnotations(IMutableAnnotatable element)
        {
            foreach (var annotationName in GetAnnotationNames()
                .Select(a => "Unicorn" + a.Substring(RelationalAnnotationNames.Prefix.Length - 1)))
            {
                element[annotationName] = "Value";
            }
        }

        private void AssertAnnotations(IMutableAnnotatable element)
        {
            foreach (var annotationName in GetAnnotationNames())
            {
                Assert.Equal("Value", (string)element[annotationName]);
            }
        }

        private static IEnumerable<string> GetAnnotationNames()
            => typeof(RelationalAnnotationNames)
                .GetTypeInfo()
                .GetRuntimeFields()
                .Where(p => p.Name != nameof(RelationalAnnotationNames.Prefix))
                .Select(p => (string)p.GetValue(null));

        private class Blog
        {
            public int Id { get; set; }

            public ICollection<Post> Posts { get; set; }
            public BlogDetails Details { get; set; }
        }

        private class Post
        {
            public int BlogId { get; set; }
            public Blog Blog { get; set; }
        }

        private class BlogDetails
        {
            public int BlogId { get; set; }

            public ICollection<Post> Posts { get; set; }
        }

        private class OwnershipModelSnapshot2_0 : ModelSnapshot
        {
            protected override void BuildModel(ModelBuilder modelBuilder)
            {
#pragma warning disable 612, 618
                modelBuilder
                    .HasAnnotation("ProductVersion", "2.0.3-rtm-10026")
                    .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                modelBuilder.Entity("Ownership.OwningType1", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.HasKey("Id");

                    b.ToTable("OwningType1");
                });

                modelBuilder.Entity("Ownership.OwningType2", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.HasKey("Id");

                    b.ToTable("OwningType2");
                });

                modelBuilder.Entity("Ownership.OwningType1", b =>
                {
                    b.OwnsOne("Ownership.OwnedType", "OwnedType1", b1 =>
                    {
                        b1.Property<int>("OwningType1Id");

                        b1.ToTable("OwningType1");

                        b1.HasOne("Ownership.OwningType1")
                            .WithOne("OwnedType1")
                            .HasForeignKey("Ownership.OwnedType", "OwningType1Id")
                            .OnDelete(DeleteBehavior.Cascade);

                        b1.OwnsOne("Ownership.NestedOwnedType", "NestedOwnedType1", b2 =>
                        {
                            b2.Property<int>("OwnedTypeOwningType1Id");

                            b2.Property<int>("Value");

                            b2.ToTable("OwningType1");

                            b2.HasOne("Ownership.OwnedType")
                                .WithOne("NestedOwnedType1")
                                .HasForeignKey("Ownership.NestedOwnedType", "OwnedTypeOwningType1Id")
                                .OnDelete(DeleteBehavior.Cascade);
                        });

                        b1.OwnsOne("Ownership.NestedOwnedType", "NestedOwnedType2", b2 =>
                        {
                            b2.Property<int>("OwnedTypeOwningType1Id");

                            b2.Property<int>("Value");

                            b2.ToTable("OwningType1");

                            b2.HasOne("Ownership.OwnedType")
                                .WithOne("NestedOwnedType2")
                                .HasForeignKey("Ownership.NestedOwnedType", "OwnedTypeOwningType1Id")
                                .OnDelete(DeleteBehavior.Cascade);
                        });
                    });

                    b.OwnsOne("Ownership.OwnedType", "OwnedType2", b1 =>
                    {
                        b1.Property<int?>("OwningType1Id");

                        b1.ToTable("OwningType1");

                        b1.HasOne("Ownership.OwningType1")
                            .WithOne("OwnedType2")
                            .HasForeignKey("Ownership.OwnedType", "OwningType1Id")
                            .OnDelete(DeleteBehavior.Cascade);

                        b1.OwnsOne("Ownership.NestedOwnedType", "NestedOwnedType1", b2 =>
                        {
                            b2.Property<int?>("OwnedTypeOwningType1Id");

                            b2.Property<int>("Value");

                            b2.ToTable("OwningType1");

                            b2.HasOne("Ownership.OwnedType")
                                .WithOne("NestedOwnedType1")
                                .HasForeignKey("Ownership.NestedOwnedType", "OwnedTypeOwningType1Id")
                                .OnDelete(DeleteBehavior.Cascade);
                        });

                        b1.OwnsOne("Ownership.NestedOwnedType", "NestedOwnedType2", b2 =>
                        {
                            b2.Property<int?>("OwnedTypeOwningType1Id");

                            b2.Property<int>("Value");

                            b2.ToTable("OwningType1");

                            b2.HasOne("Ownership.OwnedType")
                                .WithOne("NestedOwnedType2")
                                .HasForeignKey("Ownership.NestedOwnedType", "OwnedTypeOwningType1Id")
                                .OnDelete(DeleteBehavior.Cascade);
                        });
                    });
                });

                modelBuilder.Entity("Ownership.OwningType2", b =>
                {
                    b.OwnsOne("Ownership.OwnedType", "OwnedType1", b1 =>
                    {
                        b1.Property<int?>("OwningType2Id");

                        b1.ToTable("OwningType2");

                        b1.HasOne("Ownership.OwningType2")
                            .WithOne("OwnedType1")
                            .HasForeignKey("Ownership.OwnedType", "OwningType2Id")
                            .OnDelete(DeleteBehavior.Cascade);

                        b1.OwnsOne("Ownership.NestedOwnedType", "NestedOwnedType1", b2 =>
                        {
                            b2.Property<int?>("OwnedTypeOwningType2Id");

                            b2.Property<int>("Value");

                            b2.ToTable("OwningType2");

                            b2.HasOne("Ownership.OwnedType")
                                .WithOne("NestedOwnedType1")
                                .HasForeignKey("Ownership.NestedOwnedType", "OwnedTypeOwningType2Id")
                                .OnDelete(DeleteBehavior.Cascade);
                        });

                        b1.OwnsOne("Ownership.NestedOwnedType", "NestedOwnedType2", b2 =>
                        {
                            b2.Property<int?>("OwnedTypeOwningType2Id");

                            b2.Property<int>("Value");

                            b2.ToTable("OwningType2");

                            b2.HasOne("Ownership.OwnedType")
                                .WithOne("NestedOwnedType2")
                                .HasForeignKey("Ownership.NestedOwnedType", "OwnedTypeOwningType2Id")
                                .OnDelete(DeleteBehavior.Cascade);
                        });
                    });

                    b.OwnsOne("Ownership.OwnedType", "OwnedType2", b1 =>
                    {
                        b1.Property<int?>("OwningType2Id");

                        b1.ToTable("OwningType2");

                        b1.HasOne("Ownership.OwningType2")
                            .WithOne("OwnedType2")
                            .HasForeignKey("Ownership.OwnedType", "OwningType2Id")
                            .OnDelete(DeleteBehavior.Cascade);

                        b1.OwnsOne("Ownership.NestedOwnedType", "NestedOwnedType1", b2 =>
                        {
                            b2.Property<int?>("OwnedTypeOwningType2Id");

                            b2.Property<int>("Value");

                            b2.ToTable("OwningType2");

                            b2.HasOne("Ownership.OwnedType")
                                .WithOne("NestedOwnedType1")
                                .HasForeignKey("Ownership.NestedOwnedType", "OwnedTypeOwningType2Id")
                                .OnDelete(DeleteBehavior.Cascade);
                        });

                        b1.OwnsOne("Ownership.NestedOwnedType", "NestedOwnedType2", b2 =>
                        {
                            b2.Property<int?>("OwnedTypeOwningType2Id");

                            b2.Property<int>("Value");

                            b2.ToTable("OwningType2");

                            b2.HasOne("Ownership.OwnedType")
                                .WithOne("NestedOwnedType2")
                                .HasForeignKey("Ownership.NestedOwnedType", "OwnedTypeOwningType2Id")
                                .OnDelete(DeleteBehavior.Cascade);
                        });
                    });
                });
#pragma warning restore 612, 618
            }
        }

        private class OwnershipModelSnapshot2_1 : ModelSnapshot
        {
            protected override void BuildModel(ModelBuilder modelBuilder)
            {
#pragma warning disable 612, 618
                modelBuilder
                    .HasAnnotation("ProductVersion", "2.1.11-servicing-32099")
                    .HasAnnotation("Relational:MaxIdentifierLength", 128)
                    .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                modelBuilder.Entity("Ownership.OwningType1", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.HasKey("Id");

                    b.ToTable("OwningType1");
                });

                modelBuilder.Entity("Ownership.OwningType2", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.HasKey("Id");

                    b.ToTable("OwningType2");
                });

                modelBuilder.Entity("Ownership.OwningType1", b =>
                {
                    b.OwnsOne("Ownership.OwnedType", "OwnedType1", b1 =>
                    {
                        b1.Property<int>("OwningType1Id")
                            .ValueGeneratedOnAdd()
                            .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                        b1.ToTable("OwningType1");

                        b1.HasOne("Ownership.OwningType1")
                            .WithOne("OwnedType1")
                            .HasForeignKey("Ownership.OwnedType", "OwningType1Id")
                            .OnDelete(DeleteBehavior.Cascade);

                        b1.OwnsOne("Ownership.NestedOwnedType", "NestedOwnedType1", b2 =>
                        {
                            b2.Property<int>("OwnedTypeOwningType1Id")
                                .ValueGeneratedOnAdd()
                                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                            b2.Property<int>("Value");

                            b2.ToTable("OwningType1");

                            b2.HasOne("Ownership.OwnedType")
                                .WithOne("NestedOwnedType1")
                                .HasForeignKey("Ownership.NestedOwnedType", "OwnedTypeOwningType1Id")
                                .OnDelete(DeleteBehavior.Cascade);
                        });

                        b1.OwnsOne("Ownership.NestedOwnedType", "NestedOwnedType2", b2 =>
                        {
                            b2.Property<int>("OwnedTypeOwningType1Id")
                                .ValueGeneratedOnAdd()
                                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                            b2.Property<int>("Value");

                            b2.ToTable("OwningType1");

                            b2.HasOne("Ownership.OwnedType")
                                .WithOne("NestedOwnedType2")
                                .HasForeignKey("Ownership.NestedOwnedType", "OwnedTypeOwningType1Id")
                                .OnDelete(DeleteBehavior.Cascade);
                        });
                    });

                    b.OwnsOne("Ownership.OwnedType", "OwnedType2", b1 =>
                    {
                        b1.Property<int>("OwningType1Id")
                            .ValueGeneratedOnAdd()
                            .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                        b1.ToTable("OwningType1");

                        b1.HasOne("Ownership.OwningType1")
                            .WithOne("OwnedType2")
                            .HasForeignKey("Ownership.OwnedType", "OwningType1Id")
                            .OnDelete(DeleteBehavior.Cascade);

                        b1.OwnsOne("Ownership.NestedOwnedType", "NestedOwnedType1", b2 =>
                        {
                            b2.Property<int>("OwnedTypeOwningType1Id")
                                .ValueGeneratedOnAdd()
                                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                            b2.Property<int>("Value");

                            b2.ToTable("OwningType1");

                            b2.HasOne("Ownership.OwnedType")
                                .WithOne("NestedOwnedType1")
                                .HasForeignKey("Ownership.NestedOwnedType", "OwnedTypeOwningType1Id")
                                .OnDelete(DeleteBehavior.Cascade);
                        });

                        b1.OwnsOne("Ownership.NestedOwnedType", "NestedOwnedType2", b2 =>
                        {
                            b2.Property<int>("OwnedTypeOwningType1Id")
                                .ValueGeneratedOnAdd()
                                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                            b2.Property<int>("Value");

                            b2.ToTable("OwningType1");

                            b2.HasOne("Ownership.OwnedType")
                                .WithOne("NestedOwnedType2")
                                .HasForeignKey("Ownership.NestedOwnedType", "OwnedTypeOwningType1Id")
                                .OnDelete(DeleteBehavior.Cascade);
                        });
                    });
                });

                modelBuilder.Entity("Ownership.OwningType2", b =>
                {
                    b.OwnsOne("Ownership.OwnedType", "OwnedType1", b1 =>
                    {
                        b1.Property<int?>("OwningType2Id")
                            .ValueGeneratedOnAdd()
                            .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                        b1.ToTable("OwningType2");

                        b1.HasOne("Ownership.OwningType2")
                            .WithOne("OwnedType1")
                            .HasForeignKey("Ownership.OwnedType", "OwningType2Id")
                            .OnDelete(DeleteBehavior.Cascade);

                        b1.OwnsOne("Ownership.NestedOwnedType", "NestedOwnedType1", b2 =>
                        {
                            b2.Property<int?>("OwnedTypeOwningType2Id")
                                .ValueGeneratedOnAdd()
                                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                            b2.Property<int>("Value");

                            b2.ToTable("OwningType2");

                            b2.HasOne("Ownership.OwnedType")
                                .WithOne("NestedOwnedType1")
                                .HasForeignKey("Ownership.NestedOwnedType", "OwnedTypeOwningType2Id")
                                .OnDelete(DeleteBehavior.Cascade);
                        });

                        b1.OwnsOne("Ownership.NestedOwnedType", "NestedOwnedType2", b2 =>
                        {
                            b2.Property<int?>("OwnedTypeOwningType2Id")
                                .ValueGeneratedOnAdd()
                                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                            b2.Property<int>("Value");

                            b2.ToTable("OwningType2");

                            b2.HasOne("Ownership.OwnedType")
                                .WithOne("NestedOwnedType2")
                                .HasForeignKey("Ownership.NestedOwnedType", "OwnedTypeOwningType2Id")
                                .OnDelete(DeleteBehavior.Cascade);
                        });
                    });

                    b.OwnsOne("Ownership.OwnedType", "OwnedType2", b1 =>
                    {
                        b1.Property<int?>("OwningType2Id")
                            .ValueGeneratedOnAdd()
                            .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                        b1.ToTable("OwningType2");

                        b1.HasOne("Ownership.OwningType2")
                            .WithOne("OwnedType2")
                            .HasForeignKey("Ownership.OwnedType", "OwningType2Id")
                            .OnDelete(DeleteBehavior.Cascade);

                        b1.OwnsOne("Ownership.NestedOwnedType", "NestedOwnedType1", b2 =>
                        {
                            b2.Property<int?>("OwnedTypeOwningType2Id")
                                .ValueGeneratedOnAdd()
                                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                            b2.Property<int>("Value");

                            b2.ToTable("OwningType2");

                            b2.HasOne("Ownership.OwnedType")
                                .WithOne("NestedOwnedType1")
                                .HasForeignKey("Ownership.NestedOwnedType", "OwnedTypeOwningType2Id")
                                .OnDelete(DeleteBehavior.Cascade);
                        });

                        b1.OwnsOne("Ownership.NestedOwnedType", "NestedOwnedType2", b2 =>
                        {
                            b2.Property<int?>("OwnedTypeOwningType2Id")
                                .ValueGeneratedOnAdd()
                                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                            b2.Property<int>("Value");

                            b2.ToTable("OwningType2");

                            b2.HasOne("Ownership.OwnedType")
                                .WithOne("NestedOwnedType2")
                                .HasForeignKey("Ownership.NestedOwnedType", "OwnedTypeOwningType2Id")
                                .OnDelete(DeleteBehavior.Cascade);
                        });
                    });
                });
#pragma warning restore 612, 618
            }
        }

        private class OwnershipModelSnapshot2_2 : ModelSnapshot
        {
            protected override void BuildModel(ModelBuilder modelBuilder)
            {
#pragma warning disable 612, 618
                modelBuilder
                    .HasAnnotation("ProductVersion", "2.2.6-servicing-10079")
                    .HasAnnotation("Relational:MaxIdentifierLength", 128)
                    .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                modelBuilder.Entity("Ownership.OwningType1", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.HasKey("Id");

                    b.ToTable("OwningType1");
                });

                modelBuilder.Entity("Ownership.OwningType2", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.HasKey("Id");

                    b.ToTable("OwningType2");
                });

                modelBuilder.Entity("Ownership.OwningType1", b =>
                {
                    b.OwnsOne("Ownership.OwnedType", "OwnedType1", b1 =>
                    {
                        b1.Property<int>("OwningType1Id")
                            .ValueGeneratedOnAdd()
                            .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                        b1.HasKey("OwningType1Id");

                        b1.ToTable("OwningType1");

                        b1.HasOne("Ownership.OwningType1")
                            .WithOne("OwnedType1")
                            .HasForeignKey("Ownership.OwnedType", "OwningType1Id")
                            .OnDelete(DeleteBehavior.Cascade);

                        b1.OwnsOne("Ownership.NestedOwnedType", "NestedOwnedType1", b2 =>
                        {
                            b2.Property<int>("OwnedTypeOwningType1Id")
                                .ValueGeneratedOnAdd()
                                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                            b2.Property<int>("Value");

                            b2.HasKey("OwnedTypeOwningType1Id");

                            b2.ToTable("OwningType1");

                            b2.HasOne("Ownership.OwnedType")
                                .WithOne("NestedOwnedType1")
                                .HasForeignKey("Ownership.NestedOwnedType", "OwnedTypeOwningType1Id")
                                .OnDelete(DeleteBehavior.Cascade);
                        });

                        b1.OwnsOne("Ownership.NestedOwnedType", "NestedOwnedType2", b2 =>
                        {
                            b2.Property<int>("OwnedTypeOwningType1Id")
                                .ValueGeneratedOnAdd()
                                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                            b2.Property<int>("Value");

                            b2.HasKey("OwnedTypeOwningType1Id");

                            b2.ToTable("OwningType1");

                            b2.HasOne("Ownership.OwnedType")
                                .WithOne("NestedOwnedType2")
                                .HasForeignKey("Ownership.NestedOwnedType", "OwnedTypeOwningType1Id")
                                .OnDelete(DeleteBehavior.Cascade);
                        });
                    });

                    b.OwnsOne("Ownership.OwnedType", "OwnedType2", b1 =>
                    {
                        b1.Property<int>("OwningType1Id")
                            .ValueGeneratedOnAdd()
                            .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                        b1.HasKey("OwningType1Id");

                        b1.ToTable("OwningType1");

                        b1.HasOne("Ownership.OwningType1")
                            .WithOne("OwnedType2")
                            .HasForeignKey("Ownership.OwnedType", "OwningType1Id")
                            .OnDelete(DeleteBehavior.Cascade);

                        b1.OwnsOne("Ownership.NestedOwnedType", "NestedOwnedType1", b2 =>
                        {
                            b2.Property<int>("OwnedTypeOwningType1Id")
                                .ValueGeneratedOnAdd()
                                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                            b2.Property<int>("Value");

                            b2.HasKey("OwnedTypeOwningType1Id");

                            b2.ToTable("OwningType1");

                            b2.HasOne("Ownership.OwnedType")
                                .WithOne("NestedOwnedType1")
                                .HasForeignKey("Ownership.NestedOwnedType", "OwnedTypeOwningType1Id")
                                .OnDelete(DeleteBehavior.Cascade);
                        });

                        b1.OwnsOne("Ownership.NestedOwnedType", "NestedOwnedType2", b2 =>
                        {
                            b2.Property<int>("OwnedTypeOwningType1Id")
                                .ValueGeneratedOnAdd()
                                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                            b2.Property<int>("Value");

                            b2.HasKey("OwnedTypeOwningType1Id");

                            b2.ToTable("OwningType1");

                            b2.HasOne("Ownership.OwnedType")
                                .WithOne("NestedOwnedType2")
                                .HasForeignKey("Ownership.NestedOwnedType", "OwnedTypeOwningType1Id")
                                .OnDelete(DeleteBehavior.Cascade);
                        });
                    });
                });

                modelBuilder.Entity("Ownership.OwningType2", b =>
                {
                    b.OwnsOne("Ownership.OwnedType", "OwnedType1", b1 =>
                    {
                        b1.Property<int>("OwningType2Id")
                            .ValueGeneratedOnAdd()
                            .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                        b1.HasKey("OwningType2Id");

                        b1.ToTable("OwningType2");

                        b1.HasOne("Ownership.OwningType2")
                            .WithOne("OwnedType1")
                            .HasForeignKey("Ownership.OwnedType", "OwningType2Id")
                            .OnDelete(DeleteBehavior.Cascade);

                        b1.OwnsOne("Ownership.NestedOwnedType", "NestedOwnedType1", b2 =>
                        {
                            b2.Property<int?>("OwnedTypeOwningType2Id")
                                .ValueGeneratedOnAdd()
                                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                            b2.Property<int>("Value");

                            b2.HasKey("OwnedTypeOwningType2Id");

                            b2.ToTable("OwningType2");

                            b2.HasOne("Ownership.OwnedType")
                                .WithOne("NestedOwnedType1")
                                .HasForeignKey("Ownership.NestedOwnedType", "OwnedTypeOwningType2Id")
                                .OnDelete(DeleteBehavior.Cascade);
                        });

                        b1.OwnsOne("Ownership.NestedOwnedType", "NestedOwnedType2", b2 =>
                        {
                            b2.Property<int?>("OwnedTypeOwningType2Id")
                                .ValueGeneratedOnAdd()
                                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                            b2.Property<int>("Value");

                            b2.HasKey("OwnedTypeOwningType2Id");

                            b2.ToTable("OwningType2");

                            b2.HasOne("Ownership.OwnedType")
                                .WithOne("NestedOwnedType2")
                                .HasForeignKey("Ownership.NestedOwnedType", "OwnedTypeOwningType2Id")
                                .OnDelete(DeleteBehavior.Cascade);
                        });
                    });

                    b.OwnsOne("Ownership.OwnedType", "OwnedType2", b1 =>
                    {
                        b1.Property<int>("OwningType2Id")
                            .ValueGeneratedOnAdd()
                            .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                        b1.HasKey("OwningType2Id");

                        b1.ToTable("OwningType2");

                        b1.HasOne("Ownership.OwningType2")
                            .WithOne("OwnedType2")
                            .HasForeignKey("Ownership.OwnedType", "OwningType2Id")
                            .OnDelete(DeleteBehavior.Cascade);

                        b1.OwnsOne("Ownership.NestedOwnedType", "NestedOwnedType1", b2 =>
                        {
                            b2.Property<int?>("OwnedTypeOwningType2Id")
                                .ValueGeneratedOnAdd()
                                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                            b2.Property<int>("Value");

                            b2.HasKey("OwnedTypeOwningType2Id");

                            b2.ToTable("OwningType2");

                            b2.HasOne("Ownership.OwnedType")
                                .WithOne("NestedOwnedType1")
                                .HasForeignKey("Ownership.NestedOwnedType", "OwnedTypeOwningType2Id")
                                .OnDelete(DeleteBehavior.Cascade);
                        });

                        b1.OwnsOne("Ownership.NestedOwnedType", "NestedOwnedType2", b2 =>
                        {
                            b2.Property<int?>("OwnedTypeOwningType2Id")
                                .ValueGeneratedOnAdd()
                                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                            b2.Property<int>("Value");

                            b2.HasKey("OwnedTypeOwningType2Id");

                            b2.ToTable("OwningType2");

                            b2.HasOne("Ownership.OwnedType")
                                .WithOne("NestedOwnedType2")
                                .HasForeignKey("Ownership.NestedOwnedType", "OwnedTypeOwningType2Id")
                                .OnDelete(DeleteBehavior.Cascade);
                        });
                    });
                });
#pragma warning restore 612, 618
            }
        }

        private class OwnershipModelSnapshot3_0 : ModelSnapshot
        {
            protected override void BuildModel(ModelBuilder modelBuilder)
            {
#pragma warning disable 612, 618
                modelBuilder
                    .HasAnnotation("ProductVersion", "3.0.0")
                    .HasAnnotation("Relational:MaxIdentifierLength", 128)
                    .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                modelBuilder.Entity("Ownership.OwningType1", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.HasKey("Id");

                    b.ToTable("OwningType1");
                });

                modelBuilder.Entity("Ownership.OwningType2", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.HasKey("Id");

                    b.ToTable("OwningType2");
                });

                modelBuilder.Entity("Ownership.OwningType1", b =>
                {
                    b.OwnsOne("Ownership.OwnedType", "OwnedType1", b1 =>
                    {
                        b1.Property<int>("OwningType1Id")
                            .ValueGeneratedOnAdd()
                            .HasColumnType("int")
                            .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                        b1.HasKey("OwningType1Id");

                        b1.ToTable("OwningType1");

                        b1.WithOwner()
                            .HasForeignKey("OwningType1Id");

                        b1.OwnsOne("Ownership.NestedOwnedType", "NestedOwnedType1", b2 =>
                        {
                            b2.Property<int>("OwnedTypeOwningType1Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("int")
                                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                            b2.Property<int>("Value")
                                .HasColumnType("int");

                            b2.HasKey("OwnedTypeOwningType1Id");

                            b2.ToTable("OwningType1");

                            b2.WithOwner()
                                .HasForeignKey("OwnedTypeOwningType1Id");
                        });

                        b1.OwnsOne("Ownership.NestedOwnedType", "NestedOwnedType2", b2 =>
                        {
                            b2.Property<int>("OwnedTypeOwningType1Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("int")
                                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                            b2.Property<int>("Value")
                                .HasColumnType("int");

                            b2.HasKey("OwnedTypeOwningType1Id");

                            b2.ToTable("OwningType1");

                            b2.WithOwner()
                                .HasForeignKey("OwnedTypeOwningType1Id");
                        });
                    });

                    b.OwnsOne("Ownership.OwnedType", "OwnedType2", b1 =>
                    {
                        b1.Property<int>("OwningType1Id")
                            .ValueGeneratedOnAdd()
                            .HasColumnType("int")
                            .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                        b1.HasKey("OwningType1Id");

                        b1.ToTable("OwningType1");

                        b1.WithOwner()
                            .HasForeignKey("OwningType1Id");

                        b1.OwnsOne("Ownership.NestedOwnedType", "NestedOwnedType1", b2 =>
                        {
                            b2.Property<int>("OwnedTypeOwningType1Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("int")
                                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                            b2.Property<int>("Value")
                                .HasColumnType("int");

                            b2.HasKey("OwnedTypeOwningType1Id");

                            b2.ToTable("OwningType1");

                            b2.WithOwner()
                                .HasForeignKey("OwnedTypeOwningType1Id");
                        });

                        b1.OwnsOne("Ownership.NestedOwnedType", "NestedOwnedType2", b2 =>
                        {
                            b2.Property<int>("OwnedTypeOwningType1Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("int")
                                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                            b2.Property<int>("Value")
                                .HasColumnType("int");

                            b2.HasKey("OwnedTypeOwningType1Id");

                            b2.ToTable("OwningType1");

                            b2.WithOwner()
                                .HasForeignKey("OwnedTypeOwningType1Id");
                        });
                    });
                });

                modelBuilder.Entity("Ownership.OwningType2", b =>
                {
                    b.OwnsOne("Ownership.OwnedType", "OwnedType1", b1 =>
                    {
                        b1.Property<int>("OwningType2Id")
                            .ValueGeneratedOnAdd()
                            .HasColumnType("int")
                            .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                        b1.HasKey("OwningType2Id");

                        b1.ToTable("OwningType2");

                        b1.WithOwner()
                            .HasForeignKey("OwningType2Id");

                        b1.OwnsOne("Ownership.NestedOwnedType", "NestedOwnedType1", b2 =>
                        {
                            b2.Property<int?>("OwnedTypeOwningType2Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("int")
                                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                            b2.Property<int>("Value")
                                .HasColumnType("int");

                            b2.HasKey("OwnedTypeOwningType2Id");

                            b2.ToTable("OwningType2");

                            b2.WithOwner()
                                .HasForeignKey("OwnedTypeOwningType2Id");
                        });

                        b1.OwnsOne("Ownership.NestedOwnedType", "NestedOwnedType2", b2 =>
                        {
                            b2.Property<int?>("OwnedTypeOwningType2Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("int")
                                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                            b2.Property<int>("Value")
                                .HasColumnType("int");

                            b2.HasKey("OwnedTypeOwningType2Id");

                            b2.ToTable("OwningType2");

                            b2.WithOwner()
                                .HasForeignKey("OwnedTypeOwningType2Id");
                        });
                    });

                    b.OwnsOne("Ownership.OwnedType", "OwnedType2", b1 =>
                    {
                        b1.Property<int>("OwningType2Id")
                            .ValueGeneratedOnAdd()
                            .HasColumnType("int")
                            .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                        b1.HasKey("OwningType2Id");

                        b1.ToTable("OwningType2");

                        b1.WithOwner()
                            .HasForeignKey("OwningType2Id");

                        b1.OwnsOne("Ownership.NestedOwnedType", "NestedOwnedType1", b2 =>
                        {
                            b2.Property<int?>("OwnedTypeOwningType2Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("int")
                                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                            b2.Property<int>("Value")
                                .HasColumnType("int");

                            b2.HasKey("OwnedTypeOwningType2Id");

                            b2.ToTable("OwningType2");

                            b2.WithOwner()
                                .HasForeignKey("OwnedTypeOwningType2Id");
                        });

                        b1.OwnsOne("Ownership.NestedOwnedType", "NestedOwnedType2", b2 =>
                        {
                            b2.Property<int?>("OwnedTypeOwningType2Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("int")
                                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                            b2.Property<int>("Value")
                                .HasColumnType("int");

                            b2.HasKey("OwnedTypeOwningType2Id");

                            b2.ToTable("OwningType2");

                            b2.WithOwner()
                                .HasForeignKey("OwnedTypeOwningType2Id");
                        });
                    });
                });
#pragma warning restore 612, 618
            }
        }
    }
}

namespace Ownership
{
    internal class OwningType1
    {
        public int Id { get; set; }
        public OwnedType OwnedType1 { get; set; }
        public OwnedType OwnedType2 { get; set; }
    }

    internal class OwningType2
    {
        public int Id { get; set; }
        public OwnedType OwnedType1 { get; set; }
        public OwnedType OwnedType2 { get; set; }
    }

    [Owned]
    internal class OwnedType
    {
        public NestedOwnedType NestedOwnedType1 { get; set; }
        public NestedOwnedType NestedOwnedType2 { get; set; }
    }

    [Owned]
    internal class NestedOwnedType
    {
        public int Value { get; set; }
    }

    internal class OwnershipContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlServer(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Ownership");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OwningType1>();
            modelBuilder.Entity<OwningType2>();
        }
    }
}
