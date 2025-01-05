// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents the type of a complex property of a structural type.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public interface IReadOnlyComplexType : IReadOnlyTypeBase
{
    /// <summary>
    ///     Gets the associated property.
    /// </summary>
    IReadOnlyComplexProperty ComplexProperty { get; }

    /// <summary>
    ///     Gets a value indicating whether given type is one of the containing types for this complex type.
    /// </summary>
    /// <param name="type">Type to search for in declaration path.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="type" /> is one of the containing types for this complex type,
    ///     otherwise <see langword="false" />.
    /// </returns>
    bool IsContainedBy(Type type)
    {
        var currentType = this;
        while (currentType != null)
        {
            var declaringType = currentType.ComplexProperty.DeclaringType;
            if (declaringType.ClrType.IsAssignableFrom(type))
            {
                return true;
            }

            currentType = declaringType as IReadOnlyComplexType;
        }

        return false;
    }

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

        try
        {
            builder
                .Append(indentString)
                .Append("ComplexType: ")
                .Append(DisplayName());

            if (IsAbstract())
            {
                builder.Append(" Abstract");
            }

            if (this is EntityType
                && GetChangeTrackingStrategy() != ChangeTrackingStrategy.Snapshot)
            {
                builder.Append(" ChangeTrackingStrategy.").Append(GetChangeTrackingStrategy());
            }

            if ((options & MetadataDebugStringOptions.SingleLine) == 0)
            {
                var properties = GetProperties().ToList();
                if (properties.Count != 0)
                {
                    builder.AppendLine().Append(indentString).Append("  Properties: ");
                    foreach (var property in properties)
                    {
                        builder.AppendLine().Append(property.ToDebugString(options, indent + 4));
                    }
                }

                var complexProperties = GetComplexProperties().ToList();
                if (complexProperties.Count != 0)
                {
                    builder.AppendLine().Append(indentString).Append("  Complex properties: ");
                    foreach (var complexProperty in complexProperties)
                    {
                        builder.AppendLine().Append(complexProperty.ToDebugString(options, indent + 4));
                    }
                }

                if ((options & MetadataDebugStringOptions.IncludeAnnotations) != 0)
                {
                    builder.Append(AnnotationsToDebugString(indent: indent + 2));
                }
            }
        }
        catch (Exception exception)
        {
            builder.AppendLine().AppendLine(CoreStrings.DebugViewError(exception.Message));
        }

        return builder.ToString();
    }
}
