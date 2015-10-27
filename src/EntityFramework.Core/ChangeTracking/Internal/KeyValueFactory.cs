// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public abstract class KeyValueFactory
    {
        protected KeyValueFactory([NotNull] IKey key)
        {
            Key = key;
        }

        public virtual IKey Key { get; }

        public abstract IKeyValue Create(ValueBuffer valueBuffer);

        public abstract IKeyValue Create(
            [NotNull] IReadOnlyList<IProperty> properties,
            ValueBuffer valueBuffer);

        public abstract IKeyValue Create(
            [NotNull] IReadOnlyList<IProperty> properties,
            [NotNull] IPropertyAccessor propertyAccessor);
    }
}
