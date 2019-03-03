// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="IModel" /> for relational database metadata.
    /// </summary>
    public static class RelationalModelExtensions
    {
        /// <summary>
        ///     Returns the default schema to use for the model, or <c>null</c> if none has been set.
        /// </summary>
        /// <param name="model"> The model to get the default schema for. </param>
        /// <returns> The default schema. </returns>
        public static string GetDefaultSchema([NotNull] this IModel model)
            => (string)Check.NotNull(model, nameof(model))[RelationalAnnotationNames.DefaultSchema];

        /// <summary>
        ///     Sets the default schema.
        /// </summary>
        /// <param name="model"> The model to set the default schema for. </param>
        /// <param name="value"> The value to set. </param>
        public static void SetDefaultSchema([NotNull] this IMutableModel model, [CanBeNull] string value)
            => model.SetOrRemoveAnnotation(
                RelationalAnnotationNames.DefaultSchema,
                Check.NullButNotEmpty(value, nameof(value)));

        /// <summary>
        ///     Sets the default schema.
        /// </summary>
        /// <param name="model"> The model to set the default schema for. </param>
        /// <param name="value"> The value to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        public static void SetDefaultSchema(
            [NotNull] this IConventionModel model, [CanBeNull] string value, bool fromDataAnnotation = false)
            => model.SetOrRemoveAnnotation(
                RelationalAnnotationNames.DefaultSchema,
                Check.NullButNotEmpty(value, nameof(value)), fromDataAnnotation);

        /// <summary>
        ///     Returns the configuration source for the default schema.
        /// </summary>
        /// <param name="model"> The model to find configuration source for. </param>
        /// <returns> The configuration source for the default schema. </returns>
        public static ConfigurationSource? GetDefaultSchemaConfigurationSource([NotNull] this IConventionModel model)
            => model.FindAnnotation(RelationalAnnotationNames.DefaultSchema)?.GetConfigurationSource();

        /// <summary>
        ///     Returns the maximum length allowed for store identifiers.
        /// </summary>
        /// <param name="model"> The model to get the maximum identifier length for. </param>
        /// <returns> The maximum identifier length. </returns>
        public static int GetMaxIdentifierLength([NotNull] this IModel model)
            => (int?)Check.NotNull(model, nameof(model))[RelationalAnnotationNames.MaxIdentifierLength] ?? short.MaxValue;

        /// <summary>
        ///     Sets the maximum length allowed for store identifiers.
        /// </summary>
        /// <param name="model"> The model to set the default schema for. </param>
        /// <param name="length"> The value to set. </param>
        public static void SetMaxIdentifierLength([NotNull] this IMutableModel model, int? length)
            => model.SetOrRemoveAnnotation(RelationalAnnotationNames.MaxIdentifierLength, length);

        /// <summary>
        ///     Sets the maximum length allowed for store identifiers.
        /// </summary>
        /// <param name="model"> The model to set the default schema for. </param>
        /// <param name="length"> The value to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        public static void SetMaxIdentifierLength([NotNull] this IConventionModel model, int? length, bool fromDataAnnotation = false)
            => model.SetOrRemoveAnnotation(RelationalAnnotationNames.MaxIdentifierLength, length, fromDataAnnotation);

        /// <summary>
        ///     Returns the configuration source for <see cref="GetMaxIdentifierLength" />.
        /// </summary>
        /// <param name="model"> The model to find configuration source for. </param>
        /// <returns> The configuration source for <see cref="GetMaxIdentifierLength" />. </returns>
        public static ConfigurationSource? GetMaxIdentifierLengthConfigurationSource([NotNull] this IConventionModel model)
            => model.FindAnnotation(RelationalAnnotationNames.MaxIdentifierLength)?.GetConfigurationSource();

        /// <summary>
        ///     Finds an <see cref="ISequence" /> with the given name.
        /// </summary>
        /// <param name="model"> The model to find the sequence in. </param>
        /// <param name="name"> The sequence name. </param>
        /// <param name="schema"> The schema that contains the sequence. </param>
        /// <returns>
        ///     The <see cref="ISequence" /> or <c>null</c> if no sequence with the given name in
        ///     the given schema was found.
        /// </returns>
        public static ISequence FindSequence([NotNull] this IModel model, [NotNull] string name, [CanBeNull] string schema = null)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            return Sequence.FindSequence(model, name, schema);
        }

        /// <summary>
        ///     Finds an <see cref="IMutableSequence" /> with the given name.
        /// </summary>
        /// <param name="model"> The model to find the sequence in. </param>
        /// <param name="name"> The sequence name. </param>
        /// <param name="schema"> The schema that contains the sequence. </param>
        /// <returns>
        ///     The <see cref="IMutableSequence" /> or <c>null</c> if no sequence with the given name in
        ///     the given schema was found.
        /// </returns>
        public static IMutableSequence FindSequence(
            [NotNull] this IMutableModel model, [NotNull] string name, [CanBeNull] string schema = null)
            => (IMutableSequence)((IModel)model).FindSequence(name, schema);

        /// <summary>
        ///     Finds an <see cref="IConventionSequence" /> with the given name.
        /// </summary>
        /// <param name="model"> The model to find the sequence in. </param>
        /// <param name="name"> The sequence name. </param>
        /// <param name="schema"> The schema that contains the sequence. </param>
        /// <returns>
        ///     The <see cref="IConventionSequence" /> or <c>null</c> if no sequence with the given name in
        ///     the given schema was found.
        /// </returns>
        public static IConventionSequence FindSequence(
            [NotNull] this IConventionModel model, [NotNull] string name, [CanBeNull] string schema = null)
            => (IConventionSequence)((IModel)model).FindSequence(name, schema);

        /// <summary>
        ///     Either returns the existing <see cref="IMutableSequence" /> with the given name in the given schema
        ///     or creates a new sequence with the given name and schema.
        /// </summary>
        /// <param name="model"> The model to add the sequence to. </param>
        /// <param name="name"> The sequence name. </param>
        /// <param name="schema"> The schema name, or <c>null</c> to use the default schema. </param>
        /// <returns> The sequence. </returns>
        public static IMutableSequence AddSequence(
            [NotNull] this IMutableModel model, [NotNull] string name, [CanBeNull] string schema = null)
            => new Sequence(model, name, schema, ConfigurationSource.Explicit);

        /// <summary>
        ///     Either returns the existing <see cref="IMutableSequence" /> with the given name in the given schema
        ///     or creates a new sequence with the given name and schema.
        /// </summary>
        /// <param name="model"> The model to add the sequence to. </param>
        /// <param name="name"> The sequence name. </param>
        /// <param name="schema"> The schema name, or <c>null</c> to use the default schema. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The sequence. </returns>
        public static IConventionSequence AddSequence(
            [NotNull] this IConventionModel model, [NotNull] string name, [CanBeNull] string schema = null, bool fromDataAnnotation = false)
            => new Sequence(
                (IMutableModel)model, name, schema,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     Removes the <see cref="IMutableSequence" /> with the given name.
        /// </summary>
        /// <param name="model"> The model to find the sequence in. </param>
        /// <param name="name"> The sequence name. </param>
        /// <param name="schema"> The schema that contains the sequence. </param>
        /// <returns>
        ///     The removed <see cref="IMutableSequence" /> or <c>null</c> if no sequence with the given name in
        ///     the given schema was found.
        /// </returns>
        public static IMutableSequence RemoveSequence(
            [NotNull] this IMutableModel model, [NotNull] string name, [CanBeNull] string schema = null)
            => (IMutableSequence)Sequence.RemoveSequence(model, name, schema);

        /// <summary>
        ///     Removes the <see cref="IConventionSequence" /> with the given name.
        /// </summary>
        /// <param name="model"> The model to find the sequence in. </param>
        /// <param name="name"> The sequence name. </param>
        /// <param name="schema"> The schema that contains the sequence. </param>
        /// <returns>
        ///     The removed <see cref="IConventionSequence" /> or <c>null</c> if no sequence with the given name in
        ///     the given schema was found.
        /// </returns>
        public static IConventionSequence RemoveSequence(
            [NotNull] this IConventionModel model, [NotNull] string name, [CanBeNull] string schema = null)
            => (IConventionSequence)((IMutableModel)model).RemoveSequence(name, schema);

        /// <summary>
        ///     Returns all <see cref="ISequence" />s contained in the model.
        /// </summary>
        /// <param name="model"> The model to get the sequences in. </param>
        public static IReadOnlyList<ISequence> GetSequences([NotNull] this IModel model)
            => Sequence.GetSequences(model, RelationalAnnotationNames.SequencePrefix).ToList();

        /// <summary>
        ///     Returns all <see cref="IMutableSequence" />s contained in the model.
        /// </summary>
        /// <param name="model"> The model to get the sequences in. </param>
        public static IReadOnlyList<IMutableSequence> GetSequences([NotNull] this IMutableModel model)
            => (IReadOnlyList<IMutableSequence>)((IModel)model).GetSequences();

        /// <summary>
        ///     Returns all <see cref="IConventionSequence" />s contained in the model.
        /// </summary>
        /// <param name="model"> The model to get the sequences in. </param>
        public static IReadOnlyList<IConventionSequence> GetSequences([NotNull] this IConventionModel model)
            => (IReadOnlyList<IConventionSequence>)((IModel)model).GetSequences();

        /// <summary>
        ///     Finds a <see cref="IDbFunction" /> that is mapped to the method represented by the given <see cref="MethodInfo" />.
        /// </summary>
        /// <param name="model"> The model to find the function in. </param>
        /// <param name="method"> The <see cref="MethodInfo" /> for the method that is mapped to the function. </param>
        /// <returns> The <see cref="IDbFunction" /> or <c>null</c> if the method is not mapped. </returns>
        public static IDbFunction FindDbFunction([NotNull] this IModel model, [NotNull] MethodInfo method)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(method, nameof(method));

            return DbFunction.FindDbFunction(
                Check.NotNull(model, nameof(model)),
                Check.NotNull(method, nameof(method)));
        }

        /// <summary>
        ///     Finds a <see cref="IMutableDbFunction" /> that is mapped to the method represented by the given <see cref="MethodInfo" />.
        /// </summary>
        /// <param name="model"> The model to find the function in. </param>
        /// <param name="method"> The <see cref="MethodInfo" /> for the method that is mapped to the function. </param>
        /// <returns> The <see cref="IMutableDbFunction" /> or <c>null</c> if the method is not mapped. </returns>
        public static IMutableDbFunction FindDbFunction([NotNull] this IMutableModel model, [NotNull] MethodInfo method)
            => (IMutableDbFunction)((IModel)model).FindDbFunction(method);

        /// <summary>
        ///     Finds a <see cref="IConventionDbFunction" /> that is mapped to the method represented by the given <see cref="MethodInfo" />.
        /// </summary>
        /// <param name="model"> The model to find the function in. </param>
        /// <param name="method"> The <see cref="MethodInfo" /> for the method that is mapped to the function. </param>
        /// <returns> The <see cref="IConventionDbFunction" /> or <c>null</c> if the method is not mapped. </returns>
        public static IConventionDbFunction FindDbFunction([NotNull] this IConventionModel model, [NotNull] MethodInfo method)
            => (IConventionDbFunction)((IModel)model).FindDbFunction(method);

        /// <summary>
        ///     Either returns the existing <see cref="DbFunction" /> mapped to the given method
        ///     or creates a new function mapped to the method.
        /// </summary>
        /// <param name="model"> The model to add the function to. </param>
        /// <param name="methodInfo"> The <see cref="MethodInfo" /> for the method that is mapped to the function. </param>
        /// <returns> The <see cref="DbFunction" />. </returns>
        public static DbFunction AddDbFunction([NotNull] this IMutableModel model, [NotNull] MethodInfo methodInfo)
            => new DbFunction(
                Check.NotNull(methodInfo, nameof(methodInfo)), model, ConfigurationSource.Explicit);

        /// <summary>
        ///     Either returns the existing <see cref="DbFunction" /> mapped to the given method
        ///     or creates a new function mapped to the method.
        /// </summary>
        /// <param name="model"> The model to add the function to. </param>
        /// <param name="methodInfo"> The <see cref="MethodInfo" /> for the method that is mapped to the function. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The <see cref="DbFunction" />. </returns>
        public static IConventionDbFunction AddDbFunction(
            [NotNull] this IConventionModel model, [NotNull] MethodInfo methodInfo, bool fromDataAnnotation = false)
            => new DbFunction(
                Check.NotNull(methodInfo, nameof(methodInfo)), (IMutableModel)model,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     Removes the <see cref="IMutableDbFunction" /> that is mapped to the method represented by the given
        ///     <see cref="MethodInfo" />.
        /// </summary>
        /// <param name="model"> The model to find the function in. </param>
        /// <param name="method"> The <see cref="MethodInfo" /> for the method that is mapped to the function. </param>
        /// <returns> The removed <see cref="IMutableDbFunction" /> or <c>null</c> if the method is not mapped. </returns>
        public static IMutableDbFunction RemoveDbFunction([NotNull] this IMutableModel model, [NotNull] MethodInfo method)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(method, nameof(method));

            return DbFunction.RemoveDbFunction(
                Check.NotNull(model, nameof(model)),
                Check.NotNull(method, nameof(method)));
        }

        /// <summary>
        ///     Removes the <see cref="IConventionDbFunction" /> that is mapped to the method represented by the given
        ///     <see cref="MethodInfo" />.
        /// </summary>
        /// <param name="model"> The model to find the function in. </param>
        /// <param name="method"> The <see cref="MethodInfo" /> for the method that is mapped to the function. </param>
        /// <returns> The removed <see cref="IConventionDbFunction" /> or <c>null</c> if the method is not mapped. </returns>
        public static IConventionDbFunction RemoveDbFunction([NotNull] this IConventionModel model, [NotNull] MethodInfo method)
            => (IConventionDbFunction)((IMutableModel)model).RemoveDbFunction(method);

        /// <summary>
        ///     Returns all <see cref="IDbFunction" />s contained in the model.
        /// </summary>
        /// <param name="model"> The model to get the functions in. </param>
        public static IEnumerable<IDbFunction> GetDbFunctions([NotNull] this IModel model)
            => DbFunction.GetDbFunctions(model.AsModel());

        /// <summary>
        ///     Returns all <see cref="IMutableDbFunction" />s contained in the model.
        /// </summary>
        /// <param name="model"> The model to get the functions in. </param>
        public static IEnumerable<IMutableDbFunction> GetDbFunctions([NotNull] this IMutableModel model)
            => DbFunction.GetDbFunctions((Model)model);

        /// <summary>
        ///     Returns all <see cref="IConventionDbFunction" />s contained in the model.
        /// </summary>
        /// <param name="model"> The model to get the functions in. </param>
        public static IEnumerable<IConventionDbFunction> GetDbFunctions([NotNull] this IConventionModel model)
            => DbFunction.GetDbFunctions((Model)model);
    }
}
