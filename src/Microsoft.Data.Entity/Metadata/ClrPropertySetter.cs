// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class ClrPropertySetter<TEntity, TValue> : IClrPropertySetter
    {
        private readonly Action<TEntity, TValue> _setter;

        public ClrPropertySetter([NotNull] Action<TEntity, TValue> setter)
        {
            Check.NotNull(setter, "setter");

            _setter = setter;
        }

        public void SetClrValue(object instance, object value)
        {
            Check.NotNull(instance, "instance");

            _setter((TEntity)instance, (TValue)value);
        }
    }
}
