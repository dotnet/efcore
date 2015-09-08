// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class ClrPropertyGetter<TEntity, TValue> : IClrPropertyGetter
        where TEntity : class
    {
        private readonly Func<TEntity, TValue> _getter;

        public ClrPropertyGetter([NotNull] Func<TEntity, TValue> getter)
        {
            _getter = getter;
        }

        public virtual object GetClrValue(object instance) => _getter((TEntity)instance);
    }
}
