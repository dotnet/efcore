// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Ownership;

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
            var builder = new ModelBuilder();

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

            new SnapshotModelProcessor(reporter, DummyModelRuntimeInitializer.Instance).Process(model);

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
        public void Can_resolve_ISnapshotModelProcessor_from_DI()
        {
            var assembly = typeof(SnapshotModelProcessorTest).Assembly;
            var snapshotModelProcessor = new DesignTimeServicesBuilder(assembly, assembly, new TestOperationReporter(), [])
                .Build(SqlServerTestHelpers.Instance.CreateContext())
                .CreateScope()
                .ServiceProvider
                .GetRequiredService<ISnapshotModelProcessor>();

            Assert.NotNull(snapshotModelProcessor);
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

            new SnapshotModelProcessor(reporter, DummyModelRuntimeInitializer.Instance).Process(model);

            var (level, message) = reporter.Messages.Single();
            Assert.Equal(LogLevel.Warning, level);
            Assert.Equal(DesignStrings.MultipleAnnotationConflict("DefaultSchema"), message);
            Assert.Equal(2, model.GetAnnotations().Count());

            var actual = (string)model["Relational:DefaultSchema"];
            Assert.True(actual is "Value1" or "Value2");
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

            new SnapshotModelProcessor(reporter, DummyModelRuntimeInitializer.Instance).Process(model);

            var (level, message) = reporter.Messages.Single();
            Assert.Equal(LogLevel.Warning, level);
            Assert.Equal(DesignStrings.MultipleAnnotationConflict("DefaultSchema"), message);
            Assert.Equal(2, model.GetAnnotations().Count());

            var actual = (string)model["Relational:DefaultSchema"];
            Assert.True(actual is "Value1" or "Value2");
        }

        [ConditionalFact]
        public void Does_not_warn_for_duplicate_non_conflicting_annotations()
        {
            var model = new ModelBuilder().Model;
            model.SetProductVersion("1.1.2");
            model["Unicorn:DefaultSchema"] = "Value";
            model["Hippo:DefaultSchema"] = "Value";

            Assert.Equal(3, model.GetAnnotations().Count());

            var reporter = new TestOperationReporter();

            new SnapshotModelProcessor(reporter, DummyModelRuntimeInitializer.Instance).Process(model);

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

            new SnapshotModelProcessor(reporter, DummyModelRuntimeInitializer.Instance).Process(model);

            Assert.Empty(reporter.Messages);

            Assert.Equal(2, model.GetAnnotations().Count());
            Assert.Equal("Value", (string)model["Unicorn:DefaultSchema"]);
        }

        [ConditionalFact]
        public void Sets_owned_type_keys()
        {
            var builder = new ModelBuilder();

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
            new SnapshotModelProcessor(reporter, DummyModelRuntimeInitializer.Instance).Process(model);

            Assert.Empty(reporter.Messages);
            Assert.Equal(
                nameof(BlogDetails.BlogId),
                model.FindEntityType(typeof(Blog)).FindNavigation(nameof(Blog.Details)).TargetEntityType.FindPrimaryKey().Properties
                    .Single()
                    .Name);
        }

        [ConditionalTheory]
        [InlineData(typeof(OwnershipModelSnapshot2_0))]
        [InlineData(typeof(OwnershipModelSnapshot2_1))]
        [InlineData(typeof(OwnershipModelSnapshot2_2))]
        [InlineData(typeof(OwnershipModelSnapshot3_0))]
        public void Can_diff_against_older_ownership_model(Type snapshotType)
        {
            using var context = new OwnershipContext();
            AssertSameSnapshot(snapshotType, context);
        }

        [ConditionalTheory]
        [InlineData(typeof(SequenceModelSnapshot1_1))]
        [InlineData(typeof(SequenceModelSnapshot2_2))]
        [InlineData(typeof(SequenceModelSnapshot3_1))]
        public void Can_diff_against_older_sequence_model(Type snapshotType)
        {
            using var context = new SequenceContext();
            AssertSameSnapshot(snapshotType, context);
        }

        private static void AssertSameSnapshot(Type snapshotType, DbContext context)
        {
            var differ = context.GetService<IMigrationsModelDiffer>();
            var snapshot = (ModelSnapshot)Activator.CreateInstance(snapshotType);
            var reporter = new TestOperationReporter();
            var modelRuntimeInitializer =
                SqlServerTestHelpers.Instance.CreateContextServices().GetRequiredService<IModelRuntimeInitializer>();

            var model = PreprocessModel(snapshot);
            model = new SnapshotModelProcessor(reporter, modelRuntimeInitializer).Process(model, resetVersion: true);
            var currentModel = context.GetService<IDesignTimeModel>().Model;

            var differences = differ.GetDifferences(
                model.GetRelationalModel(),
                currentModel.GetRelationalModel());

            Assert.Empty(differences);

            var generator = CSharpMigrationsGeneratorTest.CreateMigrationsCodeGenerator();

            var oldSnapshotCode = generator.GenerateSnapshot(
                "MyNamespace",
                context.GetType(),
                "MySnapshot",
                model);

            var newSnapshotCode = generator.GenerateSnapshot(
                "MyNamespace",
                context.GetType(),
                "MySnapshot",
                currentModel);

            Assert.Equal(newSnapshotCode, oldSnapshotCode);
        }

        private static IModel PreprocessModel(ModelSnapshot snapshot)
        {
            var model = snapshot.Model;
            if (model.FindAnnotation(RelationalAnnotationNames.MaxIdentifierLength) == null)
            {
                ((Model)model)[RelationalAnnotationNames.MaxIdentifierLength] = 128;
            }

            foreach (EntityType entityType in model.GetEntityTypes())
            {
                var schemaAnnotation = entityType.FindAnnotation(RelationalAnnotationNames.Schema);
                if (schemaAnnotation != null
                    && schemaAnnotation.Value == null)
                {
                    entityType.RemoveAnnotation(RelationalAnnotationNames.Schema);
                }

                foreach (var property in entityType.GetProperties())
                {
                    if (property.IsForeignKey())
                    {
                        if (property.ValueGenerated != ValueGenerated.Never)
                        {
                            property.SetValueGenerated(null, ConfigurationSource.Explicit);
                        }

                        if (property.GetValueGenerationStrategy() != SqlServerValueGenerationStrategy.None)
                        {
                            property.SetValueGenerationStrategy(null);
                        }
                    }
                    else if (property.GetValueGenerationStrategy() is SqlServerValueGenerationStrategy strategy
                        && strategy != SqlServerValueGenerationStrategy.None)
                    {
                        property.SetValueGenerationStrategy(strategy);
                    }
                }
            }

            return model;
        }

        private void AddAnnotations(IMutableAnnotatable element)
        {
            foreach (var annotationName in GetAnnotationNames()
                         .Where(
                             a => a != RelationalAnnotationNames.MaxIdentifierLength
#pragma warning disable CS0618 // Type or member is obsolete
                                 && a != RelationalAnnotationNames.SequencePrefix
#pragma warning restore CS0618 // Type or member is obsolete
                                 && a.IndexOf(':') > 0)
                         .Select(a => "Unicorn" + a.Substring(RelationalAnnotationNames.Prefix.Length - 1)))
            {
                element[annotationName] = "Value";
            }
        }

        private void AssertAnnotations(IMutableAnnotatable element)
        {
            foreach (var annotationName in GetAnnotationNames()
                         .Where(
                             a => a != RelationalAnnotationNames.MaxIdentifierLength
                                 && a != RelationalAnnotationNames.RelationalModel
                                 && a != RelationalAnnotationNames.DefaultMappings
                                 && a != RelationalAnnotationNames.DefaultColumnMappings
                                 && a != RelationalAnnotationNames.TableMappings
                                 && a != RelationalAnnotationNames.TableColumnMappings
                                 && a != RelationalAnnotationNames.ViewMappings
                                 && a != RelationalAnnotationNames.ViewColumnMappings
                                 && a != RelationalAnnotationNames.SqlQueryMappings
                                 && a != RelationalAnnotationNames.SqlQueryColumnMappings
                                 && a != RelationalAnnotationNames.FunctionMappings
                                 && a != RelationalAnnotationNames.FunctionColumnMappings
                                 && a != RelationalAnnotationNames.ForeignKeyMappings
                                 && a != RelationalAnnotationNames.TableIndexMappings
                                 && a != RelationalAnnotationNames.UniqueConstraintMappings
                                 && a != RelationalAnnotationNames.RelationalOverrides
                                 && a != RelationalAnnotationNames.MappingFragments
#pragma warning disable CS0618 // Type or member is obsolete
                                 && a != RelationalAnnotationNames.SequencePrefix
#pragma warning restore CS0618 // Type or member is obsolete
                                 && a.IndexOf(':') > 0))
            {
                Assert.Equal("Value", (string)element[annotationName]);
            }
        }

        private static IEnumerable<string> GetAnnotationNames()
            => RelationalAnnotationNames.AllNames;

        private class DummyModelRuntimeInitializer : IModelRuntimeInitializer
        {
            private DummyModelRuntimeInitializer()
            {
            }

            public IModel Initialize(IModel model, IDiagnosticsLogger<DbLoggerCategory.Model.Validation> validationLogger)
                => model;

            public IModel Initialize(
                IModel model,
                bool designTime = true,
                IDiagnosticsLogger<DbLoggerCategory.Model.Validation> validationLogger = null)
                => model;

            public static DummyModelRuntimeInitializer Instance { get; } = new();
        }

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

                modelBuilder.Entity(
                    "Ownership.OwningType1", b =>
                    {
                        b.Property<int>("Id")
                            .ValueGeneratedOnAdd();

                        b.HasKey("Id");

                        b.ToTable("OwningType1");
                    });

                modelBuilder.Entity(
                    "Ownership.OwningType2", b =>
                    {
                        b.Property<int>("Id")
                            .ValueGeneratedOnAdd();

                        b.HasKey("Id");

                        b.ToTable("OwningType2");
                    });

                modelBuilder.Entity(
                    "Ownership.OwningType1", b =>
                    {
                        b.OwnsOne(
                            "Ownership.OwnedType", "OwnedType1", b1 =>
                            {
                                b1.Property<int>("OwningType1Id");

                                b1.Property<bool>("Exists");

                                b1.ToTable("OwningType1");

                                b1.HasOne("Ownership.OwningType1")
                                    .WithOne("OwnedType1")
                                    .HasForeignKey("Ownership.OwnedType", "OwningType1Id")
                                    .OnDelete(DeleteBehavior.Cascade);

                                b1.OwnsOne(
                                    "Ownership.NestedOwnedType", "NestedOwnedType1", b2 =>
                                    {
                                        b2.Property<int>("OwnedTypeOwningType1Id");

                                        b2.Property<int>("Value");

                                        b2.ToTable("OwningType1");

                                        b2.HasOne("Ownership.OwnedType")
                                            .WithOne("NestedOwnedType1")
                                            .HasForeignKey("Ownership.NestedOwnedType", "OwnedTypeOwningType1Id")
                                            .OnDelete(DeleteBehavior.Cascade);
                                    });

                                b1.OwnsOne(
                                    "Ownership.NestedOwnedType", "NestedOwnedType2", b2 =>
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

                        b.OwnsOne(
                            "Ownership.OwnedType", "OwnedType2", b1 =>
                            {
                                b1.Property<int?>("OwningType1Id");

                                b1.Property<bool>("Exists");

                                b1.ToTable("OwningType1");

                                b1.HasOne("Ownership.OwningType1")
                                    .WithOne("OwnedType2")
                                    .HasForeignKey("Ownership.OwnedType", "OwningType1Id")
                                    .OnDelete(DeleteBehavior.Cascade);

                                b1.OwnsOne(
                                    "Ownership.NestedOwnedType", "NestedOwnedType1", b2 =>
                                    {
                                        b2.Property<int?>("OwnedTypeOwningType1Id");

                                        b2.Property<int>("Value");

                                        b2.ToTable("OwningType1");

                                        b2.HasOne("Ownership.OwnedType")
                                            .WithOne("NestedOwnedType1")
                                            .HasForeignKey("Ownership.NestedOwnedType", "OwnedTypeOwningType1Id")
                                            .OnDelete(DeleteBehavior.Cascade);
                                    });

                                b1.OwnsOne(
                                    "Ownership.NestedOwnedType", "NestedOwnedType2", b2 =>
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

                modelBuilder.Entity(
                    "Ownership.OwningType2", b =>
                    {
                        b.OwnsOne(
                            "Ownership.OwnedType", "OwnedType1", b1 =>
                            {
                                b1.Property<int?>("OwningType2Id");

                                b1.Property<bool>("Exists");

                                b1.ToTable("OwningType2");

                                b1.HasOne("Ownership.OwningType2")
                                    .WithOne("OwnedType1")
                                    .HasForeignKey("Ownership.OwnedType", "OwningType2Id")
                                    .OnDelete(DeleteBehavior.Cascade);

                                b1.OwnsOne(
                                    "Ownership.NestedOwnedType", "NestedOwnedType1", b2 =>
                                    {
                                        b2.Property<int?>("OwnedTypeOwningType2Id");

                                        b2.Property<int>("Value");

                                        b2.ToTable("OwningType2");

                                        b2.HasOne("Ownership.OwnedType")
                                            .WithOne("NestedOwnedType1")
                                            .HasForeignKey("Ownership.NestedOwnedType", "OwnedTypeOwningType2Id")
                                            .OnDelete(DeleteBehavior.Cascade);
                                    });

                                b1.OwnsOne(
                                    "Ownership.NestedOwnedType", "NestedOwnedType2", b2 =>
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

                        b.OwnsOne(
                            "Ownership.OwnedType", "OwnedType2", b1 =>
                            {
                                b1.Property<int?>("OwningType2Id");

                                b1.Property<bool>("Exists");

                                b1.ToTable("OwningType2");

                                b1.HasOne("Ownership.OwningType2")
                                    .WithOne("OwnedType2")
                                    .HasForeignKey("Ownership.OwnedType", "OwningType2Id")
                                    .OnDelete(DeleteBehavior.Cascade);

                                b1.OwnsOne(
                                    "Ownership.NestedOwnedType", "NestedOwnedType1", b2 =>
                                    {
                                        b2.Property<int?>("OwnedTypeOwningType2Id");

                                        b2.Property<int>("Value");

                                        b2.ToTable("OwningType2");

                                        b2.HasOne("Ownership.OwnedType")
                                            .WithOne("NestedOwnedType1")
                                            .HasForeignKey("Ownership.NestedOwnedType", "OwnedTypeOwningType2Id")
                                            .OnDelete(DeleteBehavior.Cascade);
                                    });

                                b1.OwnsOne(
                                    "Ownership.NestedOwnedType", "NestedOwnedType2", b2 =>
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

                modelBuilder.Entity(
                    "Ownership.OwningType1", b =>
                    {
                        b.Property<int>("Id")
                            .ValueGeneratedOnAdd()
                            .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                        b.HasKey("Id");

                        b.ToTable("OwningType1");
                    });

                modelBuilder.Entity(
                    "Ownership.OwningType2", b =>
                    {
                        b.Property<int>("Id")
                            .ValueGeneratedOnAdd()
                            .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                        b.HasKey("Id");

                        b.ToTable("OwningType2");
                    });

                modelBuilder.Entity(
                    "Ownership.OwningType1", b =>
                    {
                        b.OwnsOne(
                            "Ownership.OwnedType", "OwnedType1", b1 =>
                            {
                                b1.Property<int>("OwningType1Id")
                                    .ValueGeneratedOnAdd()
                                    .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                                b1.Property<bool>("Exists");

                                b1.ToTable("OwningType1");

                                b1.HasOne("Ownership.OwningType1")
                                    .WithOne("OwnedType1")
                                    .HasForeignKey("Ownership.OwnedType", "OwningType1Id")
                                    .OnDelete(DeleteBehavior.Cascade);

                                b1.OwnsOne(
                                    "Ownership.NestedOwnedType", "NestedOwnedType1", b2 =>
                                    {
                                        b2.Property<int>("OwnedTypeOwningType1Id")
                                            .ValueGeneratedOnAdd()
                                            .HasAnnotation(
                                                "SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                                        b2.Property<int>("Value");

                                        b2.ToTable("OwningType1");

                                        b2.HasOne("Ownership.OwnedType")
                                            .WithOne("NestedOwnedType1")
                                            .HasForeignKey("Ownership.NestedOwnedType", "OwnedTypeOwningType1Id")
                                            .OnDelete(DeleteBehavior.Cascade);
                                    });

                                b1.OwnsOne(
                                    "Ownership.NestedOwnedType", "NestedOwnedType2", b2 =>
                                    {
                                        b2.Property<int>("OwnedTypeOwningType1Id")
                                            .ValueGeneratedOnAdd()
                                            .HasAnnotation(
                                                "SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                                        b2.Property<int>("Value");

                                        b2.ToTable("OwningType1");

                                        b2.HasOne("Ownership.OwnedType")
                                            .WithOne("NestedOwnedType2")
                                            .HasForeignKey("Ownership.NestedOwnedType", "OwnedTypeOwningType1Id")
                                            .OnDelete(DeleteBehavior.Cascade);
                                    });
                            });

                        b.OwnsOne(
                            "Ownership.OwnedType", "OwnedType2", b1 =>
                            {
                                b1.Property<int>("OwningType1Id")
                                    .ValueGeneratedOnAdd()
                                    .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                                b1.Property<bool>("Exists");

                                b1.ToTable("OwningType1");

                                b1.HasOne("Ownership.OwningType1")
                                    .WithOne("OwnedType2")
                                    .HasForeignKey("Ownership.OwnedType", "OwningType1Id")
                                    .OnDelete(DeleteBehavior.Cascade);

                                b1.OwnsOne(
                                    "Ownership.NestedOwnedType", "NestedOwnedType1", b2 =>
                                    {
                                        b2.Property<int>("OwnedTypeOwningType1Id")
                                            .ValueGeneratedOnAdd()
                                            .HasAnnotation(
                                                "SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                                        b2.Property<int>("Value");

                                        b2.ToTable("OwningType1");

                                        b2.HasOne("Ownership.OwnedType")
                                            .WithOne("NestedOwnedType1")
                                            .HasForeignKey("Ownership.NestedOwnedType", "OwnedTypeOwningType1Id")
                                            .OnDelete(DeleteBehavior.Cascade);
                                    });

                                b1.OwnsOne(
                                    "Ownership.NestedOwnedType", "NestedOwnedType2", b2 =>
                                    {
                                        b2.Property<int>("OwnedTypeOwningType1Id")
                                            .ValueGeneratedOnAdd()
                                            .HasAnnotation(
                                                "SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                                        b2.Property<int>("Value");

                                        b2.ToTable("OwningType1");

                                        b2.HasOne("Ownership.OwnedType")
                                            .WithOne("NestedOwnedType2")
                                            .HasForeignKey("Ownership.NestedOwnedType", "OwnedTypeOwningType1Id")
                                            .OnDelete(DeleteBehavior.Cascade);
                                    });
                            });
                    });

                modelBuilder.Entity(
                    "Ownership.OwningType2", b =>
                    {
                        b.OwnsOne(
                            "Ownership.OwnedType", "OwnedType1", b1 =>
                            {
                                b1.Property<int?>("OwningType2Id")
                                    .ValueGeneratedOnAdd()
                                    .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                                b1.Property<bool>("Exists");

                                b1.ToTable("OwningType2");

                                b1.HasOne("Ownership.OwningType2")
                                    .WithOne("OwnedType1")
                                    .HasForeignKey("Ownership.OwnedType", "OwningType2Id")
                                    .OnDelete(DeleteBehavior.Cascade);

                                b1.OwnsOne(
                                    "Ownership.NestedOwnedType", "NestedOwnedType1", b2 =>
                                    {
                                        b2.Property<int?>("OwnedTypeOwningType2Id")
                                            .ValueGeneratedOnAdd()
                                            .HasAnnotation(
                                                "SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                                        b2.Property<int>("Value");

                                        b2.ToTable("OwningType2");

                                        b2.HasOne("Ownership.OwnedType")
                                            .WithOne("NestedOwnedType1")
                                            .HasForeignKey("Ownership.NestedOwnedType", "OwnedTypeOwningType2Id")
                                            .OnDelete(DeleteBehavior.Cascade);
                                    });

                                b1.OwnsOne(
                                    "Ownership.NestedOwnedType", "NestedOwnedType2", b2 =>
                                    {
                                        b2.Property<int?>("OwnedTypeOwningType2Id")
                                            .ValueGeneratedOnAdd()
                                            .HasAnnotation(
                                                "SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                                        b2.Property<int>("Value");

                                        b2.ToTable("OwningType2");

                                        b2.HasOne("Ownership.OwnedType")
                                            .WithOne("NestedOwnedType2")
                                            .HasForeignKey("Ownership.NestedOwnedType", "OwnedTypeOwningType2Id")
                                            .OnDelete(DeleteBehavior.Cascade);
                                    });
                            });

                        b.OwnsOne(
                            "Ownership.OwnedType", "OwnedType2", b1 =>
                            {
                                b1.Property<int?>("OwningType2Id")
                                    .ValueGeneratedOnAdd()
                                    .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                                b1.Property<bool>("Exists");

                                b1.ToTable("OwningType2");

                                b1.HasOne("Ownership.OwningType2")
                                    .WithOne("OwnedType2")
                                    .HasForeignKey("Ownership.OwnedType", "OwningType2Id")
                                    .OnDelete(DeleteBehavior.Cascade);

                                b1.OwnsOne(
                                    "Ownership.NestedOwnedType", "NestedOwnedType1", b2 =>
                                    {
                                        b2.Property<int?>("OwnedTypeOwningType2Id")
                                            .ValueGeneratedOnAdd()
                                            .HasAnnotation(
                                                "SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                                        b2.Property<int>("Value");

                                        b2.ToTable("OwningType2");

                                        b2.HasOne("Ownership.OwnedType")
                                            .WithOne("NestedOwnedType1")
                                            .HasForeignKey("Ownership.NestedOwnedType", "OwnedTypeOwningType2Id")
                                            .OnDelete(DeleteBehavior.Cascade);
                                    });

                                b1.OwnsOne(
                                    "Ownership.NestedOwnedType", "NestedOwnedType2", b2 =>
                                    {
                                        b2.Property<int?>("OwnedTypeOwningType2Id")
                                            .ValueGeneratedOnAdd()
                                            .HasAnnotation(
                                                "SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

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

                modelBuilder.Entity(
                    "Ownership.OwningType1", b =>
                    {
                        b.Property<int>("Id")
                            .ValueGeneratedOnAdd()
                            .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                        b.HasKey("Id");

                        b.ToTable("OwningType1");
                    });

                modelBuilder.Entity(
                    "Ownership.OwningType2", b =>
                    {
                        b.Property<int>("Id")
                            .ValueGeneratedOnAdd()
                            .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                        b.HasKey("Id");

                        b.ToTable("OwningType2");
                    });

                modelBuilder.Entity(
                    "Ownership.OwningType1", b =>
                    {
                        b.OwnsOne(
                            "Ownership.OwnedType", "OwnedType1", b1 =>
                            {
                                b1.Property<int>("OwningType1Id")
                                    .ValueGeneratedOnAdd()
                                    .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                                b1.Property<bool>("Exists");

                                b1.HasKey("OwningType1Id");

                                b1.ToTable("OwningType1");

                                b1.HasOne("Ownership.OwningType1")
                                    .WithOne("OwnedType1")
                                    .HasForeignKey("Ownership.OwnedType", "OwningType1Id")
                                    .OnDelete(DeleteBehavior.Cascade);

                                b1.OwnsOne(
                                    "Ownership.NestedOwnedType", "NestedOwnedType1", b2 =>
                                    {
                                        b2.Property<int>("OwnedTypeOwningType1Id")
                                            .ValueGeneratedOnAdd()
                                            .HasAnnotation(
                                                "SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                                        b2.Property<int>("Value");

                                        b2.HasKey("OwnedTypeOwningType1Id");

                                        b2.ToTable("OwningType1");

                                        b2.HasOne("Ownership.OwnedType")
                                            .WithOne("NestedOwnedType1")
                                            .HasForeignKey("Ownership.NestedOwnedType", "OwnedTypeOwningType1Id")
                                            .OnDelete(DeleteBehavior.Cascade);
                                    });

                                b1.OwnsOne(
                                    "Ownership.NestedOwnedType", "NestedOwnedType2", b2 =>
                                    {
                                        b2.Property<int>("OwnedTypeOwningType1Id")
                                            .ValueGeneratedOnAdd()
                                            .HasAnnotation(
                                                "SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                                        b2.Property<int>("Value");

                                        b2.HasKey("OwnedTypeOwningType1Id");

                                        b2.ToTable("OwningType1");

                                        b2.HasOne("Ownership.OwnedType")
                                            .WithOne("NestedOwnedType2")
                                            .HasForeignKey("Ownership.NestedOwnedType", "OwnedTypeOwningType1Id")
                                            .OnDelete(DeleteBehavior.Cascade);
                                    });
                            });

                        b.OwnsOne(
                            "Ownership.OwnedType", "OwnedType2", b1 =>
                            {
                                b1.Property<int>("OwningType1Id")
                                    .ValueGeneratedOnAdd()
                                    .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                                b1.Property<bool>("Exists");

                                b1.HasKey("OwningType1Id");

                                b1.ToTable("OwningType1");

                                b1.HasOne("Ownership.OwningType1")
                                    .WithOne("OwnedType2")
                                    .HasForeignKey("Ownership.OwnedType", "OwningType1Id")
                                    .OnDelete(DeleteBehavior.Cascade);

                                b1.OwnsOne(
                                    "Ownership.NestedOwnedType", "NestedOwnedType1", b2 =>
                                    {
                                        b2.Property<int>("OwnedTypeOwningType1Id")
                                            .ValueGeneratedOnAdd()
                                            .HasAnnotation(
                                                "SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                                        b2.Property<int>("Value");

                                        b2.HasKey("OwnedTypeOwningType1Id");

                                        b2.ToTable("OwningType1");

                                        b2.HasOne("Ownership.OwnedType")
                                            .WithOne("NestedOwnedType1")
                                            .HasForeignKey("Ownership.NestedOwnedType", "OwnedTypeOwningType1Id")
                                            .OnDelete(DeleteBehavior.Cascade);
                                    });

                                b1.OwnsOne(
                                    "Ownership.NestedOwnedType", "NestedOwnedType2", b2 =>
                                    {
                                        b2.Property<int>("OwnedTypeOwningType1Id")
                                            .ValueGeneratedOnAdd()
                                            .HasAnnotation(
                                                "SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

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

                modelBuilder.Entity(
                    "Ownership.OwningType2", b =>
                    {
                        b.OwnsOne(
                            "Ownership.OwnedType", "OwnedType1", b1 =>
                            {
                                b1.Property<int>("OwningType2Id")
                                    .ValueGeneratedOnAdd()
                                    .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                                b1.Property<bool>("Exists");

                                b1.HasKey("OwningType2Id");

                                b1.ToTable("OwningType2");

                                b1.HasOne("Ownership.OwningType2")
                                    .WithOne("OwnedType1")
                                    .HasForeignKey("Ownership.OwnedType", "OwningType2Id")
                                    .OnDelete(DeleteBehavior.Cascade);

                                b1.OwnsOne(
                                    "Ownership.NestedOwnedType", "NestedOwnedType1", b2 =>
                                    {
                                        b2.Property<int?>("OwnedTypeOwningType2Id")
                                            .ValueGeneratedOnAdd()
                                            .HasAnnotation(
                                                "SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                                        b2.Property<int>("Value");

                                        b2.HasKey("OwnedTypeOwningType2Id");

                                        b2.ToTable("OwningType2");

                                        b2.HasOne("Ownership.OwnedType")
                                            .WithOne("NestedOwnedType1")
                                            .HasForeignKey("Ownership.NestedOwnedType", "OwnedTypeOwningType2Id")
                                            .OnDelete(DeleteBehavior.Cascade);
                                    });

                                b1.OwnsOne(
                                    "Ownership.NestedOwnedType", "NestedOwnedType2", b2 =>
                                    {
                                        b2.Property<int?>("OwnedTypeOwningType2Id")
                                            .ValueGeneratedOnAdd()
                                            .HasAnnotation(
                                                "SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                                        b2.Property<int>("Value");

                                        b2.HasKey("OwnedTypeOwningType2Id");

                                        b2.ToTable("OwningType2");

                                        b2.HasOne("Ownership.OwnedType")
                                            .WithOne("NestedOwnedType2")
                                            .HasForeignKey("Ownership.NestedOwnedType", "OwnedTypeOwningType2Id")
                                            .OnDelete(DeleteBehavior.Cascade);
                                    });
                            });

                        b.OwnsOne(
                            "Ownership.OwnedType", "OwnedType2", b1 =>
                            {
                                b1.Property<int>("OwningType2Id")
                                    .ValueGeneratedOnAdd()
                                    .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                                b1.Property<bool>("Exists");

                                b1.HasKey("OwningType2Id");

                                b1.ToTable("OwningType2");

                                b1.HasOne("Ownership.OwningType2")
                                    .WithOne("OwnedType2")
                                    .HasForeignKey("Ownership.OwnedType", "OwningType2Id")
                                    .OnDelete(DeleteBehavior.Cascade);

                                b1.OwnsOne(
                                    "Ownership.NestedOwnedType", "NestedOwnedType1", b2 =>
                                    {
                                        b2.Property<int?>("OwnedTypeOwningType2Id")
                                            .ValueGeneratedOnAdd()
                                            .HasAnnotation(
                                                "SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                                        b2.Property<int>("Value");

                                        b2.HasKey("OwnedTypeOwningType2Id");

                                        b2.ToTable("OwningType2");

                                        b2.HasOne("Ownership.OwnedType")
                                            .WithOne("NestedOwnedType1")
                                            .HasForeignKey("Ownership.NestedOwnedType", "OwnedTypeOwningType2Id")
                                            .OnDelete(DeleteBehavior.Cascade);
                                    });

                                b1.OwnsOne(
                                    "Ownership.NestedOwnedType", "NestedOwnedType2", b2 =>
                                    {
                                        b2.Property<int?>("OwnedTypeOwningType2Id")
                                            .ValueGeneratedOnAdd()
                                            .HasAnnotation(
                                                "SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

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
                    .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn)
                    .HasAnnotation("SqlServer:IdentitySeed", 1);

                modelBuilder.Entity(
                    "Ownership.OwningType1", b =>
                    {
                        b.Property<int>("Id")
                            .ValueGeneratedOnAdd()
                            .HasColumnType("int")
                            .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn)
                            .HasAnnotation("SqlServer:IdentitySeed", 1);

                        b.HasKey("Id");

                        b.ToTable("OwningType1");
                    });

                modelBuilder.Entity(
                    "Ownership.OwningType2", b =>
                    {
                        b.Property<int>("Id")
                            .ValueGeneratedOnAdd()
                            .HasColumnType("int")
                            .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                        b.HasKey("Id");

                        b.ToTable("OwningType2");
                    });

                modelBuilder.Entity(
                    "Ownership.OwningType1", b =>
                    {
                        b.OwnsOne(
                            "Ownership.OwnedType", "OwnedType1", b1 =>
                            {
                                b1.Property<int>("OwningType1Id")
                                    .ValueGeneratedOnAdd()
                                    .HasColumnType("int")
                                    .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                                b1.Property<bool>("Exists");

                                b1.HasKey("OwningType1Id");

                                b1.ToTable("OwningType1");

                                b1.WithOwner()
                                    .HasForeignKey("OwningType1Id");

                                b1.OwnsOne(
                                    "Ownership.NestedOwnedType", "NestedOwnedType1", b2 =>
                                    {
                                        b2.Property<int>("OwnedTypeOwningType1Id")
                                            .ValueGeneratedOnAdd()
                                            .HasColumnType("int")
                                            .HasAnnotation(
                                                "SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                                        b2.Property<int>("Value")
                                            .HasColumnType("int");

                                        b2.HasKey("OwnedTypeOwningType1Id");

                                        b2.ToTable("OwningType1");

                                        b2.WithOwner()
                                            .HasForeignKey("OwnedTypeOwningType1Id");
                                    });

                                b1.OwnsOne(
                                    "Ownership.NestedOwnedType", "NestedOwnedType2", b2 =>
                                    {
                                        b2.Property<int>("OwnedTypeOwningType1Id")
                                            .ValueGeneratedOnAdd()
                                            .HasColumnType("int")
                                            .HasAnnotation(
                                                "SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                                        b2.Property<int>("Value")
                                            .HasColumnType("int");

                                        b2.HasKey("OwnedTypeOwningType1Id");

                                        b2.ToTable("OwningType1");

                                        b2.WithOwner()
                                            .HasForeignKey("OwnedTypeOwningType1Id");
                                    });
                            });

                        b.OwnsOne(
                            "Ownership.OwnedType", "OwnedType2", b1 =>
                            {
                                b1.Property<int>("OwningType1Id")
                                    .ValueGeneratedOnAdd()
                                    .HasColumnType("int")
                                    .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                                b1.Property<bool>("Exists");

                                b1.HasKey("OwningType1Id");

                                b1.ToTable("OwningType1");

                                b1.WithOwner()
                                    .HasForeignKey("OwningType1Id");

                                b1.OwnsOne(
                                    "Ownership.NestedOwnedType", "NestedOwnedType1", b2 =>
                                    {
                                        b2.Property<int>("OwnedTypeOwningType1Id")
                                            .ValueGeneratedOnAdd()
                                            .HasColumnType("int")
                                            .HasAnnotation(
                                                "SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                                        b2.Property<int>("Value")
                                            .HasColumnType("int");

                                        b2.HasKey("OwnedTypeOwningType1Id");

                                        b2.ToTable("OwningType1");

                                        b2.WithOwner()
                                            .HasForeignKey("OwnedTypeOwningType1Id");
                                    });

                                b1.OwnsOne(
                                    "Ownership.NestedOwnedType", "NestedOwnedType2", b2 =>
                                    {
                                        b2.Property<int>("OwnedTypeOwningType1Id")
                                            .ValueGeneratedOnAdd()
                                            .HasColumnType("int")
                                            .HasAnnotation(
                                                "SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                                        b2.Property<int>("Value")
                                            .HasColumnType("int");

                                        b2.HasKey("OwnedTypeOwningType1Id");

                                        b2.ToTable("OwningType1");

                                        b2.WithOwner()
                                            .HasForeignKey("OwnedTypeOwningType1Id");
                                    });
                            });
                    });

                modelBuilder.Entity(
                    "Ownership.OwningType2", b =>
                    {
                        b.OwnsOne(
                            "Ownership.OwnedType", "OwnedType1", b1 =>
                            {
                                b1.Property<int>("OwningType2Id")
                                    .ValueGeneratedOnAdd()
                                    .HasColumnType("int")
                                    .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                                b1.Property<bool>("Exists");

                                b1.HasKey("OwningType2Id");

                                b1.ToTable("OwningType2");

                                b1.WithOwner()
                                    .HasForeignKey("OwningType2Id");

                                b1.OwnsOne(
                                    "Ownership.NestedOwnedType", "NestedOwnedType1", b2 =>
                                    {
                                        b2.Property<int?>("OwnedTypeOwningType2Id")
                                            .ValueGeneratedOnAdd()
                                            .HasColumnType("int")
                                            .HasAnnotation(
                                                "SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                                        b2.Property<int>("Value")
                                            .HasColumnType("int");

                                        b2.HasKey("OwnedTypeOwningType2Id");

                                        b2.ToTable("OwningType2");

                                        b2.WithOwner()
                                            .HasForeignKey("OwnedTypeOwningType2Id");
                                    });

                                b1.OwnsOne(
                                    "Ownership.NestedOwnedType", "NestedOwnedType2", b2 =>
                                    {
                                        b2.Property<int?>("OwnedTypeOwningType2Id")
                                            .ValueGeneratedOnAdd()
                                            .HasColumnType("int")
                                            .HasAnnotation(
                                                "SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                                        b2.Property<int>("Value")
                                            .HasColumnType("int");

                                        b2.HasKey("OwnedTypeOwningType2Id");

                                        b2.ToTable("OwningType2");

                                        b2.WithOwner()
                                            .HasForeignKey("OwnedTypeOwningType2Id");
                                    });
                            });

                        b.OwnsOne(
                            "Ownership.OwnedType", "OwnedType2", b1 =>
                            {
                                b1.Property<int>("OwningType2Id")
                                    .ValueGeneratedOnAdd()
                                    .HasColumnType("int")
                                    .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                                b1.Property<bool>("Exists");

                                b1.HasKey("OwningType2Id");

                                b1.ToTable("OwningType2");

                                b1.WithOwner()
                                    .HasForeignKey("OwningType2Id");

                                b1.OwnsOne(
                                    "Ownership.NestedOwnedType", "NestedOwnedType1", b2 =>
                                    {
                                        b2.Property<int?>("OwnedTypeOwningType2Id")
                                            .ValueGeneratedOnAdd()
                                            .HasColumnType("int")
                                            .HasAnnotation(
                                                "SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                                        b2.Property<int>("Value")
                                            .HasColumnType("int");

                                        b2.HasKey("OwnedTypeOwningType2Id");

                                        b2.ToTable("OwningType2");

                                        b2.WithOwner()
                                            .HasForeignKey("OwnedTypeOwningType2Id");
                                    });

                                b1.OwnsOne(
                                    "Ownership.NestedOwnedType", "NestedOwnedType2", b2 =>
                                    {
                                        b2.Property<int?>("OwnedTypeOwningType2Id")
                                            .ValueGeneratedOnAdd()
                                            .HasColumnType("int")
                                            .HasAnnotation(
                                                "SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

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

        private class SequenceModelSnapshot1_1 : ModelSnapshot
        {
            protected override void BuildModel(ModelBuilder modelBuilder)
                => modelBuilder
                    .HasAnnotation("ChangeDetector.SkipDetectChanges", "true")
                    .HasAnnotation("ProductVersion", "1.1.6")
                    .HasAnnotation("Relational:Sequence:Bar.Foo", "'Foo', 'Bar', '2', '2', '1', '3', 'Int32', 'True', 'True', '20'")
                    .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);
        }

        private class SequenceModelSnapshot2_2 : ModelSnapshot
        {
            protected override void BuildModel(ModelBuilder modelBuilder)
            {
#pragma warning disable 612, 618
                modelBuilder
                    .HasAnnotation("ChangeDetector.SkipDetectChanges", "true")
                    .HasAnnotation("ProductVersion", "2.2.2-servicing-10034")
                    .HasAnnotation("Relational:MaxIdentifierLength", 128)
                    .HasAnnotation("Relational:Sequence:Bar.Foo", "'Foo', 'Bar', '2', '2', '1', '3', 'Int32', 'True', 'True', '20'")
                    .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);
#pragma warning restore 612, 618
            }
        }

        private class SequenceModelSnapshot3_1 : ModelSnapshot
        {
            protected override void BuildModel(ModelBuilder modelBuilder)
            {
#pragma warning disable 612, 618
                modelBuilder
                    .HasAnnotation("ProductVersion", "3.1.1")
                    .HasAnnotation("Relational:MaxIdentifierLength", 128)
                    .HasAnnotation("Relational:Sequence:Bar.Foo", "'Foo', 'Bar', '2', '2', '1', '3', 'Int32', 'True', 'True', '20'")
                    .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);
#pragma warning restore 612, 618
            }
        }

        private class SequenceContext : DbContext
        {
            protected override void OnConfiguring(DbContextOptionsBuilder options)
                => options.UseSqlServer();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.HasSequence<int>("Foo", "Bar")
                    .StartsAt(2)
                    .HasMin(1)
                    .HasMax(3)
                    .IncrementsBy(2)
                    .IsCyclic()
                    .UseCache(20);
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
        public bool Exists { get; set; }
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
            => options.UseSqlServer();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OwningType1>();
            modelBuilder.Entity<OwningType2>();
        }
    }
}
