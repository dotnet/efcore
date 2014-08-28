// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Xunit;

#if ASPNETCORE50
using System.Reflection;
#endif

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
                compiledModel.EntityTypes.Select(e => e.Type)
                    .SequenceEqual(builtModel.EntityTypes.Select(a => a.Type)));

            Assert.True(
                compiledModel.EntityTypes.First().GetPrimaryKey().Properties.Select(p => p.Name)
                    .SequenceEqual(builtModel.EntityTypes.First().GetPrimaryKey().Properties.Select(p => p.Name)));

            Assert.True(
                compiledModel.EntityTypes.First().Properties.Select(p => p.Name)
                    .SequenceEqual(builtModel.EntityTypes.First().Properties.Select(p => p.Name)));
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

            Assert.Equal(compiledModel.EntityTypes.Select(e => compiledModel.GetReferencingForeignKeys(e).Select(fk => fk.EntityType.Name)),
                builtModel.EntityTypes.Select(e => builtModel.GetReferencingForeignKeys(e).Select(fk => fk.EntityType.Name)));
        }

        [Fact]
        public void Entities_indexes_can_be_obtained_from_compiled_model()
        {
            var compiledModel = new _OneTwoThreeContextModel();

            var indexes = compiledModel.EntityTypes.First().Indexes;

            Assert.Equal(2, indexes.Count);
            Assert.Equal(new[] { "Goo1" }, indexes[0].Properties.Select(p => p.Name));
            Assert.False(indexes[0].IsUnique);
            Assert.Equal(new[] { "Foo1", "Goo1" }, indexes[1].Properties.Select(p => p.Name));
            Assert.True(indexes[1].IsUnique);
        }

        [Fact]
        public void Property_values_can_be_read_and_set_using_compiled_metadata_without_reflection()
        {
            var options = new DbContextOptions().UseModel(new _OneTwoThreeContextModel());

            using (var context = new DbContext(options))
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
            var options = new DbContextOptions().UseModel(new _OneTwoThreeContextModel());

            using (var context = new DbContext(options))
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
            var options = new DbContextOptions()
                .UseModel(model)
                .UseInMemoryStore(persist: false);

            using (var context = new DbContext(options))
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
            var options = new DbContextOptions()
                .UseModel(new _OneTwoThreeContextModel())
                .UseInMemoryStore();

            using (var context = new DbContext(options))
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

        // [Fact] Disabled for now--needs some work to be reliable
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

            var keys = entities.SelectMany(e => e.GetPrimaryKey().Properties);
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
            var builder = new BasicModelBuilder(model);

            builder.Annotation("ModelAnnotation1", "ModelValue1");
            builder.Annotation("ModelAnnotation2", "ModelValue2");

            var entityType1 = new EntityType(typeof(KoolEntity1));
            var property = entityType1.GetOrAddProperty("Id1", typeof(int));
            entityType1.GetOrSetPrimaryKey(property);
            entityType1.GetOrAddProperty("Id2", typeof(Guid));
            entityType1.GetOrAddProperty("KoolEntity2Id", typeof(int));
            model.AddEntityType(entityType1);

            var entityType2 = new EntityType(typeof(KoolEntity2));
            entityType2.GetOrAddProperty("KoolEntity1Id1", typeof(int));
            entityType2.GetOrAddProperty("KoolEntity1Id2", typeof(Guid));
            entityType2.GetOrAddProperty("KoolEntity3Id", typeof(int));
            model.AddEntityType(entityType2);

            var entityType3 = new EntityType(typeof(KoolEntity3));
            entityType3.GetOrAddProperty("KoolEntity4Id", typeof(int));
            model.AddEntityType(entityType3);

            var entityType4 = new EntityType(typeof(KoolEntity4));
            model.AddEntityType(entityType4);

            var entityType5 = new EntityType(typeof(KoolEntity5));
            model.AddEntityType(entityType5);

            var entityType6 = new EntityType(typeof(KoolEntity6));
            entityType6.GetOrAddProperty("Kool5Id", typeof(int));
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
                var id = entityType.GetOrAddProperty(entityType.Type.GetProperty("Id"));
                entityType.GetOrSetPrimaryKey(id);
            }

            for (var i = 1; i <= 20; i++)
            {
                var type = Type.GetType("Microsoft.Data.Entity.FunctionalTests.Metadata.KoolEntity" + i);

                var entityType = model.GetEntityType(type);

                entityType.Annotations.Add(new Annotation("Annotation1", "Value1"));
                entityType.Annotations.Add(new Annotation("Annotation2", "Value2"));

                var foo = entityType.GetOrAddProperty(entityType.Type.GetProperty("Foo" + i));

                foo.Annotations.Add(new Annotation("Foo" + i + "Annotation1", "Foo" + i + "Value1"));
                foo.Annotations.Add(new Annotation("Foo" + i + "Annotation2", "Foo" + i + "Value2"));

                var goo = entityType.GetOrAddProperty(entityType.Type.GetProperty("Goo" + i));
            }

            var fk11 = entityType1.GetOrAddForeignKey(entityType2.GetPrimaryKey(), new[] { entityType1.GetProperty("KoolEntity2Id") });
            var fk21 = entityType2.GetOrAddForeignKey(entityType1.GetPrimaryKey(), new[] { entityType2.GetProperty("KoolEntity1Id1") });
            var fk22 = entityType2.GetOrAddForeignKey(entityType3.GetPrimaryKey(), new[] { entityType2.GetProperty("KoolEntity3Id") });
            var fk31 = entityType3.GetOrAddForeignKey(entityType4.GetPrimaryKey(), new[] { entityType3.GetProperty("KoolEntity4Id") });
            var fk61 = entityType6.GetOrAddForeignKey(entityType5.GetPrimaryKey(), new[] { entityType6.GetProperty("Kool5Id") });

            entityType1.AddNavigation(new Navigation(fk11, "NavTo2", pointsToPrincipal: true));
            entityType1.AddNavigation(new Navigation(fk21, "NavTo2s", pointsToPrincipal: false));
            entityType2.AddNavigation(new Navigation(fk21, "NavTo1", pointsToPrincipal: true));
            entityType2.AddNavigation(new Navigation(fk11, "NavTo1s", pointsToPrincipal: false));
            entityType2.AddNavigation(new Navigation(fk22, "NavTo3", pointsToPrincipal: true));
            entityType3.AddNavigation(new Navigation(fk22, "NavTo2s", pointsToPrincipal: false));
            entityType3.AddNavigation(new Navigation(fk31, "NavTo4", pointsToPrincipal: true));
            entityType4.AddNavigation(new Navigation(fk31, "NavTo3s", pointsToPrincipal: false));
            entityType5.AddNavigation(new Navigation(fk61, "Kool6s", pointsToPrincipal: false));
            entityType6.AddNavigation(new Navigation(fk61, "Kool5", pointsToPrincipal: true));

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
