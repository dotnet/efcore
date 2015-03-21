// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class ClrPropertyGetter<TEntity, TValue> : IClrPropertyGetter
        where TEntity : class
    {
        private readonly Func<TEntity, TValue> _getter;

        public ClrPropertyGetter([NotNull] Func<TEntity, TValue> getter)
        {
            Check.NotNull(getter, nameof(getter));

            _getter = getter;
        }

        public virtual object GetClrValue(object instance)
        {
            Check.NotNull(instance, nameof(instance));

            return _getter((TEntity)instance);
        }
    }
}
