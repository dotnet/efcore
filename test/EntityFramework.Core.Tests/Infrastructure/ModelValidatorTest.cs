// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
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

            VerifyError(CoreStrings.ShadowEntity("A"), model);
        }

        [Fact]
        public virtual void Detects_shadow_keys()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityType);
            var keyProperty = entityType.AddProperty("Key", typeof(int));
            entityType.AddKey(keyProperty);

            VerifyWarning(CoreStrings.ShadowKey("{'Key'}", typeof(A).FullName, "{'Key'}"), model);
        }

        [Fact]
        public virtual void Detects_a_null_primary_key()
        {
            var model = new Model();
            model.AddEntityType(typeof(A));

            VerifyError(CoreStrings.EntityRequiresKey(typeof(A).FullName), model);
        }

        [Fact]
        public virtual void Passes_on_escapable_foreign_key_cycles()
        {
            var model = new Model();
            var entityA = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityA);
            var keyA1 = CreateKey(entityA);
            var keyA2 = CreateKey(entityA, startingPropertyIndex: 0, propertyCount: 2);
            var entityB = model.AddEntityType(typeof(B));
            SetPrimaryKey(entityB);
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
            SetPrimaryKey(entityA);
            var keyA1 = CreateKey(entityA);
            var keyA2 = CreateKey(entityA, startingPropertyIndex: 1, propertyCount: 2);
            var entityB = model.AddEntityType(typeof(B));
            SetPrimaryKey(entityB);
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
            SetPrimaryKey(entityA);
            var keyA = CreateKey(entityA);
            var entityB = model.AddEntityType(typeof(B));
            SetPrimaryKey(entityB);
            var keyB = CreateKey(entityB);

            CreateForeignKey(keyA, keyB);
            CreateForeignKey(keyB, keyA);

            keyA.Properties[0].RequiresValueGenerator = true;

            Validate(model);
        }

        [Fact]
        public virtual void Pases_on_double_reference_to_root_principal_property()
        {
            var model = new Model();
            var entityA = model.AddEntityType(typeof(A));
            SetPrimaryKey(entityA);
            var keyA1 = CreateKey(entityA);
            var keyA2 = CreateKey(entityA, startingPropertyIndex: 0, propertyCount: 2);
            var entityB = model.AddEntityType(typeof(B));
            SetPrimaryKey(entityB);
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
            SetPrimaryKey(entityA);
            var keyA1 = CreateKey(entityA);
            var keyA2 = CreateKey(entityA, startingPropertyIndex: 0, propertyCount: 2);
            var keyA3 = CreateKey(entityA);
            var keyA4 = CreateKey(entityA, startingPropertyIndex: 2, propertyCount: 2);
            var entityB = model.AddEntityType(typeof(B));
            SetPrimaryKey(entityB);
            var keyB1 = CreateKey(entityB);
            var keyB2 = CreateKey(entityB, startingPropertyIndex: 1, propertyCount: 2);

            CreateForeignKey(keyA1, keyB1);
            CreateForeignKey(keyA2, keyB2);

            CreateForeignKey(keyB1, keyA3);
            CreateForeignKey(keyB2, keyA4);

            Validate(model);
        }

        protected Key CreateKey(EntityType entityType, int startingPropertyIndex = -1, int propertyCount = 1)
        {
            if (startingPropertyIndex == -1)
            {
                startingPropertyIndex = entityType.PropertyCount - 1;
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

        public void SetPrimaryKey(EntityType entityType)
        {
            var property = entityType.AddProperty("Id", typeof(int));
            property.IsShadowProperty = false;
            entityType.SetPrimaryKey(property);
        }

        protected ForeignKey CreateForeignKey(Key dependentKey, Key principalKey)
            => CreateForeignKey(dependentKey.DeclaringEntityType, dependentKey.Properties, principalKey);

        protected ForeignKey CreateForeignKey(EntityType dependEntityType, IReadOnlyList<Property> dependentProperties, Key principalKey)
        {
            var foreignKey = dependEntityType.AddForeignKey(dependentProperties, principalKey, principalKey.DeclaringEntityType);
            foreignKey.IsUnique = true;
            foreignKey.IsRequired = false;
            foreach (var property in dependentProperties)
            {
                property.RequiresValueGenerator = false;
            }

            return foreignKey;
        }

        protected class A
        {
            public int Id { get; set; }

            public int? P0 { get; set; }
            public int? P1 { get; set; }
            public int? P2 { get; set; }
            public int? P3 { get; set; }
        }

        protected class B : A
        {
        }

        protected virtual void Validate(IModel model) => CreateModelValidator().Validate(model);

        protected abstract void VerifyWarning(string expectedMessage, IModel model);

        protected abstract void VerifyError(string expectedMessage, IModel model);

        protected abstract ModelValidator CreateModelValidator();
    }
}
