// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class PropertyConfiguration : AnnotatableBase, ITypeMappingConfiguration
{
    private ValueConverter? _valueConverter;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public PropertyConfiguration(Type clrType)
    {
        ClrType = clrType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Type ClrType { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Apply(IMutableProperty property)
    {
        foreach (var annotation in GetAnnotations())
        {
            switch (annotation.Name)
            {
                case CoreAnnotationNames.MaxLength:
                    property.SetMaxLength((int?)annotation.Value);
                    break;
                case CoreAnnotationNames.Sentinel:
                    property.Sentinel = annotation.Value;
                    break;
                case CoreAnnotationNames.Unicode:
                    property.SetIsUnicode((bool?)annotation.Value);
                    break;
                case CoreAnnotationNames.Precision:
                    property.SetPrecision((int?)annotation.Value);
                    break;
                case CoreAnnotationNames.Scale:
                    property.SetScale((int?)annotation.Value);
                    break;
                case CoreAnnotationNames.ProviderClrType:
                    property.SetProviderClrType((Type?)annotation.Value);
                    break;
                case CoreAnnotationNames.ValueConverterType:
                    if (ClrType.UnwrapNullableType() == property.ClrType.UnwrapNullableType())
                    {
                        property.SetValueConverter((Type?)annotation.Value);
                    }

                    break;
                case CoreAnnotationNames.ValueComparerType:
                    if (ClrType.UnwrapNullableType() == property.ClrType.UnwrapNullableType())
                    {
                        property.SetValueComparer((Type?)annotation.Value);
                    }

                    break;
                case CoreAnnotationNames.ProviderValueComparerType:
                    if (ClrType.UnwrapNullableType() == property.ClrType.UnwrapNullableType())
                    {
                        property.SetProviderValueComparer((Type?)annotation.Value);
                    }

                    break;
                default:
                    if (!CoreAnnotationNames.AllNames.Contains(annotation.Name))
                    {
                        property.SetAnnotation(annotation.Name, annotation.Value);
                    }

                    break;
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual int? GetMaxLength()
        => (int?)this[CoreAnnotationNames.MaxLength];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void SetMaxLength(int? maxLength)
    {
        if (maxLength is < -1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxLength));
        }

        this[CoreAnnotationNames.MaxLength] = maxLength;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void SetSentinel(object? sentinel)
        => this[CoreAnnotationNames.Sentinel] = sentinel;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool? IsUnicode()
        => (bool?)this[CoreAnnotationNames.Unicode];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void SetIsUnicode(bool? unicode)
        => this[CoreAnnotationNames.Unicode] = unicode;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual int? GetPrecision()
        => (int?)this[CoreAnnotationNames.Precision];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void SetPrecision(int? precision)
    {
        if (precision is < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(precision));
        }

        this[CoreAnnotationNames.Precision] = precision;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual int? GetScale()
        => (int?)this[CoreAnnotationNames.Scale];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void SetScale(int? scale)
    {
        if (scale != null && scale < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(scale));
        }

        this[CoreAnnotationNames.Scale] = scale;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Type? GetProviderClrType()
        => (Type?)this[CoreAnnotationNames.ProviderClrType];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void SetProviderClrType(Type? providerClrType)
        => this[CoreAnnotationNames.ProviderClrType] = providerClrType;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ValueConverter? GetValueConverter()
    {
        if (_valueConverter != null)
        {
            return _valueConverter;
        }

        var converterType = (Type?)this[CoreAnnotationNames.ValueConverterType];
        return converterType == null
            ? null
            : _valueConverter = (ValueConverter?)Activator.CreateInstance(converterType);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void SetValueConverter(Type? converterType)
    {
        if (converterType != null)
        {
            if (!typeof(ValueConverter).IsAssignableFrom(converterType))
            {
                throw new InvalidOperationException(
                    CoreStrings.BadValueConverterType(converterType.ShortDisplayName(), typeof(ValueConverter).ShortDisplayName()));
            }
        }

        this[CoreAnnotationNames.ValueConverterType] = converterType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void SetValueComparer(Type? comparerType)
    {
        if (comparerType != null)
        {
            if (!typeof(ValueComparer).IsAssignableFrom(comparerType))
            {
                throw new InvalidOperationException(
                    CoreStrings.BadValueComparerType(comparerType.ShortDisplayName(), typeof(ValueComparer).ShortDisplayName()));
            }
        }

        this[CoreAnnotationNames.ValueComparerType] = comparerType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void SetProviderValueComparer(Type? comparerType)
    {
        if (comparerType != null)
        {
            if (!typeof(ValueComparer).IsAssignableFrom(comparerType))
            {
                throw new InvalidOperationException(
                    CoreStrings.BadValueComparerType(comparerType.ShortDisplayName(), typeof(ValueComparer).ShortDisplayName()));
            }
        }

        this[CoreAnnotationNames.ProviderValueComparerType] = comparerType;
    }
}
