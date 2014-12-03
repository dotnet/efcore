// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class ClrPropertyGetter<TEntity, TValue> : IClrPropertyGetter
    {
        private readonly Func<TEntity, TValue> _getter;

        public ClrPropertyGetter([NotNull] Func<TEntity, TValue> getter)
        {
            Check.NotNull(getter, "getter");

            _getter = getter;
        }

        public virtual object GetClrValue(object instance)
        {
            Check.NotNull(instance, "instance");

            return _getter((TEntity)instance);
        }
    }
}
