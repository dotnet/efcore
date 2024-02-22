// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a database sequence in the model.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-sequences">Database sequences</see> for more information and examples.
/// </remarks>
public interface IReadOnlySequence : IReadOnlyAnnotatable
{
    /// <summary>
    ///     Gets the name of the sequence in the database.
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     Gets the model schema of the sequence. This is the one specified in
    ///     <see cref="RelationalModelBuilderExtensions.HasSequence(ModelBuilder, string, string?)" /> and the one to use
    ///     with <see cref="RelationalModelExtensions.FindSequence(IConventionModel, string, string?)" />.
    /// </summary>
    string? ModelSchema { get; }

    /// <summary>
    ///     Gets the database schema that contains the sequence.
    /// </summary>
    string? Schema { get; }

    /// <summary>
    ///     Gets the model in which this sequence is defined.
    /// </summary>
    IReadOnlyModel Model { get; }

    /// <summary>
    ///     Gets the value at which the sequence will start.
    /// </summary>
    long StartValue { get; }

    /// <summary>
    ///     Gets the amount incremented to obtain each new value in the sequence.
    /// </summary>
    int IncrementBy { get; }

    /// <summary>
    ///     Gets the minimum value supported by the sequence, or <see langword="null" /> if none has been set.
    /// </summary>
    long? MinValue { get; }

    /// <summary>
    ///     Gets the maximum value supported by the sequence, or <see langword="null" /> if none has been set.
    /// </summary>
    long? MaxValue { get; }

    /// <summary>
    ///     Gets the type of values returned by the sequence.
    /// </summary>
    Type Type { get; }

    /// <summary>
    ///     Gets the value indicating whether the sequence will start again from the beginning when the max value
    ///     is reached.
    /// </summary>
    bool IsCyclic { get; }

    /// <summary>
    ///     Gets the value indicating whether the sequence use preallocated values.
    /// </summary>
    bool IsCached { get; }

    /// <summary>
    ///     Gets the amount of preallocated values, or <see langword="null" /> if none has been set.
    /// </summary>
    int? CacheSize { get; }

    /// <summary>
    ///     <para>
    ///         Creates a human-readable representation of the given metadata.
    ///     </para>
    ///     <para>
    ///         Warning: Do not rely on the format of the returned string.
    ///         It is designed for debugging only and may change arbitrarily between releases.
    ///     </para>
    /// </summary>
    /// <param name="options">Options for generating the string.</param>
    /// <param name="indent">The number of indent spaces to use before each new line.</param>
    /// <returns>A human-readable representation.</returns>
    string ToDebugString(MetadataDebugStringOptions options = MetadataDebugStringOptions.ShortDefault, int indent = 0)
    {
        var builder = new StringBuilder();
        var indentString = new string(' ', indent);

        builder
            .Append(indentString)
            .Append("Sequence: ");

        if (ModelSchema != null)
        {
            builder
                .Append(ModelSchema)
                .Append('.');
        }

        builder.Append(Name);

        if (IsCyclic)
        {
            builder.Append(" Cyclic");
        }

        if (StartValue != 1)
        {
            builder.Append(" Start: ")
                .Append(StartValue);
        }

        if (IncrementBy != 1)
        {
            builder.Append(" IncrementBy: ")
                .Append(IncrementBy);
        }

        if (MinValue != null)
        {
            builder.Append(" Min: ")
                .Append(MinValue);
        }

        if (MaxValue != null)
        {
            builder.Append(" Max: ")
                .Append(MaxValue);
        }

        if (!IsCached)
        {
            builder.Append(" No Cache");
        }
        else if (CacheSize != null)
        {
            builder.Append(" Cache: ")
                .Append(CacheSize);
        }
        else
        {
            builder.Append(" Cache");
        }

        if ((options & MetadataDebugStringOptions.SingleLine) == 0)
        {
            if ((options & MetadataDebugStringOptions.IncludeAnnotations) != 0)
            {
                builder.Append(AnnotationsToDebugString(indent: indent + 2));
            }
        }

        return builder.ToString();
    }
}
