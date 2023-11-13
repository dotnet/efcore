// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Relational-specific model extension methods.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public static class RelationalModelExtensions
{
    #region Default schema

    /// <summary>
    ///     Returns the default schema to use for the model, or <see langword="null" /> if none has been set.
    /// </summary>
    /// <param name="model">The model to get the default schema for.</param>
    /// <returns>The default schema.</returns>
    public static string? GetDefaultSchema(this IReadOnlyModel model)
        => (string?)model[RelationalAnnotationNames.DefaultSchema];

    /// <summary>
    ///     Sets the default schema.
    /// </summary>
    /// <param name="model">The model to set the default schema for.</param>
    /// <param name="value">The value to set.</param>
    public static void SetDefaultSchema(this IMutableModel model, string? value)
        => model.SetOrRemoveAnnotation(
            RelationalAnnotationNames.DefaultSchema,
            Check.NullButNotEmpty(value, nameof(value)));

    /// <summary>
    ///     Sets the default schema.
    /// </summary>
    /// <param name="model">The model to set the default schema for.</param>
    /// <param name="value">The value to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured schema.</returns>
    public static string? SetDefaultSchema(
        this IConventionModel model,
        string? value,
        bool fromDataAnnotation = false)
        => (string?)model.SetOrRemoveAnnotation(
            RelationalAnnotationNames.DefaultSchema,
            Check.NullButNotEmpty(value, nameof(value)), fromDataAnnotation)?.Value;

    /// <summary>
    ///     Returns the configuration source for the default schema.
    /// </summary>
    /// <param name="model">The model to find configuration source for.</param>
    /// <returns>The configuration source for the default schema.</returns>
    public static ConfigurationSource? GetDefaultSchemaConfigurationSource(this IConventionModel model)
        => model.FindAnnotation(RelationalAnnotationNames.DefaultSchema)?.GetConfigurationSource();

    #endregion Default schema

    /// <summary>
    ///     Returns the database model.
    /// </summary>
    /// <param name="model">The model to get the database model for.</param>
    /// <returns>The database model.</returns>
    public static IRelationalModel GetRelationalModel(this IModel model)
    {
        var databaseModel = (IRelationalModel?)model.FindRuntimeAnnotationValue(RelationalAnnotationNames.RelationalModel);
        if (databaseModel == null)
        {
            throw new InvalidOperationException(CoreStrings.ModelNotFinalized(nameof(GetRelationalModel)));
        }

        return databaseModel;
    }

    #region Max identifier length

    /// <summary>
    ///     Returns the maximum length allowed for store identifiers.
    /// </summary>
    /// <param name="model">The model to get the maximum identifier length for.</param>
    /// <returns>The maximum identifier length.</returns>
    public static int GetMaxIdentifierLength(this IReadOnlyModel model)
        => (int?)model[RelationalAnnotationNames.MaxIdentifierLength] ?? short.MaxValue;

    /// <summary>
    ///     Sets the maximum length allowed for store identifiers.
    /// </summary>
    /// <param name="model">The model to set the default schema for.</param>
    /// <param name="length">The value to set.</param>
    public static void SetMaxIdentifierLength(this IMutableModel model, int? length)
        => model.SetOrRemoveAnnotation(RelationalAnnotationNames.MaxIdentifierLength, length);

    /// <summary>
    ///     Sets the maximum length allowed for store identifiers.
    /// </summary>
    /// <param name="model">The model to set the default schema for.</param>
    /// <param name="length">The value to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static int? SetMaxIdentifierLength(this IConventionModel model, int? length, bool fromDataAnnotation = false)
        => (int?)model.SetOrRemoveAnnotation(
            RelationalAnnotationNames.MaxIdentifierLength,
            length,
            fromDataAnnotation)?.Value;

    /// <summary>
    ///     Returns the configuration source for <see cref="GetMaxIdentifierLength" />.
    /// </summary>
    /// <param name="model">The model to find configuration source for.</param>
    /// <returns>The configuration source for <see cref="GetMaxIdentifierLength" />.</returns>
    public static ConfigurationSource? GetMaxIdentifierLengthConfigurationSource(this IConventionModel model)
        => model.FindAnnotation(RelationalAnnotationNames.MaxIdentifierLength)?.GetConfigurationSource();

    #endregion Max identifier length

    #region Sequence

    /// <summary>
    ///     Finds a sequence with the given name.
    /// </summary>
    /// <param name="model">The model to find the sequence in.</param>
    /// <param name="name">The sequence name.</param>
    /// <param name="schema">The schema that contains the sequence.</param>
    /// <returns>
    ///     The sequence or <see langword="null" /> if no sequence with the given name in
    ///     the given schema was found.
    /// </returns>
    public static IReadOnlySequence? FindSequence(
        this IReadOnlyModel model,
        string name,
        string? schema = null)
        => Sequence.FindSequence(
            model, Check.NotEmpty(name, nameof(name)), Check.NullButNotEmpty(schema, nameof(schema)));

    /// <summary>
    ///     Finds a sequence with the given name.
    /// </summary>
    /// <param name="model">The model to find the sequence in.</param>
    /// <param name="name">The sequence name.</param>
    /// <param name="schema">The schema that contains the sequence.</param>
    /// <returns>
    ///     The sequence or <see langword="null" /> if no sequence with the given name in
    ///     the given schema was found.
    /// </returns>
    public static IMutableSequence? FindSequence(
        this IMutableModel model,
        string name,
        string? schema = null)
        => (IMutableSequence?)((IReadOnlyModel)model).FindSequence(name, schema);

    /// <summary>
    ///     Finds a sequence with the given name.
    /// </summary>
    /// <param name="model">The model to find the sequence in.</param>
    /// <param name="name">The sequence name.</param>
    /// <param name="schema">The schema that contains the sequence.</param>
    /// <returns>
    ///     The sequence or <see langword="null" /> if no sequence with the given name in
    ///     the given schema was found.
    /// </returns>
    public static IConventionSequence? FindSequence(
        this IConventionModel model,
        string name,
        string? schema = null)
        => (IConventionSequence?)((IReadOnlyModel)model).FindSequence(name, schema);

    /// <summary>
    ///     Finds a sequence with the given name.
    /// </summary>
    /// <param name="model">The model to find the sequence in.</param>
    /// <param name="name">The sequence name.</param>
    /// <param name="schema">The schema that contains the sequence.</param>
    /// <returns>
    ///     The sequence or <see langword="null" /> if no sequence with the given name in
    ///     the given schema was found.
    /// </returns>
    public static ISequence? FindSequence(
        this IModel model,
        string name,
        string? schema = null)
        => (ISequence?)((IReadOnlyModel)model).FindSequence(name, schema);

    /// <summary>
    ///     Either returns the existing <see cref="IMutableSequence" /> with the given name in the given schema
    ///     or creates a new sequence with the given name and schema.
    /// </summary>
    /// <param name="model">The model to add the sequence to.</param>
    /// <param name="name">The sequence name.</param>
    /// <param name="schema">The schema name, or <see langword="null" /> to use the default schema.</param>
    /// <returns>The sequence.</returns>
    public static IMutableSequence AddSequence(
        this IMutableModel model,
        string name,
        string? schema = null)
        => Sequence.AddSequence(model, name, schema, ConfigurationSource.Explicit);

    /// <summary>
    ///     Either returns the existing <see cref="IMutableSequence" /> with the given name in the given schema
    ///     or creates a new sequence with the given name and schema.
    /// </summary>
    /// <param name="model">The model to add the sequence to.</param>
    /// <param name="name">The sequence name.</param>
    /// <param name="schema">The schema name, or <see langword="null" /> to use the default schema.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The sequence.</returns>
    public static IConventionSequence? AddSequence(
        this IConventionModel model,
        string name,
        string? schema = null,
        bool fromDataAnnotation = false)
        => Sequence.AddSequence(
            (IMutableModel)model, name, schema,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     Removes the <see cref="IMutableSequence" /> with the given name.
    /// </summary>
    /// <param name="model">The model to find the sequence in.</param>
    /// <param name="name">The sequence name.</param>
    /// <param name="schema">The schema that contains the sequence.</param>
    /// <returns>
    ///     The removed <see cref="IMutableSequence" /> or <see langword="null" /> if no sequence with the given name in
    ///     the given schema was found.
    /// </returns>
    public static IMutableSequence? RemoveSequence(
        this IMutableModel model,
        string name,
        string? schema = null)
        => Sequence.RemoveSequence(model, name, schema);

    /// <summary>
    ///     Removes the <see cref="IConventionSequence" /> with the given name.
    /// </summary>
    /// <param name="model">The model to find the sequence in.</param>
    /// <param name="name">The sequence name.</param>
    /// <param name="schema">The schema that contains the sequence.</param>
    /// <returns>
    ///     The removed <see cref="IConventionSequence" /> or <see langword="null" /> if no sequence with the given name in
    ///     the given schema was found.
    /// </returns>
    public static IConventionSequence? RemoveSequence(
        this IConventionModel model,
        string name,
        string? schema = null)
        => Sequence.RemoveSequence((IMutableModel)model, name, schema);

    /// <summary>
    ///     Returns all sequences contained in the model.
    /// </summary>
    /// <param name="model">The model to get the sequences in.</param>
    public static IEnumerable<ISequence> GetSequences(this IModel model)
        => Sequence.GetSequences(model);

    /// <summary>
    ///     Returns all sequences contained in the model.
    /// </summary>
    /// <param name="model">The model to get the sequences in.</param>
    public static IEnumerable<IMutableSequence> GetSequences(this IMutableModel model)
        => Sequence.GetSequences(model).Cast<IMutableSequence>();

    /// <summary>
    ///     Returns all sequences contained in the model.
    /// </summary>
    /// <param name="model">The model to get the sequences in.</param>
    public static IEnumerable<IConventionSequence> GetSequences(this IConventionModel model)
        => Sequence.GetSequences(model).Cast<IConventionSequence>();

    /// <summary>
    ///     Returns all sequences contained in the model.
    /// </summary>
    /// <param name="model">The model to get the sequences in.</param>
    public static IEnumerable<IReadOnlySequence> GetSequences(this IReadOnlyModel model)
        => Sequence.GetSequences(model);

    #endregion Sequence

    #region DbFunction

    /// <summary>
    ///     Finds a function that is mapped to the method represented by the given <see cref="MethodInfo" />.
    /// </summary>
    /// <param name="model">The model to find the function in.</param>
    /// <param name="method">The <see cref="MethodInfo" /> for the method that is mapped to the function.</param>
    /// <returns>The function or <see langword="null" /> if the method is not mapped.</returns>
    public static IReadOnlyDbFunction? FindDbFunction(this IReadOnlyModel model, MethodInfo method)
        => DbFunction.FindDbFunction(model, Check.NotNull(method, nameof(method)));

    /// <summary>
    ///     Finds a function that is mapped to the method represented by the given <see cref="MethodInfo" />.
    /// </summary>
    /// <param name="model">The model to find the function in.</param>
    /// <param name="method">The <see cref="MethodInfo" /> for the method that is mapped to the function.</param>
    /// <returns>The function or <see langword="null" /> if the method is not mapped.</returns>
    public static IMutableDbFunction? FindDbFunction(this IMutableModel model, MethodInfo method)
        => (IMutableDbFunction?)((IReadOnlyModel)model).FindDbFunction(method);

    /// <summary>
    ///     Finds a function that is mapped to the method represented by the given <see cref="MethodInfo" />.
    /// </summary>
    /// <param name="model">The model to find the function in.</param>
    /// <param name="method">The <see cref="MethodInfo" /> for the method that is mapped to the function.</param>
    /// <returns>The function or <see langword="null" /> if the method is not mapped.</returns>
    public static IConventionDbFunction? FindDbFunction(this IConventionModel model, MethodInfo method)
        => (IConventionDbFunction?)((IReadOnlyModel)model).FindDbFunction(method);

    /// <summary>
    ///     Finds a function that is mapped to the method represented by the given <see cref="MethodInfo" />.
    /// </summary>
    /// <param name="model">The model to find the function in.</param>
    /// <param name="method">The <see cref="MethodInfo" /> for the method that is mapped to the function.</param>
    /// <returns>The function or <see langword="null" /> if the method is not mapped.</returns>
    public static IDbFunction? FindDbFunction(this IModel model, MethodInfo method)
        => DbFunction.FindDbFunction(model, method);

    /// <summary>
    ///     Finds a function that is mapped to the method represented by the given name.
    /// </summary>
    /// <param name="model">The model to find the function in.</param>
    /// <param name="name">The model name of the function.</param>
    /// <returns>The function or <see langword="null" /> if the method is not mapped.</returns>
    public static IReadOnlyDbFunction? FindDbFunction(this IReadOnlyModel model, string name)
        => DbFunction.FindDbFunction(model, Check.NotNull(name, nameof(name)));

    /// <summary>
    ///     Finds a function that is mapped to the method represented by the given name.
    /// </summary>
    /// <param name="model">The model to find the function in.</param>
    /// <param name="name">The model name of the function.</param>
    /// <returns>The function or <see langword="null" /> if the method is not mapped.</returns>
    public static IMutableDbFunction? FindDbFunction(this IMutableModel model, string name)
        => (IMutableDbFunction?)((IReadOnlyModel)model).FindDbFunction(name);

    /// <summary>
    ///     Finds a function that is mapped to the method represented by the given name.
    /// </summary>
    /// <param name="model">The model to find the function in.</param>
    /// <param name="name">The model name of the function.</param>
    /// <returns>The function or <see langword="null" /> if the method is not mapped.</returns>
    public static IConventionDbFunction? FindDbFunction(this IConventionModel model, string name)
        => (IConventionDbFunction?)((IReadOnlyModel)model).FindDbFunction(name);

    /// <summary>
    ///     Finds a function that is mapped to the method represented by the given name.
    /// </summary>
    /// <param name="model">The model to find the function in.</param>
    /// <param name="name">The model name of the function.</param>
    /// <returns>The function or <see langword="null" /> if the method is not mapped.</returns>
    public static IDbFunction? FindDbFunction(this IModel model, string name)
        => DbFunction.FindDbFunction(model, name);

    /// <summary>
    ///     Creates an <see cref="IMutableDbFunction" /> mapped to the given method.
    /// </summary>
    /// <param name="model">The model to add the function to.</param>
    /// <param name="methodInfo">The <see cref="MethodInfo" /> for the method that is mapped to the function.</param>
    /// <returns>The new <see cref="IMutableDbFunction" />.</returns>
    public static IMutableDbFunction AddDbFunction(this IMutableModel model, MethodInfo methodInfo)
        => DbFunction.AddDbFunction(
            model, Check.NotNull(methodInfo, nameof(methodInfo)), ConfigurationSource.Explicit);

    /// <summary>
    ///     Creates a function mapped to the given method.
    /// </summary>
    /// <param name="model">The model to add the function to.</param>
    /// <param name="methodInfo">The <see cref="MethodInfo" /> for the method that is mapped to the function.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The new function.</returns>
    public static IConventionDbFunction AddDbFunction(
        this IConventionModel model,
        MethodInfo methodInfo,
        bool fromDataAnnotation = false)
        => DbFunction.AddDbFunction(
            (IMutableModel)model, Check.NotNull(methodInfo, nameof(methodInfo)),
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     Creates a function.
    /// </summary>
    /// <param name="model">The model to add the function to.</param>
    /// <param name="name">The model name of the function.</param>
    /// <param name="returnType">The function return type.</param>
    /// <returns>The new function.</returns>
    public static IMutableDbFunction AddDbFunction(
        this IMutableModel model,
        string name,
        Type returnType)
        => DbFunction.AddDbFunction(
            model, Check.NotNull(name, nameof(name)), returnType, ConfigurationSource.Explicit);

    /// <summary>
    ///     Creates a function.
    /// </summary>
    /// <param name="model">The model to add the function to.</param>
    /// <param name="name">The model name of the function.</param>
    /// <param name="returnType">The function return type.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The new function.</returns>
    public static IConventionDbFunction AddDbFunction(
        this IConventionModel model,
        string name,
        Type returnType,
        bool fromDataAnnotation = false)
        => DbFunction.AddDbFunction(
            (IMutableModel)model,
            Check.NotNull(name, nameof(name)),
            returnType,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <summary>
    ///     Removes the function that is mapped to the method represented by the given
    ///     <see cref="MethodInfo" />.
    /// </summary>
    /// <param name="model">The model to find the function in.</param>
    /// <param name="method">The <see cref="MethodInfo" /> for the method that is mapped to the function.</param>
    /// <returns>The removed function or <see langword="null" /> if the method is not mapped.</returns>
    public static IMutableDbFunction? RemoveDbFunction(this IMutableModel model, MethodInfo method)
        => DbFunction.RemoveDbFunction(model, Check.NotNull(method, nameof(method)));

    /// <summary>
    ///     Removes the function that is mapped to the method represented by the given
    ///     <see cref="MethodInfo" />.
    /// </summary>
    /// <param name="model">The model to find the function in.</param>
    /// <param name="method">The <see cref="MethodInfo" /> for the method that is mapped to the function.</param>
    /// <returns>The removed function or <see langword="null" /> if the method is not mapped.</returns>
    public static IConventionDbFunction? RemoveDbFunction(this IConventionModel model, MethodInfo method)
        => (IConventionDbFunction?)((IMutableModel)model).RemoveDbFunction(method);

    /// <summary>
    ///     Removes the function that is mapped to the method represented by the given
    ///     <see cref="MethodInfo" />.
    /// </summary>
    /// <param name="model">The model to find the function in.</param>
    /// <param name="name">The model name of the function.</param>
    /// <returns>The removed function or <see langword="null" /> if the method is not mapped.</returns>
    public static IMutableDbFunction? RemoveDbFunction(this IMutableModel model, string name)
        => DbFunction.RemoveDbFunction(model, Check.NotNull(name, nameof(name)));

    /// <summary>
    ///     Removes the function that is mapped to the method represented by the given
    ///     <see cref="MethodInfo" />.
    /// </summary>
    /// <param name="model">The model to find the function in.</param>
    /// <param name="name">The model name of the function.</param>
    /// <returns>The removed function or <see langword="null" /> if the method is not mapped.</returns>
    public static IConventionDbFunction? RemoveDbFunction(this IConventionModel model, string name)
        => (IConventionDbFunction?)((IMutableModel)model).RemoveDbFunction(name);

    /// <summary>
    ///     Returns all functions contained in the model.
    /// </summary>
    /// <param name="model">The model to get the functions in.</param>
    public static IEnumerable<IReadOnlyDbFunction> GetDbFunctions(this IReadOnlyModel model)
        => DbFunction.GetDbFunctions(model);

    /// <summary>
    ///     Returns all functions contained in the model.
    /// </summary>
    /// <param name="model">The model to get the functions in.</param>
    public static IEnumerable<IMutableDbFunction> GetDbFunctions(this IMutableModel model)
        => DbFunction.GetDbFunctions(model).Cast<IMutableDbFunction>();

    /// <summary>
    ///     Returns all functions contained in the model.
    /// </summary>
    /// <param name="model">The model to get the functions in.</param>
    public static IEnumerable<IConventionDbFunction> GetDbFunctions(this IConventionModel model)
        => DbFunction.GetDbFunctions(model).Cast<IConventionDbFunction>();

    /// <summary>
    ///     Returns all functions contained in the model.
    /// </summary>
    /// <param name="model">The model to get the functions in.</param>
    public static IEnumerable<IDbFunction> GetDbFunctions(this IModel model)
        => DbFunction.GetDbFunctions(model);

    #endregion DbFunction

    #region Collation

    /// <summary>
    ///     Returns the database collation.
    /// </summary>
    /// <param name="model">The model to get the collation for.</param>
    /// <returns>The collation.</returns>
    public static string? GetCollation(this IReadOnlyModel model)
        => (model is RuntimeModel)
            ? throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData)
            : (string?)model[RelationalAnnotationNames.Collation];

    /// <summary>
    ///     Sets the database collation.
    /// </summary>
    /// <param name="model">The model to set the collation for.</param>
    /// <param name="value">The value to set.</param>
    public static void SetCollation(this IMutableModel model, string? value)
        => model.SetOrRemoveAnnotation(
            RelationalAnnotationNames.Collation,
            Check.NullButNotEmpty(value, nameof(value)));

    /// <summary>
    ///     Sets the database collation.
    /// </summary>
    /// <param name="model">The model to set the collation for.</param>
    /// <param name="value">The value to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured collation.</returns>
    public static string? SetCollation(this IConventionModel model, string? value, bool fromDataAnnotation = false)
        => (string?)model.SetOrRemoveAnnotation(
            RelationalAnnotationNames.Collation,
            Check.NullButNotEmpty(value, nameof(value)), fromDataAnnotation)?.Value;

    /// <summary>
    ///     Returns the configuration source for the collation.
    /// </summary>
    /// <param name="model">The model to find configuration source for.</param>
    /// <returns>The configuration source for the collation.</returns>
    public static ConfigurationSource? GetCollationConfigurationSource(this IConventionModel model)
        => model.FindAnnotation(RelationalAnnotationNames.Collation)?.GetConfigurationSource();

    #endregion Collation
}
