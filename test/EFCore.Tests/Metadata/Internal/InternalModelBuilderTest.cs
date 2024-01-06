// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;

// ReSharper disable UnusedMember.Local
// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

public class InternalModelBuilderTest
{
    [ConditionalFact]
    public void Entity_returns_same_instance_for_entity_clr_type()
    {
        var model = new Model();
        var modelBuilder = CreateModelBuilder(model);

        var entityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);

        Assert.NotNull(entityBuilder);
        Assert.NotNull(model.FindEntityType(typeof(Customer)));
        Assert.Same(entityBuilder, modelBuilder.Entity(typeof(Customer).DisplayName(), ConfigurationSource.DataAnnotation));
        Assert.NotNull(model.FindEntityType(typeof(Customer)).ClrType);
    }

    [ConditionalFact]
    public void Entity_creates_new_instance_for_entity_type_name()
    {
        var model = new Model();
        var modelBuilder = CreateModelBuilder(model);

        var entityBuilder = modelBuilder.Entity(typeof(Customer).DisplayName(), ConfigurationSource.DataAnnotation);

        Assert.NotNull(entityBuilder);
        Assert.NotNull(model.FindEntityType(typeof(Customer).DisplayName()));
        Assert.NotSame(entityBuilder, modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit));
        Assert.NotNull(model.FindEntityType(typeof(Customer)).ClrType);
    }

    [ConditionalFact]
    public void Can_ignore_lower_or_equal_source_entity_type_using_entity_clr_type()
    {
        var logger = CreateTestLogger();
        var model = new Model(new ConventionSet(), new ModelDependencies(logger));
        var modelBuilder = CreateModelBuilder(model);
        modelBuilder.Entity(typeof(Customer), ConfigurationSource.Convention);

        Assert.NotNull(modelBuilder.Ignore(typeof(Customer), ConfigurationSource.DataAnnotation));

        Assert.Null(model.FindEntityType(typeof(Customer)));
        Assert.NotNull(modelBuilder.Ignore(typeof(Customer), ConfigurationSource.DataAnnotation));
        Assert.Null(modelBuilder.Entity(typeof(Customer), ConfigurationSource.DataAnnotation));

        Assert.Null(logger.Message);

        Assert.NotNull(modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit));

        Assert.NotNull(modelBuilder.Ignore(typeof(Customer), ConfigurationSource.Explicit));
        Assert.Null(model.FindEntityType(typeof(Customer)));

        Assert.Equal(
            CoreResources.LogMappedEntityTypeIgnored(logger).GenerateMessage(nameof(Customer)),
            logger.Message);
    }

    [ConditionalFact]
    public void Can_ignore_lower_or_equal_source_entity_type_using_entity_type_name()
    {
        var logger = CreateTestLogger();
        var model = new Model(new ConventionSet(), new ModelDependencies(logger));
        var modelBuilder = CreateModelBuilder(model);
        modelBuilder.Entity(typeof(Customer).FullName!, ConfigurationSource.DataAnnotation);

        Assert.NotNull(modelBuilder.Ignore(typeof(Customer).FullName!, ConfigurationSource.DataAnnotation));

        Assert.Null(model.FindEntityType(typeof(Customer).FullName!));
        Assert.NotNull(modelBuilder.Ignore(typeof(Customer).FullName!, ConfigurationSource.Explicit));
        Assert.Null(modelBuilder.Entity(typeof(Customer).FullName!, ConfigurationSource.DataAnnotation));

        Assert.Null(logger.Message);

        Assert.NotNull(modelBuilder.Entity(typeof(Customer).FullName!, ConfigurationSource.Explicit));

        Assert.NotNull(modelBuilder.Ignore(typeof(Customer).FullName!, ConfigurationSource.Explicit));
        Assert.Null(model.FindEntityType(typeof(Customer).FullName!));

        Assert.Equal(
            CoreResources.LogMappedEntityTypeIgnored(logger).GenerateMessage(nameof(Customer)),
            logger.Message);
    }

    [ConditionalFact]
    public void Cannot_ignore_higher_source_entity_type_using_entity_clr_type()
    {
        var model = new Model();
        var modelBuilder = CreateModelBuilder(model);

        Assert.NotNull(modelBuilder.Ignore(typeof(Customer), ConfigurationSource.DataAnnotation));
        Assert.Null(modelBuilder.Entity(typeof(Customer), ConfigurationSource.DataAnnotation));
        Assert.NotNull(modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit));

        Assert.Null(modelBuilder.Ignore(typeof(Customer), ConfigurationSource.DataAnnotation));

        Assert.NotNull(model.FindEntityType(typeof(Customer)));
    }

    [ConditionalFact]
    public void Cannot_ignore_higher_source_entity_type_using_entity_type_name()
    {
        var model = new Model();
        var modelBuilder = CreateModelBuilder(model);

        Assert.NotNull(modelBuilder.Ignore(typeof(Customer).FullName, ConfigurationSource.Convention));
        Assert.Null(modelBuilder.Entity(typeof(Customer).FullName, ConfigurationSource.Convention));
        Assert.NotNull(modelBuilder.Entity(typeof(Customer).FullName, ConfigurationSource.DataAnnotation));

        Assert.Null(modelBuilder.Ignore(typeof(Customer).FullName, ConfigurationSource.Convention));

        Assert.NotNull(model.FindEntityType(typeof(Customer).FullName));
    }

    [ConditionalFact]
    public void Can_ignore_existing_entity_type_using_entity_clr_type()
    {
        var logger = CreateTestLogger();
        var model = new Model(new ConventionSet(), new ModelDependencies(logger));
        var entityType = model.AddEntityType(typeof(Customer), owned: false, ConfigurationSource.Explicit);
        var modelBuilder = CreateModelBuilder(model);
        Assert.Same(entityType, modelBuilder.Entity(typeof(Customer), ConfigurationSource.Convention)!.Metadata);
        Assert.Null(modelBuilder.Ignore(typeof(Customer), ConfigurationSource.DataAnnotation));
        Assert.NotNull(model.FindEntityType(typeof(Customer)));

        Assert.Null(logger.Message);

        Assert.NotNull(modelBuilder.Ignore(typeof(Customer), ConfigurationSource.Explicit));

        Assert.Null(model.FindEntityType(typeof(Customer)));

        Assert.Equal(
            CoreResources.LogMappedEntityTypeIgnored(logger).GenerateMessage(nameof(Customer)),
            logger.Message);
    }

    [ConditionalFact]
    public void Can_ignore_existing_entity_type_using_entity_type_name()
    {
        var logger = CreateTestLogger();
        var model = new Model(new ConventionSet(), new ModelDependencies(logger));
        var entityType = model.AddEntityType(typeof(Customer).FullName!, owned: false, ConfigurationSource.Explicit);
        var modelBuilder = CreateModelBuilder(model);

        Assert.Same(entityType, modelBuilder.Entity(typeof(Customer).FullName!, ConfigurationSource.Convention)!.Metadata);
        Assert.Null(modelBuilder.Ignore(typeof(Customer).FullName!, ConfigurationSource.DataAnnotation));
        Assert.NotNull(model.FindEntityType(typeof(Customer).FullName!));

        Assert.Null(logger.Message);

        Assert.NotNull(modelBuilder.Ignore(typeof(Customer).FullName!, ConfigurationSource.Explicit));

        Assert.Null(model.FindEntityType(typeof(Customer).FullName!));

        Assert.Equal(
            CoreResources.LogMappedEntityTypeIgnored(logger).GenerateMessage(nameof(Customer)),
            logger.Message);
    }

    [ConditionalFact]
    public void Can_ignore_entity_type_referenced_from_lower_or_equal_source_foreign_key()
    {
        var modelBuilder = CreateModelBuilder();
        modelBuilder
            .Entity(typeof(Customer), ConfigurationSource.Convention)
            .PrimaryKey(new[] { Customer.IdProperty }, ConfigurationSource.Convention);
        var orderEntityTypeBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        Assert.NotNull(
            orderEntityTypeBuilder.HasRelationship(
                typeof(Customer), new[] { Order.CustomerIdProperty }, ConfigurationSource.DataAnnotation));

        Assert.NotNull(modelBuilder.Ignore(typeof(Customer), ConfigurationSource.DataAnnotation));

        Assert.Equal(typeof(Order), modelBuilder.Metadata.GetEntityTypes().Single().ClrType);
        Assert.Empty(orderEntityTypeBuilder.Metadata.GetForeignKeys());
    }

    [ConditionalFact]
    public void Can_ignore_entity_type_referencing_higher_or_equal_source_foreign_key()
    {
        var modelBuilder = CreateModelBuilder();
        var customerEntityTypeBuilder = modelBuilder
            .Entity(typeof(Customer), ConfigurationSource.Explicit)
            .PrimaryKey(new[] { Customer.IdProperty }, ConfigurationSource.Convention);
        var orderEntityTypeBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Convention);

        Assert.NotNull(
            orderEntityTypeBuilder
                .HasRelationship(typeof(Customer), new[] { Order.CustomerIdProperty }, ConfigurationSource.DataAnnotation));

        Assert.NotNull(modelBuilder.Ignore(typeof(Order), ConfigurationSource.DataAnnotation));

        Assert.Equal(typeof(Customer), modelBuilder.Metadata.GetEntityTypes().Single().ClrType);
        Assert.Empty(customerEntityTypeBuilder.Metadata.GetReferencingForeignKeys());
    }

    [ConditionalFact]
    public void Can_ignore_entity_type_with_base_and_derived_types()
    {
        var modelBuilder = CreateModelBuilder();
        var baseEntityTypeBuilder = modelBuilder.Entity(typeof(Base), ConfigurationSource.Explicit);
        var customerEntityTypeBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Convention);
        var specialCustomerEntityTypeBuilder = modelBuilder.Entity(typeof(SpecialCustomer), ConfigurationSource.Explicit);

        Assert.NotNull(customerEntityTypeBuilder.HasBaseType(baseEntityTypeBuilder.Metadata, ConfigurationSource.Convention));
        Assert.NotNull(
            specialCustomerEntityTypeBuilder.HasBaseType(customerEntityTypeBuilder.Metadata, ConfigurationSource.Convention));

        Assert.NotNull(modelBuilder.Ignore(typeof(Customer), ConfigurationSource.DataAnnotation));

        Assert.Equal(2, modelBuilder.Metadata.GetEntityTypes().Count());
        Assert.Same(baseEntityTypeBuilder.Metadata, specialCustomerEntityTypeBuilder.Metadata.BaseType);
    }

    [ConditionalFact]
    public void Cannot_ignore_entity_type_referenced_from_higher_source_foreign_key()
    {
        var modelBuilder = CreateModelBuilder();
        modelBuilder
            .Entity(typeof(Customer), ConfigurationSource.Convention)
            .PrimaryKey(new[] { Customer.IdProperty }, ConfigurationSource.Convention);
        var orderEntityTypeBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Convention);

        Assert.NotNull(
            orderEntityTypeBuilder.HasRelationship(typeof(Customer), new[] { Order.CustomerIdProperty }, ConfigurationSource.Explicit));

        Assert.Null(modelBuilder.Ignore(typeof(Customer), ConfigurationSource.DataAnnotation));

        Assert.Equal(2, modelBuilder.Metadata.GetEntityTypes().Count());
        Assert.Single(orderEntityTypeBuilder.Metadata.GetForeignKeys());
    }

    [ConditionalFact]
    public void Ignoring_an_entity_type_removes_lower_source_orphaned_entity_types()
    {
        var modelBuilder = CreateModelBuilder();
        modelBuilder
            .Entity(typeof(Customer), ConfigurationSource.Convention)
            .PrimaryKey(new[] { Customer.IdProperty }, ConfigurationSource.Convention);
        modelBuilder
            .Entity(typeof(Product), ConfigurationSource.Convention)
            .PrimaryKey(new[] { Product.IdProperty }, ConfigurationSource.Convention);

        var orderEntityTypeBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Convention);
        orderEntityTypeBuilder.HasRelationship(typeof(Customer), new[] { Order.CustomerIdProperty }, ConfigurationSource.Convention);
        orderEntityTypeBuilder.HasRelationship(typeof(Product), new[] { Order.ProductIdProperty }, ConfigurationSource.Convention);

        Assert.NotNull(modelBuilder.Ignore(typeof(Customer), ConfigurationSource.Explicit));

        Cleanup(modelBuilder);
        Assert.Empty(modelBuilder.Metadata.GetEntityTypes());
    }

    [ConditionalFact]
    public void Ignoring_an_entity_type_does_not_remove_referenced_lower_source_entity_types()
    {
        var modelBuilder = CreateModelBuilder();
        modelBuilder
            .Entity(typeof(Customer), ConfigurationSource.Convention)
            .PrimaryKey(new[] { Customer.IdProperty }, ConfigurationSource.Convention);
        modelBuilder
            .Entity(typeof(Product), ConfigurationSource.Convention)
            .PrimaryKey(new[] { Product.IdProperty }, ConfigurationSource.Convention);

        var orderEntityTypeBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
        orderEntityTypeBuilder.HasRelationship(typeof(Product), new[] { Order.ProductIdProperty }, ConfigurationSource.Convention)
            .HasNavigation(
                "Product",
                pointsToPrincipal: true,
                ConfigurationSource.Convention);

        Assert.NotNull(modelBuilder.Ignore(typeof(Customer), ConfigurationSource.DataAnnotation));

        Cleanup(modelBuilder);
        Assert.Equal(new[] { typeof(Order), typeof(Product) }, modelBuilder.Metadata.GetEntityTypes().Select(et => et.ClrType));
        Assert.Equal(typeof(Product), orderEntityTypeBuilder.Metadata.GetForeignKeys().Single().PrincipalEntityType.ClrType);
    }

    [ConditionalFact]
    public void Ignoring_an_entity_type_does_not_remove_referencing_lower_source_entity_types()
    {
        var modelBuilder = CreateModelBuilder();
        modelBuilder
            .Entity(typeof(Customer), ConfigurationSource.Convention)
            .PrimaryKey(new[] { Customer.IdProperty }, ConfigurationSource.Convention);
        modelBuilder
            .Entity(typeof(Product), ConfigurationSource.Explicit)
            .PrimaryKey(new[] { Product.IdProperty }, ConfigurationSource.Convention);

        var orderEntityTypeBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Convention);
        orderEntityTypeBuilder.HasRelationship(typeof(Product), new[] { Order.ProductIdProperty }, ConfigurationSource.Convention)
            .HasNavigation(
                "Order",
                pointsToPrincipal: false,
                ConfigurationSource.Convention);

        Assert.NotNull(modelBuilder.Ignore(typeof(Customer), ConfigurationSource.DataAnnotation));

        Cleanup(modelBuilder);
        Assert.Equal(new[] { typeof(Order), typeof(Product) }, modelBuilder.Metadata.GetEntityTypes().Select(et => et.ClrType));
        Assert.Equal(typeof(Product), orderEntityTypeBuilder.Metadata.GetForeignKeys().Single().PrincipalEntityType.ClrType);
    }

    [ConditionalFact]
    public void Can_mark_type_as_owned_type()
    {
        var model = new Model();
        var modelBuilder = CreateModelBuilder(model);

        var entityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);

        var ownedEntityTypeBuilder = modelBuilder.Entity(typeof(Details), ConfigurationSource.Convention);
        Assert.NotNull(ownedEntityTypeBuilder);

        Assert.False(model.IsOwned(typeof(Details)));
        Assert.False(ownedEntityTypeBuilder.Metadata.IsOwned());

        Assert.Null(entityBuilder.HasOwnership(typeof(Details), nameof(Customer.Details), ConfigurationSource.Convention));

        Assert.NotNull(entityBuilder.HasOwnership(typeof(Details), nameof(Customer.Details), ConfigurationSource.DataAnnotation));

        Assert.False(model.IsOwned(typeof(Details)));
        Assert.True(ownedEntityTypeBuilder.Metadata.IsOwned());

        Assert.NotNull(modelBuilder.Ignore(typeof(Details), ConfigurationSource.DataAnnotation));

        Assert.Empty(model.FindEntityTypes(typeof(Details)));

        Assert.Null(entityBuilder.HasOwnership(typeof(Details), nameof(Customer.Details), ConfigurationSource.Convention));

        Assert.Null(modelBuilder.Owned(typeof(Details), ConfigurationSource.Convention));

        Assert.NotNull(entityBuilder.HasOwnership(typeof(Details), nameof(Customer.Details), ConfigurationSource.Explicit));

        Assert.NotNull(modelBuilder.Owned(typeof(Details), ConfigurationSource.Convention));

        Assert.NotNull(modelBuilder.Owned(typeof(Details), ConfigurationSource.DataAnnotation));

        Assert.True(model.IsOwned(typeof(Details)));

        Assert.NotNull(
            modelBuilder.Entity(typeof(Product), ConfigurationSource.Explicit)
                .HasOwnership(typeof(Details), nameof(Product.Details), ConfigurationSource.Explicit));

        Assert.Null(modelBuilder.Ignore(typeof(Details), ConfigurationSource.Convention));

        Assert.Equal(2, model.FindEntityTypes(typeof(Details)).Count());

        Assert.Equal(
            CoreStrings.ClashingSharedType(typeof(Details).Name),
            Assert.Throws<InvalidOperationException>(() => modelBuilder.Entity(typeof(Details), ConfigurationSource.Explicit)).Message);

        Assert.Equal(
            CoreStrings.ClashingOwnedEntityType(typeof(Details).Name),
            Assert.Throws<InvalidOperationException>(
                ()
                    => modelBuilder.SharedTypeEntity(nameof(Details), typeof(Details), ConfigurationSource.Explicit)).Message);

        Assert.NotNull(modelBuilder.Ignore(typeof(Details), ConfigurationSource.Explicit));

        Assert.False(model.IsOwned(typeof(Details)));

        Assert.NotNull(modelBuilder.SharedTypeEntity(nameof(Details), typeof(Details), ConfigurationSource.Explicit));

        Assert.Empty(model.FindEntityTypes(typeof(Details)).Where(e => !e.HasSharedClrType));

        Assert.Null(modelBuilder.Owned(typeof(Details), ConfigurationSource.Convention));

        Assert.Equal(
            CoreStrings.ClashingNonOwnedEntityType("Details (Details)"),
            Assert.Throws<InvalidOperationException>(() => modelBuilder.Owned(typeof(Details), ConfigurationSource.Explicit)).Message);
    }

    [ConditionalFact]
    public void Can_remove_implicitly_created_join_entity_type()
    {
        var model = new Model();
        var modelBuilder = CreateModelBuilder(model);

        var manyToManyLeft = modelBuilder.Entity(typeof(ManyToManyLeft), ConfigurationSource.Convention);
        var manyToManyRight = modelBuilder.Entity(typeof(ManyToManyRight), ConfigurationSource.Convention);
        var manyToManyLeftPK = manyToManyLeft.PrimaryKey(new[] { nameof(ManyToManyLeft.Id) }, ConfigurationSource.Convention);
        var manyToManyRightPK = manyToManyRight.PrimaryKey(new[] { nameof(ManyToManyRight.Id) }, ConfigurationSource.Convention);

        var skipNavOnLeft = manyToManyLeft.HasSkipNavigation(
            new MemberIdentity(typeof(ManyToManyLeft).GetProperty(nameof(ManyToManyLeft.Rights))),
            manyToManyRight.Metadata,
            ConfigurationSource.Convention);
        var skipNavOnRight = manyToManyRight.HasSkipNavigation(
            new MemberIdentity(typeof(ManyToManyRight).GetProperty(nameof(ManyToManyRight.Lefts))),
            manyToManyLeft.Metadata,
            ConfigurationSource.Convention);
        skipNavOnLeft.HasInverse(skipNavOnRight.Metadata, ConfigurationSource.Convention);

        var joinEntityTypeBuilder =
            model.AddEntityType(
                "JoinEntity",
                typeof(Dictionary<string, object>),
                owned: false,
                ConfigurationSource.Convention).Builder;
        var leftFK = joinEntityTypeBuilder
            .HasRelationship(
                manyToManyLeft.Metadata.Name,
                new List<string> { "ManyToManyLeft_Id" },
                manyToManyLeftPK.Metadata,
                ConfigurationSource.Convention)
            .IsUnique(false, ConfigurationSource.Convention)
            .Metadata;
        var rightFK = joinEntityTypeBuilder
            .HasRelationship(
                manyToManyRight.Metadata.Name,
                new List<string> { "ManyToManyRight_Id" },
                manyToManyRightPK.Metadata,
                ConfigurationSource.Convention)
            .IsUnique(false, ConfigurationSource.Convention)
            .Metadata;
        skipNavOnLeft.HasForeignKey(leftFK, ConfigurationSource.Convention);
        skipNavOnRight.HasForeignKey(rightFK, ConfigurationSource.Convention);
        joinEntityTypeBuilder.PrimaryKey(
            leftFK.Properties.Concat(rightFK.Properties).ToList(),
            ConfigurationSource.Convention);

        var joinEntityType = joinEntityTypeBuilder.Metadata;

        Assert.NotNull(joinEntityType);
        Assert.NotNull(modelBuilder.RemoveImplicitJoinEntity(joinEntityType));

        Assert.Empty(
            model.GetEntityTypes()
                .Where(e => e.IsImplicitlyCreatedJoinEntityType));

        var leftSkipNav = manyToManyLeft.Metadata.FindDeclaredSkipNavigation(nameof(ManyToManyLeft.Rights));
        var rightSkipNav = manyToManyRight.Metadata.FindDeclaredSkipNavigation(nameof(ManyToManyRight.Lefts));

        Assert.NotNull(leftSkipNav);
        Assert.NotNull(rightSkipNav);
    }

    [ConditionalFact]
    public void Cannot_remove_manually_created_join_entity_type()
    {
        var model = new Model();
        var modelBuilder = CreateModelBuilder(model);

        var manyToManyLeft = modelBuilder.Entity(typeof(ManyToManyLeft), ConfigurationSource.Convention);
        var manyToManyRight = modelBuilder.Entity(typeof(ManyToManyRight), ConfigurationSource.Convention);
        var manyToManyJoin = modelBuilder.Entity(typeof(ManyToManyJoin), ConfigurationSource.Convention);
        var manyToManyLeftPK = manyToManyLeft.PrimaryKey(new[] { nameof(ManyToManyLeft.Id) }, ConfigurationSource.Convention);
        var manyToManyRightPK = manyToManyRight.PrimaryKey(new[] { nameof(ManyToManyRight.Id) }, ConfigurationSource.Convention);
        manyToManyJoin.PrimaryKey(
            new[] { nameof(ManyToManyJoin.LeftId), nameof(ManyToManyJoin.RightId) }, ConfigurationSource.Convention);

        var skipNavOnLeft = manyToManyLeft.HasSkipNavigation(
            new MemberIdentity(typeof(ManyToManyLeft).GetProperty(nameof(ManyToManyLeft.Rights))),
            manyToManyRight.Metadata,
            ConfigurationSource.Convention);
        var skipNavOnRight = manyToManyRight.HasSkipNavigation(
            new MemberIdentity(typeof(ManyToManyRight).GetProperty(nameof(ManyToManyRight.Lefts))),
            manyToManyLeft.Metadata,
            ConfigurationSource.Convention);
        skipNavOnLeft.HasInverse(skipNavOnRight.Metadata, ConfigurationSource.Convention);

        var leftFK = manyToManyJoin.HasRelationship(
            manyToManyLeft.Metadata.Name,
            new[] { nameof(ManyToManyJoin.LeftId) },
            manyToManyLeftPK.Metadata,
            ConfigurationSource.Convention);
        skipNavOnLeft.Metadata.SetForeignKey(leftFK.Metadata, ConfigurationSource.Convention);
        var rightFK = manyToManyJoin.HasRelationship(
            manyToManyRight.Metadata.Name,
            new[] { nameof(ManyToManyJoin.RightId) },
            manyToManyRightPK.Metadata,
            ConfigurationSource.Convention);
        skipNavOnRight.Metadata.SetForeignKey(rightFK.Metadata, ConfigurationSource.Convention);
        skipNavOnLeft.HasInverse(skipNavOnRight.Metadata, ConfigurationSource.Convention);

        var joinEntityType = skipNavOnLeft.Metadata.JoinEntityType;
        Assert.NotNull(joinEntityType);
        Assert.Same(joinEntityType, skipNavOnRight.Metadata.JoinEntityType);

        Assert.Null(modelBuilder.RemoveImplicitJoinEntity(joinEntityType));

        var leftSkipNav = manyToManyLeft.Metadata.FindDeclaredSkipNavigation(nameof(ManyToManyLeft.Rights));
        var rightSkipNav = manyToManyRight.Metadata.FindDeclaredSkipNavigation(nameof(ManyToManyRight.Lefts));
        Assert.NotNull(leftSkipNav);
        Assert.NotNull(rightSkipNav);

        Assert.Same(leftSkipNav.JoinEntityType, rightSkipNav.JoinEntityType);
        Assert.Same(manyToManyJoin.Metadata, leftSkipNav.JoinEntityType);
    }

    [ConditionalFact]
    public void Can_add_shared_type()
    {
        var model = new Model();
        var modelBuilder = CreateModelBuilder(model);

        var sharedTypeName = "SpecialDetails";

        Assert.NotNull(modelBuilder.SharedTypeEntity(sharedTypeName, typeof(Details), ConfigurationSource.Convention));

        Assert.True(model.FindEntityType(sharedTypeName).HasSharedClrType);

        Assert.Null(modelBuilder.SharedTypeEntity(sharedTypeName, typeof(Product), ConfigurationSource.Convention));

        Assert.NotNull(modelBuilder.Entity(typeof(Product), ConfigurationSource.DataAnnotation));

        Assert.Null(modelBuilder.SharedTypeEntity(typeof(Product).DisplayName(), typeof(Product), ConfigurationSource.DataAnnotation));

        Assert.NotNull(modelBuilder.SharedTypeEntity(sharedTypeName, typeof(Product), ConfigurationSource.Explicit));

        Assert.Equal(typeof(Product), model.FindEntityType(sharedTypeName).ClrType);

        Assert.Equal(
            CoreStrings.ClashingMismatchedSharedType("SpecialDetails", nameof(Product)),
            Assert.Throws<InvalidOperationException>(
                () => modelBuilder.SharedTypeEntity(sharedTypeName, typeof(Details), ConfigurationSource.Explicit)).Message);

        Assert.NotNull(modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit));

        Assert.Equal(
            CoreStrings.ClashingNonSharedType(typeof(Customer).DisplayName(), typeof(Customer).ShortDisplayName()),
            Assert.Throws<InvalidOperationException>(
                    () => modelBuilder.SharedTypeEntity(typeof(Customer).DisplayName(), typeof(Customer), ConfigurationSource.Explicit))
                .Message);
    }

    private static TestLogger<DbLoggerCategory.Model, TestLoggingDefinitions> CreateTestLogger()
        => new() { EnabledFor = LogLevel.Warning };

    private static void Cleanup(InternalModelBuilder modelBuilder)
        => new ModelCleanupConvention(CreateDependencies())
            .ProcessModelFinalizing(
                modelBuilder,
                new ConventionContext<IConventionModelBuilder>(modelBuilder.Metadata.ConventionDispatcher));

    private static ProviderConventionSetBuilderDependencies CreateDependencies()
        => InMemoryTestHelpers.Instance.CreateContextServices().GetRequiredService<ProviderConventionSetBuilderDependencies>();

    protected virtual InternalModelBuilder CreateModelBuilder(Model model = null)
        => new(model ?? new Model());

    private class Base
    {
        public int Id { get; set; }
    }

    private class Customer : Base
    {
        public static readonly PropertyInfo IdProperty = typeof(Customer).GetProperty("Id");

        public string Name { get; set; }
        public Details Details { get; set; }
    }

    private class SpecialCustomer : Customer;

    private class Order
    {
        public static readonly PropertyInfo IdProperty = typeof(Order).GetProperty("Id");
        public static readonly PropertyInfo CustomerIdProperty = typeof(Order).GetProperty("CustomerId");
        public static readonly PropertyInfo ProductIdProperty = typeof(Order).GetProperty("ProductId");

        public int Id { get; set; }
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public Details Details { get; set; }
    }

    private class Product
    {
        public static readonly PropertyInfo IdProperty = typeof(Product).GetProperty("Id");
        public int Id { get; set; }
        public Order Order { get; set; }
        public Details Details { get; set; }
    }

    private class Details
    {
        public string Name { get; set; }
    }

    private class ManyToManyLeft
    {
        public int Id { get; set; }
        public List<ManyToManyRight> Rights { get; set; }
        public List<ManyToManyRight> OtherRights { get; set; }
    }

    private class ManyToManyRight
    {
        public int Id { get; set; }
        public List<ManyToManyLeft> Lefts { get; set; }
        public List<ManyToManyLeft> OtherLefts { get; set; }
    }

    private class ManyToManyJoin
    {
        public int LeftId { get; set; }
        public int RightId { get; set; }
    }
}
