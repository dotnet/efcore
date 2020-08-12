// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
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
        public ScaffoldingTypeMapper([NotNull] IRelationalTypeMappingSource typeMappingSource)
        {
            Check.NotNull(typeMappingSource, nameof(typeMappingSource));

            _typeMappingSource = typeMappingSource;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual TypeScaffoldingInfo FindMapping(
            string storeType,
            bool keyOrIndex,
            bool rowVersion)
        {
            // This is because certain providers can have no type specified as a default type e.g. SQLite
            Check.NotNull(storeType, nameof(storeType));

            var mapping = _typeMappingSource.FindMapping(storeType);
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
                    scale: mapping.Scale);

                scaffoldUnicode = unicodeMapping.IsUnicode != defaultTypeMapping.IsUnicode ? (bool?)defaultTypeMapping.IsUnicode : null;

                // Check for fixed-length
                var fixedLengthMapping = _typeMappingSource.FindMapping(
                    unwrappedClrType,
                    null,
                    keyOrIndex,
                    unicode: mapping.IsUnicode,
                    size: mapping.Size,
                    fixedLength: null,
                    precision: mapping.Precision,
                    scale: mapping.Scale);

                scaffoldFixedLength = fixedLengthMapping.IsFixedLength != defaultTypeMapping.IsFixedLength
                    ? (bool?)defaultTypeMapping.IsFixedLength
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
                    scale: mapping.Scale);

                scaffoldMaxLength = sizedMapping.Size != defaultTypeMapping.Size ? defaultTypeMapping.Size : null;

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
                    scale: mapping.Scale);

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
                    scale: null);

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
}
