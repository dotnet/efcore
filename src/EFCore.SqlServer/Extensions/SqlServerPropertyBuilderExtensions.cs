// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     SQL Server specific extension methods for <see cref="PropertyBuilder" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
///     for more information and examples.
/// </remarks>
public static class SqlServerPropertyBuilderExtensions
{
    /// <summary>
    ///     Configures the key property to use a sequence-based hi-lo pattern to generate values for new entities,
    ///     when targeting SQL Server. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="name">The name of the sequence.</param>
    /// <param name="schema">The schema of the sequence.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static PropertyBuilder UseHiLo(
        this PropertyBuilder propertyBuilder,
        string? name = null,
        string? schema = null)
    {
        Check.NullButNotEmpty(name, nameof(name));
        Check.NullButNotEmpty(schema, nameof(schema));

        var property = propertyBuilder.Metadata;

        name ??= SqlServerModelExtensions.DefaultHiLoSequenceName;

        var model = property.DeclaringType.Model;

        if (model.FindSequence(name, schema) == null)
        {
            model.AddSequence(name, schema).IncrementBy = 10;
        }

        property.SetValueGenerationStrategy(SqlServerValueGenerationStrategy.SequenceHiLo);
        property.SetHiLoSequenceName(name);
        property.SetHiLoSequenceSchema(schema);
        property.SetIdentitySeed(null);
        property.SetIdentityIncrement(null);

        return propertyBuilder;
    }

    /// <summary>
    ///     Configures the key property to use a sequence-based hi-lo pattern to generate values for new entities,
    ///     when targeting SQL Server. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property being configured.</typeparam>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="name">The name of the sequence.</param>
    /// <param name="schema">The schema of the sequence.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static PropertyBuilder<TProperty> UseHiLo<TProperty>(
        this PropertyBuilder<TProperty> propertyBuilder,
        string? name = null,
        string? schema = null)
        => (PropertyBuilder<TProperty>)UseHiLo((PropertyBuilder)propertyBuilder, name, schema);

    /// <summary>
    ///     Configures the database sequence used for the hi-lo pattern to generate values for the key property,
    ///     when targeting SQL Server.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="name">The name of the sequence.</param>
    /// <param name="schema">The schema of the sequence.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>A builder to further configure the sequence.</returns>
    public static IConventionSequenceBuilder? HasHiLoSequence(
        this IConventionPropertyBuilder propertyBuilder,
        string? name,
        string? schema,
        bool fromDataAnnotation = false)
    {
        if (!propertyBuilder.CanSetHiLoSequence(name, schema, fromDataAnnotation))
        {
            return null;
        }

        propertyBuilder.Metadata.SetHiLoSequenceName(name, fromDataAnnotation);
        propertyBuilder.Metadata.SetHiLoSequenceSchema(schema, fromDataAnnotation);

        return name == null
            ? null
            : propertyBuilder.Metadata.DeclaringType.Model.Builder.HasSequence(name, schema, fromDataAnnotation);
    }

    /// <summary>
    ///     Returns a value indicating whether the given name and schema can be set for the hi-lo sequence.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="name">The name of the sequence.</param>
    /// <param name="schema">The schema of the sequence.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given name and schema can be set for the hi-lo sequence.</returns>
    public static bool CanSetHiLoSequence(
        this IConventionPropertyBuilder propertyBuilder,
        string? name,
        string? schema,
        bool fromDataAnnotation = false)
    {
        Check.NullButNotEmpty(name, nameof(name));
        Check.NullButNotEmpty(schema, nameof(schema));

        return propertyBuilder.CanSetAnnotation(SqlServerAnnotationNames.HiLoSequenceName, name, fromDataAnnotation)
            && propertyBuilder.CanSetAnnotation(SqlServerAnnotationNames.HiLoSequenceSchema, schema, fromDataAnnotation);
    }

    /// <summary>
    ///     Configures the key property to use a sequence-based key value generation pattern to generate values for new entities,
    ///     when targeting SQL Server. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="name">The name of the sequence.</param>
    /// <param name="schema">The schema of the sequence.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static PropertyBuilder UseSequence(
        this PropertyBuilder propertyBuilder,
        string? name = null,
        string? schema = null)
    {
        Check.NullButNotEmpty(name, nameof(name));
        Check.NullButNotEmpty(schema, nameof(schema));

        var property = propertyBuilder.Metadata;

        property.SetValueGenerationStrategy(SqlServerValueGenerationStrategy.Sequence);
        property.SetSequenceName(name);
        property.SetSequenceSchema(schema);
        property.SetHiLoSequenceName(null);
        property.SetHiLoSequenceSchema(null);
        property.SetIdentitySeed(null);
        property.SetIdentityIncrement(null);

        return propertyBuilder;
    }

    /// <summary>
    ///     Configures the key property to use a sequence-based key value generation pattern to generate values for new entities,
    ///     when targeting SQL Server. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property being configured.</typeparam>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="name">The name of the sequence.</param>
    /// <param name="schema">The schema of the sequence.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static PropertyBuilder<TProperty> UseSequence<TProperty>(
        this PropertyBuilder<TProperty> propertyBuilder,
        string? name = null,
        string? schema = null)
        => (PropertyBuilder<TProperty>)UseSequence((PropertyBuilder)propertyBuilder, name, schema);

    /// <summary>
    ///     Configures the database sequence used for the key value generation pattern to generate values for the key property,
    ///     when targeting SQL Server.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="name">The name of the sequence.</param>
    /// <param name="schema">The schema of the sequence.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>A builder to further configure the sequence.</returns>
    public static IConventionSequenceBuilder? HasSequence(
        this IConventionPropertyBuilder propertyBuilder,
        string? name,
        string? schema,
        bool fromDataAnnotation = false)
    {
        if (!propertyBuilder.CanSetSequence(name, schema, fromDataAnnotation))
        {
            return null;
        }

        propertyBuilder.Metadata.SetSequenceName(name, fromDataAnnotation);
        propertyBuilder.Metadata.SetSequenceSchema(schema, fromDataAnnotation);

        return name == null
            ? null
            : propertyBuilder.Metadata.DeclaringType.Model.Builder.HasSequence(name, schema, fromDataAnnotation);
    }

    /// <summary>
    ///     Returns a value indicating whether the given name and schema can be set for the key value generation sequence.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="name">The name of the sequence.</param>
    /// <param name="schema">The schema of the sequence.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given name and schema can be set for the key value generation sequence.</returns>
    public static bool CanSetSequence(
        this IConventionPropertyBuilder propertyBuilder,
        string? name,
        string? schema,
        bool fromDataAnnotation = false)
    {
        Check.NullButNotEmpty(name, nameof(name));
        Check.NullButNotEmpty(schema, nameof(schema));

        return propertyBuilder.CanSetAnnotation(SqlServerAnnotationNames.SequenceName, name, fromDataAnnotation)
            && propertyBuilder.CanSetAnnotation(SqlServerAnnotationNames.SequenceSchema, schema, fromDataAnnotation);
    }

    /// <summary>
    ///     Configures the key property to use the SQL Server IDENTITY feature to generate values for new entities,
    ///     when targeting SQL Server. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="seed">The value that is used for the very first row loaded into the table.</param>
    /// <param name="increment">The incremental value that is added to the identity value of the previous row that was loaded.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static PropertyBuilder UseIdentityColumn(
        this PropertyBuilder propertyBuilder,
        long seed = 1,
        int increment = 1)
    {
        var property = propertyBuilder.Metadata;
        property.SetValueGenerationStrategy(SqlServerValueGenerationStrategy.IdentityColumn);
        property.SetIdentitySeed(seed);
        property.SetIdentityIncrement(increment);
        property.SetHiLoSequenceName(null);
        property.SetHiLoSequenceSchema(null);
        property.SetSequenceName(null);
        property.SetSequenceSchema(null);

        return propertyBuilder;
    }

    /// <summary>
    ///     Configures the key property to use the SQL Server IDENTITY feature to generate values for new entities,
    ///     when targeting SQL Server. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="seed">The value that is used for the very first row loaded into the table.</param>
    /// <param name="increment">The incremental value that is added to the identity value of the previous row that was loaded.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static PropertyBuilder UseIdentityColumn(
        this PropertyBuilder propertyBuilder,
        int seed,
        int increment = 1)
        => propertyBuilder.UseIdentityColumn((long)seed, increment);

    /// <summary>
    ///     Configures the key column to use the SQL Server IDENTITY feature to generate values for new entities,
    ///     when targeting SQL Server. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="columnBuilder">The builder for the column being configured.</param>
    /// <param name="seed">The value that is used for the very first row loaded into the table.</param>
    /// <param name="increment">The incremental value that is added to the identity value of the previous row that was loaded.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ColumnBuilder UseIdentityColumn(
        this ColumnBuilder columnBuilder,
        long seed = 1,
        int increment = 1)
    {
        var overrides = columnBuilder.Overrides;
        overrides.SetValueGenerationStrategy(SqlServerValueGenerationStrategy.IdentityColumn);
        overrides.SetIdentitySeed(seed);
        overrides.SetIdentityIncrement(increment);

        return columnBuilder;
    }

    /// <summary>
    ///     Configures the key property to use the SQL Server IDENTITY feature to generate values for new entities,
    ///     when targeting SQL Server. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property being configured.</typeparam>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="seed">The value that is used for the very first row loaded into the table.</param>
    /// <param name="increment">The incremental value that is added to the identity value of the previous row that was loaded.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static PropertyBuilder<TProperty> UseIdentityColumn<TProperty>(
        this PropertyBuilder<TProperty> propertyBuilder,
        long seed = 1,
        int increment = 1)
        => (PropertyBuilder<TProperty>)UseIdentityColumn((PropertyBuilder)propertyBuilder, seed, increment);

    /// <summary>
    ///     Configures the key property to use the SQL Server IDENTITY feature to generate values for new entities,
    ///     when targeting SQL Server. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property being configured.</typeparam>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="seed">The value that is used for the very first row loaded into the table.</param>
    /// <param name="increment">The incremental value that is added to the identity value of the previous row that was loaded.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static PropertyBuilder<TProperty> UseIdentityColumn<TProperty>(
        this PropertyBuilder<TProperty> propertyBuilder,
        int seed,
        int increment = 1)
        => (PropertyBuilder<TProperty>)UseIdentityColumn((PropertyBuilder)propertyBuilder, (long)seed, increment);

    /// <summary>
    ///     Configures the key column to use the SQL Server IDENTITY feature to generate values for new entities,
    ///     when targeting SQL Server. This method sets the property to be <see cref="ValueGenerated.OnAdd" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property being configured.</typeparam>
    /// <param name="columnBuilder">The builder for the column being configured.</param>
    /// <param name="seed">The value that is used for the very first row loaded into the table.</param>
    /// <param name="increment">The incremental value that is added to the identity value of the previous row that was loaded.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ColumnBuilder<TProperty> UseIdentityColumn<TProperty>(
        this ColumnBuilder<TProperty> columnBuilder,
        long seed = 1,
        int increment = 1)
        => (ColumnBuilder<TProperty>)UseIdentityColumn((ColumnBuilder)columnBuilder, seed, increment);

    /// <summary>
    ///     Configures the seed for SQL Server IDENTITY.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="seed">The value that is used for the very first row loaded into the table.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionPropertyBuilder? HasIdentityColumnSeed(
        this IConventionPropertyBuilder propertyBuilder,
        long? seed,
        bool fromDataAnnotation = false)
    {
        if (propertyBuilder.CanSetIdentityColumnSeed(seed, fromDataAnnotation))
        {
            propertyBuilder.Metadata.SetIdentitySeed(seed, fromDataAnnotation);
            return propertyBuilder;
        }

        return null;
    }

    /// <summary>
    ///     Configures the seed for SQL Server IDENTITY for a particular table.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="seed">The value that is used for the very first row loaded into the table.</param>
    /// <param name="storeObject">The table identifier.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionPropertyBuilder? HasIdentityColumnSeed(
        this IConventionPropertyBuilder propertyBuilder,
        long? seed,
        in StoreObjectIdentifier storeObject,
        bool fromDataAnnotation = false)
    {
        if (propertyBuilder.CanSetIdentityColumnSeed(seed, storeObject, fromDataAnnotation))
        {
            propertyBuilder.Metadata.SetIdentitySeed(seed, storeObject, fromDataAnnotation);
            return propertyBuilder;
        }

        return null;
    }

    /// <summary>
    ///     Returns a value indicating whether the given value can be set as the seed for SQL Server IDENTITY.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="seed">The value that is used for the very first row loaded into the table.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given value can be set as the seed for SQL Server IDENTITY.</returns>
    public static bool CanSetIdentityColumnSeed(
        this IConventionPropertyBuilder propertyBuilder,
        long? seed,
        bool fromDataAnnotation = false)
        => propertyBuilder.CanSetAnnotation(SqlServerAnnotationNames.IdentitySeed, seed, fromDataAnnotation);

    /// <summary>
    ///     Returns a value indicating whether the given value can be set as the seed for SQL Server IDENTITY
    ///     for a particular table.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="seed">The value that is used for the very first row loaded into the table.</param>
    /// <param name="storeObject">The table identifier.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given value can be set as the seed for SQL Server IDENTITY.</returns>
    public static bool CanSetIdentityColumnSeed(
        this IConventionPropertyBuilder propertyBuilder,
        long? seed,
        in StoreObjectIdentifier storeObject,
        bool fromDataAnnotation = false)
        => propertyBuilder.Metadata.FindOverrides(storeObject)?.Builder
                .CanSetAnnotation(
                    SqlServerAnnotationNames.IdentitySeed,
                    seed,
                    fromDataAnnotation)
            ?? true;

    /// <summary>
    ///     Configures the increment for SQL Server IDENTITY.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="increment">The incremental value that is added to the identity value of the previous row that was loaded.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionPropertyBuilder? HasIdentityColumnIncrement(
        this IConventionPropertyBuilder propertyBuilder,
        int? increment,
        bool fromDataAnnotation = false)
    {
        if (propertyBuilder.CanSetIdentityColumnIncrement(increment, fromDataAnnotation))
        {
            propertyBuilder.Metadata.SetIdentityIncrement(increment, fromDataAnnotation);
            return propertyBuilder;
        }

        return null;
    }

    /// <summary>
    ///     Configures the increment for SQL Server IDENTITY for a particular table.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="increment">The incremental value that is added to the identity value of the previous row that was loaded.</param>
    /// <param name="storeObject">The table identifier.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionPropertyBuilder? HasIdentityColumnIncrement(
        this IConventionPropertyBuilder propertyBuilder,
        int? increment,
        in StoreObjectIdentifier storeObject,
        bool fromDataAnnotation = false)
    {
        if (propertyBuilder.CanSetIdentityColumnIncrement(increment, storeObject, fromDataAnnotation))
        {
            propertyBuilder.Metadata.SetIdentityIncrement(increment, storeObject, fromDataAnnotation);
            return propertyBuilder;
        }

        return null;
    }

    /// <summary>
    ///     Returns a value indicating whether the given value can be set as the increment for SQL Server IDENTITY.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="increment">The incremental value that is added to the identity value of the previous row that was loaded.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given value can be set as the default increment for SQL Server IDENTITY.</returns>
    public static bool CanSetIdentityColumnIncrement(
        this IConventionPropertyBuilder propertyBuilder,
        int? increment,
        bool fromDataAnnotation = false)
        => propertyBuilder.CanSetAnnotation(SqlServerAnnotationNames.IdentityIncrement, increment, fromDataAnnotation);

    /// <summary>
    ///     Returns a value indicating whether the given value can be set as the increment for SQL Server IDENTITY
    ///     for a particular table.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="increment">The incremental value that is added to the identity value of the previous row that was loaded.</param>
    /// <param name="storeObject">The table identifier.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given value can be set as the default increment for SQL Server IDENTITY.</returns>
    public static bool CanSetIdentityColumnIncrement(
        this IConventionPropertyBuilder propertyBuilder,
        int? increment,
        in StoreObjectIdentifier storeObject,
        bool fromDataAnnotation = false)
        => propertyBuilder.Metadata.FindOverrides(storeObject)?.Builder
                .CanSetAnnotation(
                    SqlServerAnnotationNames.IdentityIncrement,
                    increment,
                    fromDataAnnotation)
            ?? true;

    /// <summary>
    ///     Configures the value generation strategy for the key property, when targeting SQL Server.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="valueGenerationStrategy">The value generation strategy.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionPropertyBuilder? HasValueGenerationStrategy(
        this IConventionPropertyBuilder propertyBuilder,
        SqlServerValueGenerationStrategy? valueGenerationStrategy,
        bool fromDataAnnotation = false)
    {
        if (propertyBuilder.CanSetAnnotation(
                SqlServerAnnotationNames.ValueGenerationStrategy, valueGenerationStrategy, fromDataAnnotation))
        {
            propertyBuilder.Metadata.SetValueGenerationStrategy(valueGenerationStrategy, fromDataAnnotation);
            if (valueGenerationStrategy != SqlServerValueGenerationStrategy.IdentityColumn)
            {
                propertyBuilder.HasIdentityColumnSeed(null, fromDataAnnotation);
                propertyBuilder.HasIdentityColumnIncrement(null, fromDataAnnotation);
                propertyBuilder.HasSequence(null, null, fromDataAnnotation);
            }

            if (valueGenerationStrategy != SqlServerValueGenerationStrategy.SequenceHiLo)
            {
                propertyBuilder.HasHiLoSequence(null, null, fromDataAnnotation);
                propertyBuilder.HasSequence(null, null, fromDataAnnotation);
            }

            if (valueGenerationStrategy != SqlServerValueGenerationStrategy.Sequence)
            {
                propertyBuilder.HasIdentityColumnSeed(null, fromDataAnnotation);
                propertyBuilder.HasIdentityColumnIncrement(null, fromDataAnnotation);
                propertyBuilder.HasHiLoSequence(null, null, fromDataAnnotation);
            }

            return propertyBuilder;
        }

        return null;
    }

    /// <summary>
    ///     Configures the value generation strategy for the key property, when targeting SQL Server for a particular table.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="valueGenerationStrategy">The value generation strategy.</param>
    /// <param name="storeObject">The table identifier.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionPropertyBuilder? HasValueGenerationStrategy(
        this IConventionPropertyBuilder propertyBuilder,
        SqlServerValueGenerationStrategy? valueGenerationStrategy,
        in StoreObjectIdentifier storeObject,
        bool fromDataAnnotation = false)
    {
        if (propertyBuilder.CanSetValueGenerationStrategy(valueGenerationStrategy, storeObject, fromDataAnnotation))
        {
            propertyBuilder.Metadata.SetValueGenerationStrategy(valueGenerationStrategy, storeObject, fromDataAnnotation);
            if (valueGenerationStrategy != SqlServerValueGenerationStrategy.IdentityColumn)
            {
                propertyBuilder.HasIdentityColumnSeed(null, storeObject, fromDataAnnotation);
                propertyBuilder.HasIdentityColumnIncrement(null, storeObject, fromDataAnnotation);
            }

            return propertyBuilder;
        }

        return null;
    }

    /// <summary>
    ///     Returns a value indicating whether the given value can be set as the value generation strategy.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="valueGenerationStrategy">The value generation strategy.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given value can be set as the default value generation strategy.</returns>
    public static bool CanSetValueGenerationStrategy(
        this IConventionPropertyBuilder propertyBuilder,
        SqlServerValueGenerationStrategy? valueGenerationStrategy,
        bool fromDataAnnotation = false)
        => (valueGenerationStrategy == null
                || SqlServerPropertyExtensions.IsCompatibleWithValueGeneration(propertyBuilder.Metadata))
            && propertyBuilder.CanSetAnnotation(
                SqlServerAnnotationNames.ValueGenerationStrategy, valueGenerationStrategy, fromDataAnnotation);

    /// <summary>
    ///     Returns a value indicating whether the given value can be set as the value generation strategy for a particular table.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="valueGenerationStrategy">The value generation strategy.</param>
    /// <param name="storeObject">The table identifier.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given value can be set as the default value generation strategy.</returns>
    public static bool CanSetValueGenerationStrategy(
        this IConventionPropertyBuilder propertyBuilder,
        SqlServerValueGenerationStrategy? valueGenerationStrategy,
        in StoreObjectIdentifier storeObject,
        bool fromDataAnnotation = false)
        => (valueGenerationStrategy == null
                || SqlServerPropertyExtensions.IsCompatibleWithValueGeneration(propertyBuilder.Metadata))
            && (propertyBuilder.Metadata.FindOverrides(storeObject)?.Builder
                    .CanSetAnnotation(
                        SqlServerAnnotationNames.ValueGenerationStrategy,
                        valueGenerationStrategy,
                        fromDataAnnotation)
                ?? true);

    /// <summary>
    ///     Configures whether the property's column is created as sparse when targeting SQL Server.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples. Also see
    ///     <see href="https://docs.microsoft.com/sql/relational-databases/tables/use-sparse-columns">Sparse columns</see> for
    ///     general information on SQL Server sparse columns.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="sparse">A value indicating whether the property's column is created as sparse.</param>
    /// <returns>A builder to further configure the property.</returns>
    public static PropertyBuilder IsSparse(this PropertyBuilder propertyBuilder, bool sparse = true)
    {
        propertyBuilder.Metadata.SetIsSparse(sparse);

        return propertyBuilder;
    }

    /// <summary>
    ///     Configures whether the property's column is created as sparse when targeting SQL Server.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples. Also see
    ///     <see href="https://docs.microsoft.com/sql/relational-databases/tables/use-sparse-columns">Sparse columns</see> for
    ///     general information on SQL Server sparse columns.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="sparse">A value indicating whether the property's column is created as sparse.</param>
    /// <returns>A builder to further configure the property.</returns>
    public static PropertyBuilder<TProperty> IsSparse<TProperty>(
        this PropertyBuilder<TProperty> propertyBuilder,
        bool sparse = true)
        => (PropertyBuilder<TProperty>)IsSparse((PropertyBuilder)propertyBuilder, sparse);

    /// <summary>
    ///     Configures whether the property's column is created as sparse when targeting SQL Server.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples. Also see
    ///     <see href="https://docs.microsoft.com/sql/relational-databases/tables/use-sparse-columns">Sparse columns</see> for
    ///     general information on SQL Server sparse columns.
    /// </remarks>
    /// <param name="propertyBuilder">The builder for the property being configured.</param>
    /// <param name="sparse">A value indicating whether the property's column is created as sparse.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The same builder instance if the configuration was applied, <see langword="null" /> otherwise.</returns>
    public static IConventionPropertyBuilder? IsSparse(
        this IConventionPropertyBuilder propertyBuilder,
        bool? sparse,
        bool fromDataAnnotation = false)
    {
        if (propertyBuilder.CanSetIsSparse(sparse, fromDataAnnotation))
        {
            propertyBuilder.Metadata.SetIsSparse(sparse, fromDataAnnotation);

            return propertyBuilder;
        }

        return null;
    }

    /// <summary>
    ///     Returns a value indicating whether the property's column can be configured as sparse when targeting SQL Server.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples. Also see
    ///     <see href="https://docs.microsoft.com/sql/relational-databases/tables/use-sparse-columns">Sparse columns</see> for
    ///     general information on SQL Server sparse columns.
    /// </remarks>
    /// <param name="property">The builder for the property being configured.</param>
    /// <param name="sparse">A value indicating whether the property's column is created as sparse.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The same builder instance if the configuration was applied, <see langword="null" /> otherwise.</returns>
    /// <returns>
    ///     <see langword="true" /> if the property's column can be configured as sparse when targeting SQL Server.
    /// </returns>
    public static bool CanSetIsSparse(
        this IConventionPropertyBuilder property,
        bool? sparse,
        bool fromDataAnnotation = false)
        => property.CanSetAnnotation(SqlServerAnnotationNames.Sparse, sparse, fromDataAnnotation);
}
