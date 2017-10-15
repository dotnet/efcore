// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class EnumToStringConveter<TEnum> : ValueConverter<TEnum, string>
        where TEnum : struct
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public EnumToStringConveter()
            : base(v => v.ToString(), v => ConvertToEnum(v))
        {
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
