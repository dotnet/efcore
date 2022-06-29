// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a relational database function in a model.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see> for more information and examples.
/// </remarks>
public interface IReadOnlyDbFunction : IReadOnlyAnnotatable
{
    /// <summary>
    ///     Gets the name of the function in the database.
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     Gets the schema of the function in the database.
    /// </summary>
    string? Schema { get; }

    /// <summary>
    ///     Gets the name of the function in the model.
    /// </summary>
    string ModelName { get; }

    /// <summary>
    ///     Gets the model in which this function is defined.
    /// </summary>
    IReadOnlyModel Model { get; }

    /// <summary>
    ///     Gets the CLR method which maps to the function in the database.
    /// </summary>
    MethodInfo? MethodInfo { get; }

    /// <summary>
    ///     Gets the value indicating whether the database function is built-in.
    /// </summary>
    bool IsBuiltIn { get; }

    /// <summary>
    ///     Gets the value indicating whether this function returns scalar value.
    /// </summary>
    [MemberNotNullWhen(true, nameof(TypeMapping))]
    bool IsScalar { get; }

    /// <summary>
    ///     Gets the value indicating whether this function is an aggregate function.
    /// </summary>
    bool IsAggregate { get; }

    /// <summary>
    ///     Gets the value indicating whether the database function can return null.
    /// </summary>
    bool IsNullable { get; }

    /// <summary>
    ///     Gets the configured store type string.
    /// </summary>
    string? StoreType { get; }

    /// <summary>
    ///     Gets the returned CLR type.
    /// </summary>
    Type ReturnType { get; }

    /// <summary>
    ///     Gets the type mapping for the function's return type.
    /// </summary>
    RelationalTypeMapping? TypeMapping { get; }

    /// <summary>
    ///     Gets the parameters for this function.
    /// </summary>
    IReadOnlyList<IReadOnlyDbFunctionParameter> Parameters { get; }

    /// <summary>
    ///     Gets the translation callback for performing custom translation of the method call into a SQL expression fragment.
    /// </summary>
    Func<IReadOnlyList<SqlExpression>, SqlExpression>? Translation { get; }

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
            .Append("DbFunction: ");

        builder.Append(ReturnType.ShortDisplayName())
            .Append(' ');

        if (Schema != null)
        {
            builder
                .Append(Schema)
                .Append('.');
        }

        builder.Append(Name);

        if (Name != ModelName)
        {
            builder.Append('*');
        }

        if ((options & MetadataDebugStringOptions.SingleLine) == 0)
        {
            var parameters = Parameters.ToList();
            if (parameters.Count != 0)
            {
                builder.AppendLine().Append(indentString).Append("  Parameters: ");
                foreach (var parameter in parameters)
                {
                    builder.AppendLine().Append(parameter.ToDebugString(options, indent + 4));
                }
            }

            if ((options & MetadataDebugStringOptions.IncludeAnnotations) != 0)
            {
                builder.Append(AnnotationsToDebugString(indent: indent + 2));
            }
        }

        return builder.ToString();
    }
}
