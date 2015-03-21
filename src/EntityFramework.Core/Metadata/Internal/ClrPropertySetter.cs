// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class ClrPropertySetter<TEntity, TValue> : IClrPropertySetter
        where TEntity : class
    {
        private readonly Action<TEntity, TValue> _setter;

        public ClrPropertySetter([NotNull] Action<TEntity, TValue> setter)
        {
            Check.NotNull(setter, nameof(setter));

            _setter = setter;
        }

        public virtual void SetClrValue(object instance, object value)
        {
            Check.NotNull(instance, nameof(instance));

            _setter((TEntity)instance, (TValue)value);
        }
    }
}
