// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Text;

namespace Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class JsonIdDefinition : IJsonIdDefinition
{
    private readonly IProperty? _discriminatorProperty;
    private readonly object? _discriminatorValue;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public JsonIdDefinition(IReadOnlyList<IProperty> properties)
        => Properties = properties;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public JsonIdDefinition(
        IReadOnlyList<IProperty> properties,
        IEntityType discriminatorEntityType,
        bool discriminatorIsRootType)
    {
        Properties = properties;
        DiscriminatorIsRootType = discriminatorIsRootType;
        _discriminatorProperty = discriminatorEntityType.FindDiscriminatorProperty();
        _discriminatorValue = discriminatorEntityType.GetDiscriminatorValue();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyList<IProperty> Properties { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool DiscriminatorIsRootType { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IncludesDiscriminator
        => _discriminatorProperty != null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string GenerateIdString(EntityEntry entry)
        => GenerateIdString(Properties.Select(p => entry.Property(p).CurrentValue));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string GenerateIdString(IEnumerable<object?> values)
    {
        var builder = new StringBuilder();

        if (_discriminatorProperty != null)
        {
            AppendValue(_discriminatorProperty!, _discriminatorValue!, singleValue: false);
        }

        var i = 0;
        var multipleValues = _discriminatorProperty != null || values.Skip(1).Any();
        foreach (var value in values)
        {
            AppendValue(Properties[i++], value, !multipleValues);
        }

        builder.Remove(builder.Length - 1, 1);

        return builder.ToString();

        void AppendValue(IProperty property, object? value, bool singleValue)
        {
            var converter = property.GetTypeMapping().Converter;
            AppendString(builder, converter == null ? value : converter.ConvertToProvider(value), singleValue);
            builder.Append('|');
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual void AppendString(StringBuilder builder, object? propertyValue, bool singleValue)
    {
        switch (propertyValue)
        {
            case string stringValue:
                AppendEscape(builder, stringValue, singleValue);
                return;
            case IEnumerable enumerable:
                foreach (var item in enumerable)
                {
                    AppendEscape(builder, item.ToString()!, singleValue);
                    builder.Append('|');
                }

                return;
            case DateTime dateTime:
                AppendEscape(builder, dateTime.ToString("O"), singleValue);
                return;
            default:
                if (propertyValue == null)
                {
                    builder.Append("null");
                }
                else
                {
                    AppendEscape(builder, propertyValue.ToString()!, singleValue);
                }

                return;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual StringBuilder AppendEscape(StringBuilder builder, string stringValue, bool singleValue)
    {
        var startingIndex = builder.Length;
        builder = builder.Append(stringValue);

        return singleValue
            ? builder
            : builder
                // We need this to avoid collisions with the value separator, but only when the key has multiple values.
                .Replace("|", "^|", startingIndex, builder.Length - startingIndex)
                // These are invalid characters, see https://docs.microsoft.com/dotnet/api/microsoft.azure.documents.resource.id
                .Replace("/", "^2F", startingIndex, builder.Length - startingIndex)
                .Replace("\\", "^5C", startingIndex, builder.Length - startingIndex)
                .Replace("?", "^3F", startingIndex, builder.Length - startingIndex)
                .Replace("#", "^23", startingIndex, builder.Length - startingIndex);
    }
}
