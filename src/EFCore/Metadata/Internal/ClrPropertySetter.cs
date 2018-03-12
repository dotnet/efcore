// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public sealed class ClrPropertySetter<TEntity, TValue> : IClrPropertySetter
        where TEntity : class
    {
        private readonly Action<TEntity, TValue> _setter;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ClrPropertySetter([NotNull] Action<TEntity, TValue> setter)
        {
            _setter = setter;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetClrValue(object instance, object value)
            => _setter((TEntity)instance, (TValue)value);
    }
}
