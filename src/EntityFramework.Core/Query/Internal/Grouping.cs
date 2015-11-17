// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Query.Internal
{
    public class Grouping<TKey, TElement> : IGrouping<TKey, TElement>
    {
        private readonly List<TElement> _elements = new List<TElement>();

        public Grouping([CanBeNull] TKey key)
        {
            Key = key;
        }

        public virtual TKey Key { get; }

        public virtual void Add([CanBeNull] TElement element) => _elements.Add(element);

        public virtual IEnumerator<TElement> GetEnumerator() => _elements.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
