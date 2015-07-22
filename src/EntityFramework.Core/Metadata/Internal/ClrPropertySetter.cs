// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class ClrPropertySetter<TEntity, TValue> : IClrPropertySetter
        where TEntity : class
    {
        private readonly Action<TEntity, TValue> _setter;

        public ClrPropertySetter([NotNull] Action<TEntity, TValue> setter)
        {
            _setter = setter;
        }

        public virtual void SetClrValue(object instance, object value)
            => _setter((TEntity)instance, (TValue)value);
    }
}
