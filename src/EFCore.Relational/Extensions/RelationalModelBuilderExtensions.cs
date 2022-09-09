// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Relational database specific extension methods for <see cref="ModelBuilder" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public static class RelationalModelBuilderExtensions
{
    /// <summary>
    ///     Configures a database sequence when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-sequences">Database sequences</see> for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="name">The name of the sequence.</param>
    /// <param name="schema">The schema of the sequence.</param>
    /// <returns>A builder to further configure the sequence.</returns>
    public static SequenceBuilder HasSequence(
        this ModelBuilder modelBuilder,
        string name,
        string? schema = null)
        => new(
            HasSequence(
                modelBuilder.Model,
                name,
                schema,
                ConfigurationSource.Explicit));

    /// <summary>
    ///     Configures a database sequence when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-sequences">Database sequences</see> for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="name">The name of the sequence.</param>
    /// <param name="builderAction">An action that performs configuration of the sequence.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ModelBuilder HasSequence(
        this ModelBuilder modelBuilder,
        string name,
        Action<SequenceBuilder> builderAction)
        => modelBuilder.HasSequence(name, null, builderAction);

    /// <summary>
    ///     Configures a database sequence when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-sequences">Database sequences</see> for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="name">The name of the sequence.</param>
    /// <param name="schema">The schema of the sequence.</param>
    /// <param name="builderAction">An action that performs configuration of the sequence.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ModelBuilder HasSequence(
        this ModelBuilder modelBuilder,
        string name,
        string? schema,
        Action<SequenceBuilder> builderAction)
    {
        Check.NotNull(builderAction, nameof(builderAction));

        builderAction(HasSequence(modelBuilder, name, schema));

        return modelBuilder;
    }

    /// <summary>
    ///     Configures a database sequence when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-sequences">Database sequences</see> for more information and examples.
    /// </remarks>
    /// <param name="type">The type of values the sequence will generate.</param>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="name">The name of the sequence.</param>
    /// <param name="schema">The schema of the sequence.</param>
    /// <returns>A builder to further configure the sequence.</returns>
    public static SequenceBuilder HasSequence(
        this ModelBuilder modelBuilder,
        Type type,
        string name,
        string? schema = null)
    {
        Check.NotNull(type, nameof(type));

        var sequence = HasSequence(modelBuilder.Model, name, schema, ConfigurationSource.Explicit);
        sequence.Type = type;

        return new SequenceBuilder(sequence);
    }

    /// <summary>
    ///     Configures a database sequence when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-sequences">Database sequences</see> for more information and examples.
    /// </remarks>
    /// <param name="type">The type of values the sequence will generate.</param>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="name">The name of the sequence.</param>
    /// <param name="builderAction">An action that performs configuration of the sequence.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ModelBuilder HasSequence(
        this ModelBuilder modelBuilder,
        Type type,
        string name,
        Action<SequenceBuilder> builderAction)
        => modelBuilder.HasSequence(type, name, null, builderAction);

    /// <summary>
    ///     Configures a database sequence when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-sequences">Database sequences</see> for more information and examples.
    /// </remarks>
    /// <param name="type">The type of values the sequence will generate.</param>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="name">The name of the sequence.</param>
    /// <param name="schema">The schema of the sequence.</param>
    /// <param name="builderAction">An action that performs configuration of the sequence.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ModelBuilder HasSequence(
        this ModelBuilder modelBuilder,
        Type type,
        string name,
        string? schema,
        Action<SequenceBuilder> builderAction)
    {
        Check.NotNull(builderAction, nameof(builderAction));

        builderAction(HasSequence(modelBuilder, type, name, schema));

        return modelBuilder;
    }

    /// <summary>
    ///     Configures a database sequence when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-sequences">Database sequences</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="T">The type of values the sequence will generate.</typeparam>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="name">The name of the sequence.</param>
    /// <param name="schema">The schema of the sequence.</param>
    /// <returns>A builder to further configure the sequence.</returns>
    public static SequenceBuilder HasSequence<T>(
        this ModelBuilder modelBuilder,
        string name,
        string? schema = null)
    {
        var sequence = HasSequence(modelBuilder.Model, name, schema, ConfigurationSource.Explicit);
        sequence.Type = typeof(T);

        return new SequenceBuilder(sequence);
    }

    /// <summary>
    ///     Configures a database sequence when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-sequences">Database sequences</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="T">The type of values the sequence will generate.</typeparam>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="name">The name of the sequence.</param>
    /// <param name="builderAction">An action that performs configuration of the sequence.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ModelBuilder HasSequence<T>(
        this ModelBuilder modelBuilder,
        string name,
        Action<SequenceBuilder> builderAction)
        => modelBuilder.HasSequence<T>(name, null, builderAction);

    /// <summary>
    ///     Configures a database sequence when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-sequences">Database sequences</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="T">The type of values the sequence will generate.</typeparam>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="name">The name of the sequence.</param>
    /// <param name="schema">The schema of the sequence.</param>
    /// <param name="builderAction">An action that performs configuration of the sequence.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ModelBuilder HasSequence<T>(
        this ModelBuilder modelBuilder,
        string name,
        string? schema,
        Action<SequenceBuilder> builderAction)
    {
        Check.NotNull(builderAction, nameof(builderAction));

        builderAction(HasSequence<T>(modelBuilder, name, schema));

        return modelBuilder;
    }

    /// <summary>
    ///     Configures a database sequence when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-sequences">Database sequences</see> for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="name">The name of the sequence.</param>
    /// <param name="schema">The schema of the sequence.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>A builder to further configure the sequence.</returns>
    public static IConventionSequenceBuilder HasSequence(
        this IConventionModelBuilder modelBuilder,
        string name,
        string? schema = null,
        bool fromDataAnnotation = false)
        => HasSequence(
            (IMutableModel)modelBuilder.Metadata,
            name,
            schema,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention).Builder;

    private static Sequence HasSequence(
        IMutableModel model,
        string name,
        string? schema,
        ConfigurationSource configurationSource)
    {
        Check.NotEmpty(name, nameof(name));
        Check.NullButNotEmpty(schema, nameof(schema));

        var sequence = (Sequence?)Sequence.FindSequence(model, name, schema);
        if (sequence != null)
        {
            sequence.UpdateConfigurationSource(configurationSource);
            return sequence;
        }

        return Sequence.AddSequence(model, name, schema, configurationSource);
    }

    /// <summary>
    ///     Configures a database function when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see> for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="methodInfo">The methodInfo this dbFunction uses.</param>
    /// <returns>A builder to further configure the function.</returns>
    public static DbFunctionBuilder HasDbFunction(
        this ModelBuilder modelBuilder,
        MethodInfo methodInfo)
    {
        Check.NotNull(methodInfo, nameof(methodInfo));

        var dbFunction = modelBuilder.Model.FindDbFunction(methodInfo);
        if (dbFunction == null)
        {
            dbFunction = modelBuilder.Model.AddDbFunction(methodInfo);
        }
        else
        {
            ((DbFunction)dbFunction).UpdateConfigurationSource(ConfigurationSource.Explicit);
        }

        return new DbFunctionBuilder(dbFunction);
    }

    /// <summary>
    ///     Configures a database function when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see> for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="expression">The method this dbFunction uses.</param>
    /// <returns>A builder to further configure the function.</returns>
    public static DbFunctionBuilder HasDbFunction<TResult>(
        this ModelBuilder modelBuilder,
        Expression<Func<TResult>> expression)
    {
        Check.NotNull(expression, nameof(expression));

        var methodInfo = (expression.Body as MethodCallExpression)?.Method;

        if (methodInfo == null)
        {
            throw new ArgumentException(RelationalStrings.DbFunctionExpressionIsNotMethodCall(expression));
        }

        return modelBuilder.HasDbFunction(methodInfo);
    }

    /// <summary>
    ///     Configures a database function when targeting a relational database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see> for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="methodInfo">The methodInfo this dbFunction uses.</param>
    /// <param name="builderAction">An action that performs configuration of the sequence.</param>
    /// <returns>A builder to further configure the function.</returns>
    public static ModelBuilder HasDbFunction(
        this ModelBuilder modelBuilder,
        MethodInfo methodInfo,
        Action<DbFunctionBuilder> builderAction)
    {
        Check.NotNull(builderAction, nameof(builderAction));

        builderAction(HasDbFunction(modelBuilder, methodInfo));

        return modelBuilder;
    }

    /// <summary>
    ///     Configures a relational database function.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see> for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="methodInfo">The method this function uses.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>A builder to further configure the function.</returns>
    public static IConventionDbFunctionBuilder HasDbFunction(
        this IConventionModelBuilder modelBuilder,
        MethodInfo methodInfo,
        bool fromDataAnnotation = false)
    {
        Check.NotNull(methodInfo, nameof(methodInfo));

        var dbFunction = modelBuilder.Metadata.FindDbFunction(methodInfo);
        if (dbFunction == null)
        {
            dbFunction = modelBuilder.Metadata.AddDbFunction(methodInfo, fromDataAnnotation);
        }
        else
        {
            ((DbFunction)dbFunction).UpdateConfigurationSource(
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
        }

        return dbFunction.Builder;
    }

    /// <summary>
    ///     Configures a relational database function.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see> for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="name">The name of the function.</param>
    /// <param name="returnType">The function's return type.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>A builder to further configure the function.</returns>
    public static IConventionDbFunctionBuilder HasDbFunction(
        this IConventionModelBuilder modelBuilder,
        string name,
        Type returnType,
        bool fromDataAnnotation = false)
    {
        Check.NotEmpty(name, nameof(name));

        var dbFunction = modelBuilder.Metadata.FindDbFunction(name);
        if (dbFunction == null)
        {
            dbFunction = modelBuilder.Metadata.AddDbFunction(name, returnType, fromDataAnnotation);
        }
        else
        {
            ((DbFunction)dbFunction).UpdateConfigurationSource(
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
        }

        return dbFunction.Builder;
    }

    /// <summary>
    ///     Configures the default schema that database objects should be created in, if no schema
    ///     is explicitly configured.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="schema">The default schema.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ModelBuilder HasDefaultSchema(
        this ModelBuilder modelBuilder,
        string? schema)
    {
        Check.NullButNotEmpty(schema, nameof(schema));

        modelBuilder.Model.SetDefaultSchema(schema);

        return modelBuilder;
    }

    /// <summary>
    ///     Configures the default schema that database objects should be created in, if no schema
    ///     is explicitly configured.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="schema">The default schema.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionModelBuilder? HasDefaultSchema(
        this IConventionModelBuilder modelBuilder,
        string? schema,
        bool fromDataAnnotation = false)
    {
        if (modelBuilder.CanSetDefaultSchema(schema, fromDataAnnotation))
        {
            modelBuilder.Metadata.SetDefaultSchema(schema, fromDataAnnotation);

            return modelBuilder;
        }

        return null;
    }

    /// <summary>
    ///     Returns a value indicating whether the given schema can be set as default.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="schema">The default schema.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given schema can be set as default.</returns>
    public static bool CanSetDefaultSchema(
        this IConventionModelBuilder modelBuilder,
        string? schema,
        bool fromDataAnnotation = false)
    {
        Check.NullButNotEmpty(schema, nameof(schema));

        return modelBuilder.CanSetAnnotation(RelationalAnnotationNames.DefaultSchema, schema, fromDataAnnotation);
    }

    /// <summary>
    ///     Configures the maximum length allowed for store identifiers.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="length">The value to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionModelBuilder? HasMaxIdentifierLength(
        this IConventionModelBuilder modelBuilder,
        int? length,
        bool fromDataAnnotation = false)
    {
        if (modelBuilder.CanSetMaxIdentifierLength(length, fromDataAnnotation))
        {
            modelBuilder.Metadata.SetMaxIdentifierLength(length, fromDataAnnotation);

            return modelBuilder;
        }

        return null;
    }

    /// <summary>
    ///     Returns a value indicating whether the maximum length allowed for store identifiers can be set.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="length">The value to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the maximum length allowed for store identifiers can be set.</returns>
    public static bool CanSetMaxIdentifierLength(
        this IConventionModelBuilder modelBuilder,
        int? length,
        bool fromDataAnnotation = false)
        => modelBuilder.CanSetAnnotation(RelationalAnnotationNames.MaxIdentifierLength, length, fromDataAnnotation);

    /// <summary>
    ///     Configures the database collation, which will be used by all columns without an explicit collation.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-collations">Database collations</see> for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="collation">The collation.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ModelBuilder UseCollation(
        this ModelBuilder modelBuilder,
        string? collation)
    {
        Check.NullButNotEmpty(collation, nameof(collation));

        modelBuilder.Model.SetCollation(collation);

        return modelBuilder;
    }

    /// <summary>
    ///     Configures the database collation, which will be used by all columns without an explicit collation.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-collations">Database collations</see> for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="collation">The collation.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionModelBuilder? UseCollation(
        this IConventionModelBuilder modelBuilder,
        string? collation,
        bool fromDataAnnotation = false)
    {
        if (modelBuilder.CanSetCollation(collation, fromDataAnnotation))
        {
            modelBuilder.Metadata.SetCollation(collation, fromDataAnnotation);

            return modelBuilder;
        }

        return null;
    }

    /// <summary>
    ///     Returns a value indicating whether the given collation can be set as default.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-collations">Database collations</see> for more information and examples.
    /// </remarks>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="collation">The collation.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the given collation can be set as default.</returns>
    public static bool CanSetCollation(
        this IConventionModelBuilder modelBuilder,
        string? collation,
        bool fromDataAnnotation = false)
    {
        Check.NullButNotEmpty(collation, nameof(collation));

        return modelBuilder.CanSetAnnotation(RelationalAnnotationNames.Collation, collation, fromDataAnnotation);
    }
}
