// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class ClrICollectionAccessor<TEntity, TCollection, TElement> : IClrCollectionAccessor
        where TCollection : ICollection<TElement>
    {
        private readonly Func<TEntity, TCollection> _getCollection;

        public ClrICollectionAccessor([NotNull] Func<TEntity, TCollection> getCollection)
        {
            Check.NotNull(getCollection, "getCollection");

            _getCollection = getCollection;
        }

        public virtual void Add(object instance, object value)
        {
            Check.NotNull(instance, "instance");
            Check.NotNull(value, "value");

            _getCollection((TEntity)instance).Add((TElement)value);
        }

        public virtual bool Contains(object instance, object value)
        {
            Check.NotNull(instance, "instance");
            Check.NotNull(value, "value");

            return _getCollection((TEntity)instance).Contains((TElement)value);
        }

        public virtual void Remove(object instance, object value)
        {
            Check.NotNull(instance, "instance");
            Check.NotNull(value, "value");

            _getCollection((TEntity)instance).Remove((TElement)value);
        }
    }
}
