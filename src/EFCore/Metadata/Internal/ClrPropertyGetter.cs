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
    public sealed class ClrPropertyGetter<TEntity, TValue> : IClrPropertyGetter
        where TEntity : class
    {
        private readonly Func<TEntity, TValue> _getter;
        private readonly Func<TEntity, bool> _hasDefaultValue;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ClrPropertyGetter([NotNull] Func<TEntity, TValue> getter, [NotNull] Func<TEntity, bool> hasDefaultValue)
        {
            _getter = getter;
            _hasDefaultValue = hasDefaultValue;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object GetClrValue(object instance) => _getter((TEntity)instance);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasDefaultValue(object instance) => _hasDefaultValue((TEntity)instance);
    }
}
