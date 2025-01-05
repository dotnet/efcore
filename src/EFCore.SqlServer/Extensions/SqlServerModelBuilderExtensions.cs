// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     SQL Server specific extension methods for <see cref="ModelBuilder" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
///     for more information and examples.
/// </remarks>
public static class SqlServerModelBuilderExtensions
{
    /// <summary>
    ///     Configures the model to use a sequence-based hi-lo pattern to generate values for key properties
    ///     marked as <see cref="ValueGenerated.OnAdd" />, when targeting SQL Server.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="name">The name of the sequence.</param>
    /// <param name="schema">The schema of the sequence.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ModelBuilder UseHiLo(
        this ModelBuilder modelBuilder,
        string? name = null,
        string? schema = null)
    {
        Check.NullButNotEmpty(name, nameof(name));
        Check.NullButNotEmpty(schema, nameof(schema));

        var model = modelBuilder.Model;

        name ??= SqlServerModelExtensions.DefaultHiLoSequenceName;

        if (model.FindSequence(name, schema) == null)
        {
            modelBuilder.HasSequence(name, schema).IncrementsBy(10);
        }

        model.SetValueGenerationStrategy(SqlServerValueGenerationStrategy.SequenceHiLo);
        model.SetHiLoSequenceName(name);
        model.SetHiLoSequenceSchema(schema);
        model.SetSequenceNameSuffix(null);
        model.SetSequenceSchema(null);
        model.SetIdentitySeed(null);
        model.SetIdentityIncrement(null);

        return modelBuilder;
    }

    /// <summary>
    ///     Configures the database sequence used for the hi-lo pattern to generate values for key properties
    ///     marked as <see cref="ValueGenerated.OnAdd" />, when targeting SQL Server.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="name">The name of the sequence.</param>
    /// <param name="schema">The schema of the sequence.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>A builder to further configure the sequence.</returns>
    public static IConventionSequenceBuilder? HasHiLoSequence(
        this IConventionModelBuilder modelBuilder,
        string? name,
        string? schema,
        bool fromDataAnnotation = false)
    {
        if (!modelBuilder.CanSetHiLoSequence(name, schema))
        {
            return null;
        }

        modelBuilder.Metadata.SetHiLoSequenceName(name, fromDataAnnotation);
        modelBuilder.Metadata.SetHiLoSequenceSchema(schema, fromDataAnnotation);

        return name == null ? null : modelBuilder.HasSequence(name, schema, fromDataAnnotation);
    }

    /// <summary>
    ///     Returns a value indicating whether the given name and schema can be set for the hi-lo sequence.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="name">The name of the sequence.</param>
    /// <param name="schema">The schema of the sequence.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given name and schema can be set for the hi-lo sequence.</returns>
    public static bool CanSetHiLoSequence(
        this IConventionModelBuilder modelBuilder,
        string? name,
        string? schema,
        bool fromDataAnnotation = false)
    {
        Check.NullButNotEmpty(name, nameof(name));
        Check.NullButNotEmpty(schema, nameof(schema));

        return modelBuilder.CanSetAnnotation(SqlServerAnnotationNames.HiLoSequenceName, name, fromDataAnnotation)
            && modelBuilder.CanSetAnnotation(SqlServerAnnotationNames.HiLoSequenceSchema, schema, fromDataAnnotation);
    }

    /// <summary>
    ///     Configures the model to use a sequence per hierarchy to generate values for key properties
    ///     marked as <see cref="ValueGenerated.OnAdd" />, when targeting SQL Server.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="nameSuffix">The name that will suffix the table name for each sequence created automatically.</param>
    /// <param name="schema">The schema of the sequence.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ModelBuilder UseKeySequences(
        this ModelBuilder modelBuilder,
        string? nameSuffix = null,
        string? schema = null)
    {
        Check.NullButNotEmpty(nameSuffix, nameof(nameSuffix));
        Check.NullButNotEmpty(schema, nameof(schema));

        var model = modelBuilder.Model;

        nameSuffix ??= SqlServerModelExtensions.DefaultSequenceNameSuffix;

        model.SetValueGenerationStrategy(SqlServerValueGenerationStrategy.Sequence);
        model.SetSequenceNameSuffix(nameSuffix);
        model.SetSequenceSchema(schema);
        model.SetHiLoSequenceName(null);
        model.SetHiLoSequenceSchema(null);
        model.SetIdentitySeed(null);
        model.SetIdentityIncrement(null);

        return modelBuilder;
    }

    /// <summary>
    ///     Configures the model to use the SQL Server IDENTITY feature to generate values for key properties
    ///     marked as <see cref="ValueGenerated.OnAdd" />, when targeting SQL Server. This is the default
    ///     behavior when targeting SQL Server.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="seed">The value that is used for the very first row loaded into the table.</param>
    /// <param name="increment">The incremental value that is added to the identity value of the previous row that was loaded.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ModelBuilder UseIdentityColumns(
        this ModelBuilder modelBuilder,
        long seed = 1,
        int increment = 1)
    {
        var model = modelBuilder.Model;

        model.SetValueGenerationStrategy(SqlServerValueGenerationStrategy.IdentityColumn);
        model.SetIdentitySeed(seed);
        model.SetIdentityIncrement(increment);
        model.SetSequenceNameSuffix(null);
        model.SetSequenceSchema(null);
        model.SetHiLoSequenceName(null);
        model.SetHiLoSequenceSchema(null);

        return modelBuilder;
    }

    /// <summary>
    ///     Configures the model to use the SQL Server IDENTITY feature to generate values for key properties
    ///     marked as <see cref="ValueGenerated.OnAdd" />, when targeting SQL Server. This is the default
    ///     behavior when targeting SQL Server.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="seed">The value that is used for the very first row loaded into the table.</param>
    /// <param name="increment">The incremental value that is added to the identity value of the previous row that was loaded.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ModelBuilder UseIdentityColumns(
        this ModelBuilder modelBuilder,
        int seed,
        int increment = 1)
        => modelBuilder.UseIdentityColumns((long)seed, increment);

    /// <summary>
    ///     Configures the default seed for SQL Server IDENTITY.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="seed">The value that is used for the very first row loaded into the table.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionModelBuilder? HasIdentityColumnSeed(
        this IConventionModelBuilder modelBuilder,
        long? seed,
        bool fromDataAnnotation = false)
    {
        if (modelBuilder.CanSetIdentityColumnSeed(seed, fromDataAnnotation))
        {
            modelBuilder.Metadata.SetIdentitySeed(seed, fromDataAnnotation);
            return modelBuilder;
        }

        return null;
    }

    /// <summary>
    ///     Returns a value indicating whether the given value can be set as the default seed for SQL Server IDENTITY.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="seed">The value that is used for the very first row loaded into the table.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given value can be set as the seed for SQL Server IDENTITY.</returns>
    public static bool CanSetIdentityColumnSeed(
        this IConventionModelBuilder modelBuilder,
        long? seed,
        bool fromDataAnnotation = false)
        => modelBuilder.CanSetAnnotation(SqlServerAnnotationNames.IdentitySeed, seed, fromDataAnnotation);

    /// <summary>
    ///     Configures the default increment for SQL Server IDENTITY.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="increment">The incremental value that is added to the identity value of the previous row that was loaded.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionModelBuilder? HasIdentityColumnIncrement(
        this IConventionModelBuilder modelBuilder,
        int? increment,
        bool fromDataAnnotation = false)
    {
        if (modelBuilder.CanSetIdentityColumnIncrement(increment, fromDataAnnotation))
        {
            modelBuilder.Metadata.SetIdentityIncrement(increment, fromDataAnnotation);
            return modelBuilder;
        }

        return null;
    }

    /// <summary>
    ///     Returns a value indicating whether the given value can be set as the default increment for SQL Server IDENTITY.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="increment">The incremental value that is added to the identity value of the previous row that was loaded.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given value can be set as the default increment for SQL Server IDENTITY.</returns>
    public static bool CanSetIdentityColumnIncrement(
        this IConventionModelBuilder modelBuilder,
        int? increment,
        bool fromDataAnnotation = false)
        => modelBuilder.CanSetAnnotation(SqlServerAnnotationNames.IdentityIncrement, increment, fromDataAnnotation);

    /// <summary>
    ///     Configures the default value generation strategy for key properties marked as <see cref="ValueGenerated.OnAdd" />,
    ///     when targeting SQL Server.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="valueGenerationStrategy">The value generation strategy.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionModelBuilder? HasValueGenerationStrategy(
        this IConventionModelBuilder modelBuilder,
        SqlServerValueGenerationStrategy? valueGenerationStrategy,
        bool fromDataAnnotation = false)
    {
        if (modelBuilder.CanSetValueGenerationStrategy(valueGenerationStrategy, fromDataAnnotation))
        {
            modelBuilder.Metadata.SetValueGenerationStrategy(valueGenerationStrategy, fromDataAnnotation);
            if (valueGenerationStrategy != SqlServerValueGenerationStrategy.IdentityColumn)
            {
                modelBuilder.HasIdentityColumnSeed(null, fromDataAnnotation);
                modelBuilder.HasIdentityColumnIncrement(null, fromDataAnnotation);
            }

            if (valueGenerationStrategy != SqlServerValueGenerationStrategy.SequenceHiLo)
            {
                modelBuilder.HasHiLoSequence(null, null, fromDataAnnotation);
            }

            if (valueGenerationStrategy != SqlServerValueGenerationStrategy.Sequence)
            {
                RemoveKeySequenceAnnotations();
            }

            return modelBuilder;
        }

        return null;

        void RemoveKeySequenceAnnotations()
        {
            if (modelBuilder.CanSetAnnotation(SqlServerAnnotationNames.SequenceNameSuffix, null)
                && modelBuilder.CanSetAnnotation(SqlServerAnnotationNames.SequenceSchema, null))
            {
                modelBuilder.Metadata.SetSequenceNameSuffix(null, fromDataAnnotation);
                modelBuilder.Metadata.SetSequenceSchema(null, fromDataAnnotation);
            }
        }
    }

    /// <summary>
    ///     Returns a value indicating whether the given value can be set as the default value generation strategy.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="valueGenerationStrategy">The value generation strategy.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given value can be set as the default value generation strategy.</returns>
    public static bool CanSetValueGenerationStrategy(
        this IConventionModelBuilder modelBuilder,
        SqlServerValueGenerationStrategy? valueGenerationStrategy,
        bool fromDataAnnotation = false)
        => modelBuilder.CanSetAnnotation(
            SqlServerAnnotationNames.ValueGenerationStrategy, valueGenerationStrategy, fromDataAnnotation);

    /// <summary>
    ///     Configures the maximum size for Azure SQL Database.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Units must be included, e.g. "100 MB". See Azure SQL Database documentation for all supported values.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///         <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///         for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="maxSize">The maximum size of the database.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ModelBuilder HasDatabaseMaxSize(this ModelBuilder modelBuilder, string maxSize)
    {
        Check.NotNull(maxSize, nameof(maxSize));

        modelBuilder.Model.SetDatabaseMaxSize(maxSize);

        return modelBuilder;
    }

    /// <summary>
    ///     Attempts to configure the maximum size for Azure SQL Database.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Units must be included, e.g. "100 MB". See Azure SQL Database documentation for all supported values.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///         <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///         for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="maxSize">The maximum size of the database.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionModelBuilder? HasDatabaseMaxSize(
        this IConventionModelBuilder modelBuilder,
        string? maxSize,
        bool fromDataAnnotation = false)
    {
        if (modelBuilder.CanSetDatabaseMaxSize(maxSize, fromDataAnnotation))
        {
            modelBuilder.Metadata.SetDatabaseMaxSize(maxSize, fromDataAnnotation);
            return modelBuilder;
        }

        return null;
    }

    /// <summary>
    ///     Returns a value indicating whether the given value can be set as the maximum size of the database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="maxSize">The maximum size of the database.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given value can be set as the maximum size of the database.</returns>
    public static bool CanSetDatabaseMaxSize(
        this IConventionModelBuilder modelBuilder,
        string? maxSize,
        bool fromDataAnnotation = false)
        => modelBuilder.CanSetAnnotation(SqlServerAnnotationNames.MaxDatabaseSize, maxSize, fromDataAnnotation);

    /// <summary>
    ///     Configures the service tier (EDITION) for Azure SQL Database as a string literal.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         See Azure SQL Database documentation for supported values.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///         <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///         for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="serviceTier">The service tier of the database as a string literal.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ModelBuilder HasServiceTier(this ModelBuilder modelBuilder, string serviceTier)
    {
        Check.NotNull(serviceTier, nameof(serviceTier));

        modelBuilder.Model.SetServiceTierSql("'" + serviceTier.Replace("'", "''") + "'");

        return modelBuilder;
    }

    /// <summary>
    ///     Configures the service tier (EDITION) for Azure SQL Database as a SQL expression.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         See Azure SQL Database documentation for supported values.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///         <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///         for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="serviceTier">The expression for the service tier of the database.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ModelBuilder HasServiceTierSql(this ModelBuilder modelBuilder, string serviceTier)
    {
        Check.NotNull(serviceTier, nameof(serviceTier));

        modelBuilder.Model.SetServiceTierSql(serviceTier);

        return modelBuilder;
    }

    /// <summary>
    ///     Attempts to configure the service tier (EDITION) for Azure SQL Database.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         See Azure SQL Database documentation for supported values.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///         <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///         for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="serviceTier">The expression for the service tier of the database.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionModelBuilder? HasServiceTierSql(
        this IConventionModelBuilder modelBuilder,
        string? serviceTier,
        bool fromDataAnnotation = false)
    {
        if (modelBuilder.CanSetServiceTierSql(serviceTier, fromDataAnnotation))
        {
            modelBuilder.Metadata.SetServiceTierSql(serviceTier, fromDataAnnotation);
            return modelBuilder;
        }

        return null;
    }

    /// <summary>
    ///     Returns a value indicating whether the given value can be set as the service tier of the database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="serviceTier">The expression for the service tier of the database.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given value can be set as the service tier of the database.</returns>
    public static bool CanSetServiceTierSql(
        this IConventionModelBuilder modelBuilder,
        string? serviceTier,
        bool fromDataAnnotation = false)
        => modelBuilder.CanSetAnnotation(SqlServerAnnotationNames.ServiceTierSql, serviceTier, fromDataAnnotation);

    /// <summary>
    ///     Configures the performance level (SERVICE_OBJECTIVE) for Azure SQL Database as a string literal.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         See Azure SQL Database documentation for supported values.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///         <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///         for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="performanceLevel">The performance level of the database as a string literal.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ModelBuilder HasPerformanceLevel(this ModelBuilder modelBuilder, string performanceLevel)
    {
        Check.NotNull(performanceLevel, nameof(performanceLevel));

        modelBuilder.Model.SetPerformanceLevelSql("'" + performanceLevel.Replace("'", "''") + "'");

        return modelBuilder;
    }

    /// <summary>
    ///     Configures the performance level (SERVICE_OBJECTIVE) for Azure SQL Database as a SQL expression.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         See Azure SQL Database documentation for supported values.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///         <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///         for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="performanceLevel">The expression for the performance level of the database.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ModelBuilder HasPerformanceLevelSql(this ModelBuilder modelBuilder, string performanceLevel)
    {
        Check.NotNull(performanceLevel, nameof(performanceLevel));

        modelBuilder.Model.SetPerformanceLevelSql(performanceLevel);

        return modelBuilder;
    }

    /// <summary>
    ///     Attempts to configure the performance level (SERVICE_OBJECTIVE) for Azure SQL Database.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         See Azure SQL Database documentation for supported values.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///         <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///         for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="performanceLevel">The expression for the performance level of the database.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionModelBuilder? HasPerformanceLevelSql(
        this IConventionModelBuilder modelBuilder,
        string? performanceLevel,
        bool fromDataAnnotation = false)
    {
        if (modelBuilder.CanSetPerformanceLevelSql(performanceLevel, fromDataAnnotation))
        {
            modelBuilder.Metadata.SetPerformanceLevelSql(performanceLevel, fromDataAnnotation);
            return modelBuilder;
        }

        return null;
    }

    /// <summary>
    ///     Returns a value indicating whether the given value can be set as the performance level of the database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="performanceLevel">The performance level of the database expression.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given value can be set as the performance level of the database.</returns>
    public static bool CanSetPerformanceLevelSql(
        this IConventionModelBuilder modelBuilder,
        string? performanceLevel,
        bool fromDataAnnotation = false)
        => modelBuilder.CanSetAnnotation(SqlServerAnnotationNames.PerformanceLevelSql, performanceLevel, fromDataAnnotation);
}
