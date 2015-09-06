// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Infrastructure
{
    public abstract class ModelValidatorTest
    {
        [Fact]
        public virtual void Detects_shadow_entities()
        {
            var model = new Model();
            model.AddEntityType("A");

            VerifyError(Strings.ShadowEntity("A"), model);
        }

        [Fact]
        public virtual void Detects_shadow_keys()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(A));
            var keyProperty = entityType.AddProperty("Id", typeof(int));
            entityType.AddKey(keyProperty);

            VerifyWarning(Strings.ShadowKey("{'Id'}", typeof(A).FullName, "{'Id'}"), model);
        }

        [Fact]
        public virtual void Detects_a_non_shadow_property_that_doesnt_match_a_CLR_property()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(A));
            var property = entityType.AddProperty("Id", typeof(int));
            property.IsShadowProperty = false;

            VerifyError(Strings.NoClrProperty("Id", typeof(A).FullName), model);
        }

        [Fact]
        public virtual void Detects_a_non_shadow_property_that_doesnt_match_the_CLR_property_type()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(A));
            var property = entityType.AddProperty("P0", typeof(string));
            property.IsShadowProperty = false;

            VerifyError(Strings.PropertyWrongClrType("P0", typeof(A).FullName), model);
        }

        [Fact]
        public virtual void Detects_self_referencing_properties()
        {
            var model = new Model();
            var entityA = model.AddEntityType(typeof(A));
            var keyA = CreateKey(entityA);

            CreateForeignKey(keyA, keyA);

            VerifyError(Strings.CircularDependency("'A' {'P0'} -> 'A' {'P0'}"), model);
        }

        [Fact]
        public virtual void Detects_foreign_key_cycles()
        {
            var model = new Model();
            var entityA = model.AddEntityType(typeof(A));
            var keyA = CreateKey(entityA);
            var entityB = model.AddEntityType(typeof(B));
            var keyB = CreateKey(entityB);

            CreateForeignKey(keyA, keyB);
            CreateForeignKey(keyB, keyA);

            VerifyError(Strings.CircularDependency("'A' {'P0'} -> 'B' {'P0'}, 'B' {'P0'} -> 'A' {'P0'}"), model);
        }

        [Fact]
        public virtual void Passes_on_escapable_foreign_key_cycles()
        {
            var model = new Model();
            var entityA = model.AddEntityType(typeof(A));
            var keyA1 = CreateKey(entityA);
            var keyA2 = CreateKey(entityA, startingPropertyIndex: 0, propertyCount: 2);
            var entityB = model.AddEntityType(typeof(B));
            var keyB1 = CreateKey(entityB);
            var keyB2 = CreateKey(entityB, startingPropertyIndex: 1, propertyCount: 2);

            CreateForeignKey(keyA1, keyB1);
            CreateForeignKey(keyB1, keyA1);
            CreateForeignKey(keyA2, keyB2);

            Validate(model);
        }

        [Fact]
        public virtual void Passes_on_escapable_foreign_key_cycles_not_starting_at_hub()
        {
            var model = new Model();
            var entityA = model.AddEntityType(typeof(A));
            var keyA1 = CreateKey(entityA);
            var keyA2 = CreateKey(entityA, startingPropertyIndex: 1, propertyCount: 2);
            var entityB = model.AddEntityType(typeof(B));
            var keyB1 = CreateKey(entityB);
            var keyB2 = CreateKey(entityB, startingPropertyIndex: 0, propertyCount: 2);

            CreateForeignKey(keyA1, keyB1);
            CreateForeignKey(keyB1, keyA1);
            CreateForeignKey(keyB2, keyA2);

            Validate(model);
        }

        [Fact]
        public virtual void Passes_on_foreign_key_cycle_with_one_GenerateOnAdd()
        {
            var model = new Model();
            var entityA = model.AddEntityType(typeof(A));
            var keyA = CreateKey(entityA);
            var entityB = model.AddEntityType(typeof(B));
            var keyB = CreateKey(entityB);

            CreateForeignKey(keyA, keyB);
            CreateForeignKey(keyB, keyA);

            keyA.Properties[0].RequiresValueGenerator = true;

            Validate(model);
        }

        [Fact]
        public virtual void Detects_foreign_key_cycle_with_two_GenerateOnAdd()
        {
            var model = new Model();
            var entityA = model.AddEntityType(typeof(A));
            var keyA = CreateKey(entityA);
            var entityB = model.AddEntityType(typeof(B));
            var keyB = CreateKey(entityB);

            CreateForeignKey(keyA, keyB);
            CreateForeignKey(keyB, keyA);

            keyA.Properties[0].RequiresValueGenerator = true;
            keyB.Properties[0].RequiresValueGenerator = true;

            VerifyError(Strings.ForeignKeyValueGenerationOnAdd("P0", "A", "{'P0'}"), model);
        }

        [Fact]
        public virtual void Detects_GenerateOnAdd_on_foreign_key_properties()
        {
            var model = new Model();
            var entityA = model.AddEntityType(typeof(A));
            var keyA = CreateKey(entityA);
            var entityB = model.AddEntityType(typeof(B));
            var keyB = CreateKey(entityB);

            CreateForeignKey(keyA, keyB);

            keyA.Properties[0].RequiresValueGenerator = true;

            VerifyError(Strings.ForeignKeyValueGenerationOnAdd("P0", "A", "{'P0'}"), model);
        }

        [Fact]
        public virtual void Detects_GenerateOnAdd_on_referenced_foreign_key_properties()
        {
            var model = new Model();
            var entityA = model.AddEntityType(typeof(A));
            var keyA1 = CreateKey(entityA);
            var keyA2 = CreateKey(entityA);
            var entityB = model.AddEntityType(typeof(B));
            var keyB = CreateKey(entityB);

            CreateForeignKey(keyA1, keyB);
            CreateForeignKey(keyB, keyA2);

            keyB.Properties[0].RequiresValueGenerator = true;

            VerifyError(Strings.ForeignKeyValueGenerationOnAdd("P0", "B", "{'P0'}"), model);
        }

        [Fact]
        public virtual void Detects_GenerateOnAdd_not_set_on_principal_key_properties()
        {
            var model = new Model();
            var entityA = model.AddEntityType(typeof(A));
            var keyA = CreateKey(entityA);
            var entityB = model.AddEntityType(typeof(B));
            var keyB = CreateKey(entityB);

            CreateForeignKey(keyA, keyB);

            keyB.Properties[0].RequiresValueGenerator = false;

            VerifyError(Strings.PrincipalKeyNoValueGenerationOnAdd("P0", "B"), model);
        }

        [Fact]
        public virtual void Detects_multiple_root_principal_properties()
        {
            var model = new Model();
            var entityA = model.AddEntityType(typeof(A));
            var keyA1 = CreateKey(entityA);
            var keyA2 = CreateKey(entityA, startingPropertyIndex: 0, propertyCount: 2);
            var entityB = model.AddEntityType(typeof(B));
            var keyB1 = CreateKey(entityB);
            var keyB2 = CreateKey(entityB, startingPropertyIndex: 1, propertyCount: 2);

            CreateForeignKey(keyA1, keyB1);
            CreateForeignKey(keyA2, keyB2);

            VerifyWarning(Strings.MultipleRootPrincipals("A", "{'P0'}", "B", "P0", "{'P0', 'P1'}", "B", "P1"), model);
        }

        [Fact]
        public virtual void Pases_on_double_reference_to_root_principal_property()
        {
            var model = new Model();
            var entityA = model.AddEntityType(typeof(A));
            var keyA1 = CreateKey(entityA);
            var keyA2 = CreateKey(entityA, startingPropertyIndex: 0, propertyCount: 2);
            var entityB = model.AddEntityType(typeof(B));
            var keyB1 = CreateKey(entityB);
            var keyB2 = CreateKey(entityB, startingPropertyIndex: 0, propertyCount: 2);

            CreateForeignKey(keyA1, keyB1);
            CreateForeignKey(keyA2, keyB2);

            Validate(model);
        }

        [Fact]
        public virtual void Pases_on_diamond_path_to_root_principal_property()
        {
            var model = new Model();
            var entityA = model.AddEntityType(typeof(A));
            var keyA1 = CreateKey(entityA);
            var keyA2 = CreateKey(entityA, startingPropertyIndex: 0, propertyCount: 2);
            var keyA3 = CreateKey(entityA);
            var keyA4 = CreateKey(entityA, startingPropertyIndex: 2, propertyCount: 2);
            var entityB = model.AddEntityType(typeof(B));
            var keyB1 = CreateKey(entityB);
            var keyB2 = CreateKey(entityB, startingPropertyIndex: 1, propertyCount: 2);

            CreateForeignKey(keyA1, keyB1);
            CreateForeignKey(keyA2, keyB2);

            CreateForeignKey(keyB1, keyA3);
            CreateForeignKey(keyB2, keyA4);

            Validate(model);
        }

        private Key CreateKey(EntityType entityType, int startingPropertyIndex = -1, int propertyCount = 1)
        {
            if (startingPropertyIndex == -1)
            {
                startingPropertyIndex = entityType.PropertyCount;
            }
            var keyProperties = new Property[propertyCount];
            for (var i = 0; i < propertyCount; i++)
            {
                var property = entityType.GetOrAddProperty("P" + (startingPropertyIndex + i));
                property.ClrType = typeof(int?);
                property.IsShadowProperty = false;
                keyProperties[i] = property;
                keyProperties[i].RequiresValueGenerator = true;
            }
            return entityType.AddKey(keyProperties);
        }

        private ForeignKey CreateForeignKey(Key dependentKey, Key principalKey)
        {
            var foreignKey = dependentKey.DeclaringEntityType.AddForeignKey(dependentKey.Properties, principalKey, principalKey.DeclaringEntityType);
            foreignKey.IsUnique = true;
            foreignKey.IsRequired = false;
            foreach (var property in dependentKey.Properties)
            {
                property.RequiresValueGenerator = false;
            }

            return foreignKey;
        }

        protected class A
        {
            public int? P0 { get; set; }
            public int? P1 { get; set; }
            public int? P2 { get; set; }
            public int? P3 { get; set; }
        }

        protected class B
        {
            public int? P0 { get; set; }
            public int? P1 { get; set; }
            public int? P2 { get; set; }
            public int? P3 { get; set; }
        }

        protected virtual void Validate(IModel model) => CreateModelValidator().Validate(model);

        protected abstract void VerifyWarning(string expectedMessage, IModel model);

        protected abstract void VerifyError(string expectedMessage, IModel model);

        protected abstract ModelValidator CreateModelValidator();
    }
}
