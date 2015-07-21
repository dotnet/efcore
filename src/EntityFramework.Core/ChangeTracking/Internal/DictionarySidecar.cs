// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public abstract class DictionarySidecar : Sidecar
    {
        private readonly Dictionary<IPropertyBase, object> _values;

        protected DictionarySidecar(
            [NotNull] InternalEntityEntry entry,
            [NotNull] IEnumerable<IPropertyBase> properties)
            : base(entry)
        {
            _values = properties.ToDictionary(p => p, p => (object)null);
        }

        public override bool CanStoreValue(IPropertyBase property) => _values.ContainsKey(property);

        protected override object ReadValue(IPropertyBase property) => _values[property];

        protected override void WriteValue(IPropertyBase property, object value) => _values[property] = value;
    }
}
