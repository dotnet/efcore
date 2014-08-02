// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Relational.Utilities
{
    public class CloneContext
    {
        private readonly IDictionary<object, object> _itemToCloneMap = new Dictionary<object, object>();

        public virtual int ItemCount
        {
            get { return _itemToCloneMap.Count; }
        }

        public virtual object GetOrAdd([NotNull] object item, [NotNull] Func<object> cloneFunc)
        {
            object clone;

            if (!_itemToCloneMap.TryGetValue(item, out clone))
            {
                clone = cloneFunc();
                _itemToCloneMap.Add(item, clone);
            }

            return clone;
        }
    }
}
