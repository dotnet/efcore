// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

public class ConventionDispatcherTest
{
    [ConditionalFact]
    public void Infinite_recursion_throws()
    {
        var conventions = new ConventionSet();

        conventions.Add(new InfinitePropertyAddedConvention());

        var builder = new InternalModelBuilder(new Model(conventions));
        var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
        var shadowPropertyName = "ShadowProperty";

        Assert.Equal(
            CoreStrings.ConventionsInfiniteLoop,
            Assert.Throws<InvalidOperationException>(
                () =>
                    entityBuilder.Property(typeof(int), shadowPropertyName, ConfigurationSource.Convention)).Message);
    }

    private class InfinitePropertyAddedConvention : IPropertyAddedConvention
    {
        private int _count;

        public void ProcessPropertyAdded(
            IConventionPropertyBuilder propertyBuilder,
            IConventionContext<IConventionPropertyBuilder> context)
            => ((IMutableEntityType)propertyBuilder.Metadata.DeclaringType).AddProperty("TempProperty" + _count++, typeof(int));
    }

    [InlineData(false)]
    [InlineData(true)]
    [ConditionalTheory]
    public void OnModelInitialized_calls_conventions_in_order(bool useBuilder)
    {
        var conventions = new ConventionSet();

        var convention1 = new ModelInitializedConvention(terminate: false);
        var convention2 = new ModelInitializedConvention(terminate: true);
        var convention3 = new ModelInitializedConvention(terminate: false);
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

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

        AssertSetOperations(new ModelInitializedConvention(terminate: true),
            conventions, conventions.ModelInitializedConventions);
    }

    private class ModelInitializedConvention(bool terminate) : IModelInitializedConvention
    {
        private readonly bool _terminate = terminate;
        public int Calls;

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

        var convention1 = new ModelFinalizingConvention(terminate: false);
        var convention2 = new ModelFinalizingConvention(terminate: true);
        var convention3 = new ModelFinalizingConvention(terminate: false);
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

        var model = new Model(conventions);

        if (useBuilder)
        {
            Assert.NotNull(new InternalModelBuilder(model).Metadata.FinalizeModel());
        }
        else
        {
            Assert.NotNull(model.FinalizeModel());
        }

        Assert.Equal(1, convention1.Calls);
        Assert.Equal(1, convention2.Calls);
        Assert.Equal(0, convention3.Calls);

        AssertSetOperations(new ModelFinalizingConvention(terminate: true),
            conventions, conventions.ModelFinalizingConventions);
    }

    private class ModelFinalizingConvention(bool terminate) : IModelFinalizingConvention
    {
        private readonly bool _terminate = terminate;
        public int Calls;

        public void ProcessModelFinalizing(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
        {
            Assert.NotNull(modelBuilder.Metadata.Builder);

            Calls++;

            if (_terminate)
            {
                context.StopProcessing(modelBuilder);
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
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

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

        AssertSetOperations(new ModelAnnotationChangedConvention(terminate: true),
            conventions, conventions.ModelAnnotationChangedConventions);
    }

    private class ModelAnnotationChangedConvention(bool terminate) : IModelAnnotationChangedConvention
    {
        private readonly bool _terminate = terminate;
        public readonly List<object> Calls = [];

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

        var convention1 = new EntityTypeAddedConvention(terminate: false);
        var convention2 = new EntityTypeAddedConvention(terminate: true);
        var convention3 = new EntityTypeAddedConvention(terminate: false);
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

        var builder = new InternalModelBuilder(new Model(conventions));

        var scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

        if (useBuilder)
        {
            var result = builder.Entity(typeof(Order), ConfigurationSource.Convention);

            Assert.NotNull(result);
        }
        else
        {
            var result = builder.Metadata.AddEntityType(typeof(Order), owned: false, ConfigurationSource.Convention);

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

        AssertSetOperations(new EntityTypeAddedConvention(terminate: true),
            conventions, conventions.EntityTypeAddedConventions);
    }

    private class EntityTypeAddedConvention(bool terminate) : IEntityTypeAddedConvention
    {
        private readonly bool _terminate = terminate;
        public int Calls;

        public void ProcessEntityTypeAdded(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionContext<IConventionEntityTypeBuilder> context)
        {
            Assert.Same(entityTypeBuilder, entityTypeBuilder.Metadata.Builder);

            Calls++;

            if (_terminate)
            {
                entityTypeBuilder.Metadata.Model.RemoveEntityType(entityTypeBuilder.Metadata.Name);
                context.StopProcessing();
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

        var convention1 = new TypeIgnoredConvention(terminate: false);
        var convention2 = new TypeIgnoredConvention(terminate: true);
        var convention3 = new TypeIgnoredConvention(terminate: false);
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

        var convention4 = new EntityTypeRemovedConvention(terminate: false);
        var convention5 = new EntityTypeRemovedConvention(terminate: true);
        var convention6 = new EntityTypeRemovedConvention(terminate: false);
        conventions.Add(convention4);
        conventions.Add(convention5);
        conventions.Add(convention6);

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

        AssertSetOperations(new TypeIgnoredConvention(terminate: true),
            conventions, conventions.TypeIgnoredConventions);
        AssertSetOperations(new EntityTypeRemovedConvention(terminate: true),
            conventions, conventions.EntityTypeRemovedConventions);
    }

    private class TypeIgnoredConvention(bool terminate) : ITypeIgnoredConvention
    {
        private readonly bool _terminate = terminate;
        public int Calls;

        public void ProcessTypeIgnored(
            IConventionModelBuilder modelBuilder,
            string name,
            Type type,
            IConventionContext<string> context)
        {
            Assert.Null(modelBuilder.Metadata.FindEntityType(name));
            Calls++;

            if (_terminate)
            {
                context.StopProcessing();
            }
        }
    }

    private class EntityTypeRemovedConvention(bool terminate) : IEntityTypeRemovedConvention
    {
        private readonly bool _terminate = terminate;
        public int Calls;

        public void ProcessEntityTypeRemoved(
            IConventionModelBuilder modelBuilder,
            IConventionEntityType entityType,
            IConventionContext<IConventionEntityType> context)
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
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

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

        AssertSetOperations(new EntityTypeMemberIgnoredConvention(terminate: true),
            conventions, conventions.EntityTypeMemberIgnoredConventions);
    }

    private class EntityTypeMemberIgnoredConvention(bool terminate) : IEntityTypeMemberIgnoredConvention
    {
        private readonly bool _terminate = terminate;
        public readonly List<object> Calls = [];

        public void ProcessEntityTypeMemberIgnored(
            IConventionEntityTypeBuilder entityTypeBuilder,
            string name,
            IConventionContext<string> context)
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
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

        var builder = new InternalModelBuilder(new Model(conventions))
            .Entity(typeof(SpecialOrder), ConfigurationSource.Convention);

        var scope = useScope ? builder.Metadata.Model.DelayConventions() : null;

        if (useBuilder)
        {
            Assert.NotNull(builder.HasBaseType(typeof(Order), ConfigurationSource.Convention));
        }
        else
        {
            builder.Metadata.SetBaseType(
                builder.Metadata.Model.AddEntityType(typeof(Order), owned: false, ConfigurationSource.Explicit),
                ConfigurationSource.Convention);
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
            builder.Metadata.SetBaseType(builder.Metadata.Model.FindEntityType(typeof(Order)), ConfigurationSource.Convention);
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
            builder.Metadata.SetBaseType(null, ConfigurationSource.Convention);
        }

        Assert.Equal(new[] { typeof(Order), null }, convention1.Calls);
        Assert.Equal(new[] { typeof(Order), null }, convention2.Calls);
        Assert.Empty(convention3.Calls);

        AssertSetOperations(new EntityTypeBaseTypeChangedConvention(terminate: true),
            conventions, conventions.EntityTypeBaseTypeChangedConventions);
    }

    private class EntityTypeBaseTypeChangedConvention(bool terminate) : IEntityTypeBaseTypeChangedConvention
    {
        private readonly bool _terminate = terminate;
        public readonly List<Type> Calls = [];

        public void ProcessEntityTypeBaseTypeChanged(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionEntityType newBaseType,
            IConventionEntityType oldBaseType,
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
    public void OnDiscriminatorPropertySet_calls_conventions_in_order(bool useBuilder, bool useScope)
    {
        var conventions = new ConventionSet();

        var convention1 = new DiscriminatorPropertySetConvention(terminate: false);
        var convention2 = new DiscriminatorPropertySetConvention(terminate: true);
        var convention3 = new DiscriminatorPropertySetConvention(terminate: false);
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

        var builder = new InternalModelBuilder(new Model(conventions));
        var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
        var propertyBuilder = entityBuilder.Property(Order.OrderIdProperty, ConfigurationSource.Convention);

        var scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

        if (useBuilder)
        {
            entityBuilder.HasDiscriminator(Order.OrderIdProperty, ConfigurationSource.Convention);
        }
        else
        {
            entityBuilder.Metadata.SetDiscriminatorProperty(propertyBuilder.Metadata, ConfigurationSource.Convention);
        }

        if (useScope)
        {
            Assert.Empty(convention1.Calls);
            Assert.Empty(convention2.Calls);
            scope.Dispose();
        }

        Assert.Equal(new string[] { nameof(Order.OrderId) }, convention1.Calls);
        Assert.Equal(new string[] { nameof(Order.OrderId) }, convention2.Calls);
        Assert.Empty(convention3.Calls);

        if (useBuilder)
        {
            entityBuilder.HasDiscriminator(Order.OrderIdProperty, ConfigurationSource.Convention);
        }
        else
        {
            entityBuilder.Metadata.SetDiscriminatorProperty(propertyBuilder.Metadata, ConfigurationSource.Convention);
        }

        Assert.Equal(new string[] { nameof(Order.OrderId) }, convention1.Calls);
        Assert.Equal(new string[] { nameof(Order.OrderId) }, convention2.Calls);
        Assert.Empty(convention3.Calls);

        if (useBuilder)
        {
            entityBuilder.HasNoDiscriminator(ConfigurationSource.Convention);
        }
        else
        {
            entityBuilder.Metadata.SetDiscriminatorProperty(null, ConfigurationSource.Convention);
        }

        Assert.Equal(new[] { nameof(Order.OrderId), null }, convention1.Calls);
        Assert.Equal(new[] { nameof(Order.OrderId), null }, convention2.Calls);
        Assert.Empty(convention3.Calls);

        AssertSetOperations(new DiscriminatorPropertySetConvention(terminate: true),
            conventions, conventions.DiscriminatorPropertySetConventions);
    }

    private class DiscriminatorPropertySetConvention(bool terminate) : IDiscriminatorPropertySetConvention
    {
        private readonly bool _terminate = terminate;
        public readonly List<object> Calls = [];

        public void ProcessDiscriminatorPropertySet(
            IConventionEntityTypeBuilder entityTypeBuilder,
            string name,
            IConventionContext<string> context)
        {
            Assert.True(entityTypeBuilder.Metadata.IsInModel);

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
    public void OnPrimaryKeyChanged_calls_conventions_in_order(bool useBuilder, bool useScope)
    {
        var conventions = new ConventionSet();

        var convention1 = new EntityTypePrimaryKeyChangedConvention(terminate: false);
        var convention2 = new EntityTypePrimaryKeyChangedConvention(terminate: true);
        var convention3 = new EntityTypePrimaryKeyChangedConvention(terminate: false);
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

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
                    entityBuilder.Property("OrderId", ConfigurationSource.Convention).Metadata,
                    ConfigurationSource.Convention));
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
                    entityBuilder.Property("OrderId", ConfigurationSource.Convention).Metadata,
                    ConfigurationSource.Convention));
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
            Assert.Null(entityBuilder.Metadata.SetPrimaryKey((Property)null, ConfigurationSource.Convention));
        }

        Assert.Equal(new[] { null, "OrderId" }, convention1.Calls);
        Assert.Equal(new[] { null, "OrderId" }, convention2.Calls);
        Assert.Empty(convention3.Calls);
        Assert.Null(entityBuilder.Metadata.GetPrimaryKeyConfigurationSource());

        AssertSetOperations(new EntityTypePrimaryKeyChangedConvention(terminate: true),
            conventions, conventions.EntityTypePrimaryKeyChangedConventions);
    }

    private class EntityTypePrimaryKeyChangedConvention(bool terminate) : IEntityTypePrimaryKeyChangedConvention
    {
        private readonly bool _terminate = terminate;
        public readonly List<object> Calls = [];

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
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

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

        AssertSetOperations(new EntityTypeAnnotationChangedConvention(terminate: true),
            conventions, conventions.EntityTypeAnnotationChangedConventions);
    }

    private class EntityTypeAnnotationChangedConvention(bool terminate) : IEntityTypeAnnotationChangedConvention
    {
        private readonly bool _terminate = terminate;
        public readonly List<object> Calls = [];

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
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

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

        AssertSetOperations(new ForeignKeyAddedConvention(terminate: true),
            conventions, conventions.ForeignKeyAddedConventions);
    }

    private class ForeignKeyAddedConvention(bool terminate) : IForeignKeyAddedConvention
    {
        private readonly bool _terminate = terminate;
        public readonly List<object> Calls = [];

        public void ProcessForeignKeyAdded(
            IConventionForeignKeyBuilder relationshipBuilder,
            IConventionContext<IConventionForeignKeyBuilder> context)
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
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

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

        AssertSetOperations(new ForeignKeyRemovedConvention(terminate: true),
            conventions, conventions.ForeignKeyRemovedConventions);
    }

    private class ForeignKeyRemovedConvention(bool terminate) : IForeignKeyRemovedConvention
    {
        private readonly bool _terminate = terminate;
        public readonly List<object> Calls = [];

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
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

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

        AssertSetOperations(new ForeignKeyPrincipalEndChangedConvention(terminate: true),
            conventions, conventions.ForeignKeyPrincipalEndChangedConventions);
    }

    private class ForeignKeyPrincipalEndChangedConvention(bool terminate) : IForeignKeyPrincipalEndChangedConvention
    {
        private readonly bool _terminate = terminate;
        public readonly List<object> Calls = [];

        public void ProcessForeignKeyPrincipalEndChanged(
            IConventionForeignKeyBuilder relationshipBuilder,
            IConventionContext<IConventionForeignKeyBuilder> context)
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
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

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

        Assert.Equal(new[] { ("FK", "FK2"), ("FK2", "FK3"), ("FK", "FK3") }, convention1.Calls);
        Assert.Equal(new[] { ("FK2", "FK3"), ("FK", "FK3") }, convention2.Calls);
        Assert.Empty(convention3.Calls);

        AssertSetOperations(new ForeignKeyPropertiesChangedConvention(terminate: true),
            conventions, conventions.ForeignKeyPropertiesChangedConventions);
    }

    private class ForeignKeyPropertiesChangedConvention(bool terminate) : IForeignKeyPropertiesChangedConvention
    {
        private readonly bool _terminate = terminate;
        public readonly List<(string, string)> Calls = [];

        public void ProcessForeignKeyPropertiesChanged(
            IConventionForeignKeyBuilder relationshipBuilder,
            IReadOnlyList<IConventionProperty> oldDependentProperties,
            IConventionKey oldPrincipalKey,
            IConventionContext<IReadOnlyList<IConventionProperty>> context)
        {
            Assert.NotNull(relationshipBuilder.Metadata.Builder);
            Assert.NotNull(oldDependentProperties);
            Assert.NotNull(oldPrincipalKey);

            Calls.Add((oldDependentProperties.First().Name, relationshipBuilder.Metadata.Properties.First().Name));

            if (relationshipBuilder.Metadata.Properties.First().Name == "FK2")
            {
                relationshipBuilder.Metadata.SetProperties(
                    new[]
                    {
                        relationshipBuilder.Metadata.DeclaringEntityType.Builder.Property(
                            typeof(int), "FK3").Metadata
                    },
                    relationshipBuilder.Metadata.PrincipalKey);
                context.StopProcessingIfChanged(relationshipBuilder.Metadata.Properties);
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
    public void OnForeignKeyUniquenessChanged_calls_conventions_in_order(bool useBuilder, bool useScope)
    {
        var conventions = new ConventionSet();

        var convention1 = new ForeignKeyUniquenessChangedConvention(terminate: false);
        var convention2 = new ForeignKeyUniquenessChangedConvention(terminate: true);
        var convention3 = new ForeignKeyUniquenessChangedConvention(terminate: false);
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

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

        AssertSetOperations(new ForeignKeyUniquenessChangedConvention(terminate: true),
            conventions, conventions.ForeignKeyUniquenessChangedConventions);
    }

    private class ForeignKeyUniquenessChangedConvention(bool terminate) : IForeignKeyUniquenessChangedConvention
    {
        private readonly bool _terminate = terminate;
        public readonly List<bool> Calls = [];

        public void ProcessForeignKeyUniquenessChanged(
            IConventionForeignKeyBuilder relationshipBuilder,
            IConventionContext<bool?> context)
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
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

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

        AssertSetOperations(new ForeignKeyRequirednessChangedConvention(terminate: true),
            conventions, conventions.ForeignKeyRequirednessChangedConventions);
    }

    private class ForeignKeyRequirednessChangedConvention(bool terminate) : IForeignKeyRequirednessChangedConvention
    {
        private readonly bool _terminate = terminate;
        public readonly List<bool> Calls = [];

        public void ProcessForeignKeyRequirednessChanged(
            IConventionForeignKeyBuilder relationshipBuilder,
            IConventionContext<bool?> context)
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
    public void OnForeignKeyDependentRequirednessChanged_calls_conventions_in_order(bool useBuilder, bool useScope)
    {
        var conventions = new ConventionSet();

        var convention1 = new ForeignKeyDependentRequirednessChangedConvention(terminate: false);
        var convention2 = new ForeignKeyDependentRequirednessChangedConvention(terminate: true);
        var convention3 = new ForeignKeyDependentRequirednessChangedConvention(terminate: false);
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

        var builder = new InternalModelBuilder(new Model(conventions));
        var principalEntityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
        var dependentEntityBuilder = builder.Entity(typeof(OrderDetails), ConfigurationSource.Convention);
        var foreignKey = dependentEntityBuilder.HasRelationship(principalEntityBuilder.Metadata, ConfigurationSource.Convention)
            .IsUnique(true, ConfigurationSource.Convention)
            .HasEntityTypes(principalEntityBuilder.Metadata, dependentEntityBuilder.Metadata, ConfigurationSource.Convention)
            .Metadata;

        var scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

        if (useBuilder)
        {
            foreignKey.Builder.IsRequiredDependent(true, ConfigurationSource.Convention);
        }
        else
        {
            foreignKey.IsRequiredDependent = true;
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
            foreignKey.Builder.IsRequiredDependent(true, ConfigurationSource.Convention);
        }
        else
        {
            foreignKey.IsRequiredDependent = true;
        }

        Assert.Equal(new[] { true }, convention1.Calls);
        Assert.Equal(new[] { true }, convention2.Calls);
        Assert.Empty(convention3.Calls);

        if (useBuilder)
        {
            foreignKey.Builder.IsRequiredDependent(false, ConfigurationSource.Convention);
        }
        else
        {
            foreignKey.IsRequiredDependent = false;
        }

        Assert.Equal(new[] { true, false }, convention1.Calls);
        Assert.Equal(new[] { true, false }, convention2.Calls);
        Assert.Empty(convention3.Calls);

        Assert.Same(
            foreignKey,
            dependentEntityBuilder.Metadata.RemoveForeignKey(
                foreignKey.Properties, foreignKey.PrincipalKey, foreignKey.PrincipalEntityType));

        AssertSetOperations(new ForeignKeyDependentRequirednessChangedConvention(terminate: true),
            conventions, conventions.ForeignKeyDependentRequirednessChangedConventions);
    }

    private class ForeignKeyDependentRequirednessChangedConvention(bool terminate) : IForeignKeyDependentRequirednessChangedConvention
    {
        private readonly bool _terminate = terminate;
        public readonly List<bool> Calls = [];

        public void ProcessForeignKeyDependentRequirednessChanged(
            IConventionForeignKeyBuilder relationshipBuilder,
            IConventionContext<bool?> context)
        {
            Assert.NotNull(relationshipBuilder.Metadata.Builder);

            Calls.Add(relationshipBuilder.Metadata.IsRequiredDependent);

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
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

        var builder = new InternalModelBuilder(new Model(conventions));
        var principalEntityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
        var dependentEntityBuilder = builder.Entity(typeof(OrderDetails), ConfigurationSource.Convention, shouldBeOwned: true);
        var foreignKey = dependentEntityBuilder.HasRelationship(
                principalEntityBuilder.Metadata, null, nameof(Order.OrderDetails), ConfigurationSource.Convention)
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

        AssertSetOperations(new ForeignKeyOwnershipChangedConvention(terminate: true),
            conventions, conventions.ForeignKeyOwnershipChangedConventions);
    }

    private class ForeignKeyOwnershipChangedConvention(bool terminate) : IForeignKeyOwnershipChangedConvention
    {
        private readonly bool _terminate = terminate;
        public readonly List<bool> Calls = [];

        public void ProcessForeignKeyOwnershipChanged(
            IConventionForeignKeyBuilder relationshipBuilder,
            IConventionContext<bool?> context)
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
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

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

        AssertSetOperations(new ForeignKeyAnnotationChangedConvention(terminate: true),
            conventions, conventions.ForeignKeyAnnotationChangedConventions);
    }

    private class ForeignKeyAnnotationChangedConvention(bool terminate) : IForeignKeyAnnotationChangedConvention
    {
        private readonly bool _terminate = terminate;
        public readonly List<object> Calls = [];

        /// <summary>
        ///     Called after an annotation is changed on a foreign key.
        /// </summary>
        /// <param name="relationshipBuilder">The builder for the foreign key.</param>
        /// <param name="name">The annotation name.</param>
        /// <param name="annotation">The new annotation.</param>
        /// <param name="oldAnnotation">The old annotation.</param>
        /// <param name="context">Additional information associated with convention execution.</param>
        public void ProcessForeignKeyAnnotationChanged(
            IConventionForeignKeyBuilder relationshipBuilder,
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
    public void OnForeignKeyNullNavigationSet_calls_conventions_in_order(bool useBuilder, bool useScope)
    {
        var conventions = new ConventionSet();

        var convention1 = new ForeignKeyNullNavigationSetConvention(terminate: false);
        var convention2 = new ForeignKeyNullNavigationSetConvention(terminate: true);
        var convention3 = new ForeignKeyNullNavigationSetConvention(terminate: false);
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

        var builder = new InternalModelBuilder(new Model(conventions));
        var principalEntityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
        var dependentEntityBuilder = builder.Entity(typeof(OrderDetails), ConfigurationSource.Convention);

        var scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

        if (useBuilder)
        {
            var result = dependentEntityBuilder.HasRelationship(
                principalEntityBuilder.Metadata, (MemberInfo)null, null, ConfigurationSource.Convention);

            Assert.NotNull(result);
        }
        else
        {
            var fk = dependentEntityBuilder.HasRelationship(principalEntityBuilder.Metadata, ConfigurationSource.Convention)
                .IsUnique(true, ConfigurationSource.Convention)
                .Metadata;
            var result = fk.SetDependentToPrincipal((MemberInfo)null, ConfigurationSource.Explicit);

            Assert.Null(result);

            result = fk.SetPrincipalToDependent((MemberInfo)null, ConfigurationSource.Explicit);

            Assert.Null(result);
        }

        if (useScope)
        {
            Assert.Empty(convention1.Calls);
            Assert.Empty(convention2.Calls);
            scope.Dispose();
        }

        Assert.Equal(new[] { true, false }, convention1.Calls);
        Assert.Equal(new[] { true, false }, convention2.Calls);
        Assert.Empty(convention3.Calls);

        AssertSetOperations(new ForeignKeyNullNavigationSetConvention(terminate: true),
            conventions, conventions.ForeignKeyNullNavigationSetConventions);
    }

    private class ForeignKeyNullNavigationSetConvention(bool terminate) : IForeignKeyNullNavigationSetConvention
    {
        private readonly bool _terminate = terminate;
        public readonly List<bool> Calls = [];

        public void ProcessForeignKeyNullNavigationSet(
            IConventionForeignKeyBuilder relationshipBuilder,
            bool pointsToPrincipal,
            IConventionContext<IConventionNavigation> context)
        {
            Assert.NotNull(relationshipBuilder.Metadata.Builder);

            Calls.Add(pointsToPrincipal);

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
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

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
            var result = fk.SetDependentToPrincipal(OrderDetails.OrderProperty, ConfigurationSource.Explicit);

            Assert.Equal(!useScope, result == null);

            result = fk.SetPrincipalToDependent(Order.OrderDetailsProperty, ConfigurationSource.Explicit);

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

        AssertSetOperations(new NavigationAddedConvention(terminate: true),
            conventions, conventions.NavigationAddedConventions);
    }

    private class NavigationAddedConvention(bool terminate) : INavigationAddedConvention
    {
        private readonly bool _terminate = terminate;
        public readonly List<object> Calls = [];

        public void ProcessNavigationAdded(
            IConventionNavigationBuilder navigationBuilder,
            IConventionContext<IConventionNavigationBuilder> context)
        {
            var navigation = navigationBuilder.Metadata;
            var foreignKey = navigation.ForeignKey;

            Assert.NotNull(foreignKey.Builder);

            Calls.Add(navigation.Name);

            if (_terminate)
            {
                if (navigation.IsOnDependent)
                {
                    foreignKey.SetDependentToPrincipal((string)null);
                }
                else
                {
                    foreignKey.SetPrincipalToDependent((string)null);
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
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

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

        AssertSetOperations(new NavigationAnnotationChangedConvention(terminate: true),
            conventions, conventions.NavigationAnnotationChangedConventions);
    }

    private class NavigationAnnotationChangedConvention(bool terminate) : INavigationAnnotationChangedConvention
    {
        private readonly bool _terminate = terminate;
        public readonly List<object> Calls = [];

        public virtual void ProcessNavigationAnnotationChanged(
            IConventionForeignKeyBuilder relationshipBuilder,
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
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

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
            var result = relationshipBuilder.Metadata.SetDependentToPrincipal((string)null, ConfigurationSource.Convention);

            Assert.Equal(!useScope, result == null);
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
            Assert.Null(relationshipBuilder.Metadata.SetDependentToPrincipal((string)null, ConfigurationSource.Convention));
        }

        Assert.Equal(new[] { nameof(OrderDetails.Order) }, convention1.Calls);
        Assert.Equal(new[] { nameof(OrderDetails.Order) }, convention2.Calls);
        Assert.Empty(convention3.Calls);

        AssertSetOperations(new NavigationRemovedConvention(terminate: true),
            conventions, conventions.NavigationRemovedConventions);
    }

    private class NavigationRemovedConvention(bool terminate) : INavigationRemovedConvention
    {
        private readonly bool _terminate = terminate;
        public readonly List<object> Calls = [];

        public void ProcessNavigationRemoved(
            IConventionEntityTypeBuilder sourceEntityTypeBuilder,
            IConventionEntityTypeBuilder targetEntityTypeBuilder,
            string navigationName,
            MemberInfo memberInfo,
            IConventionContext<string> context)
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
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

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
                nameof(Order.Products), null, null, secondEntityBuilder.Metadata, true, false, ConfigurationSource.Convention);

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

        AssertSetOperations(new SkipNavigationAddedConvention(terminate: true),
            conventions, conventions.SkipNavigationAddedConventions);
    }

    private class SkipNavigationAddedConvention(bool terminate) : ISkipNavigationAddedConvention
    {
        private readonly bool _terminate = terminate;
        public readonly List<object> Calls = [];

        public void ProcessSkipNavigationAdded(
            IConventionSkipNavigationBuilder skipNavigationBuilder,
            IConventionContext<IConventionSkipNavigationBuilder> context)
        {
            Assert.True(skipNavigationBuilder.Metadata.IsInModel);

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
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

        var builder = new InternalModelBuilder(new Model(conventions));
        var firstEntityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
        var secondEntityBuilder = builder.Entity(typeof(Product), ConfigurationSource.Convention);

        var navigation = firstEntityBuilder.Metadata.AddSkipNavigation(
            nameof(Order.Products), null, null, secondEntityBuilder.Metadata, true, false, ConfigurationSource.Convention);

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

        AssertSetOperations(new SkipNavigationAnnotationChangedConvention(terminate: true),
            conventions, conventions.SkipNavigationAnnotationChangedConventions);
    }

    private class SkipNavigationAnnotationChangedConvention(bool terminate) : ISkipNavigationAnnotationChangedConvention
    {
        private readonly bool _terminate = terminate;
        public readonly List<object> Calls = [];

        public virtual void ProcessSkipNavigationAnnotationChanged(
            IConventionSkipNavigationBuilder navigationBuilder,
            string name,
            IConventionAnnotation annotation,
            IConventionAnnotation oldAnnotation,
            IConventionContext<IConventionAnnotation> context)
        {
            Assert.True(navigationBuilder.Metadata.IsInModel);

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
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

        var builder = new InternalModelBuilder(new Model(conventions));
        var firstEntityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
        var secondEntityBuilder = builder.Entity(typeof(Product), ConfigurationSource.Convention);
        var joinEntityBuilder = builder.Entity(typeof(OrderProduct), ConfigurationSource.Convention);

        var foreignKey = joinEntityBuilder
            .HasRelationship(typeof(Order), new[] { OrderProduct.OrderIdProperty }, ConfigurationSource.Convention)
            .IsUnique(false, ConfigurationSource.Convention)
            .Metadata;
        var navigation = firstEntityBuilder.Metadata.AddSkipNavigation(
            nameof(Order.Products), null, null, secondEntityBuilder.Metadata, true, false, ConfigurationSource.Convention);

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

        Assert.Equal(new[] { foreignKey, null }, convention1.Calls);
        Assert.Equal(new[] { foreignKey, null }, convention2.Calls);
        Assert.Empty(convention3.Calls);

        AssertSetOperations(new SkipNavigationForeignKeyChangedConvention(terminate: true),
            conventions, conventions.SkipNavigationForeignKeyChangedConventions);
    }

    private class SkipNavigationForeignKeyChangedConvention(bool terminate) : ISkipNavigationForeignKeyChangedConvention
    {
        private readonly bool _terminate = terminate;
        public readonly List<object> Calls = [];

        public virtual void ProcessSkipNavigationForeignKeyChanged(
            IConventionSkipNavigationBuilder navigationBuilder,
            IConventionForeignKey foreignKey,
            IConventionForeignKey oldForeignKey,
            IConventionContext<IConventionForeignKey> context)
        {
            Assert.True(navigationBuilder.Metadata.IsInModel);

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
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

        var builder = new InternalModelBuilder(new Model(conventions));
        var firstEntityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
        var secondEntityBuilder = builder.Entity(typeof(Product), ConfigurationSource.Convention);

        var navigation = firstEntityBuilder.Metadata.AddSkipNavigation(
            nameof(Order.Products), null, null, secondEntityBuilder.Metadata, true, false, ConfigurationSource.Convention);
        var inverse = secondEntityBuilder.Metadata.AddSkipNavigation(
            nameof(Product.Orders), null, null, firstEntityBuilder.Metadata, true, false, ConfigurationSource.Convention);

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

        AssertSetOperations(new SkipNavigationInverseChangedConvention(terminate: true),
            conventions, conventions.SkipNavigationInverseChangedConventions);
    }

    private class SkipNavigationInverseChangedConvention(bool terminate) : ISkipNavigationInverseChangedConvention
    {
        private readonly bool _terminate = terminate;
        public readonly List<object> Calls = [];

        public virtual void ProcessSkipNavigationInverseChanged(
            IConventionSkipNavigationBuilder skipNavigationBuilder,
            IConventionSkipNavigation inverse,
            IConventionSkipNavigation oldInverse,
            IConventionContext<IConventionSkipNavigation> context)
        {
            Assert.True(skipNavigationBuilder.Metadata.IsInModel);

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
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

        var builder = new InternalModelBuilder(new Model(conventions));
        var firstEntityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
        var secondEntityBuilder = builder.Entity(typeof(Product), ConfigurationSource.Convention);

        var navigation = firstEntityBuilder.Metadata.AddSkipNavigation(
            nameof(Order.Products), null, null, secondEntityBuilder.Metadata, true, false, ConfigurationSource.Convention);

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

        AssertSetOperations(new SkipNavigationRemovedConvention(terminate: true),
            conventions, conventions.SkipNavigationRemovedConventions);
    }

    private class SkipNavigationRemovedConvention(bool terminate) : ISkipNavigationRemovedConvention
    {
        private readonly bool _terminate = terminate;
        public readonly List<object> Calls = [];

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
    public void OnTriggerAdded_calls_conventions_in_order(bool useBuilder, bool useScope)
    {
        var conventions = new ConventionSet();

        var convention1 = new TriggerAddedConvention(terminate: false);
        var convention2 = new TriggerAddedConvention(terminate: true);
        var convention3 = new TriggerAddedConvention(terminate: false);
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

        var builder = new InternalModelBuilder(new Model(conventions));
        var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);

        var scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

        if (useBuilder)
        {
            var result = entityBuilder.HasTrigger("MyTrigger", ConfigurationSource.Convention);

            Assert.Equal(!useScope, result == null);
        }
        else
        {
            var result = entityBuilder.Metadata.AddTrigger("MyTrigger", ConfigurationSource.Convention);

            Assert.Equal(!useScope, result == null);
        }

        if (useScope)
        {
            Assert.Empty(convention1.Calls);
            Assert.Empty(convention2.Calls);
            scope.Dispose();
        }

        Assert.Equal(new[] { "MyTrigger" }, convention1.Calls);
        Assert.Equal(new[] { "MyTrigger" }, convention2.Calls);
        Assert.Empty(convention3.Calls);

        AssertSetOperations(new TriggerAddedConvention(terminate: true),
            conventions, conventions.TriggerAddedConventions);
    }

    private class TriggerAddedConvention(bool terminate) : ITriggerAddedConvention
    {
        private readonly bool _terminate = terminate;
        public readonly List<object> Calls = [];

        public void ProcessTriggerAdded(IConventionTriggerBuilder triggerBuilder, IConventionContext<IConventionTriggerBuilder> context)
        {
            Assert.True(triggerBuilder.Metadata.IsInModel);

            Calls.Add(triggerBuilder.Metadata.ModelName);

            if (_terminate)
            {
                triggerBuilder.Metadata.EntityType.RemoveTrigger(triggerBuilder.Metadata.ModelName);

                context.StopProcessing();
            }
        }
    }

    [InlineData(false)]
    [InlineData(true)]
    [ConditionalTheory]
    public void OnTriggerRemoved_calls_conventions_in_order(bool useScope)
    {
        var conventions = new ConventionSet();

        var convention1 = new TriggerRemovedConvention(terminate: false);
        var convention2 = new TriggerRemovedConvention(terminate: true);
        var convention3 = new TriggerRemovedConvention(terminate: false);
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

        var builder = new InternalModelBuilder(new Model(conventions));
        var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);

        var trigger = entityBuilder.Metadata.AddTrigger("MyTrigger", ConfigurationSource.Convention);

        var scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

        var result = entityBuilder.Metadata.RemoveTrigger(trigger.ModelName);

        if (useScope)
        {
            Assert.Same(trigger, result);
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

        Assert.Equal(new[] { "MyTrigger" }, convention1.Calls);
        Assert.Equal(new[] { "MyTrigger" }, convention2.Calls);
        Assert.Empty(convention3.Calls);

        AssertSetOperations(new TriggerRemovedConvention(terminate: true),
            conventions, conventions.TriggerRemovedConventions);
    }

    private class TriggerRemovedConvention(bool terminate) : ITriggerRemovedConvention
    {
        private readonly bool _terminate = terminate;
        public readonly List<object> Calls = [];

        public void ProcessTriggerRemoved(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionTrigger trigger,
            IConventionContext<IConventionTrigger> context)
        {
            Assert.NotNull(entityTypeBuilder.Metadata.Builder);

            Calls.Add(trigger.ModelName);

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
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

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
            var result = ((IMutableEntityType)entityBuilder.Metadata).AddKey(property);

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

        AssertSetOperations(new KeyAddedConvention(terminate: true),
            conventions, conventions.KeyAddedConventions);
    }

    private class KeyAddedConvention(bool terminate) : IKeyAddedConvention
    {
        private readonly bool _terminate = terminate;
        public readonly List<object> Calls = [];

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
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

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

        AssertSetOperations(new KeyRemovedConvention(terminate: true),
            conventions, conventions.KeyRemovedConventions);
    }

    private class KeyRemovedConvention(bool terminate) : IKeyRemovedConvention
    {
        private readonly bool _terminate = terminate;
        public readonly List<object> Calls = [];

        public void ProcessKeyRemoved(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionKey key,
            IConventionContext<IConventionKey> context)
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
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

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

        AssertSetOperations(new KeyAnnotationChangedConvention(terminate: true),
            conventions, conventions.KeyAnnotationChangedConventions);
    }

    private class KeyAnnotationChangedConvention(bool terminate) : IKeyAnnotationChangedConvention
    {
        private readonly bool _terminate = terminate;
        public readonly List<object> Calls = [];

        public void ProcessKeyAnnotationChanged(
            IConventionKeyBuilder keyBuilder,
            string name,
            IConventionAnnotation annotation,
            IConventionAnnotation oldAnnotation,
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
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

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
            var result = ((IMutableEntityType)entityBuilder.Metadata).AddIndex(property);

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

        AssertSetOperations(new IndexAddedConvention(terminate: true),
            conventions, conventions.IndexAddedConventions);
    }

    private class IndexAddedConvention(bool terminate) : IIndexAddedConvention
    {
        private readonly bool _terminate = terminate;
        public readonly List<object> Calls = [];

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
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

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

        AssertSetOperations(new IndexRemovedConvention(terminate: true),
            conventions, conventions.IndexRemovedConventions);
    }

    private class IndexRemovedConvention(bool terminate) : IIndexRemovedConvention
    {
        private readonly bool _terminate = terminate;
        public readonly List<object> Calls = [];

        public void ProcessIndexRemoved(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionIndex index,
            IConventionContext<IConventionIndex> context)
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
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

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

        AssertSetOperations(new IndexUniquenessChangedConvention(terminate: true),
            conventions, conventions.IndexUniquenessChangedConventions);
    }

    private class IndexUniquenessChangedConvention(bool terminate) : IIndexUniquenessChangedConvention
    {
        private readonly bool _terminate = terminate;
        public readonly List<bool> Calls = [];

        public void ProcessIndexUniquenessChanged(
            IConventionIndexBuilder indexBuilder,
            IConventionContext<bool?> context)
        {
            Assert.NotNull(indexBuilder.Metadata.Builder);

            Calls.Add(indexBuilder.Metadata.IsUnique);

            if (_terminate)
            {
                context.StopProcessing();
            }
        }
    }

#nullable enable
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    [ConditionalTheory]
    public void OnIndexSortOrderChanged_calls_conventions_in_order(bool useBuilder, bool useScope)
    {
        var conventions = new ConventionSet();

        var convention1 = new IndexSortOrderChangedConvention(terminate: false);
        var convention2 = new IndexSortOrderChangedConvention(terminate: true);
        var convention3 = new IndexSortOrderChangedConvention(terminate: false);
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

        var builder = new InternalModelBuilder(new Model(conventions));
        var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention)!;
        var index = entityBuilder.HasIndex(new List<string> { "OrderId" }, ConfigurationSource.Convention)!.Metadata;

        var scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

        if (useBuilder)
        {
            index.Builder.IsDescending([], ConfigurationSource.Convention);
        }
        else
        {
            index.IsDescending = [];
        }

        if (useScope)
        {
            Assert.Empty(convention1.Calls);
            Assert.Empty(convention2.Calls);
            scope!.Dispose();
        }

        Assert.Equal([[]], convention1.Calls);
        Assert.Equal([[]], convention2.Calls);
        Assert.Empty(convention3.Calls);

        if (useBuilder)
        {
            index.Builder.IsDescending([], ConfigurationSource.Convention);
        }
        else
        {
            index.IsDescending = [];
        }

        Assert.Equal([[]], convention1.Calls);
        Assert.Equal([[]], convention2.Calls);
        Assert.Empty(convention3.Calls);

        if (useBuilder)
        {
            index.Builder.IsDescending(null, ConfigurationSource.Convention);
        }
        else
        {
            index.IsDescending = null;
        }

        Assert.Equal([[], null], convention1.Calls);
        Assert.Equal([[], null], convention2.Calls);
        Assert.Empty(convention3.Calls);

        Assert.Same(index, entityBuilder.Metadata.RemoveIndex(index.Properties));

        AssertSetOperations(new IndexSortOrderChangedConvention(terminate: true),
            conventions, conventions.IndexSortOrderChangedConventions);
    }

    private class IndexSortOrderChangedConvention(bool terminate) : IIndexSortOrderChangedConvention
    {
        private readonly bool _terminate = terminate;
        public readonly List<IReadOnlyList<bool>?> Calls = [];

        public void ProcessIndexSortOrderChanged(
            IConventionIndexBuilder indexBuilder,
            IConventionContext<IReadOnlyList<bool>?> context)
        {
            Assert.NotNull(indexBuilder.Metadata.Builder);

            Calls.Add(indexBuilder.Metadata.IsDescending);

            if (_terminate)
            {
                context.StopProcessing();
            }
        }
    }
#nullable restore

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
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

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

        AssertSetOperations(new IndexAnnotationChangedConvention(terminate: true),
            conventions, conventions.IndexAnnotationChangedConventions);
    }

    private class IndexAnnotationChangedConvention(bool terminate) : IIndexAnnotationChangedConvention
    {
        private readonly bool _terminate = terminate;
        public readonly List<object> Calls = [];

        public void ProcessIndexAnnotationChanged(
            IConventionIndexBuilder indexBuilder,
            string name,
            IConventionAnnotation annotation,
            IConventionAnnotation oldAnnotation,
            IConventionContext<IConventionAnnotation> context)
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
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

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
            var result = ((IMutableEntityType)entityBuilder.Metadata).AddProperty(Order.OrderIdProperty);

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

        AssertSetOperations(new PropertyAddedConvention(terminate: true),
            conventions, conventions.PropertyAddedConventions);
    }

    private class PropertyAddedConvention(bool terminate) : IPropertyAddedConvention
    {
        private readonly bool _terminate = terminate;
        public readonly List<object> Calls = [];

        public void ProcessPropertyAdded(
            IConventionPropertyBuilder propertyBuilder,
            IConventionContext<IConventionPropertyBuilder> context)
        {
            Assert.True(propertyBuilder.Metadata.IsInModel);

            Calls.Add(propertyBuilder.Metadata.Name);

            if (_terminate)
            {
                propertyBuilder.Metadata.DeclaringType.RemoveProperty(propertyBuilder.Metadata.Name);
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
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

        var model = new Model(conventions);

        var scope = useScope ? model.DelayConventions() : null;

        var propertyBuilder = model.Builder.Entity(typeof(Order), ConfigurationSource.Convention)
            .Property(typeof(string), "Name", ConfigurationSource.Convention);
        if (useBuilder)
        {
            propertyBuilder.IsRequired(true, ConfigurationSource.Convention);
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

        if (useBuilder)
        {
            propertyBuilder.IsRequired(false, ConfigurationSource.Convention);
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

        if (useBuilder)
        {
            propertyBuilder.IsRequired(false, ConfigurationSource.Convention);
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

        if (useBuilder)
        {
            propertyBuilder.IsRequired(true, ConfigurationSource.Convention);
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

        AssertSetOperations(new PropertyNullabilityChangedConvention(terminate: true),
            conventions, conventions.PropertyNullabilityChangedConventions);
    }

    private class PropertyNullabilityChangedConvention(bool terminate) : IPropertyNullabilityChangedConvention
    {
        public readonly List<bool?> Calls = [];
        private readonly bool _terminate = terminate;

        public void ProcessPropertyNullabilityChanged(
            IConventionPropertyBuilder propertyBuilder,
            IConventionContext<bool?> context)
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
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

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
            propertyBuilder.Metadata.SetField(
                nameof(Order.IntField),
                ConfigurationSource.Convention);
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
            propertyBuilder.Metadata.SetField(
                nameof(Order.IntField),
                ConfigurationSource.Convention);
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
            propertyBuilder.Metadata.SetFieldInfo(
                null,
                ConfigurationSource.Convention);
        }

        Assert.Equal(new[] { null, nameof(Order.IntField) }, convention1.Calls);
        Assert.Equal(new[] { null, nameof(Order.IntField) }, convention2.Calls);
        Assert.Empty(convention3.Calls);

        AssertSetOperations(new PropertyFieldChangedConvention(terminate: true),
            conventions, conventions.PropertyFieldChangedConventions);
    }

    private class PropertyFieldChangedConvention(bool terminate) : IPropertyFieldChangedConvention
    {
        private readonly bool _terminate = terminate;
        public readonly List<object> Calls = [];

        public void ProcessPropertyFieldChanged(
            IConventionPropertyBuilder propertyBuilder,
            FieldInfo newFieldInfo,
            FieldInfo oldFieldInfo,
            IConventionContext<FieldInfo> context)
        {
            Assert.True(propertyBuilder.Metadata.IsInModel);

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
    public void OnPropertyElementTypeChanged_calls_conventions_in_order(bool useBuilder, bool useScope)
    {
        var conventions = new ConventionSet();

        var convention1 = new PropertyElementTypeChangedConvention(terminate: false);
        var convention2 = new PropertyElementTypeChangedConvention(terminate: true);
        var convention3 = new PropertyElementTypeChangedConvention(terminate: false);
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

        var builder = new InternalModelBuilder(new Model(conventions));
        var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention)!;
        var propertyBuilder = entityBuilder.Property(Order.OrderIdsProperty, ConfigurationSource.Convention)!;

        var scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

        ElementType elementType;

        if (useBuilder)
        {
            Assert.NotNull(propertyBuilder.SetElementType(typeof(int), ConfigurationSource.Convention));
            elementType = propertyBuilder.Metadata.GetElementType()!;
        }
        else
        {
            elementType = propertyBuilder.Metadata.SetElementType(typeof(int), ConfigurationSource.Convention);
        }

        if (useScope)
        {
            Assert.Empty(convention1.Calls);
            Assert.Empty(convention2.Calls);
            scope.Dispose();
        }

        Assert.Equal(new (object, object)[] { (null, elementType) }, convention1.Calls);
        Assert.Equal(new (object, object)[] { (null, elementType) }, convention2.Calls);
        Assert.Empty(convention3.Calls);

        if (useBuilder)
        {
            Assert.NotNull(propertyBuilder.SetElementType(typeof(int), ConfigurationSource.Convention));
            elementType = propertyBuilder.Metadata.GetElementType()!;
        }
        else
        {
            elementType = propertyBuilder.Metadata.SetElementType(typeof(int), ConfigurationSource.Convention);
        }

        Assert.Equal(new (object, object)[] { (null, elementType) }, convention1.Calls);
        Assert.Equal(new (object, object)[] { (null, elementType) }, convention2.Calls);
        Assert.Empty(convention3.Calls);

        if (useBuilder)
        {
            Assert.NotNull(propertyBuilder.SetElementType(null, ConfigurationSource.Convention));
        }
        else
        {
            Assert.Null(propertyBuilder.Metadata.SetElementType(null, ConfigurationSource.Convention));
        }

        Assert.Equal(new (object, object)[] { (null, elementType), (elementType, null) }, convention1.Calls);
        Assert.Equal(new (object, object)[] { (null, elementType), (elementType, null) }, convention2.Calls);
        Assert.Empty(convention3.Calls);

        AssertSetOperations(new PropertyElementTypeChangedConvention(terminate: true),
            conventions, conventions.PropertyElementTypeChangedConventions);
    }

    private class PropertyElementTypeChangedConvention(bool terminate) : IPropertyElementTypeChangedConvention
    {
        private readonly bool _terminate = terminate;
        public readonly List<(object, object)> Calls = [];

        public void ProcessPropertyElementTypeChanged(
            IConventionPropertyBuilder propertyBuilder,
            IElementType newElementType,
            IElementType oldElementType,
            IConventionContext<IElementType> context)
        {
            Assert.True(propertyBuilder.Metadata.IsInModel);

            Calls.Add((oldElementType, newElementType));

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
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

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

        AssertSetOperations(new PropertyAnnotationChangedConvention(terminate: true),
            conventions, conventions.PropertyAnnotationChangedConventions);
    }

    private class PropertyAnnotationChangedConvention(bool terminate) : IPropertyAnnotationChangedConvention
    {
        private readonly bool _terminate = terminate;
        public readonly List<object> Calls = [];

        public void ProcessPropertyAnnotationChanged(
            IConventionPropertyBuilder propertyBuilder,
            string name,
            IConventionAnnotation annotation,
            IConventionAnnotation oldAnnotation,
            IConventionContext<IConventionAnnotation> context)
        {
            Assert.True(propertyBuilder.Metadata.IsInModel);

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
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

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

        AssertSetOperations(new PropertyRemovedConvention(terminate: true),
            conventions, conventions.PropertyRemovedConventions);
    }

    private class PropertyRemovedConvention(bool terminate) : IPropertyRemovedConvention
    {
        private readonly bool _terminate = terminate;
        public readonly List<object> Calls = [];

        public void ProcessPropertyRemoved(
            IConventionTypeBaseBuilder typeBaseBuilder,
            IConventionProperty property,
            IConventionContext<IConventionProperty> context)
        {
            Assert.NotNull(typeBaseBuilder.Metadata.Builder);

            Calls.Add(property);

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
    public void OnComplexTypePropertyAdded_calls_conventions_in_order(bool useBuilder, bool useScope)
    {
        var conventions = new ConventionSet();

        var convention1 = new PropertyAddedConvention(terminate: false);
        var convention2 = new PropertyAddedConvention(terminate: true);
        var convention3 = new PropertyAddedConvention(terminate: false);
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

        var builder = new InternalModelBuilder(new Model(conventions));
        var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
        var complexBuilder = entityBuilder.ComplexProperty(
                Order.OrderDetailsProperty, complexTypeName: null, collection: false, ConfigurationSource.Convention)
            .ComplexTypeBuilder;
        var shadowPropertyName = "ShadowProperty";

        var scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

        if (useBuilder)
        {
            var result = complexBuilder.Property(typeof(int), shadowPropertyName, ConfigurationSource.Convention);

            Assert.Equal(!useScope, result == null);
        }
        else
        {
            var result = complexBuilder.Metadata.AddProperty(
                shadowPropertyName, typeof(int), ConfigurationSource.Convention, ConfigurationSource.Convention);

            Assert.Equal(!useScope, result == null);
        }

        if (useScope)
        {
            Assert.Empty(convention1.Calls);
            Assert.Empty(convention2.Calls);
            Assert.Empty(convention3.Calls);
            scope.Dispose();
        }

        Assert.Equal(new[] { shadowPropertyName }, convention1.Calls);
        Assert.Equal(new[] { shadowPropertyName }, convention2.Calls);
        Assert.Empty(convention3.Calls);

        scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

        if (useBuilder)
        {
            var result = complexBuilder.Property(nameof(OrderDetails.Id), ConfigurationSource.Convention);

            Assert.Equal(!useScope, result == null);
        }
        else
        {
            var result = ((IMutableComplexType)complexBuilder.Metadata).AddProperty(nameof(OrderDetails.Id));

            Assert.Equal(!useScope, result == null);
        }

        if (useScope)
        {
            Assert.Equal(new[] { shadowPropertyName }, convention1.Calls);
            Assert.Equal(new[] { shadowPropertyName }, convention2.Calls);
            Assert.Empty(convention3.Calls);
            scope.Dispose();
        }

        Assert.Equal(new[] { shadowPropertyName, nameof(OrderDetails.Id) }, convention1.Calls);
        Assert.Equal(new[] { shadowPropertyName, nameof(OrderDetails.Id) }, convention2.Calls);
        Assert.Empty(convention3.Calls);

        Assert.Empty(entityBuilder.Metadata.GetProperties());
    }

    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    [ConditionalTheory]
    public void OnComplexTypePropertyNullabilityChanged_calls_conventions_in_order(bool useBuilder, bool useScope)
    {
        var conventions = new ConventionSet();

        var convention1 = new PropertyNullabilityChangedConvention(false);
        var convention2 = new PropertyNullabilityChangedConvention(true);
        var convention3 = new PropertyNullabilityChangedConvention(false);
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

        var model = new Model(conventions);

        var scope = useScope ? model.DelayConventions() : null;

        var propertyBuilder = model.Builder.Entity(typeof(Order), ConfigurationSource.Convention)
            .ComplexProperty(Order.OrderDetailsProperty, complexTypeName: null, collection: false, ConfigurationSource.Convention)
            .ComplexTypeBuilder
            .Property(typeof(string), "Name", ConfigurationSource.Convention);
        if (useBuilder)
        {
            propertyBuilder.IsRequired(true, ConfigurationSource.Convention);
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

        if (useBuilder)
        {
            propertyBuilder.IsRequired(false, ConfigurationSource.Convention);
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

        if (useBuilder)
        {
            propertyBuilder.IsRequired(false, ConfigurationSource.Convention);
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

        if (useBuilder)
        {
            propertyBuilder.IsRequired(true, ConfigurationSource.Convention);
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

    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    [ConditionalTheory]
    public void OnComplexTypePropertyFieldChanged_calls_conventions_in_order(bool useBuilder, bool useScope)
    {
        var conventions = new ConventionSet();

        var convention1 = new PropertyFieldChangedConvention(terminate: false);
        var convention2 = new PropertyFieldChangedConvention(terminate: true);
        var convention3 = new PropertyFieldChangedConvention(terminate: false);
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

        var builder = new InternalModelBuilder(new Model(conventions));
        var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
        var propertyBuilder = entityBuilder
            .ComplexProperty(Order.OrderDetailsProperty, complexTypeName: null, collection: false, ConfigurationSource.Convention)
            .ComplexTypeBuilder
            .Property(nameof(OrderDetails.Id), ConfigurationSource.Convention);

        var scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

        if (useBuilder)
        {
            Assert.NotNull(propertyBuilder.HasField(nameof(OrderDetails.IntField), ConfigurationSource.Convention));
        }
        else
        {
            propertyBuilder.Metadata.SetField(
                nameof(OrderDetails.IntField),
                ConfigurationSource.Convention);
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
            Assert.NotNull(propertyBuilder.HasField(nameof(OrderDetails.IntField), ConfigurationSource.Convention));
        }
        else
        {
            propertyBuilder.Metadata.SetField(
                nameof(OrderDetails.IntField),
                ConfigurationSource.Convention);
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
            propertyBuilder.Metadata.SetFieldInfo(
                null,
                ConfigurationSource.Convention);
        }

        Assert.Equal(new[] { null, nameof(Order.IntField) }, convention1.Calls);
        Assert.Equal(new[] { null, nameof(Order.IntField) }, convention2.Calls);
        Assert.Empty(convention3.Calls);
    }

    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    [ConditionalTheory]
    public void OnComplexTypePropertyAnnotationChanged_calls_conventions_in_order(bool useBuilder, bool useScope)
    {
        var conventions = new ConventionSet();

        var convention1 = new PropertyAnnotationChangedConvention(false);
        var convention2 = new PropertyAnnotationChangedConvention(true);
        var convention3 = new PropertyAnnotationChangedConvention(false);
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

        var builder = new InternalModelBuilder(new Model(conventions));
        var propertyBuilder = builder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention)
            .ComplexProperty(Order.OrderDetailsProperty, complexTypeName: null, collection: false, ConfigurationSource.Convention)
            .ComplexTypeBuilder
            .Property(nameof(OrderDetails.Id), ConfigurationSource.Convention);

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

    [InlineData(false)]
    [InlineData(true)]
    [ConditionalTheory]
    public void OnComplexTypePropertyRemoved_calls_conventions_in_order(bool useScope)
    {
        var conventions = new ConventionSet();

        var convention1 = new PropertyRemovedConvention(terminate: false);
        var convention2 = new PropertyRemovedConvention(terminate: true);
        var convention3 = new PropertyRemovedConvention(terminate: false);
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

        var builder = new InternalModelBuilder(new Model(conventions));
        var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
        var shadowPropertyName = "ShadowProperty";
        var property = entityBuilder
            .ComplexProperty(Order.OrderDetailsProperty, complexTypeName: null, collection: false, ConfigurationSource.Convention)
            .ComplexTypeBuilder.Metadata.AddProperty(
                shadowPropertyName, typeof(int), ConfigurationSource.Convention, ConfigurationSource.Convention);

        var scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

        var result = ((ComplexType)property.DeclaringType).RemoveProperty(property);

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

    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    [ConditionalTheory]
    public void OnComplexPropertyAdded_calls_conventions_in_order(bool useBuilder, bool useScope)
    {
        var conventions = new ConventionSet();

        var convention1 = new ComplexPropertyAddedConvention(terminate: false);
        var convention2 = new ComplexPropertyAddedConvention(terminate: true);
        var convention3 = new ComplexPropertyAddedConvention(terminate: false);
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

        var builder = new InternalModelBuilder(new Model(conventions));
        var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);

        var scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

        if (useBuilder)
        {
            var result = entityBuilder.ComplexProperty(
                Order.OrderDetailsProperty, complexTypeName: null, collection: false, ConfigurationSource.Convention);

            Assert.Equal(!useScope, result == null);
        }
        else
        {
            var result = entityBuilder.Metadata.AddComplexProperty(
                Order.OrderDetailsProperty, collection: false, ConfigurationSource.Convention);

            Assert.Equal(!useScope, result == null);
        }

        if (useScope)
        {
            Assert.Empty(convention1.Calls);
            Assert.Empty(convention2.Calls);
            scope.Dispose();
        }

        Assert.Equal(new[] { Order.OrderDetailsProperty.Name }, convention1.Calls);
        Assert.Equal(new[] { Order.OrderDetailsProperty.Name }, convention2.Calls);
        Assert.Empty(convention3.Calls);

        scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

        if (useBuilder)
        {
            var result = entityBuilder.ComplexProperty(
                Order.OtherOrderDetailsProperty, complexTypeName: null, collection: false, ConfigurationSource.Convention);

            Assert.Equal(!useScope, result == null);
        }
        else
        {
            var result = ((IMutableEntityType)entityBuilder.Metadata).AddComplexProperty(
                Order.OtherOrderDetailsProperty, collection: false);

            Assert.Equal(!useScope, result == null);
        }

        if (useScope)
        {
            Assert.Equal(new[] { Order.OrderDetailsProperty.Name }, convention1.Calls);
            Assert.Equal(new[] { Order.OrderDetailsProperty.Name }, convention2.Calls);
            Assert.Empty(convention3.Calls);
            scope.Dispose();
        }

        Assert.Equal(new[] { Order.OrderDetailsProperty.Name, Order.OtherOrderDetailsProperty.Name }, convention1.Calls);
        Assert.Equal(new[] { Order.OrderDetailsProperty.Name, Order.OtherOrderDetailsProperty.Name }, convention2.Calls);
        Assert.Empty(convention3.Calls);

        Assert.Empty(entityBuilder.Metadata.GetComplexProperties());

        AssertSetOperations(new ComplexPropertyAddedConvention(terminate: true),
            conventions, conventions.ComplexPropertyAddedConventions);
    }

    private class ComplexPropertyAddedConvention(bool terminate) : IComplexPropertyAddedConvention
    {
        private readonly bool _terminate = terminate;
        public readonly List<object> Calls = [];

        public void ProcessComplexPropertyAdded(
            IConventionComplexPropertyBuilder propertyBuilder,
            IConventionContext<IConventionComplexPropertyBuilder> context)
        {
            Assert.True(propertyBuilder.Metadata.IsInModel);

            Calls.Add(propertyBuilder.Metadata.Name);

            if (_terminate)
            {
                ((IConventionEntityType)propertyBuilder.Metadata.DeclaringType).RemoveComplexProperty(propertyBuilder.Metadata.Name);
                context.StopProcessing();
            }
        }
    }

    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    [ConditionalTheory]
    public void OnComplexPropertyNullabilityChanged_calls_conventions_in_order(bool useBuilder, bool useScope)
    {
        var conventions = new ConventionSet();

        var convention1 = new ComplexPropertyNullabilityChangedConvention(false);
        var convention2 = new ComplexPropertyNullabilityChangedConvention(true);
        var convention3 = new ComplexPropertyNullabilityChangedConvention(false);
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

        var model = new Model(conventions);

        var scope = useScope ? model.DelayConventions() : null;

        var propertyBuilder = model.Builder.Entity(typeof(Order), ConfigurationSource.Convention)
            .ComplexProperty(Order.OrderDetailsProperty, complexTypeName: null, collection: false, ConfigurationSource.Convention);
        if (useBuilder)
        {
            Assert.NotNull(propertyBuilder.IsRequired(true, ConfigurationSource.Convention));
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

        if (useBuilder)
        {
            propertyBuilder.IsRequired(false, ConfigurationSource.Convention);
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

        if (useBuilder)
        {
            propertyBuilder.IsRequired(false, ConfigurationSource.Convention);
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

        if (useBuilder)
        {
            propertyBuilder.IsRequired(true, ConfigurationSource.Convention);
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

        AssertSetOperations(new ComplexPropertyNullabilityChangedConvention(terminate: true),
            conventions, conventions.ComplexPropertyNullabilityChangedConventions);
    }

    private class ComplexPropertyNullabilityChangedConvention(bool terminate) : IComplexPropertyNullabilityChangedConvention
    {
        public readonly List<bool?> Calls = [];
        private readonly bool _terminate = terminate;

        public void ProcessComplexPropertyNullabilityChanged(
            IConventionComplexPropertyBuilder propertyBuilder,
            IConventionContext<bool?> context)
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
    public void OnComplexPropertyFieldChanged_calls_conventions_in_order(bool useBuilder, bool useScope)
    {
        var conventions = new ConventionSet();

        var convention1 = new ComplexPropertyFieldChangedConvention(terminate: false);
        var convention2 = new ComplexPropertyFieldChangedConvention(terminate: true);
        var convention3 = new ComplexPropertyFieldChangedConvention(terminate: false);
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

        var builder = new InternalModelBuilder(new Model(conventions));
        var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
        var propertyBuilder = entityBuilder
            .ComplexProperty(Order.OrderDetailsProperty, complexTypeName: null, collection: false, ConfigurationSource.Convention);

        var scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

        if (useBuilder)
        {
            Assert.NotNull(propertyBuilder.HasField(nameof(Order.OrderDetailsField), ConfigurationSource.Convention));
        }
        else
        {
            propertyBuilder.Metadata.SetField(
                nameof(Order.OrderDetailsField),
                ConfigurationSource.Convention);
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
            Assert.NotNull(propertyBuilder.HasField(nameof(Order.OrderDetailsField), ConfigurationSource.Convention));
        }
        else
        {
            propertyBuilder.Metadata.SetField(
                nameof(Order.OrderDetailsField),
                ConfigurationSource.Convention);
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
            propertyBuilder.Metadata.SetFieldInfo(
                null,
                ConfigurationSource.Convention);
        }

        Assert.Equal(new[] { null, nameof(Order.OrderDetailsField) }, convention1.Calls);
        Assert.Equal(new[] { null, nameof(Order.OrderDetailsField) }, convention2.Calls);
        Assert.Empty(convention3.Calls);

        AssertSetOperations(new ComplexPropertyFieldChangedConvention(terminate: true),
            conventions, conventions.ComplexPropertyFieldChangedConventions);
    }

    private class ComplexPropertyFieldChangedConvention(bool terminate) : IComplexPropertyFieldChangedConvention
    {
        private readonly bool _terminate = terminate;
        public readonly List<object> Calls = [];

        public void ProcessComplexPropertyFieldChanged(
            IConventionComplexPropertyBuilder propertyBuilder,
            FieldInfo newFieldInfo,
            FieldInfo oldFieldInfo,
            IConventionContext<FieldInfo> context)
        {
            Assert.True(propertyBuilder.Metadata.IsInModel);

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
    public void OnComplexPropertyAnnotationChanged_calls_conventions_in_order(bool useBuilder, bool useScope)
    {
        var conventions = new ConventionSet();

        var convention1 = new ComplexPropertyAnnotationChangedConvention(false);
        var convention2 = new ComplexPropertyAnnotationChangedConvention(true);
        var convention3 = new ComplexPropertyAnnotationChangedConvention(false);
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

        var builder = new InternalModelBuilder(new Model(conventions));
        var propertyBuilder = builder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention)
            .ComplexProperty(Order.OrderDetailsProperty, complexTypeName: null, collection: false, ConfigurationSource.Convention);

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

        AssertSetOperations(new ComplexPropertyAnnotationChangedConvention(terminate: true),
            conventions, conventions.ComplexPropertyAnnotationChangedConventions);
    }

    private class ComplexPropertyAnnotationChangedConvention(bool terminate) : IComplexPropertyAnnotationChangedConvention
    {
        private readonly bool _terminate = terminate;
        public readonly List<object> Calls = [];

        public void ProcessComplexPropertyAnnotationChanged(
            IConventionComplexPropertyBuilder propertyBuilder,
            string name,
            IConventionAnnotation annotation,
            IConventionAnnotation oldAnnotation,
            IConventionContext<IConventionAnnotation> context)
        {
            Assert.True(propertyBuilder.Metadata.IsInModel);

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
    public void OnComplexPropertyRemoved_calls_conventions_in_order(bool useScope)
    {
        var conventions = new ConventionSet();

        var convention1 = new ComplexPropertyRemovedConvention(terminate: false);
        var convention2 = new ComplexPropertyRemovedConvention(terminate: true);
        var convention3 = new ComplexPropertyRemovedConvention(terminate: false);
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

        var builder = new InternalModelBuilder(new Model(conventions));
        var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
        var property = entityBuilder
            .ComplexProperty(Order.OrderDetailsProperty, complexTypeName: null, collection: false, ConfigurationSource.Convention)
            .Metadata;

        var scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

        var result = ((EntityType)property.DeclaringType).RemoveComplexProperty(property);

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

        AssertSetOperations(new ComplexPropertyRemovedConvention(terminate: true),
            conventions, conventions.ComplexPropertyRemovedConventions);
    }

    private class ComplexPropertyRemovedConvention(bool terminate) : IComplexPropertyRemovedConvention
    {
        private readonly bool _terminate = terminate;
        public readonly List<object> Calls = [];

        public void ProcessComplexPropertyRemoved(
            IConventionTypeBaseBuilder typeBaseBuilder,
            IConventionComplexProperty property,
            IConventionContext<IConventionComplexProperty> context)
        {
            Assert.NotNull(typeBaseBuilder.Metadata.Builder);

            Calls.Add(property);

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
    public void OnComplexTypeAnnotationChanged_calls_conventions_in_order(bool useBuilder, bool useScope)
    {
        var conventions = new ConventionSet();

        var convention1 = new ComplexTypeAnnotationChangedConvention(false);
        var convention2 = new ComplexTypeAnnotationChangedConvention(true);
        var convention3 = new ComplexTypeAnnotationChangedConvention(false);
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

        var builder = new InternalModelBuilder(new Model(conventions));
        var typeBuilder = builder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention)
            .ComplexProperty(Order.OrderDetailsProperty, complexTypeName: null, collection: false, ConfigurationSource.Convention)
            .ComplexTypeBuilder;

        var scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

        if (useBuilder)
        {
            Assert.NotNull(typeBuilder.HasAnnotation("foo", "bar", ConfigurationSource.Convention));
        }
        else
        {
            typeBuilder.Metadata["foo"] = "bar";
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
            Assert.NotNull(typeBuilder.HasAnnotation("foo", "bar", ConfigurationSource.Convention));
        }
        else
        {
            typeBuilder.Metadata["foo"] = "bar";
        }

        Assert.Equal(new[] { "bar" }, convention1.Calls);
        Assert.Equal(new[] { "bar" }, convention2.Calls);
        Assert.Empty(convention3.Calls);

        if (useBuilder)
        {
            Assert.NotNull(typeBuilder.HasAnnotation("foo", null, ConfigurationSource.Convention));
        }
        else
        {
            typeBuilder.Metadata.RemoveAnnotation("foo");
        }

        Assert.Equal(new[] { "bar", null }, convention1.Calls);
        Assert.Equal(new[] { "bar", null }, convention2.Calls);
        Assert.Empty(convention3.Calls);

        typeBuilder.Metadata[CoreAnnotationNames.AfterSaveBehavior] = PropertySaveBehavior.Ignore;

        Assert.Equal(new[] { "bar", null }, convention1.Calls);

        AssertSetOperations(new ComplexTypeAnnotationChangedConvention(terminate: true),
            conventions, conventions.ComplexTypeAnnotationChangedConventions);
    }

    private class ComplexTypeAnnotationChangedConvention(bool terminate) : IComplexTypeAnnotationChangedConvention
    {
        private readonly bool _terminate = terminate;
        public readonly List<object> Calls = [];

        public void ProcessComplexTypeAnnotationChanged(
            IConventionComplexTypeBuilder propertyBuilder,
            string name,
            IConventionAnnotation annotation,
            IConventionAnnotation oldAnnotation,
            IConventionContext<IConventionAnnotation> context)
        {
            Assert.True(propertyBuilder.Metadata.IsInModel);

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
    public void OnComplexTypeMemberIgnored_calls_conventions_in_order(bool useBuilder, bool useScope)
    {
        var conventions = new ConventionSet();

        var convention1 = new ComplexTypeMemberIgnoredConvention(terminate: false);
        var convention2 = new ComplexTypeMemberIgnoredConvention(terminate: true);
        var convention3 = new ComplexTypeMemberIgnoredConvention(terminate: false);
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

        var builder = new InternalModelBuilder(new Model(conventions));
        var entityBuilder = builder.Entity(typeof(Order), ConfigurationSource.Convention);
        var complexBuilder = entityBuilder.ComplexProperty(
                Order.OrderDetailsProperty, complexTypeName: null, collection: false, ConfigurationSource.Convention)
            .ComplexTypeBuilder;
        var shadowPropertyName = "ShadowProperty";

        var scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

        if (useBuilder)
        {
            var result = complexBuilder.Ignore(shadowPropertyName, ConfigurationSource.Convention);

            Assert.NotNull(result);
        }
        else
        {
            var result = complexBuilder.Metadata.AddIgnored(shadowPropertyName, ConfigurationSource.Convention);

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
            var result = complexBuilder.Ignore(shadowPropertyName, ConfigurationSource.Convention);

            Assert.NotNull(result);
        }
        else
        {
            var result = complexBuilder.Metadata.AddIgnored(shadowPropertyName, ConfigurationSource.Convention);

            Assert.NotNull(result);
        }

        Assert.Equal(new[] { shadowPropertyName }, convention1.Calls);
        Assert.Equal(new[] { shadowPropertyName }, convention2.Calls);
        Assert.Empty(convention3.Calls);
        if (useScope)
        {
            scope.Dispose();
        }

        Assert.Empty(entityBuilder.Metadata.GetIgnoredMembers());

        AssertSetOperations(new ComplexTypeMemberIgnoredConvention(terminate: true),
            conventions, conventions.ComplexTypeMemberIgnoredConventions);
    }

    private class ComplexTypeMemberIgnoredConvention(bool terminate) : IComplexTypeMemberIgnoredConvention
    {
        private readonly bool _terminate = terminate;
        public readonly List<object> Calls = [];

        public void ProcessComplexTypeMemberIgnored(
            IConventionComplexTypeBuilder complexTypeBuilder,
            string name,
            IConventionContext<string> context)
        {
            Assert.NotNull(complexTypeBuilder.Metadata.Builder);

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
    public void OnElementTypeAnnotationChanged_calls_conventions_in_order(bool useBuilder, bool useScope)
    {
        var conventions = new ConventionSet();

        var convention1 = new ElementTypeAnnotationChangedConvention(false);
        var convention2 = new ElementTypeAnnotationChangedConvention(true);
        var convention3 = new ElementTypeAnnotationChangedConvention(false);
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

        var builder = new InternalModelBuilder(new Model(conventions));
        var elementTypeBuilder = builder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention)!
            .Property(nameof(SpecialOrder.OrderIds), ConfigurationSource.Convention)!
            .SetElementType(typeof(int), ConfigurationSource.Convention)!;

        var scope = useScope ? builder.Metadata.ConventionDispatcher.DelayConventions() : null;

        if (useBuilder)
        {
            Assert.NotNull(elementTypeBuilder.HasAnnotation("foo", "bar", ConfigurationSource.Convention));
        }
        else
        {
            elementTypeBuilder.Metadata["foo"] = "bar";
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
            Assert.NotNull(elementTypeBuilder.HasAnnotation("foo", "bar", ConfigurationSource.Convention));
        }
        else
        {
            elementTypeBuilder.Metadata["foo"] = "bar";
        }

        Assert.Equal(new[] { "bar" }, convention1.Calls);
        Assert.Equal(new[] { "bar" }, convention2.Calls);
        Assert.Empty(convention3.Calls);

        if (useBuilder)
        {
            Assert.NotNull(elementTypeBuilder.HasAnnotation("foo", null, ConfigurationSource.Convention));
        }
        else
        {
            elementTypeBuilder.Metadata.RemoveAnnotation("foo");
        }

        Assert.Equal(new[] { "bar", null }, convention1.Calls);
        Assert.Equal(new[] { "bar", null }, convention2.Calls);
        Assert.Empty(convention3.Calls);

        elementTypeBuilder.Metadata[CoreAnnotationNames.AfterSaveBehavior] = PropertySaveBehavior.Ignore;

        Assert.Equal(new[] { "bar", null }, convention1.Calls);

        AssertSetOperations(new ElementTypeAnnotationChangedConvention(terminate: true),
            conventions, conventions.ElementTypeAnnotationChangedConventions);
    }

    private class ElementTypeAnnotationChangedConvention(bool terminate) : IElementTypeAnnotationChangedConvention
    {
        private readonly bool _terminate = terminate;
        public readonly List<object> Calls = [];

        public void ProcessElementTypeAnnotationChanged(
            IConventionElementTypeBuilder builder,
            string name,
            IConventionAnnotation annotation,
            IConventionAnnotation oldAnnotation,
            IConventionContext<IConventionAnnotation> context)
        {
            Assert.True(builder.Metadata.IsInModel);

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
    public void OnElementTypeNullabilityChanged_calls_conventions_in_order(bool useBuilder, bool useScope)
    {
        var conventions = new ConventionSet();

        var convention1 = new ElementTypeNullabilityChangedConvention(false);
        var convention2 = new ElementTypeNullabilityChangedConvention(true);
        var convention3 = new ElementTypeNullabilityChangedConvention(false);
        conventions.Add(convention1);
        conventions.Add(convention2);
        conventions.Add(convention3);

        var model = new Model(conventions);
        var scope = useScope ? model.DelayConventions() : null;

        var builder = new InternalModelBuilder(model);
        var elementTypeBuilder = builder.Entity(typeof(SpecialOrder), ConfigurationSource.Convention)!
            .Property(nameof(SpecialOrder.Notes), ConfigurationSource.Convention)!
            .SetElementType(typeof(string), ConfigurationSource.Convention)!;

        if (useBuilder)
        {
            elementTypeBuilder.IsRequired(true, ConfigurationSource.Convention);
        }
        else
        {
            elementTypeBuilder.Metadata.IsNullable = false;
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

        if (useBuilder)
        {
            elementTypeBuilder.IsRequired(false, ConfigurationSource.Convention);
        }
        else
        {
            elementTypeBuilder.Metadata.IsNullable = true;
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

        if (useBuilder)
        {
            elementTypeBuilder.IsRequired(false, ConfigurationSource.Convention);
        }
        else
        {
            elementTypeBuilder.Metadata.IsNullable = true;
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

        if (useBuilder)
        {
            elementTypeBuilder.IsRequired(true, ConfigurationSource.Convention);
        }
        else
        {
            elementTypeBuilder.Metadata.IsNullable = false;
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

        AssertSetOperations(new ElementTypeNullabilityChangedConvention(terminate: true),
            conventions, conventions.ElementTypeNullabilityChangedConventions);
    }

    private class ElementTypeNullabilityChangedConvention(bool terminate) : IElementTypeNullabilityChangedConvention
    {
        public readonly List<bool?> Calls = [];
        private readonly bool _terminate = terminate;

        public void ProcessElementTypeNullabilityChanged(
            IConventionElementTypeBuilder builder,
            IConventionContext<bool?> context)
        {
            Calls.Add(builder.Metadata.IsNullable);

            if (_terminate)
            {
                context.StopProcessing();
            }
        }
    }

    private static void AssertSetOperations<TConvention>(
        TConvention newConvention, ConventionSet conventions, List<TConvention> conventionList)
        where TConvention : class, IConvention
    {
        Assert.Equal(3, conventionList.Count);
        conventions.Replace(newConvention);
        Assert.Equal(3, conventionList.Count);
        Assert.All(conventionList, c => Assert.Same(newConvention, c));

        conventions.Remove(newConvention.GetType());
        Assert.Empty(conventionList);
    }

    private class Order
    {
        public static readonly PropertyInfo OrderIdProperty = typeof(Order).GetProperty(nameof(OrderId));
        public static readonly PropertyInfo OrderIdsProperty = typeof(Order).GetProperty(nameof(OrderIds));
        public static readonly PropertyInfo OrderDetailsProperty = typeof(Order).GetProperty(nameof(OrderDetails));
        public static readonly PropertyInfo OtherOrderDetailsProperty = typeof(Order).GetProperty(nameof(OtherOrderDetails));

        public readonly int IntField = 1;
        // ReSharper disable once RedundantDefaultMemberInitializer
        public readonly OrderDetails OrderDetailsField = default;

        public int OrderId { get; set; }
        public int[] OrderIds { get; set; }
        public string[] Notes { get; set; }

        public string Name { get; set; }

        public virtual OrderDetails OrderDetails { get; set; }
        public virtual OrderDetails OtherOrderDetails { get; set; }
        public virtual ICollection<Product> Products { get; set; }
    }

    private class SpecialOrder : Order;

    private class OrderDetails
    {
        public static readonly PropertyInfo OrderProperty = typeof(OrderDetails).GetProperty(nameof(Order));
        public readonly int IntField = 1;

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
