// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore;

public class RelationalBuilderExtensionsTest
{
    [ConditionalFact]
    public void Can_set_fixed_length()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder
            .Entity<Customer>()
            .Property(e => e.Name)
            .IsFixedLength();

        var property = modelBuilder.Model.FindEntityType(typeof(Customer)).FindProperty("Name");

        Assert.True(property.IsFixedLength());

        modelBuilder
            .Entity<Customer>()
            .Property(e => e.Name)
            .IsFixedLength(false);

        Assert.False(property.IsFixedLength());
    }

    [ConditionalFact]
    public void Can_set_column_name()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder
            .Entity<Customer>()
            .Property(e => e.Name)
            .HasColumnName("Eman");

        var property = modelBuilder.Model.FindEntityType(typeof(Customer)).FindProperty("Name");

        Assert.Equal("Name", property.Name);
        Assert.Equal("Eman", property.GetColumnName());
    }

    [ConditionalFact]
    public void Can_set_column_type()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder
            .Entity<Customer>()
            .Property(e => e.Name)
            .HasColumnType("nvarchar(42)");

        var property = modelBuilder.Model.FindEntityType(typeof(Customer)).FindProperty("Name");

        Assert.Equal("nvarchar(42)", property.GetColumnType());
    }

    [ConditionalFact]
    public void Can_set_column_default_expression()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder
            .Entity<Customer>()
            .Property(e => e.Name)
            .HasDefaultValueSql("CherryCoke");

        var property = modelBuilder.Model.FindEntityType(typeof(Customer)).FindProperty("Name");

        Assert.Equal("CherryCoke", property.GetDefaultValueSql());
        Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
    }

    [ConditionalFact]
    public void Setting_column_default_expression_does_not_modify_explicitly_set_value_generated()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder
            .Entity<Customer>()
            .Property(e => e.Name)
            .ValueGeneratedOnAddOrUpdate()
            .HasDefaultValueSql("CherryCoke");

        var property = modelBuilder.Model.FindEntityType(typeof(Customer)).FindProperty("Name");

        Assert.Equal("CherryCoke", property.GetDefaultValueSql());
        Assert.Equal(ValueGenerated.OnAddOrUpdate, property.ValueGenerated);
    }

    [ConditionalFact]
    public void Can_set_column_computed_expression()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder
            .Entity<Customer>()
            .Property(e => e.Name)
            .HasComputedColumnSql("CherryCoke");

        var property = modelBuilder.Model.FindEntityType(typeof(Customer)).FindProperty("Name");

        Assert.Equal("CherryCoke", property.GetComputedColumnSql());
        Assert.Equal(ValueGenerated.OnAddOrUpdate, property.ValueGenerated);
    }

    [ConditionalFact]
    public void Setting_column_computed_expression_does_not_modify_explicitly_set_value_generated()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder
            .Entity<Customer>()
            .Property(e => e.Name)
            .ValueGeneratedNever()
            .HasComputedColumnSql("CherryCoke");

        var property = modelBuilder.Model.FindEntityType(typeof(Customer)).FindProperty("Name");

        Assert.Equal("CherryCoke", property.GetComputedColumnSql());
        Assert.Equal(ValueGenerated.Never, property.ValueGenerated);
    }

    [ConditionalFact]
    public void Can_set_column_default_value()
    {
        var modelBuilder = CreateConventionModelBuilder();
        var stringValue = "DefaultValueString";

        modelBuilder
            .Entity<Customer>()
            .Property(e => e.Name)
            .HasDefaultValue(stringValue);

        var property = modelBuilder.Model.FindEntityType(typeof(Customer)).FindProperty("Name");

        Assert.Equal(stringValue, property.GetDefaultValue());
        Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
    }

    [ConditionalFact]
    public void Can_set_column_default_value_implicit_conversion()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder
            .Entity<Customer>()
            .Property(e => e.SomeShort)
            .HasDefaultValue(7);

        var property = modelBuilder.Model.FindEntityType(typeof(Customer)).FindProperty("SomeShort");

        Assert.Equal((short)7, property.GetDefaultValue());
        Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
    }

    [ConditionalFact]
    public void Setting_column_default_value_does_not_modify_explicitly_set_value_generated()
    {
        var modelBuilder = CreateConventionModelBuilder();
        var stringValue = "DefaultValueString";

        modelBuilder
            .Entity<Customer>()
            .Property(e => e.Name)
            .ValueGeneratedOnAddOrUpdate()
            .HasDefaultValue(stringValue);

        var property = modelBuilder.Model.FindEntityType(typeof(Customer)).FindProperty("Name");

        Assert.Equal(stringValue, property.GetDefaultValue());
        Assert.Equal(ValueGenerated.OnAddOrUpdate, property.ValueGenerated);
    }

    [ConditionalFact]
    public void Can_set_column_default_value_of_enum_type()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder
            .Entity<Customer>()
            .Property(e => e.EnumValue)
            .HasDefaultValue(MyEnum.Tue);

        var property = modelBuilder.Model.FindEntityType(typeof(Customer)).FindProperty("EnumValue");

        Assert.Equal(typeof(MyEnum), property.GetDefaultValue().GetType());
        Assert.Equal(MyEnum.Tue, property.GetDefaultValue());
        Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
    }

    [ConditionalFact]
    public void Default_alternate_key_name_is_based_on_key_column_names()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder
            .Entity<Customer>()
            .HasAlternateKey(e => e.Name);

        var entityType = modelBuilder.Model.FindEntityType(typeof(Customer));
        var key = entityType.FindKey(entityType.FindProperty(nameof(Customer.Name)));

        Assert.Equal("AK_Customer_Name", key.GetName());

        modelBuilder
            .Entity<Customer>()
            .Property(e => e.Name)
            .HasColumnName("Pie");

        Assert.Equal("AK_Customer_Pie", key.GetName());

        modelBuilder
            .Entity<Customer>()
            .HasAlternateKey(e => e.Name)
            .HasName("KeyLimePie");

        Assert.Equal("KeyLimePie", key.GetName());
    }

    [ConditionalFact]
    public void Can_access_key()
    {
        var modelBuilder = CreateBuilder();
        var entityTypeBuilder = modelBuilder.Entity(typeof(Splot), ConfigurationSource.Convention);
        var idProperty = entityTypeBuilder.Property(typeof(int), "Id", ConfigurationSource.Convention).Metadata;
        var keyBuilder = entityTypeBuilder.HasKey(new[] { idProperty.Name }, ConfigurationSource.Convention);

        Assert.NotNull(keyBuilder.HasName("Splew"));
        Assert.Equal("Splew", keyBuilder.Metadata.GetName());

        Assert.NotNull(keyBuilder.HasName("Splow", fromDataAnnotation: true));
        Assert.Equal("Splow", keyBuilder.Metadata.GetName());

        Assert.Null(keyBuilder.HasName("Splod"));
        Assert.Equal("Splow", keyBuilder.Metadata.GetName());
    }

    [ConditionalFact]
    public void Default_foreign_key_name_is_based_on_fk_column_names()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder
            .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer).HasForeignKey(e => e.CustomerId);

        var foreignKey = modelBuilder.Model.FindEntityType(typeof(Order)).GetForeignKeys()
            .Single(fk => fk.PrincipalEntityType.ClrType == typeof(Customer));

        Assert.Equal("FK_Order_Customer_CustomerId", foreignKey.GetConstraintName());

        modelBuilder
            .Entity<Order>().Property(e => e.CustomerId).HasColumnName("CID");

        Assert.Equal("FK_Order_Customer_CID", foreignKey.GetConstraintName());
    }

    [ConditionalFact]
    public void Can_set_foreign_key_name_for_one_to_many()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder
            .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer)
            .HasConstraintName("LemonSupreme");

        var foreignKey = modelBuilder.Model.FindEntityType(typeof(Order)).GetForeignKeys()
            .Single(fk => fk.PrincipalEntityType.ClrType == typeof(Customer));

        Assert.Equal("LemonSupreme", foreignKey.GetConstraintName());

        modelBuilder
            .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer)
            .HasConstraintName(null);

        Assert.Equal("FK_Order_Customer_CustomerId", foreignKey.GetConstraintName());
    }

    [ConditionalFact]
    public void Can_set_foreign_key_name_for_one_to_many_with_FK_specified()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder
            .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer)
            .HasForeignKey(e => e.CustomerId)
            .HasConstraintName("LemonSupreme");

        var foreignKey = modelBuilder.Model.FindEntityType(typeof(Order)).GetForeignKeys()
            .Single(fk => fk.PrincipalEntityType.ClrType == typeof(Customer));

        Assert.Equal("LemonSupreme", foreignKey.GetConstraintName());
    }

    [ConditionalFact]
    public void Can_set_foreign_key_name_for_many_to_one()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder
            .Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders)
            .HasConstraintName("LemonSupreme");

        var foreignKey = modelBuilder.Model.FindEntityType(typeof(Order)).GetForeignKeys()
            .Single(fk => fk.PrincipalEntityType.ClrType == typeof(Customer));

        Assert.Equal("LemonSupreme", foreignKey.GetConstraintName());

        modelBuilder
            .Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders)
            .HasConstraintName(null);

        Assert.Equal("FK_Order_Customer_CustomerId", foreignKey.GetConstraintName());
    }

    [ConditionalFact]
    public void Can_set_foreign_key_name_for_many_to_one_with_FK_specified()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder
            .Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders)
            .HasForeignKey(e => e.CustomerId)
            .HasConstraintName("LemonSupreme");

        var foreignKey = modelBuilder.Model.FindEntityType(typeof(Order)).GetForeignKeys()
            .Single(fk => fk.PrincipalEntityType.ClrType == typeof(Customer));

        Assert.Equal("LemonSupreme", foreignKey.GetConstraintName());
    }

    [ConditionalFact]
    public void Can_set_foreign_key_name_for_one_to_one()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder
            .Entity<Order>().HasOne(e => e.Details).WithOne(e => e.Order)
            .HasPrincipalKey<Order>(e => e.OrderId)
            .HasConstraintName("LemonSupreme");

        var foreignKey = modelBuilder.Model.FindEntityType(typeof(OrderDetails)).GetForeignKeys().Single();

        Assert.Equal("LemonSupreme", foreignKey.GetConstraintName());

        modelBuilder
            .Entity<Order>().HasOne(e => e.Details).WithOne(e => e.Order)
            .HasConstraintName(null);

        Assert.Equal("FK_OrderDetails_Order_OrderId", foreignKey.GetConstraintName());
    }

    [ConditionalFact]
    public void Can_set_foreign_key_name_for_one_to_one_with_FK_specified()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder
            .Entity<Order>().HasOne(e => e.Details).WithOne(e => e.Order)
            .HasForeignKey<OrderDetails>(e => e.Id)
            .HasConstraintName("LemonSupreme");

        var foreignKey = modelBuilder.Model.FindEntityType(typeof(OrderDetails)).GetForeignKeys().Single();

        Assert.Equal("LemonSupreme", foreignKey.GetConstraintName());
    }

    [ConditionalFact]
    public void Can_access_index()
    {
        var modelBuilder = CreateBuilder();
        var entityTypeBuilder = modelBuilder.Entity(typeof(Splot), ConfigurationSource.Convention);
        entityTypeBuilder.Property(typeof(int), "Id", ConfigurationSource.Convention);
        var indexBuilder = entityTypeBuilder.HasIndex(new[] { "Id" }, ConfigurationSource.Convention);

        Assert.NotNull(indexBuilder.HasFilter("Splew"));
        Assert.Equal("Splew", indexBuilder.Metadata.GetFilter());

        Assert.NotNull(indexBuilder.HasFilter("Splow", fromDataAnnotation: true));
        Assert.Equal("Splow", indexBuilder.Metadata.GetFilter());

        Assert.Null(indexBuilder.HasFilter("Splod"));
        Assert.Equal("Splow", indexBuilder.Metadata.GetFilter());

        Assert.NotNull(indexBuilder.HasFilter(null, fromDataAnnotation: true));
        Assert.Null(indexBuilder.Metadata.GetFilter());

        Assert.Null(indexBuilder.HasFilter("Splod"));
        Assert.Null(indexBuilder.Metadata.GetFilter());
    }

    [ConditionalFact]
    public void Default_index_database_name_is_based_on_index_column_names()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder
            .Entity<Customer>()
            .HasIndex(e => e.Id);

        var index = modelBuilder.Model.FindEntityType(typeof(Customer)).GetIndexes().Single();

        Assert.Equal("IX_Customer_Id", index.GetDatabaseName());

        modelBuilder
            .Entity<Customer>()
            .Property(e => e.Id)
            .HasColumnName("Eendax");

        Assert.Equal("IX_Customer_Eendax", index.GetDatabaseName());
    }

    [ConditionalFact]
    public void Can_set_index_database_name()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder
            .Entity<Customer>()
            .HasIndex(e => e.Id)
            .HasDatabaseName("Eeeendeeex");

        var index = modelBuilder.Model.FindEntityType(typeof(Customer)).GetIndexes().Single();

        Assert.Equal("Eeeendeeex", index.GetDatabaseName());
    }

    [ConditionalFact]
    public void Can_write_index_filter_with_where_clauses()
    {
        var builder = CreateConventionModelBuilder();

        var returnedBuilder = builder
            .Entity<Customer>()
            .HasIndex(e => e.Id)
            .HasFilter("[Id] % 2 = 0");

        Assert.IsType<IndexBuilder<Customer>>(returnedBuilder);

        var model = builder.Model;
        var index = model.FindEntityType(typeof(Customer)).GetIndexes().Single();
        Assert.Equal("[Id] % 2 = 0", index.GetFilter());
    }

    [ConditionalFact]
    public void Can_set_table_name()
    {
        var typeBuilder = CreateBuilder().Entity(typeof(Splot), ConfigurationSource.Convention);

        Assert.NotNull(typeBuilder.ToTable("Splew"));
        Assert.Equal("Splew", typeBuilder.Metadata.GetTableName());

        Assert.NotNull(typeBuilder.ToTable("Splow", fromDataAnnotation: true));
        Assert.Equal("Splow", typeBuilder.Metadata.GetTableName());

        Assert.Null(typeBuilder.ToTable("Splod"));
        Assert.Equal("Splow", typeBuilder.Metadata.GetTableName());
    }

    [ConditionalFact]
    public void Can_set_table_name_and_schema()
    {
        var typeBuilder = CreateBuilder().Entity(typeof(Splot), ConfigurationSource.Convention);

        Assert.NotNull(typeBuilder.ToTable("Splew", "1"));
        Assert.Equal("Splew", typeBuilder.Metadata.GetTableName());
        Assert.Equal("1", typeBuilder.Metadata.GetSchema());

        Assert.NotNull(typeBuilder.ToTable("Splow", "2", fromDataAnnotation: true));
        Assert.Equal("Splow", typeBuilder.Metadata.GetTableName());
        Assert.Equal("2", typeBuilder.Metadata.GetSchema());

        Assert.Null(typeBuilder.ToTable("Splod", "3"));
        Assert.Equal("Splow", typeBuilder.Metadata.GetTableName());
        Assert.Equal("2", typeBuilder.Metadata.GetSchema());
    }

    [ConditionalFact]
    public void Can_override_existing_schema()
    {
        var typeBuilder = CreateBuilder().Entity(typeof(Splot), ConfigurationSource.Convention);

        typeBuilder.Metadata.SetSchema("Explicit");

        Assert.Null(typeBuilder.ToTable("Splod", "2", fromDataAnnotation: true));
        Assert.Equal("Splot", typeBuilder.Metadata.GetTableName());
        Assert.Equal("Explicit", typeBuilder.Metadata.GetSchema());

        Assert.NotNull(typeBuilder.ToTable("Splod", "Explicit", fromDataAnnotation: true));
        Assert.Equal("Splod", typeBuilder.Metadata.GetTableName());
        Assert.Equal("Explicit", typeBuilder.Metadata.GetSchema());

        Assert.NotNull(new EntityTypeBuilder(typeBuilder.Metadata).ToTable("Splew", "1"));
        Assert.Equal("Splew", typeBuilder.Metadata.GetTableName());
        Assert.Equal("1", typeBuilder.Metadata.GetSchema());
    }

    [ConditionalFact]
    public void Can_create_check_constraint()
    {
        var modelBuilder = CreateConventionModelBuilder();
        var entityType = modelBuilder.Entity<Customer>().Metadata;

        modelBuilder
            .Entity<Customer>()
            .ToTable(tb => tb.HasCheckConstraint("CK_Customer_AlternateId", "AlternateId > Id"));

        var checkConstraint = entityType.FindCheckConstraint("CK_Customer_AlternateId");

        Assert.NotNull(checkConstraint);
        Assert.Equal(entityType, checkConstraint.EntityType);
        Assert.Equal("CK_Customer_AlternateId", checkConstraint.Name);
        Assert.Equal("AlternateId > Id", checkConstraint.Sql);
    }

    [ConditionalFact]
    public void Can_create_check_constraint_with_duplicate_name_replaces_existing()
    {
        var modelBuilder = CreateConventionModelBuilder();
        var entityType = modelBuilder.Entity<Customer>().Metadata;

        modelBuilder
            .Entity<Customer>()
            .ToTable(tb => tb.HasCheckConstraint("CK_Customer_AlternateId", "AlternateId > Id"));

        modelBuilder
            .Entity<Customer>()
            .ToTable(tb => tb.HasCheckConstraint("CK_Customer_AlternateId", "AlternateId < Id"));

        var checkConstraint = entityType.FindCheckConstraint("CK_Customer_AlternateId");

        Assert.NotNull(checkConstraint);
        Assert.Equal(entityType, checkConstraint.EntityType);
        Assert.Equal("CK_Customer_AlternateId", checkConstraint.Name);
        Assert.Equal("AlternateId < Id", checkConstraint.Sql);
    }

    [ConditionalFact]
    public void Can_access_check_constraint()
    {
        var typeBuilder = CreateBuilder().Entity(typeof(Splot), ConfigurationSource.Convention);
        IReadOnlyEntityType entityType = typeBuilder.Metadata;

        Assert.NotNull(typeBuilder.HasCheckConstraint("Splew", "s > p"));
        Assert.Equal("Splew", entityType.GetCheckConstraints().Single().ModelName);
        Assert.Equal("s > p", entityType.GetCheckConstraints().Single().Sql);

        Assert.NotNull(typeBuilder.HasCheckConstraint("Splew", "s < p", fromDataAnnotation: true));
        Assert.Equal("Splew", entityType.GetCheckConstraints().Single().ModelName);
        Assert.Equal("s < p", entityType.GetCheckConstraints().Single().Sql);

        Assert.Null(typeBuilder.HasCheckConstraint("Splew", "s > p"));
        Assert.Equal("Splew", entityType.GetCheckConstraints().Single().ModelName);
        Assert.Equal("s < p", entityType.GetCheckConstraints().Single().Sql);
    }

    [ConditionalFact]
    public void Base_check_constraint_overrides_derived_one()
    {
        var modelBuilder = CreateBuilder();

        var derivedBuilder = modelBuilder.Entity(typeof(Splow), ConfigurationSource.Convention);
        IReadOnlyEntityType derivedEntityType = derivedBuilder.Metadata;
        derivedBuilder.HasBaseType((EntityType)null, ConfigurationSource.DataAnnotation);

        Assert.NotNull(
            derivedBuilder.HasCheckConstraint("Splew", "s < p", fromDataAnnotation: true)
                .HasName("CK_Splow", fromDataAnnotation: true));
        Assert.Equal("Splew", derivedEntityType.GetCheckConstraints().Single().ModelName);
        Assert.Equal("s < p", derivedEntityType.GetCheckConstraints().Single().Sql);
        Assert.Equal("CK_Splow", derivedEntityType.GetCheckConstraints().Single().Name);

        Assert.True(derivedBuilder.CanHaveCheckConstraint("Splew", "s < p"));
        Assert.True(derivedBuilder.CanHaveCheckConstraint("Splew", "s > p", fromDataAnnotation: true));
        Assert.False(derivedBuilder.CanHaveCheckConstraint("Splew", "s > p"));
        Assert.True(derivedBuilder.CanHaveCheckConstraint("Splot", "s > p"));

        Assert.Null(derivedBuilder.HasCheckConstraint("Splew", "s > p"));
        Assert.Equal("s < p", derivedEntityType.GetCheckConstraints().Single().Sql);

        var baseBuilder = modelBuilder.Entity(typeof(Splot), ConfigurationSource.DataAnnotation);
        IReadOnlyEntityType baseEntityType = baseBuilder.Metadata;
        Assert.Null(derivedEntityType.BaseType);
        Assert.Empty(baseEntityType.GetCheckConstraints());

        Assert.NotNull(
            baseBuilder.HasCheckConstraint("Splew", "s < p", fromDataAnnotation: true)
                .HasName("CK_Splot", fromDataAnnotation: true));
        Assert.Equal("Splew", baseEntityType.GetCheckConstraints().Single().ModelName);
        Assert.Equal("s < p", baseEntityType.GetCheckConstraints().Single().Sql);
        Assert.Equal("CK_Splot", baseEntityType.GetCheckConstraints().Single().Name);

        Assert.NotNull(derivedBuilder.HasBaseType((EntityType)baseEntityType, ConfigurationSource.DataAnnotation));

        Assert.Null(
            baseBuilder.HasCheckConstraint("Splew", "s < p", fromDataAnnotation: true)
                .HasName("CK_Splew"));
        Assert.Equal("Splew", baseEntityType.GetCheckConstraints().Single().ModelName);
        Assert.Equal("s < p", baseEntityType.GetCheckConstraints().Single().Sql);
        Assert.Equal("CK_Splot", baseEntityType.GetCheckConstraints().Single().Name);
        Assert.Empty(derivedEntityType.GetDeclaredCheckConstraints());
        Assert.Same(baseEntityType.GetCheckConstraints().Single(), derivedEntityType.GetCheckConstraints().Single());
    }

    [ConditionalFact]
    public void Base_check_constraint_overrides_derived_one_after_base_is_set()
    {
        var modelBuilder = CreateBuilder();

        var derivedBuilder = modelBuilder.Entity(typeof(Splow), ConfigurationSource.Convention);
        Assert.NotNull(derivedBuilder.HasBaseType((string)null, ConfigurationSource.DataAnnotation));
        IReadOnlyEntityType derivedEntityType = derivedBuilder.Metadata;

        Assert.NotNull(
            derivedBuilder.HasCheckConstraint("Splew", "s < p", fromDataAnnotation: true)
                .HasName("CK_Splow", fromDataAnnotation: true));
        Assert.Equal("Splew", derivedEntityType.GetCheckConstraints().Single().ModelName);
        Assert.Equal("s < p", derivedEntityType.GetCheckConstraints().Single().Sql);
        Assert.Equal("CK_Splow", derivedEntityType.GetCheckConstraints().Single().Name);

        var baseBuilder = modelBuilder.Entity(typeof(Splot), ConfigurationSource.Convention);
        IReadOnlyEntityType baseEntityType = baseBuilder.Metadata;
        Assert.Null(derivedEntityType.BaseType);

        Assert.NotNull(
            baseBuilder.HasCheckConstraint("Splew", "s < p", fromDataAnnotation: true)
                .HasName("CK_Splot", fromDataAnnotation: true));
        Assert.Equal("Splew", baseEntityType.GetCheckConstraints().Single().ModelName);
        Assert.Equal("s < p", baseEntityType.GetCheckConstraints().Single().Sql);
        Assert.Equal("CK_Splot", baseEntityType.GetCheckConstraints().Single().Name);

        Assert.NotNull(derivedBuilder.HasBaseType((EntityType)baseEntityType, ConfigurationSource.DataAnnotation));

        Assert.Null(
            baseBuilder.HasCheckConstraint("Splew", "s < p", fromDataAnnotation: true)
                .HasName("CK_Splew"));
        Assert.Equal("Splew", baseEntityType.GetCheckConstraints().Single().ModelName);
        Assert.Equal("s < p", baseEntityType.GetCheckConstraints().Single().Sql);
        Assert.Equal("CK_Splot", baseEntityType.GetCheckConstraints().Single().Name);
        Assert.Empty(derivedEntityType.GetDeclaredCheckConstraints());
        Assert.Same(baseEntityType.GetCheckConstraints().Single(), derivedEntityType.GetCheckConstraints().Single());
    }

    [ConditionalFact]
    public void Can_create_trigger()
    {
        var modelBuilder = CreateConventionModelBuilder();
        var entityType = modelBuilder.Entity<Customer>().Metadata;

        modelBuilder
            .Entity<Customer>()
            .ToTable(tb => tb.HasTrigger("Customer_Trigger"));

        var trigger = entityType.FindDeclaredTrigger("Customer_Trigger");

        Assert.NotNull(trigger);
        Assert.Same(entityType, trigger.EntityType);
        Assert.Equal("Customer_Trigger", trigger.ModelName);
        Assert.Equal("Customer_Trigger", trigger.GetDatabaseName());
    }

    [ConditionalFact]
    public void Can_create_trigger_with_duplicate_name_replaces_existing()
    {
        var modelBuilder = CreateConventionModelBuilder();
        var entityType = modelBuilder.Entity<Customer>().Metadata;

        modelBuilder
            .Entity<Customer>()
            .ToTable(tb => tb.HasTrigger("Customer_Trigger").HasDatabaseName("Table1"));

        modelBuilder
            .Entity<Customer>()
            .ToTable(tb => tb.HasTrigger("Customer_Trigger").HasDatabaseName("Table2"));

        var trigger = entityType.FindDeclaredTrigger("Customer_Trigger");

        Assert.NotNull(trigger);
        Assert.Equal(entityType, trigger.EntityType);
        Assert.Equal("Customer_Trigger", trigger.ModelName);
        Assert.Equal("Table2", trigger.GetDatabaseName());
    }

    [ConditionalFact]
    public void Can_access_trigger()
    {
        var typeBuilder = CreateBuilder().Entity(typeof(Splot), ConfigurationSource.Convention);
        IReadOnlyEntityType entityType = typeBuilder.Metadata;

        var trigger = typeBuilder.HasTrigger("Splew", ConfigurationSource.Convention);
        Assert.NotNull(trigger.HasTableName("Table1"));
        Assert.NotNull(trigger.HasTableSchema("dbo"));
        Assert.Equal("Splew", entityType.GetDeclaredTriggers().Single().ModelName);
        Assert.Equal("Table1", entityType.GetDeclaredTriggers().Single().GetTableName());
        Assert.Equal("dbo", entityType.GetDeclaredTriggers().Single().GetTableSchema());

        trigger = typeBuilder.HasTrigger("Splew", ConfigurationSource.DataAnnotation);
        Assert.NotNull(trigger.HasTableName("Table2", fromDataAnnotation: true));
        Assert.NotNull(trigger.HasTableSchema("dbo", fromDataAnnotation: true));
        Assert.Equal("Splew", entityType.GetDeclaredTriggers().Single().ModelName);
        Assert.Equal("Table2", entityType.GetDeclaredTriggers().Single().GetTableName());

        trigger = typeBuilder.HasTrigger("Splew", ConfigurationSource.Convention);
        Assert.Null(trigger.HasTableName("Table1"));
        Assert.NotNull(trigger.HasTableSchema("dbo"));
        Assert.Equal("Splew", entityType.GetDeclaredTriggers().Single().ModelName);
        Assert.Equal("Table2", entityType.GetDeclaredTriggers().Single().GetTableName());
    }

    [ConditionalFact]
    public void Can_set_discriminator_value_using_property_expression()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder
            .Entity<Customer>()
            .HasDiscriminator(b => b.Name)
            .HasValue(typeof(Customer), "1")
            .HasValue(typeof(SpecialCustomer), "2");

        var entityType = modelBuilder.Model.FindEntityType(typeof(Customer));
        Assert.Equal("Name", entityType.FindDiscriminatorProperty().Name);
        Assert.Equal(typeof(string), entityType.FindDiscriminatorProperty().ClrType);
        Assert.Equal("1", entityType.GetDiscriminatorValue());
        Assert.Equal("2", modelBuilder.Model.FindEntityType(typeof(SpecialCustomer)).GetDiscriminatorValue());
    }

    [ConditionalFact]
    public void Can_set_discriminator_value_using_property_expression_separately()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder
            .Entity<Customer>()
            .HasDiscriminator(b => b.Name)
            .HasValue("1");

        modelBuilder
            .Entity<SpecialCustomer>()
            .HasBaseType<Customer>()
            .HasDiscriminator(b => b.Name)
            .HasValue("2");

        var entityType = modelBuilder.Model.FindEntityType(typeof(Customer));
        Assert.Equal("Name", entityType.FindDiscriminatorProperty().Name);
        Assert.Equal(typeof(string), entityType.FindDiscriminatorProperty().ClrType);
        Assert.Equal("1", entityType.GetDiscriminatorValue());
        Assert.Equal("2", modelBuilder.Model.FindEntityType(typeof(SpecialCustomer)).GetDiscriminatorValue());
    }

    [ConditionalFact]
    public void Can_set_discriminator_value_using_property_name()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder
            .Entity<Customer>()
            .HasDiscriminator("Name", typeof(string))
            .HasValue(typeof(Customer), "1")
            .HasValue(typeof(SpecialCustomer), "2");

        var entityType = modelBuilder.Model.FindEntityType(typeof(Customer));
        Assert.Equal("Name", entityType.FindDiscriminatorProperty().Name);
        Assert.Equal(typeof(string), entityType.FindDiscriminatorProperty().ClrType);
        Assert.Equal("1", entityType.GetDiscriminatorValue());
        Assert.Equal("2", modelBuilder.Model.FindEntityType(typeof(SpecialCustomer)).GetDiscriminatorValue());
    }

    [ConditionalFact]
    public void Can_set_discriminator_value_using_property_name_separately()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder
            .Entity<Customer>()
            .HasDiscriminator<string>("Name")
            .HasValue("1");

        modelBuilder
            .Entity<SpecialCustomer>()
            .HasBaseType<Customer>()
            .HasDiscriminator<string>("Name")
            .HasValue("2");

        var entityType = modelBuilder.Model.FindEntityType(typeof(Customer));
        Assert.Equal("Name", entityType.FindDiscriminatorProperty().Name);
        Assert.Equal(typeof(string), entityType.FindDiscriminatorProperty().ClrType);
        Assert.Equal("1", entityType.GetDiscriminatorValue());
        Assert.Equal("2", modelBuilder.Model.FindEntityType(typeof(SpecialCustomer)).GetDiscriminatorValue());
    }

    [ConditionalFact]
    public void Can_set_discriminator_value_non_generic()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder
            .Entity(typeof(Customer))
            .HasDiscriminator("Name", typeof(string))
            .HasValue(typeof(Customer), "1")
            .HasValue(typeof(SpecialCustomer), "2");

        var entityType = modelBuilder.Model.FindEntityType(typeof(Customer));
        Assert.Equal("Name", entityType.FindDiscriminatorProperty().Name);
        Assert.Equal(typeof(string), entityType.FindDiscriminatorProperty().ClrType);
        Assert.Equal("1", entityType.GetDiscriminatorValue());
        Assert.Equal("2", modelBuilder.Model.FindEntityType(typeof(SpecialCustomer)).GetDiscriminatorValue());
    }

    [ConditionalFact]
    public void Can_set_discriminator_value_non_generic_separately()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder
            .Entity(typeof(Customer))
            .HasDiscriminator("Name", typeof(string))
            .HasValue(typeof(Customer), "1");

        modelBuilder
            .Entity(typeof(SpecialCustomer))
            .HasBaseType(typeof(Customer))
            .HasDiscriminator("Name", typeof(string))
            .HasValue(typeof(SpecialCustomer), "2");

        var entityType = modelBuilder.Model.FindEntityType(typeof(Customer));
        Assert.Equal("Name", entityType.FindDiscriminatorProperty().Name);
        Assert.Equal(typeof(string), entityType.FindDiscriminatorProperty().ClrType);
        Assert.Equal("1", entityType.GetDiscriminatorValue());
        Assert.Equal("2", modelBuilder.Model.FindEntityType(typeof(SpecialCustomer)).GetDiscriminatorValue());
    }

    [ConditionalFact]
    public void Can_set_discriminator_value_shadow_entity()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder
            .Entity(typeof(Customer).FullName)
            .HasDiscriminator("Name", typeof(string))
            .HasValue(typeof(Customer).FullName, "1")
            .HasValue(typeof(SpecialCustomer).FullName, "2");

        var entityType = modelBuilder.Model.FindEntityType(typeof(Customer));
        Assert.Equal("Name", entityType.FindDiscriminatorProperty().Name);
        Assert.Equal(typeof(string), entityType.FindDiscriminatorProperty().ClrType);
        Assert.Equal("1", entityType.GetDiscriminatorValue());
        Assert.Equal("2", modelBuilder.Model.FindEntityType(typeof(SpecialCustomer)).GetDiscriminatorValue());
    }

    [ConditionalFact]
    public void Can_set_default_discriminator_value()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder
            .Entity(typeof(Customer))
            .HasDiscriminator()
            .HasValue(typeof(Customer), "1")
            .HasValue(typeof(SpecialCustomer), "2");

        var entityType = modelBuilder.Model.FindEntityType(typeof(Customer));
        Assert.Equal("Discriminator", entityType.FindDiscriminatorProperty().Name);
        Assert.Equal(typeof(string), entityType.FindDiscriminatorProperty().ClrType);
        Assert.Equal("1", entityType.GetDiscriminatorValue());
        Assert.Equal("2", modelBuilder.Model.FindEntityType(typeof(SpecialCustomer)).GetDiscriminatorValue());
    }

    [ConditionalFact]
    public void Can_set_default_discriminator_value_separately()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder
            .Entity(typeof(Customer))
            .HasDiscriminator()
            .HasValue("1");

        modelBuilder
            .Entity(typeof(SpecialCustomer))
            .HasDiscriminator()
            .HasValue("2");

        var entityType = modelBuilder.Model.FindEntityType(typeof(Customer));
        Assert.Equal("Discriminator", entityType.FindDiscriminatorProperty().Name);
        Assert.Equal(typeof(string), entityType.FindDiscriminatorProperty().ClrType);
        Assert.Equal("1", entityType.GetDiscriminatorValue());
        Assert.Equal("2", modelBuilder.Model.FindEntityType(typeof(SpecialCustomer)).GetDiscriminatorValue());
    }

    [ConditionalFact]
    public void Can_set_schema_on_model()
    {
        var modelBuilder = CreateConventionModelBuilder();

        Assert.Null(modelBuilder.Model.GetDefaultSchema());

        modelBuilder.HasDefaultSchema("db0");

        Assert.Equal("db0", modelBuilder.Model.GetDefaultSchema());
    }

    [ConditionalFact]
    public void Model_schema_is_used_if_table_schema_not_set()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder
            .Entity<Customer>()
            .ToTable("Customizer");

        var entityType = modelBuilder.Model.FindEntityType(typeof(Customer));

        Assert.Equal("Customer", entityType.DisplayName());
        Assert.Equal("Customizer", entityType.GetTableName());
        Assert.Null(entityType.GetSchema());

        modelBuilder.HasDefaultSchema("db0");

        Assert.Equal("db0", modelBuilder.Model.GetDefaultSchema());
        Assert.Equal("Customizer", entityType.GetTableName());
        Assert.Equal("db0", entityType.GetSchema());
    }

    [ConditionalFact]
    public void Model_schema_is_not_used_if_table_schema_is_set()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.HasDefaultSchema("db0");

        modelBuilder
            .Entity<Customer>()
            .ToTable("Customizer", "db1");

        var entityType = modelBuilder.Model.FindEntityType(typeof(Customer));

        Assert.Equal("db0", modelBuilder.Model.GetDefaultSchema());
        Assert.Equal("Customer", entityType.DisplayName());
        Assert.Equal("Customizer", entityType.GetTableName());
        Assert.Equal("db1", entityType.GetSchema());
    }

    [ConditionalFact]
    public void Sequence_is_in_model_schema_if_not_specified_explicitly()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.HasDefaultSchema("Tasty");
        modelBuilder.HasSequence("Snook");

        var sequence = modelBuilder.Model.FindSequence("Snook");

        Assert.Equal("Tasty", modelBuilder.Model.GetDefaultSchema());
        ValidateSchemaNamedSequence(sequence);
    }

    [ConditionalFact]
    public void Sequence_is_not_in_model_schema_if_specified_explicitly()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.HasDefaultSchema("db0");
        modelBuilder.HasSequence("Snook", "Tasty");

        var sequence = modelBuilder.Model.FindSequence("Snook", "Tasty");

        Assert.Equal("db0", modelBuilder.Model.GetDefaultSchema());
        ValidateSchemaNamedSequence(sequence);
    }

    [ConditionalFact]
    public void Can_create_named_sequence()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.HasSequence("Snook");

        var sequence = modelBuilder.Model.FindSequence("Snook");

        ValidateNamedSequence(sequence);
    }

    private static void ValidateNamedSequence(IReadOnlySequence sequence)
    {
        Assert.Equal("Snook", sequence.Name);
        Assert.Null(sequence.Schema);
        Assert.Equal(1, sequence.IncrementBy);
        Assert.Equal(1, sequence.StartValue);
        Assert.Null(sequence.MinValue);
        Assert.Null(sequence.MaxValue);
        Assert.Same(typeof(long), sequence.Type);
    }

    [ConditionalFact]
    public void Can_create_schema_named_sequence()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.HasSequence("Snook", "Tasty");

        var sequence = modelBuilder.Model.FindSequence("Snook", "Tasty");

        ValidateSchemaNamedSequence(sequence);
    }

    private static void ValidateSchemaNamedSequence(IReadOnlySequence sequence)
    {
        Assert.Equal("Snook", sequence.Name);
        Assert.Equal("Tasty", sequence.Schema);
        Assert.Equal(1, sequence.IncrementBy);
        Assert.Equal(1, sequence.StartValue);
        Assert.Null(sequence.MinValue);
        Assert.Null(sequence.MaxValue);
        Assert.False(sequence.IsCyclic);
        Assert.True(sequence.IsCached);
        Assert.Null(sequence.CacheSize);
        Assert.Same(typeof(long), sequence.Type);
    }

    [ConditionalFact]
    public void Can_create_named_sequence_with_specific_facets()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder
            .HasSequence<int>("Snook")
            .IncrementsBy(11)
            .StartsAt(1729)
            .HasMin(111)
            .HasMax(2222)
            .IsCyclic(false)
            .UseCache(20);

        var sequence = modelBuilder.Model.FindSequence("Snook");

        ValidateNamedSpecificSequence(sequence);
    }

    [ConditionalFact]
    public void Can_create_named_sequence_with_specific_facets_non_generic()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder
            .HasSequence(typeof(int), "Snook")
            .IncrementsBy(11)
            .StartsAt(1729)
            .HasMin(111)
            .HasMax(2222)
            .IsCyclic(false)
            .UseCache(20);

        var sequence = modelBuilder.Model.FindSequence("Snook");

        ValidateNamedSpecificSequence(sequence);
    }

    [ConditionalFact]
    public void Can_create_named_sequence_with_specific_facets_using_nested_closure()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder
            .HasSequence<int>(
                "Snook", b =>
                {
                    b.IncrementsBy(11)
                        .StartsAt(1729)
                        .HasMin(111)
                        .HasMax(2222)
                        .IsCyclic(false)
                        .UseCache(20);
                });

        var sequence = modelBuilder.Model.FindSequence("Snook");

        ValidateNamedSpecificSequence(sequence);
    }

    [ConditionalFact]
    public void Can_create_named_sequence_with_specific_facets_using_nested_closure_non_generic()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder
            .HasSequence(
                typeof(int), "Snook", b =>
                {
                    b.IncrementsBy(11)
                        .StartsAt(1729)
                        .HasMin(111)
                        .HasMax(2222)
                        .IsCyclic(false)
                        .UseCache(20);
                });

        var sequence = modelBuilder.Model.FindSequence("Snook");

        ValidateNamedSpecificSequence(sequence);
    }

    private static void ValidateNamedSpecificSequence(IReadOnlySequence sequence)
    {
        Assert.Equal("Snook", sequence.Name);
        Assert.Null(sequence.Schema);
        Assert.Equal(11, sequence.IncrementBy);
        Assert.Equal(1729, sequence.StartValue);
        Assert.Equal(111, sequence.MinValue);
        Assert.Equal(2222, sequence.MaxValue);
        Assert.Same(typeof(int), sequence.Type);
        Assert.False(sequence.IsCyclic);
        Assert.True(sequence.IsCached);
        Assert.Equal(20, sequence.CacheSize);
    }

    [ConditionalFact]
    public void Can_create_schema_named_sequence_with_specific_facets()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder
            .HasSequence<int>("Snook", "Tasty")
            .IncrementsBy(11)
            .StartsAt(1729)
            .HasMin(111)
            .HasMax(2222)
            .IsCyclic(false)
            .UseCache(20);

        var sequence = modelBuilder.Model.FindSequence("Snook", "Tasty");

        ValidateSchemaNamedSpecificSequence(sequence);
    }

    [ConditionalFact]
    public void Can_create_schema_named_sequence_with_specific_facets_non_generic()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder
            .HasSequence(typeof(int), "Snook", "Tasty")
            .IncrementsBy(11)
            .StartsAt(1729)
            .HasMin(111)
            .HasMax(2222)
            .IsCyclic(false)
            .UseCache(20);

        var sequence = modelBuilder.Model.FindSequence("Snook", "Tasty");

        ValidateSchemaNamedSpecificSequence(sequence);
    }

    [ConditionalFact]
    public void Can_create_schema_named_sequence_with_specific_facets_using_nested_closure()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder
            .HasSequence<int>(
                "Snook", "Tasty", b => b.IncrementsBy(11).StartsAt(1729).HasMin(111).HasMax(2222).IsCyclic(false).UseCache(20));

        var sequence = modelBuilder.Model.FindSequence("Snook", "Tasty");

        ValidateSchemaNamedSpecificSequence(sequence);
    }

    [ConditionalFact]
    public void Can_create_schema_named_sequence_with_specific_facets_using_nested_closure_non_generic()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder
            .HasSequence(
                typeof(int), "Snook", "Tasty",
                b => b.IncrementsBy(11).StartsAt(1729).HasMin(111).HasMax(2222).IsCyclic(false).UseCache(20));

        var sequence = modelBuilder.Model.FindSequence("Snook", "Tasty");

        ValidateSchemaNamedSpecificSequence(sequence);
    }

    [ConditionalFact]
    public void Can_access_comment()
    {
        var typeBuilder = CreateBuilder().Entity(typeof(Splot), ConfigurationSource.Convention);
        var entityType = typeBuilder.Metadata;

        Assert.NotNull(typeBuilder.HasComment("My Comment"));
        Assert.Equal("My Comment", entityType.GetComment());

        Assert.NotNull(typeBuilder.HasComment("My Comment 2", fromDataAnnotation: true));
        Assert.Equal("My Comment 2", entityType.GetComment());

        Assert.Null(typeBuilder.HasComment("My Comment"));
        Assert.Equal("My Comment 2", entityType.GetComment());
    }

    [ConditionalFact]
    public void Can_create_dbFunction()
    {
        var modelBuilder = CreateConventionModelBuilder();
        var testMethod = typeof(RelationalBuilderExtensionsTest).GetTypeInfo().GetDeclaredMethod(nameof(MethodA));
        modelBuilder.HasDbFunction(testMethod);

        var dbFunc = modelBuilder.Model.FindDbFunction(testMethod) as DbFunction;

        Assert.NotNull(dbFunc);
        Assert.Equal("MethodA", dbFunc.Name);
        Assert.Null(dbFunc.Schema);
    }

    public static int MethodA(string a, int b)
        => throw new NotImplementedException();

    [ConditionalFact]
    public void Relational_entity_methods_dont_break_out_of_the_generics()
    {
        var modelBuilder = CreateConventionModelBuilder();

        AssertIsGeneric(
            modelBuilder
                .Entity<Customer>()
                .ToTable("Will"));

        AssertIsGeneric(
            modelBuilder
                .Entity<Customer>()
                .ToTable("Jay", "Simon"));
    }

    [ConditionalFact]
    public void Relational_entity_methods_have_non_generic_overloads()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder
            .Entity(typeof(Customer))
            .ToTable("Will");

        modelBuilder
            .Entity<Customer>()
            .ToTable("Jay", "Simon");
    }

    [ConditionalFact]
    public void Relational_property_methods_dont_break_out_of_the_generics()
    {
        var modelBuilder = CreateConventionModelBuilder();

        AssertIsGeneric(
            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .HasColumnName("Will"));

        AssertIsGeneric(
            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .HasColumnType("Jay"));

        AssertIsGeneric(
            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .HasDefaultValueSql("Simon"));

        AssertIsGeneric(
            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .HasComputedColumnSql("Simon"));

        AssertIsGeneric(
            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .HasDefaultValue("Neil"));
    }

    [ConditionalFact]
    public void Relational_property_methods_have_non_generic_overloads()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder
            .Entity(typeof(Customer))
            .Property(typeof(string), "Name")
            .HasColumnName("Will");

        modelBuilder
            .Entity<Customer>()
            .Property(typeof(string), "Name")
            .HasColumnName("Jay");

        modelBuilder
            .Entity<Customer>()
            .Property(typeof(string), "Name")
            .HasColumnType("Simon");

        modelBuilder
            .Entity(typeof(Customer))
            .Property(typeof(string), "Name")
            .HasColumnType("Neil");

        modelBuilder
            .Entity<Customer>()
            .Property(typeof(string), "Name")
            .HasDefaultValueSql("Simon");

        modelBuilder
            .Entity(typeof(Customer))
            .Property(typeof(string), "Name")
            .HasDefaultValueSql("Neil");

        modelBuilder
            .Entity<Customer>()
            .Property(typeof(string), "Name")
            .HasComputedColumnSql("Simon");

        modelBuilder
            .Entity(typeof(Customer))
            .Property(typeof(string), "Name")
            .HasComputedColumnSql("Neil");

        modelBuilder
            .Entity<Customer>()
            .Property(typeof(string), "Name")
            .HasDefaultValue("Simon");

        modelBuilder
            .Entity(typeof(Customer))
            .Property(typeof(string), "Name")
            .HasDefaultValue("Neil");
    }

    [ConditionalFact]
    public void Can_access_property()
    {
        var propertyBuilder = CreateBuilder()
            .Entity(typeof(Splot), ConfigurationSource.Convention)
            .Property(typeof(int), "Id", ConfigurationSource.Convention);

        Assert.NotNull(propertyBuilder.IsFixedLength(true));
        Assert.True(propertyBuilder.Metadata.IsFixedLength());
        Assert.NotNull(propertyBuilder.HasColumnName("Splew"));
        Assert.Equal("Splew", propertyBuilder.Metadata.GetColumnName());
        Assert.NotNull(propertyBuilder.HasColumnType("int"));
        Assert.Equal("int", propertyBuilder.Metadata.GetColumnType());
        Assert.NotNull(propertyBuilder.HasDefaultValue(1));
        Assert.Equal(1, propertyBuilder.Metadata.GetDefaultValue());
        Assert.NotNull(propertyBuilder.HasDefaultValueSql("2"));
        Assert.Equal("2", propertyBuilder.Metadata.GetDefaultValueSql());
        Assert.Equal(0, propertyBuilder.Metadata.GetDefaultValue());
        Assert.NotNull(propertyBuilder.HasComputedColumnSql("3"));
        Assert.Equal("3", propertyBuilder.Metadata.GetComputedColumnSql());
        Assert.Null(propertyBuilder.Metadata.GetDefaultValueSql());

        Assert.NotNull(propertyBuilder.IsFixedLength(false, fromDataAnnotation: true));
        Assert.Null(propertyBuilder.IsFixedLength(true));
        Assert.False(propertyBuilder.Metadata.IsFixedLength());
        Assert.NotNull(propertyBuilder.HasColumnName("Splow", fromDataAnnotation: true));
        Assert.Null(propertyBuilder.HasColumnName("Splod"));
        Assert.Equal("Splow", propertyBuilder.Metadata.GetColumnName());
        Assert.NotNull(propertyBuilder.HasColumnType("varchar", fromDataAnnotation: true));
        Assert.Null(propertyBuilder.HasColumnType("int"));
        Assert.Equal("varchar", propertyBuilder.Metadata.GetColumnType());
        Assert.NotNull(propertyBuilder.HasDefaultValue(0, fromDataAnnotation: true));
        Assert.Null(propertyBuilder.HasDefaultValue(1));
        Assert.Equal(0, propertyBuilder.Metadata.GetDefaultValue());
        Assert.NotNull(propertyBuilder.HasDefaultValueSql("NULL", fromDataAnnotation: true));
        Assert.Null(propertyBuilder.HasDefaultValueSql("2"));
        Assert.Equal("NULL", propertyBuilder.Metadata.GetDefaultValueSql());
        Assert.NotNull(propertyBuilder.HasComputedColumnSql("runthis()", fromDataAnnotation: true));
        Assert.Null(propertyBuilder.HasComputedColumnSql("3"));
        Assert.Equal("runthis()", propertyBuilder.Metadata.GetComputedColumnSql());
    }

    [ConditionalFact]
    public void Relational_relationship_methods_dont_break_out_of_the_generics()
    {
        var modelBuilder = CreateConventionModelBuilder();

        AssertIsGeneric(
            modelBuilder
                .Entity<Customer>().HasMany(e => e.Orders)
                .WithOne(e => e.Customer)
                .HasConstraintName("Will"));

        AssertIsGeneric(
            modelBuilder
                .Entity<Order>()
                .HasOne(e => e.Customer)
                .WithMany(e => e.Orders)
                .HasConstraintName("Jay"));

        AssertIsGeneric(
            modelBuilder
                .Entity<Order>()
                .HasOne(e => e.Details)
                .WithOne(e => e.Order)
                .HasConstraintName("Simon"));
    }

    [ConditionalFact]
    public void Relational_relationship_methods_have_non_generic_overloads()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder
            .Entity<Customer>().HasMany(typeof(Order), "Orders")
            .WithOne("Customer")
            .HasConstraintName("Will");

        modelBuilder
            .Entity<Order>()
            .HasOne(e => e.Customer)
            .WithMany(e => e.Orders)
            .HasConstraintName("Jay");

        modelBuilder
            .Entity<Order>()
            .HasOne(e => e.Details)
            .WithOne(e => e.Order)
            .HasConstraintName("Simon");
    }

    [ConditionalFact]
    public void Can_access_relationship()
    {
        var modelBuilder = CreateBuilder();
        var entityTypeBuilder = modelBuilder.Entity(typeof(Splot), ConfigurationSource.Convention);
        entityTypeBuilder.Property(typeof(int), "Id", ConfigurationSource.Convention);
        var relationshipBuilder = entityTypeBuilder.HasRelationship("Splot", new[] { "Id" }, ConfigurationSource.Convention);

        Assert.NotNull(relationshipBuilder.HasConstraintName("Splew"));
        Assert.Equal("Splew", relationshipBuilder.Metadata.GetConstraintName());

        Assert.NotNull(relationshipBuilder.HasConstraintName("Splow", fromDataAnnotation: true));
        Assert.Equal("Splow", relationshipBuilder.Metadata.GetConstraintName());

        Assert.Null(relationshipBuilder.HasConstraintName("Splod"));
        Assert.Equal("Splow", relationshipBuilder.Metadata.GetConstraintName());
    }

    private void AssertIsGeneric(EntityTypeBuilder<Customer> _)
    {
    }

    private void AssertIsGeneric(PropertyBuilder<string> _)
    {
    }

    private void AssertIsGeneric(ReferenceCollectionBuilder<Customer, Order> _)
    {
    }

    private void AssertIsGeneric(ReferenceReferenceBuilder<Order, OrderDetails> _)
    {
    }

    protected virtual ModelBuilder CreateConventionModelBuilder()
        => FakeRelationalTestHelpers.Instance.CreateConventionBuilder();

    private InternalModelBuilder CreateBuilder()
        => (InternalModelBuilder)CreateConventionModelBuilder().GetInfrastructure();

    private static void ValidateSchemaNamedSpecificSequence(IReadOnlySequence sequence)
    {
        Assert.Equal("Snook", sequence.Name);
        Assert.Equal("Tasty", sequence.Schema);
        Assert.Equal(11, sequence.IncrementBy);
        Assert.Equal(1729, sequence.StartValue);
        Assert.Equal(111, sequence.MinValue);
        Assert.Equal(2222, sequence.MaxValue);
        Assert.False(sequence.IsCyclic);
        Assert.True(sequence.IsCached);
        Assert.Equal(20, sequence.CacheSize);
        Assert.Same(typeof(int), sequence.Type);
    }

    private enum MyEnum : ulong
    {
        Sun,
        Mon,
        Tue
    }

    private class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public short SomeShort { get; set; }
        public MyEnum EnumValue { get; set; }

        public IEnumerable<Order> Orders { get; set; }
    }

    private class SpecialCustomer : Customer;

    private class Order
    {
        public int OrderId { get; set; }

        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        public OrderDetails Details { get; set; }
    }

    private class OrderDetails
    {
        public int Id { get; }

        public int OrderId { get; set; }
        public Order Order { get; }
    }

    private class Splot
    {
        public static readonly PropertyInfo SplowedProperty = typeof(Splot).GetProperty("Splowed");

        public int? Splowed { get; set; }
    }

    private class Splow : Splot;

    private class Splod : Splow;
}
