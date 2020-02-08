// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    public class ConventionDispatcherTest
    {
        [InlineData(false)]
        [InlineData(true)]
        [ConditionalTheory]
        public void OnModelInitialized_calls_conventions_in_order(bool useBuilder)
        {
            var conventions = new ConventionSet();

            var convention1 = new ModelInitializedConvention(terminate: false);
            var convention2 = new ModelInitializedConvention(terminate: true);
            var convention3 = new ModelInitializedConvention(terminate: false);
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

        private class ModelInitializedConvention : IModelInitializedConvention
        {
            private readonly bool _terminate;
            public int Calls;

            public ModelInitializedConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public void ProcessModelInitialized(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
            {
                Assert.NotNull(modelBuilder.Metadata.Builder);

                Calls++;

                if (_terminate)
                {
                    context.StopProcessing();
                }
            }
        }

        [InlineData(false)]
        [InlineData(true)]
        [ConditionalTheory]
        public void OnModelFinalized_calls_conventions_in_order(bool useBuilder)
        {
            var conventions = new ConventionSet();

            var convention1 = new ModelFinalizedConvention(terminate: false);
            var convention2 = new ModelFinalizedConvention(terminate: true);
            var convention3 = new ModelFinalizedConvention(terminate: false);
            conventions.ModelFinalizedConventions.Add(convention1);
            conventions.ModelFinalizedConventions.Add(convention2);
            conventions.ModelFinalizedConventions.Add(convention3);

            var model = new Model(conventions);

            if (useBuilder)
            {
                Assert.Same(model, new InternalModelBuilder(model).Metadata.FinalizeModel());
            }
            else
            {
                Assert.Same(model, model.FinalizeModel());
            }

            Assert.Equal(1, convention1.Calls);
            Assert.Equal(1, convention2.Calls);
            Assert.Equal(0, convention3.Calls);
        }

        private class ModelFinalizedConvention : IModelFinalizedConvention
        {
            private readonly bool _terminate;
            public int Calls;

            public ModelFinalizedConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public void ProcessModelFinalized(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
            {
                Assert.NotNull(modelBuilder.Metadata.Builder);

                Calls++;

                if (_terminate)
                {
                    context.StopProcessing();
                }
            }
        }

        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [ConditionalTheory]
        public void OnModelAnnotationChanged_calls_conventions_in_order(bool useBuilder, bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new ModelAnnotationChangedConvention(false);
            var convention2 = new ModelAnnotationChangedConvention(true);
            var convention3 = new ModelAnnotationChangedConvention(false);
            conventions.ModelAnnotationChangedConventions.Add(convention1);
            conventions.ModelAnnotationChangedConventions.Add(convention2);
            conventions.ModelAnnotationChangedConventions.Add(convention3);

            var builder = new InternalModelBuilder(new Model(conventions));

            var scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

            if (useBuilder)
            {
                Assert.NotNull(builder.HasAnnotation("foo", "bar", ConfigurationSource.Convention));
            }
            else
            {
                builder.Metadata["foo"] = "bar";
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
                Assert.NotNull(builder.HasAnnotation("foo", "bar", ConfigurationSource.Convention));
            }
            else
            {
                builder.Metadata["foo"] = "bar";
            }

            Assert.Equal(new[] { "bar" }, convention1.Calls);
            Assert.Equal(new[] { "bar" }, convention2.Calls);
            Assert.Empty(convention3.Calls);

            if (useBuilder)
            {
                Assert.NotNull(builder.HasAnnotation("foo", null, ConfigurationSource.Convention));
            }
            else
            {
                builder.Metadata.RemoveAnnotation("foo");
            }

            Assert.Equal(new[] { "bar", null }, convention1.Calls);
            Assert.Equal(new[] { "bar", null }, convention2.Calls);
            Assert.Empty(convention3.Calls);

            builder.Metadata[CoreAnnotationNames.ProductVersion] = "bar";
            Assert.Equal(new[] { "bar", null }, convention1.Calls);
        }

        private class ModelAnnotationChangedConvention : IModelAnnotationChangedConvention
        {
            private readonly bool _terminate;
            public readonly List<object> Calls = new List<object>();

            public ModelAnnotationChangedConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public void ProcessModelAnnotationChanged(
                IConventionModelBuilder propertyBuilder,
                string name,
                IConventionAnnotation annotation,
                IConventionAnnotation oldAnnotation,
                IConventionContext<IConventionAnnotation> context)
            {
                Assert.NotNull(propertyBuilder.Metadata.Builder);

                Calls.Add(annotation?.Value);

                if (_terminate)
                {
                    context.StopProcessing();
                }
            }
        }

        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [ConditionalTheory]
        public void OnEntityTypeAdded_calls_conventions_in_order(bool useBuilder, bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new EntityTypeAddedConvention(terminate: false, onlyWeak: false);
            var convention2 = new EntityTypeAddedConvention(terminate: true, onlyWeak: false);
            var convention3 = new EntityTypeAddedConvention(terminate: false, onlyWeak: false);
            conventions.EntityTypeAddedConventions.Add(convention1);
            conventions.EntityTypeAddedConventions.Add(convention2);
            conventions.EntityTypeAddedConventions.Add(convention3);

            var builder = new InternalModelBuilder(new Model(conventions));

            var scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

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
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [ConditionalTheory]
        public void OnEntityTypeAdded_calls_conventions_in_order_for_weak_entity_types(bool useBuilder, bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new EntityTypeAddedConvention(terminate: false, onlyWeak: true);
            var convention2 = new EntityTypeAddedConvention(terminate: true, onlyWeak: true);
            var convention3 = new EntityTypeAddedConvention(terminate: false, onlyWeak: true);
            conventions.EntityTypeAddedConventions.Add(convention1);
            conventions.EntityTypeAddedConventions.Add(convention2);
            conventions.EntityTypeAddedConventions.Add(convention3);

            var builder = new InternalModelBuilder(new Model(conventions));
            var owner = builder.Entity(typeof(Order), ConfigurationSource.Explicit);

            var scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

            if (useBuilder)
            {
                // Add another owned type to trigger making them weak
                owner.HasOwnership(typeof(OrderDetails), nameof(Order.OtherOrderDetails), ConfigurationSource.Convention);
                var result = owner.HasOwnership(typeof(OrderDetails), nameof(Order.OrderDetails), ConfigurationSource.Convention);

                Assert.Equal(!useScope, result == null);
            }
            else
            {
                var result = builder.Metadata.AddEntityType(
                    typeof(OrderDetails), nameof(Order.OrderDetails), owner.Metadata, ConfigurationSource.Convention);

                Assert.Equal(!useScope, result == null);
            }

            if (useScope)
            {
                Assert.Equal(0, convention1.Calls);
                Assert.Equal(0, convention2.Calls);
                scope.Dispose();
            }

            Assert.Equal(useBuilder ? 2 : 1, convention1.Calls);
            Assert.Equal(useBuilder ? 2 : 1, convention2.Calls);
            Assert.Equal(0, convention3.Calls);

            Assert.Empty(builder.Metadata.GetEntityTypes().Where(e => e.HasDefiningNavigation()));
            Assert.Null(builder.Metadata.FindEntityType(typeof(OrderDetails)));
        }

        private class EntityTypeAddedConvention : IEntityTypeAddedConvention
        {
            private readonly bool _terminate;
            private readonly bool _onlyWeak;
            public int Calls;

            public EntityTypeAddedConvention(bool terminate, bool onlyWeak)
            {
                _terminate = terminate;
                _onlyWeak = onlyWeak;
            }

            public void ProcessEntityTypeAdded(
                IConventionEntityTypeBuilder entityTypeBuilder, IConventionContext<IConventionEntityTypeBuilder> context)
            {
                Assert.Same(entityTypeBuilder, entityTypeBuilder.Metadata.Builder);
                if (entityTypeBuilder.Metadata.HasDefiningNavigation() == _onlyWeak)
                {
                    Calls++;
                }

                if (_terminate)
                {
                    if (entityTypeBuilder.Metadata.HasDefiningNavigation())
                    {
                        if (_onlyWeak)
                        {
                            entityTypeBuilder.ModelBuilder.HasNoEntityType(entityTypeBuilder.Metadata);
                            context.StopProcessing();
                        }
                    }
                    else
                    {
                        if (!_onlyWeak)
                        {
                            entityTypeBuilder.Metadata.Model.RemoveEntityType(entityTypeBuilder.Metadata.Name);
                            context.StopProcessing();
                        }
                    }
                }
            }
        }

        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [ConditionalTheory]
        public void OnEntityTypeIgnored_calls_conventions_in_order(bool useBuilder, bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new EntityTypeIgnoredConvention(terminate: false);
            var convention2 = new EntityTypeIgnoredConvention(terminate: true);
            var convention3 = new EntityTypeIgnoredConvention(terminate: false);
            conventions.EntityTypeIgnoredConventions.Add(convention1);
            conventions.EntityTypeIgnoredConventions.Add(convention2);
            conventions.EntityTypeIgnoredConventions.Add(convention3);

            var convention4 = new EntityTypeRemovedConvention(terminate: false);
            var convention5 = new EntityTypeRemovedConvention(terminate: true);
            var convention6 = new EntityTypeRemovedConvention(terminate: false);
            conventions.EntityTypeRemovedConventions.Add(convention4);
            conventions.EntityTypeRemovedConventions.Add(convention5);
            conventions.EntityTypeRemovedConventions.Add(convention6);

            var builder = new InternalModelBuilder(new Model(conventions));

            var scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

            builder.Entity(typeof(Order), ConfigurationSource.Convention);
            if (useBuilder)
            {
                Assert.NotNull(builder.Ignore(typeof(Order).DisplayName(), ConfigurationSource.Convention));
            }
            else
            {
                builder.Metadata.RemoveEntityType(typeof(Order));
                builder.Metadata.AddIgnored(typeof(Order), ConfigurationSource.Convention);
            }

            if (useScope)
            {
                Assert.Equal(0, convention1.Calls);
                Assert.Equal(0, convention2.Calls);
                Assert.Equal(0, convention3.Calls);
                Assert.Equal(0, convention4.Calls);
                Assert.Equal(0, convention5.Calls);
                Assert.Equal(0, convention6.Calls);
                scope.Dispose();
            }

            Assert.Equal(1, convention1.Calls);
            Assert.Equal(1, convention2.Calls);
            Assert.Equal(0, convention3.Calls);
            Assert.Equal(1, convention4.Calls);
            Assert.Equal(1, convention5.Calls);
            Assert.Equal(0, convention6.Calls);
        }

        private class EntityTypeIgnoredConvention : IEntityTypeIgnoredConvention
        {
            private readonly bool _terminate;
            public int Calls;

            public EntityTypeIgnoredConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public void ProcessEntityTypeIgnored(
                IConventionModelBuilder modelBuilder, string name, Type type, IConventionContext<string> context)
            {
                Assert.Null(modelBuilder.Metadata.FindEntityType(name));
                Calls++;

                if (_terminate)
                {
                    context.StopProcessing();
                }
            }
        }

        private class EntityTypeRemovedConvention : IEntityTypeRemovedConvention
        {
            private readonly bool _terminate;
            public int Calls;

            public EntityTypeRemovedConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public void ProcessEntityTypeRemoved(
                IConventionModelBuilder modelBuilder, IConventionEntityType entityType, IConventionContext<IConventionEntityType> context)
            {
                Assert.Null(modelBuilder.Metadata.FindEntityType(entityType.Name));
                Calls++;

                if (_terminate)
                {
                    context.StopProcessing();
                }
            }
        }

        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [ConditionalTheory]
        public void OnEntityTypeMemberIgnored_calls_conventions_in_order(bool useBuilder, bool useScope)
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

            var scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

            if (useBuilder)
            {
                Assert.NotNull(entityBuilder.Ignore("A", ConfigurationSource.Convention));
            }
            else
            {
                entityBuilder.Metadata.AddIgnored("A", ConfigurationSource.Convention);
            }

            if (useScope)
            {
                Assert.Empty(convention1.Calls);
                Assert.Empty(convention2.Calls);
                Assert.Empty(convention3.Calls);
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
                entityBuilder.Metadata.AddIgnored("A", ConfigurationSource.Convention);
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

            public void ProcessEntityTypeMemberIgnored(
                IConventionEntityTypeBuilder entityTypeBuilder, string name, IConventionContext<string> context)
            {
                Assert.NotNull(entityTypeBuilder.Metadata.Builder);

                Calls.Add(name);

                if (_terminate)
                {
                    context.StopProcessing();
                }
            }
        }

        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [ConditionalTheory]
        public void OnBaseTypeChanged_calls_conventions_in_order(bool useBuilder, bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new EntityTypeBaseTypeChangedConvention(terminate: false);
            var convention2 = new EntityTypeBaseTypeChangedConvention(terminate: true);
            var convention3 = new EntityTypeBaseTypeChangedConvention(terminate: false);
            conventions.EntityTypeBaseTypeChangedConventions.Add(convention1);
            conventions.EntityTypeBaseTypeChangedConventions.Add(convention2);
            conventions.EntityTypeBaseTypeChangedConventions.Add(convention3);

            var builder = new InternalModelBuilder(new Model(conventions))
                .Entity(typeof(SpecialOrder), ConfigurationSource.Convention);

            var scope = useScope ? builder.Metadata.Model.ConventionDispatcher.DelayConventions() : null;

            if (useBuilder)
            {
                Assert.NotNull(builder.HasBaseType(typeof(Order), ConfigurationSource.Convention));
            }
            else
            {
                builder.Metadata.HasBaseType(
                    builder.Metadata.Model.AddEntityType(typeof(Order), ConfigurationSource.Explicit), ConfigurationSource.Convention);
            }

            if (useScope)
            {
                Assert.Empty(convention1.Calls);
                Assert.Empty(convention2.Calls);
                Assert.Empty(convention3.Calls);
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

        private class EntityTypeBaseTypeChangedConvention : IEntityTypeBaseTypeChangedConvention
        {
            private readonly bool _terminate;
            public readonly List<Type> Calls = new List<Type>();

            public EntityTypeBaseTypeChangedConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public void ProcessEntityTypeBaseTypeChanged(
                IConventionEntityTypeBuilder entityTypeBuilder, IConventionEntityType newBaseType, IConventionEntityType oldBaseType,
                IConventionContext<IConventionEntityType> context)
            {
                Assert.NotNull(entityTypeBuilder.Metadata.Builder);

                Calls.Add(entityTypeBuilder.Metadata.BaseType?.ClrType);

                if (_terminate)
                {
                    context.StopProcessing();
                }
            }
        }

        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [ConditionalTheory]
        public void OnPrimaryKeyChanged_calls_conventions_in_order(bool useBuilder, bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new EntityTypePrimaryKeyChangedConvention(terminate: false);
            var convention2 = new EntityTypePrimaryKeyChangedConvention(terminate: true);
            var convention3 = new EntityTypePrimaryKeyChangedConvention(terminate: false);
            conventions.EntityTypePrimaryKeyChangedConventions.Add(convention1);
            conventions.EntityTypePrimaryKeyChangedConventions.Add(convention2);
            conventions.EntityTypePrimaryKeyChangedConventions.Add(convention3);

            var builder = new InternalModelBuilder(new Model(conventions));
            var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);

            entityBuilder.HasKey(new[] { "OrderId" }, ConfigurationSource.Convention);

            var scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

            if (useBuilder)
            {
                Assert.NotNull(entityBuilder.PrimaryKey(new[] { "OrderId" }, ConfigurationSource.Convention));
            }
            else
            {
                Assert.NotNull(
                    entityBuilder.Metadata.SetPrimaryKey(
                        entityBuilder.Property("OrderId", ConfigurationSource.Convention).Metadata));
            }

            if (useScope)
            {
                Assert.Empty(convention1.Calls);
                Assert.Empty(convention2.Calls);
                Assert.Empty(convention3.Calls);
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
                Assert.NotNull(
                    entityBuilder.Metadata.SetPrimaryKey(
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

        private class EntityTypePrimaryKeyChangedConvention : IEntityTypePrimaryKeyChangedConvention
        {
            private readonly bool _terminate;
            public readonly List<object> Calls = new List<object>();

            public EntityTypePrimaryKeyChangedConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public void ProcessEntityTypePrimaryKeyChanged(
                IConventionEntityTypeBuilder entityTypeBuilder,
                IConventionKey newPrimaryKey,
                IConventionKey previousPrimaryKey,
                IConventionContext<IConventionKey> context)
            {
                Assert.NotNull(entityTypeBuilder.Metadata.Builder);

                Calls.Add(previousPrimaryKey?.Properties.First().Name);

                if (_terminate)
                {
                    context.StopProcessing(newPrimaryKey);
                }
            }
        }

        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [ConditionalTheory]
        public void OnEntityTypeAnnotationChanged_calls_conventions_in_order(bool useBuilder, bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new EntityTypeAnnotationChangedConvention(terminate: false);
            var convention2 = new EntityTypeAnnotationChangedConvention(terminate: true);
            var convention3 = new EntityTypeAnnotationChangedConvention(terminate: false);
            conventions.EntityTypeAnnotationChangedConventions.Add(convention1);
            conventions.EntityTypeAnnotationChangedConventions.Add(convention2);
            conventions.EntityTypeAnnotationChangedConventions.Add(convention3);

            var builder = new InternalModelBuilder(new Model(conventions));
            var entityBuilder = builder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention);

            var scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

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
            Assert.Empty(convention3.Calls);

            entityBuilder.Metadata[CoreAnnotationNames.PropertyAccessMode] = PropertyAccessMode.Field;
            Assert.Equal(new[] { "bar", null }, convention1.Calls);
        }

        private class EntityTypeAnnotationChangedConvention : IEntityTypeAnnotationChangedConvention
        {
            private readonly bool _terminate;
            public readonly List<object> Calls = new List<object>();

            public EntityTypeAnnotationChangedConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public void ProcessEntityTypeAnnotationChanged(
                IConventionEntityTypeBuilder entityTypeBuilder,
                string name,
                IConventionAnnotation annotation,
                IConventionAnnotation oldAnnotation,
                IConventionContext<IConventionAnnotation> context)
            {
                Calls.Add(annotation?.Value);

                if (_terminate)
                {
                    context.StopProcessing();
                }
            }
        }

        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [ConditionalTheory]
        public void OnForeignKeyAdded_calls_conventions_in_order(bool useBuilder, bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new ForeignKeyAddedConvention(terminate: false);
            var convention2 = new ForeignKeyAddedConvention(terminate: true);
            var convention3 = new ForeignKeyAddedConvention(terminate: false);
            conventions.ForeignKeyAddedConventions.Add(convention1);
            conventions.ForeignKeyAddedConventions.Add(convention2);
            conventions.ForeignKeyAddedConventions.Add(convention3);

            var builder = new InternalModelBuilder(new Model(conventions));
            var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
            entityBuilder.PrimaryKey(new[] { "OrderId" }, ConfigurationSource.Convention);

            var scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

            if (useBuilder)
            {
                var result = entityBuilder.HasRelationship(entityBuilder.Metadata, ConfigurationSource.Convention);

                Assert.Equal(!useScope, result == null);
            }
            else
            {
                var result = entityBuilder.Metadata.AddForeignKey(
                    entityBuilder.Property(typeof(int), "OrderId1", ConfigurationSource.Convention).Metadata,
                    entityBuilder.Metadata.FindPrimaryKey(),
                    entityBuilder.Metadata,
                    ConfigurationSource.Convention,
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

        private class ForeignKeyAddedConvention : IForeignKeyAddedConvention
        {
            private readonly bool _terminate;
            public readonly List<object> Calls = new List<object>();

            public ForeignKeyAddedConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public void ProcessForeignKeyAdded(
                IConventionRelationshipBuilder relationshipBuilder, IConventionContext<IConventionRelationshipBuilder> context)
            {
                var fk = relationshipBuilder.Metadata;
                Assert.NotNull(fk.Builder);

                Calls.Add(fk.Properties.First().Name);

                if (_terminate)
                {
                    fk.DeclaringEntityType.RemoveForeignKey(fk.Properties, fk.PrincipalKey, fk.PrincipalEntityType);
                    context.StopProcessing();
                }
            }
        }

        [InlineData(false)]
        [InlineData(true)]
        [ConditionalTheory]
        public void OnForeignKeyRemoved_calls_conventions_in_order(bool useScope)
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
                new[] { entityBuilder.Property(typeof(int), "FK", ConfigurationSource.Convention).Metadata },
                entityBuilder.HasKey(new[] { "OrderId" }, ConfigurationSource.Convention).Metadata,
                entityBuilder.Metadata,
                ConfigurationSource.Explicit,
                ConfigurationSource.Explicit);

            var scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

            var result = entityBuilder.Metadata.RemoveForeignKey(
                foreignKey.Properties, foreignKey.PrincipalKey, foreignKey.PrincipalEntityType);

            if (useScope)
            {
                Assert.Same(foreignKey, result);
            }
            else
            {
                Assert.Null(result);
            }

            if (useScope)
            {
                Assert.Empty(convention1.Calls);
                Assert.Empty(convention2.Calls);
                scope.Dispose();
            }

            Assert.Equal(new[] { "FK" }, convention1.Calls);
            Assert.Equal(new[] { "FK" }, convention2.Calls);
            Assert.Empty(convention3.Calls);
        }

        private class ForeignKeyRemovedConvention : IForeignKeyRemovedConvention
        {
            private readonly bool _terminate;
            public readonly List<object> Calls = new List<object>();

            public ForeignKeyRemovedConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public void ProcessForeignKeyRemoved(
                IConventionEntityTypeBuilder entityTypeBuilder,
                IConventionForeignKey foreignKey,
                IConventionContext<IConventionForeignKey> context)
            {
                Assert.NotNull(entityTypeBuilder.Metadata.Builder);

                Calls.Add(foreignKey.Properties.First().Name);

                if (_terminate)
                {
                    context.StopProcessing();
                }
            }
        }

        [InlineData(false)]
        [InlineData(true)]
        [ConditionalTheory]
        public void OnForeignKeyPrincipalEndChanged_calls_conventions_in_order(bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new ForeignKeyPrincipalEndChangedConvention(terminate: false);
            var convention2 = new ForeignKeyPrincipalEndChangedConvention(terminate: true);
            var convention3 = new ForeignKeyPrincipalEndChangedConvention(terminate: false);
            conventions.ForeignKeyPrincipalEndChangedConventions.Add(convention1);
            conventions.ForeignKeyPrincipalEndChangedConventions.Add(convention2);
            conventions.ForeignKeyPrincipalEndChangedConventions.Add(convention3);

            var builder = new InternalModelBuilder(new Model(conventions));
            var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
            entityBuilder.PrimaryKey(new[] { "OrderId" }, ConfigurationSource.Convention);
            var dependentEntityBuilder = builder.Entity(typeof(OrderDetails), ConfigurationSource.Convention);
            var relationship = dependentEntityBuilder
                .HasRelationship(entityBuilder.Metadata, ConfigurationSource.Convention);

            var scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

            relationship = relationship.HasPrincipalKey(Array.Empty<string>(), ConfigurationSource.Convention);
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

            scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

            relationship.Metadata.SetPrincipalEndConfigurationSource(null);
            relationship = relationship.HasForeignKey(Array.Empty<string>(), ConfigurationSource.Convention);
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

            scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

            relationship = relationship.HasEntityTypes(
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

            relationship = relationship.HasEntityTypes(
                relationship.Metadata.PrincipalEntityType, relationship.Metadata.DeclaringEntityType, ConfigurationSource.DataAnnotation);
            Assert.NotNull(relationship);

            Assert.Equal(new[] { nameof(Order), nameof(Order), nameof(OrderDetails), nameof(OrderDetails) }, convention1.Calls);
            Assert.Equal(new[] { nameof(Order), nameof(Order), nameof(OrderDetails), nameof(OrderDetails) }, convention2.Calls);
            Assert.Empty(convention3.Calls);

            relationship = relationship.HasEntityTypes(
                relationship.Metadata.PrincipalEntityType, relationship.Metadata.DeclaringEntityType, ConfigurationSource.DataAnnotation);
            Assert.NotNull(relationship);

            Assert.Equal(new[] { nameof(Order), nameof(Order), nameof(OrderDetails), nameof(OrderDetails) }, convention1.Calls);
            Assert.Equal(new[] { nameof(Order), nameof(Order), nameof(OrderDetails), nameof(OrderDetails) }, convention2.Calls);
            Assert.Empty(convention3.Calls);
        }

        private class ForeignKeyPrincipalEndChangedConvention : IForeignKeyPrincipalEndChangedConvention
        {
            private readonly bool _terminate;
            public readonly List<object> Calls = new List<object>();

            public ForeignKeyPrincipalEndChangedConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public void ProcessForeignKeyPrincipalEndChanged(
                IConventionRelationshipBuilder relationshipBuilder, IConventionContext<IConventionRelationshipBuilder> context)
            {
                Assert.NotNull(relationshipBuilder.Metadata.Builder);

                Calls.Add(relationshipBuilder.Metadata.PrincipalEntityType.DisplayName());

                if (_terminate)
                {
                    context.StopProcessing(relationshipBuilder);
                }
            }
        }

        [InlineData(false)]
        [InlineData(true)]
        [ConditionalTheory]
        public void OnForeignKeyPropertiesChangedConvention_calls_conventions_in_order(bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new ForeignKeyPropertiesChangedConvention(terminate: false);
            var convention2 = new ForeignKeyPropertiesChangedConvention(terminate: true);
            var convention3 = new ForeignKeyPropertiesChangedConvention(terminate: false);
            conventions.ForeignKeyPropertiesChangedConventions.Add(convention1);
            conventions.ForeignKeyPropertiesChangedConventions.Add(convention2);
            conventions.ForeignKeyPropertiesChangedConventions.Add(convention3);

            var builder = new InternalModelBuilder(new Model(conventions));
            var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
            var foreignKey = entityBuilder.Metadata.AddForeignKey(
                new[] { entityBuilder.Property(typeof(int), "FK", ConfigurationSource.Convention).Metadata },
                entityBuilder.HasKey(new[] { "OrderId" }, ConfigurationSource.Convention).Metadata,
                entityBuilder.Metadata,
                ConfigurationSource.Explicit,
                ConfigurationSource.Explicit);

            var scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

            foreignKey.SetProperties(
                new[] { entityBuilder.Property(typeof(int), "FK2", ConfigurationSource.Convention).Metadata },
                foreignKey.PrincipalKey,
                ConfigurationSource.Convention);

            if (useScope)
            {
                Assert.Empty(convention1.Calls);
                Assert.Empty(convention2.Calls);
                Assert.Empty(convention3.Calls);
                scope.Dispose();
            }

            Assert.Equal(new[] { "FK" }, convention1.Calls);
            Assert.Equal(new[] { "FK" }, convention2.Calls);
            Assert.Empty(convention3.Calls);
        }

        private class ForeignKeyPropertiesChangedConvention : IForeignKeyPropertiesChangedConvention
        {
            private readonly bool _terminate;
            public readonly List<object> Calls = new List<object>();

            public ForeignKeyPropertiesChangedConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public void ProcessForeignKeyPropertiesChanged(
                IConventionRelationshipBuilder relationshipBuilder,
                IReadOnlyList<IConventionProperty> oldDependentProperties,
                IConventionKey oldPrincipalKey, IConventionContext<IConventionRelationshipBuilder> context)
            {
                Assert.NotNull(relationshipBuilder.Metadata.Builder);
                Assert.NotNull(oldDependentProperties);
                Assert.NotNull(oldPrincipalKey);

                Calls.Add(oldDependentProperties.First().Name);

                if (_terminate)
                {
                    context.StopProcessing();
                }
            }
        }

        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [ConditionalTheory]
        public void OnForeignKeyUniquenessChanged_calls_conventions_in_order(bool useBuilder, bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new ForeignKeyUniquenessChangedConvention(terminate: false);
            var convention2 = new ForeignKeyUniquenessChangedConvention(terminate: true);
            var convention3 = new ForeignKeyUniquenessChangedConvention(terminate: false);
            conventions.ForeignKeyUniquenessChangedConventions.Add(convention1);
            conventions.ForeignKeyUniquenessChangedConventions.Add(convention2);
            conventions.ForeignKeyUniquenessChangedConventions.Add(convention3);

            var builder = new InternalModelBuilder(new Model(conventions));
            var principalEntityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
            var dependentEntityBuilder = builder.Entity(typeof(OrderDetails), ConfigurationSource.Convention);
            var foreignKey = dependentEntityBuilder.HasRelationship(principalEntityBuilder.Metadata, ConfigurationSource.Convention)
                .Metadata;

            var scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

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

            Assert.Same(
                foreignKey,
                dependentEntityBuilder.Metadata.RemoveForeignKey(
                    foreignKey.Properties, foreignKey.PrincipalKey, foreignKey.PrincipalEntityType));
        }

        private class ForeignKeyUniquenessChangedConvention : IForeignKeyUniquenessChangedConvention
        {
            private readonly bool _terminate;
            public readonly List<bool> Calls = new List<bool>();

            public ForeignKeyUniquenessChangedConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public void ProcessForeignKeyUniquenessChanged(
                IConventionRelationshipBuilder relationshipBuilder, IConventionContext<IConventionRelationshipBuilder> context)
            {
                Assert.NotNull(relationshipBuilder.Metadata.Builder);

                Calls.Add(relationshipBuilder.Metadata.IsUnique);

                if (_terminate)
                {
                    context.StopProcessing();
                }
            }
        }

        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [ConditionalTheory]
        public void OnForeignKeyRequirednessChanged_calls_conventions_in_order(bool useBuilder, bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new ForeignKeyRequirednessChangedConvention(terminate: false);
            var convention2 = new ForeignKeyRequirednessChangedConvention(terminate: true);
            var convention3 = new ForeignKeyRequirednessChangedConvention(terminate: false);
            conventions.ForeignKeyRequirednessChangedConventions.Add(convention1);
            conventions.ForeignKeyRequirednessChangedConventions.Add(convention2);
            conventions.ForeignKeyRequirednessChangedConventions.Add(convention3);

            var builder = new InternalModelBuilder(new Model(conventions));
            var principalEntityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
            var dependentEntityBuilder = builder.Entity(typeof(OrderDetails), ConfigurationSource.Convention);
            var foreignKey = dependentEntityBuilder.HasRelationship(principalEntityBuilder.Metadata, ConfigurationSource.Convention)
                .Metadata;

            var scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

            if (useBuilder)
            {
                foreignKey.Builder.IsRequired(true, ConfigurationSource.Convention);
            }
            else
            {
                foreignKey.IsRequired = true;
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
                foreignKey.Builder.IsRequired(true, ConfigurationSource.Convention);
            }
            else
            {
                foreignKey.IsRequired = true;
            }

            Assert.Equal(new[] { true }, convention1.Calls);
            Assert.Equal(new[] { true }, convention2.Calls);
            Assert.Empty(convention3.Calls);

            if (useBuilder)
            {
                foreignKey.Builder.IsRequired(false, ConfigurationSource.Convention);
            }
            else
            {
                foreignKey.IsRequired = false;
            }

            Assert.Equal(new[] { true, false }, convention1.Calls);
            Assert.Equal(new[] { true, false }, convention2.Calls);
            Assert.Empty(convention3.Calls);

            Assert.Same(
                foreignKey,
                dependentEntityBuilder.Metadata.RemoveForeignKey(
                    foreignKey.Properties, foreignKey.PrincipalKey, foreignKey.PrincipalEntityType));
        }

        private class ForeignKeyRequirednessChangedConvention : IForeignKeyRequirednessChangedConvention
        {
            private readonly bool _terminate;
            public readonly List<bool> Calls = new List<bool>();

            public ForeignKeyRequirednessChangedConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public void ProcessForeignKeyRequirednessChanged(
                IConventionRelationshipBuilder relationshipBuilder, IConventionContext<IConventionRelationshipBuilder> context)
            {
                Assert.NotNull(relationshipBuilder.Metadata.Builder);

                Calls.Add(relationshipBuilder.Metadata.IsRequired);

                if (_terminate)
                {
                    context.StopProcessing();
                }
            }
        }

        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [ConditionalTheory]
        public void OnForeignKeyOwnershipChanged_calls_conventions_in_order(bool useBuilder, bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new ForeignKeyOwnershipChangedConvention(terminate: false);
            var convention2 = new ForeignKeyOwnershipChangedConvention(terminate: true);
            var convention3 = new ForeignKeyOwnershipChangedConvention(terminate: false);
            conventions.ForeignKeyOwnershipChangedConventions.Add(convention1);
            conventions.ForeignKeyOwnershipChangedConventions.Add(convention2);
            conventions.ForeignKeyOwnershipChangedConventions.Add(convention3);

            var builder = new InternalModelBuilder(new Model(conventions));
            var principalEntityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
            var dependentEntityBuilder = builder.Entity(typeof(OrderDetails), ConfigurationSource.Convention);
            var foreignKey = dependentEntityBuilder.HasRelationship(principalEntityBuilder.Metadata, ConfigurationSource.Convention)
                .Metadata;

            var scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

            if (useBuilder)
            {
                foreignKey.Builder.IsOwnership(true, ConfigurationSource.Convention);
            }
            else
            {
                foreignKey.IsOwnership = true;
            }

            if (useScope)
            {
                Assert.Empty(convention1.Calls);
                Assert.Empty(convention2.Calls);
                Assert.Empty(convention3.Calls);
                scope.Dispose();
            }

            Assert.Equal(new[] { true }, convention1.Calls);
            Assert.Equal(new[] { true }, convention2.Calls);
            Assert.Empty(convention3.Calls);

            if (useBuilder)
            {
                foreignKey.Builder.IsOwnership(true, ConfigurationSource.Convention);
            }
            else
            {
                foreignKey.IsOwnership = true;
            }

            Assert.Equal(new[] { true }, convention1.Calls);
            Assert.Equal(new[] { true }, convention2.Calls);
            Assert.Empty(convention3.Calls);

            if (useBuilder)
            {
                foreignKey.Builder.IsOwnership(false, ConfigurationSource.Convention);
            }
            else
            {
                foreignKey.IsOwnership = false;
            }

            Assert.Equal(new[] { true, false }, convention1.Calls);
            Assert.Equal(new[] { true, false }, convention2.Calls);
            Assert.Empty(convention3.Calls);

            Assert.Same(
                foreignKey,
                dependentEntityBuilder.Metadata.RemoveForeignKey(
                    foreignKey.Properties, foreignKey.PrincipalKey, foreignKey.PrincipalEntityType));
        }

        private class ForeignKeyOwnershipChangedConvention : IForeignKeyOwnershipChangedConvention
        {
            private readonly bool _terminate;
            public readonly List<bool> Calls = new List<bool>();

            public ForeignKeyOwnershipChangedConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public void ProcessForeignKeyOwnershipChanged(
                IConventionRelationshipBuilder relationshipBuilder, IConventionContext<IConventionRelationshipBuilder> context)
            {
                Assert.NotNull(relationshipBuilder.Metadata.Builder);

                Calls.Add(relationshipBuilder.Metadata.IsOwnership);

                if (_terminate)
                {
                    context.StopProcessing();
                }
            }
        }

        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [ConditionalTheory]
        public void OnForeignKeyAnnotationChanged_calls_conventions_in_order(bool useBuilder, bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new ForeignKeyAnnotationChangedConvention(terminate: false);
            var convention2 = new ForeignKeyAnnotationChangedConvention(terminate: true);
            var convention3 = new ForeignKeyAnnotationChangedConvention(terminate: false);
            conventions.ForeignKeyAnnotationChangedConventions.Add(convention1);
            conventions.ForeignKeyAnnotationChangedConventions.Add(convention2);
            conventions.ForeignKeyAnnotationChangedConventions.Add(convention3);

            var builder = new InternalModelBuilder(new Model(conventions));
            var principalEntityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
            var dependentEntityBuilder = builder.Entity(typeof(OrderDetails), ConfigurationSource.Convention);
            var foreignKey = dependentEntityBuilder.HasRelationship(principalEntityBuilder.Metadata, ConfigurationSource.Convention)
                .Metadata;

            var scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

            if (useBuilder)
            {
                Assert.NotNull(foreignKey.Builder.HasAnnotation("foo", "bar", ConfigurationSource.Convention));
            }
            else
            {
                foreignKey["foo"] = "bar";
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
                Assert.NotNull(foreignKey.Builder.HasAnnotation("foo", "bar", ConfigurationSource.Convention));
            }
            else
            {
                foreignKey["foo"] = "bar";
            }

            Assert.Equal(new[] { "bar" }, convention1.Calls);
            Assert.Equal(new[] { "bar" }, convention2.Calls);
            Assert.Empty(convention3.Calls);

            if (useBuilder)
            {
                Assert.NotNull(foreignKey.Builder.HasAnnotation("foo", null, ConfigurationSource.Convention));
            }
            else
            {
                foreignKey.RemoveAnnotation("foo");
            }

            Assert.Equal(new[] { "bar", null }, convention1.Calls);
            Assert.Equal(new[] { "bar", null }, convention2.Calls);
            Assert.Empty(convention3.Calls);

            foreignKey[CoreAnnotationNames.EagerLoaded] = true;

            Assert.Equal(new[] { "bar", null }, convention1.Calls);
        }

        private class ForeignKeyAnnotationChangedConvention : IForeignKeyAnnotationChangedConvention
        {
            private readonly bool _terminate;
            public readonly List<object> Calls = new List<object>();

            public ForeignKeyAnnotationChangedConvention(bool terminate)
            {
                _terminate = terminate;
            }

            /// <summary>
            ///     Called after an annotation is changed on a foreign key.
            /// </summary>
            /// <param name="relationshipBuilder"> The builder for the foreign key. </param>
            /// <param name="name"> The annotation name. </param>
            /// <param name="annotation"> The new annotation. </param>
            /// <param name="oldAnnotation"> The old annotation.  </param>
            /// <param name="context"> Additional information associated with convention execution. </param>
            public void ProcessForeignKeyAnnotationChanged(
                IConventionRelationshipBuilder relationshipBuilder,
                string name,
                IConventionAnnotation annotation,
                IConventionAnnotation oldAnnotation,
                IConventionContext<IConventionAnnotation> context)
            {
                Assert.NotNull(relationshipBuilder.Metadata.Builder);

                Calls.Add(annotation?.Value);

                if (_terminate)
                {
                    context.StopProcessing();
                }
            }
        }

        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [ConditionalTheory]
        public void OnNavigationAdded_calls_conventions_in_order(bool useBuilder, bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new NavigationAddedConvention(terminate: false);
            var convention2 = new NavigationAddedConvention(terminate: true);
            var convention3 = new NavigationAddedConvention(terminate: false);
            conventions.NavigationAddedConventions.Add(convention1);
            conventions.NavigationAddedConventions.Add(convention2);
            conventions.NavigationAddedConventions.Add(convention3);

            var builder = new InternalModelBuilder(new Model(conventions));
            var principalEntityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
            var dependentEntityBuilder = builder.Entity(typeof(OrderDetails), ConfigurationSource.Convention);

            var scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

            if (useBuilder)
            {
                var result = dependentEntityBuilder.HasRelationship(
                    principalEntityBuilder.Metadata, OrderDetails.OrderProperty, Order.OrderDetailsProperty,
                    ConfigurationSource.Convention);

                Assert.Equal(!useScope, result == null);
            }
            else
            {
                var fk = dependentEntityBuilder.HasRelationship(principalEntityBuilder.Metadata, ConfigurationSource.Convention)
                    .IsUnique(true, ConfigurationSource.Convention)
                    .Metadata;
                var result = fk.HasDependentToPrincipal(OrderDetails.OrderProperty, ConfigurationSource.Explicit);

                Assert.Equal(!useScope, result == null);

                result = fk.HasPrincipalToDependent(Order.OrderDetailsProperty, ConfigurationSource.Explicit);

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

        private class NavigationAddedConvention : INavigationAddedConvention
        {
            private readonly bool _terminate;
            public readonly List<object> Calls = new List<object>();

            public NavigationAddedConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public void ProcessNavigationAdded(
                IConventionRelationshipBuilder relationshipBuilder, IConventionNavigation navigation,
                IConventionContext<IConventionNavigation> context)
            {
                Assert.NotNull(relationshipBuilder.Metadata.Builder);

                Calls.Add(navigation.Name);

                if (_terminate)
                {
                    if (navigation.IsOnDependent)
                    {
                        relationshipBuilder.Metadata.HasDependentToPrincipal((string)null);
                    }
                    else
                    {
                        relationshipBuilder.Metadata.HasPrincipalToDependent((string)null);
                    }

                    context.StopProcessing();
                }
            }
        }

        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [ConditionalTheory]
        public void OnNavigationAnnotationChanged_calls_conventions_in_order(bool useBuilder, bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new NavigationAnnotationChangedConvention(terminate: false);
            var convention2 = new NavigationAnnotationChangedConvention(terminate: true);
            var convention3 = new NavigationAnnotationChangedConvention(terminate: false);
            conventions.NavigationAnnotationChangedConventions.Add(convention1);
            conventions.NavigationAnnotationChangedConventions.Add(convention2);
            conventions.NavigationAnnotationChangedConventions.Add(convention3);

            var builder = new InternalModelBuilder(new Model(conventions));
            var principalEntityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
            var dependentEntityBuilder = builder.Entity(typeof(OrderDetails), ConfigurationSource.Convention);
            var navigation = dependentEntityBuilder.HasRelationship(
                principalEntityBuilder.Metadata, OrderDetails.OrderProperty, ConfigurationSource.Convention)
                .Metadata.DependentToPrincipal;

            var scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

            if (useBuilder)
            {
                Assert.NotNull(navigation.Builder.HasAnnotation("foo", "bar", ConfigurationSource.Convention));
            }
            else
            {
                navigation["foo"] = "bar";
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
                Assert.NotNull(navigation.Builder.HasAnnotation("foo", "bar", ConfigurationSource.Convention));
            }
            else
            {
                navigation["foo"] = "bar";
            }

            Assert.Equal(new[] { "bar" }, convention1.Calls);
            Assert.Equal(new[] { "bar" }, convention2.Calls);
            Assert.Empty(convention3.Calls);

            if (useBuilder)
            {
                Assert.NotNull(navigation.Builder.HasAnnotation("foo", null, ConfigurationSource.Convention));
            }
            else
            {
                navigation.RemoveAnnotation("foo");
            }

            Assert.Equal(new[] { "bar", null }, convention1.Calls);
            Assert.Equal(new[] { "bar", null }, convention2.Calls);
            Assert.Empty(convention3.Calls);

            navigation[CoreAnnotationNames.EagerLoaded] = true;

            Assert.Equal(new[] { "bar", null }, convention1.Calls);
        }

        private class NavigationAnnotationChangedConvention : INavigationAnnotationChangedConvention
        {
            private readonly bool _terminate;
            public readonly List<object> Calls = new List<object>();

            public NavigationAnnotationChangedConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public virtual void ProcessNavigationAnnotationChanged(
                IConventionRelationshipBuilder relationshipBuilder,
                IConventionNavigation navigation,
                string name,
                IConventionAnnotation annotation,
                IConventionAnnotation oldAnnotation,
                IConventionContext<IConventionAnnotation> context)
            {
                Assert.NotNull(relationshipBuilder.Metadata.Builder);

                Calls.Add(annotation?.Value);

                if (_terminate)
                {
                    context.StopProcessing();
                }
            }
        }

        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [ConditionalTheory]
        public void OnNavigationRemoved_calls_conventions_in_order(bool useBuilder, bool useScope)
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
            var relationshipBuilder = dependentEntityBuilder.HasRelationship(
                principalEntityBuilder.Metadata, nameof(OrderDetails.Order), nameof(Order.OrderDetails), ConfigurationSource.Convention);

            var scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

            if (useBuilder)
            {
                Assert.NotNull(
                    relationshipBuilder.HasNavigation(
                        (string)null,
                        pointsToPrincipal: true,
                        ConfigurationSource.Convention));
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
                Assert.NotNull(
                    relationshipBuilder.HasNavigation(
                        (string)null,
                        pointsToPrincipal: true,
                        ConfigurationSource.Convention));
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

            public void ProcessNavigationRemoved(
                IConventionEntityTypeBuilder sourceEntityTypeBuilder, IConventionEntityTypeBuilder targetEntityTypeBuilder,
                string navigationName, MemberInfo memberInfo, IConventionContext<string> context)
            {
                Assert.NotNull(sourceEntityTypeBuilder.Metadata.Builder);

                Calls.Add(navigationName);

                if (_terminate)
                {
                    context.StopProcessing();
                }
            }
        }

        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [ConditionalTheory]
        public void OnSkipNavigationAdded_calls_conventions_in_order(bool useBuilder, bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new SkipNavigationAddedConvention(terminate: false);
            var convention2 = new SkipNavigationAddedConvention(terminate: true);
            var convention3 = new SkipNavigationAddedConvention(terminate: false);
            conventions.SkipNavigationAddedConventions.Add(convention1);
            conventions.SkipNavigationAddedConventions.Add(convention2);
            conventions.SkipNavigationAddedConventions.Add(convention3);

            var builder = new InternalModelBuilder(new Model(conventions));
            var firstEntityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
            var secondEntityBuilder = builder.Entity(typeof(Product), ConfigurationSource.Convention);

            var scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

            if (useBuilder)
            {
                firstEntityBuilder.HasSkipNavigation(
                    MemberIdentity.Create(nameof(Order.Products)), secondEntityBuilder.Metadata, ConfigurationSource.Convention);
            }
            else
            {
                var result = firstEntityBuilder.Metadata.AddSkipNavigation(
                    nameof(Order.Products), null, secondEntityBuilder.Metadata, true, false, ConfigurationSource.Convention);

                Assert.Equal(!useScope, result == null);
            }

            if (useScope)
            {
                Assert.Empty(convention1.Calls);
                Assert.Empty(convention2.Calls);
                scope.Dispose();
            }

            Assert.Equal(new[] { nameof(Order.Products) }, convention1.Calls);
            Assert.Equal(new[] { nameof(Order.Products) }, convention2.Calls);
            Assert.Empty(convention3.Calls);
        }

        private class SkipNavigationAddedConvention : ISkipNavigationAddedConvention
        {
            private readonly bool _terminate;
            public readonly List<object> Calls = new List<object>();

            public SkipNavigationAddedConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public void ProcessSkipNavigationAdded(
                IConventionSkipNavigationBuilder skipNavigationBuilder,
                IConventionContext<IConventionSkipNavigationBuilder> context)
            {
                Assert.NotNull(skipNavigationBuilder.Metadata.Builder);

                Calls.Add(skipNavigationBuilder.Metadata.Name);

                if (_terminate)
                {
                    skipNavigationBuilder.Metadata.DeclaringEntityType.RemoveSkipNavigation(skipNavigationBuilder.Metadata);

                    context.StopProcessing();
                }
            }
        }

        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [ConditionalTheory]
        public void OnSkipNavigationAnnotationChanged_calls_conventions_in_order(bool useBuilder, bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new SkipNavigationAnnotationChangedConvention(terminate: false);
            var convention2 = new SkipNavigationAnnotationChangedConvention(terminate: true);
            var convention3 = new SkipNavigationAnnotationChangedConvention(terminate: false);
            conventions.SkipNavigationAnnotationChangedConventions.Add(convention1);
            conventions.SkipNavigationAnnotationChangedConventions.Add(convention2);
            conventions.SkipNavigationAnnotationChangedConventions.Add(convention3);

            var builder = new InternalModelBuilder(new Model(conventions));
            var firstEntityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
            var secondEntityBuilder = builder.Entity(typeof(Product), ConfigurationSource.Convention);

            var navigation = firstEntityBuilder.Metadata.AddSkipNavigation(
                nameof(Order.Products), null, secondEntityBuilder.Metadata,  true, false, ConfigurationSource.Convention);

            var scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

            if (useBuilder)
            {
                Assert.NotNull(navigation.Builder.HasAnnotation("foo", "bar", ConfigurationSource.Convention));
            }
            else
            {
                navigation["foo"] = "bar";
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
                Assert.NotNull(navigation.Builder.HasAnnotation("foo", "bar", ConfigurationSource.Convention));
            }
            else
            {
                navigation["foo"] = "bar";
            }

            Assert.Equal(new[] { "bar" }, convention1.Calls);
            Assert.Equal(new[] { "bar" }, convention2.Calls);
            Assert.Empty(convention3.Calls);

            if (useBuilder)
            {
                Assert.NotNull(navigation.Builder.HasAnnotation("foo", null, ConfigurationSource.Convention));
            }
            else
            {
                navigation.RemoveAnnotation("foo");
            }

            Assert.Equal(new[] { "bar", null }, convention1.Calls);
            Assert.Equal(new[] { "bar", null }, convention2.Calls);
            Assert.Empty(convention3.Calls);

            navigation[CoreAnnotationNames.EagerLoaded] = true;

            Assert.Equal(new[] { "bar", null }, convention1.Calls);
        }

        private class SkipNavigationAnnotationChangedConvention : ISkipNavigationAnnotationChangedConvention
        {
            private readonly bool _terminate;
            public readonly List<object> Calls = new List<object>();

            public SkipNavigationAnnotationChangedConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public virtual void ProcessSkipNavigationAnnotationChanged(
                IConventionSkipNavigationBuilder navigationBuilder,
                string name,
                IConventionAnnotation annotation,
                IConventionAnnotation oldAnnotation,
                IConventionContext<IConventionAnnotation> context)
            {
                Assert.NotNull(navigationBuilder.Metadata.Builder);

                Calls.Add(annotation?.Value);

                if (_terminate)
                {
                    context.StopProcessing();
                }
            }
        }

        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [ConditionalTheory]
        public void OnSkipNavigationForeignKeyChanged_calls_conventions_in_order(bool useBuilder, bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new SkipNavigationForeignKeyChangedConvention(terminate: false);
            var convention2 = new SkipNavigationForeignKeyChangedConvention(terminate: true);
            var convention3 = new SkipNavigationForeignKeyChangedConvention(terminate: false);
            conventions.SkipNavigationForeignKeyChangedConventions.Add(convention1);
            conventions.SkipNavigationForeignKeyChangedConventions.Add(convention2);
            conventions.SkipNavigationForeignKeyChangedConventions.Add(convention3);

            var builder = new InternalModelBuilder(new Model(conventions));
            var firstEntityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
            var secondEntityBuilder = builder.Entity(typeof(Product), ConfigurationSource.Convention);
            var associationEntityBuilder = builder.Entity(typeof(OrderProduct), ConfigurationSource.Convention);

            var foreignKey = associationEntityBuilder
                .HasRelationship(typeof(Order), new[] { OrderProduct.OrderIdProperty }, ConfigurationSource.Convention)
                .IsUnique(false, ConfigurationSource.Convention)
                .Metadata;
            var navigation = firstEntityBuilder.Metadata.AddSkipNavigation(
                nameof(Order.Products), null, secondEntityBuilder.Metadata, true, false, ConfigurationSource.Convention);

            var scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

            if (useBuilder)
            {
                navigation.Builder.HasForeignKey(foreignKey, ConfigurationSource.Explicit);
            }
            else
            {
                navigation.SetForeignKey(foreignKey, ConfigurationSource.Explicit);
            }

            if (useScope)
            {
                Assert.Empty(convention1.Calls);
                Assert.Empty(convention2.Calls);
                scope.Dispose();
            }

            Assert.Equal(new[] { foreignKey }, convention1.Calls);
            Assert.Equal(new[] { foreignKey }, convention2.Calls);
            Assert.Empty(convention3.Calls);

            if (useBuilder)
            {
                navigation.Builder.HasForeignKey(null, ConfigurationSource.Explicit);
            }
            else
            {
                navigation.SetForeignKey(null, ConfigurationSource.Explicit);
            }

            Assert.Equal(new ForeignKey[] { foreignKey, null }, convention1.Calls);
            Assert.Equal(new ForeignKey[] { foreignKey, null }, convention2.Calls);
            Assert.Empty(convention3.Calls);
        }

        private class SkipNavigationForeignKeyChangedConvention : ISkipNavigationForeignKeyChangedConvention
        {
            private readonly bool _terminate;
            public readonly List<object> Calls = new List<object>();

            public SkipNavigationForeignKeyChangedConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public virtual void ProcessSkipNavigationForeignKeyChanged(
                IConventionSkipNavigationBuilder navigationBuilder,
                IConventionForeignKey foreignKey,
                IConventionForeignKey oldForeignKey,
                IConventionContext<IConventionForeignKey> context)
            {
                Assert.NotNull(navigationBuilder.Metadata.Builder);

                Calls.Add(foreignKey);

                if (foreignKey == null)
                {
                    Assert.NotNull(oldForeignKey);
                }

                if (_terminate)
                {
                    context.StopProcessing();
                }
            }
        }

        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [ConditionalTheory]
        public void OnSkipNavigationInverseChanged_calls_conventions_in_order(bool useBuilder, bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new SkipNavigationInverseChangedConvention(terminate: false);
            var convention2 = new SkipNavigationInverseChangedConvention(terminate: true);
            var convention3 = new SkipNavigationInverseChangedConvention(terminate: false);
            conventions.SkipNavigationInverseChangedConventions.Add(convention1);
            conventions.SkipNavigationInverseChangedConventions.Add(convention2);
            conventions.SkipNavigationInverseChangedConventions.Add(convention3);

            var builder = new InternalModelBuilder(new Model(conventions));
            var firstEntityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
            var secondEntityBuilder = builder.Entity(typeof(Product), ConfigurationSource.Convention);

            var navigation = firstEntityBuilder.Metadata.AddSkipNavigation(
                nameof(Order.Products), null, secondEntityBuilder.Metadata, true, false, ConfigurationSource.Convention);
            var inverse = secondEntityBuilder.Metadata.AddSkipNavigation(
                nameof(Product.Orders), null, firstEntityBuilder.Metadata, true, false, ConfigurationSource.Convention);

            var scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

            if (useBuilder)
            {
                navigation.Builder.HasInverse(inverse, ConfigurationSource.Convention);
            }
            else
            {
                var result = navigation.SetInverse(inverse, ConfigurationSource.Convention);

                if (useScope)
                {
                    Assert.Same(inverse, result);
                }
                else
                {
                    Assert.Null(result);
                }
            }

            if (useScope)
            {
                Assert.Empty(convention1.Calls);
                Assert.Empty(convention2.Calls);
                scope.Dispose();
            }

            if (useBuilder)
            {
                Assert.Equal(new[] { nameof(Product.Orders), nameof(Order.Products) }, convention1.Calls);
                Assert.Equal(new[] { nameof(Product.Orders), nameof(Order.Products) }, convention2.Calls);
            }
            else
            {
                Assert.Equal(new[] { nameof(Product.Orders) }, convention1.Calls);
                Assert.Equal(new[] { nameof(Product.Orders) }, convention2.Calls);
            }
            Assert.Empty(convention3.Calls);
        }

        private class SkipNavigationInverseChangedConvention : ISkipNavigationInverseChangedConvention
        {
            private readonly bool _terminate;
            public readonly List<object> Calls = new List<object>();

            public SkipNavigationInverseChangedConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public virtual void ProcessSkipNavigationInverseChanged(
                IConventionSkipNavigationBuilder skipNavigationBuilder,
                IConventionSkipNavigation inverse,
                IConventionSkipNavigation oldInverse,
                IConventionContext<IConventionSkipNavigation> context)
            {
                Assert.NotNull(skipNavigationBuilder.Metadata.Builder);

                Calls.Add(inverse.Name);

                if (_terminate)
                {
                    context.StopProcessing();
                }
            }
        }

        [InlineData(false)]
        [InlineData(true)]
        [ConditionalTheory]
        public void OnSkipNavigationRemoved_calls_conventions_in_order(bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new SkipNavigationRemovedConvention(terminate: false);
            var convention2 = new SkipNavigationRemovedConvention(terminate: true);
            var convention3 = new SkipNavigationRemovedConvention(terminate: false);
            conventions.SkipNavigationRemovedConventions.Add(convention1);
            conventions.SkipNavigationRemovedConventions.Add(convention2);
            conventions.SkipNavigationRemovedConventions.Add(convention3);

            var builder = new InternalModelBuilder(new Model(conventions));
            var firstEntityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
            var secondEntityBuilder = builder.Entity(typeof(Product), ConfigurationSource.Convention);

            var navigation = firstEntityBuilder.Metadata.AddSkipNavigation(
                nameof(Order.Products), null, secondEntityBuilder.Metadata, true, false, ConfigurationSource.Convention);

            var scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

            var result = firstEntityBuilder.Metadata.RemoveSkipNavigation(navigation);

            if (useScope)
            {
                Assert.Same(navigation, result);
            }
            else
            {
                Assert.Null(result);
            }

            if (useScope)
            {
                Assert.Empty(convention1.Calls);
                Assert.Empty(convention2.Calls);
                scope.Dispose();
            }

            Assert.Equal(new[] { nameof(Order.Products) }, convention1.Calls);
            Assert.Equal(new[] { nameof(Order.Products) }, convention2.Calls);
            Assert.Empty(convention3.Calls);
        }

        private class SkipNavigationRemovedConvention : ISkipNavigationRemovedConvention
        {
            private readonly bool _terminate;
            public readonly List<object> Calls = new List<object>();

            public SkipNavigationRemovedConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public void ProcessSkipNavigationRemoved(
                IConventionEntityTypeBuilder entityTypeBuilder,
                IConventionSkipNavigation navigation,
                IConventionContext<IConventionSkipNavigation> context)
            {
                Assert.NotNull(entityTypeBuilder.Metadata.Builder);

                Calls.Add(navigation.Name);

                if (_terminate)
                {
                    context.StopProcessing();
                }
            }
        }

        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [ConditionalTheory]
        public void OnKeyAdded_calls_conventions_in_order(bool useBuilder, bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new KeyAddedConvention(terminate: false);
            var convention2 = new KeyAddedConvention(terminate: true);
            var convention3 = new KeyAddedConvention(terminate: false);
            conventions.KeyAddedConventions.Add(convention1);
            conventions.KeyAddedConventions.Add(convention2);
            conventions.KeyAddedConventions.Add(convention3);

            var builder = new InternalModelBuilder(new Model(conventions));
            var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
            var keyPropertyName = "OrderId";

            var scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

            if (useBuilder)
            {
                var result = entityBuilder.HasKey(
                    new List<string> { keyPropertyName }, ConfigurationSource.Convention);

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

        private class KeyAddedConvention : IKeyAddedConvention
        {
            private readonly bool _terminate;
            public readonly List<object> Calls = new List<object>();

            public KeyAddedConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public void ProcessKeyAdded(IConventionKeyBuilder keyBuilder, IConventionContext<IConventionKeyBuilder> context)
            {
                Assert.NotNull(keyBuilder.Metadata.Builder);

                Calls.Add(keyBuilder.Metadata.Properties.First().Name);

                if (_terminate)
                {
                    keyBuilder.Metadata.DeclaringEntityType.RemoveKey(keyBuilder.Metadata.Properties);
                    context.StopProcessing();
                }
            }
        }

        [InlineData(false)]
        [InlineData(true)]
        [ConditionalTheory]
        public void OnKeyRemoved_calls_conventions_in_order(bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new KeyRemovedConvention(terminate: false);
            var convention2 = new KeyRemovedConvention(terminate: true);
            var convention3 = new KeyRemovedConvention(terminate: false);
            conventions.KeyRemovedConventions.Add(convention1);
            conventions.KeyRemovedConventions.Add(convention2);
            conventions.KeyRemovedConventions.Add(convention3);

            var builder = new InternalModelBuilder(new Model(conventions));

            var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
            var key = entityBuilder.HasKey(
                new List<string> { "OrderId" }, ConfigurationSource.Convention).Metadata;

            var scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

            if (useScope)
            {
                Assert.Same(key, entityBuilder.Metadata.RemoveKey(key.Properties));
                Assert.Empty(convention1.Calls);
                Assert.Empty(convention2.Calls);
                scope.Dispose();
            }
            else
            {
                Assert.Null(entityBuilder.Metadata.RemoveKey(key.Properties));
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

            public void ProcessKeyRemoved(
                IConventionEntityTypeBuilder entityTypeBuilder, IConventionKey key, IConventionContext<IConventionKey> context)
            {
                Assert.NotNull(entityTypeBuilder.Metadata.Builder);

                Calls.Add(key.Properties.First().Name);

                if (_terminate)
                {
                    context.StopProcessing();
                }
            }
        }

        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [ConditionalTheory]
        public void OnKeyAnnotationChanged_calls_conventions_in_order(bool useBuilder, bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new KeyAnnotationChangedConvention(terminate: false);
            var convention2 = new KeyAnnotationChangedConvention(terminate: true);
            var convention3 = new KeyAnnotationChangedConvention(terminate: false);
            conventions.KeyAnnotationChangedConventions.Add(convention1);
            conventions.KeyAnnotationChangedConventions.Add(convention2);
            conventions.KeyAnnotationChangedConventions.Add(convention3);

            var builder = new InternalModelBuilder(new Model(conventions));
            var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
            var key = entityBuilder.HasKey(
                new List<string> { "OrderId" }, ConfigurationSource.Convention).Metadata;

            var scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

            if (useBuilder)
            {
                Assert.NotNull(key.Builder.HasAnnotation("foo", "bar", ConfigurationSource.Convention));
            }
            else
            {
                key["foo"] = "bar";
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
                Assert.NotNull(key.Builder.HasAnnotation("foo", "bar", ConfigurationSource.Convention));
            }
            else
            {
                key["foo"] = "bar";
            }

            Assert.Equal(new[] { "bar" }, convention1.Calls);
            Assert.Equal(new[] { "bar" }, convention2.Calls);
            Assert.Empty(convention3.Calls);

            if (useBuilder)
            {
                Assert.NotNull(key.Builder.HasAnnotation("foo", null, ConfigurationSource.Convention));
            }
            else
            {
                key.RemoveAnnotation("foo");
            }

            Assert.Equal(new[] { "bar", null }, convention1.Calls);
            Assert.Equal(new[] { "bar", null }, convention2.Calls);
            Assert.Empty(convention3.Calls);

            key[CoreAnnotationNames.Unicode] = false;

            Assert.Equal(new[] { "bar", null }, convention1.Calls);
        }

        private class KeyAnnotationChangedConvention : IKeyAnnotationChangedConvention
        {
            private readonly bool _terminate;
            public readonly List<object> Calls = new List<object>();

            public KeyAnnotationChangedConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public void ProcessKeyAnnotationChanged(
                IConventionKeyBuilder keyBuilder,
                string name, IConventionAnnotation annotation, IConventionAnnotation oldAnnotation,
                IConventionContext<IConventionAnnotation> context)
            {
                Assert.NotNull(keyBuilder.Metadata.Builder);

                Calls.Add(annotation?.Value);

                if (_terminate)
                {
                    context.StopProcessing();
                }
            }
        }

        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [ConditionalTheory]
        public void OnIndexAdded_calls_conventions_in_order(bool useBuilder, bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new IndexAddedConvention(terminate: false);
            var convention2 = new IndexAddedConvention(terminate: true);
            var convention3 = new IndexAddedConvention(terminate: false);
            conventions.IndexAddedConventions.Add(convention1);
            conventions.IndexAddedConventions.Add(convention2);
            conventions.IndexAddedConventions.Add(convention3);

            var builder = new InternalModelBuilder(new Model(conventions));
            var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);

            var scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

            if (useBuilder)
            {
                var result = entityBuilder.HasIndex(
                    new List<string> { "OrderId" }, ConfigurationSource.Convention);

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
                Assert.Empty(convention3.Calls);
                scope.Dispose();
            }

            Assert.Equal(new[] { "OrderId" }, convention1.Calls);
            Assert.Equal(new[] { "OrderId" }, convention2.Calls);
            Assert.Empty(convention3.Calls);
        }

        private class IndexAddedConvention : IIndexAddedConvention
        {
            private readonly bool _terminate;
            public readonly List<object> Calls = new List<object>();

            public IndexAddedConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public void ProcessIndexAdded(IConventionIndexBuilder indexBuilder, IConventionContext<IConventionIndexBuilder> context)
            {
                Assert.NotNull(indexBuilder.Metadata.Builder);

                Calls.Add(indexBuilder.Metadata.Properties.First().Name);

                if (_terminate)
                {
                    indexBuilder.Metadata.DeclaringEntityType.RemoveIndex(indexBuilder.Metadata.Properties);
                    context.StopProcessing();
                }
            }
        }

        [InlineData(false)]
        [InlineData(true)]
        [ConditionalTheory]
        public void OnIndexRemoved_calls_conventions_in_order(bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new IndexRemovedConvention(terminate: false);
            var convention2 = new IndexRemovedConvention(terminate: true);
            var convention3 = new IndexRemovedConvention(terminate: false);
            conventions.IndexRemovedConventions.Add(convention1);
            conventions.IndexRemovedConventions.Add(convention2);
            conventions.IndexRemovedConventions.Add(convention3);

            var builder = new InternalModelBuilder(new Model(conventions));
            var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
            var index = entityBuilder.HasIndex(
                new List<string> { "OrderId" }, ConfigurationSource.Convention).Metadata;

            var scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

            var result = entityBuilder.Metadata.RemoveIndex(index.Properties);
            if (useScope)
            {
                Assert.Same(index, result);
            }
            else
            {
                Assert.Null(result);
            }

            if (useScope)
            {
                Assert.Empty(convention1.Calls);
                Assert.Empty(convention2.Calls);
                Assert.Empty(convention3.Calls);
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

            public void ProcessIndexRemoved(
                IConventionEntityTypeBuilder entityTypeBuilder, IConventionIndex index, IConventionContext<IConventionIndex> context)
            {
                Assert.NotNull(entityTypeBuilder.Metadata.Builder);

                Calls.Add(index.Properties.First().Name);

                if (_terminate)
                {
                    context.StopProcessing();
                }
            }
        }

        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [ConditionalTheory]
        public void OnIndexUniquenessChanged_calls_conventions_in_order(bool useBuilder, bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new IndexUniquenessChangedConvention(terminate: false);
            var convention2 = new IndexUniquenessChangedConvention(terminate: true);
            var convention3 = new IndexUniquenessChangedConvention(terminate: false);
            conventions.IndexUniquenessChangedConventions.Add(convention1);
            conventions.IndexUniquenessChangedConventions.Add(convention2);
            conventions.IndexUniquenessChangedConventions.Add(convention3);

            var builder = new InternalModelBuilder(new Model(conventions));
            var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
            var index = entityBuilder.HasIndex(
                new List<string> { "OrderId" }, ConfigurationSource.Convention).Metadata;

            var scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

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

        private class IndexUniquenessChangedConvention : IIndexUniquenessChangedConvention
        {
            private readonly bool _terminate;
            public readonly List<bool> Calls = new List<bool>();

            public IndexUniquenessChangedConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public void ProcessIndexUniquenessChanged(
                IConventionIndexBuilder indexBuilder, IConventionContext<IConventionIndexBuilder> context)
            {
                Assert.NotNull(indexBuilder.Metadata.Builder);

                Calls.Add(indexBuilder.Metadata.IsUnique);

                if (_terminate)
                {
                    context.StopProcessing();
                }
            }
        }

        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [ConditionalTheory]
        public void OnIndexAnnotationChanged_calls_conventions_in_order(bool useBuilder, bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new IndexAnnotationChangedConvention(terminate: false);
            var convention2 = new IndexAnnotationChangedConvention(terminate: true);
            var convention3 = new IndexAnnotationChangedConvention(terminate: false);
            conventions.IndexAnnotationChangedConventions.Add(convention1);
            conventions.IndexAnnotationChangedConventions.Add(convention2);
            conventions.IndexAnnotationChangedConventions.Add(convention3);

            var builder = new InternalModelBuilder(new Model(conventions));
            var indexBuilder = builder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention)
                .HasIndex(new[] { nameof(SpecialOrder.Name) }, ConfigurationSource.Convention);

            var scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

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
            Assert.Empty(convention3.Calls);

            indexBuilder.Metadata[CoreAnnotationNames.MaxLength] = 20;

            Assert.Equal(new[] { "bar", null }, convention1.Calls);
        }

        private class IndexAnnotationChangedConvention : IIndexAnnotationChangedConvention
        {
            private readonly bool _terminate;
            public readonly List<object> Calls = new List<object>();

            public IndexAnnotationChangedConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public void ProcessIndexAnnotationChanged(
                IConventionIndexBuilder indexBuilder,
                string name,
                IConventionAnnotation annotation,
                IConventionAnnotation oldAnnotation, IConventionContext<IConventionAnnotation> context)
            {
                Assert.NotNull(indexBuilder.Metadata.Builder);

                Calls.Add(annotation?.Value);

                if (_terminate)
                {
                    context.StopProcessing();
                }
            }
        }

        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [ConditionalTheory]
        public void OnPropertyAdded_calls_conventions_in_order(bool useBuilder, bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new PropertyAddedConvention(terminate: false);
            var convention2 = new PropertyAddedConvention(terminate: true);
            var convention3 = new PropertyAddedConvention(terminate: false);
            conventions.PropertyAddedConventions.Add(convention1);
            conventions.PropertyAddedConventions.Add(convention2);
            conventions.PropertyAddedConventions.Add(convention3);

            var builder = new InternalModelBuilder(new Model(conventions));
            var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
            var shadowPropertyName = "ShadowProperty";

            var scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

            if (useBuilder)
            {
                var result = entityBuilder.Property(typeof(int), shadowPropertyName, ConfigurationSource.Convention);

                Assert.Equal(!useScope, result == null);
            }
            else
            {
                var result = entityBuilder.Metadata.AddProperty(
                    shadowPropertyName, typeof(int), ConfigurationSource.Convention, ConfigurationSource.Convention);

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

            scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

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

        private class PropertyAddedConvention : IPropertyAddedConvention
        {
            private readonly bool _terminate;
            public readonly List<object> Calls = new List<object>();

            public PropertyAddedConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public void ProcessPropertyAdded(
                IConventionPropertyBuilder propertyBuilder, IConventionContext<IConventionPropertyBuilder> context)
            {
                Assert.NotNull(propertyBuilder.Metadata.Builder);

                Calls.Add(propertyBuilder.Metadata.Name);

                if (_terminate)
                {
                    propertyBuilder.Metadata.DeclaringEntityType.RemoveProperty(propertyBuilder.Metadata.Name);
                    context.StopProcessing();
                }
            }
        }

        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [ConditionalTheory]
        public void OnPropertyNullabilityChanged_calls_conventions_in_order(bool useBuilder, bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new PropertyNullabilityChangedConvention(false);
            var convention2 = new PropertyNullabilityChangedConvention(true);
            var convention3 = new PropertyNullabilityChangedConvention(false);
            conventions.PropertyNullabilityChangedConventions.Add(convention1);
            conventions.PropertyNullabilityChangedConventions.Add(convention2);
            conventions.PropertyNullabilityChangedConventions.Add(convention3);

            var builder = new ModelBuilder(conventions);

            var scope = useScope ? ((Model)builder.Model).ConventionDispatcher.DelayConventions() : null;

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

        private class PropertyNullabilityChangedConvention : IPropertyNullabilityChangedConvention
        {
            public readonly List<bool?> Calls = new List<bool?>();
            private readonly bool _terminate;

            public PropertyNullabilityChangedConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public void ProcessPropertyNullabilityChanged(
                IConventionPropertyBuilder propertyBuilder, IConventionContext<IConventionPropertyBuilder> context)
            {
                Calls.Add(propertyBuilder.Metadata.IsNullable);

                if (_terminate)
                {
                    context.StopProcessing();
                }
            }
        }

        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [ConditionalTheory]
        public void OnPropertyFieldChanged_calls_conventions_in_order(bool useBuilder, bool useScope)
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

            var scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

            if (useBuilder)
            {
                Assert.NotNull(propertyBuilder.HasField(nameof(Order.IntField), ConfigurationSource.Convention));
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
                Assert.NotNull(propertyBuilder.HasField(nameof(Order.IntField), ConfigurationSource.Convention));
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
                Assert.NotNull(propertyBuilder.HasField((string)null, ConfigurationSource.Convention));
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

            public void ProcessPropertyFieldChanged(
                IConventionPropertyBuilder propertyBuilder, FieldInfo newFieldInfo, FieldInfo oldFieldInfo,
                IConventionContext<FieldInfo> context)
            {
                Assert.NotNull(propertyBuilder.Metadata.Builder);

                Calls.Add(oldFieldInfo?.Name);

                if (_terminate)
                {
                    context.StopProcessing();
                }
            }
        }

        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [ConditionalTheory]
        public void OnPropertyAnnotationChanged_calls_conventions_in_order(bool useBuilder, bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new PropertyAnnotationChangedConvention(false);
            var convention2 = new PropertyAnnotationChangedConvention(true);
            var convention3 = new PropertyAnnotationChangedConvention(false);
            conventions.PropertyAnnotationChangedConventions.Add(convention1);
            conventions.PropertyAnnotationChangedConventions.Add(convention2);
            conventions.PropertyAnnotationChangedConventions.Add(convention3);

            var builder = new InternalModelBuilder(new Model(conventions));
            var propertyBuilder = builder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention)
                .Property(nameof(SpecialOrder.Name), ConfigurationSource.Convention);

            var scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

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
            Assert.Empty(convention3.Calls);

            propertyBuilder.Metadata[CoreAnnotationNames.AfterSaveBehavior] = PropertySaveBehavior.Ignore;

            Assert.Equal(new[] { "bar", null }, convention1.Calls);
        }

        private class PropertyAnnotationChangedConvention : IPropertyAnnotationChangedConvention
        {
            private readonly bool _terminate;
            public readonly List<object> Calls = new List<object>();

            public PropertyAnnotationChangedConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public void ProcessPropertyAnnotationChanged(
                IConventionPropertyBuilder propertyBuilder,
                string name,
                IConventionAnnotation annotation,
                IConventionAnnotation oldAnnotation, IConventionContext<IConventionAnnotation> context)
            {
                Assert.NotNull(propertyBuilder.Metadata.Builder);

                Calls.Add(annotation?.Value);

                if (_terminate)
                {
                    context.StopProcessing();
                }
            }
        }

        [InlineData(false)]
        [InlineData(true)]
        [ConditionalTheory]
        public void OnPropertyRemoved_calls_conventions_in_order(bool useScope)
        {
            var conventions = new ConventionSet();

            var convention1 = new PropertyRemovedConvention(terminate: false);
            var convention2 = new PropertyRemovedConvention(terminate: true);
            var convention3 = new PropertyRemovedConvention(terminate: false);
            conventions.PropertyRemovedConventions.Add(convention1);
            conventions.PropertyRemovedConventions.Add(convention2);
            conventions.PropertyRemovedConventions.Add(convention3);

            var builder = new InternalModelBuilder(new Model(conventions));
            var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
            var shadowPropertyName = "ShadowProperty";
            var property = entityBuilder.Metadata.AddProperty(
                    shadowPropertyName, typeof(int), ConfigurationSource.Convention, ConfigurationSource.Convention);

            var scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

            var result = entityBuilder.Metadata.RemoveProperty(property);

            if (useScope)
            {
                Assert.Same(property, result);
            }
            else
            {
                Assert.Null(result);
            }

            if (useScope)
            {
                Assert.Empty(convention1.Calls);
                Assert.Empty(convention2.Calls);
                scope.Dispose();
            }

            Assert.Equal(new[] { property }, convention1.Calls);
            Assert.Equal(new[] { property }, convention2.Calls);
            Assert.Empty(convention3.Calls);
        }

        private class PropertyRemovedConvention : IPropertyRemovedConvention
        {
            private readonly bool _terminate;
            public readonly List<object> Calls = new List<object>();

            public PropertyRemovedConvention(bool terminate)
            {
                _terminate = terminate;
            }

            public void ProcessPropertyRemoved(
                IConventionEntityTypeBuilder entityTypeBuilder,
                IConventionProperty property,
                IConventionContext<IConventionProperty> context)
            {
                Assert.NotNull(entityTypeBuilder.Metadata.Builder);

                Calls.Add(property);

                if (_terminate)
                {
                    context.StopProcessing();
                }
            }
        }

        private class Order
        {
            public static readonly PropertyInfo OrderIdProperty = typeof(Order).GetProperty(nameof(OrderId));
            public static readonly PropertyInfo OrderDetailsProperty = typeof(Order).GetProperty(nameof(OrderDetails));

            public readonly int IntField = 1;

            public int OrderId { get; set; }

            public string Name { get; set; }

            public virtual OrderDetails OrderDetails { get; set; }
            public virtual OrderDetails OtherOrderDetails { get; set; }
            public virtual ICollection<Product> Products { get; set; }
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

        private class OrderProduct
        {
            public static readonly PropertyInfo OrderIdProperty = typeof(OrderProduct).GetProperty(nameof(OrderId));
            public static readonly PropertyInfo ProductIdProperty = typeof(OrderProduct).GetProperty(nameof(ProductId));

            public int OrderId { get; set; }
            public int ProductId { get; set; }
            public virtual Order Order { get; set; }
            public virtual Product Product { get; set; }
        }

        private class Product
        {
            public int Id { get; set; }
            public virtual ICollection<Order> Orders { get; set; }
        }
    }
}
