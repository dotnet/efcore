// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.ChangeTracking;
using Xunit;

namespace Microsoft.Data.Entity.Metadata
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
                compiledModel.Entities.Select(e => e.Name)
                    .SequenceEqual(builtModel.Entities.Select(a => a.Name)));
            Assert.True(
                compiledModel.Entities.Select(e => e.StorageName)
                    .SequenceEqual(builtModel.Entities.Select(a => a.StorageName)));
            Assert.True(
                compiledModel.Entities.Select(e => e.Type)
                    .SequenceEqual(builtModel.Entities.Select(a => a.Type)));

            Assert.True(
                compiledModel.Entities.First().Key.Select(p => p.Name)
                    .SequenceEqual(builtModel.Entities.First().Key.Select(p => p.Name)));

            Assert.True(
                compiledModel.Entities.First().Properties.Select(p => p.Name)
                    .SequenceEqual(builtModel.Entities.First().Properties.Select(p => p.Name)));
            Assert.True(
                compiledModel.Entities.First().Properties.Select(p => p.StorageName)
                    .SequenceEqual(builtModel.Entities.First().Properties.Select(p => p.StorageName)));
            Assert.True(
                compiledModel.Entities.First().Properties.Select(p => p.PropertyType)
                    .SequenceEqual(builtModel.Entities.First().Properties.Select(p => p.PropertyType)));
            Assert.True(
                compiledModel.Entities.First().Properties.Select(p => p.DeclaringType)
                    .SequenceEqual(builtModel.Entities.First().Properties.Select(p => p.DeclaringType)));

            Assert.True(
                compiledModel.Entities.SelectMany(p => p.Annotations).Select(p => p.Name)
                    .SequenceEqual(builtModel.Entities.SelectMany(p => p.Annotations).Select(p => p.Name)));
            Assert.True(
                compiledModel.Entities.SelectMany(p => p.Annotations).Select(p => p.Value)
                    .SequenceEqual(builtModel.Entities.SelectMany(p => p.Annotations).Select(p => p.Value)));

            Assert.True(
                compiledModel.Entities.First().Properties.SelectMany(p => p.Annotations).Select(p => p.Name)
                    .SequenceEqual(builtModel.Entities.First().Properties.SelectMany(p => p.Annotations).Select(p => p.Name)));
            Assert.True(
                compiledModel.Entities.First().Properties.SelectMany(p => p.Annotations).Select(p => p.Value)
                    .SequenceEqual(builtModel.Entities.First().Properties.SelectMany(p => p.Annotations).Select(p => p.Value)));
        }

        [Fact]
        public void Property_values_can_be_read_and_set_using_compiled_metadata_without_reflection()
        {
            var entity = new KoolEntity15();
            var property = new _OneTwoThreeContextModel().Entity(entity).Property("Id");

            Assert.Equal(0, property.GetValue(entity));
            property.SetValue(entity, 777);
            Assert.Equal(777, property.GetValue(entity));
        }

        [Fact]
        public void EntityKeys_can_be_obtained_from_compiled_metadata_without_reflection()
        {
            var entity = new KoolEntity27 { Id = 77 };
            var key = new _OneTwoThreeContextModel().Entity(entity).CreateKey(entity);

            Assert.IsType<SimpleEntityKey<KoolEntity27, int>>(key);
            Assert.Equal(77, key.Value);
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
            // necessarily returning an accurate number. At the time of checkin the numbers are:
            //
            //  Compiled: 2168 (50)  Built: 4546256 (50) Ratio: 0.000476875917238273
            //  Compiled: 10824 (100)  Built: 4547424 (100) Ratio: 0.0023802486858494
            //  Compiled: 114112 (2500)  Built: 4549128 (2500) Ratio: 0.0250843678173048
            //  Compiled: 127480 (150)  Built: 4550144 (150) Ratio: 0.0280166957353438
            //  Compiled: 617904 (2500)  Built: 4552040 (2500) Ratio: 0.135742216676479
            //  Compiled: 618296 (7500)  Built: 4553928 (7500) Ratio: 0.135772019232627
            //  Compiled: 959032 (5000)  Built: 4555808 (5000) Ratio: 0.210507554313088
            //  Compiled: 1640336 (10000)  Built: 4558264 (10000) Ratio: 0.359859806277127
            //
            // Uncomment to get new numbers:
            //for (var i = 1; i < compiledMemory.Count; i++)
            //{
            //    Console.WriteLine(
            //        "Compiled: {0} ({1})  Built: {2} ({3}) Ratio: {4}",
            //        deltas[i].Item1,
            //        compiledMemory[i].Item1,
            //        deltas[i].Item2,
            //        builtMemory[i].Item1,
            //        (double)deltas[i].Item1 / deltas[i].Item2);
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
                Tuple.Create(-0.01, 0.01), // Just models
                Tuple.Create(-0.01, 0.01), // Model annotations
                Tuple.Create(0.01, 0.06), // All entities
                Tuple.Create(0.01, 0.06), // Properties from one entity
                Tuple.Create(0.1, 0.3), // All keys
                Tuple.Create(0.1, 0.3), // All properties
                Tuple.Create(0.1, 0.3), // All entity annotations
                Tuple.Create(0.2, 0.4) // All property annotations
            };

            for (var i = 1; i < expected.Length; i++)
            {
                var ratio = (double)deltas[i].Item1 / deltas[i].Item2;
                Assert.True(expected[i].Item1 <= ratio, "Failed: " + expected[i].Item1 + " <= " + ratio);
                Assert.True(expected[i].Item2 >= ratio, "Failed: " + expected[i].Item2 + " >= " + ratio);
            }
        }

        private static List<Tuple<int, long>> RecordModelHeapUse(Func<IModel> modelFactory)
        {
            var memory = new List<Tuple<int, long>>();
            var models = new List<IModel>();

            memory.Add(Tuple.Create(0, GetMemory()));

            for (var i = 0; i < 50; i++)
            {
                models.Add(modelFactory());
            }
            memory.Add(Tuple.Create(models.Count, GetMemory()));

            var annotations = models.SelectMany(m => m.Annotations);
            memory.Add(Tuple.Create(annotations.Count(), GetMemory()));

            var entities = models.SelectMany(m => m.Entities);
            memory.Add(Tuple.Create(entities.Count(), GetMemory()));

            var propertiesOneEntity = entities.Where(e => e.Type == typeof(KoolEntity9)).SelectMany(e => e.Properties);
            memory.Add(Tuple.Create(propertiesOneEntity.Count(), GetMemory()));

            var keys = entities.SelectMany(e => e.Key);
            memory.Add(Tuple.Create(keys.Count(), GetMemory()));

            var properties = entities.SelectMany(e => e.Properties);
            memory.Add(Tuple.Create(properties.Count(), GetMemory()));

            var entityAnnotations = entities.SelectMany(e => e.Annotations);
            memory.Add(Tuple.Create(entityAnnotations.Count(), GetMemory()));

            var propertyAnnotations = properties.SelectMany(e => e.Annotations);
            memory.Add(Tuple.Create(propertyAnnotations.Count(), GetMemory()));

            return memory;
        }

        private static IModel BuildModel()
        {
            var model = new Model();
            var builder = new ModelBuilder(model);

            builder.Annotation("ModelAnnotation1", "ModelValue1");
            builder.Annotation("ModelAnnotation2", "ModelValue2");

            for (var i = 1; i <= 50; i++)
            {
                var entityType = new EntityType(Type.GetType("Microsoft.Data.Entity.Metadata.KoolEntity" + i));
                entityType.StorageName = entityType.Name + "Table";

                entityType.AddAnnotation(new Annotation("Annotation1", "Value1"));
                entityType.AddAnnotation(new Annotation("Annotation2", "Value2"));

                var id = new Property(entityType.Type.GetProperty("Id"));
                id.StorageName = "MyKey";

                id.AddAnnotation(new Annotation("IdAnnotation1", "IdValue1"));
                id.AddAnnotation(new Annotation("IdAnnotation2", "IdValue2"));

                entityType.Key = new[] { id };
                entityType.AddProperty(id);

                var foo = new Property(entityType.Type.GetProperty("Foo"));

                foo.AddAnnotation(new Annotation("FooAnnotation1", "FooValue1"));
                foo.AddAnnotation(new Annotation("FooAnnotation2", "FooValue2"));

                entityType.AddProperty(foo);

                var goo = new Property(entityType.Type.GetProperty("Goo"));

                entityType.AddProperty(goo);

                model.AddEntity(entityType);
            }

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
