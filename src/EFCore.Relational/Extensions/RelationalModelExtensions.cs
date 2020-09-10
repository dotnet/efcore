// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Relational-specific extension methods for <see cref="IModel" /> and extension methods for <see cref="IRelationalModel" />.
    /// </summary>
    public static class RelationalModelExtensions
    {
        /// <summary>
        ///     <para>
        ///         Creates a human-readable representation of the given metadata.
        ///     </para>
        ///     <para>
        ///         Warning: Do not rely on the format of the returned string.
        ///         It is designed for debugging only and may change arbitrarily between releases.
        ///     </para>
        /// </summary>
        /// <param name="model"> The metadata item. </param>
        /// <param name="options"> Options for generating the string. </param>
        /// <param name="indent"> The number of indent spaces to use before each new line. </param>
        /// <returns> A human-readable representation. </returns>
        public static string ToDebugString(
            [NotNull] this IRelationalModel model,
            MetadataDebugStringOptions options,
            int indent = 0)
        {
            var builder = new StringBuilder();
            var indentString = new string(' ', indent);

            builder.Append(indentString).Append("RelationalModel: ");

            if (model.Collation != null)
            {
                builder.AppendLine().Append(indentString).Append("Collation: " + model.Collation);
            }

            foreach (var table in model.Tables)
            {
                builder.AppendLine().Append(table.ToDebugString(options, indent + 2));
            }

            foreach (var view in model.Views)
            {
                builder.AppendLine().Append(view.ToDebugString(options, indent + 2));
            }

            foreach (var function in model.Functions)
            {
                builder.AppendLine().Append(function.ToDebugString(options, indent + 2));
            }

            foreach (var query in model.Queries)
            {
                builder.AppendLine().Append(query.ToDebugString(options, indent + 2));
            }

            foreach (var sequence in model.Sequences)
            {
                builder.AppendLine().Append(sequence.ToDebugString(options, indent + 2));
            }

            if ((options & MetadataDebugStringOptions.IncludeAnnotations) != 0)
            {
                builder.Append(model.AnnotationsToDebugString(indent));
            }

            return builder.ToString();
        }

        /// <summary>
        ///     Returns the default schema to use for the model, or <see langword="null" /> if none has been set.
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
        /// <returns> The configured schema. </returns>
        public static string SetDefaultSchema(
            [NotNull] this IConventionModel model,
            [CanBeNull] string value,
            bool fromDataAnnotation = false)
        {
            model.SetOrRemoveAnnotation(
                RelationalAnnotationNames.DefaultSchema,
                Check.NullButNotEmpty(value, nameof(value)), fromDataAnnotation);
            return value;
        }

        /// <summary>
        ///     Returns the configuration source for the default schema.
        /// </summary>
        /// <param name="model"> The model to find configuration source for. </param>
        /// <returns> The configuration source for the default schema. </returns>
        public static ConfigurationSource? GetDefaultSchemaConfigurationSource([NotNull] this IConventionModel model)
            => model.FindAnnotation(RelationalAnnotationNames.DefaultSchema)?.GetConfigurationSource();

        /// <summary>
        ///     Returns the database model.
        /// </summary>
        /// <param name="model"> The model to get the database model for. </param>
        /// <returns> The database model. </returns>
        public static IRelationalModel GetRelationalModel([NotNull] this IModel model)
        {
            var databaseModel = (IRelationalModel)model[RelationalAnnotationNames.RelationalModel];
            if (databaseModel == null)
            {
                throw new InvalidOperationException(RelationalStrings.DatabaseModelMissing);
            }

            return databaseModel;
        }

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
        /// <returns> The configured value. </returns>
        public static int? SetMaxIdentifierLength([NotNull] this IConventionModel model, int? length, bool fromDataAnnotation = false)
        {
            model.SetOrRemoveAnnotation(RelationalAnnotationNames.MaxIdentifierLength, length, fromDataAnnotation);

            return length;
        }

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
        ///     The <see cref="ISequence" /> or <see langword="null" /> if no sequence with the given name in
        ///     the given schema was found.
        /// </returns>
        public static ISequence FindSequence([NotNull] this IModel model, [NotNull] string name, [CanBeNull] string schema = null)
            => Sequence.FindSequence(
                Check.NotNull(model, nameof(model)), Check.NotEmpty(name, nameof(name)), Check.NullButNotEmpty(schema, nameof(schema)));

        /// <summary>
        ///     Finds an <see cref="IMutableSequence" /> with the given name.
        /// </summary>
        /// <param name="model"> The model to find the sequence in. </param>
        /// <param name="name"> The sequence name. </param>
        /// <param name="schema"> The schema that contains the sequence. </param>
        /// <returns>
        ///     The <see cref="IMutableSequence" /> or <see langword="null" /> if no sequence with the given name in
        ///     the given schema was found.
        /// </returns>
        public static IMutableSequence FindSequence(
            [NotNull] this IMutableModel model,
            [NotNull] string name,
            [CanBeNull] string schema = null)
            => (IMutableSequence)((IModel)model).FindSequence(name, schema);

        /// <summary>
        ///     Finds an <see cref="IConventionSequence" /> with the given name.
        /// </summary>
        /// <param name="model"> The model to find the sequence in. </param>
        /// <param name="name"> The sequence name. </param>
        /// <param name="schema"> The schema that contains the sequence. </param>
        /// <returns>
        ///     The <see cref="IConventionSequence" /> or <see langword="null" /> if no sequence with the given name in
        ///     the given schema was found.
        /// </returns>
        public static IConventionSequence FindSequence(
            [NotNull] this IConventionModel model,
            [NotNull] string name,
            [CanBeNull] string schema = null)
            => (IConventionSequence)((IModel)model).FindSequence(name, schema);

        /// <summary>
        ///     Either returns the existing <see cref="IMutableSequence" /> with the given name in the given schema
        ///     or creates a new sequence with the given name and schema.
        /// </summary>
        /// <param name="model"> The model to add the sequence to. </param>
        /// <param name="name"> The sequence name. </param>
        /// <param name="schema"> The schema name, or <see langword="null" /> to use the default schema. </param>
        /// <returns> The sequence. </returns>
        public static IMutableSequence AddSequence(
            [NotNull] this IMutableModel model,
            [NotNull] string name,
            [CanBeNull] string schema = null)
            => Sequence.AddSequence(model, name, schema, ConfigurationSource.Explicit);

        /// <summary>
        ///     Either returns the existing <see cref="IMutableSequence" /> with the given name in the given schema
        ///     or creates a new sequence with the given name and schema.
        /// </summary>
        /// <param name="model"> The model to add the sequence to. </param>
        /// <param name="name"> The sequence name. </param>
        /// <param name="schema"> The schema name, or <see langword="null" /> to use the default schema. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The sequence. </returns>
        public static IConventionSequence AddSequence(
            [NotNull] this IConventionModel model,
            [NotNull] string name,
            [CanBeNull] string schema = null,
            bool fromDataAnnotation = false)
            => Sequence.AddSequence(
                (IMutableModel)model, name, schema,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     Removes the <see cref="IMutableSequence" /> with the given name.
        /// </summary>
        /// <param name="model"> The model to find the sequence in. </param>
        /// <param name="name"> The sequence name. </param>
        /// <param name="schema"> The schema that contains the sequence. </param>
        /// <returns>
        ///     The removed <see cref="IMutableSequence" /> or <see langword="null" /> if no sequence with the given name in
        ///     the given schema was found.
        /// </returns>
        public static IMutableSequence RemoveSequence(
            [NotNull] this IMutableModel model,
            [NotNull] string name,
            [CanBeNull] string schema = null)
            => Sequence.RemoveSequence(Check.NotNull(model, nameof(model)), name, schema);

        /// <summary>
        ///     Removes the <see cref="IConventionSequence" /> with the given name.
        /// </summary>
        /// <param name="model"> The model to find the sequence in. </param>
        /// <param name="name"> The sequence name. </param>
        /// <param name="schema"> The schema that contains the sequence. </param>
        /// <returns>
        ///     The removed <see cref="IConventionSequence" /> or <see langword="null" /> if no sequence with the given name in
        ///     the given schema was found.
        /// </returns>
        public static IConventionSequence RemoveSequence(
            [NotNull] this IConventionModel model,
            [NotNull] string name,
            [CanBeNull] string schema = null)
            => Sequence.RemoveSequence((IMutableModel)Check.NotNull(model, nameof(model)), name, schema);

        /// <summary>
        ///     Returns all <see cref="ISequence" />s contained in the model.
        /// </summary>
        /// <param name="model"> The model to get the sequences in. </param>
        public static IEnumerable<ISequence> GetSequences([NotNull] this IModel model)
            => Sequence.GetSequences(Check.NotNull(model, nameof(model)));

        /// <summary>
        ///     Returns all <see cref="IMutableSequence" />s contained in the model.
        /// </summary>
        /// <param name="model"> The model to get the sequences in. </param>
        public static IEnumerable<IMutableSequence> GetSequences([NotNull] this IMutableModel model)
            => Sequence.GetSequences(Check.NotNull(model, nameof(model)));

        /// <summary>
        ///     Returns all <see cref="IConventionSequence" />s contained in the model.
        /// </summary>
        /// <param name="model"> The model to get the sequences in. </param>
        public static IEnumerable<IConventionSequence> GetSequences([NotNull] this IConventionModel model)
            => Sequence.GetSequences(Check.NotNull(model, nameof(model)));

        /// <summary>
        ///     Finds a <see cref="IDbFunction" /> that is mapped to the method represented by the given <see cref="MethodInfo" />.
        /// </summary>
        /// <param name="model"> The model to find the function in. </param>
        /// <param name="method"> The <see cref="MethodInfo" /> for the method that is mapped to the function. </param>
        /// <returns> The <see cref="IDbFunction" /> or <see langword="null" /> if the method is not mapped. </returns>
        public static IDbFunction FindDbFunction([NotNull] this IModel model, [NotNull] MethodInfo method)
            => DbFunction.FindDbFunction(
                Check.NotNull(model, nameof(model)),
                Check.NotNull(method, nameof(method)));

        /// <summary>
        ///     Finds a <see cref="IMutableDbFunction" /> that is mapped to the method represented by the given <see cref="MethodInfo" />.
        /// </summary>
        /// <param name="model"> The model to find the function in. </param>
        /// <param name="method"> The <see cref="MethodInfo" /> for the method that is mapped to the function. </param>
        /// <returns> The <see cref="IMutableDbFunction" /> or <see langword="null" /> if the method is not mapped. </returns>
        public static IMutableDbFunction FindDbFunction([NotNull] this IMutableModel model, [NotNull] MethodInfo method)
            => (IMutableDbFunction)((IModel)model).FindDbFunction(method);

        /// <summary>
        ///     Finds a <see cref="IConventionDbFunction" /> that is mapped to the method represented by the given <see cref="MethodInfo" />.
        /// </summary>
        /// <param name="model"> The model to find the function in. </param>
        /// <param name="method"> The <see cref="MethodInfo" /> for the method that is mapped to the function. </param>
        /// <returns> The <see cref="IConventionDbFunction" /> or <see langword="null" /> if the method is not mapped. </returns>
        public static IConventionDbFunction FindDbFunction([NotNull] this IConventionModel model, [NotNull] MethodInfo method)
            => (IConventionDbFunction)((IModel)model).FindDbFunction(method);

        /// <summary>
        ///     Finds an <see cref="IDbFunction" /> that is mapped to the method represented by the given name.
        /// </summary>
        /// <param name="model"> The model to find the function in. </param>
        /// <param name="name"> The model name of the function. </param>
        /// <returns> The <see cref="IDbFunction" /> or <see langword="null" /> if the method is not mapped. </returns>
        public static IDbFunction FindDbFunction([NotNull] this IModel model, [NotNull] string name)
            => DbFunction.FindDbFunction(
                Check.NotNull(model, nameof(model)),
                Check.NotNull(name, nameof(name)));

        /// <summary>
        ///     Finds an <see cref="IMutableDbFunction" /> that is mapped to the method represented by the given name.
        /// </summary>
        /// <param name="model"> The model to find the function in. </param>
        /// <param name="name"> The model name of the function. </param>
        /// <returns> The <see cref="IMutableDbFunction" /> or <see langword="null" /> if the method is not mapped. </returns>
        public static IMutableDbFunction FindDbFunction([NotNull] this IMutableModel model, [NotNull] string name)
            => (IMutableDbFunction)((IModel)model).FindDbFunction(name);

        /// <summary>
        ///     Finds an <see cref="IConventionDbFunction" /> that is mapped to the method represented by the given name.
        /// </summary>
        /// <param name="model"> The model to find the function in. </param>
        /// <param name="name"> The model name of the function. </param>
        /// <returns> The <see cref="IConventionDbFunction" /> or <see langword="null" /> if the method is not mapped. </returns>
        public static IConventionDbFunction FindDbFunction([NotNull] this IConventionModel model, [NotNull] string name)
            => (IConventionDbFunction)((IModel)model).FindDbFunction(name);

        /// <summary>
        ///     Creates an <see cref="IMutableDbFunction" /> mapped to the given method.
        /// </summary>
        /// <param name="model"> The model to add the function to. </param>
        /// <param name="methodInfo"> The <see cref="MethodInfo" /> for the method that is mapped to the function. </param>
        /// <returns> The new <see cref="IMutableDbFunction" />. </returns>
        public static IMutableDbFunction AddDbFunction([NotNull] this IMutableModel model, [NotNull] MethodInfo methodInfo)
            => DbFunction.AddDbFunction(
                model, Check.NotNull(methodInfo, nameof(methodInfo)), ConfigurationSource.Explicit);

        /// <summary>
        ///     Creates an <see cref="IConventionDbFunction" /> mapped to the given method.
        /// </summary>
        /// <param name="model"> The model to add the function to. </param>
        /// <param name="methodInfo"> The <see cref="MethodInfo" /> for the method that is mapped to the function. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The new <see cref="IConventionDbFunction" />. </returns>
        public static IConventionDbFunction AddDbFunction(
            [NotNull] this IConventionModel model,
            [NotNull] MethodInfo methodInfo,
            bool fromDataAnnotation = false)
            => DbFunction.AddDbFunction(
                (IMutableModel)model, Check.NotNull(methodInfo, nameof(methodInfo)),
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     Creates an <see cref="IMutableDbFunction" />.
        /// </summary>
        /// <param name="model"> The model to add the function to. </param>
        /// <param name="name"> The model name of the function. </param>
        /// <param name="returnType"> The function return type. </param>
        /// <returns> The new <see cref="IMutableDbFunction" />. </returns>
        public static IMutableDbFunction AddDbFunction(
            [NotNull] this IMutableModel model,
            [NotNull] string name,
            [NotNull] Type returnType)
            => DbFunction.AddDbFunction(
                model, Check.NotNull(name, nameof(name)), returnType, ConfigurationSource.Explicit);

        /// <summary>
        ///     Creates an <see cref="IConventionDbFunction" />.
        /// </summary>
        /// <param name="model"> The model to add the function to. </param>
        /// <param name="name"> The model name of the function. </param>
        /// <param name="returnType"> The function return type. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The new <see cref="IConventionDbFunction" />. </returns>
        public static IConventionDbFunction AddDbFunction(
            [NotNull] this IConventionModel model,
            [NotNull] string name,
            [NotNull] Type returnType,
            bool fromDataAnnotation = false)
            => DbFunction.AddDbFunction(
                (IMutableModel)model,
                Check.NotNull(name, nameof(name)),
                returnType,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     Removes the <see cref="IMutableDbFunction" /> that is mapped to the method represented by the given
        ///     <see cref="MethodInfo" />.
        /// </summary>
        /// <param name="model"> The model to find the function in. </param>
        /// <param name="method"> The <see cref="MethodInfo" /> for the method that is mapped to the function. </param>
        /// <returns> The removed <see cref="IMutableDbFunction" /> or <see langword="null" /> if the method is not mapped. </returns>
        public static IMutableDbFunction RemoveDbFunction([NotNull] this IMutableModel model, [NotNull] MethodInfo method)
            => DbFunction.RemoveDbFunction(
                Check.NotNull(model, nameof(model)),
                Check.NotNull(method, nameof(method)));

        /// <summary>
        ///     Removes the <see cref="IConventionDbFunction" /> that is mapped to the method represented by the given
        ///     <see cref="MethodInfo" />.
        /// </summary>
        /// <param name="model"> The model to find the function in. </param>
        /// <param name="method"> The <see cref="MethodInfo" /> for the method that is mapped to the function. </param>
        /// <returns> The removed <see cref="IConventionDbFunction" /> or <see langword="null" /> if the method is not mapped. </returns>
        public static IConventionDbFunction RemoveDbFunction([NotNull] this IConventionModel model, [NotNull] MethodInfo method)
            => (IConventionDbFunction)((IMutableModel)model).RemoveDbFunction(method);

        /// <summary>
        ///     Removes the <see cref="IMutableDbFunction" /> that is mapped to the method represented by the given
        ///     <see cref="MethodInfo" />.
        /// </summary>
        /// <param name="model"> The model to find the function in. </param>
        /// <param name="name"> The model name of the function. </param>
        /// <returns> The removed <see cref="IMutableDbFunction" /> or <see langword="null" /> if the method is not mapped. </returns>
        public static IMutableDbFunction RemoveDbFunction([NotNull] this IMutableModel model, [NotNull] string name)
            => DbFunction.RemoveDbFunction(
                Check.NotNull(model, nameof(model)),
                Check.NotNull(name, nameof(name)));

        /// <summary>
        ///     Removes the <see cref="IConventionDbFunction" /> that is mapped to the method represented by the given
        ///     <see cref="MethodInfo" />.
        /// </summary>
        /// <param name="model"> The model to find the function in. </param>
        /// <param name="name"> The model name of the function. </param>
        /// <returns> The removed <see cref="IConventionDbFunction" /> or <see langword="null" /> if the method is not mapped. </returns>
        public static IConventionDbFunction RemoveDbFunction([NotNull] this IConventionModel model, [NotNull] string name)
            => (IConventionDbFunction)((IMutableModel)model).RemoveDbFunction(name);

        /// <summary>
        ///     Returns all <see cref="IDbFunction" />s contained in the model.
        /// </summary>
        /// <param name="model"> The model to get the functions in. </param>
        public static IEnumerable<IDbFunction> GetDbFunctions([NotNull] this IModel model)
            => DbFunction.GetDbFunctions(Check.NotNull(model, nameof(model)));

        /// <summary>
        ///     Returns all <see cref="IMutableDbFunction" />s contained in the model.
        /// </summary>
        /// <param name="model"> The model to get the functions in. </param>
        public static IEnumerable<IMutableDbFunction> GetDbFunctions([NotNull] this IMutableModel model)
            => DbFunction.GetDbFunctions((Model)Check.NotNull(model, nameof(model)));

        /// <summary>
        ///     Returns all <see cref="IConventionDbFunction" />s contained in the model.
        /// </summary>
        /// <param name="model"> The model to get the functions in. </param>
        public static IEnumerable<IConventionDbFunction> GetDbFunctions([NotNull] this IConventionModel model)
            => DbFunction.GetDbFunctions((Model)Check.NotNull(model, nameof(model)));

        /// <summary>
        ///     Returns the database collation.
        /// </summary>
        /// <param name="model"> The model to get the collation for. </param>
        /// <returns> The collation. </returns>
        public static string GetCollation([NotNull] this IModel model)
            => (string)model[RelationalAnnotationNames.Collation];

        /// <summary>
        ///     Sets the database collation.
        /// </summary>
        /// <param name="model"> The model to set the collation for. </param>
        /// <param name="value"> The value to set. </param>
        public static void SetCollation([NotNull] this IMutableModel model, [CanBeNull] string value)
            => model.SetOrRemoveAnnotation(
                RelationalAnnotationNames.Collation,
                Check.NullButNotEmpty(value, nameof(value)));

        /// <summary>
        ///     Sets the database collation.
        /// </summary>
        /// <param name="model"> The model to set the collation for. </param>
        /// <param name="value"> The value to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured collation. </returns>
        public static string SetCollation([NotNull] this IConventionModel model, [CanBeNull] string value, bool fromDataAnnotation = false)
        {
            model.SetOrRemoveAnnotation(
                RelationalAnnotationNames.Collation,
                Check.NullButNotEmpty(value, nameof(value)), fromDataAnnotation);
            return value;
        }

        /// <summary>
        ///     Returns the configuration source for the collation.
        /// </summary>
        /// <param name="model"> The model to find configuration source for. </param>
        /// <returns> The configuration source for the collation. </returns>
        public static ConfigurationSource? GetCollationConfigurationSource([NotNull] this IConventionModel model)
            => model.FindAnnotation(RelationalAnnotationNames.Collation)?.GetConfigurationSource();
    }
}
