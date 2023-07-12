// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class ScaffoldingTypeMapper : IScaffoldingTypeMapper
{
    private readonly IRelationalTypeMappingSource _typeMappingSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ScaffoldingTypeMapper(IRelationalTypeMappingSource typeMappingSource)
    {
        _typeMappingSource = typeMappingSource;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual TypeScaffoldingInfo? FindMapping(
        string storeType,
        bool keyOrIndex,
        bool rowVersion,
        Type? clrType = null)
    {
        var mapping = clrType is null
            ? _typeMappingSource.FindMapping(storeType)
            : _typeMappingSource.FindMapping(clrType, storeType);
        if (mapping == null)
        {
            return null;
        }

        var canInfer = false;
        bool? scaffoldUnicode = null;
        bool? scaffoldFixedLength = null;
        int? scaffoldMaxLength = null;
        int? scaffoldPrecision = null;
        int? scaffoldScale = null;

        var unwrappedClrType = mapping.ClrType.UnwrapNullableType();

        // Check for inference
        var defaultTypeMapping = _typeMappingSource.FindMapping(
            unwrappedClrType,
            null,
            keyOrIndex,
            unicode: mapping.IsUnicode,
            size: mapping.Size,
            rowVersion: rowVersion,
            fixedLength: mapping.IsFixedLength,
            precision: mapping.Precision,
            scale: mapping.Scale);

        if (defaultTypeMapping != null
            && string.Equals(defaultTypeMapping.StoreType, storeType, StringComparison.Ordinal))
        {
            canInfer = true;

            // Check for Unicode
            var unicodeMapping = _typeMappingSource.FindMapping(
                unwrappedClrType,
                null,
                keyOrIndex,
                unicode: null,
                size: mapping.Size,
                rowVersion: rowVersion,
                fixedLength: mapping.IsFixedLength,
                precision: mapping.Precision,
                scale: mapping.Scale)!;

            scaffoldUnicode = unicodeMapping.IsUnicode != defaultTypeMapping.IsUnicode ? defaultTypeMapping.IsUnicode : null;

            // Check for fixed-length
            var fixedLengthMapping = _typeMappingSource.FindMapping(
                unwrappedClrType,
                null,
                keyOrIndex,
                unicode: mapping.IsUnicode,
                size: mapping.Size,
                fixedLength: null,
                precision: mapping.Precision,
                scale: mapping.Scale)!;

            scaffoldFixedLength = fixedLengthMapping.IsFixedLength != defaultTypeMapping.IsFixedLength
                ? defaultTypeMapping.IsFixedLength
                : null;

            // Check for size (= max-length)
            var sizedMapping = _typeMappingSource.FindMapping(
                unwrappedClrType,
                null,
                keyOrIndex,
                unicode: mapping.IsUnicode,
                size: null,
                rowVersion: rowVersion,
                fixedLength: false, // Fixed length with no size is not valid
                precision: mapping.Precision,
                scale: mapping.Scale)!;

            scaffoldMaxLength = (sizedMapping.Size == null && defaultTypeMapping.Size == -1)
                || sizedMapping.Size == defaultTypeMapping.Size
                    ? null
                    : defaultTypeMapping.Size;

            // Check for precision
            var precisionMapping = _typeMappingSource.FindMapping(
                unwrappedClrType,
                null,
                keyOrIndex,
                unicode: mapping.IsUnicode,
                size: mapping.Size,
                rowVersion: rowVersion,
                fixedLength: mapping.IsFixedLength,
                precision: null,
                scale: mapping.Scale)!;

            scaffoldPrecision = precisionMapping.Precision != defaultTypeMapping.Precision ? defaultTypeMapping.Precision : null;

            // Check for scale
            var scaleMapping = _typeMappingSource.FindMapping(
                unwrappedClrType,
                null,
                keyOrIndex,
                unicode: mapping.IsUnicode,
                size: mapping.Size,
                rowVersion: rowVersion,
                fixedLength: mapping.IsFixedLength,
                precision: mapping.Precision,
                scale: null)!;

            scaffoldScale = scaleMapping.Scale != defaultTypeMapping.Scale ? defaultTypeMapping.Scale : null;
        }

        return new TypeScaffoldingInfo(
            mapping.ClrType,
            canInfer,
            scaffoldUnicode,
            scaffoldMaxLength,
            scaffoldFixedLength,
            scaffoldPrecision,
            scaffoldScale);
    }
}
