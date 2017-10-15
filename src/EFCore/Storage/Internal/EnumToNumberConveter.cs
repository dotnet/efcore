// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class EnumToNumberConveter<TEnum, TNum> : ValueConverter<TEnum, TNum>
        where TEnum : struct
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public EnumToNumberConveter()
            : base(ToNumber(), ToEnum())
        {
        }

        private static Expression<Func<TEnum, TNum>> ToNumber()
        {
            var param = Expression.Parameter(typeof(TEnum), "value");
            return Expression.Lambda<Func<TEnum, TNum>>(
                Expression.Convert(param, typeof(TNum)), param);
        }

        private static Expression<Func<TNum, TEnum>> ToEnum()
        {
            var param = Expression.Parameter(typeof(TNum), "value");
            return Expression.Lambda<Func<TNum, TEnum>>(
                Expression.Convert(param, typeof(TEnum)), param);
        }
    }
}
