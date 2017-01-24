// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Tests.Metadata.Conventions.Internal
{
    public class ConventionDispatcherTest
    {
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [Theory]
        public void OnEntityTypeAdded_calls_apply_on_conventions_in_order(bool useBuilder, bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new EntityTypeConvention(terminate: false, delegatedIdentity: false);
            var convention2 = new EntityTypeConvention(terminate: true, delegatedIdentity: false);
            var convention3 = new EntityTypeConvention(terminate: false, delegatedIdentity: false);
            conventions.EntityTypeAddedConventions.Add(convention1);
            conventions.EntityTypeAddedConventions.Add(convention2);
            conventions.EntityTypeAddedConventions.Add(convention3);

            var builder = new InternalModelBuilder(new Model(conventions));

            var scope = useScope ? builder.Metadata.ConventionDispatcher.StartBatch() : null;

            if (useBuilder)
            {
                var result = builder.Entity(typeof(Order), ConfigurationSource.Convention);

                Assert.Equal(!useScope, result == null);
            }
            else
            {
                var result = builder.Metadata.AddEntityType(typeof(Order), ConfigurationSource.Convention);

                Assert.Equal(!useScope, result == null);
            }

            if (useScope)
            {
                Assert.Equal(0, convention1.Calls);
                Assert.Equal(0, convention2.Calls);
                scope.Dispose();
            }

            Assert.Equal(1, convention1.Calls);
            Assert.Equal(1, convention2.Calls);
            Assert.Equal(0, convention3.Calls);

            Assert.Empty(builder.Metadata.GetEntityTypes());
            Assert.Null(builder.Metadata.FindEntityType(typeof(Order)));
        }

        [InlineData(false, false)]
        [InlineData(false, true)]
        [Theory]
        public void OnEntityTypeAdded_calls_apply_on_conventions_in_order_for_delegated_identity(bool useBuilder, bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new EntityTypeConvention(terminate: false, delegatedIdentity: true);
            var convention2 = new EntityTypeConvention(terminate: true, delegatedIdentity: true);
            var convention3 = new EntityTypeConvention(terminate: false, delegatedIdentity: true);
            conventions.EntityTypeAddedConventions.Add(convention1);
            conventions.EntityTypeAddedConventions.Add(convention2);
            conventions.EntityTypeAddedConventions.Add(convention3);

            var builder = new InternalModelBuilder(new Model(conventions));
            var orderDetails = builder.Entity(typeof(OrderDetails), ConfigurationSource.Explicit);

            var scope = useScope ? builder.Metadata.ConventionDispatcher.StartBatch() : null;

            if (!useBuilder)
            {
                var result = builder.Metadata.AddDelegatedIdentityEntityType(
                    typeof(Order), nameof(OrderDetails.Order), orderDetails.Metadata, ConfigurationSource.Convention);

                Assert.Equal(!useScope, result == null);
            }

            if (useScope)
            {
                Assert.Equal(0, convention1.Calls);
                Assert.Equal(0, convention2.Calls);
                scope.Dispose();
            }

            Assert.Equal(1, convention1.Calls);
            Assert.Equal(1, convention2.Calls);
            Assert.Equal(0, convention3.Calls);

            Assert.Empty(builder.Metadata.GetEntityTypes().Where(e => e.HasDelegatedIdentity()));
            Assert.Null(builder.Metadata.FindEntityType(typeof(Order)));
        }

        private class EntityTypeConvention : IEntityTypeConvention
        {
            private readonly bool _terminate;
            private readonly bool _delegatedIdentity;
            public int Calls;

            public EntityTypeConvention(bool terminate, bool delegatedIdentity)
            {
                _terminate = terminate;
                _delegatedIdentity = delegatedIdentity;
            }

            public InternalEntityTypeBuilder Apply(InternalEntityTypeBuilder entityTypeBuilder)
            {
                Assert.Same(entityTypeBuilder, entityTypeBuilder.Metadata.Builder);
                if (entityTypeBuilder.Metadata.HasDelegatedIdentity() == _delegatedIdentity)
                {
                    Calls++;
                }

                if (_terminate)
                {
                    if (entityTypeBuilder.Metadata.HasDelegatedIdentity())
                    {
                        if (_delegatedIdentity)
                        {
                            entityTypeBuilder.Metadata.Model.RemoveDelegatedIdentityEntityType(entityTypeBuilder.Metadata);
                        }
                    }
                    else
                    {
                        if (!_delegatedIdentity)
                        {
                            entityTypeBuilder.Metadata.Model.RemoveEntityType(entityTypeBuilder.Metadata.Name);
                        }
                    }

                    return entityTypeBuilder;
                }

                return entityTypeBuilder;
            }
        }

        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [Theory]
        public void OnEntityTypeIgnored_calls_apply_on_conventions_in_order(bool useBuilder, bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new EntityTypeIgnoredConvention(terminate: false);
            var convention2 = new EntityTypeIgnoredConvention(terminate: true);
            var convention3 = new EntityTypeIgnoredConvention(terminate: false);
            conventions.EntityTypeIgnoredConventions.Add(convention1);
            conventions.EntityTypeIgnoredConventions.Add(convention2);
            conventions.EntityTypeIgnoredConventions.Add(convention3);

            var builder = new InternalModelBuilder(new Model(conventions));

            var scope = useScope ? builder.Metadata.ConventionDispatcher.StartBatch() : null;

            if (useBuilder)
            {
                builder.Entity(typeof(Order), ConfigurationSource.Convention);
                Assert.True(builder.Ignore(typeof(Order).DisplayName(), ConfigurationSource.Convention));
            }
            else
            {
                builder.Metadata.Ignore(typeof(Order), ConfigurationSource.Convention);
            }

            if (useScope)
            {
                Assert.Equal(0, convention1.Calls);
                Assert.Equal(0, convention2.Calls);
                scope.Dispose();
            }

            Assert.Equal(1, convention1.Calls);
            Assert.Equal(1, convention2.Calls);
            Assert.Equal(0, convention3.Calls);
        }

        private class EntityTypeIgnoredConvention : IEntityTypeIgnoredConvention
        {
            private readonly bool _terminate;
            public int Calls;

            public EntityTypeIgnoredConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public bool Apply(InternalModelBuilder modelBuilder, string name, Type type)
            {
                Assert.Null(modelBuilder.Metadata.FindEntityType(name));
                Calls++;

                return !_terminate;
            }
        }

        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [Theory]
        public void OnBaseEntityTypeSet_calls_apply_on_conventions_in_order(bool useBuilder, bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new BaseTypeConvention(terminate: false);
            var convention2 = new BaseTypeConvention(terminate: true);
            var convention3 = new BaseTypeConvention(terminate: false);
            conventions.BaseEntityTypeSetConventions.Add(convention1);
            conventions.BaseEntityTypeSetConventions.Add(convention2);
            conventions.BaseEntityTypeSetConventions.Add(convention3);

            var builder = new InternalModelBuilder(new Model(conventions))
                .Entity(typeof(SpecialOrder), ConfigurationSource.Convention);

            var scope = useScope ? builder.Metadata.Model.ConventionDispatcher.StartBatch() : null;

            if (useBuilder)
            {
                Assert.NotNull(builder.HasBaseType(typeof(Order), ConfigurationSource.Convention));
            }
            else
            {
                builder.Metadata.HasBaseType(builder.Metadata.Model.AddEntityType(typeof(Order)), ConfigurationSource.Convention);
            }

            if (useScope)
            {
                Assert.Empty(convention1.Calls);
                Assert.Empty(convention2.Calls);
                scope.Dispose();
            }

            Assert.Equal(new[] { typeof(Order) }, convention1.Calls);
            Assert.Equal(new[] { typeof(Order) }, convention2.Calls);
            Assert.Empty(convention3.Calls);

            if (useBuilder)
            {
                Assert.NotNull(builder.HasBaseType(typeof(Order), ConfigurationSource.Convention));
            }
            else
            {
                builder.Metadata.HasBaseType(builder.Metadata.Model.FindEntityType(typeof(Order)), ConfigurationSource.Convention);
            }

            Assert.Equal(new[] { typeof(Order) }, convention1.Calls);
            Assert.Equal(new[] { typeof(Order) }, convention2.Calls);
            Assert.Empty(convention3.Calls);

            if (useBuilder)
            {
                Assert.NotNull(builder.HasBaseType((Type)null, ConfigurationSource.Convention));
            }
            else
            {
                builder.Metadata.HasBaseType(null, ConfigurationSource.Convention);
            }

            Assert.Equal(new[] { typeof(Order), null }, convention1.Calls);
            Assert.Equal(new[] { typeof(Order), null }, convention2.Calls);
            Assert.Empty(convention3.Calls);
        }

        private class BaseTypeConvention : IBaseTypeConvention
        {
            private readonly bool _terminate;
            public readonly List<Type> Calls = new List<Type>();

            public BaseTypeConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public bool Apply(InternalEntityTypeBuilder entityTypeBuilder, EntityType oldBaseType)
            {
                Assert.NotNull(entityTypeBuilder.Metadata.Builder);

                Calls.Add(entityTypeBuilder.Metadata.BaseType?.ClrType);

                return !_terminate;
            }
        }

        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [Theory]
        public void OnEntityTypeAnnotationSet_calls_apply_on_conventions_in_order(bool useBuilder, bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new EntityTypeAnnotationSetConvention(terminate: false);
            var convention2 = new EntityTypeAnnotationSetConvention(terminate: true);
            var convention3 = new EntityTypeAnnotationSetConvention(terminate: false);
            conventions.EntityTypeAnnotationSetConventions.Add(convention1);
            conventions.EntityTypeAnnotationSetConventions.Add(convention2);
            conventions.EntityTypeAnnotationSetConventions.Add(convention3);

            var builder = new InternalModelBuilder(new Model(conventions));
            var entityBuilder = builder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);

            var scope = useScope ? builder.Metadata.ConventionDispatcher.StartBatch() : null;

            if (useBuilder)
            {
                Assert.NotNull(entityBuilder.HasAnnotation("foo", "bar", ConfigurationSource.Convention));
            }
            else
            {
                entityBuilder.Metadata["foo"] = "bar";
            }

            if (useScope)
            {
                Assert.Empty(convention1.Calls);
                Assert.Empty(convention2.Calls);
                scope.Dispose();
            }

            Assert.Equal(new[] { "bar" }, convention1.Calls);
            Assert.Equal(new[] { "bar" }, convention2.Calls);
            Assert.Empty(convention3.Calls);

            if (useBuilder)
            {
                Assert.NotNull(entityBuilder.HasAnnotation("foo", "bar", ConfigurationSource.Convention));
            }
            else
            {
                entityBuilder.Metadata["foo"] = "bar";
            }

            Assert.Equal(new[] { "bar" }, convention1.Calls);
            Assert.Equal(new[] { "bar" }, convention2.Calls);
            Assert.Empty(convention3.Calls);

            if (useBuilder)
            {
                Assert.NotNull(entityBuilder.HasAnnotation("foo", null, ConfigurationSource.Convention));
            }
            else
            {
                entityBuilder.Metadata.RemoveAnnotation("foo");
            }

            Assert.Equal(new[] { "bar", null }, convention1.Calls);
            Assert.Equal(new[] { "bar", null }, convention2.Calls);
            //Assert.Empty(convention3.Calls);
        }

        private class EntityTypeAnnotationSetConvention : IEntityTypeAnnotationSetConvention
        {
            private readonly bool _terminate;
            public readonly List<object> Calls = new List<object>();

            public EntityTypeAnnotationSetConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public Annotation Apply(
                InternalEntityTypeBuilder entityTypeBuilder,
                string name,
                Annotation annotation,
                Annotation oldAnnotation)
            {
                Calls.Add(annotation?.Value);

                return _terminate ? null : annotation;
            }
        }

        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [Theory]
        public void OnEntityTypeMemberIgnored_calls_apply_on_conventions_in_order(bool useBuilder, bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new EntityTypeMemberIgnoredConvention(terminate: false);
            var convention2 = new EntityTypeMemberIgnoredConvention(terminate: true);
            var convention3 = new EntityTypeMemberIgnoredConvention(terminate: false);
            conventions.EntityTypeMemberIgnoredConventions.Add(convention1);
            conventions.EntityTypeMemberIgnoredConventions.Add(convention2);
            conventions.EntityTypeMemberIgnoredConventions.Add(convention3);

            var builder = new InternalModelBuilder(new Model(conventions));
            var entityBuilder = builder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);

            var scope = useScope ? builder.Metadata.ConventionDispatcher.StartBatch() : null;

            if (useBuilder)
            {
                Assert.NotNull(entityBuilder.Ignore("A", ConfigurationSource.Convention));
            }
            else
            {
                entityBuilder.Metadata.Ignore("A", ConfigurationSource.Convention);
            }

            if (useScope)
            {
                Assert.Empty(convention1.Calls);
                Assert.Empty(convention2.Calls);
                scope.Dispose();
            }

            Assert.Equal(new[] { "A" }, convention1.Calls);
            Assert.Equal(new[] { "A" }, convention2.Calls);
            Assert.Empty(convention3.Calls);

            if (useBuilder)
            {
                Assert.NotNull(entityBuilder.Ignore("A", ConfigurationSource.Convention));
            }
            else
            {
                entityBuilder.Metadata.Ignore("A", ConfigurationSource.Convention);
            }

            Assert.Equal(new[] { "A" }, convention1.Calls);
            Assert.Equal(new[] { "A" }, convention2.Calls);
            Assert.Empty(convention3.Calls);
        }

        private class EntityTypeMemberIgnoredConvention : IEntityTypeMemberIgnoredConvention
        {
            private readonly bool _terminate;
            public readonly List<object> Calls = new List<object>();

            public EntityTypeMemberIgnoredConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public bool Apply(InternalEntityTypeBuilder entityTypeBuilder, string ignoredMemberName)
            {
                Assert.NotNull(entityTypeBuilder.Metadata.Builder);

                Calls.Add(ignoredMemberName);
                return !_terminate;
            }
        }

        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [Theory]
        public void OnPropertyAdded_calls_apply_on_conventions_in_order(bool useBuilder, bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new PropertyConvention(terminate: false);
            var convention2 = new PropertyConvention(terminate: true);
            var convention3 = new PropertyConvention(terminate: false);
            conventions.PropertyAddedConventions.Add(convention1);
            conventions.PropertyAddedConventions.Add(convention2);
            conventions.PropertyAddedConventions.Add(convention3);

            var builder = new InternalModelBuilder(new Model(conventions));
            var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
            var shadowPropertyName = "ShadowProperty";

            var scope = useScope ? builder.Metadata.ConventionDispatcher.StartBatch() : null;

            if (useBuilder)
            {
                var result = entityBuilder.Property(shadowPropertyName, typeof(int), ConfigurationSource.Convention);

                Assert.Equal(!useScope, result == null);
            }
            else
            {
                var result = entityBuilder.Metadata.AddProperty(shadowPropertyName, typeof(int));

                Assert.Equal(!useScope, result == null);
            }

            if (useScope)
            {
                Assert.Empty(convention1.Calls);
                Assert.Empty(convention2.Calls);
                scope.Dispose();
            }

            Assert.Equal(new[] { shadowPropertyName }, convention1.Calls);
            Assert.Equal(new[] { shadowPropertyName }, convention2.Calls);
            Assert.Empty(convention3.Calls);

            scope = useScope ? builder.Metadata.ConventionDispatcher.StartBatch() : null;

            if (useBuilder)
            {
                var result = entityBuilder.Property(Order.OrderIdProperty, ConfigurationSource.Convention);

                Assert.Equal(!useScope, result == null);
            }
            else
            {
                var result = entityBuilder.Metadata.AddProperty(Order.OrderIdProperty);

                Assert.Equal(!useScope, result == null);
            }

            if (useScope)
            {
                Assert.Equal(new[] { shadowPropertyName }, convention1.Calls);
                Assert.Equal(new[] { shadowPropertyName }, convention2.Calls);
                scope.Dispose();
            }

            Assert.Equal(new[] { shadowPropertyName, nameof(Order.OrderId) }, convention1.Calls);
            Assert.Equal(new[] { shadowPropertyName, nameof(Order.OrderId) }, convention2.Calls);
            Assert.Empty(convention3.Calls);

            Assert.Empty(entityBuilder.Metadata.GetProperties());
        }

        private class PropertyConvention : IPropertyConvention
        {
            private readonly bool _terminate;
            public readonly List<object> Calls = new List<object>();

            public PropertyConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public InternalPropertyBuilder Apply(InternalPropertyBuilder propertyBuilder)
            {
                Assert.NotNull(propertyBuilder.Metadata.Builder);

                Calls.Add(propertyBuilder.Metadata.Name);

                if (_terminate)
                {
                    propertyBuilder.Metadata.DeclaringEntityType.RemoveProperty(propertyBuilder.Metadata.Name);
                    return null;
                }

                return propertyBuilder;
            }
        }

        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [Theory]
        public void OnPropertyFieldChanged_calls_apply_on_conventions_in_order(bool useBuilder, bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new PropertyFieldChangedConvention(terminate: false);
            var convention2 = new PropertyFieldChangedConvention(terminate: true);
            var convention3 = new PropertyFieldChangedConvention(terminate: false);
            conventions.PropertyFieldChangedConventions.Add(convention1);
            conventions.PropertyFieldChangedConventions.Add(convention2);
            conventions.PropertyFieldChangedConventions.Add(convention3);

            var builder = new InternalModelBuilder(new Model(conventions));
            var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
            var propertyBuilder = entityBuilder.Property(Order.OrderIdProperty, ConfigurationSource.Convention);

            var scope = useScope ? builder.Metadata.ConventionDispatcher.StartBatch() : null;

            if (useBuilder)
            {
                Assert.True(propertyBuilder.HasField(nameof(Order.IntField), ConfigurationSource.Convention));
            }
            else
            {
                propertyBuilder.Metadata.SetField(nameof(Order.IntField));
            }

            if (useScope)
            {
                Assert.Empty(convention1.Calls);
                Assert.Empty(convention2.Calls);
                scope.Dispose();
            }

            Assert.Equal(new string[] { null }, convention1.Calls);
            Assert.Equal(new string[] { null }, convention2.Calls);
            Assert.Empty(convention3.Calls);

            if (useBuilder)
            {
                Assert.True(propertyBuilder.HasField(nameof(Order.IntField), ConfigurationSource.Convention));
            }
            else
            {
                propertyBuilder.Metadata.SetField(nameof(Order.IntField));
            }

            Assert.Equal(new string[] { null }, convention1.Calls);
            Assert.Equal(new string[] { null }, convention2.Calls);
            Assert.Empty(convention3.Calls);

            if (useBuilder)
            {
                Assert.True(propertyBuilder.HasField(null, ConfigurationSource.Convention));
            }
            else
            {
                propertyBuilder.Metadata.SetField(null);
            }

            Assert.Equal(new[] { null, nameof(Order.IntField) }, convention1.Calls);
            Assert.Equal(new[] { null, nameof(Order.IntField) }, convention2.Calls);
            Assert.Empty(convention3.Calls);
        }

        private class PropertyFieldChangedConvention : IPropertyFieldChangedConvention
        {
            private readonly bool _terminate;
            public readonly List<object> Calls = new List<object>();

            public PropertyFieldChangedConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public bool Apply(InternalPropertyBuilder propertyBuilder, FieldInfo oldFieldInfo)
            {
                Assert.NotNull(propertyBuilder.Metadata.Builder);

                Calls.Add(oldFieldInfo?.Name);

                return !_terminate;
            }
        }

        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [Theory]
        public void OnPropertyNullableChanged_calls_apply_on_conventions_in_order(bool useBuilder, bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new PropertyNullableConvention(false);
            var convention2 = new PropertyNullableConvention(true);
            var convention3 = new PropertyNullableConvention(false);
            conventions.PropertyNullableChangedConventions.Add(convention1);
            conventions.PropertyNullableChangedConventions.Add(convention2);
            conventions.PropertyNullableChangedConventions.Add(convention3);

            var builder = new ModelBuilder(conventions);

            var scope = useScope ? ((Model)builder.Model).ConventionDispatcher.StartBatch() : null;

            var propertyBuilder = builder.Entity<Order>().Property(e => e.Name);
            if (useBuilder)
            {
                propertyBuilder.IsRequired();
            }
            else
            {
                propertyBuilder.Metadata.IsNullable = false;
            }

            if (useScope)
            {
                Assert.Empty(convention1.Calls);
                Assert.Empty(convention2.Calls);
            }
            else
            {
                Assert.Equal(new bool?[] { false }, convention1.Calls);
                Assert.Equal(new bool?[] { false }, convention2.Calls);
            }
            Assert.Empty(convention3.Calls);

            propertyBuilder = builder.Entity<Order>().Property(e => e.Name);
            if (useBuilder)
            {
                propertyBuilder.IsRequired(false);
            }
            else
            {
                propertyBuilder.Metadata.IsNullable = true;
            }

            if (useScope)
            {
                Assert.Empty(convention1.Calls);
                Assert.Empty(convention2.Calls);
            }
            else
            {
                Assert.Equal(new bool?[] { false, true }, convention1.Calls);
                Assert.Equal(new bool?[] { false, true }, convention2.Calls);
            }
            Assert.Empty(convention3.Calls);

            propertyBuilder = builder.Entity<Order>().Property(e => e.Name);
            if (useBuilder)
            {
                propertyBuilder.IsRequired(false);
            }
            else
            {
                propertyBuilder.Metadata.IsNullable = true;
            }

            if (useScope)
            {
                Assert.Empty(convention1.Calls);
                Assert.Empty(convention2.Calls);
            }
            else
            {
                Assert.Equal(new bool?[] { false, true }, convention1.Calls);
                Assert.Equal(new bool?[] { false, true }, convention2.Calls);
            }
            Assert.Empty(convention3.Calls);

            propertyBuilder = builder.Entity<Order>().Property(e => e.Name);

            if (useBuilder)
            {
                propertyBuilder.IsRequired();
            }
            else
            {
                propertyBuilder.Metadata.IsNullable = false;
            }

            scope?.Dispose();

            if (useScope)
            {
                Assert.Equal(new bool?[] { false, false, false }, convention1.Calls);
                Assert.Equal(new bool?[] { false, false, false }, convention2.Calls);
            }
            else
            {
                Assert.Equal(new bool?[] { false, true, false }, convention1.Calls);
                Assert.Equal(new bool?[] { false, true, false }, convention2.Calls);
            }
            Assert.Empty(convention3.Calls);
        }

        private class PropertyNullableConvention : IPropertyNullableConvention
        {
            public readonly List<bool?> Calls = new List<bool?>();
            private readonly bool _terminate;

            public PropertyNullableConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public bool Apply(InternalPropertyBuilder propertyBuilder)
            {
                Calls.Add(propertyBuilder.Metadata.IsNullable);

                return !_terminate;
            }
        }

        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [Theory]
        public void OnPropertyAnnotationSet_calls_apply_on_conventions_in_order(bool useBuilder, bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new PropertyAnnotationSetConvention(false);
            var convention2 = new PropertyAnnotationSetConvention(true);
            var convention3 = new PropertyAnnotationSetConvention(false);
            conventions.PropertyAnnotationSetConventions.Add(convention1);
            conventions.PropertyAnnotationSetConventions.Add(convention2);
            conventions.PropertyAnnotationSetConventions.Add(convention3);

            var builder = new InternalModelBuilder(new Model(conventions));
            var propertyBuilder = builder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention)
                .Property(nameof(SpecialOrder.Name), ConfigurationSource.Convention);

            var scope = useScope ? builder.Metadata.ConventionDispatcher.StartBatch() : null;

            if (useBuilder)
            {
                Assert.NotNull(propertyBuilder.HasAnnotation("foo", "bar", ConfigurationSource.Convention));
            }
            else
            {
                propertyBuilder.Metadata["foo"] = "bar";
            }

            if (useScope)
            {
                Assert.Empty(convention1.Calls);
                Assert.Empty(convention2.Calls);
                scope.Dispose();
            }

            Assert.Equal(new[] { "bar" }, convention1.Calls);
            Assert.Equal(new[] { "bar" }, convention2.Calls);
            Assert.Empty(convention3.Calls);

            if (useBuilder)
            {
                Assert.NotNull(propertyBuilder.HasAnnotation("foo", "bar", ConfigurationSource.Convention));
            }
            else
            {
                propertyBuilder.Metadata["foo"] = "bar";
            }

            Assert.Equal(new[] { "bar" }, convention1.Calls);
            Assert.Equal(new[] { "bar" }, convention2.Calls);
            Assert.Empty(convention3.Calls);

            if (useBuilder)
            {
                Assert.NotNull(propertyBuilder.HasAnnotation("foo", null, ConfigurationSource.Convention));
            }
            else
            {
                propertyBuilder.Metadata.RemoveAnnotation("foo");
            }

            Assert.Equal(new[] { "bar", null }, convention1.Calls);
            Assert.Equal(new[] { "bar", null }, convention2.Calls);
            //Assert.Empty(convention3.Calls);
        }

        private class PropertyAnnotationSetConvention : IPropertyAnnotationSetConvention
        {
            private readonly bool _terminate;
            public readonly List<object> Calls = new List<object>();

            public PropertyAnnotationSetConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public Annotation Apply(
                InternalPropertyBuilder propertyBuilder,
                string name,
                Annotation annotation,
                Annotation oldAnnotation)
            {
                Assert.NotNull(propertyBuilder.Metadata.Builder);

                Calls.Add(annotation?.Value);

                return _terminate ? null : annotation;
            }
        }

        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [Theory]
        public void OnKeyAdded_calls_apply_on_conventions_in_order(bool useBuilder, bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new KeyConvention(terminate: false);
            var convention2 = new KeyConvention(terminate: true);
            var convention3 = new KeyConvention(terminate: false);
            conventions.KeyAddedConventions.Add(convention1);
            conventions.KeyAddedConventions.Add(convention2);
            conventions.KeyAddedConventions.Add(convention3);

            var builder = new InternalModelBuilder(new Model(conventions));
            var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
            var keyPropertyName = "OrderId";

            var scope = useScope ? builder.Metadata.ConventionDispatcher.StartBatch() : null;

            if (useBuilder)
            {
                var result = entityBuilder.HasKey(new List<string> { keyPropertyName }, ConfigurationSource.Convention);

                Assert.Equal(!useScope, result == null);
            }
            else
            {
                var property = entityBuilder.Property(keyPropertyName, ConfigurationSource.Convention).Metadata;
                property.IsNullable = false;
                var result = entityBuilder.Metadata.AddKey(property);

                Assert.Equal(!useScope, result == null);
            }

            if (useScope)
            {
                Assert.Empty(convention1.Calls);
                Assert.Empty(convention2.Calls);
                scope.Dispose();
            }

            Assert.Equal(new[] { keyPropertyName }, convention1.Calls);
            Assert.Equal(new[] { keyPropertyName }, convention2.Calls);
            Assert.Empty(convention3.Calls);
        }

        private class KeyConvention : IKeyConvention
        {
            private readonly bool _terminate;
            public readonly List<object> Calls = new List<object>();

            public KeyConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public InternalKeyBuilder Apply(InternalKeyBuilder keyBuilder)
            {
                Assert.NotNull(keyBuilder.Metadata.Builder);

                Calls.Add(keyBuilder.Metadata.Properties.First().Name);

                if (_terminate)
                {
                    keyBuilder.Metadata.DeclaringEntityType.RemoveKey(keyBuilder.Metadata.Properties);
                    return null;
                }

                return keyBuilder;
            }
        }

        [InlineData(false)]
        [InlineData(true)]
        [Theory]
        public void OnKeyRemoved_calls_apply_on_conventions_in_order(bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new KeyRemovedConvention(terminate: false);
            var convention2 = new KeyRemovedConvention(terminate: true);
            var convention3 = new KeyRemovedConvention(terminate: false);
            conventions.KeyRemovedConventions.Add(convention1);
            conventions.KeyRemovedConventions.Add(convention2);
            //conventions.KeyRemovedConventions.Add(convention3);

            var builder = new InternalModelBuilder(new Model(conventions));

            var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
            var key = entityBuilder.HasKey(new List<string> { "OrderId" }, ConfigurationSource.Convention).Metadata;

            var scope = useScope ? builder.Metadata.ConventionDispatcher.StartBatch() : null;

            Assert.Same(key, entityBuilder.Metadata.RemoveKey(key.Properties));

            if (useScope)
            {
                Assert.Empty(convention1.Calls);
                Assert.Empty(convention2.Calls);
                scope.Dispose();
            }

            Assert.Equal(new[] { "OrderId" }, convention1.Calls);
            Assert.Equal(new[] { "OrderId" }, convention2.Calls);
            Assert.Empty(convention3.Calls);
        }

        private class KeyRemovedConvention : IKeyRemovedConvention
        {
            private readonly bool _terminate;
            public readonly List<object> Calls = new List<object>();

            public KeyRemovedConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public void Apply(InternalEntityTypeBuilder entityTypeBuilder, Key key)
            {
                Assert.NotNull(entityTypeBuilder.Metadata.Builder);

                Calls.Add(key.Properties.First().Name);
            }
        }

        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [Theory]
        public void OnPrimaryKeySet_calls_apply_on_conventions_in_order(bool useBuilder, bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new PrimaryKeyConvention(terminate: false);
            var convention2 = new PrimaryKeyConvention(terminate: true);
            var convention3 = new PrimaryKeyConvention(terminate: false);
            conventions.PrimaryKeySetConventions.Add(convention1);
            conventions.PrimaryKeySetConventions.Add(convention2);
            conventions.PrimaryKeySetConventions.Add(convention3);

            var builder = new InternalModelBuilder(new Model(conventions));
            var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);

            entityBuilder.HasKey(new[] { "OrderId" }, ConfigurationSource.Convention);

            var scope = useScope ? builder.Metadata.ConventionDispatcher.StartBatch() : null;

            if (useBuilder)
            {
                Assert.NotNull(entityBuilder.PrimaryKey(new[] { "OrderId" }, ConfigurationSource.Convention));
            }
            else
            {
                Assert.NotNull(entityBuilder.Metadata.SetPrimaryKey(
                    entityBuilder.Property("OrderId", ConfigurationSource.Convention).Metadata));
            }

            if (useScope)
            {
                Assert.Empty(convention1.Calls);
                Assert.Empty(convention2.Calls);
                scope.Dispose();
            }

            Assert.Equal(new string[] { null }, convention1.Calls);
            Assert.Equal(new string[] { null }, convention2.Calls);
            Assert.Empty(convention3.Calls);

            if (useBuilder)
            {
                Assert.NotNull(entityBuilder.PrimaryKey(new[] { "OrderId" }, ConfigurationSource.Convention));
            }
            else
            {
                Assert.NotNull(entityBuilder.Metadata.SetPrimaryKey(
                    entityBuilder.Property("OrderId", ConfigurationSource.Convention).Metadata));
            }

            Assert.Equal(new string[] { null }, convention1.Calls);
            Assert.Equal(new string[] { null }, convention2.Calls);
            Assert.Empty(convention3.Calls);

            if (useBuilder)
            {
                Assert.Null(entityBuilder.PrimaryKey((IReadOnlyList<string>)null, ConfigurationSource.Convention));
            }
            else
            {
                Assert.Null(entityBuilder.Metadata.SetPrimaryKey(null));
            }

            Assert.Equal(new[] { null, "OrderId" }, convention1.Calls);
            Assert.Equal(new[] { null, "OrderId" }, convention2.Calls);
            Assert.Empty(convention3.Calls);
            Assert.Null(entityBuilder.Metadata.GetPrimaryKeyConfigurationSource());
        }

        private class PrimaryKeyConvention : IPrimaryKeyConvention
        {
            private readonly bool _terminate;
            public readonly List<object> Calls = new List<object>();

            public PrimaryKeyConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public bool Apply(InternalEntityTypeBuilder entityTypeBuilder, Key previousPrimaryKey)
            {
                Assert.NotNull(entityTypeBuilder.Metadata.Builder);

                Calls.Add(previousPrimaryKey?.Properties.First().Name);

                return !_terminate;
            }
        }

        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [Theory]
        public void OnIndexAdded_calls_apply_on_conventions_in_order(bool useBuilder, bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new IndexConvention(terminate: false);
            var convention2 = new IndexConvention(terminate: true);
            var convention3 = new IndexConvention(terminate: false);
            conventions.IndexAddedConventions.Add(convention1);
            conventions.IndexAddedConventions.Add(convention2);
            conventions.IndexAddedConventions.Add(convention3);

            var builder = new InternalModelBuilder(new Model(conventions));
            var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);

            var scope = useScope ? builder.Metadata.ConventionDispatcher.StartBatch() : null;

            if (useBuilder)
            {
                var result = entityBuilder.HasIndex(new List<string> { "OrderId" }, ConfigurationSource.Convention);

                Assert.Equal(!useScope, result == null);
            }
            else
            {
                var property = entityBuilder.Property("OrderId", ConfigurationSource.Convention).Metadata;
                var result = entityBuilder.Metadata.AddIndex(property);

                Assert.Equal(!useScope, result == null);
            }

            if (useScope)
            {
                Assert.Empty(convention1.Calls);
                Assert.Empty(convention2.Calls);
                scope.Dispose();
            }

            Assert.Equal(new[] { "OrderId" }, convention1.Calls);
            Assert.Equal(new[] { "OrderId" }, convention2.Calls);
            Assert.Empty(convention3.Calls);
        }

        private class IndexConvention : IIndexConvention
        {
            private readonly bool _terminate;
            public readonly List<object> Calls = new List<object>();

            public IndexConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public InternalIndexBuilder Apply(InternalIndexBuilder indexBuilder)
            {
                Assert.NotNull(indexBuilder.Metadata.Builder);

                Calls.Add(indexBuilder.Metadata.Properties.First().Name);

                if (_terminate)
                {
                    indexBuilder.Metadata.DeclaringEntityType.RemoveIndex(indexBuilder.Metadata.Properties);
                }

                return indexBuilder;
            }
        }

        [InlineData(false)]
        [InlineData(true)]
        [Theory]
        public void OnIndexRemoved_calls_apply_on_conventions_in_order(bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new IndexRemovedConvention(terminate: false);
            var convention2 = new IndexRemovedConvention(terminate: true);
            var convention3 = new IndexRemovedConvention(terminate: false);
            conventions.IndexRemovedConventions.Add(convention1);
            conventions.IndexRemovedConventions.Add(convention2);
            //conventions.IndexRemovedConventions.Add(convention3);

            var builder = new InternalModelBuilder(new Model(conventions));
            var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
            var index = entityBuilder.HasIndex(new List<string> { "OrderId" }, ConfigurationSource.Convention).Metadata;

            var scope = useScope ? builder.Metadata.ConventionDispatcher.StartBatch() : null;

            Assert.Same(index, entityBuilder.Metadata.RemoveIndex(index.Properties));

            if (useScope)
            {
                Assert.Empty(convention1.Calls);
                Assert.Empty(convention2.Calls);
                scope.Dispose();
            }

            Assert.Equal(new[] { "OrderId" }, convention1.Calls);
            Assert.Equal(new[] { "OrderId" }, convention2.Calls);
            Assert.Empty(convention3.Calls);
        }

        private class IndexRemovedConvention : IIndexRemovedConvention
        {
            private readonly bool _terminate;
            public readonly List<object> Calls = new List<object>();

            public IndexRemovedConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public void Apply(InternalEntityTypeBuilder entityTypeBuilder, Index index)
            {
                Assert.NotNull(entityTypeBuilder.Metadata.Builder);

                Calls.Add(index.Properties.First().Name);
            }
        }

        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [Theory]
        public void OnIndexUniquenessChanged_calls_apply_on_conventions_in_order(bool useBuilder, bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new IndexUniquenessConvention(terminate: false);
            var convention2 = new IndexUniquenessConvention(terminate: true);
            var convention3 = new IndexUniquenessConvention(terminate: false);
            conventions.IndexUniquenessConventions.Add(convention1);
            conventions.IndexUniquenessConventions.Add(convention2);
            conventions.IndexUniquenessConventions.Add(convention3);

            var builder = new InternalModelBuilder(new Model(conventions));
            var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
            var index = entityBuilder.HasIndex(new List<string> { "OrderId" }, ConfigurationSource.Convention).Metadata;

            var scope = useScope ? builder.Metadata.ConventionDispatcher.StartBatch() : null;

            if (useBuilder)
            {
                index.Builder.IsUnique(true, ConfigurationSource.Convention);
            }
            else
            {
                index.IsUnique = true;
            }

            if (useScope)
            {
                Assert.Empty(convention1.Calls);
                Assert.Empty(convention2.Calls);
                scope.Dispose();
            }

            Assert.Equal(new[] { true }, convention1.Calls);
            Assert.Equal(new[] { true }, convention2.Calls);
            Assert.Empty(convention3.Calls);

            if (useBuilder)
            {
                index.Builder.IsUnique(true, ConfigurationSource.Convention);
            }
            else
            {
                index.IsUnique = true;
            }

            Assert.Equal(new[] { true }, convention1.Calls);
            Assert.Equal(new[] { true }, convention2.Calls);
            Assert.Empty(convention3.Calls);

            if (useBuilder)
            {
                index.Builder.IsUnique(false, ConfigurationSource.Convention);
            }
            else
            {
                index.IsUnique = false;
            }

            Assert.Equal(new[] { true, false }, convention1.Calls);
            Assert.Equal(new[] { true, false }, convention2.Calls);
            Assert.Empty(convention3.Calls);

            Assert.Same(index, entityBuilder.Metadata.RemoveIndex(index.Properties));
        }

        private class IndexUniquenessConvention : IIndexUniquenessConvention
        {
            private readonly bool _terminate;
            public readonly List<bool> Calls = new List<bool>();

            public IndexUniquenessConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public bool Apply(InternalIndexBuilder indexBuilder)
            {
                Assert.NotNull(indexBuilder.Metadata.Builder);

                Calls.Add(indexBuilder.Metadata.IsUnique);

                return !_terminate;
            }
        }

        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [Theory]
        public void OnIndexAnnotationSet_calls_apply_on_conventions_in_order(bool useBuilder, bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new IndexAnnotationSetConvention(terminate: false);
            var convention2 = new IndexAnnotationSetConvention(terminate: true);
            var convention3 = new IndexAnnotationSetConvention(terminate: false);
            conventions.IndexAnnotationSetConventions.Add(convention1);
            conventions.IndexAnnotationSetConventions.Add(convention2);
            conventions.IndexAnnotationSetConventions.Add(convention3);

            var builder = new InternalModelBuilder(new Model(conventions));
            var indexBuilder = builder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention)
                .HasIndex(new[] { nameof(SpecialOrder.Name) }, ConfigurationSource.Convention);

            var scope = useScope ? builder.Metadata.ConventionDispatcher.StartBatch() : null;

            if (useBuilder)
            {
                Assert.NotNull(indexBuilder.HasAnnotation("foo", "bar", ConfigurationSource.Convention));
            }
            else
            {
                indexBuilder.Metadata["foo"] = "bar";
            }

            if (useScope)
            {
                Assert.Empty(convention1.Calls);
                Assert.Empty(convention2.Calls);
                scope.Dispose();
            }

            Assert.Equal(new[] { "bar" }, convention1.Calls);
            Assert.Equal(new[] { "bar" }, convention2.Calls);
            Assert.Empty(convention3.Calls);

            if (useBuilder)
            {
                Assert.NotNull(indexBuilder.HasAnnotation("foo", "bar", ConfigurationSource.Convention));
            }
            else
            {
                indexBuilder.Metadata["foo"] = "bar";
            }

            Assert.Equal(new[] { "bar" }, convention1.Calls);
            Assert.Equal(new[] { "bar" }, convention2.Calls);
            Assert.Empty(convention3.Calls);

            if (useBuilder)
            {
                Assert.NotNull(indexBuilder.HasAnnotation("foo", null, ConfigurationSource.Convention));
            }
            else
            {
                indexBuilder.Metadata.RemoveAnnotation("foo");
            }

            Assert.Equal(new[] { "bar", null }, convention1.Calls);
            Assert.Equal(new[] { "bar", null }, convention2.Calls);
            //Assert.Empty(convention3.Calls);
        }

        private class IndexAnnotationSetConvention : IIndexAnnotationSetConvention
        {
            private readonly bool _terminate;
            public readonly List<object> Calls = new List<object>();

            public IndexAnnotationSetConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public Annotation Apply(
                InternalIndexBuilder indexBuilder,
                string name,
                Annotation annotation,
                Annotation oldAnnotation)
            {
                Assert.NotNull(indexBuilder.Metadata.Builder);

                Calls.Add(annotation?.Value);

                return _terminate ? null : annotation;
            }
        }

        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [Theory]
        public void OnForeignKeyAdded_calls_apply_on_conventions_in_order(bool useBuilder, bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new ForeignKeyConvention(terminate: false);
            var convention2 = new ForeignKeyConvention(terminate: true);
            var convention3 = new ForeignKeyConvention(terminate: false);
            conventions.ForeignKeyAddedConventions.Add(convention1);
            conventions.ForeignKeyAddedConventions.Add(convention2);
            conventions.ForeignKeyAddedConventions.Add(convention3);

            var builder = new InternalModelBuilder(new Model(conventions));
            var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
            entityBuilder.PrimaryKey(new[] { "OrderId" }, ConfigurationSource.Convention);

            var scope = useScope ? builder.Metadata.ConventionDispatcher.StartBatch() : null;

            if (useBuilder)
            {
                var result = entityBuilder.Relationship(entityBuilder, ConfigurationSource.Convention);

                Assert.Equal(!useScope, result == null);
            }
            else
            {
                var result = entityBuilder.Metadata.AddForeignKey(
                    entityBuilder.Property("OrderId1", typeof(int), ConfigurationSource.Convention).Metadata,
                    entityBuilder.Metadata.FindPrimaryKey(),
                    entityBuilder.Metadata,
                    ConfigurationSource.Convention);

                Assert.Equal(!useScope, result == null);
            }

            if (useScope)
            {
                Assert.Empty(convention1.Calls);
                Assert.Empty(convention2.Calls);
                scope.Dispose();
            }

            Assert.Equal(new[] { "OrderId1" }, convention1.Calls);
            Assert.Equal(new[] { "OrderId1" }, convention2.Calls);
            Assert.Empty(convention3.Calls);
        }

        private class ForeignKeyConvention : IForeignKeyConvention
        {
            private readonly bool _terminate;
            public readonly List<object> Calls = new List<object>();

            public ForeignKeyConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public InternalRelationshipBuilder Apply(InternalRelationshipBuilder relationshipBuilder)
            {
                var fk = relationshipBuilder.Metadata;
                Assert.NotNull(fk.Builder);

                Calls.Add(fk.Properties.First().Name);

                if (_terminate)
                {
                    fk.DeclaringEntityType.RemoveForeignKey(fk.Properties, fk.PrincipalKey, fk.PrincipalEntityType);
                    return null;
                }

                return relationshipBuilder;
            }
        }

        [InlineData(false)]
        [InlineData(true)]
        [Theory]
        public void OnForeignKeyRemoved_calls_apply_on_conventions_in_order(bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new ForeignKeyRemovedConvention(terminate: false);
            var convention2 = new ForeignKeyRemovedConvention(terminate: true);
            var convention3 = new ForeignKeyRemovedConvention(terminate: false);
            conventions.ForeignKeyRemovedConventions.Add(convention1);
            conventions.ForeignKeyRemovedConventions.Add(convention2);
            conventions.ForeignKeyRemovedConventions.Add(convention3);

            var builder = new InternalModelBuilder(new Model(conventions));
            var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
            var foreignKey = entityBuilder.Metadata.AddForeignKey(
                new[] { entityBuilder.Property("FK", typeof(int), ConfigurationSource.Convention).Metadata },
                entityBuilder.HasKey(new[] { "OrderId" }, ConfigurationSource.Convention).Metadata,
                entityBuilder.Metadata);

            var scope = useScope ? builder.Metadata.ConventionDispatcher.StartBatch() : null;

            Assert.NotNull(entityBuilder.Metadata.RemoveForeignKey(foreignKey.Properties, foreignKey.PrincipalKey, foreignKey.PrincipalEntityType));

            if (useScope)
            {
                Assert.Empty(convention1.Calls);
                Assert.Empty(convention2.Calls);
                scope.Dispose();
            }

            Assert.Equal(new[] { "FK" }, convention1.Calls);
            Assert.Equal(new[] { "FK" }, convention2.Calls);
            //Assert.Empty(convention3.Calls);
        }

        private class ForeignKeyRemovedConvention : IForeignKeyRemovedConvention
        {
            private readonly bool _terminate;
            public readonly List<object> Calls = new List<object>();

            public ForeignKeyRemovedConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public void Apply(InternalEntityTypeBuilder entityTypeBuilder, ForeignKey foreignKey)
            {
                Assert.NotNull(entityTypeBuilder.Metadata.Builder);

                Calls.Add(foreignKey.Properties.First().Name);
            }
        }

        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [Theory]
        public void OnNavigationAdded_calls_apply_on_conventions_in_order(bool useBuilder, bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new NavigationConvention(terminate: false);
            var convention2 = new NavigationConvention(terminate: true);
            var convention3 = new NavigationConvention(terminate: false);
            conventions.NavigationAddedConventions.Add(convention1);
            conventions.NavigationAddedConventions.Add(convention2);
            conventions.NavigationAddedConventions.Add(convention3);

            var builder = new InternalModelBuilder(new Model(conventions));
            var principalEntityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
            var dependentEntityBuilder = builder.Entity(typeof(OrderDetails), ConfigurationSource.Convention);

            var scope = useScope ? builder.Metadata.ConventionDispatcher.StartBatch() : null;

            if (useBuilder)
            {
                var result = dependentEntityBuilder.Relationship(principalEntityBuilder, OrderDetails.OrderProperty, Order.OrderDetailsProperty, ConfigurationSource.Convention);

                Assert.Equal(!useScope, result == null);
            }
            else
            {
                var fk = dependentEntityBuilder.Relationship(principalEntityBuilder, ConfigurationSource.Convention)
                    .IsUnique(true, ConfigurationSource.Convention)
                    .Metadata;
                var result = fk.HasDependentToPrincipal(OrderDetails.OrderProperty);

                Assert.Equal(!useScope, result == null);

                result = fk.HasPrincipalToDependent(Order.OrderDetailsProperty);

                Assert.Equal(!useScope, result == null);
            }

            if (useScope)
            {
                Assert.Empty(convention1.Calls);
                Assert.Empty(convention2.Calls);
                scope.Dispose();
            }

            Assert.Equal(new[] { nameof(OrderDetails.Order), nameof(Order.OrderDetails) }, convention1.Calls);
            Assert.Equal(new[] { nameof(OrderDetails.Order), nameof(Order.OrderDetails) }, convention2.Calls);
            Assert.Empty(convention3.Calls);
        }

        private class NavigationConvention : INavigationConvention
        {
            private readonly bool _terminate;
            public readonly List<object> Calls = new List<object>();

            public NavigationConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public InternalRelationshipBuilder Apply(
                InternalRelationshipBuilder relationshipBuilder, Navigation navigation)
            {
                Assert.NotNull(relationshipBuilder.Metadata.Builder);

                Calls.Add(navigation.Name);

                if (_terminate)
                {
                    if (navigation.IsDependentToPrincipal())
                    {
                        relationshipBuilder.Metadata.HasDependentToPrincipal((string)null);
                    }
                    else
                    {
                        relationshipBuilder.Metadata.HasPrincipalToDependent((string)null);
                    }
                    return null;
                }

                return relationshipBuilder;
            }
        }

        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [Theory]
        public void OnNavigationRemoved_calls_apply_on_conventions_in_order(bool useBuilder, bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new NavigationRemovedConvention(terminate: false);
            var convention2 = new NavigationRemovedConvention(terminate: true);
            var convention3 = new NavigationRemovedConvention(terminate: false);
            conventions.NavigationRemovedConventions.Add(convention1);
            conventions.NavigationRemovedConventions.Add(convention2);
            conventions.NavigationRemovedConventions.Add(convention3);

            var builder = new InternalModelBuilder(new Model(conventions));
            var principalEntityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
            var dependentEntityBuilder = builder.Entity(typeof(OrderDetails), ConfigurationSource.Convention);
            var relationshipBuilder = dependentEntityBuilder.Relationship(principalEntityBuilder, nameof(OrderDetails.Order), nameof(Order.OrderDetails), ConfigurationSource.Convention);

            var scope = useScope ? builder.Metadata.ConventionDispatcher.StartBatch() : null;

            if (useBuilder)
            {
                Assert.NotNull(relationshipBuilder.DependentToPrincipal((string)null, ConfigurationSource.Convention));
            }
            else
            {
                Assert.NotNull(relationshipBuilder.Metadata.HasDependentToPrincipal((string)null, ConfigurationSource.Convention));
            }

            if (useScope)
            {
                Assert.Empty(convention1.Calls);
                Assert.Empty(convention2.Calls);
                scope.Dispose();
            }

            Assert.Equal(new[] { nameof(OrderDetails.Order) }, convention1.Calls);
            Assert.Equal(new[] { nameof(OrderDetails.Order) }, convention2.Calls);
            Assert.Empty(convention3.Calls);

            if (useBuilder)
            {
                Assert.NotNull(relationshipBuilder.DependentToPrincipal((string)null, ConfigurationSource.Convention));
            }
            else
            {
                Assert.Null(relationshipBuilder.Metadata.HasDependentToPrincipal((string)null, ConfigurationSource.Convention));
            }

            Assert.Equal(new[] { nameof(OrderDetails.Order) }, convention1.Calls);
            Assert.Equal(new[] { nameof(OrderDetails.Order) }, convention2.Calls);
            Assert.Empty(convention3.Calls);
        }

        private class NavigationRemovedConvention : INavigationRemovedConvention
        {
            private readonly bool _terminate;
            public readonly List<object> Calls = new List<object>();

            public NavigationRemovedConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public bool Apply(
                InternalEntityTypeBuilder sourceEntityTypeBuilder, InternalEntityTypeBuilder targetEntityTypeBuilder,
                string navigationName, PropertyInfo propertyInfo)
            {
                Assert.NotNull(sourceEntityTypeBuilder.Metadata.Builder);

                Calls.Add(navigationName);

                return !_terminate;
            }
        }

        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [Theory]
        public void OnForeignKeyUniquenessChanged_calls_apply_on_conventions_in_order(bool useBuilder, bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new ForeignKeyUniquenessConvention(terminate: false);
            var convention2 = new ForeignKeyUniquenessConvention(terminate: true);
            var convention3 = new ForeignKeyUniquenessConvention(terminate: false);
            conventions.ForeignKeyUniquenessConventions.Add(convention1);
            conventions.ForeignKeyUniquenessConventions.Add(convention2);
            conventions.ForeignKeyUniquenessConventions.Add(convention3);

            var builder = new InternalModelBuilder(new Model(conventions));
            var principalEntityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
            var dependentEntityBuilder = builder.Entity(typeof(OrderDetails), ConfigurationSource.Convention);
            var foreignKey = dependentEntityBuilder.Relationship(principalEntityBuilder, ConfigurationSource.Convention).Metadata;

            var scope = useScope ? builder.Metadata.ConventionDispatcher.StartBatch() : null;

            if (useBuilder)
            {
                foreignKey.Builder.IsUnique(true, ConfigurationSource.Convention);
            }
            else
            {
                foreignKey.IsUnique = true;
            }

            if (useScope)
            {
                Assert.Empty(convention1.Calls);
                Assert.Empty(convention2.Calls);
                scope.Dispose();
            }

            Assert.Equal(new[] { true }, convention1.Calls);
            Assert.Equal(new[] { true }, convention2.Calls);
            Assert.Empty(convention3.Calls);

            if (useBuilder)
            {
                foreignKey.Builder.IsUnique(true, ConfigurationSource.Convention);
            }
            else
            {
                foreignKey.IsUnique = true;
            }

            Assert.Equal(new[] { true }, convention1.Calls);
            Assert.Equal(new[] { true }, convention2.Calls);
            Assert.Empty(convention3.Calls);

            if (useBuilder)
            {
                foreignKey.Builder.IsUnique(false, ConfigurationSource.Convention);
            }
            else
            {
                foreignKey.IsUnique = false;
            }

            Assert.Equal(new[] { true, false }, convention1.Calls);
            Assert.Equal(new[] { true, false }, convention2.Calls);
            Assert.Empty(convention3.Calls);

            Assert.Same(foreignKey,
                dependentEntityBuilder.Metadata.RemoveForeignKey(foreignKey.Properties, foreignKey.PrincipalKey, foreignKey.PrincipalEntityType));
        }

        private class ForeignKeyUniquenessConvention : IForeignKeyUniquenessConvention
        {
            private readonly bool _terminate;
            public readonly List<bool> Calls = new List<bool>();

            public ForeignKeyUniquenessConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public InternalRelationshipBuilder Apply(InternalRelationshipBuilder relationshipBuilder)
            {
                Assert.NotNull(relationshipBuilder.Metadata.Builder);

                Calls.Add(relationshipBuilder.Metadata.IsUnique);

                return _terminate ? null : relationshipBuilder;
            }
        }

        [InlineData(false)]
        [InlineData(true)]
        [Theory]
        public void OnPrincipalEndSet_calls_apply_on_conventions_in_order(bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new PrincipalEndConvention(terminate: false);
            var convention2 = new PrincipalEndConvention(terminate: true);
            var convention3 = new PrincipalEndConvention(terminate: false);
            conventions.PrincipalEndSetConventions.Add(convention1);
            conventions.PrincipalEndSetConventions.Add(convention2);
            //conventions.PrincipalEndSetConventions.Add(convention3);

            var builder = new InternalModelBuilder(new Model(conventions));
            var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
            entityBuilder.PrimaryKey(new[] { "OrderId" }, ConfigurationSource.Convention);
            var dependentEntityBuilder = builder.Entity(typeof(OrderDetails), ConfigurationSource.Convention);
            var relationship = dependentEntityBuilder
                .Relationship(entityBuilder, ConfigurationSource.Convention);

            var scope = useScope ? builder.Metadata.ConventionDispatcher.StartBatch() : null;

            relationship = relationship.HasPrincipalKey(new string[0], ConfigurationSource.Convention);
            Assert.NotNull(relationship);

            if (useScope)
            {
                Assert.Empty(convention1.Calls);
                Assert.Empty(convention2.Calls);
                scope.Dispose();
            }

            Assert.Equal(new[] { nameof(Order) }, convention1.Calls);
            Assert.Equal(new[] { nameof(Order) }, convention2.Calls);
            Assert.Empty(convention3.Calls);

            relationship = relationship.HasPrincipalKey(relationship.Metadata.PrincipalKey.Properties, ConfigurationSource.Convention);
            Assert.NotNull(relationship);

            Assert.Equal(new[] { nameof(Order) }, convention1.Calls);
            Assert.Equal(new[] { nameof(Order) }, convention2.Calls);
            Assert.Empty(convention3.Calls);

            scope = useScope ? builder.Metadata.ConventionDispatcher.StartBatch() : null;

            relationship = relationship.HasForeignKey(new string[0], ConfigurationSource.Convention);
            Assert.NotNull(relationship);

            if (useScope)
            {
                Assert.Equal(new[] { nameof(Order) }, convention1.Calls);
                Assert.Equal(new[] { nameof(Order) }, convention2.Calls);
                scope.Dispose();
            }

            Assert.Equal(new[] { nameof(Order), nameof(Order) }, convention1.Calls);
            Assert.Equal(new[] { nameof(Order), nameof(Order) }, convention2.Calls);
            Assert.Empty(convention3.Calls);

            relationship = relationship.HasForeignKey(relationship.Metadata.Properties, ConfigurationSource.Convention);
            Assert.NotNull(relationship);

            Assert.Equal(new[] { nameof(Order), nameof(Order) }, convention1.Calls);
            Assert.Equal(new[] { nameof(Order), nameof(Order) }, convention2.Calls);
            Assert.Empty(convention3.Calls);

            scope = useScope ? builder.Metadata.ConventionDispatcher.StartBatch() : null;

            relationship = relationship.RelatedEntityTypes(
                relationship.Metadata.DeclaringEntityType, relationship.Metadata.PrincipalEntityType, ConfigurationSource.Convention);
            Assert.NotNull(relationship);

            if (useScope)
            {
                Assert.Equal(new[] { nameof(Order), nameof(Order) }, convention1.Calls);
                Assert.Equal(new[] { nameof(Order), nameof(Order) }, convention2.Calls);
                scope.Dispose();
            }

            Assert.Equal(new[] { nameof(Order), nameof(Order), nameof(OrderDetails) }, convention1.Calls);
            Assert.Equal(new[] { nameof(Order), nameof(Order), nameof(OrderDetails) }, convention2.Calls);
            Assert.Empty(convention3.Calls);

            relationship = relationship.RelatedEntityTypes(
                relationship.Metadata.PrincipalEntityType, relationship.Metadata.DeclaringEntityType, ConfigurationSource.DataAnnotation);
            Assert.NotNull(relationship);

            Assert.Equal(new[] { nameof(Order), nameof(Order), nameof(OrderDetails), nameof(OrderDetails) }, convention1.Calls);
            Assert.Equal(new[] { nameof(Order), nameof(Order), nameof(OrderDetails), nameof(OrderDetails) }, convention2.Calls);
            Assert.Empty(convention3.Calls);

            relationship = relationship.RelatedEntityTypes(
                relationship.Metadata.PrincipalEntityType, relationship.Metadata.DeclaringEntityType, ConfigurationSource.DataAnnotation);
            Assert.NotNull(relationship);

            Assert.Equal(new[] { nameof(Order), nameof(Order), nameof(OrderDetails), nameof(OrderDetails) }, convention1.Calls);
            Assert.Equal(new[] { nameof(Order), nameof(Order), nameof(OrderDetails), nameof(OrderDetails) }, convention2.Calls);
            Assert.Empty(convention3.Calls);
        }

        private class PrincipalEndConvention : IPrincipalEndConvention
        {
            private readonly bool _terminate;
            public readonly List<object> Calls = new List<object>();

            public PrincipalEndConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public InternalRelationshipBuilder Apply(InternalRelationshipBuilder relationshipBuilder)
            {
                Assert.NotNull(relationshipBuilder.Metadata.Builder);

                Calls.Add(relationshipBuilder.Metadata.PrincipalEntityType.DisplayName());

                return relationshipBuilder;
            }
        }

        [InlineData(false)]
        [InlineData(true)]
        [Theory]
        public void InitializingModel_calls_apply_on_conventions_in_order(bool useBuilder)
        {
            var conventions = new ConventionSet();

            var convention1 = new ModelConvention(terminate: false);
            var convention2 = new ModelConvention(terminate: true);
            var convention3 = new ModelConvention(terminate: false);
            conventions.ModelInitializedConventions.Add(convention1);
            conventions.ModelInitializedConventions.Add(convention2);
            conventions.ModelInitializedConventions.Add(convention3);

            if (useBuilder)
            {
                Assert.NotNull(new ModelBuilder(conventions));
            }
            else
            {
                Assert.NotNull(new Model(conventions));
            }

            Assert.Equal(1, convention1.Calls);
            Assert.Equal(1, convention2.Calls);
            Assert.Equal(0, convention3.Calls);
        }

        private class ModelConvention : IModelConvention
        {
            private readonly bool _terminate;
            public int Calls;

            public ModelConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public InternalModelBuilder Apply(InternalModelBuilder modelBuilder)
            {
                Assert.NotNull(modelBuilder.Metadata.Builder);

                Calls++;

                return _terminate ? null : modelBuilder;
            }
        }

        [InlineData(false)]
        [InlineData(true)]
        [Theory]
        public void ValidatingModel_calls_apply_on_conventions_in_order(bool useBuilder)
        {
            var conventions = new ConventionSet();

            var convention1 = new ModelConvention(terminate: false);
            var convention2 = new ModelConvention(terminate: true);
            var convention3 = new ModelConvention(terminate: false);
            conventions.ModelBuiltConventions.Add(convention1);
            conventions.ModelBuiltConventions.Add(convention2);
            conventions.ModelBuiltConventions.Add(convention3);

            var model = new Model(conventions);

            if (useBuilder)
            {
                Assert.Null(new InternalModelBuilder(model).Validate());
            }
            else
            {
                Assert.Null(model.Validate());
            }

            Assert.Equal(1, convention1.Calls);
            Assert.Equal(1, convention2.Calls);
            Assert.Equal(0, convention3.Calls);
        }

        private class Order
        {
            public static readonly PropertyInfo OrderIdProperty = typeof(Order).GetProperty(nameof(OrderId));
            public static readonly PropertyInfo OrderDetailsProperty = typeof(Order).GetProperty(nameof(OrderDetails));

            public readonly int IntField = 1;

            public int OrderId { get; set; }

            public string Name { get; set; }

            public virtual OrderDetails OrderDetails { get; set; }
        }

        private class SpecialOrder : Order
        {
        }

        private class OrderDetails
        {
            public static readonly PropertyInfo OrderProperty = typeof(OrderDetails).GetProperty(nameof(Order));

            public int Id { get; set; }
            public virtual Order Order { get; set; }
        }
    }
}
