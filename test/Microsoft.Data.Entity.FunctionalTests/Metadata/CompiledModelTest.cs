// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.FunctionalTests.Metadata
{
    public class CompiledModelTest
    {
        [Fact]
        public void Entities_properties_and_annotations_can_be_obtained_from_compiled_model()
        {
            var compiledModel = new _OneTwoThreeContextModel();
            var builtModel = BuildModel();

            Assert.True(
                compiledModel.Annotations.Select(a => a.Name)
                    .SequenceEqual(builtModel.Annotations.Select(a => a.Name)));
            Assert.True(
                compiledModel.Annotations.Select(a => a.Value)
                    .SequenceEqual(builtModel.Annotations.Select(a => a.Value)));

            Assert.True(
                compiledModel.EntityTypes.Select(e => e.Name)
                    .SequenceEqual(builtModel.EntityTypes.Select(a => a.Name)));
            Assert.True(
                compiledModel.EntityTypes.Select(e => e.StorageName)
                    .SequenceEqual(builtModel.EntityTypes.Select(a => a.StorageName)));
            Assert.True(
                compiledModel.EntityTypes.Select(e => e.Type)
                    .SequenceEqual(builtModel.EntityTypes.Select(a => a.Type)));

            Assert.True(
                compiledModel.EntityTypes.First().GetKey().Properties.Select(p => p.Name)
                    .SequenceEqual(builtModel.EntityTypes.First().GetKey().Properties.Select(p => p.Name)));

            Assert.True(
                compiledModel.EntityTypes.First().Properties.Select(p => p.Name)
                    .SequenceEqual(builtModel.EntityTypes.First().Properties.Select(p => p.Name)));
            Assert.True(
                compiledModel.EntityTypes.First().Properties.Select(p => p.StorageName)
                    .SequenceEqual(builtModel.EntityTypes.First().Properties.Select(p => p.StorageName)));
            Assert.True(
                compiledModel.EntityTypes.First().Properties.Select(p => p.PropertyType)
                    .SequenceEqual(builtModel.EntityTypes.First().Properties.Select(p => p.PropertyType)));

            Assert.True(
                compiledModel.EntityTypes.SelectMany(p => p.Annotations).Select(p => p.Name)
                    .SequenceEqual(builtModel.EntityTypes.SelectMany(p => p.Annotations).Select(p => p.Name)));
            Assert.True(
                compiledModel.EntityTypes.SelectMany(p => p.Annotations).Select(p => p.Value)
                    .SequenceEqual(builtModel.EntityTypes.SelectMany(p => p.Annotations).Select(p => p.Value)));

            Assert.True(
                compiledModel.EntityTypes.First().Properties.SelectMany(p => p.Annotations).Select(p => p.Name)
                    .SequenceEqual(builtModel.EntityTypes.First().Properties.SelectMany(p => p.Annotations).Select(p => p.Name)));
            Assert.True(
                compiledModel.EntityTypes.First().Properties.SelectMany(p => p.Annotations).Select(p => p.Value)
                    .SequenceEqual(builtModel.EntityTypes.First().Properties.SelectMany(p => p.Annotations).Select(p => p.Value)));
        }

        [Fact]
        public void Property_values_can_be_read_and_set_using_compiled_metadata_without_reflection()
        {
            var configuration = new EntityConfigurationBuilder()
                .UseModel(new _OneTwoThreeContextModel())
                .BuildConfiguration();

            using (var context = new DbContext(configuration))
            {
                var entity = new KoolEntity15();
                var property = (_KoolEntity15IdProperty)context.Model.GetEntityType(entity.GetType()).TryGetProperty("Id");

                var stateEntry = context.ChangeTracker.Entry(entity).StateEntry;

                Assert.False(property.GetterCalled);
                Assert.False(property.SetterCalled);

                Assert.Equal(0, stateEntry[property]);
                Assert.True(property.GetterCalled);

                stateEntry[property] = 777;

                Assert.True(property.SetterCalled);
                Assert.Equal(777, stateEntry[property]);
            }
        }

        [Fact]
        public void Entity_can_be_materialized_using_compiled_metadata_without_reflection()
        {
            var configuration = new EntityConfigurationBuilder()
                .UseModel(new _OneTwoThreeContextModel())
                .BuildConfiguration();

            using (var context = new DbContext(configuration))
            {
                var entityType = (_KoolEntity15EntityType)context.Model.GetEntityType(typeof(KoolEntity15));

                var gu = Guid.NewGuid();
                var stateEntry = context.ChangeTracker.StateManager.GetOrMaterializeEntry(
                    entityType, new ObjectArrayValueReader(new object[] { "Foo", gu, 77 }));

                var entity = (KoolEntity15)stateEntry.Entity;

                Assert.True(entityType.CreateEntityWasUsed);
                Assert.Equal(77, entity.Id);
                Assert.Equal("Foo", entity.Foo15);
                Assert.Equal(gu, entity.Goo15);
            }
        }

        [Fact]
        public void Navigation_fixup_happens_with_compiled_metadata_when_new_entities_are_tracked()
        {
            FixupTest(new _OneTwoThreeContextModel());
        }

        [Fact]
        public void Navigation_fixup_happens_with_built_metadata_when_new_entities_are_tracked()
        {
            FixupTest(BuildModel());
        }

        private static void FixupTest(IModel model)
        {
            var configuration = new EntityConfigurationBuilder()
                .UseModel(model)
                .BuildConfiguration();

            using (var context = new DbContext(configuration))
            {
                var guid1 = Guid.NewGuid();
                var guid2 = Guid.NewGuid();
                var guid3 = Guid.NewGuid();

                context.Add(new KoolEntity1 { Id1 = 11, Id2 = guid1, KoolEntity2Id = 24 });
                context.Add(new KoolEntity1 { Id1 = 12, Id2 = guid2, KoolEntity2Id = 24 });
                context.Add(new KoolEntity1 { Id1 = 13, Id2 = guid3, KoolEntity2Id = 25 });

                context.Add(new KoolEntity2 { Id = 21, KoolEntity1Id1 = 11, KoolEntity1Id2 = guid1, KoolEntity3Id = 33 });
                context.Add(new KoolEntity2 { Id = 22, KoolEntity1Id1 = 11, KoolEntity1Id2 = guid1, KoolEntity3Id = 33 });
                context.Add(new KoolEntity2 { Id = 23, KoolEntity1Id1 = 11, KoolEntity1Id2 = guid1, KoolEntity3Id = 35 });
                context.Add(new KoolEntity2 { Id = 24, KoolEntity1Id1 = 12, KoolEntity1Id2 = guid2, KoolEntity3Id = 35 });
                context.Add(new KoolEntity2 { Id = 25, KoolEntity1Id1 = 12, KoolEntity1Id2 = guid2, KoolEntity3Id = 35 });

                context.Add(new KoolEntity3 { Id = 31 });
                context.Add(new KoolEntity3 { Id = 32 });
                context.Add(new KoolEntity3 { Id = 33 });
                context.Add(new KoolEntity3 { Id = 34 });
                context.Add(new KoolEntity3 { Id = 35 });

                Assert.Equal(3, context.ChangeTracker.Entries<KoolEntity1>().Count());
                Assert.Equal(5, context.ChangeTracker.Entries<KoolEntity2>().Count());
                Assert.Equal(5, context.ChangeTracker.Entries<KoolEntity3>().Count());

                foreach (var entry in context.ChangeTracker.Entries<KoolEntity1>())
                {
                    var entity = entry.Entity;

                    Assert.Equal(entity.KoolEntity2Id, entity.NavTo2.Id);
                    Assert.Contains(entity, entity.NavTo2.NavTo1s);
                }

                foreach (var entry in context.ChangeTracker.Entries<KoolEntity2>())
                {
                    var entity = entry.Entity;

                    Assert.Equal(entity.KoolEntity1Id1, entity.NavTo1.Id1);
                    Assert.Equal(entity.KoolEntity1Id2, entity.NavTo1.Id2);
                    Assert.Contains(entity, entity.NavTo1.NavTo2s);
                }
            }
        }

        [Fact]
        public void Navigation_fixup_happens_with_compiled_metadata_using_non_standard_collection_access()
        {
            var configuration = new EntityConfigurationBuilder()
                .UseModel(new _OneTwoThreeContextModel())
                .BuildConfiguration();

            using (var context = new DbContext(configuration))
            {
                context.Add(new KoolEntity6 { Id = 11, Kool5Id = 24 });
                context.Add(new KoolEntity5 { Id = 21 });
                context.Add(new KoolEntity6 { Id = 12, Kool5Id = 24 });
                context.Add(new KoolEntity5 { Id = 22 });
                context.Add(new KoolEntity5 { Id = 23 });
                context.Add(new KoolEntity6 { Id = 13, Kool5Id = 25 });
                context.Add(new KoolEntity5 { Id = 24 });
                context.Add(new KoolEntity5 { Id = 25 });

                Assert.Equal(3, context.ChangeTracker.Entries<KoolEntity6>().Count());
                Assert.Equal(5, context.ChangeTracker.Entries<KoolEntity5>().Count());

                foreach (var entry in context.ChangeTracker.Entries<KoolEntity6>())
                {
                    var entity = entry.Entity;

                    Assert.Equal(entity.Kool5Id, entity.Kool5.Id);
                    Assert.Contains(entity, entity.Kool5.Kool6s);
                }
            }
        }

        [Fact]
        public void Compiled_model_uses_heap_memory_on_pay_per_play_basis_and_overall_uses_less()
        {
            var compiledMemory = RecordModelHeapUse(() => new _OneTwoThreeContextModel());
            var builtMemory = RecordModelHeapUse(BuildModel);

            var compiledBaseMemory = compiledMemory[0].Item2;
            var builtBaseMemory = builtMemory[0].Item2;
            var deltas = new List<Tuple<long, long>>();
            for (var i = 0; i < compiledMemory.Count; i++)
            {
                deltas.Add(
                    Tuple.Create(
                        compiledMemory[i].Item2 - compiledBaseMemory,
                        builtMemory[i].Item2 - builtBaseMemory));
            }

            // Numbers are not 100% consistent due to other threads running and GC.GetTotalMemory not 
            // necessarily returning an accurate number. At the time of check in the numbers are:
            //
            // Compiled: 2176 (50)  Built: 1912408 (50) Ratio: 0.00113783251272741 (Just model)
            // Compiled: 13344 (100)  Built: 1912688 (100) Ratio: 0.00697656910065834 (Model annotations)
            // Compiled: 95744 (1000)  Built: 1921104 (1000) Ratio: 0.0498380098110253 (All entity types)
            // Compiled: 102912 (150)  Built: 1921352 (150) Ratio: 0.053562283225562 (Properties one entity)
            // Compiled: 117680 (250)  Built: 1921728 (250) Ratio: 0.0612365537682752 (All FKs)
            // Compiled: 143552 (500)  Built: 1922072 (500) Ratio: 0.0746860679516688 (All navigations)
            // Compiled: 308664 (1000)  Built: 1922280 (1000) Ratio: 0.160571820962607 (All keys)
            // Compiled: 341640 (3350)  Built: 1955160 (3350) Ratio: 0.174737617381698 (All properties)
            // Compiled: 550152 (2000)  Built: 1955472 (2000) Ratio: 0.281339748152876 (All entity type annotations)
            // Compiled: 762984 (2000)  Built: 1955736 (2000) Ratio: 0.390126274711924 (All property annotations)
            //
            // Uncomment to get new numbers:
            //using (var writer = File.CreateText(@"C:\Stuff\MemNumbers.txt"))
            //{
            //    for (var i = 1; i < compiledMemory.Count; i++)
            //    {
            //        writer.WriteLine(
            //            "Compiled: {0} ({1})  Built: {2} ({3}) Ratio: {4} ({5})",
            //            deltas[i].Item1,
            //            compiledMemory[i].Item1,
            //            deltas[i].Item2,
            //            builtMemory[i].Item1,
            //            (double)deltas[i].Item1 / deltas[i].Item2,
            //            compiledMemory[i].Item3);
            //    }
            //}

            // Check that both models have the same number of entities, properties, etc.
            for (var i = 0; i < compiledMemory.Count; i++)
            {
                Assert.Equal(compiledMemory[i].Item1, builtMemory[i].Item1);
            }

            // Acceptable ranges for memory ratios
            var expected = new[]
                {
                    Tuple.Create(0.0, 0.0), // Starting memory; not used
                    Tuple.Create(-0.01, 0.02), // Just model
                    Tuple.Create(-0.01, 0.02), // Model annotations
                    Tuple.Create(-0.01, 0.1), // All entity types
                    Tuple.Create(-0.01, 0.1), // Properties one entity
                    Tuple.Create(0.01, 0.1), // All FKs
                    Tuple.Create(0.01, 0.1), // All navigations
                    Tuple.Create(0.1, 0.3), // All keys
                    Tuple.Create(0.1, 0.3), // All properties
                    Tuple.Create(0.1, 0.4), // All entity type annotations
                    Tuple.Create(0.2, 0.45) // All property annotations
                };

            for (var i = 1; i < expected.Length; i++)
            {
                var ratio = (double)deltas[i].Item1 / deltas[i].Item2;
                Assert.True(expected[i].Item1 <= ratio, "Failed: " + expected[i].Item1 + " <= " + ratio);
                Assert.True(expected[i].Item2 >= ratio, "Failed: " + expected[i].Item2 + " >= " + ratio);
            }
        }

        private static List<Tuple<int, long, string>> RecordModelHeapUse(Func<IModel> modelFactory)
        {
            var memory = new List<Tuple<int, long, string>>();
            var models = new List<IModel>();

            memory.Add(Tuple.Create(0, GetMemory(), "Base"));

            for (var i = 0; i < 50; i++)
            {
                models.Add(modelFactory());
            }
            memory.Add(Tuple.Create(models.Count, GetMemory(), "Just model"));

            var annotations = models.SelectMany(m => m.Annotations);
            memory.Add(Tuple.Create(annotations.Count(), GetMemory(), "Model annotations"));

            var entities = models.SelectMany(m => m.EntityTypes).ToList();
            memory.Add(Tuple.Create(entities.Count(), GetMemory(), "All entity types"));

            var propertiesOneEntity = entities.Where(e => e.Type == typeof(KoolEntity9)).SelectMany(e => e.Properties);
            memory.Add(Tuple.Create(propertiesOneEntity.Count(), GetMemory(), "Properties one entity"));

            var fks = entities.SelectMany(e => e.ForeignKeys);
            memory.Add(Tuple.Create(fks.Count(), GetMemory(), "All FKs"));

            var navigations = entities.SelectMany(e => e.Navigations);
            memory.Add(Tuple.Create(navigations.Count(), GetMemory(), "All navigations"));

            var keys = entities.SelectMany(e => e.GetKey().Properties);
            memory.Add(Tuple.Create(keys.Count(), GetMemory(), "All keys"));

            var properties = entities.SelectMany(e => e.Properties).ToList();
            memory.Add(Tuple.Create(properties.Count(), GetMemory(), "All properties"));

            var entityAnnotations = entities.SelectMany(e => e.Annotations);
            memory.Add(Tuple.Create(entityAnnotations.Count(), GetMemory(), "All entity type annotations"));

            var propertyAnnotations = properties.SelectMany(e => e.Annotations);
            memory.Add(Tuple.Create(propertyAnnotations.Count(), GetMemory(), "All property annotations"));

            // Do something with created objects otherwise in Release build the garbage collection
            // happens before memory numbers are collected.
            Assert.NotNull(annotations);
            Assert.NotNull(entities);
            Assert.NotNull(propertiesOneEntity);
            Assert.NotNull(fks);
            Assert.NotNull(navigations);
            Assert.NotNull(keys);
            Assert.NotNull(properties);
            Assert.NotNull(entityAnnotations);
            Assert.NotNull(propertyAnnotations);

            return memory;
        }

        private static IModel BuildModel()
        {
            var model = new Model();
            var builder = new ModelBuilder(model);

            builder.Annotation("ModelAnnotation1", "ModelValue1");
            builder.Annotation("ModelAnnotation2", "ModelValue2");

            var entityType1 = new EntityType(typeof(KoolEntity1));
            var property = entityType1.AddProperty("Id1", typeof(int));
            property.StorageName = "MyKey1";
            entityType1.SetKey(property);
            entityType1.AddProperty("Id2", typeof(Guid)).StorageName = "MyKey2";
            entityType1.AddProperty("KoolEntity2Id", typeof(int));
            model.AddEntityType(entityType1);

            var entityType2 = new EntityType(typeof(KoolEntity2));
            entityType2.AddProperty("KoolEntity1Id1", typeof(int));
            entityType2.AddProperty("KoolEntity1Id2", typeof(Guid));
            entityType2.AddProperty("KoolEntity3Id", typeof(int));
            model.AddEntityType(entityType2);

            var entityType3 = new EntityType(typeof(KoolEntity3));
            entityType3.AddProperty("KoolEntity4Id", typeof(int));
            model.AddEntityType(entityType3);

            var entityType4 = new EntityType(typeof(KoolEntity4));
            model.AddEntityType(entityType4);

            var entityType5 = new EntityType(typeof(KoolEntity5));
            model.AddEntityType(entityType5);

            var entityType6 = new EntityType(typeof(KoolEntity6));
            entityType6.AddProperty("Kool5Id", typeof(int));
            model.AddEntityType(entityType6);

            for (var i = 7; i <= 20; i++)
            {
                var type = Type.GetType("Microsoft.Data.Entity.FunctionalTests.Metadata.KoolEntity" + i);

                Assert.NotNull(type);

                model.AddEntityType(new EntityType(type));
            }

            for (var i = 2; i <= 20; i++)
            {
                var type = Type.GetType("Microsoft.Data.Entity.FunctionalTests.Metadata.KoolEntity" + i);

                var entityType = model.GetEntityType(type);
                var id = entityType.AddProperty(entityType.Type.GetProperty("Id"));
                id.StorageName = "MyKey";
                entityType.SetKey(id);
            }

            for (var i = 1; i <= 20; i++)
            {
                var type = Type.GetType("Microsoft.Data.Entity.FunctionalTests.Metadata.KoolEntity" + i);

                var entityType = model.GetEntityType(type);
                entityType.StorageName = entityType.Name + "Table";

                entityType.Annotations.Add(new Annotation("Annotation1", "Value1"));
                entityType.Annotations.Add(new Annotation("Annotation2", "Value2"));

                var foo = entityType.AddProperty(entityType.Type.GetProperty("Foo" + i));

                foo.Annotations.Add(new Annotation("Foo" + i + "Annotation1", "Foo" + i + "Value1"));
                foo.Annotations.Add(new Annotation("Foo" + i + "Annotation2", "Foo" + i + "Value2"));

                var goo = entityType.AddProperty(entityType.Type.GetProperty("Goo" + i));
            }

            var fk11 = entityType1.AddForeignKey(entityType2.GetKey(), new[] { entityType1.GetProperty("KoolEntity2Id") });
            var fk21 = entityType2.AddForeignKey(entityType1.GetKey(), new[] { entityType2.GetProperty("KoolEntity1Id1") });
            var fk22 = entityType2.AddForeignKey(entityType3.GetKey(), new[] { entityType2.GetProperty("KoolEntity3Id") });
            var fk31 = entityType3.AddForeignKey(entityType4.GetKey(), new[] { entityType3.GetProperty("KoolEntity4Id") });
            var fk61 = entityType6.AddForeignKey(entityType5.GetKey(), new[] { entityType6.GetProperty("Kool5Id") });

            entityType1.AddNavigation(new Navigation(fk11, "NavTo2"));
            entityType1.AddNavigation(new Navigation(fk21, "NavTo2s"));
            entityType2.AddNavigation(new Navigation(fk21, "NavTo1"));
            entityType2.AddNavigation(new Navigation(fk11, "NavTo1s"));
            entityType2.AddNavigation(new Navigation(fk22, "NavTo3"));
            entityType3.AddNavigation(new Navigation(fk22, "NavTo2s"));
            entityType3.AddNavigation(new Navigation(fk31, "NavTo4"));
            entityType4.AddNavigation(new Navigation(fk31, "NavTo3s"));
            entityType5.AddNavigation(new Navigation(fk61, "Kool6s"));
            entityType6.AddNavigation(new Navigation(fk61, "Kool5"));

            return model;
        }

        private static long GetMemory()
        {
            for (var i = 0; i < 5; i++)
            {
                GC.GetTotalMemory(forceFullCollection: true);
            }
            return GC.GetTotalMemory(forceFullCollection: true);
        }
    }
}
