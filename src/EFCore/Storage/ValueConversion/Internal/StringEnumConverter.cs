// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class StringEnumConverter<TModel, TProvider, TEnum> : ValueConverter<TModel, TProvider>
        where TEnum : struct
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public StringEnumConverter(
            [NotNull] Expression<Func<TModel, TProvider>> convertToProviderExpression,
            [NotNull] Expression<Func<TProvider, TModel>> convertFromProviderExpression,
            [CanBeNull] ConverterMappingHints mappingHints = null)
            : base(convertToProviderExpression, convertFromProviderExpression, mappingHints)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected static new Expression<Func<TEnum, string>> ToString()
            => v => v.ToString();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected static Expression<Func<string, TEnum>> ToEnum()
        {
            if (!typeof(TEnum).UnwrapNullableType().IsEnum)
            {
                throw new InvalidOperationException(
                    CoreStrings.ConverterBadType(
                        typeof(StringEnumConverter<TModel, TProvider, TEnum>).ShortDisplayName(),
                        typeof(TEnum).ShortDisplayName(),
                        "enum types"));
            }

            return v => ConvertToEnum(v);
        }

        private static TEnum ConvertToEnum(string value)
            => Enum.TryParse<TEnum>(value, out var result)
                ? result
                : Enum.TryParse(value, true, out result)
                    ? result
                    : ulong.TryParse(value, out var ulongValue)
                        ? (TEnum)(object)ulongValue
                        : long.TryParse(value, out var longValue)
                            ? (TEnum)(object)longValue
                            : default;
    }
}
