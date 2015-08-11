// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Entity.Commands.TestUtilities;
using Microsoft.Data.Entity.Commands.Utilities;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Tests;
using Xunit;

namespace Microsoft.Data.Entity.Commands.Migrations
{
    public class ModelSnapshotTest
    {
        public class EntityWithOneProperty
        {
            public int Id { get; set; }
        }

        public class EntityWithTwoProperties
        {
            public int Id { get; set; }
            public int AlternateId { get; set; }
        }

        public class EntityWithStringProperty
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class EntityWithStringKey
        {
            public string Id { get; set; }
        }

        public class BaseEntity
        {
            public int Id { get; set; }
        }

        public class DerivedEntity : BaseEntity
        {
            public string Name { get; set; }
        }

        public class AnotherDerivedEntity : BaseEntity
        {
            public string Title { get; set; }
        }

        #region Model

        [Fact]
        public void Model_annotations_are_stored_in_snapshot()
        {
            Test(
                builder => { builder.Annotation("AnnotationName", "AnnotationValue"); },
                @"builder
    .Annotation(""AnnotationName"", ""AnnotationValue"");
",
                o =>
                    {
                        Assert.Equal(1, o.Annotations.Count());
                        Assert.Equal("AnnotationValue", o["AnnotationName"]);
                    });
        }

        [Fact]
        public void Entities_are_stored_in_model_snapshot()
        {
            Test(
                builder =>
                    {
                        builder.Entity<EntityWithOneProperty>();
                        builder.Entity<EntityWithTwoProperties>();
                    },
                @"
builder.Entity(""Microsoft.Data.Entity.Commands.Migrations.ModelSnapshotTest+EntityWithOneProperty"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Key(""Id"");
    });

builder.Entity(""Microsoft.Data.Entity.Commands.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"");

        b.Key(""Id"");
    });
",
                o =>
                    {
                        Assert.Equal(2, o.EntityTypes.Count);
                        Assert.Collection(
                            o.EntityTypes,
                            t => Assert.Equal("Microsoft.Data.Entity.Commands.Migrations.ModelSnapshotTest+EntityWithOneProperty", t.Name),
                            t => Assert.Equal("Microsoft.Data.Entity.Commands.Migrations.ModelSnapshotTest+EntityWithTwoProperties", t.Name));
                    });
        }

        #endregion

        #region EntityType

        [Fact]
        public void EntityType_annotations_are_stored_in_snapshot()
        {
            Test(
                builder => { builder.Entity<EntityWithOneProperty>().Annotation("AnnotationName", "AnnotationValue"); },
                @"
builder.Entity(""Microsoft.Data.Entity.Commands.Migrations.ModelSnapshotTest+EntityWithOneProperty"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Key(""Id"");

        b.Annotation(""AnnotationName"", ""AnnotationValue"");
    });
",
                o =>
                    {
                        Assert.Equal(1, o.EntityTypes[0].Annotations.Count());
                        Assert.Equal("AnnotationValue", o.EntityTypes[0]["AnnotationName"]);
                    });
        }

        [Fact]
        public void BaseType_is_stored_in_snapshot()
        {
            Test(
                builder =>
                    {
                        builder.Entity<DerivedEntity>().BaseType<BaseEntity>();
                        builder.Entity<AnotherDerivedEntity>().BaseType<BaseEntity>();
                    },
                @"
builder.Entity(""Microsoft.Data.Entity.Commands.Migrations.ModelSnapshotTest+BaseEntity"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Key(""Id"");
    });

builder.Entity(""Microsoft.Data.Entity.Commands.Migrations.ModelSnapshotTest+AnotherDerivedEntity"", b =>
    {
        b.BaseType(""Microsoft.Data.Entity.Commands.Migrations.ModelSnapshotTest+BaseEntity"");

        b.Property<string>(""Title"");
    });

builder.Entity(""Microsoft.Data.Entity.Commands.Migrations.ModelSnapshotTest+DerivedEntity"", b =>
    {
        b.BaseType(""Microsoft.Data.Entity.Commands.Migrations.ModelSnapshotTest+BaseEntity"");

        b.Property<string>(""Name"");
    });
",
                o =>
                    {
                        Assert.Equal(3, o.EntityTypes.Count());
                        Assert.Collection(
                            o.EntityTypes,
                            t => Assert.Equal("Microsoft.Data.Entity.Commands.Migrations.ModelSnapshotTest+AnotherDerivedEntity", t.Name),
                            t => Assert.Equal("Microsoft.Data.Entity.Commands.Migrations.ModelSnapshotTest+BaseEntity", t.Name),
                            t => Assert.Equal("Microsoft.Data.Entity.Commands.Migrations.ModelSnapshotTest+DerivedEntity", t.Name)
                            );
                    });
        }

        [Fact]
        public void Properties_are_stored_in_snapshot()
        {
            Test(
                builder => { builder.Entity<EntityWithTwoProperties>(); },
                @"
builder.Entity(""Microsoft.Data.Entity.Commands.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"");

        b.Key(""Id"");
    });
",
                o =>
                    {
                        Assert.Equal(2, o.EntityTypes.First().GetProperties().Count());
                        Assert.Collection(
                            o.EntityTypes.First().GetProperties(),
                            t => Assert.Equal("Id", t.Name),
                            t => Assert.Equal("AlternateId", t.Name)
                            );
                    });
        }

        [Fact]
        public void Primary_key_is_stored_in_snapshot()
        {
            Test(
                builder => { builder.Entity<EntityWithTwoProperties>().Key(t => new { t.Id, t.AlternateId }); },
                @"
builder.Entity(""Microsoft.Data.Entity.Commands.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"");

        b.Property<int>(""AlternateId"");

        b.Key(""Id"", ""AlternateId"");
    });
",
                o =>
                    {
                        Assert.Equal(2, o.EntityTypes.First().GetPrimaryKey().Properties.Count);
                        Assert.Collection(
                            o.EntityTypes.First().GetPrimaryKey().Properties,
                            t => Assert.Equal("Id", t.Name),
                            t => Assert.Equal("AlternateId", t.Name)
                            );
                    });
        }

        [Fact]
        public void Alternate_keys_are_stored_in_snapshot()
        {
            Test(
                builder => { builder.Entity<EntityWithTwoProperties>().AlternateKey(t => new { t.Id, t.AlternateId }); },
                @"
builder.Entity(""Microsoft.Data.Entity.Commands.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"");

        b.Key(""Id"");

        b.AlternateKey(""Id"", ""AlternateId"");
    });
",
                o =>
                {
                    Assert.Collection(
                        o.EntityTypes.First().GetDeclaredKeys().First(k => k.Properties.Count == 2).Properties,
                        t => Assert.Equal("Id", t.Name),
                        t => Assert.Equal("AlternateId", t.Name)
                        );
                });
        }

        [Fact]
        public void Indexes_are_stored_in_snapshot()
        {
            Test(
                builder => { builder.Entity<EntityWithTwoProperties>().Index(t => t.AlternateId); },
                @"
builder.Entity(""Microsoft.Data.Entity.Commands.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"");

        b.Key(""Id"");

        b.Index(""AlternateId"");
    });
",
                o =>
                    {
                        Assert.Equal(1, o.EntityTypes.First().GetIndexes().Count());
                        Assert.Equal("AlternateId", o.EntityTypes.First().GetIndexes().First().Properties[0].Name);
                    });
        }

        [Fact]
        public void Indexes_are_stored_in_snapshot_including_composite_index()
        {
            Test(
                builder => { builder.Entity<EntityWithTwoProperties>().Index(t => new { t.Id, t.AlternateId }); },
                @"
builder.Entity(""Microsoft.Data.Entity.Commands.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"");

        b.Key(""Id"");

        b.Index(""Id"", ""AlternateId"");
    });
",
                o =>
                    {
                        Assert.Equal(1, o.EntityTypes.First().GetIndexes().Count());
                        Assert.Collection(
                            o.EntityTypes.First().GetIndexes().First().Properties,
                            t => Assert.Equal("Id", t.Name),
                            t => Assert.Equal("AlternateId", t.Name));
                    });
        }

        [Fact]
        public void Foreign_keys_are_stored_in_snapshot()
        {
            Test(
                builder => { builder.Entity<EntityWithTwoProperties>().Reference<EntityWithOneProperty>().InverseReference().ForeignKey<EntityWithTwoProperties>(e => e.AlternateId); },
                @"
builder.Entity(""Microsoft.Data.Entity.Commands.Migrations.ModelSnapshotTest+EntityWithOneProperty"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Key(""Id"");
    });

builder.Entity(""Microsoft.Data.Entity.Commands.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"");

        b.Key(""Id"");
    });

builder.Entity(""Microsoft.Data.Entity.Commands.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.Reference(""Microsoft.Data.Entity.Commands.Migrations.ModelSnapshotTest+EntityWithOneProperty"")
            .InverseReference()
            .ForeignKey(""Microsoft.Data.Entity.Commands.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", ""AlternateId"");
    });
",
                o =>
                    {
                        Assert.Equal(1, o.FindEntityType(typeof(EntityWithTwoProperties)).GetForeignKeys().Count());
                        Assert.Equal("AlternateId", o.FindEntityType(typeof(EntityWithTwoProperties)).GetForeignKeys().First().Properties[0].Name);
                    });
        }

        [Fact]
        public void Relationship_principal_key_is_stored_in_snapshot()
        {
            Test(
                builder =>
                    {
                        builder.Entity<EntityWithOneProperty>().Reference<EntityWithTwoProperties>().InverseReference()
                            .ForeignKey<EntityWithOneProperty>(e => e.Id).
                            PrincipalKey<EntityWithTwoProperties>(e => e.AlternateId);
                    },
                @"
builder.Entity(""Microsoft.Data.Entity.Commands.Migrations.ModelSnapshotTest+EntityWithOneProperty"", b =>
    {
        b.Property<int>(""Id"");

        b.Key(""Id"");
    });

builder.Entity(""Microsoft.Data.Entity.Commands.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"");

        b.Key(""Id"");
    });

builder.Entity(""Microsoft.Data.Entity.Commands.Migrations.ModelSnapshotTest+EntityWithOneProperty"", b =>
    {
        b.Reference(""Microsoft.Data.Entity.Commands.Migrations.ModelSnapshotTest+EntityWithTwoProperties"")
            .InverseReference()
            .ForeignKey(""Microsoft.Data.Entity.Commands.Migrations.ModelSnapshotTest+EntityWithOneProperty"", ""Id"")
            .PrincipalKey(""Microsoft.Data.Entity.Commands.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", ""AlternateId"");
    });
",
                o =>
                    {
                        Assert.Equal(2, o.FindEntityType(typeof(EntityWithTwoProperties)).GetKeys().Count());
                        Assert.True(o.FindEntityType(typeof(EntityWithTwoProperties)).GetKeys().Any(k => k.Properties.Any(p => p.Name == "AlternateId")));
                    });
        }

        #endregion

        #region Property

        [Fact]
        public void Property_annotations_are_stored_in_snapshot()
        {
            Test(
                builder => { builder.Entity<EntityWithOneProperty>().Property<int>("Id").Annotation("AnnotationName", "AnnotationValue"); },
                @"
builder.Entity(""Microsoft.Data.Entity.Commands.Migrations.ModelSnapshotTest+EntityWithOneProperty"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd()
            .Annotation(""AnnotationName"", ""AnnotationValue"");

        b.Key(""Id"");
    });
",
                o => { Assert.Equal("AnnotationValue", o.EntityTypes[0].GetProperty("Id")["AnnotationName"]); }
                );
        }

        [Fact]
        public void Property_isNullable_is_stored_in_snapshot()
        {
            Test(
                builder => { builder.Entity<EntityWithStringProperty>().Property<string>("Name").Required(); },
                @"
builder.Entity(""Microsoft.Data.Entity.Commands.Migrations.ModelSnapshotTest+EntityWithStringProperty"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<string>(""Name"")
            .Required();

        b.Key(""Id"");
    });
",
                o => { Assert.Equal(false, o.EntityTypes[0].GetProperty("Name").IsNullable); });
        }

        [Fact]
        public void Property_ValueGenerated_value_is_stored_in_snapshot()
        {
            Test(
                builder => { builder.Entity<EntityWithTwoProperties>().Property<int>("AlternateId").ValueGeneratedOnAdd(); },
                @"
builder.Entity(""Microsoft.Data.Entity.Commands.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"")
            .ValueGeneratedOnAdd();

        b.Key(""Id"");
    });
",
                o => { Assert.Equal(ValueGenerated.OnAdd, o.EntityTypes[0].GetProperty("AlternateId").ValueGenerated); });
        }

        [Fact]
        public void Property_maxLength_is_stored_in_snapshot()
        {
            Test(
                builder => { builder.Entity<EntityWithStringProperty>().Property<string>("Name").MaxLength(100); },
                @"
builder.Entity(""Microsoft.Data.Entity.Commands.Migrations.ModelSnapshotTest+EntityWithStringProperty"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<string>(""Name"")
            .Annotation(""MaxLength"", 100);

        b.Key(""Id"");
    });
",
                o => { Assert.Equal(100, o.EntityTypes[0].GetProperty("Name").GetMaxLength()); });
        }

        [Fact]
        public void Property_RequiresValueGenerator_is_not_stored_in_snapshot()
        {
            Test(
                builder => { builder.Entity<EntityWithTwoProperties>().Property<int>("AlternateId").Metadata.RequiresValueGenerator = true; },
                @"
builder.Entity(""Microsoft.Data.Entity.Commands.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"");

        b.Key(""Id"");
    });
",
                o => { Assert.Equal(false, o.EntityTypes[0].GetProperty("AlternateId").RequiresValueGenerator); });
        }

        [Fact]
        public void Property_concurrencyToken_is_stored_in_snapshot()
        {
            Test(
                builder => { builder.Entity<EntityWithTwoProperties>().Property<int>("AlternateId").ConcurrencyToken(); },
                @"
builder.Entity(""Microsoft.Data.Entity.Commands.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"")
            .ConcurrencyToken();

        b.Key(""Id"");
    });
",
                o => { Assert.Equal(true, o.EntityTypes[0].GetProperty("AlternateId").IsConcurrencyToken); });
        }

        #endregion

        #region Index

        [Fact]
        public void Index_annotations_are_stored_in_snapshot()
        {
            Test(
                builder => { builder.Entity<EntityWithTwoProperties>().Index(t => t.AlternateId).Annotation("AnnotationName", "AnnotationValue"); },
                @"
builder.Entity(""Microsoft.Data.Entity.Commands.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"");

        b.Key(""Id"");

        b.Index(""AlternateId"")
            .Annotation(""AnnotationName"", ""AnnotationValue"");
    });
",
                o => { Assert.Equal("AnnotationValue", o.EntityTypes[0].GetIndexes().First()["AnnotationName"]); });
        }

        [Fact]
        public void Index_isUnique_is_stored_in_snapshot()
        {
            Test(
                builder => { builder.Entity<EntityWithTwoProperties>().Index(t => t.AlternateId).Unique(); },
                @"
builder.Entity(""Microsoft.Data.Entity.Commands.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"");

        b.Key(""Id"");

        b.Index(""AlternateId"")
            .Unique();
    });
",
                o => { Assert.Equal(true, o.EntityTypes[0].GetIndexes().First().IsUnique); });
        }

        #endregion

        #region ForeignKey

        [Fact]
        public void ForeignKey_annotations_are_stored_in_snapshot()
        {
            Test(
                builder =>
                    {
                        builder.Entity<EntityWithTwoProperties>()
                            .Reference<EntityWithOneProperty>()
                            .InverseReference()
                            .ForeignKey<EntityWithTwoProperties>(e => e.AlternateId)
                            .Annotation("AnnotationName", "AnnotationValue");
                    },
                @"
builder.Entity(""Microsoft.Data.Entity.Commands.Migrations.ModelSnapshotTest+EntityWithOneProperty"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Key(""Id"");
    });

builder.Entity(""Microsoft.Data.Entity.Commands.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<int>(""AlternateId"");

        b.Key(""Id"");
    });

builder.Entity(""Microsoft.Data.Entity.Commands.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", b =>
    {
        b.Reference(""Microsoft.Data.Entity.Commands.Migrations.ModelSnapshotTest+EntityWithOneProperty"")
            .InverseReference()
            .ForeignKey(""Microsoft.Data.Entity.Commands.Migrations.ModelSnapshotTest+EntityWithTwoProperties"", ""AlternateId"")
            .Annotation(""AnnotationName"", ""AnnotationValue"");
    });
",
                o => { Assert.Equal("AnnotationValue", o.FindEntityType(typeof(EntityWithTwoProperties)).GetForeignKeys().First()["AnnotationName"]); });
        }

        [Fact]
        public void ForeignKey_isRequired_is_stored_in_snapshot()
        {
            Test(
                builder =>
                    {
                        builder.Entity<EntityWithStringProperty>()
                            .Reference<EntityWithStringKey>()
                            .InverseReference()
                            .ForeignKey<EntityWithStringProperty>(e => e.Name)
                            .Required();
                    },
                @"
builder.Entity(""Microsoft.Data.Entity.Commands.Migrations.ModelSnapshotTest+EntityWithStringKey"", b =>
    {
        b.Property<string>(""Id"");

        b.Key(""Id"");
    });

builder.Entity(""Microsoft.Data.Entity.Commands.Migrations.ModelSnapshotTest+EntityWithStringProperty"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<string>(""Name"")
            .Required();

        b.Key(""Id"");
    });

builder.Entity(""Microsoft.Data.Entity.Commands.Migrations.ModelSnapshotTest+EntityWithStringProperty"", b =>
    {
        b.Reference(""Microsoft.Data.Entity.Commands.Migrations.ModelSnapshotTest+EntityWithStringKey"")
            .InverseReference()
            .ForeignKey(""Microsoft.Data.Entity.Commands.Migrations.ModelSnapshotTest+EntityWithStringProperty"", ""Name"");
    });
",
                o => { Assert.False(o.FindEntityType(typeof(EntityWithStringProperty)).GetProperty("Name").IsNullable); });
        }

        [Fact]
        public void ForeignKey_isUnique_is_stored_in_snapshot()
        {
            Test(
                builder =>
                    {
                        builder.Entity<EntityWithStringProperty>()
                            .Reference<EntityWithStringKey>()
                            .InverseCollection()
                            .ForeignKey(e => e.Name);
                    },
                @"
builder.Entity(""Microsoft.Data.Entity.Commands.Migrations.ModelSnapshotTest+EntityWithStringKey"", b =>
    {
        b.Property<string>(""Id"");

        b.Key(""Id"");
    });

builder.Entity(""Microsoft.Data.Entity.Commands.Migrations.ModelSnapshotTest+EntityWithStringProperty"", b =>
    {
        b.Property<int>(""Id"")
            .ValueGeneratedOnAdd();

        b.Property<string>(""Name"");

        b.Key(""Id"");
    });

builder.Entity(""Microsoft.Data.Entity.Commands.Migrations.ModelSnapshotTest+EntityWithStringProperty"", b =>
    {
        b.Reference(""Microsoft.Data.Entity.Commands.Migrations.ModelSnapshotTest+EntityWithStringKey"")
            .InverseCollection()
            .ForeignKey(""Name"");
    });
",
                o => { Assert.False(o.FindEntityType(typeof(EntityWithStringProperty)).GetForeignKeys().First().IsUnique); });
        }

        #endregion

        private void Test(Action<ModelBuilder> buildModel, string expectedCode, Action<IModel> assert)
        {
            var modelBuilder = TestHelpers.Instance.CreateConventionBuilder();
            buildModel(modelBuilder);
            var model = modelBuilder.Model;

            var generator = new CSharpModelGenerator(new CSharpHelper());

            var builder = new IndentedStringBuilder();
            generator.Generate(model, builder);
            var code = builder.ToString();

            Assert.Equal(expectedCode, code);

            var build = new BuildSource
            {
                References =
                {
#if !DNXCORE50
                    BuildReference.ByName("System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"),
                    BuildReference.ByName("System.Linq.Expressions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"),
                    BuildReference.ByName("System.Runtime, Version=4.0.10.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"),
#endif
                    BuildReference.ByName("EntityFramework.Core"),
                    BuildReference.ByName("EntityFramework.Relational")
                },
                Sources = { @"
                    using System;
                    using Microsoft.Data.Entity;
                    using Microsoft.Data.Entity.Metadata;
                    using Microsoft.Data.Entity.Metadata.Conventions;
                    using Microsoft.Data.Entity.Migrations.Infrastructure;


                    public static class ModelSnapshot
                    {
                        public static IModel Model
                        {
                            get
                            {
                                var builder = new ModelBuilder(new ConventionSet());
                                " + code + @"

                                return builder.Model;
                            }
                        }
                   }
                " }
            };

            var assembly = build.BuildInMemory();
            var factoryType = assembly.GetType("ModelSnapshot");
            var property = factoryType.GetTypeInfo().GetDeclaredProperty("Model");
            var value = (IModel)property.GetValue(null);

            Assert.NotNull(value);
            assert(value);
        }
    }
}
